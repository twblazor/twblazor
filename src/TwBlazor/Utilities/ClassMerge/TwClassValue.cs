// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.Text.RegularExpressions;

namespace TwBlazor.Utilities.ClassMerge;

/// <summary>
/// Classifies the value part of a Tailwind utility (the <c>red-500</c> in <c>bg-red-500</c>), which is what tells
/// apart utilities that share a prefix such as <c>text-sm</c> and <c>text-red-500</c>.
/// </summary>
internal static partial class TwClassValue
{
    private static readonly HashSet<string> _hints = new(StringComparer.Ordinal)
    {
        "length", "size", "position", "color", "image", "url", "percentage", "number", "family", "weight", "shadow", "angle", "integer",
    };

    private static readonly string[] _colorFunctions = ["#", "rgb", "hsl", "hwb", "lab", "lch", "oklab", "oklch", "color(", "color-mix("];

    [GeneratedRegex(@"^\[([a-z-]+):", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex HintRegex();

    [GeneratedRegex(@"^[+-]?\d*\.?\d+(px|rem|em|%|vh|vw|vmin|vmax|ch|ex|cap|lh|rlh|svh|lvh|dvh|svw|lvw|dvw|cm|mm|in|pt|pc|fr|deg|turn|rad)?$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex NumberWithUnitRegex();

    [GeneratedRegex(@"^(\d+(\.\d+)?)?(xs|sm|md|lg|xl)$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex TshirtRegex();

    [GeneratedRegex(@"^(inherit|current|transparent|black|white|(slate|gray|zinc|neutral|stone|taupe|mauve|mist|olive|red|orange|amber|yellow|lime|green|emerald|teal|cyan|sky|blue|indigo|violet|purple|fuchsia|pink|rose)-\d{2,4})$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex NamedColorRegex();

    [GeneratedRegex(@"(^|[_\-])(left|right|top|bottom|center)([_\-]|$)", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PositionKeywordRegex();

    [GeneratedRegex(@"^\d+/\d+$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex FractionRegex();

    /// <summary>
    /// Gets what an arbitrary value (<c>[...]</c> or the <c>(--var)</c> shorthand) holds, from its type hint
    /// (<c>[length:...]</c>) or by inspecting it.
    /// </summary>
    /// <param name="value">The value part of the utility.</param>
    /// <returns>
    /// <c>color</c>, <c>image</c>, <c>length</c>, <c>position</c>, <c>var</c> or <c>any</c> for an arbitrary value, a
    /// type hint as written (for example <c>family</c>), or <see langword="null"/> when the value is not arbitrary.
    /// </returns>
    public static string? Arbitrary(string value)
    {
        string inner;

        if (value.Length > 2 && value[0] == '[' && value[^1] == ']')
        {
            inner = value[1..^1];
        }
        else if (value.StartsWith("(--", StringComparison.Ordinal) && value[^1] == ')')
        {
            return "var";
        }
        else
        {
            return null;
        }

        if (HintRegex().Match(value) is { Success: true } hint && _hints.Contains(hint.Groups[1].Value))
        {
            return hint.Groups[1].Value switch
            {
                "url" => "image",
                "percentage" or "angle" or "integer" => "length",
                var other => other,
            };
        }

        if (_colorFunctions.Any(f => inner.StartsWith(f, StringComparison.Ordinal)))
            return "color";

        if (inner.StartsWith("url(", StringComparison.Ordinal) || inner.Contains("gradient(", StringComparison.Ordinal))
            return "image";

        if (NumberWithUnitRegex().IsMatch(inner) || inner.StartsWith("calc(", StringComparison.Ordinal) || inner.StartsWith("min(", StringComparison.Ordinal)
            || inner.StartsWith("max(", StringComparison.Ordinal) || inner.StartsWith("clamp(", StringComparison.Ordinal))
            return "length";

        if (inner.StartsWith("var(", StringComparison.Ordinal))
            return "var";

        return PositionKeywordRegex().IsMatch(inner) ? "position" : "any";
    }

    /// <summary>
    /// Gets whether <paramref name="value"/> is a plain number such as <c>4</c> or <c>0.5</c>.
    /// </summary>
    /// <param name="value">The value part of the utility.</param>
    public static bool IsNumber(string value) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _);

    /// <summary>
    /// Gets whether <paramref name="value"/> is a fraction such as <c>1/2</c>.
    /// </summary>
    /// <param name="value">The value part of the utility.</param>
    public static bool IsFraction(string value) => FractionRegex().IsMatch(value);

    /// <summary>
    /// Gets whether <paramref name="value"/> is a t-shirt size such as <c>sm</c> or <c>2xl</c>.
    /// </summary>
    /// <param name="value">The value part of the utility.</param>
    public static bool IsTshirt(string value) => TshirtRegex().IsMatch(value);

    /// <summary>
    /// Gets whether <paramref name="value"/> is a named palette color, such as <c>red-500</c> or <c>white</c>.
    /// </summary>
    /// <param name="value">The value part of the utility.</param>
    public static bool IsNamedColor(string value) => NamedColorRegex().IsMatch(value);

    /// <summary>
    /// Gets whether <paramref name="value"/> is an arbitrary value of one of the given kinds, see <see cref="Arbitrary"/>.
    /// </summary>
    /// <param name="value">The value part of the utility.</param>
    /// <param name="kinds">The kinds to accept.</param>
    public static bool IsArbitraryOf(string value, params string[] kinds) =>
        Arbitrary(value) is { } kind && kinds.Contains(kind, StringComparer.Ordinal);
}
