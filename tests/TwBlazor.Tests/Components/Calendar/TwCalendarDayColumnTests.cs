// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Calendar;

public class TwCalendarDayColumnTests : TwBlazorTestBase
{
    private static readonly DateTime _day = new(2026, 3, 18);

    private static Schedule<string> Event(string name, DateTime start, DateTime end) => new()
    {
        Name = name,
        DateTimeStart = start,
        DateTimeEnd = end
    };

    private IRenderedComponent<TwCalendarDayColumn<string>> Render(Action<ComponentParameterCollectionBuilder<TwCalendarDayColumn<string>>>? configure = null, bool editable = true) =>
        TestContext.Render<TwCalendarDayColumn<string>>(p =>
        {
            p.Add(x => x.Date, _day);
            configure?.Invoke(p);
            // Slots only exist in an editable calendar, which is what most of these tests exercise.
            p.Add(x => x.Editable, editable);
        });

    private static IReadOnlyList<AngleSharp.Dom.IElement> Slots(IRenderedComponent<TwCalendarDayColumn<string>> cut) =>
        [.. cut.FindAll("button").Where(b => b.GetAttribute("draggable") is null)];

    private static IReadOnlyList<AngleSharp.Dom.IElement> Chips(IRenderedComponent<TwCalendarDayColumn<string>> cut) =>
        [.. cut.FindAll("button[draggable]")];

    private static int FocusedSlotIndex(IRenderedComponent<TwCalendarDayColumn<string>> cut) =>
        Slots(cut).ToList().FindIndex(s => s.GetAttribute("tabindex") == "0");

    [Fact]
    public void Renders48SlotsLabelledByTime_AndAGroupLabelledByDate()
    {
        var cut = Render();

        var slots = Slots(cut);
        Assert.Equal(48, slots.Count);
        Assert.Equal(_day.ToString("h:mm tt"), slots[0].GetAttribute("aria-label"));
        Assert.Equal(_day.AddMinutes(23 * 60 + 30).ToString("h:mm tt"), slots[47].GetAttribute("aria-label"));
        Assert.Equal(_day.ToString("dddd, MMMM d"), cut.Find("[role='group']").GetAttribute("aria-label"));
    }

    [Fact]
    public void OnlyTheFirstSlot_StartsInTheTabOrder()
    {
        var cut = Render();

        Assert.Equal(0, FocusedSlotIndex(cut));
        Assert.Equal(1, Slots(cut).Count(s => s.GetAttribute("tabindex") == "0"));
    }

    [Fact]
    public void Editable_SlotClick_ReportsTheSlotStart()
    {
        DateTime? clicked = null;
        var cut = Render(p => p
            .Add(x => x.OnSlotClick, EventCallback.Factory.Create<DateTime>(this, d => clicked = d)));

        Slots(cut)[19].Click();

        Assert.Equal(_day.AddHours(9).AddMinutes(30), clicked);
    }

    [Fact]
    public void NotEditable_RendersNoSlotButtons_OnlyTheGridLines()
    {
        var cut = Render(editable: false);

        // A read-only day would otherwise put 48 unavailable buttons between a screen reader user and
        // the day's events.
        Assert.Empty(Slots(cut));
        Assert.Equal(48, cut.FindAll("div[aria-hidden='true'][style^='height']").Count);
    }

    [Fact]
    public void Editable_SlotsAreNotMarkedDisabled()
    {
        var cut = Render();

        Assert.Null(Slots(cut)[3].GetAttribute("aria-disabled"));
    }

    [Fact]
    public void FocusingASlot_ReportsItsIndex()
    {
        var focused = -1;
        var cut = Render(p => p.Add(x => x.OnSlotFocused, EventCallback.Factory.Create<int>(this, i => focused = i)));

        Slots(cut)[7].Focus();

        Assert.Equal(7, focused);
    }

    [Fact]
    public async Task FocusingASlot_WithoutAHandler_DoesNotThrow()
    {
        var cut = Render();

        var exception = Record.Exception(() => Slots(cut)[7].Focus());

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("ArrowDown", 0, 1)]
    [InlineData("ArrowUp", 5, 4)]
    [InlineData("Home", 5, 0)]
    [InlineData("End", 5, 47)]
    public async Task KeyboardNavigation_MovesTheRovingTabindex(string key, int startSlot, int expected)
    {
        var cut = Render();
        await cut.InvokeAsync(() => cut.Instance.FocusSlotAsync(startSlot));

        cut.Find("[role='group']").KeyDown(new KeyboardEventArgs { Key = key });

        Assert.Equal(expected, FocusedSlotIndex(cut));
    }

