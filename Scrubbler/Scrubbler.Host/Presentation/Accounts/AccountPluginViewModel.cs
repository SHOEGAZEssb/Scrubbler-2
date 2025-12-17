using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions.Plugin.Account;
using Scrubbler.Host.Helper;


namespace Scrubbler.Host.Presentation.Accounts;

public partial class AccountPluginViewModel : ObservableObject
{
    #region Properties

    public string Name => _plugin.Name;

    public string PluginId => _plugin.Id;

    public string? AccountId => _plugin.AccountId;

    public bool IsAuthenticated => _plugin.IsAuthenticated;

    [ObservableProperty]
    public bool _isBusy;

    public ImageSource? Icon => _icon ??= PluginIconHelper.LoadPluginIcon(_plugin);
    private ImageSource? _icon;

    public bool HasScrobbleLimit => _plugin is IHaveScrobbleLimit;

    public bool HasAccountFunctions => _plugin is IHaveAccountFunctions;

    public bool IsScrobblingEnabled
    {
        get => _plugin.IsScrobblingEnabled;
        set
        {
            if (IsScrobblingEnabled != value)
            {
                _plugin.IsScrobblingEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    public bool SupportsScrobbleLimit => _plugin is IHaveScrobbleLimit;

    public int? ScrobbleLimit
    {
        get
        {
            if (_plugin is IHaveScrobbleLimit scrobbleLimitPlugin)
                return scrobbleLimitPlugin.ScrobbleLimit;
            return null;
        }
    }

    public int? CurrentScrobbleCount
    {
        get
        {
            if (_plugin is IHaveScrobbleLimit scrobbleLimitPlugin)
                return scrobbleLimitPlugin.CurrentScrobbleCount;
            return null;
        }
    }

    public event EventHandler<bool>? RequestedIsUsingAccountFunctionsChange;

    public bool IsUsingAccountFunctions
    {
        get => _config.Value.AccountFunctionsPluginID == _plugin.Id;
        set
        {
            if (IsUsingAccountFunctions != value)
            {
                RequestedIsUsingAccountFunctionsChange?.Invoke(this, value);
                OnPropertyChanged();
            }
        }
    }

    private readonly IAccountPlugin _plugin;
    private readonly IWritableOptions<UserConfig> _config;

    #endregion Properties

    #region Construction

    public AccountPluginViewModel(IAccountPlugin plugin, IWritableOptions<UserConfig> config)
    {
        _plugin = plugin;
        _config = config;

        if (_plugin is IHaveScrobbleLimit scrobbleLimitPlugin)
        {
            scrobbleLimitPlugin.CurrentScrobbleCountChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(CurrentScrobbleCount));
            };
        }
    }

    #endregion Construction

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
            UpdateAuthenticationState();
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
            IsUsingAccountFunctions = false;
            UpdateAuthenticationState();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateAuthenticationState()
    {
        OnPropertyChanged(nameof(AccountId));
        OnPropertyChanged(nameof(IsAuthenticated));
    }
}

