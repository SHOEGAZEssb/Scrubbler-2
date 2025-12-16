using Scrubbler.Abstractions.Settings;

namespace Scrubbler.Plugin.Scrobbler.Mock;

internal class PluginSettings : IPluginSettings
{
    public string SomeSetting { get; set; } = "TestSetting";
}
