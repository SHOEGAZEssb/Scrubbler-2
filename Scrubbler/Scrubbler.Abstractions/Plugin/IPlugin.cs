namespace Scrubbler.Abstractions.Plugin;

/// <summary>
/// Specifies which platforms a plugin supports.
/// </summary>
[Flags]
public enum PlatformSupport
{
    /// <summary>
    /// Windows platform support.
    /// </summary>
    Windows = 1,
    /// <summary>
    /// macOS platform support.
    /// </summary>
    Mac = 2,
    /// <summary>
    /// Linux platform support.
    /// </summary>
    Linux = 4,
    /// <summary>
    /// All platforms supported.
    /// </summary>
    All = Windows | Mac | Linux
}

/// <summary>
/// Base interface for all plugins in the Scrubbler system.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Gets the display name of the plugin.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a description of what the plugin does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the version of the plugin.
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// Gets the platforms this plugin supports.
    /// </summary>
    PlatformSupport SupportedPlatforms { get; }

    /// <summary>
    /// Gets the view model instance for this plugin's UI.
    /// </summary>
    /// <returns>A new instance of <see cref="IPluginViewModel"/> for this plugin.</returns>
    IPluginViewModel GetViewModel();
}
