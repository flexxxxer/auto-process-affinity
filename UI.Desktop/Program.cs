using System;
using System.Linq;
using System.Threading;

using Avalonia;
using Avalonia.ReactiveUI;

namespace UI.Desktop;

static class Program
{
  [STAThread]
  public static int Main(string[] args)
  {
    var builder = BuildAvaloniaApp();
    if (args.Contains("--drm"))
    {
      SilenceConsole();

      // If Card0, Card1 and Card2 all don't work. You can also try:                 
      // return builder.StartLinuxFbDev(args);
      // return builder.StartLinuxDrm(args, "/dev/dri/card1");
      return builder.StartLinuxDrm(args, "/dev/dri/card1", 1D);
    }

    return builder.StartWithClassicDesktopLifetime(args);
  }

  static AppBuilder BuildAvaloniaApp()
       => AppBuilder.Configure<App>()
           .UsePlatformDetect()
           .WithInterFont()
           .LogToTrace()
           .UseReactiveUI();

  static void SilenceConsole()
  {
    // ReSharper disable once FunctionNeverReturns
    static void Start()
    {
      Console.CursorVisible = false;
      while (true) Console.ReadKey(true);
    }

    new Thread(Start) { IsBackground = true }.Start();
  }
}
