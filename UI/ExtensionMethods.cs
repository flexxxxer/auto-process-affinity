using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading.Tasks;

using Avalonia.Threading;

using Splat;

namespace UI;

public static class ExtensionMethods
{
  public static T GetRequiredService<T>(this IReadonlyDependencyResolver dependencyResolver)
    => dependencyResolver.GetService<T>() ?? throw new InvalidOperationException();

  public static T GetRequiredService<T>(this IReadonlyDependencyResolver dependencyResolver, string contract)
    => dependencyResolver.GetService<T>(contract) ?? throw new InvalidOperationException();

  public static T CastTo<T>(this object obj)
    => (T)obj;

  public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);

  public static Action InvokeOn(this Action action, Dispatcher dispatcher)
    => () => dispatcher.Invoke(action);

  public static Action InvokeOn(this Action action, IScheduler scheduler)
    => () => scheduler.Schedule(action);

  public static Action<T> InvokeOn<T>(this Action<T> action, IScheduler scheduler)
    => state => scheduler.Schedule(state, (_, state) => { action(state); return Disposable.Empty; });

  public static Action InvokeOn(this Func<Task> action, Dispatcher dispatcher)
    => () => dispatcher.InvokeAsync(action);
}
