using TwBlazor.Enums;

namespace TwBlazor.Tests.Utilities;

public class IconColorBuilderTests : TwBlazorTestBase
{
    [Theory]
    [InlineData(Color.Primary, "text-purple-600", "dark:text-purple-400")]
    [InlineData(Color.Accent, "text-fuchsia-600", "dark:text-fuchsia-400")]
    [InlineData(Color.Success, "text-green-600", "dark:text-green-400")]
    [InlineData(Color.Danger, "text-red-600", "dark:text-red-400")]
    [InlineData(Color.Warning, "text-[oklch(65%_0.15_80)]", "dark:text-yellow-400")]
    [InlineData(Color.Info, "text-blue-600", "dark:text-blue-400")]
    [InlineData(Color.Light, "text-gray-100", "dark:text-white")]
    [InlineData(Color.Dark, "text-gray-950", "dark:text-gray-950")]
    public void GetIconColor_ReturnsLightAndDarkClasses_ForEachColor(Color color, string expectedLight, string expectedDark)
    {
        // Act
        var result = IconColorBuilder.GetIconColor(color);

        // Assert
        Assert.Contains(expectedLight, result);
        Assert.Contains(expectedDark, result);
    }

    [Theory]
    [InlineData(Color.Primary)]
    [InlineData(Color.Accent)]
    [InlineData(Color.Success)]
    [InlineData(Color.Danger)]
    [InlineData(Color.Warning)]
    [InlineData(Color.Info)]
    public void GetIconColor_UsesBrighterDarkModeShadeThanTheTextColor_ForSemanticColors(Color color)
    {
        // Act
        var iconColor = IconColorBuilder.GetIconColor(color);

        // Assert - regression: icons reused the 200-level pastel text colors in dark mode, which looked washed out.
        Assert.DoesNotContain("-200", iconColor);
        Assert.DoesNotContain("-800", iconColor);
    }

    [Fact]
    public void GetIconColor_ReturnsEmptyString_WhenColorIsNull()
    {
        // Act
        var result = IconColorBuilder.GetIconColor(null);

        // Assert - empty so an icon with no explicit Color inherits its surrounding color via `currentColor`.
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetIconColor_ReturnsCustomClass_WhenDefinedInTheme()
    {
        // Arrange
        Theme.Components.Require<TwBlazor.Configuration.Components.TwIconTheme>().Colors.Primary = "text-custom-500 dark:text-custom-300";

        // Act
        var result = IconColorBuilder.GetIconColor(Color.Primary);

        // Assert
        Assert.Equal("text-custom-500 dark:text-custom-300", result);
    }
}
