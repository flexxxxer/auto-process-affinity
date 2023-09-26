using System;
using System.Reactive.Linq;

// ReSharper disable once CheckNamespace
namespace ReactiveUI.ExtendedRouting;

public static class ExtensionMethods
{
  public static IObservable<T> NavigateTo<T>(this RoutingState routingState, T vm)
    where T : IRoutableViewModel
    => routingState.Navigate.Execute(vm).Cast<T>();

  public static IObservable<T> NavigateTo<T>(this IScreen screen, T vm)
    where T : IRoutableViewModel
    => screen.Router.Navigate.Execute(vm).Cast<T>();
}