using UI.ViewModels;

using Avalonia.ReactiveUI;

namespace UI;

public partial class AddProcessView : ReactiveUserControl<AddProcessViewModel>
{
  public AddProcessView()
  {
    InitializeComponent();
  }
}