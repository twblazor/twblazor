using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TwBlazor.Docs.Compiler;

/// <summary>
/// A docs page read from its <c>.razor</c> source.
/// </summary>
/// <param name="Title">The <c>PageContainer</c> title, e.g. <c>TwButton</c>.</param>
/// <param name="Description">The <c>PageContainer</c> description, used as the page summary.</param>
/// <param name="ThemeType">The component theme type named by <c>ThemeType="typeof(...)"</c>, or empty.</param>
/// <param name="Body">The page content as Markdown, starting at its first section.</param>
/// <param name="ExampleComponents">Every <c>Tw*</c> component used in the page's code examples, in order of first use.</param>
public sealed record RazorPageContent(string Title, string Description, string ThemeType, string Body, IReadOnlyList<string> ExampleComponents);

/// <summary>
/// Turns a docs page's Razor source into Markdown: sections become headings, prose keeps its links and inline
/// code, and every code sample is written out in full from the same extracted snippets the site displays.
/// </summary>
/// <remarks>
/// This reads source rather than rendered output, so live demos contribute only their text, and Razor control
/// flow outside code samples is dropped. Code samples come from <c>CodeExample</c>, <c>ExampleCodeBlock</c> and
/// <c>TwCodeBlock</c>, resolving <c>CodeExamples.Get("...")</c> and the page's own <c>const string</c> fields.
/// </remarks>
public static partial class RazorMarkdownConverter
{
    private const string fenceToken = "\u0002";

