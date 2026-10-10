// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.Text.RegularExpressions;

namespace TwBlazor.Utilities.ClassMerge;

/// <summary>
/// The checks a class group can apply to the value part of a utility (the <c>red-500</c> in <c>bg-red-500</c>).
/// Each one is the tailwind-merge validator of the same name without its <c>is</c> prefix, apart from
/// <see cref="ArbitraryPositionKeywords"/>, which is TwBlazor's own.
/// </summary>
internal enum TwClassValidator
{
    Any,
    AnyNonArbitrary,
    ArbitraryFamilyName,
    ArbitraryImage,
    ArbitraryLength,
    ArbitraryNumber,
    ArbitraryPosition,

    /// <summary>
    /// An unlabelled arbitrary value written with position keywords, such as <c>[right_0.5rem_center]</c>.
    /// </summary>
    ArbitraryPositionKeywords,
    ArbitraryShadow,
    ArbitrarySize,
    ArbitraryValue,
    ArbitraryVariable,
    ArbitraryVariableFamilyName,
    ArbitraryVariableImage,
    ArbitraryVariableLength,
    ArbitraryVariablePosition,
    ArbitraryVariableShadow,
    ArbitraryVariableSize,
    ArbitraryVariableWeight,
    ArbitraryWeight,
    Fraction,
    Integer,
    NamedContainerQuery,
    Number,
    Percent,
    TshirtSize,
}

/// <summary>
/// Classifies the value part of a Tailwind utility, which is what tells apart utilities that share a prefix such
/// as <c>text-sm</c> and <c>text-red-500</c>.
/// </summary>
/// <remarks>
/// A port of tailwind-merge's validators. An arbitrary value (<c>[...]</c>) is classified by its label
/// (<c>[length:...]</c>) when it has one, and by inspecting it otherwise. An arbitrary variable
/// (<c>(--my-var)</c>) can only be classified by its label (<c>(length:--my-var)</c>).
/// </remarks>
internal static partial class TwClassValue
{
    private const RegexOptions options = RegexOptions.ECMAScript;

    private const int timeout = 1000;

    [GeneratedRegex(@"^\[(?:(\w[\w-]*):)?(.+)\]$", options | RegexOptions.IgnoreCase, timeout)]
    private static partial Regex ArbitraryValueRegex();

    [GeneratedRegex(@"^\((?:(\w[\w-]*):)?(.+)\)$", options | RegexOptions.IgnoreCase, timeout)]
    private static partial Regex ArbitraryVariableRegex();

    [GeneratedRegex(@"^\d+(?:\.\d+)?/\d+(?:\.\d+)?$", options, timeout)]
    private static partial Regex FractionRegex();

    [GeneratedRegex(@"^(\d+(\.\d+)?)?(xs|sm|md|lg|xl)$", options, timeout)]
    private static partial Regex TshirtRegex();

    [GeneratedRegex(@"\d+(%|px|r?em|[sdl]?v([hwib]|min|max)|pt|pc|in|cm|mm|cap|ch|ex|r?lh|cq(w|h|i|b|min|max))|\b(calc|min|max|clamp)\(.+\)|^0$", options, timeout)]
    private static partial Regex LengthUnitRegex();

    [GeneratedRegex(@"^(rgba?|hsla?|hwb|(ok)?(lab|lch)|color-mix|color|light-dark)\(.+\)$", options, timeout)]
    private static partial Regex ColorFunctionRegex();

    [GeneratedRegex(@"^(inset_)?-?((\d+)?\.?(\d+)[a-z]+|0)_-?((\d+)?\.?(\d+)[a-z]+|0)", options, timeout)]
    private static partial Regex ShadowRegex();

    [GeneratedRegex(@"^(url|image|image-set|cross-fade|element|(repeating-)?(linear|radial|conic)-gradient)\(.+\)$", options, timeout)]
    private static partial Regex ImageRegex();

    [GeneratedRegex(@"(^|[_-])(left|right|top|bottom|center)([_-]|$)", options, timeout)]
    private static partial Regex PositionKeywordRegex();

    [GeneratedRegex(@"^[+-]?(\d+\.?\d*|\.\d+)([eE][+-]?\d+)?$", options, timeout)]
    private static partial Regex DecimalRegex();

