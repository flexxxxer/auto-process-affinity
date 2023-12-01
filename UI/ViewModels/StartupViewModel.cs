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
      PeriodUpdateRecreate(appSettingsService.CurrentAppSettings.RunningProcessesUpdatePeriod);
      Observable
        .FromEventPattern<AppSettings>(
          h => appSettingsService.AppSettingsChanged += h,
          h => appSettingsService.AppSettingsChanged -= h)
        .ObserveOn(RxApp.MainThreadScheduler)
        .Subscribe(eventPattern => HandleAppSettingsChanged(eventPattern.EventArgs))
        .DisposeWith(d);

      Disposable
        .Create(HandleDeactivation)
        .DisposeWith(d);
    });
  }

  void PeriodUpdateRecreate(TimeSpan updatePeriod)
  {
    _periodicUpdateStick?.Dispose();
    _periodicUpdateStick = RxApp.MainThreadScheduler
      .SchedulePeriodic(updatePeriod, () => Refresh().NoAwait());
  }

  void HandleAppSettingsChanged(AppSettings newAppSettings)
  {
    static string ProcessName(MonitoredProcess p) => p.Name;

    _appSettings = newAppSettings;
    PeriodUpdateRecreate(newAppSettings.RunningProcessesUpdatePeriod);

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
        ?.Do(cp =>
        {
          p.AffinityValue = (nint)AffinityApi.BitmaskFrom(cp.AffinityMode, cp.AffinityValue);
          p.AffinityApplyingMode = cp.AffinityApplyingMode;
          p.IsCaseSensitive = cp.IsCaseSensitive;
        });
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

    static Process[] GetSourceProcesses(MonitoredProcess p, Process[]? whereToFind = null)
    {
      static string ProcessNameV2(string processName)
        => processName.EndsWith(".exe")
          ? processName.Remove(".exe")
          : $"{processName}.exe";

      static Process[]? CaseSensitiveMatch(string processName, AffinityApplyingMode affinityApplyingMode) =>
        affinityApplyingMode switch
        {
          AffinityApplyingMode.FirstWithMatchedName => Process.GetProcessesByName(processName)
            .FirstOrDefault()
            .Pipe(p => p is null ? null : new[] { p }),

          AffinityApplyingMode.AllWithMatchedName => Process.GetProcessesByName(processName)
            .Pipe(ps => ps is [] ? null : ps),

          _ => throw new ArgumentOutOfRangeException()
        };

      static Process[]? CaseInsensitiveMatch(string processName, AffinityApplyingMode affinityApplyingMode,
        Process[]? whereToFind = null)
      {
        whereToFind = whereToFind switch
        {
          null or [] => Process.GetProcesses(),
          _ => whereToFind
        };

        return affinityApplyingMode switch
        {
          AffinityApplyingMode.FirstWithMatchedName => whereToFind
            .FirstOrDefault(p => p.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
            .Pipe(p => p is null ? null : new[] { p }),

          AffinityApplyingMode.AllWithMatchedName => whereToFind
            .Where(p => p.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
            .ToArray()
            .Pipe(ps => ps is [] ? null : ps),

          _ => throw new ArgumentOutOfRangeException()
        };
      }

      var processName = p.Name;
      var applyingMode = p.AffinityApplyingMode;
      var processName2 = ProcessNameV2(p.Name);

      return p.IsCaseSensitive switch
      {
        true => CaseSensitiveMatch(processName, applyingMode)
                ?? CaseSensitiveMatch(processName2, applyingMode)
                ?? Array.Empty<Process>(),

        false => CaseInsensitiveMatch(processName, applyingMode, whereToFind) 
                 ?? CaseInsensitiveMatch(processName2, applyingMode, whereToFind)
                 ?? Array.Empty<Process>(),
      };
    }

    static MonitoredProcess.StateType SetAffinityAndGetStateType(nint affinityValue, Process[] sourceProcesses) =>
      sourceProcesses switch
      {
        [] => MonitoredProcess.StateType.NotRunning,
        _ => sourceProcesses
          .Select(p => TrySetAffinity(p, affinityValue))
          .ToArray()
          .Contains(MonitoredProcess.StateType.AffinityApplied)
          ? MonitoredProcess.StateType.AffinityApplied
          : MonitoredProcess.StateType.AffinityCantBeSet
      };

    var processesCopy = Processes.ToArray();
    var existingProcesses = Processes.Any(p => !p.IsCaseSensitive) ? Process.GetProcesses() : null;
    var processesStateTypeToSet = await processesCopy
      .AsParallel()
      .AsOrdered()
      .Select(mp => (AffinityToSet: mp.AffinityValue, Source: GetSourceProcesses(mp, existingProcesses)))
      .Select(tuple => SetAffinityAndGetStateType(tuple.AffinityToSet, tuple.Source))
      .PipeUsingTaskRun(q => q.ToArray());

    processesCopy
      .Zip(processesStateTypeToSet)
      .ForEach((mp, state) => mp.State = state);
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
      ? SelectedProcesses is [_]
      : SelectedProcesses is [_] || p is not null;

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
      ? SelectedProcesses is [_]
      : SelectedProcesses is [_] || p is not null;

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
}