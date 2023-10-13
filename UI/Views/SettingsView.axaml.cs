using UI.ViewModels;

using Avalonia.ReactiveUI;

namespace UI;

public partial class SettingsView : ReactiveUserControl<ISettingsViewModel>
{
  public SettingsView()
  {
    InitializeComponent();
  }
}