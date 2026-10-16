// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Calendar;

/// <summary>
/// Covers the optional notification callbacks <see cref="TwCalendar{T}"/> raises (created, updated,
/// deleted, search) and the corners of its drag-and-drop handling.
/// </summary>
public class TwCalendarCallbackTests : TwBlazorTestBase
{
    private static readonly DateTime _day = new(2026, 3, 18);

    private static Schedule<string> Event(string name, DateTime start, DateTime end) => new()
    {
        Name = name,
        DateTimeStart = start,
        DateTimeEnd = end
    };

    private static async Task<AngleSharp.Dom.IElement> DialogButtonAsync(IRenderedComponent<TwDialogProvider> provider, string text) =>
        (await provider.WaitForElementAsync("[role='dialog']")).QuerySelectorAll("button").First(b => b.TextContent.Trim() == text);

    [Fact]
    public void SearchIcon_InvokesOnSearch()
    {
        var searched = false;
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.OnSearch, EventCallback.Factory.Create(this, () => searched = true)));

        cut.Find("[aria-label='Search events']").Click();

        Assert.True(searched);
    }

    [Fact]
    public async Task SavingANewEvent_RaisesOnEventCreated()
    {
        Schedule<string>? created = null;
        var provider = TestContext.Render<TwDialogProvider>();
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, _day)
            .Add(x => x.View, TwCalendarView.Day)
            .Add(x => x.Editable, true)
            .Add(x => x.OnEventCreated, EventCallback.Factory.Create<Schedule<string>>(this, e => created = e)));

        cut.Find("button[aria-label='09:00']").Click();
        (await DialogButtonAsync(provider, "Save")).Click();

        await provider.WaitForAssertionAsync(() => Assert.NotNull(created));
        Assert.Equal(new DateTimeOffset(_day.AddHours(9)), created!.DateTimeStart);
        Assert.Equal(new DateTimeOffset(_day.AddHours(9).AddMinutes(30)), created.DateTimeEnd);
    }

    [Fact]
    public async Task SavingAnEditedEvent_RaisesOnEventUpdated()
    {
        var evt = Event("Design review", _day.AddHours(10), _day.AddHours(11));
        Schedule<string>? updated = null;
        var provider = TestContext.Render<TwDialogProvider>();
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, _day)
            .Add(x => x.View, TwCalendarView.Day)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [evt])
            .Add(x => x.OnEventUpdated, EventCallback.Factory.Create<Schedule<string>>(this, e => updated = e)));

        cut.Find("button[aria-label^='Design review']").Click();
        (await DialogButtonAsync(provider, "Save")).Click();

        await provider.WaitForAssertionAsync(() => Assert.NotNull(updated));
        Assert.Equal("Design review", updated!.Name);
    }

    [Fact]
    public async Task DeletingAnEvent_RemovesItAndRaisesOnEventDeleted()
    {
        var evt = Event("Design review", _day.AddHours(10), _day.AddHours(11));
        var schedules = new List<Schedule<string>> { evt };
        Schedule<string>? deleted = null;
        var provider = TestContext.Render<TwDialogProvider>();
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, _day)
            .Add(x => x.View, TwCalendarView.Day)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, schedules)
            .Add(x => x.OnEventDeleted, EventCallback.Factory.Create<Schedule<string>>(this, e => deleted = e)));

        cut.Find("button[aria-label^='Design review']").Click();
        (await DialogButtonAsync(provider, "Delete")).Click();

        await provider.WaitForAssertionAsync(() => Assert.Same(evt, deleted));
        Assert.Empty(schedules);
    }

    [Fact]
    public async Task DroppingAnEventOnItsOwnSlot_ChangesNothing()
    {
        var evt = Event("Design review", _day.AddHours(13), _day.AddHours(14));
        List<Schedule<string>>? changed = null;
        Schedule<string>? updated = null;
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, _day)
            .Add(x => x.View, TwCalendarView.Day)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [evt])
            .Add(x => x.SchedulesChanged, EventCallback.Factory.Create<List<Schedule<string>>>(this, l => changed = l))
            .Add(x => x.OnEventUpdated, EventCallback.Factory.Create<Schedule<string>>(this, e => updated = e)));

        cut.Find("button[aria-label^='Design review']").DragStart(new DragEventArgs());
        await cut.WaitForAssertionAsync(() => Assert.Contains("pointer-events-none", cut.Find("button[aria-label^='Design review']").GetAttribute("class")));

        // The drag start re-renders the column twice, a moment apart, and each render replaces the slot's drop
        // handler. Finding and dropping on the renderer's own thread stops a render landing between the two.
        await cut.InvokeAsync(() => cut.Find("button[aria-label='13:00']").Drop(new DragEventArgs()));

        await cut.WaitForAssertionAsync(() => Assert.DoesNotContain("pointer-events-none", cut.Find("button[aria-label^='Design review']").GetAttribute("class")));
        Assert.Null(changed);
        Assert.Null(updated);
        Assert.Equal(new DateTimeOffset(_day.AddHours(13)), evt.DateTimeStart);
    }

    [Fact]
    public async Task DroppingAnEventOnAnotherDayInMonthView_KeepsItsTimeOfDay_AndRaisesOnEventUpdated()
    {
        var evt = Event("Design review", _day.AddHours(13), _day.AddHours(14));
        Schedule<string>? updated = null;
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, _day)
            .Add(x => x.View, TwCalendarView.Month)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [evt])
            .Add(x => x.OnEventUpdated, EventCallback.Factory.Create<Schedule<string>>(this, e => updated = e)));

        cut.Find("button[aria-label^='Design review']").DragStart(new DragEventArgs());
        cut.Find("button[aria-label='Friday, 20 March 2026']").ParentElement!.Drop(new DragEventArgs());

        await cut.WaitForAssertionAsync(() => Assert.NotNull(updated));
        Assert.Equal(new DateTimeOffset(_day.AddDays(2).AddHours(13)), updated!.DateTimeStart);
        Assert.Equal(new DateTimeOffset(_day.AddDays(2).AddHours(14)), updated.DateTimeEnd);
    }

    [Fact]
    public async Task ClickingAnEvent_WhenNotEditable_OpensAReadOnlyDialog()
    {
        var evt = Event("Design review", _day.AddHours(10), _day.AddHours(11));
        var provider = TestContext.Render<TwDialogProvider>();
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, _day)
            .Add(x => x.View, TwCalendarView.Day)
            .Add(x => x.Schedules, [evt]));

        cut.Find("button[aria-label^='Design review']").Click();

        var dialog = await provider.WaitForElementAsync("[role='dialog']");
        Assert.DoesNotContain(dialog.QuerySelectorAll("button"), b => b.TextContent.Trim() == "Save");
    }

    [Fact]
    public void AllDayRow_DropPreviewOutsideItsDays_ShowsNoPlaceholder()
    {
        var banner = Event("Holiday", _day, _day.AddDays(1));
        var cut = TestContext.Render<TwCalendarAllDayRow<string>>(p => p
            .Add(x => x.Days, [_day])
            .Add(x => x.Schedules, [banner])
            .Add(x => x.Editable, true)
            .Add(x => x.DraggedEvent, banner)
            .Add(x => x.DropPreview, _day.AddDays(5)));

        Assert.Empty(cut.FindAll("div[aria-hidden='true'][style*='grid-column:1 / span']"));
    }

    [Fact]
    public void AllDayRow_DropPreviewInsideItsDays_ShowsAPlaceholderSpanningTheEvent()
    {
        var banner = Event("Trip", _day, _day.AddDays(2));
        var cut = TestContext.Render<TwCalendarAllDayRow<string>>(p => p
            .Add(x => x.Days, [_day, _day.AddDays(1), _day.AddDays(2)])
            .Add(x => x.Schedules, [banner])
            .Add(x => x.Editable, true)
            .Add(x => x.DraggedEvent, banner)
            .Add(x => x.DropPreview, _day.AddDays(1)));

        Assert.Single(cut.FindAll("div[aria-hidden='true'][style='grid-column:2 / span 2;grid-row:1;']"));
    }
}
