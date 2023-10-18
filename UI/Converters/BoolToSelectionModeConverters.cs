using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace UI.Converters;

public static class BoolToSelectionModeConverters
{
  /// <summary>
  /// <see langword="null"/> match <see cref="SelectionMode.Toggle"/>,
  /// <see langword="false"/> match <see cref="SelectionMode.Single"/>,
  /// <see langword="true"/> match <see cref="SelectionMode.Multiple"/>
  /// </summary>
  public static readonly IValueConverter ToSelectionMode
    = new FuncValueConverter<bool?, SelectionMode>(val => val switch
    {
      null => SelectionMode.Toggle,
      false => SelectionMode.Single,
      true => SelectionMode.Multiple
    });
  
  /// <summary>
  /// <see langword="false"/> match <see cref="DataGridSelectionMode.Single"/>,
  /// <see langword="true"/> match <see cref="DataGridSelectionMode.Extended"/>
  /// </summary>
  public static readonly IValueConverter ToDataGridSelectionMode
    = new FuncValueConverter<bool, DataGridSelectionMode>(val => val switch
    {
      false => DataGridSelectionMode.Single,
      true => DataGridSelectionMode.Extended
    });
}