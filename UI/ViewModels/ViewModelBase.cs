using System.ComponentModel;

using ReactiveUI;

using CommunityToolkit.Mvvm.ComponentModel;

namespace UI.ViewModels;

public abstract class ViewModelBase : ObservableObject, IReactiveObject
{
  public void RaisePropertyChanged(PropertyChangedEventArgs args) => OnPropertyChanged(args);

  public void RaisePropertyChanging(PropertyChangingEventArgs args) => OnPropertyChanging(args);
}
