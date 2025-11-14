namespace Scrubbler.Host.Presentation;

public sealed partial class Shell : UserControl, IContentControlProvider
{
    public Shell()
    {
        InitializeComponent();
    }
    public ContentControl ContentControl => Splash;
}
