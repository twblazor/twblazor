// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Utilities.ClassMerge;

/// <summary>
/// One entry of a class group: either a whole class (<c>inline-block</c>) or, when <paramref name="Validator"/>
/// is set, every class that starts with <paramref name="Path"/> and a dash and whose remainder passes the validator.
/// </summary>
/// <param name="Group">The name of the group the classes belong to.</param>
/// <param name="Path">The class, or the part of it before the validated value.</param>
/// <param name="Validator">The check the value after <paramref name="Path"/> must pass, if any.</param>
internal sealed record TwClassDefinition(string Group, string Path, TwClassValidator? Validator = null);

/// <summary>
/// The built-in Tailwind class groups (utilities that set the same css property, so only the last one added is
/// kept) and which groups a later class also overrides.
/// </summary>
/// <remarks>
/// The tables are generated from tailwind-merge's default config, see <c>TwClassGroups.Data.cs</c>, and looked up
/// the way that package does: a class is split on its dashes and followed through a tree of the known parts, and
/// where the tree ends the rest of the class is handed to the validators registered at that point.
/// </remarks>
internal static partial class TwClassGroups
{
    private const string arbitraryPropertyPrefix = "arbitrary..";

    private static readonly Node _root = BuildTree();

    /// <summary>
    /// Gets, for each group, the other groups a later class from it also removes.
    /// </summary>
    public static IReadOnlyDictionary<string, string[]> Conflicts { get; } = BuildConflicts();

    /// <summary>
    /// Gets, for each group, the groups a later class from it also removes when it has a <c>/</c> modifier, such as
    /// the line height in <c>text-sm/6</c>.
    /// </summary>
    public static IReadOnlyDictionary<string, string[]> ModifierConflicts { get; } = BuildModifierConflicts();

    /// <summary>
    /// Gets the variants whose position among the other variants changes the css produced, such as
    /// <c>before</c> and <c>*</c>.
    /// </summary>
    public static IReadOnlySet<string> OrderSensitiveModifiers { get; } = new HashSet<string>(BuildOrderSensitiveModifiers(), StringComparer.Ordinal);

    /// <summary>
    /// Gets the groups whose classes are looked up again with their <c>/</c> modifier, because the modifier can
    /// put the class in a different group (<c>@container</c> and <c>@container/name</c>).
    /// </summary>
    public static IReadOnlySet<string> PostfixLookupGroups { get; } = new HashSet<string>(BuildPostfixLookupGroups(), StringComparer.Ordinal);

    /// <summary>
    /// Finds the group a class belongs to.
    /// </summary>
    /// <param name="className">The class without variants, <c>!</c> or a <c>/</c> modifier.</param>
    /// <returns>The group's name, or <see langword="null"/> when the class is not a Tailwind utility.</returns>
    public static string? Find(string className)
    {
        if (className.StartsWith('[') && className.EndsWith(']'))
        {
            var content = className.Length > 1 ? className[1..^1] : string.Empty;
            var colon = content.IndexOf(':', StringComparison.Ordinal);
            return colon > 0 ? arbitraryPropertyPrefix + content[..colon] : null;
        }

        var parts = className.Split('-');

        // A negative value (-inset-1) gives an empty first part, which is skipped.
        var start = parts[0].Length == 0 && parts.Length > 1 ? 1 : 0;
        return Find(parts, start, _root);
    }

    private static string? Find(string[] parts, int index, Node node)
    {
        if (index == parts.Length)
            return node.Group;

        if (node.Next.TryGetValue(parts[index], out var next) && Find(parts, index + 1, next) is { } group)
            return group;

        if (node.Validators is null)
            return null;

        var rest = string.Join('-', parts, index, parts.Length - index);

        foreach (var (matches, validatedGroup) in node.Validators)
        {
            if (matches(rest))
                return validatedGroup;
        }

        return null;
    }

    private static Node BuildTree()
    {
        var root = new Node();

        foreach (var definition in BuildExtensions().Concat(BuildDefinitions()))
        {
            var node = root;

            if (definition.Path.Length > 0)
            {
                foreach (var part in definition.Path.Split('-'))
                {
                    if (!node.Next.TryGetValue(part, out var next))
                        node.Next[part] = next = new Node();

                    node = next;
                }
            }

            if (definition.Validator is { } validator)
                (node.Validators ??= []).Add((TwClassValue.Get(validator), definition.Group));
            else
                node.Group = definition.Group;
        }

        return root;
    }

    /// <summary>
    /// The definitions TwBlazor adds to tailwind-merge's. They are registered first, so they are checked before the
    /// generated ones that share their path.
    /// </summary>
    /// <remarks>
    /// tailwind-merge reads every unlabelled arbitrary background value it cannot identify as a color. Tailwind
    /// itself reads one made of position keywords (<c>bg-[right_0.5rem_center]</c>) as a background position, and
    /// the default theme positions the select's chevron that way next to a background color.
    /// </remarks>
    private static TwClassDefinition[] BuildExtensions() =>
    [
        new("bg-position", "bg", TwClassValidator.ArbitraryPositionKeywords),
    ];

    private sealed class Node
    {
        public Dictionary<string, Node> Next { get; } = new(StringComparer.Ordinal);

        public List<(Func<string, bool> Matches, string Group)>? Validators { get; set; }

        public string? Group { get; set; }
    }
}
