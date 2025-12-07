using System.Collections.ObjectModel;

namespace Scrubbler.Host.Presentation.Navigation;
internal partial class MenuNavigationItemViewModel : NavigationItemViewModelBase
{
    public IconSource? Icon { get; }
    
    public ObservableCollection<NavigationItemViewModelBase> Children { get; } = [];

    [ObservableProperty]
    private bool _isExpanded; // for groups

    public bool HasChildren => Children.Count > 0;

    public MenuNavigationItemViewModel(string title, IconSource? icon, object? content = null)
        : base(title, content)
    {
        Icon = icon ?? new SymbolIconSource() { Symbol = Symbol.Placeholder };
    }
}
