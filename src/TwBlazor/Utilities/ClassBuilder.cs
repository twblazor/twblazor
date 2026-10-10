// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Utilities.ClassMerge;

namespace TwBlazor.Utilities;

/// <summary>
/// Builds a string of css classes. <see cref="Build"/> merges conflicting Tailwind utilities, so the class added last
/// wins (a consumer's <c>px-2</c> replaces a theme's <c>px-4</c>) instead of both reaching the element. How it merges
/// is configured through <see cref="Configuration.TwBlazorOptions.ClassMerge"/>.
/// </summary>
public struct ClassBuilder
{
    private string classes;
    public ClassBuilder(string value) => classes = value;

    public ClassBuilder AddValue(string value)
    {
        classes += value;
        return this;
    }

    /// <summary>
    /// Adds a css class to the string.
    /// </summary>
    /// <param name="value">The css class to add.</param>
    /// <returns>ClassBuilder</returns>
    public ClassBuilder AddClass(string value) => AddValue(" " + value?.Trim());

    /// <summary>
    /// Adds a conditional css class to the string.
    /// </summary>
    /// <param name="value">The css class to add.</param>
    /// <param name="condition">Only add if condition is true.</param>
    /// <returns>ClassBuilder</returns>
    public ClassBuilder AddClass(string value, bool condition) => condition ? this.AddClass(value) : this;

    /// <summary>
    /// Builds the final string of css classes, resolving conflicting Tailwind utilities in favour of the one added last.
    /// Classes that are not Tailwind utilities are kept as they are.
    /// </summary>
    /// <returns>The final constructed string of css classes.</returns>
    public string Build() => TwClassMerger.Current.Merge(classes);
}
