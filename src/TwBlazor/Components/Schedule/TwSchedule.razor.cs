// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Models;
using TwBlazor.Services;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// A Day/Week/Month schedule/agenda component. Composes <see cref="TwScheduleDayView{T}"/>,
/// <see cref="TwScheduleWeekView{T}"/>, and <see cref="TwScheduleMonthView{T}"/> for its three views,
/// and owns opening the built-in <see cref="TwScheduleEventDialog{T}"/> for creating/editing/viewing
/// events.
/// </summary>
/// <typeparam name="T">The type of <see cref="Schedule{T}.Value"/> for the events shown.</typeparam>
public partial class TwSchedule<T> : TwBlazorComponentBase
{
    [Inject] private ITwDialogService dialogService { get; set; } = null!;

    private TwScheduleTheme theme => options.Theme.Components.Require<TwScheduleTheme>();

    /// <summary>
    /// The date driving the main view: the day shown in Day view, or the week/month containing it in
    /// Week/Month view.
    /// </summary>
    [Parameter] public DateTime SelectedDate { get; set; } = DateTime.Today;

    /// <summary>
    /// The bound <see cref="SelectedDate"/> value; invoked whenever navigation (Previous/Today/Next,
    /// a day click in Week/Month view, or the Day view's mini-calendar) moves it.
    /// </summary>
    [Parameter] public EventCallback<DateTime> SelectedDateChanged { get; set; }

    /// <summary>
    /// The currently displayed view.
    /// </summary>
    [Parameter] public TwScheduleView View { get; set; } = TwScheduleView.Week;

    /// <summary>
    /// The bound <see cref="View"/> value; invoked whenever the view switcher, or a day click that
    /// drills into the Day view, changes it.
    /// </summary>
    [Parameter] public EventCallback<TwScheduleView> ViewChanged { get; set; }

    /// <summary>
    /// The schedule entries shown across all three views.
    /// </summary>
    [Parameter] public List<Schedule<T>> Schedules { get; set; } = [];

    /// <summary>
    /// The bound <see cref="Schedules"/> value; invoked whenever the built-in event dialog creates,
    /// edits, or deletes an entry.
    /// </summary>
    [Parameter] public EventCallback<List<Schedule<T>>> SchedulesChanged { get; set; }

    /// <summary>
    /// Invoked with the new entry after the built-in dialog creates one.
    /// </summary>
    [Parameter] public EventCallback<Schedule<T>> OnEventCreated { get; set; }

    /// <summary>
    /// Invoked with the updated entry after the built-in dialog saves edits to an existing one.
    /// </summary>
    [Parameter] public EventCallback<Schedule<T>> OnEventUpdated { get; set; }

    /// <summary>
    /// Invoked with the removed entry after the built-in dialog deletes it.
    /// </summary>
    [Parameter] public EventCallback<Schedule<T>> OnEventDeleted { get; set; }

    /// <summary>
    /// Global gate for editing: shows the header's "+" add button, opens the event dialog in edit
    /// mode (rather than read-only) for events that don't set their own <see cref="Schedule{T}.ReadOnly"/>,
    /// and lets events be dragged to reschedule them (see the Day/Week/Month view remarks on exactly
    /// what dragging does in each).
    /// </summary>
    [Parameter] public bool Editable { get; set; }

    /// <summary>
    /// The CSS <c>max-height</c> (e.g. <c>"40rem"</c>, <c>"600px"</c>) of the Day/Week views'
    /// scrollable time grid. Not used by the Month view, which never scrolls.
    /// </summary>
    [Parameter] public string MaxHeight { get; set; } = "40rem";

    /// <summary>
    /// The time of day the Day/Week views' time grid is scrolled to the first time it mounts (e.g.
    /// <c>TimeSpan.FromHours(8)</c> for 8am, the default). Only applied once per mount - navigating
    /// between dates while staying on the same view preserves whatever the user has since scrolled to,
    /// rather than resetting it on every date change. Not used by the Month view, which never scrolls.
    /// </summary>
    [Parameter] public TimeSpan ScrollToTime { get; set; } = TimeSpan.FromHours(8);

