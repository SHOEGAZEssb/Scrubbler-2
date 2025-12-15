using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Scrubbler.Plugin.Scrobblers.DatabaseScrobbler;

internal abstract partial class SearchResultViewModel : ObservableObject
{
    public Uri Image { get; }
    public string Name { get; }

    public event EventHandler<SearchResultViewModel>? OnClicked;

    protected SearchResultViewModel(Uri image, string name)
    {
        Image = image;
        Name = name;
    }

    [RelayCommand]
    private void Clicked()
    {
        OnClicked?.Invoke(this, this);
    }
}

internal sealed class ArtistResultViewModel : SearchResultViewModel
{
    public ArtistResultViewModel(Uri artistImage, string artistName)
        : base(artistImage, artistName) { }
}

internal sealed class AlbumResultViewModel : SearchResultViewModel
{
    public string ArtistName { get; }

    public AlbumResultViewModel(Uri albumImage, string albumName, string artistName)
        : base(albumImage, albumName)
    {
        ArtistName = artistName;
    }
}

