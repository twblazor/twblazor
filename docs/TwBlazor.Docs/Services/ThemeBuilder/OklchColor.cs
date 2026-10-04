using System.Globalization;
using TwBlazor.Utilities;

namespace TwBlazor.Docs.Services.ThemeBuilder;

/// <summary>
/// A color in the OKLCH space, the space Tailwind's own palette is defined in.
/// </summary>
/// <param name="Lightness">Perceptual lightness from 0 (black) to 1 (white).</param>
/// <param name="Chroma">Colorfulness, roughly 0 to 0.4 for in-gamut colors.</param>
/// <param name="Hue">Hue angle in degrees, 0 to 360.</param>
internal readonly record struct OklchColor(double Lightness, double Chroma, double Hue)
{
    /// <summary>
    /// Converts a <c>#rgb</c> or <c>#rrggbb</c> color via <see cref="ColorConverter"/>. Returns
    /// <see langword="false"/> for anything else (including alpha forms, which a theme color cannot carry).
    /// </summary>
    /// <param name="hex">The color text, with or without the leading <c>#</c>.</param>
    /// <param name="color">The converted color when this returns <see langword="true"/>.</param>
    public static bool TryParseHex(string? hex, out OklchColor color)
    {
        color = default;
        var text = hex?.Trim() ?? string.Empty;

        if (text.Length > 0 && text[0] != '#')
            text = "#" + text;

        if (text.Length == 9 || !ColorConverter.TryHexToRgb(text, out var rgb))
            return false;

        var (l, c, h) = ColorConverter.RgbToOklch(rgb.R, rgb.G, rgb.B);
        color = new OklchColor(l, c, h);
        return true;
    }

    /// <summary>
    /// The color as sRGB (0-255 per channel), for contrast calculations.
    /// </summary>
    public (double R, double G, double B) ToRgb() => ColorConverter.OklchToRgb(Lightness, Chroma, Hue);

    /// <summary>
    /// The WCAG contrast ratio (1 to 21) between this color and <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The other color as sRGB (0-255 per channel).</param>
    public double ContrastWith((double R, double G, double B) other) => ColorConverter.GetContrastRatio(ToRgb(), other);

    /// <summary>
    /// Formats the color as CSS, e.g. <c>oklch(55.8% 0.288 302.3)</c>.
    /// </summary>
    public string ToCss() => Format(' ');

    /// <summary>
    /// Formats the color as a Tailwind arbitrary value, e.g. <c>[oklch(55.8%_0.288_302.3)]</c>, ready to
    /// follow a utility prefix such as <c>bg-</c>.
    /// </summary>
    public string ToTailwindValue() => $"[{Format('_')}]";

    private string Format(char separator) => string.Create(
        CultureInfo.InvariantCulture,
        $"oklch({Math.Round(Lightness * 100, 1)}%{separator}{Math.Round(Chroma, 3)}{separator}{Math.Round(Hue, 1)})");
}
