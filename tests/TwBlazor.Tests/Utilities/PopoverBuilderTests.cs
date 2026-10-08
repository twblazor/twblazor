using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Utilities;

public class PopoverBuilderTests : TwBlazorTestBase
{
    private PopoverBuilder popoverBuilder => TestContext.Services.GetRequiredService<PopoverBuilder>();

    private TwPopoverTheme popoverTheme => Theme.Components.Require<TwOverlayTheme>().Popover;

    private TwTooltipTheme tooltipTheme => Theme.Components.Require<TwOverlayTheme>().Tooltip;

    [Fact]
    public void GetSurfaceClasses_IncludesThemeBackgroundAndBorder()
    {
        // Act
        var result = popoverBuilder.GetSurfaceClasses(null, null);

        // Assert
        Assert.Contains(popoverTheme.Background, result);
        Assert.Contains(popoverTheme.Border, result);
    }

    [Fact]
    public void GetSurfaceClasses_UsesCustomRounded_WhenProvided()
    {
        // Act
        var result = popoverBuilder.GetSurfaceClasses(Rounded.Full, null);

        // Assert
        Assert.Contains(Theme.Rounded.Full, result);
    }

    [Fact]
    public void GetSurfaceClasses_UsesThemePopoverRounded_WhenComponentRoundedNotProvided()
    {
        // Arrange
        popoverTheme.Rounded = Rounded.Md;

        // Act
        var result = popoverBuilder.GetSurfaceClasses(null, null);

        // Assert
        Assert.Contains(Theme.Rounded.Md, result);
    }

    [Fact]
    public void GetSurfaceClasses_FallsBackToGlobalDefaultRounded_WhenNoOverridesSet()
    {
        // Arrange
        popoverTheme.Rounded = null;
        Theme.Rounded.DefaultRounded = Rounded.Sm;

        // Act
        var result = popoverBuilder.GetSurfaceClasses(null, null);

        // Assert
        Assert.Contains(Theme.Rounded.Sm, result);
    }

    [Fact]
    public void GetSurfaceClasses_UsesCustomShadow_WhenProvided()
    {
        // Act
        var result = popoverBuilder.GetSurfaceClasses(null, Shadow.Lg);

        // Assert
        Assert.Contains(Theme.Shadows.Lg, result);
    }

    [Fact]
    public void GetSurfaceClasses_UsesThemePopoverShadow_WhenComponentShadowNotProvided()
    {
        // Arrange
        popoverTheme.Shadow = Shadow.Md;

        // Act
        var result = popoverBuilder.GetSurfaceClasses(null, null);

        // Assert
        Assert.Contains(Theme.Shadows.Md, result);
    }

    [Fact]
    public void GetSurfaceClasses_FallsBackToGlobalDefaultShadow_WhenNoOverridesSet()
    {
        // Arrange
        popoverTheme.Shadow = null;
        Theme.Shadows.DefaultShadow = Shadow.Lg;

        // Act
        var result = popoverBuilder.GetSurfaceClasses(null, null);

        // Assert
        Assert.Contains(Theme.Shadows.Lg, result);
    }

    [Fact]
    public void GetSurfaceClasses_AppendsCustomClass_WhenProvided()
    {
        // Act
        var result = popoverBuilder.GetSurfaceClasses(null, null, "my-custom-surface");

        // Assert
        Assert.Contains("my-custom-surface", result);
    }

    [Fact]
    public void GetSurfaceClasses_DoesNotAppendCustomClass_WhenNullOrWhitespace()
    {
        // Act
        var result = popoverBuilder.GetSurfaceClasses(null, null, "  ");

        // Assert
        Assert.DoesNotContain("  ", result.Trim());
    }

    [Fact]
    public void GetTooltipWrapperClasses_ReturnsThemeWrapper_WithGroupHook()
    {
        var result = popoverBuilder.GetTooltipWrapperClasses();

        Assert.Equal(tooltipTheme.Wrapper, result);
        Assert.Contains("group", result.Split(' '));
    }

    [Fact]
    public void GetTooltipClasses_IncludesThemeBubbleAndPopoverSurface()
    {
        var result = popoverBuilder.GetTooltipClasses(false);

        Assert.Contains(tooltipTheme.Bubble, result);
        Assert.Contains(popoverBuilder.GetSurfaceClasses(null, null), result);
    }

    [Fact]
    public void GetTooltipClasses_RevealsOnHoverAndKeyboardFocusOnly()
    {
        var classes = popoverBuilder.GetTooltipClasses(false).Split(' ');

        Assert.Contains("invisible", classes);
        Assert.Contains("group-hover:visible", classes);
        Assert.Contains("group-has-[:focus-visible]:visible", classes);
    }

    [Fact]
    public void GetTooltipClasses_AddsHidden_OnlyWhenDismissed()
    {
        Assert.DoesNotContain(Theme.Display.Hidden, popoverBuilder.GetTooltipClasses(false).Split(' '));
        Assert.Contains(Theme.Display.Hidden, popoverBuilder.GetTooltipClasses(true).Split(' '));
    }

    [Theory]
    [InlineData(TooltipPlacement.Top)]
    [InlineData(TooltipPlacement.Bottom)]
    [InlineData(TooltipPlacement.Left)]
    [InlineData(TooltipPlacement.Right)]
    public void GetTooltipClasses_AddsOnlyTheRequestedPlacement(TooltipPlacement placement)
    {
        var placements = new Dictionary<TooltipPlacement, string>
        {
            [TooltipPlacement.Top] = tooltipTheme.Top,
            [TooltipPlacement.Bottom] = tooltipTheme.Bottom,
            [TooltipPlacement.Left] = tooltipTheme.Left,
            [TooltipPlacement.Right] = tooltipTheme.Right
        };

        var result = popoverBuilder.GetTooltipClasses(false, placement);

        foreach (var (key, classes) in placements)
        {
            Assert.Equal(key == placement, result.Contains(classes));
        }
    }

    [Fact]
    public void GetTooltipClasses_DefaultsToTopPlacement()
    {
        Assert.Contains(tooltipTheme.Top, popoverBuilder.GetTooltipClasses(false));
    }

    [Fact]
    public void GetTooltipClasses_UsesRoundedShadowAndCustomClassOverrides()
    {
        var result = popoverBuilder.GetTooltipClasses(false, TooltipPlacement.Top, Rounded.Full, Shadow.Lg, "my-bubble");

        Assert.Contains(Theme.Rounded.Full, result);
        Assert.Contains(Theme.Shadows.Lg, result);
        Assert.Contains("my-bubble", result);
    }

    [Fact]
    public void GetTooltipClasses_FollowsThemeOverrides()
    {
        tooltipTheme.Bubble = "custom-bubble";

        Assert.Contains("custom-bubble", popoverBuilder.GetTooltipClasses(false));
    }

    [Fact]
    public void GetTooltipClasses_FollowsPopoverSurfaceOverrides()
    {
        popoverTheme.Background = "custom-popover-bg";

        Assert.Contains("custom-popover-bg", popoverBuilder.GetTooltipClasses(false));
    }
}
