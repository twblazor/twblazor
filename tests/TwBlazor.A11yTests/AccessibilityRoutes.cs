using System.Reflection;
using System.Text.Json;

namespace TwBlazor.A11yTests;

/// <summary>
/// Every routable page in TwBlazor.Docs (one demo page per component, showing every color/state
/// variant), paired with whether a dark-mode pass should also be scanned. The two sidebar preview
/// routes always force light mode (see TwBlazor.Docs' themeToggle.js `isPreviewPage`), so a dark
/// pass there wouldn't reflect anything a real user can reach.
/// </summary>
public static class AccessibilityRoutes
{
    /// <summary>
    /// The number of <c>AccessibilityScanShard*</c> classes the scans are split across.
    /// </summary>
    public const int ShardCount = 4;

    public static TheoryData<string, bool> LightAndDark => ToTheoryData(AllScans());

    /// <summary>
    /// Gets one shard's share of the scans. They are dealt out in turn, so every shard gets a similar number of
    /// pages and a similar mix of light and dark ones.
    /// </summary>
    /// <param name="index">The shard, from 0 to <see cref="ShardCount"/> - 1.</param>
    public static TheoryData<string, bool> Shard(int index) =>
        ToTheoryData(AllScans().Where((_, position) => position % ShardCount == index));

    private static IEnumerable<(string Route, bool Dark)> AllScans()
    {
        foreach (var route in _all)
        {
            yield return (route, false);
            if (!_previewRoutes.Contains(route))
            {
                yield return (route, true);
            }
        }
    }

    private static TheoryData<string, bool> ToTheoryData(IEnumerable<(string Route, bool Dark)> scans)
    {
        var data = new TheoryData<string, bool>();
        foreach (var (route, dark) in scans)
        {
            data.Add(route, dark);
        }
        return data;
    }

    private static readonly string[] _previewRoutes =
    [
        "/sidebar/preview",
        "/sidebar/preview-navigation",
        "/sidebar/preview-item-content",
    ];

    // Pages that are not in the component catalogue. The home page is the first one anybody sees.
    private static readonly string[] _sitePages =
    [
        "/",
        "/get-started",
        "/tailwind-blazor",
        "/why-twblazor",
        "/theme",
        "/class-merge",
        "/localization",
    ];

    private static readonly string[] _all = BuildAllRoutes();

    private static string[] BuildAllRoutes()
    {
        List<string> routes = [.. _previewRoutes, .. _sitePages, .. LoadComponentRoutes()];
        return [.. routes.OrderBy(route => route, StringComparer.Ordinal)];
    }

    private static IEnumerable<string> LoadComponentRoutes()
    {
        var docsAssembly = Assembly.Load("TwBlazor.Docs");
        using var stream = docsAssembly.GetManifestResourceStream("TwBlazor.Docs.components.json")
            ?? throw new InvalidOperationException("Embedded resource 'components.json' was not found.");

        var categories = JsonSerializer.Deserialize<List<ComponentCategory>>(stream, JsonSerializerOptions.Web) ?? [];
        return categories.SelectMany(c => c.Items).SelectMany(FlattenRoutes);
    }

    // An entry with nested Items (e.g. "Dates & Time" grouping the date/time pickers under "Forms")
    // has no Url of its own, so it recurses into its children instead - to any nesting depth
    // components.json uses - rather than yielding an empty route for the group itself.
    private static IEnumerable<string> FlattenRoutes(ComponentEntry entry) =>
        entry.Items.Count > 0 ? entry.Items.SelectMany(FlattenRoutes) : [entry.Url];

    private sealed class ComponentCategory
    {
        public List<ComponentEntry> Items { get; set; } = [];
    }

    private sealed class ComponentEntry
    {
        public string Url { get; set; } = string.Empty;
        public List<ComponentEntry> Items { get; set; } = [];
    }
}
