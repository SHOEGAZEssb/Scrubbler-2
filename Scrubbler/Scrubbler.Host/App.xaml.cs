using Microsoft.Extensions.DependencyInjection;
using Scrubbler.Abstractions.Settings;
using Scrubbler.Host.Presentation.Accounts;
using Scrubbler.Host.Presentation.Logging;
using Scrubbler.Host.Presentation.Plugins;
using Scrubbler.Host.Services;
using Scrubbler.Host.Services.Logging;

namespace Scrubbler.Host;
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }
    public IHost? Host { get; private set; }

    public event EventHandler? Ready;

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        if (Environment.GetEnvironmentVariable("SCRUBBLER_PLUGIN_MODE") == "Debug")
        {
            var slnDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
            Environment.SetEnvironmentVariable("SOLUTIONDIR", slnDir);
        }

        var builder = this.CreateBuilder(args)
            // Add navigation support for toolkit controls such as TabBar and NavigationView
            .UseToolkitNavigation()
            .Configure(host => host
#if DEBUG
                // Switch to Development environment when running in DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseLogging(configure: (context, logBuilder) =>
                {
                    // Configure log levels for different categories of logging
                    logBuilder
                        .SetMinimumLevel(
                            context.HostingEnvironment.IsDevelopment() ?
                                LogLevel.Trace :
                                LogLevel.Warning)

                        // Default filters for core Uno Platform namespaces
                        .CoreLogLevel(LogLevel.Warning).XamlBindingLogLevel(LogLevel.Trace);

                }, enableUnoLogging: true)

                .UseSerialization()
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        .EmbeddedSource<App>()
                        .Section<AppConfig>()
                        .Section<UserConfig>()
                )

                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<HostLogService>();
                    services.AddHostedService<HostLogInitializer>();
                    services.AddSingleton<LogViewModel>();
                    services.AddHostedService<LogViewModelInitializer>();

                    services.AddSingleton<IUserFeedbackService, UserFeedbackService>();
                    services.AddSingleton<IDialogService, DialogService>();
                    services.AddSingleton<ISettingsStore, JsonSettingsStore>();
                    services.AddSingleton<IPluginManager, PluginManager>();
                    services.AddTransient<AccountsViewModel>();
                    services.AddTransient<PluginManagerViewModel>();
                })

                .UseNavigation(RegisterRoutes)
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        Host = await builder.NavigateAsync<Shell>();
        Ready?.Invoke(this, EventArgs.Empty);
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellViewModel)),
            new ViewMap<MainPage, MainViewModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellViewModel>(),
                Nested:
                [
                    new ("Main", View: views.FindByViewModel<MainViewModel>(), IsDefault:true),
                ]
            )
        );
    }
}
