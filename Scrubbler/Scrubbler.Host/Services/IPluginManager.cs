using Scrubbler.Abstractions.Plugin;

namespace Scrubbler.Host.Services;

/// <summary>
/// Manages discovery, installation, and lifetime of plugins.
/// </summary>
public interface IPluginManager
{
    /// <summary>
    /// Returns the plugins currently installed and active.
    /// </summary>
    IEnumerable<IPlugin> InstalledPlugins { get; }

    /// <summary>
    /// Returns the plugins available for installation (from repositories).
    /// May be empty until repositories are queried.
    /// </summary>
    List<PluginManifestEntry> AvailablePlugins { get; }

    bool IsFetchingPlugins { get; }
    bool IsAnyAccountPluginScrobbling { get; }

    event EventHandler<bool>? IsFetchingPluginsChanged;
    event EventHandler? PluginInstalled;
    event EventHandler? PluginUninstalled;
    event EventHandler? IsAnyAccountPluginScrobblingChanged;

    /// <summary>
    /// Installs a plugin given its metadata.
    /// </summary>
    Task InstallAsync(PluginManifestEntry plugin);

    /// <summary>
    /// Uninstalls a plugin.
    /// </summary>
    Task UninstallAsync(IPlugin plugin);
}
