using Moq;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Host.Presentation.Plugins;
using Scrubbler.Host.Services;

namespace Scrubbler.Test.Presentation.Plugins;

/// <summary>
/// Tests for AvailablePluginsViewModel (focused on Refresh method).
/// Note: The source class is partial; the test class is partial as required.
/// </summary>
public partial class AvailablePluginsViewModelTests
{
    /// <summary>
    /// Verifies that Refresh leaves Plugins empty when the IPluginManager provides no available plugins.
    /// Input: IPluginManager.AvailablePlugins is an empty list and InstalledPlugins is empty.
    /// Expected: ViewModel.Plugins is not null and remains empty; constructor subscribes to expected events.
    /// </summary>
    [Test]
    public void Refresh_NoAvailablePlugins_PluginsEmptyAndEventsSubscribed()
    {
        // Arrange
        var managerMock = new Mock<IPluginManager>(MockBehavior.Strict);

        // Provide an empty list for available plugins (no PluginManifestEntry instances required).
        managerMock.SetupGet(m => m.AvailablePlugins).Returns([]);

        // No installed plugins.
        managerMock.SetupGet(m => m.InstalledPlugins).Returns([]);

        // Provide expected simple property for IsFetchingPlugins (used by IsBusy property).
        managerMock.SetupGet(m => m.IsFetchingPlugins).Returns(false);

        // Expectation: constructor subscribes to these events. Allow subscription via SetupAdd.
        managerMock.SetupAdd(m => m.IsFetchingPluginsChanged += It.IsAny<EventHandler<bool>>()).Verifiable();
        managerMock.SetupAdd(m => m.PluginUninstalled += It.IsAny<EventHandler>()).Verifiable();

        // Act
        var vm = new AvailablePluginsViewModel(managerMock.Object);

        // Assert - Plugins collection must be created and empty
        Assert.That(vm.Plugins, Is.Not.Null, "Plugins collection should be created on construction.");
        Assert.That(vm.Plugins, Is.Empty, "Plugins should remain empty when there are no available plugins.");

        // Verify the constructor subscribed to the plugin manager events exactly once.
        managerMock.VerifyAdd(m => m.IsFetchingPluginsChanged += It.IsAny<EventHandler<bool>>(), Times.Once);
        managerMock.VerifyAdd(m => m.PluginUninstalled += It.IsAny<EventHandler>(), Times.Once);
    }

    [Test]
    public void FilteredPlugins_SearchText_FiltersByNameAndDescription_CaseInsensitive()
    {
        // Arrange
        var managerMock = new Mock<IPluginManager>(MockBehavior.Strict);

        var metaA = new PluginManifestEntry(
            "A.Plugin.Type",
            "AlphaPlugin",
            "1.0.0",
            "Second plugin",
            "Type",
            ["All"],
            new Uri("http://example.com/a.zip"),
            null);

        var metaB = new PluginManifestEntry(
            "B.Plugin.Type",
            "BetaPlugin",
            "1.0.0",
            "Contains Alpha in description",
            "Type",
            ["All"],
            new Uri("http://example.com/b.zip"),
            null);
        managerMock.SetupGet(m => m.AvailablePlugins).Returns([metaA, metaB]);
        managerMock.SetupGet(m => m.InstalledPlugins).Returns([]);
        managerMock.SetupGet(m => m.IsFetchingPlugins).Returns(false);
        managerMock.SetupAdd(m => m.IsFetchingPluginsChanged += It.IsAny<EventHandler<bool>>()).Verifiable();
        managerMock.SetupAdd(m => m.PluginUninstalled += It.IsAny<EventHandler>()).Verifiable();

        // Act
        var vm = new AvailablePluginsViewModel(managerMock.Object)
        {
            // search is case-insensitive and matches both by name and description
            SearchText = "alpha"
        };

        var filtered = vm.FilteredPlugins.ToList();

        // Assert
        Assert.That(filtered, Has.Count.EqualTo(2), "Both items should match 'alpha' in name or description.");

        // Now restrict to a term that only matches metaA's description
        vm.SearchText = "second";
        filtered = [.. vm.FilteredPlugins];
        Assert.That(filtered, Has.Count.EqualTo(1));
        Assert.That(filtered[0].Name, Is.EqualTo(metaA.Name));
    }

