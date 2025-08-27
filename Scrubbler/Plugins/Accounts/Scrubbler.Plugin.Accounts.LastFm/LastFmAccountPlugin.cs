using Microsoft.UI.Xaml.Controls;
using Scrubbler.Abstractions;
using Shoegaze.LastFM.Authentication;

namespace Scrubbler.Plugin.Accounts.LastFm;

/// <summary>
/// A plugin that connects to a Last.fm account using session keys.
/// Implements IAccountPlugin so authentication persists between runs.
/// </summary>
public class LastFmAccountPlugin : IAccountPlugin
{
    #region Properties

    public string Name => "Last.fm";

    public bool IsAuthenticated => !string.IsNullOrEmpty(_sessionKey);

    public string Description => "Scrobble to a last.fm account";

    public PlatformSupport SupportedPlatforms => PlatformSupport.All;

    public IconSource? Icon => throw new NotImplementedException();

    private const string AccountIdKey = "LastFmAccountId";
    private const string SessionKeyKey = "LastFmSessionKey";

    private string? _sessionKey;

    public string? AccountId { get; private set; }

    private ApiKeyStorage _apiKeyStorage;

    #endregion Properties

    public LastFmAccountPlugin()
    {
        _apiKeyStorage = new ApiKeyStorage("LAST_FM_API_KEY", "LAST_FM_API_KEY", "environment.env");
    }

    public async Task LoadAsync(ISecureStore secureStore)
    {
        AccountId = await secureStore.GetAsync(AccountIdKey);
        _sessionKey = await secureStore.GetAsync(SessionKeyKey);
    }

    public async Task SaveAsync(ISecureStore secureStore)
    {
        if (AccountId != null)
            await secureStore.SaveAsync(AccountIdKey, AccountId);

        if (_sessionKey != null)
            await secureStore.SaveAsync(SessionKeyKey, _sessionKey);
    }

    public async Task AuthenticateAsync()
    {
        var a = new LastfmAuthService("70d5cd49cba6da4be4e985a0ede393df", "2230e17a1196da95cf24dc2060b27218");
        var sessions = await a.AuthenticateAsync();

        AccountId = sessions.Username;
        _sessionKey = sessions.SessionKey;
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
}
