using UI.ViewModels;

using Avalonia.ReactiveUI;
using Avalonia.Interactivity;
using Avalonia.Input;
using System.Linq;
using ReactiveUI;

namespace UI.Views;

public partial class AddProcessView : ReactiveUserControl<IAddProcessViewModel>
{
  public AddProcessView()
  {
    InitializeComponent();

    var textBoxesWhereOnlyDigitsAllowed = new[]
    {
      EvenAffinityModeFirstNValueTextBox,
      FirstNAffinityModeValueTextBox,
      LastNAffinityModeValueTextBox,
    };

    foreach(var tb in textBoxesWhereOnlyDigitsAllowed)
    {
      tb.AddHandler(TextInputEvent, TextBox_TextInput_AllowOnlyDigits, RoutingStrategies.Tunnel);
    }
  }

  private void TextBox_TextInput_AllowOnlyDigits(object? _, TextInputEventArgs e)
  {
    static bool NotDigit(char c) => char.IsDigit(c);
    e.Handled = e.Text?.Any(NotDigit) is not true;
  }

  protected override void OnLoaded(RoutedEventArgs e)
  {
    ProcessNameTextBox.Focus();
  }
}