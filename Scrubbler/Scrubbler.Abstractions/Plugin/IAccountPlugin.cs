namespace Scrubbler.Abstractions.Plugin;

/// <summary>
/// Represents an account integration (e.g., Last.fm, Spotify).
/// Account plugins handle authentication and provide access to user identity.
/// </summary>
public interface IAccountPlugin : IPersistentPlugin
{
    /// <summary>
    /// Gets the unique identifier of the account (e.g., username, email).
    /// Null if the user has not authenticated yet.
    /// </summary>
    string? AccountId { get; }

    /// <summary>
    /// Indicates whether the user is currently authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    bool IsScrobblingEnabled { get; set; }

    /// <summary>
    /// Initiates an authentication flow (OAuth, API key, etc.).
    /// May prompt the user for credentials or open a web view.
    /// </summary>
    Task AuthenticateAsync();

    /// <summary>
    /// Logs out the account and clears authentication state.
    /// </summary>
    Task LogoutAsync();

    Task ScrobbleAsync(IEnumerable<ScrobbleData> scrobbles);
}
