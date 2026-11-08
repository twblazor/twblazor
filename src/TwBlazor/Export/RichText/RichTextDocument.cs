// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Net;
using System.Text;

namespace TwBlazor.Export.RichText;

/// <summary>
/// The kind of a paragraph-level block of a <see cref="RichTextDocument"/>.
/// </summary>
internal enum RichTextBlockKind
{
    Paragraph,
    Heading1,
    Heading2,
    Heading3,
    ListItem,
    Code
}

/// <summary>
/// A stretch of text with one set of formats, or a line break.
/// </summary>
internal sealed class RichTextRun
{
    public string Text { get; set; } = string.Empty;

    public bool Bold { get; init; }

    public bool Italic { get; init; }

    public bool Underline { get; init; }

    public bool Strike { get; init; }

    public bool Code { get; init; }

    public string? Href { get; init; }

    public bool IsBreak { get; init; }

    /// <summary>
    /// Gets whether this run has the same formats and link as <paramref name="other"/>, so the two can be joined.
    /// </summary>
    public bool SameStyle(RichTextRun other) =>
        !IsBreak && !other.IsBreak && Bold == other.Bold && Italic == other.Italic && Underline == other.Underline
        && Strike == other.Strike && Code == other.Code && Href == other.Href;
}

/// <summary>
/// A paragraph-level block: a paragraph, heading, list item or code block, with the quotes and lists around it.
/// </summary>
internal sealed class RichTextBlock
{
    public RichTextBlockKind Kind { get; init; }

    /// <summary>
    /// Gets how many quotes the block is inside.
    /// </summary>
    public int QuoteDepth { get; init; }

    /// <summary>
    /// Gets how deeply a list item is nested, from 0 for an item of a top level list.
    /// </summary>
    public int ListLevel { get; init; }

    /// <summary>
    /// Gets whether a list item belongs to a numbered list.
    /// </summary>
    public bool Ordered { get; init; }

    /// <summary>
    /// Gets an id shared by the items of one list, so a writer can tell two lists apart.
    /// </summary>
    public int ListId { get; init; }

    /// <summary>
    /// Gets a list item's position in its list, from 1.
    /// </summary>
    public int Number { get; init; }

    public List<RichTextRun> Runs { get; } = [];

    /// <summary>
    /// Gets the text of a code block, with its line breaks and spaces kept.
    /// </summary>
    public StringBuilder CodeText { get; } = new();
}

/// <summary>
/// Rich text as a flat list of blocks, read from the HTML a <see cref="Components.TwTextEditor"/> produces. The
/// writers in this namespace, used by <see cref="Components.TwTextEditorExport"/>, write every format from it.
/// </summary>
/// <remarks>
/// The reader handles the tags the editor writes (and the HTML the browser serializes them as). Other tags are
/// unwrapped, keeping their text, and scripts, styles and similar elements are dropped with what is inside them, so
/// the reader is safe to run on HTML from anywhere. Links keep only http, https, mailto, tel and relative addresses.
/// </remarks>
internal sealed class RichTextDocument
{
    private static readonly HashSet<string> _voidTags = new(StringComparer.Ordinal) { "br", "hr", "img", "input", "meta", "link", "wbr", "area", "base", "col", "embed", "source", "track" };

    private static readonly HashSet<string> _droppedTags = new(StringComparer.Ordinal) { "script", "style", "template", "noscript", "iframe", "object", "svg", "math", "head", "title", "textarea", "select", "button" };

    private static readonly string[] _safeSchemes = ["http", "https", "mailto", "tel"];

    private readonly HtmlNode _root;

    private RichTextDocument(HtmlNode root, List<RichTextBlock> blocks)
    {
        _root = root;
        Blocks = blocks;
    }

    /// <summary>
    /// Gets the blocks, in reading order.
    /// </summary>
    public IReadOnlyList<RichTextBlock> Blocks { get; }

