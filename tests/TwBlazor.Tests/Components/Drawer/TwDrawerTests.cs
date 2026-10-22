using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Drawer;

public class TwDrawerTests : TwBlazorTestBase
{
    private TwDrawerTheme theme => Theme.Components.Require<TwDrawerTheme>();

    private IRenderedComponent<TwDrawer> Render(Action<ComponentParameterCollectionBuilder<TwDrawer>>? configure = null) =>
        TestContext.Render<TwDrawer>(p =>
        {
            p.Add(x => x.Id, "drawer");
            p.Add(x => x.Title, "Edit profile");
            p.Add(x => x.ChildContent, "Drawer content");
            configure?.Invoke(p);
        });

    private static object? Option(JSRuntimeInvocation invocation, string name) =>
        invocation.Arguments[3]!.GetType().GetProperty(name)!.GetValue(invocation.Arguments[3]);

    private static string FirstClass(string classes) => classes.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

    [Fact]
    public void TwDrawer_Closed_RendersAnInertHiddenPanelWithoutItsContent()
    {
        var cut = Render();

        var panel = cut.Find("#drawer");
        Assert.Equal("closed", panel.GetAttribute("data-state"));
        Assert.True(panel.HasAttribute("inert"));
        Assert.DoesNotContain("Drawer content", panel.TextContent);
        Assert.Equal("closed", cut.Find("[aria-hidden='true']").GetAttribute("data-state"));
        Assert.False(cut.Instance.IsOpen);
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twDrawer.open");
    }

    [Fact]
    public void TwDrawer_Open_RendersAModalDialogNamedAndDescribedByItsHeader()
    {
        var cut = Render(p => p
            .Add(x => x.Open, true)
            .Add(x => x.Description, "Changes are public."));

        var panel = cut.Find("#drawer");
        Assert.Equal("dialog", panel.GetAttribute("role"));
        Assert.Equal("true", panel.GetAttribute("aria-modal"));
        Assert.Equal("open", panel.GetAttribute("data-state"));
        Assert.False(panel.HasAttribute("inert"));
        Assert.Equal("-1", panel.GetAttribute("tabindex"));
        Assert.Equal("drawer-title", panel.GetAttribute("aria-labelledby"));
        Assert.Equal("drawer-description", panel.GetAttribute("aria-describedby"));
        Assert.Null(panel.GetAttribute("aria-label"));
        Assert.Equal("Edit profile", cut.Find("h2#drawer-title").TextContent);
        Assert.Equal("Changes are public.", cut.Find("#drawer-description").TextContent);
        Assert.Contains("Drawer content", panel.TextContent);
        Assert.True(cut.Instance.IsOpen);
    }

    [Fact]
    public void TwDrawer_Open_AsksTheScriptToTrapFocusAndHandleEscape()
    {
        Render(p => p.Add(x => x.Open, true));

        var open = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twDrawer.open");
        Assert.Equal(true, Option(open, "modal"));
        Assert.Equal(true, Option(open, "closeOnEscape"));
    }

    [Fact]
    public void TwDrawer_NonModal_HasNoBackdropOrAriaModal()
    {
        var cut = Render(p => p
            .Add(x => x.Open, true)
            .Add(x => x.Modal, false));

        Assert.Null(cut.Find("#drawer").GetAttribute("aria-modal"));
        Assert.Empty(cut.FindAll("[aria-hidden='true'][data-state]"));
        var open = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twDrawer.open");
        Assert.Equal(false, Option(open, "modal"));
    }

    [Fact]
    public void TwDrawer_WithoutTitle_FallsBackToAGenericName()
    {
        var cut = TestContext.Render<TwDrawer>(p => p
            .Add(x => x.Id, "drawer")
            .Add(x => x.Open, true));

        var panel = cut.Find("#drawer");
        Assert.Null(panel.GetAttribute("aria-labelledby"));
        Assert.False(string.IsNullOrWhiteSpace(panel.GetAttribute("aria-label")));
        Assert.Empty(cut.FindAll("h2"));
    }

