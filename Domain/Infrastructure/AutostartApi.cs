using System;
using System.IO;

using ShellLink;

namespace Domain.Infrastructure;

public static class AutostartApi
{
#if IS_WINDOWS
  private static bool TryChangeAutostartModeWindows(bool isAutostartEnabled)
  {
    var startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
    var shortcutPath = Path.Combine(startupFolder, "AutoProcessAffinity.lnk");

    if (isAutostartEnabled && !File.Exists(shortcutPath))
    {
      ProcessApi.GetExecutingAppFilePath()
        .Pipe(Shortcut.CreateShortcut)
        .WriteToFile(shortcutPath);
      
      return true;
    }
    if (!isAutostartEnabled && File.Exists(shortcutPath))
    {
      File.Delete(shortcutPath);
      return true;
    }
    
    return false;
  }
#endif


  public static bool? TryChangeAutostartMode(bool isAutostartEnabled)
  {
#if IS_WINDOWS
    return TryChangeAutostartModeWindows(isAutostartEnabled);
#elif IS_LINUX
    return null;
#else
    return null;
#endif
  }
}