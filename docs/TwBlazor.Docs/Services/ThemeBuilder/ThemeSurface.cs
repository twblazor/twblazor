using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TwBlazor.Docs.Services.ThemeBuilder;

/// <summary>
/// The picked neutral surface colors: the card and panel background in light mode and in dark mode, and the page
/// body behind them.
/// </summary>
/// <param name="Light">The light mode background as hex.</param>
/// <param name="Dark">The dark mode background as hex.</param>
/// <param name="BodyLight">The light mode page body as hex, or <see langword="null"/> to keep the default body's
/// distance from <paramref name="Light"/>.</param>
/// <param name="BodyDark">The dark mode page body as hex, or <see langword="null"/> to keep the default body's
/// distance from <paramref name="Dark"/>.</param>
internal sealed record ThemeSurfaces(string Light, string Dark, string? BodyLight = null, string? BodyDark = null)
{
    /// <summary>
    /// What the default theme uses.
    /// </summary>
    public static ThemeSurfaces Default { get; } = new(ThemeSurface.DefaultLightHex, ThemeSurface.DefaultDarkHex);
}

/// <summary>
/// Recolors the default theme's neutral surfaces from one picked background per mode.
/// </summary>
/// <remarks>
/// The default theme writes its neutral surfaces as literal <c>oklch()</c> values: a family of grays for light
/// mode and a family of blue-grays for dark mode, covering backgrounds, borders, tracks and popovers. The two
/// families share no value, so a value alone says which mode it belongs to. Picking a background moves its
/// whole family: each member keeps its lightness distance from the background and takes the picked hue.
/// </remarks>
internal static partial class ThemeSurface
{
    /// <summary>
    /// The default light mode background.
    /// </summary>
    public const string DefaultLightHex = "#ffffff";

    private const string scope = "#theme-builder";

    private static readonly OklchColor _defaultLight = new(1, 0, 0);
    private static readonly OklchColor _defaultDark = new(0.28, 0.016, 252.42);

    // The default theme's page body (NeutralSurface.Body), a step away from the background in each mode.
    private static readonly OklchColor _defaultBodyLight = new(0.985, 0, 0);
    private static readonly OklchColor _defaultBodyDark = new(0.20, 0.015, 253);

    // The default theme's heading text in each mode, which has to stay readable on the picked background.
    private static readonly OklchColor _lightText = new(0.21, 0.006, 285.885);
    private static readonly OklchColor _darkText = new(0.97807, 0.029, 256.847);

    /// <summary>
    /// The default dark mode background as hex.
    /// </summary>
    public static string DefaultDarkHex { get; } = ToHex(_defaultDark);

