namespace Scrubbler.Abstractions.Settings;

public interface ISettingsStore
{
    Task SetAsync<T>(string key, T value, CancellationToken ct = default);
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
}
