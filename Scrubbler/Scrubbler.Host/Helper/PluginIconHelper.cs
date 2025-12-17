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
        if (_iconCache.TryGetValue(plugin.Id, out var cached))
            return cached;

        try
        {
            using var fs = File.OpenRead(filePath);

            var bmp = new BitmapImage();
            bmp.SetSource(fs.AsRandomAccessStream());

            _iconCache[plugin.Id] = bmp;
            return bmp;
        }
        catch
        {
            return null;
        }
    }

    public static void UnloadPluginIcon(IPlugin plugin)
    {
        UnloadPluginIcon(plugin.Id);
    }

    public static void UnloadPluginIcon(string id)
    {
        try
        {
            if (_iconCache.TryRemove(id, out var icon))
                icon.Dispose();
        }
        catch
        {
            // Ignore
        }
    }
}
