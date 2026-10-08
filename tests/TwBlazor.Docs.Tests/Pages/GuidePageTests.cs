using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Docs.Pages.Guides;
using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Tests.Pages;

public class GuidePageTests : DocsTestBase
{
    private (IRenderedComponent<TComponent> Page, IRenderedComponent<HeadOutlet> Head) Render<TComponent>()
        where TComponent : IComponent
    {
        var head = TestContext.Render<HeadOutlet>();
        var page = TestContext.Render<TComponent>();
        head.WaitForState(() => head.Markup.Contains("canonical"));
        return (page, head);
    }

    [Fact]
    public void TailwindBlazor_TargetsTheTailwindBlazorSearchTerm()
    {
        // Arrange & Act
        var (page, head) = Render<TailwindBlazor>();

        // Assert
        var title = head.Find("title").TextContent;
        Assert.Contains("Tailwind Blazor", title);
        Assert.True(title.Length <= SiteMetadata.MaxTitleLength, title);
        Assert.Equal("https://twblazor.com/tailwind-blazor", head.Find("link[rel='canonical']").GetAttribute("href"));
        Assert.Contains("Tailwind", page.Find("h1").TextContent);
    }

    [Fact]
    public void BlazorComponentLibraryComparison_TargetsTheComponentsLibrarySearchTerm()
    {
        // Arrange & Act
        var (page, head) = Render<BlazorComponentLibraryComparison>();

        // Assert
        var title = head.Find("title").TextContent;
        Assert.Contains("Blazor Components Library", title);
        Assert.True(title.Length <= SiteMetadata.MaxTitleLength, title);
        Assert.Equal("https://twblazor.com/blazor-component-library-comparison", head.Find("link[rel='canonical']").GetAttribute("href"));
        Assert.Equal("Blazor component library comparison", page.Find("h1").TextContent);
    }

    [Fact]
    public void BlazorComponentLibraryComparison_ListsTheLibrariesInAnAccessibleTable()
    {
        // Arrange & Act
        var (page, _) = Render<BlazorComponentLibraryComparison>();

        // Assert
        Assert.NotEmpty(page.Find("table caption").TextContent);
        Assert.All(page.FindAll("thead th"), th => Assert.Equal("col", th.GetAttribute("scope")));
        Assert.All(page.FindAll("tbody th"), th => Assert.Equal("row", th.GetAttribute("scope")));
        Assert.Contains(page.FindAll("tbody th"), th => th.TextContent == "twblazor");
    }

    [Fact]
    public void BlazorComponentLibraryComparison_HighlightsTwBlazorAsMitAndTailwindNative()
    {
        // Arrange & Act
        var (page, _) = Render<BlazorComponentLibraryComparison>();

        // Assert
        var cells = page.FindAll("#library-features tbody tr")
            .First(r => r.QuerySelector("th")!.TextContent == "twblazor")
            .QuerySelectorAll("td");
        Assert.Equal("MIT", cells[0].TextContent);
        Assert.All(cells.Skip(1), c => Assert.Contains("Yes", c.QuerySelector(".sr-only")!.TextContent));
    }

    [Fact]
    public void BlazorComponentLibraryComparison_ShowsEachFaqAsACollapsedSection()
    {
        // Arrange & Act
        var (page, _) = Render<BlazorComponentLibraryComparison>();

        // Assert
        var triggers = page.FindAll("button[aria-expanded]");
        Assert.NotEmpty(triggers);
        Assert.All(triggers, t => Assert.Equal("false", t.GetAttribute("aria-expanded")));
        Assert.Contains(triggers, t => t.TextContent.Contains("What is the best Blazor components library?"));
    }

    [Fact]
    public void TailwindBlazor_EmitsFaqJsonLdAndLinksToGetStarted()
    {
        // Arrange & Act
        var (page, head) = Render<TailwindBlazor>();

        // Assert
        Assert.Contains(head.FindAll("script[type='application/ld+json']"), s => s.TextContent.Contains("FAQPage"));
        Assert.Contains(page.FindAll("a"), a => a.GetAttribute("href") == "/get-started");
    }

    [Fact]
    public void BlazorComponentLibraryComparison_EmitsFaqJsonLdAndLinksToGetStarted()
    {
        // Arrange & Act
        var (page, head) = Render<BlazorComponentLibraryComparison>();

        // Assert
        Assert.Contains(head.FindAll("script[type='application/ld+json']"), s => s.TextContent.Contains("FAQPage"));
        Assert.Contains(page.FindAll("a"), a => a.GetAttribute("href") == "/get-started");
    }
}
