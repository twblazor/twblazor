// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Calendar;

public class TwCalendarAllDayTests : TwBlazorTestBase
{
    // Wednesday 18 March 2026; its Monday-Sunday week is 16-22 March.
    private static readonly DateTime wednesday = new(2026, 3, 18);

    private static Schedule<string> Event(string name, DateTime start, DateTime end, bool readOnly = false) => new()
    {
        Name = name,
        DateTimeStart = new DateTimeOffset(start, TimeSpan.Zero),
        DateTimeEnd = new DateTimeOffset(end, TimeSpan.Zero),
        ReadOnly = readOnly
    };

    private IRenderedComponent<TwCalendar<string>> Render(TwCalendarView view, bool editable, params Schedule<string>[] events) =>
        TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, wednesday)
            .Add(x => x.View, view)
            .Add(x => x.Editable, editable)
            .Add(x => x.Schedules, [.. events]));

    private static AngleSharp.Dom.IElement? Strip(IRenderedComponent<TwCalendar<string>> cut) =>
        cut.FindAll("[role='group'][aria-label='All day events']").SingleOrDefault();

    [Theory]
    [InlineData(TwCalendarView.Day)]
    [InlineData(TwCalendarView.Week)]
    public void NoStrip_WhenNoEventNeedsIt(TwCalendarView view)
    {
        var cut = Render(view, false, Event("Standup", wednesday.AddHours(9), wednesday.AddHours(10)));

        Assert.Null(Strip(cut));
        Assert.DoesNotContain("All day", cut.Markup);
    }

    [Theory]
    [InlineData(TwCalendarView.Day)]
    [InlineData(TwCalendarView.Week)]
    public void AllDayEvent_ShowsInTheStrip_NotInTheTimeGrid(TwCalendarView view)
    {
        var cut = Render(view, false, Event("Holiday", wednesday, wednesday.AddDays(1)));

        var strip = Strip(cut);
        Assert.NotNull(strip);
        Assert.Contains("Holiday", strip!.TextContent);
        Assert.Contains("All day", cut.Markup);

        var grid = cut.Find("div.overflow-y-auto");
        Assert.DoesNotContain("Holiday", grid.TextContent);
    }

    [Fact]
    public void TimedEvent_StaysInTheGrid_WhileAnAllDayEventSitsInTheStrip()
    {
        var cut = Render(TwCalendarView.Day, false,
            Event("Holiday", wednesday, wednesday.AddDays(1)),
            Event("Standup", wednesday.AddHours(9), wednesday.AddHours(10)));

        Assert.DoesNotContain("Standup", Strip(cut)!.TextContent);
        Assert.Contains("Standup", cut.Find("div.overflow-y-auto").TextContent);
    }

    [Fact]
    public void WeekView_MultiDayEvent_IsOneBarAcrossItsColumns()
    {
        // Wednesday to Friday, all day: columns 3-5 of the week.
        var cut = Render(TwCalendarView.Week, false, Event("Conference", wednesday, wednesday.AddDays(3)));

        var bar = Strip(cut)!.QuerySelector("button")!;

        Assert.Contains("grid-column:3 / span 3", bar.GetAttribute("style"));
        Assert.Contains("grid-row:1", bar.GetAttribute("style"));
        Assert.Single(Strip(cut)!.QuerySelectorAll("button"));
    }

    [Fact]
    public void WeekView_StripGridHasOneColumnPerDay()
    {
        var cut = Render(TwCalendarView.Week, false, Event("Conference", wednesday, wednesday.AddDays(3)));

        Assert.Contains("grid-template-columns:repeat(7,minmax(0,1fr))", Strip(cut)!.GetAttribute("style"));
    }

    [Fact]
    public void DayView_StripGridHasOneColumn()
    {
        var cut = Render(TwCalendarView.Day, false, Event("Holiday", wednesday, wednesday.AddDays(1)));

        Assert.Contains("grid-template-columns:repeat(1,minmax(0,1fr))", Strip(cut)!.GetAttribute("style"));
    }

    [Fact]
    public void WeekView_BarStartingBeforeTheWeek_IsClippedAndSquaredOff()
    {
        var cut = Render(TwCalendarView.Week, false, Event("Long", wednesday.AddDays(-5), wednesday.AddDays(1)));

        var bar = Strip(cut)!.QuerySelector("button")!;

        Assert.Contains("grid-column:1 / span 3", bar.GetAttribute("style")); // Mon, Tue, Wed
        Assert.Contains("rounded-l-none", bar.GetAttribute("class"));
        Assert.DoesNotContain("rounded-r-none", bar.GetAttribute("class"));
    }

    [Fact]
    public void WeekView_OverlappingBars_UseSeparateRows()
    {
        var cut = Render(TwCalendarView.Week, false,
            Event("First", wednesday, wednesday.AddDays(2)),
            Event("Second", wednesday.AddDays(1), wednesday.AddDays(3)));

        var rows = Strip(cut)!.QuerySelectorAll("button").Select(b => b.GetAttribute("style")!).ToList();

        Assert.Contains(rows, s => s.Contains("grid-row:1"));
        Assert.Contains(rows, s => s.Contains("grid-row:2"));
    }

    [Fact]
    public void DayView_ShowsAnEventThatStartedTheDayBefore_InTheStrip()
    {
        // 17 March to 20 March all day, viewed on the 18th.
        var cut = Render(TwCalendarView.Day, false, Event("Trip", wednesday.AddDays(-1), wednesday.AddDays(2)));

        Assert.Contains("Trip", Strip(cut)!.TextContent);
    }

    [Fact]
    public void AllDayBar_UsesTheEventsColor()
    {
        var evt = Event("Holiday", wednesday, wednesday.AddDays(1));
        evt.Color = "#2563eb";
        var cut = Render(TwCalendarView.Day, false, evt);

        var style = Strip(cut)!.QuerySelector("button")!.GetAttribute("style");

        Assert.Contains("border-left-color:#2563eb;", style);
    }

    [Fact]
    public void AllDayBar_OpensTheEventDialog_WhenClicked()
    {
        var provider = TestContext.Render<TwDialogProvider>();
        var cut = Render(TwCalendarView.Day, true, Event("Holiday", wednesday, wednesday.AddDays(1)));

        Strip(cut)!.QuerySelector("button")!.Click();

        var dialog = provider.WaitForElement("[role='dialog']");
        Assert.Contains("Edit event", dialog.TextContent);
    }

    [Fact]
    public void AllDayBar_HasAnAccessibleName_WithItsDates()
    {
        var cut = Render(TwCalendarView.Day, false, Event("Holiday", wednesday, wednesday.AddDays(3)));

        Assert.Equal("Holiday, 18 Mar 2026 - 20 Mar 2026 (all day)", Strip(cut)!.QuerySelector("button")!.GetAttribute("aria-label"));
    }

    [Fact]
    public void TimedEventCrossingMidnight_ShowsOnBothDaysInTheGrid()
    {
        var evt = Event("Night shift", wednesday.AddHours(22), wednesday.AddDays(1).AddHours(3));

        var first = Render(TwCalendarView.Day, false, evt);
        Assert.Contains("Night shift", first.Find("div.overflow-y-auto").TextContent);
        Assert.Null(Strip(first));

        var secondDay = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, wednesday.AddDays(1))
            .Add(x => x.View, TwCalendarView.Day)
            .Add(x => x.Schedules, [evt]));
        var chip = secondDay.Find("button[aria-label^='Night shift']");
        Assert.Contains("top:0rem", chip.GetAttribute("style")); // clipped to the start of the second day
    }

    [Fact]
    public void TimedEventCrossingMidnight_ShowsOnBothDaysInTheWeekGrid()
    {
        var evt = Event("Night shift", wednesday.AddHours(22), wednesday.AddDays(1).AddHours(3));
        var cut = Render(TwCalendarView.Week, false, evt);

        Assert.Equal(2, cut.FindAll("button[aria-label^='Night shift']").Count);
    }

    #region Dragging

    /// <summary>
    /// Re-finds and triggers until it lands: the drag-start handler's own renders can replace the
    /// strip's event handlers between a Find and the trigger, which bUnit reports as an unknown handler.
    /// </summary>
    private static void DragEnterCell(IRenderedComponent<TwCalendar<string>> cut, int column) =>
        cut.WaitForAssertion(() => Strip(cut)!.QuerySelectorAll("div[aria-hidden='true']")
            .Single(d => d.GetAttribute("style")!.StartsWith($"grid-column:{column};")).DragEnter(new DragEventArgs()));

    private static void StartDrag(IRenderedComponent<TwCalendar<string>> cut, string name)
    {
        cut.Find($"button[aria-label^='{name}']").DragStart(new DragEventArgs());
        cut.WaitForAssertion(() => Assert.Contains("pointer-events-none", cut.Find($"button[aria-label^='{name}']").GetAttribute("class")));
    }

    [Fact]
    public void DraggingAnAllDayEvent_ToAnotherDay_MovesIt_KeepingMidnightAndLength()
    {
        List<Schedule<string>>? saved = null;
        var evt = Event("Conference", wednesday, wednesday.AddDays(2));
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, wednesday)
            .Add(x => x.View, TwCalendarView.Week)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [evt])
            .Add(x => x.SchedulesChanged, EventCallback.Factory.Create<List<Schedule<string>>>(this, l => saved = l)));

        StartDrag(cut, "Conference");
        cut.WaitForAssertion(() => Strip(cut)!.QuerySelectorAll("div[aria-hidden='true']").ToList()[4].Drop(new DragEventArgs())); // Friday

        cut.WaitForAssertion(() => Assert.NotNull(saved));
        var moved = Assert.Single(saved!);
        Assert.Equal(new DateTimeOffset(wednesday.AddDays(2), TimeSpan.Zero), moved.DateTimeStart);
        Assert.Equal(new DateTimeOffset(wednesday.AddDays(4), TimeSpan.Zero), moved.DateTimeEnd);
    }

    [Fact]
    public void DraggingAnAllDayEvent_OverTheTimeGrid_MovesItToThatDay_NotThatTime()
    {
        List<Schedule<string>>? saved = null;
        var evt = Event("Conference", wednesday, wednesday.AddDays(1));
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, wednesday)
            .Add(x => x.View, TwCalendarView.Week)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [evt])
            .Add(x => x.SchedulesChanged, EventCallback.Factory.Create<List<Schedule<string>>>(this, l => saved = l)));

        StartDrag(cut, "Conference");
        var thursday = cut.FindComponents<TwCalendarDayColumn<string>>()[3];
        thursday.Find("button[aria-label='2:00 PM']").Drop(new DragEventArgs());

        cut.WaitForAssertion(() => Assert.NotNull(saved));
        Assert.Equal(new DateTimeOffset(wednesday.AddDays(1), TimeSpan.Zero), Assert.Single(saved!).DateTimeStart);
    }

    [Fact]
    public void DraggingAnAllDayEvent_ShowsAPlaceholderInTheStrip_AtTheHoveredDay_SizedToItsLength()
    {
        var evt = Event("Conference", wednesday, wednesday.AddDays(2)); // two days
        var cut = Render(TwCalendarView.Week, true, evt);

        StartDrag(cut, "Conference");
        DragEnterCell(cut, 6); // Saturday

        cut.WaitForAssertion(() =>
        {
            var placeholder = Strip(cut)!.QuerySelectorAll("div[aria-hidden='true']").Single(d => d.ClassList.Contains("border-dashed"));
            Assert.Contains("grid-column:6 / span 2", placeholder.GetAttribute("style"));
        });
    }

    [Fact]
    public void DraggingAnAllDayEvent_PlaceholderIsClippedAtTheEndOfTheWeek()
    {
        var evt = Event("Conference", wednesday, wednesday.AddDays(3)); // three days
        var cut = Render(TwCalendarView.Week, true, evt);

        StartDrag(cut, "Conference");
        DragEnterCell(cut, 7); // Sunday

        cut.WaitForAssertion(() =>
        {
            var placeholder = Strip(cut)!.QuerySelectorAll("div[aria-hidden='true']").Single(d => d.ClassList.Contains("border-dashed"));
            Assert.Contains("grid-column:7 / span 1", placeholder.GetAttribute("style"));
        });
    }

    [Fact]
    public void DraggingAnAllDayEvent_DoesNotShowATimedPlaceholderInTheGrid()
    {
        var evt = Event("Conference", wednesday, wednesday.AddDays(1));
        var cut = Render(TwCalendarView.Day, true, evt);

        StartDrag(cut, "Conference");
        cut.FindComponent<TwCalendarDayColumn<string>>().Find("button[aria-label='10:00 AM']").DragEnter(new DragEventArgs());

        Assert.Empty(cut.FindComponent<TwCalendarDayColumn<string>>().FindAll("div[aria-hidden='true']"));
    }

    [Fact]
    public void DraggingATimedEvent_OverTheStrip_DoesNotMoveIt()
    {
        List<Schedule<string>>? saved = null;
        var timed = Event("Standup", wednesday.AddHours(9), wednesday.AddHours(10));
        var allDay = Event("Holiday", wednesday, wednesday.AddDays(1));
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, wednesday)
            .Add(x => x.View, TwCalendarView.Day)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [timed, allDay])
            .Add(x => x.SchedulesChanged, EventCallback.Factory.Create<List<Schedule<string>>>(this, l => saved = l)));

        StartDrag(cut, "Standup");
        Strip(cut)!.QuerySelectorAll("div[aria-hidden='true']")[0].Drop(new DragEventArgs());

        Assert.Null(saved);
    }

    [Fact]
    public void ReadOnlyAllDayEvent_IsNotDraggable()
    {
        var cut = Render(TwCalendarView.Day, true, Event("Holiday", wednesday, wednesday.AddDays(1), readOnly: true));

        Assert.Equal("false", Strip(cut)!.QuerySelector("button")!.GetAttribute("draggable"));
    }

    [Fact]
    public void AllDayEvent_IsNotDraggable_WhenTheCalendarIsNotEditable()
    {
        var cut = Render(TwCalendarView.Day, false, Event("Holiday", wednesday, wednesday.AddDays(1)));

        Assert.Equal("false", Strip(cut)!.QuerySelector("button")!.GetAttribute("draggable"));
    }

    [Fact]
    public void DragEnd_ClearsTheStripPlaceholder_AndRestoresBars()
    {
        var evt = Event("Conference", wednesday, wednesday.AddDays(1));
        var cut = Render(TwCalendarView.Week, true, evt);

        StartDrag(cut, "Conference");
        DragEnterCell(cut, 2);
        cut.WaitForAssertion(() => Assert.Contains(Strip(cut)!.QuerySelectorAll("div[aria-hidden='true']"), d => d.ClassList.Contains("border-dashed")));

        Strip(cut)!.QuerySelector("button")!.DragEnd(new DragEventArgs());

        cut.WaitForAssertion(() =>
        {
            Assert.DoesNotContain(Strip(cut)!.QuerySelectorAll("div[aria-hidden='true']"), d => d.ClassList.Contains("border-dashed"));
            Assert.DoesNotContain("pointer-events-none", Strip(cut)!.QuerySelector("button")!.GetAttribute("class"));
        });
    }

    #endregion
}
