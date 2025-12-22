using System;
using System.Collections.Generic;
using System.Text;
using MediaPlayerScrobblerBase;

namespace Scrubbler.Test.AppleMusicScrobblerTest;

internal sealed class ManualTickSource : ITickSource
{
    public event EventHandler? Tick;

    public void Start() { }
    public void Stop() { }
    public void Dispose() { }

    public void Fire()
        => Tick?.Invoke(this, EventArgs.Empty);
}

