// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Schedule;

public class TwScheduleEventLayoutTests
{
    private static Schedule<string> Event(string name, int startHour, int startMinute, int endHour, int endMinute) => new()
    {
        Name = name,
        DateTimeStart = new DateTimeOffset(2026, 3, 18, startHour, startMinute, 0, TimeSpan.Zero),
        DateTimeEnd = new DateTimeOffset(2026, 3, 18, endHour, endMinute, 0, TimeSpan.Zero)
    };

    [Fact]
    public void LayoutEvents_NoOverlap_EachGetsItsOwnSingleColumn()
    {
        var a = Event("A", 9, 0, 10, 0);
        var b = Event("B", 11, 0, 12, 0);

        var slots = TwScheduleEventLayout.LayoutEvents(new[] { a, b });

        Assert.Equal(2, slots.Count);
        Assert.All(slots, slot => Assert.Equal(1, slot.ColumnCount));
        Assert.All(slots, slot => Assert.Equal(0, slot.Column));
    }

    [Fact]
    public void LayoutEvents_TwoOverlapping_GetSeparateColumnsInATwoColumnCluster()
    {
        var a = Event("A", 9, 0, 10, 0);
        var b = Event("B", 9, 30, 10, 30);

        var slots = TwScheduleEventLayout.LayoutEvents(new[] { a, b });

        var slotA = Assert.Single(slots, s => s.Event == a);
        var slotB = Assert.Single(slots, s => s.Event == b);

        Assert.Equal(2, slotA.ColumnCount);
        Assert.Equal(2, slotB.ColumnCount);
        Assert.NotEqual(slotA.Column, slotB.Column);
    }

    [Fact]
    public void LayoutEvents_BackToBackEvents_ShareOneColumn()
    {
        // B starts exactly when A ends, so they don't overlap and can share column 0.
        var a = Event("A", 9, 0, 10, 0);
        var b = Event("B", 10, 0, 11, 0);

        var slots = TwScheduleEventLayout.LayoutEvents(new[] { a, b });

        Assert.All(slots, slot => Assert.Equal(0, slot.Column));
        Assert.All(slots, slot => Assert.Equal(1, slot.ColumnCount));
    }

    [Fact]
    public void LayoutEvents_TransitiveChainOverlap_SharesOneClusterEvenWithoutDirectOverlap()
    {
        // A overlaps B, B overlaps C, but A and C don't directly overlap - all three still form
        // one cluster (via the running max-end check), and since A and C never overlap each
        // other, the greedy algorithm reuses A's column for C once A has ended. Peak concurrency
        // within the cluster is 2 (A+B, then B+C), so the cluster only ever needs 2 columns even
        // though it contains 3 events.
        var a = Event("A", 9, 0, 10, 0);
        var b = Event("B", 9, 30, 10, 30);
        var c = Event("C", 10, 15, 11, 0);

        var slots = TwScheduleEventLayout.LayoutEvents(new[] { a, b, c });

        Assert.All(slots, slot => Assert.Equal(2, slot.ColumnCount));

        var slotA = Assert.Single(slots, s => s.Event == a);
        var slotB = Assert.Single(slots, s => s.Event == b);
        var slotC = Assert.Single(slots, s => s.Event == c);

        Assert.Equal(slotA.Column, slotC.Column);
        Assert.NotEqual(slotA.Column, slotB.Column);
    }

    [Fact]
    public void LayoutEvents_ClusterEnding_FreesColumnZeroForALaterEvent()
    {
        var a = Event("A", 9, 0, 10, 0);
        var b = Event("B", 9, 30, 10, 30);
        // C starts well after both A and B have ended - a fresh cluster starting back at column 0.
        var c = Event("C", 14, 0, 15, 0);

        var slots = TwScheduleEventLayout.LayoutEvents(new[] { a, b, c });

        var slotC = Assert.Single(slots, s => s.Event == c);
        Assert.Equal(0, slotC.Column);
        Assert.Equal(1, slotC.ColumnCount);
    }

    [Fact]
    public void LayoutEvents_ThreeMutuallyOverlappingEvents_GetThreeDistinctColumns()
    {
        var a = Event("A", 9, 0, 11, 0);
        var b = Event("B", 9, 0, 11, 0);
        var c = Event("C", 9, 0, 11, 0);

        var slots = TwScheduleEventLayout.LayoutEvents(new[] { a, b, c });

        Assert.All(slots, slot => Assert.Equal(3, slot.ColumnCount));
        Assert.Equal(3, slots.Select(s => s.Column).Distinct().Count());
    }

    [Fact]
    public void LayoutEvents_EmptyInput_ReturnsEmpty()
    {
        var slots = TwScheduleEventLayout.LayoutEvents(Array.Empty<Schedule<string>>());

        Assert.Empty(slots);
    }
}
