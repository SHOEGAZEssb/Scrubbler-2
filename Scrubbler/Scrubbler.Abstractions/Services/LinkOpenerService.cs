using Windows.System;

namespace Scrubbler.Abstractions.Services;

public class LinkOpenerService : ILinkOpenerService
{
    public async Task OpenLink(string url)
    {
        await Launcher.LaunchUriAsync(new Uri(url));
    }
}
