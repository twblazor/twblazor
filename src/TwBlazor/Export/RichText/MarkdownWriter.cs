// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Text;
using System.Text.RegularExpressions;

namespace TwBlazor.Export.RichText;

/// <summary>
/// Writes a <see cref="RichTextDocument"/> as Markdown: CommonMark, with GitHub's <c>~~strikethrough~~</c>.
/// </summary>
/// <remarks>Markdown has no underline, so underlined text is kept as <c>&lt;u&gt;</c> tags, which CommonMark allows.
/// Characters Markdown would read as formatting are escaped with a backslash.</remarks>
internal static partial class MarkdownWriter
{
    private const string specialCharacters = "\\`*_[]<>~|";

    /// <summary>
    /// Writes the document.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <returns>The Markdown, ending with a line break, or an empty string for an empty document.</returns>
    public static string Write(RichTextDocument document)
    {
        var markdown = new StringBuilder();
        RichTextBlock? previous = null;
        RichTextBlock? topItem = null;

        foreach (var block in document.Blocks)
        {
            if (previous is not null)
            {
                AppendGap(markdown, previous, block, BlockSpacing.Between(previous, block, topItem));
            }

            topItem = BlockSpacing.TopItemAfter(block, topItem);
            WriteBlock(markdown, block);
            previous = block;
        }

        return markdown.Length == 0 ? string.Empty : markdown.Append('\n').ToString();
    }

    /// <summary>
    /// Writes the space between two blocks. A blank line inside a quote keeps the quote's marker, so the quote
    /// continues across it.
    /// </summary>
    private static void AppendGap(StringBuilder markdown, RichTextBlock previous, RichTextBlock block, BlockGap gap)
    {
        markdown.Append('\n');
        if (gap == BlockGap.LineBreak)
        {
            return;
        }

        var quote = QuotePrefix(Math.Min(previous.QuoteDepth, block.QuoteDepth)).TrimEnd();
        markdown.Append(quote).Append('\n');

        // Markdown joins two lists of the same kind that follow each other, so a comment keeps them apart.
        if (gap == BlockGap.ListBreak)
        {
            markdown.Append(QuotePrefix(block.QuoteDepth)).Append("<!-- -->\n").Append(quote).Append('\n');
        }
    }

    private static string QuotePrefix(int depth) => string.Concat(Enumerable.Repeat("> ", depth));

    private static void WriteBlock(StringBuilder markdown, RichTextBlock block)
    {
        var quote = QuotePrefix(block.QuoteDepth);

        if (block.Kind == RichTextBlockKind.Code)
        {
            var code = block.CodeText.ToString();
            var fence = new string('`', Math.Max(3, LongestRun(code, '`') + 1));
            markdown.Append(quote).Append(fence).Append('\n');
            foreach (var line in code.Split('\n'))
            {
                markdown.Append(quote).Append(line).Append('\n');
            }

            markdown.Append(quote).Append(fence);
            return;
        }

        var (marker, continuation) = block.Kind switch
        {
            RichTextBlockKind.Heading1 => ("# ", string.Empty),
            RichTextBlockKind.Heading2 => ("## ", string.Empty),
            RichTextBlockKind.Heading3 => ("### ", string.Empty),
            RichTextBlockKind.ListItem => ListMarker(block),
            _ => (string.Empty, string.Empty),
        };

        var text = Inline(block.Runs);
        if (block.Kind is RichTextBlockKind.Paragraph or RichTextBlockKind.ListItem)
        {
            text = EscapeLineStart(text);
        }

        var lines = text.Split('\n');
        markdown.Append(quote).Append(marker).Append(lines[0]);
        for (var i = 1; i < lines.Length; i++)
        {
            markdown.Append('\n').Append(quote).Append(continuation).Append(lines[i]);
        }
    }

    /// <summary>
    /// The marker of a list item, indented four spaces per level so nested lists stay nested, and the indent its
    /// following lines need to stay in the item.
    /// </summary>
    private static (string Marker, string Continuation) ListMarker(RichTextBlock block)
    {
        var indent = new string(' ', block.ListLevel * 4);
        var marker = block.Ordered ? $"{block.Number}. " : "- ";
        return (indent + marker, indent + new string(' ', marker.Length));
    }

