using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using IconEnum = TwBlazor.Enums.Icon;

namespace TwBlazor.Tests.Components.Toggle;

public class TwToggleTests : TwBlazorTestBase
{
    private TwToggleTheme theme => Theme.Components.Require<TwToggleTheme>();

    private static string[] ClassesOf(AngleSharp.Dom.IElement element) =>
        (element.GetAttribute("class") ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);

    private static void AssertHasAll(string expected, string[] actual)
    {
        foreach (var token in expected.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            Assert.Contains(token, actual);
        }
    }

    [Fact]
    public void TwToggle_RendersANativeButton_NotPressedByDefault()
    {
        // Act
        var cut = TestContext.Render<TwToggle>(p => p.Add(x => x.Label, "Bold"));

        // Assert
        var button = cut.Find("button");
        Assert.Equal("button", button.GetAttribute("type"));
        Assert.Equal("false", button.GetAttribute("aria-pressed"));
        Assert.Equal("off", button.GetAttribute("data-state"));
        Assert.Null(button.GetAttribute("role"));
        Assert.Contains("Bold", button.TextContent);
    }

    [Fact]
    public void TwToggle_Pressed_SetsAriaPressedAndDataState()
    {
        // Act
        var cut = TestContext.Render<TwToggle>(p => p.Add(x => x.Pressed, true));

        // Assert
        var button = cut.Find("button");
        Assert.Equal("true", button.GetAttribute("aria-pressed"));
        Assert.Equal("on", button.GetAttribute("data-state"));
    }

