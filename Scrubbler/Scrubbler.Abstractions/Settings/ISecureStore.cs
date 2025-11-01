namespace Scrubbler.Abstractions.Settings;

/// <summary>
/// Interface for securely storing and retrieving sensitive data (e.g., API keys, passwords, tokens).
/// </summary>
/// <remarks>
/// Data stored via this interface should be encrypted. For non-sensitive settings, use <see cref="ISettingsStore"/>.
/// </remarks>
public interface ISecureStore
{
    /// <summary>
    /// Securely stores a string value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to associate with the value.</param>
    /// <param name="value">The value to store securely.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SaveAsync(string key, string value);
    
    /// <summary>
    /// Retrieves a securely stored value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the value if found; otherwise, <c>null</c>.</returns>
    Task<string?> GetAsync(string key);
    
    /// <summary>
    /// Removes a securely stored value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveAsync(string key);
}
