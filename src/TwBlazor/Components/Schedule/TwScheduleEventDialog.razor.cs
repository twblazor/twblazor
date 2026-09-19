// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Services;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// The built-in create/edit/view dialog opened by <see cref="TwSchedule{T}"/> when an event is added
/// or clicked. Edits a working copy of <see cref="Schedule{T}"/>'s Name/Start/End directly; any
/// <typeparamref name="T"/>-specific fields are rendered via the optional
/// <see cref="TwSchedule{T}.EventDialogContent"/> template instead, since this dialog has no way to
/// build a generic edit form for an arbitrary <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of <see cref="Schedule{T}.Value"/> for the event being edited.</typeparam>
public partial class TwScheduleEventDialog<T> : TwBlazorComponentBase
{
    private TwScheduleTheme theme => options.Theme.Components.Require<TwScheduleTheme>();

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
    /// Optional extra fields rendered below Start/End, for <typeparamref name="T"/>-specific data -
    /// see <see cref="TwSchedule{T}.EventDialogContent"/>.
    /// </summary>
    [Parameter] public RenderFragment<Schedule<T>>? ContentTemplate { get; set; }

    // Matches TwScheduleTheme.EventChip's default background (bg-purple-600) - shown as the color
    // picker's starting swatch for an event that hasn't set its own Color yet, rather than the color
    // picker's own default of black.
    private const string defaultEventColor = "#9333ea";

    private string fieldsClasses => new ClassBuilder(theme.EventDialogFields).Build();

    private string actionsClasses => new ClassBuilder(theme.EventDialogActions).Build();

    private string readOnlyFieldClasses => new ClassBuilder(theme.EventDialogReadOnlyField).Build();

    private string readOnlyLabelClasses => new ClassBuilder(theme.EventDialogReadOnlyLabel).Build();

    private string readOnlyValueClasses => new ClassBuilder(theme.EventDialogReadOnlyValue).Build();

    private static string FormatDateTime(DateTimeOffset value) => value.DateTime.ToString("d MMM yyyy, h:mm tt");

    private string GetColor() => WorkingEvent.Color ?? defaultEventColor;

    private void SetColor(string value) => WorkingEvent.Color = value;

    private DateTime GetLocalStart() => WorkingEvent.DateTimeStart.DateTime;

    private void SetLocalStart(DateTime value) => WorkingEvent.DateTimeStart = new DateTimeOffset(value, WorkingEvent.DateTimeStart.Offset);

    private DateTime GetLocalEnd() => WorkingEvent.DateTimeEnd.DateTime;

    private void SetLocalEnd(DateTime value) => WorkingEvent.DateTimeEnd = new DateTimeOffset(value, WorkingEvent.DateTimeEnd.Offset);

    private void Save() => dialogInstance?.Close(new TwScheduleEventDialogResult<T>(TwScheduleEventDialogAction.Save, WorkingEvent));

    private void Delete() => dialogInstance?.Close(new TwScheduleEventDialogResult<T>(TwScheduleEventDialogAction.Delete, WorkingEvent));

    private void Cancel() => dialogInstance?.Cancel();
}

/// <summary>
/// Which action a closed <see cref="TwScheduleEventDialog{T}"/> represents.
/// </summary>
public enum TwScheduleEventDialogAction
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
/// The result of a <see cref="TwScheduleEventDialog{T}"/> closing successfully (not canceled).
/// </summary>
/// <typeparam name="T">The type of <see cref="Schedule{T}.Value"/> for the affected event.</typeparam>
public sealed record TwScheduleEventDialogResult<T>(TwScheduleEventDialogAction Action, Schedule<T> Event);
