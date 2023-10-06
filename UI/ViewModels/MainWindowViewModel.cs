using UI.DomainWrappers;

using Domain;
using Domain.Infrastructure;

using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Diagnostics;

using Avalonia;
using Avalonia.Controls;

using ReactiveUI;

using CommunityToolkit.Mvvm.ComponentModel;

using Hardware.Info;

using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace UI.ViewModels;

public interface IMainWindowViewModel : IScreen
{
  double WindowHeight { get; set; }

  double WindowWidth { get; set; }

  string WindowTitleText { get; }

  WindowStartupLocation WindowStartupLocationMode { get; }

  Interaction<PixelPoint, Unit> SetWindowPosition { get; }
}

public partial class MainWindowViewModel : ViewModelBase, IMainWindowViewModel, IActivatableViewModel
{
  [ObservableProperty] double _windowHeight;
  [ObservableProperty] double _windowWidth;
  [ObservableProperty] string _windowTitleText = "";
  [ObservableProperty] WindowStartupLocation _windowStartupLocationMode;

  public string DefaultWindowTitleText { get; }

  public ViewModelActivator Activator { get; } = new();

  public Interaction<PixelPoint, Unit> SetWindowPosition { get; } = new();

  public MainWindowViewModel(HardwareInfo hwInfo, AdminPrivilegesStatus privilegesStatus, IOptionsMonitor<AppSettings> appSettings) 
  {
    var startupOptions = appSettings.CurrentValue.StartupOptions;
    WindowTitleText = DefaultWindowTitleText = MakeDefaultTitlePostfix(privilegesStatus);
    (WindowHeight, WindowWidth) = startupOptions.StartupSizeMode switch
    {
      StartupSizeMode.Optimal => MakeOptimalStartupSize(hwInfo),
      StartupSizeMode.Specified or StartupSizeMode.RememberLast or _ => TupleFrom(startupOptions.StartupSize)
    };
    (var locationValues, WindowStartupLocationMode) = startupOptions.StartupLocationMode switch
    {
      StartupLocationMode.CenterScreen => (null as (int, int)?, WindowStartupLocation.CenterScreen),
      StartupLocationMode.RememberLast => (TupleFrom(startupOptions.StartupLocation), WindowStartupLocation.Manual),
      StartupLocationMode.Default or _ => (null, WindowStartupLocation.Manual),
    };
    this.WhenActivated(async (CompositeDisposable d) =>
    {
      if(locationValues is (var x, var y))
      {
        await Task.Yield(); // "wait" for full view activation (and interaction registrations)
        await SetWindowPosition.Handle(new(x, y));
      }
    });
  }

  static (int, int) TupleFrom(StartupOptions.StartupLocationValues locationValues)
    => (locationValues.X, locationValues.Y);

  static (double, double) TupleFrom(StartupOptions.StartupSizeValues sizeValues)
    => (sizeValues.Height, sizeValues.Width);

  static string MakeDefaultTitlePostfix(AdminPrivilegesStatus privilegesStatus)
  {
    var defaultTitlePostfix = OSTypeApi.CurrentOS switch
    {
      OSType.Linux when privilegesStatus.IsAdmin => " (Root)",
      OSType.Windows when privilegesStatus.IsAdmin => " (Administrator)",
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
  public double WindowHeight { get; set; } = 400;
  
  public double WindowWidth { get; set; } = 266;

  public string WindowTitleText { get; } = Application.Current?.Name ?? "";

  public WindowStartupLocation WindowStartupLocationMode { get; } = WindowStartupLocation.Manual;

  public Interaction<PixelPoint, Unit> SetWindowPosition { get; } = new();

  // fake one router: MainWindowViewModel not used as IScreen but implements it cause
  // if not then will be exception when MainWindow.DataContext will be set (ReactiveUI side problem)
  RoutingState IScreen.Router => null!;
}