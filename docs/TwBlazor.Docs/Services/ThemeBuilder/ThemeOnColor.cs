namespace TwBlazor.Docs.Services.ThemeBuilder;

/// <summary>
/// Which text color to use on a filled semantic color (the text of a filled button or chip).
/// </summary>
public enum OnColorMode
{
    /// <summary>Pick whichever of light or dark text reads better on the color.</summary>
    Auto,

    /// <summary>Always use light text.</summary>
    Light,

    /// <summary>Always use dark text.</summary>
    Dark,
}

/// <summary>
/// Chooses readable text for a filled color, so a light brand color (white, pale yellow) doesn't end up with
/// unreadable light text on it.
/// </summary>
internal static class ThemeOnColor
{
    /// <summary>The light text class the default theme uses on filled colors (gray-100).</summary>
    public const string LightClass = "text-gray-100";

    /// <summary>The dark text class the default theme uses on filled colors (gray-950).</summary>
    public const string DarkClass = "text-gray-950";

    private const string lightHex = "#f3f4f6";
    private const string darkHex = "#030712";

    private static readonly (double R, double G, double B) _lightRgb = (243, 244, 246);
    private static readonly (double R, double G, double B) _darkRgb = (3, 7, 18);

    /// <summary>
    /// Resolves <see cref="OnColorMode.Auto"/> to light or dark text, whichever has the higher WCAG contrast
    /// against <paramref name="background"/>. An explicit mode is returned unchanged.
    /// </summary>
    /// <param name="mode">The requested mode.</param>
    /// <param name="background">The filled color the text sits on.</param>
    public static OnColorMode Resolve(OnColorMode mode, OklchColor background)
    {
        if (mode != OnColorMode.Auto)
            return mode;

        return background.ContrastWith(_darkRgb) > background.ContrastWith(_lightRgb) ? OnColorMode.Dark : OnColorMode.Light;
    }

    /// <summary>
    /// The WCAG contrast ratio of the resolved text color against <paramref name="background"/>.
    /// </summary>
    /// <param name="resolved">Light or dark; <see cref="OnColorMode.Auto"/> is resolved first.</param>
    /// <param name="background">The filled color the text sits on.</param>
    public static double Contrast(OnColorMode resolved, OklchColor background) =>
        background.ContrastWith(Resolve(resolved, background) == OnColorMode.Dark ? _darkRgb : _lightRgb);

    /// <summary>
    /// The inline <c>color</c> declaration for the resolved text color, used where the live preview can't change a class.
    /// </summary>
    /// <param name="resolved">Light or dark.</param>
    public static string Style(OnColorMode resolved) => $"color: {(resolved == OnColorMode.Dark ? darkHex : lightHex)};";
}
