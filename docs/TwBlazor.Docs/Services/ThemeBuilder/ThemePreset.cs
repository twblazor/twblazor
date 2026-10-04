using TwBlazor.Enums;

namespace TwBlazor.Docs.Services.ThemeBuilder;

/// <summary>
/// A ready-made starting point for the theme builder: the six semantic colors and the enum defaults.
/// </summary>
/// <param name="Name">The preset's name.</param>
/// <param name="Description">A few words on its character.</param>
/// <param name="Colors">Hex color per <see cref="ThemeColorFamily.Name"/>.</param>
/// <param name="Defaults">The enum defaults.</param>
internal sealed record ThemePreset(string Name, string Description, IReadOnlyDictionary<string, string> Colors, ThemeDefaults Defaults)
{
    /// <summary>
    /// The example themes, with the default (purple) theme first.
    /// </summary>
    public static readonly IReadOnlyList<ThemePreset> _all =
    [
        new("Purple", "The default theme", ThemeColorFamily.All.ToDictionary(f => f.Name, f => f.DefaultHex), ThemeDefaults.Default),

        // Purples and violets
        Create("Lavender", "Soft violet, gentle corners", "#7c3aed", "#c026d3", "#15803d", "#dc2626", "#ca8a04", "#2563eb", Rounded.Lg, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled),
        Create("Grape", "Rich violet, crisp edges", "#6d28d9", "#a21caf", "#15803d", "#b91c1c", "#b45309", "#1d4ed8", Rounded.Sm, Shadow.Md, InputVariant.Outlined, ButtonVariant.Filled),
        Create("Orchid", "Magenta with a violet partner", "#a21caf", "#7c3aed", "#166534", "#be123c", "#d97706", "#0369a1", Rounded.Md, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled),
        Create("Plum", "Deep wine and berry", "#86198f", "#be123c", "#166534", "#9f1239", "#b45309", "#1d4ed8", Rounded.Md, Shadow.Md, InputVariant.Filled, ButtonVariant.Elevated),
        Create("Dusk", "Twilight purple, deep shadows", "#5b21b6", "#be185d", "#15803d", "#b91c1c", "#d97706", "#0369a1", Rounded.Lg, Shadow.Lg, InputVariant.Filled, ButtonVariant.Elevated),

        // Blues
        Create("Ocean", "Cool blues, rounder corners", "#0369a1", "#0d9488", "#15803d", "#dc2626", "#d97706", "#4f46e5", Rounded.Lg, Shadow.Md, InputVariant.Outlined, ButtonVariant.Filled),
        Create("Midnight", "Indigo night, outlined inputs", "#4338ca", "#7c3aed", "#15803d", "#b91c1c", "#d97706", "#0369a1", Rounded.Md, Shadow.Lg, InputVariant.Outlined, ButtonVariant.Filled),
        Create("Indigo", "Classic indigo, balanced", "#4338ca", "#7c3aed", "#15803d", "#be123c", "#ca8a04", "#0369a1", Rounded.Md, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled),
        Create("Cobalt", "Bold blue, straight down the middle", "#1d4ed8", "#0369a1", "#15803d", "#b91c1c", "#d97706", "#4338ca", Rounded.Md, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled),
        Create("Navy", "Dark blue, sharp and quiet", "#1e3a8a", "#0f766e", "#166534", "#991b1b", "#b45309", "#1e40af", Rounded.Sm, Shadow.None, InputVariant.Outlined, ButtonVariant.Filled),
        Create("Sky", "Open blue, light touch", "#0369a1", "#7c3aed", "#15803d", "#dc2626", "#ca8a04", "#0e7490", Rounded.Lg, Shadow.Sm, InputVariant.Default, ButtonVariant.Filled),
        Create("Arctic", "Icy blue, text buttons", "#0284c7", "#6366f1", "#15803d", "#dc2626", "#d97706", "#0369a1", Rounded.Lg, Shadow.Sm, InputVariant.Outlined, ButtonVariant.Text),
        Create("Cyan", "Electric teal-blue", "#0e7490", "#4f46e5", "#15803d", "#b91c1c", "#d97706", "#0369a1", Rounded.Md, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled),

        // Greens and teals
        Create("Forest", "Earthy greens, flat and square", "#15803d", "#b45309", "#4d7c0f", "#b91c1c", "#ca8a04", "#0e7490", Rounded.Sm, Shadow.None, InputVariant.Default, ButtonVariant.Filled),
        Create("Emerald", "Jewel green, outlined inputs", "#047857", "#0f766e", "#15803d", "#b91c1c", "#b45309", "#0369a1", Rounded.Lg, Shadow.Md, InputVariant.Outlined, ButtonVariant.Filled),
        Create("Mint", "Fresh green-teal", "#0f766e", "#0369a1", "#15803d", "#be123c", "#b45309", "#4338ca", Rounded.Lg, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled),
        Create("Teal", "Calm teal, friendly corners", "#0d9488", "#0284c7", "#15803d", "#dc2626", "#d97706", "#4f46e5", Rounded.Lg, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled),
        Create("Lime", "Zesty olive-lime", "#4d7c0f", "#0f766e", "#166534", "#b91c1c", "#ca8a04", "#0369a1", Rounded.Md, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled),
        Create("Moss", "Muted woodland green", "#3f6212", "#854d0e", "#166534", "#991b1b", "#ca8a04", "#0f766e", Rounded.Sm, Shadow.None, InputVariant.Default, ButtonVariant.Filled),
        Create("Aurora", "Green-teal with violet glow", "#0f766e", "#7c3aed", "#15803d", "#be123c", "#ca8a04", "#0284c7", Rounded.Lg, Shadow.Lg, InputVariant.Filled, ButtonVariant.Elevated),

        // Reds, pinks and oranges
        Create("Sunset", "Warm oranges and pinks, deep shadows", "#ea580c", "#db2777", "#15803d", "#be123c", "#ca8a04", "#7c3aed", Rounded.Lg, Shadow.Lg, InputVariant.Filled, ButtonVariant.Elevated),
        Create("Rose", "Romantic rose and violet", "#be123c", "#9333ea", "#15803d", "#b91c1c", "#d97706", "#0369a1", Rounded.Lg, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled),
        Create("Cherry", "Deep red, clean lines", "#b91c1c", "#be185d", "#166534", "#991b1b", "#b45309", "#1d4ed8", Rounded.Sm, Shadow.Sm, InputVariant.Outlined, ButtonVariant.Filled),
        Create("Bubblegum", "Playful pink", "#be185d", "#7c3aed", "#15803d", "#dc2626", "#ca8a04", "#0284c7", Rounded.Lg, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled),
        Create("Coral", "Burnt orange, elevated buttons", "#c2410c", "#be185d", "#15803d", "#b91c1c", "#ca8a04", "#0e7490", Rounded.Lg, Shadow.Md, InputVariant.Filled, ButtonVariant.Elevated),
        Create("Volcano", "Molten red and orange", "#b91c1c", "#c2410c", "#15803d", "#7f1d1d", "#b45309", "#1d4ed8", Rounded.Sm, Shadow.Lg, InputVariant.Filled, ButtonVariant.Elevated),
 
        Create("Amber", "Golden brown, flat", "#b45309", "#a21caf", "#15803d", "#b91c1c", "#ca8a04", "#1d4ed8", Rounded.Sm, Shadow.None, InputVariant.Default, ButtonVariant.Filled),
        Create("Terracotta", "Baked clay and sage", "#9a3412", "#a16207", "#3f6212", "#991b1b", "#b45309", "#155e75", Rounded.Sm, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled),
        Create("Espresso", "Dark roast, no shadows", "#78350f", "#a16207", "#3f6212", "#991b1b", "#b45309", "#155e75", Rounded.Sm, Shadow.None, InputVariant.Default, ButtonVariant.Filled),

        // Neutrals
        Create("Slate", "Monochrome, sharp and outlined", "#334155", "#64748b", "#15803d", "#b91c1c", "#b45309", "#0369a1", Rounded.None, Shadow.None, InputVariant.Outlined, ButtonVariant.Outlined),
        Create("Graphite", "Neutral grey, square and outlined", "#374151", "#6b7280", "#15803d", "#b91c1c", "#b45309", "#0369a1", Rounded.None, Shadow.None, InputVariant.Outlined, ButtonVariant.Outlined),
        Create("Stone", "Warm grey, text buttons", "#57534e", "#a16207", "#3f6212", "#9a3412", "#b45309", "#0e7490", Rounded.Sm, Shadow.Sm, InputVariant.Default, ButtonVariant.Text),
    ];

