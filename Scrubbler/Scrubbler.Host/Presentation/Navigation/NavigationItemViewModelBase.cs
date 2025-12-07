namespace Scrubbler.Host.Presentation.Navigation;

internal class NavigationItemViewModelBase(string title, object? content) : ObservableObject
{
    #region Properties

    public string Title { get; } = title;

    public object? Content { get; } = content;

    #endregion Properties
}
