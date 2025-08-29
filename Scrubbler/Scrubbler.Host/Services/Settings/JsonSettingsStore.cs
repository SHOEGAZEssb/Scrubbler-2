using System.Text.Json;
using Scrubbler.Abstractions.Settings;

namespace Scrubbler.Host.Services.Settings;

public class JsonSettingsStore : ISettingsStore
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private Dictionary<string, JsonElement> _settings = [];

    public JsonSettingsStore(string? filePath = null)
    {
        _filePath = filePath ??
            Path.Combine(AppContext.BaseDirectory, "settings.json");

        if (File.Exists(_filePath))
        {
            var json = File.ReadAllText(_filePath);
            _settings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? [];
        }
    }

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

