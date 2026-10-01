// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Components;

/// <summary>
/// Works out which calendar days an event covers and how events that cover whole days are packed into
/// rows ("lanes") across a run of days. Shared by the Day/Week all-day strip
/// (<see cref="TwCalendarAllDayRow{T}"/>) and the Month view, so both agree on what counts as an
/// all-day or multi-day event.
/// </summary>
internal static class TwCalendarSpans
{
    /// <summary>
    /// One event's bar within a run of days: it starts at <see cref="StartColumn"/> (0-based), covers
    /// <see cref="Span"/> days, and sits in row <see cref="Lane"/> (0-based). The two flags say whether
    /// the event also covers days before the first or after the last day of the run, so the bar can be
    /// drawn with a square edge there.
    /// </summary>
    public readonly record struct Segment<T>(Schedule<T> Event, int StartColumn, int Span, int Lane, bool ContinuesBefore, bool ContinuesAfter);

    /// <summary>
    /// Whether the event covers whole days: it starts and ends exactly at midnight, for example
    /// <c>1 Oct 00:00</c> to <c>2 Oct 00:00</c> (one day) or to <c>4 Oct 00:00</c> (three days).
    /// </summary>
    public static bool IsAllDay<T>(Schedule<T> evt) => IsAllDay(evt.DateTimeStart, evt.DateTimeEnd);

    /// <inheritdoc cref="IsAllDay{T}(Schedule{T})"/>
    public static bool IsAllDay(DateTimeOffset start, DateTimeOffset end) =>
        end > start
        && start.DateTime.TimeOfDay == TimeSpan.Zero
        && end.DateTime.TimeOfDay == TimeSpan.Zero;

    /// <summary>
    /// The last calendar day the event touches. An end at exactly midnight belongs to the day before,
    /// so an all-day event ending at <c>2 Oct 00:00</c> ends on 1 Oct.
    /// </summary>
    public static DateTime GetLastDay<T>(Schedule<T> evt) => GetLastDay(evt.DateTimeStart, evt.DateTimeEnd);

    /// <inheritdoc cref="GetLastDay{T}(Schedule{T})"/>
    public static DateTime GetLastDay(DateTimeOffset start, DateTimeOffset endOffset)
    {
        var end = endOffset.DateTime;
        var last = end.TimeOfDay == TimeSpan.Zero && endOffset > start ? end.Date.AddDays(-1) : end.Date;
        return last < start.DateTime.Date ? start.DateTime.Date : last;
    }

    /// <summary>
    /// Whether the event touches more than one calendar day.
    /// </summary>
    public static bool IsMultiDay<T>(Schedule<T> evt) => GetLastDay(evt) > evt.DateTimeStart.DateTime.Date;

    /// <summary>
    /// Whether the Day and Week views show the event in the all-day strip above the time grid instead of
    /// as a block in it: all-day events, and anything lasting a day or longer.
    /// </summary>
    public static bool IsBanner<T>(Schedule<T> evt) => IsAllDay(evt) || evt.DateTimeEnd - evt.DateTimeStart >= TimeSpan.FromDays(1);

    /// <summary>
    /// Whether the event touches <paramref name="day"/> at all.
    /// </summary>
    public static bool OverlapsDay<T>(Schedule<T> evt, DateTime day) =>
        evt.DateTimeStart.DateTime.Date <= day.Date && GetLastDay(evt) >= day.Date;

    /// <summary>
    /// The number of days the event covers, at least one.
    /// </summary>
    public static int GetDayCount<T>(Schedule<T> evt) => (GetLastDay(evt) - evt.DateTimeStart.DateTime.Date).Days + 1;

    /// <summary>
    /// Lays out the events that touch the <paramref name="dayCount"/> days from <paramref name="firstDay"/>
    /// as bars, clipped to that run of days and packed into the fewest lanes: longer and earlier events
    /// first, each placed in the first lane whose previous bar has already ended.
    /// </summary>
    public static IReadOnlyList<Segment<T>> GetSegments<T>(IEnumerable<Schedule<T>> events, DateTime firstDay, int dayCount)
    {
        var first = firstDay.Date;
        var last = first.AddDays(dayCount - 1);

        var clipped = events
            .Where(e => e.DateTimeStart.DateTime.Date <= last && GetLastDay(e) >= first)
            .Select(e =>
            {
                var start = e.DateTimeStart.DateTime.Date;
                var end = GetLastDay(e);
                var from = start < first ? first : start;
                var to = end > last ? last : end;
                return (Event: e, Start: (from - first).Days, End: (to - first).Days, Before: start < first, After: end > last);
            })
            .OrderBy(s => s.Start)
            .ThenByDescending(s => s.End - s.Start)
            .ThenBy(s => s.Event.DateTimeStart)
            .ToList();

        var laneEnds = new List<int>();
        var segments = new List<Segment<T>>(clipped.Count);

        foreach (var s in clipped)
        {
            var lane = laneEnds.FindIndex(end => end < s.Start);
            if (lane < 0)
            {
                lane = laneEnds.Count;
                laneEnds.Add(s.End);
            }
            else
            {
                laneEnds[lane] = s.End;
            }

            segments.Add(new Segment<T>(s.Event, s.Start, s.End - s.Start + 1, lane, s.Before, s.After));
        }

        return segments;
    }
}
