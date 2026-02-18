using System.ComponentModel;
using Moq;
using Scrubbler.Abstractions.Plugin;
using Scrubbler.Abstractions.Services;
using Scrubbler.Host.Presentation.Plugins;
using Scrubbler.Host.Services;

namespace Scrubbler.Test.Presentation.Plugins;

/// <summary>
/// Tests for Scrubbler.Host.Presentation.Plugins.ScrobblePluginHostViewModel focusing on the ShowScrobbleBar property.
/// </summary>
public partial class ScrobblePluginHostViewModelTests
{
    /// <summary>
    /// Verifies that ShowScrobbleBar returns the current PluginViewModel.ReadyForScrobbling value.
    /// Input conditions: Plugin view model ReadyForScrobbling set to true/false.
    /// Expected result: ShowScrobbleBar equals the provided ReadyForScrobbling value.
    /// </summary>
    /// <param name="readyForScrobbling">Initial ReadyForScrobbling value on the plugin view model.</param>
    [TestCase(true)]
    [TestCase(false)]
    public void ShowScrobbleBar_WhenUnderlyingReadyForScrobblingIsSet_ReturnsSameValue(bool readyForScrobbling)
    {
        // Arrange
        var pluginVmMock = new Mock<IScrobblePluginViewModel>();
        // The interface exposes read-only properties; use SetupGet to provide values.
        pluginVmMock.SetupGet(p => p.ReadyForScrobbling).Returns(readyForScrobbling);
        pluginVmMock.SetupGet(p => p.CanScrobble).Returns(false);
        pluginVmMock.SetupGet(p => p.IsBusy).Returns(false);

        var pluginMock = new Mock<IScrobblePlugin>();
        pluginMock.Setup(p => p.GetViewModel()).Returns(pluginVmMock.Object);

        var pluginManagerMock = new Mock<IPluginManager>();
        // Ensure the event exists so constructor can subscribe without issue.
        // No special setup required for the event itself.

        var feedbackMock = new Mock<IUserFeedbackService>();
        var dialogMock = new Mock<IDialogService>();

        var sut = new ScrobblePluginHostViewModel(pluginMock.Object, pluginManagerMock.Object, feedbackMock.Object, dialogMock.Object);

        // Act
        var result = sut.ShowScrobbleBar;

        // Assert
        Assert.That(result, Is.EqualTo(readyForScrobbling));
    }

    /// <summary>
    /// Verifies IsBusy returns the logical OR of the base IsBusy (IsScrobbling) and the PluginViewModel.IsBusy.
    /// Test inputs: combinations of baseIsScrobbling and pluginIsBusy.
    /// Expected result: IsBusy equals (baseIsScrobbling || pluginIsBusy).
    /// </summary>
    /// <param name="baseIsScrobbling">Value to set on the generated IsScrobbling property from PluginHostViewModelBase.</param>
    /// <param name="pluginIsBusy">Value returned by the IScrobblePluginViewModel.IsBusy property.</param>
    /// <param name="expected">Expected result for ScrobblePluginHostViewModel.IsBusy.</param>
    [TestCase(false, false, false)]
    [TestCase(true, false, true)]
    [TestCase(false, true, true)]
    [TestCase(true, true, true)]
    public void IsBusy_BaseAndPluginBusyStates_CombinedWithOr(bool baseIsScrobbling, bool pluginIsBusy, bool expected)
    {
        // Arrange
        // Create a mock IScrobblePluginViewModel. It typically implements INotifyPropertyChanged;
        // Moq will generate the event support if the interface requires it.
        var pluginViewModelMock = new Mock<IScrobblePluginViewModel>(MockBehavior.Strict);
        pluginViewModelMock.SetupGet(p => p.IsBusy).Returns(pluginIsBusy);

        // The plugin returns the view model instance via GetViewModel().
        var pluginMock = new Mock<IScrobblePlugin>(MockBehavior.Strict);
        pluginMock.Setup(p => p.GetViewModel()).Returns(pluginViewModelMock.Object);

        var pluginManagerMock = new Mock<IPluginManager>(MockBehavior.Strict);
        var feedbackMock = new Mock<IUserFeedbackService>(MockBehavior.Strict);
        var dialogServiceMock = new Mock<IDialogService>(MockBehavior.Strict);

        // Act
        var vm = new ScrobblePluginHostViewModel(
            pluginMock.Object,
            pluginManagerMock.Object,
            feedbackMock.Object,
            dialogServiceMock.Object)
        {
            // Set the generated IsScrobbling property on the base class to simulate base.IsBusy.
            // The CommunityToolkit.Mvvm ObservableProperty source generator creates a public IsScrobbling property.
            IsScrobbling = baseIsScrobbling
        };

        var actual = vm.IsBusy;

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }

