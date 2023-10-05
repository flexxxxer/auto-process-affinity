using System;
using System.Linq;
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

public class MainViewModel : ViewModelBase, IMainViewModel, IActivatableViewModel
{
  public ViewModelActivator Activator { get; } = new();

  public RoutingState Router { get; } = new RoutingState();

  public MainViewModel(MainWindowViewModel mainWindowViewModel) 
  {
    // ReSharper disable once AsyncVoidLambda
    this.WhenActivated(async (CompositeDisposable d) =>
    {
      Router.CurrentViewModel
        .Where(vm => vm is not null)
        .Select(vm => vm!)
        .Subscribe(vm =>
        {
          mainWindowViewModel.WindowTitleText = vm switch
          {
            AddProcessViewModel => "New process rule",
            SelectCurrentlyRunnableProcessViewModel => "Selecting process",
            ISettingsViewModel => "Settings",
            StartupViewModel or _ => mainWindowViewModel.DefaultWindowTitleText,
          };
        })
        .DisposeWith(d);

      _ = await Locator.Current
        .GetRequiredService<StartupViewModel>()
        .RouteThrought(Router);
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

