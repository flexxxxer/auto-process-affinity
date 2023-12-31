﻿using System;
using System.Linq;

namespace Domain.Infrastructure;

public static class AppSettingsValidation
{
  static AppSettings ValidateRunningProcessesUpdatePeriod(AppSettings appSettings)
    => appSettings.RunningProcessesUpdatePeriod switch
    {
      var period when period <= TimeSpan.Zero => appSettings with
      {
        RunningProcessesUpdatePeriod = TimeSpan.FromSeconds(2)
      },
      _ => appSettings
    };

  static AppSettings ValidateStartupOptions(AppSettings appSettings)
  {
    static AppSettings ValidateIsNotNull(AppSettings appSettings)
      => appSettings.StartupOptions switch
      {
        null => appSettings with { StartupOptions = StartupOptions.Default },
        _ => appSettings
      };

    static AppSettings ValidateStartupLocation(AppSettings appSettings)
    {
      var fixedStartupLocationValue = appSettings.StartupOptions.StartupLocation switch
      {
        null or { X: < 0 } or { Y: < 0 } => new StartupOptions.StartupLocationValues(),
        var location => location
      };

      var fixedStartupLocationMode = appSettings.StartupOptions.StartupLocationMode switch
      {
        not StartupLocationMode.RememberLast
          and not StartupLocationMode.CenterScreen
          and not StartupLocationMode.Default => StartupLocationMode.Default,

        var location => location
      };

      return appSettings with
      {
        StartupOptions = appSettings.StartupOptions with
        {
          StartupLocation = fixedStartupLocationValue,
          StartupLocationMode = fixedStartupLocationMode
        }
      };
    }

    static AppSettings ValidateStartupSize(AppSettings appSettings)
    {
      var fixedStartupSizeValue = appSettings.StartupOptions.StartupSize switch
      {
        null or { Width: < 0 } or { Height: < 0 } => new StartupOptions.StartupSizeValues(),
        var size => size
      };

      var fixedStartupSizeMode = appSettings.StartupOptions.StartupSizeMode switch
      {
        not StartupSizeMode.RememberLast
          and not StartupSizeMode.Specified
          and not StartupSizeMode.Optimal => StartupSizeMode.Optimal,

        var location => location
      };

      return appSettings with
      {
        StartupOptions = appSettings.StartupOptions with
        {
          StartupSize = fixedStartupSizeValue,
          StartupSizeMode = fixedStartupSizeMode
        }
      };
    }
    
    static AppSettings ValidateStartupWindowState(AppSettings appSettings)
    {
      var fixedStartupWindowState = appSettings.StartupOptions.StartupWindowState switch
      {
        not StartupWindowState.MinimizedToTray
          and not StartupWindowState.Minimized
          and not StartupWindowState.Normal => StartupWindowState.Normal,

        var darkThemeVariant => darkThemeVariant,
      };

      return appSettings with
      {
        StartupOptions = appSettings.StartupOptions with { StartupWindowState = fixedStartupWindowState }
      };
    }

    return appSettings
      .Pipe(ValidateIsNotNull)
      .Pipe(ValidateStartupLocation)
      .Pipe(ValidateStartupWindowState)
      .Pipe(ValidateStartupSize);
  }

  static AppSettings ValidateUxOptions(AppSettings appSettings)
    => appSettings.UxOptions switch
    {
      null => appSettings with { UxOptions = UxOptions.Default },
      _ => appSettings
    };

  static AppSettings ValidateUiOptions(AppSettings appSettings)
  {
    static AppSettings ValidateIsNotNull(AppSettings appSettings)
      => appSettings.UiOptions switch
      {
        null => appSettings with { UiOptions = UiOptions.Default },
        _ => appSettings
      };

    static AppSettings ValidateTheme(AppSettings appSettings)
    {
      var fixedAppTheme = appSettings.UiOptions.Theme switch
      {
        not AppTheme.Dark
          and not AppTheme.Light
          and not AppTheme.System => AppTheme.System,

        var theme => theme,
      };

      return appSettings with
      {
        UiOptions = appSettings.UiOptions with { Theme = fixedAppTheme }
      };
    }

    static AppSettings ValidateDarkThemeVariant(AppSettings appSettings)
    {
      var fixedDarkThemeVariant = appSettings.UiOptions.DarkThemeVariant switch
      {
        not AppDarkThemeVariant.AmoledDark
          and not AppDarkThemeVariant.Dark
          and not AppDarkThemeVariant.MediumDark
          and not AppDarkThemeVariant.LowDark => AppDarkThemeVariant.LowDark,

        var darkThemeVariant => darkThemeVariant,
      };

      return appSettings with
      {
        UiOptions = appSettings.UiOptions with { DarkThemeVariant = fixedDarkThemeVariant }
      };
    }

    return appSettings
      .Pipe(ValidateIsNotNull)
      .Pipe(ValidateTheme)
      .Pipe(ValidateDarkThemeVariant);
  }

  static AppSettings ValidateConfiguredProcesses(AppSettings appSettings)
  {
    static ConfiguredProcess? ValidateConfiguredProcess(ConfiguredProcess? cp)
      => cp is null or { Name: null } 
         || Enum.GetValues<AffinityMode>().Contains(cp.AffinityMode) is false
         || Enum.GetValues<AffinityApplyingMode>().Contains(cp.AffinityApplyingMode) is false
        ? null
        : cp;
    
    // serializer abnormally serializes [] as null
    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
    var configuredProcesses = appSettings.ConfiguredProcesses ?? Array.Empty<ConfiguredProcess>();
    
    return appSettings with
    {
      // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
      ConfiguredProcesses = configuredProcesses
        .Select(ValidateConfiguredProcess)
        .WhereNotNull()
        .ToArray()
    };
  }

  public static AppSettings ValidateAndFix(this AppSettings currentAppSettings)
    => currentAppSettings
      .Pipe(ValidateRunningProcessesUpdatePeriod)
      .Pipe(ValidateStartupOptions)
      .Pipe(ValidateUxOptions)
      .Pipe(ValidateUiOptions)
      .Pipe(ValidateConfiguredProcesses);
}