using AngleSharp.Dom;
using Bunit;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Collapse;

public class TwCollapseTests : TwBlazorTestBase
{
    [Fact]
    public void TwCollapse_Renders_WithDefaultValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCollapse>();

        // Assert
        var container = cut.Find("div.tw-collapse");
        Assert.NotNull(container);
        Assert.Contains("tw-collapse", container.GetAttribute("class"));
    }

    [Fact]
    public void TwCollapse_GeneratesId_WhenNotProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCollapse>();

        // Assert
        var container = cut.Find("div.tw-collapse");
        var id = container.GetAttribute("id");
        Assert.NotNull(id);
        Assert.StartsWith("collapse-", id);
    }

    [Fact]
    public void TwCollapse_UsesProvidedId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCollapse>(parameters => parameters
            .Add(p => p.Id, "my-collapse"));

        // Assert
        var container = cut.Find("div.tw-collapse");
        Assert.Equal("my-collapse", container.GetAttribute("id"));
    }

    [Fact]
    public void TwCollapse_Renders_WithTitle()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCollapse>(parameters => parameters
            .Add(p => p.Title, "Section Title"));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("Section Title", button.TextContent);
    }

    [Fact]
    public void TwCollapse_Renders_WithHeaderContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCollapse>(parameters => parameters
            .Add(p => p.HeaderContent, RenderFragmentBuilder("Custom Header")));

        // Assert
        var button = cut.Find("button");
        Assert.Contains("Custom Header", button.TextContent);
    }

    [Fact]
    public void TwCollapse_Renders_WithChildContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCollapse>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.ChildContent, RenderFragmentBuilder("Collapsible content")));

        // Assert
        var content = cut.Find("div[role='group']");
        Assert.Contains("Collapsible content", content.TextContent);
    }

    [Fact]
    public void TwCollapse_IsClosed_ByDefault()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCollapse>(parameters => parameters
            .Add(p => p.ChildContent, RenderFragmentBuilder("Content")));

        // Assert
        var content = cut.Find("div[role='group']");
        var classes = content.GetAttribute("class")?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
        Assert.Contains("hidden", classes);
    }

    [Fact]
    public void TwCollapse_IsOpen_ShowsContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCollapse>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.ChildContent, RenderFragmentBuilder("Visible content")));

        // Assert
        var content = cut.Find("div[role='group']");
        var classes = content.GetAttribute("class")?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
        Assert.DoesNotContain("hidden", classes);
    }

    [Fact]
    public void TwCollapse_Trigger_TogglesOpen_WhenClosed()
    {
        // Arrange
        var cut = TestContext.Render<TwCollapse>(parameters => parameters
            .Add(p => p.Title, "Click Me"));

        // Act
        cut.Find("button").Click();

        // Assert
        var content = cut.Find("div[role='group']");
        var classes = content.GetAttribute("class")?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
        Assert.DoesNotContain("hidden", classes);
    }

    [Fact]
    public void TwCollapse_Trigger_TogglesClosed_WhenOpen()
    {
        // Arrange
        var cut = TestContext.Render<TwCollapse>(parameters => parameters
            .Add(p => p.IsOpen, true));

        // Act
        cut.Find("button").Click();

        // Assert
        var content = cut.Find("div[role='group']");
        var classes = content.GetAttribute("class")?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
        Assert.Contains("hidden", classes);
    }

    [Fact]
    public void TwCollapse_Trigger_HasAriaExpandedFalse_WhenClosed()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCollapse>(parameters => parameters
            .Add(p => p.IsOpen, false));

        // Assert
        var button = cut.Find("button");
        Assert.Equal("false", button.GetAttribute("aria-expanded"));
    }

    [Fact]
    public void TwCollapse_Trigger_HasAriaExpandedTrue_WhenOpen()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCollapse>(parameters => parameters
            .Add(p => p.IsOpen, true));

        // Assert
        var button = cut.Find("button");
        Assert.Equal("true", button.GetAttribute("aria-expanded"));
    }

    [Fact]
    public void TwCollapse_ContentPanel_HasCorrectAriaAttributes()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCollapse>(parameters => parameters
            .Add(p => p.Id, "test-collapse"));

        // Assert
        var content = cut.Find("div[role='group']");
        Assert.Equal("test-collapse-content", content.GetAttribute("id"));
        Assert.Equal("test-collapse-trigger", content.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void TwCollapse_Trigger_HasCorrectAriaControls()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCollapse>(parameters => parameters
            .Add(p => p.Id, "test-collapse"));

        // Assert
        var button = cut.Find("button");
        Assert.Equal("test-collapse-content", button.GetAttribute("aria-controls"));
    }

    [Fact]
    public void TwCollapse_InvokesIsOpenChanged_WhenToggled()
    {
        // Arrange
        bool? capturedValue = null;
        var cut = TestContext.Render<TwCollapse>(parameters => parameters
            .Add(p => p.IsOpenChanged, (bool v) => capturedValue = v));

        // Act
        cut.Find("button").Click();

        // Assert
        Assert.True(capturedValue);
    }

    [Fact]
    public void TwCollapse_InvokesOnToggle_WhenToggled()
    {
        // Arrange
        bool? capturedValue = null;
        var cut = TestContext.Render<TwCollapse>(parameters => parameters
            .Add(p => p.OnToggle, (bool v) => capturedValue = v));

        // Act
        cut.Find("button").Click();

        // Assert
        Assert.True(capturedValue);
    }

    [Fact]
    public void TwCollapse_InvokesOnToggle_WithFalse_WhenClosedFromOpen()
    {
        // Arrange
        bool? capturedValue = null;
        var cut = TestContext.Render<TwCollapse>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnToggle, (bool v) => capturedValue = v));

        // Act
        cut.Find("button").Click();

        // Assert
        Assert.False(capturedValue);
    }

    [Fact]
    public void TwCollapse_ChevronIcon_RotatesWhenOpen()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCollapse>(parameters => parameters
            .Add(p => p.IsOpen, true));

        // Assert
        var icon = cut.FindComponent<TwIcon>();
        Assert.Contains("rotate-180", icon.Find("*").GetAttribute("class"));
    }

    [Fact]
    public void TwCollapse_ChevronIcon_DoesNotRotate_WhenClosed()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCollapse>(parameters => parameters
            .Add(p => p.IsOpen, false));

        // Assert
        var icon = cut.FindComponent<TwIcon>();
        Assert.DoesNotContain("rotate-180", icon.Find("*").GetAttribute("class") ?? string.Empty);
    }

    [Fact]
    public void TwCollapse_Renders_WithCustomClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCollapse>(parameters => parameters
            .Add(p => p.Class, "my-custom-class"));

        // Assert
        var container = cut.Find("div.tw-collapse");
        Assert.Contains("my-custom-class", container.GetAttribute("class"));
    }

    [Fact]
    public void TwCollapse_Renders_WithRoundedClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCollapse>();

        // Assert
        var container = cut.Find("div.tw-collapse");
        Assert.Contains("rounded", container.GetAttribute("class"));
    }

    [Fact]
    public void TwCollapse_Trigger_UpdatesAriaExpanded_AfterToggle()
    {
        // Arrange
        var cut = TestContext.Render<TwCollapse>(parameters => parameters
            .Add(p => p.IsOpen, false));

        // Act
        cut.Find("button").Click();

        // Assert
        var button = cut.Find("button");
        Assert.Equal("true", button.GetAttribute("aria-expanded"));
    }
}
