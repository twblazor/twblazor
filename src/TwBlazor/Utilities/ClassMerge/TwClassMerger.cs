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
/// <c>!</c> importance, so <c>hover:px-2</c> never removes <c>px-4</c>. The algorithm and the built-in groups
/// follow the tailwind-merge npm package, see <see cref="TwClassGroups"/>.
/// </remarks>
internal sealed class TwClassMerger
{
    private static volatile TwClassMerger current = new(new TwClassMergeOptions());

    private readonly bool _enabled;
    private readonly string? _prefix;
    private readonly int _cacheSize;
    private readonly List<CustomGroup> _customGroups;
    private readonly Dictionary<string, string[]> _conflicts;
    private readonly Dictionary<string, string[]> _modifierConflicts;
    private readonly HashSet<string> _orderSensitiveVariants;
    private readonly ConcurrentDictionary<string, string> _cache = new(StringComparer.Ordinal);

    /// <summary>
    /// Creates a merger from <paramref name="options"/>, with its custom groups checked before the built-in ones.
    /// </summary>
    /// <param name="options">The merge configuration.</param>
    public TwClassMerger(TwClassMergeOptions options)
    {
        _enabled = options.Enabled;
        _prefix = string.IsNullOrWhiteSpace(options.Prefix) ? null : options.Prefix.Trim().TrimEnd(':') + ":";
        _cacheSize = options.CacheSize;
        _customGroups = [.. options.Groups.Select(group => new CustomGroup(group.Name, [.. group.Prefixes], [.. group.Classes]))];
        _conflicts = TwClassGroups.Conflicts.ToDictionary(c => c.Key, c => c.Value, StringComparer.Ordinal);
        _modifierConflicts = TwClassGroups.ModifierConflicts.ToDictionary(c => c.Key, c => c.Value, StringComparer.Ordinal);
        _orderSensitiveVariants = new HashSet<string>(TwClassGroups.OrderSensitiveModifiers, StringComparer.Ordinal);

        foreach (var group in options.Groups)
        {
            Extend(_conflicts, group.Name, group.Overrides);
            Extend(_modifierConflicts, group.Name, group.ModifierOverrides);
        }

        foreach (var variant in options.OrderSensitiveVariants.Where(v => !string.IsNullOrWhiteSpace(v)))
        {
            _orderSensitiveVariants.Add(variant.Trim().TrimEnd(':'));
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

        if (_cacheSize <= 0)
            return MergeUncached(classes);

        if (_cache.TryGetValue(classes, out var cached))
            return cached;

        var merged = MergeUncached(classes);

        if (_cache.Count >= _cacheSize)
            _cache.Clear();

        _cache[classes] = merged;
        return merged;
    }

    private static void Extend(Dictionary<string, string[]> conflicts, string group, IReadOnlyList<string> overrides)
    {
        if (overrides.Count == 0)
            return;

        conflicts[group] = conflicts.TryGetValue(group, out var existing)
            ? [.. existing.Union(overrides, StringComparer.Ordinal)]
            : [.. overrides];
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
        if (_prefix is not null)
        {
            // In a prefixed build a class without the prefix is not a Tailwind utility, whatever it is called.
            if (!token.StartsWith(_prefix, StringComparison.Ordinal))
                return true;

            token = token[_prefix.Length..];
        }

        var parsed = Parse(token);
        var hasModifier = parsed.ModifierPosition > 0;
        string? group;

        if (hasModifier)
        {
            group = FindGroup(parsed.Utility[..parsed.ModifierPosition]);

            // The modifier can change the group: @container is a container type, @container/name a named container.
            if (group is not null && TwClassGroups.PostfixLookupGroups.Contains(group) && FindGroup(parsed.Utility) is { } withModifier && withModifier != group)
            {
                group = withModifier;
                hasModifier = false;
            }
        }
        else
        {
            group = FindGroup(parsed.Utility);
        }

        if (group is null)
        {
            if (!hasModifier)
                return true;

            // What looked like a modifier may be part of the value, as in the fraction of w-1/2.
            group = FindGroup(parsed.Utility);

            if (group is null)
                return true;

            hasModifier = false;
        }

        var scope = parsed.Scope;

        if (!claimed.Add(scope + group))
            return false;

        if (_conflicts.TryGetValue(group, out var overridden))
        {
            foreach (var other in overridden)
                claimed.Add(scope + other);
        }

        if (hasModifier && _modifierConflicts.TryGetValue(group, out var overriddenByModifier))
        {
            foreach (var other in overriddenByModifier)
                claimed.Add(scope + other);
        }

        return true;
    }

    private string? FindGroup(string utility)
    {
        if (_customGroups.Count > 0)
        {
            var name = utility.Length > 1 && utility[0] == '-' ? utility[1..] : utility;

            foreach (var custom in _customGroups)
            {
                if (custom.Matches(name))
                    return custom.Name;
            }
        }

        return TwClassGroups.Find(utility);
    }

    /// <summary>
    /// A group from <see cref="TwClassMergeOptions.Groups"/>.
    /// </summary>
    /// <param name="Name">The group's name.</param>
    /// <param name="Prefixes">The class prefixes that belong to the group.</param>
    /// <param name="Classes">The classes that belong to the group by their whole name.</param>
    private sealed record CustomGroup(string Name, string[] Prefixes, string[] Classes)
    {
        /// <summary>
        /// Gets whether <paramref name="utility"/> is one of the classes or prefixes, or starts with a prefix
        /// followed by a dash.
        /// </summary>
        /// <param name="utility">The class without variants, <c>!</c>, a leading <c>-</c> or a <c>/</c> modifier.</param>
        public bool Matches(string utility) =>
            Array.IndexOf(Classes, utility) >= 0
            || Array.Exists(Prefixes, prefix => utility == prefix || (utility.Length > prefix.Length && utility[prefix.Length] == '-' && utility.StartsWith(prefix, StringComparison.Ordinal)));
    }

    /// <summary>
    /// The pieces of a class that matter when deciding whether two classes conflict.
    /// </summary>
    /// <param name="Scope">The variants and importance, e.g. <c>hover:md:!</c>, so only classes in the same scope conflict.</param>
    /// <param name="Utility">The class with its variants and importance removed.</param>
    /// <param name="ModifierPosition">The index in <paramref name="Utility"/> of its last top-level <c>/</c>, or 0 when it has none.</param>
    private readonly record struct ParsedClass(string Scope, string Utility, int ModifierPosition);

    private ParsedClass Parse(string token)
    {
        var variants = new List<string>();
        var bracketDepth = 0;
        var parenDepth = 0;
        var start = 0;
        var slash = -1;

        for (var i = 0; i < token.Length; i++)
        {
            var character = token[i];

            if (bracketDepth == 0 && parenDepth == 0)
            {
                if (character == ':')
                {
                    variants.Add(token[start..i]);
                    start = i + 1;
                    continue;
                }

                if (character == '/')
                {
                    slash = i;
                    continue;
                }
            }

            switch (character)
            {
                case '[':
                    bracketDepth++;
                    break;
                case ']':
                    bracketDepth--;
                    break;
                case '(':
                    parenDepth++;
                    break;
                case ')':
                    parenDepth--;
                    break;
            }
        }

        var utility = token[start..];
        var modifierPosition = slash > start ? slash - start : 0;
        var important = false;

        if (utility.EndsWith('!'))
        {
            important = true;
            utility = utility[..^1];
        }
        else if (utility.StartsWith('!'))
        {
            // The Tailwind v3 position of the important modifier.
            important = true;
            utility = utility[1..];
            modifierPosition = Math.Max(modifierPosition - 1, 0);
        }

        var scope = string.Join(':', SortVariants(variants)) + (important ? "!" : string.Empty) + "|";

        return new ParsedClass(scope, utility, modifierPosition);
    }

    /// <summary>
    /// Sorts the variants so that <c>hover:focus:</c> and <c>focus:hover:</c> give the same scope, without moving
    /// any of them across an arbitrary variant (<c>[&amp;>*]</c>) or an order sensitive one (<c>before</c>), since
    /// which side of those a variant is on changes the css produced.
    /// </summary>
    /// <param name="variants">The variants in the order they were written.</param>
    private List<string> SortVariants(List<string> variants)
    {
        if (variants.Count < 2)
            return variants;

        var sorted = new List<string>(variants.Count);
        var segment = new List<string>();

        foreach (var variant in variants)
        {
            if (variant.StartsWith('[') || _orderSensitiveVariants.Contains(variant))
            {
                segment.Sort(StringComparer.Ordinal);
                sorted.AddRange(segment);
                segment.Clear();
                sorted.Add(variant);
            }
            else
            {
                segment.Add(variant);
            }
        }

        segment.Sort(StringComparer.Ordinal);
        sorted.AddRange(segment);
        return sorted;
    }
}
