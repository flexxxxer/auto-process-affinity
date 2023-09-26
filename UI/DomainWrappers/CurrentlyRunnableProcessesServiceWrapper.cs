using Domain;

using System;

using Microsoft.Extensions.Options;

namespace UI.DomainWrappers;

class CurrentlyRunnableProcessesServiceWrapper : IDisposable
{
  readonly CurrentlyRunnableProcessesService _processesService;

  public CurrentlyRunnableProcessesServiceWrapper(CurrentlyRunnableProcessesService processesService, 
    IOptionsMonitor<AppSettings> options)
  {
    _processesService = processesService;
    HandleSettingsChanged(options.CurrentValue);
    options.OnChange(HandleSettingsChanged);
  }

  void HandleSettingsChanged(AppSettings newSettings)
  {
    _processesService.UpdateInterval = newSettings.RunningProcessesUpdatePeriod;
    newSettings
      .ConfiguredProcesses
      .ForEach(p => _processesService.ProcessNamesToExclude.TryAdd(p.Name, default));
  }

  public void Dispose() => _processesService.Dispose();
}
