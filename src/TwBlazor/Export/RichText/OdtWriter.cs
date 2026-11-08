// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.Xml;
using TwBlazor.Export.Packaging;

namespace TwBlazor.Export.RichText;

/// <summary>
/// Writes a <see cref="RichTextDocument"/> as an OpenDocument text document (ODF 1.3).
/// </summary>
/// <remarks>Headings use the standard Heading 1 to 3 styles with outline levels, so they appear in the navigator and
/// a table of contents. Lists are real nested lists, each numbered list starting again from 1.</remarks>
internal static class OdtWriter
{
    /// <summary>The media type of an OpenDocument text document.</summary>
    public const string MediaType = "application/vnd.oasis.opendocument.text";

    private const string office = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
    private const string style = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";
    private const string text = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
    private const string fo = "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0";
    private const string xlink = "http://www.w3.org/1999/xlink";
    private const string dc = "http://purl.org/dc/elements/1.1/";
    private const string manifest = "urn:oasis:names:tc:opendocument:xmlns:manifest:1.0";
    private const string version = "1.3";
    private const string name = "name";
    private const string family = "family";
    private const string styleName = "style-name";

    private const string bulletListStyle = "List_Bullet";
    private const string numberListStyle = "List_Number";

    /// <summary>
    /// Writes the document.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="title">The document's title, stored in its properties, or <see langword="null"/> for none.</param>
    /// <returns>The contents of an <c>.odt</c> file.</returns>
    public static byte[] Write(RichTextDocument document, string? title)
    {
        var spanStyles = new SpanStyles();

        using var package = new ZipPackage();
        package.AddText("mimetype", MediaType, compress: false);
        package.AddXml("META-INF/manifest.xml", WriteManifest);
        package.AddXml("meta.xml", writer => WriteMeta(writer, title));
        package.AddXml("styles.xml", WriteStyles);
        package.AddXml("content.xml", writer => WriteContent(writer, document, spanStyles));
        return package.ToArray();
    }

    private static void WriteManifest(XmlWriter writer)
    {
        writer.WriteStartElement("manifest", "manifest", manifest);
        writer.WriteAttributeString("manifest", "version", manifest, version);
        WriteEntry("/", MediaType);
        WriteEntry("content.xml", "text/xml");
        WriteEntry("styles.xml", "text/xml");
        WriteEntry("meta.xml", "text/xml");
        writer.WriteEndElement();

        void WriteEntry(string path, string mediaType)
        {
            writer.WriteStartElement("manifest", "file-entry", manifest);
            writer.WriteAttributeString("manifest", "full-path", manifest, path);
            if (path == "/")
            {
                writer.WriteAttributeString("manifest", "version", manifest, version);
            }

            writer.WriteAttributeString("manifest", "media-type", manifest, mediaType);
            writer.WriteEndElement();
        }
    }

