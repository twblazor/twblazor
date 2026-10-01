// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace TwBlazor.Components;

/// <summary>
/// Validates a <see cref="Schedule{T}.Color"/> value and turns it into the inline <c>style</c>
/// declarations event cards need, shared by <see cref="TwCalendarDayColumn{T}"/> and
/// <see cref="TwCalendarMonthView{T}"/>.
/// </summary>
internal static partial class TwCalendarColors
{
    /// <summary>
    /// Returns the inline style for an event card (the Day/Week grid's absolutely-positioned card,
    /// or the Month view's row): a light tint of <paramref name="color"/> for the background, the
    /// full color for the left accent border and the text, or an empty string when
    /// <paramref name="color"/> is <see langword="null"/>/empty/not a well-formed hex color (e.g.
    /// unvalidated data from an external source) - in which case the card falls back to
    /// <see cref="TwBlazor.Configuration.Components.TwCalendarTheme.EventChip"/>'s own class-based
    /// styling instead of rendering with none at all.
    /// </summary>
    public static string GetEventCardStyle(string? color)
    {
        if (!IsValidHexColor(color))
        {
            return string.Empty;
        }

        var solid = ExpandShorthand(color!);
        // A plain 6-digit hex (the only shape TwColorPicker's default Hex/no-alpha output produces)
        // gets an appended alpha channel for the tint; anything already carrying its own alpha (an
        // 8-digit hex from elsewhere) is used as-is rather than guessing how to re-tint it.
        var tint = solid.Length == 7 ? $"{solid}26" : solid; // ~15% alpha (0x26 of 0xFF)

        return $"background-color:{tint};border-left-color:{solid};color:{solid};";
    }

    /// <summary>
    /// Returns the inline style for a dialog highlight card: the same light tint background and solid
    /// left accent border as <see cref="GetEventCardStyle"/>, but without recoloring the text, so body
    /// text keeps the theme's normal contrast. Empty when <paramref name="color"/> isn't a valid hex
    /// color, leaving the theme's own classes in charge.
    /// </summary>
    public static string GetAccentCardStyle(string? color)
    {
        if (!IsValidHexColor(color))
        {
            return string.Empty;
        }

        var solid = ExpandShorthand(color!);
        var tint = solid.Length == 7 ? $"{solid}26" : solid;

        return $"background-color:{tint};border-left-color:{solid};";
    }

    /// <summary>
    /// Returns an inline <c>background-color</c> declaration in the solid color, for the dialog's accent
    /// bar, or an empty string when <paramref name="color"/> isn't a valid hex color.
    /// </summary>
    public static string GetSolidBackgroundStyle(string? color) =>
        IsValidHexColor(color) ? $"background-color:{ExpandShorthand(color!)};" : string.Empty;

    /// <summary>
    /// Returns an inline <c>color</c> declaration in the solid color, for icons that pick up the event's
    /// color, or an empty string when <paramref name="color"/> isn't a valid hex color.
    /// </summary>
    public static string GetSolidTextStyle(string? color) =>
        IsValidHexColor(color) ? $"color:{ExpandShorthand(color!)};" : string.Empty;

    private static string ExpandShorthand(string hex) =>
        hex.Length == 4 ? $"#{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}" : hex;

    private static bool IsValidHexColor(string? color) =>
        !string.IsNullOrWhiteSpace(color) && HexColorRegex().IsMatch(color);

    [GeneratedRegex("^#[0-9A-Fa-f]{3}([0-9A-Fa-f]{3}([0-9A-Fa-f]{2})?)?$")]
    private static partial Regex HexColorRegex();
}
