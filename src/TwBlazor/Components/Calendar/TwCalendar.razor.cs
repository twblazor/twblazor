// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Models;
using TwBlazor.Services;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// A Day/Week/Month schedule/agenda component. Composes <see cref="TwCalendarDayView{T}"/>,
/// <see cref="TwCalendarWeekView{T}"/>, and <see cref="TwCalendarMonthView{T}"/> for its three views,
/// and owns opening the built-in <see cref="TwCalendarEventDialog{T}"/> for creating/editing/viewing
/// events.
/// </summary>
/// <typeparam name="T">The type of <see cref="Schedule{T}.Value"/> for the events shown.</typeparam>
public partial class TwCalendar<T> : TwBlazorComponentBase
{
    [Inject] private ITwDialogService dialogService { get; set; } = null!;

    private TwCalendarTheme theme => options.Theme.Components.Require<TwCalendarTheme>();

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
    [Parameter] public TwCalendarView View { get; set; } = TwCalendarView.Week;

    /// <summary>
    /// The bound <see cref="View"/> value; invoked whenever the view switcher, or a day click that
    /// drills into the Day view, changes it.
    /// </summary>
    [Parameter] public EventCallback<TwCalendarView> ViewChanged { get; set; }

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
    /// When <see langword="true"/>, the Day view shows a mini-calendar beside its time grid for
    /// quickly jumping to another day. Defaults to <see langword="false"/>. Not used by the Week or
    /// Month views.
    /// </summary>
    [Parameter] public bool ShowDayCalendar { get; set; }

    /// <summary>
    /// When set, shows a search icon button in the header that invokes this callback. No search
    /// logic lives in <see cref="TwCalendar{T}"/> itself - the search UI (and navigating to a found
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
    /// The properties of <see cref="Schedule{T}.Value"/> the event dialog shows, as property name to
    /// label. Each one appears as a labeled value in the read-only dialog and as an editor in the
    /// create/edit dialog, matched to the property's type: text, number, switch (<see cref="bool"/>),
    /// date and time picker, or select (enum). Properties without a public setter, or of any other
    /// type, show as read-only text. Names that don't match a public property are ignored. An empty
    /// label falls back to the property name split into words. Entries render in dictionary order.
    /// </summary>
    /// <remarks>
    /// Resolved with reflection on <typeparamref name="T"/>. New events need a public parameterless
    /// constructor on <typeparamref name="T"/> so the dialog has something to edit. The dialog works
    /// on a shallow copy of <see cref="Schedule{T}.Value"/>, so cancelling never changes the original.
    /// </remarks>
    [Parameter] public IReadOnlyDictionary<string, string>? DialogProperties { get; set; }

    /// <summary>
    /// Optional extra fields rendered inside the built-in create/edit dialog, after Name/Start/End/Color
    /// and any <see cref="DialogProperties"/> - for data the dialog can't generate an editor for on its
    /// own. The template's <c>context</c> is the dialog's working <see cref="Schedule{T}"/> copy, whose
    /// <c>Value</c> is a shallow copy of the original, so mutating it in place is safe until Save.
    /// </summary>
    [Parameter] public RenderFragment<Schedule<T>>? EventDialogContent { get; set; }

    private string containerClasses => new ClassBuilder(theme.Container).AddClass(Class).Build();

    private string headerClasses => new ClassBuilder(theme.Header).Build();

    private string headerDateGroupClasses => new ClassBuilder(theme.HeaderDateGroup).Build();

    private string headerTitleRowClasses => new ClassBuilder(theme.HeaderTitleRow).Build();

    private string headerTitleClasses => new ClassBuilder(theme.HeaderTitle).Build();

    private string headerSubtitleClasses => new ClassBuilder(theme.HeaderSubtitle).Build();

    private string GetNavButtonClasses() => new ClassBuilder(theme.NavButton).Build();

    private string previousTooltip => $"Previous {viewUnit}";

    private string nextTooltip => $"Next {viewUnit}";

    private string viewUnit => View switch
    {
        TwCalendarView.Day => "day",
        TwCalendarView.Month => "month",
        _ => "week"
    };

