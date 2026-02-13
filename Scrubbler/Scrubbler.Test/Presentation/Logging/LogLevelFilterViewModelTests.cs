using NUnit.Framework;
using Scrubbler.Host.Models;
using Scrubbler.Host.Presentation.Logging;
using Microsoft.Extensions.Logging;

namespace Scrubbler.Test.Presentation.Logging;

/// <summary>
/// Tests for Scrubbler.Host.Presentation.Logging.LogLevelFilterViewModel.
/// </summary>
[TestFixture]
public class LogLevelFilterViewModelTests
{
    /// <summary>
    /// Verifies that PassesFilter returns true when the message level does not match the filter's level.
    /// </summary>
    [Test]
    public void PassesFilter_MessageLevelDoesNotMatch_ReturnsTrue()
    {
        // Arrange
        var filter = new LogLevelFilterViewModel(LogLevel.Warning);
        var message = new LogMessage(DateTime.UtcNow, LogLevel.Information, "ModuleA", "Test message");

        // Act
        var result = filter.PassesFilter(message);

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Verifies that PassesFilter returns true when the message level matches and the filter is enabled.
    /// </summary>
    [Test]
    public void PassesFilter_MessageLevelMatchesAndFilterEnabled_ReturnsTrue()
    {
        // Arrange
        var filter = new LogLevelFilterViewModel(LogLevel.Warning);
        var message = new LogMessage(DateTime.UtcNow, LogLevel.Warning, "ModuleA", "Test message");

        // Act
        var result = filter.PassesFilter(message);

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Verifies that PassesFilter returns false when the message level matches and the filter is disabled.
    /// </summary>
    [Test]
    public void PassesFilter_MessageLevelMatchesAndFilterDisabled_ReturnsFalse()
    {
        // Arrange
        var filter = new LogLevelFilterViewModel(LogLevel.Warning)
        {
            IsEnabled = false
        };
        var message = new LogMessage(DateTime.UtcNow, LogLevel.Warning, "ModuleA", "Test message");

        // Act
        var result = filter.PassesFilter(message);

        // Assert
        Assert.That(result, Is.False);
    }
}
