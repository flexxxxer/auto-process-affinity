using UI.ViewModels;

using Avalonia.ReactiveUI;

namespace UI.Views;

public partial class MainWindow : ReactiveWindow<IMainWindowViewModel>
{
  public MainWindow()
  {
    InitializeComponent();
  }
}
