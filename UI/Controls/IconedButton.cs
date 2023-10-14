using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace UI.Controls;

public class IconedButton : Button
{
  public static readonly StyledProperty<StreamGeometry> IconGeometryProperty =
      AvaloniaProperty.Register<IconedButton, StreamGeometry>(nameof(IconGeometryProperty));

  public static readonly StyledProperty<IBrush> IconStrokeProperty =
      AvaloniaProperty.Register<IconedButton, IBrush>(nameof(IconStrokeProperty));

  public static readonly StyledProperty<double> IconStrokeThicknessProperty =
      AvaloniaProperty.Register<IconedButton, double>(nameof(IconStrokeThicknessProperty));

  public static readonly StyledProperty<ITransform> IconRenderTransformProperty =
      AvaloniaProperty.Register<IconedButton, ITransform>(nameof(IconRenderTransformProperty));

  public static readonly StyledProperty<RelativePoint> IconRenderTransformOriginProperty =
      AvaloniaProperty.Register<IconedButton, RelativePoint>(nameof(IconRenderTransformOriginProperty));

  public static readonly StyledProperty<IBrush> IconFillProperty =
      AvaloniaProperty.Register<IconedButton, IBrush>(nameof(IconFillProperty));

  public static readonly StyledProperty<Thickness> IconMarginProperty =
      AvaloniaProperty.Register<IconedButton, Thickness>(nameof(IconMarginProperty));

  public StreamGeometry IconGeometry
  {
    get => GetValue(IconGeometryProperty);
    set => SetValue(IconGeometryProperty, value);
  }

  public IBrush IconStroke
  {
    get => GetValue(IconStrokeProperty);
    set => SetValue(IconStrokeProperty, value);
  }

  public double IconStrokeThickness
  {
    get => GetValue(IconStrokeThicknessProperty);
    set => SetValue(IconStrokeThicknessProperty, value);
  }

  public ITransform IconRenderTransform
  {
    get => GetValue(IconRenderTransformProperty);
    set => SetValue(IconRenderTransformProperty, value);
  }

  public RelativePoint IconRenderTransformOrigin
  {
    get => GetValue(IconRenderTransformOriginProperty);
    set => SetValue(IconRenderTransformOriginProperty, value);
  }

  public IBrush IconFill
  {
    get => GetValue(IconFillProperty);
    set => SetValue(IconFillProperty, value);
  }

  public Thickness IconMargin
  {
    get => GetValue(IconMarginProperty);
    set => SetValue(IconMarginProperty, value);
  }

  public IconedButton()
  {
    IconStroke = Brushes.White;
    IconStrokeThickness = 1;
    IconRenderTransform = new ScaleTransform(0.5, 0.5);
    IconRenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
    Margin = new Thickness(0, 0, 0, 0);
    IconMargin = new Thickness(0, 0, 0, 0);
    MaxHeight = double.PositiveInfinity;
    ContentTemplate = null;
    ContextMenu = null;
  }
}
