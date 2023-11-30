using System;

namespace Domain.Infrastructure;

public enum AffinityMode
{
  AllEven = 1,
  FirstNEven,
  FirstN,
  LastN,
  CustomBitmask,
}

public enum AffinityApplyingMode
{
  AllWithMatchedName = 1,
  FirstWithMatchedName,
}

public sealed record ConfiguredProcess
{
  public string Name { get; init; } = "";

  public AffinityMode AffinityMode { get; init; }
  
  public AffinityApplyingMode AffinityApplyingMode { get; init; }
  
  public bool IsCaseSensitive { get; init; }

  public long AffinityValue { get; init; }
}

public enum StartupLocationMode
{
  RememberLast = 1,
  Default,
  CenterScreen,
}

public enum StartupSizeMode
{
  RememberLast = 1,
  Specified,
  Optimal,
}

public enum StartupWindowState
{
  Normal = 1,
  Minimized,
  MinimizedToTray,
}

public enum AppTheme
{
  Dark = 1,
  Light,
  System,
}

public enum AppDarkThemeVariant
{
  AmoledDark = 1,
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

  public StartupWindowState StartupWindowState { get; init; }

  public StartupLocationMode StartupLocationMode { get; init; }

  public StartupLocationValues StartupLocation { get; init; } = new();

  public StartupSizeMode StartupSizeMode { get; init; }

  public StartupSizeValues StartupSize { get; init; } = new();

  public static StartupOptions Default => new()
  {
    Autostart = false,
    StartupWindowState = StartupWindowState.Normal,
    StartupLocationMode = StartupLocationMode.CenterScreen,
    StartupLocation = new(),
    StartupSizeMode = StartupSizeMode.Optimal,
    StartupSize = new(),
  };
}

public sealed record UxOptions
{
  public required bool UseOldSchoolAddEditStyle { get; init; }

  public required bool HideProcessDescriptionFromSelectingProcessView { get; init; }

  public required bool HideToTrayInsteadOfClosing { get; init; }

  public static UxOptions Default => new()
  {
    UseOldSchoolAddEditStyle = false,
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

public sealed record AppSettings
{
  public required TimeSpan RunningProcessesUpdatePeriod { get; init; }

  public required ConfiguredProcess[] ConfiguredProcesses { get; init; }

  public required StartupOptions StartupOptions { get; init; }

  public required UxOptions UxOptions { get; init; }

  public required UiOptions UiOptions { get; init; }

  public static AppSettings Default => new()
  {
    RunningProcessesUpdatePeriod = TimeSpan.FromSeconds(2),
    ConfiguredProcesses = Array.Empty<ConfiguredProcess>(),
    StartupOptions = StartupOptions.Default,
    UxOptions = UxOptions.Default,
    UiOptions = UiOptions.Default,
  };
}

public sealed record AppSettingsWrapperForHostOptions
{
  public required AppSettings AppSettings { get; init; }
}