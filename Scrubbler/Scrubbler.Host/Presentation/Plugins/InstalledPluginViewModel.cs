using CommunityToolkit.Mvvm.Input;
using Scrubbler.Abstractions.Plugin;

namespace Scrubbler.Host.Presentation.Plugins;

internal partial class InstalledPluginViewModel : ObservableObject, IDisposable
{
    #region Properties

    public event EventHandler<string>? UninstallRequested;
    public event EventHandler<string>? UpdateRequested;

    public string Name { get; }
    public string Description { get; }
    public Version Version { get; }
    public Uri? Icon { get; }

    public bool CanBeUpdated { get; }

    public string PluginType { get; }

    private readonly string _id;

    #endregion Properties

    #region Construction

    // do not use primary constructor to avoid caching the IPlugin reference
#pragma warning disable IDE0290 // Use primary constructor
    public InstalledPluginViewModel(IPlugin plugin, bool canBeUpdated)
#pragma warning restore IDE0290 // Use primary constructor
    {
        Name = plugin.Name;
        _id = plugin.Id;
        Description = plugin.Description;
        Version = plugin.Version;
        Icon = plugin.IconUri;
        CanBeUpdated = canBeUpdated;
        PluginType = plugin.ResolvePluginType();
    }

    #endregion Construction

    [RelayCommand]
    private void Uninstall()
    {
        UninstallRequested?.Invoke(this, _id);
    }

    [RelayCommand(CanExecute = nameof(CanBeUpdated))]
    private void Update()
    {
        UpdateRequested?.Invoke(this, _id);
    }

    public void Dispose()
    {
        UninstallRequested = null;
        UpdateRequested = null;
    }
}
