using System.Text.Json;

namespace Scrubbler.Abstractions.Settings;

/// <summary>
/// Implementation of <see cref="ISettingsStore"/> that persists settings to a JSON file.
/// </summary>
/// <remarks>
/// Settings are stored in a single JSON file with pretty-printed formatting.
/// Thread-safe operations are ensured through a semaphore lock.
/// </remarks>
public class JsonSettingsStore : ISettingsStore
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly Dictionary<string, JsonElement> _settings = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonSettingsStore"/> class.
    /// </summary>
    /// <param name="filePath">The path to the JSON settings file. If <c>null</c>, defaults to "settings.json" in the application base directory.</param>
    public JsonSettingsStore(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(AppContext.BaseDirectory, "settings.json");

        if (File.Exists(_filePath))
        {
            var json = File.ReadAllText(_filePath);
            _settings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? [];
        }
    }

    /// <summary>
    /// Stores a value associated with the specified key in the JSON file.
    /// </summary>
    /// <typeparam name="T">The type of value to store.</typeparam>
    /// <param name="key">The key to associate with the value.</param>
    /// <param name="value">The value to store.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SetAsync<T>(string key, T value, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            _settings[key] = JsonSerializer.SerializeToElement(value);
            await SaveAsync(ct);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Retrieves a value associated with the specified key from the JSON file.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve.</typeparam>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the value if found; otherwise, <c>default(T)</c>.</returns>
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (_settings.TryGetValue(key, out var elem))
                return elem.Deserialize<T>();
            return default;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Removes a value associated with the specified key from the JSON file.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (_settings.Remove(key))
                await SaveAsync(ct);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task SaveAsync(CancellationToken ct)
    {
        var opts = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(_settings, opts);
        await File.WriteAllTextAsync(_filePath, json, ct);
    }
}

