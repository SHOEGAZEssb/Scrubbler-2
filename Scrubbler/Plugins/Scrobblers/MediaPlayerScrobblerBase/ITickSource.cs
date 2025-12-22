namespace MediaPlayerScrobblerBase;

public interface ITickSource : IDisposable
{
    event EventHandler Tick;

    void Start();
    void Stop();
}
