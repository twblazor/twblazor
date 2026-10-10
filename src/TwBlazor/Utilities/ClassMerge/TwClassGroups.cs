// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Utilities.ClassMerge;

/// <summary>
/// One group of Tailwind utilities that set the same css property, so only the last one added is kept.
/// </summary>
/// <param name="Name">The group's name.</param>
/// <param name="Matches">Whether a utility (without variants, <c>!</c>, a leading <c>-</c> or a <c>/</c> modifier) belongs to the group.</param>
internal sealed record TwClassGroupRule(string Name, Func<string, bool> Matches);

/// <summary>
/// The built-in Tailwind groups and which groups a later class also overrides. Rules are checked in order, so a
/// specific prefix (<c>border-x</c>) must come before the general one (<c>border</c>).
/// </summary>
internal static class TwClassGroups
{
    private static readonly string[] _sides = ["", "-x", "-y", "-s", "-e", "-t", "-r", "-b", "-l"];

    private static readonly string[] _borderStyles = ["solid", "dashed", "dotted", "double", "hidden", "none"];

    private static readonly string[] _alignContent = ["normal", "center", "start", "end", "between", "around", "evenly", "baseline", "stretch"];

    private static readonly string[] _positions =
    [
        "top", "bottom", "left", "right", "center", "top-left", "top-right", "bottom-left", "bottom-right", "left-top", "left-bottom", "right-top", "right-bottom",
    ];

    private static readonly string[] _fontWeights = ["thin", "extralight", "light", "normal", "medium", "semibold", "bold", "extrabold", "black"];

    /// <summary>
    /// Gets the built-in rules, in the order they are checked.
    /// </summary>
    public static IReadOnlyList<TwClassGroupRule> Rules { get; } = BuildRules();

    /// <summary>
    /// Gets, for each group, the other groups a later class from it also removes.
    /// </summary>
    public static IReadOnlyDictionary<string, string[]> Conflicts { get; } = BuildConflicts();

    /// <summary>
    /// Creates a matcher for the utilities starting with <paramref name="prefix"/> followed by a dash.
    /// </summary>
    /// <param name="prefix">The prefix, e.g. <c>px</c>.</param>
    /// <param name="bare">Whether the prefix on its own (<c>border</c>) also matches.</param>
    /// <param name="value">When set, the value after the dash must also satisfy this.</param>
    public static Func<string, bool> Prefixed(string prefix, bool bare = false, Func<string, bool>? value = null) =>
        utility =>
        {
            if (utility == prefix)
                return bare;

            return utility.StartsWith(prefix + "-", StringComparison.Ordinal) && (value is null || value(utility[(prefix.Length + 1)..]));
        };

    private static Func<string, bool> Words(params string[] words)
    {
        var set = new HashSet<string>(words, StringComparer.Ordinal);
        return set.Contains;
    }

    private static bool IsWidth(string value) =>
        TwClassValue.IsNumber(value) || TwClassValue.IsArbitraryOf(value, "length", "number", "percentage");

    private static bool IsFontSize(string value) =>
        value == "base" || TwClassValue.IsTshirt(value) || TwClassValue.IsArbitraryOf(value, "length", "size");

    private static bool IsShadowSize(string value) =>
        value is "inner" or "none" or "2xs" or "xs" || TwClassValue.IsTshirt(value) || TwClassValue.IsArbitraryOf(value, "shadow", "any");

    private static bool IsFontWeight(string value) =>
        _fontWeights.Contains(value) || TwClassValue.IsNumber(value) || TwClassValue.IsArbitraryOf(value, "weight", "number");

    private static bool IsGradientImage(string value) =>
        value == "none"
        || value.StartsWith("gradient-", StringComparison.Ordinal)
        || value.StartsWith("linear-", StringComparison.Ordinal)
        || value.StartsWith("radial", StringComparison.Ordinal)
        || value.StartsWith("conic", StringComparison.Ordinal)
        || TwClassValue.IsArbitraryOf(value, "image");

    private static bool IsGradientStop(string value) =>
        value.EndsWith('%') || TwClassValue.IsArbitraryOf(value, "length");