    [Theory]
    [InlineData("ArrowUp", 0)]
    [InlineData("ArrowDown", 47)]
    [InlineData("Home", 0)]
    [InlineData("End", 47)]
    public async Task KeyboardNavigation_AtTheEdge_StaysPut(string key, int slot)
    {
        var cut = Render();
        await cut.InvokeAsync(() => cut.Instance.FocusSlotAsync(slot));

        cut.Find("[role='group']").KeyDown(new KeyboardEventArgs { Key = key });

        Assert.Equal(slot, FocusedSlotIndex(cut));
    }

    [Theory]
    [InlineData("ArrowLeft")]
    [InlineData("ArrowRight")]
    [InlineData("a")]
    public async Task HorizontalAndOtherKeys_AreLeftUnhandled(string key)
    {
        var cut = Render();
        await cut.InvokeAsync(() => cut.Instance.FocusSlotAsync(10));

        cut.Find("[role='group']").KeyDown(new KeyboardEventArgs { Key = key });

        Assert.Equal(10, FocusedSlotIndex(cut));
    }

    [Fact]
    public async Task FocusSlotAsync_ClampsOutOfRangeIndexes()
    {
        var cut = Render();

        await cut.InvokeAsync(() => cut.Instance.FocusSlotAsync(500));
        Assert.Equal(47, FocusedSlotIndex(cut));

        await cut.InvokeAsync(() => cut.Instance.FocusSlotAsync(-3));
        Assert.Equal(0, FocusedSlotIndex(cut));
    }

    [Fact]
    public async Task ChangingTheDate_ResetsTheRovingTabindex_ButReRenderingTheSameDateDoesNot()
    {
        var cut = Render();
        await cut.InvokeAsync(() => cut.Instance.FocusSlotAsync(12));

        cut.Render(p => p.Add(x => x.Date, _day));
        Assert.Equal(12, FocusedSlotIndex(cut));

        cut.Render(p => p.Add(x => x.Date, _day.AddDays(1)));
        Assert.Equal(0, FocusedSlotIndex(cut));
    }

    [Fact]
    public void EventChip_IsPositionedByItsStartAndDuration()
    {
        var cut = Render(p => p.Add(x => x.Events, [Event("Standup", _day.AddHours(9), _day.AddHours(11))]));

        var chip = Chips(cut).Single();

        Assert.Contains("top:54rem;", chip.GetAttribute("style"));
        Assert.Contains("height:12rem;", chip.GetAttribute("style"));
        Assert.Contains("left:0%;", chip.GetAttribute("style"));
        Assert.Contains("width:100%;", chip.GetAttribute("style"));
        Assert.Contains("Standup", chip.TextContent);
        Assert.Contains("9:00 AM", chip.TextContent);
    }

    [Fact]
    public void EventChip_HasAnAccessibleLabelWithItsTimeRange()
    {
        var evt = Event("Standup", _day.AddHours(9), _day.AddHours(10));
        var cut = Render(p => p.Add(x => x.Events, [evt]));

        Assert.Equal($"Standup, {evt.DateTimeStart:h:mm tt} to {evt.DateTimeEnd:h:mm tt}", Chips(cut).Single().GetAttribute("aria-label"));
    }

    [Fact]
    public void EventStartingTheDayBefore_IsClippedToMidnight()
    {
        var cut = Render(p => p.Add(x => x.Events, [Event("Overnight", _day.AddHours(-2), _day.AddHours(10))]));

        var style = Chips(cut).Single().GetAttribute("style");

        Assert.Contains("top:0rem;", style);
        Assert.Contains("height:60rem;", style);
    }

    [Fact]
    public void EventEndingTheDayAfter_IsClippedToTheEndOfTheDay()
    {
        var cut = Render(p => p.Add(x => x.Events, [Event("Late", _day.AddHours(22), _day.AddDays(1).AddHours(3))]));

        var style = Chips(cut).Single().GetAttribute("style");

        Assert.Contains("top:132rem;", style);
        Assert.Contains("height:12rem;", style);
    }

    [Fact]
    public void ZeroLengthEvent_StillGetsAMinimumHeight()
    {
        var cut = Render(p => p.Add(x => x.Events, [Event("Instant", _day.AddHours(9), _day.AddHours(9))]));

        Assert.Contains("height:0.5rem;", Chips(cut).Single().GetAttribute("style"));
    }

