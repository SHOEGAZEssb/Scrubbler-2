namespace Scrubbler.Host.Presentation.Navigation;

internal class NavigationStatusEventArgs(int errors, int warnings, int infos) : EventArgs
{
    #region Properties

    public int Errors { get; } = errors;

    public int Warnings { get; } = warnings;

    public int Infos { get; } = infos;

    #endregion Properties
}
