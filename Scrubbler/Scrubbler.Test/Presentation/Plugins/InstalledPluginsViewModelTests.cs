using Moq;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Host.Presentation.Plugins;
using Scrubbler.Host.Services;

namespace Scrubbler.Test.Presentation.Plugins;

/// <summary>
/// Tests for InstalledPluginsViewModel constructor behavior (subscriptions and initial Refresh call).
/// </summary>
public partial class InstalledPluginsViewModelTests
{
    /// <summary>
    /// Verifies that constructing InstalledPluginsViewModel with a valid IPluginManager:
    /// - Calls Refresh (which accesses InstalledPlugins) during construction.
    /// - Initializes the Plugins collection and leaves it empty when the manager reports no installed plugins.
    /// Input conditions: manager.InstalledPlugins and manager.AvailablePlugins are empty lists.
    /// Expected result: Plugins collection is created and empty; InstalledPlugins getter was accessed at least once.
    /// </summary>
    [Test]
    public void InstalledPluginsViewModel_Constructor_WithValidManager_InitializesPluginsAndCallsRefresh()
    {
        // Arrange
        var installedPlugins = new List<IPlugin>();
        var availablePlugins = new List<PluginManifestEntry>();

        var managerMock = new Mock<IPluginManager>(MockBehavior.Strict);
        managerMock.SetupGet(m => m.InstalledPlugins).Returns(installedPlugins);
        managerMock.SetupGet(m => m.AvailablePlugins).Returns(availablePlugins);
        // Other properties/events not used during construction do not need setup in loose scenarios.
        // Act
        var vm = new InstalledPluginsViewModel(managerMock.Object);

        // Assert
        // Ensure Plugins collection was created and is empty (Refresh cleared/filled from manager).
        Assert.That(vm.Plugins, Is.Not.Null, "Plugins collection should be created by the viewmodel.");
        Assert.That(vm.Plugins, Is.Empty, "Plugins collection should be empty when manager reports no installed plugins.");

        // Verify that Refresh accessed InstalledPlugins during construction
        managerMock.VerifyGet(m => m.InstalledPlugins, Times.AtLeastOnce, "Constructor should call Refresh which enumerates InstalledPlugins.");
    }

    /// <summary>
    /// Verifies that the constructor subscribes to PluginInstalled and that raising the event causes Refresh to run.
    /// Input conditions: manager initially reports no installed plugins. After construction the PluginInstalled event is raised.
    /// Expected result: InstalledPlugins getter is accessed as a result of the event (indicating Refresh ran).
    /// </summary>
    [Test]
    public void InstalledPluginsViewModel_Constructor_SubscribesToPluginInstalled_EventTriggersRefresh()
    {
        // Arrange
        var installedPlugins = new List<IPlugin>();
        var availablePlugins = new List<PluginManifestEntry>();

        var managerMock = new Mock<IPluginManager>(MockBehavior.Strict);
        managerMock.SetupGet(m => m.InstalledPlugins).Returns(installedPlugins);
        managerMock.SetupGet(m => m.AvailablePlugins).Returns(availablePlugins);

        var vm = new InstalledPluginsViewModel(managerMock.Object);

        // Clear recorded invocations so we only observe calls caused by the raised event.
        managerMock.Invocations.Clear();

        // Act - raise the PluginInstalled event (sender provided; args as EventArgs.Empty)
        managerMock.Raise(m => m.PluginInstalled += null!, managerMock.Object, EventArgs.Empty);

        // Assert - Refresh should have accessed InstalledPlugins as part of handling the event
        managerMock.VerifyGet(m => m.InstalledPlugins, Times.AtLeastOnce, "Raising PluginInstalled should invoke Refresh which accesses InstalledPlugins.");
    }

    /// <summary>
    /// Verifies that the constructor subscribes to IsFetchingPluginsChanged and that:
    /// - Raising the event with false triggers Refresh (InstalledPlugins accessed).
    /// - Raising the event with true does NOT trigger Refresh.
    /// Input conditions: manager reports no installed plugins; event is raised with provided boolean.
    /// Expected result: InstalledPlugins getter call count matches expected behavior for the boolean input.
    /// </summary>
    /// <param name="isFetching">The boolean value to raise with the IsFetchingPluginsChanged event.</param>
    [TestCase(false)]
    [TestCase(true)]
    public void InstalledPluginsViewModel_Constructor_SubscribesToIsFetchingPluginsChanged_EventConditionallyTriggersRefresh(bool isFetching)
    {
        // Arrange
        var installedPlugins = new List<IPlugin>();
        var availablePlugins = new List<PluginManifestEntry>();

        var managerMock = new Mock<IPluginManager>(MockBehavior.Strict);
        managerMock.SetupGet(m => m.InstalledPlugins).Returns(installedPlugins);
        managerMock.SetupGet(m => m.AvailablePlugins).Returns(availablePlugins);

        var vm = new InstalledPluginsViewModel(managerMock.Object);

        // Clear recorded invocations so we only observe calls caused by the raised event.
        managerMock.Invocations.Clear();

        // Act - raise the IsFetchingPluginsChanged event with the test boolean
        managerMock.Raise(m => m.IsFetchingPluginsChanged += null!, managerMock.Object, isFetching);

        // Assert
        if (!isFetching)
        {
            // When fetching finished (false) Refresh should be called
            managerMock.VerifyGet(m => m.InstalledPlugins, Times.AtLeastOnce, "Raising IsFetchingPluginsChanged(false) should invoke Refresh which accesses InstalledPlugins.");
        }
        else
        {
            // When fetching started (true) Refresh should NOT be called
            managerMock.VerifyGet(m => m.InstalledPlugins, Times.Never, "Raising IsFetchingPluginsChanged(true) should not invoke Refresh.");
        }
    }

}
