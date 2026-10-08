using Bunit;
using TwBlazor.Docs.Layout;

namespace TwBlazor.Docs.Tests.Layout;

public class SiteFooterTests : DocsTestBase
{
    [Fact]
    public void Render_ShowsGuideGitHubDocumentationAndLicenseLinks()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        var hrefs = cut.FindAll("footer a").Select(a => a.GetAttribute("href")).ToList();
        Assert.Equal([SiteFooter.TailwindGuidePath, SiteFooter.ComparisonPath, SiteFooter.GitHubUrl, SiteFooter.DocumentationUrl, SiteFooter.LicenseUrl], hrefs);
    }

    [Fact]
    public void Render_SeparatesTheLinksWithDecorativeBullets()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        var separators = cut.FindAll("footer span[aria-hidden='true']");
        Assert.Equal(4, separators.Count);
        Assert.All(separators, s => Assert.Equal("•", s.TextContent));
    }

    [Fact]
    public void Render_OpensExternalLinksInANewTabSafely()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        Assert.All(cut.FindAll("footer a[target]"), a =>
        {
            Assert.Equal("_blank", a.GetAttribute("target"));
            Assert.Equal("noopener noreferrer", a.GetAttribute("rel"));
            Assert.Contains("opens in a new tab", a.TextContent);
        });
    }

    [Fact]
    public void Render_LinksToTheGuidesInTheSameTab()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        var guides = cut.FindAll("footer a").Where(a => a.GetAttribute("href")!.StartsWith('/')).ToList();
        Assert.Equal(["Tailwind Blazor guide", "Why twblazor?"], guides.Select(a => a.TextContent));
        Assert.All(guides, a => Assert.Null(a.GetAttribute("target")));
    }

    [Fact]
    public void Render_ExposesTheLinksAsALabelledNavigationLandmark()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        Assert.Equal("Footer", cut.Find("footer nav").GetAttribute("aria-label"));
    }

    [Fact]
    public void Render_ShowsTheOpenSourceStatementWithAHeartIcon()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        var statement = cut.Find("footer p");
        Assert.Contains("always will be free and open source", statement.TextContent);
        Assert.Equal("true", statement.QuerySelector("i")?.GetAttribute("aria-hidden"));
    }
}
