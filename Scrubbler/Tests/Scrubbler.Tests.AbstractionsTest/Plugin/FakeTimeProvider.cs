namespace Scrubbler.Tests.AbstractionsTest.Plugin;

internal sealed class FakeTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    private DateTimeOffset _utcNow = utcNow.ToUniversalTime();

    public void SetUtcNow(DateTimeOffset utcNow)
    {
        _utcNow = utcNow.ToUniversalTime();
    }

    public void Advance(TimeSpan delta)
    {
        _utcNow = _utcNow.Add(delta);
    }

    public override DateTimeOffset GetUtcNow() => _utcNow;
}


