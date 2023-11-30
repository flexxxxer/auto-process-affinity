using Domain.Infrastructure;

using System;
using System.Reactive.Linq;
using ReactiveUI;

namespace UI.DomainWrappers;

public class AutostartChangeService : IDisposable
{
  readonly IDisposable _handleSettingsChangedStick;
  
  public AutostartChangeService(AppSettingChangeService appSettingsService)
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
    var autostartEnabled = newAppSettings.StartupOptions.Autostart;
    _ = AutostartApi.TryChangeAutostartMode(autostartEnabled);
  }

  public void Dispose()
    => _handleSettingsChangedStick.Dispose();
}