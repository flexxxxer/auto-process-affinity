using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Styling;

using ReactiveUI;

using Splat;

namespace UI.ViewModels;

public interface IMainViewModel : IScreen
{
}

public partial class MainViewModel : ReactiveObject, IMainViewModel, IActivatableViewModel
{
  public ViewModelActivator Activator { get; } = new();

  public RoutingState Router { get; } = new RoutingState();

  public MainViewModel() 
  {
    this.WhenActivated(async (CompositeDisposable d) =>
    {
      _ = await Router.Navigate.Execute(Locator.Current.GetRequiredService<StartupViewModel>()!);
      ;
      // ReactiveCommand
      //   .CreateFromObservable(() => Router.Navigate.Execute(Locator.Current.GetRequiredService<StartupViewModel>()!))
      //   .DisposeWith(d)
      //   .Execute();
    });
  }
}

public sealed partial class DesignMainViewModel : ViewModelBase, IMainViewModel
{
  public RoutingState Router { get; } = new RoutingState();

  public DesignMainViewModel()
  {
    if (App.IsDesignMode) Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
  }
}

