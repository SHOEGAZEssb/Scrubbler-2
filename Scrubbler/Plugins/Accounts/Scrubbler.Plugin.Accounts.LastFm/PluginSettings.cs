using Scrubbler.Abstractions.Settings;

namespace Scrubbler.Plugin.Accounts.LastFm;
internal class PluginSettings : IPluginSettings
{
    public bool IsScrobblingEnabled { get; set; }

    public PluginSettings()
    {
        IsScrobblingEnabled = false;
    }
}
