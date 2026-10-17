using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Compiler;

/// <summary>
/// An agent skill in the Agent Skills format: a folder holding a <c>SKILL.md</c> and the reference pages it points to.
/// </summary>
/// <param name="Name">The skill's name, which is also its folder name, e.g. <c>twblazor-setup</c>.</param>
/// <param name="Description">What the skill does and when to use it, as written in its frontmatter.</param>
/// <param name="Files">Each file's text keyed by its path inside the skill folder, e.g. <c>references/button.md</c>.</param>
public sealed record AgentSkill(string Name, string Description, IReadOnlyDictionary<string, string> Files);

/// <summary>
/// The files written for AI assistants: the <c>llms.txt</c> index, the all-in-one <c>llms-full.txt</c>, one
/// Markdown file per docs page and the agent skills.
/// </summary>
/// <param name="LlmsTxt">The <c>llms.txt</c> index.</param>
/// <param name="LlmsFullTxt">The index followed by every page in full.</param>
/// <param name="Pages">Each page's Markdown keyed by its path relative to <c>wwwroot</c>, e.g. <c>chart/area.md</c>.</param>
/// <param name="Skills">The agent skills, in name order.</param>
public sealed record LlmsOutput(string LlmsTxt, string LlmsFullTxt, IReadOnlyDictionary<string, string> Pages, IReadOnlyList<AgentSkill> Skills);

/// <summary>
/// The hand-written inputs: the <c>llms.txt</c> template, the partials it shares with the skills and one
/// template per skill.
/// </summary>
/// <param name="LlmsTemplate">The <c>llms.txt</c> template.</param>
/// <param name="Partials">Shared text by name, inserted with <c>{{include:name}}</c>.</param>
/// <param name="SkillTemplates">Each skill's template keyed by the skill name, starting with its frontmatter.</param>
public sealed record LlmsTemplates(string LlmsTemplate, IReadOnlyDictionary<string, string> Partials, IReadOnlyDictionary<string, string> SkillTemplates);

/// <summary>
/// Builds <see cref="LlmsOutput"/> from the docs pages, <c>components.json</c> and the library source, so the
/// files AI assistants read can never drift from the docs people read.
/// </summary>
/// <remarks>
/// Every template may use <c>{{include:name}}</c>, <c>{{link:/route}}</c>, <c>{{version}}</c> (from the Get
/// Started install command), <c>{{components}}</c> (every entry in <c>components.json</c>, described by its
/// page's own description) and <c>{{themes}}</c> (the theme type of each component). <c>llms.txt</c> also has
/// <c>{{docs}}</c> (the guide pages) and <c>{{skills}}</c>. A link points at the skill's own
/// <c>references/</c> copy when the page is bundled with the skill, and at the site otherwise. A skill's
/// frontmatter names its pages with <c>references:</c>, a comma separated list of routes or <c>components</c>
/// for every component page.
/// </remarks>
public static partial class LlmsGenerator
{
    /// <summary>
    /// The routes left out of the Markdown output: the home page has no docs content and the Theme builder is
    /// an interactive tool, so the templates link to it directly.
    /// </summary>
    public static readonly IReadOnlySet<string> ExcludedRoutes = new HashSet<string>(StringComparer.Ordinal) { "/", "/theme" };

    /// <summary>
    /// The name of the zip holding every skill, served as <c>/skills/twblazor-skills.zip</c>.
    /// </summary>
    public const string SkillsBundleName = "twblazor-skills";

    /// <summary>
    /// The longest skill description the Agent Skills format allows.
    /// </summary>
    public const int MaxSkillDescriptionLength = 1024;

    private const string getStartedRoute = "/get-started";
    private const string componentsReference = "components";

    private sealed record CatalogEntry(string Display, string Name, string Url);

    private sealed record CatalogCategory(string Name, IReadOnlyList<CatalogEntry> Entries);

    private sealed record SiteContent(
        string Origin,
        string Version,
        IReadOnlyList<CatalogCategory> Catalog,
        IReadOnlyDictionary<string, RazorPageContent> Contents,
        IReadOnlyDictionary<string, string> ComponentByRoute,
        IReadOnlyDictionary<string, string> Partials);