    [Fact]
    public void OverlappingEvents_ShareTheColumnWidth()
    {
        var cut = Render(p => p.Add(x => x.Events,
        [
            Event("A", _day.AddHours(9), _day.AddHours(11)),
            Event("B", _day.AddHours(10), _day.AddHours(12))
        ]));

        var styles = Chips(cut).Select(c => c.GetAttribute("style")).ToList();

        Assert.All(styles, s => Assert.Contains("width:50%;", s));
        Assert.Contains(styles, s => s!.Contains("left:0%;"));
        Assert.Contains(styles, s => s!.Contains("left:50%;"));
    }

    [Fact]
    public void EventColor_IsAppliedToTheChip()
    {
        var evt = Event("Coloured", _day.AddHours(9), _day.AddHours(10));
        evt.Color = "#9333ea";
        var cut = Render(p => p.Add(x => x.Events, [evt]));

        Assert.Contains("#9333ea", Chips(cut).Single().GetAttribute("style"));
    }

    [Fact]
    public void EventContentTemplate_ReplacesTheDefaultChipContent()
    {
        var cut = Render(p => p
            .Add(x => x.Events, [Event("Standup", _day.AddHours(9), _day.AddHours(10))])
            .Add(x => x.EventContentTemplate, evt => builder => builder.AddMarkupContent(0, $"<em>custom {evt.Name}</em>")));

        var chip = Chips(cut).Single();

        Assert.Equal("custom Standup", chip.QuerySelector("em")!.TextContent);
        Assert.DoesNotContain("9:00 AM", chip.TextContent);
    }

    [Fact]
    public void ClickingAChip_ReportsTheEvent()
    {
        var evt = Event("Standup", _day.AddHours(9), _day.AddHours(10));
        Schedule<string>? clicked = null;
        var cut = Render(p => p
            .Add(x => x.Events, [evt])
            .Add(x => x.OnEventClick, EventCallback.Factory.Create<Schedule<string>>(this, e => clicked = e)));

        Chips(cut).Single().Click();

        Assert.Same(evt, clicked);
    }

    [Fact]
    public void Editable_EventChip_IsDraggable_AndStartingADragReportsTheEvent()
    {
        var evt = Event("Standup", _day.AddHours(9), _day.AddHours(10));
        Schedule<string>? dragged = null;
        var cut = Render(p => p
            .Add(x => x.Events, [evt])
            .Add(x => x.OnEventDragStart, EventCallback.Factory.Create<Schedule<string>>(this, e => dragged = e)));

        var chip = Chips(cut).Single();
        chip.DragStart();

        Assert.Equal("true", chip.GetAttribute("draggable"));
        Assert.Same(evt, dragged);
    }

    [Fact]
    public void ReadOnlyEvent_IsNotDraggable_AndIgnoresDragStart()
    {
        var evt = Event("Locked", _day.AddHours(9), _day.AddHours(10));
        evt.ReadOnly = true;
        var dragStarted = false;
        var cut = Render(p => p
            .Add(x => x.Events, [evt])
            .Add(x => x.OnEventDragStart, EventCallback.Factory.Create<Schedule<string>>(this, _ => dragStarted = true)));

        var chip = Chips(cut).Single();
        chip.DragStart();

        Assert.Equal("false", chip.GetAttribute("draggable"));
        Assert.False(dragStarted);
    }

    [Fact]
    public void NonEditableColumn_EventsAreNotDraggable()
    {
        var cut = Render(p => p.Add(x => x.Events, [Event("Standup", _day.AddHours(9), _day.AddHours(10))]), editable: false);

        Assert.Equal("false", Chips(cut).Single().GetAttribute("draggable"));
    }

    [Fact]
    public void EndingADrag_IsReported()
    {
        var ended = false;
        var cut = Render(p => p
            .Add(x => x.Events, [Event("Standup", _day.AddHours(9), _day.AddHours(10))])
            .Add(x => x.OnEventDragEnd, EventCallback.Factory.Create(this, () => ended = true)));

        Chips(cut).Single().DragEnd();

        Assert.True(ended);
    }

    [Fact]
    public void WhileDragging_ChipsIgnorePointerEvents()
    {
        var evt = Event("Standup", _day.AddHours(9), _day.AddHours(10));
        var cut = Render(p => p
            .Add(x => x.Events, [evt])
            .Add(x => x.DraggedEvent, evt));

        Assert.Contains(Theme.Interaction.PointerEventsNone, Chips(cut).Single().GetAttribute("class"));
    }

    [Fact]
    public void Editable_DragEnterAndDrop_ReportTheSlotStart()
    {
        DateTime? over = null;
        DateTime? dropped = null;
        var cut = Render(p => p
            .Add(x => x.OnEventDragOver, EventCallback.Factory.Create<DateTime>(this, d => over = d))
            .Add(x => x.OnEventDrop, EventCallback.Factory.Create<DateTime>(this, d => dropped = d)));

        Slots(cut)[20].DragEnter();
        Slots(cut)[20].Drop();

        Assert.Equal(_day.AddHours(10), over);
        Assert.Equal(_day.AddHours(10), dropped);
    }

