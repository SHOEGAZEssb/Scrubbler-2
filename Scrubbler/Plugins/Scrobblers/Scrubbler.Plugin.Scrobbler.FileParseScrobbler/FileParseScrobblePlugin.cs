using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Services;
using Scrubbler.Abstractions.Settings;

namespace Scrubbler.Plugin.Scrobbler.FileParseScrobbler;

[PluginMetadata(
    Name = "File Parse Scrobbler",
    Description = "Parses scrobbles from local files",
    SupportedPlatforms = PlatformSupport.All)]
public sealed class FileParseScrobblePlugin : PluginBase, IScrobblePlugin, IPersistentPlugin
{
    #region Properties

    private readonly JsonSettingsStore _settingsStore;
    private PluginSettings _settings = new();

    #endregion Properties

    #region Construction

    public FileParseScrobblePlugin(IModuleLogServiceFactory logFactory)
        : base(logFactory)
    {
        var settingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Scrubbler", "Plugins", Name);
        Directory.CreateDirectory(settingsDir);
        _settingsStore = new JsonSettingsStore(Path.Combine(settingsDir, "settings.json"));
    }

    #endregion Construction

    public override IPluginViewModel GetViewModel()
    {
        throw new NotImplementedException();
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
