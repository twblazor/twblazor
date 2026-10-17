using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Compiler;

/// <summary>
/// The files written for AI assistants: the <c>llms.txt</c> index, the all-in-one <c>llms-full.txt</c> and
/// one Markdown file per docs page.
/// </summary>
/// <param name="LlmsTxt">The <c>llms.txt</c> index.</param>
/// <param name="LlmsFullTxt">The index followed by every page in full.</param>
/// <param name="Pages">Each page's Markdown keyed by its path relative to <c>wwwroot</c>, e.g. <c>chart/area.md</c>.</param>
public sealed record LlmsOutput(string LlmsTxt, string LlmsFullTxt, IReadOnlyDictionary<string, string> Pages);

/// <summary>
/// Builds <see cref="LlmsOutput"/> from the docs pages, <c>components.json</c> and the library source, so the
/// files AI assistants read can never drift from the docs people read.
/// </summary>
/// <remarks>
/// <c>llms.txt</c> starts from a hand-written template (the summary, the rules and the setup steps) and
/// fills in <c>{{version}}</c> from the Get Started install command, <c>{{docs}}</c> with the guide pages and
/// <c>{{components}}</c> with every entry in <c>components.json</c>, described by its page's own description.
/// </remarks>
public static partial class LlmsGenerator
{
    /// <summary>
    /// The routes left out of the Markdown output: the home page has no docs content and the Theme builder is
    /// an interactive tool, so the template links to it directly.
    /// </summary>
    public static readonly IReadOnlySet<string> ExcludedRoutes = new HashSet<string>(StringComparer.Ordinal) { "/", "/theme" };

    private const string getStartedRoute = "/get-started";

    private sealed record CatalogEntry(string Display, string Name, string Url);

    private sealed record CatalogCategory(string Name, IReadOnlyList<CatalogEntry> Entries);

