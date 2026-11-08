// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Text;
using TwBlazor.Enums;
using TwBlazor.Export.RichText;
using TwBlazor.Models;

namespace TwBlazor.Components;

/// <summary>
/// Converts the HTML a <see cref="TwTextEditor"/> produces into Markdown, Word, OpenDocument, HTML, plain text and Rich
/// Text Format files. <see cref="TwTextEditor"/> uses it for its export menu, and it can be called on its own, for
/// example to build a file on the server from stored HTML.
/// </summary>
/// <remarks>
/// Every converter reads the HTML with the same rules as the editor: only its tags are kept (paragraphs, three heading
/// levels, bold, italic, underline, strikethrough, inline code, links, bulleted and numbered lists, quotes and code
/// blocks), other tags are unwrapped, and scripts, styles and unsafe links are dropped. So HTML from anywhere can be
/// converted safely. The converters use only the .NET libraries; no other package is needed.
/// </remarks>
public static class TwTextEditorExport
{
    /// <summary>The media type of a Markdown file.</summary>
    public const string MarkdownContentType = "text/markdown";

    /// <summary>The media type of a Word document.</summary>
    public const string DocxContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    /// <summary>The media type of an OpenDocument text document.</summary>
    public const string OdtContentType = OdtWriter.MediaType;

    /// <summary>The media type of a web page.</summary>
    public const string HtmlContentType = "text/html";

    /// <summary>The media type of a plain text file.</summary>
    public const string PlainTextContentType = "text/plain";

    /// <summary>The media type of a Rich Text Format file.</summary>
    public const string RtfContentType = "application/rtf";

    /// <summary>
    /// Converts HTML to Markdown, in the CommonMark syntax with GitHub's <c>~~strikethrough~~</c>. Underlined text is
    /// kept as <c>&lt;u&gt;</c> tags, since Markdown has no underline.
    /// </summary>
    /// <param name="html">The editor's HTML.</param>
    /// <returns>The Markdown, or an empty string when there is no text.</returns>
    public static string ToMarkdown(string? html) => MarkdownWriter.Write(RichTextDocument.Parse(html));

    /// <summary>
    /// Converts HTML to plain text, with a blank line between paragraphs, a bullet or number before each list item,
    /// <c>&gt;</c> before quoted lines, and each link's address after its text.
    /// </summary>
    /// <param name="html">The editor's HTML.</param>
    /// <returns>The text, or an empty string when there is no text.</returns>
    public static string ToPlainText(string? html) => PlainTextWriter.Write(RichTextDocument.Parse(html));

    /// <summary>
    /// Converts HTML to a web page that opens on its own in a browser, with simple readable styles that follow the
    /// reader's light or dark setting.
    /// </summary>
    /// <param name="html">The editor's HTML.</param>
    /// <param name="title">The page's title.</param>
    /// <param name="language">The language of the text, such as <c>en-GB</c>. Defaults to <c>en</c>.</param>
    /// <returns>The web page.</returns>
    public static string ToHtmlDocument(string? html, string title = "Document", string language = "en") =>
        HtmlDocumentWriter.Write(RichTextDocument.Parse(html), title, language);

    /// <summary>
    /// Converts HTML to Rich Text Format, which almost every word processor opens.
    /// </summary>
    /// <param name="html">The editor's HTML.</param>
    /// <param name="title">The document's title, stored in its properties, or <see langword="null"/> for none.</param>
    /// <returns>The RTF.</returns>
    public static string ToRtf(string? html, string? title = null) => RtfWriter.Write(RichTextDocument.Parse(html), title);

    /// <summary>
    /// Converts HTML to a Word document. Headings use Word's Heading 1 to 3 styles, lists are real Word lists and
    /// links are real hyperlinks.
    /// </summary>
    /// <param name="html">The editor's HTML.</param>
    /// <param name="title">The document's title, stored in its properties, or <see langword="null"/> for none.</param>
    /// <returns>The contents of a <c>.docx</c> file.</returns>
    public static byte[] ToDocx(string? html, string? title = null) => DocxWriter.Write(RichTextDocument.Parse(html), title);

