// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Reflection;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;

namespace TwBlazor.Tests.Components.Calendar;

public class TwCalendarWeekViewTests : TwBlazorTestBase
{
    // Wednesday; the displayed week is Monday 16 to Sunday 22 March 2026.
    private static readonly DateTime _wednesday = new(2026, 3, 18);
    private static readonly DateTime _monday = new(2026, 3, 16);

    private static Schedule<string> Event(string name, DateTime start, DateTime end) => new()
    {
        Name = name,
        DateTimeStart = start,
        DateTimeEnd = end
    };

    private IRenderedComponent<TwCalendarWeekView<string>> Render(Action<ComponentParameterCollectionBuilder<TwCalendarWeekView<string>>>? configure = null, DateTime? date = null) =>
        TestContext.Render<TwCalendarWeekView<string>>(p =>
        {
            p.Add(x => x.Date, date ?? _wednesday);
            configure?.Invoke(p);
        });

    private static IReadOnlyList<AngleSharp.Dom.IElement> Columns(IRenderedComponent<TwCalendarWeekView<string>> cut) =>
        [.. cut.FindAll("[role='group']:not([aria-label='All day events'])")];

    private static AngleSharp.Dom.IElement Slot(IRenderedComponent<TwCalendarWeekView<string>> cut, int column, int slot) =>
        Columns(cut)[column].QuerySelectorAll("button:not([draggable])")[slot];

    private static void SetNow(IRenderedComponent<TwCalendarWeekView<string>> cut, DateTime now)
    {
        typeof(TwCalendarWeekView<string>).GetField("now", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(cut.Instance, now);
        cut.Render(p => p.Add(x => x.MaxHeight, "40rem"));
    }

    private static AngleSharp.Dom.IElement Header(IRenderedComponent<TwCalendarWeekView<string>> cut, DateTime day) =>
        cut.FindAll("button").First(b => b.TextContent.Trim() == day.ToString("ddd d"));

    [Fact]
    public void RendersSevenDayHeaders_MondayToSunday()
    {
        var cut = Render();

        for (var i = 0; i < 7; i++)
        {
            Assert.NotNull(Header(cut, _monday.AddDays(i)));
        }
    }

    [Fact]
    public void RendersSevenDayColumns_AndA24HourGutter()
    {
        var cut = Render();

        Assert.Equal(7, Columns(cut).Count);
        Assert.Equal(24, cut.FindAll("div[style='height:6rem;']").Count);
    }

    [Fact]
    public void DayHeaderClick_ReportsThatDay()
    {
        DateTime? clicked = null;
        var cut = Render(p => p.Add(x => x.OnDayHeaderClick, EventCallback.Factory.Create<DateTime>(this, d => clicked = d)));

        Header(cut, _monday.AddDays(2)).Click();

        Assert.Equal(_monday.AddDays(2), clicked);
    }

    [Fact]
    public void DayHeaders_ShowAPointerCursor_OnlyWhenClickable()
    {
        var clickable = Render(p => p.Add(x => x.OnDayHeaderClick, EventCallback.Factory.Create<DateTime>(this, _ => { })));
        var plain = Render();

        Assert.Contains(Theme.Interaction.PointerCursor, Header(clickable, _monday).GetAttribute("class"));
        Assert.DoesNotContain(Theme.Interaction.PointerCursor, Header(plain, _monday).GetAttribute("class"));
    }

    [Fact]
    public void HighlightDate_PulsesOnlyThatColumn()
    {
        var withHighlight = Render(p => p.Add(x => x.HighlightDate, _wednesday));
        var without = Render();
        var highlightClass = Theme.Components.Require<TwCalendarTheme>().TodayHighlight.Split(' ')[0];

        var highlighted = Columns(withHighlight).Select(c => c.GetAttribute("class")!.Contains(highlightClass)).ToList();

        Assert.Equal([false, false, true, false, false, false, false], highlighted);
        Assert.All(Columns(without), c => Assert.DoesNotContain(highlightClass, c.GetAttribute("class")));
    }

    [Fact]
    public void EventsAreFilteredIntoTheirOwnDayColumn()
    {
        var cut = Render(p => p.Add(x => x.Schedules,
        [
            Event("Mon event", _monday.AddHours(9), _monday.AddHours(10)),
            Event("Fri event", _monday.AddDays(4).AddHours(9), _monday.AddDays(4).AddHours(10)),
            Event("Next week", _monday.AddDays(8).AddHours(9), _monday.AddDays(8).AddHours(10))
        ]));

        var columns = Columns(cut);

        Assert.Contains("Mon event", columns[0].TextContent);
        Assert.Contains("Fri event", columns[4].TextContent);
        Assert.DoesNotContain("Next week", cut.Markup);
    }

    [Fact]
    public void BannerEvents_AreNotInTheTimeGrid()
    {
        var cut = Render(p => p.Add(x => x.Schedules, [Event("Holiday", _monday, _monday.AddDays(1))]));

        Assert.All(Columns(cut), c => Assert.DoesNotContain("Holiday", c.TextContent));
        Assert.Contains("Holiday", cut.Markup);
    }

    [Fact]
    public void NowIndicator_ShowsADotOnTodaysColumn_WhenTodayIsInTheWeek()
    {
        var cut = Render(date: DateTime.Today);
        SetNow(cut, DateTime.Today.AddHours(6));

        var line = cut.Find("div[aria-hidden='true'][style='top:36rem;']");
        var expectedLeft = (100m / 7 * (((int)DateTime.Today.DayOfWeek + 6) % 7)).ToString(System.Globalization.CultureInfo.InvariantCulture);

        Assert.Equal($"left:{expectedLeft}%;", Assert.Single(line.QuerySelectorAll("span")).GetAttribute("style"));
    }

    [Fact]
    public void NowIndicator_HasNoDot_WhenTodayIsNotInTheWeek()
    {
        var cut = Render(date: DateTime.Today.AddDays(-30));
        SetNow(cut, DateTime.Today.AddHours(6));

        var line = cut.Find("div[aria-hidden='true'][style='top:36rem;']");

        Assert.Empty(line.QuerySelectorAll("span"));
    }

    [Fact]
    public void NowTick_RefreshesTheCurrentTime()
    {
        var cut = Render();
        SetNow(cut, DateTime.Today.AddHours(1));
        Assert.NotEmpty(cut.FindAll("div[style='top:6rem;']"));

        typeof(TwCalendarWeekView<string>).GetMethod("OnNowTick", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(cut.Instance, [null]);

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll("div[style='top:6rem;']")));
    }

