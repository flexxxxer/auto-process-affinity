using Domain.Infrastructure;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Domain;

public static partial class Ext
{
  public static T Do<T>(this T self, Action whatToDo)
  {
    whatToDo();
    return self;
  }

  public static T DoIf<T>(this T self, bool @if, Action whatToDo)
  {
    if(@if) whatToDo();
    return self;
  }

  public static T Do<T>(this T self, Action<T> whatToDo)
  {
    whatToDo(self);
    return self;
  }

  public static T DoIf<T>(this T self, bool @if, Action<T> whatToDo)
  {
    if(@if) whatToDo(self);
    return self;
  }

  public static TOut Pipe<TIn, TOut>(this TIn self, Func<TIn, TOut> func)
    => func(self);

  public static TOut PipeThenDispose<TIn, TOut>(this TIn self, Func<TIn, TOut> func)
    where TIn : IDisposable
  {
    try
    {
      return func(self);
    }
    finally
    {
      self.Dispose();
    }
  }

  public static async Task<TOut> PipeUsingTaskRun<TIn, TOut>(this TIn self, Func<TIn, TOut> func)
    => await Task.Run(() => func(self));

  public static void ForEach<T>(this IEnumerable<T> seq, Action<T> action)
  {
    foreach (var item in seq)
    {
      action(item);
    }
  }

  public static void ForEach<T1, T2>(this IEnumerable<(T1, T2)> seq, Action<T1, T2> action)
  {
    foreach (var (arg1, arg2) in seq)
    {
      action(arg1, arg2);
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

  public static string Remove(this string str, string text)
    => str.Replace(text, string.Empty);

  public static string RemoveVmPostfix(this string str)
    => str.TrimEnd("ViewModel");

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

  // ReSharper disable once MemberCanBePrivate.Global
  public static string TrimEnd(this string source, string value) 
    => source.EndsWith(value)
      ? source.Remove(source.LastIndexOf(value, StringComparison.Ordinal))
      : source;

  public static void NoAwait(this Task? task)
  {
    task?.ContinueWith(_ => { }, TaskContinuationOptions.ExecuteSynchronously);
  }

  public static void AddRange<T>(this Collection<T> where, IEnumerable<T> seq)
  {
    foreach (T item in seq)
    {
      where.Add(item);
    }
  }

  public static void AddTo<T>(this IEnumerable<T> seq, Collection<T> where) => where.AddRange(seq);

  public static AppSettingsWrapperForHostOptions WrapBeforeSerialization(this AppSettings appSettings)
    => new() { AppSettings = appSettings };

  public static char ToUpper(this char c) => char.ToUpper(c);
  
  public static char ToLower(this char c) => char.ToLower(c);

  [GeneratedRegex("(\\B[A-Z])")]
  private static partial Regex WordsRegex();
  
  public static string ToFriendlyString<T>(this T enumValue) where T : Enum => enumValue.ToString()
    .Pipe(enumAsStr => WordsRegex().Replace(enumAsStr, " $1"))
    .Pipe(enumAsStr => enumAsStr.ToLower())
    .Pipe(enumAsStr => enumAsStr[0].ToUpper() + enumAsStr[1..]);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Action MakeDegalate(Action action) => action;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Action<T> MakeDelegate<T>(Action<T> action) => action;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Func<T> MakeDelegate<T>(Func<T> action) => action;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Func<T1, T2> MakeDelegate<T1, T2>(Func<T1, T2> action) => action;

  public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> seq) 
    => seq.Where(x => x is not null)!;
}
