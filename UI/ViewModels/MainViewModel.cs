using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;

using ReactiveUI;
using ReactiveUI.ExtendedRouting;

using CommunityToolkit.Mvvm.Input;

using Splat;

namespace UI.ViewModels;

public interface IMainViewModel : IScreen
{
  IAsyncRelayCommand GoToSettingsCommand { get; }

  IRelayCommand ExitCommand { get; }
}

public partial class MainViewModel : ActivatableViewModelBase, IMainViewModel
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
  
  [RelayCommand]
  async Task GoToSettings()
    => await Locator.Current
      .GetRequiredService<SettingsViewModel>()
      .RouteThrough(this);

  [RelayCommand]
  void Exit() => Application.Current
    ?.ApplicationLifetime
    ?.TryCastTo<IClassicDesktopStyleApplicationLifetime>()
    ?.Shutdown();
}

public sealed partial class DesignMainViewModel : ViewModelBase, IMainViewModel
{
  public RoutingState Router { get; } = new();

  public DesignMainViewModel()
  {
    Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
  }

  [RelayCommand]
  Task GoToSettings() => Task.CompletedTask;

  [RelayCommand]
  void Exit() { }
}

