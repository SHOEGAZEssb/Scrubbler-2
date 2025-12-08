using System;
using System.Collections.Generic;
using System.Text;

namespace Scrubbler.Abstractions.Plugin.Account;

public interface ICanOpenLinks
{
    void OpenArtistLink(string artistName);
    void OpenAlbumLink(string albumName, string artistName);
    void OpenTrackLink(string trackName, string artistName, string? albumName);
    void OpenTagLink(string tagName);
}
