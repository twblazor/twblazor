using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Configuration;
using TwBlazor.Configuration.Components;
using TwBlazor.Docs.Compiler;
using TwBlazor.Docs.Pages.Showcase.Shared;
using TwBlazor.Enums;
using HomePage = TwBlazor.Docs.Pages.Home;
using ThemePage = TwBlazor.Docs.Pages.ThemeBuilder.Theme;

namespace TwBlazor.Docs.Tests.Pages;

/// <summary>
/// Tests the brand-inspired example themes: the theme files themselves, the pages that show them and the links to them.
/// </summary>
public class ShowcasePageTests : DocsTestBase
{
    public static TheoryData<string> Slugs => [.. ShowcaseBrand.All.Select(brand => brand.Slug)];

    private static ShowcaseBrand Brand(string slug) => ShowcaseBrand.All.Single(brand => brand.Slug == slug);

    private TwBlazorOptions Options => TestContext.Services.GetRequiredService<TwBlazorOptions>();

    private IRenderedComponent<ShowcasePage> Render(ShowcaseBrand brand, bool interactive = true)
    {
        TestContext.Renderer.SetRendererInfo(new RendererInfo("WebAssembly", interactive));
        return TestContext.Render<ShowcasePage>(p => p.Add(x => x.Brand, brand));
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
        var cadbury = ShowcaseBrand.Cadbury.Theme;

        Assert.Equal("rounded-sm", microsoft.Rounded.Md);
        Assert.Contains("border-b-2", microsoft.Components.Require<TwInputTheme>().FocusBorder);

        Assert.Equal("rounded-3xl", apple.Rounded.Lg);
        Assert.Equal(Shadow.None, apple.Shadows.DefaultShadow);
        Assert.Contains("rounded-full!", apple.Components.Require<TwButtonTheme>().Base);

        Assert.Equal("border-2", lego.Border.Width.Thin);
        Assert.Contains("0_4px_0_0", lego.Shadows.Sm);
        Assert.Contains("text-gray-950", lego.Colors.SurfaceColors.Filled.Accent);

        Assert.Contains("rounded-full!", tesco.Components.Require<TwButtonTheme>().Base);
        Assert.Contains("min-h-11", tesco.Components.Require<TwButtonTheme>().Base);

        Assert.Equal(InputVariant.Filled, cadbury.Components.Require<TwInputTheme>().DefaultInputVariant);
        Assert.Contains("font-chocolate", cadbury.Components.Require<TwCardTheme>().Title);
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

        Assert.Same(brand.Theme, Options.Theme);
        Assert.Equal(brand.Stylesheet, cut.Find("link[rel='stylesheet']").GetAttribute("href"));
        Assert.Contains(brand.HeroTitle, cut.Find("h1").TextContent);
        Assert.Equal(brand.Products.Count, cut.FindAll("#showcase-products h3").Count);
        Assert.Contains("public static TwBlazorTheme CreateTheme()", cut.Find("#showcase-theme pre code").TextContent);
        Assert.Contains(brand.Theme.Colors.LightBackground.Medium.Primary, cut.Find("#showcase-products button").ClassName);
    }

    [Theory]
    [MemberData(nameof(Slugs))]
    public void Page_SaysItIsNotAffiliatedWithTheBrand(string slug)
    {
        var cut = Render(Brand(slug));

        Assert.Equal(ShowcaseBrand.Disclaimer, cut.Find("#showcase-disclaimer").TextContent);
        Assert.Contains($"Not affiliated with {Brand(slug).Name}", cut.Markup);
        Assert.Empty(cut.FindAll("img"));
    }

    [Fact]
    public void Page_LeavesTheSiteThemeAlone_UntilItRunsInTheBrowser()
    {
        var cut = Render(ShowcaseBrand.Lego, interactive: false);

        Assert.Same(Theme, Options.Theme);
        Assert.Contains("Loading the LEGO-inspired theme", cut.Find("[role='status']").TextContent);
        Assert.Empty(cut.FindAll("h1"));
        Assert.Equal(ShowcaseBrand.Disclaimer, cut.Find("#showcase-disclaimer").TextContent);
    }

    [Fact]
    public async Task Page_PutsTheSiteThemeBack_WhenTheVisitorLeaves()
    {
        Render(ShowcaseBrand.Tesco);
        Assert.Same(ShowcaseBrand.Tesco.Theme, Options.Theme);

        await TestContext.DisposeComponentsAsync();

        Assert.Same(Theme, Options.Theme);
    }

    [Fact]
    public void ThemeSwitcher_KeepsTheNextPagesTheme_WhenOneShowcasePageReplacesAnother()
    {
        ShowcaseThemeSwitcher.Apply(Options, ShowcaseBrand.Apple);

        // The page being navigated to starts before the page being left is disposed.
        ShowcaseThemeSwitcher.Apply(Options, ShowcaseBrand.Cadbury);
        ShowcaseThemeSwitcher.Restore(Options, ShowcaseBrand.Apple);
        Assert.Same(ShowcaseBrand.Cadbury.Theme, Options.Theme);

        ShowcaseThemeSwitcher.Restore(Options, ShowcaseBrand.Cadbury);
        Assert.Same(Theme, Options.Theme);
    }

    [Fact]
    public void Page_CountsWhatIsAddedToTheBasket()
    {
        var cut = Render(ShowcaseBrand.Microsoft);
        Assert.Contains("Your basket is empty.", cut.Find("#showcase-products [aria-live]").TextContent);

        cut.Find("#showcase-products button").Click();
        Assert.Contains("1 item in your basket.", cut.Find("#showcase-products [aria-live]").TextContent);

        cut.Find("#showcase-products button").Click();
        Assert.Contains("2 items in your basket.", cut.Find("#showcase-products [aria-live]").TextContent);
    }

    [Fact]
    public void Page_LinksToEveryExample_AndMarksTheCurrentOne()
    {
        var cut = Render(ShowcaseBrand.Cadbury);
        var links = cut.FindAll("nav[aria-label='Example themes'] a");

        Assert.Equal(ShowcaseBrand.All.Select(brand => brand.Path), links.Select(link => link.GetAttribute("href")));
        Assert.Equal("Cadbury", Assert.Single(links, link => link.GetAttribute("aria-current") == "page").TextContent);
    }

    [Fact]
    public void PillButtons_StayPills_WhateverRoundingAButtonIsGiven()
    {
        var cut = Render(ShowcaseBrand.Apple);

        var buttons = cut.FindAll("#showcase-hero-heading ~ div a, #showcase-products button");

        Assert.Equal(2 + ShowcaseBrand.Apple.Products.Count, buttons.Count);
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
    public void ThemePage_LinksToEveryExample_WithTheDisclaimer()
    {
        var page = TestContext.Render<ThemePage>();

        Assert.Equal(ShowcaseBrand.All.Select(brand => brand.Path), page.FindAll("a[id^='theme-example-']").Select(link => link.GetAttribute("href")));
        Assert.Equal(ShowcaseBrand.Disclaimer, page.Find("#theme-examples-disclaimer").TextContent);
    }

    [Fact]
    public void Disclaimer_NamesEveryBrand()
    {
        Assert.All((string[])["Microsoft", "Apple", "LEGO", "Tesco", "Cadbury"], name => Assert.Contains(name, ShowcaseBrand.Disclaimer));
        Assert.Contains("not affiliated", ShowcaseBrand.Disclaimer);
    }

    #endregion
}