using Scrubbler.Abstractions.Plugin;
using Scrubbler.Plugin.Accounts.LastFm;

namespace Scrubbler.Host.Services;

public class PluginManager : IPluginManager
{
    private readonly List<IPlugin> _installed = new();

    public PluginManager()
    {
        // TODO: Replace with dynamic discovery later
        _installed.Add(new LastFmAccountPlugin());
        // _installed.Add(new ManualScrobblePlugin());
    }

    public IEnumerable<IPlugin> InstalledPlugins => _installed;

    public IEnumerable<IPluginMetadata> AvailablePlugins =>
        Enumerable.Empty<IPluginMetadata>(); // TODO: fetch from repo

    public Task InstallAsync(IPluginMetadata plugin)
    {
        // TODO: download & load plugin assembly
        throw new NotImplementedException();
    }

    public Task UninstallAsync(IPlugin plugin)
    {
        _installed.Remove(plugin);
        return Task.CompletedTask;
    }
}
