using System.ComponentModel;
using Moq;
using Scrubbler.Abstractions.Plugin.Account;
using Scrubbler.Host.Models;
using Scrubbler.Host.Presentation.Accounts;

namespace Scrubbler.Test.Presentation.Accounts;

[TestFixture]
public class AccountPluginViewModelTests
{
    /// <summary>
    /// Verifies that UpdateIsUsingAccountFunctions raises PropertyChanged for the IsUsingAccountFunctions property.
    /// Input conditions:
    /// - A mock IAccountPlugin and IWritableOptions&lt;UserConfig&gt; are provided to the viewmodel ctor.
    /// - No special setup for scrobble-limit or account-functions interfaces is required.
    /// Expected result:
    /// - Calling UpdateIsUsingAccountFunctions causes a single PropertyChanged event with PropertyName == "IsUsingAccountFunctions".
    /// </summary>
    [Test]
    public void UpdateIsUsingAccountFunctions_PropertyChangedRaised_ForIsUsingAccountFunctions()
    {
        // Arrange
        var pluginMock = new Mock<IAccountPlugin>(MockBehavior.Strict);
        pluginMock.SetupGet(p => p.Id).Returns("plugin-1");
        // Other members of IAccountPlugin are not accessed by UpdateIsUsingAccountFunctions; keep strict mock minimal.
        var config = new UserConfig { AccountFunctionsPluginID = "plugin-1" };
        var configMock = new Mock<IWritableOptions<UserConfig>>(MockBehavior.Strict);
        configMock.SetupGet(c => c.Value).Returns(config);

        var vm = new AccountPluginViewModel(pluginMock.Object, configMock.Object);

        var received = new List<string?>();
        vm.PropertyChanged += (s, e) => received.Add(e?.PropertyName);

        // Act
        vm.UpdateIsUsingAccountFunctions();

        // Assert
        Assert.That(received, Is.Not.Null, "PropertyChanged subscriber should have been invoked.");
        Assert.That(received, Has.Count.EqualTo(1), "Exactly one PropertyChanged event should be raised.");
        Assert.That(received[0], Is.EqualTo(nameof(AccountPluginViewModel.IsUsingAccountFunctions)),
            "The raised PropertyChanged event should be for IsUsingAccountFunctions.");
    }

    /// <summary>
    /// Verifies that UpdateIsUsingAccountFunctions does not raise PropertyChanged for unrelated property names.
    /// Input conditions:
    /// - ViewModel constructed with simple mocks.
    /// Expected result:
    /// - After calling UpdateIsUsingAccountFunctions the only PropertyChanged name observed is IsUsingAccountFunctions.
    /// </summary>
    [Test]
    public void UpdateIsUsingAccountFunctions_DoesNotRaiseOtherPropertyNames()
    {
        // Arrange
        var pluginMock = new Mock<IAccountPlugin>(MockBehavior.Strict);
        pluginMock.SetupGet(p => p.Id).Returns("plugin-2");
        var config = new UserConfig { AccountFunctionsPluginID = "other-plugin" };
        var configMock = new Mock<IWritableOptions<UserConfig>>(MockBehavior.Strict);
        configMock.SetupGet(c => c.Value).Returns(config);

        var vm = new AccountPluginViewModel(pluginMock.Object, configMock.Object);

        var received = new List<string?>();
        vm.PropertyChanged += (s, e) => received.Add(e?.PropertyName);

        // Act
        vm.UpdateIsUsingAccountFunctions();

        // Assert
        Assert.That(received, Is.Not.Null);
        Assert.That(received, Is.Not.Empty, "At least one PropertyChanged event should be raised.");
        // Ensure none of the raised names are unexpected (only IsUsingAccountFunctions is allowed)
        foreach (var name in received)
        {
            Assert.That(name, Is.EqualTo(nameof(AccountPluginViewModel.IsUsingAccountFunctions)),
                $"Unexpected PropertyChanged name '{name}' was raised.");
        }
    }

