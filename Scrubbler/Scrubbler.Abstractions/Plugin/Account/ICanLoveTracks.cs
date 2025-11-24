namespace Scrubbler.Abstractions.Plugin.Account;

public interface ICanLoveTracks : IAccountFunction
{
    Task<string?> SetLoveState(string artistName, string trackName, string? albumName, bool isLoved);

    Task<string?> GetLoveState(string artistName, string trackName, string? albumName, out bool isLoved);
}
