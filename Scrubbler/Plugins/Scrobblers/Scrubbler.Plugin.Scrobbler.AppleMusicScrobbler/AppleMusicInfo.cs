namespace Scrubbler.Plugin.Scrobbler.AppleMusicScrobbler;

internal class AppleMusicInfo(string songName, string songAlbumArtist, string songAlbum, string songArtist) : IEquatable<AppleMusicInfo>
{
    public string SongName = songName;
    public string SongAlbumArtist = songAlbumArtist;
    public string SongAlbum = songAlbum;
    public string SongArtist = songArtist;
    public int SongDuration;

    public bool Equals(AppleMusicInfo? other)
    {
        return other != null && other.SongName == SongName && other.SongArtist == SongArtist && other.SongAlbumArtist == SongAlbumArtist && other.SongDuration == SongDuration;
    }

    public override bool Equals(object? obj) => Equals(obj as AppleMusicInfo);

    public static bool operator ==(AppleMusicInfo? a1, AppleMusicInfo? a2)
    {
        if (a1 is null && a2 is null)
        {
            return true;
        }
        else if (a1 is null || a2 is null)
        {
            return false;
        }
        else
        {
            return a1.Equals(a2);
        }
    }

    public static bool operator !=(AppleMusicInfo? a1, AppleMusicInfo? a2)
    {
        return !(a1 == a2);
    }

    public override int GetHashCode()
    {
        return SongName.GetHashCode() ^ SongArtist.GetHashCode() ^ SongAlbumArtist.GetHashCode();
    }
}
