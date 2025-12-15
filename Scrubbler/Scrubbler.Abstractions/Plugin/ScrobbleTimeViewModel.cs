using CommunityToolkit.Mvvm.ComponentModel;

namespace Scrubbler.Abstractions.Plugin;

public partial class ScrobbleTimeViewModel : ObservableObject
{
    #region Properties

    public DateTimeOffset Date
    {
        get => UseCurrentTime ? DateTime.Now.Date : _date;
        set
        {
            if (Date != value)
            {
                _date = value;
                OnPropertyChanged();
            }
        }
    }
    private DateTimeOffset _date;

    /// <summary>
    /// The selected time.
    /// </summary>
    public TimeSpan Time
    {
        get => UseCurrentTime ? DateTime.Now.TimeOfDay : _time;
        set
        {
            if (Time != value)
            {
                _time = value;
                OnPropertyChanged();
            }
        }
    }
    private TimeSpan _time;

    /// <summary>
    /// If <see cref="DateTime.Now"/> should be used
    /// for <see cref="Time"/>.
    /// </summary>
    public bool UseCurrentTime
    {
        get { return _useCurrentTime; }
        set
        {
            if (UseCurrentTime != value)
            {
                if (!value)
                    Time = DateTime.Now.TimeOfDay;

                _useCurrentTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Time));
            }
        }
    }
    private bool _useCurrentTime;

    public DateTimeOffset Timestamp => Date + Time;

    /// <summary>
    /// Gets if the selected <see cref="Time"/>
    /// is valid for a last.fm scrobble.
    /// </summary>
    /// <returns>True if time is "newer" than <see cref="MainViewModel.MinimumDateTime"/>,
    /// otherwise false.</returns>
    public bool IsTimeValid => Timestamp >= DateTime.Now.Subtract(TimeSpan.FromDays(14)) && Timestamp < DateTime.Now.AddDays(1);

    #endregion Properties

    #region Construction

    /// <summary>
    /// Constructor.
    /// </summary>
    public ScrobbleTimeViewModel()
    {
        UseCurrentTime = true;
        _ = UpdateCurrentTimeAsync();
    }

    #endregion Construction

    /// <summary>
    /// Task for notifying the UI that
    /// the <see cref="Time"/> has changed.
    /// </summary>
    /// <returns>Task.</returns>
    private async Task UpdateCurrentTimeAsync()
    {
        while (true)
        {
            if (UseCurrentTime)
            {
                OnPropertyChanged(nameof(Time));
                OnPropertyChanged(nameof(Date));
            }

            OnPropertyChanged(nameof(IsTimeValid));
            await Task.Delay(1000);
        }
    }
}
