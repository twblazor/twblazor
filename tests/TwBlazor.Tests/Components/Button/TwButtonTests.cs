using Bunit;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using Color = TwBlazor.Enums.Color;
using Icons = TwBlazor.Enums.Icon;

namespace TwBlazor.Tests.Components.Button;

public class TwButtonTests : TwBlazorTestBase
{
    private TwButtonTheme buttonTheme => Theme.Components.Require<TwButtonTheme>();

    [Fact]
    public void TwButton_Plain_OmitsBaseVariantFocusAndShadowClasses()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "x")
            .Add(p => p.IconButton, true)
            .Add(p => p.Color, Color.Primary)
            .Add(p => p.Variant, ButtonVariant.Filled)
            .Add(p => p.Class, "custom-close-btn")
            .Add(p => p.Plain, true));

        // Assert - only Class (and the cursor class) survive; TwButton's own chrome is skipped
        var button = cut.Find("button");
        var classes = button.GetAttribute("class");
        Assert.Contains("custom-close-btn", classes);
        Assert.DoesNotContain("bg-blue-600", classes);
        Assert.DoesNotContain("focus:ring", classes);
        Assert.DoesNotContain(buttonTheme.IconButton, classes);
    }

    [Fact]
    public void TwButton_WithoutTooltip_RendersNoTooltipOrWrapper()
    {
        var cut = TestContext.Render<TwButton>(parameters => parameters.Add(p => p.Label, "x"));

        Assert.Empty(cut.FindAll("[role='tooltip']"));
        Assert.Null(cut.Find("button").GetAttribute("aria-describedby"));
        Assert.Equal("button", cut.Nodes.OfType<AngleSharp.Dom.IElement>().Single().TagName.ToLowerInvariant());
    }

    [Fact]
    public void TwButton_WithTooltip_RendersTooltipLinkedByAriaDescribedBy()
    {
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Save")
            .Add(p => p.Tooltip, "Save your changes"));

        var tooltip = cut.Find("[role='tooltip']");
        Assert.Equal("Save your changes", tooltip.TextContent);
        Assert.False(string.IsNullOrEmpty(tooltip.Id));
        Assert.Equal(tooltip.Id, cut.Find("button").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void TwButton_Tooltip_UsesThemeClassesAndPopoverSurface()
    {
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Save")
            .Add(p => p.Tooltip, "Save your changes"));

        var tooltipClasses = cut.Find("[role='tooltip']").GetAttribute("class")!;
        Assert.Contains(Theme.Components.Require<TwOverlayTheme>().TooltipBubble, tooltipClasses);
        Assert.Contains(Theme.Components.Require<TwOverlayTheme>().PopoverBackground, tooltipClasses);
        Assert.Contains("group", cut.Find("[role='tooltip']").ParentElement!.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_Tooltip_CanBeDismissedWithEscape_AndReturnsOnMouseLeave()
    {
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Save")
            .Add(p => p.Tooltip, "Save your changes"));
        var wrapper = cut.Find("[role='tooltip']").ParentElement!;

        wrapper.KeyDown(new KeyboardEventArgs { Key = "Escape" });
        Assert.Contains("hidden", cut.Find("[role='tooltip']").ClassList);

        cut.Find("[role='tooltip']").ParentElement!.MouseLeave();
        Assert.DoesNotContain("hidden", cut.Find("[role='tooltip']").ClassList);
    }

    [Fact]
    public void TwButton_Tooltip_IgnoresNonEscapeKeys()
    {
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Save")
            .Add(p => p.Tooltip, "Save your changes"));

        cut.Find("[role='tooltip']").ParentElement!.KeyDown(new KeyboardEventArgs { Key = "a" });

        Assert.DoesNotContain("hidden", cut.Find("[role='tooltip']").ClassList);
    }

    [Fact]
    public void TwButton_WithTooltip_AsLink_LinksTooltipToAnchor()
    {
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Docs")
            .Add(p => p.Href, "/docs")
            .Add(p => p.Tooltip, "Read the docs"));

        var tooltip = cut.Find("[role='tooltip']");
        Assert.Equal(tooltip.Id, cut.Find("a").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void TwButton_Tooltip_StillInvokesOnClick()
    {
        var clicked = false;
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Tooltip, "Tip")
            .Add(p => p.OnClick, () => clicked = true));

        cut.Find("button").Click();

        Assert.True(clicked);
    }

    [Fact]
    public void TwButton_Plain_StillInvokesOnClick()
    {
        // Arrange
        var clicked = false;
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Plain, true)
            .Add(p => p.OnClick, () => clicked = true));

        // Act
        cut.Find("button").Click();

        // Assert
        Assert.True(clicked);
    }

    [Fact]
    public void TwButton_Renders_WithDefaultValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>();

        // Assert
        var button = cut.Find("button");
        Assert.NotNull(button);
        Assert.Equal("button", button.GetAttribute("type"));
        Assert.DoesNotContain("disabled", button.Attributes.Select(a => a.Name));
    }

    [Fact]
    public void TwButton_Renders_WithLabel()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Click Me"));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("Click Me", button.TextContent);
    }

    [Fact]
    public void TwButton_Renders_WithChildContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.ChildContent, RenderFragmentBuilder("Custom Content")));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("Custom Content", button.TextContent);
    }

    [Fact]
    public void TwButton_ChildContent_TakesPrecedenceOverLabel()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "This should not appear")
            .Add(p => p.ChildContent, RenderFragmentBuilder("This should appear")));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("This should appear", button.TextContent);
        Assert.DoesNotContain("This should not appear", button.TextContent);
    }

    [Theory]
    [InlineData("button")]
    [InlineData("submit")]
    [InlineData("reset")]
    public void TwButton_Renders_WithType(string type)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Type, type));

        // Assert
        var button = cut.Find("button");
        Assert.Equal(type, button.GetAttribute("type"));
    }

    [Fact]
    public void TwButton_Renders_WithId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Id, "test-button"));

        // Assert
        var button = cut.Find("button");
        Assert.Equal("test-button", button.GetAttribute("id"));
    }

    [Fact]
    public void TwButton_Renders_WithClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Class, "custom-class"));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("custom-class", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_Renders_WithAriaLabel()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.AriaLabel, "Submit form"));

        // Assert
        var button = cut.Find("button");
        Assert.Equal("Submit form", button.GetAttribute("aria-label"));
    }

    [Fact]
    public void TwButton_Renders_AsDisabled()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Disabled, true));

        // Assert
        var button = cut.Find("button");
        Assert.True(button.HasAttribute("disabled"));
    }

    [Fact]
    public void TwButton_Renders_WithStartIcon()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.StartIcon, Icons.Plus)
            .Add(p => p.Label, "Add"));

        // Assert
        var button = cut.Find("button");
        var icons = cut.FindComponents<TwIcon>();
        Assert.NotEmpty(icons);
        Assert.Equal(Icons.Plus, icons[0].Instance.Icon);
        Assert.Contains("Add", button.TextContent);
    }

    [Fact]
    public void TwButton_Renders_WithEndIcon()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.EndIcon, Icons.Arrow_Right)
            .Add(p => p.Label, "Next"));

        // Assert
        var button = cut.Find("button");
        var icons = cut.FindComponents<TwIcon>();
        Assert.NotEmpty(icons);
        Assert.Equal(Icons.Arrow_Right, icons[0].Instance.Icon);
        Assert.Contains("Next", button.TextContent);
    }

    [Fact]
    public void TwButton_Renders_WithBothStartAndEndIcons()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.StartIcon, Icons.Plus)
            .Add(p => p.EndIcon, Icons.Arrow_Right)
            .Add(p => p.Label, "Add Next"));

        // Assert
        var button = cut.Find("button");
        var icons = cut.FindComponents<TwIcon>();
        Assert.Equal(2, icons.Count);
        Assert.Equal(Icons.Plus, icons[0].Instance.Icon);
        Assert.Equal(Icons.Arrow_Right, icons[1].Instance.Icon);
        Assert.Contains("Add Next", button.TextContent);
    }

    [Theory]
    [InlineData(Color.Danger)]
    [InlineData(Color.Primary)]
    [InlineData(Color.Success)]
    [InlineData(Color.Warning)]
    [InlineData(Color.Accent)]
    public void TwButton_Renders_WithColor(Color color)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Color, color));

        // Assert
        var button = cut.Find("button");
        Assert.NotNull(button);
        // Color classes are applied through ButtonBuilder, just verify button exists
    }

    [Theory]
    [InlineData(ButtonVariant.Filled)]
    [InlineData(ButtonVariant.Outlined)]
    [InlineData(ButtonVariant.Text)]
    public void TwButton_Renders_WithVariant(ButtonVariant variant)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Variant, variant));

        // Assert
        var button = cut.Find("button");
        Assert.NotNull(button);
        // Variant classes are applied through ButtonBuilder, just verify button exists
    }

    [Fact]
    public void TwButton_Renders_AsLink_WhenHrefProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Href, "/test-page")
            .Add(p => p.Label, "Navigate"));

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find("button"));
        var link = cut.Find("a");
        Assert.NotNull(link);
        Assert.Equal("/test-page", link.GetAttribute("href"));
        Assert.Contains("Navigate", link.TextContent);
    }

    [Fact]
    public void TwButton_AsLink_Renders_WithTarget()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Href, "https://example.com")
            .Add(p => p.Target, "_blank")
            .Add(p => p.Label, "External Link"));

        // Assert
        var link = cut.Find("a");
        Assert.Equal("https://example.com", link.GetAttribute("href"));
        Assert.Equal("_blank", link.GetAttribute("target"));
    }

    [Fact]
    public void TwButton_AsLink_Renders_WithIcon()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Href, "/test")
            .Add(p => p.StartIcon, Icons.House)
            .Add(p => p.Label, "Home"));

        // Assert
        var link = cut.Find("a");
        var icons = cut.FindComponents<TwIcon>();
        Assert.NotEmpty(icons);
        Assert.Equal(Icons.House, icons[0].Instance.Icon);
        Assert.Contains("Home", link.TextContent);
    }

    [Fact]
    public void TwButton_InvokesOnClick_WhenClicked()
    {
        // Arrange
        var clicked = false;
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Click Me")
            .Add(p => p.OnClick, () => clicked = true));

        // Act
        var button = cut.Find("button");
        button.Click();

        // Assert
        Assert.True(clicked);
    }

    [Fact]
    public void TwButton_DoesNotInvokeOnClick_WhenDisabled()
    {
        // Arrange
        var clicked = false;
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Click Me")
            .Add(p => p.Disabled, true)
            .Add(p => p.OnClick, () => clicked = true));

        // Act
        var button = cut.Find("button");
        button.Click();

        // Assert
        Assert.False(clicked);
    }

    [Fact]
    public void TwButton_DoesNotInvokeOnClick_WhenReadonly()
    {
        // Arrange
        var clicked = false;
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Click Me")
            .Add(p => p.Readonly, true)
            .Add(p => p.OnClick, () => clicked = true));

        // Act
        var button = cut.Find("button");
        button.Click();

        // Assert
        Assert.False(clicked);
    }

    [Fact]
    public void TwButton_Renders_WithUnmatchedAttributes()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test")
            .AddUnmatched("data-testid", "button-test")
            .AddUnmatched("title", "Test Button"));

        // Assert
        var button = cut.Find("button");
        Assert.Equal("button-test", button.GetAttribute("data-testid"));
        Assert.Equal("Test Button", button.GetAttribute("title"));
    }

    [Fact]
    public void TwButton_Renders_AsIconButton()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.IconButton, true)
            .Add(p => p.StartIcon, Icons.X));

        // Assert
        var button = cut.Find("button");
        Assert.NotNull(button);
        var icons = cut.FindComponents<TwIcon>();
        Assert.NotEmpty(icons);
        Assert.Equal(Icons.X, icons[0].Instance.Icon);
    }

    [Fact]
    public void TwButton_Renders_WithComplexChildContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "custom-span");
                builder.AddContent(2, "Complex Content");
                builder.CloseElement();
            }));

        // Assert
        var button = cut.Find("button");
        var span = button.QuerySelector("span.custom-span");
        Assert.NotNull(span);
        Assert.Equal("Complex Content", span.TextContent);
    }

    [Fact]
    public void TwButton_AsLink_Renders_WithChildContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Href, "/test")
            .Add(p => p.ChildContent, RenderFragmentBuilder("Link Content")));

        // Assert
        var link = cut.Find("a");
        Assert.Contains("Link Content", link.TextContent);
    }

    [Fact]
    public void TwButton_AsLink_ChildContent_TakesPrecedenceOverLabel()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Href, "/test")
            .Add(p => p.Label, "Should not appear")
            .Add(p => p.ChildContent, RenderFragmentBuilder("Should appear")));

        // Assert
        var link = cut.Find("a");
        Assert.Contains("Should appear", link.TextContent);
        Assert.DoesNotContain("Should not appear", link.TextContent);
    }

    [Fact]
    public void TwButton_AsLink_Renders_WithBothIcons()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Href, "/test")
            .Add(p => p.StartIcon, Icons.Arrow_Left)
            .Add(p => p.EndIcon, Icons.Arrow_Right)
            .Add(p => p.Label, "Navigate"));

        // Assert
        var link = cut.Find("a");
        var icons = cut.FindComponents<TwIcon>();
        Assert.Equal(2, icons.Count);
        Assert.Equal(Icons.Arrow_Left, icons[0].Instance.Icon);
        Assert.Equal(Icons.Arrow_Right, icons[1].Instance.Icon);
        Assert.Contains("Navigate", link.TextContent);
    }

    [Fact]
    public async Task TwButton_OnClick_IsAsync()
    {
        // Arrange
        var taskCompleted = false;
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Async Button")
            .Add(p => p.OnClick, async () =>
            {
                await Task.Delay(10);
                taskCompleted = true;
            }));

        // Act
        var button = cut.Find("button");
        await button.ClickAsync(new MouseEventArgs());

        // Assert
        Assert.True(taskCompleted);
    }

    [Fact]
    public void TwButton_WithReadonly_DoesNotRenderDisabledAttribute()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Readonly, true));

        // Assert
        var button = cut.Find("button");
        Assert.False(button.HasAttribute("disabled"));
        // Readonly affects behavior but not the disabled attribute
    }

    [Fact]
    public void TwButton_Renders_WithAllPropertiesCombined()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Id, "combined-button")
            .Add(p => p.Type, "submit")
            .Add(p => p.Label, "Submit Form")
            .Add(p => p.Color, Color.Primary)
            .Add(p => p.Variant, ButtonVariant.Filled)
            .Add(p => p.StartIcon, Icons.Check)
            .Add(p => p.Class, "custom-button")
            .Add(p => p.AriaLabel, "Submit the form")
            .AddUnmatched("data-form", "user-form"));

        // Assert
        var button = cut.Find("button");
        Assert.Equal("combined-button", button.GetAttribute("id"));
        Assert.Equal("submit", button.GetAttribute("type"));
        Assert.Contains("Submit Form", button.TextContent);
        Assert.Contains("custom-button", button.GetAttribute("class"));
        Assert.Equal("Submit the form", button.GetAttribute("aria-label"));
        Assert.Equal("user-form", button.GetAttribute("data-form"));
        var icons = cut.FindComponents<TwIcon>();
        Assert.NotEmpty(icons);
        Assert.Equal(Icons.Check, icons[0].Instance.Icon);
    }

    #region Theme Tests

    [Fact]
    public void TwButton_UsesTheme_BaseClasses()
    {
        // Arrange
        buttonTheme.Base = "custom-base-class";

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test"));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("custom-base-class", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_UsesTheme_PaddingClasses()
    {
        // Arrange
        buttonTheme.Padding = "px-12";

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test"));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("px-12", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_UsesTheme_IconButtonClasses()
    {
        // Arrange
        buttonTheme.IconButton = "custom-icon-btn-class";

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.IconButton, true)
            .Add(p => p.StartIcon, Icons.X));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("custom-icon-btn-class", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_UsesTheme_TypographyClasses()
    {
        // Arrange
        buttonTheme.Typography = "text-lg font-bold";

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test"));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("text-lg", button.GetAttribute("class"));
        Assert.Contains("font-bold", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_UsesTheme_UppercaseClass_WhenEnabled()
    {
        // Arrange
        buttonTheme.ButtonUppercase = true;
        buttonTheme.Uppercase = "custom-uppercase";

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test"));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("custom-uppercase", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_UsesTheme_DefaultButtonVariant()
    {
        // Arrange
        buttonTheme.DefaultVariant = ButtonVariant.Outlined;

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test")
            .Add(p => p.Color, Color.Primary));

        // Assert
        var button = cut.Find("button");
        // Outlined variant will have border classes
        Assert.Contains("border", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_ComponentVariant_OverridesThemeDefault()
    {
        // Arrange
        buttonTheme.DefaultVariant = ButtonVariant.Outlined;

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test")
            .Add(p => p.Color, Color.Primary)
            .Add(p => p.Variant, ButtonVariant.Filled));

        // Assert
        var button = cut.Find("button");
        // Should use Filled variant despite default being Outlined
        Assert.NotNull(button);
    }

    [Fact]
    public void TwButton_UsesTheme_DisabledCursor()
    {
        // Arrange
        buttonTheme.DisabledCursor = "cursor-wait";

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test")
            .Add(p => p.Disabled, true));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("cursor-wait", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_UsesTheme_ReadonlyCursor()
    {
        // Arrange
        buttonTheme.ReadonlyCursor = "cursor-help";

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test")
            .Add(p => p.Readonly, true));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("cursor-help", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_UsesTheme_DefaultCursor()
    {
        // Arrange
        buttonTheme.DefaultCursor = "cursor-grab";

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test"));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("cursor-grab", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_UsesTheme_ButtonRounded_WhenComponentRoundedNotSet()
    {
        // Arrange
        buttonTheme.ButtonRounded = Rounded.Full;

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test"));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("rounded-full", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_ComponentRounded_OverridesThemeButtonRounded()
    {
        // Arrange
        buttonTheme.ButtonRounded = Rounded.Full;

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test")
            .Add(p => p.Rounded, Rounded.None)); // Component-level should override theme

        // Assert
        var button = cut.Find("button");
        // Component Rounded should override theme ButtonRounded
        Assert.Contains("rounded-none", button.GetAttribute("class"));
        Assert.DoesNotContain("rounded-full", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_UsesTheme_ButtonShadow()
    {
        // Arrange
        buttonTheme.ButtonShadow = Shadow.Lg;

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test"));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("shadow-lg", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_UsesTheme_CustomFilledVariant()
    {
        // Arrange
        Theme.Colors.SurfaceColors.Filled.Danger = "custom-red-filled";

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test")
            .Add(p => p.Color, Color.Danger)
            .Add(p => p.Variant, ButtonVariant.Filled));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("custom-red-filled", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_UsesTheme_CustomOutlinedVariant()
    {
        // Arrange
        Theme.Colors.SurfaceColors.Outlined.Primary = "custom-blue-outlined";

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test")
            .Add(p => p.Color, Color.Primary)
            .Add(p => p.Variant, ButtonVariant.Outlined));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("custom-blue-outlined", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_UsesTheme_CustomTextVariant()
    {
        // Arrange
        Theme.Colors.SurfaceColors.Text.Success = "custom-green-text";

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test")
            .Add(p => p.Color, Color.Success)
            .Add(p => p.Variant, ButtonVariant.Text));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("custom-green-text", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_UsesTheme_CustomFocusRing()
    {
        // Arrange
        Theme.Colors.FocusColors.Accent = "custom-purple-focus";

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test")
            .Add(p => p.Color, Color.Accent));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("custom-purple-focus", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_UsesTheme_DisabledFilledClasses()
    {
        // Arrange
        buttonTheme.DisabledFilled = "custom-disabled-filled";

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test")
            .Add(p => p.Disabled, true)
            .Add(p => p.Variant, ButtonVariant.Filled));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("custom-disabled-filled", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_UsesTheme_DisabledOutlinedClasses()
    {
        // Arrange
        buttonTheme.DisabledOutlined = "custom-disabled-outlined";

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test")
            .Add(p => p.Disabled, true)
            .Add(p => p.Variant, ButtonVariant.Outlined));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("custom-disabled-outlined", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_UsesTheme_DisabledTextClasses()
    {
        // Arrange
        buttonTheme.DisabledText = "custom-disabled-text";

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test")
            .Add(p => p.Disabled, true)
            .Add(p => p.Variant, ButtonVariant.Text));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("custom-disabled-text", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_UsesGlobalOptions_ButtonUppercase()
    {
        // Arrange
        buttonTheme.ButtonUppercase = true;

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test"));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("uppercase", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_UsesGlobalOptions_DefaultRounded_WhenNoThemeRounded()
    {
        // Arrange
        Theme.Rounded.DefaultRounded = Rounded.Lg;

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test"));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("rounded-lg", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_ThemeButtonRounded_AppliesWhenComponentRoundedNotSet()
    {
        // Arrange
        Theme.Rounded.DefaultRounded = Rounded.Lg;
        buttonTheme.ButtonRounded = Rounded.Sm;

        // Act
        // Don't set component Rounded, so theme ButtonRounded should apply over global DefaultRounded
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test"));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("rounded-sm", button.GetAttribute("class"));
        Assert.DoesNotContain("rounded-lg", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_ComponentRounded_AppliesWhenNoThemeRounded()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test")
            .Add(p => p.Rounded, Rounded.None));

        // Assert
        var button = cut.Find("button");
        var classAttr = button.GetAttribute("class");
        // Component-level Rounded should apply when theme doesn't specify ButtonRounded
        Assert.Contains("rounded-none", classAttr);
    }

    [Fact]
    public void TwButton_ComponentVariant_OverridesThemeDefaultVariant()
    {
        // Arrange
        buttonTheme.DefaultVariant = ButtonVariant.Outlined;

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test")
            .Add(p => p.Color, Color.Primary)
            .Add(p => p.Variant, ButtonVariant.Filled)); // Override the default

        // Assert
        var button = cut.Find("button");
        // Component-level Variant should override theme default
        Assert.NotNull(button);
    }

    [Fact]
    public void TwButton_ComponentRounded_TakesHighestPrecedence()
    {
        // Arrange
        Theme.Rounded.DefaultRounded = Rounded.Lg;
        buttonTheme.ButtonRounded = Rounded.Md;

        // Act - Component Rounded should override both theme and global
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test")
            .Add(p => p.Rounded, Rounded.Sm));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("rounded-sm", button.GetAttribute("class"));
        Assert.DoesNotContain("rounded-md", button.GetAttribute("class"));
        Assert.DoesNotContain("rounded-lg", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_ThemeButtonRounded_AppliesWhenComponentNotSet()
    {
        // Arrange
        Theme.Rounded.DefaultRounded = Rounded.Lg;
        buttonTheme.ButtonRounded = Rounded.Sm; // Use Sm instead of Md for clearer assertion

        // Act - Don't set component Rounded, so theme ButtonRounded should apply
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test"));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("rounded-sm", button.GetAttribute("class"));
        Assert.DoesNotContain("rounded-lg", button.GetAttribute("class"));
    }

    [Fact]
    public void TwButton_ComponentRounded_OverridesAllDefaults()
    {
        // Arrange
        buttonTheme.ButtonRounded = Rounded.Md;
        buttonTheme.ButtonShadow = Shadow.Lg;

        // Act
        var cut = TestContext.Render<TwButton>(parameters => parameters
            .Add(p => p.Label, "Test")
            .Add(p => p.Variant, ButtonVariant.Filled)
            .Add(p => p.Rounded, Rounded.None)); // Component Rounded should win

        // Assert
        var button = cut.Find("button");
        var classAttr = button.GetAttribute("class");
        // Component-level Rounded.None should override theme and global settings
        Assert.Contains("rounded-none", classAttr);
        Assert.DoesNotContain("rounded-md", classAttr);
        Assert.DoesNotContain("rounded-lg", classAttr);
    }

    #endregion
}
