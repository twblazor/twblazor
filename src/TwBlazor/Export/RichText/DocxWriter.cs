// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.Xml;
using TwBlazor.Export.Packaging;

namespace TwBlazor.Export.RichText;

/// <summary>
/// Writes a <see cref="RichTextDocument"/> as a Word document (Office Open XML WordprocessingML).
/// </summary>
/// <remarks>Headings use Word's built-in Heading 1 to 3 styles, so they appear in the navigation pane and a table of
/// contents. Lists are real Word lists, each numbered list starting again from 1, and links are real hyperlinks.</remarks>
internal static class DocxWriter
{
    private const string w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
    private const string r = OpenXmlParts.RelationshipsNamespace;
    private const string val = "val";

    private const int bulletNumberingId = 1;
    private const int bulletAbstractId = 0;
    private const int numberAbstractId = 1;

    // Measurements in twentieths of a point (twips).
    private const int levelIndent = 720;
    private const int hangingIndent = 360;
    private const int quoteIndent = 720;

    private static readonly string[] _bullets = ["•", "◦", "▪"];

    /// <summary>
    /// Writes the document.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="title">The document's title, stored in its properties, or <see langword="null"/> for none.</param>
    /// <returns>The contents of a <c>.docx</c> file.</returns>
    public static byte[] Write(RichTextDocument document, string? title)
    {
        // Each numbered list gets its own numbering instance so it starts again from 1; bulleted lists share one.
        var numberingIds = new Dictionary<int, int>();
        foreach (var block in document.Blocks.Where(b => b is { Kind: RichTextBlockKind.ListItem, Ordered: true }))
        {
            numberingIds.TryAdd(block.ListId, bulletNumberingId + 1 + numberingIds.Count);
        }

        var links = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var href in document.Blocks.SelectMany(b => b.Runs).Select(run => run.Href).OfType<string>())
        {
            links.TryAdd(href, $"rId{4 + links.Count}");
        }

