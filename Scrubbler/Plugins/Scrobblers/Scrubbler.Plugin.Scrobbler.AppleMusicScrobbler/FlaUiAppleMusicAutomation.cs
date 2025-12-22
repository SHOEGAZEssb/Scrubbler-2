using System.Diagnostics;
using System.Text.RegularExpressions;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.UIA3;

namespace Scrubbler.Plugin.Scrobbler.AppleMusicScrobbler;

internal sealed class FlaUiAppleMusicAutomation(bool composerAsArtist = false) : IAppleMusicAutomation
{
    private UIA3Automation? _automation;
    private AutomationElement? _songPanel;

    private static readonly Regex ComposerPerformerRegex =
        new(@"By\s.*?\s\u2014", RegexOptions.Compiled);

    private static readonly string[] Separator = [" \u2014 "];
    private readonly bool _composerAsArtist = composerAsArtist;

    public void Connect()
    {
        _songPanel = GetSongPanel();
    }

    public void Disconnect()
    {
        _automation?.Dispose();
        _automation = null;
        _songPanel = null;
    }

    public AppleMusicInfo? GetCurrentSong()
    {
        if (_songPanel == null)
            return null;

        var songFieldsPanel = _songPanel.FindFirstChild("LCD");
        var songFields = songFieldsPanel?
            .FindAllChildren(new ConditionFactory(new UIA3PropertyLibrary())
                .ByAutomationId("myScrollViewer")) ?? [];

        if (songFieldsPanel == null || songFields.Length != 2)
            return null;

        var nameEl = songFields[0];
        var metaEl = songFields[1];

        if (nameEl.BoundingRectangle.Bottom > metaEl.BoundingRectangle.Bottom)
            (nameEl, metaEl) = (metaEl, nameEl);

        var (artist, album, _) =
            ParseSongAlbumArtist(metaEl.Name, _composerAsArtist);

        var scrubber = songFieldsPanel.FindFirstChild("LCDScrubber");
        if (scrubber == null)
            return null;

        var duration =
            (int)scrubber.Patterns.RangeValue.Pattern.Maximum.Value;

        return new AppleMusicInfo(nameEl.Name, artist, album, artist)
        {
            SongDuration = duration
        };
    }

    public int GetCurrentPositionSeconds()
    {
        if (_songPanel == null)
            return -1;

        var scrubber = _songPanel
            .FindFirstChild("LCD")
            ?.FindFirstChild("LCDScrubber");

        return scrubber == null
            ? -1
            : (int)scrubber.Patterns.RangeValue.Pattern.Value.Value;
    }

    public void Dispose() => Disconnect();

    // ---------------- private helpers ----------------

    private AutomationElement GetSongPanel()
    {
        var proc = Process.GetProcessesByName("AppleMusic").FirstOrDefault()
            ?? throw new InvalidOperationException("Apple Music not running");

        _automation = new UIA3Automation();

        var window = _automation.GetDesktop()
            .FindFirstDescendant(c => c.ByProcessId(proc.Id))
            ?? FlaUI.Core.Application.Attach(proc.Id)
                .GetMainWindow(_automation)
            ?? throw new InvalidOperationException("Apple Music window not found");

        var panel = window.Name == "Mini Player"
            ? window.FindFirstChild(cf => cf.ByClassName("Microsoft.UI.Content.DesktopChildSiteBridge"))
            : FindFirstDescendantWithAutomationId(window, "TransportBar");

        return panel ?? throw new InvalidOperationException("Song panel not found");
    }

    private static AutomationElement? FindFirstDescendantWithAutomationId(AutomationElement root, string id)
    {
        var queue = new Queue<AutomationElement>();
        queue.Enqueue(root);

        while (queue.Count > 0 && queue.Count < 25)
        {
            var el = queue.Dequeue();
            if (el.Properties.AutomationId.IsSupported &&
                el.AutomationId == id)
                return el;

            foreach (var c in el.FindAllChildren())
                queue.Enqueue(c);
        }

        return null;
    }

    private static Tuple<string, string, string?> ParseSongAlbumArtist(string value, bool composerAsArtist)
    {
        string artist;
        string album;
        string? performer = null;

        var match = ComposerPerformerRegex.Matches(value);
        if (match.Count > 0)
        {
            var parts = value.Split(Separator, StringSplitOptions.None);
            var composer = parts[0][3..];
            performer = parts[1];

            artist = composerAsArtist ? composer : performer;
            album = parts[2];
        }
        else
        {
            var parts = value.Split(Separator, StringSplitOptions.None);
            artist = parts[0];
            album = parts.Length > 1 ? parts[1] : parts[0];
        }

        return Tuple.Create(artist, album, performer);
    }
}