    /// <summary>
    /// Verifies that the constructor assigns PluginViewModel, subscribes to PluginViewModel.PropertyChanged
    /// and to IPluginManager.IsAnyAccountPluginScrobblingChanged such that:
    /// - Raising PropertyChanged with CanScrobble calls NotifyCanExecuteChanged on both ScrobbleCommand and PreviewCommand
    /// - Raising PropertyChanged with ReadyForScrobbling raises PropertyChanged for ShowScrobbleBar on the viewmodel
    /// - Raising PropertyChanged with IsBusy raises PropertyChanged for IsBusy on the viewmodel
    /// - Raising manager.IsAnyAccountPluginScrobblingChanged triggers ScrobbleCommand.NotifyCanExecuteChanged
    /// Input conditions: valid IScrobblePlugin whose GetViewModel returns an IScrobblePluginViewModel mock; valid manager/feedback/dialog mocks.
    /// Expected result: assignments and event notifications occur as described.
    /// </summary>
    [Test]
    public void Constructor_ValidPlugin_SubscribesToEventsAndAssignsPluginViewModel()
    {
        // Arrange
        var pluginViewModelMock = new Mock<IScrobblePluginViewModel>(MockBehavior.Strict);
        // Setup basic getters used by properties (not strictly required for this test but kept explicit)
        pluginViewModelMock.SetupGet(p => p.CanScrobble).Returns(true);
        pluginViewModelMock.SetupGet(p => p.ReadyForScrobbling).Returns(true);
        pluginViewModelMock.SetupGet(p => p.IsBusy).Returns(false);

        var pluginMock = new Mock<IScrobblePlugin>(MockBehavior.Strict);
        pluginMock.Setup(p => p.GetViewModel()).Returns(pluginViewModelMock.Object);

        var managerMock = new Mock<IPluginManager>(MockBehavior.Strict);
        // No setup for events necessary; we'll raise them via Moq

        var feedbackMock = new Mock<IUserFeedbackService>(MockBehavior.Strict);
        var dialogServiceMock = new Mock<IDialogService>(MockBehavior.Strict);

        // Act
        var vm = new ScrobblePluginHostViewModel(pluginMock.Object, managerMock.Object, feedbackMock.Object, dialogServiceMock.Object);

        // Assert - PluginViewModel assigned
        Assert.That(vm.PluginViewModel, Is.SameAs(pluginViewModelMock.Object), "PluginViewModel should be assigned from plugin.GetViewModel().");

        // Arrange - subscribe to command CanExecuteChanged events to observe NotifyCanExecuteChanged calls
        int scrobbleCommandNotifications = 0;
        int previewCommandNotifications = 0;
        vm.ScrobbleCommand.CanExecuteChanged += (s, e) => scrobbleCommandNotifications++;
        vm.PreviewCommand.CanExecuteChanged += (s, e) => previewCommandNotifications++;

        // Act/Assert - raising CanScrobble PropertyChanged should notify both commands once
        pluginViewModelMock.Raise(p => p.PropertyChanged += null, new PropertyChangedEventArgs(nameof(IScrobblePluginViewModel.CanScrobble)));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(scrobbleCommandNotifications, Is.EqualTo(1), "ScrobbleCommand should be notified when CanScrobble changes.");
            Assert.That(previewCommandNotifications, Is.EqualTo(1), "PreviewCommand should be notified when CanScrobble changes.");
        }

        // Arrange - capture vm's PropertyChanged notifications
        string? lastVmPropertyChanged = null;
        vm.PropertyChanged += (s, e) => lastVmPropertyChanged = e.PropertyName;

        // Act - ReadyForScrobbling changed should raise ShowScrobbleBar property change on vm
        pluginViewModelMock.Raise(p => p.PropertyChanged += null, new PropertyChangedEventArgs(nameof(IScrobblePluginViewModel.ReadyForScrobbling)));
        Assert.That(lastVmPropertyChanged, Is.EqualTo(nameof(vm.ShowScrobbleBar)), "Changing ReadyForScrobbling should raise ShowScrobbleBar PropertyChanged on the VM.");

        // Act - IsBusy changed should raise IsBusy property change on vm
        lastVmPropertyChanged = null;
        pluginViewModelMock.Raise(p => p.PropertyChanged += null, new PropertyChangedEventArgs(nameof(IScrobblePluginViewModel.IsBusy)));
        Assert.That(lastVmPropertyChanged, Is.EqualTo(nameof(vm.IsBusy)), "Changing IsBusy on the plugin viewmodel should raise IsBusy PropertyChanged on the VM.");

        // Act/Assert - raising manager's IsAnyAccountPluginScrobblingChanged should notify ScrobbleCommand
        int scrobbleNotificationsFromManager = 0;
        vm.ScrobbleCommand.CanExecuteChanged += (s, e) => scrobbleNotificationsFromManager++;
        managerMock.Raise(m => m.IsAnyAccountPluginScrobblingChanged += null, EventArgs.Empty);
        Assert.That(scrobbleNotificationsFromManager, Is.GreaterThanOrEqualTo(1), "Raising IsAnyAccountPluginScrobblingChanged should call NotifyCanExecuteChanged on ScrobbleCommand.");
    }

    /// <summary>
    /// Verifies that the constructor throws a NullReferenceException when plugin.GetViewModel() does not return an IScrobblePluginViewModel
    /// (the code casts with 'as' and immediately subscribes to PluginViewModel.PropertyChanged without a null-check).
    /// Input conditions: IScrobblePlugin.GetViewModel returns null.
    /// Expected result: constructor throws NullReferenceException.
    /// </summary>
    [Test]
    public void Constructor_PluginGetViewModelReturnsNull_ThrowsNullReferenceException()
    {
        // Arrange
        var pluginMock = new Mock<IScrobblePlugin>(MockBehavior.Strict);
        // Return null intentionally to simulate a plugin that doesn't expose an IScrobblePluginViewModel
        pluginMock.Setup(p => p.GetViewModel()).Returns((IPluginViewModel?)null!);

        var managerMock = new Mock<IPluginManager>(MockBehavior.Strict);
        var feedbackMock = new Mock<IUserFeedbackService>(MockBehavior.Strict);
        var dialogServiceMock = new Mock<IDialogService>(MockBehavior.Strict);

        // Act & Assert
        Assert.Throws<NullReferenceException>(() =>
        {
            // This should throw when the constructor attempts to subscribe to PluginViewModel.PropertyChanged
            _ = new ScrobblePluginHostViewModel(pluginMock.Object, managerMock.Object, feedbackMock.Object, dialogServiceMock.Object);
        });
    }
}
