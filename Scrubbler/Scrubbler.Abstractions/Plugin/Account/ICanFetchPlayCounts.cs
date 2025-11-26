namespace Scrubbler.Abstractions.Plugin.Account;

public interface ICanFetchPlayCounts
{
    Task<(string? errorMessage, int playCount)> GetArtistPlayCount(string artistName);

    Task<(string? errorMessage, int playCount)> GetTrackPlayCount(string artistName, string trackName);

    Task<(string? errorMessage, int playCount)> GetAlbumPlayCount(string artistName, string albumName);
}
