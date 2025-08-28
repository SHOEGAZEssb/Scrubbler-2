namespace Scrubbler.Host.Services.Logging;

public record LogMessage(
    DateTime Timestamp,
    LogLevel Level,
    string Module,
    string Message,
    Exception? Exception = null
);
