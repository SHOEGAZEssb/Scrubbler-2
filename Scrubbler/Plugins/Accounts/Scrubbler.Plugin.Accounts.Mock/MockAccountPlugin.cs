using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Plugin.Account;
using Scrubbler.Abstractions.Services;

namespace Scrubbler.Plugin.Accounts.Mock;

[PluginMetadata(
    Name = "Mock",
    Description = "A fake account plugin for testing",
    SupportedPlatforms = PlatformSupport.All)]
public class MockAccountPlugin(IModuleLogServiceFactory logFactory) : PluginBase(logFactory), IAccountPlugin
{
    public string? AccountId => IsAuthenticated ? "Fake User" : null;

    public bool IsAuthenticated { get; private set; }

    public bool IsScrobblingEnabled { get; set; }

    public event EventHandler? IsScrobblingEnabledChanged;

    public Task AuthenticateAsync()
    {
        IsAuthenticated = true;
        return Task.CompletedTask;
    }

    public override IPluginViewModel GetViewModel()
    {
        throw new NotImplementedException();
    }

    public Task LoadAsync()
    {
        return Task.CompletedTask;
    }

    public Task LogoutAsync()
    {
        IsAuthenticated = false;
        return Task.CompletedTask;
    }

    public Task SaveAsync()
    {
        return Task.CompletedTask;
    }

    public Task<ScrobbleResponse> ScrobbleAsync(IEnumerable<ScrobbleData> scrobbles)
    {
        return Task.FromResult(new ScrobbleResponse(true, null));
    }
}

