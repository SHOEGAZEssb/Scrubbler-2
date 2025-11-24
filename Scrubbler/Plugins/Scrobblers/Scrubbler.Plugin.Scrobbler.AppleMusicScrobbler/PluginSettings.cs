using Scrubbler.Abstractions.Settings;

namespace Scrubbler.Plugin.Scrobbler.AppleMusicScrobbler;

internal class PluginSettings : IPluginSettings
{
    public bool AutoConnect { get; set; } = false;
}
