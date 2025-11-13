using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.Input;

namespace Scrubbler.Abstractions.Plugin;

public abstract partial class ScrobbleMultipleViewModelBase<T> : ScrobblePluginViewModelBase where T : IScrobbableObjectViewModel
{
    #region Properties

    /// <summary>
    /// Gets if the scrobble button on the ui is enabled.
    /// </summary>
    public override bool CanScrobble => Scrobbles.Any(i => i.ToScrobble);

    /// <summary>
    /// The collection of scrobbles.
    /// </summary>
    public ObservableCollection<T> Scrobbles
    {
        get { return _scrobbles; }
        protected set
        {
            if (Scrobbles != null)
            {
                Scrobbles.CollectionChanged -= Scrobbles_CollectionChanged;
                DisconnectExistingToScrobbleEvent();
            }

            _scrobbles = value;
            if (Scrobbles != null)
                _scrobbles.CollectionChanged += Scrobbles_CollectionChanged;
            ConnectExistingToScrobbleEvent();

            OnPropertyChanged();
            NotifyProperties();
        }
    }
    private ObservableCollection<T> _scrobbles = [];

    /// <summary>
    /// Gets if all scrobbles can currently be selected.
    /// </summary>
    public bool CanCheckAll => Scrobbles.Any(s => s.CanBeScrobbled && !s.ToScrobble);

    /// <summary>
    /// Gets if all scrobbles can currently be unchecked.
    /// </summary>
    public bool CanUncheckAll => Scrobbles.Any(s => s.ToScrobble);

    /// <summary>
    /// Gets if selected scrobbles can be checked.
    /// </summary>
    public bool CanCheckSelected => Scrobbles.Any(s => s.IsSelected && !s.ToScrobble);

    /// <summary>
    /// Gets if selected scrobbles can be unchecked.
    /// </summary>
    public bool CanUncheckSelected => Scrobbles.Any(s => s.IsSelected && s.ToScrobble);

    /// <summary>
    /// Gets the amount of scrobbles that are
    /// marked as "ToScrobble".
    /// </summary>
    public virtual int ToScrobbleCount => Scrobbles.Where(s => s.ToScrobble).Count();

    /// <summary>
    /// Max amount of scrobbable scrobbles.
    /// </summary>
    public virtual int MaxToScrobbleCount => Scrobbles.Count;

    /// <summary>
    /// Gets the amount of selected scrobbles.
    /// </summary>
    public int SelectedCount => Scrobbles.Where(s => s.IsSelected).Count();

    #endregion Properties

    /// <summary>
    /// Marks all scrobbles as "ToScrobble".
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCheckAll))]
    protected virtual void CheckAll()
    {
        SetToScrobbleState(Scrobbles, true);
    }

    /// <summary>
    /// Marks all scrobbles as not "ToScrobble".
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanUncheckAll))]
    protected virtual void UncheckAll()
    {
        SetToScrobbleState(Scrobbles, false);
    }

    /// <summary>
    /// Marks all selected scrobbles as "ToScrobble".
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCheckSelected))]
    protected virtual void CheckSelected()
    {
        SetToScrobbleState(Scrobbles.Where(s => s.IsSelected), true);
    }

    /// <summary>
    /// Marks all selected scrobbles as not "ToScrobble".
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanUncheckSelected))]
    protected virtual void UncheckSelected()
    {
        SetToScrobbleState(Scrobbles.Where(s => s.IsSelected), false);
    }

    /// <summary>
    /// Notifies that properties have changed.
    /// </summary>
    protected void NotifyProperties()
    {
        CheckAllCommand.NotifyCanExecuteChanged();
        UncheckAllCommand.NotifyCanExecuteChanged();
        CheckSelectedCommand.NotifyCanExecuteChanged();
        UncheckSelectedCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanScrobble));
        OnPropertyChanged(nameof(CanCheckAll));
        OnPropertyChanged(nameof(CanUncheckAll));
        OnPropertyChanged(nameof(CanCheckSelected));
        OnPropertyChanged(nameof(CanUncheckSelected));
        OnPropertyChanged(nameof(ToScrobbleCount));
        OnPropertyChanged(nameof(MaxToScrobbleCount));
        OnPropertyChanged(nameof(SelectedCount));
    }

    /// <summary>
    /// Sets the "ToScrobble" state of the given <paramref name="toSet"/>.
    /// </summary>
    /// <param name="toSet">Items whose "ToScrobble" state to set.</param>
    /// <param name="state">State to set.</param>
    protected void SetToScrobbleState(IEnumerable<T> toSet, bool state)
    {
        foreach (var item in toSet.Take(toSet.Count() - 1))
            item.UpdateToScrobbleSilent(state);

        // set last one manually to trigger the event
        toSet.Last().ToScrobble = state;
        Scrobbles = new ObservableCollection<T>(Scrobbles); // refreshes the view
    }

    /// <summary>
    /// Connects events when <see cref="Scrobbles"/> change.
    /// </summary>
    /// <param name="sender">Ignored.</param>
    /// <param name="e">EventArgs.</param>
    private void Scrobbles_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            foreach (T scrobble in e.NewItems)
            {
                scrobble.ToScrobbleChanged += Scrobble_StateChanged;
                scrobble.IsSelectedChanged += Scrobble_StateChanged;
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
        {
            foreach (T scrobble in e.OldItems)
            {
                scrobble.ToScrobbleChanged -= Scrobble_StateChanged;
                scrobble.IsSelectedChanged -= Scrobble_StateChanged;
            }
        }

        NotifyProperties();
    }

    /// <summary>
    /// Notifies that properties have changed.
    /// </summary>
    /// <param name="sender">Ignored.</param>
    /// <param name="e">Ignored.</param>
    private void Scrobble_StateChanged(object? sender, EventArgs e)
    {
        NotifyProperties();
    }

    /// <summary>
    /// Connects the ToScrobbleChanged event
    /// for all scrobbles.
    /// </summary>
    private void ConnectExistingToScrobbleEvent()
    {
        foreach (T scrobble in Scrobbles)
        {
            scrobble.ToScrobbleChanged += Scrobble_StateChanged;
            scrobble.IsSelectedChanged += Scrobble_StateChanged;
        }
    }

    /// <summary>
    /// Disconnects the ToScrobbleChanged event
    /// for all scrobbles.
    /// </summary>
    private void DisconnectExistingToScrobbleEvent()
    {
        foreach (T scrobble in Scrobbles)
        {
            scrobble.ToScrobbleChanged -= Scrobble_StateChanged;
            scrobble.IsSelectedChanged -= Scrobble_StateChanged;
        }
    }
}
