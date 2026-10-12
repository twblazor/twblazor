using Bunit;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Link;

public class TwLinkTests : TwBlazorTestBase
{
    [Fact]
    public void TwLink_Renders_WithDefaultValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwLink>();

        // Assert
        var anchor = cut.Find("a");
        Assert.NotNull(anchor);
        Assert.Equal("", anchor.GetAttribute("href"));
        // Verify ID is auto-generated
        var id = anchor.GetAttribute("id");
        Assert.NotNull(id);
        Assert.StartsWith("link-", id);
    }

    [Fact]
    public void TwLink_Renders_WithHref()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwLink>(parameters => parameters
            .Add(p => p.Href, "https://example.com"));

        // Assert
        var anchor = cut.Find("a");
        Assert.Equal("https://example.com", anchor.GetAttribute("href"));
    }

    [Fact]
    public void TwLink_Renders_WithTarget()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwLink>(parameters => parameters
            .Add(p => p.Href, "/test")
            .Add(p => p.Target, "_blank"));

        // Assert
        var anchor = cut.Find("a");
        Assert.Equal("_blank", anchor.GetAttribute("target"));
    }

    [Fact]
    public void TwLink_Renders_WithId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwLink>(parameters => parameters
            .Add(p => p.Id, "test-link"));

        // Assert
        var anchor = cut.Find("a");
        Assert.Equal("test-link", anchor.GetAttribute("id"));
    }

    [Fact]
    public void TwLink_Renders_WithClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwLink>(parameters => parameters
            .Add(p => p.Class, "custom-class text-blue-500"));

        // Assert
        var anchor = cut.Find("a");
        Assert.Contains("custom-class", anchor.GetAttribute("class"));
        Assert.Contains("text-blue-500", anchor.GetAttribute("class"));
    }

    [Fact]
    public void TwLink_Renders_WithAriaLabel()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwLink>(parameters => parameters
            .Add(p => p.AriaLabel, "Navigate to home"));

        // Assert
        var anchor = cut.Find("a");
        Assert.Equal("Navigate to home", anchor.GetAttribute("aria-label"));
    }

    [Fact]
    public void TwLink_Renders_WithChildContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwLink>(parameters => parameters
            .Add(p => p.ChildContent, "<span>Click me</span>"));

        // Assert
        Assert.Equal("Click me", cut.Find("span").TextContent);
    }

    [Fact]
    public void TwLink_Renders_WithMultipleAttributes()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwLink>(parameters => parameters
            .Add(p => p.Href, "/dashboard")
            .Add(p => p.Target, "_self")
            .Add(p => p.Id, "nav-link")
            .Add(p => p.Class, "nav-item")
            .Add(p => p.AriaLabel, "Dashboard link"));

        // Assert
        var anchor = cut.Find("a");
        Assert.Equal("/dashboard", anchor.GetAttribute("href"));
        Assert.Equal("_self", anchor.GetAttribute("target"));
        Assert.Equal("nav-link", anchor.GetAttribute("id"));
        Assert.Contains("nav-item", anchor.GetAttribute("class"));
        Assert.Equal("Dashboard link", anchor.GetAttribute("aria-label"));
    }

    [Fact]
    public void TwLink_Renders_WithUnmatchedAttributes()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwLink>(parameters => parameters
            .Add(p => p.Href, "/test")
            .AddUnmatched("data-testid", "link-test")
            .AddUnmatched("title", "Test Link"));

        // Assert
        var anchor = cut.Find("a");
        Assert.Equal("link-test", anchor.GetAttribute("data-testid"));
        Assert.Equal("Test Link", anchor.GetAttribute("title"));
    }

    [Fact]
    public void TwLink_Renders_WithStyle()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwLink>(parameters => parameters
            .Add(p => p.Style, "color: red; font-weight: bold;"));

        // Assert
        var anchor = cut.Find("a");
        Assert.Equal("color: red; font-weight: bold;", anchor.GetAttribute("style"));
    }

    [Fact]
    public void TwLink_Renders_WithComplexChildContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwLink>(parameters => parameters
            .Add(p => p.Href, "/home")
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenElement(0, "i");
                builder.AddAttribute(1, "class", "icon");
                builder.CloseElement();
                builder.AddContent(2, " Home");
            }));

        // Assert
        var anchor = cut.Find("a");
        Assert.NotNull(anchor.QuerySelector("i"));
        Assert.Equal("Home", anchor.TextContent.Trim());
    }

    [Fact]
    public void TwLink_WithCustomClass_KeepsThePrimaryFocusRing()
    {
        // Arrange & Act - a custom Class replaces the link's default look, but must not leave it with the browser's own focus outline
        var cut = TestContext.Render<TwLink>(parameters => parameters
            .Add(p => p.Href, "/alert")
            .Add(p => p.Class, "underline"));

        // Assert
        var classes = cut.Find("a").ClassList;
        Assert.Contains("underline", classes);
        Assert.Contains("focus-visible:ring-2", classes);
        Assert.Contains("focus-visible:ring-purple-500", classes);
        Assert.Contains("focus:outline-hidden", classes);
    }

    [Fact]
    public void TwLink_WithCustomClass_LetsThatClassOverrideTheFocusRing()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwLink>(parameters => parameters
            .Add(p => p.Href, "/alert")
            .Add(p => p.Class, "focus-visible:ring-red-500"));

        // Assert
        var classes = cut.Find("a").ClassList;
        Assert.Contains("focus-visible:ring-red-500", classes);
        Assert.DoesNotContain("focus-visible:ring-purple-500", classes);
    }
}
