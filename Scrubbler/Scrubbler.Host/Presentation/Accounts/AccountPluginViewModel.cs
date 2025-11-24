using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions.Plugin.Account;


namespace Scrubbler.Host.Presentation.Accounts;

public partial class AccountPluginViewModel : ObservableObject
{
    private readonly IAccountPlugin _plugin;

    public string Name => _plugin.Name;

    [ObservableProperty]
    private string? _accountId;

    [ObservableProperty]
    private bool _isAuthenticated;

    [ObservableProperty]
    private bool _isBusy;

    public bool IsScrobblingEnabled
    {
        get => _plugin.IsScrobblingEnabled;
        set
        {
            if (_plugin.IsScrobblingEnabled != value)
            {
                _plugin.IsScrobblingEnabled = value;
                OnPropertyChanged();
            }
        }
    }

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
        try
        {
            IsBusy = true;
            await _plugin.AuthenticateAsync();
            UpdateState();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LogoutAsync()
    {
        try
        {
            IsBusy = true;
            await _plugin.LogoutAsync();
            UpdateState();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateState()
    {
        AccountId = _plugin.AccountId;
        IsAuthenticated = _plugin.IsAuthenticated;
    }
}

