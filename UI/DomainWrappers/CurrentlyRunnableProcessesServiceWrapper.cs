using Domain;

using System;

using Microsoft.Extensions.Options;
using System.Reactive.Disposables;
using System.Linq;

namespace UI.DomainWrappers;

class CurrentlyRunnableProcessesServiceWrapper : IDisposable
{
  readonly CurrentlyRunnableProcessesService _processesService;

  public CurrentlyRunnableProcessesServiceWrapper(CurrentlyRunnableProcessesService processesService, 
    IOptionsMonitor<AppSettings> options)
  {
    _processesService = processesService;
    HandleSettingsChanged(options.CurrentValue);

    options
      .OnChange(HandleSettingsChanged)
      ?.DisposeWith(App.AppLifetimeDisposable);
  }

  void HandleSettingsChanged(AppSettings newSettings)
  {
    _processesService.UpdateInterval = newSettings.RunningProcessesUpdatePeriod;
    var processNamesToExclude = _processesService.ProcessNamesToExclude;
    newSettings.ConfiguredProcesses
      .Select(p => p.Name)
      .SelectMany(name => new[] { name, name.Remove(".exe") })
      .ForEach(name => processNamesToExclude.TryAdd(name, default));
  }

  public void Dispose() => _processesService.Dispose();
}
