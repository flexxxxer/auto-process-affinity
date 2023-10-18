using Domain;
using Domain.Infrastructure;

using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.Reactive.Disposables;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace UI.DomainWrappers;

public sealed class AppSettingChangeService : IDisposable
{
  readonly IHostEnvironment _hostEnvironment;
  readonly SemaphoreSlim _lockGuard = new(1);
  readonly IDisposable? _handleAppSettingsChangedStick;
  readonly IDisposable _handleAppSettingsChangedThrottleStick;
  
  public AppSettings CurrentAppSettings { get; private set; }
  
  public event EventHandler<AppSettings>? AppSettingsChanged;
  
  public AppSettingChangeService(IHostEnvironment hostEnvironment, IOptionsMonitor<AppSettings> appSettings)
  {
    _hostEnvironment = hostEnvironment;
    CurrentAppSettings = appSettings.CurrentValue;

    var handleAppSettingsChangedSpecial = Ext
      .MakeDelegate<AppSettings>(HandleAppSettingsChanged)
      .ThrottleInvokes(TimeSpan.FromSeconds(0.6), out _handleAppSettingsChangedThrottleStick);

    _handleAppSettingsChangedStick = appSettings
      .OnChange(handleAppSettingsChangedSpecial);
  }
  
  void OnAppSettingsChanged(AppSettings newAppSettings) => AppSettingsChanged?.Invoke(this, newAppSettings);
  
  void HandleAppSettingsChanged(AppSettings newAppSettings)
  {
    _lockGuard.Wait();
    try
    {
      CurrentAppSettings = newAppSettings;
      OnAppSettingsChanged(newAppSettings);
    }
    finally
    {
      _lockGuard.Release();
    }
  }

  public async Task MakeChangeAsync(Func<AppSettings, AppSettings> makeChangeFunc)
  {
    await _lockGuard.WaitAsync();
    try
    {
      CurrentAppSettings = makeChangeFunc(CurrentAppSettings);
      var configFilePath = Path.Combine(_hostEnvironment.ContentRootPath, "appsettings.json");
      await using FileStream fileStream = new(configFilePath, FileMode.Create);
      await JsonSerializer.SerializeAsync(fileStream, CurrentAppSettings.WrapBeforeSerialization(), SerializerOptions.Default);
    }
    finally
    {
      _lockGuard.Release();
    }
  }

  public void Dispose()
  {
    _handleAppSettingsChangedThrottleStick.Dispose();
    _handleAppSettingsChangedStick?.Dispose();
  }
}
