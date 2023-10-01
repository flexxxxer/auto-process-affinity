using System;
using System.Reactive.Linq;

// ReSharper disable once CheckNamespace
namespace ReactiveUI.ExtendedRouting;

public static class ExtensionMethods
{
  public static IObservable<T> RouteThrought<T>(this T vm, IScreen screen)
    where T : IRoutableViewModel
    => screen.Router.Navigate.Execute(vm).Cast<T>();

  public static IObservable<T> RouteThrought<T>(this T vm, RoutingState routingState)
    where T : IRoutableViewModel
    => routingState.Navigate.Execute(vm).Cast<T>();
}