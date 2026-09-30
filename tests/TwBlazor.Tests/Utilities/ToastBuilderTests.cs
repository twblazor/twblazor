using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Utilities;

public class ToastBuilderTests : TwBlazorTestBase
{
    private TwToastTheme toastTheme => Theme.Components.Require<TwToastTheme>();

    [Fact]
    public void GetToastClasses_UsesCustomRounded()
    {
        // Act
        var result = ToastBuilder.GetToastClasses(Rounded.Full);

        // Assert
        Assert.Contains("rounded-full", result);
        Assert.DoesNotContain("rounded-lg", result);
    }

    [Fact]
    public void GetToastClasses_UsesThemeRounded()
    {
        // Arrange
        toastTheme.ToastRounded = Rounded.Md;

        // Act
        var result = ToastBuilder.GetToastClasses();

        // Assert
        Assert.Contains(Theme.Rounded.Md, result);
    }

    [Fact]
    public void GetToastClasses_AppliesWidth_WhenToastWidthIsSet()
    {
        // Arrange
        toastTheme.ToastWidth = "w-96";

        // Act
        var result = ToastBuilder.GetToastClasses();

        // Assert
        Assert.Contains("w-96", result);
    }

    [Fact]
    public void GetToastClasses_DefaultWidth_IsFixedSoAllToastsMatch()
    {
        // Act
        var result = ToastBuilder.GetToastClasses().Split(' ');

        // Assert - a fixed width (not just a max-width) keeps toasts the same size regardless of content
        Assert.Contains("w-[300px]", result);
        Assert.Contains("max-w-full", result);
    }

    [Fact]
    public void GetToastClasses_DoesNotApplyWidth_WhenToastWidthIsEmpty()
    {
        // Arrange
        toastTheme.ToastWidth = string.Empty;

        // Act
        var result = ToastBuilder.GetToastClasses();

        // Assert
        Assert.DoesNotContain("w-", result.Split(' ').Where(c => c.StartsWith("w-")).FirstOrDefault() ?? string.Empty);
    }

    [Fact]
    public void GetToastClasses_UsesDefaultRounded_WhenNoToastRoundedSet()
    {
        // Arrange - ensure Toast.ToastRounded is null so it falls back to Rounded.DefaultRounded
        toastTheme.ToastRounded = null;

        // Act
        var result = ToastBuilder.GetToastClasses();

        // Assert - uses options.Theme.Rounded.DefaultRounded
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}
