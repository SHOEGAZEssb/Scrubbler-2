using Scrubbler.Abstractions.Plugin;

namespace Scrubbler.Host.Services;

/// <summary>
/// Manages discovery, installation, and lifetime of plugins.
/// </summary>
public interface IPluginManager
{
    /// <summary>
    /// Gets the plugins currently installed and active in the application.
    /// </summary>
    /// <returns>A collection of installed <see cref="IPlugin"/> instances.</returns>
    IEnumerable<IPlugin> InstalledPlugins { get; }

    /// <summary>
    /// Gets the plugins available for installation from configured repositories.
    /// </summary>
    /// <remarks>
    /// May be empty until repositories are queried.
    /// </remarks>
    /// <returns>A list of <see cref="PluginManifestEntry"/> representing available plugins.</returns>
    List<PluginManifestEntry> AvailablePlugins { get; }

    /// <summary>
    /// Gets a value indicating whether the plugin manager is currently fetching available plugins from repositories.
    /// </summary>
    bool IsFetchingPlugins { get; }

    /// <summary>
    /// Gets a value indicating whether any account plugin currently has scrobbling enabled.
    /// </summary>
    bool IsAnyAccountPluginScrobbling { get; }

    /// <summary>
    /// Event that is raised when <see cref="IsFetchingPlugins"/> changes.
    /// </summary>
    event EventHandler<bool>? IsFetchingPluginsChanged;

    /// <summary>
    /// Event that is raised when a plugin is installed.
    /// </summary>
    event EventHandler? PluginInstalled;

    /// <summary>
    /// Event that is raised when a plugin is uninstalled.
    /// </summary>
    event EventHandler? PluginUninstalled;

    /// <summary>
    /// Event that is raised when <see cref="IsAnyAccountPluginScrobbling"/> changes.
    /// </summary>
    event EventHandler? IsAnyAccountPluginScrobblingChanged;

    /// <summary>
    /// Installs a plugin from its manifest entry.
    /// </summary>
    /// <param name="plugin">The <see cref="PluginManifestEntry"/> describing the plugin to install.</param>
    /// <returns>A task that represents the asynchronous installation operation.</returns>
    /// <exception cref="HttpRequestException">Thrown when downloading the plugin package fails.</exception>
    /// <exception cref="IOException">Thrown when file operations fail during installation.</exception>
    Task InstallAsync(PluginManifestEntry plugin);

    /// <summary>
    /// Uninstalls a plugin and removes its files from disk.
    /// </summary>
    /// <param name="plugin">The <see cref="IPlugin"/> instance to uninstall.</param>
    /// <returns>A task that represents the asynchronous uninstallation operation.</returns>
    Task UninstallAsync(IPlugin plugin);
}
