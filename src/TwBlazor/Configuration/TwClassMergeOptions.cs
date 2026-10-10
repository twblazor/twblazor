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
    /// Gets or sets whether conflicting classes are merged. Defaults to <see langword="true"/>. When
    /// <see langword="false"/> every class is rendered as written.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets the custom class groups, checked before the built-in Tailwind groups. Use these for your own utilities
    /// (for example <c>@utility</c> classes) or to change what a built-in group conflicts with.
    /// </summary>
    public IList<TwClassGroup> Groups { get; } = [];
}
