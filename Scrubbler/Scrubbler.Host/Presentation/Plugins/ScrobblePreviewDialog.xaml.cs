using Scrubbler.Abstractions;

namespace Scrubbler.Host.Presentation.Plugins;

public sealed partial class ScrobblePreviewDialog : ContentDialog
{
    public ScrobblePreviewViewModel ViewModel { get; }

    public ScrobblePreviewDialog(IEnumerable<ScrobbleData> scrobbles)
    {
        this.InitializeComponent();
        ViewModel = new ScrobblePreviewViewModel(scrobbles);
        DataContext = ViewModel;
    }
}
