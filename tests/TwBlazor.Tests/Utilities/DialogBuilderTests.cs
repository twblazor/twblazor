using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Utilities;

public class DialogBuilderTests : TwBlazorTestBase
{
    private DialogBuilder dialogBuilder => TestContext.Services.GetRequiredService<DialogBuilder>();

    private TwDialogTheme dialogTheme => Theme.Components.Require<TwOverlayTheme>().Dialog;


    #region GetPositionClasses

    [Theory]
    [InlineData(DialogPosition.Center, "items-center justify-center")]
    [InlineData(DialogPosition.CenterLeft, "items-center justify-start")]
    [InlineData(DialogPosition.CenterRight, "items-center justify-end")]
    [InlineData(DialogPosition.TopCenter, "items-start justify-center")]
    [InlineData(DialogPosition.TopLeft, "items-start justify-start")]
    [InlineData(DialogPosition.TopRight, "items-start justify-end")]
    [InlineData(DialogPosition.BottomCenter, "items-end justify-center")]
    [InlineData(DialogPosition.BottomLeft, "items-end justify-start")]
    [InlineData(DialogPosition.BottomRight, "items-end justify-end")]
    public void GetPositionClasses_ReturnsCorrectClasses_ForEachPosition(DialogPosition position, string expected)
    {
        // Act
        var result = dialogBuilder.GetPositionClasses(position);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetPositionClasses_DefaultsToCenter_WhenNull()
    {
        // Act
        var result = dialogBuilder.GetPositionClasses(null);

        // Assert
        Assert.Equal("items-center justify-center", result);
    }

    [Fact]
    public void GetPositionClasses_ReadsFromTheme_NotHardcoded()
    {
        // Arrange
        Theme.Anchor.TopCenter = "custom-top-center-classes";

        // Act
        var result = dialogBuilder.GetPositionClasses(DialogPosition.TopCenter);

        // Assert
        Assert.Equal("custom-top-center-classes", result);
    }

    #endregion

    #region GetMaxWidthClasses

    [Theory]
    [InlineData(DialogMaxWidth.Small, "sm:max-w-lg")]
    [InlineData(DialogMaxWidth.Medium, "sm:max-w-xl")]
    [InlineData(DialogMaxWidth.Large, "sm:max-w-3xl")]
    public void GetMaxWidthClasses_ReturnsCorrectClass_ForEachBreakpoint(DialogMaxWidth maxWidth, string expected)
    {
        // Act
        var result = dialogBuilder.GetMaxWidthClasses(maxWidth);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetMaxWidthClasses_ReturnsEmptyString_WhenFalse()
    {
        // Act
        var result = dialogBuilder.GetMaxWidthClasses(DialogMaxWidth.False);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetMaxWidthClasses_DefaultsToSmall_WhenNull()
    {
        // Act
        var result = dialogBuilder.GetMaxWidthClasses(null);

        // Assert
        Assert.Equal("sm:max-w-lg", result);
    }

    #endregion

    #region GetBackdropClasses

    [Fact]
    public void GetBackdropClasses_IncludesThemeBackdropAndPosition()
    {
        // Act
        var result = dialogBuilder.GetBackdropClasses(DialogPosition.Center);

        // Assert
        Assert.Contains(dialogTheme.Backdrop, result);
        Assert.Contains("items-center justify-center", result);
    }

    [Fact]
    public void GetBackdropClasses_AppendsCustomBackdropClass_WhenProvided()
    {
        // Act
        var result = dialogBuilder.GetBackdropClasses(DialogPosition.Center, "my-custom-backdrop");

        // Assert
        Assert.Contains("my-custom-backdrop", result);
    }

    [Fact]
    public void GetBackdropClasses_DoesNotAppendCustomClass_WhenNullOrWhitespace()
    {
        // Act
        var result = dialogBuilder.GetBackdropClasses(DialogPosition.Center, "  ");

        // Assert
        Assert.DoesNotContain("  ", result.Trim());
    }

    #endregion

    #region GetSurfaceClasses

    [Fact]
    public void GetSurfaceClasses_IncludesThemeSurface()
    {
        // Act
        var result = dialogBuilder.GetSurfaceClasses(DialogMaxWidth.Small, false, false, null, null);

        // Assert
        Assert.Contains(dialogTheme.Surface, result);
    }

    [Fact]
    public void GetSurfaceClasses_AppliesMaxWidth_WhenNotFullScreen()
    {
        // Act
        var result = dialogBuilder.GetSurfaceClasses(DialogMaxWidth.Large, false, false, null, null);

        // Assert
        Assert.Contains("sm:max-w-3xl", result);
    }

    [Fact]
    public void GetSurfaceClasses_AppliesFullWidth_WhenTrueAndNotFullScreen()
    {
        // Arrange - use a marker distinct from the baseline "w-full" already present in dialogTheme.Surface
        dialogTheme.FullWidth = "full-width-marker";

        // Act
        var result = dialogBuilder.GetSurfaceClasses(DialogMaxWidth.Small, true, false, null, null);

        // Assert
        Assert.Contains("full-width-marker", result);
    }

    [Fact]
    public void GetSurfaceClasses_DoesNotApplyFullWidth_WhenFalse()
    {
        // Arrange
        dialogTheme.FullWidth = "full-width-marker";

        // Act
        var result = dialogBuilder.GetSurfaceClasses(DialogMaxWidth.Small, false, false, null, null);

        // Assert
        Assert.DoesNotContain("full-width-marker", result);
    }

    [Fact]
    public void GetSurfaceClasses_AppliesFullScreenClasses_AndSkipsMaxWidthAndFullWidth()
    {
        // Arrange
        dialogTheme.FullWidth = "full-width-marker";

        // Act
        var result = dialogBuilder.GetSurfaceClasses(DialogMaxWidth.Large, true, true, null, null);

        // Assert
        Assert.Contains(dialogTheme.FullScreen, result);
        Assert.DoesNotContain("sm:max-w-3xl", result);
        Assert.DoesNotContain("full-width-marker", result);
    }

    [Fact]
    public void GetSurfaceClasses_UsesRoundedNone_WhenFullScreen()
    {
        // Act
        var result = dialogBuilder.GetSurfaceClasses(DialogMaxWidth.Small, false, true, null, null);

        // Assert
        Assert.Contains(Theme.Rounded.None, result);
    }

    [Fact]
    public void GetSurfaceClasses_UsesCustomRounded_WhenProvided()
    {
        // Act
        var result = dialogBuilder.GetSurfaceClasses(DialogMaxWidth.Small, false, false, Rounded.Full, null);

        // Assert
        Assert.Contains(Theme.Rounded.Full, result);
    }

    [Fact]
    public void GetSurfaceClasses_UsesThemeDialogRounded_WhenComponentRoundedNotProvided()
    {
        // Arrange
        dialogTheme.Rounded = Rounded.Md;

        // Act
        var result = dialogBuilder.GetSurfaceClasses(DialogMaxWidth.Small, false, false, null, null);

        // Assert
        Assert.Contains(Theme.Rounded.Md, result);
    }

    [Fact]
    public void GetSurfaceClasses_FallsBackToGlobalDefaultRounded_WhenNoOverridesSet()
    {
        // Arrange
        dialogTheme.Rounded = null;
        Theme.Rounded.DefaultRounded = Rounded.Sm;

        // Act
        var result = dialogBuilder.GetSurfaceClasses(DialogMaxWidth.Small, false, false, null, null);

        // Assert
        Assert.Contains(Theme.Rounded.Sm, result);
    }

    [Fact]
    public void GetSurfaceClasses_UsesCustomShadow_WhenProvided()
    {
        // Act
        var result = dialogBuilder.GetSurfaceClasses(DialogMaxWidth.Small, false, false, null, Shadow.Lg);

        // Assert
        Assert.Contains(Theme.Shadows.Lg, result);
    }

    [Fact]
    public void GetSurfaceClasses_UsesThemeDialogShadow_WhenComponentShadowNotProvided()
    {
        // Arrange
        dialogTheme.Shadow = Shadow.Md;

        // Act
        var result = dialogBuilder.GetSurfaceClasses(DialogMaxWidth.Small, false, false, null, null);

        // Assert
        Assert.Contains(Theme.Shadows.Md, result);
    }

    [Fact]
    public void GetSurfaceClasses_AppendsCustomClass_WhenProvided()
    {
        // Act
        var result = dialogBuilder.GetSurfaceClasses(DialogMaxWidth.Small, false, false, null, null, "my-custom-surface");

        // Assert
        Assert.Contains("my-custom-surface", result);
    }

    #endregion
}
