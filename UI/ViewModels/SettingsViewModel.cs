using Avalonia;
using Avalonia.Styling;

using ReactiveUI;

namespace UI.ViewModels;

public interface ISettingsViewModel
{
}

public class SettingsViewModel : ViewModelBase, ISettingsViewModel, IActivatableViewModel
{
  public ViewModelActivator Activator { get; } = new();

  public SettingsViewModel() { }
}

public sealed partial class DesignSettingsViewModel : ViewModelBase, ISettingsViewModel
{
  public DesignSettingsViewModel()
  {
    if (App.IsDesignMode) Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
  }
}