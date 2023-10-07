using UI.ViewModels;

using Avalonia.ReactiveUI;
using Avalonia.Interactivity;

namespace UI.Views;

public partial class AddProcessView : ReactiveUserControl<IAddProcessViewModel>
{
  public AddProcessView()
  {
    InitializeComponent();
  }

  protected override void OnLoaded(RoutedEventArgs e)
  {
    ProcessNameTextBox.Focus();
  }
}