using Domain;
using Domain.Infrastructure;

using UI.DomainWrappers;

using System;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;

using Avalonia;
using Avalonia.Styling;

using ReactiveUI;

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

using DynamicData.Binding;

using Microsoft.Extensions.Options;

namespace UI.ViewModels;

public interface ISettingsViewModel
{
  int ProcessesUpdateRateInSeconds { get; set; }
  bool UseOldSchoolAddEditStyle { get; set; }
  bool HideProcessDescription { get; set; }
  bool HideInTrayInsteadOfClosing { get; set; }
  bool LoadOnSystemStartup { get; set; }
  bool RunMinimized { get; set; }
  StartupLocationMode StartupLocationMode { get; set; }
  StartupSizeMode StartupSizeMode { get; set; }
  double StartupWindowWidth { get; set; }
  double StartupWindowHeight { get; set; }
  bool RunWithAdministratorOrRootPrivileges { get; set; }
  ReadOnlyObservableCollection<StartupLocationMode> StartupLocationModes { get; }
  ReadOnlyObservableCollection<StartupSizeMode> StartupSizeModes { get; }

  IRelayCommand GoBackCommand { get; }
}

public sealed partial class SettingsViewModel : RoutableAndActivatableViewModelBase, ISettingsViewModel
{
  [ObservableProperty] int _processesUpdateRateInSeconds;
  [ObservableProperty] bool _useOldSchoolAddEditStyle;
  [ObservableProperty] bool _hideProcessDescription;
  [ObservableProperty] bool _hideInTrayInsteadOfClosing;
  [ObservableProperty] bool _loadOnSystemStartup;
  [ObservableProperty] bool _runMinimized;
  [ObservableProperty] StartupLocationMode _startupLocationMode;
  [ObservableProperty] StartupSizeMode _startupSizeMode;
  [ObservableProperty] double _startupWindowWidth;
  [ObservableProperty] double _startupWindowHeight;
  [ObservableProperty] bool _runWithAdministratorOrRootPrivileges;
  [ObservableProperty] ReadOnlyObservableCollection<StartupLocationMode> _startupLocationModes;
  [ObservableProperty] ReadOnlyObservableCollection<StartupSizeMode> _startupSizeModes;

  readonly AppSettingChangeService _settingChangeService;

  public SettingsViewModel(AppSettingChangeService settingChangeService,
    IOptionsSnapshot<AppSettings> appSettings) 
  {
    _settingChangeService = settingChangeService;
    StartupLocationModes = new(new(Enum.GetValues<StartupLocationMode>()));
    StartupSizeModes = new(new(Enum.GetValues<StartupSizeMode>()));

    FillFromAppSettings(appSettings.Value);
    this.WhenActivated((CompositeDisposable d) =>
    {
      this.WhenAnyPropertyChanged(Array.Empty<string>())
        .Throttle(TimeSpan.FromSeconds(0.5))
        .Subscribe(_ => UpdateAppSettings())
        .DisposeWith(d);
    });
  }

  void FillFromAppSettings(AppSettings appSettings)
  {
    ProcessesUpdateRateInSeconds = (int)appSettings.RunningProcessesUpdatePeriod.TotalSeconds;
    UseOldSchoolAddEditStyle = appSettings.UxOptions.UseOldSchoolAddEditStyle;
    HideProcessDescription = appSettings.UxOptions.HideProcessDescriptionFromSelectingProcessView;
    HideInTrayInsteadOfClosing = appSettings.UxOptions.HideToTrayInsteadOfClosing;
    LoadOnSystemStartup = appSettings.StartupOptions.Autostart;
    RunMinimized = appSettings.StartupOptions.Minimized;
    StartupLocationMode = appSettings.StartupOptions.StartupLocationMode;
    StartupSizeMode = appSettings.StartupOptions.StartupSizeMode;
    StartupWindowWidth = appSettings.StartupOptions.StartupSize.Width;
    StartupWindowHeight = appSettings.StartupOptions.StartupSize.Height;
    RunWithAdministratorOrRootPrivileges = appSettings.SystemLevelStartupOptions.RunWithAdminOrRootPrivileges;
  }

  void UpdateAppSettings()
  {
    AppSettings Update(AppSettings appSettings) => appSettings with
    {
      RunningProcessesUpdatePeriod = TimeSpan.FromSeconds(ProcessesUpdateRateInSeconds),
      UxOptions = appSettings.UxOptions with
      {
        UseOldSchoolAddEditStyle = UseOldSchoolAddEditStyle,
        HideProcessDescriptionFromSelectingProcessView = HideProcessDescription,
        HideToTrayInsteadOfClosing = HideInTrayInsteadOfClosing,
      },
      StartupOptions = appSettings.StartupOptions with
      {
        Autostart = LoadOnSystemStartup,
        Minimized = RunMinimized,
        StartupLocationMode = StartupLocationMode,
        StartupSizeMode = StartupSizeMode,
        StartupSize = new() { Width = StartupWindowWidth, Height = StartupWindowHeight },
      },
      SystemLevelStartupOptions = appSettings.SystemLevelStartupOptions with
      {
        RunWithAdminOrRootPrivileges = RunWithAdministratorOrRootPrivileges,
      }
    };

    _settingChangeService
      .MakeChangeAsync(Update)
      .NoAwait();
  }

  [RelayCommand]
  void GoBack()
  {
    HostScreen.Router.NavigateBack.Execute();
  }
}

public sealed partial class DesignSettingsViewModel : ViewModelBase, ISettingsViewModel
{
  [ObservableProperty] int _processesUpdateRateInSeconds;
  [ObservableProperty] bool _useOldSchoolAddEditStyle;
  [ObservableProperty] bool _hideProcessDescription;
  [ObservableProperty] bool _hideInTrayInsteadOfClosing;
  [ObservableProperty] bool _loadOnSystemStartup;
  [ObservableProperty] bool _runMinimized;
  [ObservableProperty] StartupLocationMode _startupLocationMode;
  [ObservableProperty] StartupSizeMode _startupSizeMode;
  [ObservableProperty] double _startupWindowWidth;
  [ObservableProperty] double _startupWindowHeight;
  [ObservableProperty] bool _runWithAdministratorOrRootPrivileges;
  [ObservableProperty] ReadOnlyObservableCollection<StartupLocationMode> _startupLocationModes;
  [ObservableProperty] ReadOnlyObservableCollection<StartupSizeMode> _startupSizeModes;

  public DesignSettingsViewModel()
  {
    StartupLocationModes = new(new(Enum.GetValues<StartupLocationMode>()));
    StartupSizeModes = new(new(Enum.GetValues<StartupSizeMode>()));
    Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
  }

  [RelayCommand]
  void GoBack()
  { }
}