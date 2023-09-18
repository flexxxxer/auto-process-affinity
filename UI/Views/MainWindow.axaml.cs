using UI.ViewModels;

using Avalonia.ReactiveUI;

namespace UI.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
  public MainWindow()
  {
    InitializeComponent();
  }
}
