// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using TwBlazor.Export.Packaging;

namespace TwBlazor.Export.RichText;

/// <summary>
/// Writes a <see cref="RichTextDocument"/> as an OpenDocument text document (ODF 1.3).
/// </summary>
/// <remarks>Headings use the standard Heading 1 to 3 styles with outline levels, so they appear in the navigator and
/// a table of contents. Lists are real nested lists, each numbered list starting again from 1.</remarks>
internal static partial class OdtWriter
{
    /// <summary>The media type of an OpenDocument text document.</summary>
    public const string MediaType = "application/vnd.oasis.opendocument.text";

    private const string officeNs = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
    private const string styleNs = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";
    private const string textNs = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
    private const string foNs = "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0";
    private const string xlinkNs = "http://www.w3.org/1999/xlink";
    private const string dcNs = "http://purl.org/dc/elements/1.1/";
    private const string manifestNs = "urn:oasis:names:tc:opendocument:xmlns:manifest:1.0";
    private const string odfVersion = "1.3";
    private const string name = "name";
    private const string family = "family";
    private const string styleName = "style-name";

    private const string officePrefix = "office";
    private const string stylePrefix = "style";
    private const string textPrefix = "text";
    private const string manifestPrefix = "manifest";
    private const string xmlnsPrefix = "xmlns";
    private const string versionAttribute = "version";
    private const string marginTop = "margin-top";
    private const string marginBottom = "margin-bottom";
    private const string fontSize = "font-size";
    private const string fontWeight = "font-weight";
    private const string standardStyle = "Standard";
    private const string quotationsStyle = "Quotations";
    private const string smallSpacing = "0.21cm";

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
        writer.WriteStartElement(manifestPrefix, manifestPrefix, manifestNs);
        writer.WriteAttributeString(manifestPrefix, versionAttribute, manifestNs, odfVersion);
        WriteEntry("/", MediaType);
        WriteEntry("content.xml", "text/xml");
        WriteEntry("styles.xml", "text/xml");
        WriteEntry("meta.xml", "text/xml");
        writer.WriteEndElement();

