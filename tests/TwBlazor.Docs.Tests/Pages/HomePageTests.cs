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
    public void Render_InvitesVisitorsToTheDiscordServer_BeforeTheClosingCallToAction()
    {
        // Arrange & Act
        var page = TestContext.Render<Home>();

        // Assert
        var section = page.Find("section[aria-labelledby='community-heading']");
        Assert.Contains("Discord", section.QuerySelector("h2#community-heading")!.TextContent);

        var link = section.QuerySelector("a#landing-discord")!;
        Assert.Equal(TwBlazor.Docs.Layout.DiscordInviteLink.PermanentUrl, link.GetAttribute("href"));
        Assert.Equal("_blank", link.GetAttribute("target"));
        Assert.Equal("noopener noreferrer", link.GetAttribute("rel"));
        Assert.Contains("Join the Discord", link.TextContent);
        Assert.Contains("opens in a new tab", link.TextContent);

        var sections = page.FindAll("section").ToList();
        Assert.True(sections.IndexOf(section) < sections.FindIndex(s => s.QuerySelector("#cta-get-started") is not null));
    }

    [Fact]
    public void Render_AdvertisesLocalization_ListsEveryShippedLanguage_AndLinksToItsPage()
    {
        // Arrange & Act
        var page = TestContext.Render<Home>();

        // Assert
        var section = page.Find("section[aria-labelledby='localization-heading']");
        Assert.Contains("language", section.QuerySelector("h2#localization-heading")!.TextContent);
        Assert.Equal("/localization", section.QuerySelector("a#localization-docs")!.GetAttribute("href"));

        var cultures = section.QuerySelectorAll("#localization-languages > li > span[lang]").Select(span => span.GetAttribute("lang")!).ToList();
        Assert.Equal(25, cultures.Count);
        Assert.Equal("en", cultures[0]);
        var resources = new System.Resources.ResourceManager("TwBlazor.Resources.TwBlazorStrings", typeof(TwBlazor.Localization.TwLocalizer).Assembly);
        Assert.All(cultures.Skip(1), culture =>
            Assert.NotNull(resources.GetResourceSet(System.Globalization.CultureInfo.GetCultureInfo(culture), true, false)));
    }

    [Fact]
    public void LanguageList_OnAWideScreen_ShowsTheFirstTen_AndLinksToTheRest()
    {
        // Arrange & Act
        var page = TestContext.Render<Home>();

        // Assert - every language stays in the list; the later ones are only hidden from the "lg" breakpoint up
        var languages = page.FindAll("#localization-languages > li").Where(item => item.QuerySelector("span[lang]") is not null).ToList();
        Assert.Equal(10, languages.Count(item => !item.ClassList.Contains("lg:hidden")));
        Assert.Equal(15, languages.Count(item => item.ClassList.Contains("lg:hidden")));
        Assert.All(languages.Take(10), item => Assert.DoesNotContain("lg:hidden", item.ClassList));

        var more = page.Find("#localization-more-languages");
        Assert.Equal("/localization#localization-languages", more.GetAttribute("href"));
        Assert.Equal("...", more.QuerySelector("[aria-hidden='true']")!.TextContent);
        Assert.Equal("and 15 more languages", more.QuerySelector(".sr-only")!.TextContent);
        Assert.Equal(["hidden", "lg:inline-flex"], more.ParentElement!.ClassList);
    }

    [Fact]
    public void Render_AdvertisesRightToLeft_AfterLocalization_AndLinksToItsPage()
    {
        // Arrange & Act
        var page = TestContext.Render<Home>();

        // Assert
        var section = page.Find("section[aria-labelledby='rtl-heading']");
        Assert.Contains("right to left", section.QuerySelector("h2#rtl-heading")!.TextContent);
        Assert.Equal("/direction-provider", section.QuerySelector("a#rtl-docs")!.GetAttribute("href"));
        Assert.Equal(3, section.QuerySelectorAll("ul > li").Length);

        var sections = page.FindAll("section").ToList();
        var localization = sections.FindIndex(s => s.GetAttribute("aria-labelledby") == "localization-heading");
        Assert.Equal(localization + 1, sections.IndexOf(section));
    }

    [Fact]
    public void RightToLeftExample_StartsRightToLeft_InArabic()
    {
        // Arrange & Act
        var page = TestContext.Render<Home>();

        // Assert
        var preview = page.Find("#rtl-demo-preview");
        Assert.Equal("rtl", preview.GetAttribute("dir"));
        Assert.Equal("ar", preview.GetAttribute("lang"));
        Assert.Equal("true", page.Find("#rtl-demo-rtl").GetAttribute("aria-pressed"));
        Assert.Equal("false", page.Find("#rtl-demo-ltr").GetAttribute("aria-pressed"));
        Assert.Equal("<TwDirectionProvider Direction=\"TextDirection.RightToLeft\">", page.Find("#rtl-demo-markup").TextContent);
        Assert.Contains("bi-chevron-left", preview.QuerySelector("[aria-label='Next page'] i")!.GetAttribute("class"));
    }

    [Fact]
    public void RightToLeftExample_SwitchesToLeftToRight_InEnglish()
    {
        // Arrange
        var page = TestContext.Render<Home>();

        // Act
        page.Find("#rtl-demo-ltr").Click();

        // Assert
        var preview = page.Find("#rtl-demo-preview");
        Assert.Equal("ltr", preview.GetAttribute("dir"));
        Assert.Equal("en", preview.GetAttribute("lang"));
        Assert.Equal("true", page.Find("#rtl-demo-ltr").GetAttribute("aria-pressed"));
        Assert.Equal("<TwDirectionProvider Direction=\"TextDirection.LeftToRight\">", page.Find("#rtl-demo-markup").TextContent);
        Assert.Contains("Your changes were saved.", preview.TextContent);
        Assert.Contains("bi-chevron-right", preview.QuerySelector("[aria-label='Next page'] i")!.GetAttribute("class"));
    }

    [Fact]
    public void Render_SaysTheTranslationsWereProducedByAi()
    {
        // Arrange & Act
        var page = TestContext.Render<Home>();

        // Assert
        var disclaimer = page.Find("#localization-disclaimer");
        Assert.Contains("produced by AI", disclaimer.TextContent);
        Assert.Equal("/localization#localization-contributing", disclaimer.QuerySelector("a")!.GetAttribute("href"));
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
