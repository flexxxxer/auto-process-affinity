using System.Runtime.InteropServices;

namespace Domain.Infrastructure;

public enum OSType
{
  Windows,
  Linux,
  // other not supported now
  Unknown
}

public static class OSTypeApi
{
  public static OSType CurrentOS => (null as object) switch
  {
    _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => OSType.Windows,
    _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => OSType.Linux,
    _ => OSType.Unknown
  };
}