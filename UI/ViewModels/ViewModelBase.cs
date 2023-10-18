using System.ComponentModel;

using ReactiveUI;

using CommunityToolkit.Mvvm.ComponentModel;

using Splat;

namespace UI.ViewModels;

public abstract class ViewModelBase : ObservableObject, IReactiveObject
{
  public void RaisePropertyChanged(PropertyChangedEventArgs args) => OnPropertyChanged(args);

  public void RaisePropertyChanging(PropertyChangingEventArgs args) => OnPropertyChanging(args);
}

public abstract class RoutableViewModelBase : ViewModelBase, IRoutableViewModel
{
  public string UrlPathSegment => GetType().Name;

  public IScreen HostScreen { get; init; } = Locator.Current.GetRequiredService<IScreen>();
}

public abstract class ActivatableViewModelBase : ViewModelBase, IActivatableViewModel
{
  public ViewModelActivator Activator { get; } = new();
}

public abstract class RoutableAndActivatableViewModelBase : RoutableViewModelBase, IActivatableViewModel
{
  public ViewModelActivator Activator { get; } = new();
}