        void WriteEntry(string path, string mediaType)
        {
            writer.WriteStartElement(manifestPrefix, "file-entry", manifestNs);
            writer.WriteAttributeString(manifestPrefix, "full-path", manifestNs, path);
            if (path == "/")
            {
                writer.WriteAttributeString(manifestPrefix, versionAttribute, manifestNs, odfVersion);
            }

            writer.WriteAttributeString(manifestPrefix, "media-type", manifestNs, mediaType);
            writer.WriteEndElement();
        }
    }

    private static void WriteMeta(XmlWriter writer, string? title)
    {
        writer.WriteStartElement(officePrefix, "document-meta", officeNs);
        writer.WriteAttributeString(xmlnsPrefix, "dc", null, dcNs);
        writer.WriteAttributeString(officePrefix, versionAttribute, officeNs, odfVersion);
        writer.WriteStartElement("meta", officeNs);
        if (!string.IsNullOrWhiteSpace(title))
        {
            writer.WriteElementString("dc", "title", dcNs, XmlCharacters.Clean(title));
        }

        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    #region Styles

    private static void WriteStyles(XmlWriter writer)
    {
        writer.WriteStartElement(officePrefix, "document-styles", officeNs);
        WriteNamespaces(writer);
        writer.WriteAttributeString(officePrefix, versionAttribute, officeNs, odfVersion);
        writer.WriteStartElement("styles", officeNs);

        writer.WriteStartElement(stylePrefix, "default-style", styleNs);
        writer.WriteAttributeString(family, styleNs, "paragraph");
        WriteParagraphProperties(writer, (marginTop, "0cm"), (marginBottom, "0.28cm"), ("line-height", "115%"));
        WriteTextProperties(writer, ("font-family", "Calibri"), (fontSize, "11pt"));
        writer.WriteEndElement();

        WriteParagraphStyle(writer, standardStyle, standardStyle, null);
        WriteParagraphStyle(writer, "Heading_20_1", "Heading 1", 1, (marginTop, "0.64cm"), (marginBottom, smallSpacing), (fontSize, "16pt"), (fontWeight, "bold"));
        WriteParagraphStyle(writer, "Heading_20_2", "Heading 2", 2, (marginTop, "0.49cm"), (marginBottom, smallSpacing), (fontSize, "14pt"), (fontWeight, "bold"));
        WriteParagraphStyle(writer, "Heading_20_3", "Heading 3", 3, (marginTop, "0.42cm"), (marginBottom, smallSpacing), (fontSize, "12pt"), (fontWeight, "bold"));
        WriteParagraphStyle(writer, quotationsStyle, quotationsStyle, null, ("margin-left", "1.27cm"), ("font-style", "italic"), ("color", "#595959"));
        WriteParagraphStyle(writer, "Preformatted_20_Text", "Preformatted Text", null, (marginTop, smallSpacing), (marginBottom, smallSpacing), ("background-color", "#f2f2f2"), ("padding", "0.1cm"), ("font-family", "Consolas"), (fontSize, "10pt"));

        writer.WriteStartElement(stylePrefix, stylePrefix, styleNs);
        writer.WriteAttributeString(name, styleNs, "Internet_20_link");
        writer.WriteAttributeString("display-name", styleNs, "Internet Link");
        writer.WriteAttributeString(family, styleNs, textPrefix);
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
        writer.WriteStartElement(stylePrefix, stylePrefix, styleNs);
        writer.WriteAttributeString(name, styleNs, id);
        writer.WriteAttributeString("display-name", styleNs, displayName);
        writer.WriteAttributeString(family, styleNs, "paragraph");
        if (id != standardStyle)
        {
            writer.WriteAttributeString("parent-style-name", styleNs, standardStyle);
            writer.WriteAttributeString("next-style-name", styleNs, standardStyle);
        }

        if (outlineLevel is int level)
        {
            writer.WriteAttributeString("default-outline-level", styleNs, level.ToString(CultureInfo.InvariantCulture));
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
        writer.WriteStartElement(stylePrefix, "paragraph-properties", styleNs);
        foreach (var (property, value) in properties)
        {
            writer.WriteAttributeString(property, foNs, value);
        }

        if (keepWithNext)
        {
            writer.WriteAttributeString("keep-with-next", foNs, "always");
        }

        writer.WriteEndElement();
    }

    private static void WriteTextProperties(XmlWriter writer, params (string Name, string Value)[] properties)
    {
        writer.WriteStartElement(stylePrefix, "text-properties", styleNs);
        foreach (var (property, value) in properties)
        {
            var ns = property.StartsWith("text-underline", StringComparison.Ordinal) || property == "text-line-through-style" ? styleNs : foNs;
            writer.WriteAttributeString(property, ns, value);
        }

        writer.WriteEndElement();
    }

    private static void WriteListStyle(XmlWriter writer, string id, bool ordered)
    {
        string[] bullets = ["•", "◦", "▪"];
        string[] formats = ["1", "a", "i"];

        writer.WriteStartElement(textPrefix, "list-style", textNs);
        writer.WriteAttributeString(name, styleNs, id);
        for (var level = 1; level <= 10; level++)
        {
            writer.WriteStartElement(textPrefix, ordered ? "list-level-style-number" : "list-level-style-bullet", textNs);
            writer.WriteAttributeString("level", textNs, level.ToString(CultureInfo.InvariantCulture));
            if (ordered)
            {
                writer.WriteAttributeString("num-suffix", styleNs, ".");
                writer.WriteAttributeString("num-format", styleNs, formats[(level - 1) % formats.Length]);
            }
            else
            {
                writer.WriteAttributeString("bullet-char", textNs, bullets[(level - 1) % bullets.Length]);
            }

            writer.WriteStartElement(stylePrefix, "list-level-properties", styleNs);
            writer.WriteAttributeString("list-level-position-and-space-mode", textNs, "label-alignment");
            writer.WriteStartElement(stylePrefix, "list-level-label-alignment", styleNs);
            writer.WriteAttributeString("label-followed-by", textNs, "listtab");
            writer.WriteAttributeString("list-tab-stop-position", textNs, Centimetres(level * 1.27));
            writer.WriteAttributeString("text-indent", foNs, "-0.635cm");
            writer.WriteAttributeString("margin-left", foNs, Centimetres(level * 1.27));
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

    /// <summary>
    /// Gets the text properties of an automatic span style for one combination of inline formats.
    /// </summary>
    private static (string Name, string Value)[] SpanProperties((bool Bold, bool Italic, bool Underline, bool Strike, bool Code) key)
    {
        var properties = new List<(string, string)>();
        if (key.Bold)
        {
            properties.Add((fontWeight, "bold"));
        }

        if (key.Italic)
        {
            properties.Add(("font-style", "italic"));
        }

        if (key.Underline)
        {
            properties.AddRange([("text-underline-style", "solid"), ("text-underline-width", "auto"), ("text-underline-color", "font-color")]);
        }

        if (key.Strike)
        {
            properties.Add(("text-line-through-style", "solid"));
        }

        if (key.Code)
        {
            properties.AddRange([("font-family", "Consolas"), (fontSize, "10pt")]);
        }

        return [.. properties];
    }

    private static void WriteContent(XmlWriter writer, RichTextDocument document, SpanStyles spanStyles)
    {
        // The automatic styles come before the body in the file, so the span styles are collected first.
        foreach (var run in document.Blocks.SelectMany(b => b.Runs))
        {
            spanStyles.NameFor(run);
        }

        var quoteDepths = document.Blocks.Select(b => b.QuoteDepth).Where(d => d > 1).Distinct().Order().ToList();

        writer.WriteStartElement(officePrefix, "document-content", officeNs);
        WriteNamespaces(writer);
        writer.WriteAttributeString(officePrefix, versionAttribute, officeNs, odfVersion);

        writer.WriteStartElement("automatic-styles", officeNs);
        foreach (var (key, styleId) in spanStyles.Names)
        {
            writer.WriteStartElement(stylePrefix, stylePrefix, styleNs);
            writer.WriteAttributeString(name, styleNs, styleId);
            writer.WriteAttributeString(family, styleNs, textPrefix);
            WriteTextProperties(writer, SpanProperties(key));
            writer.WriteEndElement();
        }

        // Quotes inside quotes are indented further.
        foreach (var depth in quoteDepths)
        {
            writer.WriteStartElement(stylePrefix, stylePrefix, styleNs);
            writer.WriteAttributeString(name, styleNs, $"Quote{depth}");
            writer.WriteAttributeString(family, styleNs, "paragraph");
            writer.WriteAttributeString("parent-style-name", styleNs, quotationsStyle);
            WriteParagraphProperties(writer, ("margin-left", Centimetres(depth * 1.27)));
            writer.WriteEndElement();
        }

        writer.WriteEndElement();

        writer.WriteStartElement("body", officeNs);
        writer.WriteStartElement(textPrefix, officeNs);
        WriteBlocks(writer, document.Blocks, spanStyles);
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteNamespaces(XmlWriter writer)
    {
        writer.WriteAttributeString(xmlnsPrefix, stylePrefix, null, styleNs);
        writer.WriteAttributeString(xmlnsPrefix, textPrefix, null, textNs);
        writer.WriteAttributeString(xmlnsPrefix, "fo", null, foNs);
        writer.WriteAttributeString(xmlnsPrefix, "xlink", null, xlinkNs);
    }

    /// <summary>
    /// Writes the blocks, rebuilding nested lists from the list items' levels: a deeper item opens a list inside the
    /// open item, and an item of another list closes the open one.
    /// </summary>
    private static void WriteBlocks(XmlWriter writer, IReadOnlyList<RichTextBlock> blocks, SpanStyles spanStyles)
    {
        var lists = new ListNesting(writer);
        foreach (var block in blocks)
        {
            if (block.Kind == RichTextBlockKind.ListItem)
            {
                lists.OpenItem(block);
            }
            else
            {
                lists.CloseTo(0);
            }

            WriteParagraph(writer, block, spanStyles);
        }

        lists.CloseTo(0);
    }

    /// <summary>
    /// Keeps track of the lists and list items left open while list items are written, so each item lands in the
    /// right list at the right depth.
    /// </summary>
    private sealed class ListNesting(XmlWriter writer)
    {
        // Each open list, with whether one of its items is still open.
        private readonly List<(int ListId, bool ItemOpen)> _open = [];

        /// <summary>
        /// Closes the open lists deeper than <paramref name="depth"/>, with their open items.
        /// </summary>
        public void CloseTo(int depth)
        {
            while (_open.Count > depth)
            {
                if (_open[^1].ItemOpen)
                {
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                _open.RemoveAt(_open.Count - 1);
            }
        }

        /// <summary>
        /// Opens a list item for <paramref name="block"/>, opening the lists it needs and closing an item or a list of
        /// another list at its depth. The item is left open, so a deeper list can go inside it.
        /// </summary>
        public void OpenItem(RichTextBlock block)
        {
            var depth = block.ListLevel + 1;
            CloseTo(depth);
            if (_open.Count == depth && _open[^1].ListId != block.ListId)
            {
                CloseTo(depth - 1);
            }

            while (_open.Count < depth)
            {
                OpenList(block, needsItem: _open.Count + 1 < depth);
            }

            if (_open[^1].ItemOpen)
            {
                writer.WriteEndElement();
            }

            writer.WriteStartElement(textPrefix, "list-item", textNs);
            _open[^1] = (block.ListId, true);
        }

        /// <summary>
        /// Opens a list. A level with no item of its own, such as a list that starts nested, gets an empty item.
        /// </summary>
        private void OpenList(RichTextBlock block, bool needsItem)
        {
            writer.WriteStartElement(textPrefix, "list", textNs);
            writer.WriteAttributeString(styleName, textNs, block.Ordered ? numberListStyle : bulletListStyle);
            _open.Add((block.ListId, needsItem));
            if (needsItem)
            {
                writer.WriteStartElement(textPrefix, "list-item", textNs);
            }
        }
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
            _ when block.QuoteDepth == 1 => quotationsStyle,
            _ => standardStyle,
        };

        var heading = block.Kind is RichTextBlockKind.Heading1 or RichTextBlockKind.Heading2 or RichTextBlockKind.Heading3;
        writer.WriteStartElement(textPrefix, heading ? "h" : "p", textNs);
        writer.WriteAttributeString(styleName, textNs, paragraphStyle);
        if (heading)
        {
            writer.WriteAttributeString("outline-level", textNs, (block.Kind - RichTextBlockKind.Heading1 + 1).ToString(CultureInfo.InvariantCulture));
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
                writer.WriteStartElement(textPrefix, "a", textNs);
                writer.WriteAttributeString("xlink", "type", xlinkNs, "simple");
                writer.WriteAttributeString("xlink", "href", xlinkNs, href);
                writer.WriteAttributeString(styleName, textNs, "Internet_20_link");
            }

            do
            {
                var run = runs[i];
                if (run.IsBreak)
                {
                    writer.WriteElementString(textPrefix, "line-break", textNs, null);
                    state.AtLineStart = true;
                }
                else if (spanStyles.NameFor(run) is { } span)
                {
                    writer.WriteStartElement(textPrefix, "span", textNs);
                    writer.WriteAttributeString(styleName, textNs, span);
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
        foreach (var token in WhiteSpaceRuns().Split(XmlCharacters.Clean(value)))
        {
            if (token.Length == 0)
            {
                continue;
            }

            switch (token[0])
            {
                case '\n':
                    writer.WriteElementString(textPrefix, "line-break", textNs, null);
                    state.AtLineStart = true;
                    break;
                case '\t':
                    writer.WriteElementString(textPrefix, "tab", textNs, null);
                    state.AtLineStart = false;
                    break;
                case ' ':
                    WriteSpaces(writer, token.Length, state);
                    break;
                default:
                    writer.WriteString(token);
                    state.AtLineStart = false;
                    break;
            }
        }
    }

    /// <summary>
    /// Writes a run of spaces: one plain space, unless the run starts a line, and a <c>text:s</c> element for the rest.
    /// </summary>
    private static void WriteSpaces(XmlWriter writer, int count, TextState state)
    {
        if (!state.AtLineStart)
        {
            writer.WriteString(" ");
            count--;
        }

        state.AtLineStart = false;
        if (count == 0)
        {
            return;
        }

        writer.WriteStartElement(textPrefix, "s", textNs);
        if (count > 1)
        {
            writer.WriteAttributeString("c", textNs, count.ToString(CultureInfo.InvariantCulture));
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Splits text into runs of spaces, line breaks and tabs, keeping them, and the text between them.
    /// </summary>
    [GeneratedRegex("( +|\n|\t)")]
    private static partial Regex WhiteSpaceRuns();

    #endregion
}