    /// <summary>
    /// Gets whether the given colors and defaults are exactly this preset, ignoring hex case and shorthand.
    /// </summary>
    /// <param name="colors">Hex color per <see cref="ThemeColorFamily.Name"/>.</param>
    /// <param name="defaults">The enum defaults.</param>
    public bool Matches(IReadOnlyDictionary<string, string> colors, ThemeDefaults defaults) =>
        defaults == Defaults && ThemeColorFamily.All.All(f => colors.TryGetValue(f.Name, out var hex) && Same(hex, Colors[f.Name]));

    private static bool Same(string a, string b) =>
        OklchColor.TryParseHex(a, out var x) && OklchColor.TryParseHex(b, out var y) && x.ToCss() == y.ToCss();

    private static ThemePreset Create(
        string name,
        string description,
        string primary,
        string accent,
        string success,
        string danger,
        string warning,
        string info,
        Rounded rounded,
        Shadow shadow,
        InputVariant inputVariant,
        ButtonVariant buttonVariant) =>
        new(
            name,
            description,
            new Dictionary<string, string>
            {
                ["Primary"] = primary,
                ["Accent"] = accent,
                ["Success"] = success,
                ["Danger"] = danger,
                ["Warning"] = warning,
                ["Info"] = info,
            },
            new ThemeDefaults(rounded, shadow, inputVariant, buttonVariant));
}
