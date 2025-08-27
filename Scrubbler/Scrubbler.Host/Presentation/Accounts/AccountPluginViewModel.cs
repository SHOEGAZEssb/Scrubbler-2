using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions;


namespace Scrubbler.Host.Presentation.Accounts;

public partial class AccountPluginViewModel : ObservableObject
{
    private readonly IAccountPlugin _plugin;

    public string Name => _plugin.Name;

    [ObservableProperty]
    private string? _accountId;

    [ObservableProperty]
    private bool _isAuthenticated;

    public IRelayCommand AuthenticateCommand { get; }
    public IRelayCommand LogoutCommand { get; }

    public AccountPluginViewModel(IAccountPlugin plugin)
    {
        _plugin = plugin;

        _accountId = plugin.AccountId;
        _isAuthenticated = plugin.IsAuthenticated;

        AuthenticateCommand = new AsyncRelayCommand(AuthenticateAsync);
        LogoutCommand = new AsyncRelayCommand(LogoutAsync);
    }

    private async Task AuthenticateAsync()
    {
        await _plugin.AuthenticateAsync();
        UpdateState();
    }

    private async Task LogoutAsync()
    {
        await _plugin.LogoutAsync();
        UpdateState();
    }

    private void UpdateState()
    {
        AccountId = _plugin.AccountId;
        IsAuthenticated = _plugin.IsAuthenticated;
    }
}

