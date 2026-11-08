using TwBlazor.Export.RichText;

namespace TwBlazor.Tests.Export;

public class RichTextDocumentTests
{
    private static IReadOnlyList<RichTextBlock> Blocks(string html) => RichTextDocument.Parse(html).Blocks;

    private static string TextOf(RichTextBlock block) => string.Concat(block.Runs.Select(r => r.IsBreak ? "\n" : r.Text));

    [Fact]
    public void Reads_ParagraphsAndHeadings()
    {
        var blocks = Blocks("<h1>One</h1><h2>Two</h2><h3>Three</h3><h5>Five</h5><p>Text</p>");

        Assert.Equal(
            [RichTextBlockKind.Heading1, RichTextBlockKind.Heading2, RichTextBlockKind.Heading3, RichTextBlockKind.Heading3, RichTextBlockKind.Paragraph],
            blocks.Select(b => b.Kind));
        Assert.Equal("Five", TextOf(blocks[3]));
    }

    [Fact]
    public void Reads_InlineFormats_AndJoinsRunsWithTheSameFormats()
    {
        var block = Assert.Single(Blocks("<p>a<b>b</b><strong>c</strong><i>d</i><u>e</u><s>f</s><del>g</del><code>h</code></p>"));

        Assert.Equal(["a", "bc", "d", "e", "fg", "h"], block.Runs.Select(r => r.Text));
        Assert.True(block.Runs[1].Bold);
        Assert.True(block.Runs[2].Italic);
        Assert.True(block.Runs[3].Underline);
        Assert.True(block.Runs[4].Strike);
        Assert.True(block.Runs[5].Code);
    }

    [Fact]
    public void Reads_NestedFormats()
    {
        var block = Assert.Single(Blocks("<p><strong>bold <em>both</em></strong></p>"));

        Assert.False(block.Runs[0].Italic);
        Assert.True(block.Runs[1] is { Bold: true, Italic: true, Text: "both" });
    }

    [Fact]
    public void Reads_SafeLinks_AndDropsUnsafeOnes()
    {
        var blocks = Blocks("<p><a href=\"https://example.com\">ok</a> <a href=\"javascript:alert(1)\">bad</a> <a href='/docs'>rel</a></p>");

        var runs = blocks[0].Runs;
        Assert.Equal("https://example.com", runs[0].Href);
        Assert.Null(runs.Single(r => r.Text.Contains("bad", StringComparison.Ordinal)).Href);
        Assert.Equal("/docs", runs[^1].Href);
    }

    [Theory]
    [InlineData("https://a.b", "https://a.b")]
    [InlineData("mailto:a@b.c", "mailto:a@b.c")]
    [InlineData("tel:123", "tel:123")]
    [InlineData("#top", "#top")]
    [InlineData("page.html", "page.html")]
    [InlineData("javascript:x", null)]
    [InlineData("java\nscript:x", null)]
    [InlineData(" JAVASCRIPT:x", null)]
    [InlineData("data:text/html,x", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void SafeHref_KeepsOnlySafeAddresses(string? href, string? expected)
    {
        Assert.Equal(expected, RichTextDocument.SafeHref(href));
    }

    [Fact]
    public void Collapses_WhiteSpace_LikeHtml_ButKeepsNonBreakingSpaces()
    {
        var block = Assert.Single(Blocks("<p>\n  one   two\n<b> three </b> four&nbsp;&nbsp;five  </p>"));

        Assert.Equal("one two three four  five", TextOf(block));
    }

    [Fact]
    public void Reads_LineBreaks_AndDropsTrailingOnes()
    {
        var block = Assert.Single(Blocks("<p>one<br>two<br></p>"));

        Assert.Equal("one\ntwo", TextOf(block));
    }

    [Fact]
    public void Reads_CodeBlocks_KeepingTheirSpacesAndLines()
    {
        var block = Assert.Single(Blocks("<pre>\n  if (a &lt; b)\n    run();<br>done\n</pre>"));

        Assert.Equal(RichTextBlockKind.Code, block.Kind);
        Assert.Equal("  if (a < b)\n    run();\ndone", block.CodeText.ToString());
    }

    [Fact]
    public void Reads_Lists_WithLevelsNumbersAndSeparateIds()
    {
        var blocks = Blocks("<ol><li>a</li><li>b<ul><li>c</li></ul></li></ol><ol><li>d</li></ol>");

        Assert.All(blocks, b => Assert.Equal(RichTextBlockKind.ListItem, b.Kind));
        Assert.Equal([0, 0, 1, 0], blocks.Select(b => b.ListLevel));
        Assert.Equal([1, 2, 1, 1], blocks.Select(b => b.Number));
        Assert.Equal([true, true, false, true], blocks.Select(b => b.Ordered));
        Assert.NotEqual(blocks[0].ListId, blocks[3].ListId);
        Assert.Equal(blocks[0].ListId, blocks[1].ListId);
    }

    [Fact]
    public void Treats_AParagraphStartingAListItem_AsTheItemsText()
    {
        var block = Assert.Single(Blocks("<ul><li><p>item</p></li></ul>"));

        Assert.Equal(RichTextBlockKind.ListItem, block.Kind);
        Assert.Equal("item", TextOf(block));
    }

    [Fact]
    public void Reads_Quotes_WithTheirDepth()
    {
        var blocks = Blocks("<blockquote><p>one</p><blockquote>two</blockquote></blockquote><p>three</p>");

        Assert.Equal([1, 2, 0], blocks.Select(b => b.QuoteDepth));
        Assert.Equal("two", TextOf(blocks[1]));
    }

    [Fact]
    public void Wraps_LooseText_InAParagraph_AndSkipsEmptyParagraphs()
    {
        var blocks = Blocks("loose <b>text</b><p></p><p> </p><p><br></p><p>next</p>");

        Assert.Equal(["loose text", "next"], blocks.Select(TextOf));
    }

    [Fact]
    public void Drops_ScriptsAndStyles_WithWhatIsInsideThem_AndUnwrapsUnknownTags()
    {
        var blocks = Blocks("<p>a<script>alert('<b>x</b>')</script><style>p{}</style><span class=\"x\">b</span><img src=x onerror=y>c<!-- note --></p>");

        Assert.Equal("abc", TextOf(Assert.Single(blocks)));
    }

    [Fact]
    public void Survives_MalformedHtml()
    {
        var blocks = Blocks("<p>one <b>two</p><p>three < four</p></div></b><p");

        Assert.Equal("one two", TextOf(blocks[0]));
        Assert.Equal("three < four", TextOf(blocks[1]));
    }

    [Fact]
    public void Reads_NothingFromNullOrEmptyHtml()
    {
        Assert.Empty(Blocks(string.Empty));
        Assert.Empty(RichTextDocument.Parse(null).Blocks);
    }

    [Fact]
    public void ToCleanHtml_KeepsOnlyTheEditorsTags_AndEscapesText()
    {
        var clean = RichTextDocument.Parse("<div onclick=x>a &amp; <b style=y>b</b><a href=\"javascript:x\">c</a><a href=\"https://e.com?a=1&amp;b=2\">d</a><br><font>e</font></div><h4>f</h4>").ToCleanHtml();

        Assert.Equal("<p>a &amp; <strong>b</strong>c<a href=\"https://e.com?a=1&amp;b=2\">d</a><br>e</p><h3>f</h3>", clean);
    }
}
