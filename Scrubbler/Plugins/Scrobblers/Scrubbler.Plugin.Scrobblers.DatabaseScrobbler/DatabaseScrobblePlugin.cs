using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Services;

namespace Scrubbler.Plugin.Scrobblers.DatabaseScrobbler;

public class DatabaseScrobblePlugin(IModuleLogServiceFactory logFactory) : PluginBase(logFactory), IScrobblePlugin
{
    public override IPluginViewModel GetViewModel()
    {
        throw new NotImplementedException();
    }
}

