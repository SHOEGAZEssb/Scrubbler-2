namespace Scrubbler.Host.Presentation.Navigation;

internal class NavigationTemplateSelector : DataTemplateSelector
{
    public DataTemplate? MenuTemplate { get; set; }
    public DataTemplate? PluginTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        return item switch
        {
            PluginNavigationItemViewModel => PluginTemplate!,
            MenuNavigationItemViewModel => MenuTemplate!,
            _ => base.SelectTemplateCore(item)
        };
    }
}
