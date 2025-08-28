namespace Scrubbler.Host.Models;

public record LogMessage(
    DateTime Timestamp,
    LogLevel Level,
    string Module,
    string Message,
    Exception? Exception = null
);
