﻿using Domain;

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
      .SchedulePeriodic(newAppSettings.RunningProcessesUpdatePeriod, Refresh);

    var newProcesses = newAppSettings.ConfiguredProcesses
      .Select(MonitoredProcess.CreateFrom)
      .ToArray();

    var removeFromExisting = Processes.ExceptBy(newProcesses.Select(ProcessName), p => p.Name);
    var addToExisting = newProcesses.ExceptBy(Processes.Select(ProcessName), p => p.Name);

    Processes.RemoveMany(removeFromExisting);
    Processes.AddRange(addToExisting);
  }

  void HandleDeactivation()
  {
  }

  void Refresh()
  {
    static MonitoredProcess.StateType TrySetAffinity(Process p, long affinity)
      => ProcessApi.TrySetProcessorAffinity(p, (nint)affinity)
          ? MonitoredProcess.StateType.AffinityApplied
          : MonitoredProcess.StateType.AffinityCantBeSet;

    foreach(var process in Processes)
    {
      var normalizedName = process.Name.EndsWith(".exe") 
        ? process.Name.Remove(".exe")
        : process.Name + ".exe";

      var sourceProcess = Process.GetProcessesByName(normalizedName).FirstOrDefault() 
        ?? Process.GetProcessesByName(process.Name).FirstOrDefault();

      process.State = sourceProcess switch
      {
        null => MonitoredProcess.StateType.NotRunning,
        not null => TrySetAffinity(sourceProcess, process.AffinityValue),
      };
    }
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