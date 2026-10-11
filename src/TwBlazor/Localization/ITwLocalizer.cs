// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Localization;

/// <summary>
/// Supplies the text and the date and time formats TwBlazor components render with.
/// </summary>
/// <remarks>
/// <see cref="TwLocalizer"/> is registered by <c>AddTwBlazor</c> and reads <c>.resx</c> files. Register your own
/// implementation to take the strings from somewhere else.
/// </remarks>
public interface ITwLocalizer
{
    /// <summary>
    /// Gets the text for a key in the current language.
    /// </summary>
    /// <param name="key">One of the <see cref="TwStrings"/> keys.</param>
    /// <returns>The text, or <paramref name="key"/> when nothing is registered for it.</returns>
    string this[string key] { get; }

    /// <summary>
    /// Gets the text for a key in the current language, with its <c>{0}</c> placeholders filled in.
    /// </summary>
    /// <param name="key">One of the <see cref="TwStrings"/> keys.</param>
    /// <param name="arguments">The values for the placeholders.</param>
    /// <returns>The formatted text.</returns>
    string this[string key, params object?[] arguments] { get; }

    /// <summary>
    /// Gets the date and time formats for the current culture.
    /// </summary>
    TwDateTimeFormats Formats { get; }
}
