namespace Scrubbler.Abstractions.Plugin.Account;

public class AccountFunctionContainer
{
    public ICanFetchPlayCounts? FetchPlayCountsObject { get; }

    public ICanLoveTracks? LoveTrackObject { get; }

    public ICanFetchTags? FetchTagsObject { get; }

    public ICanUpdateNowPlaying? UpdateNowPlayingObject { get; }

    public ICanOpenLinks? OpenLinksObject { get; }

    public AccountFunctionContainer(IAccountPlugin? plugin)
    {
        if (plugin is ICanFetchPlayCounts fetchPlayCounts)
            FetchPlayCountsObject = fetchPlayCounts;
        if (plugin is ICanLoveTracks loveTracks)
            LoveTrackObject = loveTracks;
        if (plugin is ICanFetchTags fetchTags)
            FetchTagsObject = fetchTags;
        if (plugin is ICanUpdateNowPlaying updateNowPlaying)
            UpdateNowPlayingObject = updateNowPlaying;
        if (plugin is ICanOpenLinks openLinks)
            OpenLinksObject = openLinks;
    }
}
