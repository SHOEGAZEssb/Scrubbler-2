namespace Scrubbler.Host.Helper;

/// <summary>
/// A <see cref="DataTemplateSelector"/> that uses <see cref="ViewLocator"/> to resolve views for view models.
/// </summary>
public class ViewLocatorTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// Selects a data template for the given item by using the view locator to resolve the view.
    /// </summary>
    /// <param name="item">The item (typically a view model) for which to select a template.</param>
    /// <param name="container">The container (unused).</param>
    /// <returns>A <see cref="DataTemplate"/> that wraps the resolved view, or <c>null</c> if the item is <c>null</c>.</returns>
    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        if (item == null)
            return null!;

        var element = ViewLocator.Resolve(item);

        // Wrap the resolved view in a DataTemplate on the fly
        return new DataTemplate(() => element);
    }
}
