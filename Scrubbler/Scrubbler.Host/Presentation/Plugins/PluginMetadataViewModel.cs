using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Scrubbler.Abstractions.Plugin;

namespace Scrubbler.Host.Presentation.Plugins;
internal class PluginMetadataViewModel : ObservableObject
{
    public event EventHandler<PluginManifestEntry>? InstallRequested;

    public string Name => _meta.Name;
    public string Description => _meta.Description;
    public Version? Version => new(_meta.Version);
    public ImageSource? Icon => _icon ??= new BitmapImage(_meta.IconUri);
    private ImageSource? _icon;
    public string PluginType => _meta.PluginType;

    public ICommand InstallCommand { get; }

    private readonly PluginManifestEntry _meta;

    public PluginMetadataViewModel(PluginManifestEntry meta)
    {
        _meta = meta;
        InstallCommand = new RelayCommand(() =>
            InstallRequested?.Invoke(this, _meta));
    }
}

