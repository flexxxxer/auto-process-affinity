using UI.ViewModels;

using Avalonia.ReactiveUI;

namespace UI.Views;

public partial class SettingsView : ReactiveUserControl<ISettingsViewModel>
{
  public SettingsView()
  {
    InitializeComponent();
  }
}