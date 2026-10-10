using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Utilities;

public class ButtonBuilderTests : TwBlazorTestBase
{
    private TwButtonTheme buttonTheme => Theme.Components.Require<TwButtonTheme>();

    #region GetBaseClasses Tests

    [Fact]
    public void GetBaseClasses_ReturnsBaseClassesWithPadding_WhenNotIconButton()
    {
        // Arrange


        // Act
        var result = ButtonBuilder.GetBaseClasses(iconButton: false, dense: false);

        // Assert
        Assert.Contains(buttonTheme.Base, result);
        Assert.Contains(buttonTheme.Padding, result);
        Assert.DoesNotContain(buttonTheme.IconButton, result);
        Assert.DoesNotContain(buttonTheme.DensePadding, result);
    }

    [Fact]
    public void GetBaseClasses_ReturnsIconButtonClasses_WhenIconButton()
    {
        // Act
        var result = ButtonBuilder.GetBaseClasses(iconButton: true, dense: false);

        // Assert - the icon button's own classes replace the base ones they conflict with (its fixed size over the height)
        Assert.Contains("overflow-hidden", result);
        Assert.Contains(buttonTheme.IconButton, result);
        Assert.DoesNotContain(buttonTheme.Padding, result);
        Assert.DoesNotContain(buttonTheme.DensePadding, result);
    }

