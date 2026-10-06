using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Tabs;

public class TwTabTests : TwBlazorTestBase
{
    [Fact]
    public void TwTab_Renders_WithLabel()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "My Tab");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Tab Content")));
                builder.CloseComponent();
            }));

        // Assert
        var button = cut.Find("button[role='tab']");
        Assert.Contains("My Tab", button.TextContent);
    }

    [Fact]
    public void TwTab_Renders_ChildContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "This is the tab content")));
                builder.CloseComponent();
            }));

        // Assert
        var tabpanel = cut.Find("div[role='tabpanel']");
        Assert.Contains("This is the tab content", tabpanel.TextContent);
    }

    [Fact]
    public void TwTab_AppliesColorFromParameter()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Red Tab");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content")));
                builder.AddAttribute(4, "Color", Color.Danger);
                builder.CloseComponent();
            }));

        // Assert
        var button = cut.Find("button[role='tab']");
        Assert.Contains("text-red", button.GetAttribute("class"));
    }

    [Fact]
    public void TwTab_InheritsColorFromParent_WhenNotSet()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.TabColor, Color.Primary)
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content")));
                builder.CloseComponent();
            }));

        // Assert
        var button = cut.Find("button[role='tab']");
        Assert.Contains("text-purple", button.GetAttribute("class"));
    }

    [Fact]
    public void TwTab_OverridesParentColor_WhenColorSet()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.TabColor, Color.Primary)
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content")));
                builder.AddAttribute(4, "Color", Color.Danger);
                builder.CloseComponent();
            }));

        // Assert
        var button = cut.Find("button[role='tab']");
        Assert.Contains("text-red", button.GetAttribute("class"));
        Assert.DoesNotContain("text-purple", button.GetAttribute("class"));
    }

    [Fact]
    public void TwTab_AppliesDisabledState()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content")));
                builder.AddAttribute(4, "Disabled", true);
                builder.CloseComponent();
            }));

        // Assert
        var button = cut.Find("button[role='tab']");
        Assert.Contains("opacity-40", button.GetAttribute("class"));
        Assert.Contains("cursor-not-allowed", button.GetAttribute("class"));
        Assert.True(button.HasAttribute("disabled"));
    }

    [Fact]
    public void TwTab_ActiveTab_HasCorrectClasses()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();

                builder.OpenComponent<TwTab>(4);
                builder.AddAttribute(5, "Label", "Tab 2");
                builder.AddAttribute(6, "ChildContent", (RenderFragment)(b => b.AddContent(7, "Content 2")));
                builder.CloseComponent();
            }));

        // Assert
        var buttons = cut.FindAll("button[role='tab']");
        var activeButton = buttons[0];

        // Active tab should have the underline indicator
        Assert.Contains("after:scale-x-100", activeButton.GetAttribute("class"));
    }

    [Fact]
    public void TwTab_InactiveTab_HasCorrectHoverClasses()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();

                builder.OpenComponent<TwTab>(4);
                builder.AddAttribute(5, "Label", "Tab 2");
                builder.AddAttribute(6, "ChildContent", (RenderFragment)(b => b.AddContent(7, "Content 2")));
                builder.CloseComponent();
            }));

        // Assert
        var buttons = cut.FindAll("button[role='tab']");
        var inactiveButton = buttons[1];

        // Inactive tab should have hover effects
        Assert.Contains("hover:text-[oklch(21%_0.006_285.885)]", inactiveButton.GetAttribute("class"));
        Assert.Contains("after:scale-x-0", inactiveButton.GetAttribute("class"));
        Assert.Contains("hover:after:scale-x-100", inactiveButton.GetAttribute("class"));
    }

    [Fact]
    public void TwTab_AppliesDensePadding_WhenParentIsDense()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.Dense, true)
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content")));
                builder.CloseComponent();
            }));

        // Assert
        var button = cut.Find("button[role='tab']");
        Assert.Contains("px-4", button.GetAttribute("class"));
    }

    [Fact]
    public void TwTab_AppliesNormalPadding_WhenParentIsNotDense()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.Dense, false)
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content")));
                builder.CloseComponent();
            }));

        // Assert
        var button = cut.Find("button[role='tab']");
        Assert.Contains("py-5 px-6", button.GetAttribute("class"));
    }

    [Fact]
    public void TwTab_AppliesBaseClasses()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content")));
                builder.CloseComponent();
            }));

        // Assert
        var button = cut.Find("button[role='tab']");
        var classAttribute = button.GetAttribute("class");

        Assert.Contains("relative", classAttribute);
        Assert.Contains("tracking-wide", classAttribute);
        Assert.Contains("font-medium", classAttribute);
        Assert.Contains("text-sm", classAttribute);
        Assert.Contains("transition-colors", classAttribute);
        Assert.Contains("duration-200", classAttribute);
    }

    [Fact]
    public void TwTab_AppliesAfterPseudoElementClasses()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content")));
                builder.CloseComponent();
            }));

        // Assert
        var button = cut.Find("button[role='tab']");
        var classAttribute = button.GetAttribute("class");

        // Check for after pseudo-element styles
        Assert.Contains("after:absolute", classAttribute);
        Assert.Contains("after:bottom-0", classAttribute);
        Assert.Contains("after:left-0", classAttribute);
        Assert.Contains("after:right-0", classAttribute);
        Assert.Contains("after:h-0.5", classAttribute);
        Assert.Contains("after:bg-current", classAttribute);
        Assert.Contains("after:transition-transform", classAttribute);
        Assert.Contains("after:duration-200", classAttribute);
    }

    [Fact]
    public void TwTab_MultipleTabsSwitching_WorksCorrectly()
    {
        // Arrange
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();

                builder.OpenComponent<TwTab>(4);
                builder.AddAttribute(5, "Label", "Tab 2");
                builder.AddAttribute(6, "ChildContent", (RenderFragment)(b => b.AddContent(7, "Content 2")));
                builder.CloseComponent();

                builder.OpenComponent<TwTab>(8);
                builder.AddAttribute(9, "Label", "Tab 3");
                builder.AddAttribute(10, "ChildContent", (RenderFragment)(b => b.AddContent(11, "Content 3")));
                builder.CloseComponent();
            }));

        var buttons = cut.FindAll("button[role='tab']");

        // Act & Assert - Switch to Tab 2
        buttons[1].Click();
        var tabpanel = cut.Find("div[role='tabpanel']");
        Assert.Contains("Content 2", tabpanel.TextContent);
        Assert.DoesNotContain("Content 1", tabpanel.TextContent);
        Assert.DoesNotContain("Content 3", tabpanel.TextContent);

        // Act & Assert - Switch to Tab 3
        buttons[2].Click();
        tabpanel = cut.Find("div[role='tabpanel']");
        Assert.Contains("Content 3", tabpanel.TextContent);
        Assert.DoesNotContain("Content 1", tabpanel.TextContent);
        Assert.DoesNotContain("Content 2", tabpanel.TextContent);

        // Act & Assert - Switch back to Tab 1
        buttons[0].Click();
        tabpanel = cut.Find("div[role='tabpanel']");
        Assert.Contains("Content 1", tabpanel.TextContent);
        Assert.DoesNotContain("Content 2", tabpanel.TextContent);
        Assert.DoesNotContain("Content 3", tabpanel.TextContent);
    }

    [Fact]
    public void TwTab_DisabledTab_CannotBeActivated()
    {
        // Arrange
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();

                builder.OpenComponent<TwTab>(4);
                builder.AddAttribute(5, "Label", "Tab 2");
                builder.AddAttribute(6, "ChildContent", (RenderFragment)(b => b.AddContent(7, "Content 2")));
                builder.AddAttribute(8, "Disabled", true);
                builder.CloseComponent();

                builder.OpenComponent<TwTab>(9);
                builder.AddAttribute(10, "Label", "Tab 3");
                builder.AddAttribute(11, "ChildContent", (RenderFragment)(b => b.AddContent(12, "Content 3")));
                builder.CloseComponent();
            }));

        var buttons = cut.FindAll("button[role='tab']");

        // Act - Try to click disabled tab
        buttons[1].Click();

        // Assert - Tab 1 should still be active
        var tabpanel = cut.Find("div[role='tabpanel']");
        Assert.Contains("Content 1", tabpanel.TextContent);
        Assert.DoesNotContain("Content 2", tabpanel.TextContent);

        // Act - Switch to Tab 3
        buttons[2].Click();
        tabpanel = cut.Find("div[role='tabpanel']");
        Assert.Contains("Content 3", tabpanel.TextContent);

        // Act - Try to click disabled tab again
        buttons[1].Click();

        // Assert - Tab 3 should still be active
        tabpanel = cut.Find("div[role='tabpanel']");
        Assert.Contains("Content 3", tabpanel.TextContent);
        Assert.DoesNotContain("Content 2", tabpanel.TextContent);
    }

    [Fact]
    public void TwTab_WithCustomClass_AppliesClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content")));
                builder.AddAttribute(4, "Class", "custom-tab-class");
                builder.CloseComponent();
            }));

        // Assert
        var button = cut.Find("button[role='tab']");
        Assert.Contains("custom-tab-class", button.GetAttribute("class"));
    }
}
