using Domain;

using UI.DomainWrappers;

using Avalonia;
using Avalonia.Styling;

using Microsoft.Extensions.Options;
using ReactiveUI;
using System.Reactive.Disposables;
using CommunityToolkit.Mvvm.Input;

namespace UI.ViewModels;

public interface ISettingsViewModel
{
  IRelayCommand GoBackCommand { get; }
}

public sealed partial class SettingsViewModel : RoutableAndActivatableViewModelBase, ISettingsViewModel
{
  public SettingsViewModel(AppSettingChangeService settingChangeService,
    IOptions<AppSettings> appSettings) 
  {

    this.WhenActivated((CompositeDisposable d) =>
    {

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
  public DesignSettingsViewModel()
  {
    Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
  }

  [RelayCommand]
  void GoBack()
  { }
}