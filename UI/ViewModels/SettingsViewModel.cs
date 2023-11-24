using Domain;
using Domain.Infrastructure;

using UI.DomainWrappers;

using System;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Linq;

using Avalonia;
using Avalonia.Styling;

using ReactiveUI;

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

using DynamicData.Binding;

// ReSharper disable WithExpressionModifiesAllMembers

namespace UI.ViewModels;

public interface ISettingsViewModel
{
  int? ProcessesUpdateRateInSeconds { get; set; }
  bool UseOldSchoolAddEditStyle { get; set; }
  bool HideProcessDescription { get; set; }
  bool HideInTrayInsteadOfClosing { get; set; }
  bool LoadOnSystemStartup { get; set; }
  StartupWindowState StartupWindowState { get; set; }
  StartupLocationMode StartupLocationMode { get; set; }
  StartupSizeMode StartupSizeMode { get; set; }
  double? StartupWindowWidth { get; set; }
  double? StartupWindowHeight { get; set; }
  bool RunWithAdministratorOrRootPrivileges { get; set; }
  bool ShowSystemTitleBar { get; set; }
  AppTheme Theme { get; set; }
  AppDarkThemeVariant DarkThemeVariant { get; set; }
  ReadOnlyObservableCollection<StartupLocationMode> StartupLocationModes { get; }
  ReadOnlyObservableCollection<StartupSizeMode> StartupSizeModes { get; }
  ReadOnlyObservableCollection<AppTheme> AppThemes { get; }
  ReadOnlyObservableCollection<AppDarkThemeVariant> DarkThemeVariants { get; }
  ObservableCollection<StartupWindowState> StartupWindowStates { get; }

  IRelayCommand GoBackCommand { get; }
}

public sealed partial class SettingsViewModel : RoutableAndActivatableViewModelBase, ISettingsViewModel
{
  [ObservableProperty] int? _processesUpdateRateInSeconds;
  [ObservableProperty] bool _useOldSchoolAddEditStyle;
  [ObservableProperty] bool _hideProcessDescription;
  [ObservableProperty] bool _hideInTrayInsteadOfClosing;
  [ObservableProperty] bool _loadOnSystemStartup;
  [ObservableProperty] StartupWindowState _startupWindowState;
  [ObservableProperty] StartupLocationMode _startupLocationMode;
  [ObservableProperty] StartupSizeMode _startupSizeMode;
  [ObservableProperty] double? _startupWindowWidth;
  [ObservableProperty] double? _startupWindowHeight;
  [ObservableProperty] bool _runWithAdministratorOrRootPrivileges;
  [ObservableProperty] bool _showSystemTitleBar;
  [ObservableProperty] AppTheme _theme;
  [ObservableProperty] AppDarkThemeVariant _darkThemeVariant;
  [ObservableProperty] ReadOnlyObservableCollection<StartupLocationMode> _startupLocationModes;
  [ObservableProperty] ReadOnlyObservableCollection<StartupSizeMode> _startupSizeModes;
  [ObservableProperty] ReadOnlyObservableCollection<AppTheme> _appThemes;
  [ObservableProperty] ReadOnlyObservableCollection<AppDarkThemeVariant> _darkThemeVariants;
  [ObservableProperty] ObservableCollection<StartupWindowState> _startupWindowStates;

  readonly AppSettingChangeService _settingChangeService;

