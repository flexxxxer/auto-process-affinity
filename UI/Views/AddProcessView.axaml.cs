using UI.ViewModels;

using Avalonia.ReactiveUI;

using ReactiveUI;

namespace UI.Views;

public partial class AddProcessView : ReactiveUserControl<IAddProcessViewModel>
{
  public AddProcessView()
  {
    InitializeComponent();

    this.WhenActivated(_ =>
    {
      ProcessNameTextBox.Focus();
    });
  }
}