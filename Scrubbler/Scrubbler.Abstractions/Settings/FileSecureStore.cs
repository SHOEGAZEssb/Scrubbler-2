using System.Security.Cryptography;
using System.Text;

namespace Scrubbler.Abstractions.Settings;

/// <summary>
/// Very simple file-based secure store.
/// Uses AES encryption with a local key.
/// On real platforms (Windows native, Android/iOS), swap out for DPAPI/Keychain/Keystore.
/// </summary>
public class FileSecureStore : ISecureStore
{
    private readonly string _storePath;

    private readonly byte[] _encryptionKey;
    private readonly byte[] _encryptionIV = new byte[16]; // all zeros is fine for local dev

    private readonly Dictionary<string, string> _cache;

    public FileSecureStore(string fileName, string key)
    {
        _storePath = fileName;
        _encryptionKey = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        Directory.CreateDirectory(Path.GetDirectoryName(_storePath)!);
        _cache = LoadFromDisk();
    }

    public Task SaveAsync(string key, string value)
    {
        _cache[key] = value;
        SaveToDisk();
        return Task.CompletedTask;
    }

    public Task<string?> GetAsync(string key)
    {
        return Task.FromResult(_cache.TryGetValue(key, out var value) ? value : null);
    }

    public Task RemoveAsync(string key)
    {
        if (_cache.Remove(key))
            SaveToDisk();
        return Task.CompletedTask;
    }

    private void SaveToDisk()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(_cache);
        var bytes = Encoding.UTF8.GetBytes(json);
        var encrypted = Encrypt(bytes);
        File.WriteAllBytes(_storePath, encrypted);
    }

    private Dictionary<string, string> LoadFromDisk()
    {
        if (!File.Exists(_storePath))
            return [];

        try
        {
            var encrypted = File.ReadAllBytes(_storePath);
            var bytes = Decrypt(encrypted);
            var json = Encoding.UTF8.GetString(bytes);
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                   ?? [];
        }
        catch
        {
            return [];
        }
    }

    private byte[] Encrypt(byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.IV = _encryptionIV;
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        cs.Write(data, 0, data.Length);
        cs.FlushFinalBlock();
        return ms.ToArray();
    }

    private byte[] Decrypt(byte[] encrypted)
    {
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.IV = _encryptionIV;
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
        cs.Write(encrypted, 0, encrypted.Length);
        cs.FlushFinalBlock();
        return ms.ToArray();
    }
}
