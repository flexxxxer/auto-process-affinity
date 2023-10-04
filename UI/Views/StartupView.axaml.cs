using UI.ViewModels;

using Avalonia.ReactiveUI;

namespace UI.Views;

public partial class StartupView : ReactiveUserControl<IStartupViewModel>
{
  public StartupView()
  {
    InitializeComponent();
  }
}