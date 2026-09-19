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
            .Add(x => x.WithInputRoot, false));

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
            .Add(x => x.WithInputRoot, true));

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
        Assert.IsAssignableFrom<DotNetObjectReference<TwPopoverPickerComponentBase>>(invocation.Arguments[3]);
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