    private string viewSwitcherClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Align.Center)
        .AddClass(options.Theme.Spacing.Gap.Sm)
        .Build();

    private string GetViewButtonClasses(TwCalendarView view) => new ClassBuilder(theme.NavButton)
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

    private async Task SetViewAsync(TwCalendarView view)
    {
        View = view;
        if (ViewChanged.HasDelegate)
        {
            await ViewChanged.InvokeAsync(view);
        }
    }

    private Task PreviousAsync() => SetSelectedDateAsync(View switch
    {
        TwCalendarView.Day => SelectedDate.AddDays(-1),
        TwCalendarView.Week => SelectedDate.AddDays(-7),
        TwCalendarView.Month => SelectedDate.AddMonths(-1),
        _ => SelectedDate
    });

    private Task NextAsync() => SetSelectedDateAsync(View switch
    {
        TwCalendarView.Day => SelectedDate.AddDays(1),
        TwCalendarView.Week => SelectedDate.AddDays(7),
        TwCalendarView.Month => SelectedDate.AddMonths(1),
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
        await SetViewAsync(TwCalendarView.Day);
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
            Value = CreateValue(),
            DateTimeStart = start,
            DateTimeEnd = start.AddMinutes(30)
        };

        return OpenEventDialogAsync(workingEvent, workingEvent, isNew: true);
    }

    private Task OnEventClickedAsync(Schedule<T> evt) => OpenEventDialogAsync(evt, CloneEvent(evt), isNew: false);

    /// <summary>
    /// A blank <typeparamref name="T"/> for a new event, only when <see cref="DialogProperties"/> asks
    /// for fields to edit and <typeparamref name="T"/> has a public parameterless constructor.
    /// </summary>
    private T? CreateValue() =>
        DialogProperties is { Count: > 0 } && typeof(T) != typeof(string) && (typeof(T).IsValueType || typeof(T).GetConstructor(Type.EmptyTypes) is not null)
            ? Activator.CreateInstance<T>()
            : default;

    /// <summary>
    /// A shallow copy of <paramref name="value"/> for the dialog to edit, so Cancel leaves the real
    /// entry untouched. Value types and strings are already copies.
    /// </summary>
    private static T? CloneValue(T? value)
    {
        if (value is null || typeof(T).IsValueType || value is string)
        {
            return value;
        }

        var memberwiseClone = typeof(object).GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        return (T?)memberwiseClone?.Invoke(value, null) ?? value;
    }

    private static Schedule<T> CloneEvent(Schedule<T> source) => new()
    {
        Name = source.Name,
        Value = CloneValue(source.Value),
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
        var title = isNew ? "New event" : (readOnly ? (workingEvent.Name ?? "Event") : "Edit event");

        var parameters = new TwDialogParameters
        {
            ["WorkingEvent"] = workingEvent,
            ["IsNew"] = isNew,
            ["ReadOnly"] = readOnly,
            ["Fields"] = TwCalendarDialogField.Resolve(typeof(T), DialogProperties),
            ["ContentTemplate"] = EventDialogContent
        };

        var options = new TwDialogOptions { MaxWidth = DialogMaxWidth.Medium, FullWidth = true };

        var reference = await dialogService.ShowAsync<TwCalendarEventDialog<T>>(title, parameters, options);
        var result = await reference.GetReturnValueAsync<TwCalendarEventDialogResult<T>>();

        if (result is null)
        {
            return;
        }

        if (result.Action == TwCalendarEventDialogAction.Delete)
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
    /// cross from one <see cref="TwCalendarDayColumn{T}"/> into a sibling one in Week view.
    /// </summary>
    private Schedule<T>? draggedEvent;

    /// <summary>
    /// Whether the drag has been live long enough for the views to switch into drag mode (chips stop
    /// intercepting pointer events so slots underneath can receive the drop, and the landing
    /// placeholder is drawn).
    /// </summary>
    private bool isDragActive;

    /// <summary>
    /// Where the dragged event would land if released now: the hovered slot's start in Day/Week view,
    /// or the hovered day in Month view. <see langword="null"/> until the pointer enters a drop target.
    /// </summary>
    private DateTime? dropPreview;

    private Schedule<T>? activeDraggedEvent => isDragActive ? draggedEvent : null;

    private static readonly TimeSpan _dragActivationDelay = TimeSpan.FromMilliseconds(25);

    /// <summary>
    /// Records the dragged event, then flips into drag mode after a short delay. Re-rendering the
    /// chips synchronously inside <c>dragstart</c> makes Chrome cancel the drag, so the visual change
    /// is deferred until the browser has committed to it.
    /// </summary>
    private async Task OnEventDragStartAsync(Schedule<T> evt)
    {
        draggedEvent = evt;
        dropPreview = null;

        await Task.Delay(_dragActivationDelay);

        if (draggedEvent == evt)
        {
            isDragActive = true;
        }
    }

    private void OnEventDragOver(DateTime target)
    {
        if (draggedEvent is not null)
        {
            dropPreview = target;
        }
    }

    private void OnEventDragEnd() => ClearDragState();

    private void ClearDragState()
    {
        draggedEvent = null;
        isDragActive = false;
        dropPreview = null;
    }

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
        ClearDragState();

        if (evt is null || !Editable || evt.ReadOnly)
        {
            return;
        }

        var newStart = View == TwCalendarView.Month || TwCalendarSpans.IsBanner(evt)
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
/// The three views <see cref="TwCalendar{T}"/> can display.
/// </summary>
public enum TwCalendarView
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
/// A single entry rendered by <see cref="TwCalendar{T}"/>.
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
    /// <see cref="TwCalendar{T}.Editable"/> is <see langword="true"/> globally.
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>
    /// The hex color (e.g. <c>"#7C3AED"</c>) used for this entry's chip background across all three
    /// views, edited via a <see cref="TwColorPicker"/> in the built-in event dialog. <see langword="null"/>
    /// (the default) falls back to <see cref="TwBlazor.Configuration.Components.TwCalendarTheme.EventChip"/>'s
    /// own background.
    /// </summary>
    public string? Color { get; set; }
}
