using Domain;
using Domain.Infrastructure;

using UI.ViewModels;
using UI.Views;
using UI.DomainWrappers;

using System;
using System.Reactive.Disposables;

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

  public static CompositeDisposable Lifetime { get; } = new();

  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);
    Init();
  }

  void Init()
  {
    if(!IsDesignMode) ConfigureMicrosoftHostServices();
    ConfigureSplatServices();
  }

  void ConfigureMicrosoftHostServices()
  {
    var host = Host
      .CreateDefaultBuilder()
      .ConfigureAppConfiguration((_, cb) =>
      {
        cb.AddJsonFile("appsettings.json", false, true);
      })
      .ConfigureServices((hb, scs) =>
      {
        scs.Configure<AppSettings>(hb.Configuration.GetSection("AppSettings"));
      })
      .Build();

    Locator.CurrentMutable
      .RegisterLazySingletonAnd(host.Services.GetRequiredService<IOptions<AppSettings>>)
      .RegisterLazySingletonAnd(host.Services.GetRequiredService<IOptionsMonitor<AppSettings>>)
      .RegisterLazySingletonAnd(host.Services.GetRequiredService<IHostEnvironment>)
      ;
  }

  void ConfigureSplatServices()
  {
    SR.SetupIOC();
    SR.RegisterLazySingleton<MainWindowViewModel>();
    SR.RegisterLazySingleton<MainViewModel>();
    SR.Register<StartupViewModel>();
    SR.Register<AddProcessViewModel>();
    SR.Register<SelectCurrentlyRunnableProcessViewModel>();
    SR.Register<SettingsViewModel>();
    SR.RegisterLazySingleton<CurrentlyRunnableProcessesService>();
    SR.RegisterLazySingleton<CurrentlyRunnableProcessesServiceWrapper>();
    SR.RegisterLazySingleton<AppSettingChangeService>();

    Locator.CurrentMutable
      .RegisterLazySingletonAnd<MainWindow>(() => new() { DataContext = Locator.Current.GetRequiredService<MainWindowViewModel>() })
      .RegisterLazySingletonAnd<MainView>(() => new() { DataContext = Locator.Current.GetRequiredService<MainViewModel>() })
      .RegisterLazySingletonAnd<IScreen>(Locator.Current.GetRequiredService<MainViewModel>)
      .RegisterLazySingletonAnd<HardwareInfo>(() => new())
      .RegisterLazySingletonAnd<AdminPrivilegesStatus>()
      ;

    if (!IsDesignMode)
    {
      _ = Locator.Current.GetRequiredService<CurrentlyRunnableProcessesServiceWrapper>();
    }
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
    => Lifetime.Dispose();
}