    /// <summary>
    /// When set, shows a search icon button in the header that invokes this callback. No search
    /// logic lives in <see cref="TwSchedule{T}"/> itself - the search UI (and navigating to a found
    /// event via <see cref="SelectedDate"/>/<see cref="View"/>) is left entirely to the caller, since
    /// what "search" means depends on the caller's own data and <typeparamref name="T"/>.
    /// </summary>
    [Parameter] public EventCallback OnSearch { get; set; }

    /// <summary>
    /// Optional custom rendering for an event's chip/block content across all three views; defaults
    /// to just <see cref="Schedule{T}.Name"/>.
    /// </summary>
    [Parameter] public RenderFragment<Schedule<T>>? EventContentTemplate { get; set; }

    /// <summary>
    /// Optional extra fields rendered inside the built-in create/edit dialog, below Name/Start/End -
    /// for <typeparamref name="T"/>-specific data the dialog can't generate a form for on its own.
    /// The template's <c>context</c> is the dialog's working <see cref="Schedule{T}"/> copy; mutate
    /// <c>context.Value</c> in place for reference-type <typeparamref name="T"/>.
    /// </summary>
    [Parameter] public RenderFragment<Schedule<T>>? EventDialogContent { get; set; }

    private string containerClasses => new ClassBuilder(theme.Container).AddClass(Class).Build();

    private string headerClasses => new ClassBuilder(theme.Header).Build();

    private string headerDateGroupClasses => new ClassBuilder(theme.HeaderDateGroup).Build();

    private string headerTitleRowClasses => new ClassBuilder(theme.HeaderTitleRow).Build();

    private string headerTitleClasses => new ClassBuilder(theme.HeaderTitle).Build();

    private string headerSubtitleClasses => new ClassBuilder(theme.HeaderSubtitle).Build();

    private string GetNavButtonClasses() => new ClassBuilder(theme.NavButton).Build();

