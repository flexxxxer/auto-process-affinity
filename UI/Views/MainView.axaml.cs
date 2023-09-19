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
        MainViewModel mvm => mvm,
        _ => Locator.Current.GetRequiredService<MainViewModel>()
      };
    });
  }
}
