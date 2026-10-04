using System.Text.Json;
using System.Text.RegularExpressions;

namespace TwBlazor.Docs.Compiler;

/// <summary>
/// Works out which component theme types each documented component needs, so the theme builder can include
/// everything a selected component depends on. A missing theme throws at runtime
/// (<c>TwBlazorComponents.Require</c>), so a component that renders other components (a date picker using
/// buttons and textfields, a calendar opening a dialog) needs their themes too.
/// </summary>
/// <remarks>
/// Every source file is keyed by its type name (the file name up to the first dot). A file's own theme
/// requirements are its <c>Require&lt;TwXxxTheme&gt;()</c> calls; its dependencies are every other known type
/// name its code mentions, found by scanning identifiers after stripping comments. The result is the
/// transitive closure, so it can over-include when a type is merely mentioned, never under-include.
/// </remarks>
public static partial class ComponentThemeDependencyScanner
{
    [GeneratedRegex(@"Require<(Tw\w+Theme)>", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RequireRegex();

    [GeneratedRegex(@"[A-Za-z_]\w*", RegexOptions.None, matchTimeoutMilliseconds: 5000)]
    private static partial Regex IdentifierRegex();

    [GeneratedRegex(@"/\*.*?\*/|@\*.*?\*@|//[^\n]*", RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex CommentRegex();

    /// <summary>
    /// Returns, for each name in <paramref name="componentNames"/>, the theme types it needs. A name with no
    /// matching source file maps to an empty list.
    /// </summary>
    /// <param name="sourceRoot">The library's source folder, scanned recursively for <c>.cs</c> and <c>.razor</c> files.</param>
    /// <param name="componentNames">The component type names to resolve, e.g. <c>TwDatePicker</c>.</param>
    public static Dictionary<string, List<string>> Scan(string sourceRoot, IEnumerable<string> componentNames)
    {
        var files = Directory.GetFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(sourceRoot, "*.razor", SearchOption.AllDirectories))
            .Where(f => !IsBuildOutput(f));

        var sources = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var file in files)
        {
            var stem = Path.GetFileName(file).Split('.')[0];
            var code = CommentRegex().Replace(File.ReadAllText(file), string.Empty);
            sources[stem] = sources.TryGetValue(stem, out var existing) ? existing + "\n" + code : code;
        }

        var themes = sources.ToDictionary(s => s.Key, s => RequireRegex().Matches(s.Value).Select(m => m.Groups[1].Value).ToHashSet(StringComparer.Ordinal));
        var references = sources.ToDictionary(
            s => s.Key,
            s => IdentifierRegex().Matches(s.Value).Select(m => m.Value).Where(id => id != s.Key && sources.ContainsKey(id)).ToHashSet(StringComparer.Ordinal));

        return componentNames
            .Distinct(StringComparer.Ordinal)
            .ToDictionary(name => name, name => Closure(name, themes, references));
    }

    /// <summary>
    /// Reads every <c>name</c> value out of <c>components.json</c>, including those of entries nested in groups.
    /// </summary>
    public static List<string> ReadComponentNames(string componentsJsonPath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(componentsJsonPath));
        var names = new List<string>();
        Collect(document.RootElement, names);
        return names;
    }

    private static void Collect(JsonElement element, List<string> names)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Array:
                foreach (var child in element.EnumerateArray())
                    Collect(child, names);
                break;
            case JsonValueKind.Object:
                if (element.TryGetProperty("name", out var name) && name.GetString() is { Length: > 0 } value)
                    names.Add(value);
                if (element.TryGetProperty("items", out var items))
                    Collect(items, names);
                break;
        }
    }

    private static List<string> Closure(string start, Dictionary<string, HashSet<string>> themes, Dictionary<string, HashSet<string>> references)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var queue = new Queue<string>();
        var result = new SortedSet<string>(StringComparer.Ordinal);

        if (themes.ContainsKey(start))
            queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!visited.Add(current))
                continue;

            result.UnionWith(themes[current]);
            foreach (var reference in references[current])
                queue.Enqueue(reference);
        }

        return [.. result];
    }

    private static bool IsBuildOutput(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
}
