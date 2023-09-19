using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;

using ReactiveUI;

namespace UI.ViewModels;

public class ViewModelBase : ObservableObject, IReactiveObject
{
  public void RaisePropertyChanged(PropertyChangedEventArgs args) => OnPropertyChanged(args);

  public void RaisePropertyChanging(PropertyChangingEventArgs args) => OnPropertyChanging(args);
}
