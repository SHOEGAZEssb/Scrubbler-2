namespace Scrubbler.Abstractions.Plugin;

/// <summary>
/// Represents metadata about a plugin available for installation.
/// </summary>
/// <param name="Id">The unique identifier of the plugin.</param>
/// <param name="Name">The display name of the plugin.</param>
/// <param name="Version">The version string of the plugin.</param>
/// <param name="Description">A description of what the plugin does.</param>
/// <param name="PluginType">The type of plugin (e.g., "Account Plugin", "Scrobble Plugin").</param>
/// <param name="SupportedPlatforms">A list of platform names this plugin supports.</param>
/// <param name="SourceUri">The URI from which the plugin package can be downloaded.</param>
/// <param name="IconUri">Optional URI to the plugin's icon image.</param>
public record PluginManifestEntry(
    string Id,
    string Name,
    string Version,
    string Description,
    string PluginType,
    IReadOnlyList<string> SupportedPlatforms,
    Uri SourceUri,
    Uri? IconUri = null
);

