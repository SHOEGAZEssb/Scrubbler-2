using System;
using System.Collections.Generic;
using System.Text;
using DiscordRPC;
using Scrubbler.PluginBase.Discord;

namespace Scrubbler.Host.Services;

internal class DiscordRichPresence : IDiscordRichPresence
{
    #region Properties

    private readonly DiscordRpcClient _client;

    #endregion Properties

    #region Construction

    public DiscordRichPresence()
    {
        _client = new DiscordRpcClient("1473046974709239828");
    }

    #endregion Construction

    public void Clear()
    {
        _client.ClearPresence();
    }

    public void Publish(NowPlayingPresence presence)
    {
        _client.SetPresence(new RichPresence()
        {
            Details = presence.Details,
            State = presence.State,
            Assets = new Assets()
            {
                LargeImageKey = presence.LargeImageKey,
                LargeImageText = presence.LargeImageText,
                SmallImageKey = presence.SmallImageKey,
                SmallImageText = presence.SmallImageText
            }
        });
    }
}
