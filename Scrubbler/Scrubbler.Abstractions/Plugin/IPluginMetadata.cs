namespace Scrubbler.Abstractions.Plugin;

/// <summary>
/// Describes a plugin package that can be installed.
/// </summary>
public interface IPluginMetadata
{
    /// <summary>
    /// Unique identifier for the plugin (e.g. "LastFm.Account").
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Display name (e.g. "Last.fm Account").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Version of the plugin.
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// Short description of what the plugin does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// List of platforms this plugin supports (Windows, Linux, macOS, WASM).
    /// </summary>
    IReadOnlyList<string> SupportedPlatforms { get; }
}
