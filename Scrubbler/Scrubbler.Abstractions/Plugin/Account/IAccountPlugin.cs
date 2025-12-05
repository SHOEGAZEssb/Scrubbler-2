namespace Scrubbler.Abstractions.Plugin.Account;

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

    /// <summary>
    /// Gets or sets whether scrobbling to this account is currently enabled.
    /// </summary>
    bool IsScrobblingEnabled { get; set; }

    /// <summary>
    /// Event that is fired when <see cref="IsScrobblingEnabled"/> changes.
    /// </summary>
    event EventHandler? IsScrobblingEnabledChanged;

    /// <summary>
    /// Initiates an authentication flow (OAuth, API key, etc.).
    /// May prompt the user for credentials or open a web view.
    /// </summary>
    /// <returns>A task that represents the asynchronous authentication operation.</returns>
    /// <exception cref="Exception">Thrown when authentication fails.</exception>
    Task AuthenticateAsync();

    /// <summary>
    /// Logs out the account and clears authentication state.
    /// </summary>
    /// <returns>A task that represents the asynchronous logout operation.</returns>
    Task LogoutAsync();

    /// <summary>
    /// Submits the provided scrobbles to the connected account.
    /// </summary>
    /// <param name="scrobbles">The collection of tracks to scrobble.</param>
    /// <returns>A task that represents the asynchronous scrobble operation,
    /// containing the scrobble response.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the account is not authenticated or scrobbling is disabled.</exception>
    Task<ScrobbleResponse> ScrobbleAsync(IEnumerable<ScrobbleData> scrobbles);
}
