using Bunit;
using TwBlazor.Docs.Layout;

namespace TwBlazor.Docs.Tests.Layout;

public class SiteFooterTests : DocsTestBase
{
    [Fact]
    public void Render_ShowsGitHubDocumentationAndLicenseLinks()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        var hrefs = cut.FindAll("footer a").Select(a => a.GetAttribute("href")).ToList();
        Assert.Equal([SiteFooter.GitHubUrl, SiteFooter.DocumentationUrl, SiteFooter.LicenseUrl], hrefs);
    }

    [Fact]
    public void Render_SeparatesTheLinksWithDecorativeBullets()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        var separators = cut.FindAll("footer span[aria-hidden='true']");
        Assert.Equal(2, separators.Count);
        Assert.All(separators, s => Assert.Equal("•", s.TextContent));
    }

    [Fact]
    public void Render_OpensExternalLinksInANewTabSafely()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        Assert.All(cut.FindAll("footer a"), a =>
        {
            Assert.Equal("_blank", a.GetAttribute("target"));
            Assert.Equal("noopener noreferrer", a.GetAttribute("rel"));
            Assert.Contains("opens in a new tab", a.TextContent);
        });
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
