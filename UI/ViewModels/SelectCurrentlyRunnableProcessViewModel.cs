using ReactiveUI;

namespace UI.ViewModels;

public interface ISelectCurrentlyRunnableProcessViewModel
{

}

public class SelectCurrentlyRunnableProcessViewModel : ViewModelBase,
  ISelectCurrentlyRunnableProcessViewModel, IActivatableViewModel
{
  public ViewModelActivator Activator { get; } = new();

  public SelectCurrentlyRunnableProcessViewModel() { }
}

public sealed partial class DesignSelectCurrentlyRunnableProcessViewModel : ViewModelBase,
  ISelectCurrentlyRunnableProcessViewModel
{

}