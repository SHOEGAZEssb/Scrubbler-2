using Scrubbler.Abstractions.Settings;

namespace Scrubbler.Tests.AbstractionsTest.Settings;

[TestFixture]
internal class FileSecureStoreTest
{
    private string _tempFilePath = null!;

    private const string EncryptionKey = "super-secret-key";

    [SetUp]
    public void SetUp()
    {
        _tempFilePath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid():N}",
            "securestore.bin");
    }

    [TearDown]
    public void TearDown()
    {
        var dir = Path.GetDirectoryName(_tempFilePath)!;
        if (Directory.Exists(dir))
            Directory.Delete(dir, recursive: true);
    }

    [Test]
    public async Task SaveAndGetValueTest()
    {
        var store = new FileSecureStore(_tempFilePath, EncryptionKey);
        await store.SaveAsync("token", "abc123");
        var result = await store.GetAsync("token");
        Assert.That(result, Is.EqualTo("abc123"));
    }

    [Test]
    public async Task GetMissingKeyReturnsNull()
    {
        var store = new FileSecureStore(_tempFilePath, EncryptionKey);
        var result = await store.GetAsync("missing");
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task RemoveDeletesValue()
    {
        var store = new FileSecureStore(_tempFilePath, EncryptionKey);

        await store.SaveAsync("secret", "value");
        await store.RemoveAsync("secret");

        var result = await store.GetAsync("secret");
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ValuesPersistAcrossInstances()
    {
        var store1 = new FileSecureStore(_tempFilePath, EncryptionKey);
        await store1.SaveAsync("password", "hunter2");

        var store2 = new FileSecureStore(_tempFilePath, EncryptionKey);
        var result = await store2.GetAsync("password");

        Assert.That(result, Is.EqualTo("hunter2"));
    }

    [Test]
    public async Task DataIsEncryptedOnDisk()
    {
        var store = new FileSecureStore(_tempFilePath, EncryptionKey);
        await store.SaveAsync("apiKey", "plaintext-secret");

        var rawBytes = File.ReadAllBytes(_tempFilePath);
        var rawText = System.Text.Encoding.UTF8.GetString(rawBytes);

        Assert.That(rawText, Does.Not.Contain("plaintext-secret"));
        Assert.That(rawText, Does.Not.Contain("apiKey"));
    }

    [Test]
    public async Task WrongEncryptionKeyResultsInEmptyStore()
    {
        var store1 = new FileSecureStore(_tempFilePath, EncryptionKey);
        await store1.SaveAsync("token", "correct");

        var store2 = new FileSecureStore(_tempFilePath, "wrong-key");
        var result = await store2.GetAsync("token");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void CorruptedFileDoesNotThrowAndResultsInEmptyStore()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_tempFilePath)!);
        File.WriteAllBytes(_tempFilePath, [1, 2, 3, 4, 5]); // invalid ciphertext

        Assert.That(
            () => new FileSecureStore(_tempFilePath, EncryptionKey),
            Throws.Nothing);
    }
}
