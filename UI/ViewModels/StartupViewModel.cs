using ReactiveUI;

namespace UI.ViewModels;

public interface IStartupViewModel
{
}

public class StartupViewModel : ViewModelBase, IStartupViewModel, IActivatableViewModel
{
  public ViewModelActivator Activator { get; } = new();

  public StartupViewModel() { }
}

public sealed partial class DesignStartupViewModel : ViewModelBase, IStartupViewModel
{

}