    private string viewSwitcherClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Align.Center)
        .AddClass(options.Theme.Spacing.Gap.Sm)
        .Build();

    private string GetViewButtonClasses(TwScheduleView view) => new ClassBuilder(theme.NavButton)
        .AddClass(theme.ViewButtonActive, View == view)
        .Build();

    private string headerControlsClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Align.Center)
        .AddClass(options.Theme.Flexbox.Wrap)
        .AddClass(options.Theme.Spacing.Gap.Md)
        .Build();

    /// <summary>
    /// The header's title: the full month name and year (e.g. "September 2026"). Shown the same way
    /// across all three views - see <see cref="weekRangeLabel"/>'s remarks for why the week it names
    /// is always meaningful regardless of which view is active.
    /// </summary>
    private string monthYearLabel => SelectedDate.ToString("MMMM yyyy");

    /// <summary>
    /// The label for the header's week-number <see cref="TwChip"/> (e.g. "Week 3").
    /// </summary>
    private string weekChipLabel => $"Week {DateHelpers.GetWeekOfMonth(SelectedDate)}";

    /// <summary>
    /// The header's subtitle: the Monday-Sunday date range containing <see cref="SelectedDate"/> (e.g.
    /// "14 Sep 2026 - 20 Sep 2026"). Shown regardless of the active view - even in Day/Month view,
    /// <see cref="SelectedDate"/> still belongs to exactly one week, so the range (and the week-number
    /// chip beside the title) stays meaningful.
    /// </summary>
    private string weekRangeLabel
    {
        get
        {
            var start = DateHelpers.GetStartOfWeek(SelectedDate);
            var end = start.AddDays(6);
            return $"{start:d MMM yyyy} - {end:d MMM yyyy}";
        }
    }

    private async Task SetSelectedDateAsync(DateTime date)
    {
        SelectedDate = date;
        if (SelectedDateChanged.HasDelegate)
        {
            await SelectedDateChanged.InvokeAsync(date);
        }
    }

    private async Task SetViewAsync(TwScheduleView view)
    {
        View = view;
        if (ViewChanged.HasDelegate)
        {
            await ViewChanged.InvokeAsync(view);
        }
    }

    private Task PreviousAsync() => SetSelectedDateAsync(View switch
    {
        TwScheduleView.Day => SelectedDate.AddDays(-1),
        TwScheduleView.Week => SelectedDate.AddDays(-7),
        TwScheduleView.Month => SelectedDate.AddMonths(-1),
        _ => SelectedDate
    });

    private Task NextAsync() => SetSelectedDateAsync(View switch
    {
        TwScheduleView.Day => SelectedDate.AddDays(1),
        TwScheduleView.Week => SelectedDate.AddDays(7),
        TwScheduleView.Month => SelectedDate.AddMonths(1),
        _ => SelectedDate
    });

    private Task TodayAsync() => SetSelectedDateAsync(DateTime.Today);

    private Task OnSearchClickedAsync() => OnSearch.HasDelegate ? OnSearch.InvokeAsync() : Task.CompletedTask;

    /// <summary>
    /// Switches to the Day view for the given date - wired up to Week view's day headers and Month
    /// view's day numbers/"+N more" overflow labels.
    /// </summary>
    private async Task SwitchToDayAsync(DateTime day)
    {
        await SetSelectedDateAsync(day);
        await SetViewAsync(TwScheduleView.Day);
    }

    private Task StartCreateEventAsync(DateTime start)
    {
        if (!Editable)
        {
            return Task.CompletedTask;
        }

        var workingEvent = new Schedule<T>
        {
            Name = string.Empty,
            DateTimeStart = start,
            DateTimeEnd = start.AddMinutes(30)
        };

        return OpenEventDialogAsync(workingEvent, workingEvent, isNew: true);
    }

    private Task OnEventClickedAsync(Schedule<T> evt) => OpenEventDialogAsync(evt, CloneEvent(evt), isNew: false);

    private static Schedule<T> CloneEvent(Schedule<T> source) => new()
    {
        Name = source.Name,
        Value = source.Value,
        DateTimeStart = source.DateTimeStart,
        DateTimeEnd = source.DateTimeEnd,
        ReadOnly = source.ReadOnly,
        Color = source.Color
    };

    /// <summary>
    /// Opens the built-in event dialog and applies whatever the user did (save, delete, or cancel).
    /// </summary>
    /// <param name="original">The real entry in <see cref="Schedules"/> being edited, or the same
    /// instance as <paramref name="workingEvent"/> when creating a new one (there's nothing in
    /// <see cref="Schedules"/> yet to distinguish it from).</param>
    /// <param name="workingEvent">The dialog's working copy - see <see cref="CloneEvent"/>.</param>
    /// <param name="isNew">Whether this is a brand-new event rather than an edit of an existing one.</param>
    private async Task OpenEventDialogAsync(Schedule<T> original, Schedule<T> workingEvent, bool isNew)
    {
        var readOnly = !isNew && (original.ReadOnly || !Editable);
        // Read-only opens with no default TwDialog title bar - TwScheduleEventDialog draws its own
        // color/name header instead (see TwScheduleEventDialogHeader) - so the title passed here only
        // matters as TwDialog's accessible-name fallback (see TwDialog.effectiveAriaLabel) for that case.
        var title = isNew ? "New event" : (readOnly ? (workingEvent.Name ?? "Event") : "Edit event");

        var parameters = new TwDialogParameters
        {
            ["WorkingEvent"] = workingEvent,
            ["IsNew"] = isNew,
            ["ReadOnly"] = readOnly,
            ["ContentTemplate"] = EventDialogContent
        };

        var options = new TwDialogOptions { NoHeader = readOnly };

        var reference = await dialogService.ShowAsync<TwScheduleEventDialog<T>>(title, parameters, options);
        var result = await reference.GetReturnValueAsync<TwScheduleEventDialogResult<T>>();

        if (result is null)
        {
            return;
        }

        if (result.Action == TwScheduleEventDialogAction.Delete)
        {
            await RemoveEventAsync(original);
        }
        else if (isNew)
        {
            await AddEventAsync(result.Event);
        }
        else
        {
            await ReplaceEventAsync(original, result.Event);
        }
    }

    private async Task AddEventAsync(Schedule<T> evt)
    {
        Schedules.Add(evt);
        await RaiseSchedulesChangedAsync();

        if (OnEventCreated.HasDelegate)
        {
            await OnEventCreated.InvokeAsync(evt);
        }
    }

    private async Task ReplaceEventAsync(Schedule<T> original, Schedule<T> updated)
    {
        var index = Schedules.IndexOf(original);
        if (index >= 0)
        {
            Schedules[index] = updated;
        }

        await RaiseSchedulesChangedAsync();

        if (OnEventUpdated.HasDelegate)
        {
            await OnEventUpdated.InvokeAsync(updated);
        }
    }

    private async Task RemoveEventAsync(Schedule<T> evt)
    {
        Schedules.Remove(evt);
        await RaiseSchedulesChangedAsync();

        if (OnEventDeleted.HasDelegate)
        {
            await OnEventDeleted.InvokeAsync(evt);
        }
    }

    private Task RaiseSchedulesChangedAsync() =>
        SchedulesChanged.HasDelegate ? SchedulesChanged.InvokeAsync(Schedules) : Task.CompletedTask;

    /// <summary>
    /// The event currently being dragged (native HTML5 drag-and-drop), captured from whichever view's
    /// event chip raised <c>OnEventDragStart</c> - see the Day/Week/Month view components' own
    /// <c>OnEventDragStart</c> parameters. Tracked here (rather than in each view) because a drag can
    /// cross from one <see cref="TwScheduleDayColumn{T}"/> into a sibling one in Week view.
    /// </summary>
    private Schedule<T>? draggedEvent;

    private void OnEventDragStart(Schedule<T> evt) => draggedEvent = evt;

    /// <summary>
    /// Applies a completed drag: Day/Week views report <paramref name="dropTarget"/> as the exact new
    /// start (day and time, snapped to a 30-minute slot); the Month view reports just a date (its
    /// cells have no time-of-day of their own), so here that's interpreted as "keep the event's
    /// existing time-of-day, move it to this date" - matching the Month view's own drag semantics.
    /// Either way, the event's original duration is preserved.
    /// </summary>
    private async Task OnEventDropAsync(DateTime dropTarget)
    {
        var evt = draggedEvent;
        draggedEvent = null;

        if (evt is null || !Editable || evt.ReadOnly)
        {
            return;
        }

        var newStart = View == TwScheduleView.Month
            ? dropTarget.Date + evt.DateTimeStart.TimeOfDay
            : dropTarget;

        var delta = newStart - evt.DateTimeStart.DateTime;
        if (delta == TimeSpan.Zero)
        {
            return;
        }

        var updated = CloneEvent(evt);
        updated.DateTimeStart = evt.DateTimeStart + delta;
        updated.DateTimeEnd = evt.DateTimeEnd + delta;

        await ReplaceEventAsync(evt, updated);
    }
}

