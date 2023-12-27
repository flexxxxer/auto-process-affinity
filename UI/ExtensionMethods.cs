using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

using Avalonia.Threading;

using ReactiveUI;

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
  
  public static T? TryCastTo<T>(this object obj)
    => obj is T retyped ?  retyped : default;

  public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);

  public static Action InvokeOn(this Action action, Dispatcher dispatcher)
    => () => dispatcher.Invoke(action);

  public static Action InvokeOn(this Action action, IScheduler scheduler)
    => () => scheduler.Schedule(action);

  public static Action<T> InvokeOn<T>(this Action<T> action, IScheduler scheduler)
    => state => scheduler.Schedule(state, (_, passedState) => { action(passedState); return Disposable.Empty; });

  public static Action<T> ThrottleInvokes<T>(this Action<T> action, TimeSpan throttleTimeout, out IDisposable methodThrottleStick)
  {
    Subject<T> methodPick = new();

    methodThrottleStick = methodPick
      .Throttle(throttleTimeout)
      .Subscribe(action);

    return methodPick.OnNext;
  }

  public static void WhenFirstTimeActivated(this IActivatableViewModel viewModel, Action activationAction)
  {
    var firstActivationDisposable = new SingleAssignmentDisposable();

    viewModel.WhenActivated(disposables =>
    {
      // Check if this is the first activation
      if (!firstActivationDisposable.IsDisposed)
      {
        // Call the activation action and pass the firstActivationDisposable
        activationAction();
      }

      // Dispose the disposable after first activation
      disposables.Add(firstActivationDisposable);
    });
  }

  public static Action InvokeOn(this Func<Task> action, Dispatcher dispatcher)
    => () => dispatcher.InvokeAsync(action);
}