    [Fact]
    public void TwDrawer_AriaLabel_TakesPrecedenceOverTheTitle()
    {
        var cut = Render(p => p
            .Add(x => x.AriaLabel, "Profile editor")
            .Add(x => x.Open, true));

        var panel = cut.Find("#drawer");
        Assert.Equal("Profile editor", panel.GetAttribute("aria-label"));
        Assert.Null(panel.GetAttribute("aria-labelledby"));
    }

    [Theory]
    [InlineData(DrawerPosition.Start)]
    [InlineData(DrawerPosition.End)]
    [InlineData(DrawerPosition.Top)]
    [InlineData(DrawerPosition.Bottom)]
    public void TwDrawer_Position_AppliesThatEdgesClasses(DrawerPosition position)
    {
        var cut = Render(p => p.Add(x => x.Position, position));

        var expected = position switch
        {
            DrawerPosition.Start => theme.Start,
            DrawerPosition.Top => theme.Top,
            DrawerPosition.Bottom => theme.Bottom,
            _ => theme.End
        };
        var classes = cut.Find("#drawer").GetAttribute("class")!;
        Assert.Contains(FirstClass(expected), classes);
        Assert.Contains(FirstClass(theme.Panel), classes);
    }

    [Fact]
    public void TwDrawer_DefaultsToTheEndEdgeAndMediumWidth()
    {
        var cut = Render();

        var classes = cut.Find("#drawer").GetAttribute("class")!;
        Assert.Contains("end-0", classes);
        Assert.Contains("max-w-md", classes);
    }

    [Theory]
    [InlineData(DrawerPosition.End, DrawerSize.Small, "max-w-xs")]
    [InlineData(DrawerPosition.Start, DrawerSize.Large, "max-w-2xl")]
    [InlineData(DrawerPosition.Bottom, DrawerSize.Medium, "max-h-[70dvh]")]
    [InlineData(DrawerPosition.Top, DrawerSize.Full, "h-dvh")]
    public void TwDrawer_Size_SetsWidthOnSidesAndHeightOnTopAndBottom(DrawerPosition position, DrawerSize size, string expected)
    {
        var cut = Render(p => p
            .Add(x => x.Position, position)
            .Add(x => x.Size, size));

        Assert.Contains(expected, cut.Find("#drawer").GetAttribute("class")!.Split(' '));
    }

    [Fact]
    public void TwDrawer_CustomClasses_AreAppliedToPanelBackdropAndBody()
    {
        var cut = Render(p => p
            .Add(x => x.Open, true)
            .Add(x => x.Class, "custom-panel")
            .Add(x => x.BackdropClass, "custom-backdrop")
            .Add(x => x.ContentClass, "custom-body"));

        Assert.Contains("custom-panel", cut.Find("#drawer").GetAttribute("class"));
        Assert.Single(cut.FindAll(".custom-backdrop"));
        Assert.Contains("Drawer content", cut.Find(".custom-body").TextContent);
    }

    [Fact]
    public void TwDrawer_FooterContent_RendersBelowTheBody()
    {
        var cut = Render(p => p
            .Add(x => x.Open, true)
            .Add(x => x.FooterContent, "<button>Save</button>"));

        var footer = cut.Find("#drawer > div:last-child");
        Assert.Contains(FirstClass(theme.Footer), footer.GetAttribute("class"));
        Assert.Equal("Save", footer.TextContent);
    }

    [Fact]
    public async Task TwDrawer_CloseButton_ClosesAndRaisesOpenChanged()
    {
        var changes = new List<bool>();
        var cut = Render(p => p
            .Add(x => x.Open, true)
            .Add(x => x.OpenChanged, value => changes.Add(value)));

        cut.Find("[data-tw-drawer-close]").Click();

        await cut.WaitForAssertionAsync(() => Assert.Equal("closed", cut.Find("#drawer").GetAttribute("data-state")));
        Assert.Equal([false], changes);
        Assert.False(cut.Instance.IsOpen);
        var close = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twDrawer.close");
        Assert.Equal(true, close.Arguments[1]);
    }

