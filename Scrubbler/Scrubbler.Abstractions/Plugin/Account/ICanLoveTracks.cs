namespace Scrubbler.Abstractions.Plugin.Account;

public interface ICanLoveTracks : IAccountFunction
{
    Task<string?> SetLoveState(string artistName, string trackName, string? albumName, bool isLoved);

    Task<(string? errorMessage, bool isLoved)> GetLoveState(string artistName, string trackName, string? albumName);
}
