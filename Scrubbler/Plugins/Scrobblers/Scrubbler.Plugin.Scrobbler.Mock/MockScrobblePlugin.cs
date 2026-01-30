using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Services;
using Scrubbler.Abstractions.Settings;

namespace Scrubbler.Plugin.Scrobbler.Mock;

[PluginMetadata(
    Name = "Test Plugin",
    Description = "Just for testing purposes",
    SupportedPlatforms = PlatformSupport.All)]
public class MockScrobblePlugin : Abstractions.Plugin.PluginBase, IScrobblePlugin, IPersistentPlugin
{
    #region Properties

    private readonly JsonSettingsStore _settingsStore;
    private PluginSettings _settings = new();
    private readonly TestViewModel _vm;

    #endregion Properties

    public MockScrobblePlugin(IModuleLogServiceFactory logFactory)
        : base(logFactory)
    {
        var settingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Scrubbler", "Plugins", Name);
        Directory.CreateDirectory(settingsDir);
        _settingsStore = new JsonSettingsStore(Path.Combine(settingsDir, "settings.json"));
        _vm = new TestViewModel();
    }

    public override IPluginViewModel GetViewModel()
    {
        return _vm;
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

