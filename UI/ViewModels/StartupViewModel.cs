using Domain;

using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;

using Avalonia;
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

  public StartupViewModel(IOptionsMonitor<AppSettings> appSettings, IScreen screen) 
  {
    HostScreen = screen;
    appSettings.CurrentValue.Do(HandleAppSettingsChanged);

    this.WhenActivated((CompositeDisposable d) =>
    {
      appSettings
        .OnChange(HandleAppSettingsChanged)
        ?.DisposeWith(d);
    });
  }

  void HandleAppSettingsChanged(AppSettings newAppSettings)
  {
    newAppSettings.ConfiguredProcesses
      .Select(MonitoredProcess.CreateFrom)
      .AddTo(Processes);
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