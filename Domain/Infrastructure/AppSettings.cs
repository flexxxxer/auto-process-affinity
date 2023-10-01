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
  public string Name { get; set; } = "";

  public AffinityMode AffinityMode { get; set; }

  public long AffinityValue { get; set; }
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
    public int X { get; set; }

    public int Y { get; set; }
  }

  public sealed record StartupSizeValues
  {
    public int Height { get; set; }

    public int Width { get; set; }
  }

  public bool Autostart { get; set; }

  public bool Minimized { get; set; }

  public StartupLocationMode StartupLocationMode { get; set; }

  public StartupLocationValues StartupLocation { get; set; } = new();

  public StartupSizeMode StartupSizeMode { get; set; }

  public StartupSizeValues StartupSize { get; set; } = new();

  public static StartupOptions Default => new()
  {
    Autostart = false,
    Minimized = false,
    StartupLocationMode = StartupLocationMode.Default,
    StartupLocation = new(),
    StartupSizeMode = StartupSizeMode.Optimal,
    StartupSize = new(),
  };
}

public sealed record AppSettings
{
  public TimeSpan RunningProcessesUpdatePeriod { get; set; } = TimeSpan.FromSeconds(2);

  public ConfiguredProcess[] ConfiguredProcesses { get; set; } = Array.Empty<ConfiguredProcess>();

  public StartupOptions StartupOptions { get; set; } = StartupOptions.Default;

  public static AppSettings Default => new();
}

public sealed record AppSettingsWrapperForHostOptions
{
  public required AppSettings AppSettings { get; set; }
}