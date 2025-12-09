namespace Scrubbler.Abstractions.Plugin.Account;

public interface ICanOpenLinks
{
    Task OpenArtistLink(string artistName);
    Task OpenAlbumLink(string albumName, string artistName);
    Task OpenTrackLink(string trackName, string artistName, string? albumName);
    Task OpenTagLink(string tagName);
}
