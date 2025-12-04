using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Scrubbler.Host.Services.Logging;

namespace Scrubbler.Host.Presentation.Logging;

internal partial class LogViewModel : ObservableObject, IHostedService
{
    #region Properties

    public ObservableCollection<LogMessage> Entries { get; } = [];

    public ObservableCollection<LogMessage> FilteredEntries { get; } = [];

    public ObservableCollection<LogLevelFilterViewModel> LogLevelFilters { get; } = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    #endregion Properties

    #region Construction

    public LogViewModel(HostLogService hostLogService)
    {
        hostLogService.MessageLogged += Add;

        foreach (LogLevel level in Enum.GetValues<LogLevel>())
        {
            var filterVm = new LogLevelFilterViewModel(level);
            filterVm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(LogLevelFilterViewModel.IsEnabled))
                {
                    RebuildFilteredEntries();
                }
            };
            LogLevelFilters.Add(filterVm);
        }
    }

    #endregion Construction

    [RelayCommand]
    private void Clear()
    {
        Entries.Clear();
    }

    private void RebuildFilteredEntries()
    {
        FilteredEntries.Clear();
        foreach (var entry in Entries)
        {
            if (MatchesFilter(entry))
                FilteredEntries.Add(entry);
        }
    }

    private bool MatchesFilter(LogMessage entry)
    {
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            if (!entry.Message.Contains(SearchText, StringComparison.OrdinalIgnoreCase) &&
                (entry.Exception == null || !entry.Exception.Message.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        foreach (var filter in LogLevelFilters)
        {
            if (!filter.PassesFilter(entry))
                return false;
        }

        return true;
    }


    public Task StartAsync(CancellationToken ct) => Task.CompletedTask;
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

    private void Add(LogMessage entry)
    {
        Entries.Add(entry);

        if (MatchesFilter(entry))
            FilteredEntries.Add(entry);
    }

    partial void OnSearchTextChanged(string value)
    {
        RebuildFilteredEntries();
    }
}