    [Fact]
    public void ScrollContainer_UsesTheMaxHeight()
    {
        var cut = Render(p => p.Add(x => x.MaxHeight, "25rem"));

        Assert.NotEmpty(cut.FindAll("div[style='max-height:25rem;']"));
    }

    [Fact]
    public void MountingScrollsToTheConfiguredTimeOnce()
    {
        var cut = Render(p => p.Add(x => x.ScrollToTime, TimeSpan.FromHours(6)));
        cut.Render(p => p.Add(x => x.Date, _wednesday.AddDays(7)));

        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twCalendar.scrollToFraction");
        Assert.Equal(0.25, (double)invocation.Arguments[1]!);
    }

    [Fact]
    public void MountingWhenTheCircuitIsDisconnected_DoesNotThrow()
    {
        TestContext.JSInterop.SetupVoid("twCalendar.scrollToFraction", _ => true).SetException(new JSDisconnectedException("gone"));

        var exception = Record.Exception(() => Render());

        Assert.Null(exception);
    }

    [Fact]
    public async Task Disposing_StopsTheTimer_WithoutThrowing()
    {
        var cut = Render();

        var exception = await Record.ExceptionAsync(() => TestContext.DisposeAsync().AsTask());

        Assert.Null(exception);
        Assert.NotNull(cut);
    }

    [Fact]
    public void ArrowRight_MovesTheRovingTabindexToTheNextColumnAtTheSameRow()
    {
        var cut = Render();
        Slot(cut, 1, 5).Focus();

        Slot(cut, 1, 5).KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        Assert.Equal("0", Slot(cut, 2, 5).GetAttribute("tabindex"));
    }

    [Fact]
    public void ArrowLeft_MovesTheRovingTabindexToThePreviousColumnAtTheSameRow()
    {
        var cut = Render();
        Slot(cut, 3, 8).Focus();

        Slot(cut, 3, 8).KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        Assert.Equal("0", Slot(cut, 2, 8).GetAttribute("tabindex"));
    }