/// <summary>
/// The three views <see cref="TwSchedule{T}"/> can display.
/// </summary>
public enum TwScheduleView
{
    /// <summary>
    /// A single day's 24-hour time-slot grid, with a mini-calendar for quickly jumping to another day.
    /// </summary>
    Day,

    /// <summary>
    /// A Monday-Sunday week of time-slot grids side by side.
    /// </summary>
    Week,

    /// <summary>
    /// A full month grid (5 or 6 weeks, whichever the month spans).
    /// </summary>
    Month
}

/// <summary>
/// A single entry rendered by <see cref="TwSchedule{T}"/>.
/// </summary>
/// <typeparam name="T">The type of <see cref="Value"/> associated with the entry.</typeparam>
public class Schedule<T>
{
    /// <summary>
    /// The display name of the entry.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The value associated with the entry.
    /// </summary>
    public T? Value { get; set; }

    /// <summary>
    /// The start date and time of the entry.
    /// </summary>
    public DateTimeOffset DateTimeStart { get; set; }

    /// <summary>
    /// The end date and time of the entry.
    /// </summary>
    public DateTimeOffset DateTimeEnd { get; set; }

    /// <summary>
    /// When <see langword="true"/>, this entry always opens read-only in the event dialog, even when
    /// <see cref="TwSchedule{T}.Editable"/> is <see langword="true"/> globally.
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>
    /// The hex color (e.g. <c>"#7C3AED"</c>) used for this entry's chip background across all three
    /// views, edited via a <see cref="TwColorPicker"/> in the built-in event dialog. <see langword="null"/>
    /// (the default) falls back to <see cref="TwBlazor.Configuration.Components.TwScheduleTheme.EventChip"/>'s
    /// own background.
    /// </summary>
    public string? Color { get; set; }
}
