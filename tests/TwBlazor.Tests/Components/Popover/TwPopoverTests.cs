using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Models;

namespace TwBlazor.Tests.Components.Popover;

public class TwPopoverTests : TwBlazorTestBase
{
    private TwPopoverTheme theme => Theme.Components.Require<TwOverlayTheme>().Popover;

    private IRenderedComponent<TwPopover> Render(Action<ComponentParameterCollectionBuilder<TwPopover>>? configure = null) =>
        TestContext.Render<TwPopover>(p =>
        {
            p.Add(x => x.Id, "pop");
            p.Add(x => x.Label, "Share");
            p.Add(x => x.ChildContent, "Panel content");
            configure?.Invoke(p);
        });

    private static object? Option(JSRuntimeInvocation invocation, string name) =>
        invocation.Arguments[4]!.GetType().GetProperty(name)!.GetValue(invocation.Arguments[4]);

    [Fact]
    public void TwPopover_Closed_RendersTheButtonTiedToThePanel()
    {
        var cut = Render();

        var trigger = cut.Find("#pop-trigger");
        Assert.Equal("dialog", trigger.GetAttribute("aria-haspopup"));
        Assert.Equal("false", trigger.GetAttribute("aria-expanded"));
        Assert.Equal("pop-panel", trigger.GetAttribute("aria-controls"));
        Assert.Contains("Share", trigger.TextContent);
        Assert.Empty(cut.FindAll("#pop-panel"));
    }

    [Fact]
    public void TwPopover_Root_HasTheThemeAndCustomClasses()
    {
        var cut = Render(p => p.Add(x => x.Class, "custom-root"));

        var root = cut.Find("#pop");
        Assert.Contains(theme.Root.Split(' ')[0], root.GetAttribute("class"));
        Assert.Contains("custom-root", root.GetAttribute("class"));
    }

    [Fact]
    public async Task TwPopover_Click_OpensANonModalDialog_AndAsksTheScriptToPlaceIt()
    {
        var opened = new List<bool>();
        var cut = Render(p => p.Add(x => x.OpenChanged, value => opened.Add(value)));

        cut.Find("#pop-trigger").Click();

        var panel = await cut.WaitForElementAsync("#pop-panel");
        Assert.Equal("dialog", panel.GetAttribute("role"));
        Assert.Null(panel.GetAttribute("aria-modal"));
        Assert.Equal("pop-trigger", panel.GetAttribute("aria-labelledby"));
        Assert.Equal("-1", panel.GetAttribute("tabindex"));
        Assert.True(panel.HasAttribute("data-tw-popover"));
        Assert.Contains("Panel content", panel.TextContent);
        Assert.Contains(theme.Panel.Split(' ')[0], panel.GetAttribute("class"));
        Assert.Contains(theme.Background.Split(' ')[0], panel.GetAttribute("class"));
        Assert.Equal("true", cut.Find("#pop-trigger").GetAttribute("aria-expanded"));
        Assert.Equal([true], opened);
        Assert.True(cut.Instance.IsOpen);

        var open = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twPopover.open");
        Assert.Equal("bottom", Option(open, "placement"));
        Assert.Equal("start", Option(open, "align"));
        Assert.Equal(8, Option(open, "offset"));
        Assert.True((bool)Option(open, "autoFocus")!);
        Assert.True((bool)Option(open, "closeOnOutsideClick")!);
    }

    [Fact]
    public async Task TwPopover_PassesItsPlacementAndOptionsToTheScript()
    {
        var cut = Render(p => p
            .Add(x => x.Placement, PopoverPlacement.End)
            .Add(x => x.Align, PopoverAlign.Center)
            .Add(x => x.Offset, 12)
            .Add(x => x.AutoFocus, false)
            .Add(x => x.CloseOnOutsideClick, false));

        cut.Find("#pop-trigger").Click();

        await cut.WaitForAssertionAsync(() =>
        {
            var open = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twPopover.open");
            Assert.Equal("end", Option(open, "placement"));
            Assert.Equal("center", Option(open, "align"));
            Assert.Equal(12, Option(open, "offset"));
            Assert.False((bool)Option(open, "autoFocus")!);
            Assert.False((bool)Option(open, "closeOnOutsideClick")!);
        });
    }

    [Fact]
    public async Task TwPopover_SecondClick_ClosesThePanel_AndReturnsFocusToTheButton()
    {
        var changes = new List<bool>();
        var cut = Render(p => p.Add(x => x.OpenChanged, value => changes.Add(value)));
        cut.Find("#pop-trigger").Click();
        await cut.WaitForElementAsync("#pop-panel");

        cut.Find("#pop-trigger").Click();

        await cut.WaitForAssertionAsync(() => Assert.Empty(cut.FindAll("#pop-panel")));
        Assert.Equal([true, false], changes);
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twPopover.close");
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twMenu.focus");
    }

