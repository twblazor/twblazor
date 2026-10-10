using Bunit;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Components;
using TwBlazor.Docs.Layout;

namespace TwBlazor.Docs.Tests.Layout;

public class NotFoundContentTests : DocsTestBase
{
    [Fact]
    public void Render_ExplainsThePageIsMissing_UnderASingleHeading()
    {
        // Arrange & Act
        var cut = TestContext.Render<NotFoundContent>();

        // Assert
        var heading = Assert.Single(cut.FindAll("h1"));
        Assert.Contains("can't find", heading.TextContent);
        Assert.Equal(heading.Id, cut.Find("section").GetAttribute("aria-labelledby"));
        Assert.Contains("Error 404", cut.Markup);
    }

    [Fact]
    public void Render_SetsTheTitle_AndKeepsThePageOutOfSearchResults()
    {
        // Arrange
        var head = TestContext.Render<HeadOutlet>();

        // Act
        TestContext.Render<NotFoundContent>();
        head.WaitForState(() => head.Markup.Contains("robots"));

        // Assert
        Assert.Equal("Page not found | twblazor", head.Find("title").TextContent);
        Assert.Equal("noindex", head.Find("meta[name='robots']").GetAttribute("content"));
    }

    [Fact]
    public void Render_OffersAWayHome_AndLinksToPagesThatExist()
    {
        // Arrange & Act
        var cut = TestContext.Render<NotFoundContent>();

        // Assert
        Assert.Equal("/", cut.Find("#not-found-home").GetAttribute("href"));

        var links = cut.FindAll("nav[aria-label='Popular pages'] a").Select(a => a.GetAttribute("href")).ToList();
        Assert.Equal(["/get-started", "/theme", "/class-merge", "/button"], links);
    }

    [Fact]
    public void Search_OpensTheSearchDialog()
    {
        // Arrange
        var provider = TestContext.Render<TwDialogProvider>();
        var cut = TestContext.Render<NotFoundContent>();

        // Act
        cut.Find("#not-found-search").Click();

        // Assert
        provider.WaitForAssertion(() => Assert.NotEmpty(provider.FindAll("[role='dialog']")));
    }

    [Fact]
    public void Render_ShowsTheLogo_AsADecorativeImage()
    {
        // Arrange & Act
        var cut = TestContext.Render<NotFoundContent>();

        // Assert
        var logo = Assert.Single(cut.FindAll("img"));
        Assert.EndsWith("images/logo.svg", logo.GetAttribute("src"));
        Assert.Equal(string.Empty, logo.GetAttribute("alt"));
        Assert.Empty(cut.FindAll("svg"));
    }
}
