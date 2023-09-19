using System;

namespace Domain;

public sealed class ConfiguredProcess
{
  public string Name { get; set; } = "";

  public int AffinityValue { get; set; }
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

public sealed class StartupOptions
{
  public sealed class StartupLocationValues
  {
    public int X { get; set; }

    public int Y { get; set; }
  }

  public sealed class StartupSizeValues
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

public class AppSettings
{
  public TimeSpan RunningProcessesUpdatePeriod { get; set; } = TimeSpan.FromSeconds(2);

  public ConfiguredProcess[] ConfiguredProcesses { get; set; } = Array.Empty<ConfiguredProcess>();

  public StartupOptions StartupOptions { get; set; } = StartupOptions.Default;

  public static AppSettings Default => new();
}

