using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Configuration;
using TwBlazor.Configuration.Components;
using TwBlazor.Docs.Compiler;
using TwBlazor.Docs.Pages.Showcase;
using TwBlazor.Docs.Pages.Showcase.Shared;
using TwBlazor.Enums;
using HomePage = TwBlazor.Docs.Pages.Home;

namespace TwBlazor.Docs.Tests.Pages;

/// <summary>
/// Tests the brand-inspired example themes: the theme files themselves, the pages that show them and the links to them.
/// </summary>
public class ShowcasePageTests : DocsTestBase
{
    public static TheoryData<string> Slugs => [.. ShowcaseBrand.All.Select(brand => brand.Slug)];

    private static ShowcaseBrand Brand(string slug) => ShowcaseBrand.All.Single(brand => brand.Slug == slug);

    private TwBlazorOptions options => TestContext.Services.GetRequiredService<TwBlazorOptions>();

    private IRenderedComponent<IComponent> Render(ShowcaseBrand brand, bool interactive = true)
    {
        TestContext.Renderer.SetRendererInfo(new RendererInfo("WebAssembly", interactive));
        return brand.Slug switch
        {
            "microsoft" => TestContext.Render<IComponent>(builder => Open<MicrosoftShowcase>(builder)),
            "apple" => TestContext.Render<IComponent>(builder => Open<AppleShowcase>(builder)),
            "lego" => TestContext.Render<IComponent>(builder => Open<LegoShowcase>(builder)),
            "tesco" => TestContext.Render<IComponent>(builder => Open<TescoShowcase>(builder)),
            _ => TestContext.Render<IComponent>(builder => Open<Trading212Showcase>(builder))
        };

        static void Open<T>(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder) where T : IComponent
        {
            builder.OpenComponent<T>(0);
            builder.CloseComponent();
        }
    }

