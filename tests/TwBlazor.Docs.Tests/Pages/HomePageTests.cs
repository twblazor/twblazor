using Bunit;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Docs.Pages;
using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Tests.Pages;

public class HomePageTests : DocsTestBase
{
    [Fact]
    public async Task Render_TargetsTheTailwindBlazorComponentLibrarySearchTerms()
    {
        // Arrange
        var head = TestContext.Render<HeadOutlet>();

        // Act
        var page = TestContext.Render<Home>();
        await head.WaitForStateAsync(() => head.Markup.Contains("canonical"));

        // Assert
        var title = head.Find("title").TextContent;
        Assert.Contains("Tailwind Blazor", title);
        Assert.Contains("Component Library", title);
        Assert.True(title.Length <= SiteMetadata.MaxTitleLength, title);
        Assert.Contains("Blazor components library", head.Find("meta[name='description']").GetAttribute("content"));
        Assert.Equal("The Tailwind CSS component library for Blazor", page.Find("h1").TextContent.Trim());
    }

    [Fact]
    public void CodeWindows_HaveAWindowsStyleTitleBar_HiddenFromAssistiveTechnology()
    {
        // Arrange & Act
        var page = TestContext.Render<Home>();

        // Assert
        var bars = page.FindAll("[data-stage] [aria-hidden='true'].border-b");
        Assert.Equal(["Deploy.razor", "Theme.cs", "Terminal and Home.razor"], bars.Select(bar => bar.QuerySelector(".truncate")!.TextContent));
        Assert.All(bars, bar => Assert.Equal(3, bar.QuerySelectorAll(".w-10").Length));
        Assert.Empty(page.FindAll("[data-stage] .bg-red-400\\/80"));
    }

    [Fact]
    public void LiveBuild_TypesSixLines_AndPopsEachComponentInAWrapperThatHugsIt()
    {
        // Arrange & Act
        var page = TestContext.Render<Home>();

        // Assert
        var stage = page.Find("figure[aria-labelledby='live-build-caption'] [data-stage]");
        Assert.Equal(6, stage.QuerySelectorAll(".stage-line").Length);

        var pops = stage.QuerySelectorAll(".stage-pop");
        Assert.Equal(6, pops.Length);
        Assert.All(pops, pop => Assert.Contains("grid", pop.ClassList));
        Assert.NotNull(stage.QuerySelector("#live-build-service"));
        Assert.NotNull(stage.QuerySelector("#live-build-tests"));
        Assert.NotNull(stage.QuerySelector("#live-build-auto-deploy"));
        Assert.Contains("6 components rendered", stage.QuerySelector(".stage-done")!.TextContent);
    }

    [Fact]
    public void ThemeBuild_ShowsTheButtonBeforeAndAfter_AndATagForEachChange()
    {
        // Arrange & Act
        var page = TestContext.Render<Home>();

        // Assert
        var stage = page.Find("figure[aria-labelledby='theme-build-caption'] [data-stage]");
        Assert.DoesNotContain("theme-morph", stage.QuerySelector("#theme-build-before")!.ClassList);
        Assert.Contains("theme-morph", stage.QuerySelector("#theme-build-after")!.ClassList);
        Assert.Contains("\"bg-pink-700\"", stage.QuerySelector("code")!.TextContent);

        var changes = stage.QuerySelectorAll("#theme-build-changes > li.stage-pop");
        Assert.Equal(["Corners: full", "Shadow: large", "Colour: pink"], changes.Select(change => change.TextContent.Trim()));
        Assert.Equal(stage.QuerySelectorAll(".stage-line").Length, changes.Length);
    }

    [Fact]
    public void FeatureCards_HaveShortOneLineTitles()
    {
        // Arrange & Act
        var page = TestContext.Render<Home>();

        // Assert
        Assert.Equal(["Fast to build", "Fully customisable", "Composable", "Accessible"], page.FindAll("section[aria-labelledby='features-heading'] h3").Select(h => h.TextContent.Trim()));
    }

    [Fact]
    public async Task Render_EmitsWebSiteAndLibraryJsonLd()
    {
        // Arrange
        var head = TestContext.Render<HeadOutlet>();

        // Act
        TestContext.Render<Home>();
        await head.WaitForStateAsync(() => head.Markup.Contains("ld+json"));

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
    public void Render_AdvertisesLocalization_ListsShippedLanguages_AndLinksToItsPage()
    {
        // Arrange & Act
        var page = TestContext.Render<Home>();

        // Assert
        var section = page.Find("section[aria-labelledby='localization-heading']");
        Assert.Contains("language", section.QuerySelector("h2#localization-heading")!.TextContent);
        Assert.Equal("/localization", section.QuerySelector("a#localization-docs")!.GetAttribute("href"));

        Assert.Contains("25 languages, built in", section.TextContent);

        var cultures = section.QuerySelectorAll("#localization-languages > li > span[lang]").Select(span => span.GetAttribute("lang")!).ToList();
        Assert.Equal("en", cultures[0]);
        var resources = new System.Resources.ResourceManager("TwBlazor.Resources.TwBlazorStrings", typeof(TwBlazor.Localization.TwLocalizer).Assembly);
        Assert.All(cultures.Skip(1), culture =>
            Assert.NotNull(resources.GetResourceSet(System.Globalization.CultureInfo.GetCultureInfo(culture), true, false)));
    }

    [Fact]
    public void LanguageList_OnEveryScreen_ShowsTheFirstTen_AndLinksToTheRest()
    {
        // Arrange & Act
        var page = TestContext.Render<Home>();

        // Assert
        var languages = page.FindAll("#localization-languages > li").Where(item => item.QuerySelector("span[lang]") is not null).ToList();
        Assert.Equal(10, languages.Count);
        Assert.DoesNotContain(page.FindAll("#localization-languages li"), item => item.ClassList.Contains("hidden") || item.ClassList.Contains("lg:hidden"));

        var more = page.Find("#localization-more-languages");
        Assert.Equal("/localization#localization-languages", more.GetAttribute("href"));
        Assert.Equal("...", more.QuerySelector("[aria-hidden='true']")!.TextContent);
        Assert.Equal("and 15 more languages", more.QuerySelector(".sr-only")!.TextContent);
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
        Assert.Equal(3, section.QuerySelectorAll("ul.mt-6 > li").Length);

        var headings = page.FindAll("section").Select(s => s.GetAttribute("aria-labelledby")).ToList();
        Assert.Equal(headings.IndexOf("localization-heading") + 1, headings.IndexOf("rtl-heading"));
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
