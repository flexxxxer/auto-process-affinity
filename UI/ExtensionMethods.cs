using Splat;
using System;

namespace UI;

public static class ExtensionMethods
{
  public static T GetRequiredService<T>(this IReadonlyDependencyResolver dependencyResolver)
    => dependencyResolver.GetService<T>() ?? throw new InvalidOperationException();

  public static T GetRequiredService<T>(this IReadonlyDependencyResolver dependencyResolver, string contract)
    => dependencyResolver.GetService<T>(contract) ?? throw new InvalidOperationException();
}
