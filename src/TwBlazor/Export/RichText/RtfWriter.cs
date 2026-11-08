// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.Text;

namespace TwBlazor.Export.RichText;

/// <summary>
/// Writes a <see cref="RichTextDocument"/> as Rich Text Format. Every character outside ASCII is written as a
/// Unicode escape, so the file reads the same in any language.
/// </summary>
internal static class RtfWriter
{
    // Measurements in twentieths of a point (twips).
    private const int listIndent = 360;
    private const int quoteIndent = 720;

    /// <summary>
    /// Writes the document.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="title">The document's title, or <see langword="null"/> for none.</param>
    /// <returns>The RTF.</returns>
    public static string Write(RichTextDocument document, string? title)
    {
        var rtf = new StringBuilder();
        rtf.Append(@"{\rtf1\ansi\ansicpg1252\deff0\uc1");
        rtf.Append(@"{\fonttbl{\f0\fswiss Calibri;}{\f1\fmodern Consolas;}}");
        rtf.Append(@"{\colortbl;\red5\green99\blue193;\red89\green89\blue89;\red242\green242\blue242;}");
        if (!string.IsNullOrWhiteSpace(title))
        {
            rtf.Append(@"{\info{\title ").Append(Escape(title)).Append("}}");
        }

        rtf.Append(@"\fs22").Append('\n');

        foreach (var block in document.Blocks)
        {
            WriteBlock(rtf, block);
        }

        rtf.Append('}');
        return rtf.ToString();
    }

    private static void WriteBlock(StringBuilder rtf, RichTextBlock block)
    {
        var left = block.QuoteDepth * quoteIndent;
        rtf.Append(@"\pard\plain\fs22\sa160\sl276\slmult1");

        switch (block.Kind)
        {
            case RichTextBlockKind.Heading1:
                rtf.Append(@"\sb360\keepn\b\fs36");
                break;
            case RichTextBlockKind.Heading2:
                rtf.Append(@"\sb280\keepn\b\fs30");
                break;
            case RichTextBlockKind.Heading3:
                rtf.Append(@"\sb240\keepn\b\fs26");
                break;
            case RichTextBlockKind.Code:
                rtf.Append(@"\f1\fs20\cbpat3");
                break;
            case RichTextBlockKind.ListItem:
                left += (block.ListLevel + 1) * listIndent;
                rtf.Append(CultureInfo.InvariantCulture, $@"\fi-{listIndent}\tx{left}");
                break;
        }

        if (block.QuoteDepth > 0 && block.Kind != RichTextBlockKind.Code)
        {
            rtf.Append(@"\i\cf2");
        }

        if (left > 0)
        {
            rtf.Append(CultureInfo.InvariantCulture, $@"\li{left}");
        }

        rtf.Append(' ');

        if (block.Kind == RichTextBlockKind.ListItem)
        {
            rtf.Append(block.Ordered ? block.Number.ToString(CultureInfo.InvariantCulture) + "." : @"\bullet").Append(@"\tab ");
        }

        if (block.Kind == RichTextBlockKind.Code)
        {
            rtf.Append(Escape(block.CodeText.ToString()).Replace("\n", @"\line ", StringComparison.Ordinal));
        }
        else
        {
            WriteRuns(rtf, block.Runs);
        }

        rtf.Append(@"\par").Append('\n');
    }

    private static void WriteRuns(StringBuilder rtf, List<RichTextRun> runs)
    {
        var i = 0;
        while (i < runs.Count)
        {
            var href = runs[i].Href;
            if (href is null)
            {
                WriteRun(rtf, runs[i]);
                i++;
                continue;
            }

            rtf.Append(@"{\field{\*\fldinst{HYPERLINK """).Append(Escape(href).Replace("\"", "%22", StringComparison.Ordinal)).Append(@"""}}{\fldrslt{\ul\cf1 ");
            while (i < runs.Count && runs[i].Href == href)
            {
                WriteRun(rtf, runs[i]);
                i++;
            }

            rtf.Append("}}}");
        }
    }

    private static void WriteRun(StringBuilder rtf, RichTextRun run)
    {
        if (run.IsBreak)
        {
            rtf.Append(@"\line ");
            return;
        }

        rtf.Append('{');
        var start = rtf.Length;
        if (run.Bold)
        {
            rtf.Append(@"\b");
        }

        if (run.Italic)
        {
            rtf.Append(@"\i");
        }

        if (run.Underline)
        {
            rtf.Append(@"\ul");
        }

        if (run.Strike)
        {
            rtf.Append(@"\strike");
        }

        if (run.Code)
        {
            rtf.Append(@"\f1\fs20");
        }

        // A space ends a control word, but straight after a brace it would be part of the text.
        if (rtf.Length > start)
        {
            rtf.Append(' ');
        }

        rtf.Append(Escape(run.Text)).Append('}');
    }

    /// <summary>
    /// Escapes RTF's control characters and writes every character outside ASCII as <c>\uN?</c>, where N is the
    /// UTF-16 code unit as a signed 16-bit number, as RTF requires.
    /// </summary>
    internal static string Escape(string text)
    {
        var escaped = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            switch (c)
            {
                case '\\' or '{' or '}':
                    escaped.Append('\\').Append(c);
                    break;
                case '\t':
                    escaped.Append(@"\tab ");
                    break;
                case < ' ' and not '\n':
                    break;
                case > '\u007f':
                    escaped.Append(CultureInfo.InvariantCulture, $@"\u{(short)c}?");
                    break;
                default:
                    escaped.Append(c);
                    break;
            }
        }

        return escaped.ToString();
    }
}