    private static List<TwClassGroupRule> BuildRules()
    {
        List<TwClassGroupRule> rules = [];

        void Add(string name, Func<string, bool> matches) => rules.Add(new TwClassGroupRule(name, matches));
        void Prefix(string name, bool bare = false) => Add(name, Prefixed(name, bare));

        // Layout
        Add("display", Words("block", "inline-block", "inline", "flex", "inline-flex", "table", "inline-table", "table-caption", "table-cell", "table-column", "table-column-group", "table-footer-group", "table-header-group", "table-row-group", "table-row", "flow-root", "grid", "inline-grid", "contents", "list-item", "hidden"));
        Add("position", Words("static", "fixed", "absolute", "relative", "sticky"));
        Add("visibility", Words("visible", "invisible", "collapse"));
        Add("isolation", Words("isolate", "isolation-auto"));
        Add("box-sizing", Words("box-border", "box-content"));
        Add("container", Words("container"));
        Add("sr", Words("sr-only", "not-sr-only"));
        Prefix("aspect");
        Prefix("columns");
        Prefix("float");
        Prefix("clear");
        Prefix("z");
        Add("inset-shadow", Prefixed("inset-shadow", bare: true, value: IsShadowSize));
        Add("inset-shadow-color", Prefixed("inset-shadow"));
        Prefix("inset-x");
        Prefix("inset-y");
        Prefix("inset");
        Prefix("start");
        Prefix("end");
        Prefix("top");
        Prefix("right");
        Prefix("bottom");
        Prefix("left");
        Prefix("overflow-x");
        Prefix("overflow-y");
        Prefix("overflow");
        Prefix("overscroll-x");
        Prefix("overscroll-y");
        Prefix("overscroll");
        Add("object-fit", Prefixed("object", value: v => v is "contain" or "cover" or "fill" or "none" or "scale-down"));
        Prefix("object");

        // Flexbox and grid
        Add("flex-direction", Prefixed("flex", value: v => v is "row" or "row-reverse" or "col" or "col-reverse"));
        Add("flex-wrap", Prefixed("flex", value: v => v is "wrap" or "wrap-reverse" or "nowrap"));
        Prefix("flex");
        Prefix("basis");
        Prefix("grow", bare: true);
        Prefix("shrink", bare: true);
        Prefix("order");
        Prefix("grid-cols");
        Prefix("grid-rows");
        Prefix("grid-flow");
        Prefix("auto-cols");
        Prefix("auto-rows");
        Add("col-span", Prefixed("col", value: v => v.StartsWith("span-", StringComparison.Ordinal)));
        Prefix("col-start");
        Prefix("col-end");
        Prefix("col");
        Add("row-span", Prefixed("row", value: v => v.StartsWith("span-", StringComparison.Ordinal)));
        Prefix("row-start");
        Prefix("row-end");
        Prefix("row");
        Prefix("gap-x");
        Prefix("gap-y");
        Prefix("gap");
        Prefix("justify-items");
        Prefix("justify-self");
        Prefix("justify");
        Add("content-align", Prefixed("content", value: v => _alignContent.Contains(v)));
        Prefix("content");
        Prefix("items");
        Prefix("self");
        Prefix("place-content");
        Prefix("place-items");
        Prefix("place-self");

        // Spacing
        foreach (var spacing in new[] { "px", "py", "ps", "pe", "pt", "pr", "pb", "pl", "p", "mx", "my", "ms", "me", "mt", "mr", "mb", "ml", "m" })
            Prefix(spacing);

        Prefix("space-x");
        Prefix("space-y");

        // Sizing
        Prefix("size");
        Prefix("min-w");
        Prefix("max-w");
        Prefix("w");
        Prefix("min-h");
        Prefix("max-h");
        Prefix("h");

        // Typography
        Add("font-style", Words("italic", "not-italic"));
        Add("font-smoothing", Words("antialiased", "subpixel-antialiased"));
        Add("fvn-normal", Words("normal-nums"));
        Add("fvn-ordinal", Words("ordinal"));
        Add("fvn-slashed-zero", Words("slashed-zero"));
        Add("fvn-figure", Words("lining-nums", "oldstyle-nums"));
        Add("fvn-spacing", Words("proportional-nums", "tabular-nums"));
        Add("fvn-fraction", Words("diagonal-fractions", "stacked-fractions"));
        Add("text-transform", Words("uppercase", "lowercase", "capitalize", "normal-case"));
        Add("text-decoration", Words("underline", "overline", "line-through", "no-underline"));
        Add("text-overflow", Words("truncate", "text-ellipsis", "text-clip"));
        Add("text-wrap", Prefixed("text", value: v => v is "wrap" or "nowrap" or "balance" or "pretty"));
        Add("text-align", Prefixed("text", value: v => v is "left" or "center" or "right" or "justify" or "start" or "end"));
        Add("font-size", Prefixed("text", value: IsFontSize));
        Add("text-color", Prefixed("text"));
        Prefix("font-stretch");
        Add("font-weight", Prefixed("font", value: IsFontWeight));
        Add("font-family", Prefixed("font"));
        Prefix("leading");
        Prefix("tracking");
        Prefix("line-clamp");
        Add("list-position", Prefixed("list", value: v => v is "inside" or "outside"));
        Prefix("list");
        Prefix("underline-offset");
        Add("decoration-style", Prefixed("decoration", value: v => v is "solid" or "dashed" or "dotted" or "double" or "wavy"));
        Add("decoration-thickness", Prefixed("decoration", value: v => v is "auto" or "from-font" || IsWidth(v)));
        Add("decoration-color", Prefixed("decoration"));
        Prefix("whitespace");
        Prefix("break");
        Prefix("hyphens");
        Prefix("align");
        Prefix("indent");

        // Backgrounds
        Add("bg-attachment", Prefixed("bg", value: v => v is "fixed" or "local" or "scroll"));
        Prefix("bg-clip");
        Prefix("bg-origin");
        Prefix("bg-blend");
        Add("bg-repeat", Prefixed("bg", value: v => v is "repeat" or "no-repeat" or "repeat-x" or "repeat-y" or "repeat-round" or "repeat-space"));
        Add("bg-position", Prefixed("bg", value: v => _positions.Contains(v) || TwClassValue.IsArbitraryOf(v, "position")));
        Add("bg-size", Prefixed("bg", value: v => v is "auto" or "cover" or "contain" || TwClassValue.IsArbitraryOf(v, "length", "size")));
        Add("bg-image", Prefixed("bg", value: IsGradientImage));
        Add("bg-color", Prefixed("bg"));
        Add("gradient-from-pos", Prefixed("from", value: IsGradientStop));
        Add("gradient-via-pos", Prefixed("via", value: IsGradientStop));
        Add("gradient-to-pos", Prefixed("to", value: IsGradientStop));
        Prefix("from");
        Prefix("via");
        Prefix("to");

        // Borders
        Add("border-collapse", Words("border-collapse", "border-separate"));
        Prefix("border-spacing-x");
        Prefix("border-spacing-y");
        Prefix("border-spacing");
        Add("border-style", Prefixed("border", value: v => _borderStyles.Contains(v)));

        foreach (var side in _sides.Skip(1).Append(string.Empty))
            Add("border-w" + side, Prefixed("border" + side, bare: true, value: IsWidth));

        foreach (var side in _sides.Skip(1).Append(string.Empty))
            Add("border-color" + side, Prefixed("border" + side));

        foreach (var corner in new[] { "ss", "se", "ee", "es", "tl", "tr", "br", "bl", "s", "e", "t", "r", "b", "l" })
            Add("rounded-" + corner, Prefixed("rounded-" + corner, bare: true));

        Prefix("rounded", bare: true);
        Prefix("divide-x-reverse", bare: true);
        Prefix("divide-y-reverse", bare: true);
        Prefix("divide-x", bare: true);
        Prefix("divide-y", bare: true);
        Add("divide-style", Prefixed("divide", value: v => _borderStyles.Contains(v)));
        Add("divide-color", Prefixed("divide"));
        Prefix("outline-offset");
        Add("outline-style", Prefixed("outline", value: v => v is "none" or "hidden" or "solid" or "dashed" or "dotted" or "double"));
        Add("outline-w", Prefixed("outline", bare: true, value: IsWidth));
        Add("outline-color", Prefixed("outline"));
        Add("ring-inset", Words("ring-inset"));
        Add("ring-offset-w", Prefixed("ring-offset", value: IsWidth));
        Add("ring-offset-color", Prefixed("ring-offset"));
        Add("ring-w", Prefixed("ring", bare: true, value: IsWidth));
        Add("ring-color", Prefixed("ring"));

        // Effects
        Add("shadow", Prefixed("shadow", bare: true, value: IsShadowSize));
        Add("shadow-color", Prefixed("shadow"));
        Prefix("opacity");
        Prefix("mix-blend");

        // Filters
        Add("filter", Words("filter", "filter-none"));
        Prefix("blur", bare: true);
        Prefix("brightness");
        Prefix("contrast");
        Prefix("drop-shadow", bare: true);
        Prefix("grayscale", bare: true);
        Prefix("hue-rotate");
        Prefix("invert", bare: true);
        Prefix("saturate");
        Prefix("sepia", bare: true);
        Add("backdrop-filter", Words("backdrop-filter", "backdrop-filter-none"));
        Prefix("backdrop-blur", bare: true);
        Prefix("backdrop-brightness");
        Prefix("backdrop-contrast");
        Prefix("backdrop-grayscale", bare: true);
        Prefix("backdrop-hue-rotate");
        Prefix("backdrop-invert", bare: true);
        Prefix("backdrop-opacity");
        Prefix("backdrop-saturate");
        Prefix("backdrop-sepia", bare: true);

        // Transitions and animation
        Prefix("transition", bare: true);
        Prefix("duration");
        Prefix("ease");
        Prefix("delay");
        Prefix("animate");

        // Transforms
        Add("transform", Words("transform", "transform-gpu", "transform-none"));
        Prefix("scale-x");
        Prefix("scale-y");
        Prefix("scale");
        Prefix("rotate");
        Prefix("translate-x");
        Prefix("translate-y");
        Prefix("skew-x");
        Prefix("skew-y");
        Prefix("origin");

        // Interactivity
        Prefix("accent");
        Prefix("appearance");
        Prefix("caret");
        Prefix("cursor");
        Prefix("pointer-events");
        Prefix("resize", bare: true);
        Add("scroll-behavior", Words("scroll-auto", "scroll-smooth"));
        Prefix("snap");
        Prefix("touch");
        Prefix("select");
        Prefix("will-change");

        // SVG
        Prefix("fill");
        Add("stroke-w", Prefixed("stroke", value: IsWidth));
        Add("stroke-color", Prefixed("stroke"));

        return rules;
    }

