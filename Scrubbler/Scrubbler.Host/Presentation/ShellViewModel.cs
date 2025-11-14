namespace Scrubbler.Host.Presentation;

/// <summary>
/// View model for the shell/main navigation of the application.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ShellViewModel"/> class.
/// </remarks>
/// <param name="navigator">The navigation service for navigating between pages.</param>
public class ShellViewModel(
    INavigator navigator)
{
    private readonly INavigator _navigator = navigator;
}
