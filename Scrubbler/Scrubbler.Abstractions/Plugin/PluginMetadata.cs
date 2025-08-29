namespace Scrubbler.Abstractions.Plugin;

public partial record PluginMetadata(
    string Id,
    string Name,
    Version Version,
    string Description,
    IReadOnlyList<string> SupportedPlatforms
) : IPluginMetadata;
