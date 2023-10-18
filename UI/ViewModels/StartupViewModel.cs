using Domain;
using Domain.Infrastructure;

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
using Avalonia.Controls.ApplicationLifetimes;

using ReactiveUI;
using ReactiveUI.ExtendedRouting;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DynamicData;

using Splat;

namespace UI.ViewModels;

public interface IStartupViewModel
{
  ObservableCollection<MonitoredProcess> Processes { get; }
  
  ReadOnlyCollection<MonitoredProcess> SelectedProcesses { get; set; }
  
  bool UseOldSchoolAddEditStyle { get; }

  IAsyncRelayCommand AddMonitoredProcessCommand { get; }

  IAsyncRelayCommand<MonitoredProcess?> RemoveMonitoredProcessCommand { get; }
  
  IAsyncRelayCommand RemoveAllSelectedMonitoredProcessesCommand { get; }

  IAsyncRelayCommand<MonitoredProcess?> EditMonitoredProcessCommand { get; }

  IAsyncRelayCommand GoToSettingsCommand { get; }

  IRelayCommand ExitCommand { get; }
}

public partial class StartupViewModel : RoutableAndActivatableViewModelBase, IStartupViewModel
{
  [ObservableProperty] ObservableCollection<MonitoredProcess> _processes = new();
  
  [ObservableProperty] 
  [NotifyCanExecuteChangedFor(nameof(RemoveMonitoredProcessCommand))]
  [NotifyCanExecuteChangedFor(nameof(RemoveAllSelectedMonitoredProcessesCommand))]
  [NotifyCanExecuteChangedFor(nameof(EditMonitoredProcessCommand))]
  bool _useOldSchoolAddEditStyle;

  [ObservableProperty] 
  [NotifyCanExecuteChangedFor(nameof(RemoveMonitoredProcessCommand))]
  [NotifyCanExecuteChangedFor(nameof(RemoveAllSelectedMonitoredProcessesCommand))]
  [NotifyCanExecuteChangedFor(nameof(EditMonitoredProcessCommand))]
  ReadOnlyCollection<MonitoredProcess> _selectedProcesses = new(Array.Empty<MonitoredProcess>());

  IDisposable? _periodicUpdateStick;
  AppSettings _appSettings;
  readonly AppSettingChangeService _appSettingService;

  public StartupViewModel(AppSettingChangeService appSettingsService)
  {
    _appSettings = appSettingsService.CurrentAppSettings;
    _appSettingService = appSettingsService;

    HandleAppSettingsChanged(_appSettings);
    this.WhenActivated(d =>
    {
      Observable
        .FromEventPattern<AppSettings>(
          h => appSettingsService.AppSettingsChanged += h,
          h => appSettingsService.AppSettingsChanged -= h)
        .ObserveOn(RxApp.MainThreadScheduler)
        .Subscribe(eventPattern => HandleAppSettingsChanged(eventPattern.EventArgs));

      Disposable
        .Create(HandleDeactivation)
        .DisposeWith(d);
    });
  }

  void HandleAppSettingsChanged(AppSettings newAppSettings)
  {
    static string ProcessName(MonitoredProcess p) => p.Name;

    _appSettings = newAppSettings;
    _periodicUpdateStick?.Dispose();
    _periodicUpdateStick = RxApp.MainThreadScheduler
      .SchedulePeriodic(newAppSettings.RunningProcessesUpdatePeriod, () => Refresh().NoAwait());
    
    UseOldSchoolAddEditStyle = newAppSettings.UxOptions.UseOldSchoolAddEditStyle;
    var configuredProcesses = newAppSettings.ConfiguredProcesses;
    var newProcesses = configuredProcesses
      .Select(MonitoredProcess.CreateFrom)
      .ToArray();

    var removeFromExisting = Processes.ExceptBy(newProcesses.Select(ProcessName), ProcessName);
    var addToExisting = newProcesses.ExceptBy(Processes.Select(ProcessName), ProcessName);

    Processes.RemoveMany(removeFromExisting);
    Processes.AddRange(addToExisting);

    foreach (var p in Processes)
    {
      configuredProcesses
        .FirstOrDefault(cp => cp.Name == p.Name)
        ?.Do(cp => p.AffinityValue = (nint)AffinityApi.BitmaskFrom(cp.AffinityMode, cp.AffinityValue));
    }
  }

  void HandleDeactivation()
  {
    _periodicUpdateStick?.Dispose();
  }

  async Task Refresh()
  {
    static MonitoredProcess.StateType TrySetAffinity(Process p, long affinity)
      => p.TrySetProcessorAffinity((nint)affinity)
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

    var processesStateTypeToSet = await Processes
      .Select(p => p.AffinityValue)
      .Zip(sourceProcesses)
      .AsParallel()
      .AsOrdered()
      .Select(tuple => SetAffinityAndGetStateType(tuple.First, tuple.Second))
      .PipeUsingTaskRun(q => q.ToArray());

    Processes
      .Zip(processesStateTypeToSet)
      .ForEach((monitoredProcess, state) => monitoredProcess.State = state);
  }

