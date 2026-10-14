using TwBlazor.Builders;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Utilities;

public class ChipBuilderTests : TwBlazorTestBase
{
    #region GetBaseClasses Tests
    [Fact]
    public void GetBaseClasses_ReturnsCorrectClasses_ForSmallSize()
    {
        // Arrange & Act
        var result = ChipBuilder.GetBaseClasses(ChipSize.Small, false, false);

        // Assert
        Assert.Contains("text-[10px]", result);
        Assert.Contains("leading-none", result);
        Assert.Contains("px-1.5", result);
        Assert.Contains("h-5", result);
    }

    [Fact]
    public void GetBaseClasses_ReturnsCorrectClasses_ForMediumSize()
    {
        // Act
        var result = ChipBuilder.GetBaseClasses(ChipSize.Medium, false, false);

        // Assert
        Assert.Contains("text-xs", result);
        Assert.Contains("h-6", result);
    }

    [Fact]
    public void GetBaseClasses_ReturnsCorrectClasses_ForLargeSize()
    {
        // Act
        var result = ChipBuilder.GetBaseClasses(ChipSize.Large, false, false);

        // Assert
        Assert.Contains("text-sm", result);
        Assert.Contains("h-8", result);
    }

    [Fact]
    public void GetBaseClasses_IncludesClickableClasses_WhenClickableAndNotDisabled()
    {
        // Act
        var result = ChipBuilder.GetBaseClasses(ChipSize.Medium, true, false);

        // Assert
        Assert.Contains("cursor-pointer", result);
        Assert.Contains("shadow-sm", result);
    }

    [Fact]
    public void GetBaseClasses_IncludesDisabledClasses_WhenDisabled()
    {
        // Act
        var result = ChipBuilder.GetBaseClasses(ChipSize.Medium, true, true);

        // Assert
        Assert.Contains("cursor-not-allowed", result);
    }

    #endregion

    #region GetVariantClasses Tests

    [Fact]
    public void GetVariantClasses_ReturnsFilledClasses_ForFilledVariant()
    {
        // Act
        var result = ChipBuilder.GetVariantClasses(ButtonVariant.Filled, Color.Primary, false);

        // Assert
        Assert.Contains("bg-purple-600", result);
        Assert.Contains("text-gray-100", result);
    }

    [Fact]
    public void GetVariantClasses_ReturnsOutlinedClasses_ForOutlinedVariant()
    {
        // Act
        var result = ChipBuilder.GetVariantClasses(ButtonVariant.Outlined, Color.Danger, false);

        // Assert
        Assert.Contains("border", result);
        Assert.Contains("text-red-700", result);
    }

    [Fact]
    public void GetVariantClasses_ReturnsTextClasses_ForTextVariant()
    {
        // Act
        var result = ChipBuilder.GetVariantClasses(ButtonVariant.Text, Color.Accent, false);

        // Assert
        Assert.Contains("text-fuchsia-700", result);
        Assert.Contains("bg-transparent", result);
    }

    [Fact]
    public void GetVariantClasses_ReturnsDisabledClasses_WhenDisabled_ForFilledVariant()
    {
        // Act
        var result = ChipBuilder.GetVariantClasses(ButtonVariant.Filled, Color.Primary, true);

        // Assert
        Assert.Contains("bg-[oklch(21%_0.006_285.885)]/15", result);
        Assert.Contains("text-[oklch(21%_0.006_285.885)]/40", result);
    }

    [Fact]
    public void GetVariantClasses_ReturnsDisabledClasses_WhenDisabled_ForOutlinedVariant()
    {
        // Act
        var result = ChipBuilder.GetVariantClasses(ButtonVariant.Outlined, Color.Primary, true);

        // Assert
        Assert.Contains("border", result);
        Assert.Contains("text-[oklch(21%_0.006_285.885)]/40", result);
    }

    [Fact]
    public void GetVariantClasses_ReturnsDisabledClasses_WhenDisabled_ForTextVariant()
    {
        // Act
        var result = ChipBuilder.GetVariantClasses(ButtonVariant.Text, Color.Primary, true);

        // Assert
        Assert.Contains("text-[oklch(21%_0.006_285.885)]/40", result);
        Assert.Contains("bg-transparent", result);
    }

    [Fact]
    public void GetVariantClasses_ReturnsCustomFilledClass_WhenDefinedInTheme()
    {
        // Arrange
        var customClass = "custom-filled-blue hover:bg-blue-700";
        Theme.Colors.SurfaceColors.Filled.Primary = customClass;

        // Act
        var result = ChipBuilder.GetVariantClasses(ButtonVariant.Filled, Color.Primary, false);

        // Assert
        Assert.Equal(customClass, result);
    }

    #endregion

    #region All Colors Coverage Tests

    [Fact]
    public void GetVariantClasses_HandlesAllColors_ForFilledVariant()
    {
        // Arrange
        var supportedColors = new[]
        {
            Color.Danger, Color.Accent, Color.Warning, Color.Success,
            Color.Primary, Color.Info, Color.Light, Color.Dark
        };

        // Act & Assert
        foreach (var color in supportedColors)
        {
            var result = ChipBuilder.GetVariantClasses(ButtonVariant.Filled, color, false);
            Assert.NotEmpty(result);
        }
    }

    [Fact]
    public void GetVariantClasses_HandlesAllColors_ForOutlinedVariant()
    {
        // Arrange
        var supportedColors = new[]
        {
            Color.Danger, Color.Accent, Color.Warning, Color.Success,
            Color.Primary, Color.Info, Color.Light, Color.Dark
        };

        // Act & Assert
        foreach (var color in supportedColors)
        {
            var result = ChipBuilder.GetVariantClasses(ButtonVariant.Outlined, color, false);
            Assert.NotEmpty(result);
        }
    }

    [Fact]
    public void GetVariantClasses_HandlesAllColors_ForTextVariant()
    {
        // Arrange
        var supportedColors = new[]
        {
            Color.Danger, Color.Accent, Color.Warning, Color.Success,
            Color.Primary, Color.Info, Color.Light, Color.Dark
        };

        // Act & Assert
        foreach (var color in supportedColors)
        {
            var result = ChipBuilder.GetVariantClasses(ButtonVariant.Text, color, false);
            Assert.NotEmpty(result);
        }
    }

    [Fact]
    public void GetBaseClasses_ReturnsDefaultMediumClasses_ForUnknownSize()
    {
        // Act - cast an out-of-range value to hit the default case in the switch
        var result = ChipBuilder.GetBaseClasses((ChipSize)99, false, false);

        // Assert - default case returns Chip.Md classes (same as Medium)
        var mediumResult = ChipBuilder.GetBaseClasses(ChipSize.Medium, false, false);
        Assert.Equal(mediumResult, result);
    }

    #endregion
}
