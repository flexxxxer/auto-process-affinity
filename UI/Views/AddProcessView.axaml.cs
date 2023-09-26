using UI.ViewModels;

using Avalonia.ReactiveUI;

namespace UI.Views;

public partial class AddProcessView : ReactiveUserControl<AddProcessViewModel>
{
  public AddProcessView()
  {
    InitializeComponent();
  }
}