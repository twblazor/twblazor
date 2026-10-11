// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Localization;

/// <summary>
/// Helpers for reading text from an <see cref="ITwLocalizer"/>.
/// </summary>
public static class TwLocalizerExtensions
{
    /// <summary>
    /// Gets the singular or plural form of a text, depending on <paramref name="count"/>.
    /// </summary>
    /// <param name="localizer">The localizer to read from.</param>
    /// <param name="count">The number that decides the form.</param>
    /// <param name="oneKey">The key of the singular form, used when <paramref name="count"/> is 1.</param>
    /// <param name="otherKey">The key of the plural form.</param>
    /// <param name="arguments">The values for the text's placeholders.</param>
    /// <returns>The formatted text.</returns>
    public static string Plural(this ITwLocalizer localizer, int count, string oneKey, string otherKey, params object?[] arguments) =>
        localizer[count == 1 ? oneKey : otherKey, arguments];
}
