using UI.DomainWrappers;

using Domain;
using Domain.Infrastructure;

using System;
using System.Threading.Tasks;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Diagnostics;

using Avalonia;
using Avalonia.Controls;

using ReactiveUI;

using CommunityToolkit.Mvvm.ComponentModel;

using DynamicData.Binding;

using Hardware.Info;

namespace UI.ViewModels;

public interface IMainWindowViewModel : IScreen
{
  PixelPoint WindowPosition { get; set; }

  WindowState WindowState { get; }
  
  double WindowHeight { get; set; }

  double WindowWidth { get; set; }

  string WindowTitleText { get; }

  WindowStartupLocation WindowStartupLocationMode { get; }

  Interaction<PixelPoint, Unit> SetWindowPosition { get; }
}

public partial class MainWindowViewModel : ActivatableViewModelBase, IMainWindowViewModel
{
  [ObservableProperty] double _windowHeight;
  [ObservableProperty] double _windowWidth;
  [ObservableProperty] string _windowTitleText = "";
  [ObservableProperty] PixelPoint _windowPosition;
  [ObservableProperty] WindowStartupLocation _windowStartupLocationMode;
  [ObservableProperty] WindowState _windowState;

  public string DefaultWindowTitleText { get; }

  public Interaction<PixelPoint, Unit> SetWindowPosition { get; } = new();

  IDisposable? _rememberLastSizeStick;
  IDisposable? _rememberLastPositionStick;
  readonly AppSettingChangeService _appSettingsService;

  public MainWindowViewModel(HardwareInfo hwInfo, AdminPrivilegesStatus privilegesStatus, 
    AppSettingChangeService appSettingsService) 
  {
    _appSettingsService = appSettingsService;
    var startupOptions = appSettingsService.CurrentAppSettings.StartupOptions;
    WindowTitleText = DefaultWindowTitleText = MakeDefaultTitlePostfix(privilegesStatus);
    (WindowHeight, WindowWidth) = startupOptions.StartupSizeMode switch
    {
      StartupSizeMode.Optimal => MakeOptimalStartupSize(hwInfo),
      StartupSizeMode.Specified or StartupSizeMode.RememberLast => TupleFrom(startupOptions.StartupSize),
      _ => throw new ArgumentOutOfRangeException()
    };
    (var locationValues, WindowStartupLocationMode) = startupOptions.StartupLocationMode switch
    {
      StartupLocationMode.CenterScreen => (null as (int, int)?, WindowStartupLocation.CenterScreen),
      StartupLocationMode.RememberLast => (TupleFrom(startupOptions.StartupLocation), WindowStartupLocation.Manual),
      StartupLocationMode.Default => (null, WindowStartupLocation.Manual),
      _ => throw new ArgumentOutOfRangeException()
    };
    WindowState = startupOptions.Minimized
      ? WindowState.Minimized
      : WindowState.Normal;

    HandleAppSettingsChanged(appSettingsService.CurrentAppSettings);

    // ReSharper disable once AsyncVoidLambda
    this.WhenActivated(async d =>
    {
      Observable
        .FromEventPattern<AppSettings>(
          h => appSettingsService.AppSettingsChanged += h,
          h => appSettingsService.AppSettingsChanged -= h)
        .ObserveOn(RxApp.MainThreadScheduler)
        .Subscribe(eventPattern => HandleAppSettingsChanged(eventPattern.EventArgs));

      if (locationValues is var (x, y))
      {
        await Task.Yield(); // "wait" for full view activation (and interaction registrations)
        await SetWindowPosition.Handle(new PixelPoint(x, y));
      }

      Disposable
        .Create(HandleDeactivation)
        .DisposeWith(d);
    });
  }

