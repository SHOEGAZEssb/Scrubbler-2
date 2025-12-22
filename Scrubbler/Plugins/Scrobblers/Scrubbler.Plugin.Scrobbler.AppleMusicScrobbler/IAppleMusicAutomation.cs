namespace Scrubbler.Plugin.Scrobbler.AppleMusicScrobbler;

/// <summary>
/// Provides access to Apple Music playback information.
/// </summary>
internal interface IAppleMusicAutomation : IDisposable
{
    /// <summary>
    /// Attempts to connect to Apple Music.
    /// </summary>
    void Connect();

    /// <summary>
    /// Disconnects from Apple Music.
    /// </summary>
    void Disconnect();

    /// <summary>
    /// Gets the currently playing song, or null if nothing is playing.
    /// </summary>
    AppleMusicInfo? GetCurrentSong();

    /// <summary>
    /// Gets the current playback position in seconds.
    /// </summary>
    int GetCurrentPositionSeconds();
}
