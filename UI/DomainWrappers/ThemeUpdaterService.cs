using Domain.Infrastructure;

using System;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Media;

using ReactiveUI;

namespace UI.DomainWrappers;

class ThemeUpdaterService : IDisposable
{
  readonly IDisposable _handleSettingsChangedStick;

  public ThemeUpdaterService(AppSettingChangeService appSettingsService)
  {
    HandleSettingsChanged(appSettingsService.CurrentAppSettings);

    _handleSettingsChangedStick = Observable
      .FromEventPattern<AppSettings>(
        h => appSettingsService.AppSettingsChanged += h,
        h => appSettingsService.AppSettingsChanged -= h)
      .ObserveOn(RxApp.MainThreadScheduler)
      .Subscribe(eventPattern => HandleSettingsChanged(eventPattern.EventArgs));
  }

  ResourceDictionary? _overridenDarkResources;
  
  void HandleSettingsChanged(AppSettings newAppSettings)
  {
    if (Application.Current is { } app)
    {
      app.RequestedThemeVariant = newAppSettings.UiOptions.Theme switch
      {
        AppTheme.System => ThemeVariant.Default,
        AppTheme.Light => ThemeVariant.Light,
        AppTheme.Dark => ThemeVariant.Dark,
        _ => null,
      };
      
      var appResources = Application.Current.Resources;
      appResources.Remove(_overridenDarkResources ?? new());

      var darkColorNewValue = newAppSettings.UiOptions.DarkThemeVariant switch
      {
        AppDarkThemeVariant.AmoledDark => Color.FromArgb(255, 0, 0, 0),
        AppDarkThemeVariant.Dark => Color.FromArgb(255, 23, 23, 23),
        AppDarkThemeVariant.MediumDark => Color.FromArgb(255, 31, 31, 31),
        AppDarkThemeVariant.LowDark => Color.FromArgb(255, 43, 43, 43),
        _ => throw new ArgumentOutOfRangeException()
      };

      var brushesWhichColorNeedToChange = new[]
      {
        "DataGridColumnHeaderBackgroundBrush",
        "SystemRegionBrush",
        "SystemControlBackgroundAltHighBrush",
        "ExpanderContentBackground",
      };

      _overridenDarkResources = new ResourceDictionary();
      var overridenBrushesWithSpecifiedColor = new ResourceDictionary();
      foreach (var brushName in brushesWhichColorNeedToChange)
        overridenBrushesWithSpecifiedColor.Add(brushName, darkColorNewValue);

      _overridenDarkResources.ThemeDictionaries.Add(ThemeVariant.Dark, overridenBrushesWithSpecifiedColor);
      appResources.MergedDictionaries.Add(_overridenDarkResources);

      static object? ResourceFromStylesWithName(string name)
        => Application.Current?.Styles.Resources.TryGetResource(name, ThemeVariant.Dark, out var resource) is true
          ? resource
          : null;

      static Color? ColorFromBrushWithName(string name)
        => ResourceFromStylesWithName(name)?.TryCastTo<ISolidColorBrush>()?.Color;
    }
  }

  public void Dispose()
    => _handleSettingsChangedStick.Dispose();
}
