using System;

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
}
