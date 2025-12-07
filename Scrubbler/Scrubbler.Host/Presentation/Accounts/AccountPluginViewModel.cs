using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Scrubbler.Abstractions.Plugin.Account;
using Scrubbler.Host.Helper;
using SkiaSharp;


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

    public ImageSource? Icon => _icon ??= PluginIconHelper.LoadPluginIcon(_plugin);
    private ImageSource? _icon;

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

        if (_plugin is IHaveScrobbleLimit scrobbleLimitPlugin)
        {
            scrobbleLimitPlugin.CurrentScrobbleCountChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(CurrentScrobbleCount));
            };
        }
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