  void HandleAppSettingsChanged(AppSettings newAppSettings)
  {
    static IDisposable? DisposeAndGetNull(IDisposable? disposable)
    {
      disposable?.Dispose();
      return null;
    }

    void UpdateStartupSize(MainWindowViewModel vm)
    {
      var (height, width) = (vm.WindowHeight, vm.WindowWidth);
      _appSettingsService
        .MakeChangeAsync(appSettings => appSettings with
        {
          StartupOptions = appSettings.StartupOptions with
          {
            StartupSize = new() { Height = height, Width = width }
          }
        })
        .NoAwait();
    }

    void UpdateStartupLocation(PixelPoint position)
    {
      _appSettingsService
        .MakeChangeAsync(appSettings => appSettings with
        {
          StartupOptions = appSettings.StartupOptions with
          {
            StartupLocation = new() { X = position.X, Y = position.Y }
          }
        })
        .NoAwait();
    }

    _rememberLastSizeStick = (newAppSettings.StartupOptions.StartupSizeMode, _rememberLastSizeStick) switch
    {
      (not StartupSizeMode.RememberLast, _) => DisposeAndGetNull(_rememberLastSizeStick),
      (StartupSizeMode.RememberLast, { } stick) => stick,
      (StartupSizeMode.RememberLast, null) => this
        .Do(UpdateStartupSize)
        .WhenAnyPropertyChanged(nameof(WindowHeight), nameof(WindowWidth))
        .Throttle(TimeSpan.FromSeconds(0.5))
        .WhereNotNull()
        .Subscribe(UpdateStartupSize)
    };

    _rememberLastPositionStick = (newAppSettings.StartupOptions.StartupLocationMode, _rememberLastPositionStick) switch
    {
      (not StartupLocationMode.RememberLast, _) => DisposeAndGetNull(_rememberLastPositionStick),
      (StartupLocationMode.RememberLast, { } stick) => stick,
      (StartupLocationMode.RememberLast, null) => this
        .Do(vm => UpdateStartupLocation(vm.WindowPosition))
        .WhenPropertyChanged(vm => vm.WindowPosition)
        .Throttle(TimeSpan.FromSeconds(0.5))
        .WhereNotNull()
        .Select(propertyChanged => propertyChanged.Value)
        .Subscribe(UpdateStartupLocation)
    };
  }

  void HandleDeactivation()
  {
    _rememberLastSizeStick?.Dispose();
    _rememberLastPositionStick?.Dispose();
  }

  static (int, int) TupleFrom(StartupOptions.StartupLocationValues locationValues)
    => (locationValues.X, locationValues.Y);

  static (double, double) TupleFrom(StartupOptions.StartupSizeValues sizeValues)
    => (sizeValues.Height, sizeValues.Width);

  static string MakeDefaultTitlePostfix(AdminPrivilegesStatus privilegesStatus)
  {
    var defaultTitlePostfix = OsTypeApi.CurrentOs switch
    {
      OsType.Linux when privilegesStatus.IsAdmin => " (Root)",
      OsType.Windows when privilegesStatus.IsAdmin => " (Administrator)",
      _ => ""
    };

    return Process.GetCurrentProcess()
      .ProcessName.Remove(".exe") 
      + defaultTitlePostfix;
  }

  static (double Height, double Width) MakeOptimalStartupSize(HardwareInfo hwInfo)
  {
    hwInfo.RefreshVideoControllerList();
    var verticalRes = hwInfo
        .VideoControllerList
        .MaxBy(vc => vc.CurrentVerticalResolution)
        ?.CurrentVerticalResolution ?? 1080;

    var height = verticalRes / 1.8;
    var width = height / 1.5;
    return (height, width);
  }

  // fake one router: MainWindowViewModel not used as IScreen but implements it cause
  // if not then will be exception when MainWindow.DataContext will be set (ReactiveUI side problem)
  RoutingState IScreen.Router => null!;
}

public sealed class DesignMainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
  public PixelPoint WindowPosition { get; set; }
  
  public double WindowHeight { get; set; } = 400;
  
  public double WindowWidth { get; set; } = 266;

  public string WindowTitleText { get; } = Application.Current?.Name ?? "";

  public WindowStartupLocation WindowStartupLocationMode { get; } = WindowStartupLocation.Manual;

  public WindowState WindowState { get; } = WindowState.Normal;

  public Interaction<PixelPoint, Unit> SetWindowPosition { get; } = new();

  // fake one router: MainWindowViewModel not used as IScreen but implements it cause
  // if not then will be exception when MainWindow.DataContext will be set (ReactiveUI side problem)
  RoutingState IScreen.Router => null!;
}