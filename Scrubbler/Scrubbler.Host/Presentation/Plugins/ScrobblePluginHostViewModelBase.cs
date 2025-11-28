using Scrubbler.Abstractions;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Plugin.Account;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation.Plugins;

internal abstract class ScrobblePluginHostViewModelBase(IPlugin plugin, IPluginManager pluginManager, IUserFeedbackService userFeedbackService, IDialogService dialogService) : ObservableObject
{
    #region Properties

    protected readonly IPlugin _plugin = plugin;
    protected readonly IPluginManager _pluginManager = pluginManager;
    protected readonly IUserFeedbackService _userFeedbackService = userFeedbackService;
    protected readonly IDialogService _dialogService = dialogService;

    #endregion Properties

    protected async Task ScrobbleToPlugins(IEnumerable<ScrobbleData> scrobbles)
    {
        foreach (var account in _pluginManager.InstalledPlugins.OfType<IAccountPlugin>().Where(p => p.IsScrobblingEnabled))
        {
            await account.ScrobbleAsync(scrobbles);
        }
    }
}
