using Domain;
using Domain.Infrastructure;

using UI.ViewModels;
using UI.Views;
using UI.DomainWrappers;

using System;
using System.IO;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using ReactiveUI;

using Splat;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using Hardware.Info;

using SR = Splat.SplatRegistrations;
using IApplicationLifetime = Avalonia.Controls.ApplicationLifetimes.IApplicationLifetime;

namespace UI;

public class App : Application
{
  public static bool IsDesignMode => Design.IsDesignMode;

  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);
    Init();
  }

  void Init()
  {
    ConfigureUiServices();
    if (!IsDesignMode)
    {
      ConfigureMicrosoftHostServices();
      ConfigureCustomServices();
      SetupServices();
      DataContext = Locator.Current.GetRequiredService<AppDataContext>();
    }
  }

  void ConfigureMicrosoftHostServices()
  {
    var appFolderPath = ProcessApi
      .GetExecutingAppFilePath()
      .Pipe(Path.GetDirectoryName)!;
    
    var host = Host
      .CreateDefaultBuilder()
      .UseContentRoot(appFolderPath)
      .ConfigureAppConfiguration((_, cb) =>
      {
        cb.SetBasePath(appFolderPath);
        cb.AddJsonFile("appsettings.json", false, true);
      })
      .ConfigureServices((hb, scs) =>
      {
        scs.AddOptions<AppSettings>()
          .Bind(hb.Configuration.GetSection(nameof(AppSettings)));
      })
      .Build();

    Locator.CurrentMutable
      .RegisterAnd(host.Services.GetRequiredService<IOptions<AppSettings>>)
      .RegisterAnd(host.Services.GetRequiredService<IOptionsMonitor<AppSettings>>)
      .RegisterAnd(host.Services.GetRequiredService<IOptionsSnapshot<AppSettings>>)
      .RegisterAnd(host.Services.GetRequiredService<IHostEnvironment>)
      ;
  }

  void ConfigureUiServices()
  {
    SR.SetupIOC();
    SR.RegisterLazySingleton<AppDataContext>();
    SR.RegisterLazySingleton<MainWindowViewModel>();
    SR.RegisterLazySingleton<MainViewModel>();
    SR.Register<StartupViewModel>();
    SR.Register<AddProcessViewModel>();
    SR.Register<SelectCurrentlyRunnableProcessViewModel>();
    SR.Register<SettingsViewModel>();
    SR.Register<AboutViewModel>();

    Locator.CurrentMutable
      .RegisterLazySingletonAnd<MainWindow>(() => new() { DataContext = Locator.Current.GetRequiredService<MainWindowViewModel>() })
      .RegisterLazySingletonAnd<MainView>(() => new() { DataContext = Locator.Current.GetRequiredService<MainViewModel>() })
      .RegisterLazySingletonAnd<IScreen>(Locator.Current.GetRequiredService<MainViewModel>)
      .RegisterLazySingletonAnd<HardwareInfo>(() => new())
      .RegisterLazySingletonAnd<AdminPrivilegesStatus>()
      ;
  }

  void ConfigureCustomServices()
  {
    SR.RegisterLazySingleton<CurrentlyRunnableProcessesService>();
    SR.RegisterLazySingleton<CurrentlyRunnableProcessesServiceWrapper>();
    SR.RegisterLazySingleton<AppSettingChangeService>();
    SR.RegisterLazySingleton<ThemeUpdaterService>();
    SR.RegisterLazySingleton<AutostartChangeService>();

    Locator.CurrentMutable
      .RegisterLazySingletonAnd<HardwareInfo>(() => new())
      .RegisterLazySingletonAnd<AdminPrivilegesStatus>()
      ;
  }
  
  void SetupServices()
  {
    var appSettingsService = Locator.Current.GetRequiredService<AppSettingChangeService>();
    var currentSettings = appSettingsService.CurrentAppSettings;
    var fixedSettings = currentSettings.ValidateAndFix();
    if (currentSettings != fixedSettings)
    {
      appSettingsService
        .MakeChangeAsync(_ => fixedSettings)
        .NoAwait();
    }
    
    // services to instantiate manually
    new[]
    {
      typeof(CurrentlyRunnableProcessesServiceWrapper),
      typeof(ThemeUpdaterService),
      typeof(AutostartChangeService),
    }.ForEach(serviceType => _ = Locator.Current.GetService(serviceType));
  }

  public override void OnFrameworkInitializationCompleted()
  {
    IApplicationLifetime? _ = ApplicationLifetime switch
    {
      IClassicDesktopStyleApplicationLifetime desktop => desktop
        .Do(d => d.MainWindow = Locator.Current.GetRequiredService<MainWindow>())
        .Do(d => d.ShutdownRequested += HandleAppShutdown)
        .Do(d => d.Exit += HandleAppExit),

      ISingleViewApplicationLifetime singleViewPlatform => singleViewPlatform
        .Do(svp => svp.MainView = Locator.Current.GetRequiredService<MainView>()),

      _ when IsDesignMode => null,
      _ => throw new PlatformNotSupportedException()
    };

    base.OnFrameworkInitializationCompleted();
  }

  void HandleAppShutdown(object? _, ShutdownRequestedEventArgs args)
  { }
  
  void HandleAppExit(object? _, ControlledApplicationLifetimeExitEventArgs args)
  { }
}
