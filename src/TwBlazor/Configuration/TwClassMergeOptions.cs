// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Configuration;

/// <summary>
/// Controls how conflicting Tailwind classes are merged when a component builds its <c>class</c> attribute.
/// </summary>
/// <remarks>
/// Optional. Out of the box the last conflicting utility wins (a consumer's <c>px-2</c> replaces a theme's
/// <c>px-4</c>) and classes that are not Tailwind utilities are left alone. The configuration is applied when
/// <c>AddTwBlazor</c> is called and is shared by the whole application.
/// </remarks>
public sealed class TwClassMergeOptions
{
    /// <summary>
    /// The number of merge results cached unless <see cref="CacheSize"/> is set.
    /// </summary>
    public const int DefaultCacheSize = 2048;

    /// <summary>
    /// Gets or sets whether conflicting classes are merged. Defaults to <see langword="true"/>. When
    /// <see langword="false"/> every class is rendered as written.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the prefix of your Tailwind build, without its colon: <c>tw</c> for classes written as
    /// <c>tw:px-4</c>. Defaults to <see langword="null"/>, for a build with no prefix.
    /// </summary>
    /// <remarks>
    /// Set it to the value given to <c>prefix(...)</c> in your stylesheet. Only classes that start with the prefix
    /// are then treated as Tailwind utilities. Every other class is left alone, even one named like a utility
    /// (<c>hidden</c>, <c>container</c>), since in a prefixed build such a class comes from somewhere else.
    /// Prefixed classes are merged with each other whether or not this is set.
    /// </remarks>
    public string? Prefix { get; set; }

    /// <summary>
    /// Gets or sets how many merge results are cached, keyed on the whole class string. Defaults to
    /// <see cref="DefaultCacheSize"/>. When the cache is full it is emptied and starts again. Zero or less turns
    /// caching off.
    /// </summary>
    public int CacheSize { get; set; } = DefaultCacheSize;

    /// <summary>
    /// Gets the custom class groups, checked before the built-in Tailwind groups. Use these for your own utilities
    /// (for example <c>@utility</c> classes) or to change what a built-in group conflicts with.
    /// </summary>
    public IList<TwClassGroup> Groups { get; } = [];

    /// <summary>
    /// Gets the extra variants whose position among the other variants matters, added to the built-in ones such
    /// as <c>before</c> and <c>*</c>.
    /// </summary>
    /// <remarks>
    /// Add a variant you define with <c>@custom-variant</c> when it changes which element is styled, the way a
    /// pseudo-element does. <c>hover:my-variant:px-2</c> and <c>my-variant:hover:px-2</c> are then kept apart
    /// instead of being treated as the same class. Write each one without its colon.
    /// </remarks>
    public IList<string> OrderSensitiveVariants { get; } = [];
}
