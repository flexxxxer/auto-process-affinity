using Avalonia.Data.Converters;
using System.Linq;

namespace UI.Converters;

static class ObjectConverters
{
  public static readonly IMultiValueConverter IsEquals
    = new FuncMultiValueConverter<object, bool>(objs => objs.Distinct().Count() is 1);
}