    [Fact]
    public void ArrowRight_OnTheLastColumn_StaysPut()
    {
        var cut = Render();
        Slot(cut, 6, 4).Focus();

        Slot(cut, 6, 4).KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        Assert.Equal("0", Slot(cut, 6, 4).GetAttribute("tabindex"));
        Assert.Equal("-1", Slot(cut, 6, 0).GetAttribute("tabindex"));
    }

    [Fact]
    public void ArrowLeft_OnTheFirstColumn_StaysPut()
    {
        var cut = Render();
        Slot(cut, 0, 7).Focus();

        Slot(cut, 0, 7).KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        Assert.Equal("0", Slot(cut, 0, 7).GetAttribute("tabindex"));
        Assert.Equal("-1", Slot(cut, 1, 7).GetAttribute("tabindex"));
    }

    [Fact]
    public void OtherKeys_AreIgnored()
    {
        var cut = Render();
        Slot(cut, 2, 3).Focus();

        Slot(cut, 2, 3).KeyDown(new KeyboardEventArgs { Key = "x" });

        Assert.Equal("-1", Slot(cut, 3, 3).GetAttribute("tabindex"));
        Assert.Equal("-1", Slot(cut, 1, 3).GetAttribute("tabindex"));
    }

    [Fact]
    public void SlotClick_ReportsTheColumnsDayAndTime()
    {
        DateTime? clicked = null;
        var cut = Render(p => p
            .Add(x => x.Editable, true)
            .Add(x => x.OnSlotClick, EventCallback.Factory.Create<DateTime>(this, d => clicked = d)));

        Slot(cut, 3, 18).Click();

        Assert.Equal(_monday.AddDays(3).AddHours(9), clicked);
    }

    [Fact]
    public void ChipClick_IsForwarded()
    {
        var evt = Event("Standup", _monday.AddHours(9), _monday.AddHours(10));
        Schedule<string>? clicked = null;
        var cut = Render(p => p
            .Add(x => x.Schedules, [evt])
            .Add(x => x.OnEventClick, EventCallback.Factory.Create<Schedule<string>>(this, e => clicked = e)));

        cut.Find("[role='group']:not([aria-label='All day events']) button[draggable]").Click();

        Assert.Same(evt, clicked);
    }

    [Fact]
    public void Drag_CallbacksAreForwardedToTheColumns()
    {
        var evt = Event("Standup", _monday.AddHours(9), _monday.AddHours(10));
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

        var chip = cut.Find("[role='group']:not([aria-label='All day events']) button[draggable]");
        chip.DragStart();
        Slot(cut, 5, 2).DragEnter();
        Slot(cut, 5, 2).Drop();
        cut.Find("[role='group']:not([aria-label='All day events']) button[draggable]").DragEnd();

        Assert.Same(evt, started);
        Assert.Equal(_monday.AddDays(5).AddHours(1), over);
        Assert.Equal(_monday.AddDays(5).AddHours(1), dropped);
        Assert.True(ended);
    }

    [Fact]
    public void DropPreview_ShowsAPlaceholderOnlyInTheTargetDaysColumn()
    {
        var cut = Render(p => p
            .Add(x => x.Editable, true)
            .Add(x => x.DraggedEvent, Event("Moving", _monday.AddHours(9), _monday.AddHours(10)))
            .Add(x => x.DropPreview, _monday.AddDays(2).AddHours(15)));

        var withPlaceholder = Columns(cut).Select(c => c.QuerySelector("div[aria-hidden='true']") is not null).ToList();

        Assert.Equal([false, false, true, false, false, false, false], withPlaceholder);
    }

    [Fact]
    public void EventContentTemplate_IsForwarded()
    {
        var cut = Render(p => p
            .Add(x => x.Schedules, [Event("Standup", _monday.AddHours(9), _monday.AddHours(10))])
            .Add(x => x.EventContentTemplate, evt => builder => builder.AddMarkupContent(0, $"<em>tpl {evt.Name}</em>")));

        Assert.Equal("tpl Standup", cut.Find("[role='group']:not([aria-label='All day events']) em").TextContent);
    }

    [Fact]
    public void Class_IsAppliedToTheRoot()
    {
        var cut = Render(p => p.Add(x => x.Class, "my-week-view"));

        Assert.Contains("my-week-view", cut.Find("div").GetAttribute("class"));
    }
}
