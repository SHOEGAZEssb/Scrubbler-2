using CommunityToolkit.Mvvm.ComponentModel;

namespace Scrubbler.Abstractions;

public partial class ScrobbableObjectViewModel(string artistName, string trackName, string? albumName = null, string? albumArtistName = null) : ObservableObject, IScrobbableObjectViewModel
{
    #region Properties

    [ObservableProperty]
    private bool _toScrobble;

    [ObservableProperty]
    private bool _isSelected;

    public event EventHandler? ToScrobbleChanged;
    public event EventHandler? IsSelectedChanged;

    public virtual bool CanBeScrobbled => true;

    /// <summary>
    /// Name of the artist.
    /// </summary>
    public string ArtistName
    {
        get => _artistName;
        set
        {
            if (ArtistName != value)
            {
                if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                    return;

                _artistName = value;
                OnPropertyChanged();
            }
        }
    }
    private string _artistName = artistName;

    /// <summary>
    /// Name of the track.
    /// </summary>
    public string TrackName
    {
        get => _trackName;
        set
        {
            if (TrackName != value)
            {
                if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                    return;

                _trackName = value;
                OnPropertyChanged();
            }
        }
    }
    private string _trackName = trackName;

    /// <summary>
    /// Name of the album.
    /// </summary>
    public string? AlbumName
    {
        get => _albumName;
        set
        {
            if (AlbumName != value)
            {
                _albumName = value;
                OnPropertyChanged();
            }
        }
    }
    private string? _albumName = albumName;

    /// <summary>
    /// Name of the album artist.
    /// </summary>
    public string? AlbumArtistName
    {
        get => _albumArtistName;
        set
        {
            if (AlbumArtistName != value)
            {
                _albumArtistName = value;
                OnPropertyChanged();
            }
        }
    }
    private string? _albumArtistName = albumArtistName;

    #endregion Properties

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
    }

    partial void OnIsSelectedChanged(bool value)
    {
        IsSelectedChanged?.Invoke(this, EventArgs.Empty);
    }
}
