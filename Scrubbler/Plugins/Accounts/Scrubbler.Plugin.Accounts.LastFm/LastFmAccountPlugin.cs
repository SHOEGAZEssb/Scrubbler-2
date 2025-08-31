using System.Reflection;
using Microsoft.UI.Xaml.Controls;
using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Logging;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Settings;
using Shoegaze.LastFM.Authentication;

namespace Scrubbler.Plugin.Accounts.LastFm;

/// <summary>
/// A plugin that connects to a Last.fm account using session keys.
/// Implements IAccountPlugin so authentication persists between runs.
/// </summary>
public class LastFmAccountPlugin : IAccountPlugin
{
    #region Properties

    #region IPlugin

    public string Name => "Last.fm";

    public string Description => "Scrobble to a last.fm account";
    public Version Version => typeof(LastFmAccountPlugin).Assembly.GetName().Version!;

    public PlatformSupport SupportedPlatforms => PlatformSupport.All;

    public IconSource? Icon => new SymbolIconSource() { Symbol = Symbol.Play };

    public ILogService LogService { get; set; }

    private readonly ISecureStore _secureStore;
    private readonly ISettingsStore _settingsStore;
    private PluginSettings _settings = new();

    #endregion IPlugin

    public bool IsAuthenticated => !string.IsNullOrEmpty(_sessionKey);

    public bool IsScrobblingEnabled
    {
        get => _settings.IsScrobblingEnabled;
        set => _settings.IsScrobblingEnabled = value;
    }

    private const string AccountIdKey = "LastFmAccountId";
    private const string SessionKeyKey = "LastFmSessionKey";
    private const string IsScrobblingEnabledKey = "LastFmIsScrobblingEnabled";

    private string? _sessionKey;

    public string? AccountId { get; private set; }

    private readonly ApiKeyStorage _apiKeyStorage;

    #endregion Properties

    public LastFmAccountPlugin()
    {
        _apiKeyStorage = new ApiKeyStorage("LAST_FM_API_KEY", "LAST_FM_API_SECRET", "environment.env");
        LogService = new NoopLogger();

        var pluginDir = Path.GetDirectoryName(GetType().Assembly.Location)!;
        _secureStore = new FileSecureStore(Path.Combine(pluginDir, "settings.dat"), Name);
        _settingsStore = new JsonSettingsStore(Path.Combine(pluginDir, "settings.json"));
    }

    public async Task LoadAsync()
    {
        LogService.Debug("Loading settings...");

        AccountId = await _secureStore.GetAsync(AccountIdKey);
        _sessionKey = await _secureStore.GetAsync(SessionKeyKey);
        _settings = await _settingsStore.GetOrCreateAsync<PluginSettings>(Name);
    }

    public async Task SaveAsync()
    {
        if (AccountId == null)
            await _secureStore.RemoveAsync(AccountIdKey);
        else
            await _secureStore.SaveAsync(AccountIdKey, AccountId);

        if (_sessionKey == null)
            await _secureStore.RemoveAsync(SessionKeyKey);
        else
            await _secureStore.SaveAsync(SessionKeyKey, _sessionKey);

        await _settingsStore.SetAsync(Name, _settings);
    }

    public async Task AuthenticateAsync()
    {
        if (_apiKeyStorage.ApiKey == null || _apiKeyStorage.ApiSecret == null)
        {
            // todo: throw, log ?
            return;
        }

        LogService.Debug("Starting OAuth flow");
        var a = new LastfmAuthService(_apiKeyStorage.ApiKey, _apiKeyStorage.ApiSecret);
        var sessions = await a.AuthenticateAsync();

        AccountId = sessions.Username;
        _sessionKey = sessions.SessionKey;
        LogService.Debug($"Finished OAuth flow. Logged in as {AccountId}");
    }

    public Task LogoutAsync()
    {
        AccountId = null;
        _sessionKey = null;
        return Task.CompletedTask;
    }

    public IPluginViewModel GetViewModel()
    {
        throw new NotImplementedException();
    }

    public async Task ScrobbleAsync(IEnumerable<ScrobbleData> scrobbles)
    {
        if (!IsScrobblingEnabled || !IsAuthenticated)
            return; // todo: should never happen: log

        throw new NotImplementedException();
    }
}
