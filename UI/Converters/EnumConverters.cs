using Avalonia.Data.Converters;
using Domain;
using System;

namespace UI.Converters;

static class EnumConverters
{
  public static IValueConverter AnyEnumToFriendlyString =
    new FuncValueConverter<Enum, string>(enumVal => enumVal?.ToFriendlyString() ?? string.Empty);
}
