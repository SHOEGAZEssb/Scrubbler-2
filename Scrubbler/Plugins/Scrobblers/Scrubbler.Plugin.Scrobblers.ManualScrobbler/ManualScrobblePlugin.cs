using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Services;

namespace Scrubbler.Plugin.Scrobblers.ManualScrobbler;

/// <summary>
/// Plugin that allows users to manually enter track details and scrobble them.
/// </summary>
/// <seealso cref="IScrobblePlugin"/>
[PluginMetadata(
    Name = "Manual Scrobbler",
    Description = "Enter track details manually and scrobble them",
    SupportedPlatforms = PlatformSupport.All)]
public class ManualScrobblePlugin(IModuleLogServiceFactory logFactory) : Abstractions.Plugin.PluginBase(logFactory), IScrobblePlugin
{
    #region Properties

    private readonly ManualScrobbleViewModel _vm = new();

    #endregion Properties

    /// <summary>
    /// Gets the view model instance for this plugin's UI.
    /// </summary>
    /// <returns>The <see cref="IPluginViewModel"/> instance for this plugin.</returns>
    public override IPluginViewModel GetViewModel()
    {
        return _vm;
    }
}
