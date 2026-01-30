using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Services;
using Scrubbler.Plugin.Accounts.LastFm;
using Shoegaze.LastFM;

namespace Scrubbler.Plugin.Scrobblers.DatabaseScrobbler;

[PluginMetadata(
    Name = "Database Scrobbler",
    Description = "Search and scrobble from various online music databases",
    SupportedPlatforms = PlatformSupport.All)]
public class DatabaseScrobblePlugin : Abstractions.Plugin.PluginBase, IScrobblePlugin
{
    #region Properties

    private readonly ApiKeyStorage _apiKeyStorage;
    private readonly LastfmClient _lastFmClient;
    private readonly DatabaseScrobbleViewModel _vm;

    #endregion Properties

    #region Construction

    public DatabaseScrobblePlugin(IModuleLogServiceFactory logFactory)
        : base(logFactory)
    {
        var pluginDir = Path.GetDirectoryName(GetType().Assembly.Location)!;
        _apiKeyStorage = new ApiKeyStorage(PluginDefaults.ApiKey, PluginDefaults.ApiSecret, Path.Combine(pluginDir, "environment.env"));
        _lastFmClient = new LastfmClient(_apiKeyStorage.ApiKey, _apiKeyStorage.ApiSecret);
        _vm = new DatabaseScrobbleViewModel(_logService, _lastFmClient);
    }

    #endregion Construction

    public override IPluginViewModel GetViewModel()
    {
        return _vm;
    }
}

