namespace Scrubbler.Abstractions;

/// <summary>
/// Interface for an object that can be scrobbled.
/// </summary>
public interface IScrobbableObjectViewModel
{
    string TrackName { get; set; }

    string ArtistName { get; set; }

    string? AlbumName { get; set; }

    string? AlbumArtistName { get; set; }

    /// <summary>
    /// If true, this object should be
    /// scrobbled.
    /// </summary>
    bool ToScrobble { get; set; }

    /// <summary>
    /// If this object is selected in the UI.
    /// </summary>
    bool IsSelected { get; set; }

    /// <summary>
    /// Gets if this track can be scrobbled with the current configuration.
    /// </summary>
    bool CanBeScrobbled { get; }

    /// <summary>
    /// Event that triggers when <see cref="ToScrobble"/>
    /// changes.
    /// </summary>
    event EventHandler? ToScrobbleChanged;

    /// <summary>
    /// Event that triggers when <see cref="IsSelected"/>
    /// changes.
    /// </summary>
    event EventHandler? IsSelectedChanged;

    /// <summary>
    /// Updates the value of <see cref="ToScrobble"/>
    /// without notifying.
    /// </summary>
    /// <param name="toScrobble">ToScrobble value.</param>
    void UpdateToScrobbleSilent(bool toScrobble);

    /// <summary>
    /// Updates the value of <see cref="IsSelected"/>
    /// without notifying.
    /// </summary>
    /// <param name="isSelected">IsSelected value.</param>
    void UpdateIsSelectedSilent(bool isSelected);
}
