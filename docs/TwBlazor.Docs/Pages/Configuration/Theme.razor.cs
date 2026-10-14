using TwBlazor.Configuration.Color;
using Microsoft.JSInterop;
using TwBlazor.Docs.Services;
using TwBlazor.Docs.Services.ThemeBuilder;
using IconEnum = TwBlazor.Enums.Icon;

namespace TwBlazor.Docs.Pages.Configuration;

/// <summary>
/// The theme builder page: pick colors and components, see every sample component follow and copy the generated
/// <c>Theme.cs</c>. The choices are applied to this page's own element, never to the application's theme.
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

    /// <summary>
    /// A group of sample components on the page, linked from the bar at the top.
    /// </summary>
    /// <param name="Id">The id of the group's section.</param>
    /// <param name="Label">The name shown in the bar.</param>
    private sealed record PreviewType(string Id, string Label);

    private static readonly PreviewType[] _previewTypes =
    [
        new("preview-actions", "Buttons"),
        new("preview-forms", "Forms"),
        new("preview-feedback", "Feedback"),
        new("preview-navigation", "Navigation"),
        new("preview-data", "Data"),
        new("preview-charts", "Charts"),
    ];

    private const string ledeClass = "mt-1 mb-3 text-xs text-gray-700 dark:text-gray-300";
    private const string noteClass = "mb-4 text-xs text-gray-700 dark:text-gray-300";

    // Wider than the docs' sidebar, so a color picker and its text select fit without crowding.
    private const string sidebarClass = "max-lg:w-80 lg:w-88";

    private bool isSidebarOpen = true;
    private bool colorsOpen = true;

    private TwSurfacePalette surface => Options.Theme.Colors.NeutralSurface;

    private TwNeutralTextPalette text => Options.Theme.Colors.NeutralText;

    private string sectionHeadingClass => $"text-sm font-semibold uppercase tracking-wide {text.Heading}";

    private string previewHeadingClass => $"text-xl font-bold tracking-tight {text.Heading}";

    private string jumpLinkClass => $"inline-flex min-h-11 items-center whitespace-nowrap rounded-sm px-1 text-sm underline-offset-4 hover:underline focus-visible:outline-2 focus-visible:-outline-offset-2 focus-visible:outline-current {text.Secondary}";

    private string pageClass => $"{surface.Background} {text.Heading}";

    // The picked colors, as custom properties scoped to the page's own element.
    private string pageStyle => ThemePreviewStyle.Build(_colors);

    private IEnumerable<PreviewType> visiblePreviewTypes => _previewTypes.Where(type => ThemeBuilderPreview.ShowsSection(type.Id, _selected));

    // A container query, so the row holds as many themes as the page has room for beside the settings panel.
    private const string presetContainerClass = "@container flex flex-col gap-3";
    private const string presetGridClass = "grid grid-cols-2 gap-2 @xl:grid-cols-4 @5xl:grid-cols-8";

    private const string toggleIconClass = "text-xl";

    private ThemeSurfaces surfaces = ThemeSurfaces.Default;
    private string surfaceCss = string.Empty;
    private bool surfacesOpen;

    private IconEnum themeIcon = IconEnum.Moon;

    private string themeToggleLabel => themeIcon == IconEnum.Sun ? "Switch to light theme" : "Switch to dark theme";

    // Announced after the theme is switched: nothing else about the change reaches a screen reader.
    private string themeStatusMessage = string.Empty;

    private static readonly ThemeTemplate _template = ThemeTemplate.Default;

    private readonly Dictionary<string, string> _colors = ThemeColorFamily.All.ToDictionary(f => f.Name, f => f.DefaultHex);
    private readonly List<ComponentGroup> _groups = BuildGroups();
    private readonly HashSet<string> _selected = [];

    private readonly Dictionary<string, OnColorMode> _onColors = ThemeColorFamily.All.ToDictionary(f => f.Name, _ => OnColorMode.Auto);

    private bool defaultsOpen;
    private bool componentsOpen;

    private ThemeDefaults defaults = ThemeDefaults.Default;

    private string generatedCode = string.Empty;
    private int codeVersion;
    private int includedThemeCount;
    private int totalComponents;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        totalComponents = _groups.Sum(g => g.Components.Count);
        SelectAll();
    }

    private static List<ComponentGroup> BuildGroups() =>
    [
        .. ComponentCatalog.LoadLeafEntries()
            .Where(leaf => !string.IsNullOrEmpty(leaf.Entry.Name))
            .GroupBy(leaf => leaf.Category)
            .Select(g => new ComponentGroup(g.Key, [.. g.Select(leaf => new ComponentOption(leaf.Entry.Name, leaf.Entry.Display))]))
    ];

    // The collapse content clips its overflow, which would cut off the color picker and select popups inside it.
    private const string collapseClass = "[&>[role=region]]:overflow-visible";

    private int GroupSelectedCount(ComponentGroup group) => group.Components.Count(c => _selected.Contains(c.Name));

    private bool? GroupState(ComponentGroup group)
    {
        var count = group.Components.Count(c => _selected.Contains(c.Name));

        if (count == 0)
            return false;

        return count == group.Components.Count ? true : null;
    }

    private void ToggleGroup(ComponentGroup group)
    {
        // A mixed group resolves to fully selected, matching how a native tri-state checkbox behaves.
        var select = GroupState(group) != true;

        foreach (var name in group.Components.Select(c => c.Name))
        {
            if (select)
                _selected.Add(name);
            else
                _selected.Remove(name);
        }

        Regenerate();
    }

    private void ToggleComponent(string name)
    {
        if (!_selected.Remove(name))
            _selected.Add(name);

        Regenerate();
    }

    private void SelectAll()
    {
        _selected.UnionWith(_groups.SelectMany(g => g.Components).Select(c => c.Name));
        Regenerate();
    }

    private void ClearAll()
    {
        _selected.Clear();
        Regenerate();
    }

    private void SetColor(string name, string value)
    {
        _colors[name] = value;
        Regenerate();
    }

    // One row on a wide screen.
    private const int presetsPerPage = 8;

    private int presetPage = 1;

    private static int presetPageCount => (int)Math.Ceiling(ThemePreset._all.Count / (double)presetsPerPage);

    private IEnumerable<ThemePreset> visiblePresets => ThemePreset._all.Skip((presetPage - 1) * presetsPerPage).Take(presetsPerPage);

    private bool IsActive(ThemePreset preset) =>
        preset.Matches(_colors, defaults) && _onColors.Values.All(mode => mode == OnColorMode.Auto) && !ThemeSurface.IsCustomized(surfaces);

    private static string PresetClass(bool active) =>
        "flex w-full min-w-0 items-center gap-2 rounded-lg border px-3 py-2 text-start text-sm transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600 "
        + (active
            ? "border-purple-600 bg-purple-50 dark:border-purple-400 dark:bg-purple-950"
            : "border-gray-200 hover:bg-gray-50 dark:border-gray-700 dark:hover:bg-gray-800");

    private void ApplyPreset(ThemePreset preset)
    {
        foreach (var name in ThemeColorFamily.All.Select(f => f.Name))
        {
            _colors[name] = preset.Colors[name];
            _onColors[name] = OnColorMode.Auto;
        }

        defaults = preset.Defaults;
        surfaces = ThemeSurfaces.Default;
        Regenerate();
    }

    private void SetOnColor(string name, OnColorMode value)
    {
        _onColors[name] = value;
        Regenerate();
    }

    private void ResetColors()
    {
        foreach (var family in ThemeColorFamily.All)
        {
            _colors[family.Name] = family.DefaultHex;
            _onColors[family.Name] = OnColorMode.Auto;
        }

        Regenerate();
    }

    /// <summary>
    /// Describes the text color a filled button or chip of this color gets, and how readable it is.
    /// </summary>
    private string FilledContrast(ThemeColorFamily family)
    {
        if (family.FilledBackground(_colors[family.Name]) is not { } background)
            return "Enter a valid hex color.";

        var resolved = ThemeOnColor.Resolve(_onColors[family.Name], background);
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
        if (!family.IsCustomized(_colors[family.Name]) || ThemeColorRamp.Generate(_colors[family.Name]) is not { } ramp)
            return null;

        var ratio = ramp[family.ForegroundShade].ContrastWith((255, 255, 255));

        return ratio < 3
            ? string.Create(System.Globalization.CultureInfo.InvariantCulture, $"Only {ratio:0.0}:1 against white, so outlined and text buttons in light mode will be hard to see.")
            : null;
    }

    /// <summary>
    /// Reads the site's light or dark setting for the toggle, and closes the settings panel on a small screen,
    /// where it is a drawer over the page, the same way the docs do.
    /// </summary>
    /// <param name="firstRender">Whether this is the first render.</param>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        try
        {
            themeIcon = await JS.InvokeAsync<bool>("themeToggle.isDarkMode") ? IconEnum.Sun : IconEnum.Moon;
            isSidebarOpen = !await JS.InvokeAsync<bool>("twSidebar.isMobileViewport");
            StateHasChanged();
        }
        catch (Exception exception) when (exception is JSException or JSDisconnectedException or InvalidOperationException or OperationCanceledException)
        {
            // Prerendering, or the visitor has left: the panel keeps its default.
        }
    }

    private void SetSurface(bool dark, string value)
    {
        surfaces = dark ? surfaces with { Dark = value } : surfaces with { Light = value };
        Regenerate();
    }

    private void ResetSurfaces()
    {
        surfaces = ThemeSurfaces.Default;
        Regenerate();
    }

    /// <summary>
    /// Describes how readable the theme's own text is on a picked background.
    /// </summary>
    private string SurfaceContrast(bool dark)
    {
        if (ThemeSurface.TextContrast(dark ? surfaces.Dark : surfaces.Light, dark) is not { } ratio)
            return "Enter a valid hex color.";

        var grade = ratio switch
        {
            >= 7 => "AAA",
            >= 4.5 => "AA",
            >= 3 => "large text only",
            _ => "fails contrast",
        };

        return string.Create(System.Globalization.CultureInfo.InvariantCulture, $"Text on this background: {ratio:0.0}:1 ({grade})");
    }

    /// <summary>
    /// Switches the site between light and dark, exactly as the button in the docs' own top bar does.
    /// </summary>
    private async Task ToggleThemeAsync()
    {
        var isDark = await JS.InvokeAsync<bool>("themeToggle.toggle");
        themeIcon = isDark ? IconEnum.Sun : IconEnum.Moon;
        themeStatusMessage = isDark ? "Dark theme on" : "Light theme on";
    }

    private void SetDefaults(ThemeDefaults value)
    {
        defaults = value;
        Regenerate();
    }

    private void ResetDefaults() => SetDefaults(ThemeDefaults.Default);

    private void Regenerate()
    {
        var themeTypes = _template.ResolveThemeTypes(_selected);

        generatedCode = ThemeCodeGenerator.Generate(_template, themeTypes, _colors, defaults, _onColors, surfaces);
        surfaceCss = ThemeSurface.BuildCss(_template, surfaces);
        includedThemeCount = _template.Blocks.Count(b => themeTypes.Contains(b.TypeName));

        // The code block highlights only on first render, so a new key makes it render (and highlight) afresh.
        codeVersion++;
    }
}
