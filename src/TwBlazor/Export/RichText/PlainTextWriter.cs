// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Text;

namespace TwBlazor.Export.RichText;

/// <summary>
/// Writes a <see cref="RichTextDocument"/> as plain text: a blank line between paragraphs, a bullet or number before
/// each list item, <c>&gt;</c> before quoted lines, and a link's address after its text.
/// </summary>
internal static class PlainTextWriter
{
    /// <summary>
    /// Writes the document.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="newLine">The line break to use. Defaults to <c>\n</c>.</param>
    /// <returns>The text, ending with a line break, or an empty string for an empty document.</returns>
    public static string Write(RichTextDocument document, string newLine = "\n")
    {
        var text = new StringBuilder();
        RichTextBlock? previous = null;
        RichTextBlock? topItem = null;

        foreach (var block in document.Blocks)
        {
            if (previous is not null)
            {
                text.Append(BlockSpacing.Between(previous, block, topItem) == BlockGap.LineBreak ? "\n" : "\n\n");
            }

            topItem = BlockSpacing.TopItemAfter(block, topItem);
            WriteBlock(text, block);
            previous = block;
        }

        if (text.Length == 0)
        {
            return string.Empty;
        }

        text.Append('\n');
        return newLine == "\n" ? text.ToString() : text.Replace("\n", newLine).ToString();
    }

    /// <summary>
    /// Writes one block, with its quote markers and list marker, keeping later lines lined up under the first.
    /// </summary>
    private static void WriteBlock(StringBuilder text, RichTextBlock block)
    {
        var quote = string.Concat(Enumerable.Repeat("> ", block.QuoteDepth));
        var (marker, continuation) = block.Kind == RichTextBlockKind.ListItem
            ? ListMarker(block)
            : (string.Empty, string.Empty);
        var body = block.Kind == RichTextBlockKind.Code ? block.CodeText.ToString() : Inline(block.Runs);

        var lines = body.Split('\n');
        text.Append(quote).Append(marker).Append(lines[0]);
        for (var i = 1; i < lines.Length; i++)
        {
            text.Append('\n').Append(quote).Append(continuation).Append(lines[i]);
        }
    }

    private static (string Marker, string Continuation) ListMarker(RichTextBlock block)
    {
        var indent = new string(' ', block.ListLevel * 2);
        var marker = block.Ordered ? $"{block.Number}. " : "• ";
        return (indent + marker, indent + new string(' ', marker.Length));
    }

    /// <summary>
    /// Joins the runs' text, with a line break for each break and a link's address after its text when they differ.
    /// </summary>
    internal static string Inline(List<RichTextRun> runs)
    {
        var text = new StringBuilder();
        var i = 0;
        while (i < runs.Count)
        {
            // A link may be several runs, such as partly bold text, so its address follows the last of them.
            var href = runs[i].Href;
            var end = i + 1;
            while (href is not null && end < runs.Count && runs[end].Href == href)
            {
                end++;
            }

            var span = string.Concat(runs.Skip(i).Take(end - i).Select(run => run.IsBreak ? "\n" : run.Text));
            text.Append(span);
            if (href is not null && !ShowsItsAddress(span, href))
            {
                text.Append(" (").Append(href).Append(')');
            }

            i = end;
        }

        return text.ToString();
    }

    /// <summary>
    /// Gets whether a link's text is already its address, such as an email address linked with <c>mailto:</c>.
    /// </summary>
    private static bool ShowsItsAddress(string linkText, string href)
    {
        var shown = linkText.Trim();
        var address = href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ? href[7..] : href;
        return string.Equals(shown, address, StringComparison.OrdinalIgnoreCase)
            || string.Equals(shown, href, StringComparison.OrdinalIgnoreCase);
    }
}
