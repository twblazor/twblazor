using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Docs.Layout;
using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Tests.Layout;

public class PageContainerTests : DocsTestBase
{
    private IRenderedComponent<PageContainer> Render(string path = "/no-such-page", string? seoTitle = null) =>
        TestContext.Render<PageContainer>(parameters => parameters
            .Add(p => p.Title, "TwCard")
            .Add(p => p.Path, path)
            .Add(p => p.Description, "TwCard is a Blazor card component.")
            .Add(p => p.SeoTitle, seoTitle)
            .Add(p => p.ChildContent, (RenderFragment)(builder => builder.AddMarkupContent(0, "<p id=\"body\">Body</p>"))));

    [Fact]
    public void Render_ShowsTheDescriptionAsTheIntroUnderTheHeading()
    {
        // Arrange & Act
        var cut = Render();

        // Assert - answer engines quote body text, so the intro must be in the page, not only in a meta tag.
        Assert.Equal("TwCard", cut.Find("h1").TextContent);
        Assert.Equal("TwCard is a Blazor card component.", cut.Find("h1 + p").TextContent);
    }

    [Fact]
    public void Render_TitlesThePageWithTheNamePipeSiteDashSummaryShape()
    {
        // Arrange
        var head = TestContext.Render<Microsoft.AspNetCore.Components.Web.HeadOutlet>();

        // Act
        Render();
        head.WaitForState(() => head.Markup.Contains("<title>"));

        // Assert
        Assert.Equal("TwCard | twblazor - Blazor Card Component", head.Find("title").TextContent);
    }

    [Fact]
    public void Render_UsesTheExplicitSeoTitle_WhenOneIsGiven()
    {
        // Arrange
        var head = TestContext.Render<Microsoft.AspNetCore.Components.Web.HeadOutlet>();

        // Act
        Render(seoTitle: "Get Started | twblazor - Install Tailwind CSS Components for Blazor");
        head.WaitForState(() => head.Markup.Contains("<title>"));

        // Assert
        Assert.Equal("Get Started | twblazor - Install Tailwind CSS Components for Blazor", head.Find("title").TextContent);
    }

    [Fact]
    public void Render_ShowsNoLastUpdatedLine_WhenNoDateIsKnown()
    {
        // Arrange & Act
        var cut = Render("/no-such-page");

        // Assert
        Assert.DoesNotContain("Last updated", cut.Markup);
        Assert.Empty(cut.FindAll("time"));
    }

    [Fact]
    public void Render_RendersTheChildContentAfterTheIntro()
    {
        // Arrange & Act
        var cut = Render();

        // Assert
        Assert.Equal("Body", cut.Find("#body").TextContent);
    }

    [Fact]
    public void Render_ShowsTheComponentCategoryAboveTheHeading()
    {
        // Arrange
        var (category, entry) = ComponentCatalog.LoadLeafEntries().First();

        // Act
        var cut = Render(entry.Url);

        // Assert
        Assert.Equal(category, cut.Find("h1").PreviousElementSibling!.TextContent);
    }

    [Fact]
    public void Render_ShowsTheCategoryInNormalCase()
    {
        // Arrange
        var (_, entry) = ComponentCatalog.LoadLeafEntries().First();

        // Act
        var cut = Render(entry.Url);

        // Assert
        Assert.DoesNotContain("uppercase", cut.Find("h1").PreviousElementSibling!.ClassList);
    }

    [Fact]
    public void Render_UsesTheGivenCategory_OverTheCatalogOne()
    {
        // Arrange & Act
        var cut = TestContext.Render<PageContainer>(parameters => parameters
            .Add(p => p.Title, "Get Started")
            .Add(p => p.Path, "/get-started")
            .Add(p => p.Description, "Install it.")
            .Add(p => p.Category, "Introduction"));

        // Assert
        Assert.Equal("Introduction", cut.Find("h1").PreviousElementSibling!.TextContent);
    }

    [Fact]
    public void Render_ShowsNoCategory_WhenThePageIsNotInTheCatalog()
    {
        // Arrange & Act
        var cut = Render("/no-such-page");

        // Assert
        Assert.Null(cut.Find("h1").PreviousElementSibling);
    }

    private static readonly string[] _cardTitles = ["Basic", "Colors"];

    [Fact]
    public void Render_ListsEachPageCardInTheOnThisPageNavigation()
    {
        // Arrange & Act
        var cut = TestContext.Render<PageContainer>(parameters => parameters
            .Add(p => p.Title, "TwCard")
            .Add(p => p.Path, "/card")
            .Add(p => p.Description, "TwCard is a Blazor card component.")
            .Add(p => p.ChildContent, (RenderFragment)(builder =>
            {
                foreach (var title in _cardTitles)
                {
                    builder.OpenComponent<PageCard>(0);
                    builder.AddAttribute(1, nameof(PageCard.Title), title);
                    builder.CloseComponent();
                }
            })));

        // Assert
        var links = cut.FindAll("nav a");
        Assert.Equal(["Basic", "Colors"], links.Select(link => link.TextContent));
        Assert.All(links, link => Assert.NotNull(cut.Find("#" + link.GetAttribute("href")!.Split('#')[1])));
    }

    [Fact]
    public void Render_ShowsNoOutline_WhenThePageHasNoCards()
    {
        // Arrange & Act
        var cut = Render();

        // Assert
        Assert.Empty(cut.FindAll("nav"));
    }

    [Fact]
    public void Render_ShowsNoThemeCard_WhenNoThemeTypeIsGiven()
    {
        // Arrange & Act
        var cut = Render();

        // Assert
        Assert.DoesNotContain("Theme Configuration", cut.Markup);
    }
}
