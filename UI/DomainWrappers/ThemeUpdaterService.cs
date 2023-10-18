using Domain.Infrastructure;

using System;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Styling;

using ReactiveUI;

namespace UI.DomainWrappers;

class ThemeUpdaterService : IDisposable
{
  readonly IDisposable _handleSettingsChangedStick;

  public ThemeUpdaterService(AppSettingChangeService appSettingsService)
  {
    HandleSettingsChanged(appSettingsService.CurrentAppSettings);

    _handleSettingsChangedStick = Observable
      .FromEventPattern<AppSettings>(
        h => appSettingsService.AppSettingsChanged += h,
        h => appSettingsService.AppSettingsChanged -= h)
      .ObserveOn(RxApp.MainThreadScheduler)
      .Subscribe(eventPattern => HandleSettingsChanged(eventPattern.EventArgs));
  }

  void HandleSettingsChanged(AppSettings newAppSettings)
  {
    if (Application.Current is { } app)
    {
      app.RequestedThemeVariant = newAppSettings.UiOptions.Theme switch
      {
        AppTheme.System => ThemeVariant.Default,
        AppTheme.Light => ThemeVariant.Light,
        AppTheme.Dark => ThemeVariant.Dark,
        _ => null,
      };
    }
  }

  public void Dispose()
    => _handleSettingsChangedStick.Dispose();
}