    [GeneratedRegex(@"^0([xX][0-9a-fA-F]+|[oO][0-7]+|[bB][01]+)$", options, timeout)]
    private static partial Regex RadixIntegerRegex();

    /// <summary>
    /// Gets the check for <paramref name="validator"/>.
    /// </summary>
    /// <param name="validator">The validator to get.</param>
    /// <returns>A function that says whether a utility's value passes the check.</returns>
    public static Func<string, bool> Get(TwClassValidator validator) =>
        validator switch
        {
            TwClassValidator.Any => _ => true,
            TwClassValidator.AnyNonArbitrary => value => !IsArbitraryValue(value) && !IsArbitraryVariable(value),
            TwClassValidator.ArbitraryFamilyName => value => IsArbitraryValue(value, IsFamilyNameLabel, _ => false),
            TwClassValidator.ArbitraryImage => value => IsArbitraryValue(value, IsImageLabel, ImageRegex().IsMatch),
            TwClassValidator.ArbitraryLength => value => IsArbitraryValue(value, IsLengthLabel, IsLength),
            TwClassValidator.ArbitraryNumber => value => IsArbitraryValue(value, IsNumberLabel, IsNumber),
            TwClassValidator.ArbitraryPosition => value => IsArbitraryValue(value, IsPositionLabel, _ => false),
            TwClassValidator.ArbitraryPositionKeywords => value => IsArbitraryValue(value, _ => false, IsPositionKeywords),
            TwClassValidator.ArbitraryShadow => value => IsArbitraryValue(value, IsShadowLabel, ShadowRegex().IsMatch),
            TwClassValidator.ArbitrarySize => value => IsArbitraryValue(value, IsSizeLabel, _ => false),
            TwClassValidator.ArbitraryValue => IsArbitraryValue,
            TwClassValidator.ArbitraryVariable => IsArbitraryVariable,
            TwClassValidator.ArbitraryVariableFamilyName => value => IsArbitraryVariable(value, IsFamilyNameLabel),
            TwClassValidator.ArbitraryVariableImage => value => IsArbitraryVariable(value, IsImageLabel),
            TwClassValidator.ArbitraryVariableLength => value => IsArbitraryVariable(value, IsLengthLabel),
            TwClassValidator.ArbitraryVariablePosition => value => IsArbitraryVariable(value, IsPositionLabel),
            TwClassValidator.ArbitraryVariableShadow => value => IsArbitraryVariable(value, IsShadowLabel, matchNoLabel: true),
            TwClassValidator.ArbitraryVariableSize => value => IsArbitraryVariable(value, IsSizeLabel),
            TwClassValidator.ArbitraryVariableWeight => value => IsArbitraryVariable(value, IsWeightLabel, matchNoLabel: true),
            TwClassValidator.ArbitraryWeight => value => IsArbitraryValue(value, IsWeightLabel, _ => true),
            TwClassValidator.Fraction => IsFraction,
            TwClassValidator.Integer => IsInteger,
            TwClassValidator.NamedContainerQuery => IsNamedContainerQuery,
            TwClassValidator.Number => IsNumber,
            TwClassValidator.Percent => IsPercent,
            TwClassValidator.TshirtSize => IsTshirtSize,
            _ => throw new ArgumentOutOfRangeException(nameof(validator), validator, null),
        };

    /// <summary>
    /// Gets whether <paramref name="value"/> is an arbitrary value, such as <c>[10px]</c> or <c>[color:red]</c>.
    /// </summary>
    /// <param name="value">The value part of the utility.</param>
    public static bool IsArbitraryValue(string value) => ArbitraryValueRegex().IsMatch(value);

    /// <summary>
    /// Gets whether <paramref name="value"/> is an arbitrary variable, such as <c>(--my-var)</c>.
    /// </summary>
    /// <param name="value">The value part of the utility.</param>
    public static bool IsArbitraryVariable(string value) => ArbitraryVariableRegex().IsMatch(value);

    /// <summary>
    /// Gets whether <paramref name="value"/> is a fraction such as <c>1/2</c>.
    /// </summary>
    /// <param name="value">The value part of the utility.</param>
    public static bool IsFraction(string value) => FractionRegex().IsMatch(value);

