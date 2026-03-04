using System.Collections.ObjectModel;
using Scrubbler.PluginBase;

namespace Scrubbler.Host.Presentation.Plugins;

public sealed partial class ScrobblePreviewViewModel(IEnumerable<ScrobbleData> scrobbles) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ScrobbleData> _scrobbles = new(scrobbles);
}
