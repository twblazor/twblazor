using TwBlazor.Docs.Compiler;
using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Tests.Build;

public class SitemapGeneratorTests
{
    [Fact]
    public void GenerateSitemap_WritesAnAbsoluteLocForEveryPage()
    {
        // Arrange
        PageEntry[] entries = [new("/", null), new("/card", null)];

        // Act
        var xml = SitemapGenerator.GenerateSitemap(entries, "https://example.com/");

        // Assert
        Assert.Contains("<loc>https://example.com/</loc>", xml);
        Assert.Contains("<loc>https://example.com/card</loc>", xml);
    }

    [Fact]
    public void GenerateSitemap_WritesLastmodOnlyForPagesWithAKnownDate()
    {
        // Arrange
        PageEntry[] entries = [new("/card", new DateOnly(2026, 9, 4)), new("/alert", null)];

        // Act
        var xml = SitemapGenerator.GenerateSitemap(entries);

        // Assert
        Assert.Equal(1, xml.Split("<lastmod>").Length - 1);
        Assert.Contains("<lastmod>2026-09-04</lastmod>", xml);
    }

    [Fact]
    public void GenerateSitemap_OmitsChangefreq()
    {
        // Arrange & Act
        var xml = SitemapGenerator.GenerateSitemap([new PageEntry("/card", null)]);

        // Assert
        Assert.DoesNotContain("changefreq", xml);
    }

    [Fact]
    public void GenerateSitemap_RanksHomeAboveGetStartedAboveEveryOtherPage()
    {
        // Arrange
        PageEntry[] entries = [new("/", null), new("/get-started", null), new("/card", null), new("/alert", null)];

        // Act
        var priorities = System.Xml.Linq.XDocument.Parse(SitemapGenerator.GenerateSitemap(entries))
            .Descendants()
            .Where(e => e.Name.LocalName == "url")
            .ToDictionary(
                url => url.Elements().Single(e => e.Name.LocalName == "loc").Value,
                url => double.Parse(url.Elements().Single(e => e.Name.LocalName == "priority").Value, System.Globalization.CultureInfo.InvariantCulture));

        // Assert
        var home = priorities["https://twblazor.com/"];
        var getStarted = priorities["https://twblazor.com/get-started"];
        Assert.True(home > getStarted);
        Assert.True(getStarted > priorities["https://twblazor.com/card"]);
        Assert.Equal(priorities["https://twblazor.com/card"], priorities["https://twblazor.com/alert"]);
    }

    [Fact]
    public void GenerateSitemap_EscapesXmlSpecialCharactersInTheRoute()
    {
        // Arrange & Act
        var xml = SitemapGenerator.GenerateSitemap([new PageEntry("/a&b", null)]);

        // Assert
        Assert.Contains("<loc>https://twblazor.com/a&amp;b</loc>", xml);
    }

    [Fact]
    public void GenerateSitemap_ProducesWellFormedXml()
    {
        // Arrange & Act
        var xml = SitemapGenerator.GenerateSitemap([new PageEntry("/", new DateOnly(2026, 1, 2)), new PageEntry("/card", null)]);

        // Assert
        var document = System.Xml.Linq.XDocument.Parse(xml);
        Assert.Equal("urlset", document.Root!.Name.LocalName);
    }

    [Fact]
    public void GeneratePageLastModifiedClass_ListsDatedPagesInRouteOrderAndSkipsUndatedOnes()
    {
        // Arrange
        PageEntry[] entries = [new("/card", new DateOnly(2026, 9, 4)), new("/alert", new DateOnly(2026, 1, 12)), new("/chip", null)];

        // Act
        var code = SitemapGenerator.GeneratePageLastModifiedClass(entries);

        // Assert
        Assert.Contains("[\"/alert\"] = new DateOnly(2026, 1, 12),", code);
        Assert.Contains("[\"/card\"] = new DateOnly(2026, 9, 4),", code);
        Assert.DoesNotContain("/chip", code);
        Assert.True(code.IndexOf("/alert", StringComparison.Ordinal) < code.IndexOf("/card", StringComparison.Ordinal));
    }

    [Fact]
    public void DefaultBaseUrl_MatchesTheOriginTheCanonicalUrlsAreBuiltFrom()
    {
        // Assert - a mismatch would put sitemap URLs on a different origin than the pages' canonical tags.
        Assert.Equal(SitemapGenerator.DefaultBaseUrl, SiteMetadata.BaseUrl);
    }
}
