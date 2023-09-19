using UI.ViewModels;
using UI.Views;

using System;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using ReactiveUI;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Splat;
using Hardware.Info;
using Microsoft.Extensions.Configuration;
using Domain;
using Microsoft.Extensions.Options;

using SR = Splat.SplatRegistrations;

namespace UI;

public partial class App : Application
{
  public static bool IsDesignMode => Avalonia.Controls.Design.IsDesignMode;

  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);
    Init();
  }

  void Init()
  {
    var host = Host
      .CreateDefaultBuilder()
      .ConfigureAppConfiguration(cb =>
      {
        cb.AddJsonFile("appsettings.json", false, true);
      })
      .ConfigureServices((hb, scs) =>
      {
        scs.Configure<AppSettings>(hb.Configuration.GetSection(""));
      })
      .UseEnvironment(Environments.Development)
      .Build();

    ConfigureMicosoftHostServices(host);
    ConfigureSplatServices();
  }

  void ConfigureMicosoftHostServices(IHost host)
  {
    Locator.CurrentMutable
      .RegisterLazySingletonAnd(host.Services.GetRequiredService<IOptions<AppSettings>>)
      .RegisterLazySingletonAnd(host.Services.GetRequiredService<IOptionsMonitor<AppSettings>>);
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

    Locator.CurrentMutable
      .RegisterLazySingletonAnd<MainWindow>(() => new() { DataContext = Locator.Current.GetRequiredService<MainWindowViewModel>() })
      .RegisterLazySingletonAnd<MainView>(() => new() { DataContext = Locator.Current.GetRequiredService<MainViewModel>() })
      .RegisterLazySingletonAnd<IScreen>(Locator.Current.GetRequiredService<MainViewModel>)
      .RegisterLazySingletonAnd<HardwareInfo>(() => new());
  }

  public override void OnFrameworkInitializationCompleted()
  {
    _ = ApplicationLifetime switch
    {
      IClassicDesktopStyleApplicationLifetime desktop => 
        desktop.MainWindow = Locator.Current.GetRequiredService<MainWindow>(),

      ISingleViewApplicationLifetime singleViewPlatform =>
        singleViewPlatform.MainView = Locator.Current.GetRequiredService<MainView>(),

      _ when IsDesignMode => null,
      _ => throw new PlatformNotSupportedException()
    };
    
    base.OnFrameworkInitializationCompleted();
  }
}
