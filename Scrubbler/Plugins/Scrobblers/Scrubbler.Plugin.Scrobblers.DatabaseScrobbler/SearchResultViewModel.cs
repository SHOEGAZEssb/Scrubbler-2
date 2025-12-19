using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Scrubbler.Plugin.Scrobblers.DatabaseScrobbler;

internal abstract partial class SearchResultViewModel(Uri image, string name) : ObservableObject
{
    #region Properties

    public Uri Image { get; } = image;
    public string Name { get; } = name;

    public event EventHandler<SearchResultViewModel>? OnClicked;

    #endregion Properties

    [RelayCommand]
    private void Clicked()
    {
        OnClicked?.Invoke(this, this);
    }
}

internal sealed class ArtistResultViewModel(Uri artistImage, string artistName) : SearchResultViewModel(artistImage, artistName)
{
}

internal sealed class AlbumResultViewModel(Uri albumImage, string albumName, string artistName) : SearchResultViewModel(albumImage, albumName)
{
    #region Properties

    public string ArtistName { get; } = artistName;

    #endregion Properties
}
