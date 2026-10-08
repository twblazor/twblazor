using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Tooltip;

public class TwTooltipTests : TwBlazorTestBase
{
    private TwPopoverTheme popoverTheme => Theme.Components.Require<TwOverlayTheme>().Popover;

    private TwTooltipTheme tooltipTheme => Theme.Components.Require<TwOverlayTheme>().Tooltip;

    private IRenderedComponent<TwTooltip> RenderTooltip(Action<ComponentParameterCollectionBuilder<TwTooltip>>? configure = null) =>
        TestContext.Render<TwTooltip>(parameters =>
        {
            parameters
                .Add(p => p.Text, "More detail")
                .AddChildContent("<p id=\"target\">Hover me</p>");
            configure?.Invoke(parameters);
        });

    #region Rendering

    [Fact]
    public void TwTooltip_RendersChildContent()
    {
        var cut = RenderTooltip();

        Assert.Equal("Hover me", cut.Find("#target").TextContent);
    }

    [Fact]
    public void TwTooltip_RendersTextInTooltipRoleElement()
    {
        var cut = RenderTooltip();

        Assert.Equal("More detail", cut.Find("[role='tooltip']").TextContent.Trim());
    }

    [Fact]
    public void TwTooltip_TooltipContent_TakesPrecedenceOverText()
    {
        var cut = RenderTooltip(p => p.Add(x => x.TooltipContent, (RenderFragment)(b => b.AddMarkupContent(0, "<strong>Rich</strong>"))));

        var tooltip = cut.Find("[role='tooltip']");
        Assert.NotNull(tooltip.QuerySelector("strong"));
        Assert.DoesNotContain("More detail", tooltip.TextContent);
    }

    [Fact]
    public void TwTooltip_WithNoTextOrContent_RendersNoBubbleOrAccessibilityAttributes()
    {
        var cut = TestContext.Render<TwTooltip>(p => p.AddChildContent("<p>Plain</p>"));

        var wrapper = cut.Find("div");
        Assert.Empty(cut.FindAll("[role='tooltip']"));
        Assert.Null(wrapper.GetAttribute("aria-describedby"));
        Assert.Null(wrapper.GetAttribute("tabindex"));
    }

    [Fact]
    public void TwTooltip_Disabled_RendersContentWithoutBubble()
    {
        var cut = RenderTooltip(p => p.Add(x => x.Disabled, true));

        Assert.Equal("Hover me", cut.Find("#target").TextContent);
        Assert.Empty(cut.FindAll("[role='tooltip']"));
        Assert.Null(cut.Find("div").GetAttribute("tabindex"));
    }

    #endregion

    #region Accessibility

