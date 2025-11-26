using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions.Plugin.Account;


namespace Scrubbler.Host.Presentation.Accounts;

public partial class AccountPluginViewModel : ObservableObject
{
    #region Properties

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

    public event EventHandler<bool>? RequestedIsUsingAccountFunctionsChange;

    public bool IsUsingAccountFunctions
    {
        get => _config.Value.AccountFunctionsPluginID == _plugin.Name;
        set
        {
            RequestedIsUsingAccountFunctionsChange?.Invoke(this, value);
            OnPropertyChanged();
        }
    }

    private readonly IAccountPlugin _plugin;
    private readonly IWritableOptions<UserConfig> _config;

    #endregion Properties

    public AccountPluginViewModel(IAccountPlugin plugin, IWritableOptions<UserConfig> config)
    {
        _plugin = plugin;
        _config = config;

        AccountId = plugin.AccountId;
        IsAuthenticated = plugin.IsAuthenticated;
    }

    internal void UpdateIsUsingAccountFunctions()
    {
        OnPropertyChanged(nameof(IsUsingAccountFunctions));
    }

    [RelayCommand]
    private async Task Authenticate()
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

    [RelayCommand]
    private async Task Logout()
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