    /// <summary>
    /// Writes the runs, opening and closing the formatting markers only where the formats change, so adjacent runs
    /// never produce ambiguous runs of asterisks. Spaces are kept outside the markers, which Markdown requires.
    /// </summary>
    private static string Inline(List<RichTextRun> runs)
    {
        var text = new StringBuilder();
        var open = new List<string>();

        foreach (var run in runs)
        {
            if (run.IsBreak)
            {
                CloseAll(text, open);
                text.Append("\\\n");
                continue;
            }

            var wanted = Markers(run);
            var keep = 0;
            while (keep < open.Count && keep < wanted.Count && open[keep] == wanted[keep])
            {
                keep++;
            }

            Close(text, open, keep);

            var body = run.Code ? CodeSpan(run.Text) : Escape(run.Text);
            var leading = body.Length - body.TrimStart(' ').Length;
            text.Append(' ', leading);
            body = body[leading..];
            if (body.Length == 0)
            {
                continue;
            }

            for (var i = keep; i < wanted.Count; i++)
            {
                text.Append(Opening(wanted[i]));
                open.Add(wanted[i]);
            }

            text.Append(body);
        }

        CloseAll(text, open);
        return text.ToString();
    }

    /// <summary>
    /// The markers a run needs, outermost first. A link is a marker too, so the text inside it can be formatted.
    /// </summary>
    private static List<string> Markers(RichTextRun run)
    {
        var markers = new List<string>();
        if (run.Href is not null)
        {
            markers.Add("link:" + run.Href);
        }

        if (run.Bold)
        {
            markers.Add("**");
        }

        if (run.Italic)
        {
            markers.Add("*");
        }

        if (run.Strike)
        {
            markers.Add("~~");
        }

        if (run.Underline)
        {
            markers.Add("u");
        }

        return markers;
    }

    private static string Opening(string marker) => marker switch
    {
        "u" => "<u>",
        _ when marker.StartsWith("link:", StringComparison.Ordinal) => "[",
        _ => marker,
    };

    private static string Closing(string marker) => marker switch
    {
        "u" => "</u>",
        _ when marker.StartsWith("link:", StringComparison.Ordinal) => $"]({LinkDestination(marker[5..])})",
        _ => marker,
    };

    private static void CloseAll(StringBuilder text, List<string> open) => Close(text, open, 0);

    /// <summary>
    /// Closes the open markers down to <paramref name="keep"/>, moving any spaces before them to after them.
    /// </summary>
    private static void Close(StringBuilder text, List<string> open, int keep)
    {
        if (open.Count <= keep)
        {
            return;
        }

        var spaces = 0;
        while (text.Length > 0 && text[^1] == ' ')
        {
            text.Length--;
            spaces++;
        }

        for (var i = open.Count - 1; i >= keep; i--)
        {
            text.Append(Closing(open[i]));
            open.RemoveAt(i);
        }

        text.Append(' ', spaces);
    }

    private static string LinkDestination(string href)
    {
        var escaped = href.Replace("\\", "\\\\", StringComparison.Ordinal);
        return escaped.Any(c => c is ' ' or '(' or ')' or '<' or '>')
            ? $"<{escaped.Replace("<", "%3C", StringComparison.Ordinal).Replace(">", "%3E", StringComparison.Ordinal)}>"
            : escaped;
    }

    private static string CodeSpan(string code)
    {
        var fence = new string('`', LongestRun(code, '`') + 1);
        var padding = code.StartsWith('`') || code.EndsWith('`') ? " " : string.Empty;
        return fence + padding + code + padding + fence;
    }

    private static int LongestRun(string text, char c)
    {
        int longest = 0, current = 0;
        foreach (var ch in text)
        {
            current = ch == c ? current + 1 : 0;
            longest = Math.Max(longest, current);
        }

        return longest;
    }

    private static string Escape(string text)
    {
        var escaped = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (specialCharacters.Contains(c))
            {
                escaped.Append('\\');
            }

            escaped.Append(c);
        }

        return escaped.ToString();
    }

    /// <summary>
    /// Escapes text at the start of a paragraph that Markdown would read as a heading, list item or quote.
    /// </summary>
    private static string EscapeLineStart(string text)
    {
        if (text.Length == 0)
        {
            return text;
        }

        if (text[0] is '#' or '-' or '+' or '=')
        {
            return "\\" + text;
        }

        return OrderedMarker().Replace(text, "$1\\$2", 1);
    }

    [GeneratedRegex(@"^(\d+)([.)])")]
    private static partial Regex OrderedMarker();
}
