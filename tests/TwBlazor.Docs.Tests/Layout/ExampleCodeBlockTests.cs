using Bunit;
using TwBlazor.Docs.Layout;

namespace TwBlazor.Docs.Tests.Layout;

public class ExampleCodeBlockTests : DocsTestBase
{
    private static string Lines(int count) => string.Join('\n', Enumerable.Range(1, count).Select(i => $"<p>Line {i}</p>"));

    private IRenderedComponent<ExampleCodeBlock> Render(string razor, string csharp = "") =>
        TestContext.Render<ExampleCodeBlock>(parameters => parameters
            .Add(p => p.Razor, razor)
            .Add(p => p.CSharp, csharp));

    [Fact]
    public void Render_ShowsAShortHtmlOnlySnippetDirectly()
    {
        // Arrange & Act
        var cut = Render(Lines(ExampleCodeBlock.ShortSnippetLineLimit - 1));

        // Assert
        Assert.Empty(cut.FindAll("button[aria-expanded]"));
        Assert.Single(cut.FindAll("pre"));
        Assert.DoesNotContain("Show code", cut.Markup);
    }

    [Fact]
    public void Render_KeepsASnippetOfTheLimitOrLongerInTheExpansionPanel()
    {
        // Arrange & Act
        var cut = Render(Lines(ExampleCodeBlock.ShortSnippetLineLimit));

        // Assert
        Assert.Contains("Show code", cut.Markup);
    }

    [Fact]
    public void Render_KeepsAShortSnippetInThePanel_WhenItHasCSharpToo()
    {
        // Arrange & Act
        var cut = Render(Lines(2), "private int count;");

        // Assert
        Assert.Contains("Show code", cut.Markup);
    }

    [Fact]
    public void Render_IgnoresSurroundingBlankLinesWhenCountingLines()
    {
        // Arrange & Act
        var cut = Render("\n" + Lines(ExampleCodeBlock.ShortSnippetLineLimit - 1) + "\n\n");

        // Assert
        Assert.DoesNotContain("Show code", cut.Markup);
    }

    [Fact]
    public void Render_KeepsACSharpOnlySnippetInThePanel()
    {
        // Arrange & Act
        var cut = Render(string.Empty, "private int count;");

        // Assert
        Assert.Contains("Show code", cut.Markup);
    }

    [Fact]
    public void Render_OmitsTheRazorBlock_WhenOnlyCSharpIsPresent()
    {
        // Arrange & Act
        var cut = Render(string.Empty, "private int count;");

        // Assert
        Assert.Single(cut.FindAll("pre"));
        Assert.Contains("@code", cut.Markup);
    }

    [Fact]
    public void Render_OmitsTheRazorBlock_WhenRazorIsWhitespace()
    {
        // Arrange & Act
        var cut = Render("  \n ", "private int count;");

        // Assert
        Assert.Single(cut.FindAll("pre"));
    }

    [Fact]
    public void Render_RendersNothing_WhenBothSnippetsAreEmpty()
    {
        // Arrange & Act
        var cut = Render(string.Empty);

        // Assert
        Assert.Empty(cut.FindAll("pre"));
        Assert.DoesNotContain("Show code", cut.Markup);
    }
}
