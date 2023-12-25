using Domain;
using Domain.Infrastructure;

using System.Diagnostics;

using Avalonia;
using Avalonia.Styling;

using CommunityToolkit.Mvvm.Input;

namespace UI.ViewModels;

public interface IAboutViewModel
{
  IRelayCommand<string> OpenUrlCommand { get; }
  IRelayCommand GoBackCommand { get; }
}

public sealed partial class AboutViewModel : RoutableAndActivatableViewModelBase, IAboutViewModel
{
  [RelayCommand]
  void OpenUrl(string url)
  {
    var currentOs = OsTypeApi.CurrentOs;
    var startInfo = new ProcessStartInfo(url)
      .DoIf(currentOs is OsType.Windows, si => si.UseShellExecute = true)
      .DoIf(currentOs is OsType.Linux, si => (si.FileName, si.Arguments) = ("x-www-browser", url));

    using var _ = Process.Start(startInfo);
  }

  [RelayCommand]
  void GoBack()
  {
    HostScreen.Router.NavigateBack.Execute();
  }
}

public sealed partial class DesignAboutViewModel : ViewModelBase, IAboutViewModel
{
  public DesignAboutViewModel()
  {
    Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
  }

  [RelayCommand]
  void OpenUrl(string _) { }
  
  [RelayCommand]
  void GoBack() { }
}