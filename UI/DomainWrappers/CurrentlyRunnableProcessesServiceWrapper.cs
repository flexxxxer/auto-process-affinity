using Domain;
using Domain.Infrastructure;

using System;
using System.Linq;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace UI.DomainWrappers;

class CurrentlyRunnableProcessesServiceWrapper : IDisposable
{
  readonly CurrentlyRunnableProcessesService _processesService;

  public CurrentlyRunnableProcessesServiceWrapper(CurrentlyRunnableProcessesService processesService, 
    AppSettingChangeService appSettingsService)
  {
    _processesService = processesService;
    HandleSettingsChanged(appSettingsService.CurrentAppSettings);

    Observable
      .FromEventPattern<AppSettings>(
        h => appSettingsService.AppSettingsChanged += h,
        h => appSettingsService.AppSettingsChanged -= h)
      .Throttle(TimeSpan.FromSeconds(0.6))
      .Subscribe(eventPattern => HandleSettingsChanged(eventPattern.EventArgs))
      .DisposeWith(App.Lifetime);
  }

  void HandleSettingsChanged(AppSettings newSettings)
  {
    _processesService.UpdateInterval = newSettings.RunningProcessesUpdatePeriod;
    var processNamesToExclude = _processesService.ProcessNamesToExclude;
    processNamesToExclude.Clear();
    newSettings.ConfiguredProcesses
      .Select(p => p.Name)
      .Append(Process.GetCurrentProcess().ProcessName)
      .SelectMany(name => new[] { name, name.Remove(".exe") })
      .ForEach(name => processNamesToExclude.TryAdd(name, default));
  }

  public void Dispose() => _processesService.Dispose();
}