    [Fact]
    public void TwDrawer_CloseButton_HasAnAccessibleName()
    {
        var cut = Render(p => p.Add(x => x.Open, true));

        Assert.False(string.IsNullOrWhiteSpace(cut.Find("[data-tw-drawer-close]").GetAttribute("aria-label")));
    }

    [Fact]
    public void TwDrawer_ShowCloseButtonFalse_HidesIt()
    {
        var cut = Render(p => p
            .Add(x => x.Open, true)
            .Add(x => x.ShowCloseButton, false));

        Assert.Empty(cut.FindAll("[data-tw-drawer-close]"));
    }

    [Fact]
    public async Task TwDrawer_BackdropClick_Closes()
    {
        var cut = Render(p => p.Add(x => x.Open, true));

        cut.Find("[aria-hidden='true'][data-state]").Click();

        await cut.WaitForAssertionAsync(() => Assert.False(cut.Instance.IsOpen));
    }

    [Fact]
    public void TwDrawer_BackdropClick_StaysOpen_WhenCloseOnBackdropClickIsOff()
    {
        var cut = Render(p => p
            .Add(x => x.Open, true)
            .Add(x => x.CloseOnBackdropClick, false));

        cut.Find("[aria-hidden='true'][data-state]").Click();

        Assert.True(cut.Instance.IsOpen);
    }

    [Fact]
    public async Task TwDrawer_CloseFromEscape_Closes()
    {
        var cut = Render(p => p.Add(x => x.Open, true));

        await cut.InvokeAsync(() => cut.Instance.CloseFromEscapeAsync());

        Assert.False(cut.Instance.IsOpen);
        Assert.Equal("closed", cut.Find("#drawer").GetAttribute("data-state"));
    }

    [Fact]
    public async Task TwDrawer_CloseFromEscape_StaysOpen_WhenCloseOnEscapeIsOff()
    {
        var cut = Render(p => p
            .Add(x => x.Open, true)
            .Add(x => x.CloseOnEscape, false));

        await cut.InvokeAsync(() => cut.Instance.CloseFromEscapeAsync());

        Assert.True(cut.Instance.IsOpen);
        var open = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twDrawer.open");
        Assert.Equal(false, Option(open, "closeOnEscape"));
    }

    [Fact]
    public async Task TwDrawer_OpenAndCloseFromCode_RaiseOpenChanged()
    {
        var changes = new List<bool>();
        var cut = Render(p => p.Add(x => x.OpenChanged, value => changes.Add(value)));

        await cut.InvokeAsync(() => cut.Instance.OpenAsync());
        Assert.Equal("open", cut.Find("#drawer").GetAttribute("data-state"));

        await cut.InvokeAsync(() => cut.Instance.ToggleAsync());
        Assert.Equal("closed", cut.Find("#drawer").GetAttribute("data-state"));

        Assert.Equal([true, false], changes);
    }

    [Fact]
    public void TwDrawer_OpenParameter_OpensAndClosesWithoutEchoingOpenChanged()
    {
        var changes = new List<bool>();
        var cut = Render(p => p
            .Add(x => x.Open, true)
            .Add(x => x.OpenChanged, value => changes.Add(value)));

        cut.Render(p => p.Add(x => x.Open, false));

        Assert.False(cut.Instance.IsOpen);
        Assert.Empty(changes);
        Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twDrawer.close");
    }

    [Fact]
    public void TwDrawer_AfterClosing_KeepsItsContent()
    {
        var cut = Render(p => p.Add(x => x.Open, true));

        cut.Render(p => p.Add(x => x.Open, false));

        Assert.Contains("Drawer content", cut.Find("#drawer").TextContent);
    }

    [Fact]
    public async Task TwDrawer_Dispose_ReleasesTheScriptWithoutMovingFocus()
    {
        var cut = Render(p => p.Add(x => x.Open, true));

        await cut.Instance.DisposeAsync();

        var close = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twDrawer.close");
        Assert.Equal(false, close.Arguments[1]);
    }

    [Fact]
    public async Task TwDrawer_Dispose_DoesNothing_WhenNeverOpened()
    {
        var cut = Render();

        await cut.Instance.DisposeAsync();

        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twDrawer.close");
    }
}
