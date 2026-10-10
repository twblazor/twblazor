using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using TwBlazor.Components;

namespace TwBlazor.Tests;

public class TwPopoverPickerComponentBaseTests : TwBlazorTestBase
{
    public TwPopoverPickerComponentBaseTests()
    {
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void TriggerInputRef_DefaultsToNull_WhenNotOverriddenByDerivedPicker()
    {
        // Arrange & Act - TwDatePicker/TwTimePicker both override triggerInputRef with a real
        // element; a picker that doesn't must fall back to the base class's own default.
        var cut = TestContext.Render<TestPopoverPickerComponent>();

        // Assert
        Assert.Null(cut.Instance.TriggerInputRef);
    }

    [Fact]
    public async Task OnIconClickAsync_FocusesDefaultElementReference_WhenNoTriggerAndNoInputRoot()
    {
        // Arrange - with no derived triggerInputRef and no rendered InputRoot, OnIconClickAsync's
        // fallback chain (triggerInputRef ?? InputRoot?.RootRef ?? default) bottoms out at a
        // default(ElementReference) rather than throwing.
        var cut = TestContext.Render<TestPopoverPickerComponent>(p => p
            .Add(x => x.WithInputRoot, false)
            .Add(x => x.ReadOnly, true));

        // Act
        await cut.Instance.ClickIconAsync();

        // Assert
        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.focusSurface");
        Assert.IsType<ElementReference>(invocation.Arguments[0]);
    }

    [Fact]
    public async Task OnIconClickAsync_FocusesInputRootSurface_WhenNoTriggerButInputRootIsRendered()
    {
        // Arrange - with no derived triggerInputRef but a rendered InputRoot, the fallback should
        // focus the InputRoot's own surface rather than a default(ElementReference).
        var cut = TestContext.Render<TestPopoverPickerComponent>(p => p
            .Add(x => x.WithInputRoot, true)
            .Add(x => x.ReadOnly, true));

        // Act
        await cut.Instance.ClickIconAsync();

        // Assert
        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.focusSurface");
        Assert.IsType<ElementReference>(invocation.Arguments[0]);
    }

    [Fact]
    public async Task OnIconClickAsync_DoesNothing_WhenDisabled()
    {
        // Arrange
        var cut = TestContext.Render<TestPopoverPickerComponent>(p => p
            .Add(x => x.Disabled, true));

        // Act
        await cut.Instance.ClickIconAsync();

        // Assert
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.focusSurface");
    }

    [Fact]
    public async Task ReleasePanelTrapAsync_UnregistersScrollReposition_ForEveryDerivedPicker()
    {
        // Regression test: this is the one shared cleanup path every popover picker (date, date
        // range, date-time range, time, time range, select, color) closes through, so unregistering
        // the scroll/resize listener registered alongside twPicker.positionPanelFixed belongs here
        // once rather than being duplicated per picker.
        var cut = TestContext.Render<TestPopoverPickerComponent>();

        await cut.Instance.ReleaseTrapAsync();

        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.unregisterScrollReposition");
    }

    [Fact]
    public async Task RegisterPanelScrollBehaviorAsync_PassesADotNetObjectReference_SoJsCanCloseOnScroll()
    {
        // Regression test: twPicker.registerScrollReposition's desktop close-on-scroll path needs a
        // callable reference back to this component to invoke its JSInvokable Close() method - this
        // is the one shared place (used by every popover picker) that supplies it, so it belongs
        // here once rather than being duplicated per picker.
        var cut = TestContext.Render<TestPopoverPickerComponent>();
        var panel = new ElementReference();

        await cut.Instance.RegisterScrollBehaviorAsync(panel);

        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.registerScrollReposition");
        Assert.IsType<ElementReference>(invocation.Arguments[1]);
        Assert.False((bool)invocation.Arguments[2]!);
        Assert.IsType<DotNetObjectReference<TwPopoverPickerComponentBase>>(invocation.Arguments[3], exactMatch: false);
    }

    [Fact]
    public async Task RegisterPanelScrollBehaviorAsync_ForwardsMatchAnchorWidth()
    {
        // TwSelect's options panel needs matchAnchorWidth=true so it spans the trigger's width -
        // see the remarks on TwInputTheme.SelectPanelPosition.
        var cut = TestContext.Render<TestPopoverPickerComponent>();
        var panel = new ElementReference();

        await cut.Instance.RegisterScrollBehaviorAsync(panel, matchAnchorWidth: true);

        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.registerScrollReposition");
        Assert.True((bool)invocation.Arguments[2]!);
    }

    private static IEnumerable<string> InertCalls(BunitContext context) => context.JSInterop.Invocations
        .Select(i => i.Identifier)
        .Where(id => id is "twDialog.setBackgroundInert" or "twDialog.clearBackgroundInert");

    [Fact]
    public async Task ApplyPanelTrapAsync_TrapsFocusInPanel_ThenMakesBackgroundInert()
    {
        var cut = TestContext.Render<TestPopoverPickerComponent>();

        await cut.Instance.ApplyTrapAsync(new ElementReference());

        var identifiers = TestContext.JSInterop.Invocations.Select(i => i.Identifier).ToList();
        Assert.True(identifiers.IndexOf("twDialog.trapFocus") < identifiers.IndexOf("twDialog.setBackgroundInert"));
        Assert.Contains("twDialog.trapFocus", identifiers);
    }

    [Fact]
    public async Task ClosedPanel_WhoseTrapWasReArmedDuringRelease_HasInertLiftedOnTheNextRender()
    {
        // Regression test: picking a date releases the trap (several awaited JS calls) while the
        // component re-renders with the panel still open, and that render re-armed the trap after the
        // release had cleared it. Nothing then lifted it, leaving a dialog's Save/Cancel buttons inert.
        var cut = TestContext.Render<TestPopoverPickerComponent>();
        var panel = new ElementReference();
        cut.Instance.SetOpen(true);
        await cut.Instance.ApplyTrapAsync(panel);

        await cut.Instance.ReleaseTrapAsync();
        await cut.Instance.ApplyTrapAsync(panel); // the interleaved render's late re-arm
        cut.Instance.SetOpen(false);
        await cut.Instance.RerenderAsync();

        Assert.Equal("twDialog.clearBackgroundInert", InertCalls(TestContext).Last());
    }

    [Fact]
    public async Task OpenPanel_IsNotClearedByARender()
    {
        var cut = TestContext.Render<TestPopoverPickerComponent>();
        cut.Instance.SetOpen(true);
        await cut.Instance.ApplyTrapAsync(new ElementReference());
        var before = InertCalls(TestContext).Count();

        await cut.Instance.RerenderAsync();

        Assert.DoesNotContain("twDialog.clearBackgroundInert", InertCalls(TestContext).Skip(before));
    }

    [Fact]
    public async Task ClosedPanel_NeverArmed_DoesNotTouchInert()
    {
        var cut = TestContext.Render<TestPopoverPickerComponent>();
        var before = InertCalls(TestContext).Count();

        await cut.Instance.RerenderAsync();

        Assert.Empty(InertCalls(TestContext).Skip(before));
    }

    [Fact]
    public async Task ClosedPanel_HasInertLiftedOnce_NotOnEveryLaterRender()
    {
        var cut = TestContext.Render<TestPopoverPickerComponent>();
        cut.Instance.SetOpen(true);
        await cut.Instance.ApplyTrapAsync(new ElementReference());
        cut.Instance.SetOpen(false);
        await cut.Instance.RerenderAsync();
        var before = InertCalls(TestContext).Count();

        await cut.Instance.RerenderAsync();

        Assert.Empty(InertCalls(TestContext).Skip(before));
    }
}

/// <summary>
/// Minimal concrete picker used only to exercise <see cref="TwPopoverPickerComponentBase"/>'s own
/// default behaviour (its <c>triggerInputRef</c> default and <c>OnIconClickAsync</c> fallback chain)
/// independently of any real derived picker, which always supplies its own trigger element.
/// </summary>
public class TestPopoverPickerComponent : TwPopoverPickerComponentBase
{
    [Parameter] public bool WithInputRoot { get; set; } = true;

    public ElementReference? TriggerInputRef => triggerInputRef;

    public Task ClickIconAsync() => OnIconClickAsync();

    public Task ReleaseTrapAsync() => ReleasePanelTrapAsync();

    public Task ApplyTrapAsync(ElementReference panel) => ApplyPanelTrapAsync(panel);

    public void SetOpen(bool open) => isFocused = open;

    public Task RerenderAsync() => InvokeAsync(StateHasChanged);

    public Task RegisterScrollBehaviorAsync(ElementReference panel, bool matchAnchorWidth = false) =>
        RegisterPanelScrollBehaviorAsync(panel, matchAnchorWidth);

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (WithInputRoot)
        {
            builder.OpenComponent<TwInputRoot>(0);
            builder.AddComponentReferenceCapture(1, instance => InputRoot = (TwInputRoot)instance);
            builder.CloseComponent();
        }
    }
}
