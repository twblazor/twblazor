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
    private const int defaultTintAlpha = 0x26; // ~15% alpha (0x26 of 0xFF)

    private const double minContrast = 5.5; // WCAG AA is 4.5:1; axe measures the dimmed labels about 0.2 lower than this model

    private static readonly (double R, double G, double B) _lightSurface = (255, 255, 255);

    private static readonly (double R, double G, double B) _darkSurface = (17, 24, 39);

    /// <summary>
    /// Returns the inline style for an event card (the Day/Week grid's absolutely-positioned card,
    /// or the Month view's row): a light tint of <paramref name="color"/> for the background, the
    /// full color for the left accent border, and a text color derived from it that stays readable on
    /// that tint (a darker shade in light mode, a lighter one in dark mode, picked with
    /// <c>light-dark()</c>), or an empty string when <paramref name="color"/> is
    /// <see langword="null"/>/empty/not a well-formed hex color (e.g. unvalidated data from an
    /// external source) - in which case the card falls back to
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
        var tint = solid.Length == 7 ? $"{solid}{defaultTintAlpha:x2}" : solid;
        var alpha = (solid.Length == 7 ? defaultTintAlpha : Convert.ToInt32(solid[7..], 16)) / 255d;
        var rgb = (R: (double)Convert.ToInt32(solid[1..3], 16), G: Convert.ToInt32(solid[3..5], 16), B: Convert.ToInt32(solid[5..7], 16));

        var lightText = ReadableText(rgb, alpha, _lightSurface, (0, 0, 0));
        var darkText = ReadableText(rgb, alpha, _darkSurface, (255, 255, 255));

        return $"background-color:{tint};border-left-color:{solid};color:light-dark({lightText},{darkText});";
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

    /// <summary>
    /// The least-shifted blend of <paramref name="rgb"/> towards <paramref name="target"/> that still
    /// reads at <see cref="minContrast"/> on the tint, in the worst case the chip can be shown in: the
    /// time label dimmed to 75% opacity, inside a read-only chip dimmed to 90%.
    /// </summary>
    private static string ReadableText((double R, double G, double B) rgb, double alpha, (double R, double G, double B) surface, (double R, double G, double B) target)
    {
        var background = Mix(surface, rgb, alpha);
        var text = rgb;

        for (var step = 0; step <= 40; step++)
        {
            text = Mix(rgb, target, step / 40d);
            // Axe measures the dimmed text against the undimmed tint, so the opacities multiply (0.75 * 0.9).
            var dimmedText = Mix(background, text, 0.75 * 0.9);

            if (Contrast(dimmedText, background) >= minContrast)
            {
                break;
            }
        }

        return $"#{(int)Math.Round(text.R):x2}{(int)Math.Round(text.G):x2}{(int)Math.Round(text.B):x2}";
    }

    /// <summary>
    /// <paramref name="top"/> laid over <paramref name="bottom"/> at <paramref name="amount"/> (0-1).
    /// </summary>
    private static (double R, double G, double B) Mix((double R, double G, double B) bottom, (double R, double G, double B) top, double amount) =>
        (bottom.R + ((top.R - bottom.R) * amount), bottom.G + ((top.G - bottom.G) * amount), bottom.B + ((top.B - bottom.B) * amount));

    private static double Contrast((double R, double G, double B) a, (double R, double G, double B) b)
    {
        var la = Luminance(a);
        var lb = Luminance(b);
        return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
    }

    private static double Luminance((double R, double G, double B) c)
    {
        static double Channel(double value)
        {
            var v = value / 255d;
            return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Channel(c.R)) + (0.7152 * Channel(c.G)) + (0.0722 * Channel(c.B));
    }

    private static string ExpandShorthand(string hex) =>
        hex.Length == 4 ? $"#{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}" : hex;

    private static bool IsValidHexColor(string? color) =>
        !string.IsNullOrWhiteSpace(color) && HexColorRegex().IsMatch(color);

    [GeneratedRegex("^#[0-9A-Fa-f]{3}([0-9A-Fa-f]{3}([0-9A-Fa-f]{2})?)?$")]
    private static partial Regex HexColorRegex();
}
