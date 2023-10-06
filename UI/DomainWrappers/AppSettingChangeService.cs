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
using ReactiveUI;

namespace UI.DomainWrappers;

public sealed class AppSettingChangeService
{
  readonly IHostEnvironment _hostEnvironment;
  readonly SemaphoreSlim _lockGuard = new(1);
  AppSettings _currentAppSettings;

  public AppSettingChangeService(IHostEnvironment hostEnvironment, IOptionsMonitor<AppSettings> appSettings)
  {
    _hostEnvironment = hostEnvironment;
    _currentAppSettings = appSettings.CurrentValue;

    appSettings
      .OnChange(((Action<AppSettings>)HandleAppSettingsChanged)
        .ThrottleInvokes(TimeSpan.FromSeconds(1)))
      ?.DisposeWith(App.AppLifetimeDisposable);
  }

  void HandleAppSettingsChanged(AppSettings newAppSettings)
  {
    _lockGuard.Wait();
    try
    {
      _currentAppSettings = newAppSettings;
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
      _currentAppSettings = makeChangeFunc(_currentAppSettings);
      var configFilePath = Path.Combine(_hostEnvironment.ContentRootPath, "appsettings.json");
      await using FileStream fileStream = new(configFilePath, FileMode.Create);
      await JsonSerializer.SerializeAsync(fileStream, _currentAppSettings.WrapBeforeSerialization(), SerializerOptions.Default);
    }
    finally
    {
      _lockGuard.Release();
    }
  }
}
