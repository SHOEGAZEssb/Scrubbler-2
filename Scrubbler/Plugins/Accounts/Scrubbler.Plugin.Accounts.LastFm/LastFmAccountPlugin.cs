using Microsoft.UI.Xaml.Controls;
using Scrubbler.Abstractions;

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

    #endregion Properties

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
        // TODO: Implement OAuth handshake with Last.fm
        // For now, fake a login

        await Task.Delay(500); // simulate network

        AccountId = "timst";          // would come from Last.fm API
        _sessionKey = "abc123session"; // real session key from Last.fm
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
