namespace Scrubbler.Host.Helper;
public class ViewLocatorTemplateSelector : DataTemplateSelector
{
    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        if (item == null)
            return null!;

        var element = ViewLocator.Resolve(item);

        // Wrap the resolved view in a DataTemplate on the fly
        return new DataTemplate(() => element);
    }
}
