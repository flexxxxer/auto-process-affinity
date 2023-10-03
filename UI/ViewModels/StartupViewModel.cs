using Domain;

using UI.ViewModels.Entities;
using UI.DomainWrappers;

using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Diagnostics;
using System.Reactive.Concurrency;

using Avalonia;
using Avalonia.Styling;

using ReactiveUI;
using ReactiveUI.ExtendedRouting;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Splat;

using Microsoft.Extensions.Options;
using DynamicData;

namespace UI.ViewModels;

public interface IStartupViewModel
{
  ObservableCollection<MonitoredProcess> Processes { get; }

  IAsyncRelayCommand AddMonitoredProcessCommand { get; }

  IAsyncRelayCommand<MonitoredProcess?> RemoveMonitoredProcessCommand { get; }

  IRelayCommand<MonitoredProcess?> EditMonitoredProcessCommand { get; }
}

public partial class StartupViewModel : ViewModelBase, IStartupViewModel, IActivatableViewModel, IRoutableViewModel
{
  [ObservableProperty] ObservableCollection<MonitoredProcess> _processes = new();

  public ViewModelActivator Activator { get; } = new();
  public string? UrlPathSegment { get; } = nameof(StartupViewModel).RemoveVmPostfix();
  public IScreen HostScreen { get; }

  IDisposable? _periodicUpdateStick = null;
  readonly IOptionsMonitor<AppSettings> _appSettings;
  readonly AppSettingChangeService _appSettingService;

  public StartupViewModel(IOptionsMonitor<AppSettings> appSettings, AppSettingChangeService appSettingService, IScreen screen) 
  {
    HostScreen = screen;
    _appSettings = appSettings;
    _appSettingService = appSettingService;

    HandleAppSettingsChanged(appSettings.CurrentValue);
    var handleAppSettingsChangedSpecial = ((Action<AppSettings>)HandleAppSettingsChanged)
      .ThrottleInvokes(TimeSpan.FromSeconds(1))
      .InvokeOn(RxApp.MainThreadScheduler);

    this.WhenActivated((CompositeDisposable d) =>
    {
      appSettings
        .OnChange(handleAppSettingsChangedSpecial)
        ?.DisposeWith(d);

      Disposable
        .Create(HandleDeactivation)
        .DisposeWith(d);
    });
  }

  void HandleAppSettingsChanged(AppSettings newAppSettings)
  {
    static string ProcessName(MonitoredProcess p) => p.Name;

    _periodicUpdateStick?.Dispose();
    _periodicUpdateStick = RxApp.MainThreadScheduler
      .SchedulePeriodic(null as object, newAppSettings.RunningProcessesUpdatePeriod, async _ => await Refresh());

    var configuredProcesses = newAppSettings.ConfiguredProcesses;

    var newProcesses = configuredProcesses
      .Select(MonitoredProcess.CreateFrom)
      .ToArray();

    var removeFromExisting = Processes.ExceptBy(newProcesses.Select(ProcessName), ProcessName);
    var addToExisting = newProcesses.ExceptBy(Processes.Select(ProcessName), ProcessName);

    Processes.RemoveMany(removeFromExisting);
    Processes.AddRange(addToExisting);

    foreach(var p in Processes)
    {
      configuredProcesses
        .FirstOrDefault(cp => cp.Name == p.Name)
        ?.Do(cp => p.AffinityValue = (nint)AffinityApi.BitmaskFrom(cp.AffinityMode, cp.AffinityValue));
    }
  }

  void HandleDeactivation()
  {
  }

  async Task Refresh()
  {
    static MonitoredProcess.StateType TrySetAffinity(Process p, long affinity)
      => ProcessApi.TrySetProcessorAffinity(p, (nint)affinity)
          ? MonitoredProcess.StateType.AffinityApplied
          : MonitoredProcess.StateType.AffinityCantBeSet;

    static Process? GetSourceProcess(MonitoredProcess p)
    {
      var processName = p.Name;
      var normalizedName = processName.EndsWith(".exe")
        ? processName.Remove(".exe")
        : processName + ".exe";

      return Process.GetProcessesByName(normalizedName).FirstOrDefault()
        ?? Process.GetProcessesByName(processName).FirstOrDefault();
    }

    static MonitoredProcess.StateType SetAffinityAndGetStateType(nint affinityValue, Process? sourceProcess)
      => sourceProcess switch
      {
        not null => TrySetAffinity(sourceProcess, affinityValue),
        null => MonitoredProcess.StateType.NotRunning,
      };

    var sourceProcesses = await Processes
      .AsParallel()
      .AsOrdered()
      .Select(GetSourceProcess)
      .PipeUsingTaskRun(q => q.ToArray());

    var processesAffinityToSet = await Processes
      .Select(p => p.AffinityValue)
      .Zip(sourceProcesses)
      .AsParallel()
      .AsOrdered()
      .Select(tuple => SetAffinityAndGetStateType(tuple.First, tuple.Second))
      .PipeUsingTaskRun(q => q.ToArray());

    Processes
      .Zip(processesAffinityToSet)
      .ForEach((monitoredProcess, state) => monitoredProcess.State = state);

    await Task.Delay(TimeSpan.FromSeconds(5));
  }

  [RelayCommand]
  async Task AddMonitoredProcess()
  {
    var processesVm = await Locator.Current
      .GetRequiredService<AddProcessViewModel>()
      .RouteThrought(HostScreen);

    var selectedProcess = await processesVm.Result;

    if (selectedProcess is not null)
    {
      Processes.Add(MonitoredProcess.CreateFrom(selectedProcess));

      await _appSettingService.MakeChangeAsync(previousSettings =>
      {
        return previousSettings with
        {
          ConfiguredProcesses = previousSettings.ConfiguredProcesses
            .Append(selectedProcess)
            .ToArray()
        };
      });
    }
  }

  [RelayCommand]
  async Task RemoveMonitoredProcess(MonitoredProcess? p)
  {
    if (p is not null)
    {
      Processes.Remove(p);

      await _appSettingService.MakeChangeAsync(previousSettings =>
      {
        var removedProcessName = p.Name;
        return previousSettings with
        {
          ConfiguredProcesses = previousSettings.ConfiguredProcesses
            .Where(cp => cp.Name != removedProcessName)
            .ToArray()
        };
      });
    }
  }

  [RelayCommand]
  void EditMonitoredProcess(MonitoredProcess? p)
  {
  }
}

public sealed partial class DesignStartupViewModel : ViewModelBase, IStartupViewModel
{
  public DesignStartupViewModel()
  {
    if (App.IsDesignMode) Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;

    Processes.AddRange(new[]
    {
      new MonitoredProcess() { Name = "ExampleProcess.exe", State = MonitoredProcess.StateType.NotRunning },
      new MonitoredProcess() { Name = "Avalonia.exe", State = MonitoredProcess.StateType.AffinityCantBeSet },
      new MonitoredProcess() { Name = "devenv.exe", State = MonitoredProcess.StateType.AffinityApplied },
      new MonitoredProcess() { Name = "explorer.exe", State = MonitoredProcess.StateType.NotYetApplied },
    });
  }

  [ObservableProperty] ObservableCollection<MonitoredProcess> _processes = new();

  [RelayCommand]
  Task AddMonitoredProcess() => Task.CompletedTask;

  [RelayCommand]
  Task RemoveMonitoredProcess(MonitoredProcess? p) => Task.CompletedTask;

  [RelayCommand]
  void EditMonitoredProcess(MonitoredProcess? p) { }
}