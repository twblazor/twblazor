// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;

namespace TwBlazor.Utilities;

/// <summary>
/// Shared date/time calculations used by calendar-style components (e.g. <see cref="TwBlazor.Components.TwCalendar{T}"/>).
/// </summary>
public static class DateHelpers
{
    /// <summary>
    /// Gets the ISO 8601 week number (1-53) for the given date.
    /// </summary>
    public static int GetWeekOfYear(DateTime date) => ISOWeek.GetWeekOfYear(date);

    /// <summary>
    /// Gets the Monday that starts the week containing <paramref name="date"/>.
    /// </summary>
    public static DateTime GetStartOfWeek(DateTime date)
    {
        // DayOfWeek.Sunday == 0, so this maps Monday -> 0 ... Sunday -> 6 instead.
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-offset);
    }

    /// <summary>
    /// Gets the 1-based week-of-month for <paramref name="date"/> (1-5, occasionally 6), counting
    /// Monday-start weeks from the one containing the 1st of the month - the same grid a Month view
    /// would lay the date out in, so this always matches which visual row it falls on there.
    /// </summary>
    public static int GetWeekOfMonth(DateTime date)
    {
        var firstOfMonth = new DateTime(date.Year, date.Month, 1, 0, 0, 0, date.Kind);
        var firstWeekStart = GetStartOfWeek(firstOfMonth);
        return ((date.Date - firstWeekStart).Days / 7) + 1;
    }
}
