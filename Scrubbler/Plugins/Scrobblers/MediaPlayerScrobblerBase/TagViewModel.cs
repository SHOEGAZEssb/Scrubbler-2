using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MediaPlayerScrobblerBase;

public partial class TagViewModel(string tagName) : ObservableObject
{
    #region Properties

    public event EventHandler<string>? OpenLinkRequested;

    public string Name { get; } = tagName;

    #endregion Properties

    [RelayCommand]
    private void Clicked(string tagName)
    {
        OpenLinkRequested?.Invoke(this, tagName);
    }
}
