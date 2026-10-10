// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Collections.Concurrent;
using TwBlazor.Configuration;

namespace TwBlazor.Utilities.ClassMerge;

/// <summary>
/// Merges a string of css classes so that, of the Tailwind utilities that set the same property, only the last
/// one is kept (<c>px-4 px-2</c> becomes <c>px-2</c>). Classes that are not recognised Tailwind utilities are
/// always kept, in their original order.
/// </summary>
/// <remarks>
/// Two classes only conflict when they share the same variants (<c>hover:</c>, <c>md:</c>) and the same
/// <c>!</c> importance, so <c>hover:px-2</c> never removes <c>px-4</c>.
/// </remarks>
internal sealed class TwClassMerger
{
    private const int maxCacheSize = 2048;

    private static volatile TwClassMerger current = new(new TwClassMergeOptions());

    private readonly bool _enabled;
    private readonly List<TwClassGroupRule> _rules;
    private readonly Dictionary<string, string[]> _conflicts;
    private readonly ConcurrentDictionary<string, string> _cache = new(StringComparer.Ordinal);

    /// <summary>
    /// Creates a merger from <paramref name="options"/>, with its custom groups checked before the built-in ones.
    /// </summary>
    /// <param name="options">The merge configuration.</param>
    public TwClassMerger(TwClassMergeOptions options)
    {
        _enabled = options.Enabled;
        _rules = [.. options.Groups.Select(ToRule), .. TwClassGroups.Rules];
        _conflicts = TwClassGroups.Conflicts.ToDictionary(c => c.Key, c => c.Value, StringComparer.Ordinal);

        foreach (var group in options.Groups.Where(g => g.Overrides.Count > 0))
        {
            _conflicts[group.Name] = _conflicts.TryGetValue(group.Name, out var existing)
                ? [.. existing.Union(group.Overrides, StringComparer.Ordinal)]
                : [.. group.Overrides];
        }
    }

    /// <summary>
    /// Gets the merger the whole application uses. It starts with the default configuration.
    /// </summary>
    public static TwClassMerger Current => current;

    /// <summary>
    /// Replaces <see cref="Current"/> with a merger built from <paramref name="options"/>.
    /// </summary>
    /// <param name="options">The merge configuration.</param>
    public static void Configure(TwClassMergeOptions options) => current = new TwClassMerger(options);

    /// <summary>
    /// Merges <paramref name="classes"/>.
    /// </summary>
    /// <param name="classes">Space separated css classes.</param>
    /// <returns>The classes with conflicts removed and whitespace normalised, or just trimmed when merging is disabled.</returns>
    public string Merge(string? classes)
    {
        if (string.IsNullOrWhiteSpace(classes))
            return string.Empty;

        if (!_enabled)
            return classes.Trim();

        if (_cache.TryGetValue(classes, out var cached))
            return cached;

        var merged = MergeUncached(classes);

        if (_cache.Count >= maxCacheSize)
            _cache.Clear();

        _cache[classes] = merged;
        return merged;
    }

    private static TwClassGroupRule ToRule(TwClassGroup group)
    {
        var matchers = group.Prefixes.Select(prefix => TwClassGroups.Prefixed(prefix, bare: true)).ToList();
        return new TwClassGroupRule(group.Name, utility => matchers.Exists(match => match(utility)));
    }

    private string MergeUncached(string classes)
    {
        var tokens = classes.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var keep = new bool[tokens.Length];
        var claimed = new HashSet<string>(StringComparer.Ordinal);

        for (var i = tokens.Length - 1; i >= 0; i--)
        {
            keep[i] = Claim(tokens[i], claimed);
        }

        return string.Join(' ', tokens.Where((_, index) => keep[index]));
    }

    private bool Claim(string token, HashSet<string> claimed)
    {
        var parsed = Parse(token);
        var group = FindGroup(parsed.Utility);

        if (group is null)
            return true;

        var scope = parsed.Scope;

        if (!claimed.Add(scope + group))
            return false;

        if (_conflicts.TryGetValue(group, out var overridden))
        {
            foreach (var other in overridden)
                claimed.Add(scope + other);
        }

        if (parsed.HasModifier && group == "font-size")
            claimed.Add(scope + "leading");

        return true;
    }

    private string? FindGroup(string utility)
    {
        if (utility.Length > 2 && utility[0] == '[' && utility[^1] == ']' && utility.IndexOf(':', StringComparison.Ordinal) is var colon and > 1)
            return "arbitrary:" + utility[1..colon];

        foreach (var rule in _rules)
        {
            if (rule.Matches(utility))
                return rule.Name;
        }

        return null;
    }

    /// <summary>
    /// The pieces of a class that matter when deciding whether two classes conflict.
    /// </summary>
    /// <param name="Scope">The sorted variants and importance, e.g. <c>hover:md:!</c>, so only classes in the same scope conflict.</param>
    /// <param name="Utility">The utility with variants, importance, a leading <c>-</c> and any <c>/</c> modifier removed.</param>
    /// <param name="HasModifier">Whether the utility had a <c>/</c> modifier such as the line height in <c>text-sm/6</c>.</param>
    private readonly record struct ParsedClass(string Scope, string Utility, bool HasModifier);

    private static ParsedClass Parse(string token)
    {
        var variants = new List<string>();
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
                    variants.Add(token[start..i]);
                    start = i + 1;
                    break;
            }
        }

        var utility = token[start..];
        var important = false;

        if (utility.StartsWith('!'))
        {
            important = true;
            utility = utility[1..];
        }
        else if (utility.EndsWith('!'))
        {
            important = true;
            utility = utility[..^1];
        }

        if (utility.Length > 1 && utility[0] == '-')
            utility = utility[1..];

        var slash = LastTopLevelSlash(utility);
        var hasModifier = slash > 0;

        if (hasModifier)
            utility = utility[..slash];

        variants.Sort(StringComparer.Ordinal);
        var scope = string.Concat(variants.Select(v => v + ":")) + (important ? "!" : string.Empty) + "|";

        return new ParsedClass(scope, utility, hasModifier);
    }

    private static int LastTopLevelSlash(string utility)
    {
        var depth = 0;
        var slash = -1;

        for (var i = 0; i < utility.Length; i++)
        {
            switch (utility[i])
            {
                case '[' or '(':
                    depth++;
                    break;
                case ']' or ')':
                    depth--;
                    break;
                case '/' when depth == 0:
                    slash = i;
                    break;
            }
        }

        return slash;
    }
}
