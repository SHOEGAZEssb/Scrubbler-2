using Scrubbler.Abstractions;

namespace Scrubbler.Tests.AbstractionsTest;

[TestFixture]
internal class ApiKeyStorageTest
{
    private string _envFilePath = null!;

    [SetUp]
    public void SetUp()
    {
        _envFilePath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid():N}.env");
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_envFilePath))
            File.Delete(_envFilePath);
    }

    [Test]
    public void UsesValuesFromEnvFile_WhenPresent()
    {
        File.WriteAllText(
            _envFilePath,
            """
            API_KEY=my-key
            API_SECRET=my-secret
            """);

        var storage = new ApiKeyStorage(
            apiKeyDefault: "API_KEY",
            apiSecretDefault: "API_SECRET",
            envFile: _envFilePath);

        Assert.That(storage.ApiKey, Is.EqualTo("my-key"));
        Assert.That(storage.ApiSecret, Is.EqualTo("my-secret"));
    }

    [Test]
    public void FallsBackToDefaults_WhenEnvFileMissing()
    {
        var storage = new ApiKeyStorage(
            apiKeyDefault: "default-key",
            apiSecretDefault: "default-secret",
            envFile: _envFilePath);

        Assert.That(storage.ApiKey, Is.EqualTo("default-key"));
        Assert.That(storage.ApiSecret, Is.EqualTo("default-secret"));
    }

    [Test]
    public void FallsBackToDefaults_WhenEnvFileDoesNotContainKeys()
    {
        File.WriteAllText(
            _envFilePath,
            """
            SOME_OTHER_KEY=value
            """);

        var storage = new ApiKeyStorage(
            apiKeyDefault: "fallback-key",
            apiSecretDefault: "fallback-secret",
            envFile: _envFilePath);

        Assert.That(storage.ApiKey, Is.EqualTo("fallback-key"));
        Assert.That(storage.ApiSecret, Is.EqualTo("fallback-secret"));
    }

    [Test]
    public void EnvFile_IsCaseInsensitive()
    {
        File.WriteAllText(
            _envFilePath,
            """
            api_key=lowercase-key
            api_secret=lowercase-secret
            """);

        var storage = new ApiKeyStorage(
            apiKeyDefault: "API_KEY",
            apiSecretDefault: "API_SECRET",
            envFile: _envFilePath);

        Assert.That(storage.ApiKey, Is.EqualTo("lowercase-key"));
        Assert.That(storage.ApiSecret, Is.EqualTo("lowercase-secret"));
    }

    [Test]
    public void IgnoresCommentsAndEmptyLines()
    {
        File.WriteAllText(
            _envFilePath,
            """
            # comment

            API_KEY=key
            API_SECRET=secret
            """);

        var storage = new ApiKeyStorage(
            apiKeyDefault: "API_KEY",
            apiSecretDefault: "API_SECRET",
            envFile: _envFilePath);

        Assert.That(storage.ApiKey, Is.EqualTo("key"));
        Assert.That(storage.ApiSecret, Is.EqualTo("secret"));
    }

    [Test]
    public void Throws_WhenDefaultsAreNull()
    {
        Assert.That(
            () => new ApiKeyStorage(null!, null!, _envFilePath),
            Throws.Exception);
    }
}
