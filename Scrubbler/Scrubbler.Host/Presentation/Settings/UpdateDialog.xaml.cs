using Scrubbler.Host.Updates;

namespace Scrubbler.Host.Presentation.Settings;

public sealed partial class UpdateDialog : ContentDialog
{
    public UpdateDialogViewModel ViewModel { get; }

    public UpdateDialog(UpdateInfo info)
    {
        this.InitializeComponent();
        ViewModel = new UpdateDialogViewModel(info);
    }
}
