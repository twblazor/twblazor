// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Services;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// The built-in create/edit/view dialog opened by <see cref="TwCalendar{T}"/> when an event is added
/// or clicked. Edits a working copy of <see cref="Schedule{T}"/>'s Name/Start/End directly; any
/// <typeparamref name="T"/>-specific fields are rendered via the optional
/// <see cref="TwCalendar{T}.EventDialogContent"/> template instead, since this dialog has no way to
/// build a generic edit form for an arbitrary <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of <see cref="Schedule{T}.Value"/> for the event being edited.</typeparam>
public partial class TwCalendarEventDialog<T> : TwBlazorComponentBase
{
    private TwCalendarTheme theme => options.Theme.Components.Require<TwCalendarTheme>();

    [CascadingParameter] private TwDialogInstance? dialogInstance { get; set; }

    /// <summary>
    /// The event being created or edited. A working copy - mutated in place by this dialog's fields
    /// and (for reference-type <typeparamref name="T"/>) by <see cref="ContentTemplate"/> - and only
    /// handed back to the caller on <see cref="Save"/>.
    /// </summary>
    [Parameter, EditorRequired] public Schedule<T> WorkingEvent { get; set; } = null!;

    /// <summary>
    /// Whether <see cref="WorkingEvent"/> is a brand-new event (hides the Delete button) as opposed
    /// to an existing one being edited.
    /// </summary>
    [Parameter] public bool IsNew { get; set; }

    /// <summary>
    /// When <see langword="true"/>, every field renders read-only and only a "Close" button is shown.
    /// </summary>
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>
    /// The properties of <see cref="Schedule{T}.Value"/> to show (read-only mode) or edit, resolved from
    /// <see cref="TwCalendar{T}.DialogProperties"/>. They render after the built-in fields and before
    /// <see cref="ContentTemplate"/>.
    /// </summary>
    [Parameter] public IReadOnlyList<TwCalendarDialogField> Fields { get; set; } = [];

    /// <summary>
    /// Optional extra fields rendered below Start/End, for <typeparamref name="T"/>-specific data -
    /// see <see cref="TwCalendar{T}.EventDialogContent"/>.
    /// </summary>
    [Parameter] public RenderFragment<Schedule<T>>? ContentTemplate { get; set; }

    // Matches TwCalendarTheme.EventChip's default background (bg-purple-600) - shown as the color
    // picker's starting swatch for an event that hasn't set its own Color yet, rather than the color
    // picker's own default of black.
    private const string defaultEventColor = "#9333ea";

    private string fieldsClasses => new ClassBuilder(theme.EventDialogFields).Build();

    private string actionsClasses => new ClassBuilder(theme.EventDialogActions).Build();

    private string readOnlyFieldClasses => new ClassBuilder(theme.EventDialogReadOnlyField).Build();

    private string readOnlyLabelClasses => new ClassBuilder(theme.EventDialogReadOnlyLabel).Build();

    private string readOnlyValueClasses => new ClassBuilder(theme.EventDialogReadOnlyValue).Build();

    private string summaryClasses => new ClassBuilder(theme.EventDialogSummary).Build();

    private string summaryIconClasses => new ClassBuilder(theme.EventDialogSummaryIcon).Build();

    private string accentClasses => new ClassBuilder(theme.EventDialogAccent).Build();

    private string dateRowClasses => new ClassBuilder(theme.EventDialogDateRow).Build();

    private string accentCardStyle => TwCalendarColors.GetAccentCardStyle(WorkingEvent.Color);

    private string accentIconStyle => TwCalendarColors.GetSolidTextStyle(WorkingEvent.Color);

    private string accentBarStyle => TwCalendarColors.GetSolidBackgroundStyle(GetColor());

    /// <summary>
    /// Formats the event's time span on one line: a shared date followed by the time range when it
    /// starts and ends on the same day (<c>1 Oct 2026 10:00 - 11:00</c>), or both full dates when it
    /// spans days (<c>1 Oct 2026 22:00 - 2 Oct 2026 01:00</c>).
    /// </summary>
    internal static string FormatRange(DateTimeOffset start, DateTimeOffset end)
    {
        var from = start.DateTime;
        var to = end.DateTime;

        return from.Date == to.Date
            ? $"{from:d MMM yyyy} {from:HH:mm} - {to:HH:mm}"
            : $"{from:d MMM yyyy HH:mm} - {to:d MMM yyyy HH:mm}";
    }

    /// <summary>
    /// Applies <paramref name="mutation"/> to <see cref="Schedule{T}.Value"/> through a box, then writes
    /// it back, so edits also land when <typeparamref name="T"/> is a struct (reflection would
    /// otherwise change a throwaway copy).
    /// </summary>
    private void Mutate(Action<object> mutation)
    {
        if (WorkingEvent.Value is not { } value)
        {
            return;
        }

        object boxed = value;
        mutation(boxed);
        WorkingEvent.Value = (T)boxed;
    }

    private string GetColor() => WorkingEvent.Color ?? defaultEventColor;

    private void SetColor(string value) => WorkingEvent.Color = value;

    private DateTime GetLocalStart() => WorkingEvent.DateTimeStart.DateTime;

    private void SetLocalStart(DateTime value) => WorkingEvent.DateTimeStart = new DateTimeOffset(value, WorkingEvent.DateTimeStart.Offset);

    private DateTime GetLocalEnd() => WorkingEvent.DateTimeEnd.DateTime;

    private void SetLocalEnd(DateTime value) => WorkingEvent.DateTimeEnd = new DateTimeOffset(value, WorkingEvent.DateTimeEnd.Offset);

    private void Save() => dialogInstance?.Close(new TwCalendarEventDialogResult<T>(TwCalendarEventDialogAction.Save, WorkingEvent));

    private void Delete() => dialogInstance?.Close(new TwCalendarEventDialogResult<T>(TwCalendarEventDialogAction.Delete, WorkingEvent));

    private void Cancel() => dialogInstance?.Cancel();
}

/// <summary>
/// Which action a closed <see cref="TwCalendarEventDialog{T}"/> represents.
/// </summary>
public enum TwCalendarEventDialogAction
{
    /// <summary>
    /// The event was created or its edits should be applied.
    /// </summary>
    Save,

    /// <summary>
    /// The event should be removed.
    /// </summary>
    Delete
}

/// <summary>
/// The result of a <see cref="TwCalendarEventDialog{T}"/> closing successfully (not canceled).
/// </summary>
/// <typeparam name="T">The type of <see cref="Schedule{T}.Value"/> for the affected event.</typeparam>
public sealed record TwCalendarEventDialogResult<T>(TwCalendarEventDialogAction Action, Schedule<T> Event);
