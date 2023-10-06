using System;

namespace Domain;

public enum AffinityMode
{
  AllEven,
  FirstNEven,
  FirstN,
  LastN,
  CustomBitmask,
}

public sealed record ConfiguredProcess
{
  public string Name { get; init; } = "";

  public AffinityMode AffinityMode { get; init; }

  public long AffinityValue { get; init; }
}

public enum StartupLocationMode
{
  RememberLast,
  Default,
  CenterScreen,
}

public enum StartupSizeMode
{
  RememberLast,
  Specified,
  Optimal,
}

public sealed record StartupOptions
{
  public sealed record StartupLocationValues
  {
    public int X { get; init; }

    public int Y { get; init; }
  }

  public sealed record StartupSizeValues
  {
    public double Height { get; init; }

    public double Width { get; init; }
  }

  public bool Autostart { get; init; }

  public bool Minimized { get; init; }

  public StartupLocationMode StartupLocationMode { get; init; }

  public StartupLocationValues StartupLocation { get; init; } = new();

  public StartupSizeMode StartupSizeMode { get; init; }

  public StartupSizeValues StartupSize { get; init; } = new();

  public static StartupOptions Default => new()
  {
    Autostart = false,
    Minimized = false,
    StartupLocationMode = StartupLocationMode.CenterScreen,
    StartupLocation = new(),
    StartupSizeMode = StartupSizeMode.Optimal,
    StartupSize = new(),
  };
}

public sealed record AppSettings
{
  public TimeSpan RunningProcessesUpdatePeriod { get; init; } = TimeSpan.FromSeconds(2);

  public ConfiguredProcess[] ConfiguredProcesses { get; init; } = Array.Empty<ConfiguredProcess>();

  public StartupOptions StartupOptions { get; init; } = StartupOptions.Default;

  public static AppSettings Default => new();
}

public sealed record AppSettingsWrapperForHostOptions
{
  public required AppSettings AppSettings { get; init; }
}