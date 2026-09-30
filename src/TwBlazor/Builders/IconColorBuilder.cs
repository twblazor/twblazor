// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Configuration;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Builders;

/// <summary>
/// Resolves the color classes for an icon glyph from <see cref="TwIconTheme.Colors"/>.
/// </summary>
/// <remarks>
/// Icons are graphics rather than body text, so they are held to the 3:1 non-text contrast ratio instead of
/// the 4.5:1 text ratio that <see cref="ColorBuilder.GetTextColor"/> is tuned for. Reusing the text colors
/// made icons look muddy in light mode (e.g. brown instead of yellow) and washed out in dark mode.
/// </remarks>
public class IconColorBuilder(TwBlazorOptions options)
{
    private TwIconTheme theme => options.Theme.Components.Require<TwIconTheme>();

    /// <summary>
    /// Gets the light and dark mode text color classes for an icon of the specified color.
    /// </summary>
    /// <param name="color">The icon color. If null or unrecognized, an empty string is returned so the icon inherits its surrounding color.</param>
    /// <returns>The color classes from <see cref="TwIconTheme.Colors"/>, or an empty string.</returns>
    public string GetIconColor(Color? color) => ColorBuilder.GetPaletteColor(color, theme.Colors, string.Empty);
}
