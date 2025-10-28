using Scrubbler.Abstractions;
using System.Collections.ObjectModel;

namespace Scrubbler.Host.Presentation.Plugins;

public sealed partial class ScrobblePreviewViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ScrobbleData> _scrobbles;

    public ScrobblePreviewViewModel(IEnumerable<ScrobbleData> scrobbles)
    {
        _scrobbles = new ObservableCollection<ScrobbleData>(scrobbles);
    }
}
