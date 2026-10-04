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
        Create("Lavender", "Soft violet, gentle corners", (Palette.Violet600, Palette.Fuchsia600, Palette.Green700, Palette.Red600, Palette.Yellow600, Palette.Blue600), new(Rounded.Lg, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Grape", "Rich violet, crisp edges", (Palette.Violet700, Palette.Fuchsia700, Palette.Green700, Palette.Red700, Palette.Amber700, Palette.Blue700), new(Rounded.Sm, Shadow.Md, InputVariant.Outlined, ButtonVariant.Filled)),
        Create("Orchid", "Magenta with a violet partner", (Palette.Fuchsia700, Palette.Violet600, Palette.Green800, Palette.Rose700, Palette.Amber600, Palette.Sky700), new(Rounded.Md, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Plum", "Deep wine and berry", (Palette.Fuchsia800, Palette.Rose700, Palette.Green800, Palette.Rose800, Palette.Amber700, Palette.Blue700), new(Rounded.Md, Shadow.Md, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Dusk", "Twilight purple, deep shadows", (Palette.Violet800, Palette.Pink700, Palette.Green700, Palette.Red700, Palette.Amber600, Palette.Sky700), new(Rounded.Lg, Shadow.Lg, InputVariant.Filled, ButtonVariant.Elevated)),

        // Blues
        Create("Ocean", "Cool blues, rounder corners", (Palette.Sky700, Palette.Teal600, Palette.Green700, Palette.Red600, Palette.Amber600, Palette.Indigo600), new(Rounded.Lg, Shadow.Md, InputVariant.Outlined, ButtonVariant.Filled)),
        Create("Midnight", "Indigo night, outlined inputs", (Palette.Indigo700, Palette.Violet600, Palette.Green700, Palette.Red700, Palette.Amber600, Palette.Sky700), new(Rounded.Md, Shadow.Lg, InputVariant.Outlined, ButtonVariant.Filled)),
        Create("Indigo", "Classic indigo, balanced", (Palette.Indigo700, Palette.Violet600, Palette.Green700, Palette.Rose700, Palette.Yellow600, Palette.Sky700), new(Rounded.Md, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Cobalt", "Bold blue, straight down the middle", (Palette.Blue700, Palette.Sky700, Palette.Green700, Palette.Red700, Palette.Amber600, Palette.Indigo700), new(Rounded.Md, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Navy", "Dark blue, sharp and quiet", (Palette.Blue900, Palette.Teal700, Palette.Green800, Palette.Red800, Palette.Amber700, Palette.Blue800), new(Rounded.Sm, Shadow.None, InputVariant.Outlined, ButtonVariant.Filled)),
        Create("Sky", "Open blue, light touch", (Palette.Sky700, Palette.Violet600, Palette.Green700, Palette.Red600, Palette.Yellow600, Palette.Cyan700), new(Rounded.Lg, Shadow.Sm, InputVariant.Default, ButtonVariant.Filled)),
        Create("Arctic", "Icy blue, text buttons", (Palette.Sky600, Palette.Indigo500, Palette.Green700, Palette.Red600, Palette.Amber600, Palette.Sky700), new(Rounded.Lg, Shadow.Sm, InputVariant.Outlined, ButtonVariant.Text)),
        Create("Cyan", "Electric teal-blue", (Palette.Cyan700, Palette.Indigo600, Palette.Green700, Palette.Red700, Palette.Amber600, Palette.Sky700), new(Rounded.Md, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled)),

        // Greens and teals
        Create("Forest", "Earthy greens, flat and square", (Palette.Green700, Palette.Amber700, Palette.Lime700, Palette.Red700, Palette.Yellow600, Palette.Cyan700), new(Rounded.Sm, Shadow.None, InputVariant.Default, ButtonVariant.Filled)),
        Create("Emerald", "Jewel green, outlined inputs", (Palette.Emerald700, Palette.Teal700, Palette.Green700, Palette.Red700, Palette.Amber700, Palette.Sky700), new(Rounded.Lg, Shadow.Md, InputVariant.Outlined, ButtonVariant.Filled)),
        Create("Mint", "Fresh green-teal", (Palette.Teal700, Palette.Sky700, Palette.Green700, Palette.Rose700, Palette.Amber700, Palette.Indigo700), new(Rounded.Lg, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Teal", "Calm teal, friendly corners", (Palette.Teal600, Palette.Sky600, Palette.Green700, Palette.Red600, Palette.Amber600, Palette.Indigo600), new(Rounded.Lg, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Lime", "Zesty olive-lime", (Palette.Lime700, Palette.Teal700, Palette.Green800, Palette.Red700, Palette.Yellow600, Palette.Sky700), new(Rounded.Md, Shadow.Sm, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Moss", "Muted woodland green", (Palette.Lime800, Palette.Yellow800, Palette.Green800, Palette.Red800, Palette.Yellow600, Palette.Teal700), new(Rounded.Sm, Shadow.None, InputVariant.Default, ButtonVariant.Filled)),
        Create("Aurora", "Green-teal with violet glow", (Palette.Teal700, Palette.Violet600, Palette.Green700, Palette.Rose700, Palette.Yellow600, Palette.Sky600), new(Rounded.Lg, Shadow.Lg, InputVariant.Filled, ButtonVariant.Elevated)),

        // Reds, pinks and oranges
        Create("Sunset", "Warm oranges and pinks, deep shadows", (Palette.Orange600, Palette.Pink600, Palette.Green700, Palette.Rose700, Palette.Yellow600, Palette.Violet600), new(Rounded.Lg, Shadow.Lg, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Rose", "Romantic rose and violet", (Palette.Rose700, Palette.Purple600, Palette.Green700, Palette.Red700, Palette.Amber600, Palette.Sky700), new(Rounded.Lg, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Cherry", "Deep red, clean lines", (Palette.Red700, Palette.Pink700, Palette.Green800, Palette.Red800, Palette.Amber700, Palette.Blue700), new(Rounded.Sm, Shadow.Sm, InputVariant.Outlined, ButtonVariant.Filled)),
        Create("Bubblegum", "Playful pink", (Palette.Pink700, Palette.Violet600, Palette.Green700, Palette.Red600, Palette.Yellow600, Palette.Sky600), new(Rounded.Lg, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Coral", "Burnt orange, elevated buttons", (Palette.Orange700, Palette.Pink700, Palette.Green700, Palette.Red700, Palette.Yellow600, Palette.Cyan700), new(Rounded.Lg, Shadow.Md, InputVariant.Filled, ButtonVariant.Elevated)),
        Create("Volcano", "Molten red and orange", (Palette.Red700, Palette.Orange700, Palette.Green700, Palette.Red900, Palette.Amber700, Palette.Blue700), new(Rounded.Sm, Shadow.Lg, InputVariant.Filled, ButtonVariant.Elevated)),
 
        Create("Amber", "Golden brown, flat", (Palette.Amber700, Palette.Fuchsia700, Palette.Green700, Palette.Red700, Palette.Yellow600, Palette.Blue700), new(Rounded.Sm, Shadow.None, InputVariant.Default, ButtonVariant.Filled)),
        Create("Terracotta", "Baked clay and sage", (Palette.Orange800, Palette.Yellow700, Palette.Lime800, Palette.Red800, Palette.Amber700, Palette.Cyan800), new(Rounded.Sm, Shadow.Md, InputVariant.Filled, ButtonVariant.Filled)),
        Create("Espresso", "Dark roast, no shadows", (Palette.Amber900, Palette.Yellow700, Palette.Lime800, Palette.Red800, Palette.Amber700, Palette.Cyan800), new(Rounded.Sm, Shadow.None, InputVariant.Default, ButtonVariant.Filled)),

        // Neutrals
        Create("Slate", "Monochrome, sharp and outlined", (Palette.Slate700, Palette.Slate500, Palette.Green700, Palette.Red700, Palette.Amber700, Palette.Sky700), new(Rounded.None, Shadow.None, InputVariant.Outlined, ButtonVariant.Outlined)),
        Create("Graphite", "Neutral grey, square and outlined", (Palette.Gray700, Palette.Gray500, Palette.Green700, Palette.Red700, Palette.Amber700, Palette.Sky700), new(Rounded.None, Shadow.None, InputVariant.Outlined, ButtonVariant.Outlined)),
        Create("Stone", "Warm grey, text buttons", (Palette.Stone600, Palette.Yellow700, Palette.Lime800, Palette.Orange800, Palette.Amber700, Palette.Cyan700), new(Rounded.Sm, Shadow.Sm, InputVariant.Default, ButtonVariant.Text)),
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
        (string Primary, string Accent, string Success, string Danger, string Warning, string Info) colors,
        ThemeDefaults defaults) =>
        new(
            name,
            description,
            new Dictionary<string, string>
            {
                ["Primary"] = colors.Primary,
                ["Accent"] = colors.Accent,
                ["Success"] = colors.Success,
                ["Danger"] = colors.Danger,
                ["Warning"] = colors.Warning,
                ["Info"] = colors.Info,
            },
            defaults);

    /// <summary>
    /// The Tailwind palette shades the example themes are built from, named by palette and shade.
    /// </summary>
    private static class Palette
    {
        public const string Amber600 = "#d97706";
        public const string Amber700 = "#b45309";
        public const string Amber900 = "#78350f";
        public const string Blue600 = "#2563eb";
        public const string Blue700 = "#1d4ed8";
        public const string Blue800 = "#1e40af";
        public const string Blue900 = "#1e3a8a";
        public const string Cyan700 = "#0e7490";
        public const string Cyan800 = "#155e75";
        public const string Emerald700 = "#047857";
        public const string Fuchsia600 = "#c026d3";
        public const string Fuchsia700 = "#a21caf";
        public const string Fuchsia800 = "#86198f";
        public const string Gray500 = "#6b7280";
        public const string Gray700 = "#374151";
        public const string Green700 = "#15803d";
        public const string Green800 = "#166534";
        public const string Indigo500 = "#6366f1";
        public const string Indigo600 = "#4f46e5";
        public const string Indigo700 = "#4338ca";
        public const string Lime700 = "#4d7c0f";
        public const string Lime800 = "#3f6212";
        public const string Orange600 = "#ea580c";
        public const string Orange700 = "#c2410c";
        public const string Orange800 = "#9a3412";
        public const string Pink600 = "#db2777";
        public const string Pink700 = "#be185d";
        public const string Purple600 = "#9333ea";
        public const string Red600 = "#dc2626";
        public const string Red700 = "#b91c1c";
        public const string Red800 = "#991b1b";
        public const string Red900 = "#7f1d1d";
        public const string Rose700 = "#be123c";
        public const string Rose800 = "#9f1239";
        public const string Sky600 = "#0284c7";
        public const string Sky700 = "#0369a1";
        public const string Slate500 = "#64748b";
        public const string Slate700 = "#334155";
        public const string Stone600 = "#57534e";
        public const string Teal600 = "#0d9488";
        public const string Teal700 = "#0f766e";
        public const string Violet600 = "#7c3aed";
        public const string Violet700 = "#6d28d9";
        public const string Violet800 = "#5b21b6";
        public const string Yellow600 = "#ca8a04";
        public const string Yellow700 = "#a16207";
        public const string Yellow800 = "#854d0e";
    }
}
