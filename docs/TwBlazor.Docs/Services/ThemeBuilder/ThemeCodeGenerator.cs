using System.Text;
using System.Text.RegularExpressions;
using TwBlazor.Enums;

namespace TwBlazor.Docs.Services.ThemeBuilder;

/// <summary>
/// Produces a complete, compilable <c>Theme.cs</c> from the default theme: only the chosen component themes,
/// only the shared utilities they use, and the customized semantic colors swapped in.
/// </summary>
/// <remarks>
/// The default theme is the template, so this never carries its own copy of any class string. Colors are
/// replaced textually: <c>bg-purple-600</c> becomes <c>bg-[oklch(...)]</c>, an arbitrary value Tailwind
/// compiles from <c>Theme.cs</c> like any other class. A color left on its default is not touched, so an
/// unmodified theme keeps its readable Tailwind names.
/// </remarks>
internal static partial class ThemeCodeGenerator
{
    [GeneratedRegex(@"[A-Za-z_]\w*", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex IdentifierRegex();

    /// <summary>
    /// Generates the file.
    /// </summary>
    /// <param name="template">The default theme, split into pieces.</param>
    /// <param name="themeTypes">The component theme types to include, e.g. from <see cref="ThemeTemplate.ResolveThemeTypes"/>.</param>
    /// <param name="colors">Picked hex color per <see cref="ThemeColorFamily.Name"/>; missing or default colors are left unchanged.</param>
    /// <param name="defaults">The enum defaults to set; <see langword="null"/> keeps the default theme's own.</param>
    /// <param name="onColors">Text mode on filled colors per <see cref="ThemeColorFamily.Name"/>; missing means <see cref="OnColorMode.Auto"/>.</param>
    public static string Generate(
        ThemeTemplate template,
        IReadOnlySet<string> themeTypes,
        IReadOnlyDictionary<string, string> colors,
        ThemeDefaults? defaults = null,
        IReadOnlyDictionary<string, OnColorMode>? onColors = null)
    {
        var blocks = template.Blocks.Where(b => themeTypes.Contains(b.TypeName)).Select(b => b with { Text = ApplyBlockDefaults(b, defaults) }).ToList();

        var body = string.Join('\n', ApplyDefaults(template.ReturnPrefix, defaults), string.Join(",\n", blocks.Select(b => b.Text)), template.ReturnSuffix);
        body = ApplyOnColors(body, colors, onColors);
        var statements = KeepUsedStatements(template.Statements, body);

        var sb = new StringBuilder();
        foreach (var @using in template.Usings)
            sb.Append("using ").Append(@using).Append(";\n");

        sb.Append("\npublic static class Theme\n{\n    public static TwBlazorTheme CreateDefaultTheme()\n    {\n");
        sb.Append(string.Join("\n\n", statements.Select(s => ApplyDefaults(s.Text, defaults)))).Append("\n\n");
        sb.Append(body).Append("\n    }\n}\n");

        return ApplyColors(sb.ToString(), colors);
    }

    [GeneratedRegex(@"Filled = new\(\)\s*\{.*?\n\s*\},", RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FilledBlockRegex();

    /// <summary>
    /// Rewrites each customized color's filled surface (<c>SurfaceColors.Filled</c>, used by buttons and chips)
    /// with readable text: light or dark text, and hover and active shades that move away from the text's
    /// contrast rather than towards it. A color left on its default is untouched unless a mode was chosen.
    /// </summary>
    private static string ApplyOnColors(string body, IReadOnlyDictionary<string, string> colors, IReadOnlyDictionary<string, OnColorMode>? onColors)
    {
        var filled = FilledBlockRegex().Match(body);
        if (!filled.Success)
            return body;

        var block = filled.Value;

        foreach (var family in ThemeColorFamily.All)
        {
            colors.TryGetValue(family.Name, out var hex);
            var mode = onColors is not null && onColors.TryGetValue(family.Name, out var requested) ? requested : OnColorMode.Auto;

            if (family.ResolveOnColor(hex, mode) is { } resolved)
                block = ApplyOnColor(block, family, resolved);
        }

        return body[..filled.Index] + block + body[(filled.Index + filled.Length)..];
    }

    private static string ApplyOnColor(string block, ThemeColorFamily family, OnColorMode resolved)
    {
        var dark = resolved == OnColorMode.Dark;
        var step = dark ? -100 : 100;
        var text = dark ? ThemeOnColor.DarkClass : ThemeOnColor.LightClass;

        return Regex.Replace(
            block,
            $@"^(\s*{family.Name} = \$?"")(.*)("",?)\s*$",
            match =>
            {
                var content = Regex.Replace(match.Groups[2].Value, @"(\{text\.\w+\.\w+\}|text-(?:white|black|gray-\d+))$", text, RegexOptions.None, TimeSpan.FromSeconds(5));
                content = Regex.Replace(content, $@"hover:bg-{family.TailwindName}-\d+", $"hover:bg-{family.TailwindName}-{family.FilledShade + step}", RegexOptions.None, TimeSpan.FromSeconds(5));
                content = Regex.Replace(content, $@"active:bg-{family.TailwindName}-\d+", $"active:bg-{family.TailwindName}-{family.FilledShade + (2 * step)}", RegexOptions.None, TimeSpan.FromSeconds(5));
                return match.Groups[1].Value + content + match.Groups[3].Value;
            },
            RegexOptions.Multiline,
            TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Sets the global defaults that live in the shared utility statements (rounding and shadow) and in the
    /// component blocks (input and button style). A missing property is added to its block, which is how the
    /// button default works: the default theme leaves it to the component's built-in value.
    /// </summary>
    private static string ApplyDefaults(string text, ThemeDefaults? defaults)
    {
        if (defaults is null)
            return text;

        text = Regex.Replace(text, @"(DefaultRounded\s*=\s*Rounded\.)\w+", $"${{1}}{defaults.Rounded}", RegexOptions.None, TimeSpan.FromSeconds(5));
        return Regex.Replace(text, @"(DefaultShadow\s*=\s*Shadow\.)\w+", $"${{1}}{defaults.Shadow}", RegexOptions.None, TimeSpan.FromSeconds(5));
    }

    private static string ApplyBlockDefaults(ThemeBlock block, ThemeDefaults? defaults)
    {
        if (defaults is null)
            return block.Text;

        return block.TypeName switch
        {
            "TwInputTheme" => SetProperty(block.Text, "DefaultInputVariant", nameof(InputVariant), defaults.InputVariant.ToString()),
            "TwButtonTheme" => SetProperty(block.Text, "DefaultVariant", nameof(ButtonVariant), defaults.ButtonVariant.ToString(), omitWhenBuiltIn: ThemeDefaults.Default.ButtonVariant.ToString()),
            _ => block.Text,
        };
    }

    private static string SetProperty(string blockText, string property, string enumType, string value, string? omitWhenBuiltIn = null)
    {
        var existing = new Regex($@"({property}\s*=\s*{enumType}\.)\w+", RegexOptions.None, TimeSpan.FromSeconds(5));
        if (existing.IsMatch(blockText))
            return existing.Replace(blockText, $"${{1}}{value}");

        if (value == omitWhenBuiltIn)
            return blockText;

        // Insert as the first property, matching the block's own indentation.
        var open = blockText.IndexOf('{', StringComparison.Ordinal);
        var lineStart = blockText.LastIndexOf('\n', open) + 1;
        var indent = new string(' ', open - lineStart) + "    ";
        var lineEnd = blockText.IndexOf('\n', open);

        return blockText.Insert(lineEnd, $"\n{indent}{property} = {enumType}.{value},");
    }

    /// <summary>
    /// Keeps a statement only if the body, or another kept statement, mentions its name. Matching is by
    /// identifier, so it can keep something unused but never drop something needed.
    /// </summary>
    private static List<ThemeStatement> KeepUsedStatements(IReadOnlyList<ThemeStatement> statements, string body)
    {
        var referenced = Identifiers(body);
        var kept = new HashSet<ThemeStatement>();
        bool changed;

        do
        {
            changed = false;
            foreach (var statement in statements.Where(s => !kept.Contains(s) && referenced.Contains(s.Name)))
            {
                kept.Add(statement);
                referenced.UnionWith(Identifiers(statement.Text));
                changed = true;
            }
        }
        while (changed);

        return [.. statements.Where(kept.Contains)];
    }

    private static HashSet<string> Identifiers(string text) =>
        IdentifierRegex().Matches(text).Select(m => m.Value).ToHashSet(StringComparer.Ordinal);

    private static string ApplyColors(string code, IReadOnlyDictionary<string, string> colors)
    {
        foreach (var family in ThemeColorFamily.All)
        {
            if (!colors.TryGetValue(family.Name, out var hex) || !family.IsCustomized(hex) || ThemeColorRamp.Generate(hex) is not { } ramp)
                continue;

            code = Regex.Replace(
                code,
                $@"-{family.TailwindName}-(\d{{2,3}})(?!\d)",
                match => int.TryParse(match.Groups[1].Value, out var shade) && ramp.TryGetValue(shade, out var color)
                    ? "-" + color.ToTailwindValue()
                    : match.Value,
                RegexOptions.None,
                TimeSpan.FromSeconds(5));
        }

        return code;
    }
}
