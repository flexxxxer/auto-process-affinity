using Domain;

using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Diagnostics;

using Avalonia;
using Avalonia.Threading;
using Avalonia.Styling;

using ReactiveUI;
using ReactiveUI.ExtendedRouting;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Splat;

using Microsoft.Extensions.Options;

using UI.ViewModels.Entities;

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

  readonly DispatcherTimer _timer;
  readonly object _syncObj = new();

  public StartupViewModel(IOptionsMonitor<AppSettings> appSettings, IScreen screen) 
  {
    HostScreen = screen;
    appSettings.CurrentValue.Do(HandleAppSettingsChanged);

    _timer = new() 
    {
      Interval = appSettings.CurrentValue.RunningProcessesUpdatePeriod
    };

    this.WhenActivated((CompositeDisposable d) =>
    {
      _timer.Tick += Refresh;
      _timer.Start();

      appSettings
        .OnChange(HandleAppSettingsChanged)
        ?.DisposeWith(d);
      Disposable
        .Create(HandleDeactivation)
        .DisposeWith(d);
    });
  }

  void HandleAppSettingsChanged(AppSettings newAppSettings)
  {
    newAppSettings.ConfiguredProcesses
      .Select(MonitoredProcess.CreateFrom)
      .AddTo(Processes);
  }

  void HandleDeactivation()
  {
    _timer.Tick -= Refresh;
    _timer.Stop();
  }

  void Refresh(object? _, EventArgs __)
  {
    static MonitoredProcess.StateType TrySetAffinity(Process p, long affinity)
      => ProcessApi.TrySetProcessorAffinity(p, (nint)affinity)
          ? MonitoredProcess.StateType.AffinityApplied
          : MonitoredProcess.StateType.AffinityCantBeSet;

    foreach(var p in Processes)
    {
      var normalizedName = p.Name.TrimEnd(".exe");

      var sourceProcess = Process.GetProcessesByName(p.Name).FirstOrDefault() 
        ?? Process.GetProcessesByName(normalizedName).FirstOrDefault();

      p.State = sourceProcess switch
      {
        null => MonitoredProcess.StateType.NotRunning,
        not null => TrySetAffinity(sourceProcess, p.AffinityValue),
      };
    }
  }

  [RelayCommand]
  async Task AddMonitoredProcess()
  {
    var processesVm = await HostScreen
      .NavigateTo(Locator.Current.GetRequiredService<AddProcessViewModel>());

    var selectedProcess = await processesVm.Result;

    if (selectedProcess is not null) Processes.Add(MonitoredProcess.CreateFrom(selectedProcess));
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