using System.Text.Json;

namespace TwBlazor.Docs.Services;

/// <summary>
/// One entry in <c>components.json</c>: either a leaf link to a component's docs page, or a group
/// (e.g. "Date &amp; Time Pickers") whose own <see cref="Items"/> are the actual leaves.
/// </summary>
internal sealed class ComponentEntry
{
    public string Id { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;

    /// <summary>
    /// The component's TwBlazor type name (e.g. <c>"TwChip"</c>), used to link to its docfx API page.
    /// Empty for group entries, which have no single type of their own.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// The name of the theme configuration type (e.g. <c>"TwChipTheme"</c>) whose "Theme Configuration"
    /// table appears on this entry's own page - i.e. the page that shows the full table rather than an
    /// "Inherited from" note (see <c>ThemeConfigurationCard</c>'s canonical-owner handling). Empty when
    /// the entry's page has no theme table of its own, or merely reuses another page's theme, so a
    /// shared theme type (e.g. <c>TwDatePickerTheme</c> across all four date/time pickers) surfaces
    /// exactly once in search rather than once per page that happens to display it.
    /// </summary>
    public string Theme { get; set; } = string.Empty;

    public List<ComponentEntry> Items { get; set; } = [];

    /// <summary>
    /// Whether the entry is new. An entry is also new when the category or group that holds it is flagged
    /// <c>isNew</c>, so a whole section can be marked once instead of on every entry.
    /// </summary>
    public bool IsNew { get; set; }
}

/// <summary>
/// One category from <c>components.json</c> (e.g. "Forms"), grouping the components listed under it.
/// </summary>
internal sealed class ComponentCategory
{
    public string Category { get; set; } = string.Empty;

#pragma warning disable S3459, S1144 // Populated by JSON deserialization from components.json - the setter has no visible caller for Sonar's static analysis to see
    /// <summary>
    /// Whether everything in the category is new. Every entry inside it is then treated as new too.
    /// </summary>
    public bool IsNew { get; set; }
#pragma warning restore S3459, S1144

    public List<ComponentEntry> Items { get; set; } = [];
}

/// <summary>
/// Loads and flattens <c>components.json</c> - the single source of truth for both the sidebar
/// navigation (<see cref="Layout.Navigation"/>) and the component half of the search index
/// (<see cref="SearchIndex"/>), so the two can never drift apart.
/// </summary>
internal static class ComponentCatalog
{
    private static List<ComponentCategory>? categories;

    /// <summary>
    /// Returns the raw category/entry tree exactly as authored in <c>components.json</c>.
    /// </summary>
    public static List<ComponentCategory> LoadCategories() => categories ??= ReadEmbeddedJson();

    /// <summary>
    /// Returns every leaf entry (an actual component with a docs page) across all categories,
    /// depth-first - group entries like "Date &amp; Time Pickers" are expanded rather than returned
    /// themselves, since they have no page or type of their own.
    /// </summary>
    public static IEnumerable<(string Category, ComponentEntry Entry)> LoadLeafEntries()
    {
        foreach (var category in LoadCategories())
        {
            foreach (var entry in Flatten(category.Items))
            {
                yield return (category.Category, entry);
            }
        }
    }

    private static IEnumerable<ComponentEntry> Flatten(IEnumerable<ComponentEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (entry.Items.Count > 0)
            {
                foreach (var child in Flatten(entry.Items))
                {
                    yield return child;
                }
            }
            else
            {
                yield return entry;
            }
        }
    }

    private static List<ComponentCategory> ReadEmbeddedJson()
    {
        var assembly = typeof(ComponentCatalog).Assembly;
        using var stream = assembly.GetManifestResourceStream("TwBlazor.Docs.components.json")
            ?? throw new InvalidOperationException("Embedded resource 'components.json' was not found.");

        var loaded = JsonSerializer.Deserialize<List<ComponentCategory>>(stream, JsonSerializerOptions.Web) ?? [];

        foreach (var category in loaded)
        {
            InheritIsNew(category.Items, category.IsNew);
        }

        return loaded;
    }

    private static void InheritIsNew(List<ComponentEntry> entries, bool parentIsNew)
    {
        foreach (var entry in entries)
        {
            entry.IsNew |= parentIsNew;
            InheritIsNew(entry.Items, entry.IsNew);
        }
    }
}
