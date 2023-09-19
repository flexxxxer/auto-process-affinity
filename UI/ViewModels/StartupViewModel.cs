using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain;
using DynamicData;
using Microsoft.Extensions.Options;
using ReactiveUI;
using Splat;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using UI.ViewModels.Entities;

namespace UI.ViewModels;

public interface IStartupViewModel
{
  ObservableCollection<MonitoredProcess> Processes { get; }

  IRelayCommand AddMonitoredProcessCommand { get; }

  IRelayCommand<MonitoredProcess?> RemoveMonitoredProcessCommand { get; }

  IRelayCommand<MonitoredProcess?> EditMonitoredProcessCommand { get; }
}

public partial class StartupViewModel : ViewModelBase, IStartupViewModel, IActivatableViewModel, IRoutableViewModel
{
  public ViewModelActivator Activator { get; } = new();

  public string? UrlPathSegment { get; } = nameof(StartupViewModel).RemoveVmPostfix();

  public IScreen HostScreen { get; }

  [ObservableProperty] ObservableCollection<MonitoredProcess> _processes = new();
  readonly IOptionsMonitor<AppSettings> _appSettings;

  public StartupViewModel(IOptionsMonitor<AppSettings> appSettings) 
  {
    HostScreen = Locator.Current.GetRequiredService<IScreen>();
    _appSettings = appSettings;

    this.WhenActivated((CompositeDisposable d) =>
    {
      _appSettings
        .OnChange(HandleAppSettingsChanged)
        ?.DisposeWith(d);
    });
  }

  void HandleAppSettingsChanged(AppSettings newAppSettings)
  {

  }

  [RelayCommand]
  void AddMonitoredProcess()
  {
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
  void AddMonitoredProcess() { }

  [RelayCommand]
  void RemoveMonitoredProcess(MonitoredProcess? p) { }

  [RelayCommand]
  void EditMonitoredProcess(MonitoredProcess? p) { }
}