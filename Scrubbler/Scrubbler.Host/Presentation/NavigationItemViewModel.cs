using System.Collections.ObjectModel;

namespace Scrubbler.Host.Presentation;
internal partial class NavigationItemViewModel : ObservableObject
{
    public string Title { get; }
    public IconSource? Icon { get; }
    public object? Content { get; }
    public ObservableCollection<NavigationItemViewModel> Children { get; } = [];

    [ObservableProperty]
    private bool _isExpanded; // for groups

    public bool HasChildren => Children.Count > 0;

    public NavigationItemViewModel(string title, IconSource? icon, object? content = null)
    {
        Title = title;
        Icon = icon ?? new SymbolIconSource() { Symbol = Symbol.Placeholder };
        Content = content;
    }
}
