namespace Scrubbler.Abstractions;

public class ScrobbleResponse(bool success, string? errorMessage)
{
    #region Properties

    public bool Success { get; } = success;

    public string? ErrorMessage { get; } = errorMessage;

    #endregion Properties
}
