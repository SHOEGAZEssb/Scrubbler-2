namespace Scrubbler.Abstractions.Plugin.Account;

public interface IHaveScrobbleLimit
{
    event EventHandler? CurrentScrobbleCountChanged;

    int ScrobbleLimit { get; }

    int CurrentScrobbleCount { get; }

    bool HasReachedScrobbleLimit { get; }

    Task UpdateScrobbleCount();
}
