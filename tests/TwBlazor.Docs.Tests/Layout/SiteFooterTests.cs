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
    public void Render_UsesTwFooter_WithALabelledNavigationLandmark()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        Assert.NotNull(cut.Find("footer#site-footer"));
        Assert.Equal("Footer", cut.Find("footer nav").GetAttribute("aria-label"));
    }

    [Fact]
    public void Render_GroupsTheLinksIntoFourHeadedColumns()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        Assert.Equal(["Get started", "Guides", "Resources", "Community"], cut.FindAll("footer nav h2").Select(h => h.TextContent.Trim()));
        Assert.All(cut.FindAll("footer nav ul"), list => Assert.NotEmpty(list.QuerySelectorAll("li a")));
    }

    [Fact]
    public void Render_LinksToTheDocsGuides_InTheSameTab()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        var internalLinks = cut.FindAll("footer nav a").Where(a => a.GetAttribute("href")!.StartsWith('/')).ToList();
        Assert.Contains(internalLinks, a => a.GetAttribute("href") == "/get-started");
        Assert.Contains(internalLinks, a => a.GetAttribute("href") == "/tailwind-blazor");
        Assert.All(internalLinks, a => Assert.Null(a.GetAttribute("target")));
    }

    [Fact]
    public void Render_LinksToDiscordGitHubDocumentationLicenseAndLlms()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        var hrefs = cut.FindAll("footer a").Select(a => a.GetAttribute("href")).ToList();
        Assert.Contains(SiteFooter.DiscordUrl, hrefs);
        Assert.Contains(SiteFooter.GitHubUrl, hrefs);
        Assert.Contains(SiteFooter.DocumentationUrl, hrefs);
        Assert.Contains(SiteFooter.LicenseUrl, hrefs);
        Assert.Contains(SiteFooter.LlmsTxtUrl, hrefs);
    }

    [Fact]
    public void Render_LinksToLlmsTxt_WithADecorativeIcon()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        var llms = cut.Find($"footer a[href='{SiteFooter.LlmsTxtUrl}']");
        Assert.StartsWith("llms.txt", llms.TextContent.Trim());
        Assert.Contains("bi-robot", llms.QuerySelector("i")!.GetAttribute("class"));
        Assert.Equal("true", llms.QuerySelector("i")!.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void Render_OpensExternalLinksInANewTabSafely()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        var external = cut.FindAll("footer a").Where(a => a.GetAttribute("href")!.StartsWith("https://")).ToList();
        Assert.NotEmpty(external);
        Assert.All(external, a =>
        {
            Assert.Equal("_blank", a.GetAttribute("target"));
            Assert.Equal("noopener noreferrer", a.GetAttribute("rel"));
            Assert.Contains("opens in a new tab", a.TextContent);
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
    public void Render_ShowsTheOpenSourceStatement()
    {
        // Arrange & Act
        var cut = TestContext.Render<SiteFooter>();

        // Assert
        Assert.Contains(cut.FindAll("footer p"), p => p.TextContent.Contains("always will be free and open source"));
    }
}
