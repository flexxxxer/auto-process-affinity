using ReactiveUI;

namespace UI.ViewModels;

public interface IAddProcessViewModel
{

}

public class AddProcessViewModel : ViewModelBase, IAddProcessViewModel, IActivatableViewModel
{
  public ViewModelActivator Activator { get; } = new();

  public AddProcessViewModel() { }
}

public sealed partial class DesignAddProcessViewModel : ViewModelBase, IAddProcessViewModel
{

}