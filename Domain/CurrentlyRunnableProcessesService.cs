using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using DynamicData;

namespace Domain;

public sealed class CurrentlyRunnedProcessDto
{
  public string Name { get; }

  public int ProcessId { get; }

  public string Description { get; }

  internal Process SourceProcess { get; }

  public CurrentlyRunnedProcessDto(Process sourceProcess)
  {
    SourceProcess = sourceProcess;
    Name = sourceProcess.ProcessName();
    ProcessId = sourceProcess.Id;
    Description = sourceProcess.ProcessDescription() ?? "";
  }
}

/// <summary>
/// This is stateful service (for optimization purposes)
/// </summary>
public sealed class CurrentlyRunnableProcessesService : IDisposable
{
  readonly System.Timers.Timer _refreshProcessesTimer;
  readonly List<CurrentlyRunnedProcessDto> _currentlyRunningProcesses;
  readonly Dictionary<int, CurrentlyRunnedProcessDto> _currentlyRunningProcessesProcessIdKeyed;
  readonly object _syncObj = new();

  public record UpdateChangeset(CurrentlyRunnedProcessDto[] Dead, CurrentlyRunnedProcessDto[] Created);

  EventHandler<UpdateChangeset>? _updateEvent;
  public event EventHandler<UpdateChangeset>? Update
  {
    add
    {
      if(_updateEvent is null && _isInitialed is 1)
      {
        _refreshProcessesTimer.Start();
      }
      _updateEvent += value;
    }
    remove
    {
      _updateEvent -= value;
      if(_updateEvent is null)
      {
        _refreshProcessesTimer.Stop();
      }
    }
  }

  public List<CurrentlyRunnedProcessDto> CurrentlyRunningProcesses
  {
    get
    {
      Monitor.Enter(_syncObj);
      try
      {
        return _currentlyRunningProcesses.ToList();
      }
      finally
      {
        Monitor.Exit(_syncObj);
      }
    }
  }

  public TimeSpan UpdateInterval
  {
    get => TimeSpan.FromMilliseconds(_refreshProcessesTimer.Interval);
    set => _refreshProcessesTimer.Interval = value.TotalMilliseconds;
  }

  public ConcurrentDictionary<string, byte> ProcessNamesToExclude { get; set; } = new();

  public CurrentlyRunnableProcessesService(TimeSpan? interval = null)
  {
    var usedInterval = interval switch
    {
      { } notNull when notNull >= TimeSpan.FromSeconds(1) => notNull,
      _ => TimeSpan.FromSeconds(1)
    };
    _currentlyRunningProcesses = new(512);
    _currentlyRunningProcessesProcessIdKeyed = new(512);
    _refreshProcessesTimer = new(usedInterval);
    _refreshProcessesTimer.Elapsed += Refresh;
  }

  int _isInitialed;

  public async Task InitAsync()
  {
    if(Interlocked.CompareExchange(ref _isInitialed, 1, 0) == 0)
    {
      await Task.Run(() => Refresh(null!, null!));
      if (_updateEvent is not null) _refreshProcessesTimer.Start();
    }
  }

  void Refresh(object? _, ElapsedEventArgs __)
  {
    if (!Monitor.TryEnter(_syncObj)) return;
    
    try
    {
      _currentlyRunningProcesses
        .AsParallel()
        .ForAll(p => p.SourceProcess.Refresh());

      var processesToRemove = _currentlyRunningProcesses
        .Where(p => p.SourceProcess.HasExited || ProcessNamesToExclude.ContainsKey(p.Name))
        .ToArray();

      _currentlyRunningProcesses.RemoveMany(processesToRemove);
      processesToRemove.ForEach(p => _currentlyRunningProcessesProcessIdKeyed.Remove(p.ProcessId));

      var processesToAdd = Process.GetProcesses()
        .Select(p => new CurrentlyRunnedProcessDto(p))
        .AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
        .Where(p => !ProcessNamesToExclude.ContainsKey(p.Name)
          && !_currentlyRunningProcessesProcessIdKeyed.ContainsKey(p.ProcessId)
          && p.SourceProcess.IsProcessorAffinityCanBeSet())
        .ToArray();

      _currentlyRunningProcesses.AddRange(processesToAdd);
      processesToAdd.ForEach(p => _currentlyRunningProcessesProcessIdKeyed.Add(p.ProcessId, p));

      _updateEvent?.Invoke(this, new(processesToRemove, processesToAdd));
    }
    finally
    {
      Monitor.Exit(_syncObj);
    }
  }

  public void Dispose()
  {
    _refreshProcessesTimer.Elapsed -= Refresh;
    _refreshProcessesTimer.Dispose();
  }
}
