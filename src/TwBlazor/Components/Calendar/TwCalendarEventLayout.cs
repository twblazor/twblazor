// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Components;

/// <summary>
/// Computes a side-by-side column layout for a set of same-day <see cref="Schedule{T}"/> events, the
/// way Google Calendar/MS Teams lay out overlapping meetings, so <see cref="TwCalendarDayColumn{T}"/>
/// can position each event with a <c>left</c>/<c>width</c> percentage instead of stacking overlapping
/// events directly on top of each other.
/// </summary>
public static class TwCalendarEventLayout
{
    /// <summary>
    /// One event's position within its overlap cluster: <see cref="Column"/> (0-based) of
    /// <see cref="ColumnCount"/> total columns in the cluster it belongs to.
    /// </summary>
    public readonly record struct Slot<T>(Schedule<T> Event, int Column, int ColumnCount);

    /// <summary>
    /// Lays out <paramref name="events"/> (expected to all fall within the same day) into side-by-side
    /// columns.
    /// </summary>
    /// <remarks>
    /// Events are grouped into maximal clusters of transitively-overlapping events (an event that
    /// doesn't directly overlap another can still share a cluster via a third event that overlaps
    /// both), then columns are assigned within each cluster greedily: each event goes in the first
    /// column whose last-placed event already ended by the time this one starts, or a new column if
    /// none is free. Every event in a cluster shares that cluster's total column count, so a cluster's
    /// events always divide the width evenly - this is the simple "even columns" layout rather than the
    /// more elaborate variant that reclaims width from columns that end up empty for part of the
    /// cluster's span.
    /// </remarks>
    public static IReadOnlyList<Slot<T>> LayoutEvents<T>(IEnumerable<Schedule<T>> events)
    {
        var ordered = events
            .OrderBy(e => e.DateTimeStart)
            .ThenByDescending(e => e.DateTimeEnd)
            .ToList();

        var result = new List<Slot<T>>(ordered.Count);
        var cluster = new List<Schedule<T>>();
        var clusterEnd = DateTimeOffset.MinValue;

        foreach (var current in ordered)
        {
            if (cluster.Count > 0 && current.DateTimeStart >= clusterEnd)
            {
                result.AddRange(LayoutCluster(cluster));
                cluster.Clear();
            }

            cluster.Add(current);
            if (current.DateTimeEnd > clusterEnd)
            {
                clusterEnd = current.DateTimeEnd;
            }
        }

        if (cluster.Count > 0)
        {
            result.AddRange(LayoutCluster(cluster));
        }

        return result;
    }

    private static IEnumerable<Slot<T>> LayoutCluster<T>(List<Schedule<T>> cluster)
    {
        // The end time of the last event placed in each column, in column order.
        var columnEnds = new List<DateTimeOffset>();
        var columnByEvent = new Dictionary<Schedule<T>, int>();

        foreach (var evt in cluster)
        {
            var placed = false;
            for (var col = 0; col < columnEnds.Count; col++)
            {
                if (columnEnds[col] <= evt.DateTimeStart)
                {
                    columnEnds[col] = evt.DateTimeEnd;
                    columnByEvent[evt] = col;
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                columnByEvent[evt] = columnEnds.Count;
                columnEnds.Add(evt.DateTimeEnd);
            }
        }

        var columnCount = columnEnds.Count;
        foreach (var evt in cluster)
        {
            yield return new Slot<T>(evt, columnByEvent[evt], columnCount);
        }
    }
}
