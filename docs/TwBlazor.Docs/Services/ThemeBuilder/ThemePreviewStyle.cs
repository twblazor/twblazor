using System.Text;

namespace TwBlazor.Docs.Services.ThemeBuilder;

/// <summary>
/// Builds the inline style that makes the theme builder's live preview show the picked colors.
/// </summary>
/// <remarks>
/// Tailwind compiles <c>bg-purple-600</c> to <c>var(--color-purple-600)</c>, so redefining those variables on
/// the preview's wrapper recolors every real component inside it using the already compiled stylesheet. No
/// runtime Tailwind build is needed, and nothing outside the wrapper is affected.
/// </remarks>
internal static class ThemePreviewStyle
{
    /// <summary>
    /// Returns the custom property declarations for every customized color, or an empty string when all
    /// colors are still on their defaults.
    /// </summary>
    /// <param name="colors">Picked hex color per <see cref="ThemeColorFamily.Name"/>.</param>
    public static string Build(IReadOnlyDictionary<string, string> colors)
    {
        var sb = new StringBuilder();

        foreach (var family in ThemeColorFamily.All)
        {
            if (!colors.TryGetValue(family.Name, out var hex) || !family.IsCustomized(hex) || ThemeColorRamp.Generate(hex) is not { } ramp)
                continue;

            foreach (var (shade, color) in ramp)
                sb.Append("--color-").Append(family.TailwindName).Append('-').Append(shade).Append(": ").Append(color.ToCss()).Append("; ");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// The inline <c>color</c> declaration that gives <paramref name="family"/>'s filled buttons and chips
    /// readable text in the preview, or an empty string to keep the default theme's text class. The preview
    /// runs on the docs site's own theme, so unlike the generated file it can't change the class itself.
    /// </summary>
    /// <param name="family">The semantic color.</param>
    /// <param name="colors">Picked hex color per <see cref="ThemeColorFamily.Name"/>.</param>
    /// <param name="onColors">Text mode per <see cref="ThemeColorFamily.Name"/>.</param>
    public static string FilledText(ThemeColorFamily family, IReadOnlyDictionary<string, string> colors, IReadOnlyDictionary<string, OnColorMode> onColors)
    {
        colors.TryGetValue(family.Name, out var hex);
        onColors.TryGetValue(family.Name, out var mode);

        return family.ResolveOnColor(hex, mode) is { } resolved ? ThemeOnColor.Style(resolved) : string.Empty;
    }
}