  [RelayCommand]
  async Task AddMonitoredProcess()
  {
    var addProcessVm = await Locator.Current
      .GetRequiredService<AddProcessViewModel>()
      .RouteThrough(HostScreen);

    if (await addProcessVm.Result is { } configuredProcess)
    {
      Processes.Add(MonitoredProcess.CreateFrom(configuredProcess));
      await _appSettingService.MakeChangeAsync(previousSettings => previousSettings with
      {
        ConfiguredProcesses = previousSettings.ConfiguredProcesses
          .Append(configuredProcess)
          .ToArray()
      });
    }
  }

  bool CanRemoveMonitoredProcess(MonitoredProcess? p)
    => UseOldSchoolAddEditStyle
      ? p is not null || SelectedProcesses is [_]
      : SelectedProcesses is [_];
  
  [RelayCommand(CanExecute = nameof(CanRemoveMonitoredProcess))]
  async Task RemoveMonitoredProcess(MonitoredProcess? p)
  {
    p ??= SelectedProcesses.FirstOrDefault();
    
    if (p is not null)
    {
      Processes.Remove(p);
      var removedProcessName = p.Name;
      
      await _appSettingService.MakeChangeAsync(previousSettings => previousSettings with
      {
        ConfiguredProcesses = previousSettings.ConfiguredProcesses
          .Where(cp => cp.Name != removedProcessName)
          .ToArray()
      });
    }
  }

  bool CanRemoveAllSelectedMonitoredProcesses()
    => UseOldSchoolAddEditStyle && SelectedProcesses.Count > 1;
  
  [RelayCommand(CanExecute = nameof(CanRemoveAllSelectedMonitoredProcesses))]
  async Task RemoveAllSelectedMonitoredProcesses()
  {
    if (SelectedProcesses is { Count: > 1 } selectedProcesses)
    {
      Processes.RemoveMany(selectedProcesses);
      var removedProcessesNames = selectedProcesses
        .Select(x => x.Name)
        .ToHashSet();
      
      await _appSettingService.MakeChangeAsync(previousSettings => previousSettings with
      {
        ConfiguredProcesses = previousSettings.ConfiguredProcesses
          .Where(cp => !removedProcessesNames.Contains(cp.Name))
          .ToArray()
      });
    }
  }

  bool CanEditMonitoredProcess(MonitoredProcess? p) 
    => UseOldSchoolAddEditStyle
      ? p is not null || SelectedProcesses is [_]
      : SelectedProcesses is [_];
  
  [RelayCommand(CanExecute = nameof(CanEditMonitoredProcess))]
  async Task EditMonitoredProcess(MonitoredProcess? p)
  {
    p ??= SelectedProcesses.FirstOrDefault();
    
    var configuredProcessToEdit = _appSettings
      .ConfiguredProcesses
      .FirstOrDefault(cp => cp.Name == p?.Name);
    
    if (p is not null && configuredProcessToEdit is not null)
    {
      var addProcessVm = Locator.Current
        .GetRequiredService<AddProcessViewModel>()
        .Do(vm => vm.ToEdit = configuredProcessToEdit);

      var processesVm = await addProcessVm.RouteThrough(HostScreen);
      if (await processesVm.Result is { } configuredProcess)
      {
        await _appSettingService.MakeChangeAsync(previousSettings => previousSettings with
        {
          ConfiguredProcesses = previousSettings.ConfiguredProcesses
            .Select(cp => cp.Name == configuredProcess.Name ? configuredProcess : cp)
            .ToArray()
        });
      }
    }
  }

  [RelayCommand]
  async Task GoToSettings()
    => await Locator.Current
      .GetRequiredService<SettingsViewModel>()
      .RouteThrough(HostScreen);

  [RelayCommand]
  void Exit() => Application.Current
    ?.ApplicationLifetime
    ?.TryCastTo<IClassicDesktopStyleApplicationLifetime>()
    ?.Shutdown();
}

public sealed partial class DesignStartupViewModel : ViewModelBase, IStartupViewModel
{
  [ObservableProperty] ObservableCollection<MonitoredProcess> _processes = new();
  [ObservableProperty] ReadOnlyCollection<MonitoredProcess> _selectedProcesses = new(Array.Empty<MonitoredProcess>());
  [ObservableProperty] bool _useOldSchoolAddEditStyle;
  
  public DesignStartupViewModel()
  {
    Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;

    Processes.AddRange(new[]
    {
      new MonitoredProcess() { Name = "ExampleProcess.exe", State = MonitoredProcess.StateType.NotRunning },
      new MonitoredProcess() { Name = "Avalonia.exe", State = MonitoredProcess.StateType.AffinityCantBeSet },
      new MonitoredProcess() { Name = "devenv.exe", State = MonitoredProcess.StateType.AffinityApplied },
      new MonitoredProcess() { Name = "explorer.exe", State = MonitoredProcess.StateType.NotYetApplied },
    });
  }

  [RelayCommand]
  Task AddMonitoredProcess()
  {
    UseOldSchoolAddEditStyle = !UseOldSchoolAddEditStyle;
    return Task.CompletedTask;
  }
  
  [RelayCommand]
  Task RemoveMonitoredProcess(MonitoredProcess? p) => Task.CompletedTask;
  
  [RelayCommand]
  Task RemoveAllSelectedMonitoredProcesses() => Task.CompletedTask;

  [RelayCommand]
  Task EditMonitoredProcess(MonitoredProcess? p) => Task.CompletedTask;

  [RelayCommand]
  Task GoToSettings() => Task.CompletedTask;

  [RelayCommand]
  void Exit() { }
}