    [Test]
    public void FilteredPlugins_ShowOnlyCompatible_FiltersByCompatibilityFlag()
    {
        // Arrange
        var managerMock = new Mock<IPluginManager>(MockBehavior.Strict);

        var metaCompatible = new PluginManifestEntry(
            "C.Plugin.Type",
            "CompatiblePlugin",
            "1.0.0",
            "Compatible",
            "Type",
            ["All"],
            new Uri("http://example.com/c.zip"),
            null);

        var metaIncompatible = new PluginManifestEntry(
            "D.Plugin.Type",
            "IncompatiblePlugin",
            "1.0.0",
            "Incompatible",
            "Type",
            ["NonExistentPlatform"],
            new Uri("http://example.com/d.zip"),
            null);
        managerMock.SetupGet(m => m.AvailablePlugins).Returns([metaCompatible, metaIncompatible]);
        managerMock.SetupGet(m => m.InstalledPlugins).Returns([]);
        managerMock.SetupGet(m => m.IsFetchingPlugins).Returns(false);
        managerMock.SetupAdd(m => m.IsFetchingPluginsChanged += It.IsAny<EventHandler<bool>>()).Verifiable();
        managerMock.SetupAdd(m => m.PluginUninstalled += It.IsAny<EventHandler>()).Verifiable();

        // Act
        var vm = new AvailablePluginsViewModel(managerMock.Object);

        // By default ShowOnlyCompatible == true, so only the compatible plugin should be visible in FilteredPlugins
        var filtered = vm.FilteredPlugins.ToList();
        Assert.That(filtered, Has.Count.EqualTo(1));
        Assert.That(filtered[0].Name, Is.EqualTo(metaCompatible.Name));

        // When we disable the compatibility filter, both plugins should be visible
        vm.ShowOnlyCompatible = false;
        filtered = [.. vm.FilteredPlugins];
        Assert.That(filtered, Has.Count.EqualTo(2));
    }

