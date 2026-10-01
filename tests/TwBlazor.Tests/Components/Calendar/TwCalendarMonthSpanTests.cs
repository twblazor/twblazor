// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Calendar;

/// <summary>
/// Events that cover more than one day are drawn as one bar across the Month view's day cells, not as a
/// separate chip in each (or only in the first).
/// </summary>
public class TwCalendarMonthSpanTests : TwBlazorTestBase
{
    // March 2026: Monday-first rows are 23 Feb-1 Mar, 2-8, 9-15, 16-22, 23-29, 30 Mar-5 Apr.
    private static readonly DateTime viewMonth = new(2026, 3, 15);

    private static Schedule<string> Event(string name, DateTime start, DateTime end, string? color = null) => new()
    {
        Name = name,
        DateTimeStart = new DateTimeOffset(start, TimeSpan.Zero),
        DateTimeEnd = new DateTimeOffset(end, TimeSpan.Zero),
        Color = color
    };

    private IRenderedComponent<TwCalendarMonthView<string>> Render(bool editable = false, params Schedule<string>[] events) =>
        TestContext.Render<TwCalendarMonthView<string>>(p => p
            .Add(x => x.Date, viewMonth)
            .Add(x => x.Editable, editable)
            .Add(x => x.Schedules, [.. events]));

    private static AngleSharp.Dom.IElement CellOf(IRenderedComponent<TwCalendarMonthView<string>> cut, DateTime day) =>
        cut.Find($"button[aria-label='{day:MMMM d, yyyy}']").Closest("td")!;

    [Fact]
    public void MultiDayEvent_IsOneBar_StartingInItsFirstDaysCell()
    {
        var cut = Render(false, Event("Conference", new DateTime(2026, 3, 11), new DateTime(2026, 3, 14))); // Wed-Fri

        var bars = cut.FindAll("button[aria-label^='Conference']");

        var bar = Assert.Single(bars);
        Assert.Same(CellOf(cut, new DateTime(2026, 3, 11)), bar.Closest("td"));
    }

    [Fact]
    public void MultiDayBar_IsWideEnoughForEveryDayItCovers()
    {
        var cut = Render(false, Event("Conference", new DateTime(2026, 3, 11), new DateTime(2026, 3, 14))); // three days

        var style = cut.Find("button[aria-label^='Conference']").GetAttribute("style");

        Assert.Contains("width:calc(300% + 2 * (0.5rem + 1px))", style);
    }

    [Fact]
    public void MultiDayBar_HasRoundedEnds_WhenFullyInsideOneWeek()
    {
        var cut = Render(false, Event("Conference", new DateTime(2026, 3, 11), new DateTime(2026, 3, 14)));

        var classes = cut.Find("button[aria-label^='Conference']").GetAttribute("class")!;

        Assert.DoesNotContain("rounded-l-none", classes);
        Assert.DoesNotContain("rounded-r-none", classes);
        Assert.Contains("z-10", classes); // paints above the cells it crosses
    }

    [Fact]
    public void MultiDayBar_LeavesAnInvisibleSpacerInEachCellItCrosses()
    {
        var cut = Render(false, Event("Conference", new DateTime(2026, 3, 11), new DateTime(2026, 3, 14)));

        foreach (var day in new[] { 12, 13 })
        {
            var cell = CellOf(cut, new DateTime(2026, 3, day));
            Assert.Single(cell.QuerySelectorAll("div[aria-hidden='true'].h-5"));
            Assert.Empty(cell.QuerySelectorAll("button[aria-label^='Conference']"));
        }

        Assert.Empty(CellOf(cut, new DateTime(2026, 3, 14)).QuerySelectorAll("div[aria-hidden='true'].h-5"));
    }

    [Fact]
    public void SingleDayEvent_InACrossedCell_SitsBelowTheBar()
    {
        var cut = Render(false,
            Event("Conference", new DateTime(2026, 3, 11), new DateTime(2026, 3, 14)),
            Event("Lunch", new DateTime(2026, 3, 12, 12, 0, 0), new DateTime(2026, 3, 12, 13, 0, 0)));

        var list = CellOf(cut, new DateTime(2026, 3, 12)).QuerySelector("button[aria-label^='Lunch']")!.ParentElement!;
        var children = list.Children.ToList();

        Assert.Equal("div", children[0].TagName.ToLowerInvariant()); // the spacer under the bar
        Assert.Contains("Lunch", children[1].GetAttribute("aria-label"));
    }