    /// <summary>
    /// Gets whether <paramref name="value"/> is a number such as <c>4</c>, <c>0.5</c> or <c>1e3</c>.
    /// </summary>
    /// <param name="value">The value part of the utility.</param>
    public static bool IsNumber(string value) => TryParseNumber(value, out _);

    /// <summary>
    /// Gets whether <paramref name="value"/> is a whole number such as <c>4</c> or <c>4.0</c>.
    /// </summary>
    /// <param name="value">The value part of the utility.</param>
    public static bool IsInteger(string value) =>
        TryParseNumber(value, out var number) && double.IsFinite(number) && Math.Floor(number) == number;

    /// <summary>
    /// Gets whether <paramref name="value"/> is a percentage such as <c>50%</c>.
    /// </summary>
    /// <param name="value">The value part of the utility.</param>
    public static bool IsPercent(string value) => value.EndsWith('%') && IsNumber(value[..^1]);

    /// <summary>
    /// Gets whether <paramref name="value"/> is a t-shirt size such as <c>sm</c> or <c>2xl</c>.
    /// </summary>
    /// <param name="value">The value part of the utility.</param>
    public static bool IsTshirtSize(string value) => TshirtRegex().IsMatch(value);

    /// <summary>
    /// Gets whether <paramref name="value"/> is a named container, such as <c>@container/main</c>,
    /// <c>@container-size/main</c> or <c>@container-normal/main</c>.
    /// </summary>
    /// <param name="value">The whole class.</param>
    public static bool IsNamedContainerQuery(string value)
    {
        const string container = "@container";

        if (!value.StartsWith(container, StringComparison.Ordinal))
            return false;

        var rest = value[container.Length..];

        return HasNameAfter(rest, "/") || HasNameAfter(rest, "-size/") || HasNameAfter(rest, "-normal/");
    }

    private static bool HasNameAfter(string rest, string separator) =>
        rest.Length > separator.Length && rest.StartsWith(separator, StringComparison.Ordinal);

    // Color functions can hold percentages, so hsl(0 0% 0%) would otherwise be read as a length.
    private static bool IsLength(string value) => LengthUnitRegex().IsMatch(value) && !ColorFunctionRegex().IsMatch(value);

    private static bool IsPositionKeywords(string value) =>
        PositionKeywordRegex().IsMatch(value) && !ImageRegex().IsMatch(value) && !ColorFunctionRegex().IsMatch(value);

    private static bool IsArbitraryValue(string value, Func<string, bool> testLabel, Func<string, bool> testValue)
    {
        var match = ArbitraryValueRegex().Match(value);

        if (!match.Success)
            return false;

        return match.Groups[1].Length > 0 ? testLabel(match.Groups[1].Value) : testValue(match.Groups[2].Value);
    }

    private static bool IsArbitraryVariable(string value, Func<string, bool> testLabel, bool matchNoLabel = false)
    {
        var match = ArbitraryVariableRegex().Match(value);

        if (!match.Success)
            return false;

        return match.Groups[1].Length > 0 ? testLabel(match.Groups[1].Value) : matchNoLabel;
    }

    private static bool IsPositionLabel(string label) => label is "position" or "percentage";

    private static bool IsImageLabel(string label) => label is "image" or "url";

    private static bool IsSizeLabel(string label) => label is "length" or "size" or "bg-size";

    private static bool IsLengthLabel(string label) => label == "length";

    private static bool IsNumberLabel(string label) => label == "number";

    private static bool IsFamilyNameLabel(string label) => label == "family-name";

    private static bool IsWeightLabel(string label) => label is "number" or "weight";

    private static bool IsShadowLabel(string label) => label == "shadow";

    // Accepts what JavaScript's Number() does, since that is what tailwind-merge's number check is built on.
    private static bool TryParseNumber(string value, out double number)
    {
        number = 0;

        if (value.Length == 0)
            return false;

        if (RadixIntegerRegex().IsMatch(value))
            return true;

        if (value is "Infinity" or "+Infinity" or "-Infinity")
        {
            number = double.PositiveInfinity;
            return true;
        }

        return DecimalRegex().IsMatch(value) && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
    }
}