    /// <summary>
    /// Partial test placeholder: verifies behavior when AvailablePlugins contains entries and some are installed.
    /// Purpose: exercise the branch that skips already-installed plugins and adds not-installed ones,
    /// including wiring of InstallRequested event on created PluginMetadataViewModel instances.
    /// Input conditions to provide before enabling this test:
    /// - A means to construct PluginManifestEntry instances with a known Id value (string).
    /// - Installed IPlugin instances whose runtime Type.FullName can be made to match those Id values.
    /// Expected:
    /// - Entries in AvailablePlugins whose Id matches any InstalledPlugins' type FullName are skipped.
    /// - Non-matching entries produce PluginMetadataViewModel items added to Plugins and have InstallRequested wired.
    ///
    /// NOTE: The concrete shape (constructors/properties) of PluginManifestEntry is not available in the provided scope.
    /// To complete this test:
    /// 1) Provide a factory method that produces PluginManifestEntry instances with an assignable Id (and other properties used by PluginMetadataViewModel).
    /// 2) Option A: If you can create a concrete IPlugin implementer within the test assembly whose runtime FullName matches the desired Id,
    ///    create that as an inner helper type and include it here (allowed only as an inner helper).
    /// 3) Option B: If not feasible, use an actual PluginManifestEntry implementation exposed by the production code that allows construction.
    /// Until the above is available, mark the test as Inconclusive to avoid false positives.
    /// </summary>
    [Test]
    public void Refresh_WithAvailablePlugins_SkipsInstalledAndAddsUninstalled_Partial()
    {
        // Arrange
        var managerMock = new Mock<IPluginManager>(MockBehavior.Strict);

        // Create a mock installed plugin. We'll make one available plugin whose Id equals the runtime type full name of this mock
        // so it should be considered 'installed' and therefore skipped by Refresh.
        var installedPluginMock = new Mock<IPlugin>(MockBehavior.Strict);
        installedPluginMock.SetupGet(p => p.Id).Returns(installedPluginMock.Object.GetType().FullName!);

        // Create two manifests: one that corresponds to the installed plugin (Id == installedPlugin.GetType().FullName)
        // and one that represents an uninstalled plugin.
        var metaInstalled = new PluginManifestEntry(
            installedPluginMock.Object.Id,
            "InstalledPlugin",
            "1.0.0",
            "Installed plugin",
            "Type",
            ["Any"],
            new Uri("http://example.com/installed.zip"),
            null);

        var metaUninstalled = new PluginManifestEntry(
            "Uninstalled.Plugin.Type",
            "UninstalledPlugin",
            "1.2.3",
            "Not installed plugin",
            "Type",
            ["All"],
            new Uri("http://example.com/uninstalled.zip"),
            null);

        // Provide available and installed lists
        managerMock.SetupGet(m => m.AvailablePlugins).Returns([metaInstalled, metaUninstalled]);
        managerMock.SetupGet(m => m.InstalledPlugins).Returns([installedPluginMock.Object]);
        managerMock.SetupGet(m => m.IsFetchingPlugins).Returns(false);

        // Allow subscription to events in constructor
        managerMock.SetupAdd(m => m.IsFetchingPluginsChanged += It.IsAny<EventHandler<bool>>()).Verifiable();
        managerMock.SetupAdd(m => m.PluginUninstalled += It.IsAny<EventHandler>()).Verifiable();

        // Expect InstallAsync to be called when the remaining PluginMetadataViewModel's InstallCommand is executed.
        managerMock.Setup(m => m.InstallAsync(It.Is<PluginManifestEntry>(p => p.Id == metaUninstalled.Id)))
                   .Returns(Task.CompletedTask)
                   .Verifiable();

        // Act
        var vm = new AvailablePluginsViewModel(managerMock.Object);

        // Assert - only the uninstalled manifest should have produced a PluginMetadataViewModel
        Assert.That(vm.Plugins, Is.Not.Null);
        Assert.That(vm.Plugins, Has.Count.EqualTo(1), "Only uninstalled available plugins should be added to the viewmodel.");

        var created = vm.Plugins[0];
        Assert.That(created.Name, Is.EqualTo(metaUninstalled.Name));

        // Execute the InstallCommand to ensure the viewmodel wired the InstallRequested handler to call InstallAsync
        created.InstallCommand.Execute(null);

        // Verify expectations
        managerMock.VerifyAdd(m => m.IsFetchingPluginsChanged += It.IsAny<EventHandler<bool>>(), Times.Once);
        managerMock.VerifyAdd(m => m.PluginUninstalled += It.IsAny<EventHandler>(), Times.Once);
        managerMock.Verify(m => m.InstallAsync(It.Is<PluginManifestEntry>(p => p.Id == metaUninstalled.Id)), Times.Once);
    }

    /// <summary>
    /// Verifies that the constructor invokes Refresh and that when there are no available plugins the Plugins collection is empty
    /// and IsBusy reflects the manager's IsFetchingPlugins value.
    /// Input conditions:
    /// - IPluginManager.AvailablePlugins is empty.
    /// - IPluginManager.InstalledPlugins is empty.
    /// - IPluginManager.IsFetchingPlugins is false.
    /// Expected result:
    /// - Plugins collection is created and empty after construction.
    /// - IsBusy equals the manager's IsFetchingPlugins (false).
    /// </summary>
    [Test]
    public void AvailablePluginsViewModel_Constructor_WithEmptyAvailablePlugins_InitializesEmptyPluginsAndIsBusy()
    {
        // Arrange
        var available = new List<PluginManifestEntry>();
        var installed = Enumerable.Empty<IPlugin>();
        var managerMock = new Mock<IPluginManager>(MockBehavior.Strict);

        // dynamic return values so Refresh invoked from ctor will see current lists
        managerMock.SetupGet(m => m.AvailablePlugins).Returns(() => available);
        managerMock.SetupGet(m => m.InstalledPlugins).Returns(() => installed);
        managerMock.SetupGet(m => m.IsFetchingPlugins).Returns(false);

        // Act
        var vm = new AvailablePluginsViewModel(managerMock.Object);

        // Assert
        // Plugins should be created and cleared by Refresh during construction
        Assert.That(vm.Plugins, Is.Not.Null, "Plugins collection should be instantiated by the viewmodel.");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(vm.Plugins, Is.Empty, "Plugins should be empty when manager.AvailablePlugins is empty.");
            Assert.That(vm.IsBusy, Is.False, "IsBusy should reflect manager.IsFetchingPlugins (false).");
        }
    }

}
