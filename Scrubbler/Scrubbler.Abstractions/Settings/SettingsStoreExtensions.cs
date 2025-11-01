namespace Scrubbler.Abstractions.Settings;

/// <summary>
/// Extension methods for <see cref="ISettingsStore"/>.
/// </summary>
public static class SettingsStoreExtensions
{
    /// <summary>
    /// Gets a value from the store or creates a new one if it does not exist.
    /// </summary>
    /// <typeparam name="T">Type of the settings object.</typeparam>
    /// <param name="store">The settings store.</param>
    /// <param name="key">The key to fetch.</param>
    /// <param name="factory">
    /// Optional factory for creating the default value. If <c>null</c>,
    /// <c>new T()</c> is used.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the loaded or newly created settings object.</returns>
    /// <seealso cref="ISettingsStore"/>
    public static async Task<T> GetOrCreateAsync<T>(
        this ISettingsStore store,
        string key,
        Func<T>? factory = null,
        CancellationToken ct = default) where T : IPluginSettings, new()
    {
        var value = await store.GetAsync<T>(key, ct);

        if (value != null)
            return value;

        var created = factory != null ? factory() : new T();

        await store.SetAsync(key, created, ct);
        return created;
    }
}
