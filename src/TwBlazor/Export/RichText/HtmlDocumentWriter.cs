// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Net;

namespace TwBlazor.Export.RichText;

/// <summary>
/// Writes a <see cref="RichTextDocument"/> as a web page that opens on its own in a browser: the clean HTML in a full
/// document with a title, the language and simple readable styles that follow the reader's light or dark setting.
/// </summary>
internal static class HtmlDocumentWriter
{
    private const string styles = """
        :root { color-scheme: light dark; }
        body { max-width: 42rem; margin: 3rem auto; padding: 0 1.25rem; font: 1rem/1.65 system-ui, -apple-system, "Segoe UI", sans-serif; }
        h1, h2, h3 { line-height: 1.25; margin: 1.6em 0 0.5em; }
        h1 { font-size: 1.75rem; } h2 { font-size: 1.4rem; } h3 { font-size: 1.15rem; }
        blockquote { margin: 1em 0; padding-left: 1em; border-left: 3px solid color-mix(in srgb, currentColor 30%, transparent); font-style: italic; }
        pre { padding: 0.75rem 1rem; border-radius: 0.5rem; overflow-x: auto; background: color-mix(in srgb, currentColor 6%, transparent); }
        code { font-family: ui-monospace, "Cascadia Code", Consolas, monospace; font-size: 0.9em; }
        a { text-underline-offset: 3px; }
        """;

    /// <summary>
    /// Writes the document.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="title">The page's title.</param>
    /// <param name="language">The language of the text, such as <c>en-GB</c>, for screen readers and spell checkers.</param>
    /// <returns>The web page.</returns>
    public static string Write(RichTextDocument document, string title, string language)
    {
        var encodedTitle = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(title) ? "Document" : title);
        var encodedLanguage = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(language) ? "en" : language);
        return $"""
            <!DOCTYPE html>
            <html lang="{encodedLanguage}">
            <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>{encodedTitle}</title>
            <style>
            {styles}
            </style>
            </head>
            <body>
            {document.ToCleanHtml()}
            </body>
            </html>

            """;
    }
}
