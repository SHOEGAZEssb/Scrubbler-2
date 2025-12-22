using SystemTimer = System.Timers.Timer;

namespace MediaPlayerScrobblerBase;

public sealed class TimerTickSource : ITickSource
{
    private readonly SystemTimer _timer;

    public event EventHandler? Tick;

    public TimerTickSource(int intervalMs)
    {
        _timer = new SystemTimer(intervalMs);
        _timer.Elapsed += (_, _) => Tick?.Invoke(this, EventArgs.Empty);
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();
    public void Dispose() => _timer.Dispose();
}
