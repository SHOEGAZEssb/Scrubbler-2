using Moq;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Plugin.Account;
using Scrubbler.Host.Models;
using Scrubbler.Host.Presentation.Accounts;
using Scrubbler.Host.Services;


namespace Scrubbler.Test.Presentation.Accounts;

/// <summary>
/// Tests for AccountsViewModel constructor behavior: wiring of plugin-manager events and initial population from InstalledPlugins.
/// </summary>
public partial class AccountsViewModelTests
{
    /// <summary>
    /// Verifies that the constructor populates the Accounts collection from IAccountPlugin entries in IPluginManager.InstalledPlugins,
    /// and that the constructor subscribes to the PluginInstalled and PluginUninstalled events on the plugin manager.
    /// Input: IPluginManager.InstalledPlugins contains one IAccountPlugin and one non-account IPlugin.
    /// Expected: Accounts contains exactly one AccountPluginViewModel and both PluginInstalled and PluginUninstalled add handlers were registered.
    /// </summary>
    [Test]
    public void AccountsViewModel_Constructor_PopulatesAccountsAndSubscribesToPluginManagerEvents()
    {
        // Arrange
        var pluginManagerMock = new Mock<IPluginManager>(MockBehavior.Strict);

        // Create an account plugin and a non-account plugin to ensure OfType filtering works
        var accountPluginMock = new Mock<IAccountPlugin>(MockBehavior.Loose);
        var otherPluginMock = new Mock<IPlugin>(MockBehavior.Loose);

        IEnumerable<IPlugin> installedPlugins = [accountPluginMock.Object, otherPluginMock.Object];
        pluginManagerMock.SetupGet(p => p.InstalledPlugins).Returns(installedPlugins);

        EventHandler? capturedInstalledHandler = null;
        pluginManagerMock
            .SetupAdd(p => p.PluginInstalled += It.IsAny<EventHandler>())
            .Callback<EventHandler?>(h => capturedInstalledHandler = h);

        EventHandler? capturedUninstalledHandler = null;
        pluginManagerMock
            .SetupAdd(p => p.PluginUninstalled += It.IsAny<EventHandler>())
            .Callback<EventHandler?>(h => capturedUninstalledHandler = h);

        var configMock = new Mock<IWritableOptions<UserConfig>>(MockBehavior.Strict);
        // Not used during construction in this test, but allow no unexpected calls.
        // Act
        var vm = new AccountsViewModel(pluginManagerMock.Object, configMock.Object);

        // Assert
        Assert.That(vm.Accounts, Is.Not.Null, "Accounts collection should be created by the viewmodel.");
        // Only the IAccountPlugin should have been turned into an AccountPluginViewModel
        Assert.That(vm.Accounts, Has.Count.EqualTo(1), "Accounts should contain one entry for the IAccountPlugin in InstalledPlugins.");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(vm.Accounts[0], Is.InstanceOf<AccountPluginViewModel>(), "Item should be an AccountPluginViewModel.");

            // Ensure event handlers were subscribed
            Assert.That(capturedInstalledHandler, Is.Not.Null, "Constructor should subscribe to PluginInstalled.");
            Assert.That(capturedUninstalledHandler, Is.Not.Null, "Constructor should subscribe to PluginUninstalled.");
        }
    }

    /// <summary>
    /// Verifies that when the plugin manager raises PluginInstalled, the registered handler causes the viewmodel to refresh and update Accounts.
    /// Input: Initially InstalledPlugins is empty; after raising PluginInstalled, InstalledPlugins contains one IAccountPlugin.
    /// Expected: Accounts is updated to include one AccountPluginViewModel after the event.
    /// </summary>
    [Test]
    public void AccountsViewModel_PluginInstalledEvent_RefreshesAccountsCollection()
    {
        // Arrange
        var pluginManagerMock = new Mock<IPluginManager>(MockBehavior.Strict);

        // start with empty installed plugins, but allow dynamic change by using a backing variable
        IEnumerable<IPlugin> installedPlugins = [];
        pluginManagerMock.SetupGet(p => p.InstalledPlugins).Returns(() => installedPlugins);

        EventHandler? installedHandler = null;
        pluginManagerMock
            .SetupAdd(p => p.PluginInstalled += It.IsAny<EventHandler>())
            .Callback<EventHandler?>(h => installedHandler = h);

        // We don't care about PluginUninstalled for this test but constructor will add a handler; allow it
        pluginManagerMock
            .SetupAdd(p => p.PluginUninstalled += It.IsAny<EventHandler>())
            .Callback<EventHandler?>(_ => { /* ignore */ });

        var configMock = new Mock<IWritableOptions<UserConfig>>(MockBehavior.Strict);

        // Act - construct the viewmodel; initial Accounts should reflect empty installedPlugins
        var vm = new AccountsViewModel(pluginManagerMock.Object, configMock.Object);

        // Sanity
        Assert.That(vm.Accounts, Is.Not.Null);
        Assert.That(vm.Accounts, Is.Empty, "Initially Accounts should be empty when InstalledPlugins is empty.");

        // Now simulate installing an account plugin by changing the installedPlugins reference
        var accountPluginMock = new Mock<IAccountPlugin>(MockBehavior.Loose);
        installedPlugins = [accountPluginMock.Object];

        // Act - invoke the captured installed handler to simulate the plugin manager raising the event
        Assert.That(installedHandler, Is.Not.Null, "PluginInstalled handler should have been captured during construction.");
        installedHandler!(pluginManagerMock.Object, EventArgs.Empty);

        // Assert - Accounts should be refreshed and contain one entry for the account plugin
        Assert.That(vm.Accounts, Has.Count.EqualTo(1), "After PluginInstalled event, Accounts should contain the newly installed IAccountPlugin.");
        Assert.That(vm.Accounts[0], Is.InstanceOf<AccountPluginViewModel>(), "Refreshed item should be an AccountPluginViewModel.");
    }
}
