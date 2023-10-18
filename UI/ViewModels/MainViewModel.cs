using Domain;
using Domain.Infrastructure;

using UI.DomainWrappers;

using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using ReactiveUI;
using ReactiveUI.ExtendedRouting;

using Avalonia;
using Avalonia.Styling;

using Splat;

using Microsoft.Extensions.Options;

namespace UI.ViewModels;

public interface IMainViewModel : IScreen
{
}

public class MainViewModel : ActivatableViewModelBase, IMainViewModel
{
  public RoutingState Router { get; } = new RoutingState();

  public MainViewModel(MainWindowViewModel mainWindowViewModel, AppSettingChangeService appSettingService,
    IOptions<AppSettings> appSettings) 
  {
    // ReSharper disable once AsyncVoidLambda
    this.WhenActivated(async d =>
    {
      Router.CurrentViewModel
        .Where(vm => vm is not null)
        .Select(vm => vm!)
        .Subscribe(vm =>
        {
          mainWindowViewModel.WindowTitleText = vm switch
          {
            AddProcessViewModel => "New process rule",
            SelectCurrentlyRunnableProcessViewModel => "Selecting process",
            SettingsViewModel => "Settings",
            StartupViewModel or _ => mainWindowViewModel.DefaultWindowTitleText,
          };
        })
        .DisposeWith(d);

      if(ValidateAndReturnFixed(appSettings.Value) is { } fixedAppSettings)
      {
        await appSettingService.MakeChangeAsync(_ => fixedAppSettings);
      }

      _ = await Locator.Current
        .GetRequiredService<StartupViewModel>()
        .RouteThrough(Router);
    });
  }

  static AppSettings? ValidateAndReturnFixed(AppSettings currentAppSettings)
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

      return appSettings
        .Pipe(ValidateIsNotNull)
        .Pipe(ValidateStartupLocation)
        .Pipe(ValidateStartupSize)
        ;
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
        .Pipe(ValidateDarkThemeVariant)
        ;
    }

    static AppSettings ValidateSystemLevelStartupOptions(AppSettings appSettings)
      => appSettings.SystemLevelStartupOptions switch
      {
        null => appSettings with { SystemLevelStartupOptions = SystemLevelStartupOptions.Default },
        _ => appSettings
      };

    var fixedAppSettings = currentAppSettings
      .Pipe(ValidateRunningProcessesUpdatePeriod)
      .Pipe(ValidateStartupOptions)
      .Pipe(ValidateUxOptions)
      .Pipe(ValidateUiOptions)
      .Pipe(ValidateSystemLevelStartupOptions)
      ;

    return currentAppSettings != fixedAppSettings
      ? fixedAppSettings
      : null;
  }
}

public sealed class DesignMainViewModel : ViewModelBase, IMainViewModel
{
  public RoutingState Router { get; } = new RoutingState();

  public DesignMainViewModel()
  {
    Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
  }
}

