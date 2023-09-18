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

public sealed partial class DesignStartupViewModel : ViewModelBase, ISettingsViewModel
{

}