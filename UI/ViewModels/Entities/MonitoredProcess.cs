using Domain;

using CommunityToolkit.Mvvm.ComponentModel;

namespace UI.ViewModels.Entities;

public sealed partial class MonitoredProcess : ObservableObject
{
  public enum StateType
  {
    NotRunning,
    NotYetApplied,
    AffinityCantBeSet,
    AffinityApplied,
  }

  [ObservableProperty] string _name = "";
  [ObservableProperty] StateType _state = StateType.NotRunning;
  [ObservableProperty] nint _affinityValue;

  public static MonitoredProcess CreateFrom(ConfiguredProcess configuredProcess)
    => new()
    {
      Name = configuredProcess.Name,
      AffinityValue = (nint)configuredProcess.AffinityValue,
      State = StateType.NotYetApplied
    };
}