    [Fact]
    public void EventSpanningTwoWeeks_IsOneBarPerWeek_WithSquaredOffEdges()
    {
        // Friday 13 March to Tuesday 17 March: the row ending Sunday 15th, then the row starting Monday 16th.
        var cut = Render(false, Event("Offsite", new DateTime(2026, 3, 13), new DateTime(2026, 3, 18)));

        var bars = cut.FindAll("button[aria-label^='Offsite']");

        Assert.Equal(2, bars.Count);
        var first = bars.Single(b => ReferenceEquals(b.Closest("td"), CellOf(cut, new DateTime(2026, 3, 13))));
        var second = bars.Single(b => ReferenceEquals(b.Closest("td"), CellOf(cut, new DateTime(2026, 3, 16))));

        Assert.Contains("width:calc(300% + 2 * (0.5rem + 1px))", first.GetAttribute("style")); // Fri, Sat, Sun
        Assert.Contains("rounded-r-none", first.GetAttribute("class"));
        Assert.DoesNotContain("rounded-l-none", first.GetAttribute("class"));

        Assert.Contains("width:calc(200% + 1 * (0.5rem + 1px))", second.GetAttribute("style")); // Mon, Tue
        Assert.Contains("rounded-l-none", second.GetAttribute("class"));
        Assert.DoesNotContain("rounded-r-none", second.GetAttribute("class"));
    }

    [Fact]
    public void AllDayEvent_OnOneDay_IsAnOrdinaryRow_LabelledAllDay()
    {
        var cut = Render(false, Event("Holiday", new DateTime(2026, 3, 11), new DateTime(2026, 3, 12)));

        var row = CellOf(cut, new DateTime(2026, 3, 11)).QuerySelector("button[aria-label^='Holiday']")!;

        Assert.Contains("All day", row.TextContent);
        Assert.Empty(CellOf(cut, new DateTime(2026, 3, 11)).QuerySelectorAll("div[aria-hidden='true'].h-5"));
    }

    [Fact]
    public void AllDayEvent_OverThreeDays_EndsOnItsLastDay_NotTheMidnightAfter()
    {
        // 11 March 00:00 to 14 March 00:00 covers the 11th, 12th and 13th only.
        var cut = Render(false, Event("Holiday", new DateTime(2026, 3, 11), new DateTime(2026, 3, 14)));

        Assert.Contains("width:calc(300%", cut.Find("button[aria-label^='Holiday']").GetAttribute("style"));
        Assert.Empty(CellOf(cut, new DateTime(2026, 3, 14)).QuerySelectorAll("div[aria-hidden='true'].h-5"));
    }

    [Fact]
    public void TimedEventCrossingMidnight_IsABarOverBothDays()
    {
        var cut = Render(false, Event("Night shift", new DateTime(2026, 3, 11, 22, 0, 0), new DateTime(2026, 3, 12, 3, 0, 0)));

        var bar = Assert.Single(cut.FindAll("button[aria-label^='Night shift']"));

        Assert.Contains("width:calc(200%", bar.GetAttribute("style"));
    }

    [Fact]
    public void BarUsesTheEventsColor()
    {
        var cut = Render(false, Event("Conference", new DateTime(2026, 3, 11), new DateTime(2026, 3, 14), "#2563eb"));

        Assert.Contains("border-left-color:#2563eb;", cut.Find("button[aria-label^='Conference']").GetAttribute("style"));
    }

    [Fact]
    public void OverlappingBars_StackInLanes_AndBothCellsKeepAlignedSpacers()
    {
        var cut = Render(false,
            Event("First", new DateTime(2026, 3, 10), new DateTime(2026, 3, 13)), // Tue-Thu
            Event("Second", new DateTime(2026, 3, 11), new DateTime(2026, 3, 14))); // Wed-Fri

        // Wednesday has both bars passing through: First's spacer (lane 0) then Second's bar (lane 1).
        var wednesdayList = CellOf(cut, new DateTime(2026, 3, 11)).QuerySelector("button[aria-label^='Second']")!.ParentElement!;
        var children = wednesdayList.Children.ToList();
        Assert.Equal("div", children[0].TagName.ToLowerInvariant());
        Assert.Contains("Second", children[1].GetAttribute("aria-label"));

        // Thursday still reserves two lanes, though only Second starts earlier, so Second's spacer is lane 1.
        var thursday = CellOf(cut, new DateTime(2026, 3, 12));
        Assert.Equal(2, thursday.QuerySelectorAll("div[aria-hidden='true'].h-5").Length);
    }

