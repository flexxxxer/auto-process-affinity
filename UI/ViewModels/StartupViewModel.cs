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


namespace UI.ViewModels;

public interface IStartupViewModel
{
  ObservableCollection<MonitoredProcess> Processes { get; }

  IAsyncRelayCommand AddMonitoredProcessCommand { get; }

  IRelayCommand<MonitoredProcess?> RemoveMonitoredProcessCommand { get; }

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
  readonly AppSettingSaveService _settingSaveService;

  public StartupViewModel(IOptionsMonitor<AppSettings> appSettings, AppSettingSaveService settingSaveService, IScreen screen) 
  {
    HostScreen = screen;
    _appSettings = appSettings;
    _settingSaveService = settingSaveService;

    var handleAppSettingsChanged = HandleAppSettingsChanged;
    handleAppSettingsChanged = handleAppSettingsChanged.InvokeOn(RxApp.MainThreadScheduler);

    this.WhenActivated((CompositeDisposable d) =>
    {
      appSettings
        .Do(s => s.CurrentValue.Do(handleAppSettingsChanged))
        .Pipe(s => s.OnChange(handleAppSettingsChanged))
        ?.DisposeWith(d);

      Disposable
        .Create(HandleDeactivation)
        .DisposeWith(d);
    });
  }

  void HandleAppSettingsChanged(AppSettings newAppSettings)
  {
    _periodicUpdateStick?.Dispose();
    _periodicUpdateStick = RxApp.MainThreadScheduler
      .SchedulePeriodic(newAppSettings.RunningProcessesUpdatePeriod, Refresh);

    Processes.Clear();
    newAppSettings.ConfiguredProcesses
      .Select(MonitoredProcess.CreateFrom)
      .AddTo(Processes);
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
      var newAppSettingss = _appSettings.CurrentValue with
      {
        ConfiguredProcesses = _appSettings.CurrentValue.ConfiguredProcesses
          .Append(selectedProcess)
          .ToArray()
      };

      await _settingSaveService.SaveNewAppSetting(newAppSettingss);
    }
  }

  [RelayCommand]
  void RemoveMonitoredProcess(MonitoredProcess? p)
  {

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
  void RemoveMonitoredProcess(MonitoredProcess? p) { }

  [RelayCommand]
  void EditMonitoredProcess(MonitoredProcess? p) { }
}