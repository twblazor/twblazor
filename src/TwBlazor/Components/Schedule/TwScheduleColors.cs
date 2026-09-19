// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace TwBlazor.Components;

/// <summary>
/// Validates a <see cref="Schedule{T}.Color"/> value and turns it into the inline <c>style</c>
/// declarations event cards need, shared by <see cref="TwScheduleDayColumn{T}"/> and
/// <see cref="TwScheduleMonthView{T}"/>.
/// </summary>
internal static partial class TwScheduleColors
{
    /// <summary>
    /// Returns the inline style for an event card (the Day/Week grid's absolutely-positioned card,
    /// or the Month view's row): a light tint of <paramref name="color"/> for the background, the
    /// full color for the left accent border and the text, or an empty string when
    /// <paramref name="color"/> is <see langword="null"/>/empty/not a well-formed hex color (e.g.
    /// unvalidated data from an external source) - in which case the card falls back to
    /// <see cref="TwBlazor.Configuration.Components.TwScheduleTheme.EventChip"/>'s own class-based
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

    private static string ExpandShorthand(string hex) =>
        hex.Length == 4 ? $"#{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}" : hex;

    private static bool IsValidHexColor(string? color) =>
        !string.IsNullOrWhiteSpace(color) && HexColorRegex().IsMatch(color);

    [GeneratedRegex("^#[0-9A-Fa-f]{3}([0-9A-Fa-f]{3}([0-9A-Fa-f]{2})?)?$")]
    private static partial Regex HexColorRegex();
}