    [Fact]
    public async Task TwPopover_Title_HeadsAndNamesThePanel()
    {
        var cut = Render(p => p.Add(x => x.Title, "Share this page"));
        cut.Find("#pop-trigger").Click();

        var panel = await cut.WaitForElementAsync("#pop-panel");
        var title = cut.Find("#pop-title");
        Assert.Equal("H2", title.TagName);
        Assert.Equal("Share this page", title.TextContent);
        Assert.Equal(theme.Title, title.GetAttribute("class"));
        Assert.Equal("pop-title", panel.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public async Task TwPopover_CloseButton_ClosesThePanel()
    {
        var cut = Render(p => p.Add(x => x.ShowCloseButton, true));
        cut.Find("#pop-trigger").Click();
        await cut.WaitForElementAsync("#pop-panel");

        var close = cut.Find("#pop-panel button[aria-label='Close']");
        close.Click();

        await cut.WaitForAssertionAsync(() => Assert.Empty(cut.FindAll("#pop-panel")));
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twMenu.focus");
    }

    [Fact]
    public async Task TwPopover_NoCloseButton_ByDefault()
    {
        var cut = Render();
        cut.Find("#pop-trigger").Click();
        await cut.WaitForElementAsync("#pop-panel");

        Assert.Empty(cut.FindAll("#pop-panel button[aria-label='Close']"));
    }

    [Fact]
    public async Task TwPopover_Escape_ReturnsFocusToTheButton()
    {
        var cut = Render();
        cut.Find("#pop-trigger").Click();
        await cut.WaitForElementAsync("#pop-panel");

        await cut.InvokeAsync(() => cut.Instance.CloseFromEscape());

        Assert.Empty(cut.FindAll("#pop-panel"));
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twMenu.focus");
    }

    [Fact]
    public async Task TwPopover_OutsideClick_ClosesWithoutMovingFocus()
    {
        var closed = false;
        var cut = Render(p => p.Add(x => x.OpenChanged, value => closed = !value));
        cut.Find("#pop-trigger").Click();
        await cut.WaitForElementAsync("#pop-panel");

        await cut.InvokeAsync(() => cut.Instance.CloseFromOutsideClick());

        Assert.Empty(cut.FindAll("#pop-panel"));
        Assert.True(closed);
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twMenu.focus");
    }

    [Fact]
    public async Task TwPopover_FocusLeavingThePanel_ClosesWithoutMovingFocus()
    {
        var cut = Render();
        cut.Find("#pop-trigger").Click();
        await cut.WaitForElementAsync("#pop-panel");

        await cut.InvokeAsync(() => cut.Instance.CloseFromFocusOut());

        Assert.Empty(cut.FindAll("#pop-panel"));
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twMenu.focus");
    }

    [Fact]
    public async Task TwPopover_Disabled_DoesNotOpen()
    {
        var cut = Render(p => p.Add(x => x.Disabled, true));

        await cut.InvokeAsync(() => cut.Instance.OpenAsync());

        Assert.Empty(cut.FindAll("#pop-panel"));
        Assert.False(cut.Instance.IsOpen);
    }

    [Fact]
    public async Task TwPopover_OpenParameter_OpensAndClosesThePanel()
    {
        var changes = new List<bool>();
        var cut = Render(p => p.Add(x => x.Open, true).Add(x => x.OpenChanged, value => changes.Add(value)));
        await cut.WaitForElementAsync("#pop-panel");

        cut.Render(p => p.Add(x => x.Open, false));

        await cut.WaitForAssertionAsync(() => Assert.Empty(cut.FindAll("#pop-panel")));
        Assert.Equal([true], changes);
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twMenu.focus");
    }

    [Fact]
    public async Task TwPopover_ToggleAsync_OpensThenCloses()
    {
        var cut = Render();

        await cut.InvokeAsync(() => cut.Instance.ToggleAsync());
        Assert.True(cut.Instance.IsOpen);

        await cut.InvokeAsync(() => cut.Instance.ToggleAsync());
        Assert.False(cut.Instance.IsOpen);
    }

    [Fact]
    public void TwPopover_IconOnly_IsNamedByItsAriaLabel()
    {
        var cut = TestContext.Render<TwPopover>(p => p
            .Add(x => x.Id, "pop")
            .Add(x => x.Icon, Enums.Icon.Info_Circle)
            .Add(x => x.IconOnly, true)
            .Add(x => x.AriaLabel, "More information"));

        var trigger = cut.Find("#pop-trigger");
        Assert.Equal("More information", trigger.GetAttribute("aria-label"));
        Assert.Equal("dialog", trigger.GetAttribute("aria-haspopup"));
    }

    [Fact]
    public async Task TwPopover_TriggerContent_GetsTheAttributes_AndCanToggleThePanel()
    {
        PopoverTriggerContext? captured = null;
        var cut = Render(p => p.Add(x => x.TriggerContent, context => builder =>
        {
            captured = context;
            builder.OpenElement(0, "button");
            builder.AddMultipleAttributes(1, context.Attributes);
            builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, context.ToggleAsync));
            builder.AddContent(3, "Custom");
            builder.CloseElement();
        }));

        var trigger = cut.Find("button#pop-trigger");
        Assert.Equal("dialog", trigger.GetAttribute("aria-haspopup"));
        Assert.Equal("pop-panel", trigger.GetAttribute("aria-controls"));
        Assert.False(captured!.IsOpen);

        trigger.Click();

        await cut.WaitForElementAsync("#pop-panel");
        Assert.True(captured!.IsOpen);
        Assert.Equal("true", cut.Find("button#pop-trigger").GetAttribute("aria-expanded"));
    }

    [Fact]
    public async Task TwPopover_PanelClass_IsAddedToThePanel()
    {
        var cut = Render(p => p.Add(x => x.PanelClass, "w-96"));
        cut.Find("#pop-trigger").Click();

        var panel = await cut.WaitForElementAsync("#pop-panel");
        Assert.Contains("w-96", panel.GetAttribute("class"));
    }

    [Fact]
    public async Task TwPopover_Dispose_WhileOpen_ReleasesItsListeners()
    {
        var cut = Render();
        cut.Find("#pop-trigger").Click();
        await cut.WaitForElementAsync("#pop-panel");

        await cut.Instance.DisposeAsync();

        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twPopover.close");
    }
}
