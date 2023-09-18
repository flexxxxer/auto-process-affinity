using System.ComponentModel;
using System.Diagnostics;
using System;
using System.IO;

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
}
