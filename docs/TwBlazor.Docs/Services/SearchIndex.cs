using TwBlazor.Docs.Layout;

namespace TwBlazor.Docs.Services;

/// <summary>
/// Which part of the search index a <see cref="SearchResult"/> came from, so results can be grouped
/// and ordered: components in this library, then their theme configuration tables, then external
/// docfx class documentation.
/// </summary>
internal enum SearchResultKind
{
    Component,
    Theme,
    Documentation
}

/// <summary>
/// One search result: a component's docs page, the page section documenting a theme class, or a
/// class's page on the docfx API site.
/// </summary>
internal sealed record SearchResult(string Title, string Subtitle, string Url, SearchResultKind Kind)
{
    public bool IsExternal => Kind == SearchResultKind.Documentation;
}

/// <summary>
/// Builds and searches the docs site's search index: every component page from <c>components.json</c>,
/// the theme configuration table each of those pages documents, and a docfx API page for each
/// component's underlying type.
/// </summary>
/// <remarks>
/// The index is built once from <see cref="ComponentCatalog"/> and cached - <c>components.json</c> only
/// changes at build time, so there is nothing to invalidate at runtime.
/// </remarks>
internal static class SearchIndex
{
    // The docfx site is published from the repo root under this sub-path (see docfx.json / deploy-docs.yml).
#pragma warning disable S1075 // Fixed docfx publish location, not environment-specific
    private const string apiBaseUrl = "https://twblazor.github.io/twblazor/";
#pragma warning restore S1075

    // Every documented component's control type lives in this namespace (see components.json's "name"
    // field, e.g. "TwChip" -> TwBlazor.Components.TwChip).
    private const string componentNamespace = "TwBlazor.Components";

    // Theme configuration classes (e.g. TwChipTheme) live in this namespace instead - see
    // components.json's "theme" field.
    private const string themeNamespace = "TwBlazor.Configuration.Components";

    private static IReadOnlyList<SearchResult>? components;
    private static IReadOnlyList<SearchResult>? themes;
    private static IReadOnlyList<SearchResult>? documentation;

    /// <summary>
    /// Searches the index for <paramref name="term"/>, matching case-insensitively against each
    /// result's title. Returns components, then their theme configuration, then docfx documentation -
    /// the order search results should be grouped and displayed in.
    /// </summary>
    public static (IReadOnlyList<SearchResult> Components, IReadOnlyList<SearchResult> Themes, IReadOnlyList<SearchResult> Documentation) Search(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return ([], [], []);
        }

        components ??= BuildComponents();
        themes ??= BuildThemes();
        documentation ??= BuildDocumentation();

        return (
            [.. components.Where(r => Matches(r, term))],
            [.. themes.Where(r => Matches(r, term))],
            [.. documentation.Where(r => Matches(r, term))]);
    }

    private static bool Matches(SearchResult result, string term) =>
        result.Title.Contains(term, StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyList<SearchResult> BuildComponents() =>
        [.. ComponentCatalog.LoadLeafEntries()
            .Select(entry => new SearchResult(entry.Entry.Display, entry.Category, entry.Entry.Url, SearchResultKind.Component))];

    // Only one result per theme type, even though components.json may list the same "theme" value on
    // several entries in principle - each theme is recorded only against its canonical page (the one
    // whose ThemeConfigurationCard shows the full table rather than an "Inherited from" note), so this
    // never actually needs to de-duplicate, but doesn't rely on that either.
    private static List<SearchResult> BuildThemes()
    {
        var assembly = typeof(TwBlazor.Components.TwCard).Assembly;
        var results = new List<SearchResult>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (_, entry) in ComponentCatalog.LoadLeafEntries())
        {
            if (string.IsNullOrEmpty(entry.Theme) || !seen.Add(entry.Theme))
            {
                continue;
            }

            var type = assembly.GetType($"{themeNamespace}.{entry.Theme}");
            if (type is null)
            {
                continue;
            }

            var url = $"{entry.Url}#{ThemeConfigurationCard.AnchorId}";
            results.Add(new SearchResult(entry.Theme, "Theme Configuration", url, SearchResultKind.Theme));
        }

        return results;
    }

    // Only links to types that actually resolve in the TwBlazor assembly - components.json's "name"
    // field is hand-authored, so a typo or a not-yet-documented entry is skipped rather than shipping
    // a dead docfx link.
    private static List<SearchResult> BuildDocumentation()
    {
        var assembly = typeof(TwBlazor.Components.TwCard).Assembly;
        var results = new List<SearchResult>();

        foreach (var (_, entry) in ComponentCatalog.LoadLeafEntries())
        {
            if (string.IsNullOrEmpty(entry.Name))
            {
                continue;
            }

            var type = assembly.GetType($"{componentNamespace}.{entry.Name}");
            if (type is null)
            {
                continue;
            }

            var url = $"{apiBaseUrl}api/{type.FullName}.html";
            results.Add(new SearchResult(entry.Name, "API Documentation", url, SearchResultKind.Documentation));
        }

        return results;
    }
}
