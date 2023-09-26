using System.Reactive.Disposables;
using System.Reactive.Linq;

using ReactiveUI;
using ReactiveUI.ExtendedRouting;

using Avalonia;
using Avalonia.Styling;

using Splat;

namespace UI.ViewModels;

public interface IMainViewModel : IScreen
{
}

public class MainViewModel : ReactiveObject, IMainViewModel, IActivatableViewModel
{
  public ViewModelActivator Activator { get; } = new();

  public RoutingState Router { get; } = new RoutingState();

  public MainViewModel() 
  {
    // ReSharper disable once AsyncVoidLambda
    this.WhenActivated(async (CompositeDisposable d) =>
    {
      _ = await Router.NavigateTo(Locator.Current.GetRequiredService<StartupViewModel>());
    });
  }
}

public sealed class DesignMainViewModel : ViewModelBase, IMainViewModel
{
  public RoutingState Router { get; } = new RoutingState();

  public DesignMainViewModel()
  {
    if (App.IsDesignMode) Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
  }
}

