using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Scrubbler.Abstractions.Plugin;

namespace Scrubbler.Host.Presentation.Plugins;
internal partial class PluginMetadataViewModel(PluginManifestEntry meta) : ObservableObject
{
    #region Properties

    public event EventHandler<PluginManifestEntry>? InstallRequested;

    public string Name => _meta.Name;
    public string Description => _meta.Description;
    public Version? Version => new(_meta.Version);
    public ImageSource? Icon => _icon ??= new BitmapImage(_meta.IconUri);
    private ImageSource? _icon;
    public string PluginType => _meta.PluginType;
    public IEnumerable<string> SupportedPlatforms => _meta.SupportedPlatforms;
    public bool IsCompatible => IsCompatibleWithCurrentPlatform(_meta);

    private readonly PluginManifestEntry _meta = meta;

    #endregion Properties

    [RelayCommand]
    private void Install()
    {
        if (!IsCompatible)
            return;

        InstallRequested?.Invoke(this, _meta);
    }

    private static bool IsCompatibleWithCurrentPlatform(PluginManifestEntry meta)
    {
        if (meta.SupportedPlatforms.Contains("All"))
            return true;

        // todo: check if we can move away from strings and use an enum or something more robust for platform identifiers
        foreach (var platform in meta.SupportedPlatforms)
        {
            if (OperatingSystem.IsOSPlatform(platform))
                return true;
        }

        return false;
    }
}
