namespace Scrubbler.Host.Presentation.Navigation;

internal abstract class NavigationItemViewModelBase(string title, object? content) : ObservableObject
{
    #region Properties

    public string Title { get; } = title;

    public object? Content { get; } = content;

    public abstract bool IsSelected { get; set; }

    #endregion Properties
}
