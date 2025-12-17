using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Services;
using Scrubbler.Abstractions.Settings;

namespace Scrubbler.Plugin.Scrobbler.Mock;

[PluginMetadata(
    Name = "Test Plugin",
    Description = "Just for testing purposes",
    SupportedPlatforms = PlatformSupport.All)]
public class MockScrobblePlugin : PluginBase, IScrobblePlugin
{
    #region Properties

    private readonly ISettingsStore _settingsStore;
    private PluginSettings _settings = new();

    #endregion Properties

    public MockScrobblePlugin(IModuleLogServiceFactory logFactory)
        : base(logFactory)
    {
        var settingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Scrubbler", "Plugins", Name);
        Directory.CreateDirectory(settingsDir);
        _settingsStore = new JsonSettingsStore(Path.Combine(settingsDir, "settings.json"));
    }

    public override IPluginViewModel GetViewModel()
    {
        return null!;
    }

    public async Task LoadAsync()
    {
        _logService.Debug("Loading settings...");
        _settings = await _settingsStore.GetOrCreateAsync<PluginSettings>(Name);
    }

    public async Task SaveAsync()
    {
        _logService.Debug("Saving settings...");
        await _settingsStore.SetAsync(Name, _settings);
    }
}

