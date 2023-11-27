using Domain;
using Domain.Infrastructure;

using UI.Views;
using UI.ViewModels;
using UI.DomainWrappers;

using System;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ReactiveUI;

using Splat;

namespace UI;

public partial class AppDataContext : ActivatableViewModelBase
{
  readonly MainWindow _mainWindow;
  readonly IScreen _hostScreen;

  [ObservableProperty] bool _isTrayIconVisible;
  
  public AppDataContext(MainWindow mainWindow, IScreen hostScreen,
    AppSettingChangeService appSettingsService)
  {
    _mainWindow = mainWindow;
    _hostScreen = hostScreen;
    
    HandleAppSettingsChanged(appSettingsService.CurrentAppSettings);
    Observable
      .FromEventPattern<AppSettings>(
        h => appSettingsService.AppSettingsChanged += h,
        h => appSettingsService.AppSettingsChanged -= h)
      .ObserveOn(RxApp.MainThreadScheduler)
      .Subscribe(eventPattern => HandleAppSettingsChanged(eventPattern.EventArgs));
  }

  void HandleAppSettingsChanged(AppSettings newAppSettings)
  {
    IsTrayIconVisible = newAppSettings.UxOptions.HideToTrayInsteadOfClosing;
  }

  [RelayCommand]
  void Show() => _mainWindow.WindowState = WindowState.Normal;

  [RelayCommand]
  void SwitchWindowState() => _mainWindow.WindowState = _mainWindow.WindowState switch
  {
    WindowState.Maximized 
      or WindowState.FullScreen 
      or WindowState.Normal => WindowState.Minimized,
    
    _ => WindowState.Normal,
  };

  [RelayCommand]
  void ShowSettings()
  {
    _hostScreen.Router.NavigateAndReset.Execute(Locator.Current.GetRequiredService<StartupViewModel>());
    _hostScreen.Router.Navigate.Execute(Locator.Current.GetRequiredService<SettingsViewModel>());
  }

  [RelayCommand]
  void Exit() 
    => Application.Current
      ?.Pipe(app => app.ApplicationLifetime)
      ?.Pipe(lifetime => lifetime as IClassicDesktopStyleApplicationLifetime)
      ?.Do(lifetime => lifetime.Shutdown());
}