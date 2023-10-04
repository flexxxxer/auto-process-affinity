using UI.ViewModels;

using Avalonia.ReactiveUI;

using Splat;

namespace UI.Views;

public partial class MainView : ReactiveUserControl<IMainViewModel>
{
  public MainView()
  {
    InitializeComponent();
    DataContext = DataContext switch
    {
      not DesignMainViewModel when App.IsDesignMode => new DesignMainViewModel(),
      _ => Locator.Current.GetRequiredService<MainViewModel>()
    };
  }
}
