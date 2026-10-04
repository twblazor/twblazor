// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;

namespace TwBlazor.Tests.Components.Calendar;

public class TwCalendarMonthViewInteractionTests : TwBlazorTestBase
{
    private static readonly DateTime _march15 = new(2026, 3, 15);

    private static Schedule<string> Event(string name, DateTime day, int startHour, int endHour) => new()
    {
        Name = name,
        DateTimeStart = day.Date.AddHours(startHour),
        DateTimeEnd = day.Date.AddHours(endHour)
    };

    private IRenderedComponent<TwCalendarMonthView<string>> Render(Action<ComponentParameterCollectionBuilder<TwCalendarMonthView<string>>>? configure = null, DateTime? date = null) =>
        TestContext.Render<TwCalendarMonthView<string>>(p =>
        {
            p.Add(x => x.Date, date ?? _march15);
            configure?.Invoke(p);
        });

    private static AngleSharp.Dom.IElement DayButton(IRenderedComponent<TwCalendarMonthView<string>> cut, DateTime day) =>
        cut.Find($"button[aria-label='{day:MMMM d, yyyy}']");

    private static AngleSharp.Dom.IElement Cell(IRenderedComponent<TwCalendarMonthView<string>> cut, DateTime day) =>
        DayButton(cut, day).ParentElement!;

    private static DateTime FocusedDay(IRenderedComponent<TwCalendarMonthView<string>> cut)
    {
        var label = cut.FindAll("td button[tabindex='0']").Single().GetAttribute("aria-label")!;
        return DateTime.ParseExact(label, "MMMM d, yyyy", System.Globalization.CultureInfo.CurrentCulture);
    }

    [Theory]
    [InlineData("ArrowRight", 16)]
    [InlineData("ArrowLeft", 14)]
    [InlineData("ArrowDown", 22)]
    [InlineData("ArrowUp", 8)]
    public void ArrowKeys_MoveTheRovingTabindex(string key, int expectedDay)
    {
        var cut = Render();

        DayButton(cut, _march15).KeyDown(new KeyboardEventArgs { Key = key });

        Assert.Equal(new DateTime(2026, 3, expectedDay), FocusedDay(cut));
    }

    [Theory]
    [InlineData("Home", 9)]
    [InlineData("End", 15)]
    public void HomeAndEnd_JumpToTheEdgesOfTheWeekRow(string key, int expectedDay)
    {
        var cut = Render(date: new DateTime(2026, 3, 11));

        DayButton(cut, new DateTime(2026, 3, 11)).KeyDown(new KeyboardEventArgs { Key = key });

        Assert.Equal(new DateTime(2026, 3, expectedDay), FocusedDay(cut));
    }

    [Theory]
    [InlineData("ArrowLeft")]
    [InlineData("ArrowUp")]
    [InlineData("Home")]
    public void KeysThatWouldLeaveTheGrid_StayOnTheFirstCell(string key)
    {
        // June 2026 starts on a Monday, so the 1st is the very first cell of the grid.
        var june1 = new DateTime(2026, 6, 1);
        var cut = Render(date: june1);

        DayButton(cut, june1).KeyDown(new KeyboardEventArgs { Key = key });

        Assert.Equal(june1, FocusedDay(cut));
    }

