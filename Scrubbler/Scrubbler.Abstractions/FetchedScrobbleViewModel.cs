using CommunityToolkit.Mvvm.ComponentModel;

namespace Scrubbler.Abstractions;

public partial class FetchedScrobbleViewModel : ObservableObject, IScrobbableObjectViewModel
{
    #region Properties

    [ObservableProperty]
    private bool _toScrobble;

    [ObservableProperty]
    private bool _isSelected;

    public bool CanBeScrobbled => Timestamp > DateTime.Now.Subtract(TimeSpan.FromDays(14));

    public event EventHandler? ToScrobbleChanged;
    public event EventHandler? IsSelectedChanged;

    /// <summary>
    /// Name of the artist.
    /// </summary>
    public string ArtistName
    {
        get => _scrobble.Artist;
        set
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                return;

            _scrobble.Artist = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Name of the track.
    /// </summary>
    public string TrackName
    {
        get => _scrobble.Track;
        set
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                return;

            _scrobble.Track = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Name of the album.
    /// </summary>
    public string? AlbumName
    {
        get => _scrobble.Album;
        set
        {
            _scrobble.Album = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Name of the album artist.
    /// </summary>
    public string? AlbumArtistName
    {
        get => _scrobble.AlbumArtist;
        set
        {
            _scrobble.AlbumArtist = value;
            OnPropertyChanged();
        }
    }

    public DateTimeOffset Timestamp => _scrobble.Timestamp;

    private readonly ScrobbleData _scrobble;

    #endregion Properties

    #region Construction

    public FetchedScrobbleViewModel(ScrobbleData scrobble)
    {
        _scrobble = scrobble;
    }

    #endregion Construction

    public void UpdateIsSelectedSilent(bool isSelected)
    {
#pragma warning disable MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
        _isSelected = isSelected;
#pragma warning restore MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
    }

    public void UpdateToScrobbleSilent(bool toScrobble)
    {
#pragma warning disable MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
        _toScrobble = toScrobble;
#pragma warning restore MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
    }

    partial void OnToScrobbleChanged(bool value)
    {
        ToScrobbleChanged?.Invoke(this, EventArgs.Empty);
        OnPropertyChanged(nameof(CanBeScrobbled));
    }

    partial void OnIsSelectedChanged(bool value)
    {
        IsSelectedChanged?.Invoke(this, EventArgs.Empty);
        OnPropertyChanged(nameof(CanBeScrobbled));
    }
}
