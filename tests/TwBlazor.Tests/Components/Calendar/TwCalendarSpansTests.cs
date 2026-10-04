// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Calendar;

public class TwCalendarSpansTests
{
    private static Schedule<string> Event(string name, DateTime start, DateTime end) => new()
    {
        Name = name,
        DateTimeStart = new DateTimeOffset(start, TimeSpan.Zero),
        DateTimeEnd = new DateTimeOffset(end, TimeSpan.Zero)
    };

    private static readonly DateTime monday = new(2026, 3, 16);

    [Theory]
    [InlineData(2026, 3, 18, 0, 0, 2026, 3, 19, 0, 0, true)] // one whole day
    [InlineData(2026, 3, 18, 0, 0, 2026, 3, 21, 0, 0, true)] // three whole days
    [InlineData(2026, 3, 18, 0, 0, 2026, 3, 18, 23, 59, false)] // ends a minute short of midnight
    [InlineData(2026, 3, 18, 9, 0, 2026, 3, 19, 0, 0, false)] // does not start at midnight
    [InlineData(2026, 3, 18, 0, 0, 2026, 3, 18, 0, 0, false)] // zero length
    [InlineData(2026, 3, 18, 10, 0, 2026, 3, 18, 11, 0, false)]
    public void IsAllDay_RequiresStartAndEndAtMidnight(int sy, int sm, int sd, int sh, int smin, int ey, int em, int ed, int eh, int emin, bool expected)
    {
        var evt = Event("e", new DateTime(sy, sm, sd, sh, smin, 0), new DateTime(ey, em, ed, eh, emin, 0));

        Assert.Equal(expected, TwCalendarSpans.IsAllDay(evt));
    }

    [Fact]
    public void GetLastDay_TreatsAMidnightEndAsBelongingToTheDayBefore()
    {
        Assert.Equal(new DateTime(2026, 3, 20), TwCalendarSpans.GetLastDay(Event("e", new DateTime(2026, 3, 18), new DateTime(2026, 3, 21))));
        Assert.Equal(new DateTime(2026, 3, 18), TwCalendarSpans.GetLastDay(Event("e", new DateTime(2026, 3, 18), new DateTime(2026, 3, 19))));
    }

    [Fact]
    public void GetLastDay_UsesTheEndDate_WhenItEndsDuringADay()
    {
        Assert.Equal(new DateTime(2026, 3, 19), TwCalendarSpans.GetLastDay(Event("e", new DateTime(2026, 3, 18, 22, 0, 0), new DateTime(2026, 3, 19, 3, 0, 0))));
    }

    [Fact]
    public void GetLastDay_NeverGoesBeforeTheStartDay_ForZeroLengthOrReversedEvents()
    {
        var zero = Event("e", new DateTime(2026, 3, 18), new DateTime(2026, 3, 18));
        var reversed = Event("e", new DateTime(2026, 3, 18, 10, 0, 0), new DateTime(2026, 3, 17, 10, 0, 0));

        Assert.Equal(new DateTime(2026, 3, 18), TwCalendarSpans.GetLastDay(zero));
        Assert.Equal(new DateTime(2026, 3, 18), TwCalendarSpans.GetLastDay(reversed));
    }

    [Theory]
    [InlineData(2026, 3, 18, 10, 0, 2026, 3, 18, 11, 0, false, false)]
    [InlineData(2026, 3, 18, 22, 0, 2026, 3, 19, 3, 0, true, false)] // crosses midnight but under a day: timed, two calendar days
    [InlineData(2026, 3, 18, 10, 0, 2026, 3, 19, 11, 0, true, true)] // 25 hours
    [InlineData(2026, 3, 18, 0, 0, 2026, 3, 19, 0, 0, false, true)] // one all-day event
    [InlineData(2026, 3, 18, 0, 0, 2026, 3, 21, 0, 0, true, true)]
    public void IsMultiDay_And_IsBanner_ClassifyEvents(int sy, int sm, int sd, int sh, int smin, int ey, int em, int ed, int eh, int emin, bool multiDay, bool banner)
    {
        var evt = Event("e", new DateTime(sy, sm, sd, sh, smin, 0), new DateTime(ey, em, ed, eh, emin, 0));

        Assert.Equal(multiDay, TwCalendarSpans.IsMultiDay(evt));
        Assert.Equal(banner, TwCalendarSpans.IsBanner(evt));
    }

