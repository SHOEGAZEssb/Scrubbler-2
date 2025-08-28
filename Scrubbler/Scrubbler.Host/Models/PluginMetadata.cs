namespace Scrubbler.Host.Models;

public partial record PluginMetadata(
    string Id,
    string Name,
    Version Version,
    string Description,
    Uri? IconUri,
    IReadOnlyList<string> SupportedPlatforms,
    Uri SourceUri
) : IPluginMetadata;
