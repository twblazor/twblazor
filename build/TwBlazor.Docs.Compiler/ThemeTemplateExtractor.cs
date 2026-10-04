using System.Text.RegularExpressions;

namespace TwBlazor.Docs.Compiler;

/// <summary>
/// A top-level <c>var</c> statement from <c>Theme.cs</c>'s shared "utility classes" region, together with any
/// follow-up assignments to it (e.g. <c>transition.ColorsFast = ...;</c>).
/// </summary>
/// <param name="Name">The declared variable's name.</param>
/// <param name="Text">The statement's source text, with its original indentation.</param>
public sealed record ThemeStatement(string Name, string Text);

/// <summary>
/// One entry of <c>Theme.cs</c>'s <c>Components = [ ... ]</c> list.
/// </summary>
/// <param name="TypeName">The component theme type the entry configures, e.g. <c>TwAlertTheme</c>.</param>
/// <param name="Text">The entry's source text without its trailing comma, with its original indentation.</param>
public sealed record ThemeBlock(string TypeName, string Text);

/// <summary>
/// <c>Theme.cs</c> split into the pieces the theme builder recombines: the shared statements, the fixed start
/// and end of the returned theme, and each individually selectable component theme.
/// </summary>
/// <param name="Usings">The namespaces the file imports, without the <c>using</c> keyword or semicolon.</param>
/// <param name="Statements">The shared utility statements, in source order.</param>
/// <param name="ReturnPrefix">Everything from <c>return new TwBlazorTheme</c> up to and including the opening bracket of the components list.</param>
/// <param name="Blocks">The component themes, in source order.</param>
/// <param name="ReturnSuffix">The closing bracket of the components list and the end of the returned theme.</param>
public sealed record ThemeTemplate(
    IReadOnlyList<string> Usings,
    IReadOnlyList<ThemeStatement> Statements,
    string ReturnPrefix,
    IReadOnlyList<ThemeBlock> Blocks,
    string ReturnSuffix);

/// <summary>
/// Splits <c>Theme.cs</c> into a <see cref="ThemeTemplate"/> so the docs' theme builder always works from the
/// real default theme rather than a hand-maintained copy: adding a component theme or a shared utility to
/// <c>Theme.cs</c> is picked up on the next build with no builder changes.
/// </summary>
/// <remarks>
/// Relies on <c>Theme.cs</c>'s layout: shared statements sit at 8 spaces inside <c>#region utility classes</c>,
/// the returned theme's <c>Components</c> list at 12 spaces, and each entry at 16 spaces.
/// </remarks>
public static partial class ThemeTemplateExtractor
{
    private const string utilityRegionMarker = "#region utility classes";
    private const string statementIndent = "        ";
    private const string componentsListIndent = "            ";
    private const string blockIndent = "                ";

    // Only needed for the hot reload attribute at the top of the docs' own Theme.cs.
    private static readonly string[] excludedUsings = ["System.Reflection.Metadata"];

