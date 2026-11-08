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
        var topListId = 0;

        foreach (var block in document.Blocks)
        {
            if (block is { Kind: RichTextBlockKind.ListItem, ListLevel: 0 } && previous is not null && block.ListId != topListId)
            {
                // A new list starts after a blank line, like any other block.
                previous = new RichTextBlock { Kind = RichTextBlockKind.Paragraph };
            }

            if (block is { Kind: RichTextBlockKind.ListItem, ListLevel: 0 })
            {
                topListId = block.ListId;
            }

            if (previous is not null)
            {
                var sameList = previous.Kind == RichTextBlockKind.ListItem && block.Kind == RichTextBlockKind.ListItem;
                text.Append('\n');
                if (!sameList)
                {
                    text.Append('\n');
                }
            }

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

            previous = block;
        }

        if (text.Length == 0)
        {
            return string.Empty;
        }

        text.Append('\n');
        return newLine == "\n" ? text.ToString() : text.Replace("\n", newLine).ToString();
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
        for (var i = 0; i < runs.Count; i++)
        {
            var run = runs[i];
            if (run.IsBreak)
            {
                text.Append('\n');
                continue;
            }

            text.Append(run.Text);

            var linkEnds = run.Href is not null && (i + 1 == runs.Count || runs[i + 1].Href != run.Href);
            if (linkEnds)
            {
                var start = i;
                while (start > 0 && runs[start - 1].Href == run.Href)
                {
                    start--;
                }

                var linkText = string.Concat(runs.Skip(start).Take(i - start + 1).Select(r => r.Text));
                var address = run.Href!.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ? run.Href[7..] : run.Href;
                if (!string.Equals(linkText.Trim(), address, StringComparison.OrdinalIgnoreCase) && !string.Equals(linkText.Trim(), run.Href, StringComparison.OrdinalIgnoreCase))
                {
                    text.Append(" (").Append(run.Href).Append(')');
                }
            }
        }

        return text.ToString();
    }
}
