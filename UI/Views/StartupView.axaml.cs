using UI.ViewModels;

using Avalonia.ReactiveUI;

namespace UI;

public partial class StartupView : ReactiveUserControl<StartupViewModel>
{
  public StartupView()
  {
    InitializeComponent();
  }
}