    [GeneratedRegex(@"^using\s+([\w.]+);", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex UsingRegex();

    [GeneratedRegex(@"^var\s+(\w+)\s*=", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex DeclarationRegex();

    [GeneratedRegex(@"^(\w+)\s*[.\[]", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FollowUpAssignmentRegex();

    [GeneratedRegex(@"\bnew\s+(Tw\w+Theme)\b", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ThemeTypeRegex();

    /// <summary>Reads and splits the file at <paramref name="filePath"/>.</summary>
    public static ThemeTemplate ExtractFile(string filePath) => Extract(File.ReadAllText(filePath));

    /// <summary>Splits the source text of a <c>Theme.cs</c>-shaped file.</summary>
    /// <exception cref="InvalidOperationException">The source doesn't have the layout described in the type's remarks.</exception>
    public static ThemeTemplate Extract(string source)
    {
        var lines = source.Replace("\r\n", "\n").Split('\n');

        var usings = lines
            .Select(l => UsingRegex().Match(l))
            .Where(m => m.Success && !excludedUsings.Contains(m.Groups[1].Value))
            .Select(m => m.Groups[1].Value)
            .ToList();

        var regionStart = Array.FindIndex(lines, l => l.Contains(utilityRegionMarker, StringComparison.Ordinal));
        if (regionStart < 0)
            throw new InvalidOperationException($"'{utilityRegionMarker}' not found.");

        var regionEnd = Array.FindIndex(lines, regionStart, l => l.TrimStart().StartsWith("#endregion", StringComparison.Ordinal));
        if (regionEnd < 0)
            throw new InvalidOperationException($"'{utilityRegionMarker}' has no matching #endregion.");

        var statements = ParseStatements(lines[(regionStart + 1)..regionEnd]);

        var returnStart = Array.FindIndex(lines, regionEnd, l => l.TrimStart().StartsWith("return new TwBlazorTheme", StringComparison.Ordinal));
        if (returnStart < 0)
            throw new InvalidOperationException("'return new TwBlazorTheme' not found.");

        var componentsStart = Array.FindIndex(lines, returnStart, l => l == $"{componentsListIndent}Components =");
        if (componentsStart < 0 || lines[componentsStart + 1] != $"{componentsListIndent}[")
            throw new InvalidOperationException("The 'Components = [' list was not found.");

        var listOpen = componentsStart + 1;
        var listClose = Array.FindIndex(lines, listOpen + 1, l => l == $"{componentsListIndent}]");
        if (listClose < 0)
            throw new InvalidOperationException("The 'Components' list is not closed.");

        var blocks = ParseBlocks(lines[(listOpen + 1)..listClose], statements);

        var returnEnd = Array.FindIndex(lines, listClose, l => l.TrimStart().StartsWith("};", StringComparison.Ordinal));
        if (returnEnd < 0)
            throw new InvalidOperationException("The returned theme is not closed.");

        return new ThemeTemplate(
            usings,
            statements,
            string.Join('\n', lines[returnStart..(listOpen + 1)]),
            blocks,
            string.Join('\n', lines[listClose..(returnEnd + 1)]));
    }

    private static List<ThemeStatement> ParseStatements(string[] region)
    {
        var statements = new List<(string Name, List<string> Lines)>();
        var pending = new List<string>();
        (string Name, List<string> Lines)? current = null;

        foreach (var line in region)
        {
            var trimmed = line.Trim();
            var atStatementLevel = line.StartsWith(statementIndent, StringComparison.Ordinal)
                && !line.StartsWith(statementIndent + " ", StringComparison.Ordinal);

            if (atStatementLevel && trimmed.StartsWith("//", StringComparison.Ordinal))
            {
                pending.Add(line);
                continue;
            }

            if (atStatementLevel && DeclarationRegex().Match(trimmed) is { Success: true } declaration)
            {
                current = (declaration.Groups[1].Value, [.. pending, line]);
                statements.Add(current.Value);
                pending.Clear();
                continue;
            }

            if (atStatementLevel && FollowUpAssignmentRegex().Match(trimmed) is { Success: true } assignment)
            {
                var target = statements.FindLast(s => s.Name == assignment.Groups[1].Value);
                if (target.Lines is null)
                    throw new InvalidOperationException($"Assignment to undeclared variable '{assignment.Groups[1].Value}'.");

                target.Lines.AddRange(pending);
                target.Lines.Add(line);
                pending.Clear();
                current = target;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                if (pending.Count > 0)
                    pending.Add(line);
                continue;
            }

            current?.Lines.Add(line);
        }

        return [.. statements.Select(s => new ThemeStatement(s.Name, string.Join('\n', s.Lines)))];
    }

    private static List<ThemeBlock> ParseBlocks(string[] list, List<ThemeStatement> statements)
    {
        var groups = new List<List<string>>();

        foreach (var line in list)
        {
            var startsBlock = line.StartsWith(blockIndent, StringComparison.Ordinal)
                && !line.StartsWith(blockIndent + " ", StringComparison.Ordinal)
                && !line.TrimStart().StartsWith('}')
                && !line.TrimStart().StartsWith('{');

            if (startsBlock)
                groups.Add([line]);
            else if (groups.Count > 0)
                groups[^1].Add(line);
        }

        return [.. groups.Select(g => ToBlock(g, statements))];
    }

    private static ThemeBlock ToBlock(List<string> lines, List<ThemeStatement> statements)
    {
        var text = string.Join('\n', lines).TrimEnd();
        if (text.EndsWith(',')) text = text[..^1];

        var typeMatch = ThemeTypeRegex().Match(text);
        if (typeMatch.Success)
            return new ThemeBlock(typeMatch.Groups[1].Value, text);

        // A bare variable reference (e.g. "overlayTheme") - its type is found where it's declared.
        var variable = text.Trim();
        var declaration = statements.Find(s => s.Name == variable)
            ?? throw new InvalidOperationException($"Could not determine the theme type of component entry '{variable}'.");

        var declaredType = ThemeTypeRegex().Match(declaration.Text);
        return declaredType.Success
            ? new ThemeBlock(declaredType.Groups[1].Value, text)
            : throw new InvalidOperationException($"Variable '{variable}' is not a component theme.");
    }
}
