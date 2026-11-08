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

    private const string strongTag = "strong";
    private const string emphasisTag = "em";

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

    private static HtmlNode ParseTree(string html) => new HtmlReader(html).Read();

    /// <summary>
    /// Reads HTML into a tree of <see cref="HtmlNode"/>s, one token at a time: text, a comment, an end tag or a start
    /// tag. It forgives what browsers forgive, such as a stray <c>&lt;</c> or an end tag with no start tag.
    /// </summary>
    private sealed class HtmlReader(string html)
    {
        private readonly HtmlNode _root = new() { Tag = "#root" };
        private HtmlNode current = null!;
        private int position;

        public HtmlNode Read()
        {
            current = _root;
            while (position < html.Length)
            {
                if (html[position] != '<')
                {
                    ReadText();
                }
                else if (string.CompareOrdinal(html, position, "<!--", 0, 4) == 0)
                {
                    SkipComment();
                }
                else if (position + 1 < html.Length && html[position + 1] == '/')
                {
                    ReadEndTag();
                }
                else if (position + 1 < html.Length && char.IsAsciiLetter(html[position + 1]))
                {
                    ReadStartTag();
                }
                else
                {
                    AppendText(current, "<");
                    position++;
                }
            }

            return _root;
        }

        private void ReadText()
        {
            var end = html.IndexOf('<', position);
            if (end < 0)
            {
                end = html.Length;
            }

            AppendText(current, WebUtility.HtmlDecode(html[position..end]));
            position = end;
        }

        private void SkipComment()
        {
            var close = html.IndexOf("-->", position + 4, StringComparison.Ordinal);
            position = close < 0 ? html.Length : close + 3;
        }

        /// <summary>
        /// Closes the nearest open element with the end tag's name, and any left open inside it. An end tag with no
        /// open element of that name is ignored.
        /// </summary>
        private void ReadEndTag()
        {
            var name = ReadName(position + 2);
            var close = html.IndexOf('>', position);
            position = close < 0 ? html.Length : close + 1;

            for (var node = current; node is not null && node != _root; node = node.Parent)
            {
                if (node.Tag == name)
                {
                    current = node.Parent!;
                    return;
                }
            }
        }

        private void ReadStartTag()
        {
            var tag = ReadName(position + 1);
            position += 1 + tag.Length;
            var (selfClosing, href) = ReadAttributes();

            if (_droppedTags.Contains(tag))
            {
                if (!selfClosing)
                {
                    SkipPast("</" + tag);
                }

                return;
            }

            var element = new HtmlNode { Tag = tag, Parent = current, Href = tag == "a" ? SafeHref(href) : null };
            current.Children.Add(element);
            if (!selfClosing && !_voidTags.Contains(tag))
            {
                current = element;
            }
        }

        /// <summary>
        /// Skips everything up to and including the end tag that starts with <paramref name="endTag"/>, such as a
        /// script's body.
        /// </summary>
        private void SkipPast(string endTag)
        {
            var close = html.IndexOf(endTag, position, StringComparison.OrdinalIgnoreCase);
            var closeEnd = close < 0 ? -1 : html.IndexOf('>', close);
            position = closeEnd < 0 ? html.Length : closeEnd + 1;
        }

        private string ReadName(int start)
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
        private (bool SelfClosing, string? Href) ReadAttributes()
        {
            string? href = null;
            while (position < html.Length)
            {
                var c = html[position];
                if (c == '>')
                {
                    position++;
                    return (false, href);
                }

                if (c == '/' && position + 1 < html.Length && html[position + 1] == '>')
                {
                    position += 2;
                    return (true, href);
                }

                if (char.IsWhiteSpace(c) || c == '/')
                {
                    position++;
                    continue;
                }

                var (name, value) = ReadAttribute();
                if (name == "href" && value is not null)
                {
                    href = WebUtility.HtmlDecode(value);
                }
            }

            return (false, href);
        }

        private (string Name, string? Value) ReadAttribute()
        {
            var nameStart = position;
            while (position < html.Length && !char.IsWhiteSpace(html[position]) && html[position] is not '=' and not '>' and not '/')
            {
                position++;
            }

            var name = html[nameStart..position].ToLowerInvariant();
            SkipWhiteSpace();
            if (position >= html.Length || html[position] != '=')
            {
                return (name, null);
            }

            position++;
            SkipWhiteSpace();
            return (name, ReadAttributeValue());
        }

        private string ReadAttributeValue()
        {
            if (position < html.Length && html[position] is '"' or '\'')
            {
                var quote = html[position];
                var close = html.IndexOf(quote, position + 1);
                if (close < 0)
                {
                    close = html.Length;
                }

                var quoted = html[(position + 1)..close];
                position = Math.Min(close + 1, html.Length);
                return quoted;
            }

            var start = position;
            while (position < html.Length && !char.IsWhiteSpace(html[position]) && html[position] != '>')
            {
                position++;
            }

            return html[start..position];
        }

        private void SkipWhiteSpace()
        {
            while (position < html.Length && char.IsWhiteSpace(html[position]))
            {
                position++;
            }
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
    }

    #endregion

    #region Clean HTML

    private static readonly Dictionary<string, string> _cleanTags = new(StringComparer.Ordinal)
    {
        ["p"] = "p", ["div"] = "p", ["h1"] = "h1", ["h2"] = "h2", ["h3"] = "h3", ["h4"] = "h3", ["h5"] = "h3", ["h6"] = "h3",
        [strongTag] = strongTag, ["b"] = strongTag, [emphasisTag] = emphasisTag, ["i"] = emphasisTag, ["u"] = "u", ["s"] = "s", ["strike"] = "s",
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
                case strongTag or "b":
                    Walk(node, style with { Bold = true });
                    break;
                case emphasisTag or "i":
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
            var value = CollapseWhiteSpace(text);
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
        /// Turns line breaks and tabs into spaces and runs of spaces into one, as HTML shows them.
        /// </summary>
        private static string CollapseWhiteSpace(string text)
        {
            var collapsed = new StringBuilder(text.Length);
            foreach (var c in text)
            {
                var ch = c is '\r' or '\n' or '\t' or '\f' ? ' ' : c;
                if (ch != ' ' || collapsed.Length == 0 || collapsed[^1] != ' ')
                {
                    collapsed.Append(ch);
                }
            }

            return collapsed.ToString();
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

            TrimTrailingSpace(block.Runs);

            // An empty list item is kept so the numbering of the items after it does not change.
            if (block.Runs.Count > 0 || block.Kind == RichTextBlockKind.ListItem)
            {
                Blocks.Add(block);
            }
        }

        /// <summary>
        /// Removes the white space and line breaks at the end of a block's runs, as a browser does when it shows them.
        /// </summary>
        private static void TrimTrailingSpace(List<RichTextRun> runs)
        {
            while (runs.Count > 0 && (runs[^1].IsBreak || runs[^1].Text.TrimEnd(' ').Length == 0))
            {
                runs.RemoveAt(runs.Count - 1);
            }

            if (runs.Count > 0)
            {
                runs[^1].Text = runs[^1].Text.TrimEnd(' ');
            }
        }
    }

    #endregion
}
