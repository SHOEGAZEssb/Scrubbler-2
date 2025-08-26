using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Scrubbler.Abstractions;

namespace Scrubbler.Plugin.ManualScrobbler;
public partial class ManualScrobbleViewModel : ScrobblePluginViewModelBase
{
    #region Properties

    [ObservableProperty]
    private string _artistName = string.Empty;

    [ObservableProperty]
    private string _trackName = string.Empty;

    [ObservableProperty]
    private string _albumName = string.Empty;

    [ObservableProperty]
    private string _albumArtistName = string.Empty;

    [ObservableProperty]
    private int _amount;

    [ObservableProperty]
    private DateTime _playedAt;

    #endregion Properties

    public override IEnumerable<ScrobbleData> GetScrobbles()
    {
        IsBusy = true;

        try
        {
            var scrobbles = new ScrobbleData[Amount];
            for (int i = 0; i < scrobbles.Length; i++)
            {
                scrobbles[i] = new ScrobbleData();
            }

            return scrobbles;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
