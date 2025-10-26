namespace Scrubbler.Abstractions.Settings;

public interface ISecureStore
{
    Task SaveAsync(string key, string value);
    Task<string?> GetAsync(string key);
    Task RemoveAsync(string key);
}
