using Scrubbler.Abstractions;
using System.Collections.ObjectModel;

namespace Scrubbler.Host.Presentation.Plugins;

public sealed partial class ScrobblePreviewViewModel(IEnumerable<ScrobbleData> scrobbles) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ScrobbleData> _scrobbles = new(scrobbles);
}
