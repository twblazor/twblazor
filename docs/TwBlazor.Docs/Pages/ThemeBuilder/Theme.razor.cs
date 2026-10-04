using TwBlazor.Docs.Services;
using TwBlazor.Docs.Services.ThemeBuilder;

namespace TwBlazor.Docs.Pages.ThemeBuilder;

/// <summary>
/// The theme builder page: pick colors and components, preview the result and copy the generated <c>Theme.cs</c>.
/// </summary>
public partial class Theme
{
    /// <summary>
    /// One selectable component.
    /// </summary>
    /// <param name="Name">The component type name, e.g. <c>TwChip</c>.</param>
    /// <param name="Display">The name shown next to its checkbox.</param>
    private sealed record ComponentOption(string Name, string Display);

    /// <summary>
    /// A category of components with a checkbox that toggles them all.
    /// </summary>
    /// <param name="Label">The category name.</param>
    /// <param name="Components">The category's components.</param>
    private sealed record ComponentGroup(string Label, IReadOnlyList<ComponentOption> Components);

    // A container query, so the light and dark panes sit side by side whenever the card is wide enough (36rem),
    // however much room the sidebar and the "On this page" list take from the viewport.
    // The container and the grid it sizes must be different elements: a container query can't style its own container.
    private const string previewContainerClass = "@container";
    private const string previewGridClass = "grid grid-cols-1 gap-4 @lg:grid-cols-2";

    private static readonly ThemeTemplate template = ThemeTemplate.Default;

    private readonly Dictionary<string, string> colors = ThemeColorFamily.All.ToDictionary(f => f.Name, f => f.DefaultHex);
    private readonly List<ComponentGroup> groups = BuildGroups();
    private readonly HashSet<string> selected = [];

    private readonly Dictionary<string, OnColorMode> onColors = ThemeColorFamily.All.ToDictionary(f => f.Name, _ => OnColorMode.Auto);

    private ThemeDefaults defaults = ThemeDefaults.Default;

    private string generatedCode = string.Empty;
    private int codeVersion;
    private int includedThemeCount;
    private int totalComponents;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        totalComponents = groups.Sum(g => g.Components.Count);
        SelectAll();
    }

    private static List<ComponentGroup> BuildGroups() =>
    [
        .. ComponentCatalog.LoadLeafEntries()
            .Where(leaf => !string.IsNullOrEmpty(leaf.Entry.Name))
            .GroupBy(leaf => leaf.Category)
            .Select(g => new ComponentGroup(g.Key, [.. g.Select(leaf => new ComponentOption(leaf.Entry.Name, leaf.Entry.Display))]))
    ];

    private bool? GroupState(ComponentGroup group)
    {
        var count = group.Components.Count(c => selected.Contains(c.Name));
        return count == 0 ? false : count == group.Components.Count ? true : null;
    }

    private void ToggleGroup(ComponentGroup group)
    {
        // A mixed group resolves to fully selected, matching how a native tri-state checkbox behaves.
        var select = GroupState(group) != true;

        foreach (var component in group.Components)
        {
            if (select)
                selected.Add(component.Name);
            else
                selected.Remove(component.Name);
        }

        Regenerate();
    }

    private void ToggleComponent(string name)
    {
        if (!selected.Remove(name))
            selected.Add(name);

        Regenerate();
    }

    private void SelectAll()
    {
        selected.UnionWith(groups.SelectMany(g => g.Components).Select(c => c.Name));
        Regenerate();
    }

    private void ClearAll()
    {
        selected.Clear();
        Regenerate();
    }

    private void SetColor(string name, string value)
    {
        colors[name] = value;
        Regenerate();
    }

    private void SetOnColor(string name, OnColorMode value)
    {
        onColors[name] = value;
        Regenerate();
    }

    private void ResetColors()
    {
        foreach (var family in ThemeColorFamily.All)
        {
            colors[family.Name] = family.DefaultHex;
            onColors[family.Name] = OnColorMode.Auto;
        }

        Regenerate();
    }

    /// <summary>
    /// Describes the text color a filled button or chip of this color gets, and how readable it is.
    /// </summary>
    private string FilledContrast(ThemeColorFamily family)
    {
        if (family.FilledBackground(colors[family.Name]) is not { } background)
            return "Enter a valid hex color.";

        var resolved = ThemeOnColor.Resolve(onColors[family.Name], background);
        var ratio = ThemeOnColor.Contrast(resolved, background);
        var grade = ratio switch
        {
            >= 7 => "AAA",
            >= 4.5 => "AA",
            >= 3 => "large text only",
            _ => "fails contrast",
        };

        return string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{resolved} text on filled: {ratio:0.0}:1 ({grade})");
    }

    /// <summary>
    /// Warns when a color you changed is too faint to read as text or a border on a white page, which is how outlined
    /// and text buttons use it. The filled button text is handled separately and can't fix this.
    /// </summary>
    private string? ForegroundWarning(ThemeColorFamily family)
    {
        if (!family.IsCustomized(colors[family.Name]) || ThemeColorRamp.Generate(colors[family.Name]) is not { } ramp)
            return null;

        var ratio = ramp[ThemeColorRamp.AnchorShade].ContrastWith((255, 255, 255));

        return ratio < 3
            ? string.Create(System.Globalization.CultureInfo.InvariantCulture, $"Only {ratio:0.0}:1 against white, so outlined and text buttons in light mode will be hard to see.")
            : null;
    }

    private void SetDefaults(ThemeDefaults value)
    {
        defaults = value;
        Regenerate();
    }

    private void ResetDefaults() => SetDefaults(ThemeDefaults.Default);

    private void Regenerate()
    {
        var themeTypes = template.ResolveThemeTypes(selected);

        generatedCode = ThemeCodeGenerator.Generate(template, themeTypes, colors, defaults, onColors);
        includedThemeCount = template.Blocks.Count(b => themeTypes.Contains(b.TypeName));

        // The code block highlights only on first render, so a new key makes it render (and highlight) afresh.
        codeVersion++;
    }
}
