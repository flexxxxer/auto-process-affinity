using System.Runtime.InteropServices;

namespace Domain.Infrastructure;

public enum OsType
{
  Windows,
  Linux,
  // other not supported now
  Unknown
}

public static class OsTypeApi
{
  public static OsType CurrentOs => (null as object) switch
  {
    _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => OsType.Windows,
    _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => OsType.Linux,
    _ => OsType.Unknown
  };
}