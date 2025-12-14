using System;
using System.Collections.Generic;
using System.Text;

namespace Scrubbler.Abstractions.Plugin;

public abstract class ScrobbleMultipleTimeViewModelBase<T> : ScrobbleMultipleViewModelBase<T> where T : IScrobbableObjectViewModel
{
    #region Properties

    public ScrobbleTimeViewModel ScrobbleTimeVM { get; }

    public override bool CanScrobble => base.CanScrobble && ScrobbleTimeVM.IsTimeValid;

    #endregion Properties

    #region Construction

    protected ScrobbleMultipleTimeViewModelBase()
    {
        ScrobbleTimeVM = new ScrobbleTimeViewModel();
        ScrobbleTimeVM.PropertyChanged += ScrobbleTimeVM_PropertyChanged;
    }

    #endregion Construction

    private void ScrobbleTimeVM_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CanScrobble));
    }
}
