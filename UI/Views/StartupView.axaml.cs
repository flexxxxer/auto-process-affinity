using UI.ViewModels;

using Avalonia.ReactiveUI;

namespace UI.Views;

public partial class StartupView : ReactiveUserControl<StartupViewModel>
{
  public StartupView()
  {
    InitializeComponent();
  }
}