        using var package = new ZipPackage();
        package.AddXml("[Content_Types].xml", writer => OpenXmlParts.WriteContentTypes(writer,
            ("/word/document.xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"),
            ("/word/styles.xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml"),
            ("/word/numbering.xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.numbering+xml"),
            ("/docProps/core.xml", "application/vnd.openxmlformats-package.core-properties+xml")));
        package.AddXml("_rels/.rels", writer => OpenXmlParts.WriteRelationships(writer,
        [
            new("rId1", OpenXmlParts.OfficeDocumentRelationship, "word/document.xml"),
            new("rId2", OpenXmlParts.CorePropertiesRelationship, "docProps/core.xml"),
        ]));
        package.AddXml("docProps/core.xml", writer => OpenXmlParts.WriteCoreProperties(writer, title));
        package.AddXml("word/_rels/document.xml.rels", writer => OpenXmlParts.WriteRelationships(writer,
        [
            new("rId1", OpenXmlParts.StylesRelationship, "styles.xml"),
            new("rId2", OpenXmlParts.NumberingRelationship, "numbering.xml"),
            // Word refuses a link address with a space in it, so spaces are percent-encoded.
            .. links.Select(link => new OpenXmlRelationship(link.Value, OpenXmlParts.HyperlinkRelationship, link.Key.Replace(" ", "%20", StringComparison.Ordinal), External: true)),
        ]));
        package.AddXml("word/styles.xml", WriteStyles);
        package.AddXml("word/numbering.xml", writer => WriteNumbering(writer, numberingIds.Values));
        package.AddXml("word/document.xml", writer => WriteDocument(writer, document, numberingIds, links));
        return package.ToArray();
    }

    private static void WriteDocument(XmlWriter writer, RichTextDocument document, Dictionary<int, int> numberingIds, Dictionary<string, string> links)
    {
        writer.WriteStartElement("w", "document", w);
        writer.WriteAttributeString("xmlns", "r", null, r);
        writer.WriteStartElement("body", w);

        foreach (var block in document.Blocks)
        {
            writer.WriteStartElement("p", w);
            WriteParagraphProperties(writer, block, numberingIds);

            if (block.Kind == RichTextBlockKind.Code)
            {
                var lines = block.CodeText.ToString().Split('\n');
                for (var i = 0; i < lines.Length; i++)
                {
                    if (i > 0)
                    {
                        WriteBreak(writer);
                    }

                    WriteRun(writer, new RichTextRun { Text = lines[i] });
                }
            }
            else
            {
                WriteRuns(writer, block.Runs, links);
            }

            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteParagraphProperties(XmlWriter writer, RichTextBlock block, Dictionary<int, int> numberingIds)
    {
        var style = block.Kind switch
        {
            RichTextBlockKind.Heading1 => "Heading1",
            RichTextBlockKind.Heading2 => "Heading2",
            RichTextBlockKind.Heading3 => "Heading3",
            RichTextBlockKind.Code => "Code",
            RichTextBlockKind.ListItem => "ListParagraph",
            _ when block.QuoteDepth > 0 => "Quote",
            _ => null,
        };

        writer.WriteStartElement("pPr", w);
        if (style is not null)
        {
            WriteValue(writer, "pStyle", style);
        }

        if (block.Kind == RichTextBlockKind.ListItem)
        {
            writer.WriteStartElement("numPr", w);
            WriteValue(writer, "ilvl", Math.Min(block.ListLevel, 8).ToString(CultureInfo.InvariantCulture));
            WriteValue(writer, "numId", (block.Ordered ? numberingIds[block.ListId] : bulletNumberingId).ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }

        if (block.QuoteDepth > 0)
        {
            var left = block.QuoteDepth * quoteIndent + (block.Kind == RichTextBlockKind.ListItem ? (Math.Min(block.ListLevel, 8) + 1) * levelIndent : 0);
            writer.WriteStartElement("ind", w);
            writer.WriteAttributeString("left", w, left.ToString(CultureInfo.InvariantCulture));
            if (block.Kind == RichTextBlockKind.ListItem)
            {
                writer.WriteAttributeString("hanging", w, hangingIndent.ToString(CultureInfo.InvariantCulture));
            }

            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private static void WriteRuns(XmlWriter writer, List<RichTextRun> runs, Dictionary<string, string> links)
    {
        var i = 0;
        while (i < runs.Count)
        {
            var href = runs[i].Href;
            if (href is null)
            {
                WriteRunOrBreak(writer, runs[i]);
                i++;
                continue;
            }

            writer.WriteStartElement("hyperlink", w);
            writer.WriteAttributeString("id", r, links[href]);
            while (i < runs.Count && runs[i].Href == href)
            {
                WriteRunOrBreak(writer, runs[i]);
                i++;
            }

            writer.WriteEndElement();
        }
    }

    private static void WriteRunOrBreak(XmlWriter writer, RichTextRun run)
    {
        if (run.IsBreak)
        {
            WriteBreak(writer);
            return;
        }

        WriteRun(writer, run);
    }

    private static void WriteBreak(XmlWriter writer)
    {
        writer.WriteStartElement("r", w);
        writer.WriteElementString("br", w, null);
        writer.WriteEndElement();
    }

    private static void WriteRun(XmlWriter writer, RichTextRun run)
    {
        writer.WriteStartElement("r", w);

        // The run properties must be written in the order the schema lists them.
        if (run.Href is not null || run.Code || run.Bold || run.Italic || run.Strike || run.Underline)
        {
            writer.WriteStartElement("rPr", w);
            if (run.Href is not null)
            {
                WriteValue(writer, "rStyle", "Hyperlink");
            }
            else if (run.Code)
            {
                WriteValue(writer, "rStyle", "CodeChar");
            }

            if (run.Bold)
            {
                writer.WriteElementString("b", w, null);
            }

            if (run.Italic)
            {
                writer.WriteElementString("i", w, null);
            }

            if (run.Strike)
            {
                writer.WriteElementString("strike", w, null);
            }

            if (run.Underline)
            {
                WriteValue(writer, "u", "single");
            }

            writer.WriteEndElement();
        }

        writer.WriteStartElement("t", w);
        writer.WriteAttributeString("xml", "space", null, "preserve");
        writer.WriteString(XmlCharacters.Clean(run.Text));
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteValue(XmlWriter writer, string element, string value)
    {
        writer.WriteStartElement(element, w);
        writer.WriteAttributeString(val, w, value);
        writer.WriteEndElement();
    }

    #region Styles

    private static void WriteStyles(XmlWriter writer)
    {
        writer.WriteStartElement("w", "styles", w);

        writer.WriteStartElement("docDefaults", w);
        writer.WriteStartElement("rPrDefault", w);
        writer.WriteStartElement("rPr", w);
        WriteFonts(writer, "Calibri");
        WriteValue(writer, "sz", "22");
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteStartElement("pPrDefault", w);
        writer.WriteStartElement("pPr", w);
        WriteSpacing(writer, before: 0, after: 160, line: 276);
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();

        WriteParagraphStyle(writer, "Normal", "Normal", isDefault: true);
        WriteHeadingStyle(writer, 1, size: 32, before: 360);
        WriteHeadingStyle(writer, 2, size: 28, before: 280);
        WriteHeadingStyle(writer, 3, size: 24, before: 240);

        WriteParagraphStyle(writer, "Quote", "Quote", paragraph: () =>
        {
            writer.WriteStartElement("ind", w);
            writer.WriteAttributeString("left", w, quoteIndent.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }, run: () =>
        {
            writer.WriteElementString("i", w, null);
            WriteValue(writer, "color", "595959");
        });

        WriteParagraphStyle(writer, "ListParagraph", "List Paragraph", paragraph: () => writer.WriteElementString("contextualSpacing", w, null));

        WriteParagraphStyle(writer, "Code", "Code", paragraph: () =>
        {
            writer.WriteStartElement("shd", w);
            writer.WriteAttributeString(val, w, "clear");
            writer.WriteAttributeString("color", w, "auto");
            writer.WriteAttributeString("fill", w, "F2F2F2");
            writer.WriteEndElement();
            WriteSpacing(writer, before: 120, after: 120, line: 240);
        }, run: () =>
        {
            WriteFonts(writer, "Consolas");
            WriteValue(writer, "sz", "20");
        });

        WriteCharacterStyle(writer, "Hyperlink", "Hyperlink", () =>
        {
            WriteValue(writer, "color", "0563C1");
            WriteValue(writer, "u", "single");
        });

        WriteCharacterStyle(writer, "CodeChar", "Code Char", () =>
        {
            WriteFonts(writer, "Consolas");
            WriteValue(writer, "sz", "20");
        });

        writer.WriteEndElement();
    }

    private static void WriteHeadingStyle(XmlWriter writer, int level, int size, int before)
    {
        WriteParagraphStyle(writer, $"Heading{level}", $"heading {level}", paragraph: () =>
        {
            writer.WriteElementString("keepNext", w, null);
            WriteSpacing(writer, before, after: 120, line: 240);
            WriteValue(writer, "outlineLvl", (level - 1).ToString(CultureInfo.InvariantCulture));
        }, run: () =>
        {
            writer.WriteElementString("b", w, null);
            WriteValue(writer, "sz", size.ToString(CultureInfo.InvariantCulture));
        });
    }

    private static void WriteParagraphStyle(XmlWriter writer, string id, string name, bool isDefault = false, Action? paragraph = null, Action? run = null)
    {
        writer.WriteStartElement("style", w);
        writer.WriteAttributeString("type", w, "paragraph");
        if (isDefault)
        {
            writer.WriteAttributeString("default", w, "1");
        }

        writer.WriteAttributeString("styleId", w, id);
        WriteValue(writer, "name", name);
        if (!isDefault)
        {
            WriteValue(writer, "basedOn", "Normal");
            WriteValue(writer, "next", "Normal");
            writer.WriteElementString("qFormat", w, null);
        }

        if (paragraph is not null)
        {
            writer.WriteStartElement("pPr", w);
            paragraph();
            writer.WriteEndElement();
        }

        if (run is not null)
        {
            writer.WriteStartElement("rPr", w);
            run();
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private static void WriteCharacterStyle(XmlWriter writer, string id, string name, Action run)
    {
        writer.WriteStartElement("style", w);
        writer.WriteAttributeString("type", w, "character");
        writer.WriteAttributeString("styleId", w, id);
        WriteValue(writer, "name", name);
        writer.WriteStartElement("rPr", w);
        run();
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteFonts(XmlWriter writer, string font)
    {
        writer.WriteStartElement("rFonts", w);
        writer.WriteAttributeString("ascii", w, font);
        writer.WriteAttributeString("hAnsi", w, font);
        writer.WriteAttributeString("cs", w, font);
        writer.WriteEndElement();
    }

    private static void WriteSpacing(XmlWriter writer, int before, int after, int line)
    {
        writer.WriteStartElement("spacing", w);
        writer.WriteAttributeString("before", w, before.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("after", w, after.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("line", w, line.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("lineRule", w, "auto");
        writer.WriteEndElement();
    }

    #endregion

    #region Numbering

    private static void WriteNumbering(XmlWriter writer, IEnumerable<int> orderedNumberingIds)
    {
        writer.WriteStartElement("w", "numbering", w);
        WriteAbstractNumbering(writer, bulletAbstractId, ordered: false);
        WriteAbstractNumbering(writer, numberAbstractId, ordered: true);

        WriteNumberingInstance(writer, bulletNumberingId, bulletAbstractId, restart: false);
        foreach (var id in orderedNumberingIds)
        {
            WriteNumberingInstance(writer, id, numberAbstractId, restart: true);
        }

        writer.WriteEndElement();
    }

    private static void WriteAbstractNumbering(XmlWriter writer, int id, bool ordered)
    {
        writer.WriteStartElement("abstractNum", w);
        writer.WriteAttributeString("abstractNumId", w, id.ToString(CultureInfo.InvariantCulture));
        WriteValue(writer, "multiLevelType", "hybridMultilevel");

        for (var level = 0; level < 9; level++)
        {
            writer.WriteStartElement("lvl", w);
            writer.WriteAttributeString("ilvl", w, level.ToString(CultureInfo.InvariantCulture));
            WriteValue(writer, "start", "1");
            WriteValue(writer, "numFmt", ordered ? (level % 3) switch { 1 => "lowerLetter", 2 => "lowerRoman", _ => "decimal" } : "bullet");
            WriteValue(writer, "lvlText", ordered ? $"%{level + 1}." : _bullets[level % _bullets.Length]);
            WriteValue(writer, "lvlJc", "left");
            writer.WriteStartElement("pPr", w);
            writer.WriteStartElement("ind", w);
            writer.WriteAttributeString("left", w, ((level + 1) * levelIndent).ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("hanging", w, hangingIndent.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private static void WriteNumberingInstance(XmlWriter writer, int id, int abstractId, bool restart)
    {
        writer.WriteStartElement("num", w);
        writer.WriteAttributeString("numId", w, id.ToString(CultureInfo.InvariantCulture));
        WriteValue(writer, "abstractNumId", abstractId.ToString(CultureInfo.InvariantCulture));
        if (restart)
        {
            for (var level = 0; level < 9; level++)
            {
                writer.WriteStartElement("lvlOverride", w);
                writer.WriteAttributeString("ilvl", w, level.ToString(CultureInfo.InvariantCulture));
                WriteValue(writer, "startOverride", "1");
                writer.WriteEndElement();
            }
        }

        writer.WriteEndElement();
    }

    #endregion
}
