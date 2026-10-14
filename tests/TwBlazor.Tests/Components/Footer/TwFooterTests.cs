using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using IconEnum = TwBlazor.Enums.Icon;
using TwBlazor.Models;

namespace TwBlazor.Tests.Components.Footer;

/// <summary>
/// Tests <see cref="TwFooter"/>: what it renders from its sections, slots and bottom row, and how it is exposed
/// to assistive technology.
/// </summary>
public class TwFooterTests : TwBlazorTestBase
{
    private static readonly FooterSection[] _sections =
    [
        new("Product", [("Features", "/features"), ("Pricing", "/pricing")]),
        new("Company", [new FooterLink("About", "/about"), new FooterLink("Blog", "https://blog.example.com", IconEnum.Rss, OpensInNewTab: true)])
    ];

    private TwFooterTheme theme => Theme.Components.Require<TwFooterTheme>();

    private IRenderedComponent<TwFooter> Render(Action<ComponentParameterCollectionBuilder<TwFooter>>? configure = null) =>
        TestContext.Render<TwFooter>(p =>
        {
            p.Add(x => x.Sections, _sections);
            configure?.Invoke(p);
        });

    #region Structure

    [Fact]
    public void RendersAFooterLandmark_WithTheThemesClasses()
    {
        var footer = Render(p => p.Add(x => x.Class, "custom")).Find("footer");

        Assert.Contains("custom", footer.ClassName);
        Assert.All(theme.Footer.Split(' ', StringSplitOptions.RemoveEmptyEntries), value => Assert.Contains(value, footer.ClassList));
    }

    [Fact]
    public void RendersEachSection_AsAHeadingAndAListOfItsLinks()
    {
        var cut = Render();
        var lists = cut.FindAll("nav ul");

        Assert.Equal(["Product", "Company"], cut.FindAll("nav h2").Select(heading => heading.TextContent));
        Assert.Equal(2, lists.Count);
        Assert.Equal(["Features", "Pricing"], lists[0].QuerySelectorAll("li > a").Select(link => link.TextContent));
        Assert.Equal(["/features", "/pricing"], lists[0].QuerySelectorAll("li > a").Select(link => link.GetAttribute("href")));
    }

    [Fact]
    public void WithNothingSet_RendersAnEmptyFooter_WithoutNavigationOrBottomRow()
    {
        var cut = TestContext.Render<TwFooter>();

        Assert.Single(cut.FindAll("footer"));
        Assert.Empty(cut.FindAll("nav"));
        Assert.Empty(cut.FindAll("ul"));
        Assert.Empty(cut.FindAll("p"));
    }

    [Fact]
    public void LeavesOutSectionsAndLinks_ThatHaveNothingToShow()
    {
        FooterSection[] sections =
        [
            new("Kept", [("Home", "/"), ("No address", " "), (" ", "/blank-label")]),
            new(" ", [("Untitled", "/untitled")]),
            new("Empty", [])
        ];

        var cut = TestContext.Render<TwFooter>(p => p.Add(x => x.Sections, sections));

        Assert.Equal(["Kept"], cut.FindAll("nav h2").Select(heading => heading.TextContent));
        Assert.Equal(["Home"], cut.FindAll("nav a").Select(link => link.TextContent));
    }

    [Fact]
    public void RendersTheSlots_InReadingOrder()
    {
        var cut = Render(p => p
            .Add(x => x.BrandContent, (RenderFragment)(b => b.AddMarkupContent(0, "<span id=\"brand\">Acme</span>")))
            .Add(x => x.ChildContent, (RenderFragment)(b => b.AddMarkupContent(0, "<span id=\"extra\">Newsletter</span>")))
            .Add(x => x.BottomContent, (RenderFragment)(b => b.AddMarkupContent(0, "<span id=\"social\">Social</span>")))
            .Add(x => x.Copyright, "2026 Acme"));

        var markup = cut.Markup;
        int[] order = [markup.IndexOf("id=\"brand\""), markup.IndexOf("<nav"), markup.IndexOf("id=\"extra\""), markup.IndexOf("2026 Acme"), markup.IndexOf("id=\"social\"")];

        Assert.DoesNotContain(-1, order);
        Assert.Equal(order.Order(), order);
    }

    #endregion

    #region Bottom row

