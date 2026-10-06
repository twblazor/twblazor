using TwBlazor.Builders;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Utilities;

public class ColorBuilderTests : TwBlazorTestBase
{
    #region GetTextColor Tests

    [Theory]
    [InlineData(Color.Primary, "text-purple-600", "dark:text-purple-200")]
    [InlineData(Color.Accent, "text-fuchsia-600", "dark:text-fuchsia-200")]
    [InlineData(Color.Success, "text-green-800", "dark:text-green-600")]
    [InlineData(Color.Danger, "text-red-700", "dark:text-red-200")]
    [InlineData(Color.Warning, "text-yellow-800", "dark:text-yellow-600")]
    [InlineData(Color.Info, "text-blue-600", "dark:text-blue-200")]
    [InlineData(Color.Light, "text-white", "dark:text-white")]
    [InlineData(Color.Dark, "text-gray-950", "dark:text-gray-950")]
    public void GetTextColor_ReturnsCorrectClasses_ForEachColor(Color color, string expectedLight, string expectedDark)
    {
        // Act
        var result = ColorBuilder.GetTextColor(color);

        // Assert
        Assert.Contains(expectedLight, result);
        Assert.Contains(expectedDark, result);
    }

    [Fact]
    public void GetTextColor_ReturnsEmptyString_WhenColorIsNull()
    {
        // Act
        var result = ColorBuilder.GetTextColor(null);

        // Assert - empty, not a neutral default: TwIcon (the other consumer of this method) needs an
        // icon with no explicit Color to inherit its surrounding element's color via `currentColor`.
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetTextColor_ReturnsCustomClass_WhenDefinedInTheme()
    {
        // Arrange
        var customClass = "text-custom-500 dark:text-custom-400";
        Theme.Colors.TextColors.Medium.Primary = customClass;

        // Act
        var result = ColorBuilder.GetTextColor(Color.Primary);

        // Assert
        Assert.Contains("text-custom-500", result);
    }

    #endregion

    #region GetBorderColor Tests

    [Theory]
    [InlineData(Color.Primary, "border-purple-600")]
    [InlineData(Color.Accent, "border-fuchsia-600")]
    [InlineData(Color.Success, "border-green-600")]
    [InlineData(Color.Danger, "border-red-600")]
    [InlineData(Color.Warning, "border-yellow-700")]
    [InlineData(Color.Info, "border-blue-600")]
    [InlineData(Color.Light, "border-gray-100")]
    [InlineData(Color.Dark, "border-gray-900")]
    public void GetBorderColor_ReturnsCorrectClasses_ForEachColor(Color color, string expectedClass)
    {
        // Act
        var result = ColorBuilder.GetBorderColor(color);

        // Assert
        Assert.Contains(expectedClass, result);
    }

    [Fact]
    public void GetBorderColor_ReturnsPrimaryDefault_WhenColorIsNull()
    {
        // Act
        var result = ColorBuilder.GetBorderColor(null);

        // Assert
        Assert.Equal(Theme.Border.Colors.Primary, result);
    }

    #endregion

    #region GetFocusRing Tests

    [Theory]
    [InlineData(Color.Primary, "focus-visible:ring-purple-500")]
    [InlineData(Color.Accent, "focus-visible:ring-fuchsia-500")]
    [InlineData(Color.Success, "focus-visible:ring-green-600")]
    [InlineData(Color.Danger, "focus-visible:ring-red-500")]
    [InlineData(Color.Warning, "focus-visible:ring-yellow-500")]
    [InlineData(Color.Info, "focus-visible:ring-blue-500")]
    [InlineData(Color.Light, "focus-visible:ring-gray-400")]
    [InlineData(Color.Dark, "focus-visible:ring-gray-700")]
    public void GetFocusRing_ReturnsCorrectClasses_ForEachColor(Color color, string expectedRing)
    {
        // Act
        var result = ColorBuilder.GetFocusRing(color);

        // Assert
        Assert.Contains(expectedRing, result);
    }

    [Fact]
    public void GetFocusRing_FallsBackToPrimaryRing_WhenColorIsNull()
    {
        // A null/unrecognized color must still produce a visible focus ring - buttons remove the
        // native outline (focus:outline-none), so an empty ring here would leave keyboard focus
        // invisible. See ColorBuilder.GetFocusRing remarks.
        // Act
        var result = ColorBuilder.GetFocusRing(null);

        // Assert
        Assert.Contains("focus-visible:ring-purple-500", result);
        Assert.NotEqual(string.Empty, result);
    }

    [Theory]
    [InlineData(Color.Primary, "peer-focus-visible:ring-purple-500")]
    [InlineData(Color.Danger, "peer-focus-visible:ring-red-500")]
    [InlineData(Color.Dark, "peer-focus-visible:ring-gray-700")]
    [InlineData(null, "peer-focus-visible:ring-purple-500")]
    public void GetPeerFocusRing_ReturnsRingWidthAndColor_ForEachColor(Color? color, string expectedRing)
    {
        // Act
        var result = ColorBuilder.GetPeerFocusRing(color);

        // Assert
        Assert.Contains("peer-focus-visible:ring-2", result);
        Assert.Contains(expectedRing, result);
    }

    [Fact]
    public void GetFocusRing_ReturnsCustomClass_WhenDefinedInTheme()
    {
        // Arrange
        var customFocus = "focus:ring-custom-500/20";
        Theme.Colors.FocusColors.Primary = customFocus;

        // Act
        var result = ColorBuilder.GetFocusRing(Color.Primary);

        // Assert
        Assert.Contains(customFocus, result);
    }

    #endregion

    #region GetOutlinedVariantColor Tests

    [Theory]
    [InlineData(Color.Primary, "border-purple-600", "text-purple-600")]
    [InlineData(Color.Accent, "border-fuchsia-600", "text-fuchsia-600")]
    [InlineData(Color.Success, "border-green-600", "text-green-800")]
    [InlineData(Color.Danger, "border-red-600", "text-red-700")]
    [InlineData(Color.Warning, "border-yellow-700", "text-yellow-800")]
    [InlineData(Color.Info, "border-blue-600", "text-blue-600")]
    [InlineData(Color.Light, "border-gray-100", "text-gray-200")]
    [InlineData(Color.Dark, "border-[oklch(21%_0.006_285.885)]/25", "text-gray-950")]
    public void GetOutlinedVariantColor_ReturnsCorrectClasses_ForEachColor(Color color, string expectedBorder, string expectedText)
    {
        // Act
        var result = ColorBuilder.GetOutlinedVariantColor(color);

        // Assert
        Assert.Contains(expectedBorder, result);
        Assert.Contains(expectedText, result);
        Assert.Contains("bg-transparent", result);
    }

    [Fact]
    public void GetOutlinedVariantColor_FallsBackToPrimary_WhenColorIsNull()
    {
        // Act
        var result = ColorBuilder.GetOutlinedVariantColor(null);

        // Assert
        Assert.Equal(ColorBuilder.GetOutlinedVariantColor(Color.Primary), result);
        Assert.NotEqual(string.Empty, result);
    }

    [Fact]
    public void GetOutlinedVariantColor_IncludesBorderClass()
    {
        // Act
        var result = ColorBuilder.GetOutlinedVariantColor(Color.Primary);

        // Assert
        Assert.Contains("border", result);
    }

    [Fact]
    public void GetOutlinedVariantColor_IncludesDarkModeClasses()
    {
        // Act
        var result = ColorBuilder.GetOutlinedVariantColor(Color.Primary);

        // Assert
        Assert.Contains("dark:border-purple-500", result);
        Assert.Contains("dark:hover:bg-purple-900/20", result);
    }

    #endregion

    #region GetTextVariantColor Tests

    [Theory]
    [InlineData(Color.Primary, "text-purple-600")]
    [InlineData(Color.Accent, "text-fuchsia-600")]
    [InlineData(Color.Success, "text-green-800")]
    [InlineData(Color.Danger, "text-red-700")]
    [InlineData(Color.Warning, "text-yellow-800")]
    [InlineData(Color.Info, "text-blue-600")]
    [InlineData(Color.Light, "text-gray-200")]
    [InlineData(Color.Dark, "text-gray-900")]
    public void GetTextVariantColor_ReturnsCorrectClasses_ForEachColor(Color color, string expectedText)
    {
        // Act
        var result = ColorBuilder.GetTextVariantColor(color);

        // Assert
        Assert.Contains(expectedText, result);
        Assert.Contains("bg-transparent", result);
    }

    [Fact]
    public void GetTextVariantColor_ReturnsEmptyString_WhenColorIsNull()
    {
        // Act
        var result = ColorBuilder.GetTextVariantColor(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetTextVariantColor_IncludesHoverEffects()
    {
        // Act
        var result = ColorBuilder.GetTextVariantColor(Color.Primary);

        // Assert
        Assert.Contains("hover:bg-purple-50", result);
    }

    [Fact]
    public void GetTextVariantColor_IncludesDarkModeClasses()
    {
        // Act
        var result = ColorBuilder.GetTextVariantColor(Color.Primary);

        // Assert
        Assert.Contains("dark:hover:bg-purple-900/20", result);
    }

    #endregion

    #region GetFilledVariantColor Tests

    [Theory]
    [InlineData(Color.Primary, "bg-purple-600", "text-gray-100")]
    [InlineData(Color.Accent, "bg-fuchsia-700", "text-gray-100")]
    [InlineData(Color.Success, "bg-green-700", "text-white")]
    [InlineData(Color.Danger, "bg-red-700", "text-gray-100")]
    [InlineData(Color.Warning, "bg-yellow-600", "text-gray-950")]
    [InlineData(Color.Info, "bg-blue-600", "text-gray-100")]
    [InlineData(Color.Light, "bg-gray-100", "text-gray-950")]
    [InlineData(Color.Dark, "bg-gray-900", "text-gray-100")]
    public void GetFilledVariantColor_ReturnsCorrectClasses_ForEachColor(Color color, string expectedBg, string expectedText)
    {
        // Act
        var result = ColorBuilder.GetFilledVariantColor(color);

        // Assert
        Assert.Contains(expectedBg, result);
        Assert.Contains(expectedText, result);
    }

    [Fact]
    public void GetFilledVariantColor_ReturnsEmptyString_WhenColorIsNull()
    {
        // Act
        var result = ColorBuilder.GetFilledVariantColor(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetFilledVariantColor_IncludesHoverAndActiveStates()
    {
        // Act
        var result = ColorBuilder.GetFilledVariantColor(Color.Primary);

        // Assert
        Assert.Contains("hover:bg-purple-700", result);
        Assert.Contains("active:bg-purple-800", result);
    }

    [Fact]
    public void GetFilledVariantColor_Light_UsesWhiteBackgroundWithDarkText()
    {
        // Act
        var result = ColorBuilder.GetFilledVariantColor(Color.Light);

        // Assert
        Assert.Contains("bg-gray-100", result);
        Assert.Contains("text-gray-950", result);
    }

    [Fact]
    public void GetFilledVariantColor_ReturnsCustomClass_WhenDefinedInTheme()
    {
        // Arrange
        var customClass = "bg-custom-600 hover:bg-custom-700 text-white";
        Theme.Colors.SurfaceColors.Filled.Primary = customClass;

        // Act
        var result = ColorBuilder.GetFilledVariantColor(Color.Primary);

        // Assert
        Assert.Equal(customClass, result);
    }

    #endregion
}
