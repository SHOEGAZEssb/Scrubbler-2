using Microsoft.Extensions.DependencyInjection;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        var app = (App)Application.Current;
        app.Ready += App_Ready;
    }

    private void App_Ready(object? sender, EventArgs e)
    {
        var app = (App)Application.Current;
        var feedback = app.Host?.Services.GetRequiredService<IUserFeedbackService>();
        if (feedback is UserFeedbackService impl)
            impl.AttachInfoBar(GlobalInfoBar);
    }
}