    [Fact]
    public void TwTooltip_LinksWrapperToTooltipViaAriaDescribedBy()
    {
        var cut = RenderTooltip();

        var tooltip = cut.Find("[role='tooltip']");
        Assert.False(string.IsNullOrEmpty(tooltip.Id));
        Assert.Equal(tooltip.Id, cut.Find("div").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void TwTooltip_IsKeyboardFocusable_ByDefault()
    {
        var cut = RenderTooltip();

        Assert.Equal("0", cut.Find("div").GetAttribute("tabindex"));
    }

    [Fact]
    public void TwTooltip_Focusable_False_RemovesWrapperFromTabOrder()
    {
        var cut = RenderTooltip(p => p.Add(x => x.Focusable, false));

        Assert.Null(cut.Find("div").GetAttribute("tabindex"));
        Assert.NotNull(cut.Find("div").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void TwTooltip_RevealsOnHoverAndKeyboardFocus()
    {
        var classes = RenderTooltip().Find("[role='tooltip']").ClassList;

        Assert.Contains("invisible", classes);
        Assert.Contains("group-hover:visible", classes);
        Assert.Contains("group-focus-visible:visible", classes);
        Assert.Contains("group-has-[:focus-visible]:visible", classes);
    }

    [Fact]
    public void TwTooltip_CanBeDismissedWithEscape_AndReturnsOnMouseLeave()
    {
        var cut = RenderTooltip();

        cut.Find("div").KeyDown(new KeyboardEventArgs { Key = "Escape" });
        Assert.Contains("hidden", cut.Find("[role='tooltip']").ClassList);

        cut.Find("div").MouseLeave();
        Assert.DoesNotContain("hidden", cut.Find("[role='tooltip']").ClassList);
    }

    [Fact]
    public void TwTooltip_Dismissal_ResetsOnFocusOut()
    {
        var cut = RenderTooltip();

        cut.Find("div").KeyDown(new KeyboardEventArgs { Key = "Escape" });
        cut.Find("div").FocusOut();

        Assert.DoesNotContain("hidden", cut.Find("[role='tooltip']").ClassList);
    }

    [Fact]
    public void TwTooltip_IgnoresNonEscapeKeys()
    {
        var cut = RenderTooltip();

        cut.Find("div").KeyDown(new KeyboardEventArgs { Key = "a" });

        Assert.DoesNotContain("hidden", cut.Find("[role='tooltip']").ClassList);
    }

    [Fact]
    public void TwTooltip_AppliesAriaLabelAndLabelledBy_ToWrapper()
    {
        var cut = RenderTooltip(p => p
            .Add(x => x.AriaLabel, "Definition")
            .Add(x => x.AriaLabelledBy, "heading-1"));

        var wrapper = cut.Find("div");
        Assert.Equal("Definition", wrapper.GetAttribute("aria-label"));
        Assert.Equal("heading-1", wrapper.GetAttribute("aria-labelledby"));
    }

    #endregion

    #region Placement

    [Theory]
    [InlineData(TooltipPlacement.Top)]
    [InlineData(TooltipPlacement.Bottom)]
    [InlineData(TooltipPlacement.Left)]
    [InlineData(TooltipPlacement.Right)]
    public void TwTooltip_Placement_AppliesMatchingThemeClasses(TooltipPlacement placement)
    {
        var cut = RenderTooltip(p => p.Add(x => x.Placement, placement));

        var expected = placement switch
        {
            TooltipPlacement.Bottom => tooltipTheme.Bottom,
            TooltipPlacement.Left => tooltipTheme.Left,
            TooltipPlacement.Right => tooltipTheme.Right,
            _ => tooltipTheme.Top
        };
        var classes = cut.Find("[role='tooltip']").GetAttribute("class")!;

        Assert.Contains(expected, classes);
        foreach (var other in new[] { tooltipTheme.Top, tooltipTheme.Bottom, tooltipTheme.Left, tooltipTheme.Right }.Where(x => x != expected))
        {
            Assert.DoesNotContain(other, classes);
        }
    }

    [Fact]
    public void TwTooltip_DefaultsToTopPlacement()
    {
        var cut = RenderTooltip();

        Assert.Contains(tooltipTheme.Top, cut.Find("[role='tooltip']").GetAttribute("class"));
    }

    #endregion

    #region Styling

    [Fact]
    public void TwTooltip_UsesThemeClassesAndPopoverSurface()
    {
        var cut = RenderTooltip();

        var classes = cut.Find("[role='tooltip']").GetAttribute("class")!;
        Assert.Contains(tooltipTheme.Bubble, classes);
        Assert.Contains(popoverTheme.Background, classes);
        Assert.Contains("group", cut.Find("div").ClassList);
    }

    [Fact]
    public void TwTooltip_Class_AppliesToWrapper_AndTooltipClass_ToBubble()
    {
        var cut = RenderTooltip(p => p
            .Add(x => x.Class, "my-wrapper")
            .Add(x => x.TooltipClass, "my-bubble"));

        Assert.Contains("my-wrapper", cut.Find("div").ClassList);
        Assert.DoesNotContain("my-bubble", cut.Find("div").ClassList);
        Assert.Contains("my-bubble", cut.Find("[role='tooltip']").ClassList);
    }

    [Fact]
    public void TwTooltip_RoundedAndShadow_ApplyToBubble()
    {
        var cut = RenderTooltip(p => p
            .Add(x => x.Rounded, Rounded.Full)
            .Add(x => x.Shadow, Shadow.Lg));

        var classes = cut.Find("[role='tooltip']").GetAttribute("class")!;
        Assert.Contains(Theme.Rounded.Full, classes);
        Assert.Contains(Theme.Shadows.Lg, classes);
    }

    [Fact]
    public void TwTooltip_GeneratesId_WhenNotProvided_AndUsesProvidedId()
    {
        Assert.StartsWith("tooltip-", RenderTooltip().Find("div").Id);

        var cut = RenderTooltip(p => p.Add(x => x.Id, "my-tip"));

        Assert.Equal("my-tip", cut.Find("div").Id);
        Assert.Equal("my-tip-tooltip", cut.Find("[role='tooltip']").Id);
    }

    [Fact]
    public void TwTooltip_PassesThroughUnmatchedAttributes()
    {
        var cut = RenderTooltip(p => p.AddUnmatched("data-test", "yes"));

        Assert.Equal("yes", cut.Find("div").GetAttribute("data-test"));
    }

    #endregion
}
