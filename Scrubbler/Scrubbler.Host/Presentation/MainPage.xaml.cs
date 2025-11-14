using Microsoft.Extensions.DependencyInjection;
using Scrubbler.Host.Services;

namespace Scrubbler.Host.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
        var app = (App)Application.Current;
        app.Ready += App_Ready;
    }

    private void App_Ready(object? sender, EventArgs e)
    {
        var app = (App)Application.Current;
        var feedback = app.Host?.Services.GetRequiredService<IUserFeedbackService>();
        if (feedback is UserFeedbackService impl)
            impl.AttachInfoBar(GlobalInfoBar);

        var dialogService = app.Host?.Services.GetRequiredService<IDialogService>();
        if (dialogService is DialogService dImpl)
            dImpl.InitializeXamlRoot(this);
    }
}
