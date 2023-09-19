using Domain;

using Avalonia;
using Avalonia.Styling;

using ReactiveUI;

using Splat;

namespace UI.ViewModels;

public interface IAddProcessViewModel
{

}

public class AddProcessViewModel : ViewModelBase, IAddProcessViewModel, IActivatableViewModel, IRoutableViewModel
{
  public ViewModelActivator Activator { get; } = new();

  public string? UrlPathSegment => nameof(AddProcessViewModel).RemoveVmPostfix();

  public IScreen HostScreen { get; }

  public AddProcessViewModel(IScreen screen) 
  {
    HostScreen = screen;
  }
}

public sealed partial class DesignAddProcessViewModel : ViewModelBase, IAddProcessViewModel
{
  public DesignAddProcessViewModel()
  {
    if (App.IsDesignMode) Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
  }
}