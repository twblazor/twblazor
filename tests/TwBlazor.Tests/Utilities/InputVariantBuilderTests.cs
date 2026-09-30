using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Utilities;

public class InputVariantBuilderTests : TwBlazorTestBase
{
    private TwInputTheme inputTheme => Theme.Components.Require<TwInputTheme>();

    #region GetTextInputClasses Tests - Default Variant

    [Fact]
    public void GetTextInputClasses_ReturnsDefaultClasses_ForDefaultVariant()
    {
        // Act
        var result = InputVariantBuilder.GetClasses(InputVariant.Default, inputTheme);

        // Assert
        Assert.Contains("border-b-1", result);
        Assert.Contains(inputTheme.FilledBorder, result);
        Assert.Contains(inputTheme.FocusBorder, result);
        Assert.Contains("bg-transparent", result);
        Assert.Contains("px-0", result);
    }

    [Fact]
    public void GetTextInputClasses_DefaultVariantHasBottomBorderOnly()
    {
        // Act
        var result = InputVariantBuilder.GetClasses(InputVariant.Default, inputTheme);

        // Assert
        Assert.Contains("border-b-1", result);
        Assert.DoesNotContain("border-1 ", result);
    }

    #endregion

    #region GetTextInputClasses Tests - Outlined Variant

    [Fact]
    public void GetTextInputClasses_ReturnsOutlinedClasses_ForOutlinedVariant()
    {
        // Act
        var result = InputVariantBuilder.GetClasses(InputVariant.Outlined, inputTheme);

        // Assert
        Assert.Contains("border-1", result);
        Assert.Contains(inputTheme.OutlinedBorder, result);
        Assert.Contains(inputTheme.FocusBorder, result);
        Assert.Contains("bg-transparent", result);
        Assert.Contains("px-3", result);
        Assert.Contains("rounded", result);
    }

    [Fact]
    public void GetTextInputClasses_OutlinedVariantHasFullBorder()
    {
        // Act
        var result = InputVariantBuilder.GetClasses(InputVariant.Outlined, inputTheme);

        // Assert
        Assert.Contains("border-1", result);
        Assert.DoesNotContain("border-b-1", result);
    }

    [Fact]
    public void GetTextInputClasses_OutlinedVariantIncludesRounded()
    {
        // Act
        var result = InputVariantBuilder.GetClasses(InputVariant.Outlined, inputTheme);

        // Assert
        Assert.Contains("rounded", result);
    }

    [Fact]
    public void GetTextInputClasses_OutlinedVariantHasPadding()
    {
        // Act
        var result = InputVariantBuilder.GetClasses(InputVariant.Outlined, inputTheme);

        // Assert
        Assert.Contains("px-3", result);
    }

    #endregion

    #region GetTextInputClasses Tests - Filled Variant

    [Fact]
    public void GetTextInputClasses_ReturnsFilledClasses_ForFilledVariant()
    {
        // Act
        var result = InputVariantBuilder.GetClasses(InputVariant.Filled, inputTheme);

        // Assert
        Assert.Contains("border-b-1", result);
        Assert.Contains(inputTheme.FilledBorder, result);
        Assert.Contains(inputTheme.FocusBorder, result);
        Assert.Contains(inputTheme.FilledBackgroundColor, result);
        Assert.Contains("px-3", result);
        Assert.Contains("rounded", result);
    }

    [Fact]
    public void GetTextInputClasses_FilledVariantHasBottomBorderOnly()
    {
        // Act
        var result = InputVariantBuilder.GetClasses(InputVariant.Filled, inputTheme);

        // Assert
        Assert.Contains("border-b-1", result);
        Assert.DoesNotContain("border-1 ", result);
    }

    [Fact]
    public void GetTextInputClasses_FilledVariantHasBackgroundColor()
    {
        // Act
        var result = InputVariantBuilder.GetClasses(InputVariant.Filled, inputTheme);

        // Assert
        Assert.Contains(inputTheme.FilledBackgroundColor, result);
    }

    [Fact]
    public void GetTextInputClasses_FilledVariant_RoundsOnlyTopCorners_NotBottom()
    {
        // Act
        var result = InputVariantBuilder.GetClasses(InputVariant.Filled, inputTheme);

        // Assert — Filled only draws a flat, full-width bottom border (border-b-1), so rounding
        // the bottom corners of the background would make that border cut across the curve
        // instead of following a flat edge. Only the top corners should be rounded.
        Assert.Contains("rounded-t", result);
        Assert.DoesNotContain("rounded-b", result);
        Assert.Equal(RoundedBuilder.GetRoundedTop(), RoundedBuilder.GetRoundedTop(Theme.Rounded.DefaultRounded));
        Assert.Contains(RoundedBuilder.GetRoundedTop(), result);
    }

    #endregion

    #region All Variants Coverage

    [Theory]
    [InlineData(InputVariant.Default)]
    [InlineData(InputVariant.Outlined)]
    [InlineData(InputVariant.Filled)]
    public void GetTextInputClasses_ReturnsNonEmptyString_ForAllVariants(InputVariant variant)
    {
        // Act
        var result = InputVariantBuilder.GetClasses(variant, inputTheme);

        // Assert
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData(InputVariant.Default)]
    [InlineData(InputVariant.Outlined)]
    [InlineData(InputVariant.Filled)]
    public void GetTextInputClasses_IncludesBorderClasses_ForAllVariants(InputVariant variant)
    {
        // Act
        var result = InputVariantBuilder.GetClasses(variant, inputTheme);

        // Assert
        Assert.Contains("border", result);
    }

    #endregion

    #region Dark Mode Support

    [Fact]
    public void GetTextInputClasses_IncludesDarkModeClasses_WhenUsingDefaultTheme()
    {
        // Act
        var result = InputVariantBuilder.GetClasses(InputVariant.Default, inputTheme);

        // Assert
        Assert.Contains("dark:", result);
    }

    [Fact]
    public void GetTextInputClasses_FilledVariant_IncludesDarkBackgroundColor()
    {
        // Act
        var result = InputVariantBuilder.GetClasses(InputVariant.Filled, inputTheme);

        // Assert
        Assert.Contains("dark:bg-[oklch(34%_0.018_253)]", result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void GetTextInputClasses_UnknownVariant_ReturnsDefaultClasses()
    {
        // Arrange - Cast an invalid enum value
        var unknownVariant = (InputVariant)999;

        // Act
        var result = InputVariantBuilder.GetClasses(unknownVariant, inputTheme);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("border-b-1", result);
        Assert.Contains("bg-transparent", result);
    }

    #endregion
}
