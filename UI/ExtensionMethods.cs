using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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

  public static Action<T> ThrottleInvokes<T>(this Action<T> action, TimeSpan throttleTimeout)
  {
    Subject<T> methodPick = new();

    methodPick.Throttle(throttleTimeout)
      .Subscribe(action)
      .DisposeWith(App.AppLifetimeDisposable);

    return (arg) => methodPick.OnNext(arg);
  }

  public static Func<T, Task> ThrottleInvokes<T>(this Func<T, Task> action, TimeSpan throttleTimeout)
  {
    Subject<T> methodPick = new();

    methodPick.Throttle(throttleTimeout)
      .Subscribe(async arg => await action(arg))
      .DisposeWith(App.AppLifetimeDisposable);

    return async arg => 
    {
      methodPick.OnNext(arg);
    };
  }

  public static Action InvokeOn(this Func<Task> action, Dispatcher dispatcher)
    => () => dispatcher.InvokeAsync(action);
}
