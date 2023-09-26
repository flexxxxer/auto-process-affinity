using UI.ViewModels;

using Avalonia.ReactiveUI;

using ReactiveUI;

using Splat;

namespace UI.Views;

public partial class MainView : ReactiveUserControl<MainViewModel>
{
  public MainView()
  {
    InitializeComponent();
    this.WhenActivated(d =>
    {
      DataContext = DataContext switch
      {
        { } mvm when mvm is MainViewModel or DesignMainViewModel => mvm,
        _ => Locator.Current.GetRequiredService<MainViewModel>()
      };
    });
  }
}
