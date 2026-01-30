using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Services;
using Shoegaze.LastFM;

namespace Scrubbler.Plugin.Scrobblers.FriendScrobbler;

[PluginMetadata(
    Name = "Friend Scrobbler",
    Description = "Scrobble tracks from another last.fm user",
    SupportedPlatforms = PlatformSupport.All)]
public class FriendScrobblePlugin : Abstractions.Plugin.PluginBase, IScrobblePlugin
{
    #region Properties

    private readonly ApiKeyStorage _apiKeyStorage;
    private readonly FriendScrobbleViewModel _vm;

    #endregion Properties

    /// <summary>
    /// Initializes a new instance of the <see cref="ManualScrobblePlugin"/> class.
    /// </summary>
    public FriendScrobblePlugin(IModuleLogServiceFactory logFactory)
        : base(logFactory)
    {
        var pluginDir = Path.GetDirectoryName(GetType().Assembly.Location)!;
        _apiKeyStorage = new ApiKeyStorage(PluginDefaults.ApiKey, PluginDefaults.ApiSecret, Path.Combine(pluginDir, "environment.env"));
        _vm = new FriendScrobbleViewModel(new LastfmClient(_apiKeyStorage.ApiKey, _apiKeyStorage.ApiSecret));
    }

    /// <summary>
    /// Gets the view model instance for this plugin's UI.
    /// </summary>
    /// <returns>The <see cref="IPluginViewModel"/> instance for this plugin.</returns>
    public override IPluginViewModel GetViewModel()
    {
        return _vm;
    }
}
