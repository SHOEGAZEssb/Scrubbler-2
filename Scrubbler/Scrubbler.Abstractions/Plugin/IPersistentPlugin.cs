namespace Scrubbler.Abstractions.Plugin;

/// <summary>
/// A plugin that persists state across application restarts.
/// </summary>
public interface IPersistentPlugin : IPlugin
{
    /// <summary>
    /// Load plugin state from secure or non-secure storage.
    /// Called once at startup.
    /// </summary>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    /// <exception cref="Exception">Thrown when loading fails.</exception>
    Task LoadAsync();

    /// <summary>
    /// Save plugin state to secure or non-secure storage.
    /// Called when application exits or when plugin requests persistence.
    /// </summary>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    /// <exception cref="Exception">Thrown when saving fails.</exception>
    Task SaveAsync();
}
