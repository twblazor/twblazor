using Bunit;
using ContainerPage = TwBlazor.Docs.Pages.Container.Container;
using FlexPage = TwBlazor.Docs.Pages.Flex.Flex;
using GridPage = TwBlazor.Docs.Pages.Grid.Grid;

namespace TwBlazor.Docs.Tests.Pages;

/// <summary>
/// Renders the layout component pages, so an example that throws while rendering fails a test.
/// </summary>
public class LayoutPagesTests : DocsTestBase
{
    [Fact]
    public void GridPage_RendersEveryExample()
    {
        var page = TestContext.Render<GridPage>();

        Assert.Equal(7, page.FindAll("ul[aria-label='Photos'] > li").Count);
        Assert.Contains("Lake District", page.Markup);
        Assert.Equal(3, page.FindAll("article button").Count(button => button.TextContent.Contains("Choose plan", StringComparison.Ordinal)));
    }

    [Fact]
    public void FlexPage_RendersEveryExample()
    {
        var page = TestContext.Render<FlexPage>();

        Assert.Equal(4, page.FindAll("ul[aria-label='Account summary'] > li").Count);
        Assert.Equal(10, page.FindAll("ul[aria-label='Topics'] > li").Count);
    }

    [Fact]
    public void ContainerPage_RendersEveryExample()
    {
        var page = TestContext.Render<ContainerPage>();

        Assert.NotNull(page.Find("section[aria-labelledby='container-offers-heading']"));
        Assert.Contains("max-w-2xl", page.Find("article").ClassName);
        Assert.NotEmpty(page.FindAll("pre"));
        Assert.Empty(page.FindAll("div.bg-gray-950 pre"));
    }
}
