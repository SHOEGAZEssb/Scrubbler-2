namespace Scrubbler.Abstractions.Settings;

/// <summary>
/// Interface for storing and retrieving plugin settings.
/// </summary>
/// <remarks>
/// Settings stored via this interface are not encrypted. For sensitive data, use <see cref="ISecureStore"/>.
/// </remarks>
public interface ISettingsStore
{
    /// <summary>
    /// Stores a value associated with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of value to store.</typeparam>
    /// <param name="key">The key to associate with the value.</param>
    /// <param name="value">The value to store.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SetAsync<T>(string key, T value, CancellationToken ct = default);
    
    /// <summary>
    /// Retrieves a value associated with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve.</typeparam>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the value if found; otherwise, <c>default(T)</c>.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    
    /// <summary>
    /// Removes a value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveAsync(string key, CancellationToken ct = default);
}
