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

    #endregion IPlugin

    public bool IsAuthenticated => !string.IsNullOrEmpty(_sessionKey);

    public bool IsScrobblingEnabled { get; set; }

    private const string AccountIdKey = "LastFmAccountId";
    private const string SessionKeyKey = "LastFmSessionKey";
    private const string IsScrobblingEnabledKey = "LastFmIsScrobblingEnabled";

    private string? _sessionKey;

    public string? AccountId { get; private set; }

    private ApiKeyStorage _apiKeyStorage;

    #endregion Properties

    public LastFmAccountPlugin()
    {
        _apiKeyStorage = new ApiKeyStorage("LAST_FM_API_KEY", "LAST_FM_API_SECRET", "environment.env");
        LogService = new NoopLogger();
    }

    public async Task LoadAsync(ISecureStore secureStore)
    {
        LogService.Debug("Loading settings...");

        AccountId = await secureStore.GetAsync(AccountIdKey);
        _sessionKey = await secureStore.GetAsync(SessionKeyKey);
        var s = await secureStore.GetAsync(IsScrobblingEnabledKey);
        IsScrobblingEnabled = s != null && bool.Parse(s);
    }

    public async Task SaveAsync(ISecureStore secureStore)
    {
        if (AccountId == null)
            await secureStore.RemoveAsync(AccountIdKey);
        else
            await secureStore.SaveAsync(AccountIdKey, AccountId);

        if (_sessionKey == null)
            await secureStore.RemoveAsync(SessionKeyKey);
        else
            await secureStore.SaveAsync(SessionKeyKey, _sessionKey);

        await secureStore.SaveAsync(IsScrobblingEnabledKey, IsScrobblingEnabled.ToString());
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