    /// <summary>
    /// Converts HTML to an OpenDocument text document (ODF 1.3), the format of LibreOffice. Headings use the standard
    /// heading styles, and lists are real nested lists.
    /// </summary>
    /// <param name="html">The editor's HTML.</param>
    /// <param name="title">The document's title, stored in its properties, or <see langword="null"/> for none.</param>
    /// <returns>The contents of an <c>.odt</c> file.</returns>
    public static byte[] ToOdt(string? html, string? title = null) => OdtWriter.Write(RichTextDocument.Parse(html), title);

    /// <summary>
    /// Converts HTML to a file in <paramref name="format"/>, with the file name, extension and media type to save it
    /// with. Text formats are UTF-8, with a byte order mark for plain text so Windows Notepad shows any language.
    /// </summary>
    /// <param name="html">The editor's HTML.</param>
    /// <param name="format">The format.</param>
    /// <param name="fileName">The file name without its extension, also used as the document's title. Characters a
    /// file name cannot hold are replaced. Defaults to <c>document</c>.</param>
    /// <returns>The file.</returns>
    public static ExportFile Export(string? html, TextEditorExportFormat format, string fileName = "document")
    {
        var title = string.IsNullOrWhiteSpace(fileName) ? "document" : fileName.Trim();
        var safeName = SafeFileName(title);
        var utf8 = new UTF8Encoding(false);

        return format switch
        {
            TextEditorExportFormat.Markdown => new ExportFile { FileName = $"{safeName}.md", ContentType = MarkdownContentType, Content = utf8.GetBytes(ToMarkdown(html)) },
            TextEditorExportFormat.Docx => new ExportFile { FileName = $"{safeName}.docx", ContentType = DocxContentType, Content = ToDocx(html, title) },
            TextEditorExportFormat.Odt => new ExportFile { FileName = $"{safeName}.odt", ContentType = OdtContentType, Content = ToOdt(html, title) },
            TextEditorExportFormat.Html => new ExportFile { FileName = $"{safeName}.html", ContentType = HtmlContentType, Content = utf8.GetBytes(ToHtmlDocument(html, title)) },
            TextEditorExportFormat.PlainText => new ExportFile { FileName = $"{safeName}.txt", ContentType = PlainTextContentType, Content = [.. Encoding.UTF8.GetPreamble(), .. utf8.GetBytes(PlainTextWriter.Write(RichTextDocument.Parse(html), "\r\n"))] },
            TextEditorExportFormat.Rtf => new ExportFile { FileName = $"{safeName}.rtf", ContentType = RtfContentType, Content = Encoding.ASCII.GetBytes(ToRtf(html, title)) },
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };
    }

    /// <summary>
    /// Gets the extension a format's files are saved with, such as <c>.docx</c>.
    /// </summary>
    /// <param name="format">The format.</param>
    /// <returns>The extension, with its dot.</returns>
    public static string ExtensionOf(TextEditorExportFormat format) => format switch
    {
        TextEditorExportFormat.Markdown => ".md",
        TextEditorExportFormat.Docx => ".docx",
        TextEditorExportFormat.Odt => ".odt",
        TextEditorExportFormat.Html => ".html",
        TextEditorExportFormat.PlainText => ".txt",
        TextEditorExportFormat.Rtf => ".rtf",
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
    };

    /// <summary>
    /// Replaces the characters no common file system allows in a file name.
    /// </summary>
    private static string SafeFileName(string name)
    {
        var safe = new StringBuilder(name.Length);
        foreach (var c in name)
        {
            safe.Append(c < ' ' || "<>:\"/\\|?*".Contains(c) ? '_' : c);
        }

        var result = safe.ToString().Trim().TrimEnd('.');
        return result.Length == 0 ? "document" : result;
    }
}
