using ReactiveUI;

namespace UI.ViewModels;

public interface IMainViewModel
{
}

public partial class MainViewModel : ViewModelBase, IMainViewModel, IActivatableViewModel
{
  public ViewModelActivator Activator { get; } = new();

  public MainViewModel() { }
}

public sealed partial class DesignMainViewModel : ViewModelBase, IMainViewModel
{
  
}