    /// <summary>
    /// Reads HTML into blocks.
    /// </summary>
    /// <param name="html">The HTML, or <see langword="null"/> for an empty document.</param>
    /// <returns>The document.</returns>
    public static RichTextDocument Parse(string? html)
    {
        var tree = ParseTree(html ?? string.Empty);
        var flattener = new Flattener();
        flattener.Walk(tree, default);
        flattener.EndBlock();
        return new RichTextDocument(tree, flattener.Blocks);
    }

    /// <summary>
    /// Writes the document back out as HTML made only of the tags the editor produces, with every text escaped and
    /// every unsafe link removed.
    /// </summary>
    /// <returns>The clean HTML.</returns>
    public string ToCleanHtml()
    {
        var html = new StringBuilder();
        WriteCleanHtml(_root, html);
        return html.ToString();
    }

    /// <summary>
    /// Returns the address when it is safe to link to: http, https, mailto, tel or relative. Control characters and
    /// white space are ignored while checking, since browsers ignore them inside <c>java&#10;script:</c>.
    /// </summary>
    /// <param name="href">The address.</param>
    /// <returns>The trimmed address, or <see langword="null"/> when it is not allowed.</returns>
    internal static string? SafeHref(string? href)
    {
        var trimmed = href?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            return null;
        }

        var compact = new string([.. trimmed.Where(c => c > ' ' && !char.IsControl(c)) ]);
        var colon = compact.IndexOf(':');
        if (colon <= 0 || !char.IsAsciiLetter(compact[0]) || !compact[..colon].All(c => char.IsAsciiLetterOrDigit(c) || c is '+' or '-' or '.'))
        {
            return trimmed;
        }

