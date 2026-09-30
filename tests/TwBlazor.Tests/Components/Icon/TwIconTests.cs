using Bunit;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using Icons = TwBlazor.Enums.Icon;

namespace TwBlazor.Tests.Components.Icon;

public class TwIconTests : TwBlazorTestBase
{
    private TwButtonTheme buttonTheme => Theme.Components.Require<TwButtonTheme>();
    private TwIconTheme iconTheme => Theme.Components.Require<TwIconTheme>();

    [Fact]
    public void TwIcon_Renders_WithDefaultValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwIcon>();

        // Assert
        var icon = cut.Find("i");
        Assert.Contains("bi bi-", icon.GetAttribute("class"));
        Assert.Equal("true", icon.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void TwIcon_RendersButton_WithVariantAndColorApplied()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.Sun)
            .Add(p => p.Color, Color.Primary)
            .Add(p => p.ButtonVariant, ButtonVariant.Filled)
            .Add(p => p.OnClick, () => { }));

        // Assert - Color must reach the wrapped TwButton, or Filled (and other
        // variants that key off Color) render with no variant classes at all.
        var button = cut.Find("button");
        var classes = button.GetAttribute("class");
        Assert.Contains("bg-purple-600", classes);
    }

    [Fact]
    public void TwIcon_Renders_WithSpecificIcon()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.House));

        // Assert
        var icon = cut.Find("i");
        Assert.Contains("bi-house", icon.GetAttribute("class"));
    }

    [Fact]
    public void TwIcon_Renders_WithColor()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.Heart)
            .Add(p => p.Color, Color.Danger));

        // Assert
        var icon = cut.Find("i");
        Assert.Contains("text-red", icon.GetAttribute("class"));
    }

    [Fact]
    public void TwIcon_Renders_WithId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.Gear)
            .Add(p => p.Id, "settings-icon"));

        // Assert
        var icon = cut.Find("i");
        Assert.Equal("settings-icon", icon.GetAttribute("id"));
    }

    [Fact]
    public void TwIcon_Renders_WithClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.Star)
            .Add(p => p.Class, "custom-icon-class"));

        // Assert
        var icon = cut.Find("i");
        Assert.Contains("custom-icon-class", icon.GetAttribute("class"));
    }

    [Fact]
    public void TwIcon_Renders_WithOnClickDelegate()
    {
        // Arrange
        var clicked = false;

        // Act
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.Trash)
            .Add(p => p.OnClick, () => clicked = true));

        // Assert - should render TwButton when OnClick has delegate
        var button = cut.Find("button");
        Assert.NotNull(button);

        var icon = cut.Find("i");
        Assert.Contains("bi-trash", icon.GetAttribute("class"));

        // Act - trigger click
        button.Click();

        // Assert - callback was invoked
        Assert.True(clicked);
    }

    [Fact]
    public void TwIcon_Renders_WithoutOnClickDelegate_NoButton()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.Check));

        // Assert - should not render button
        Assert.Throws<ElementNotFoundException>(() => cut.Find("button"));

        // Should render icon directly
        var icon = cut.Find("i");
        Assert.NotNull(icon);
    }

    [Fact]
    public void TwIcon_Renders_WithCursorPointer_WhenOnClickProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.Pencil)
            .Add(p => p.OnClick, () => { }));

        // Assert - button should be present (cursor-pointer handled by TwButton)
        var button = cut.Find("button");
        Assert.NotNull(button);
    }

    [Fact]
    public void TwIcon_Renders_WithOnMouseOver()
    {
        // Arrange
        var mouseOverTriggered = false;

        // Act
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.Info)
            .Add(p => p.OnMouseOver, () => mouseOverTriggered = true));

        // Assert
        var icon = cut.Find("i");
        icon.MouseOver();
        Assert.True(mouseOverTriggered);
    }

    [Fact]
    public void TwIcon_Renders_WithAriaLabel_WhenOnClickProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.X)
            .Add(p => p.OnClick, () => { })
            .Add(p => p.AriaLabel, "Close dialog"));

        // Assert - AriaLabel should be on button
        var button = cut.Find("button");
        Assert.Equal("Close dialog", button.GetAttribute("aria-label"));
    }

    [Fact]
    public void TwIcon_Renders_WithUnmatchedAttributes()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.Download)
            .AddUnmatched("data-testid", "download-icon")
            .AddUnmatched("title", "Download file"));

        // Assert
        var icon = cut.Find("i");
        Assert.Equal("download-icon", icon.GetAttribute("data-testid"));
        Assert.Equal("Download file", icon.GetAttribute("title"));
    }

    [Fact]
    public void TwIcon_Renders_WithMultipleParameters()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.Bell)
            .Add(p => p.Color, Color.Primary)
            .Add(p => p.Id, "notification-icon")
            .Add(p => p.Class, "notification-badge"));

        // Assert
        var icon = cut.Find("i");
        Assert.Equal("notification-icon", icon.GetAttribute("id"));
        Assert.Contains("bi-bell", icon.GetAttribute("class"));
        Assert.Contains("text-purple", icon.GetAttribute("class"));
        Assert.Contains("notification-badge", icon.GetAttribute("class"));
    }

    [Fact]
    public void TwIcon_Renders_WithOnClickAndOnMouseOver()
    {
        // Arrange
        var clicked = false;
        var mouseOver = false;

        // Act
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.Search)
            .Add(p => p.OnClick, () => clicked = true)
            .Add(p => p.OnMouseOver, () => mouseOver = true));

        // Assert
        var button = cut.Find("button");
        var icon = cut.Find("i");

        button.Click();
        icon.MouseOver();

        Assert.True(clicked);
        Assert.True(mouseOver);
    }

    [Fact]
    public void TwIcon_Renders_WithColorAndClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.Check)
            .Add(p => p.Color, Color.Success)
            .Add(p => p.Class, "success-icon large"));

        // Assert
        var icon = cut.Find("i");
        var classes = icon.GetAttribute("class");
        Assert.Contains("text-green", classes);
        Assert.Contains("success-icon", classes);
        Assert.Contains("large", classes);
    }

    [Fact]
    public void TwIcon_RendersButton_WithClassParameter()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.Trash)
            .Add(p => p.Class, "delete-btn")
            .Add(p => p.OnClick, () => { }));

        // Assert - Class styles the interactive button (the focusable/hoverable element),
        // not the inner icon glyph, so the button's hit area matches its visual size.
        var button = cut.Find("button");
        Assert.Contains("delete-btn", button.GetAttribute("class"));
    }

    [Fact]
    public void TwIcon_RendersButton_Plain_OmitsButtonChrome()
    {
        // Arrange & Act - mirrors TwChip's close button usage
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.X)
            .Add(p => p.Class, "close-btn")
            .Add(p => p.Plain, true)
            .Add(p => p.OnClick, () => { }));

        // Assert - the wrapped button keeps the caller's Class but skips TwButton's
        // default sizing/variant/focus-ring chrome, so it doesn't look out of place
        // inside an already-styled control like a chip.
        var button = cut.Find("button");
        var classes = button.GetAttribute("class");
        Assert.Contains("close-btn", classes);
        Assert.DoesNotContain(buttonTheme.IconButton, classes);
    }

    [Fact]
    public void TwIcon_RendersButton_WithClassAndRootClass_Combined()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.Trash)
            .Add(p => p.Class, "delete-btn")
            .Add(p => p.RootClass, "absolute right-3")
            .Add(p => p.OnClick, () => { }));

        // Assert - both RootClass (layout hook) and Class (styling) end up on the button
        var button = cut.Find("button");
        var classes = button.GetAttribute("class");
        Assert.Contains("delete-btn", classes);
        Assert.Contains("absolute right-3", classes);

        // The icon glyph itself stays bare
        var icon = cut.Find("i");
        Assert.DoesNotContain("delete-btn", icon.GetAttribute("class"));
    }

    [Fact]
    public void TwIcon_Renders_DifferentIcons()
    {
        // Test multiple icon types to ensure enum conversion works
        var icons = new[] { Icons.House, Icons.Heart, Icons.Star, Icons.Gear, Icons.Trash };

        foreach (var icon in icons)
        {
            // Arrange & Act
            var cut = TestContext.Render<TwIcon>(parameters => parameters
                .Add(p => p.Icon, icon));

            // Assert
            var iconElement = cut.Find("i");
            Assert.Contains("bi-", iconElement.GetAttribute("class"));
        }
    }

    [Theory]
    [InlineData(Color.Light, "bg-gray-100", "text-gray-950")]
    [InlineData(Color.Dark, "bg-gray-900", "text-gray-100")]
    public void TwIcon_RendersButton_Filled_IconGlyphInheritsButtonTextColor_InsteadOfOverridingIt(Color color, string expectedButtonBg, string expectedButtonText)
    {
        // Arrange & Act - regression test: the glyph used to carry its own GetTextColor class on
        // top of the button's Filled variant classes. For Light/Dark that forced the same tone as
        // the button's own background (e.g. text-gray-100 on a bg-gray-100 button), making the icon
        // disappear. The glyph must now inherit its color from the button via currentColor instead.
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.Sun)
            .Add(p => p.Color, color)
            .Add(p => p.ButtonVariant, ButtonVariant.Filled)
            .Add(p => p.OnClick, () => { }));

        // Assert - the button carries the correct paired background/text classes...
        var button = cut.Find("button");
        var buttonClasses = button.GetAttribute("class");
        Assert.Contains(expectedButtonBg, buttonClasses);
        Assert.Contains(expectedButtonText, buttonClasses);

        // ...and the glyph itself carries no text color class that could conflict with it.
        var icon = cut.Find("i");
        var iconClasses = icon.GetAttribute("class");
        Assert.DoesNotContain("text-", iconClasses);
    }

    [Fact]
    public void TwIcon_RendersButton_WithHoverAndPulseClasses()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.Trash)
            .Add(p => p.OnClick, () => { }));

        // Assert - default (non-Plain, non-Disabled) icon buttons get the circular
        // hover state-layer and press pulse, even with no Color set (where the button's own
        // variant/color classes are empty).
        var button = cut.Find("button");
        var classes = button.GetAttribute("class");
        Assert.Contains(iconTheme.HoverBackground, classes);
        Assert.Contains(iconTheme.Pulse, classes);
    }

    [Fact]
    public void TwIcon_RendersButton_WithAbsoluteRootClass_OmitsConflictingRelative()
    {
        // Arrange & Act - RootClass positioning callers (e.g. TwCodeBlock's copy button, which passes
        // "absolute right-3 mt-2") must keep their own "absolute": Tailwind's ".relative" and
        // ".absolute" rules have equal specificity, so if the pulse's own "relative" were kept
        // alongside it, whichever rule is later in the generated stylesheet would silently win and
        // could break the caller's intended overlay positioning.
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.Copy)
            .Add(p => p.RootClass, "absolute right-3 mt-2")
            .Add(p => p.OnClick, () => { }));

        // Assert
        var button = cut.Find("button");
        var classes = button.GetAttribute("class");
        Assert.Contains("absolute", classes);
        Assert.Contains("tw-icon-pulse", classes);
        Assert.DoesNotContain("relative", classes);
    }

    [Fact]
    public void TwIcon_RendersButton_Plain_OmitsHoverAndPulseClasses()
    {
        // Arrange & Act - Plain hands full control of appearance to the caller (e.g. a chip's close
        // button, which already themes its own hover), so it shouldn't also get the icon-button pulse.
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.X)
            .Add(p => p.Plain, true)
            .Add(p => p.OnClick, () => { }));

        // Assert
        var button = cut.Find("button");
        var classes = button.GetAttribute("class");
        Assert.DoesNotContain(iconTheme.HoverBackground, classes);
        Assert.DoesNotContain(iconTheme.Pulse, classes);
    }

    [Fact]
    public void TwIcon_RendersButton_Disabled_OmitsHoverAndPulseClasses()
    {
        // Arrange & Act - a disabled icon button shouldn't hint at interactivity it doesn't have.
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.Trash)
            .Add(p => p.Disabled, true)
            .Add(p => p.OnClick, () => { }));

        // Assert
        var button = cut.Find("button");
        var classes = button.GetAttribute("class");
        Assert.DoesNotContain(iconTheme.HoverBackground, classes);
        Assert.DoesNotContain(iconTheme.Pulse, classes);
    }

    [Fact]
    public void TwIcon_Renders_WithNullColor()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwIcon>(parameters => parameters
            .Add(p => p.Icon, Icons.Info)
            .Add(p => p.Color, null));

        // Assert
        var icon = cut.Find("i");
        Assert.NotNull(icon);
        Assert.Contains("bi-info", icon.GetAttribute("class"));
    }

    [Fact]
    public void ShouldUseIconColorTheme_NotTextColors_WhenColorIsSet()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwIcon>(p => p
            .Add(x => x.Icon, TwBlazor.Enums.Icon.Star)
            .Add(x => x.Color, Color.Warning));

        // Assert - yellow-800 (the body text color) rendered as brown; icons use a vivid gold that still clears 3:1 on white.
        var classes = cut.Find("i").GetAttribute("class");
        Assert.Contains("text-[oklch(65%_0.15_80)]", classes);
        Assert.Contains("dark:text-yellow-400", classes);
        Assert.DoesNotContain("text-yellow-800", classes);
    }
}
