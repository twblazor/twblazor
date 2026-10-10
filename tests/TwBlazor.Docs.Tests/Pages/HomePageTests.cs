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
        Assert.Contains("/why-twblazor", hrefs);
    }

    [Fact]
    public void Render_AdvertisesClassMerging_AndLinksToItsPage()
    {
        // Arrange & Act
        var page = TestContext.Render<Home>();

        // Assert
        var section = page.Find("section[aria-labelledby='merge-heading']");
        Assert.Contains("Tailwind class merging", section.QuerySelector("h2#merge-heading")!.TextContent);
        Assert.Equal("/class-merge", section.QuerySelector("a#merge-docs")!.GetAttribute("href"));
        Assert.Equal(3, section.QuerySelectorAll("ul > li").Length);
    }

    [Fact]
    public void ClassMergingExample_ShowsTheMergedClasses_AndStrikesOutTheReplacedOne()
    {
        // Arrange & Act
        var page = TestContext.Render<Home>();

        // Assert
        Assert.Equal("true", page.Find("#merge-example-padding").GetAttribute("aria-pressed"));
        Assert.Equal("inline-flex items-center rounded-md bg-purple-700 py-2 text-sm font-semibold text-white px-3", page.Find("#merge-result").TextContent);
        Assert.Equal(page.Find("#merge-result").TextContent, page.Find("#merge-preview").GetAttribute("class"));
        Assert.Equal("Replaced: px-6", Assert.Single(page.FindAll("section[aria-labelledby='merge-heading'] del")).TextContent);
    }

    [Theory]
    [InlineData("shorthand", "p-3", "px-6", "py-2")]
    [InlineData("shape", "rounded-full", "rounded-md")]
    [InlineData("color", "bg-pink-700", "bg-purple-700")]
    public void ClassMergingExample_Updates_WhenAnotherExampleIsChosen(string id, string yourClass, params string[] replaced)
    {
        // Arrange
        var page = TestContext.Render<Home>();

        // Act
        page.Find($"#merge-example-{id}").Click();

        // Assert
        var result = page.Find("#merge-result").TextContent.Split(' ');
        Assert.Equal(yourClass, result[^1]);
        Assert.All(replaced, name => Assert.DoesNotContain(name, result));
        Assert.Equal(replaced.Select(name => $"Replaced: {name}"), page.FindAll("section[aria-labelledby='merge-heading'] del").Select(del => del.TextContent));
        Assert.Equal("true", page.Find($"#merge-example-{id}").GetAttribute("aria-pressed"));
        Assert.Equal("false", page.Find("#merge-example-padding").GetAttribute("aria-pressed"));
    }
}
