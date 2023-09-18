﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Domain;

public static class Extensions
{
  public static T Do<T>(this T self, Action whatToDo)
  {
    whatToDo();
    return self;
  }

  public static T Do<T>(this T self, Action<T> whatToDo)
  {
    whatToDo(self);
    return self;
  }

  public static TOut Pipe<TIn, TOut>(this TIn self, Func<TIn, TOut> func)
      => func(self);

  public static void ForEach<T>(this IEnumerable<T> seq, Action<T> action)
  {
    foreach (var item in seq)
    {
      action(item);
    }
  }

  public static bool ContainsText(this string str, string text, bool ignoreCase = true, bool invariantCulture = true)
  {
    var comparisonType = (ignoreCase, invariantCulture) switch
    {
      (false, false) => StringComparison.Ordinal,
      (true, false) => StringComparison.OrdinalIgnoreCase,
      (false, true) => StringComparison.InvariantCulture,
      (true, true) => StringComparison.InvariantCultureIgnoreCase
    };

    return str.Contains(text, comparisonType);
  }

  public static IEnumerable<T> ForEachDo<T>(this IEnumerable<T> seq, Action<T> action)
  {
    foreach (var item in seq)
    {
      action(item);
      yield return item;
    }
  }

  public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> seq)
      => new(seq);

  public static bool In<T>(this T self, params T[] values) where T : IEquatable<T>
      => values.Contains(self);

  public static string TrimEnd(this string source, string value) 
    => source.EndsWith(value)
      ? source.Remove(source.LastIndexOf(value))
      : source;

  public static void NoAwait(this Task? task)
  {
    task?.ContinueWith(_ => { }, TaskContinuationOptions.ExecuteSynchronously);
  }

  public static async Task<T[]> ProcessQueryAsync<T>(this ParallelQuery<T> query) => await Task.Run(() => query.ToArray());
}