    [GeneratedRegex(@"^@code\b", RegexOptions.Multiline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex CodeBlockStartRegex();

    [GeneratedRegex(@"@\*.*?\*@", RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RazorCommentRegex();

    [GeneratedRegex(@"<!--.*?-->", RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex HtmlCommentRegex();

    [GeneratedRegex(@"^[ \t]*@(page|using|inject|layout|inherits|implements|attribute|namespace|typeparam|rendermode)\b.*$", RegexOptions.Multiline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex DirectiveRegex();

    [GeneratedRegex("""(?:const|static\s+readonly)\s+string\s+(?<name>\w+)\s*=\s*(?<quotes>"{3,})[ \t]*\r?\n(?<body>.*?)\r?\n[ \t]*\k<quotes>\s*;""", RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RawStringFieldRegex();

    [GeneratedRegex("""<(?<close>/?)(?<name>[A-Za-z][\w.:-]*)(?<attrs>(?:\s+[^\s"'>/=]+(?:\s*=\s*(?:"[^"]*"|'[^']*'|[^\s>"']+))?)*)\s*(?<self>/?)>""", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex TagRegex();

    [GeneratedRegex("""(?<name>[\w:-]+)\s*=\s*(?:"(?<value>[^"]*)"|'(?<value>[^']*)')""", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AttributeRegex();

    [GeneratedRegex(@"<(?<name>CodeExample|ExampleCodeBlock|TwCodeBlock)\b", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex CodeSampleStartRegex();

    [GeneratedRegex("""\bContent\s*=\s*"(?<value>@CodeExamples\.Get\(\s*"[^"]+"\s*\)|[^"]*)"(?=[\s/>])""", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ContentAttributeRegex();

    [GeneratedRegex("""CodeExamples\.Get\(\s*"(?<name>[^"]+)"\s*\)""", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex CodeExampleReferenceRegex();

    [GeneratedRegex(@"typeof\((?<type>[\w.]+)\)", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex TypeofRegex();

    [GeneratedRegex(@"<(?<name>Tw[A-Z]\w*)\b", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ComponentTagRegex();

    [GeneratedRegex(@"^\s*(?:@(?:if|else|foreach|for|while|switch)\b.*|\}\s*(?:else\b.*)?|\{)\s*$", RegexOptions.Multiline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RazorControlLineRegex();

    [GeneratedRegex(@"(?<![\w])@[A-Za-z_][\w.]*(?:\([^()]*\))?", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RazorExpressionRegex();

    [GeneratedRegex("""const\s+string\s+(?<name>\w+)\s*=\s*"(?<value>[^"\\\r\n]*)"\s*;""", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex StringConstantRegex();

    [GeneratedRegex(@"=\s*""@(?<name>\w+)""", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ConstantAttributeRegex();

    [GeneratedRegex("""\s@[\w:-]+\s*=\s*"[^"]*"\s*""", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex DirectiveAttributeRegex();

    [GeneratedRegex(@"(?<!`)``(?!`)", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EmptyInlineCodeRegex();

    [GeneratedRegex(@"^\|(?:\s*\|)+[ \t]*\n?", RegexOptions.Multiline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EmptyTableRowRegex();

    [GeneratedRegex(@"^\|[^\n]*\|\n\|(?: --- \|)+[ \t]*(?:\n(?!\|)|\z)", RegexOptions.Multiline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex HeaderOnlyTableRegex();

    [GeneratedRegex(@"\s+", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[ \t]{2,}", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RepeatedSpacesRegex();

    [GeneratedRegex(@"\n{3,}", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ExtraBlankLinesRegex();

    [GeneratedRegex(@"^(?:-|#+)\s*$", RegexOptions.Multiline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EmptyMarkerLineRegex();

    private sealed class State(Func<string, string> resolveLink)
    {
        public StringBuilder Output { get; } = new();
        public Stack<string?> Links { get; } = new();
        public Stack<bool> Spans { get; } = new();
        public Func<string, string> ResolveLink { get; } = resolveLink;
        public int TabDepth { get; set; }
        public int SkipDepth { get; set; }
        public bool InTable { get; set; }
        public int TableRow { get; set; }
        public int RowCells { get; set; }
    }

    /// <summary>
    /// Converts one page.
    /// </summary>
    /// <param name="source">The page's <c>.razor</c> source.</param>
    /// <param name="examples">The extracted code samples by name, as used to generate <c>CodeExamples.cs</c>.</param>
    /// <param name="resolveLink">Maps an <c>href</c> as written in the page to the URL to write in the Markdown.</param>
    public static RazorPageContent Convert(string source, IReadOnlyDictionary<string, string> examples, Func<string, string> resolveLink)
    {
        var codeStart = CodeBlockStartRegex().Match(source);
        var markup = codeStart.Success ? source[..codeStart.Index] : source;
        var fields = RawStringFields(codeStart.Success ? source[codeStart.Index..] : string.Empty);

        markup = RazorCommentRegex().Replace(markup, string.Empty);
        markup = HtmlCommentRegex().Replace(markup, string.Empty);
        markup = DirectiveRegex().Replace(markup, string.Empty);

        var fences = new List<string>();
        var exampleComponents = new List<string>();
        markup = ReplaceCodeSamples(markup, examples, fields, fences, exampleComponents);

        var container = TagRegex().Matches(markup).FirstOrDefault(m => m.Groups["name"].Value == "PageContainer" && m.Groups["close"].Length == 0);
        var attributes = container is null ? new Dictionary<string, string>() : Attributes(container.Groups["attrs"].Value);

        // Runtime values (often quoted strings inside quoted attributes) are dropped before tags are read, so
        // they can neither break a tag apart nor leak into the text.
        // An attribute naming one of the page's own string constants, such as href="@docsUrl", keeps its value.
        var constants = StringConstantRegex().Matches(codeStart.Success ? source[codeStart.Index..] : string.Empty)
            .GroupBy(m => m.Groups["name"].Value, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().Groups["value"].Value, StringComparer.Ordinal);
        markup = ConstantAttributeRegex().Replace(markup, m => constants.TryGetValue(m.Groups["name"].Value, out var value) ? $"=\"{value}\"" : m.Value);

        markup = RazorControlLineRegex().Replace(markup, " ");
        markup = markup.Replace("@@", "\u0001", StringComparison.Ordinal);
        markup = DirectiveAttributeRegex().Replace(markup, " ");
        markup = RemoveRazorExpressions(markup).Replace('\u0001', '@');

        var body = Render(markup, new State(resolveLink));
        body = EmptyInlineCodeRegex().Replace(body, string.Empty);
        body = EmptyTableRowRegex().Replace(body, string.Empty);
        body = HeaderOnlyTableRegex().Replace(body, string.Empty);
        body = Regex.Replace(body, $"[ \\t]*{fenceToken}(\\d+){fenceToken}[ \\t]*", m => "\n\n" + fences[int.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture)] + "\n\n", RegexOptions.None, TimeSpan.FromSeconds(5));
        body = ExtraBlankLinesRegex().Replace(body, "\n\n").Trim();

        return new RazorPageContent(
            Decode(attributes.GetValueOrDefault("Title", string.Empty)),
            Decode(attributes.GetValueOrDefault("Description", string.Empty)),
            attributes.TryGetValue("ThemeType", out var themeType) && TypeofRegex().Match(themeType) is { Success: true } typeOf ? typeOf.Groups["type"].Value.Split('.')[^1] : string.Empty,
            body,
            exampleComponents.Distinct(StringComparer.Ordinal).ToList());
    }

    private static Dictionary<string, string> RawStringFields(string code) =>
        RawStringFieldRegex().Matches(code)
            .GroupBy(m => m.Groups["name"].Value, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => SnippetTextUtils.Dedent(g.First().Groups["body"].Value), StringComparer.Ordinal);

    /// <summary>
    /// Swaps every code sample for a token, so the prose clean-up that follows never touches code.
    /// </summary>
    private static string ReplaceCodeSamples(string markup, IReadOnlyDictionary<string, string> examples, Dictionary<string, string> fields, List<string> fences, List<string> exampleComponents)
    {
        var sb = new StringBuilder();
        var pos = 0;

        while (true)
        {
            var match = CodeSampleStartRegex().Match(markup, pos);
            if (!match.Success)
            {
                sb.Append(markup, pos, markup.Length - pos);
                return sb.ToString();
            }

            var start = match.Index;
            var name = match.Groups["name"].Value;

            // Attribute values such as Content="@CodeExamples.Get("Name")" nest quotes, which a general tag
            // pattern cannot pair up. The code block tags are always self-closing and their values may hold a ">"
            // (a Tailwind selector such as [&>*]), so they run to "/>"; CodeExample runs to its first ">".
            var selfClose = name == "CodeExample" ? -1 : markup.IndexOf("/>", start, StringComparison.Ordinal);
            var close = selfClose >= 0 ? selfClose + 1 : markup.IndexOf('>', start);
            var end = close < 0 ? markup.Length : close + 1;
            var openTag = markup[start..end];
            var selfClosing = openTag.EndsWith("/>", StringComparison.Ordinal);
            var attributes = Attributes(openTag);
            if (ContentAttributeRegex().Match(openTag) is { Success: true } contentAttribute)
                attributes["Content"] = contentAttribute.Groups["value"].Value;

            sb.Append(markup, pos, start - pos);

            var samples = new List<(string Language, string Code)>();

            if (!selfClosing)
                end = SkipToClose(markup, end, name);

            if (name == "CodeExample")
            {

                if (attributes.TryGetValue("Name", out var exampleName) && examples.TryGetValue(exampleName, out var razor))
                    samples.Add(("razor", razor));

                if (attributes.TryGetValue("CSharpName", out var csharpName) && examples.TryGetValue(csharpName, out var csharp))
                    samples.Add(("csharp", csharp));
            }
            else
            {
                var language = name == "ExampleCodeBlock" ? "razor" : FenceLanguage(attributes.GetValueOrDefault("Language"));
                var razorKey = name == "ExampleCodeBlock" ? "Razor" : "Content";

                if (attributes.TryGetValue(razorKey, out var content) && ResolveValue(content, examples, fields) is { Length: > 0 } code)
                    samples.Add((language, code));

                if (name == "ExampleCodeBlock" && attributes.TryGetValue("CSharp", out var csharpContent) && ResolveValue(csharpContent, examples, fields) is { Length: > 0 } csharpCode)
                    samples.Add(("csharp", csharpCode));
            }

            foreach (var (language, code) in samples)
            {
                if (language == "razor")
                    exampleComponents.AddRange(ComponentTagRegex().Matches(code).Select(m => m.Groups["name"].Value));

                sb.Append(fenceToken).Append(fences.Count).Append(fenceToken);
                fences.Add(Fence(language, code));
            }

            pos = end;
        }
    }

    private static int SkipToClose(string markup, int from, string name)
    {
        var depth = 1;

        foreach (Match tag in TagRegex().Matches(markup[from..]).Where(t => t.Groups["name"].Value == name && t.Groups["self"].Length == 0))
        {
            depth += tag.Groups["close"].Length > 0 ? -1 : 1;
            if (depth == 0)
                return from + tag.Index + tag.Length;
        }

        return markup.Length;
    }

    private static string? ResolveValue(string value, IReadOnlyDictionary<string, string> examples, Dictionary<string, string> fields)
    {
        if (CodeExampleReferenceRegex().Match(value) is { Success: true } reference)
            return examples.GetValueOrDefault(reference.Groups["name"].Value);

        var field = value.TrimStart('@').Trim();
        return fields.GetValueOrDefault(field);
    }

    private static string FenceLanguage(string? language) => language switch
    {
        null or "" or "html" => "razor",
        _ => language,
    };

    private static string Fence(string language, string code)
    {
        var longestRun = Regex.Matches(code, "`+", RegexOptions.None, TimeSpan.FromSeconds(5)).Select(m => m.Length).DefaultIfEmpty(0).Max();
        var fence = new string('`', Math.Max(3, longestRun + 1));
        return $"{fence}{language}\n{code.Replace("\r\n", "\n").TrimEnd()}\n{fence}";
    }

    private static Dictionary<string, string> Attributes(string text) =>
        AttributeRegex().Matches(text)
            .GroupBy(m => m.Groups["name"].Value, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().Groups["value"].Value, StringComparer.Ordinal);

    private static string Render(string markup, State state)
    {
        var pos = 0;

        foreach (Match tag in TagRegex().Matches(markup))
        {
            AppendText(markup[pos..tag.Index], state);
            ApplyTag(tag, state);
            pos = tag.Index + tag.Length;
        }

        AppendText(markup[pos..], state);

        var text = state.Output.ToString().Replace("\r\n", "\n");
        text = string.Join('\n', text.Split('\n').Select(line => RepeatedSpacesRegex().Replace(line, " ").Trim()));
        return EmptyMarkerLineRegex().Replace(text, string.Empty);
    }

    private static void AppendText(string text, State state)
    {
        if (state.SkipDepth > 0 || text.Length == 0)
            return;

        text = WhitespaceRegex().Replace(Decode(text), " ");

        if (state.InTable)
            text = text.Replace("|", "\\|", StringComparison.Ordinal);

        state.Output.Append(text);
    }

    /// <summary>
    /// Drops <c>@(...)</c> and <c>@name.member(...)</c> expressions, whose values only exist at runtime.
    /// </summary>
    private static string RemoveRazorExpressions(string text)
    {
        var sb = new StringBuilder();
        var i = 0;

        while (i < text.Length)
        {
            if (text[i] == '@' && i + 1 < text.Length && text[i + 1] == '(' && (i == 0 || !char.IsLetterOrDigit(text[i - 1])))
            {
                var depth = 0;
                var j = i + 1;
                for (; j < text.Length; j++)
                {
                    depth += text[j] switch { '(' => 1, ')' => -1, _ => 0 };
                    if (depth == 0)
                        break;
                }

                i = j + 1;
                continue;
            }

            sb.Append(text[i]);
            i++;
        }

        return RazorExpressionRegex().Replace(sb.ToString(), string.Empty);
    }

    private static void ApplyTag(Match tag, State state)
    {
        var name = tag.Groups["name"].Value;
        var isClose = tag.Groups["close"].Length > 0;
        var isSelfClosing = tag.Groups["self"].Length > 0;
        var lower = name.ToLowerInvariant();

        if (lower is "script" or "style" or "svg" or "pageseo" or "headcontent")
        {
            if (!isSelfClosing)
                state.SkipDepth += isClose ? -1 : 1;
            return;
        }

        // Screen reader only text, such as "(opens in a new tab)", describes the rendered page rather than its content.
        if (lower == "span" && !isSelfClosing)
        {
            var srOnly = !isClose && Attributes(tag.Groups["attrs"].Value).GetValueOrDefault("class")?.Split(' ').Contains("sr-only") == true;
            var wasSrOnly = isClose && state.Spans.Count > 0 && state.Spans.Pop();

            if (!isClose)
                state.Spans.Push(srOnly);

            if (srOnly)
                state.SkipDepth++;
            else if (wasSrOnly)
                state.SkipDepth--;

            return;
        }

        if (state.SkipDepth > 0)
            return;

        var output = state.Output;
        var attributes = isClose ? new Dictionary<string, string>() : Attributes(tag.Groups["attrs"].Value);

        switch (lower)
        {
            case "pagecard" when !isClose:
                output.Append("\n\n").Append(state.TabDepth > 0 ? "### " : "## ").Append(Decode(attributes.GetValueOrDefault("Title", string.Empty))).Append("\n\n");
                break;
            case "twtab" when !isClose:
                output.Append("\n\n## ").Append(Decode(attributes.GetValueOrDefault("Label", string.Empty))).Append("\n\n");
                state.TabDepth++;
                break;
            case "twtab":
                state.TabDepth = Math.Max(0, state.TabDepth - 1);
                output.Append("\n\n");
                break;
            case "twcollapse" when !isClose && attributes.TryGetValue("Title", out var collapseTitle):
                output.Append("\n\n**").Append(Decode(collapseTitle)).Append("**\n\n");
                break;
            case "h1" or "h2" or "h3" or "h4" or "h5" when !isClose:
                output.Append("\n\n").Append(new string('#', Math.Min(6, lower[1] - '0' + 2))).Append(' ');
                break;
            case "li" when !isClose:
                output.Append("\n- ");
                break;
            case "br":
                output.Append('\n');
                break;
            case "code" or "kbd":
                output.Append('`');
                break;
            case "strong" or "b":
                output.Append("**");
                break;
            case "em":
                output.Append('*');
                break;
            case "a" or "twlink" when !isClose:
                var href = attributes.GetValueOrDefault("href") ?? attributes.GetValueOrDefault("Href");
                if (string.IsNullOrWhiteSpace(href) || href.Contains('@'))
                    href = null;
                state.Links.Push(href);
                if (href is not null)
                    output.Append('[');
                break;
            case "a" or "twlink":
                if (state.Links.Count > 0 && state.Links.Pop() is { } target)
                    output.Append("](").Append(state.ResolveLink(target)).Append(')');
                break;
            case "caption":
                output.Append(isClose ? "**\n\n" : "\n\n**");
                break;
            case "dt" when !isClose:
                output.Append("\n\n**");
                break;
            case "dt":
                output.Append("**: ");
                break;
            case "table" or "twtable":
                output.Append("\n\n");
                state.InTable = !isClose;
                state.TableRow = 0;
                break;
            case "tr" when !isClose:
                output.Append("\n|");
                state.RowCells = 0;
                break;
            case "tr":
                if (state.TableRow++ == 0)
                    output.Append("\n|").Append(string.Concat(Enumerable.Repeat(" --- |", Math.Max(1, state.RowCells))));
                break;
            case "th" or "td" when !isClose:
                output.Append(' ');
                state.RowCells++;
                break;
            case "th" or "td":
                output.Append(" |");
                break;
            case "p" or "div" or "section" or "article" or "ul" or "ol" or "dl" or "dd" or "description" or "pagecard"
                or "twalert" or "twcard" or "twcollapse" or "h1" or "h2" or "h3" or "h4" or "h5" or "header" or "footer" or "main" or "nav" or "aside":
                output.Append("\n\n");
                break;
        }
    }

    private static string Decode(string text) => WebUtility.HtmlDecode(text).Replace(' ', ' ');
}