    [Fact]
    public void RendersTheCopyrightLine_AndTheBottomLinks()
    {
        FooterLink[] legal = [("Privacy", "/privacy"), ("Terms", "/terms")];
        var cut = Render(p => p.Add(x => x.Copyright, "2026 Acme Ltd").Add(x => x.BottomLinks, legal));

        Assert.Equal("2026 Acme Ltd", cut.Find("footer p").TextContent);
        Assert.Equal(["Privacy", "Terms"], cut.FindAll("footer > div > div ul a").Select(link => link.TextContent));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WithoutACopyrightLineOrBottomContent_HasNoBottomRow(string? copyright)
    {
        var cut = Render(p => p.Add(x => x.Copyright, copyright));

        Assert.Empty(cut.FindAll("footer p"));
        Assert.Equal(2, cut.FindAll("ul").Count);
    }

    [Fact]
    public void BottomContentAlone_StillRendersTheBottomRow()
    {
        var cut = TestContext.Render<TwFooter>(p => p.Add(x => x.BottomContent, (RenderFragment)(b => b.AddMarkupContent(0, "<span id=\"social\">Social</span>"))));

        Assert.Contains(theme.Bottom.Split(' ')[0], cut.Find("#social").ParentElement!.ClassName);
    }

    #endregion

    #region Accessibility

    [Fact]
    public void NamesItsNavigationLandmark_SoItCanBeToldFromOthersOnThePage()
    {
        Assert.Equal("Footer links", Render().Find("nav").GetAttribute("aria-label"));
        Assert.Equal("Site map", Render(p => p.Add(x => x.NavigationLabel, "Site map")).Find("nav").GetAttribute("aria-label"));
    }

    [Fact]
    public void NamesEachList_ByItsHeading()
    {
        var cut = Render();
        var headings = cut.FindAll("nav h2");
        var lists = cut.FindAll("nav ul");

        Assert.All(headings, heading => Assert.False(string.IsNullOrWhiteSpace(heading.Id)));
        Assert.Equal(headings.Select(heading => heading.Id), lists.Select(list => list.GetAttribute("aria-labelledby")));
        Assert.Equal(headings.Count, headings.Select(heading => heading.Id).Distinct().Count());
    }

    [Theory]
    [InlineData(2, "h2")]
    [InlineData(3, "h3")]
    [InlineData(0, "h1")]
    [InlineData(9, "h6")]
    public void UsesTheHeadingLevelAsked_KeptWithinTheLevelsThatExist(int level, string expected)
    {
        var cut = Render(p => p.Add(x => x.HeadingLevel, level));

        Assert.Equal(2, cut.FindAll($"nav {expected}").Count);
    }

    [Fact]
    public void ALinkThatOpensInANewTab_SaysSo_AndDoesNotHandOverTheOpener()
    {
        var link = Render().FindAll("nav a").Single(item => item.GetAttribute("href") == "https://blog.example.com");

        Assert.Equal("_blank", link.GetAttribute("target"));
        Assert.Equal("noopener noreferrer", link.GetAttribute("rel"));
        Assert.Equal("Blog (opens in a new tab)", link.TextContent);
        Assert.Contains("sr-only", link.QuerySelector("span")!.ClassName);
    }

    [Fact]
    public void AnOrdinaryLink_OpensInPlace()
    {
        var link = Render().FindAll("nav a")[0];

        Assert.False(link.HasAttribute("target"));
        Assert.False(link.HasAttribute("rel"));
        Assert.Empty(link.QuerySelectorAll("span"));
    }

    [Fact]
    public void ALinksIcon_IsDecorative()
    {
        var icon = Render().FindAll("nav a").Single(item => item.GetAttribute("href") == "https://blog.example.com").QuerySelector("i")!;

        Assert.Equal("true", icon.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void EveryLink_IsAnAnchorWithAnAddress_AndAVisibleFocusStyle()
    {
        var links = Render(p => p.Add(x => x.BottomLinks, [new FooterLink("Privacy", "/privacy")])).FindAll("footer a");

        Assert.Equal(5, links.Count);
        Assert.All(links, link =>
        {
            Assert.False(string.IsNullOrWhiteSpace(link.GetAttribute("href")));
            Assert.Contains("focus-visible:outline-2", link.ClassName);
            Assert.False(link.HasAttribute("tabindex"));
        });
    }

    [Fact]
    public void PassesItsAccessibleNameAndAttributesToTheFooter()
    {
        var footer = Render(p => p.Add(x => x.AriaLabel, "Site footer").Add(x => x.Id, "site-footer").AddUnmatched("data-area", "footer")).Find("footer");

        Assert.Equal("Site footer", footer.GetAttribute("aria-label"));
        Assert.Equal("site-footer", footer.Id);
        Assert.Equal("footer", footer.GetAttribute("data-area"));
    }

    #endregion

    #region Models

    [Fact]
    public void ALinkCanBeWrittenAsALabelAndAnAddress()
    {
        FooterLink link = ("Docs", "/docs");

        Assert.Equal(new FooterLink("Docs", "/docs"), link);
        Assert.Null(link.Icon);
        Assert.False(link.OpensInNewTab);
    }

    #endregion
}