    [Fact]
    public void TwToggle_Click_TogglesAndRaisesPressedChanged()
    {
        // Arrange
        var raised = new List<bool>();
        var cut = TestContext.Render<TwToggle>(p => p
            .Add(x => x.PressedChanged, (bool value) => raised.Add(value)));

        // Act
        cut.Find("button").Click();
        cut.Find("button").Click();

        // Assert
        Assert.Equal([true, false], raised);
        Assert.Equal("false", cut.Find("button").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void TwToggle_Click_UpdatesItsOwnState_WithoutABinding()
    {
        // Arrange
        var cut = TestContext.Render<TwToggle>();

        // Act
        cut.Find("button").Click();

        // Assert
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-pressed"));
        AssertHasAll(theme.Pressed, ClassesOf(cut.Find("button")));
    }

    [Fact]
    public void TwToggle_Disabled_IsNativelyDisabled_AndDoesNotToggle()
    {
        // Arrange
        var raised = false;
        var cut = TestContext.Render<TwToggle>(p => p
            .Add(x => x.Disabled, true)
            .Add(x => x.PressedChanged, (bool _) => raised = true));

        // Act
        var button = cut.Find("button");

        // Assert
        Assert.True(button.HasAttribute("disabled"));
        AssertHasAll(theme.Disabled, ClassesOf(button));
        Assert.DoesNotContain(theme.Hover.Split(' ')[0], ClassesOf(button));
        Assert.False(raised);
    }

    [Fact]
    public void TwToggle_Unpressed_UsesUnpressedAndHoverClasses()
    {
        // Act
        var classes = ClassesOf(TestContext.Render<TwToggle>().Find("button"));

        // Assert
        AssertHasAll(theme.Unpressed, classes);
        AssertHasAll(theme.Hover, classes);
        Assert.DoesNotContain(theme.PressedForcedColors.Split(' ')[0], classes);
    }

    [Fact]
    public void TwToggle_Pressed_AddsForcedColorsClasses_AndDropsHover()
    {
        // Act
        var classes = ClassesOf(TestContext.Render<TwToggle>(p => p.Add(x => x.Pressed, true)).Find("button"));

        // Assert
        AssertHasAll(theme.PressedForcedColors, classes);
        AssertHasAll(theme.Pressed, classes);
        Assert.DoesNotContain(theme.Hover.Split(' ')[0], classes);
    }

    [Theory]
    [InlineData(Color.Primary)]
    [InlineData(Color.Danger)]
    [InlineData(Color.Success)]
    public void TwToggle_PressedWithColor_UsesThePaletteEntry(Color color)
    {
        // Act
        var classes = ClassesOf(TestContext.Render<TwToggle>(p => p
            .Add(x => x.Pressed, true)
            .Add(x => x.Color, color)).Find("button"));

        // Assert
        var expected = color switch
        {
            Color.Primary => theme.Colors.Primary,
            Color.Danger => theme.Colors.Danger,
            _ => theme.Colors.Success
        };
        AssertHasAll(expected, classes);
        Assert.DoesNotContain(theme.Pressed.Split(' ')[0], classes);
    }

    [Fact]
    public void TwToggle_Color_SetsTheFocusRingColor()
    {
        // Act
        var classes = ClassesOf(TestContext.Render<TwToggle>(p => p.Add(x => x.Color, Color.Danger)).Find("button"));

        // Assert
        AssertHasAll(Theme.Colors.FocusColors.Danger, classes);
    }

    [Fact]
    public void TwToggle_HasAVisibleFocusRing_ByDefault()
    {
        // Act
        var classes = ClassesOf(TestContext.Render<TwToggle>().Find("button"));

        // Assert
        AssertHasAll(Theme.Colors.FocusColors.Primary, classes);
        Assert.Contains("focus-visible:ring-2", classes);
    }

    [Fact]
    public void TwToggle_Outlined_AddsTheBorder()
    {
        // Act
        var classes = ClassesOf(TestContext.Render<TwToggle>(p => p.Add(x => x.Variant, ToggleVariant.Outlined)).Find("button"));

        // Assert
        AssertHasAll(theme.Outlined, classes);
    }

    [Fact]
    public void TwToggle_TextVariant_HasNoBorder()
    {
        // Act
        var classes = ClassesOf(TestContext.Render<TwToggle>().Find("button"));

        // Assert
        Assert.DoesNotContain(theme.Outlined.Split(' ')[0], classes);
    }

    [Theory]
    [InlineData(Size.Sm)]
    [InlineData(Size.Md)]
    [InlineData(Size.Lg)]
    public void TwToggle_Size_AppliesSizeAndIconClasses(Size size)
    {
        // Act
        var cut = TestContext.Render<TwToggle>(p => p
            .Add(x => x.Size, size)
            .Add(x => x.Icon, IconEnum.Type_Bold)
            .Add(x => x.AriaLabel, "Bold"));

        // Assert
        var (sizeClasses, iconClasses) = size switch
        {
            Size.Sm => (theme.Sm, theme.IconSm),
            Size.Lg => (theme.Lg, theme.IconLg),
            _ => (theme.Md, theme.IconMd)
        };
        AssertHasAll(sizeClasses, ClassesOf(cut.Find("button")));
        AssertHasAll(iconClasses, ClassesOf(cut.Find("i")));
    }

    [Fact]
    public void TwToggle_DefaultSize_IsMedium()
    {
        // Act
        var classes = ClassesOf(TestContext.Render<TwToggle>().Find("button"));

        // Assert
        AssertHasAll(theme.Md, classes);
    }

    [Fact]
    public void TwToggle_IconOnly_HasTheAriaLabel_AndADecorativeIcon()
    {
        // Act
        var cut = TestContext.Render<TwToggle>(p => p
            .Add(x => x.Icon, IconEnum.Type_Italic)
            .Add(x => x.AriaLabel, "Italic"));

        // Assert
        Assert.Equal("Italic", cut.Find("button").GetAttribute("aria-label"));
        Assert.Equal("true", cut.Find("i").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void TwToggle_ChildContent_ReplacesTheLabel()
    {
        // Act
        var cut = TestContext.Render<TwToggle>(p => p
            .Add(x => x.Label, "Label")
            .Add(x => x.ChildContent, (RenderFragment)(b => b.AddMarkupContent(0, "<strong>Custom</strong>"))));

        // Assert
        Assert.Contains("Custom", cut.Find("button strong").TextContent);
        Assert.DoesNotContain("Label", cut.Find("button").TextContent);
    }

    [Fact]
    public void TwToggle_Class_IsMergedLast_AndOverridesTheTheme()
    {
        // Act
        var classes = ClassesOf(TestContext.Render<TwToggle>(p => p
            .Add(x => x.Class, "h-12 custom-class")).Find("button"));

        // Assert
        Assert.Contains("custom-class", classes);
        Assert.Contains("h-12", classes);
        Assert.DoesNotContain("h-9", classes);
    }

    [Fact]
    public void TwToggle_DefaultRounding_FollowsTheTheme()
    {
        // Act
        var classes = ClassesOf(TestContext.Render<TwToggle>().Find("button"));

        // Assert
        Assert.Contains(RoundedBuilder.GetRounded(Theme.Rounded.DefaultRounded), classes);
    }

    [Fact]
    public void TwToggle_RoundedParameter_WinsOverTheTheme()
    {
        // Act
        var classes = ClassesOf(TestContext.Render<TwToggle>(p => p.Add(x => x.Rounded, Rounded.Full)).Find("button"));

        // Assert
        Assert.Contains(Theme.Rounded.Full, classes);
        Assert.DoesNotContain(Theme.Rounded.Lg, classes);
    }

    [Fact]
    public void TwToggle_ThemeToggleRounded_WinsOverTheGlobalDefault()
    {
        // Arrange
        theme.ToggleRounded = Rounded.None;

        // Act
        var classes = ClassesOf(TestContext.Render<TwToggle>().Find("button"));

        // Assert
        Assert.Contains(Theme.Rounded.None, classes);
    }

    [Fact]
    public void TwToggle_ThemeDefaultVariant_IsUsed_WhenNoVariantIsSet()
    {
        // Arrange
        theme.DefaultVariant = ToggleVariant.Outlined;

        // Act
        var classes = ClassesOf(TestContext.Render<TwToggle>().Find("button"));

        // Assert
        AssertHasAll(theme.Outlined, classes);
    }

    [Fact]
    public void TwToggle_Shadow_AddsTheShadowClass()
    {
        // Act
        var classes = ClassesOf(TestContext.Render<TwToggle>(p => p.Add(x => x.Shadow, Shadow.Lg)).Find("button"));

        // Assert
        Assert.Contains(Theme.Shadows.Lg, classes);
    }

    [Fact]
    public void TwToggle_IdStyleAndAttributes_ReachTheButton()
    {
        // Act
        var cut = TestContext.Render<TwToggle>(p => p
            .Add(x => x.Id, "my-toggle")
            .Add(x => x.Style, "color: red")
            .AddUnmatched("data-test", "value"));

        // Assert
        var button = cut.Find("button");
        Assert.Equal("my-toggle", button.Id);
        Assert.Equal("color: red", button.GetAttribute("style"));
        Assert.Equal("value", button.GetAttribute("data-test"));
    }

    [Fact]
    public void TwToggle_ConsumerCannotDesyncAriaPressed()
    {
        // Act
        var cut = TestContext.Render<TwToggle>(p => p
            .Add(x => x.Pressed, true)
            .AddUnmatched("aria-pressed", "false"));

        // Assert
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void TwToggle_Tooltip_IsLinkedWithAriaDescribedBy()
    {
        // Act
        var cut = TestContext.Render<TwToggle>(p => p
            .Add(x => x.Id, "bold")
            .Add(x => x.AriaLabel, "Bold")
            .Add(x => x.Tooltip, "Ctrl+B"));

        // Assert
        var tooltip = cut.Find("[role='tooltip']");
        Assert.Equal("Ctrl+B", tooltip.TextContent);
        Assert.Equal(tooltip.Id, cut.Find("button").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void TwToggle_HasForcedColorsSupportInItsBase()
    {
        // Act
        var classes = ClassesOf(TestContext.Render<TwToggle>().Find("button"));

        // Assert
        Assert.Contains("forced-colors:border", classes);
        Assert.Contains("motion-reduce:transition-none", classes);
    }
}
