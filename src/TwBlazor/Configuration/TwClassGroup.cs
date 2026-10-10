// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Configuration;

/// <summary>
/// A custom set of css classes that conflict with each other, so only the last one added survives a merge.
/// </summary>
/// <remarks>
/// A class belongs to the group when it is one of <see cref="Prefixes"/> or starts with one of them followed by a
/// dash, so the prefix <c>btn</c> matches <c>btn</c>, <c>btn-sm</c> and <c>btn-lg</c>. Reusing the name of a
/// built-in group (for example <c>p</c> or <c>text-color</c>) adds your classes to that group instead.
/// </remarks>
/// <param name="Name">The group's name, unique among your custom groups.</param>
/// <param name="Prefixes">The class prefixes that belong to the group.</param>
public sealed record TwClassGroup(string Name, IReadOnlyList<string> Prefixes)
{
    /// <summary>
    /// Gets the names of other groups, built-in or custom, that a later class from this group also removes. For
    /// example <c>px</c> overrides <c>pr</c> and <c>pl</c>.
    /// </summary>
    public IReadOnlyList<string> Overrides { get; init; } = [];
}