    [Fact]
    public void OverlapsDay_IncludesEveryDayTheEventTouches_AndNoOthers()
    {
        var evt = Event("e", new DateTime(2026, 3, 18, 22, 0, 0), new DateTime(2026, 3, 20, 3, 0, 0));

        Assert.False(TwCalendarSpans.OverlapsDay(evt, new DateTime(2026, 3, 17)));
        Assert.True(TwCalendarSpans.OverlapsDay(evt, new DateTime(2026, 3, 18, 15, 0, 0)));
        Assert.True(TwCalendarSpans.OverlapsDay(evt, new DateTime(2026, 3, 19)));
        Assert.True(TwCalendarSpans.OverlapsDay(evt, new DateTime(2026, 3, 20)));
        Assert.False(TwCalendarSpans.OverlapsDay(evt, new DateTime(2026, 3, 21)));
    }

    [Fact]
    public void GetDayCount_CountsCalendarDaysTouched()
    {
        Assert.Equal(1, TwCalendarSpans.GetDayCount(Event("e", new DateTime(2026, 3, 18, 9, 0, 0), new DateTime(2026, 3, 18, 10, 0, 0))));
        Assert.Equal(3, TwCalendarSpans.GetDayCount(Event("e", new DateTime(2026, 3, 18), new DateTime(2026, 3, 21))));
        Assert.Equal(2, TwCalendarSpans.GetDayCount(Event("e", new DateTime(2026, 3, 18, 22, 0, 0), new DateTime(2026, 3, 19, 3, 0, 0))));
    }

    [Fact]
    public void GetSegments_PlacesAnEventAtItsColumn_WithItsSpan()
    {
        var evt = Event("e", monday.AddDays(2), monday.AddDays(5)); // Wed-Fri, all day

        var segment = Assert.Single(TwCalendarSpans.GetSegments([evt], monday, 7));

        Assert.Equal(2, segment.StartColumn);
        Assert.Equal(3, segment.Span);
        Assert.Equal(0, segment.Lane);
        Assert.False(segment.ContinuesBefore);
        Assert.False(segment.ContinuesAfter);
    }

    [Fact]
    public void GetSegments_ClipsToTheRun_AndFlagsTheContinuingEdges()
    {
        var evt = Event("e", monday.AddDays(-2), monday.AddDays(10)); // starts two days before, ends the Thursday after

        var segment = Assert.Single(TwCalendarSpans.GetSegments([evt], monday, 7));

        Assert.Equal(0, segment.StartColumn);
        Assert.Equal(7, segment.Span);
        Assert.True(segment.ContinuesBefore);
        Assert.True(segment.ContinuesAfter);
    }

    [Fact]
    public void GetSegments_ExcludesEventsOutsideTheRun()
    {
        var before = Event("before", monday.AddDays(-5), monday.AddDays(-2));
        var after = Event("after", monday.AddDays(8), monday.AddDays(9));

        Assert.Empty(TwCalendarSpans.GetSegments([before, after], monday, 7));
    }

    [Fact]
    public void GetSegments_StacksOverlappingEventsInSeparateLanes_AndReusesAFreeLane()
    {
        var a = Event("a", monday, monday.AddDays(3)); // Mon-Wed
        var b = Event("b", monday.AddDays(1), monday.AddDays(3)); // Tue-Wed, overlaps a
        var c = Event("c", monday.AddDays(3), monday.AddDays(5)); // Thu-Fri, free again in lane 0

        var segments = TwCalendarSpans.GetSegments([c, b, a], monday, 7);

        Assert.Equal(0, segments.Single(s => s.Event == a).Lane);
        Assert.Equal(1, segments.Single(s => s.Event == b).Lane);
        Assert.Equal(0, segments.Single(s => s.Event == c).Lane);
    }

    [Fact]
    public void GetSegments_PlacesTheLongerEventFirst_WhenTwoStartTogether()
    {
        var shorter = Event("short", monday, monday.AddDays(1));
        var longer = Event("long", monday, monday.AddDays(4));

        var segments = TwCalendarSpans.GetSegments([shorter, longer], monday, 7);

        Assert.Equal(0, segments.Single(s => s.Event == longer).Lane);
        Assert.Equal(1, segments.Single(s => s.Event == shorter).Lane);
    }

    [Fact]
    public void GetSegments_SupportsASingleDayRun()
    {
        var evt = Event("e", monday, monday.AddDays(1));

        var segment = Assert.Single(TwCalendarSpans.GetSegments([evt], monday, 1));

        Assert.Equal(0, segment.StartColumn);
        Assert.Equal(1, segment.Span);
    }
}
