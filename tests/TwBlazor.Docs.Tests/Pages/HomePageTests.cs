using Bunit;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Docs.Pages;
using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Tests.Pages;

public class HomePageTests : DocsTestBase
{
    [Fact]
    public void Render_TargetsTheTailwindBlazorComponentLibrarySearchTerms()
    {
        // Arrange
        var head = TestContext.Render<HeadOutlet>();

        // Act
        var page = TestContext.Render<Home>();
        head.WaitForState(() => head.Markup.Contains("canonical"));

        // Assert
        var title = head.Find("title").TextContent;
        Assert.Contains("Tailwind Blazor", title);
        Assert.Contains("Component Library", title);
        Assert.True(title.Length <= SiteMetadata.MaxTitleLength, title);
        Assert.Contains("Blazor components library", head.Find("meta[name='description']").GetAttribute("content"));
        Assert.Equal("The Tailwind CSS component library for Blazor", page.Find("h1").TextContent.Trim());
    }

    [Fact]
    public void Render_EmitsWebSiteAndLibraryJsonLd()
    {
        // Arrange
        var head = TestContext.Render<HeadOutlet>();

        // Act
        TestContext.Render<Home>();
        head.WaitForState(() => head.Markup.Contains("ld+json"));

        // Assert
        var json = head.Find("script[type='application/ld+json']").TextContent;
        Assert.Contains("SoftwareSourceCode", json);
        Assert.Contains("WebSite", json);
    }

    [Fact]
    public void Render_LinksToTheGuides()
    {
        // Arrange & Act
        var page = TestContext.Render<Home>();

        // Assert
        var hrefs = page.FindAll("a").Select(a => a.GetAttribute("href")).ToList();
        Assert.Contains("/tailwind-blazor", hrefs);
        Assert.Contains("/blazor-component-library-comparison", hrefs);
    }
}
