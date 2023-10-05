using Domain;

using System.Diagnostics;
using System.Linq;

using Avalonia;

using ReactiveUI;

using CommunityToolkit.Mvvm.ComponentModel;

using Hardware.Info;
using UI.DomainWrappers;
using Domain.Infrastructure;

namespace UI.ViewModels;

public interface IMainWindowViewModel : IScreen
{
  double WindowHeight { get; }

  double WindowWidth { get; }

  string WindowTitleText { get; }
}

public partial class MainWindowViewModel : ViewModelBase, IMainWindowViewModel, IActivatableViewModel
{
  [ObservableProperty] double _windowHeight = 10;
  [ObservableProperty] double _windowWidth = 10;
  [ObservableProperty] string _windowTitleText = "";

  public string DefaultWindowTitleText { get; }

  public ViewModelActivator Activator { get; } = new();

  public MainWindowViewModel(HardwareInfo hwInfo, AdminPrivilegesStatus privilegesStatus) 
  {
    var defaultTitlePostfix = OSTypeApi.CurrentOS switch
    {
      OSType.Linux when privilegesStatus.IsAdmin => " (Root)",
      OSType.Windows when privilegesStatus.IsAdmin => " (Administrator)",
      _ => ""
    };

    DefaultWindowTitleText = Process.GetCurrentProcess()
      .ProcessName.Remove(".exe")
      + defaultTitlePostfix;

    WindowTitleText = DefaultWindowTitleText;

    hwInfo.RefreshVideoControllerList();
    var verticalRes = hwInfo
        .VideoControllerList
        .MaxBy(vc => vc.CurrentVerticalResolution)
        ?.CurrentVerticalResolution ?? 1080;

    WindowHeight = verticalRes / 1.8;
    WindowWidth = WindowHeight / 1.5;
  }

  // fake one router: MainWindowViewModel not used as IScreen but implements it cause
  // if not then will be exception when MainWindow.DataContext will be set (ReactiveUI side problem)
  RoutingState IScreen.Router => null!;
}

public sealed class DesignMainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
  public double WindowHeight { get; } = 400;

  public double WindowWidth { get; } = 266;

  public string WindowTitleText { get; } = Application.Current?.Name ?? "";

  // fake one router: MainWindowViewModel not used as IScreen but implements it cause
  // if not then will be exception when MainWindow.DataContext will be set (ReactiveUI side problem)
  RoutingState IScreen.Router => null!;
}