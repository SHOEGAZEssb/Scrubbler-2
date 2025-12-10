namespace Scrubbler.Abstractions.Plugin.Account;

public interface ICanUpdateNowPlaying : IHaveAccountFunctions
{
    Task<string?> UpdateNowPlaying(string artistName, string trackName, string? albumName);
}
