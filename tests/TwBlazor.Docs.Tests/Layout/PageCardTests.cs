using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Docs.Layout;

namespace TwBlazor.Docs.Tests.Layout;

public class PageCardTests : DocsTestBase
{
    private static RenderFragment Body => builder => builder.AddMarkupContent(0, "<p id=\"body\">Body</p>");

    [Fact]
    public void Render_ShowsTheTitleAsASectionHeading()
    {
        // Arrange & Act
        var cut = TestContext.Render<PageCard>(parameters => parameters
            .Add(p => p.Title, "Colors")
            .Add(p => p.ChildContent, Body));

        // Assert
        var section = cut.Find("div");
        var heading = cut.Find("h2");
        Assert.Equal("Colors", heading.TextContent);
        Assert.Equal("Body", cut.Find("#body").TextContent);
    }

    [Fact]
    public void Render_IsARoundedShadowedSurface()
    {
        // Arrange & Act
        var cut = TestContext.Render<PageCard>(parameters => parameters
            .Add(p => p.Title, "Colors"));

        // Assert
        var classes = cut.Find("div").GetAttribute("class")!;
        Assert.Contains("shadow-sm", classes);
        Assert.Contains("rounded", classes);
    }

    [Fact]
    public void Render_UsesTheGivenId()
    {
        // Arrange & Act
        var cut = TestContext.Render<PageCard>(parameters => parameters
            .Add(p => p.Id, "theme-configuration")
            .Add(p => p.Title, "Theme Configuration"));

        // Assert
        Assert.Equal("theme-configuration", cut.Find("div").Id);
    }

    [Fact]
    public void Render_DerivesTheIdFromTheTitle_WhenNoneIsGiven()
    {
        // Arrange & Act
        var cut = TestContext.Render<PageCard>(parameters => parameters
            .Add(p => p.Title, "Dense Alerts"));

        // Assert
        Assert.Equal("dense-alerts", cut.Find("div").Id);
    }

    [Fact]
    public void Render_OmitsTheHeading_WhenThereIsNoTitle()
    {
        // Arrange & Act
        var cut = TestContext.Render<PageCard>(parameters => parameters
            .Add(p => p.ChildContent, Body));

        // Assert
        Assert.Empty(cut.FindAll("h2"));
    }

    [Fact]
    public void Render_RegistersItsTitleOnTheCascadedOutline()
    {
        // Arrange
        var outline = new PageOutline();

        // Act
        TestContext.Render<CascadingValue<PageOutline>>(parameters => parameters
            .Add(p => p.Value, outline)
            .Add(p => p.ChildContent, (RenderFragment)(builder =>
            {
                builder.OpenComponent<PageCard>(0);
                builder.AddAttribute(1, nameof(PageCard.Title), "Colors");
                builder.CloseComponent();
            })));

        // Assert
        var item = Assert.Single(outline.Items);
        Assert.Equal("Colors", item.Title);
        Assert.Equal("colors", item.Id);
    }

    [Fact]
    public void Dispose_RemovesTheCardFromTheOutline()
    {
        // Arrange
        var outline = new PageOutline();
        var host = TestContext.Render<CascadingValue<PageOutline>>(parameters => parameters
            .Add(p => p.Value, outline)
            .Add(p => p.ChildContent, (RenderFragment)(builder =>
            {
                builder.OpenComponent<PageCard>(0);
                builder.AddAttribute(1, nameof(PageCard.Title), "Colors");
                builder.CloseComponent();
            })));

        // Act
        host.Render(parameters => parameters
            .Add(p => p.Value, outline)
            .Add(p => p.ChildContent, (RenderFragment)(_ => { })));

        // Assert
        Assert.Empty(outline.Items);
    }
}
