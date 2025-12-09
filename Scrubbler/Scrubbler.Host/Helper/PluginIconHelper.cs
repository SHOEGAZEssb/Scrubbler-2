using Microsoft.UI.Xaml.Media.Imaging;
using Scrubbler.Abstractions.Plugin;
using SkiaSharp;

namespace Scrubbler.Host.Helper;

internal class PluginIconHelper
{
    public static ImageSource? LoadPluginIcon(IPlugin plugin)
    {
        var filePath = Path.Combine(Path.GetDirectoryName(plugin.GetType().Assembly.Location)!, "icon.png");
        if (!File.Exists(filePath))
            return null;

        // Decode the PNG using Skia
        using var skBitmap = SKBitmap.Decode(filePath);

        if (skBitmap == null)
            return null;

        try
        {
            // Convert Skia bitmap into PNG in memory
            using var skData = skBitmap.Encode(SKEncodedImageFormat.Png, 100);
            using var ms = new MemoryStream(skData.ToArray());

            // Create a BitmapImage Uno can render
            var bmp = new BitmapImage();
            bmp.SetSource(ms.AsRandomAccessStream());

            return bmp;
        }
        catch
        {
            return null;
        }
    }
}
