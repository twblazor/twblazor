using Bunit;
using LinkPage = TwBlazor.Docs.Pages.Link.Link;
using TextPage = TwBlazor.Docs.Pages.Text.Text;

namespace TwBlazor.Docs.Tests.Pages;

/// <summary>
/// Renders the typography pages, so an example that throws while rendering fails a test.
/// </summary>
public class TypographyPagesTests : DocsTestBase
{
    [Fact]
    public void TextPage_RendersItsExamples()
    {
        var page = TestContext.Render<TextPage>();

        Assert.Contains("A paragraph rendered by TwText.", page.Markup);
        Assert.Contains("5xl bold", page.Markup);
        Assert.Contains("A long line of text that no longer fits its container.", page.Markup);
        Assert.NotNull(page.Find("h1"));
    }

    [Fact]
    public void LinkPage_RendersItsExamples()
    {
        var page = TestContext.Render<LinkPage>();

        Assert.Contains("Primary link", page.Markup);
        Assert.Contains("Secondary neutral link", page.Markup);
        Assert.Contains("(opens in new tab)", page.Markup);
        Assert.NotEmpty(page.FindAll("a[href='#']"));
    }
}