    [Fact]
    public void DropPlaceholder_IsSizedToTheDraggedEventAndPositionedAtThePreview()
    {
        var dragged = Event("Moving", _day.AddHours(9), _day.AddHours(11));
        var cut = Render(p => p
            .Add(x => x.DraggedEvent, dragged)
            .Add(x => x.DropPreview, _day.AddHours(14)));

        var placeholder = cut.Find("div[aria-hidden='true']");

        Assert.Contains("top:84rem;", placeholder.GetAttribute("style"));
        Assert.Contains("height:12rem;", placeholder.GetAttribute("style"));
        Assert.Contains("Moving", placeholder.TextContent);
        Assert.Contains("2:00 PM", placeholder.TextContent);
    }

    [Fact]
    public void DropPlaceholder_IsClippedToTheEndOfTheDay()
    {
        var dragged = Event("Moving", _day.AddHours(9), _day.AddHours(13));
        var cut = Render(p => p
            .Add(x => x.DraggedEvent, dragged)
            .Add(x => x.DropPreview, _day.AddHours(22)));

        Assert.Contains("height:12rem;", cut.Find("div[aria-hidden='true']").GetAttribute("style"));
    }

    [Fact]
    public void DropPlaceholder_IsNotShown_WhenThePreviewIsOnADifferentDay()
    {
        var cut = Render(p => p
            .Add(x => x.DraggedEvent, Event("Moving", _day.AddHours(9), _day.AddHours(11)))
            .Add(x => x.DropPreview, _day.AddDays(1).AddHours(9)));

        Assert.Empty(cut.FindAll("div[aria-hidden='true']"));
    }

    [Fact]
    public void DropPlaceholder_IsNotShown_WithoutAPreview()
    {
        var cut = Render(p => p
            .Add(x => x.DraggedEvent, Event("Moving", _day.AddHours(9), _day.AddHours(11))));

        Assert.Empty(cut.FindAll("div[aria-hidden='true']"));
    }

    [Fact]
    public void DropPlaceholder_IsNotShown_WhenNotEditable()
    {
        var cut = Render(p => p
            .Add(x => x.DraggedEvent, Event("Moving", _day.AddHours(9), _day.AddHours(11)))
            .Add(x => x.DropPreview, _day.AddHours(9)), editable: false);

        Assert.Empty(cut.FindAll("div[aria-hidden='true']:not([style^='height'])"));
    }

    [Fact]
    public void DropPlaceholder_IsNotShown_ForAllDayEvents()
    {
        var cut = Render(p => p
            .Add(x => x.DraggedEvent, Event("Holiday", _day, _day.AddDays(1)))
            .Add(x => x.DropPreview, _day.AddHours(9)));

        Assert.Empty(cut.FindAll("div[aria-hidden='true']"));
    }

    [Fact]
    public async Task Mounting_RegistersTheKeydownGuard_AndDisposingUnregistersIt()
    {
        var cut = Render();
        TestContext.JSInterop.VerifyInvoke("twTabs.registerKeydownGuard");

        await TestContext.DisposeAsync();

        TestContext.JSInterop.VerifyInvoke("twTabs.unregisterKeydownGuard");
    }

    [Fact]
    public async Task Mounting_WhenTheCircuitIsDisconnected_SkipsRegistrationAndDisposesQuietly()
    {
        TestContext.JSInterop.SetupVoid("twTabs.registerKeydownGuard", _ => true).SetException(new JSDisconnectedException("gone"));
        var cut = Render();

        var exception = await Record.ExceptionAsync(() => TestContext.DisposeAsync().AsTask());

        Assert.Null(exception);
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twTabs.unregisterKeydownGuard");
        Assert.NotNull(cut);
    }

    [Theory]
    [InlineData(typeof(JSDisconnectedException))]
    [InlineData(typeof(InvalidOperationException))]
    public async Task Disposing_SwallowsTeardownJsErrors(Type exceptionType)
    {
        var cut = Render();
        var exception = (Exception)(exceptionType == typeof(JSDisconnectedException)
            ? new JSDisconnectedException("gone")
            : new InvalidOperationException("no js"));
        TestContext.JSInterop.SetupVoid("twTabs.unregisterKeydownGuard", _ => true).SetException(exception);

        var thrown = await Record.ExceptionAsync(() => TestContext.DisposeAsync().AsTask());

        Assert.Null(thrown);
        Assert.NotNull(cut);
    }
}