  public SettingsViewModel(AppSettingChangeService settingChangeService) 
  {
    _settingChangeService = settingChangeService;
    StartupLocationModes = new(new(Enum.GetValues<StartupLocationMode>()));
    StartupSizeModes = new(new(Enum.GetValues<StartupSizeMode>()));
    AppThemes = new(new(Enum.GetValues<AppTheme>()));
    DarkThemeVariants = new(new(Enum.GetValues<AppDarkThemeVariant>()));
    StartupWindowStates = new(Enum.GetValues<StartupWindowState>().Except(new[] {StartupWindowState.MinimizedToTray}));

    FillFromAppSettings(settingChangeService.CurrentAppSettings);
    this.WhenActivated(d =>
    {
      this.WhenAnyPropertyChanged(Array.Empty<string>())
        .Throttle(TimeSpan.FromSeconds(0.25))
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
    StartupWindowState = appSettings.StartupOptions.StartupWindowState;
    StartupLocationMode = appSettings.StartupOptions.StartupLocationMode;
    StartupSizeMode = appSettings.StartupOptions.StartupSizeMode;
    StartupWindowWidth = appSettings.StartupOptions.StartupSize.Width;
    StartupWindowHeight = appSettings.StartupOptions.StartupSize.Height;
    RunWithAdministratorOrRootPrivileges = appSettings.SystemLevelStartupOptions.RunWithAdminOrRootPrivileges;
    ShowSystemTitleBar = appSettings.UiOptions.ShowSystemTitleBar;
    Theme = appSettings.UiOptions.Theme;
    DarkThemeVariant = appSettings.UiOptions.DarkThemeVariant;
  }

  partial void OnHideInTrayInsteadOfClosingChanged(bool value)
  {
    if (value)
    {
      StartupWindowStates.Add(StartupWindowState.MinimizedToTray);
    }
    else
    {
      if (StartupWindowState is StartupWindowState.MinimizedToTray)
      {
        StartupWindowState = StartupWindowState.Minimized;
      }
      StartupWindowStates.Remove(StartupWindowState.MinimizedToTray);
    }
  }

  void UpdateAppSettings()
  {
    AppSettings Update(AppSettings appSettings) => appSettings with
    {
      RunningProcessesUpdatePeriod = ProcessesUpdateRateInSeconds is not { } notNull 
        ? appSettings.RunningProcessesUpdatePeriod
        : TimeSpan.FromSeconds(notNull),

      UiOptions = appSettings.UiOptions with
      {
        ShowSystemTitleBar = ShowSystemTitleBar,
        Theme = Theme,
        DarkThemeVariant = DarkThemeVariant,
      },
      UxOptions = appSettings.UxOptions with
      {
        UseOldSchoolAddEditStyle = this.UseOldSchoolAddEditStyle,
        HideProcessDescriptionFromSelectingProcessView = this.HideProcessDescription,
        HideToTrayInsteadOfClosing = this.HideInTrayInsteadOfClosing,
      },
      StartupOptions = appSettings.StartupOptions with
      {
        Autostart = LoadOnSystemStartup,
        StartupWindowState = StartupWindowState,
        StartupLocationMode = StartupLocationMode,
        StartupSizeMode = StartupSizeMode,
        StartupSize = StartupWindowHeight is not { } notNullH || StartupWindowWidth is not { } notNullW
          ? appSettings.StartupOptions.StartupSize
          : new() { Height = notNullH, Width = notNullW }
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
  [ObservableProperty] int? _processesUpdateRateInSeconds;
  [ObservableProperty] bool _useOldSchoolAddEditStyle;
  [ObservableProperty] bool _hideProcessDescription;
  [ObservableProperty] bool _hideInTrayInsteadOfClosing;
  [ObservableProperty] bool _loadOnSystemStartup;
  [ObservableProperty] StartupWindowState _startupWindowState;
  [ObservableProperty] StartupLocationMode _startupLocationMode;
  [ObservableProperty] StartupSizeMode _startupSizeMode;
  [ObservableProperty] double? _startupWindowWidth;
  [ObservableProperty] double? _startupWindowHeight;
  [ObservableProperty] bool _runWithAdministratorOrRootPrivileges;
  [ObservableProperty] bool _showSystemTitleBar;
  [ObservableProperty] AppTheme _theme;
  [ObservableProperty] AppDarkThemeVariant _darkThemeVariant;
  [ObservableProperty] ReadOnlyObservableCollection<StartupLocationMode> _startupLocationModes;
  [ObservableProperty] ReadOnlyObservableCollection<StartupSizeMode> _startupSizeModes;
  [ObservableProperty] ReadOnlyObservableCollection<AppTheme> _appThemes;
  [ObservableProperty] ReadOnlyObservableCollection<AppDarkThemeVariant> _darkThemeVariants;
  [ObservableProperty] ObservableCollection<StartupWindowState> _startupWindowStates;
  
  public DesignSettingsViewModel()
  {
    StartupLocationModes = new(new(Enum.GetValues<StartupLocationMode>()));
    StartupSizeModes = new(new(Enum.GetValues<StartupSizeMode>()));
    AppThemes = new(new(Enum.GetValues<AppTheme>()));
    DarkThemeVariants = new(new(Enum.GetValues<AppDarkThemeVariant>()));
    StartupWindowStates = new(Enum.GetValues<StartupWindowState>());
    Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
  }

  [RelayCommand]
  void GoBack()
  { }
}