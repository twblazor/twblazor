// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Reflection;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Calendar;

public class TwCalendarDayViewTests : TwBlazorTestBase
{
    private static readonly DateTime day = new(2026, 3, 18);

    private static Schedule<string> Event(string name, DateTime start, DateTime end) => new()
    {
        Name = name,
        DateTimeStart = start,
        DateTimeEnd = end
    };

    private IRenderedComponent<TwCalendarDayView<string>> Render(Action<ComponentParameterCollectionBuilder<TwCalendarDayView<string>>>? configure = null) =>
        TestContext.Render<TwCalendarDayView<string>>(p =>
        {
            p.Add(x => x.Date, day);
            configure?.Invoke(p);
        });

    private static void SetNow(IRenderedComponent<TwCalendarDayView<string>> cut, DateTime now)
    {
        typeof(TwCalendarDayView<string>).GetField("now", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(cut.Instance, now);
        cut.Render(p => p.Add(x => x.MaxHeight, "40rem"));
    }

    [Fact]
    public void Renders24HourLabels_AndTheNowLabel()
    {
        var cut = Render();
        SetNow(cut, day.AddHours(13).AddMinutes(30));

        var labels = cut.FindAll("div[style='height:6rem;']");

        Assert.Equal(24, labels.Count);
        Assert.Equal(day.ToString("h tt"), labels[0].TextContent);
        Assert.Equal(day.AddHours(23).ToString("h tt"), labels[23].TextContent);
        Assert.Contains(cut.FindAll("div[style='top:81rem;']"), e => e.TextContent == day.AddHours(13).AddMinutes(30).ToString("h:mm tt"));
    }

    [Fact]
    public void NowIndicator_IsPositionedByTheCurrentTime_AndAlwaysShowsItsDot()
    {
        var cut = Render();
        SetNow(cut, day.AddHours(6));

        var line = cut.Find("div[aria-hidden='true'][style='top:36rem;']");

        Assert.Single(line.QuerySelectorAll("span[style='left:0;']"));
    }

    [Fact]
    public void ScrollContainer_UsesTheMaxHeight()
    {
        var cut = Render(p => p.Add(x => x.MaxHeight, "20rem"));

        Assert.NotEmpty(cut.FindAll("div[style='max-height:20rem;']"));
    }

    [Fact]
    public void MountingScrollsToTheConfiguredTime()
    {
        Render(p => p.Add(x => x.ScrollToTime, TimeSpan.FromHours(12)));

        var invocation = TestContext.JSInterop.VerifyInvoke("twCalendar.scrollToFraction");
        Assert.Equal(0.5, (double)invocation.Arguments[1]!);
    }

    [Theory]
    [InlineData(-5, 0)]
    [InlineData(48, 1)]
    public void ScrollToTime_IsClampedToASingleDay(int hours, double expectedFraction)
    {
        Render(p => p.Add(x => x.ScrollToTime, TimeSpan.FromHours(hours)));

        var invocation = TestContext.JSInterop.VerifyInvoke("twCalendar.scrollToFraction");
        Assert.Equal(expectedFraction, (double)invocation.Arguments[1]!);
    }

    [Fact]
    public void ScrollsOnlyOnTheFirstRender()
    {
        var cut = Render();

        cut.Render(p => p.Add(x => x.Date, day.AddDays(1)));

        Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twCalendar.scrollToFraction");
    }

    [Fact]
    public void MountingWhenTheCircuitIsDisconnected_DoesNotThrow()
    {
        TestContext.JSInterop.SetupVoid("twCalendar.scrollToFraction", _ => true).SetException(new JSDisconnectedException("gone"));

        var exception = Record.Exception(() => Render());

        Assert.Null(exception);
    }

    [Fact]
    public void NowTick_RefreshesTheCurrentTime()
    {
        var cut = Render();
        SetNow(cut, day.AddHours(1));
        Assert.NotEmpty(cut.FindAll("div[style='top:6rem;']"));

        typeof(TwCalendarDayView<string>).GetMethod("OnNowTick", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(cut.Instance, [null]);

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll("div[style='top:6rem;']")));
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
    public void OnlyTimedEventsTouchingTheDay_AreShownInTheGrid()
    {
        var cut = Render(p => p.Add(x => x.Schedules,
        [
            Event("Standup", day.AddHours(9), day.AddHours(10)),
            Event("Overnight", day.AddHours(-3), day.AddHours(2)),
            Event("Other day", day.AddDays(2).AddHours(9), day.AddDays(2).AddHours(10)),
            Event("Holiday", day, day.AddDays(1))
        ]));

        var chipLabels = cut.FindAll("[role='group']:not([aria-label='All day events']) button[draggable]").Select(b => b.GetAttribute("aria-label")!).ToList();

        Assert.Equal(2, chipLabels.Count);
        Assert.Contains(chipLabels, l => l.StartsWith("Standup"));
        Assert.Contains(chipLabels, l => l.StartsWith("Overnight"));
    }

    [Fact]
    public void AllDayEvents_GoInTheStripAboveTheGrid()
    {
        var cut = Render(p => p.Add(x => x.Schedules, [Event("Holiday", day, day.AddDays(1))]));

        Assert.DoesNotContain(cut.FindAll("[role='group']:not([aria-label='All day events']) button[draggable]"), b => b.GetAttribute("aria-label")!.StartsWith("Holiday"));
        Assert.Contains("Holiday", cut.Markup);
    }

    [Fact]
    public void EditableSlotClick_IsForwarded()
    {
        DateTime? clicked = null;
        var cut = Render(p => p
            .Add(x => x.Editable, true)
            .Add(x => x.OnSlotClick, EventCallback.Factory.Create<DateTime>(this, d => clicked = d)));

        cut.FindAll("[role='group']:not([aria-label='All day events']) button")[2].Click();

        Assert.Equal(day.AddHours(1), clicked);
    }

    [Fact]
    public void ChipClick_IsForwarded()
    {
        var evt = Event("Standup", day.AddHours(9), day.AddHours(10));
        Schedule<string>? clicked = null;
        var cut = Render(p => p
            .Add(x => x.Schedules, [evt])
            .Add(x => x.OnEventClick, EventCallback.Factory.Create<Schedule<string>>(this, e => clicked = e)));

        cut.Find("[role='group']:not([aria-label='All day events']) button[draggable]").Click();

        Assert.Same(evt, clicked);
    }

    [Fact]
    public void Class_IsAppliedToTheRoot()
    {
        var cut = Render(p => p.Add(x => x.Class, "my-day-view"));

        Assert.Contains("my-day-view", cut.Find("div").GetAttribute("class"));
    }

    [Fact]
    public void Drag_CallbacksAreForwardedToTheColumn()
    {
        var evt = Event("Standup", day.AddHours(9), day.AddHours(10));
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
        cut.FindAll("[role='group']:not([aria-label='All day events']) button")[4].DragEnter();
        cut.FindAll("[role='group']:not([aria-label='All day events']) button")[4].Drop();
        cut.Find("[role='group']:not([aria-label='All day events']) button[draggable]").DragEnd();

        Assert.Same(evt, started);
        Assert.Equal(day.AddHours(2), over);
        Assert.Equal(day.AddHours(2), dropped);
        Assert.True(ended);
    }

    [Fact]
    public void DraggedEventAndPreview_ShowThePlaceholder()
    {
        var cut = Render(p => p
            .Add(x => x.Editable, true)
            .Add(x => x.DraggedEvent, Event("Moving", day.AddHours(9), day.AddHours(10)))
            .Add(x => x.DropPreview, day.AddHours(15)));

        Assert.Contains(cut.FindAll("[role='group']:not([aria-label='All day events']) div[aria-hidden='true']"), d => d.TextContent.Contains("Moving"));
    }

    [Fact]
    public void EventContentTemplate_IsForwarded()
    {
        var cut = Render(p => p
            .Add(x => x.Schedules, [Event("Standup", day.AddHours(9), day.AddHours(10))])
            .Add(x => x.EventContentTemplate, evt => builder => builder.AddMarkupContent(0, $"<em>tpl {evt.Name}</em>")));

        Assert.Equal("tpl Standup", cut.Find("[role='group']:not([aria-label='All day events']) em").TextContent);
    }
}
