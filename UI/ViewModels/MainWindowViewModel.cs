using System.Linq;

using ReactiveUI;

using Hardware.Info;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Reactive.Linq;
using Splat;

namespace UI.ViewModels;

public interface IMainWindowViewModel
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
}

public sealed partial class DesignMainWindowViewModel : ViewModelBase, IMainWindowViewModel
{
  public double WindowHeight { get; } = 400;

  public double WindowWidth { get; } = 266;
}