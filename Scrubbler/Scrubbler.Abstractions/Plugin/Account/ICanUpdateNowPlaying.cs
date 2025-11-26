namespace Scrubbler.Abstractions.Plugin.Account;

public interface ICanUpdateNowPlaying
{
    Task<string?> UpdateNowPlaying(string artistName, string trackName, string? albumName);
}
