// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Components;

/// <summary>
/// The 30-minute time-slot grid geometry <see cref="TwCalendarDayColumn{T}"/> lays events out against.
/// Duplicated here (rather than read off a <c>TwCalendarDayColumn&lt;T&gt;</c> instance, which is generic
/// and whose constants are private) so <see cref="TwCalendarDayView{T}"/> and
/// <see cref="TwCalendarWeekView{T}"/> can position their current-time indicator/gutter label using the
/// exact same minutes-to-rem conversion.
/// </summary>
internal static class TwCalendarTimeGrid
{
    /// <summary>The length, in minutes, of one grid row - see <see cref="TwBlazor.Configuration.Components.TwCalendarTheme.SlotRow"/>.</summary>
    public const int MinutesPerSlot = 30;

    /// <summary>The rendered height, in rem, of one <see cref="MinutesPerSlot"/> row.</summary>
    public const decimal SlotHeightRem = 3m;

    /// <summary>
    /// Converts a time-of-day (in minutes from midnight) into a <c>top</c> offset in rem from the
    /// grid's top edge, clamped to a single day.
    /// </summary>
    public static decimal GetOffsetRem(double minutesFromMidnight)
    {
        var clamped = Math.Clamp(minutesFromMidnight, 0, 24 * 60);
        return (decimal)(clamped / MinutesPerSlot) * SlotHeightRem;
    }
}