    // A light surface is a pure gray; a dark surface is the blue-gray whose hue sits between 252 and 255.
    [GeneratedRegex(@"oklch\((?<l>\d+(?:\.\d+)?)%_(?<c>0|0\.01\d)_(?<h>0|25[2-4](?:\.\d+)?)\)", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SurfaceValueRegex();

    [GeneratedRegex(@"[^\s""{}$]*\[oklch\([^\]]*\)\][^\s""{}]*", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ColorTokenRegex();

    /// <summary>
    /// Backgrounds faintly tinted toward a hue: a near-white for light mode and the default dark background's
    /// lightness for dark mode, so the theme's text keeps its contrast while the page takes on a little color.
    /// </summary>
    /// <param name="hue">The OKLCH hue to lean toward, in degrees.</param>
    /// <param name="strength">How strongly to tint, from 0 (no tint) to 1.</param>
    /// <param name="lightnessShift">A small change in lightness, applied in full to the dark background and by half
    /// to the light one, so themes of similar hue still get backgrounds of their own.</param>
    /// <returns>The tinted light and dark backgrounds.</returns>
    public static ThemeSurfaces Tinted(double hue, double strength, double lightnessShift = 0)
    {
        var amount = Math.Clamp(strength, 0, 1);
        var shift = Math.Clamp(lightnessShift, -0.02, 0.02);

        return new(
            ToHex(new OklchColor(Math.Min(0.995, 0.985 + (shift / 2)), 0.012 * amount, hue)),
            ToHex(new OklchColor(_defaultDark.Lightness + shift, 0.03 * amount, hue)));
    }

    /// <summary>
    /// Pastel backgrounds: a clearly tinted light mode page that still keeps the theme's text at AAA, and the same
    /// dark mode background as <see cref="Tinted"/> at full strength.
    /// </summary>
    /// <param name="hue">The OKLCH hue to lean toward, in degrees.</param>
    /// <param name="lightnessShift">A small change in lightness, as for <see cref="Tinted"/>.</param>
    /// <param name="darkChroma">How colorful the dark background is.</param>
    /// <returns>The pastel light and dark backgrounds.</returns>
    public static ThemeSurfaces Pastel(double hue, double lightnessShift = 0, double darkChroma = 0.03)
    {
        var shift = Math.Clamp(lightnessShift, -0.02, 0.02);
        var light = new OklchColor(0.972 + (shift / 2), 0.024, hue);

        // Near white, some hues (blues and violets especially) fall outside sRGB at this chroma, and rounding to hex
        // would quietly drop most of the tint. A touch darker makes room for it; past that the chroma eases instead,
        // so the page stays light.
        while (!KeepsItsTint(light) && light.Chroma > 0.016)
        {
            light = light.Lightness - 0.002 >= 0.962
                ? light with { Lightness = light.Lightness - 0.002 }
                : light with { Chroma = light.Chroma - 0.001 };
        }

        return new(
            ToHex(light),
            ToHex(new OklchColor(_defaultDark.Lightness + shift, darkChroma, hue)));
    }

    /// <summary>
    /// Gets whether either background differs from the default theme's.
    /// </summary>
    /// <param name="surfaces">The picked backgrounds.</param>
    public static bool IsCustomized(ThemeSurfaces surfaces) =>
        IsCustomized(surfaces.Light, DefaultLightHex)
        || IsCustomized(surfaces.Dark, DefaultDarkHex)
        || IsCustomized(surfaces.BodyLight, ToHex(_defaultBodyLight))
        || IsCustomized(surfaces.BodyDark, ToHex(_defaultBodyDark));

    /// <summary>
    /// The page body a mode ends up with: the one picked for it, or else the default body moved along with the
    /// picked background.
    /// </summary>
    /// <param name="surfaces">The picked backgrounds.</param>
    /// <param name="dark">Whether to give the dark mode body.</param>
    /// <returns>The body as hex.</returns>
    public static string EffectiveBody(ThemeSurfaces surfaces, bool dark)
    {
        if ((dark ? surfaces.BodyDark : surfaces.BodyLight) is { } picked)
            return picked;

        var body = dark ? _defaultBodyDark : _defaultBodyLight;

        return ToHex(Recolor(body, surfaces) ?? body);
    }

    /// <summary>
    /// The WCAG contrast ratio of the default theme's heading text on a picked background, or
    /// <see langword="null"/> when <paramref name="hex"/> isn't a valid color.
    /// </summary>
    /// <param name="hex">The picked background.</param>
    /// <param name="dark">Whether it is the dark mode background.</param>
    public static double? TextContrast(string? hex, bool dark)
    {
        if (!OklchColor.TryParseHex(hex, out var background))
            return null;

        var text = dark ? _darkText : _lightText;

        return text.ContrastWith(background.ToRgb());
    }

    /// <summary>
    /// Rewrites every neutral surface value in a theme file for the picked backgrounds. A background left on
    /// its default leaves that mode's values untouched.
    /// </summary>
    /// <param name="code">The theme file.</param>
    /// <param name="surfaces">The picked backgrounds.</param>
    public static string ApplyToCode(string code, ThemeSurfaces surfaces) =>
        IsCustomized(surfaces)
            ? SurfaceValueRegex().Replace(code, match => Recolor(match, surfaces) is { } color ? color.ToTailwindValue()[1..^1] : match.Value)
            : code;

    /// <summary>
    /// Builds the style rules that show the picked backgrounds on the theme builder page, or an empty string
    /// when both are on their defaults.
    /// </summary>
    /// <remarks>
    /// The page runs on the site's registered theme, whose surface classes carry literal values that no custom
    /// property can reach. So each of those classes gets a rule of its own, matched by the class name and
    /// limited to the page's element. The rules join Tailwind's own <c>utilities</c> layer, so they compete with
    /// the compiled stylesheet exactly as the class they replace does.
    /// </remarks>
    /// <param name="template">The default theme, whose text names every surface class.</param>
    /// <param name="surfaces">The picked backgrounds.</param>
    public static string BuildCss(ThemeTemplate template, ThemeSurfaces surfaces)
    {
        if (!IsCustomized(surfaces))
            return string.Empty;

        var sb = new StringBuilder("@layer utilities {\n");

        foreach (var token in SurfaceTokens(template))
        {
            if (Translate(token, surfaces) is { } rule)
                sb.Append(rule).Append('\n');
        }

        return sb.Append('}').ToString();
    }

    /// <summary>
    /// Every distinct class in the default theme that sets a neutral surface value, with plain classes before
    /// their variants and light mode before dark, so a later rule may override an earlier one as Tailwind's do.
    /// </summary>
    /// <param name="template">The default theme.</param>
    public static IReadOnlyList<string> SurfaceTokens(ThemeTemplate template)
    {
        var text = string.Join('\n', template.Statements.Select(s => s.Text).Concat(template.Blocks.Select(b => b.Text)).Append(template.ReturnPrefix).Append(template.ReturnSuffix));

        return
        [
            .. ColorTokenRegex().Matches(text)
                .Select(m => m.Value)
                .Where(token => SurfaceValueRegex().IsMatch(token))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(token => SplitVariants(token).Variants.Contains("dark") ? 1 : 0)
                .ThenBy(token => SplitVariants(token).Variants.Count)
                .ThenBy(token => token, StringComparer.Ordinal)
        ];
    }

    /// <summary>
    /// Translates one surface class into the rule that recolors it on the page, or <see langword="null"/> when
    /// the class uses a variant or utility this does not know.
    /// </summary>
    /// <param name="token">The class, e.g. <c>dark:hover:bg-[oklch(23.26%_0.014_253.1)]</c>.</param>
    /// <param name="surfaces">The picked backgrounds.</param>
    public static string? Translate(string token, ThemeSurfaces surfaces)
    {
        var (variants, utility) = SplitVariants(token);
        var value = SurfaceValueRegex().Match(utility);
        var important = utility.StartsWith('!');
        var name = utility[(important ? 1 : 0)..value.Index].TrimEnd('[').TrimEnd('-');

        if (!value.Success || !utility.EndsWith(value.Value + "]", StringComparison.Ordinal) || Property(name) is not { } property)
            return null;

        // A value that is not recolored is written exactly as the theme has it: reading it in and formatting it again
        // can round it to a slightly different color.
        var color = Recolor(value, surfaces)?.ToCss() ?? value.Value.Replace('_', ' ');
        var dark = variants.Contains("dark");

        // The mode test sits on the element that carries the class, so it reads the same whatever follows it.
        var selector = $"[class~=\"{token}\"]:where({(dark ? ".dark, .dark *" : ":not(.dark, .dark *)")})";
        var wrappers = new List<string>();

        // Variants apply from the utility outwards.
        for (var i = variants.Count - 1; i >= 0; i--)
        {
            if (variants[i] == "dark")
                continue;

            if (!ApplyVariant(variants[i], ref selector, wrappers))
                return null;
        }

        if (name == "divide")
            selector = $":where({selector} > :not(:last-child))";

        var rule = $":where({scope}) {selector} {{ {property}: {color}{(important ? " !important" : string.Empty)}; }}";

        return wrappers.Aggregate(rule, (inner, wrapper) => $"{wrapper} {{ {inner} }}");
    }

    private static bool ApplyVariant(string variant, ref string selector, List<string> wrappers)
    {
        switch (variant)
        {
            case "hover":
                selector += ":hover";
                wrappers.Add("@media (hover: hover)");
                return true;
            case "focus":
            case "focus-visible":
                selector += ":" + variant;
                return true;
            case "peer-focus-visible":
                selector += ":is(:where(.peer):focus-visible ~ *)";
                return true;
        }

        if (Inner(variant, "has-") is { } has)
        {
            selector += $":has({has})";
            return true;
        }

        if (Inner(variant, "data-") is { } data)
        {
            var parts = data.Split('=', 2);
            selector += parts.Length == 2 ? $"[data-{parts[0]}=\"{parts[1]}\"]" : $"[data-{data}]";
            return true;
        }

        if (Inner(variant, "supports-") is { } supports)
        {
            wrappers.Add($"@supports ({supports})");
            return true;
        }

        if (Inner(variant, string.Empty) is { } arbitrary && arbitrary.StartsWith('&'))
        {
            selector += arbitrary[1..].Replace('_', ' ');
            return true;
        }

        return false;
    }

    private static string? Inner(string variant, string prefix) =>
        variant.StartsWith(prefix + "[", StringComparison.Ordinal) && variant.EndsWith(']') ? variant[(prefix.Length + 1)..^1] : null;

    private static string? Property(string utility) => utility switch
    {
        "bg" => "background-color",
        "border" or "divide" => "border-color",
        "text" => "color",
        "fill" => "fill",
        "stroke" => "stroke",
        "ring" => "--tw-ring-color",
        "ring-offset" => "--tw-ring-offset-color",
        _ => null,
    };

    /// <summary>
    /// Splits a class into its variants and its utility. A colon inside square brackets or parentheses belongs
    /// to an arbitrary variant, so it does not split.
    /// </summary>
    private static (List<string> Variants, string Utility) SplitVariants(string token)
    {
        var parts = new List<string>();
        var depth = 0;
        var start = 0;

        for (var i = 0; i < token.Length; i++)
        {
            switch (token[i])
            {
                case '[' or '(':
                    depth++;
                    break;
                case ']' or ')':
                    depth--;
                    break;
                case ':' when depth == 0:
                    parts.Add(token[start..i]);
                    start = i + 1;
                    break;
            }
        }

        return (parts, token[start..]);
    }

    private static OklchColor? Recolor(Match value, ThemeSurfaces surfaces) => Recolor(Parse(value), surfaces);

    private static OklchColor? Recolor(OklchColor original, ThemeSurfaces surfaces)
    {
        var dark = original.Chroma > 0;

        // The page body follows its own pick when there is one, rather than the background's.
        if (Same(original, dark ? _defaultBodyDark : _defaultBodyLight)
            && (dark ? surfaces.BodyDark : surfaces.BodyLight) is { } bodyHex
            && OklchColor.TryParseHex(bodyHex, out var body))
        {
            return body;
        }

        var hex = dark ? surfaces.Dark : surfaces.Light;
        var defaultBase = dark ? _defaultDark : _defaultLight;

        if (!IsCustomized(hex, dark ? DefaultDarkHex : DefaultLightHex) || !OklchColor.TryParseHex(hex, out var picked))
            return null;

        var lightness = Math.Clamp(original.Lightness + picked.Lightness - defaultBase.Lightness, 0, 1);

        return new OklchColor(lightness, picked.Chroma, picked.Hue);
    }

    private static bool Same(OklchColor a, OklchColor b) =>
        Math.Abs(a.Lightness - b.Lightness) < 0.0005 && Math.Abs(a.Chroma - b.Chroma) < 0.0005 && Math.Abs(a.Hue - b.Hue) < 0.05;

    private static OklchColor Parse(Match value) => new(
        double.Parse(value.Groups["l"].Value, CultureInfo.InvariantCulture) / 100,
        double.Parse(value.Groups["c"].Value, CultureInfo.InvariantCulture),
        double.Parse(value.Groups["h"].Value, CultureInfo.InvariantCulture));

    private static bool IsCustomized(string? hex, string defaultHex) =>
        OklchColor.TryParseHex(hex, out var picked) && OklchColor.TryParseHex(defaultHex, out var original) && picked.ToCss() != original.ToCss();

    private static string ToHex(OklchColor color)
    {
        var (r, g, b) = color.ToRgb();

        return string.Create(CultureInfo.InvariantCulture, $"#{Channel(r):x2}{Channel(g):x2}{Channel(b):x2}");
    }

    private static int Channel(double value) => (int)Math.Clamp(Math.Round(value), 0, 255);

    // Checked on the stored hex rather than the raw conversion, which clamps out of range channels.
    private static bool KeepsItsTint(OklchColor color) =>
        OklchColor.TryParseHex(ToHex(color), out var stored) && stored.Chroma >= color.Chroma * 0.9;
}