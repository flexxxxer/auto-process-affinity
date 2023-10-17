using Domain;

using UI.DomainWrappers;

using Avalonia;
using Avalonia.Styling;

using Microsoft.Extensions.Options;
using ReactiveUI;
using System.Reactive.Disposables;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData.Binding;
using System;
using System.Reactive.Linq;
using System.Collections.ObjectModel;

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
  int StartupLocationX { get; set; }
  int StartupLocationY { get; set; }
  StartupSizeMode StartupSizeMode { get; set; }
  double StartupWindowWidth { get; set; }
  double StartupWindowHeight { get; set; }
  bool RunWithAdministratorOrRootPrivieges { get; set; }
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
  [ObservableProperty] int _startupLocationX;
  [ObservableProperty] int _startupLocationY;
  [ObservableProperty] StartupSizeMode _startupSizeMode;
  [ObservableProperty] double _startupWindowWidth;
  [ObservableProperty] double _startupWindowHeight;
  [ObservableProperty] bool _runWithAdministratorOrRootPrivieges;
  [ObservableProperty] ReadOnlyObservableCollection<StartupLocationMode> _startupLocationModes;
  [ObservableProperty] ReadOnlyObservableCollection<StartupSizeMode> _startupSizeModes;

  public SettingsViewModel(AppSettingChangeService settingChangeService,
    IOptionsSnapshot<AppSettings> appSettings) 
  {
    StartupLocationModes = new(new(Enum.GetValues<StartupLocationMode>()));
    StartupSizeModes = new(new(Enum.GetValues<StartupSizeMode>()));

    this.WhenActivated((CompositeDisposable d) =>
    {
      this.WhenAnyPropertyChanged(Array.Empty<string>())
        .Throttle(TimeSpan.FromSeconds(1))
        .Subscribe(_ => { })
        .DisposeWith(d);
    });
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
  [ObservableProperty] int _startupLocationX;
  [ObservableProperty] int _startupLocationY;
  [ObservableProperty] StartupSizeMode _startupSizeMode;
  [ObservableProperty] double _startupWindowWidth;
  [ObservableProperty] double _startupWindowHeight;
  [ObservableProperty] bool _runWithAdministratorOrRootPrivieges;
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