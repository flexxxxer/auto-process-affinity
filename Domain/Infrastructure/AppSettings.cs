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

public enum AppTheme
{
  Dark,
  Light,
  System,
}

public enum AppDarkThemeVariant
{
  AmoledDark,
  Dark,
  MediumDark,
  LowDark,
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

public sealed record UxOptions
{
  public required bool UseOldScoolAddEditStyle { get; init; }

  public required bool HideProcessDescriptionFromSelectingProcessView { get; init; }

  public required bool HideToTrayInsteadOfClosing { get; init; }

  public static UxOptions Default => new()
  {
    UseOldScoolAddEditStyle = false,
    HideProcessDescriptionFromSelectingProcessView = false,
    HideToTrayInsteadOfClosing = true,
  };
}

public sealed record UiOptions
{
  public required bool ShowSystemTitleBar { get; init; }

  public required AppTheme Theme { get; init; }

  public required AppDarkThemeVariant DarkThemeVariant { get; init; }

  public static UiOptions Default => new()
  {
    ShowSystemTitleBar = true,
    Theme = AppTheme.System,
    DarkThemeVariant = AppDarkThemeVariant.LowDark,
  };
}

public sealed record SystemLevelStartupOptions
{
  public required bool RunWithAdminOrRootPrivileges { get; init; }

  public static SystemLevelStartupOptions Default => new()
  {
    RunWithAdminOrRootPrivileges = false,
  };
}

public sealed record AppSettings
{
  public required TimeSpan RunningProcessesUpdatePeriod { get; init; }

  public required ConfiguredProcess[] ConfiguredProcesses { get; init; }

  public required StartupOptions StartupOptions { get; init; }

  public required UxOptions UxOptions { get; init; }

  public required UiOptions UiOptions { get; init; }

  public required SystemLevelStartupOptions SystemLevelStartupOptions { get; init; }

  public static AppSettings Default => new()
  {
    RunningProcessesUpdatePeriod = TimeSpan.FromSeconds(2),
    ConfiguredProcesses = Array.Empty<ConfiguredProcess>(),
    StartupOptions = StartupOptions.Default,
    UxOptions = UxOptions.Default,
    UiOptions = UiOptions.Default,
    SystemLevelStartupOptions = SystemLevelStartupOptions.Default,
  };
}

public sealed record AppSettingsWrapperForHostOptions
{
  public required AppSettings AppSettings { get; init; }
}