    [GeneratedRegex(@"dotnet add package twblazor --version (?<version>[^\s`""<]+)", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex InstallVersionRegex();

    [GeneratedRegex(@"\{\{include:(?<name>[\w-]+)\}\}", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex IncludeRegex();

    [GeneratedRegex(@"\{\{link:(?<route>/[\w\-/]*)\}\}", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LinkRegex();

    [GeneratedRegex(@"\{\{[^{}]*\}\}", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PlaceholderRegex();

    [GeneratedRegex(@"\A---\r?\n(?<frontmatter>.*?)\r?\n---\r?\n(?<body>.*)\z", RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FrontmatterRegex();

    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SkillNameRegex();

    /// <summary>
    /// Generates every file.
    /// </summary>
    /// <param name="templates">The hand-written templates.</param>
    /// <param name="componentsJson">The text of <c>components.json</c>.</param>
    /// <param name="pages">Each docs page's route and <c>.razor</c> source.</param>
    /// <param name="examples">The extracted code samples by name.</param>
    /// <param name="components">The library's components from <see cref="ComponentParameterScanner.Scan"/>.</param>
    /// <param name="baseUrl">The site origin without a trailing slash.</param>
    public static LlmsOutput Generate(
        LlmsTemplates templates,
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

        string SiteLink(string route) => markdownRoutes.Contains(route) ? origin + MarkdownPath(route) : origin + route;

        string ResolveHref(string route, string href)
        {
            if (href.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || href.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                return href;

            return href.StartsWith('#') ? origin + route + href : SiteLink(href.StartsWith('/') ? href : "/" + href);
        }

        var contents = new Dictionary<string, RazorPageContent>(StringComparer.Ordinal);
        var markdown = new SortedDictionary<string, string>(StringComparer.Ordinal);

        foreach (var (route, source) in documented)
        {
            var content = RazorMarkdownConverter.Convert(source, examples, href => ResolveHref(route, href));
            if (content.Title.Length == 0)
                continue;

            var primary = componentByRoute.TryGetValue(route, out var name) && name.Length > 0 ? name : components.ContainsKey(content.Title) ? content.Title : null;
            contents[route] = content;
            markdown[PagePath(route)] = RenderPage(route, content, primary, components, origin);
        }

        var version = pages.Where(p => p.Route == getStartedRoute).Select(p => InstallVersionRegex().Match(p.Source)).FirstOrDefault(m => m.Success)?.Groups["version"].Value
            ?? throw new InvalidOperationException("No 'dotnet add package twblazor --version <version>' line found on the Get Started page.");

        var site = new SiteContent(origin, version, catalog, contents, componentByRoute, templates.Partials);

        var skills = templates.SkillTemplates
            .OrderBy(t => t.Key, StringComparer.Ordinal)
            .Select(t => BuildSkill(t.Key, t.Value, site, markdown, SiteLink))
            .ToList();

        var llms = Render(templates.LlmsTemplate, site, SiteLink, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["docs"] = RenderDocsList(site, SiteLink),
            ["skills"] = RenderSkillList(skills, origin),
        });

        var full = new StringBuilder(llms);
        foreach (var page in markdown.Values)
            full.Append("\n---\n\n").Append(page);

        return new LlmsOutput(llms, full.ToString(), markdown, skills);
    }

    /// <summary>
    /// Returns the Markdown file's URL path for a page route, e.g. <c>/chart/area</c> becomes <c>/chart/area.md</c>.
    /// </summary>
    /// <param name="route">The page route.</param>
    public static string MarkdownPath(string route) => route.TrimEnd('/') + ".md";

    private static string PagePath(string route) => MarkdownPath(route).TrimStart('/');

    private static AgentSkill BuildSkill(string name, string template, SiteContent site, IReadOnlyDictionary<string, string> markdown, Func<string, string> siteLink)
    {
        var parts = FrontmatterRegex().Match(template.Replace("\r\n", "\n"));
        if (!parts.Success)
            throw new InvalidOperationException($"Skill '{name}' must start with a --- frontmatter block.");

        var frontmatter = parts.Groups["frontmatter"].Value.Split('\n')
            .Select(line => line.Split(':', 2))
            .Where(pair => pair.Length == 2)
            .ToDictionary(pair => pair[0].Trim(), pair => pair[1].Trim(), StringComparer.Ordinal);

        var description = frontmatter.GetValueOrDefault("description", string.Empty);

        if (frontmatter.GetValueOrDefault("name") != name || !SkillNameRegex().IsMatch(name) || name.Length > 64)
            throw new InvalidOperationException($"Skill '{name}' must declare 'name: {name}', in lowercase letters, numbers and hyphens of at most 64 characters, matching its file name.");

        if (description.Length == 0 || description.Length > MaxSkillDescriptionLength || description.IndexOfAny(['<', '>']) >= 0)
            throw new InvalidOperationException($"Skill '{name}' needs a description of 1 to {MaxSkillDescriptionLength} characters without angle brackets.");

        var references = frontmatter.GetValueOrDefault("references", string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .SelectMany(reference => reference == componentsReference ? site.Catalog.SelectMany(c => c.Entries).Select(e => e.Url) : [reference])
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var files = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var route in references)
        {
            if (!markdown.TryGetValue(PagePath(route), out var page))
                throw new InvalidOperationException($"Skill '{name}' references '{route}', which has no docs page.");

            files["references/" + PagePath(route)] = page;
        }

        var bundled = references.ToHashSet(StringComparer.Ordinal);
        string SkillLink(string route) => bundled.Contains(route) ? "references" + MarkdownPath(route) : siteLink(route);

        var body = Render(parts.Groups["body"].Value, site, SkillLink, new Dictionary<string, string>(StringComparer.Ordinal));
        var quotedDescription = "\"" + description.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";
        files["SKILL.md"] = $"---\nname: {name}\ndescription: {quotedDescription}\n---\n\n{body}";

        return new AgentSkill(name, description, files);
    }

    /// <summary>
    /// Fills a template. An unknown placeholder throws, so a typo cannot reach an assistant as literal text.
    /// </summary>
    private static string Render(string template, SiteContent site, Func<string, string> link, IReadOnlyDictionary<string, string> extra)
    {
        var text = template.Replace("\r\n", "\n");

        for (var depth = 0; depth < 5 && IncludeRegex().IsMatch(text); depth++)
        {
            text = IncludeRegex().Replace(text, m => site.Partials.TryGetValue(m.Groups["name"].Value, out var partial)
                ? partial.Replace("\r\n", "\n").TrimEnd()
                : throw new InvalidOperationException($"Unknown partial '{m.Groups["name"].Value}'."));
        }

        text = LinkRegex().Replace(text, m => link(m.Groups["route"].Value));

        var values = new Dictionary<string, Func<string>>(StringComparer.Ordinal)
        {
            ["version"] = () => site.Version,
            ["components"] = () => RenderComponentList(site, link),
            ["themes"] = () => RenderThemeTable(site),
        };

        foreach (var (key, value) in extra)
            values[key] = () => value;

        text = PlaceholderRegex().Replace(text, m => values.TryGetValue(m.Value[2..^2], out var value)
            ? value()
            : throw new InvalidOperationException($"Unknown placeholder '{m.Value}'."));

        return text.TrimEnd() + "\n";
    }

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

    private static string RenderDocsList(SiteContent site, Func<string, string> link)
    {
        var sb = new StringBuilder("## Documentation\n\n");
        sb.Append("- [Theme builder](").Append(site.Origin).Append("/theme): pick brand colours, surfaces, defaults and components, then generate `Theme.cs`. The only supported way to create or change a theme. It is an interactive page, so it has no Markdown version.\n");

        foreach (var (route, content) in site.Contents.Where(c => !site.ComponentByRoute.ContainsKey(c.Key)).OrderBy(c => c.Key == getStartedRoute ? 0 : 1).ThenBy(c => c.Key, StringComparer.Ordinal))
            sb.Append("- [").Append(content.Title).Append("](").Append(link(route)).Append("): ").Append(content.Description).Append('\n');

        return sb.ToString().TrimEnd();
    }

    private static string RenderSkillList(IReadOnlyList<AgentSkill> skills, string origin)
    {
        if (skills.Count == 0)
            return string.Empty;

        var sb = new StringBuilder("## Agent skills\n\n");
        sb.Append("The same guidance packaged as Agent Skills. Download [every skill in one zip](").Append(origin).Append("/skills/").Append(SkillsBundleName).Append(".zip) and extract it into `.claude/skills/`, or read a skill directly:\n\n");

        foreach (var skill in skills)
            sb.Append("- [").Append(skill.Name).Append("](").Append(origin).Append("/skills/").Append(skill.Name).Append("/SKILL.md): ").Append(skill.Description).Append('\n');

        return sb.ToString().TrimEnd();
    }

    private static string RenderComponentList(SiteContent site, Func<string, string> link)
    {
        var sb = new StringBuilder();

        foreach (var category in site.Catalog)
        {
            sb.Append("## ").Append(category.Name).Append("\n\n");

            foreach (var entry in category.Entries)
            {
                sb.Append("- [").Append(entry.Display).Append("](").Append(link(entry.Url)).Append(')');

                if (entry.Name.Length > 0)
                    sb.Append(" `").Append(entry.Name).Append('`');

                if (site.Contents.TryGetValue(entry.Url, out var content) && content.Description.Length > 0)
                    sb.Append(": ").Append(content.Description);

                sb.Append('\n');
            }

            sb.Append('\n');
        }

        return sb.ToString().TrimEnd();
    }

    private static string RenderThemeTable(SiteContent site)
    {
        var sb = new StringBuilder("| Component | Theme type |\n| --- | --- |\n");

        foreach (var entry in site.Catalog.SelectMany(c => c.Entries))
        {
            if (entry.Name.Length > 0 && site.Contents.TryGetValue(entry.Url, out var content) && content.ThemeType.Length > 0)
                sb.Append("| `").Append(entry.Name).Append("` | `").Append(content.ThemeType).Append("` |\n");
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
