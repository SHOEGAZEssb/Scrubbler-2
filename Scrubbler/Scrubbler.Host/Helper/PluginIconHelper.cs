using System.Collections.Concurrent;
using Microsoft.UI.Xaml.Media.Imaging;
using Scrubbler.Abstractions.Plugin;

namespace Scrubbler.Host.Helper;

internal static class PluginIconHelper
{
    private static readonly ConcurrentDictionary<string, ImageSource> _iconCache = new();

    public static ImageSource? LoadPluginIcon(IPlugin plugin)
    {
        var filePath = Path.Combine(
            Path.GetDirectoryName(plugin.GetType().Assembly.Location)!,
            "icon.png"
        );

        if (!File.Exists(filePath))
            return null;

        // Return cached if present
        if (_iconCache.TryGetValue(filePath, out var cached))
            return cached;

        try
        {
            using var fs = File.OpenRead(filePath);

            var bmp = new BitmapImage();
            bmp.SetSource(fs.AsRandomAccessStream());

            _iconCache[filePath] = bmp;
            return bmp;
        }
        catch
        {
            return null;
        }
    }

    public static void UnloadPluginIcon(IPlugin plugin)
    {
        try
        {
            var filePath = Path.Combine(
                Path.GetDirectoryName(plugin.GetType().Assembly.Location)!,
                "icon.png"
            );

            if (_iconCache.TryRemove(filePath, out var icon))
                icon.Dispose();
        }
        catch
        {
            // Ignore
        }
    }
}
