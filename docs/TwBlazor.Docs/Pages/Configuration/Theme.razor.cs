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

    // The settings panel sits beside the samples on a wide screen and stays put while they scroll. On a narrow
    // screen it comes first, above them.
    private const string layoutClass = "lg:grid lg:grid-cols-[22rem_minmax(0,1fr)] xl:grid-cols-[24rem_minmax(0,1fr)]";
    
    private const string skipLinkClass = "sr-only focus:not-sr-only focus:fixed focus:top-2 focus:start-2 focus:z-50 focus:rounded focus:bg-white focus:px-3 focus:py-2 focus:text-sm focus:font-medium focus:text-gray-950 focus:shadow-lg";

    // Two rows of four, so a whole page of example themes fits in the panel.
    private const string presetGridClass = "mt-2 grid grid-cols-4 gap-1.5";

    // Each color's picker and its text select share a row, with the contrast it gives on the line below.
    private const string colorRowClass = "grid grid-cols-[minmax(0,1fr)_7rem] items-end gap-x-2 gap-y-1";
    private const string warningClass = "col-span-2 text-xs text-amber-700 dark:text-amber-400";

    private const string toggleIconClass = "text-xl";

    private TwSurfacePalette surface => Options.Theme.Colors.NeutralSurface;

    private TwNeutralTextPalette text => Options.Theme.Colors.NeutralText;

    private string pageClass => $"{surface.Background} {text.Heading}";

    // The samples sit on the page body, so the cards among them stand out the way they would in an app.
    private string mainClass => $"min-w-0 p-4 focus:outline-none sm:p-6 lg:p-8 {surface.Body}";

    private string panelColumnClass => $"border-b {surface.Border} {surface.BackgroundSubtle} lg:border-e lg:border-b-0";

    private const string panelClass = "p-4 lg:sticky lg:top-0 lg:h-dvh lg:overflow-y-auto lg:overscroll-contain";

    private string panelHeadingClass => $"text-base font-semibold {text.Heading}";

    private string subheadingClass => $"text-xs font-semibold uppercase tracking-wide {text.Secondary}";

    private string columnCaptionClass => $"mt-2 mb-1 grid grid-cols-[minmax(0,1fr)_7rem] gap-x-2 text-xs {text.Secondary}";

    private const string colorListClass = "flex flex-col gap-3";

    private string contrastClass => $"col-span-2 text-xs tabular-nums {text.Secondary}";

    private string hintClass => $"text-xs {text.Secondary}";

    private string groupClass => $"overflow-hidden rounded-lg border {surface.Border}";

    private string groupHeaderClass => $"flex items-center justify-between gap-2 border-b px-2.5 py-1.5 {surface.Border} {surface.BackgroundSubtle}";

    private string countClass => $"text-xs tabular-nums {text.Secondary}";

    private string titleClass => $"text-2xl font-bold tracking-tight md:text-3xl {text.Heading}";

    private string previewHeadingClass => $"text-xl font-bold tracking-tight {text.Heading}";

    private string codeLinkClass => $"inline-flex min-h-9 items-center gap-1.5 rounded-lg border px-3 text-sm font-medium {surface.Border} {surface.Hover} focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600";

    // The picked colors, as custom properties scoped to the page's own element.
    private string pageStyle => ThemePreviewStyle.Build(_colors);

    private ThemeSurfaces surfaces = ThemeSurfaces.Default;
    private string surfaceCss = string.Empty;

    private IconEnum themeIcon = IconEnum.Moon;

    private string themeToggleLabel => themeIcon == IconEnum.Sun ? "Switch to light theme" : "Switch to dark theme";

    // Announced after the theme is switched: nothing else about the change reaches a screen reader.
    private string themeStatusMessage = string.Empty;

    private static readonly ThemeTemplate _template = ThemeTemplate.Default;

    private readonly Dictionary<string, string> _colors = ThemeColorFamily.All.ToDictionary(f => f.Name, f => f.DefaultHex);
    private readonly List<ComponentGroup> _groups = BuildGroups();
    private readonly HashSet<string> _selected = [];

    private readonly Dictionary<string, OnColorMode> _onColors = ThemeColorFamily.All.ToDictionary(f => f.Name, _ => OnColorMode.Auto);

    private ThemeDefaults defaults = ThemeDefaults.Default;

    private string generatedCode = string.Empty;
    private int codeVersion;
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

    private const int presetsPerPage = 8;

    private int presetPage = 1;

    /// <summary>
    /// A filter chip above the example themes, with the swatch shown beside its name.
    /// </summary>
    /// <param name="Tone">The color family it shows, or <see langword="null"/> for every theme.</param>
    /// <param name="Label">The chip's text.</param>
    /// <param name="Swatch">The hex color of its swatch, or <see langword="null"/> for none.</param>
    private sealed record ToneFilter(ThemeTone? Tone, string Label, string? Swatch);

    private static readonly ToneFilter[] _toneFilters =
    [
        new(null, "All", null),
        new(ThemeTone.Purple, "Purple", "#9333ea"),
        new(ThemeTone.Blue, "Blue", "#2563eb"),
        new(ThemeTone.Green, "Green", "#16a34a"),
        new(ThemeTone.Pink, "Pink", "#db2777"),
        new(ThemeTone.Red, "Red", "#dc2626"),
        new(ThemeTone.Orange, "Orange", "#ea580c"),
        new(ThemeTone.Neutral, "Neutral", "#6b7280"),
    ];

    private ThemeTone? toneFilter;

    private List<ThemePreset> filteredPresets => [.. ThemePreset._all.Where(preset => toneFilter is null || preset.Tone == toneFilter)];

    private int presetPageCount => Math.Max(1, (int)Math.Ceiling(filteredPresets.Count / (double)presetsPerPage));

    private IEnumerable<ThemePreset> visiblePresets => filteredPresets.Skip((presetPage - 1) * presetsPerPage).Take(presetsPerPage);

    private string presetFilterStatus
    {
        get
        {
            var count = filteredPresets.Count;
            var tone = _toneFilters.First(filter => filter.Tone == toneFilter);

            return toneFilter is null ? $"Showing all {count} example themes" : $"Showing {count} {tone.Label.ToLowerInvariant()} example themes";
        }
    }

    private void SetToneFilter(ThemeTone? tone)
    {
        toneFilter = tone;
        presetPage = 1;
    }

    // A neutral fill rather than purple: purple follows the picked primary, which might not keep white text readable.
    private string ToneChipClass(bool selected) =>
        "inline-flex min-h-7 items-center gap-1.5 rounded-full border px-2.5 text-xs font-medium transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600 "
        + (selected
            ? "border-gray-900 bg-gray-900 text-white dark:border-white dark:bg-white dark:text-gray-950"
            : $"border-gray-300 bg-white hover:bg-gray-50 dark:border-gray-600 dark:bg-gray-900 dark:hover:bg-gray-800 {text.Secondary}");

    private bool IsActive(ThemePreset preset) =>
        preset.Matches(_colors, defaults, surfaces) && _onColors.Values.All(mode => mode == OnColorMode.Auto);

    // The chosen theme gets a thicker ring as well as the color, so it stands out without relying on color alone.
    private static string PresetClass(bool active) =>
        "flex w-full min-w-0 flex-col items-start gap-1.5 rounded-md border px-2 py-2 text-start text-xs transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600 "
        + (active
            ? "border-purple-600 bg-purple-50 ring-1 ring-purple-600 dark:border-purple-400 dark:bg-purple-950 dark:ring-purple-400"
            : "border-gray-200 bg-white hover:border-gray-300 hover:bg-gray-50 dark:border-gray-700 dark:bg-gray-900 dark:hover:bg-gray-800");

    private void ApplyPreset(ThemePreset preset)
    {
        foreach (var name in ThemeColorFamily.All.Select(f => f.Name))
        {
            _colors[name] = preset.Colors[name];
            _onColors[name] = OnColorMode.Auto;
        }

        defaults = preset.Defaults;
        surfaces = preset.Surfaces;
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
            ? string.Create(System.Globalization.CultureInfo.InvariantCulture, $"Only {ratio:0.0}:1 on white, so outlined buttons will be hard to see.")
            : null;
    }

    /// <summary>
    /// Reads the site's light or dark setting for the toggle.
    /// </summary>
    /// <param name="firstRender">Whether this is the first render.</param>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        try
        {
            themeIcon = await JS.InvokeAsync<bool>("themeToggle.isDarkMode") ? IconEnum.Sun : IconEnum.Moon;
            StateHasChanged();
        }
        catch (Exception exception) when (exception is JSException or JSDisconnectedException or InvalidOperationException or OperationCanceledException)
        {
            // Prerendering, or the visitor has left: the toggle keeps its default.
        }
    }

    private void SetSurface(bool dark, string value)
    {
        surfaces = dark ? surfaces with { Dark = value } : surfaces with { Light = value };
        Regenerate();
    }

    private void SetBody(bool dark, string value)
    {
        surfaces = dark ? surfaces with { BodyDark = value } : surfaces with { BodyLight = value };
        Regenerate();
    }

    /// <summary>
    /// Describes how readable the theme's own text is on the page body a mode ends up with.
    /// </summary>
    private string BodyContrast(bool dark) => DescribeContrast(ThemeSurface.TextContrast(ThemeSurface.EffectiveBody(surfaces, dark), dark));

    private void ResetSurfaces()
    {
        surfaces = ThemeSurfaces.Default;
        Regenerate();
    }

    /// <summary>
    /// Describes how readable the theme's own text is on a picked background.
    /// </summary>
    private string SurfaceContrast(bool dark) => DescribeContrast(ThemeSurface.TextContrast(dark ? surfaces.Dark : surfaces.Light, dark));

    /// <summary>
    /// Describes a contrast ratio for the theme's text, with the WCAG level it reaches.
    /// </summary>
    private static string DescribeContrast(double? contrast)
    {
        if (contrast is not { } ratio)
            return "Enter a valid hex color.";

        var grade = ratio switch
        {
            >= 7 => "AAA",
            >= 4.5 => "AA",
            >= 3 => "large text only",
            _ => "fails contrast",
        };

        return string.Create(System.Globalization.CultureInfo.InvariantCulture, $"Text {ratio:0.0}:1 ({grade})");
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

        // The code block highlights only on first render, so a new key makes it render (and highlight) afresh.
        codeVersion++;
    }
}