    [Fact]
    public void GetBaseClasses_Uses_defaultTheme_WhenThemeIsNull()
    {
        // Act
        var result = ButtonBuilder.GetBaseClasses(iconButton: false, dense: false);

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetBaseClasses_IncludesRoundedClasses_WhenNotIconButton()
    {
        // Arrange
        buttonTheme.ButtonRounded = Rounded.Lg;

        // Act
        var result = ButtonBuilder.GetBaseClasses(iconButton: false, dense: false);

        // Assert
        Assert.Contains("rounded-lg", result);
    }

    [Fact]
    public void GetBaseClasses_UsesComponentRounded_WhenProvided()
    {
        // Arrange
        buttonTheme.ButtonRounded = Rounded.Lg;

        // Act
        var result = ButtonBuilder.GetBaseClasses(iconButton: false, dense: false, Rounded.Sm);

        // Assert
        // Component Rounded should take precedence over theme ButtonRounded
        Assert.Contains("rounded-sm", result);
        Assert.DoesNotContain("rounded-lg", result);
    }

    [Fact]
    public void GetBaseClasses_UsesThemeButtonRounded_WhenComponentRoundedNotProvided()
    {
        // Arrange
        buttonTheme.ButtonRounded = Rounded.Lg;

        // Act
        var result = ButtonBuilder.GetBaseClasses(iconButton: false, dense: false, null);

        // Assert
        // Theme ButtonRounded should apply when component Rounded is not set
        Assert.Contains("rounded-lg", result);
    }

    [Fact]
    public void GetBaseClasses_UsesGlobalDefaultRounded_WhenNoComponentOrThemeRounded()
    {
        // Arrange
        buttonTheme.ButtonRounded = null;
        Theme.Rounded.DefaultRounded = Rounded.Sm;

        // Act
        var result = ButtonBuilder.GetBaseClasses(iconButton: false, dense: false, null);

        // Assert
        // Global default should apply when neither component nor theme specify rounded
        Assert.Contains("rounded-sm", result);
    }

    [Fact]
    public void GetBaseClasses_DoesNotIncludeRounded_WhenIconButton()
    {
        // Arrange
        buttonTheme.ButtonRounded = Rounded.Lg;

        // Act
        var result = ButtonBuilder.GetBaseClasses(iconButton: true, dense: false);

        // Assert
        Assert.DoesNotContain("rounded-lg", result);
    }

    [Fact]
    public void GetBaseClasses_ReturnsDensePaddingClasses_WhenDenseAndNotIconButton()
    {
        // Act
        var result = ButtonBuilder.GetBaseClasses(iconButton: false, dense: true);

        // Assert
        Assert.Contains(buttonTheme.Base, result);
        Assert.Contains(buttonTheme.DensePadding, result);
        Assert.DoesNotContain(buttonTheme.Padding, result);
        Assert.DoesNotContain(buttonTheme.IconButton, result);
    }

    [Fact]
    public void GetBaseClasses_DoesNotIncludeDensePadding_WhenIconButton()
    {
        // Act
        var result = ButtonBuilder.GetBaseClasses(iconButton: true, dense: true);

        // Assert
        Assert.Contains(buttonTheme.IconButton, result);
        Assert.DoesNotContain(buttonTheme.DensePadding, result);
        Assert.DoesNotContain(buttonTheme.Padding, result);
    }

    [Fact]
    public void GetBaseClasses_DoesNotIncludeDensePadding_WhenNotDense()
    {
        // Act
        var result = ButtonBuilder.GetBaseClasses(iconButton: false, dense: false);

        // Assert
        Assert.Contains(buttonTheme.Padding, result);
        Assert.DoesNotContain(buttonTheme.DensePadding, result);
    }

    #endregion

    #region GetTypographyClasses Tests

    [Fact]
    public void GetTypographyClasses_ReturnsTypographyWithUppercase_WhenUppercaseIsTrue()
    {
        // Act
        var result = ButtonBuilder.GetTypographyClasses(uppercase: true);

        // Assert
        Assert.Contains(buttonTheme.Typography, result);
        Assert.Contains(buttonTheme.Uppercase, result);
    }

    [Fact]
    public void GetTypographyClasses_ReturnsTypographyWithoutUppercase_WhenUppercaseIsFalse()
    {
        // Act
        var result = ButtonBuilder.GetTypographyClasses(uppercase: false);

        // Assert
        Assert.Contains(buttonTheme.Typography, result);
        Assert.DoesNotContain(buttonTheme.Uppercase, result);
    }

    [Fact]
    public void GetTypographyClasses_Uses_defaultTheme_WhenThemeIsNull()
    {
        // Act
        var result = ButtonBuilder.GetTypographyClasses(uppercase: true);

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetTypographyClasses_UppercaseIsTrue_ByDefault()
    {
        // Act
        var result = ButtonBuilder.GetTypographyClasses(uppercase: true);

        // Assert
        Assert.Contains(buttonTheme.Uppercase, result);
    }

    #endregion

    #region GetCursorClasses Tests

    [Fact]
    public void GetCursorClasses_ReturnsDisabledCursor_WhenDisabled()
    {
        // Act
        var result = ButtonBuilder.GetCursorClasses(disabled: true, @readonly: false);

        // Assert
        Assert.Equal(buttonTheme.DisabledCursor, result);
    }

    [Fact]
    public void GetCursorClasses_ReturnsReadonlyCursor_WhenReadonly()
    {
        // Act
        var result = ButtonBuilder.GetCursorClasses(disabled: false, @readonly: true);

        // Assert
        Assert.Equal(buttonTheme.ReadonlyCursor, result);
    }

    [Fact]
    public void GetCursorClasses_ReturnsDefaultCursor_WhenNotDisabledOrReadonly()
    {
        // Act
        var result = ButtonBuilder.GetCursorClasses(disabled: false, @readonly: false);

        // Assert
        Assert.Equal(buttonTheme.DefaultCursor, result);
    }

    [Fact]
    public void GetCursorClasses_PrioritizesDisabled_OverReadonly()
    {
        // Act
        var result = ButtonBuilder.GetCursorClasses(disabled: true, @readonly: true);

        // Assert
        Assert.Equal(buttonTheme.DisabledCursor, result);
    }

    [Fact]
    public void GetCursorClasses_Uses_defaultTheme_WhenThemeIsNull()
    {
        // Act
        var result = ButtonBuilder.GetCursorClasses(disabled: false, @readonly: false);

        // Assert
        Assert.NotEmpty(result);
    }

    #endregion

    #region GetVariantClasses Tests - Disabled

    [Theory]
    [InlineData(ButtonVariant.Outlined)]
    [InlineData(ButtonVariant.Text)]
    [InlineData(ButtonVariant.Filled)]
    [InlineData(ButtonVariant.Elevated)]
    public void GetVariantClasses_ReturnsDisabledClasses_WhenDisabled(ButtonVariant variant)
    {
        // Act
        var result = ButtonBuilder.GetVariantClasses(variant, Color.Primary, disabled: true);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("cursor-not-allowed", result);
    }

    [Fact]
    public void GetVariantClasses_ReturnsDisabledOutlined_ForOutlinedWhenDisabled()
    {
        // Act
        var result = ButtonBuilder.GetVariantClasses(ButtonVariant.Outlined, Color.Primary, disabled: true);

        // Assert
        Assert.Equal(buttonTheme.DisabledOutlined, result);
    }

    [Fact]
    public void GetVariantClasses_ReturnsDisabledText_ForTextWhenDisabled()
    {
        // Act
        var result = ButtonBuilder.GetVariantClasses(ButtonVariant.Text, Color.Primary, disabled: true);

        // Assert
        Assert.Equal(buttonTheme.DisabledText, result);
    }

    [Theory]
    [InlineData(ButtonVariant.Filled)]
    [InlineData(ButtonVariant.Elevated)]
    public void GetVariantClasses_ReturnsDisabledFilled_ForFilledVariantsWhenDisabled(ButtonVariant variant)
    {
        // Act
        var result = ButtonBuilder.GetVariantClasses(variant, Color.Primary, disabled: true);

        // Assert
        Assert.Equal(buttonTheme.DisabledFilled, result);
    }

    [Fact]
    public void GetVariantClasses_DisabledTakesPrecedence_OverShadowOverride()
    {
        // Act - disabled short-circuits before the variant/shadowOverride switch is ever reached
        var result = ButtonBuilder.GetVariantClasses(ButtonVariant.Elevated, Color.Primary, disabled: true, shadowOverride: Shadow.Sm);

        // Assert
        Assert.Equal(buttonTheme.DisabledFilled, result);
    }

    #endregion

    #region GetVariantClasses Tests - Elevated

    [Fact]
    public void GetVariantClasses_ReturnsFilledVariantColor_ForElevatedVariant()
    {
        // Act
        var result = ButtonBuilder.GetVariantClasses(ButtonVariant.Elevated, Color.Primary, disabled: false);

        // Assert
        Assert.Contains("bg-purple-600", result);
        Assert.Contains("text-gray-100", result);
    }

    [Fact]
    public void GetVariantClasses_IncludesFixedLgShadow_ForElevatedVariant_WhenNoShadowOverride()
    {
        // Act
        var result = ButtonBuilder.GetVariantClasses(ButtonVariant.Elevated, Color.Primary, disabled: false, shadowOverride: null);

        // Assert
        Assert.Contains(Theme.Shadows.Lg, result);
    }

    [Fact]
    public void GetVariantClasses_OmitsFixedShadow_ForElevatedVariant_WhenShadowOverrideProvided()
    {
        // Act - an explicit Shadow parameter should win over the variant's fixed Lg shadow
        var result = ButtonBuilder.GetVariantClasses(ButtonVariant.Elevated, Color.Primary, disabled: false, shadowOverride: Shadow.Sm);

        // Assert
        Assert.DoesNotContain(Theme.Shadows.Lg, result);
        Assert.Equal(ColorBuilder.GetFilledVariantColor(Color.Primary), result);
    }

    [Theory]
    [InlineData(Color.Primary, "bg-purple-600")]
    [InlineData(Color.Accent, "bg-fuchsia-700")]
    [InlineData(Color.Success, "bg-green-700")]
    [InlineData(Color.Danger, "bg-red-700")]
    [InlineData(Color.Warning, "bg-yellow-600")]
    [InlineData(Color.Info, "bg-blue-600")]
    [InlineData(Color.Light, "bg-gray-100")]
    [InlineData(Color.Dark, "bg-gray-900")]
    public void GetVariantClasses_ReturnsCorrectColorClass_ForElevatedVariant(Color color, string expectedClass)
    {
        // Act
        var result = ButtonBuilder.GetVariantClasses(ButtonVariant.Elevated, color, disabled: false);

        // Assert
        Assert.Contains(expectedClass, result);
    }

    #endregion

    #region GetVariantClasses Tests - Filled

    [Fact]
    public void GetVariantClasses_ReturnsFilledClasses_ForFilledVariant()
    {
        // Act
        var result = ButtonBuilder.GetVariantClasses(ButtonVariant.Filled, Color.Primary, disabled: false);

        // Assert
        Assert.Contains("bg-purple-600", result);
        Assert.Contains("text-gray-100", result);
    }

    [Fact]
    public void GetVariantClasses_ReturnsCustomFilledClass_WhenDefinedInTheme()
    {
        // Arrange
        var customClass = "custom-filled-purple hover:bg-purple-700";
        Theme.Colors.SurfaceColors.Filled.Primary = customClass;

        // Act
        var result = ButtonBuilder.GetVariantClasses(ButtonVariant.Filled, Color.Primary, disabled: false);

        // Assert
        Assert.Equal(customClass, result);
    }

    [Theory]
    [InlineData(Color.Accent, "bg-fuchsia-700", "text-gray-100")]
    [InlineData(Color.Success, "bg-green-700", "text-white")]
    [InlineData(Color.Danger, "bg-red-700", "text-gray-100")]
    [InlineData(Color.Warning, "bg-yellow-600", "text-gray-950")]
    [InlineData(Color.Info, "bg-blue-600", "text-gray-100")]
    [InlineData(Color.Light, "bg-gray-100", "text-gray-950")]
    [InlineData(Color.Dark, "bg-gray-900", "text-gray-100")]
    public void GetVariantClasses_ReturnsCorrectTextColor_ForFilledVariant(Color color, string expectedBg, string expectedText)
    {
        // Act
        var result = ButtonBuilder.GetVariantClasses(ButtonVariant.Filled, color, disabled: false);

        // Assert
        Assert.Contains(expectedBg, result);
        Assert.Contains(expectedText, result);
    }

    #endregion

    #region GetVariantClasses Tests - Outlined

    [Theory]
    [InlineData(Color.Primary, "border-purple-600", "text-purple-600")]
    [InlineData(Color.Accent, "border-fuchsia-600", "text-fuchsia-600")]
    [InlineData(Color.Success, "border-green-600", "text-green-800")]
    [InlineData(Color.Danger, "border-red-600", "text-red-700")]
    [InlineData(Color.Warning, "border-yellow-700", "text-yellow-800")]
    [InlineData(Color.Info, "border-blue-600", "text-blue-600")]
    [InlineData(Color.Light, "border-gray-100", "text-gray-200")]
    [InlineData(Color.Dark, "border-[oklch(21%_0.006_285.885)]/50", "text-gray-950")]
    public void GetVariantClasses_ReturnsCorrectOutlinedClasses_ForEachColor(Color color, string expectedBorder, string expectedText)
    {
        // Act
        var result = ButtonBuilder.GetVariantClasses(ButtonVariant.Outlined, color, disabled: false);

        // Assert
        Assert.Contains(expectedBorder, result);
        Assert.Contains(expectedText, result);
        Assert.Contains("bg-transparent", result);
    }

    [Fact]
    public void GetVariantClasses_ReturnsCustomOutlinedClass_WhenDefinedInTheme()
    {
        // Arrange
        var customClass = "custom-outlined-purple border-2 border-purple-500";
        Theme.Colors.SurfaceColors.Outlined.Primary = customClass;

        // Act
        var result = ButtonBuilder.GetVariantClasses(ButtonVariant.Outlined, Color.Primary, disabled: false);

        // Assert
        Assert.Equal(customClass, result);
    }

    [Fact]
    public void GetVariantClasses_OutlinedIncludesBorder()
    {
        // Act
        var result = ButtonBuilder.GetVariantClasses(ButtonVariant.Outlined, Color.Primary, disabled: false);

        // Assert
        Assert.Contains("border", result);
    }

    #endregion

    #region GetVariantClasses Tests - Text

    [Theory]
    [InlineData(Color.Primary, "text-purple-600")]
    [InlineData(Color.Accent, "text-fuchsia-600")]
    [InlineData(Color.Success, "text-green-800")]
    [InlineData(Color.Danger, "text-red-700")]
    [InlineData(Color.Warning, "text-yellow-800")]
    [InlineData(Color.Info, "text-blue-600")]
    [InlineData(Color.Light, "text-gray-200")]
    [InlineData(Color.Dark, "text-gray-900")]
    public void GetVariantClasses_ReturnsCorrectTextClasses_ForEachColor(Color color, string expectedText)
    {
        // Act
        var result = ButtonBuilder.GetVariantClasses(ButtonVariant.Text, color, disabled: false);

        // Assert
        Assert.Contains(expectedText, result);
        Assert.Contains("bg-transparent", result);
    }

    [Fact]
    public void GetVariantClasses_ReturnsCustomTextClass_WhenDefinedInTheme()
    {
        // Arrange
        var customClass = "custom-text-purple text-purple-500";
        Theme.Colors.SurfaceColors.Text.Primary = customClass;

        // Act
        var result = ButtonBuilder.GetVariantClasses(ButtonVariant.Text, Color.Primary, disabled: false);

        // Assert
        Assert.Equal(customClass, result);
    }

    [Fact]
    public void GetVariantClasses_TextVariantDoesNotIncludeBorder()
    {
        // Act
        var result = ButtonBuilder.GetVariantClasses(ButtonVariant.Text, Color.Primary, disabled: false);

        // Assert
        Assert.DoesNotContain("border", result);
    }

    #endregion

    #region GetVariantClasses Tests - Edge Cases

    [Fact]
    public void GetVariantClasses_ReturnsEmptyString_WhenVariantIsNull()
    {
        // Act
        var result = ButtonBuilder.GetVariantClasses(null, Color.Primary, disabled: false);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetVariantClasses_ReturnsEmptyString_WhenColorIsNull()
    {
        // Act
        var result = ButtonBuilder.GetVariantClasses(ButtonVariant.Filled, null, disabled: false);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region Dark Mode Classes Tests

    [Fact]
    public void GetVariantClasses_OutlinedIncludesDarkModeClasses()
    {
        // Act
        var result = ButtonBuilder.GetVariantClasses(ButtonVariant.Outlined, Color.Primary, disabled: false);

        // Assert
        Assert.Contains("dark:border-purple-500", result);
        Assert.Contains("dark:hover:bg-purple-900/20", result);
    }

    [Fact]
    public void GetVariantClasses_TextIncludesDarkModeClasses()
    {
        // Act
        var result = ButtonBuilder.GetVariantClasses(ButtonVariant.Text, Color.Primary, disabled: false);

        // Assert
        Assert.Contains("dark:hover:bg-purple-900/20", result);
    }

    #endregion

    #region GetDisabledClasses Tests

    [Fact]
    public void GetDisabledClasses_ReturnsDisabledOutlined_ForOutlinedVariant()
    {
        // Act
        var result = ButtonBuilder.GetDisabledClasses(ButtonVariant.Outlined);

        // Assert
        Assert.Equal(buttonTheme.DisabledOutlined, result);
    }

    [Fact]
    public void GetDisabledClasses_ReturnsDisabledText_ForTextVariant()
    {
        // Act
        var result = ButtonBuilder.GetDisabledClasses(ButtonVariant.Text);

        // Assert
        Assert.Equal(buttonTheme.DisabledText, result);
    }

    [Theory]
    [InlineData(ButtonVariant.Filled)]
    [InlineData(ButtonVariant.Elevated)]
    public void GetDisabledClasses_ReturnsDisabledFilled_ForFilledVariants(ButtonVariant variant)
    {
        // Act
        var result = ButtonBuilder.GetDisabledClasses(variant);

        // Assert
        Assert.Equal(buttonTheme.DisabledFilled, result);
    }

    [Fact]
    public void GetDisabledClasses_ReturnsEmptyString_WhenVariantIsNull()
    {
        // Act
        var result = ButtonBuilder.GetDisabledClasses(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    #endregion
}
