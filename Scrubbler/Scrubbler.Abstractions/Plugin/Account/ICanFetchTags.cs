namespace Scrubbler.Abstractions.Plugin.Account;

public interface ICanFetchTags
{
    Task<(string? errorMessage, IEnumerable<string> tags)> GetArtistTags(string artistName);

    Task<(string? errorMessage, IEnumerable<string> tags)> GetTrackTags(string artistName, string trackName);

    Task<(string? errorMessage, IEnumerable<string> tags)> GetAlbumTags(string artistName, string albumName);
}
