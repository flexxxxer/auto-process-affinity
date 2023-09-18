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
using Splat.Microsoft.Extensions.DependencyInjection;
using Splat.Microsoft.Extensions.Logging;
using Hardware.Info;

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
      .ConfigureServices(services =>
      {
        services.UseMicrosoftDependencyResolver();
        var resolver = Locator.CurrentMutable;
        resolver.InitializeSplat();
        resolver.InitializeReactiveUI();

        ConfigureServices(services);
      })
      .ConfigureLogging(loggingBuilder =>
      {
        loggingBuilder.AddSplat();
      })
      .UseEnvironment(Environments.Development)
      .Build();

    host.Services.UseMicrosoftDependencyResolver();
  }

  void ConfigureServices(IServiceCollection s)
  {
    s.AddSingleton<MainWindowViewModel>()
      .AddSingleton<MainViewModel>()
      .AddSingleton<StartupViewModel>()
      .AddSingleton<AddProcessViewModel>()
      .AddSingleton<SelectCurrentlyRunnableProcessViewModel>()
      .AddSingleton<MainWindow>(s => new() { DataContext = s.GetService<MainWindowViewModel>() })
      .AddSingleton<MainView >(s => new() { DataContext = s.GetService<MainViewModel>() })
      .AddSingleton<StartupView >(s => new() { DataContext = s.GetService<StartupViewModel>() })
      .AddSingleton<AddProcessView>(s => new() { DataContext = s.GetService<AddProcessViewModel>() })
      .AddSingleton<SelectCurrentlyRunnableProcessView>(s 
        => new() { DataContext = s.GetService<SelectCurrentlyRunnableProcessViewModel>() })
      .AddSingleton<IViewFor<MainWindowViewModel>, MainWindow>(s => s.GetRequiredService<MainWindow>())
      .AddSingleton<IViewFor<MainViewModel>, MainView>(s => s.GetRequiredService<MainView>())
      .AddSingleton<IViewFor<StartupViewModel>, StartupView>(s => s.GetRequiredService<StartupView>())
      .AddSingleton<IViewFor<AddProcessViewModel>, AddProcessView>(s => s.GetRequiredService<AddProcessView>())
      .AddSingleton<IViewFor<SelectCurrentlyRunnableProcessViewModel>, SelectCurrentlyRunnableProcessView>(s 
        => s.GetRequiredService<SelectCurrentlyRunnableProcessView>());

    s.AddTransient<HardwareInfo>(_ => new());
  }

  public override void OnFrameworkInitializationCompleted()
  {
    _ = ApplicationLifetime switch
    {
      IClassicDesktopStyleApplicationLifetime desktop => 
        desktop.MainWindow = Locator.Current.GetService<MainWindow>(),

      ISingleViewApplicationLifetime singleViewPlatform =>
        singleViewPlatform.MainView = Locator.Current.GetService<MainView>(),

      _ when IsDesignMode => null,
      _ => throw new PlatformNotSupportedException()
    };
    
    base.OnFrameworkInitializationCompleted();
  }
}
