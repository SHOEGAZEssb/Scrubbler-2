using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Plugin.Account;
using Scrubbler.Abstractions.Services;
using Scrubbler.Abstractions.Settings;
using Shoegaze.LastFM;

namespace Scrubbler.Plugin.Scrobbler.AppleMusicScrobbler;

[PluginMetadata(
    Name = "Apple Music Scrobbler",
    Description = "Automatically scrobble tracks playing in the Apple Music desktop app",
    SupportedPlatforms = PlatformSupport.Windows)]
public class AppleMusicScrobblePlugin : PluginBase, IAutoScrobblePlugin, IPersistentPlugin, IAcceptAccountFunctions
{
    #region Properties

    private readonly ApiKeyStorage _apiKeyStorage;
    private readonly AppleMusicScrobbleViewModel _vm;
    private readonly ISettingsStore _settingsStore;
    private PluginSettings _settings = new();

    #endregion Properties

    /// <summary>
    /// Initializes a new instance of the <see cref="ManualScrobblePlugin"/> class.
    /// </summary>
    public AppleMusicScrobblePlugin(IModuleLogServiceFactory logFactory)
        : base(logFactory)
    {
        var pluginDir = Path.GetDirectoryName(GetType().Assembly.Location)!;
        _apiKeyStorage = new ApiKeyStorage(PluginDefaults.ApiKey, PluginDefaults.ApiSecret, Path.Combine(pluginDir, "environment.env"));
        _settingsStore = new JsonSettingsStore(Path.Combine(pluginDir, "settings.json"));
        _vm = new AppleMusicScrobbleViewModel(new LastfmClient(_apiKeyStorage.ApiKey, _apiKeyStorage.ApiSecret), _logService);
    }

    /// <summary>
    /// Gets the view model instance for this plugin's UI.
    /// </summary>
    /// <returns>The <see cref="IPluginViewModel"/> instance for this plugin.</returns>
    public override IPluginViewModel GetViewModel()
    {
        return _vm;
    }

    public async Task LoadAsync()
    {
        _logService.Debug("Loading settings...");

        _settings = await _settingsStore.GetOrCreateAsync<PluginSettings>(Name);
        _vm.SetInitialAutoConnectState(_settings.AutoConnect);
    }

    public async Task SaveAsync()
    {
        _logService.Debug("Saving settings...");

        _settings.AutoConnect = _vm.AutoConnect;
        await _settingsStore.SetAsync(Name, _settings);
    }

    public void SetAccountFunctionsContainer(AccountFunctionContainer container)
    {
        _vm.FunctionContainer = container;
    }
}
