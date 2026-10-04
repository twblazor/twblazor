namespace TwBlazor.Docs.Services.ThemeBuilder;

/// <summary>
/// A semantic theme color that can be customized, and the Tailwind palette the default theme draws it from.
/// </summary>
/// <param name="Name">The semantic color's display name, matching <c>Color</c> enum member, e.g. <c>Primary</c>.</param>
/// <param name="TailwindName">The Tailwind palette the default theme uses for it, e.g. <c>purple</c>.</param>
/// <param name="DefaultHex">That palette's 600 shade as hex, the color the picker starts on.</param>
/// <param name="FilledShade">The shade the default theme fills buttons and chips of this color with.</param>
/// <param name="ForegroundShade">The shade the default theme draws this color as a border on a light page, as outlined buttons do.</param>
internal sealed record ThemeColorFamily(string Name, string TailwindName, string DefaultHex, int FilledShade, int ForegroundShade)
{
    /// <summary>
    /// The customizable colors. The neutral surface colors (black, gray and white) are deliberately absent:
    /// they stay as the default theme defines them.
    /// </summary>
    public static readonly IReadOnlyList<ThemeColorFamily> All =
    [
        new("Primary", "purple", "#9810fa", 600, 600),
        new("Accent", "fuchsia", "#c800de", 700, 600),
        new("Success", "green", "#00a63e", 700, 600),
        new("Danger", "red", "#e7000b", 700, 600),
        new("Warning", "yellow", "#d08700", 600, 700),
        new("Info", "blue", "#155dfc", 600, 600),
    ];

    /// <summary>
    /// Gets whether <paramref name="hex"/> is a valid color other than this family's default, i.e. whether it
    /// changes anything.
    /// </summary>
    public bool IsCustomized(string? hex) =>
        OklchColor.TryParseHex(hex, out _) && !string.Equals(Normalize(hex), Normalize(DefaultHex), StringComparison.Ordinal);

    /// <summary>
    /// The color the default theme fills buttons and chips with once <paramref name="hex"/> is picked, or
    /// <see langword="null"/> when <paramref name="hex"/> isn't a valid color.
    /// </summary>
    /// <param name="hex">The picked color.</param>
    public OklchColor? FilledBackground(string? hex) =>
        ThemeColorRamp.Generate(hex) is { } ramp ? ramp[FilledShade] : null;

    /// <summary>
    /// The text color to use on this color's filled surfaces, or <see langword="null"/> when the generated
    /// theme should keep the default theme's own text color (an untouched color left on <see cref="OnColorMode.Auto"/>).
    /// </summary>
    /// <param name="hex">The picked color.</param>
    /// <param name="mode">The requested text mode.</param>
    public OnColorMode? ResolveOnColor(string? hex, OnColorMode mode)
    {
        if (mode != OnColorMode.Auto)
            return mode;

        return IsCustomized(hex) && FilledBackground(hex) is { } background ? ThemeOnColor.Resolve(mode, background) : null;
    }

    private static string Normalize(string? hex)
    {
        return OklchColor.TryParseHex(hex, out var color) ? color.ToCss() : string.Empty;
    }
}
