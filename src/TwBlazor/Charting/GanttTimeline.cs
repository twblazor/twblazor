// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using TwBlazor.Enums;

namespace TwBlazor.Charting;

/// <summary>
/// Maps dates to positions across a Gantt plot (0 to 100) and back.
/// </summary>
/// <remarks>
/// At <see cref="GanttZoom.Auto"/> time runs evenly from the first date to the last. At every other zoom level the
/// range is cut into calendar slots (days, weeks, months...) of equal width, as a printed planner is, so a month
/// column is as wide as any other month whatever its number of days.
/// </remarks>
internal sealed class GanttTimeline
{
    private const int autoTickCount = 4;

    // Stops a long range at a fine zoom level (years of hours) building an unusable number of columns.
    private const int maxSlots = 4000;

    private readonly DateTime[] _boundaries;

    private GanttTimeline(GanttZoom zoom, DateTime start, DateTime end, DateTime[] boundaries)
    {
        Zoom = zoom;
        Start = start;
        End = end;
        _boundaries = boundaries;
    }

    /// <summary>Gets the zoom level.</summary>
    public GanttZoom Zoom { get; }

    /// <summary>Gets the first date on the plot.</summary>
    public DateTime Start { get; }

    /// <summary>Gets the last date on the plot.</summary>
    public DateTime End { get; }

    /// <summary>Gets the number of calendar slots, or 0 at <see cref="GanttZoom.Auto"/>.</summary>
    public int SlotCount => Math.Max(0, _boundaries.Length - 1);

    /// <summary>Gets the width of one slot as a percentage of the plot.</summary>
    public double SlotSize => SlotCount == 0 ? 0 : 100.0 / SlotCount;

    /// <summary>
    /// Creates the timeline for a range. At a calendar zoom level the range widens to whole slots.
    /// </summary>
    public static GanttTimeline Create(GanttZoom zoom, DateTime start, DateTime end, CultureInfo culture)
    {
        if (zoom == GanttZoom.Auto)
        {
            return new GanttTimeline(zoom, start, end > start ? end : start.AddDays(1), []);
        }

        var first = Floor(start, zoom, culture);
        // The slot holding the end is always included, so a task or milestone ending on a boundary is drawn inside
        // the plot rather than on its edge.
        var last = Add(Floor(end, zoom, culture), zoom);

        var boundaries = new List<DateTime> { first };
        while (boundaries[^1] < last && boundaries.Count <= maxSlots)
        {
            boundaries.Add(Add(boundaries[^1], zoom));
        }

        return new GanttTimeline(zoom, first, boundaries[^1], [.. boundaries]);
    }

    /// <summary>Gets the position of a date across the plot, from 0 to 100.</summary>
    public double Position(DateTime date)
    {
        if (SlotCount == 0)
        {
            return (date - Start).Ticks / (double)Math.Max(1, (End - Start).Ticks) * 100;
        }

        if (date <= Start)
        {
            return 0;
        }

        if (date >= End)
        {
            return 100;
        }

        var slot = Array.BinarySearch(_boundaries, date);
        if (slot < 0)
        {
            slot = ~slot - 1;
        }

        slot = Math.Clamp(slot, 0, SlotCount - 1);
        var from = _boundaries[slot];
        var to = _boundaries[slot + 1];
        return (slot + (date - from).Ticks / (double)(to - from).Ticks) * SlotSize;
    }

    /// <summary>Gets the date at a position across the plot, from 0 to 100.</summary>
    public DateTime At(double position)
    {
        if (SlotCount == 0)
        {
            return Start.AddTicks((long)((End - Start).Ticks * position / 100));
        }

        var slots = Math.Clamp(position / SlotSize, 0, SlotCount);
        var slot = Math.Min((int)Math.Floor(slots), SlotCount - 1);
        var from = _boundaries[slot];
        return from.AddTicks((long)((_boundaries[slot + 1] - from).Ticks * (slots - slot)));
    }

    /// <summary>
    /// Moves a date to the nearest slot boundary, or to the nearest midnight at <see cref="GanttZoom.Auto"/>.
    /// </summary>
    public DateTime Snap(DateTime date)
    {
        if (SlotCount == 0)
        {
            var day = date.Date;
            return date - day >= TimeSpan.FromHours(12) ? day.AddDays(1) : day;
        }

        return At(Math.Round(Position(date) / SlotSize) * SlotSize);
    }

    /// <summary>Gets the slots, each with its first and last moment.</summary>
    public IEnumerable<(DateTime Start, DateTime End)> Slots()
    {
        for (var i = 0; i < SlotCount; i++)
        {
            yield return (_boundaries[i], _boundaries[i + 1]);
        }
    }

    /// <summary>
    /// Gets the axis labels: a few evenly spaced dates at <see cref="GanttZoom.Auto"/>, otherwise one label in
    /// the middle of each slot.
    /// </summary>
    public IReadOnlyList<ChartTick> Ticks(Func<DateTime, string> formatDate, Func<DateTime, int, string> formatSlot)
    {
        if (SlotCount == 0)
        {
            var span = (End - Start).Ticks;
            return [.. Enumerable.Range(0, autoTickCount + 1).Select(tick => new ChartTick(100.0 * tick / autoTickCount, formatDate(Start.AddTicks(span * tick / autoTickCount))))];
        }

        return [.. Enumerable.Range(0, SlotCount).Select(slot => new ChartTick((slot + 0.5) * SlotSize, formatSlot(_boundaries[slot], slot)))];
    }

    /// <summary>Gets the start of the slot a date falls in.</summary>
    public static DateTime Floor(DateTime date, GanttZoom zoom, CultureInfo culture) => zoom switch
    {
        GanttZoom.Hour => new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0, date.Kind),
        GanttZoom.Week => date.Date.AddDays(-(((int)date.DayOfWeek - (int)culture.DateTimeFormat.FirstDayOfWeek + 7) % 7)),
        GanttZoom.Month => new DateTime(date.Year, date.Month, 1, 0, 0, 0, date.Kind),
        GanttZoom.Quarter => new DateTime(date.Year, (date.Month - 1) / 3 * 3 + 1, 1, 0, 0, 0, date.Kind),
        GanttZoom.Year => new DateTime(date.Year, 1, 1, 0, 0, 0, date.Kind),
        _ => date.Date
    };

    /// <summary>Gets the start of the next slot.</summary>
    public static DateTime Add(DateTime date, GanttZoom zoom) => zoom switch
    {
        GanttZoom.Hour => date.AddHours(1),
        GanttZoom.Week => date.AddDays(7),
        GanttZoom.Month => date.AddMonths(1),
        GanttZoom.Quarter => date.AddMonths(3),
        GanttZoom.Year => date.AddYears(1),
        _ => date.AddDays(1)
    };

    /// <summary>Gets the default width of one slot, in pixels, at a zoom level.</summary>
    public static int DefaultSlotWidth(GanttZoom zoom) => zoom switch
    {
        GanttZoom.Hour => 44,
        GanttZoom.Day => 40,
        GanttZoom.Week => 58,
        GanttZoom.Month => 74,
        GanttZoom.Quarter => 82,
        _ => 88
    };
}