    [Fact]
    public void ManyBars_LeaveNoRoomForSingleDayRows_ShowingMoreLinkInstead()
    {
        var events = new[]
        {
            Event("A", new DateTime(2026, 3, 10), new DateTime(2026, 3, 12)),
            Event("B", new DateTime(2026, 3, 10), new DateTime(2026, 3, 12)),
            Event("C", new DateTime(2026, 3, 10), new DateTime(2026, 3, 12)),
            Event("Lunch", new DateTime(2026, 3, 10, 12, 0, 0), new DateTime(2026, 3, 10, 13, 0, 0))
        };
        var cut = Render(false, events);

        var cell = CellOf(cut, new DateTime(2026, 3, 10));

        Assert.Empty(cell.QuerySelectorAll("button[aria-label^='Lunch']"));
        Assert.Contains("+1 more", cell.TextContent);
    }

    [Fact]
    public void BarAriaLabel_NamesTheDatesItCovers()
    {
        var cut = Render(false, Event("Conference", new DateTime(2026, 3, 11), new DateTime(2026, 3, 14)));

        Assert.Equal("Conference, 11 Mar to 13 Mar", cut.Find("button[aria-label^='Conference']").GetAttribute("aria-label"));
    }

    [Fact]
    public void Bar_OpensTheEvent_WhenClicked()
    {
        Schedule<string>? clicked = null;
        var evt = Event("Conference", new DateTime(2026, 3, 11), new DateTime(2026, 3, 14));
        var cut = TestContext.Render<TwCalendarMonthView<string>>(p => p
            .Add(x => x.Date, viewMonth)
            .Add(x => x.Schedules, [evt])
            .Add(x => x.OnEventClick, EventCallback.Factory.Create<Schedule<string>>(this, e => clicked = e)));

        cut.Find("button[aria-label^='Conference']").Click();

        Assert.Same(evt, clicked);
    }

    [Fact]
    public void Bar_IsDraggable_OnlyWhenEditableAndNotReadOnly()
    {
        var editable = Render(true, Event("Conference", new DateTime(2026, 3, 11), new DateTime(2026, 3, 14)));
        var locked = Event("Locked", new DateTime(2026, 3, 11), new DateTime(2026, 3, 14));
        locked.ReadOnly = true;
        var readOnly = Render(true, locked);
        var notEditable = Render(false, Event("Conference", new DateTime(2026, 3, 11), new DateTime(2026, 3, 14)));

        Assert.Equal("true", editable.Find("button[aria-label^='Conference']").GetAttribute("draggable"));
        Assert.Equal("false", readOnly.Find("button[aria-label^='Locked']").GetAttribute("draggable"));
        Assert.Equal("false", notEditable.Find("button[aria-label^='Conference']").GetAttribute("draggable"));
    }

    [Fact]
    public void DraggingABar_ToAnotherDay_MovesTheWholeEvent_KeepingItsLength()
    {
        List<Schedule<string>>? saved = null;
        var evt = Event("Conference", new DateTime(2026, 3, 11), new DateTime(2026, 3, 14)); // three days
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, viewMonth)
            .Add(x => x.View, TwCalendarView.Month)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [evt])
            .Add(x => x.SchedulesChanged, EventCallback.Factory.Create<List<Schedule<string>>>(this, l => saved = l)));

        cut.Find("button[aria-label^='Conference']").DragStart(new DragEventArgs());
        cut.WaitForAssertion(() => cut.FindAll("button").First(b => b.TextContent.Trim() == "20").Closest("td")!.Drop(new DragEventArgs()));

        cut.WaitForAssertion(() => Assert.NotNull(saved));
        var moved = Assert.Single(saved!);
        Assert.Equal(new DateTimeOffset(2026, 3, 20, 0, 0, 0, TimeSpan.Zero), moved.DateTimeStart);
        Assert.Equal(new DateTimeOffset(2026, 3, 23, 0, 0, 0, TimeSpan.Zero), moved.DateTimeEnd);
    }

    [Fact]
    public void ContentTemplate_AppliesToBars()
    {
        var cut = TestContext.Render<TwCalendarMonthView<string>>(p => p
            .Add(x => x.Date, viewMonth)
            .Add(x => x.Schedules, [Event("Conference", new DateTime(2026, 3, 11), new DateTime(2026, 3, 14))])
            .Add(x => x.EventContentTemplate, evt => builder => builder.AddContent(0, $"custom {evt.Name}")));

        Assert.Contains("custom Conference", cut.Find("button[aria-label^='Conference']").TextContent);
    }
}
