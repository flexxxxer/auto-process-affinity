using System;

using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace UI.Converters;

public class SteamGeometryToBitmapExtension : MarkupExtension
{
  public Geometry Geometry { get; set; } = null!;
  public IBrush Fill { get; set; } = null!;

  public override object ProvideValue(IServiceProvider serviceProvider)
  {
    var visual = new Path
    {
      Data = Geometry, Fill = Fill,
    };

    var bitmap = new RenderTargetBitmap(new PixelSize(26, 26));
    bitmap.Render(visual);
    return bitmap;
  }
}