        return _safeSchemes.Contains(compact[..colon], StringComparer.OrdinalIgnoreCase) ? trimmed : null;
    }

    #region Reading the HTML

    private sealed class HtmlNode
    {
        public string? Tag { get; init; }

        public string Text { get; set; } = string.Empty;

        public string? Href { get; init; }

        public HtmlNode? Parent { get; init; }

        public List<HtmlNode> Children { get; } = [];
    }

    private static HtmlNode ParseTree(string html)
    {
        var root = new HtmlNode { Tag = "#root" };
        var current = root;
        var i = 0;

        while (i < html.Length)
        {
            if (html[i] != '<')
            {
                var end = html.IndexOf('<', i);
                if (end < 0)
                {
                    end = html.Length;
                }

                AppendText(current, WebUtility.HtmlDecode(html[i..end]));
                i = end;
                continue;
            }

            if (string.CompareOrdinal(html, i, "<!--", 0, 4) == 0)
            {
                var close = html.IndexOf("-->", i + 4, StringComparison.Ordinal);
                i = close < 0 ? html.Length : close + 3;
                continue;
            }

            if (i + 1 < html.Length && html[i + 1] == '/')
            {
                var close = html.IndexOf('>', i);
                var name = ReadName(html, i + 2);
                i = close < 0 ? html.Length : close + 1;
                for (var node = current; node is not null && node != root; node = node.Parent)
                {
                    if (node.Tag == name)
                    {
                        current = node.Parent!;
                        break;
                    }
                }

                continue;
            }

            if (i + 1 >= html.Length || !char.IsAsciiLetter(html[i + 1]))
            {
                AppendText(current, "<");
                i++;
                continue;
            }

            var tag = ReadName(html, i + 1);
            var (tagEnd, selfClosing, href) = ReadAttributes(html, i + 1 + tag.Length);
            i = tagEnd;

            if (_droppedTags.Contains(tag))
            {
                if (!selfClosing)
                {
                    var close = html.IndexOf("</" + tag, i, StringComparison.OrdinalIgnoreCase);
                    var closeEnd = close < 0 ? -1 : html.IndexOf('>', close);
                    i = closeEnd < 0 ? html.Length : closeEnd + 1;
                }

                continue;
            }

            var element = new HtmlNode { Tag = tag, Parent = current, Href = tag == "a" ? SafeHref(href) : null };
            current.Children.Add(element);
            if (!selfClosing && !_voidTags.Contains(tag))
            {
                current = element;
            }
        }

        return root;
    }

    private static void AppendText(HtmlNode parent, string text)
    {
        if (text.Length == 0)
        {
            return;
        }

        if (parent.Children.Count > 0 && parent.Children[^1].Tag is null)
        {
            parent.Children[^1].Text += text;
            return;
        }

        parent.Children.Add(new HtmlNode { Text = text, Parent = parent });
    }

    private static string ReadName(string html, int start)
    {
        var end = start;
        while (end < html.Length && (char.IsAsciiLetterOrDigit(html[end]) || html[end] is '-' or ':'))
        {
            end++;
        }

        return html[start..end].ToLowerInvariant();
    }

    /// <summary>
    /// Reads a start tag's attributes up to its closing <c>&gt;</c>, keeping only <c>href</c>.
    /// </summary>
    private static (int End, bool SelfClosing, string? Href) ReadAttributes(string html, int start)
    {
        string? href = null;
        var i = start;

        while (i < html.Length)
        {
            var c = html[i];
            if (c == '>')
            {
                return (i + 1, false, href);
            }

            if (c == '/' && i + 1 < html.Length && html[i + 1] == '>')
            {
                return (i + 2, true, href);
            }

            if (char.IsWhiteSpace(c) || c == '/')
            {
                i++;
                continue;
            }

            var nameStart = i;
            while (i < html.Length && !char.IsWhiteSpace(html[i]) && html[i] is not '=' and not '>' and not '/')
            {
                i++;
            }

            var name = html[nameStart..i].ToLowerInvariant();
            while (i < html.Length && char.IsWhiteSpace(html[i]))
            {
                i++;
            }

            string? value = null;
            if (i < html.Length && html[i] == '=')
            {
                i++;
                while (i < html.Length && char.IsWhiteSpace(html[i]))
                {
                    i++;
                }

                if (i < html.Length && html[i] is '"' or '\'')
                {
                    var quote = html[i];
                    var close = html.IndexOf(quote, i + 1);
                    if (close < 0)
                    {
                        close = html.Length;
                    }

                    value = html[(i + 1)..close];
                    i = Math.Min(close + 1, html.Length);
                }
                else
                {
                    var valueStart = i;
                    while (i < html.Length && !char.IsWhiteSpace(html[i]) && html[i] != '>')
                    {
                        i++;
                    }

                    value = html[valueStart..i];
                }
            }

            if (name == "href" && value is not null)
            {
                href = WebUtility.HtmlDecode(value);
            }
        }

        return (html.Length, false, href);
    }

    #endregion

    #region Clean HTML

    private static readonly Dictionary<string, string> _cleanTags = new(StringComparer.Ordinal)
    {
        ["p"] = "p", ["div"] = "p", ["h1"] = "h1", ["h2"] = "h2", ["h3"] = "h3", ["h4"] = "h3", ["h5"] = "h3", ["h6"] = "h3",
        ["strong"] = "strong", ["b"] = "strong", ["em"] = "em", ["i"] = "em", ["u"] = "u", ["s"] = "s", ["strike"] = "s",
        ["del"] = "s", ["ul"] = "ul", ["ol"] = "ol", ["li"] = "li", ["blockquote"] = "blockquote", ["pre"] = "pre",
        ["code"] = "code", ["a"] = "a", ["br"] = "br",
    };

    private static void WriteCleanHtml(HtmlNode node, StringBuilder html)
    {
        foreach (var child in node.Children)
        {
            if (child.Tag is null)
            {
                html.Append(WebUtility.HtmlEncode(child.Text));
                continue;
            }

            if (!_cleanTags.TryGetValue(child.Tag, out var tag) || (tag == "a" && child.Href is null))
            {
                WriteCleanHtml(child, html);
                continue;
            }

            if (tag == "br")
            {
                html.Append("<br>");
                continue;
            }

            html.Append('<').Append(tag);
            if (tag == "a")
            {
                html.Append(" href=\"").Append(WebUtility.HtmlEncode(child.Href)).Append('"');
            }

            html.Append('>');
            WriteCleanHtml(child, html);
            html.Append("</").Append(tag).Append('>');
        }
    }

    #endregion

    #region Flattening into blocks

    /// <summary>
    /// The inline formats in effect while walking the tree.
    /// </summary>
    private readonly record struct InlineStyle(bool Bold, bool Italic, bool Underline, bool Strike, bool Code, string? Href);

    private sealed class ListContext
    {
        public bool Ordered { get; init; }

        public int Id { get; init; }

        public int Count { get; set; }
    }

    private sealed class Flattener
    {
        private readonly Stack<ListContext> _lists = new();
        private int quoteDepth;
        private int nextListId = 1;
        private RichTextBlock? current;

        public List<RichTextBlock> Blocks { get; } = [];

        public void Walk(HtmlNode node, InlineStyle style)
        {
            foreach (var child in node.Children)
            {
                Visit(child, style);
            }
        }

        private void Visit(HtmlNode node, InlineStyle style)
        {
            if (node.Tag is null)
            {
                AddText(node.Text, style);
                return;
            }

            switch (node.Tag)
            {
                case "br":
                    EnsureBlock().Runs.Add(new RichTextRun { IsBreak = true });
                    break;
                case "strong" or "b":
                    Walk(node, style with { Bold = true });
                    break;
                case "em" or "i":
                    Walk(node, style with { Italic = true });
                    break;
                case "u":
                    Walk(node, style with { Underline = true });
                    break;
                case "s" or "strike" or "del":
                    Walk(node, style with { Strike = true });
                    break;
                case "code":
                    Walk(node, style with { Code = true });
                    break;
                case "a":
                    Walk(node, node.Href is null ? style : style with { Href = node.Href });
                    break;
                case "p" or "div":
                    if (current is { Kind: RichTextBlockKind.ListItem, Runs.Count: 0 })
                    {
                        // A paragraph that starts a list item is the item's own text.
                        Walk(node, style);
                        break;
                    }

                    StartBlock(RichTextBlockKind.Paragraph);
                    Walk(node, style);
                    EndBlock();
                    break;
                case "h1":
                    Heading(node, style, RichTextBlockKind.Heading1);
                    break;
                case "h2":
                    Heading(node, style, RichTextBlockKind.Heading2);
                    break;
                case "h3" or "h4" or "h5" or "h6":
                    Heading(node, style, RichTextBlockKind.Heading3);
                    break;
                case "pre":
                    Code(node);
                    break;
                case "blockquote":
                    EndBlock();
                    quoteDepth++;
                    Walk(node, style);
                    EndBlock();
                    quoteDepth--;
                    break;
                case "ul" or "ol":
                    EndBlock();
                    _lists.Push(new ListContext { Ordered = node.Tag == "ol", Id = nextListId++ });
                    Walk(node, style);
                    EndBlock();
                    _lists.Pop();
                    break;
                case "li":
                    ListItem(node, style);
                    break;
                default:
                    Walk(node, style);
                    break;
            }
        }

        private void Heading(HtmlNode node, InlineStyle style, RichTextBlockKind kind)
        {
            StartBlock(kind);
            Walk(node, style);
            EndBlock();
        }

        private void ListItem(HtmlNode node, InlineStyle style)
        {
            EndBlock();
            if (_lists.Count == 0)
            {
                _lists.Push(new ListContext { Ordered = false, Id = nextListId++ });
                ListItem(node, style);
                _lists.Pop();
                return;
            }

            var list = _lists.Peek();
            list.Count++;
            current = new RichTextBlock
            {
                Kind = RichTextBlockKind.ListItem,
                QuoteDepth = quoteDepth,
                ListLevel = _lists.Count - 1,
                Ordered = list.Ordered,
                ListId = list.Id,
                Number = list.Count,
            };
            Walk(node, style);
            EndBlock();
        }

        private void Code(HtmlNode node)
        {
            StartBlock(RichTextBlockKind.Code);
            var block = current!;
            AppendCode(node, block.CodeText);

            // As in HTML, a line break straight after <pre> is not part of the text.
            if (block.CodeText.Length > 0 && block.CodeText[0] == '\n')
            {
                block.CodeText.Remove(0, 1);
            }

            while (block.CodeText.Length > 0 && block.CodeText[^1] is '\n' or '\r')
            {
                block.CodeText.Length--;
            }

            EndBlock();
        }

        private static void AppendCode(HtmlNode node, StringBuilder text)
        {
            foreach (var child in node.Children)
            {
                if (child.Tag is null)
                {
                    text.Append(child.Text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace(' ', ' '));
                }
                else if (child.Tag == "br")
                {
                    text.Append('\n');
                }
                else
                {
                    AppendCode(child, text);
                }
            }
        }

        private void StartBlock(RichTextBlockKind kind)
        {
            EndBlock();
            current = new RichTextBlock { Kind = kind, QuoteDepth = quoteDepth };
        }

        private RichTextBlock EnsureBlock()
        {
            if (current is null)
            {
                StartBlock(RichTextBlockKind.Paragraph);
            }

            return current!;
        }

        /// <summary>
        /// Adds text with HTML's white space rules: line breaks and tabs are spaces, runs of spaces are one space, and a
        /// block does not start with a space. A non-breaking space is kept as a space, so spaces typed in a row survive.
        /// </summary>
        private void AddText(string text, InlineStyle style)
        {
            var collapsed = new StringBuilder(text.Length);
            foreach (var c in text)
            {
                var ch = c is '\r' or '\n' or '\t' or '\f' ? ' ' : c;
                if (ch == ' ' && collapsed.Length > 0 && collapsed[^1] == ' ')
                {
                    continue;
                }

                collapsed.Append(ch);
            }

            var value = collapsed.ToString();
            if (value.Length == 0 || (value == " " && current is null))
            {
                return;
            }

            var block = EnsureBlock();
            var last = block.Runs.Count > 0 ? block.Runs[^1] : null;
            if (last is null || last.IsBreak || last.Text.EndsWith(' '))
            {
                value = value.TrimStart(' ');
            }

            value = value.Replace(' ', ' ');
            if (value.Length == 0)
            {
                return;
            }

            var run = new RichTextRun { Text = value, Bold = style.Bold, Italic = style.Italic, Underline = style.Underline, Strike = style.Strike, Code = style.Code, Href = style.Href };
            if (last is not null && last.SameStyle(run))
            {
                last.Text += value;
                return;
            }

            block.Runs.Add(run);
        }

        /// <summary>
        /// Finishes the open block: trims the space at its end, and drops it when it holds no text.
        /// </summary>
        public void EndBlock()
        {
            if (current is null)
            {
                return;
            }

            var block = current;
            current = null;

            if (block.Kind == RichTextBlockKind.Code)
            {
                Blocks.Add(block);
                return;
            }

            while (block.Runs.Count > 0 && block.Runs[^1] is { IsBreak: false } last && last.Text.TrimEnd(' ').Length == 0)
            {
                block.Runs.RemoveAt(block.Runs.Count - 1);
            }

            while (block.Runs.Count > 0 && block.Runs[^1].IsBreak)
            {
                block.Runs.RemoveAt(block.Runs.Count - 1);
            }

            if (block.Runs.Count > 0)
            {
                block.Runs[^1].Text = block.Runs[^1].Text.TrimEnd(' ');
            }

            // An empty list item is kept so the numbering of the items after it does not change.
            if (block.Runs.Count > 0 || block.Kind == RichTextBlockKind.ListItem)
            {
                Blocks.Add(block);
            }
        }
    }

    #endregion
}