    private static Dictionary<string, string[]> BuildConflicts()
    {
        var conflicts = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["p"] = ["px", "py", "ps", "pe", "pt", "pr", "pb", "pl"],
            ["px"] = ["pr", "pl"],
            ["py"] = ["pt", "pb"],
            ["m"] = ["mx", "my", "ms", "me", "mt", "mr", "mb", "ml"],
            ["mx"] = ["mr", "ml"],
            ["my"] = ["mt", "mb"],
            ["inset"] = ["inset-x", "inset-y", "start", "end", "top", "right", "bottom", "left"],
            ["inset-x"] = ["right", "left"],
            ["inset-y"] = ["top", "bottom"],
            ["gap"] = ["gap-x", "gap-y"],
            ["overflow"] = ["overflow-x", "overflow-y"],
            ["overscroll"] = ["overscroll-x", "overscroll-y"],
            ["size"] = ["w", "h"],
            ["scale"] = ["scale-x", "scale-y"],
            ["font-size"] = ["leading"],
            ["rounded"] = ["rounded-s", "rounded-e", "rounded-t", "rounded-r", "rounded-b", "rounded-l", "rounded-ss", "rounded-se", "rounded-ee", "rounded-es", "rounded-tl", "rounded-tr", "rounded-br", "rounded-bl"],
            ["rounded-s"] = ["rounded-ss", "rounded-es"],
            ["rounded-e"] = ["rounded-se", "rounded-ee"],
            ["rounded-t"] = ["rounded-tl", "rounded-tr"],
            ["rounded-r"] = ["rounded-tr", "rounded-br"],
            ["rounded-b"] = ["rounded-br", "rounded-bl"],
            ["rounded-l"] = ["rounded-tl", "rounded-bl"],
            ["fvn-normal"] = ["fvn-ordinal", "fvn-slashed-zero", "fvn-figure", "fvn-spacing", "fvn-fraction"],
            ["border-spacing"] = ["border-spacing-x", "border-spacing-y"],
        };

        foreach (var property in new[] { "border-w", "border-color" })
        {
            conflicts[property] = [.. _sides.Skip(1).Select(side => property + side)];
            conflicts[property + "-x"] = [property + "-r", property + "-l"];
            conflicts[property + "-y"] = [property + "-t", property + "-b"];
        }

        return conflicts;
    }
}
