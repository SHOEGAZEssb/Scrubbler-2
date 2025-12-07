using Scrubbler.Abstractions.Plugin;
using Scrubbler.Host.Helper;
using Scrubbler.Host.Presentation.Plugins;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation.Navigation;

internal sealed class PluginNavigationItemViewModel(IPlugin plugin, IPluginManager manager, IUserFeedbackService feedbackService, IDialogService dialogService)
    : NavigationItemViewModelBase(plugin.Name, MakeHostViewModel(plugin, manager, feedbackService, dialogService))
{
    public ImageSource? Icon => _icon ??= PluginIconHelper.LoadPluginIcon(plugin);
    private ImageSource? _icon;


    private static ScrobblePluginHostViewModelBase MakeHostViewModel(IPlugin plugin, IPluginManager manager, IUserFeedbackService feedbackService, IDialogService dialogService)
    {
        if (plugin is IScrobblePlugin sp)
            return new ScrobblePluginHostViewModel(sp, manager, feedbackService, dialogService);
        if (plugin is IAutoScrobblePlugin asp)
            return new AutoScrobblePluginHostViewModel(asp, manager, feedbackService, dialogService);

        throw new NotSupportedException($"Plugin type {plugin.GetType().FullName} is not supported in the navigation.");
    }
}
