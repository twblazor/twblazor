using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Utilities;

public class PopoverBuilderTests : TwBlazorTestBase
{
    private PopoverBuilder popoverBuilder => TestContext.Services.GetRequiredService<PopoverBuilder>();

    private TwOverlayTheme overlayTheme => Theme.Components.Require<TwOverlayTheme>();

    [Fact]
    public void GetSurfaceClasses_IncludesThemeBackgroundAndBorder()
    {
        // Act
        var result = popoverBuilder.GetSurfaceClasses(null, null);

        // Assert
        Assert.Contains(overlayTheme.PopoverBackground, result);
        Assert.Contains(overlayTheme.PopoverBorder, result);
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
        overlayTheme.PopoverRounded = Rounded.Md;

        // Act
        var result = popoverBuilder.GetSurfaceClasses(null, null);

        // Assert
        Assert.Contains(Theme.Rounded.Md, result);
    }

    [Fact]
    public void GetSurfaceClasses_FallsBackToGlobalDefaultRounded_WhenNoOverridesSet()
    {
        // Arrange
        overlayTheme.PopoverRounded = null;
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
        overlayTheme.PopoverShadow = Shadow.Md;

        // Act
        var result = popoverBuilder.GetSurfaceClasses(null, null);

        // Assert
        Assert.Contains(Theme.Shadows.Md, result);
    }

    [Fact]
    public void GetSurfaceClasses_FallsBackToGlobalDefaultShadow_WhenNoOverridesSet()
    {
        // Arrange
        overlayTheme.PopoverShadow = null;
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

        Assert.Equal(overlayTheme.TooltipWrapper, result);
        Assert.Contains("group", result.Split(' '));
    }

    [Fact]
    public void GetTooltipClasses_IncludesThemeBubbleAndPopoverSurface()
    {
        var result = popoverBuilder.GetTooltipClasses(false);

        Assert.Contains(overlayTheme.TooltipBubble, result);
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

    [Fact]
    public void GetTooltipClasses_FollowsThemeOverrides()
    {
        overlayTheme.TooltipBubble = "custom-bubble";

        Assert.Contains("custom-bubble", popoverBuilder.GetTooltipClasses(false));
    }

    [Fact]
    public void GetTooltipClasses_FollowsPopoverSurfaceOverrides()
    {
        overlayTheme.PopoverBackground = "custom-popover-bg";

        Assert.Contains("custom-popover-bg", popoverBuilder.GetTooltipClasses(false));
    }
}