    /// <summary>
    /// Validates setter behavior of IsUsingAccountFunctions:
    /// - When the new value differs from the computed current value, the RequestedIsUsingAccountFunctionsChange event is raised with the new value
    ///   and PropertyChanged is raised for "IsUsingAccountFunctions".
    /// - When the new value equals the current value, no event or PropertyChanged is raised.
    /// Test cases cover initial config values equal/not-equal to plugin Id and various set values.
    /// </summary>
    /// <param name="initialConfigId">Initial AccountFunctionsPluginID from configuration (nullable).</param>
    /// <param name="pluginId">The IAccountPlugin.Id value (non-null).</param>
    /// <param name="setValue">The value to assign to IsUsingAccountFunctions.</param>
    /// <param name="expectInvoke">Whether the RequestedIsUsingAccountFunctionsChange event and PropertyChanged are expected.</param>
    [TestCase(null, "p1", true, true)]    // initial false -> set true => invoked
    [TestCase(null, "p1", false, false)]  // initial false -> set false => no-op
    [TestCase("p1", "p1", true, false)]   // initial true -> set true => no-op
    [TestCase("p1", "p1", false, true)]   // initial true -> set false => invoked
    public void IsUsingAccountFunctions_Setter_ValueDifferent_RaisesEventAndPropertyChanged(string? initialConfigId, string pluginId, bool setValue, bool expectInvoke)
    {
        // Arrange
        var pluginMock = new Mock<IAccountPlugin>();
        pluginMock.SetupGet(p => p.Id).Returns(pluginId);

        var userConfig = new UserConfig { AccountFunctionsPluginID = initialConfigId };
        var configMock = new Mock<IWritableOptions<UserConfig>>();
        configMock.SetupGet(c => c.Value).Returns(userConfig);

        var vm = new AccountPluginViewModel(pluginMock.Object, configMock.Object);

        bool eventRaised = false;
        bool eventValue = false;
        int eventCount = 0;

        vm.RequestedIsUsingAccountFunctionsChange += (s, e) =>
        {
            eventRaised = true;
            eventValue = e;
            eventCount++;
        };

        var changedProperties = new List<string>();
        ((INotifyPropertyChanged)vm).PropertyChanged += (s, e) =>
        {
            // PropertyName may be null in some implementations; guard against that.
            changedProperties.Add(e.PropertyName ?? string.Empty);
        };

        // Act
        vm.IsUsingAccountFunctions = setValue;

        // Assert
        if (expectInvoke)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(eventCount, Is.EqualTo(1), "RequestedIsUsingAccountFunctionsChange should be raised exactly once when value changes.");
                Assert.That(eventRaised, Is.True, "RequestedIsUsingAccountFunctionsChange should be raised when value changes.");
                Assert.That(eventValue, Is.EqualTo(setValue), "Event should be raised with the assigned value.");
                Assert.That(changedProperties, Has.One.Items);
            }
            Assert.That(changedProperties, Does.Contain(nameof(vm.IsUsingAccountFunctions)));
        }
        else
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(eventCount, Is.Zero, "RequestedIsUsingAccountFunctionsChange should not be raised when value does not change.");
                Assert.That(eventRaised, Is.False, "RequestedIsUsingAccountFunctionsChange should not be raised when value does not change.");
                Assert.That(changedProperties, Is.Empty, "PropertyChanged should not be raised when value does not change.");
            }
        }
    }

    /// <summary>
    /// Verifies that when a plugin that does NOT implement IHaveScrobbleLimit is passed to the constructor,
    /// the view model does not report scrobble-limit support and CurrentScrobbleCount remains null.
    /// Input: Mock IAccountPlugin not implementing IHaveScrobbleLimit and a default UserConfig value.
    /// Expected: SupportsScrobbleLimit is false and CurrentScrobbleCount is null; construction does not throw.
    /// </summary>
    [Test]
    public void AccountPluginViewModel_Constructor_PluginWithoutScrobbleLimit_NoSubscriptionAndNullCurrentCount()
    {
        // Arrange
        var pluginMock = new Mock<IAccountPlugin>(MockBehavior.Strict);
        // Ensure any accessed members on IAccountPlugin that the viewmodel might call are arranged as strict (none expected for this test).
        var configMock = new Mock<IWritableOptions<UserConfig>>(MockBehavior.Strict);
        configMock.SetupGet(c => c.Value).Returns(new UserConfig());

        // Act
        AccountPluginViewModel vm = null!;
        Assert.DoesNotThrow(() => vm = new AccountPluginViewModel(pluginMock.Object, configMock.Object));

        using (Assert.EnterMultipleScope())
        {
            // Assert
            // SupportsScrobbleLimit should be false when plugin does not implement IHaveScrobbleLimit
            Assert.That(vm.SupportsScrobbleLimit, Is.False, "Plugin not implementing IHaveScrobbleLimit should not support scrobble limits.");
            // CurrentScrobbleCount should be null when plugin does not implement the scrobble-limit interface
            Assert.That(vm.CurrentScrobbleCount, Is.Null, "CurrentScrobbleCount should be null for plugins without scrobble limit support.");
        }
    }

    /// <summary>
    /// Partial/inconclusive test for constructor wiring when plugin implements IHaveScrobbleLimit.
    /// Purpose: validate that the constructor subscribes to CurrentScrobbleCountChanged so that
    /// raising that event results in PropertyChanged for CurrentScrobbleCount.
    /// Input: a plugin that implements IHaveScrobbleLimit.
    /// Expected: raising CurrentScrobbleCountChanged causes the viewmodel to raise PropertyChanged for CurrentScrobbleCount.
    ///
    /// NOTE: This test is marked Inconclusive because the exact delegate signature of
    /// IHaveScrobbleLimit.CurrentScrobbleCountChanged (EventHandler, EventHandler<int>, etc.) is not available
    /// in the provided scope for robustly setting up and raising the event via Moq. To complete this test:
    /// - Replace the Assert.Inconclusive call with actual event raising using Moq.Raise with the correct delegate type,
    ///   e.g. scrobbleMock.Raise(m => m.CurrentScrobbleCountChanged += null, EventArgs.Empty) if the event is EventHandler,
    ///   or scrobbleMock.Raise(m => m.CurrentScrobbleCountChanged += null, 123) if the event uses an int payload, etc.
    /// - Then assert that a PropertyChanged event for nameof(AccountPluginViewModel.CurrentScrobbleCount) was observed.
    /// </summary>
    [Test]
    public void AccountPluginViewModel_Constructor_WithScrobbleLimitPlugin_SubscribesToEvent_PropertyChangedRaised_Partial()
    {
        // Arrange
        var pluginMock = new Mock<IAccountPlugin>(MockBehavior.Strict);
        // Add the IHaveScrobbleLimit behavior to the same mock so the tested constructor path executes.
        var scrobbleMock = pluginMock.As<IHaveScrobbleLimit>();
        // Provide a CurrentScrobbleCount getter so CurrentScrobbleCount property can be read if needed.
        scrobbleMock.SetupGet(s => s.CurrentScrobbleCount).Returns(123);

        var configMock = new Mock<IWritableOptions<UserConfig>>(MockBehavior.Strict);
        configMock.SetupGet(c => c.Value).Returns(new UserConfig());

        // Act
        var vm = new AccountPluginViewModel(pluginMock.Object, configMock.Object);

        // We want to verify that constructor subscribed to CurrentScrobbleCountChanged and that raising it
        // causes PropertyChanged(nameof(CurrentScrobbleCount)). However, the exact event handler delegate
        // type is not available in the provided scope for safe, compile-time correct invocation via Moq.Raise.
        // Therefore this test is left inconclusive with guidance above to complete it when the event signature is known.
        Assert.Inconclusive("Partial test: cannot raise IHaveScrobbleLimit.CurrentScrobbleCountChanged because its delegate signature is not available in the provided scope. See test XML comment for completion steps.");
    }

    /// <summary>
    /// Verifies that AccountPluginViewModel.IsAuthenticated returns the exact value provided by the underlying plugin.
    /// Input conditions: plugin.IsAuthenticated is set to both true and false (parameterized).
    /// Expected result: The view model returns the same boolean value as the plugin.
    /// </summary>
    /// <param name="pluginValue">The value the mocked plugin will return for IsAuthenticated.</param>
    [TestCase(true)]
    [TestCase(false)]
    public void IsAuthenticated_PluginReportsValue_ReturnsSame(bool pluginValue)
    {
        // Arrange
        var pluginMock = new Mock<IAccountPlugin>(MockBehavior.Strict);
        pluginMock.SetupGet(p => p.IsAuthenticated).Returns(pluginValue);

        var configMock = new Mock<IWritableOptions<UserConfig>>(MockBehavior.Strict);
        // Provide a simple UserConfig instance; tests here don't depend on its contents.
        configMock.SetupGet(c => c.Value).Returns(new UserConfig());

        var viewModel = new AccountPluginViewModel(pluginMock.Object, configMock.Object);

        // Act
        var result = viewModel.IsAuthenticated;

        // Assert
        Assert.That(result, Is.EqualTo(pluginValue));
    }

    /// <summary>
    /// Ensures accessing IsAuthenticated does not modify plugin state or throw exceptions.
    /// Input conditions: plugin.IsAuthenticated returns false.
    /// Expected result: Reading the property is safe and returns the value without invoking any plugin methods that could change state.
    /// </summary>
    [Test]
    public void IsAuthenticated_AccessDoesNotThrowOrChangeState()
    {
        // Arrange
        var pluginMock = new Mock<IAccountPlugin>(MockBehavior.Strict);
        pluginMock.SetupGet(p => p.IsAuthenticated).Returns(false);

        var configMock = new Mock<IWritableOptions<UserConfig>>(MockBehavior.Strict);
        configMock.SetupGet(c => c.Value).Returns(new UserConfig());

        var viewModel = new AccountPluginViewModel(pluginMock.Object, configMock.Object);

        // Act & Assert - reading should not throw and should return expected value
        Assert.DoesNotThrow(() =>
        {
            var value = viewModel.IsAuthenticated;
            Assert.That(value, Is.False);
        });

        // Verify that only the property getter was accessed (no other setups invoked)
        pluginMock.VerifyGet(p => p.IsAuthenticated, Times.AtLeastOnce);
    }

    /// <summary>
    /// Provides a variety of string values to validate the Name property behavior, including empty, whitespace,
    /// special characters, and a very long string to exercise boundary conditions of string handling.
    /// </summary>
    private static IEnumerable<string> NameTestCases()
    {
        yield return "NormalPluginName";
        yield return "";
        yield return "   ";
        yield return "SpecialChars_!@#$%^&*()\n\t\r";
        // Very long string (10k chars) to test boundary / performance-like behavior
        yield return new string('A', 10_000);
    }

    /// <summary>
    /// The Name property should return exactly what the underlying IAccountPlugin.Name provides.
    /// Input conditions: various non-null string values provided by the plugin mock (including empty, whitespace, special chars, and very long string).
    /// Expected result: AccountPluginViewModel.Name equals the plugin's Name for each provided input.
    /// </summary>
    /// <param name="pluginName">The plugin Name returned by the mocked IAccountPlugin.</param>
    [TestCaseSource(nameof(NameTestCases))]
    public void Name_ReturnsUnderlyingPluginName_ForVariousStrings(string pluginName)
    {
        // Arrange
        var pluginMock = new Mock<IAccountPlugin>(MockBehavior.Strict);
        pluginMock.SetupGet(p => p.Name).Returns(pluginName);

        var configMock = new Mock<IWritableOptions<UserConfig>>(MockBehavior.Loose);

        var vm = new AccountPluginViewModel(pluginMock.Object, configMock.Object);

        // Act
        var result = vm.Name;

        // Assert
        Assert.That(result, Is.EqualTo(pluginName));
    }

    /// <summary>
    /// The Name property should reflect dynamic changes in the underlying plugin's Name getter if the plugin's implementation yields a changing value.
    /// Input conditions: plugin mock uses a backing variable for Name; the variable is updated after ViewModel creation.
    /// Expected result: AccountPluginViewModel.Name reflects both the initial and updated values.
    /// </summary>
    [Test]
    public void Name_ReflectsUnderlyingPluginNameChanges()
    {
        // Arrange
        string nameBacking = "InitialName";
        var pluginMock = new Mock<IAccountPlugin>(MockBehavior.Strict);
        // Return value using a lambda so changes to the backing field are observed
        pluginMock.SetupGet(p => p.Name).Returns(() => nameBacking);

        var configMock = new Mock<IWritableOptions<UserConfig>>(MockBehavior.Loose);

        var vm = new AccountPluginViewModel(pluginMock.Object, configMock.Object);

        // Act & Assert - initial value
        Assert.That(vm.Name, Is.EqualTo("InitialName"));

        // Act - change underlying value
        nameBacking = "UpdatedName";

        // Assert - ViewModel should reflect the updated plugin name
        Assert.That(vm.Name, Is.EqualTo("UpdatedName"));
    }

    /// <summary>
    /// Provides various plugin id values to validate PluginId forwarding.
    /// Includes empty, whitespace, special characters and a very long value.
    /// </summary>
    private static IEnumerable<string> IdTestCases()
    {
        yield return "plugin-1";
        yield return "";
        yield return "   ";
        yield return "spécial-çhârâctęrs-!@#$%^&*()";
        // very long value (constructed at runtime)
        yield return new string('x', 10000);
    }

    /// <summary>
    /// Provides IconUri test cases including null and a valid Uri.
    /// </summary>
    private static IEnumerable<Uri?> IconUriCases()
    {
        yield return null;
        yield return new Uri("http://example.com/icon.png");
    }

    /// <summary>
    /// Verifies that when the underlying plugin does NOT implement IHaveScrobbleLimit the
    /// view model returns null for ScrobbleLimit.
    /// Input conditions: plugin implements only IAccountPlugin (no IHaveScrobbleLimit).
    /// Expected result: AccountPluginViewModel.ScrobbleLimit is null.
    /// </summary>
    [Test]
    public void ScrobbleLimit_PluginDoesNotImplementIHaveScrobbleLimit_ReturnsNull()
    {
        // Arrange
        var pluginMock = new Mock<IAccountPlugin>(MockBehavior.Strict);
        var configMock = new Mock<IWritableOptions<UserConfig>>(MockBehavior.Strict);

        var viewModel = new AccountPluginViewModel(pluginMock.Object, configMock.Object);

        // Act
        var result = viewModel.ScrobbleLimit;

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Verifies that SupportsScrobbleLimit returns true when the provided IAccountPlugin implements IHaveScrobbleLimit,
    /// and false when it does not.
    /// Input conditions:
    /// - implementsLimit = true: plugin mock also implements IHaveScrobbleLimit
    /// - implementsLimit = false: plugin mock does not implement IHaveScrobbleLimit
    /// Expected result:
    /// - SupportsScrobbleLimit matches implementsLimit.
    /// </summary>
    /// <param name="implementsLimit">Whether the plugin should implement IHaveScrobbleLimit.</param>
    [TestCase(true)]
    [TestCase(false)]
    public void SupportsScrobbleLimit_PluginImplementsIHaveScrobbleLimit_ReturnsExpected(bool implementsLimit)
    {
        // Arrange
        var pluginMock = new Mock<IAccountPlugin>(MockBehavior.Strict);

        // If we need the mock to implement IHaveScrobbleLimit, cast the mock to that interface.
#pragma warning disable IDE0059 // Unnecessary assignment of a value
        Mock<IHaveScrobbleLimit>? scrobbleLimitMock = null;
        if (implementsLimit)
        {
            scrobbleLimitMock = pluginMock.As<IHaveScrobbleLimit>();
            // Setup nothing else; presence of the interface implementation is sufficient for the 'is' check.
        }
#pragma warning restore IDE0059 // Unnecessary assignment of a value

        var configMock = new Mock<IWritableOptions<UserConfig>>(MockBehavior.Strict);

        // Act
        var vm = new AccountPluginViewModel(pluginMock.Object, configMock.Object);
        var result = vm.SupportsScrobbleLimit;

        // Assert
        Assert.That(result, Is.EqualTo(implementsLimit));
    }

    /// <summary>
    /// Verifies that the IsScrobblingEnabled getter reflects the plugin state, and that the setter:
    /// - updates the underlying plugin property only when the value changes,
    /// - raises PropertyChanged for IsScrobblingEnabled only when a change occurs.
    ///
    /// Input conditions:
    /// - initial: the plugin's starting IsScrobblingEnabled value.
    /// - setValue: the value assigned to AccountPluginViewModel.IsScrobblingEnabled.
    ///
    /// Expected outcome:
    /// - expectedSetCalls: number of times the plugin setter is invoked (0 or 1).
    /// - expectedPropertyChanged: number of PropertyChanged events raised for IsScrobblingEnabled (0 or 1).
    /// </summary>
    [TestCase(true, true, 0, 0)]
    [TestCase(true, false, 1, 1)]
    [TestCase(false, false, 0, 0)]
    [TestCase(false, true, 1, 1)]
    public void IsScrobblingEnabled_SetValue_UpdatesPluginAndNotifies_WhenAppropriate(
        bool initial,
        bool setValue,
        int expectedSetCalls,
        int expectedPropertyChanged)
    {
        // Arrange
        var pluginMock = new Mock<IAccountPlugin>(MockBehavior.Strict);

        // Manage a backing variable to simulate plugin property behavior.
        bool pluginBacking = initial;
        int setCalls = 0;

        pluginMock.SetupGet(p => p.IsScrobblingEnabled).Returns(() => pluginBacking);
        pluginMock.SetupSet(p => p.IsScrobblingEnabled = It.IsAny<bool>())
                  .Callback<bool>(v =>
                  {
                      setCalls++;
                      pluginBacking = v;
                  });

        var configMock = new Mock<IWritableOptions<UserConfig>>(MockBehavior.Loose);

        var vm = new AccountPluginViewModel(pluginMock.Object, configMock.Object);

        int propertyChangedCount = 0;
        vm.PropertyChanged += (sender, args) =>
        {
            // Only count notifications for the IsScrobblingEnabled property.
            if (args?.PropertyName == nameof(AccountPluginViewModel.IsScrobblingEnabled))
            {
                propertyChangedCount++;
            }
        };

        // Act & Assert - verify initial getter reflects plugin state before any changes.
        Assert.That(vm.IsScrobblingEnabled, Is.EqualTo(initial), "Getter should reflect the plugin initial state.");

        // Act - set the property to the test value.
        vm.IsScrobblingEnabled = setValue;

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(setCalls, Is.EqualTo(expectedSetCalls), "Plugin setter invocation count mismatch.");
            Assert.That(propertyChangedCount, Is.EqualTo(expectedPropertyChanged), "PropertyChanged invocation count mismatch.");
            // ViewModel getter should reflect the plugin backing value after operation.
            Assert.That(vm.IsScrobblingEnabled, Is.EqualTo(pluginBacking), "ViewModel getter should reflect plugin backing value after set.");
        }

        // Verify mock expectations where strict behavior applies.
        pluginMock.VerifyGet(p => p.IsScrobblingEnabled, Times.AtLeastOnce);
        pluginMock.VerifySet(p => p.IsScrobblingEnabled = It.IsAny<bool>(), Times.Exactly(expectedSetCalls));
    }

}