    [Fact]
    public void ArrowDown_PastTheLastRow_ClampsToTheLastCell()
    {
        // June 2026's grid is 5 rows (1 Jun to 5 Jul).
        var june30 = new DateTime(2026, 6, 30);
        var cut = Render(date: june30);

        DayButton(cut, june30).KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });

        Assert.Equal(new DateTime(2026, 7, 5), FocusedDay(cut));
    }

    [Fact]
    public void OtherKeys_AreIgnored()
    {
        var cut = Render();

        DayButton(cut, _march15).KeyDown(new KeyboardEventArgs { Key = "x" });

        Assert.Equal(_march15, FocusedDay(cut));
    }

    [Fact]
    public void ChangingTheMonth_ResetsTheFocusedCell_ButReRenderingTheSameMonthDoesNot()
    {
        var cut = Render();
        DayButton(cut, _march15).KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        cut.Render(p => p.Add(x => x.Date, _march15));
        Assert.Equal(new DateTime(2026, 3, 16), FocusedDay(cut));

        cut.Render(p => p.Add(x => x.Date, new DateTime(2026, 4, 10)));
        Assert.Equal(new DateTime(2026, 4, 10), FocusedDay(cut));
    }

    [Fact]
    public async Task Mounting_RegistersTheKeydownGuard_AndDisposingUnregistersIt()
    {
        Render();
        TestContext.JSInterop.VerifyInvoke("twTabs.registerKeydownGuard");

        await TestContext.DisposeAsync();

        TestContext.JSInterop.VerifyInvoke("twTabs.unregisterKeydownGuard");
    }

    [Fact]
    public async Task Mounting_WhenTheCircuitIsDisconnected_SkipsRegistrationAndDisposesQuietly()
    {
        TestContext.JSInterop.SetupVoid("twTabs.registerKeydownGuard", _ => true).SetException(new JSDisconnectedException("gone"));
        Render();

        var exception = await Record.ExceptionAsync(() => TestContext.DisposeAsync().AsTask());

        Assert.Null(exception);
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twTabs.unregisterKeydownGuard");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Disposing_SwallowsTeardownJsErrors(bool disconnected)
    {
        Render();
        Exception error = disconnected ? new JSDisconnectedException("gone") : new InvalidOperationException("no js");
        TestContext.JSInterop.SetupVoid("twTabs.unregisterKeydownGuard", _ => true).SetException(error);

        var exception = await Record.ExceptionAsync(() => TestContext.DisposeAsync().AsTask());

        Assert.Null(exception);
    }

    [Fact]
    public void DayNumberClick_ReportsTheDay()
    {
        DateTime? clicked = null;
        var cut = Render(p => p.Add(x => x.OnDayClick, EventCallback.Factory.Create<DateTime>(this, d => clicked = d)));

        DayButton(cut, new DateTime(2026, 3, 10)).Click();

        Assert.Equal(new DateTime(2026, 3, 10), clicked);
    }

    [Fact]
    public void MoreThanThreeEvents_CollapseIntoAnOverflowLabel_ThatOpensTheDay()
    {
        var day = new DateTime(2026, 3, 10);
        DateTime? clicked = null;
        var cut = Render(p => p
            .Add(x => x.Schedules, [.. Enumerable.Range(0, 5).Select(i => Event($"Event {i}", day, 8 + i, 9 + i))])
            .Add(x => x.OnDayClick, EventCallback.Factory.Create<DateTime>(this, d => clicked = d)));

        var cell = Cell(cut, day);
        Assert.Equal(3, cell.QuerySelectorAll("button[draggable]").Length);

        var overflow = cell.QuerySelectorAll("button").Single(b => b.TextContent.Contains("more"));
        Assert.Equal("+2 more", overflow.TextContent.Trim());
        overflow.Click();

        Assert.Equal(day, clicked);
    }

    [Fact]
    public void EventChip_ShowsItsNameAndStartTime_AndReportsClicks()
    {
        var day = new DateTime(2026, 3, 10);
        var evt = Event("Standup", day, 9, 10);
        Schedule<string>? clicked = null;
        var cut = Render(p => p
            .Add(x => x.Schedules, [evt])
            .Add(x => x.OnEventClick, EventCallback.Factory.Create<Schedule<string>>(this, e => clicked = e)));

        var chip = Cell(cut, day).QuerySelector("button[draggable]")!;

        Assert.Contains("Standup", chip.TextContent);
        Assert.Contains("9:00 AM", chip.TextContent);
        Assert.Equal($"Standup, {evt.DateTimeStart.DateTime:h:mm tt}", chip.GetAttribute("aria-label"));
        chip.Click();
        Assert.Same(evt, clicked);
    }

    [Fact]
    public void EventContentTemplate_ReplacesTheChipContent_ForSingleDayAndMultiDayEvents()
    {
        var day = new DateTime(2026, 3, 10);
        var cut = Render(p => p
            .Add(x => x.Schedules,
            [
                Event("Standup", day, 9, 10),
                new Schedule<string> { Name = "Trip", DateTimeStart = day.AddDays(1), DateTimeEnd = day.AddDays(3) }
            ])
            .Add(x => x.EventContentTemplate, evt => builder => builder.AddMarkupContent(0, $"<em>tpl {evt.Name}</em>")));

        var templated = cut.FindAll("em").Select(e => e.TextContent).ToList();

        Assert.Contains("tpl Standup", templated);
        Assert.Contains("tpl Trip", templated);
    }

    [Fact]
    public void MultiDayEventBar_IsClickableAndDraggable()
    {
        var day = new DateTime(2026, 3, 10);
        var trip = new Schedule<string> { Name = "Trip", DateTimeStart = day, DateTimeEnd = day.AddDays(2).AddHours(10) };
        Schedule<string>? clicked = null;
        Schedule<string>? dragged = null;
        var cut = Render(p => p
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [trip])
            .Add(x => x.OnEventClick, EventCallback.Factory.Create<Schedule<string>>(this, e => clicked = e))
            .Add(x => x.OnEventDragStart, EventCallback.Factory.Create<Schedule<string>>(this, e => dragged = e)));

        var bar = cut.Find("button[aria-label^='Trip']");
        Assert.Equal("true", bar.GetAttribute("draggable"));
        bar.DragStart();
        cut.Find("button[aria-label^='Trip']").Click();

        Assert.Same(trip, dragged);
        Assert.Same(trip, clicked);
    }

    [Fact]
    public void Editable_DragLifecycle_ReportsStartOverDropAndEnd()
    {
        var day = new DateTime(2026, 3, 10);
        var target = new DateTime(2026, 3, 12);
        var evt = Event("Standup", day, 9, 10);
        Schedule<string>? started = null;
        DateTime? over = null;
        DateTime? dropped = null;
        var ended = false;
        var cut = Render(p => p
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [evt])
            .Add(x => x.OnEventDragStart, EventCallback.Factory.Create<Schedule<string>>(this, e => started = e))
            .Add(x => x.OnEventDragOver, EventCallback.Factory.Create<DateTime>(this, d => over = d))
            .Add(x => x.OnEventDrop, EventCallback.Factory.Create<DateTime>(this, d => dropped = d))
            .Add(x => x.OnEventDragEnd, EventCallback.Factory.Create(this, () => ended = true)));

        var chip = Cell(cut, day).QuerySelector("button[draggable]")!;
        Assert.Equal("true", chip.GetAttribute("draggable"));
        chip.DragStart();
        Cell(cut, target).DragEnter();
        Cell(cut, target).Drop();
        Cell(cut, day).QuerySelector("button[draggable]")!.DragEnd();

        Assert.Same(evt, started);
        Assert.Equal(target, over);
        Assert.Equal(target, dropped);
        Assert.True(ended);
    }

    [Fact]
    public void ReadOnlyEvent_IsNotDraggable()
    {
        var day = new DateTime(2026, 3, 10);
        var evt = Event("Locked", day, 9, 10);
        evt.ReadOnly = true;
        var started = false;
        var cut = Render(p => p
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [evt])
            .Add(x => x.OnEventDragStart, EventCallback.Factory.Create<Schedule<string>>(this, _ => started = true)));

        var chip = Cell(cut, day).QuerySelector("button[draggable]")!;
        chip.DragStart();

        Assert.Equal("false", chip.GetAttribute("draggable"));
        Assert.False(started);
    }

    [Fact]
    public void NotEditable_DragAndDrop_AreIgnored()
    {
        var day = new DateTime(2026, 3, 10);
        var called = false;
        var cut = Render(p => p
            .Add(x => x.Schedules, [Event("Standup", day, 9, 10)])
            .Add(x => x.OnEventDragStart, EventCallback.Factory.Create<Schedule<string>>(this, _ => called = true))
            .Add(x => x.OnEventDragOver, EventCallback.Factory.Create<DateTime>(this, _ => called = true))
            .Add(x => x.OnEventDrop, EventCallback.Factory.Create<DateTime>(this, _ => called = true)));

        Cell(cut, day).QuerySelector("button[draggable]")!.DragStart();
        Cell(cut, day).DragEnter();
        Cell(cut, day).Drop();

        Assert.False(called);
    }

    [Fact]
    public void DropPlaceholder_ShowsInlineInTheHoveredDay()
    {
        var day = new DateTime(2026, 3, 10);
        var target = new DateTime(2026, 3, 12);
        var cut = Render(p => p
            .Add(x => x.Editable, true)
            .Add(x => x.DraggedEvent, Event("Moving", day, 9, 10))
            .Add(x => x.DropPreview, target));

        Assert.Contains("Moving", Cell(cut, target).QuerySelector("div[aria-hidden='true'][style='height:1.5rem;']")!.TextContent);
        Assert.Single(cut.FindAll("div[aria-hidden='true'][style='height:1.5rem;']"));
    }

    [Fact]
    public void DropPlaceholder_IsHidden_WhenNotEditableOrWithoutAPreview()
    {
        var day = new DateTime(2026, 3, 10);
        var notEditable = Render(p => p
            .Add(x => x.DraggedEvent, Event("Moving", day, 9, 10))
            .Add(x => x.DropPreview, day));
        var noPreview = Render(p => p
            .Add(x => x.Editable, true)
            .Add(x => x.DraggedEvent, Event("Moving", day, 9, 10)));

        Assert.Empty(notEditable.FindAll("div[aria-hidden='true'][style='height:1.5rem;']"));
        Assert.Empty(noPreview.FindAll("div[aria-hidden='true'][style='height:1.5rem;']"));
    }

    [Fact]
    public void DaysOutsideTheMonth_AreStyledAsPrevNext_AndHighlightedDayPulses()
    {
        var theme = Theme.Components.Require<TwCalendarTheme>();
        var cut = Render(p => p.Add(x => x.HighlightDate, new DateTime(2026, 3, 10)));

        var prevNextClass = theme.MonthCellPrevNext.Split(' ')[0];
        var highlightClass = theme.TodayHighlight.Split(' ')[0];

        Assert.Contains(prevNextClass, Cell(cut, new DateTime(2026, 2, 25)).GetAttribute("class"));
        Assert.DoesNotContain(prevNextClass, Cell(cut, new DateTime(2026, 3, 10)).GetAttribute("class"));
        Assert.Contains(highlightClass, Cell(cut, new DateTime(2026, 3, 10)).GetAttribute("class"));
        Assert.DoesNotContain(highlightClass, Cell(cut, new DateTime(2026, 3, 11)).GetAttribute("class"));
    }

    [Fact]
    public void AllDayEvent_ShowsAllDayInsteadOfAStartTime()
    {
        var day = new DateTime(2026, 3, 10);
        var cut = Render(p => p.Add(x => x.Schedules, [new Schedule<string> { Name = "Holiday", DateTimeStart = day, DateTimeEnd = day.AddDays(1) }]));

        var chip = Cell(cut, day).QuerySelector("button[draggable]")!;
        Assert.Contains("All day", chip.TextContent);
        Assert.Equal("Holiday, All day", chip.GetAttribute("aria-label"));
    }
}
