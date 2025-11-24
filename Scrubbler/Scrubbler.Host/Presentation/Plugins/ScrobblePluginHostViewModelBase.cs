using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Plugin.Account;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation.Plugins;

internal abstract class ScrobblePluginHostViewModelBase : ObservableObject
{
    #region Properties

    protected readonly IPlugin _plugin;
    protected readonly IPluginManager _pluginManager;
    protected readonly IUserFeedbackService _userFeedbackService;
    protected readonly IDialogService _dialogService;

    #endregion Properties

    public ScrobblePluginHostViewModelBase(IPlugin plugin, IPluginManager pluginManager, IUserFeedbackService userFeedbackService, IDialogService dialogService)
    {
        _plugin = plugin;
        _pluginManager = pluginManager;
        _userFeedbackService = userFeedbackService;
        _dialogService = dialogService;
    }

    protected async Task ScrobbleToPlugins(IEnumerable<ScrobbleData> scrobbles)
    {
        foreach (var account in _pluginManager.InstalledPlugins.OfType<IAccountPlugin>().Where(p => p.IsScrobblingEnabled))
        {
            await account.ScrobbleAsync(scrobbles);
        }
    }
}
