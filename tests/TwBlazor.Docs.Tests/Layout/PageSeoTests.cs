using Bunit;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Docs.Layout;

namespace TwBlazor.Docs.Tests.Layout;

public class PageSeoTests : DocsTestBase
{
    private IRenderedComponent<HeadOutlet> RenderHead(string path = "/card")
    {
        var head = TestContext.Render<HeadOutlet>();
        TestContext.Render<PageSeo>(parameters => parameters
            .Add(p => p.Title, "TwCard | twblazor - Blazor Card Component")
            .Add(p => p.Description, "TwCard is a Blazor card component.")
            .Add(p => p.Path, path));
        head.WaitForState(() => head.Markup.Contains("canonical"));
        return head;
    }

    [Fact]
    public void Render_EmitsTheCanonicalUrlAsAnAbsoluteAddress()
    {
        // Arrange & Act
        var head = RenderHead("/card");

        // Assert
        Assert.Equal("https://twblazor.com/card", head.Find("link[rel='canonical']").GetAttribute("href"));
        Assert.Equal("https://twblazor.com/card", head.Find("meta[property='og:url']").GetAttribute("content"));
    }

    [Fact]
    public void Render_UsesOneDescriptionAndTitleAcrossMetaOpenGraphAndTwitter()
    {
        // Arrange & Act
        var head = RenderHead();

        // Assert
        Assert.Equal("TwCard is a Blazor card component.", head.Find("meta[name='description']").GetAttribute("content"));
        Assert.Equal("TwCard is a Blazor card component.", head.Find("meta[property='og:description']").GetAttribute("content"));
        Assert.Equal("TwCard is a Blazor card component.", head.Find("meta[name='twitter:description']").GetAttribute("content"));
        Assert.Equal("TwCard | twblazor - Blazor Card Component", head.Find("meta[property='og:title']").GetAttribute("content"));
        Assert.Equal("TwCard | twblazor - Blazor Card Component", head.Find("meta[name='twitter:title']").GetAttribute("content"));
    }

    [Fact]
    public void Render_UsesTheLargePngSocialImageWithItsDimensionsAndAltText()
    {
        // Arrange & Act
        var head = RenderHead();

        // Assert - Facebook, LinkedIn and X don't render SVG, and "summary_large_image" needs a wide image.
        var image = head.Find("meta[property='og:image']").GetAttribute("content");
        Assert.Equal("https://twblazor.com/_content/TwBlazor.Docs/images/og-image.png", image);
        Assert.Equal(image, head.Find("meta[name='twitter:image']").GetAttribute("content"));
        Assert.Equal("image/png", head.Find("meta[property='og:image:type']").GetAttribute("content"));
        Assert.Equal("1200", head.Find("meta[property='og:image:width']").GetAttribute("content"));
        Assert.Equal("630", head.Find("meta[property='og:image:height']").GetAttribute("content"));
        Assert.False(string.IsNullOrWhiteSpace(head.Find("meta[property='og:image:alt']").GetAttribute("content")));
        Assert.Equal("summary_large_image", head.Find("meta[name='twitter:card']").GetAttribute("content"));
    }

    [Fact]
    public void Render_DeclaresTheSiteNameAndLocale()
    {
        // Arrange & Act
        var head = RenderHead();

        // Assert
        Assert.Equal("twblazor", head.Find("meta[property='og:site_name']").GetAttribute("content"));
        Assert.Equal("en_GB", head.Find("meta[property='og:locale']").GetAttribute("content"));
    }

    [Fact]
    public void Render_OmitsUpdatedTime_WhenNoDateIsKnownForThePage()
    {
        // Arrange & Act
        var head = RenderHead("/no-such-page");

        // Assert
        Assert.Empty(head.FindAll("meta[property='og:updated_time']"));
    }
}
