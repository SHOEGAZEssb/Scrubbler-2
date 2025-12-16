using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Services;

namespace Scrubbler.Plugin.Scrobbler.Mock;

[PluginMetadata(
    Name = "Test Plugin",
    Description = "Just for testing purposes",
    SupportedPlatforms = PlatformSupport.All)]
public class MockScrobblePlugin : PluginBase
{
    public MockScrobblePlugin(IModuleLogServiceFactory logFactory)
        : base(logFactory)
    {
    }

    public override IPluginViewModel GetViewModel()
    {
        throw new NotImplementedException();
    }
}

