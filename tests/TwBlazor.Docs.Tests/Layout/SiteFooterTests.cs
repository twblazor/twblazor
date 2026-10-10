using Bunit;
using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Docs.Layout;
using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Tests.Layout;

public class SiteFooterTests : DocsTestBase
{
    private sealed class FakeInviteSource(string? invite) : IDiscordInviteSource
    {
        public int Lookups { get; private set; }

        public Task<string?> GetInviteAsync(CancellationToken cancellationToken = default)
        {
            Lookups++;
            return Task.FromResult(invite);
        }
    }

    private FakeInviteSource UseInviteSource(string? invite)
    {
        var source = new FakeInviteSource(invite);
        TestContext.Services.AddSingleton<IDiscordInviteSource>(source);
        return source;
    }

    [Theory]
    [InlineData("pointerenter")]
    [InlineData("focus")]
    [InlineData("touchstart")]
    public void Discord_SwapsInTheServersOwnInvite_WhenTheVisitorReachesForTheLink(string trigger)
    {
        // Arrange
        var source = UseInviteSource("https://discord.com/invite/server-owned");
        var cut = TestContext.Render<SiteFooter>();
        Assert.Equal(SiteFooter.DiscordUrl, cut.Find("#footer-discord").GetAttribute("href"));
        Assert.Equal(0, source.Lookups);

        // Act
        cut.Find("#footer-discord").TriggerEvent("on" + trigger, EventArgs.Empty);

        // Assert
        Assert.Equal("https://discord.com/invite/server-owned", cut.Find("#footer-discord").GetAttribute("href"));
        Assert.Equal(1, source.Lookups);
    }

    [Fact]
    public void Discord_KeepsThePermanentLink_WhenNoInviteCanBeFound()
    {
        // Arrange
        var source = UseInviteSource(null);
        var cut = TestContext.Render<SiteFooter>();

        // Act
        cut.Find("#footer-discord").TriggerEvent("onpointerenter", EventArgs.Empty);

        // Assert
        Assert.Equal(SiteFooter.DiscordUrl, cut.Find("#footer-discord").GetAttribute("href"));
        Assert.Equal(1, source.Lookups);
    }

    [Fact]
    public void Discord_LooksTheInviteUpOnlyOnce()
    {
        // Arrange
        var source = UseInviteSource("https://discord.com/invite/server-owned");
        var cut = TestContext.Render<SiteFooter>();

        // Act
        cut.Find("#footer-discord").TriggerEvent("onpointerenter", EventArgs.Empty);
        cut.Find("#footer-discord").TriggerEvent("onfocus", EventArgs.Empty);
        cut.Find("#footer-discord").TriggerEvent("onpointerenter", EventArgs.Empty);

        // Assert
        Assert.Equal(1, source.Lookups);
    }

    [Fact]
    public void Render_ShowsDiscordGitHubDocumentationAndLicenseLinks()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        var hrefs = cut.FindAll("footer a").Select(a => a.GetAttribute("href")).ToList();
        Assert.Equal([SiteFooter.DiscordUrl, SiteFooter.GitHubUrl, SiteFooter.DocumentationUrl, SiteFooter.LicenseUrl], hrefs);
    }

    [Fact]
    public void Render_SeparatesTheLinksWithDecorativeBullets()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        var separators = cut.FindAll("footer span[aria-hidden='true']");
        Assert.Equal(3, separators.Count);
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
    public void Render_OnlyLinksToOtherSites_SoEveryLinkOpensInANewTab()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        var links = cut.FindAll("footer a");
        Assert.Equal(4, links.Count);
        Assert.All(links, a =>
        {
            Assert.StartsWith("https://", a.GetAttribute("href"));
            Assert.Equal("_blank", a.GetAttribute("target"));
        });
    }

    [Fact]
    public void Render_LinksToTheDiscordServer_WithItsIcon()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        var discord = cut.Find("#footer-discord");
        Assert.Equal(SiteFooter.DiscordUrl, discord.GetAttribute("href"));
        Assert.StartsWith("Discord", discord.TextContent.Trim());
        Assert.Contains("bi-discord", discord.QuerySelector("i")!.GetAttribute("class"));
        Assert.Equal("true", discord.QuerySelector("i")!.GetAttribute("aria-hidden"));
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
