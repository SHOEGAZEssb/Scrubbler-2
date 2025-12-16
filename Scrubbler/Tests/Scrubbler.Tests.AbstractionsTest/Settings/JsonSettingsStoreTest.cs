using Scrubbler.Abstractions.Settings;

namespace Scrubbler.Tests.AbstractionsTest.Settings;

[TestFixture]
internal class JsonSettingsStoreTest
{
    private string _tempFilePath = null!;

    [SetUp]
    public void SetUp()
    {
        _tempFilePath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid():N}.json");
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_tempFilePath))
            File.Delete(_tempFilePath);
    }

    [Test]
    public async Task SetAndGetValueTest()
    {
        var store = new JsonSettingsStore(_tempFilePath);
        await store.SetAsync("answer", 42);
        var result = await store.GetAsync<int>("answer");
        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public async Task GetMissingKeyReturnsDefault()
    {
        var store = new JsonSettingsStore(_tempFilePath);
        var result = await store.GetAsync<string>("missing");
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task RemoveKeyDeletesValue()
    {
        var store = new JsonSettingsStore(_tempFilePath);

        await store.SetAsync("flag", true);
        await store.RemoveAsync("flag");

        var result = await store.GetAsync<bool>("flag");

        Assert.That(result, Is.False); // default(bool)
    }

    [Test]
    public async Task ValuesPersistAcrossInstances()
    {
        var store1 = new JsonSettingsStore(_tempFilePath);
        await store1.SetAsync("name", "Scrubbler");

        var store2 = new JsonSettingsStore(_tempFilePath);
        var result = await store2.GetAsync<string>("name");

        Assert.That(result, Is.EqualTo("Scrubbler"));
    }

    [Test]
    public async Task ComplexObjectIsSerializedAndDeserialized()
    {
        var store = new JsonSettingsStore(_tempFilePath);

        var value = new TestSettings
        {
            Volume = 75,
            Enabled = true
        };

        await store.SetAsync("settings", value);

        var result = await store.GetAsync<TestSettings>("settings");

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result!.Volume, Is.EqualTo(75));
            Assert.That(result.Enabled, Is.True);
        }
    }

    private sealed class TestSettings
    {
        public int Volume { get; set; }
        public bool Enabled { get; set; }
    }
}
