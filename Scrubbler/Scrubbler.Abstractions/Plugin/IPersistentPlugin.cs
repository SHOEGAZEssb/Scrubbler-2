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
    Task LoadAsync(ISecureStore secureStore);

    /// <summary>
    /// Save plugin state to secure or non-secure storage.
    /// Called when application exits or when plugin requests persistence.
    /// </summary>
    Task SaveAsync(ISecureStore secureStore);
}
