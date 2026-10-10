using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Utilities;

public class ShadowBuilderTests : TwBlazorTestBase
{
    private TwButtonTheme buttonTheme => Theme.Components.Require<TwButtonTheme>();

    #region GetShadow Tests

    [Theory]
    [InlineData(Shadow.None, "shadow-none")]
    [InlineData(Shadow.Sm, "shadow-sm")]
    [InlineData(Shadow.Md, "shadow")]
    [InlineData(Shadow.Lg, "shadow-lg")]
    public void GetShadow_ReturnsCorrectClass_ForEachShadowValue(Shadow shadow, string expectedClass)
    {
        // Act
        var result = ShadowBuilder.GetShadow(shadow);

        // Assert
        Assert.Equal(expectedClass, result);
    }

    [Fact]
    public void GetShadow_ReturnsEmptyString_WhenShadowIsNull()
    {
        // Act
        var result = ShadowBuilder.GetShadow(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetShadow_ReturnsThemeSm_ForUnrecognizedShadowValue()
    {
        // Arrange & Act - a Shadow value outside the defined enum members (None/Sm/Md/Lg) doesn't
        // match any named case or the `null` case, so it falls through to the `_ => Shadows.Sm` default.
        var result = ShadowBuilder.GetShadow((Shadow)999);

        // Assert
        Assert.Equal(Theme.Shadows.Sm, result);
    }

    #endregion

    #region GetButtonShadow - Theme Configuration Tests

    [Fact]
    public void GetButtonShadow_ReturnsEmptyString_WhenButtonThemeIsNull()
    {
        // Act
        var result = ShadowBuilder.GetButtonShadow(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetButtonShadow_ReturnsEmptyString_WhenButtonShadowIsNull()
    {
        // Arrange
        buttonTheme.ButtonShadow = null;

        // Act
        var result = ShadowBuilder.GetButtonShadow(buttonTheme);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetButtonShadow_ReturnsEmptyString_WhenButtonShadowIsNone()
    {
        // Arrange
        buttonTheme.ButtonShadow = Shadow.None;

        // Act
        var result = ShadowBuilder.GetButtonShadow(buttonTheme);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetButtonShadow_ReturnsBaseShadow_WithHoverAndActive_WhenConfiguredInTheme()
    {
        // Arrange
        buttonTheme.ButtonShadow = Shadow.Sm;

        // Act
        var result = ShadowBuilder.GetButtonShadow(buttonTheme);

        // Assert
        Assert.Contains(Theme.Shadows.Sm, result);
        Assert.Contains(Theme.Shadows.HoverSm, result);
        Assert.Contains(Theme.Shadows.ActiveMd, result);
    }

    [Fact]
    public void GetButtonShadow_UsesThemeHoverSm_ForSmShadowLevel()
    {
        // Arrange
        buttonTheme.ButtonShadow = Shadow.Sm;

        // Act
        var result = ShadowBuilder.GetButtonShadow(buttonTheme);

        // Assert
        Assert.Contains(Theme.Shadows.Sm, result);
        Assert.Contains(Theme.Shadows.HoverSm, result);
        Assert.Contains(Theme.Shadows.ActiveMd, result);
    }

    [Fact]
    public void GetButtonShadow_UsesThemeHoverMd_ForMdShadowLevel()
    {
        // Arrange
        buttonTheme.ButtonShadow = Shadow.Md;

        // Act
        var result = ShadowBuilder.GetButtonShadow(buttonTheme);

        // Assert
        Assert.Contains(Theme.Shadows.Md, result);
        Assert.Contains(Theme.Shadows.HoverMd, result);
        Assert.Contains(Theme.Shadows.ActiveMd, result);
    }

    [Fact]
    public void GetButtonShadow_UsesThemeHoverLg_ForLgShadowLevel()
    {
        // Arrange
        buttonTheme.ButtonShadow = Shadow.Lg;

        // Act
        var result = ShadowBuilder.GetButtonShadow(buttonTheme);

        // Assert
        Assert.Contains(Theme.Shadows.Lg, result);
        Assert.Contains(Theme.Shadows.HoverLg, result);
        Assert.Contains(Theme.Shadows.ActiveMd, result);
    }

    #endregion

    #region GetButtonShadow - Hover Parameter Tests

    [Fact]
    public void GetButtonShadow_ExcludesHoverShadow_WhenIncludeHoverIsFalse()
    {
        // Arrange
        buttonTheme.ButtonShadow = Shadow.Sm;

        // Act
        var result = ShadowBuilder.GetButtonShadow(buttonTheme, includeHover: false);

        // Assert
        Assert.Contains(Theme.Shadows.Sm, result);
        Assert.DoesNotContain("hover:", result);
        Assert.Contains(Theme.Shadows.ActiveMd, result);
    }

    [Fact]
    public void GetButtonShadow_IncludesHoverShadow_WhenIncludeHoverIsTrue()
    {
        // Arrange
        buttonTheme.ButtonShadow = Shadow.Md;

        // Act
        var result = ShadowBuilder.GetButtonShadow(buttonTheme, includeHover: true);

        // Assert
        Assert.Contains(Theme.Shadows.Md, result);
        Assert.Contains(Theme.Shadows.HoverMd, result);
        Assert.Contains(Theme.Shadows.ActiveMd, result);
    }

    #endregion

    #region GetButtonShadow - Active Parameter Tests

    [Fact]
    public void GetButtonShadow_ExcludesActiveShadow_WhenIncludeActiveIsFalse()
    {
        // Arrange
        buttonTheme.ButtonShadow = Shadow.Sm;

        // Act
        var result = ShadowBuilder.GetButtonShadow(buttonTheme, includeActive: false);

        // Assert
        Assert.Contains(Theme.Shadows.Sm, result);
        Assert.Contains(Theme.Shadows.HoverSm, result);
        Assert.DoesNotContain("active:", result);
    }

    [Fact]
    public void GetButtonShadow_IncludesActiveShadow_WhenIncludeActiveIsTrue()
    {
        // Arrange
        buttonTheme.ButtonShadow = Shadow.Lg;

        // Act
        var result = ShadowBuilder.GetButtonShadow(buttonTheme, includeActive: true);

        // Assert
        Assert.Contains(Theme.Shadows.Lg, result);
        Assert.Contains(Theme.Shadows.HoverLg, result);
        Assert.Contains(Theme.Shadows.ActiveMd, result);
    }

    [Fact]
    public void GetButtonShadow_ReturnsOnlyBaseShadow_WhenBothHoverAndActiveAreFalse()
    {
        // Arrange
        buttonTheme.ButtonShadow = Shadow.Md;

        // Act
        var result = ShadowBuilder.GetButtonShadow(buttonTheme, includeHover: false, includeActive: false);

        // Assert
        Assert.Equal(Theme.Shadows.Md, result);
        Assert.DoesNotContain("hover:", result);
        Assert.DoesNotContain("active:", result);
    }

    #endregion

    #region Global vs Component Level Configuration Tests

    [Fact]
    public void GetButtonShadow_UsesGlobalThemeConfiguration()
    {
        // Arrange - Simulating global theme configuration
        buttonTheme.ButtonShadow = Shadow.Lg;

        // Act
        var result = ShadowBuilder.GetButtonShadow(buttonTheme);

        // Assert
        Assert.Contains(Theme.Shadows.Lg, result);
        Assert.Contains(Theme.Shadows.HoverLg, result);
    }

    [Fact]
    public void GetButtonShadow_CanBeOverriddenAtComponentLevel_ByPassingDifferentTheme()
    {
        // Arrange - Global theme
        buttonTheme.ButtonShadow = Shadow.Lg;
        var globalResult = ShadowBuilder.GetButtonShadow(buttonTheme);

        // Component-level override (simulated by changing the shadow)
        buttonTheme.ButtonShadow = Shadow.Sm;
        var componentResult = ShadowBuilder.GetButtonShadow(buttonTheme);

        // Assert - Global configuration (first result)
        Assert.Contains(Theme.Shadows.Lg, globalResult);
        Assert.Contains(Theme.Shadows.HoverLg, globalResult);

        // Assert - Component override (second result)
        Assert.Contains(Theme.Shadows.Sm, componentResult);
        Assert.Contains(Theme.Shadows.HoverSm, componentResult);
        Assert.DoesNotContain(Theme.Shadows.Lg, componentResult);
    }

    [Fact]
    public void GetButtonShadow_ComponentCanDisableShadow_BySettingNone()
    {
        // Arrange - Global theme has shadow enabled
        buttonTheme.ButtonShadow = Shadow.Md;
        var globalResult = ShadowBuilder.GetButtonShadow(buttonTheme);

        // Component explicitly disables shadow
        buttonTheme.ButtonShadow = Shadow.None;
        var componentResult = ShadowBuilder.GetButtonShadow(buttonTheme);

        // Assert
        Assert.NotEmpty(globalResult); // Global has shadow
        Assert.Empty(componentResult); // Component has no shadow
    }

    [Fact]
    public void GetButtonShadow_ComponentCanDisableShadow_BySettingNull()
    {
        // Arrange - Global theme has shadow enabled
        buttonTheme.ButtonShadow = Shadow.Md;
        var globalResult = ShadowBuilder.GetButtonShadow(buttonTheme);

        // Component explicitly disables shadow by setting null
        buttonTheme.ButtonShadow = null;
        var componentResult = ShadowBuilder.GetButtonShadow(buttonTheme);

        // Assert
        Assert.NotEmpty(globalResult); // Global has shadow
        Assert.Empty(componentResult); // Component has no shadow
    }

    #endregion

    #region Edge Cases and Branch Coverage

    [Fact]
    public void GetButtonShadow_ReturnsEmptyString_WhenThemeObjectExistsButButtonShadowIsNotSet()
    {
        // Arrange - Theme exists but ButtonShadow is default (null)
        buttonTheme.ButtonShadow = null;

        // Act
        var result = ShadowBuilder.GetButtonShadow(buttonTheme);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetButtonShadow_HandlesAllParameterCombinations()
    {
        // Arrange
        buttonTheme.ButtonShadow = Shadow.Lg;

        // Act & Assert - Test all 4 combinations
        var bothTrue = ShadowBuilder.GetButtonShadow(buttonTheme, includeHover: true, includeActive: true);
        Assert.Contains(Theme.Shadows.Lg, bothTrue);
        Assert.Contains(Theme.Shadows.HoverLg, bothTrue);
        Assert.Contains(Theme.Shadows.ActiveMd, bothTrue);

        var hoverOnly = ShadowBuilder.GetButtonShadow(buttonTheme, includeHover: true, includeActive: false);
        Assert.Contains(Theme.Shadows.Lg, hoverOnly);
        Assert.Contains(Theme.Shadows.HoverLg, hoverOnly);
        Assert.DoesNotContain("active:", hoverOnly);

        var activeOnly = ShadowBuilder.GetButtonShadow(buttonTheme, includeHover: false, includeActive: true);
        Assert.Contains(Theme.Shadows.Lg, activeOnly);
        Assert.DoesNotContain("hover:", activeOnly);
        Assert.Contains(Theme.Shadows.ActiveMd, activeOnly);

        var noneEnabled = ShadowBuilder.GetButtonShadow(buttonTheme, includeHover: false, includeActive: false);
        Assert.Equal(Theme.Shadows.Lg, noneEnabled);
    }

    [Fact]
    public void GetButtonShadow_BuildsCorrectClassString_WithMultipleClasses()
    {
        // Arrange
        buttonTheme.ButtonShadow = Shadow.Sm;

        // Act
        var result = ShadowBuilder.GetButtonShadow(buttonTheme);

        // Assert
        var classes = result.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(3, classes.Length);
        Assert.Contains(Theme.Shadows.Sm, classes);
        Assert.Contains(Theme.Shadows.HoverSm, classes);
        Assert.Contains(Theme.Shadows.ActiveMd, classes);
    }

    [Fact]
    public void GetButtonShadow_UsesThemeNone_ForHoverShadow_WhenShadowLevelIsUnrecognized()
    {
        // Arrange & Act - an out-of-range Shadow value isn't Shadow.None, so the early-return guard
        // is skipped, and GetShadow(...) falls back to its own `_ => Sm` default (a non-empty base
        // shadow). That leaves the inner hover-shadow switch's own `_ => Shadows.None` default case
        // as the only one that matches, since the value is none of Sm/Md/Lg either.
        var result = ShadowBuilder.GetButtonShadow(null, overrideShadow: (Shadow)999);

        // Assert
        // The base shadow is replaced because the none shadow is added after it and sets the same property.
        Assert.DoesNotContain(Theme.Shadows.Sm, result);
        Assert.Contains(Theme.Shadows.None, result);
        Assert.DoesNotContain(Theme.Shadows.HoverSm, result);
        Assert.DoesNotContain(Theme.Shadows.HoverMd, result);
        Assert.DoesNotContain(Theme.Shadows.HoverLg, result);
    }

    #endregion

    #region Theme Property Coverage Tests

    [Fact]
    public void Theme_HasAllRequiredHoverShadowProperties()
    {
        // Assert - Verify all hover shadow properties are defined
        Assert.NotNull(Theme.Shadows.HoverSm);
        Assert.NotNull(Theme.Shadows.HoverMd);
        Assert.NotNull(Theme.Shadows.HoverLg);

        // Verify they're not empty
        Assert.NotEmpty(Theme.Shadows.HoverSm);
        Assert.NotEmpty(Theme.Shadows.HoverMd);
        Assert.NotEmpty(Theme.Shadows.HoverLg);

        // Verify they contain "hover:"
        Assert.Contains("hover:", Theme.Shadows.HoverSm);
        Assert.Contains("hover:", Theme.Shadows.HoverMd);
        Assert.Contains("hover:", Theme.Shadows.HoverLg);
    }

    [Fact]
    public void Theme_HasActiveShadowProperty()
    {
        // Assert
        Assert.NotNull(Theme.Shadows.ActiveMd);
        Assert.NotEmpty(Theme.Shadows.ActiveMd);
        Assert.Contains("active:", Theme.Shadows.ActiveMd);
    }

    [Fact]
    public void GetButtonShadow_UsesCorrectThemePropertyForEachShadowLevel()
    {
        // Test Sm maps to HoverSm
        buttonTheme.ButtonShadow = Shadow.Sm;
        var resultSm = ShadowBuilder.GetButtonShadow(buttonTheme);
        var classesSm = resultSm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Assert.Contains(Theme.Shadows.HoverSm, classesSm);
        Assert.DoesNotContain(Theme.Shadows.HoverMd, classesSm);
        Assert.DoesNotContain(Theme.Shadows.HoverLg, classesSm);

        // Test Md maps to HoverMd
        buttonTheme.ButtonShadow = Shadow.Md;
        var resultMd = ShadowBuilder.GetButtonShadow(buttonTheme);
        var classesMd = resultMd.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Assert.Contains(Theme.Shadows.HoverMd, classesMd);
        Assert.DoesNotContain(Theme.Shadows.HoverSm, classesMd);
        Assert.DoesNotContain(Theme.Shadows.HoverLg, classesMd);

        // Test Lg maps to HoverLg
        buttonTheme.ButtonShadow = Shadow.Lg;
        var resultLg = ShadowBuilder.GetButtonShadow(buttonTheme);
        var classesLg = resultLg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Assert.Contains(Theme.Shadows.HoverLg, classesLg);
        Assert.DoesNotContain(Theme.Shadows.HoverMd, classesLg);
        Assert.DoesNotContain(Theme.Shadows.HoverSm, classesLg);
    }

    [Fact]
    public void GetButtonShadow_AlwaysUsesActiveMd_WhenActiveIsIncluded()
    {
        // Test with different shadow levels - all should use ActiveMd
        var shadowLevels = new[] { Shadow.Sm, Shadow.Md, Shadow.Lg };

        foreach (var shadowLevel in shadowLevels)
        {
            buttonTheme.ButtonShadow = shadowLevel;
            var result = ShadowBuilder.GetButtonShadow(buttonTheme, includeActive: true);

            Assert.Contains(Theme.Shadows.ActiveMd, result);
        }
    }

    #endregion
}
