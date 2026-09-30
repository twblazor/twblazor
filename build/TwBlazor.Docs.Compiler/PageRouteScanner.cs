using System.Text.RegularExpressions;

namespace TwBlazor.Docs.Compiler;

/// <summary>
/// A routable, indexable docs page discovered by <see cref="PageRouteScanner"/>.
/// </summary>
/// <param name="Route">The page's route template, e.g. <c>/card</c>.</param>
/// <param name="SourcePath">The absolute path of the <c>.razor</c> file that declares the route.</param>
public sealed record PageRoute(string Route, string SourcePath);

/// <summary>
/// Discovers the docs pages that belong in the sitemap by reading their <c>@page</c> directives,
/// so the sitemap can never drift from the pages that actually exist.
/// </summary>
public static partial class PageRouteScanner
{
    /// <summary>
    /// Returns every static, indexable route declared under <paramref name="pagesPath"/>, ordered with
    /// the home page first and the rest alphabetically.
    /// </summary>
    /// <remarks>
    /// Pages carrying a <c>noindex</c> robots meta tag (e.g. the sidebar preview frames) and parameterised
    /// routes are skipped. Only a <c>@page</c> directive at the very start of a line counts, so a
    /// <c>@page</c> shown inside a code sample is never mistaken for a real route.
    /// </remarks>
    /// <param name="pagesPath">The docs project's <c>Pages</c> directory.</param>
    /// <returns>The discovered routes, one per distinct route template.</returns>
    public static IReadOnlyList<PageRoute> Scan(string pagesPath)
    {
        var routes = new Dictionary<string, PageRoute>(StringComparer.Ordinal);

        foreach (var file in Directory.EnumerateFiles(pagesPath, "*.razor", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            if (NoIndexPattern().IsMatch(text))
            {
                continue;
            }

            foreach (Match match in PageDirectivePattern().Matches(text))
            {
                var route = match.Groups["route"].Value;
                if (!route.Contains('{'))
                {
                    routes.TryAdd(route, new PageRoute(route, file));
                }
            }
        }

        return [.. routes.Values.OrderBy(static r => r.Route == "/" ? 0 : 1).ThenBy(static r => r.Route, StringComparer.Ordinal)];
    }

    [GeneratedRegex("""^@page\s+"(?<route>[^"]+)"[ \t]*\r?$""", RegexOptions.Multiline)]
    private static partial Regex PageDirectivePattern();

    [GeneratedRegex("""<meta\s+name="robots"\s+content="[^"]*noindex""", RegexOptions.IgnoreCase)]
    private static partial Regex NoIndexPattern();
}
