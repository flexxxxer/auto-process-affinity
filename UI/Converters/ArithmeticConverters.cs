using System;
using System.Linq;

using Avalonia.Data.Converters;

namespace UI.Converters;

public static class ArithmeticConverters
{
  public static readonly IMultiValueConverter FirstGreaterThanSecond
    = new FuncMultiValueConverter<IComparable, bool>(x =>
    {
      var toCompare = x.ToArray();

      return toCompare switch
      {
        [] => false,
        [var first, var second] => first?.CompareTo(second) > 0,
        [..] => false
      };
    });
  
  public static readonly IMultiValueConverter FirstLessThanSecond
    = new FuncMultiValueConverter<IComparable, bool>(x =>
    {
      var toCompare = x.ToArray();

      return toCompare switch
      {
        [] => false,
        [var first, var second] => first?.CompareTo(second) < 0,
        [..] => false
      };
    });
}