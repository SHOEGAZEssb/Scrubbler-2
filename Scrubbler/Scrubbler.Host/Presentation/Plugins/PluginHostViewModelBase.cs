using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Plugin.Account;
using Scrubbler.Abstractions.Services;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation.Plugins;

internal abstract partial class PluginHostViewModelBase(IPlugin plugin, IPluginManager pluginManager, IUserFeedbackService userFeedbackService, IDialogService dialogService) : ObservableObject
{
    #region Properties

    public virtual bool IsBusy => IsScrobbling;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBusy))]
    private bool _isScrobbling;

    protected readonly IPlugin _plugin = plugin;
    protected readonly IPluginManager _pluginManager = pluginManager;
    protected readonly IDialogService _dialogService = dialogService;
    private readonly IUserFeedbackService _userFeedbackService = userFeedbackService;

    #endregion Properties

    protected async Task ScrobbleToPlugins(IEnumerable<ScrobbleData> scrobbles)
    {
        IsScrobbling = true;

        try
        {
            var results = new List<(IAccountPlugin plugin, ScrobbleResponse response)>();
            foreach (var account in _pluginManager.InstalledPlugins.OfType<IAccountPlugin>().Where(p => p.IsScrobblingEnabled))
            {
                results.Add((account, await account.ScrobbleAsync(scrobbles)));
            }

            ShowScrobbleErrors(results);
        }
        finally
        {
            IsScrobbling = false;
        }
    }

    private void ShowScrobbleErrors(IEnumerable<(IAccountPlugin plugin, ScrobbleResponse response)> scrobbleResults)
    {
        var failedScrobbles = scrobbleResults.Where(r => !r.response.Success).ToList();
        if (failedScrobbles.Count != 0)
        {
            var message = "Some scrobbles failed:\n" + string.Join("\n", failedScrobbles.Select(r => $"{r.plugin.AccountId}: {r.response.ErrorMessage}"));
            _userFeedbackService.ShowError(message);
        }
        else
            _userFeedbackService.ShowSuccess("Successfully scrobbled to all accounts");
    }
}
