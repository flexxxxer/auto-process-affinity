using System;
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

public class MainViewModel : ActivatableViewModelBase, IMainViewModel
{
  public RoutingState Router { get; } = new();

  public MainViewModel(MainWindowViewModel mainWindowViewModel) 
  {
    // ReSharper disable once AsyncVoidLambda
    this.WhenActivated(async d =>
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
            SettingsViewModel => "Settings",
            StartupViewModel or _ => mainWindowViewModel.DefaultWindowTitleText,
          };
        })
        .DisposeWith(d);

      _ = await Locator.Current
        .GetRequiredService<StartupViewModel>()
        .RouteThrough(Router);
    });
  }
}

public sealed class DesignMainViewModel : ViewModelBase, IMainViewModel
{
  public RoutingState Router { get; } = new();

  public DesignMainViewModel()
  {
    Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
  }
}

