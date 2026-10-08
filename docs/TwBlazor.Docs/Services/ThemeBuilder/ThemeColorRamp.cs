namespace TwBlazor.Docs.Services.ThemeBuilder;

/// <summary>
/// Expands one picked color into the 50 to 950 shade ramp the default theme's Tailwind classes address
/// (<c>bg-purple-50</c> through <c>bg-purple-950</c>).
/// </summary>
/// <remarks>
/// The picked color becomes shade 600, the main brand shade throughout the default theme. The other shades keep
/// its hue and follow the lightness and chroma curve of Tailwind's own palette, stretched so the lightest shade
/// stays near-white and the darkest stays dark whatever lightness was picked.
/// </remarks>
internal static class ThemeColorRamp
{
    /// <summary>The shade the picked color is assigned to.</summary>
    public const int AnchorShade = 600;

    /// <summary>Every shade in the ramp, lightest first.</summary>
    public static readonly IReadOnlyList<int> Shades = [50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 950];

    private const double lightestLightness = 0.977;
    private const double darkestLightness = 0.291;
    private const double anchorLightness = 0.558;

    private static readonly Dictionary<int, double> referenceLightness = new()
    {
        [50] = 0.977,
        [100] = 0.946,
        [200] = 0.902,
        [300] = 0.827,
        [400] = 0.714,
        [500] = 0.627,
        [600] = anchorLightness,
        [700] = 0.496,
        [800] = 0.438,
        [900] = 0.381,
        [950] = darkestLightness,
    };

    private static readonly Dictionary<int, double> chromaFactor = new()
    {
        [50] = 0.05,
        [100] = 0.10,
        [200] = 0.20,
        [300] = 0.45,
        [400] = 0.70,
        [500] = 0.92,
        [600] = 1.00,
        [700] = 0.97,
        [800] = 0.83,
        [900] = 0.70,
        [950] = 0.58,
    };

    /// <summary>
    /// Builds the ramp for <paramref name="hex"/>, or returns <see langword="null"/> when it isn't a valid color.
    /// </summary>
    /// <param name="hex">The picked color as <c>#rrggbb</c>.</param>
    public static IReadOnlyDictionary<int, OklchColor>? Generate(string? hex)
    {
        if (!OklchColor.TryParseHex(hex, out var anchor))
            return null;

        var lightTarget = Math.Max(lightestLightness, anchor.Lightness);
        var darkTarget = Math.Min(darkestLightness, anchor.Lightness * 0.5);

        return Shades.ToDictionary(shade => shade, shade => shade == AnchorShade
            ? anchor
            : new OklchColor(LightnessFor(shade, anchor.Lightness, lightTarget, darkTarget), anchor.Chroma * chromaFactor[shade], anchor.Hue));
    }

    private static double LightnessFor(int shade, double anchor, double lightTarget, double darkTarget)
    {
        var reference = referenceLightness[shade];

        return shade < AnchorShade
            ? anchor + (reference - anchorLightness) / (lightestLightness - anchorLightness) * (lightTarget - anchor)
            : anchor - (anchorLightness - reference) / (anchorLightness - darkestLightness) * (anchor - darkTarget);
    }
}
