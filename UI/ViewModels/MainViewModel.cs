using Domain.Infrastructure;

using UI.DomainWrappers;

using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;

using ReactiveUI;
using ReactiveUI.ExtendedRouting;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Splat;

namespace UI.ViewModels;

public interface IMainViewModel : IScreen
{
  IAsyncRelayCommand GoToSettingsCommand { get; }

  IRelayCommand ExitCommand { get; }
  
  bool IsCustomTitleBarUsed { get; }
  
  bool IsMenuVisible { get; }
}

public partial class MainViewModel : ActivatableViewModelBase, IMainViewModel
{
  [ObservableProperty] bool _isCustomTitleBarUsed;
  [ObservableProperty] bool _isMenuVisible;
  
  public RoutingState Router { get; } = new();

  public MainViewModel(MainWindowViewModel mainWindowViewModel, AppSettingChangeService appSettingsService) 
  {
    var routeVmChanged = Router
      .CurrentViewModel
      .WhereNotNull();

    var showSystemTitleBarSettingChanged = Observable
      .FromEventPattern<AppSettings>(
        h => appSettingsService.AppSettingsChanged += h,
        h => appSettingsService.AppSettingsChanged -= h)
      .ObserveOn(RxApp.MainThreadScheduler)
      .Select(eventPattern => eventPattern.EventArgs.UiOptions.ShowSystemTitleBar);

    IsCustomTitleBarUsed = appSettingsService.CurrentAppSettings.UiOptions.ShowSystemTitleBar is false;
    
    // ReSharper disable once AsyncVoidLambda
    this.WhenActivated(async d =>
    {
      routeVmChanged
        .Select(vm => vm switch
        {
          IAddProcessViewModel => "New process rule",
          ISelectCurrentlyRunnableProcessViewModel => "Selecting process",
          ISettingsViewModel => "Settings",
          IStartupViewModel or _ => mainWindowViewModel.DefaultWindowTitleText,
        })
        .Subscribe(newTitle => mainWindowViewModel.WindowTitleText = newTitle)
        .DisposeWith(d);
      
      routeVmChanged
        .Select(vm => vm is IStartupViewModel)
        .Subscribe(isVisible => IsMenuVisible = isVisible)
        .DisposeWith(d);
      
      showSystemTitleBarSettingChanged
        .Subscribe(systemTitleBarUsed => IsCustomTitleBarUsed = systemTitleBarUsed is false)
        .DisposeWith(d);

      _ = await Locator.Current
        .GetRequiredService<StartupViewModel>()
        .RouteThrough(Router);
    });
  }
  
  [RelayCommand]
  async Task GoToSettings()
    => await Locator.Current
      .GetRequiredService<SettingsViewModel>()
      .RouteThrough(this);

  [RelayCommand]
  void Exit() => Application.Current
    ?.ApplicationLifetime
    ?.TryCastTo<IClassicDesktopStyleApplicationLifetime>()
    ?.Shutdown();
}

public sealed partial class DesignMainViewModel : ViewModelBase, IMainViewModel
{
  [ObservableProperty] bool _isCustomTitleBarUsed = true;
  [ObservableProperty] bool _isMenuVisible = true;
  
  public RoutingState Router { get; } = new();
  
  public DesignMainViewModel()
  {
    Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
  }

  [RelayCommand]
  Task GoToSettings() => Task.CompletedTask;

  [RelayCommand]
  void Exit() { }
}

