using System;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using Domain.Infrastructure;

namespace Domain;

public static class ProcessApi
{
  public static bool IsProcessorAffinityCanBeSet(this Process process)
  {
    try
    {
      if (process.HasExited)
      {
        return false;
      }
      _ = process.ProcessorAffinity;
      return true;
    }
    catch (Exception e) when (e is Win32Exception or InvalidOperationException)
    {
      return false;
    }
  }

  public static bool TrySetProcessorAffinity(this Process process, nint value)
  {
    try
    {
      if (process.HasExited)
      {
        return false;
      }
      process.ProcessorAffinity = value;
      return true;
    }
    catch
    {
      return false;
    }
  }

  public static string? ProcessDescription(this Process process)
  {
    try
    {
      return process.HasExited ? null : (process.MainModule?.FileVersionInfo.FileDescription);
    }
    catch
    {
      return null;
    }
  }

  public static string ProcessName(this Process process)
  {
    try
    {
      return process.HasExited
          ? process.ProcessName
          : process.MainModule
              ?.FileName
              .Pipe(Path.GetFileName)
              ?? process.ProcessName;
    }
    catch
    {
      return process.ProcessName;
    }
  }

  public static bool? IsCurrentProcessRunningWithAdminOrRootPrivileges()
  {
    return OsTypeApi.CurrentOs switch
    {
      OsType.Windows => CheckIfAdmin(),
      OsType.Linux => CheckIfRootV1() ? true : CheckIfRootV2(),
      _ => null
    };

    bool CheckIfAdmin() => WindowsIdentity.GetCurrent()
      .PipeThenDispose(id => new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator));

    bool CheckIfRootV1() => Environment.GetEnvironmentVariable("USER_ID") is "0";

    bool? CheckIfRootV2()
    {
      try
      {
        var command = $"ps -o uid= -p {Environment.ProcessId}";

        ProcessStartInfo startInfo = new()
        {
          FileName = "/bin/bash",
          Arguments = $"-c \"{command}\"",
          RedirectStandardOutput = true,
          UseShellExecute = false,
          CreateNoWindow = true,
        };

        using Process process = new();
        process.StartInfo = startInfo;
        process.Start();
        var result = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();

        return result is "0";
      }
      catch
      {
        return null;
      }
    }
  }
}
