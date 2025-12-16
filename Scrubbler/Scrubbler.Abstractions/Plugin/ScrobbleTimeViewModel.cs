using CommunityToolkit.Mvvm.ComponentModel;

namespace Scrubbler.Abstractions.Plugin;

public partial class ScrobbleTimeViewModel : ObservableObject, IDisposable
{
    private readonly TimeProvider _timeProvider;
    private readonly CancellationTokenSource _cts = new();

    private DateTimeOffset _date;
    private TimeSpan _time;
    private bool _useCurrentTime;

    public DateTimeOffset Date
    {
        get => UseCurrentTime ? _timeProvider.GetLocalNow().Date : _date;
        set
        {
            if (Date != value)
            {
                _date = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Timestamp));
            }
        }
    }

    public TimeSpan Time
    {
        get => UseCurrentTime ? _timeProvider.GetLocalNow().TimeOfDay : _time;
        set
        {
            if (Time != value)
            {
                _time = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Timestamp));
            }
        }
    }

    public bool UseCurrentTime
    {
        get => _useCurrentTime;
        set
        {
            if (_useCurrentTime != value)
            {
                if (!value)
                    _time = _timeProvider.GetLocalNow().TimeOfDay;

                _useCurrentTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Time));
                OnPropertyChanged(nameof(Date));
                OnPropertyChanged(nameof(Timestamp));
            }
        }
    }

    public DateTimeOffset Timestamp => Date + Time;

    public bool IsTimeValid
    {
        get
        {
            var now = _timeProvider.GetLocalNow();
            return Timestamp >= now.AddDays(-14) && Timestamp < now.AddDays(1);
        }
    }

    public ScrobbleTimeViewModel(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
        UseCurrentTime = true;

        _ = UpdateLoopAsync(_cts.Token);
    }

    private async Task UpdateLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                if (UseCurrentTime)
                {
                    OnPropertyChanged(nameof(Time));
                    OnPropertyChanged(nameof(Date));
                    OnPropertyChanged(nameof(Timestamp));
                }

                OnPropertyChanged(nameof(IsTimeValid));
                await Task.Delay(1000, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // expected
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
