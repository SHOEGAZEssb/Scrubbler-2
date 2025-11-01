namespace Scrubbler.Host.Presentation;

/// <summary>
/// View model for the shell/main navigation of the application.
/// </summary>
public class ShellViewModel
{
    private readonly INavigator _navigator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
    /// </summary>
    /// <param name="navigator">The navigation service for navigating between pages.</param>
    public ShellViewModel(
        INavigator navigator)
    {
        _navigator = navigator;
        // Add code here to initialize or attach event handlers to singleton services
    }
}