    [GeneratedRegex(@"dotnet add package twblazor --version (?<version>[^\s`""<]+)", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex InstallVersionRegex();

    /// <summary>
    /// Generates every file.
    /// </summary>
    /// <param name="template">The <c>llms.txt</c> template.</param>
    /// <param name="componentsJson">The text of <c>components.json</c>.</param>
    /// <param name="pages">Each docs page's route and <c>.razor</c> source.</param>
    /// <param name="examples">The extracted code samples by name.</param>
    /// <param name="components">The library's components from <see cref="ComponentParameterScanner.Scan"/>.</param>
    /// <param name="baseUrl">The site origin without a trailing slash.</param>
    public static LlmsOutput Generate(
        string template,
        string componentsJson,
        IReadOnlyList<(string Route, string Source)> pages,
        IReadOnlyDictionary<string, string> examples,
        IReadOnlyDictionary<string, ComponentInfo> components,
        string baseUrl = SitemapGenerator.DefaultBaseUrl)
    {
        var origin = baseUrl.TrimEnd('/');
        var catalog = ReadCatalog(componentsJson);
        var componentByRoute = catalog.SelectMany(c => c.Entries).GroupBy(e => e.Url, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First().Name, StringComparer.Ordinal);
        var documented = pages.Where(p => !ExcludedRoutes.Contains(p.Route)).ToList();
        var markdownRoutes = documented.Select(p => p.Route).ToHashSet(StringComparer.Ordinal);

        string ResolveLink(string route, string href)
        {
            if (href.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || href.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                return href;

            if (href.StartsWith('#'))
                return origin + route + href;

            var path = href.StartsWith('/') ? href : "/" + href;
            return markdownRoutes.Contains(path) ? origin + MarkdownPath(path) : origin + path;
        }

        var contents = new Dictionary<string, RazorPageContent>(StringComparer.Ordinal);
        var markdown = new SortedDictionary<string, string>(StringComparer.Ordinal);

        foreach (var (route, source) in documented)
        {
            var content = RazorMarkdownConverter.Convert(source, examples, href => ResolveLink(route, href));
            if (content.Title.Length == 0)
                continue;

            var primary = componentByRoute.TryGetValue(route, out var name) && name.Length > 0 ? name : components.ContainsKey(content.Title) ? content.Title : null;
            contents[route] = content;
            markdown[MarkdownPath(route).TrimStart('/')] = RenderPage(route, content, primary, components, origin);
        }

        var version = pages.Where(p => p.Route == getStartedRoute).Select(p => InstallVersionRegex().Match(p.Source)).FirstOrDefault(m => m.Success)?.Groups["version"].Value
            ?? throw new InvalidOperationException("No 'dotnet add package twblazor --version <version>' line found on the Get Started page.");

        var llms = template.Replace("\r\n", "\n")
            .Replace("{{version}}", version, StringComparison.Ordinal)
            .Replace("{{docs}}", RenderDocsList(contents, componentByRoute, origin), StringComparison.Ordinal)
            .Replace("{{components}}", RenderComponentList(catalog, contents, origin), StringComparison.Ordinal)
            .TrimEnd() + "\n";

        var full = new StringBuilder(llms);
        foreach (var page in markdown.Values)
            full.Append("\n---\n\n").Append(page);

        return new LlmsOutput(llms, full.ToString(), markdown);
    }

    /// <summary>
    /// Returns the Markdown file's URL path for a page route, e.g. <c>/chart/area</c> becomes <c>/chart/area.md</c>.
    /// </summary>
    /// <param name="route">The page route.</param>
    public static string MarkdownPath(string route) => route.TrimEnd('/') + ".md";

    private static string RenderPage(string route, RazorPageContent content, string? primary, IReadOnlyDictionary<string, ComponentInfo> components, string origin)
    {
        var sb = new StringBuilder();
        sb.Append("# ").Append(content.Title).Append("\n\n");

        if (content.Description.Length > 0)
            sb.Append("> ").Append(content.Description).Append("\n\n");

        sb.Append("Documentation page: ").Append(origin).Append(route).Append("\n\n");

        if (content.Body.Length > 0)
            sb.Append(content.Body).Append("\n\n");

        AppendKeyboard(sb, route);

        if (primary is not null && components.TryGetValue(primary, out var primaryInfo))
            AppendParameters(sb, primaryInfo, content.ExampleComponents, components);

        if (content.ThemeType.Length > 0)
        {
            sb.Append("## Theme\n\n")
                .Append("Styled by `").Append(content.ThemeType).Append("`. Include it by ticking this component on the Theme builder (")
                .Append(origin).Append("/theme) and replacing `Theme.cs` with the newly generated file. Never write or edit `")
                .Append(content.ThemeType).Append("` by hand.\n\n");
        }

        return sb.ToString().TrimEnd() + "\n";
    }

    private static void AppendKeyboard(StringBuilder sb, string route)
    {
        if (KeyboardShortcuts.ForPath(route) is not { } guide)
            return;

        sb.Append("## Keyboard navigation\n\n").Append(guide.Summary).Append("\n\n");

        if (guide.Shortcuts.Count == 0)
            return;

        sb.Append("| Key | What it does |\n| --- | --- |\n");
        foreach (var shortcut in guide.Shortcuts)
            sb.Append("| ").Append(string.Join(" or ", shortcut.Keys.Select(k => $"`{k}`"))).Append(" | ").Append(Cell(shortcut.Action)).Append(" |\n");

        sb.Append('\n');
    }

    /// <summary>
    /// Lists the parameters of the page's component and of the companions its examples use, the other
    /// components declared in the same folder (such as <c>TwButtonGroup</c> beside <c>TwButton</c>).
    /// </summary>
    private static void AppendParameters(StringBuilder sb, ComponentInfo primary, IReadOnlyList<string> exampleComponents, IReadOnlyDictionary<string, ComponentInfo> components)
    {
        var companions = exampleComponents
            .Where(n => n != primary.Name && components.TryGetValue(n, out var info) && string.Equals(info.Folder, primary.Folder, StringComparison.OrdinalIgnoreCase))
            .Select(n => components[n]);

        sb.Append("## Parameters\n\n")
            .Append("This is every parameter the component accepts. Do not use parameters that are not listed here.\n\n");

        foreach (var component in new[] { primary }.Concat(companions))
        {
            sb.Append("### ").Append(component.Name).Append("\n\n");

            if (component.Summary.Length > 0)
                sb.Append(component.Summary).Append("\n\n");

            if (component.Parameters.Count == 0)
            {
                sb.Append("No parameters.\n\n");
                continue;
            }

            sb.Append("| Parameter | Type | Description |\n| --- | --- | --- |\n");
            foreach (var parameter in component.Parameters)
                sb.Append("| `").Append(parameter.Name).Append("` | `").Append(Cell(parameter.Type)).Append("` | ").Append(Cell(parameter.Description)).Append(" |\n");

            sb.Append('\n');
        }
    }

    private static string RenderDocsList(Dictionary<string, RazorPageContent> contents, Dictionary<string, string> componentByRoute, string origin)
    {
        var sb = new StringBuilder("## Documentation\n\n");
        sb.Append("- [Theme builder](").Append(origin).Append("/theme): pick brand colours, surfaces, defaults and components, then generate `Theme.cs`. The only supported way to create or change a theme. It is an interactive page, so it has no Markdown version.\n");

        foreach (var (route, content) in contents.Where(c => !componentByRoute.ContainsKey(c.Key)).OrderBy(c => c.Key == getStartedRoute ? 0 : 1).ThenBy(c => c.Key, StringComparer.Ordinal))
            sb.Append("- [").Append(content.Title).Append("](").Append(origin).Append(MarkdownPath(route)).Append("): ").Append(content.Description).Append('\n');

        return sb.ToString().TrimEnd();
    }

    private static string RenderComponentList(IReadOnlyList<CatalogCategory> catalog, Dictionary<string, RazorPageContent> contents, string origin)
    {
        var sb = new StringBuilder();

        foreach (var category in catalog)
        {
            sb.Append("## ").Append(category.Name).Append("\n\n");

            foreach (var entry in category.Entries)
            {
                var link = contents.ContainsKey(entry.Url) ? origin + MarkdownPath(entry.Url) : origin + entry.Url;
                sb.Append("- [").Append(entry.Display).Append("](").Append(link).Append(')');

                if (entry.Name.Length > 0)
                    sb.Append(" `").Append(entry.Name).Append('`');

                if (contents.TryGetValue(entry.Url, out var content) && content.Description.Length > 0)
                    sb.Append(": ").Append(content.Description);

                sb.Append('\n');
            }

            sb.Append('\n');
        }

        return sb.ToString().TrimEnd();
    }

    private static List<CatalogCategory> ReadCatalog(string componentsJson)
    {
        using var document = JsonDocument.Parse(componentsJson);
        var categories = new List<CatalogCategory>();

        foreach (var category in document.RootElement.EnumerateArray())
        {
            var entries = new List<CatalogEntry>();
            Flatten(category.GetProperty("items"), null, entries);
            categories.Add(new CatalogCategory(category.GetProperty("category").GetString() ?? string.Empty, entries));
        }

        return categories;
    }

    private static void Flatten(JsonElement items, string? group, List<CatalogEntry> entries)
    {
        foreach (var item in items.EnumerateArray())
        {
            var display = item.TryGetProperty("display", out var d) ? d.GetString() ?? string.Empty : string.Empty;

            if (item.TryGetProperty("items", out var children))
            {
                Flatten(children, display, entries);
                continue;
            }

            var url = item.TryGetProperty("url", out var u) ? u.GetString() ?? string.Empty : string.Empty;
            var name = item.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty;
            entries.Add(new CatalogEntry(group is null ? display : $"{group}: {display}", name, url));
        }
    }

    private static string Cell(string text) => text.Replace("|", "\\|", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal);
}