    private static void WriteMeta(XmlWriter writer, string? title)
    {
        writer.WriteStartElement("office", "document-meta", office);
        writer.WriteAttributeString("xmlns", "dc", null, dc);
        writer.WriteAttributeString("office", "version", office, version);
        writer.WriteStartElement("meta", office);
        if (!string.IsNullOrWhiteSpace(title))
        {
            writer.WriteElementString("dc", "title", dc, XmlCharacters.Clean(title));
        }

        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    #region Styles

    private static void WriteStyles(XmlWriter writer)
    {
        writer.WriteStartElement("office", "document-styles", office);
        WriteNamespaces(writer);
        writer.WriteAttributeString("office", "version", office, version);
        writer.WriteStartElement("styles", office);

        writer.WriteStartElement("style", "default-style", style);
        writer.WriteAttributeString(family, style, "paragraph");
        WriteParagraphProperties(writer, ("margin-top", "0cm"), ("margin-bottom", "0.28cm"), ("line-height", "115%"));
        WriteTextProperties(writer, ("font-family", "Calibri"), ("font-size", "11pt"));
        writer.WriteEndElement();

        WriteParagraphStyle(writer, "Standard", "Standard", null);
        WriteParagraphStyle(writer, "Heading_20_1", "Heading 1", 1, ("margin-top", "0.64cm"), ("margin-bottom", "0.21cm"), ("font-size", "16pt"), ("font-weight", "bold"));
        WriteParagraphStyle(writer, "Heading_20_2", "Heading 2", 2, ("margin-top", "0.49cm"), ("margin-bottom", "0.21cm"), ("font-size", "14pt"), ("font-weight", "bold"));
        WriteParagraphStyle(writer, "Heading_20_3", "Heading 3", 3, ("margin-top", "0.42cm"), ("margin-bottom", "0.21cm"), ("font-size", "12pt"), ("font-weight", "bold"));
        WriteParagraphStyle(writer, "Quotations", "Quotations", null, ("margin-left", "1.27cm"), ("font-style", "italic"), ("color", "#595959"));
        WriteParagraphStyle(writer, "Preformatted_20_Text", "Preformatted Text", null, ("margin-top", "0.21cm"), ("margin-bottom", "0.21cm"), ("background-color", "#f2f2f2"), ("padding", "0.1cm"), ("font-family", "Consolas"), ("font-size", "10pt"));

        writer.WriteStartElement("style", "style", style);
        writer.WriteAttributeString(name, style, "Internet_20_link");
        writer.WriteAttributeString("display-name", style, "Internet Link");
        writer.WriteAttributeString(family, style, "text");
        WriteTextProperties(writer, ("color", "#0563c1"), ("text-underline-style", "solid"), ("text-underline-width", "auto"), ("text-underline-color", "font-color"));
        writer.WriteEndElement();

        WriteListStyle(writer, bulletListStyle, ordered: false);
        WriteListStyle(writer, numberListStyle, ordered: true);

        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    /// <summary>
    /// Writes a paragraph style. Properties are split between paragraph and text properties by name: margins,
    /// padding, line height and background go on the paragraph, the rest on the text.
    /// </summary>
    private static void WriteParagraphStyle(XmlWriter writer, string id, string displayName, int? outlineLevel, params (string Name, string Value)[] properties)
    {
        writer.WriteStartElement("style", "style", style);
        writer.WriteAttributeString(name, style, id);
        writer.WriteAttributeString("display-name", style, displayName);
        writer.WriteAttributeString(family, style, "paragraph");
        if (id != "Standard")
        {
            writer.WriteAttributeString("parent-style-name", style, "Standard");
            writer.WriteAttributeString("next-style-name", style, "Standard");
        }

        if (outlineLevel is int level)
        {
            writer.WriteAttributeString("default-outline-level", style, level.ToString(CultureInfo.InvariantCulture));
        }

        var paragraph = properties.Where(p => p.Name.StartsWith("margin", StringComparison.Ordinal) || p.Name is "padding" or "line-height" or "background-color").ToArray();
        var textProperties = properties.Except(paragraph).ToArray();
        if (paragraph.Length > 0 || outlineLevel is not null)
        {
            WriteParagraphProperties(writer, paragraph, keepWithNext: outlineLevel is not null);
        }

        if (textProperties.Length > 0)
        {
            WriteTextProperties(writer, textProperties);
        }

        writer.WriteEndElement();
    }

    private static void WriteParagraphProperties(XmlWriter writer, params (string Name, string Value)[] properties) =>
        WriteParagraphProperties(writer, properties, keepWithNext: false);

    private static void WriteParagraphProperties(XmlWriter writer, (string Name, string Value)[] properties, bool keepWithNext)
    {
        writer.WriteStartElement("style", "paragraph-properties", style);
        foreach (var (property, value) in properties)
        {
            writer.WriteAttributeString(property, fo, value);
        }

        if (keepWithNext)
        {
            writer.WriteAttributeString("keep-with-next", fo, "always");
        }

        writer.WriteEndElement();
    }

    private static void WriteTextProperties(XmlWriter writer, params (string Name, string Value)[] properties)
    {
        writer.WriteStartElement("style", "text-properties", style);
        foreach (var (property, value) in properties)
        {
            var ns = property.StartsWith("text-underline", StringComparison.Ordinal) || property == "text-line-through-style" ? style : fo;
            writer.WriteAttributeString(property, ns, value);
        }

        writer.WriteEndElement();
    }

    private static void WriteListStyle(XmlWriter writer, string id, bool ordered)
    {
        string[] bullets = ["•", "◦", "▪"];
        string[] formats = ["1", "a", "i"];

        writer.WriteStartElement("text", "list-style", text);
        writer.WriteAttributeString(name, style, id);
        for (var level = 1; level <= 10; level++)
        {
            writer.WriteStartElement("text", ordered ? "list-level-style-number" : "list-level-style-bullet", text);
            writer.WriteAttributeString("level", text, level.ToString(CultureInfo.InvariantCulture));
            if (ordered)
            {
                writer.WriteAttributeString("num-suffix", style, ".");
                writer.WriteAttributeString("num-format", style, formats[(level - 1) % formats.Length]);
            }
            else
            {
                writer.WriteAttributeString("bullet-char", text, bullets[(level - 1) % bullets.Length]);
            }

            writer.WriteStartElement("style", "list-level-properties", style);
            writer.WriteAttributeString("list-level-position-and-space-mode", text, "label-alignment");
            writer.WriteStartElement("style", "list-level-label-alignment", style);
            writer.WriteAttributeString("label-followed-by", text, "listtab");
            writer.WriteAttributeString("list-tab-stop-position", text, Centimetres(level * 1.27));
            writer.WriteAttributeString("text-indent", fo, "-0.635cm");
            writer.WriteAttributeString("margin-left", fo, Centimetres(level * 1.27));
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private static string Centimetres(double value) => value.ToString("0.###", CultureInfo.InvariantCulture) + "cm";

    #endregion

    #region Content

    /// <summary>
    /// The automatic text styles for each combination of inline formats used in the document.
    /// </summary>
    private sealed class SpanStyles
    {
        private readonly Dictionary<(bool, bool, bool, bool, bool), string> _names = [];

        public IReadOnlyDictionary<(bool Bold, bool Italic, bool Underline, bool Strike, bool Code), string> Names => _names;

        public string? NameFor(RichTextRun run)
        {
            if (!run.Bold && !run.Italic && !run.Underline && !run.Strike && !run.Code)
            {
                return null;
            }

            var key = (run.Bold, run.Italic, run.Underline, run.Strike, run.Code);
            if (!_names.TryGetValue(key, out var styleId))
            {
                styleId = $"T{_names.Count + 1}";
                _names[key] = styleId;
            }

            return styleId;
        }
    }

    private static void WriteContent(XmlWriter writer, RichTextDocument document, SpanStyles spanStyles)
    {
        // The automatic styles come before the body in the file, so the span styles are collected first.
        foreach (var run in document.Blocks.SelectMany(b => b.Runs))
        {
            spanStyles.NameFor(run);
        }

        var quoteDepths = document.Blocks.Select(b => b.QuoteDepth).Where(d => d > 1).Distinct().Order().ToList();

        writer.WriteStartElement("office", "document-content", office);
        WriteNamespaces(writer);
        writer.WriteAttributeString("office", "version", office, version);

        writer.WriteStartElement("automatic-styles", office);
        foreach (var (key, styleId) in spanStyles.Names)
        {
            writer.WriteStartElement("style", "style", style);
            writer.WriteAttributeString(name, style, styleId);
            writer.WriteAttributeString(family, style, "text");
            var properties = new List<(string, string)>();
            if (key.Bold)
            {
                properties.Add(("font-weight", "bold"));
            }

            if (key.Italic)
            {
                properties.Add(("font-style", "italic"));
            }

            if (key.Underline)
            {
                properties.Add(("text-underline-style", "solid"));
                properties.Add(("text-underline-width", "auto"));
                properties.Add(("text-underline-color", "font-color"));
            }

            if (key.Strike)
            {
                properties.Add(("text-line-through-style", "solid"));
            }

            if (key.Code)
            {
                properties.Add(("font-family", "Consolas"));
                properties.Add(("font-size", "10pt"));
            }

            WriteTextProperties(writer, [.. properties]);
            writer.WriteEndElement();
        }

        // Quotes inside quotes are indented further.
        foreach (var depth in quoteDepths)
        {
            writer.WriteStartElement("style", "style", style);
            writer.WriteAttributeString(name, style, $"Quote{depth}");
            writer.WriteAttributeString(family, style, "paragraph");
            writer.WriteAttributeString("parent-style-name", style, "Quotations");
            WriteParagraphProperties(writer, ("margin-left", Centimetres(depth * 1.27)));
            writer.WriteEndElement();
        }

        writer.WriteEndElement();

        writer.WriteStartElement("body", office);
        writer.WriteStartElement("text", office);
        WriteBlocks(writer, document.Blocks, spanStyles);
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteNamespaces(XmlWriter writer)
    {
        writer.WriteAttributeString("xmlns", "style", null, style);
        writer.WriteAttributeString("xmlns", "text", null, text);
        writer.WriteAttributeString("xmlns", "fo", null, fo);
        writer.WriteAttributeString("xmlns", "xlink", null, xlink);
    }

    /// <summary>
    /// Writes the blocks, rebuilding nested lists from the list items' levels: a deeper item opens a list inside the
    /// open item, and an item of another list closes the open one.
    /// </summary>
    private static void WriteBlocks(XmlWriter writer, IReadOnlyList<RichTextBlock> blocks, SpanStyles spanStyles)
    {
        // Each open list, with whether one of its items is still open.
        var open = new List<(int ListId, bool ItemOpen)>();

        void CloseTo(int depth)
        {
            while (open.Count > depth)
            {
                if (open[^1].ItemOpen)
                {
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                open.RemoveAt(open.Count - 1);
            }
        }

        foreach (var block in blocks)
        {
            if (block.Kind != RichTextBlockKind.ListItem)
            {
                CloseTo(0);
                WriteParagraph(writer, block, spanStyles);
                continue;
            }

            var depth = block.ListLevel + 1;
            CloseTo(depth);
            if (open.Count == depth && open[^1].ListId != block.ListId)
            {
                CloseTo(depth - 1);
            }

            while (open.Count < depth)
            {
                writer.WriteStartElement("text", "list", text);
                writer.WriteAttributeString(styleName, text, block.Ordered ? numberListStyle : bulletListStyle);
                open.Add((block.ListId, false));

                if (open.Count < depth)
                {
                    // A level with no item of its own, such as a list that starts nested: give it an empty item.
                    writer.WriteStartElement("text", "list-item", text);
                    open[^1] = (open[^1].ListId, true);
                }
            }

            if (open[^1].ItemOpen)
            {
                writer.WriteEndElement();
            }

            writer.WriteStartElement("text", "list-item", text);
            open[^1] = (block.ListId, true);
            WriteParagraph(writer, block, spanStyles);
        }

        CloseTo(0);
    }

    private static void WriteParagraph(XmlWriter writer, RichTextBlock block, SpanStyles spanStyles)
    {
        var paragraphStyle = block.Kind switch
        {
            RichTextBlockKind.Heading1 => "Heading_20_1",
            RichTextBlockKind.Heading2 => "Heading_20_2",
            RichTextBlockKind.Heading3 => "Heading_20_3",
            RichTextBlockKind.Code => "Preformatted_20_Text",
            _ when block.QuoteDepth > 1 => $"Quote{block.QuoteDepth}",
            _ when block.QuoteDepth == 1 => "Quotations",
            _ => "Standard",
        };

        var heading = block.Kind is RichTextBlockKind.Heading1 or RichTextBlockKind.Heading2 or RichTextBlockKind.Heading3;
        writer.WriteStartElement("text", heading ? "h" : "p", text);
        writer.WriteAttributeString(styleName, text, paragraphStyle);
        if (heading)
        {
            writer.WriteAttributeString("outline-level", text, (block.Kind - RichTextBlockKind.Heading1 + 1).ToString(CultureInfo.InvariantCulture));
        }

        var writerState = new TextState();
        if (block.Kind == RichTextBlockKind.Code)
        {
            WriteText(writer, block.CodeText.ToString(), writerState);
        }
        else
        {
            WriteRuns(writer, block.Runs, spanStyles, writerState);
        }

        writer.WriteEndElement();
    }

    private static void WriteRuns(XmlWriter writer, List<RichTextRun> runs, SpanStyles spanStyles, TextState state)
    {
        var i = 0;
        while (i < runs.Count)
        {
            var href = runs[i].Href;
            if (href is not null)
            {
                writer.WriteStartElement("text", "a", text);
                writer.WriteAttributeString("xlink", "type", xlink, "simple");
                writer.WriteAttributeString("xlink", "href", xlink, href);
                writer.WriteAttributeString(styleName, text, "Internet_20_link");
            }

            do
            {
                var run = runs[i];
                if (run.IsBreak)
                {
                    writer.WriteElementString("text", "line-break", text, null);
                    state.AtLineStart = true;
                }
                else if (spanStyles.NameFor(run) is { } span)
                {
                    writer.WriteStartElement("text", "span", text);
                    writer.WriteAttributeString(styleName, text, span);
                    WriteText(writer, run.Text, state);
                    writer.WriteEndElement();
                }
                else
                {
                    WriteText(writer, run.Text, state);
                }

                i++;
            }
            while (i < runs.Count && href is not null && runs[i].Href == href);

            if (href is not null)
            {
                writer.WriteEndElement();
            }
        }
    }

    private sealed class TextState
    {
        public bool AtLineStart { get; set; } = true;
    }

    /// <summary>
    /// Writes text the way OpenDocument stores white space: a line break and a tab are elements, and a space at the
    /// start of a line or after another space is a <c>text:s</c> element, since plain spaces there would be collapsed.
    /// </summary>
    private static void WriteText(XmlWriter writer, string value, TextState state)
    {
        var clean = XmlCharacters.Clean(value);
        var chunk = new System.Text.StringBuilder();

        void Flush()
        {
            if (chunk.Length > 0)
            {
                writer.WriteString(chunk.ToString());
                chunk.Clear();
            }
        }

        var i = 0;
        while (i < clean.Length)
        {
            var c = clean[i];
            if (c == '\n')
            {
                Flush();
                writer.WriteElementString("text", "line-break", text, null);
                state.AtLineStart = true;
                i++;
                continue;
            }

            if (c == '\t')
            {
                Flush();
                writer.WriteElementString("text", "tab", text, null);
                state.AtLineStart = false;
                i++;
                continue;
            }

            if (c == ' ')
            {
                var count = 0;
                while (i < clean.Length && clean[i] == ' ')
                {
                    count++;
                    i++;
                }

                if (!state.AtLineStart)
                {
                    chunk.Append(' ');
                    count--;
                }

                if (count > 0)
                {
                    Flush();
                    writer.WriteStartElement("text", "s", text);
                    if (count > 1)
                    {
                        writer.WriteAttributeString("c", text, count.ToString(CultureInfo.InvariantCulture));
                    }

                    writer.WriteEndElement();
                }

                state.AtLineStart = false;
                continue;
            }

            chunk.Append(c);
            state.AtLineStart = false;
            i++;
        }

        Flush();
    }

    #endregion
}
