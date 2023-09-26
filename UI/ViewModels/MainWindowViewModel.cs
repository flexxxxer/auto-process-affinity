using System.Linq;

using ReactiveUI;

using CommunityToolkit.Mvvm.ComponentModel;

using Hardware.Info;

namespace UI.ViewModels;

public interface IMainWindowViewModel : IScreen
{
  double WindowHeight { get; }

  double WindowWidth { get; }
}

public partial class MainWindowViewModel : ViewModelBase, IMainWindowViewModel, IActivatableViewModel
{
  [ObservableProperty] double _windowHeight = 10;
  [ObservableProperty] double _windowWidth = 10;

  public ViewModelActivator Activator { get; } = new();

  public MainWindowViewModel(HardwareInfo hwInfo) 
  {
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

  // fake one router: MainWindowViewModel not used as IScreen but implements it cause
  // if not then will be exception when MainWindow.DataContext will be set (ReactiveUI side problem)
  RoutingState IScreen.Router => null!;
}