    #region Theme files

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Theme_HasEveryComponentThemeTheDefaultHas(string slug)
    {
        var expected = Theme.Components.Select(component => component.GetType()).OrderBy(type => type.Name);
        var actual = Brand(slug).Theme.Components.Select(component => component.GetType()).OrderBy(type => type.Name);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Theme_ReplacesTheDefaultBrandColors(string slug)
    {
        var colors = Brand(slug).Theme.Colors;
        string[] classes = [colors.LightBackground.Medium.Primary, colors.LightBackground.Medium.Accent, colors.TextColors.Medium.Primary, colors.SurfaceColors.Filled.Primary, colors.FocusColors.Primary];

        Assert.All(classes, value => Assert.DoesNotContain("-purple-", value));
        Assert.All(classes, value => Assert.DoesNotContain("-fuchsia-", value));
        Assert.NotEqual(Theme.Colors.LightBackground.Medium.Primary, colors.LightBackground.Medium.Primary);
    }

    [Fact]
    public void Themes_EachHaveTheirOwnShape()
    {
        var microsoft = ShowcaseBrand.Microsoft.Theme;
        var apple = ShowcaseBrand.Apple.Theme;
        var lego = ShowcaseBrand.Lego.Theme;
        var tesco = ShowcaseBrand.Tesco.Theme;

        Assert.Equal("rounded-xs", microsoft.Rounded.Md);
        Assert.Contains("border-b-2", microsoft.Components.Require<TwInputTheme>().FocusBorder);
        Assert.Contains("bg-neutral-900", microsoft.Colors.SurfaceColors.Filled.Accent);

        Assert.Equal("rounded-3xl", apple.Rounded.Lg);
        Assert.Equal(Shadow.None, apple.Shadows.DefaultShadow);
        Assert.Contains("rounded-full!", apple.Components.Require<TwButtonTheme>().Base);

        // The orange call to action carries dark text, and the bar is yellow.
        Assert.Equal("bg-brickorange-400 hover:bg-brickorange-300 active:bg-brickorange-500 text-gray-950", lego.Colors.SurfaceColors.Filled.Accent);
        Assert.Contains("bg-brickyellow-400 text-black", lego.Components.Require<TwSidebarTheme>().Navbar);
        Assert.Contains("rounded-full!", lego.Components.Require<TwButtonTheme>().Base);

        Assert.Contains("rounded-full!", tesco.Components.Require<TwButtonTheme>().Base);
        Assert.Contains("min-h-11", tesco.Components.Require<TwButtonTheme>().Base);
        Assert.Equal(Shadow.None, tesco.Shadows.DefaultShadow);

        var trading = ShowcaseBrand.Trading212.Theme;
        Assert.Contains("bg-slate-950 text-white", trading.Components.Require<TwSidebarTheme>().Navbar);
        Assert.Contains("rounded-full!", trading.Components.Require<TwButtonTheme>().Base);
        Assert.Equal("bg-tradeblue-600", trading.Colors.LightBackground.Medium.Primary);
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Theme_HasItsOwnStylesheet_BuiltFromItsOwnThemeFile(string slug)
    {
        var css = Path.Combine(Paths.DocsProjectPath, "wwwroot", "css", "showcase");
        var input = File.ReadAllText(Path.Combine(css, $"{slug}.input.css"));
        var output = File.ReadAllText(Path.Combine(css, $"{slug}.css"));
        var primary = Brand(slug).Theme.Colors.LightBackground.Medium.Primary;

        Assert.Contains("Theme.cs\";", input);
        Assert.Contains($".{primary} {{", output);
    }

    #endregion

    #region Pages

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Page_AppliesItsTheme_AndShowsTheThemeFile(string slug)
    {
        var brand = Brand(slug);
        var cut = Render(brand);

        Assert.Same(brand.Theme, options.Theme);
        Assert.Equal(brand.Stylesheet, cut.Find("link[rel='stylesheet']").GetAttribute("href"));
        Assert.Single(cut.FindAll("h1"));
        Assert.Equal(brand.Products.Count, cut.FindAll("#showcase-products article :is(h3, h4)").Count);
        Assert.All(brand.Products, product => Assert.Contains(product.Name, cut.Find("#showcase-products").TextContent));

        cut.FindAll("#showcase-theme [role='tab']").Single(tab => tab.TextContent.Trim() == "Theme.cs").Click();
        Assert.Contains("public static TwBlazorTheme CreateTheme()", cut.Find("#showcase-theme-file pre code").TextContent);
        Assert.Contains(brand.Theme.Colors.LightBackground.Medium.Primary, cut.Find("#showcase-components button").ClassName);
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Page_SaysItIsNotAffiliatedWithTheBrand(string slug)
    {
        var cut = Render(Brand(slug));

        Assert.Equal(ShowcaseBrand.Disclaimer, cut.Find("#showcase-disclaimer").TextContent);
        Assert.Contains($"Not affiliated with {Brand(slug).Name}", cut.Markup);
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Page_ShowsTheBrandsLogo_OrItsNameWhereItHasNone(string slug)
    {
        var brand = Brand(slug);
        var link = Render(brand).Find($"a[href='{brand.Path}']:not([aria-current])");

        if (brand.LogoUrl is null)
        {
            Assert.Contains(brand.Name, link.TextContent);
            Assert.Empty(link.QuerySelectorAll("img"));
        }
        else
        {
            var logo = link.QuerySelector("img")!;
            Assert.Equal(brand.LogoUrl, logo.GetAttribute("src"));
            Assert.Equal(brand.Name, logo.GetAttribute("alt"));
        }
    }

    [Fact]
    public void Logos_AreSetForEveryBrandThatHasNoIcon()
    {
        Assert.All(ShowcaseBrand.All, brand => Assert.True(brand.LogoUrl is not null || brand.LogoIcon is not null, brand.Name));

        // The two logo files in the repository are the ones their pages point at.
        var images = Path.Combine(Paths.DocsProjectPath, "wwwroot", "images", "showcase");
        Assert.All((ShowcaseBrand[])[ShowcaseBrand.Tesco, ShowcaseBrand.Trading212], brand =>
        {
            Assert.EndsWith($"/images/showcase/{brand.Slug}.svg", brand.LogoUrl);
            var svg = File.ReadAllText(Path.Combine(images, $"{brand.Slug}.svg"));
            Assert.Contains("<svg", svg);

            // A logo sits on the page's own bar, so it must not bring a backdrop of its own.
            Assert.DoesNotContain("<rect width=\"640\"", svg);
        });
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Pictures_AreAllServedFromTheSite(string slug)
    {
        var brand = Brand(slug);
        var images = Path.Combine(Paths.DocsProjectPath, "wwwroot", "images", "showcase");
        string?[] sources = [brand.LogoUrl, brand.HeroImage, .. brand.Products.Select(product => product.Image)];

        // Another site's picture would need its host in the content security policy, and breaks if that site moves it.
        Assert.All(sources.OfType<string>(), source =>
        {
            Assert.StartsWith($"{ShowcaseBrand.LocalImages}/", source);
            Assert.True(File.Exists(Path.Combine(images, source[(ShowcaseBrand.LocalImages.Length + 1)..])), source);
        });
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Page_GivesEveryPictureATextAlternative_AndSendsNoReferrerToOtherSites(string slug)
    {
        var pictures = Render(Brand(slug)).FindAll("#showcase-main img");

        Assert.All(pictures, picture =>
        {
            var source = picture.GetAttribute("src")!;
            Assert.Matches("^(https://|_content/TwBlazor.Docs/images/showcase/)", source);
            Assert.True(picture.HasAttribute("alt"));

            if (source.StartsWith("https://", StringComparison.Ordinal))
            {
                Assert.Equal("no-referrer", picture.GetAttribute("referrerpolicy"));
            }
        });
        Assert.Equal(Brand(slug).Products.Any(product => product.Image is not null), pictures.Count > 0);
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Page_UsesTwChipForEveryBadge(string slug)
    {
        var cut = Render(Brand(slug));

        // A badge drawn by hand would be a small span with its own background: every one on the page is a TwChip.
        Assert.NotEmpty(cut.FindComponents<TwBlazor.Components.TwChip>());
        Assert.Empty(cut.FindAll("#showcase-main span[class*='px-2'][class*='py-0.5']"));
    }

    [Fact]
    public void IconButtons_AreAsWideAsButtonsAreTall_SoTheyStayRound()
    {
        Assert.All(ShowcaseBrand.All, brand =>
        {
            var button = brand.Theme.Components.Require<TwButtonTheme>();
            var height = button.Base.Split(' ').Single(value => value.StartsWith("min-h-", StringComparison.Ordinal))["min-h-".Length..];

            Assert.Contains($"size-{height}", button.IconButton.Split(' '));
            Assert.Contains("rounded-full", button.IconButton.Split(' '));
        });
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Page_ShowsItsOwnRazorSource_BesideTheThemeFile(string slug)
    {
        var brand = Brand(slug);
        var cut = Render(brand);

        Assert.Contains($"@page \"{brand.Path}\"", cut.Find("#showcase-razor pre code").TextContent);
        Assert.Contains("<ShowcasePage Brand=\"brand\">", cut.Find("#showcase-razor pre code").TextContent);
        Assert.Equal(["Showcase.razor", "Theme.cs"], cut.FindAll("#showcase-theme [role='tab']").Select(tab => tab.TextContent.Trim()));
        Assert.Contains("Showcase.razor", cut.Find("#showcase-razor").TextContent);
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Page_UsesIconsForItsIconOnlyActions_AndCardsForItsProducts(string slug)
    {
        var cut = Render(Brand(slug));

        Assert.DoesNotContain(cut.FindComponents<TwBlazor.Components.TwButton>(), button => button.Instance.IconButton && button.Instance.StartIcon is not null && cut.Find("#showcase-main").Contains(button.Find("button, a")));
        Assert.True(cut.FindComponents<TwBlazor.Components.TwCard>().Count >= Brand(slug).Products.Count);
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Page_EndsWithTheBrandsOwnFooter_DrawnByTwFooter(string slug)
    {
        var brand = Brand(slug);
        var cut = Render(brand);
        var footer = Assert.Single(cut.FindComponents<TwBlazor.Components.TwFooter>());
        var theme = brand.Theme.Components.Require<TwFooterTheme>();

        // One footer landmark per page: the notice under it is a note, not a second footer.
        Assert.Single(cut.FindAll("footer"));
        Assert.Equal(4, footer.FindAll("nav h2").Count);
        Assert.Equal($"{brand.Name} example footer", footer.Find("nav").GetAttribute("aria-label"));
        Assert.Contains(theme.Footer.Split(' ')[^1], footer.Find("footer").ClassName);
        Assert.NotEqual(Theme.Components.Require<TwFooterTheme>().Footer, theme.Footer);
        Assert.Equal("note", cut.Find("#showcase-disclaimer").ParentElement!.GetAttribute("role"));
    }

    [Fact]
    public void TradingPage_ShowsACandlestickChartOfFortyTradingDays()
    {
        var cut = Render(ShowcaseBrand.Trading212);
        var chart = cut.FindComponent<TwBlazor.Components.TwCandlestickChart>().Instance;

        Assert.Equal(40, chart.Items.Count);
        Assert.All(chart.Items, candle => Assert.True(candle.High >= Math.Max(candle.Open, candle.Close) && candle.Low <= Math.Min(candle.Open, candle.Close)));
        Assert.Contains(chart.Items, candle => candle.Close > candle.Open);
        Assert.Contains(chart.Items, candle => candle.Close < candle.Open);
        Assert.Equal(chart.Items.Skip(1).Select(candle => candle.Open), chart.Items.SkipLast(1).Select(candle => candle.Close));
    }

    [Fact]
    public void Gallery_ChartsSixMonthsOfWeeklySales_BesideTheMonthlyTotals()
    {
        var cut = Render(ShowcaseBrand.Microsoft);

        // Sales is the first tab, so its charts are showing without a click.
        Assert.Equal(["Sales", "Orders", "Help"], cut.FindAll("#showcase-components [role='tab']").Select(tab => tab.TextContent.Trim()));

        var line = cut.FindComponent<TwBlazor.Components.TwLineChart>().Instance;
        Assert.Equal(26, line.Categories.Count);
        Assert.Equal(3, line.Series.Count);
        Assert.All(line.Series, series => Assert.Equal(26, series.Values.Count));
        Assert.Contains(null, line.Series[2].Values);
        Assert.Single(cut.FindComponents<TwBlazor.Components.TwColumnChart>());
    }

    [Fact]
    public void RobotsFile_KeepsCrawlersOutOfTheShowcasePages()
    {
        var robots = File.ReadAllText(Path.Combine(Paths.DocsProjectPath, "..", "TwBlazor.WASM", "wwwroot", "robots.txt"));
        var groups = robots.Split("User-agent: *")[1].Split("User-agent: GPTBot");

        Assert.All(groups, group => Assert.Contains("Disallow: /showcase/", group));
    }

    [Theory]
    [InlineData("microsoft", false)]
    [InlineData("apple", false)]
    [InlineData("lego", true)]
    [InlineData("tesco", true)]
    [InlineData("trading212", false)]
    public void Page_IgnoresDarkMode_WhereTheBrandsOwnSiteHasNone(string slug, bool lightOnly)
    {
        var cut = Render(Brand(slug));
        var page = cut.Find($"#showcase-{slug} > div:nth-of-type(2)");

        Assert.Equal(lightOnly, Brand(slug).LightOnly);
        Assert.Equal(lightOnly, page.ClassList.Contains("theme-light"));
    }

    [Fact]
    public void Page_LeavesTheSiteThemeAlone_UntilItRunsInTheBrowser()
    {
        var cut = Render(ShowcaseBrand.Lego, interactive: false);

        Assert.Same(Theme, options.Theme);
        Assert.Contains("Loading the LEGO-inspired theme", cut.Find("[role='status']").TextContent);
        Assert.Empty(cut.FindAll("h1"));
        Assert.Equal(ShowcaseBrand.Disclaimer, cut.Find("#showcase-disclaimer").TextContent);
    }

    [Fact]
    public async Task Page_PutsTheSiteThemeBack_WhenTheVisitorLeaves()
    {
        Render(ShowcaseBrand.Tesco);
        Assert.Same(ShowcaseBrand.Tesco.Theme, options.Theme);

        await TestContext.DisposeComponentsAsync();

        Assert.Same(Theme, options.Theme);
    }

    [Fact]
    public void ThemeSwitcher_KeepsTheNextPagesTheme_WhenOneShowcasePageReplacesAnother()
    {
        ShowcaseThemeSwitcher.Apply(options, ShowcaseBrand.Apple);

        // The page being navigated to starts before the page being left is disposed.
        ShowcaseThemeSwitcher.Apply(options, ShowcaseBrand.Tesco);
        ShowcaseThemeSwitcher.Restore(options, ShowcaseBrand.Apple);
        Assert.Same(ShowcaseBrand.Tesco.Theme, options.Theme);

        ShowcaseThemeSwitcher.Restore(options, ShowcaseBrand.Tesco);
        Assert.Same(Theme, options.Theme);
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Page_CountsWhatIsAddedToTheBasket(string slug)
    {
        var cut = Render(Brand(slug));
        Assert.Contains("is empty.", cut.Find("#showcase-products [aria-live]").TextContent);

        cut.Find("#showcase-products article button").Click();
        Assert.StartsWith("1 in your", cut.Find("#showcase-products [aria-live]").TextContent);

        cut.Find("#showcase-products article button").Click();
        Assert.StartsWith("2 in your", cut.Find("#showcase-products [aria-live]").TextContent);
    }

    [Fact]
    public void Page_LinksToEveryExample_AndMarksTheCurrentOne()
    {
        var cut = Render(ShowcaseBrand.Tesco);
        var links = cut.FindAll("nav[aria-label='Example themes'] a");

        Assert.Equal([.. ShowcaseBrand.All.Select(brand => brand.Path), "/theme"], links.Select(link => link.GetAttribute("href")));
        Assert.Equal("Tesco", Assert.Single(links, link => link.GetAttribute("aria-current") == "page").TextContent);
    }

    [Fact]
    public void Page_EndsTheExamplesWithALinkToTheThemeBuilder()
    {
        var link = Render(ShowcaseBrand.Lego).Find("nav[aria-label='Example themes'] a:last-child");

        Assert.Equal("showcase-create-your-own", link.Id);
        Assert.Equal("/theme", link.GetAttribute("href"));
        Assert.Equal("Create your own", link.TextContent.Trim());
        Assert.Null(link.GetAttribute("aria-current"));
    }

    [Fact]
    public void Page_StillLeadsBackToTheExamplesOnTheHomePage()
    {
        var back = Render(ShowcaseBrand.Lego).Find("a:contains('Back to twblazor')");

        Assert.Equal("/#theme-case-study", back.GetAttribute("href"));
    }

    [Theory]
    [InlineData("apple")]
    [InlineData("lego")]
    [InlineData("tesco")]
    [InlineData("trading212")]
    public void PillButtons_StayPills_WhateverRoundingAButtonIsGiven(string slug)
    {
        var buttons = Render(Brand(slug)).FindAll("#showcase-products article button");

        Assert.NotEmpty(buttons);
        Assert.All(buttons, button => Assert.Contains("rounded-full!", button.ClassName));
    }

    #endregion

    #region Links to the examples

    [Fact]
    public void LandingPage_HasACaseStudy_LinkingToEveryExample_WithTheDisclaimer()
    {
        var page = TestContext.Render<HomePage>();

        Assert.Equal(ShowcaseBrand.All.Select(brand => brand.Path), page.FindAll("#theme-case-study ul a").Select(link => link.GetAttribute("href")));
        Assert.Equal(ShowcaseBrand.Disclaimer, page.Find("#theme-case-study-disclaimer").TextContent);
    }

    [Fact]
    public void Disclaimer_NamesEveryBrand()
    {
        var disclaimer = ShowcaseBrand.Disclaimer;

        Assert.All((string[])["Microsoft", "Apple", "LEGO", "Tesco", "Trading 212"], name => Assert.Contains(name, disclaimer));
        Assert.Contains("not affiliated", disclaimer);
    }

    #endregion
}