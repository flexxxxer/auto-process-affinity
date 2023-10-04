using UI.ViewModels;

using Avalonia.ReactiveUI;

namespace UI.Views;

public partial class AddProcessView : ReactiveUserControl<IAddProcessViewModel>
{
  public AddProcessView()
  {
    InitializeComponent();
  }
}