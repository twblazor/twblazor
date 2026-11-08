using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using TwBlazor.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.TextEditor;

public class TwTextEditorExportTests
{
    private const string sample =
        "<h1>Title</h1><p>Some <strong>bold</strong>, <em>italic</em>, <u>under</u>, <s>gone</s> and <a href=\"https://example.com\">a link</a>.</p>"
        + "<ul><li>One</li><li>Two<ul><li>Nested</li></ul></li></ul><ol><li>First</li><li>Second</li></ol><ol><li>Again</li></ol>"
        + "<blockquote><p>Quoted</p></blockquote><pre>code  line\n  indented</pre>";

    private static readonly XNamespace _w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
    private static readonly XNamespace _r = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace _text = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
    private static readonly XNamespace _xlink = "http://www.w3.org/1999/xlink";

    private static ZipArchive Open(byte[] bytes) => new(new MemoryStream(bytes), ZipArchiveMode.Read);

    private static XDocument Part(ZipArchive zip, string path)
    {
        using var stream = zip.GetEntry(path)!.Open();
        return XDocument.Load(stream);
    }

    #region Markdown

    [Fact]
    public void Markdown_WritesHeadingsFormatsAndLinks()
    {
        var markdown = TwTextEditorExport.ToMarkdown(sample);

        Assert.StartsWith("# Title\n\nSome **bold**, *italic*, <u>under</u>, ~~gone~~ and [a link](https://example.com).\n", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void Markdown_WritesNestedLists_AndKeepsSeparateNumberedListsApart()
    {
        var markdown = TwTextEditorExport.ToMarkdown(sample);

        Assert.Contains("- One\n- Two\n    - Nested\n\n1. First\n2. Second\n\n<!-- -->\n\n1. Again\n", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void Markdown_WritesQuotesAndFencedCode()
    {
        var markdown = TwTextEditorExport.ToMarkdown(sample);

        Assert.EndsWith("> Quoted\n\n```\ncode  line\n  indented\n```\n", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void Markdown_NestsFormats_WithoutAmbiguousMarkers_AndKeepsSpacesOutside()
    {
        Assert.Equal("**bold *both*** plain\n", TwTextEditorExport.ToMarkdown("<p><b>bold <i>both</i></b> plain</p>"));
        Assert.Equal("a **b** c\n", TwTextEditorExport.ToMarkdown("<p>a<b> b </b>c</p>"));
    }

    [Fact]
    public void Markdown_EscapesCharactersMarkdownWouldReadAsFormatting()
    {
        Assert.Equal("\\*not\\* \\_em\\_ \\[x\\](y) \\<b\\> a\\\\b\n", TwTextEditorExport.ToMarkdown("<p>*not* _em_ [x](y) &lt;b&gt; a\\b</p>"));
        Assert.Equal("\\# not a heading\n\n1\\. not a list\n\n\\- nor this\n", TwTextEditorExport.ToMarkdown("<p># not a heading</p><p>1. not a list</p><p>- nor this</p>"));
    }

    [Fact]
    public void Markdown_WritesLineBreaks_AndInlineCode_WithBackticksInside()
    {
        Assert.Equal("one\\\ntwo\n", TwTextEditorExport.ToMarkdown("<p>one<br>two</p>"));
        Assert.Equal("use ``a`b``\n", TwTextEditorExport.ToMarkdown("<p>use <code>a`b</code></p>"));
    }

    [Fact]
    public void Markdown_UsesALongerFence_WhenTheCodeHoldsOne()
    {
        Assert.Equal("````\n```\n````\n", TwTextEditorExport.ToMarkdown("<pre>```</pre>"));
    }

    [Fact]
    public void Markdown_WrapsLinkAddressesWithSpaces()
    {
        Assert.Equal("[doc](</my file.html>)\n", TwTextEditorExport.ToMarkdown("<p><a href=\"/my file.html\">doc</a></p>"));
    }

    [Fact]
    public void Markdown_IsEmpty_ForAnEmptyEditor()
    {
        Assert.Equal(string.Empty, TwTextEditorExport.ToMarkdown(string.Empty));
        Assert.Equal(string.Empty, TwTextEditorExport.ToMarkdown("<p><br></p>"));
    }

    #endregion

    #region Plain text

    [Fact]
    public void PlainText_WritesMarkersQuotesAndLinkAddresses()
    {
        var text = TwTextEditorExport.ToPlainText(sample);

        Assert.Equal(
            "Title\n\nSome bold, italic, under, gone and a link (https://example.com).\n\n• One\n• Two\n  • Nested\n\n1. First\n2. Second\n\n1. Again\n\n> Quoted\n\ncode  line\n  indented\n",
            text);
    }

    [Fact]
    public void PlainText_LeavesOutALinksAddress_WhenItIsTheLinksText()
    {
        Assert.Equal("Mail ada@example.com\n", TwTextEditorExport.ToPlainText("<p>Mail <a href=\"mailto:ada@example.com\">ada@example.com</a></p>"));
    }

    #endregion

    #region HTML

    [Fact]
    public void HtmlDocument_IsAFullPage_WithTheCleanContent()
    {
        var page = TwTextEditorExport.ToHtmlDocument("<p onclick=\"x\">Hi<script>bad()</script></p>", "Notes & more", "en-GB");

        Assert.StartsWith("<!DOCTYPE html>", page, StringComparison.Ordinal);
        Assert.Contains("<html lang=\"en-GB\">", page, StringComparison.Ordinal);
        Assert.Contains("<title>Notes &amp; more</title>", page, StringComparison.Ordinal);
        Assert.Contains("<meta charset=\"utf-8\">", page, StringComparison.Ordinal);
        Assert.Contains("<p>Hi</p>", page, StringComparison.Ordinal);
        Assert.DoesNotContain("bad()", page, StringComparison.Ordinal);
        Assert.DoesNotContain("onclick", page, StringComparison.Ordinal);
    }

    #endregion

    #region RTF

    [Fact]
    public void Rtf_WritesFormatsListsAndLinks()
    {
        var rtf = TwTextEditorExport.ToRtf(sample, "Notes");

        Assert.StartsWith(@"{\rtf1\ansi", rtf, StringComparison.Ordinal);
        Assert.EndsWith("}", rtf, StringComparison.Ordinal);
        Assert.Contains(@"{\info{\title Notes}}", rtf, StringComparison.Ordinal);
        Assert.Contains(@"{\b bold}", rtf, StringComparison.Ordinal);
        Assert.Contains(@"{\i italic}", rtf, StringComparison.Ordinal);
        Assert.Contains(@"{\ul under}", rtf, StringComparison.Ordinal);
        Assert.Contains(@"{\strike gone}", rtf, StringComparison.Ordinal);
        Assert.Contains(@"HYPERLINK ""https://example.com""", rtf, StringComparison.Ordinal);
        Assert.Contains(@"\bullet\tab {One}", rtf, StringComparison.Ordinal);
        Assert.Contains(@"2.\tab {Second}", rtf, StringComparison.Ordinal);
        Assert.Contains(@"code  line\line   indented", rtf, StringComparison.Ordinal);
        Assert.Equal(CountOf(rtf, '{'), CountOf(rtf, '}'));
    }

    [Fact]
    public void Rtf_EscapesControlCharacters_AndWritesUnicodeAsEscapes()
    {
        var rtf = TwTextEditorExport.ToRtf("<p>a{b}c\\ é 日 \U0001F600</p>");

        Assert.Contains(@"{a\{b\}c\\ \u233?", rtf, StringComparison.Ordinal);
        Assert.Contains(@"\" + "u26085?", rtf, StringComparison.Ordinal);
        Assert.Contains(@"\u-10179?\u-8704?", rtf, StringComparison.Ordinal);
        Assert.True(rtf.All(c => c < 128));
    }

    private static int CountOf(string text, char c) => text.Count(ch => ch == c) - text.Split("\\" + c).Length + 1;

    #endregion

    #region Word

    [Fact]
    public void Docx_IsAValidPackage_WithTheTitle()
    {
        using var zip = Open(TwTextEditorExport.ToDocx(sample, "Notes"));

        foreach (var part in new[] { "[Content_Types].xml", "_rels/.rels", "word/document.xml", "word/styles.xml", "word/numbering.xml", "word/_rels/document.xml.rels", "docProps/core.xml" })
        {
            Assert.NotNull(zip.GetEntry(part));
            Part(zip, part);
        }

        XNamespace dc = "http://purl.org/dc/elements/1.1/";
        Assert.Equal("Notes", Part(zip, "docProps/core.xml").Root!.Element(dc + "title")!.Value);
    }

    [Fact]
    public void Docx_UsesWordsHeadingQuoteAndCodeStyles()
    {
        using var zip = Open(TwTextEditorExport.ToDocx(sample));
        var styles = Part(zip, "word/document.xml").Descendants(_w + "pStyle").Select(s => (string)s.Attribute(_w + "val")!).ToList();

        Assert.Equal("Heading1", styles[0]);
        Assert.Contains("Quote", styles);
        Assert.Contains("Code", styles);
        var styleIds = Part(zip, "word/styles.xml").Descendants(_w + "style").Select(s => (string)s.Attribute(_w + "styleId")!);
        Assert.Superset(styles.ToHashSet(), styleIds.ToHashSet());
    }

    [Fact]
    public void Docx_WritesRealLists_WithEachNumberedListStartingAgain()
    {
        using var zip = Open(TwTextEditorExport.ToDocx(sample));
        var items = Part(zip, "word/document.xml").Descendants(_w + "numPr")
            .Select(n => (Level: (string)n.Element(_w + "ilvl")!.Attribute(_w + "val")!, Id: (string)n.Element(_w + "numId")!.Attribute(_w + "val")!))
            .ToList();

        Assert.Equal(["0", "0", "1", "0", "0", "0"], items.Select(i => i.Level));
        Assert.Equal(items[0].Id, items[2].Id);
        Assert.Equal(items[3].Id, items[4].Id);
        Assert.NotEqual(items[4].Id, items[5].Id);

        var numbering = Part(zip, "word/numbering.xml").Root!;
        var restarted = numbering.Elements(_w + "num").Single(n => (string)n.Attribute(_w + "numId")! == items[5].Id);
        Assert.NotEmpty(restarted.Elements(_w + "lvlOverride"));
    }

    [Fact]
    public void Docx_WritesFormatsAndHyperlinks()
    {
        using var zip = Open(TwTextEditorExport.ToDocx(sample));
        var document = Part(zip, "word/document.xml");

        Assert.Contains(document.Descendants(_w + "r"), r => r.Element(_w + "rPr")?.Element(_w + "b") is not null && r.Value == "bold");
        Assert.Contains(document.Descendants(_w + "r"), r => r.Element(_w + "rPr")?.Element(_w + "strike") is not null && r.Value == "gone");

        var link = Assert.Single(document.Descendants(_w + "hyperlink"));
        Assert.Equal("a link", link.Value);
        var id = (string)link.Attribute(_r + "id")!;
        var relationship = Part(zip, "word/_rels/document.xml.rels").Root!.Elements().Single(e => (string)e.Attribute("Id")! == id);
        Assert.Equal("https://example.com", (string)relationship.Attribute("Target")!);
        Assert.Equal("External", (string)relationship.Attribute("TargetMode")!);
    }

    [Fact]
    public void Docx_KeepsSpaces_AndDropsCharactersXmlCannotHold()
    {
        using var zip = Open(TwTextEditorExport.ToDocx("<p>a\u0001b</p><pre>  two  spaces</pre>"));
        var texts = Part(zip, "word/document.xml").Descendants(_w + "t").ToList();

        Assert.Equal("ab", texts[0].Value);
        Assert.Equal("  two  spaces", texts[1].Value);
        Assert.All(texts, t => Assert.Equal("preserve", (string)t.Attribute(XNamespace.Xml + "space")!));
    }

    #endregion

    #region OpenDocument

    [Fact]
    public void Odt_StartsWithAnUncompressedMimetype_AndListsItsParts()
    {
        using var zip = Open(TwTextEditorExport.ToOdt(sample, "Notes"));

        var mimetype = zip.Entries[0];
        Assert.Equal("mimetype", mimetype.FullName);
        Assert.Equal(mimetype.Length, mimetype.CompressedLength);
        using (var reader = new StreamReader(mimetype.Open()))
        {
            Assert.Equal(TwTextEditorExport.OdtContentType, reader.ReadToEnd());
        }

        XNamespace manifest = "urn:oasis:names:tc:opendocument:xmlns:manifest:1.0";
        var paths = Part(zip, "META-INF/manifest.xml").Descendants(manifest + "file-entry").Select(e => (string)e.Attribute(manifest + "full-path")!);
        Assert.Equal(["/", "content.xml", "styles.xml", "meta.xml"], paths);

        XNamespace dc = "http://purl.org/dc/elements/1.1/";
        Assert.Equal("Notes", Part(zip, "meta.xml").Descendants(dc + "title").Single().Value);
    }

    [Fact]
    public void Odt_WritesHeadingsWithOutlineLevels_AndLinks()
    {
        using var zip = Open(TwTextEditorExport.ToOdt(sample));
        var content = Part(zip, "content.xml");

        var heading = content.Descendants(_text + "h").Single();
        Assert.Equal("1", (string)heading.Attribute(_text + "outline-level")!);
        Assert.Equal("Title", heading.Value);

        var link = content.Descendants(_text + "a").Single();
        Assert.Equal("https://example.com", (string)link.Attribute(_xlink + "href")!);
    }

    [Fact]
    public void Odt_NestsLists_AndStartsANewListForEachList()
    {
        using var zip = Open(TwTextEditorExport.ToOdt(sample));
        var body = Part(zip, "content.xml").Descendants(_text + "list").Where(l => l.Parent!.Name.LocalName == "text").ToList();

        Assert.Equal(3, body.Count);
        var nested = body[0].Descendants(_text + "list").Single();
        Assert.Equal("Nested", nested.Value);
        Assert.Equal("List_Bullet", (string)body[0].Attribute(_text + "style-name")!);
        Assert.Equal("List_Number", (string)body[1].Attribute(_text + "style-name")!);
        Assert.Equal(["First", "Second"], body[1].Elements(_text + "list-item").Select(i => i.Value));
    }

    [Fact]
    public void Odt_WritesRepeatedSpaces_AndLineBreaks_AsElements()
    {
        using var zip = Open(TwTextEditorExport.ToOdt("<pre>a   b\n  c</pre>"));
        var paragraph = Part(zip, "content.xml").Descendants(_text + "p").Single();

        var spaces = paragraph.Elements(_text + "s").ToList();
        Assert.Equal("2", (string)spaces[0].Attribute(_text + "c")!);
        Assert.Single(paragraph.Elements(_text + "line-break"));
        Assert.Equal("2", (string)spaces[1].Attribute(_text + "c")!);
    }

    [Fact]
    public void Odt_GivesEachFormatCombinationOneSpanStyle()
    {
        using var zip = Open(TwTextEditorExport.ToOdt("<p><b>a</b> <b>b</b> <i>c</i></p>"));
        var content = Part(zip, "content.xml");
        XNamespace style = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";

        Assert.Equal(2, content.Descendants(style + "style").Count(s => (string)s.Attribute(style + "family")! == "text"));
        Assert.Equal(3, content.Descendants(_text + "span").Count());
    }

    #endregion

    #region Export

    [Theory]
    [InlineData(TextEditorExportFormat.Markdown, "Notes.md", TwTextEditorExport.MarkdownContentType)]
    [InlineData(TextEditorExportFormat.Docx, "Notes.docx", TwTextEditorExport.DocxContentType)]
    [InlineData(TextEditorExportFormat.Odt, "Notes.odt", TwTextEditorExport.OdtContentType)]
    [InlineData(TextEditorExportFormat.Html, "Notes.html", TwTextEditorExport.HtmlContentType)]
    [InlineData(TextEditorExportFormat.PlainText, "Notes.txt", TwTextEditorExport.PlainTextContentType)]
    [InlineData(TextEditorExportFormat.Rtf, "Notes.rtf", TwTextEditorExport.RtfContentType)]
    public void Export_NamesTheFile_AndSetsItsMediaType(TextEditorExportFormat format, string fileName, string contentType)
    {
        var file = TwTextEditorExport.Export(sample, format, "Notes");

        Assert.Equal(fileName, file.FileName);
        Assert.Equal(contentType, file.ContentType);
        Assert.NotEmpty(file.Content);
        Assert.EndsWith(TwTextEditorExport.ExtensionOf(format), file.FileName, StringComparison.Ordinal);
    }

    [Fact]
    public void Export_WritesPlainText_WithAByteOrderMark_AndWindowsLineBreaks()
    {
        var file = TwTextEditorExport.Export("<p>é</p><p>b</p>", TextEditorExportFormat.PlainText);

        Assert.Equal(Encoding.UTF8.GetPreamble(), file.Content[..3]);
        Assert.Equal("é\r\n\r\nb\r\n", Encoding.UTF8.GetString(file.Content[3..]));
    }

    [Fact]
    public void Export_WritesMarkdown_InUtf8WithoutAByteOrderMark()
    {
        var file = TwTextEditorExport.Export("<p>é</p>", TextEditorExportFormat.Markdown);

        Assert.Equal("é\n", Encoding.UTF8.GetString(file.Content));
        Assert.NotEqual(0xEF, file.Content[0]);
    }

    [Theory]
    [InlineData("a/b:c*?", "a_b_c__.md")]
    [InlineData("  ", "document.md")]
    [InlineData("trailing.", "trailing.md")]
    public void Export_ReplacesCharactersAFileNameCannotHold(string name, string expected)
    {
        Assert.Equal(expected, TwTextEditorExport.Export("<p>x</p>", TextEditorExportFormat.Markdown, name).FileName);
    }

    [Fact]
    public void Export_RefusesAnUnknownFormat()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TwTextEditorExport.Export("<p>x</p>", (TextEditorExportFormat)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => TwTextEditorExport.ExtensionOf((TextEditorExportFormat)99));
    }

    #endregion
}
