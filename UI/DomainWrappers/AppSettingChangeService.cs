using Domain;
using Domain.Infrastructure;

using System;
using System.Threading.Tasks;

using System.Threading;
using System.IO;
using System.Text.Json;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Reactive.Disposables;

namespace UI.DomainWrappers;

public delegate void AppSettingsChangedHandler(AppSettings newAppSettings);

public sealed class AppSettingChangeService
{
  readonly IHostEnvironment _hostEnvironment;
  readonly SemaphoreSlim _lockGuard = new(1);
  
  public AppSettings CurrentAppSettings { get; private set; }
  
  public event AppSettingsChangedHandler? AppSettingsChanged;
  
  public AppSettingChangeService(IHostEnvironment hostEnvironment, IOptionsMonitor<AppSettings> appSettings)
  {
    _hostEnvironment = hostEnvironment;
    CurrentAppSettings = appSettings.CurrentValue;

    var handleAppSettingsChangedSpecial = ((Action<AppSettings>)HandleAppSettingsChanged)
      .ThrottleInvokes(TimeSpan.FromSeconds(0.6));
    
    appSettings
      .OnChange(handleAppSettingsChangedSpecial)
      ?.DisposeWith(App.Lifetime);
  }
  
  void OnAppSettingsChanged(AppSettings newAppSettings) => AppSettingsChanged?.Invoke(newAppSettings);
  
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

  
}
