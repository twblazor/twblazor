// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Renders one day's 30-minute time-slot grid and its absolutely-positioned events. Shared by
/// <see cref="TwScheduleDayView{T}"/> (a single instance) and <see cref="TwScheduleWeekView{T}"/>
/// (seven instances side by side) - it renders no time labels or date header of its own, since those
/// only need to appear once per view, not once per day column.
/// </summary>
/// <typeparam name="T">The type of <see cref="Schedule{T}.Value"/> for the events shown.</typeparam>
/// <remarks>
/// Event positioning is expressed in <c>rem</c> rather than percentages: each of the 48 half-hour
/// rows is exactly 3rem tall (matching the <c>h-12</c> baked into <see cref="TwScheduleTheme.SlotRow"/>),
/// so an event's offset from midnight converts directly to a <c>top</c>/<c>height</c> in the same unit
/// without needing the container's total height to be sized explicitly. Overriding
/// <see cref="TwScheduleTheme.SlotRow"/> with a different row height would need the constants below
/// updated to match.
/// </remarks>
public partial class TwScheduleDayColumn<T> : TwBlazorComponentBase, IAsyncDisposable
{
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;

    private TwScheduleTheme theme => options.Theme.Components.Require<TwScheduleTheme>();

    private const int slotsPerDay = 48;
    private const decimal slotHeightRem = 3m;
    private const int minutesPerSlot = 24 * 60 / slotsPerDay;

    /// <summary>
    /// The day this column represents (only the date component is used).
    /// </summary>
    [Parameter, EditorRequired] public DateTime Date { get; set; }

    /// <summary>
    /// The events to render for this day. The caller is expected to have already filtered
    /// <see cref="TwSchedule{T}.Schedules"/> down to events starting on <see cref="Date"/> - events
    /// spanning multiple days aren't specially clipped/split across day columns in this first pass.
    /// </summary>
    [Parameter] public IReadOnlyList<Schedule<T>> Events { get; set; } = [];

    /// <summary>
    /// When <see langword="true"/>, empty slots are clickable (invoking <see cref="OnSlotClick"/>) to
    /// start creating a new event at that time.
    /// </summary>
    [Parameter] public bool Editable { get; set; }

    /// <summary>
    /// Optional custom rendering for an event's chip content; defaults to just <see cref="Schedule{T}.Name"/>.
    /// </summary>
    [Parameter] public RenderFragment<Schedule<T>>? EventContentTemplate { get; set; }

    /// <summary>
    /// Invoked with the clicked slot's start date/time when an empty slot is activated while
    /// <see cref="Editable"/>.
    /// </summary>
    [Parameter] public EventCallback<DateTime> OnSlotClick { get; set; }

    /// <summary>
    /// Invoked with the event when one of its chips is activated.
    /// </summary>
    [Parameter] public EventCallback<Schedule<T>> OnEventClick { get; set; }

    /// <summary>
    /// Invoked with the 0-based slot index whenever a slot in this column gains focus (by click, Tab,
    /// or arrow-key movement). <see cref="TwScheduleWeekView{T}"/> tracks this across its seven
    /// columns so an ArrowLeft/ArrowRight press knows which row to continue at in the neighboring
    /// column.
    /// </summary>
    [Parameter] public EventCallback<int> OnSlotFocused { get; set; }

    /// <summary>
    /// Invoked with an event when dragging it starts (native HTML5 drag-and-drop). Only meaningful
    /// when <see cref="Editable"/> and the event isn't <see cref="Schedule{T}.ReadOnly"/> - see
    /// <see cref="TwSchedule{T}"/>'s <c>draggedEvent</c> field, which tracks this across day columns.
    /// </summary>
    [Parameter] public EventCallback<Schedule<T>> OnEventDragStart { get; set; }

    /// <summary>
    /// Invoked with a slot's start date/time when a dragged event is dropped there.
    /// </summary>
    [Parameter] public EventCallback<DateTime> OnEventDrop { get; set; }

    /// <summary>
    /// Invoked with the start of the slot the dragged event is currently hovering, so the owner can
    /// track where it would land (see <see cref="DropPreview"/>).
    /// </summary>
    [Parameter] public EventCallback<DateTime> OnEventDragOver { get; set; }

    /// <summary>
    /// Invoked when a drag that started on one of this column's chips ends, whether or not it was
    /// dropped on a valid target.
    /// </summary>
    [Parameter] public EventCallback OnEventDragEnd { get; set; }

    /// <summary>
    /// The event currently being dragged, once the drag is live; <see langword="null"/> otherwise.
    /// While set, chips ignore pointer events so the slots beneath them (for example the slot of an
    /// event already sitting at the target time) can receive the drag and drop.
    /// </summary>
    [Parameter] public Schedule<T>? DraggedEvent { get; set; }

    /// <summary>
    /// Where <see cref="DraggedEvent"/> would land if released now. The placeholder only renders when
    /// this falls on <see cref="Date"/>.
    /// </summary>
    [Parameter] public DateTime? DropPreview { get; set; }

    private string columnClasses => new ClassBuilder(options.Theme.Position.Relative)
        .AddClass(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Col)
        .AddClass(Class).Build();

    private IReadOnlyList<TwScheduleEventLayout.Slot<T>> layoutSlots => TwScheduleEventLayout.LayoutEvents(Events);

    /// <summary>
    /// The offset in minutes from midnight, clamped to the visible day - a multi-day event's portion
    /// outside this day simply doesn't render (see the remarks on <see cref="Events"/>).
    /// </summary>
    private double GetStartMinutes(Schedule<T> evt)
    {
        var minutes = (evt.DateTimeStart.DateTime.Date == Date.Date)
            ? evt.DateTimeStart.DateTime.TimeOfDay.TotalMinutes
            : 0;
        return Math.Clamp(minutes, 0, 24 * 60);
    }

    private double GetEndMinutes(Schedule<T> evt)
    {
        var minutes = (evt.DateTimeEnd.DateTime.Date == Date.Date)
            ? evt.DateTimeEnd.DateTime.TimeOfDay.TotalMinutes
            : 24 * 60;
        return Math.Clamp(minutes, 0, 24 * 60);
    }

    private string GetChipStyle(TwScheduleEventLayout.Slot<T> slot)
    {
        var top = (decimal)(GetStartMinutes(slot.Event) / minutesPerSlot) * slotHeightRem;
        var height = (decimal)((GetEndMinutes(slot.Event) - GetStartMinutes(slot.Event)) / minutesPerSlot) * slotHeightRem;
        var widthPercent = 100m / slot.ColumnCount;
        var leftPercent = widthPercent * slot.Column;
        var color = TwScheduleColors.GetEventCardStyle(slot.Event.Color);

        return $"top:{top.ToString(System.Globalization.CultureInfo.InvariantCulture)}rem;height:{Math.Max(height, 0.5m).ToString(System.Globalization.CultureInfo.InvariantCulture)}rem;left:{leftPercent.ToString(System.Globalization.CultureInfo.InvariantCulture)}%;width:{widthPercent.ToString(System.Globalization.CultureInfo.InvariantCulture)}%;{color}";
    }

    private string GetChipClasses(Schedule<T> evt) => new ClassBuilder(theme.EventChip)
        .AddClass(theme.EventChipReadOnly, evt.ReadOnly || !Editable)
        .AddClass(theme.EventChipDraggable, IsDraggable(evt))
        .AddClass(options.Theme.Interaction.PointerEventsNone, DraggedEvent is not null)
        .Build();

    private string placeholderClasses => new ClassBuilder(theme.DropPlaceholder)
        .AddClass(options.Theme.Position.Absolute)
        .AddClass(options.Theme.Interaction.PointerEventsNone)
        .AddClass(options.Theme.Spacing.Padding.Sm)
        .Build();

    /// <summary>
    /// The inline style positioning the landing placeholder, or <see langword="null"/> when there is
    /// nothing to show: no live drag, no hover target, or a target on a different day. Sized to the
    /// dragged event's duration, clipped to the end of the day.
    /// </summary>
    private string? placeholderStyle
    {
        get
        {
            if (!Editable || DraggedEvent is null || DropPreview is not { } preview || preview.Date != Date.Date)
            {
                return null;
            }

            var startMinutes = preview.TimeOfDay.TotalMinutes;
            var durationMinutes = Math.Min((DraggedEvent.DateTimeEnd - DraggedEvent.DateTimeStart).TotalMinutes, 24 * 60 - startMinutes);
            var top = TwScheduleTimeGrid.GetOffsetRem(startMinutes);
            var height = Math.Max((decimal)(durationMinutes / minutesPerSlot) * slotHeightRem, 0.5m);

            return $"top:{top.ToString(System.Globalization.CultureInfo.InvariantCulture)}rem;height:{height.ToString(System.Globalization.CultureInfo.InvariantCulture)}rem;left:0;width:100%;";
        }
    }

    private string chipTitleClasses => new ClassBuilder(theme.EventChipTitle).Build();

    private string chipTimeClasses => new ClassBuilder(theme.EventChipTime).Build();

    /// <summary>
    /// The event's start time (e.g. "9:00 AM"), shown as the card's subtitle line below its name.
    /// </summary>
    private static string GetStartTimeLabel(Schedule<T> evt) => evt.DateTimeStart.DateTime.ToString("h:mm tt");

    private bool IsDraggable(Schedule<T> evt) => Editable && !evt.ReadOnly;

    private Task OnChipDragStartAsync(Schedule<T> evt) =>
        IsDraggable(evt) ? OnEventDragStart.InvokeAsync(evt) : Task.CompletedTask;

    private Task OnChipDragEndAsync() => OnEventDragEnd.InvokeAsync();

    private Task OnSlotDragEnterAsync(int slotIndex) =>
        Editable ? OnEventDragOver.InvokeAsync(Date.Date.AddMinutes(slotIndex * minutesPerSlot)) : Task.CompletedTask;

    private Task OnSlotDropAsync(int slotIndex) =>
        Editable ? OnEventDrop.InvokeAsync(Date.Date.AddMinutes(slotIndex * minutesPerSlot)) : Task.CompletedTask;

    private string GetSlotClasses() => new ClassBuilder(theme.SlotRow)
        .AddClass(options.Theme.Sizing.FullWidth)
        .AddClass(options.Theme.Flexbox.ShrinkNone)
        .AddClass(theme.SlotRowHover, Editable)
        .AddClass(options.Theme.Interaction.PointerCursor, Editable)
        .AddClass(options.Theme.Interaction.DisabledCursor, !Editable)
        .Build();

    private Task OnSlotClickedAsync(int slotIndex) =>
        Editable ? OnSlotClick.InvokeAsync(Date.Date.AddMinutes(slotIndex * minutesPerSlot)) : Task.CompletedTask;

    private Task OnSlotFocusedAsync(int slotIndex)
    {
        focusedSlot = slotIndex;
        return OnSlotFocused.HasDelegate ? OnSlotFocused.InvokeAsync(slotIndex) : Task.CompletedTask;
    }

    private Task OnEventClickedAsync(Schedule<T> evt) => OnEventClick.InvokeAsync(evt);

    /// <summary>
    /// Reference to the day column's grid element, used to register the shared keydown guard (see
    /// <see cref="TwBlazor.Components.DatePicker.TwDatePickerDayView"/>'s identical use of it) that
    /// suppresses the browser's default scroll behavior for ArrowUp/ArrowDown/Home/End.
    /// </summary>
    private ElementReference gridRef;

    private bool keydownGuardRegistered;

    private sealed class SlotRef
    {
        public ElementReference Element;
    }

    private readonly Dictionary<int, SlotRef> _slotRefs = [];

    /// <summary>
    /// The 0-based half-hour slot index (0 = midnight, 47 = 23:30) currently holding this column's
    /// roving tabindex.
    /// </summary>
    private int focusedSlot;

    private SlotRef GetSlotRef(int slot)
    {
        if (!_slotRefs.TryGetValue(slot, out var slotRef))
        {
            slotRef = new SlotRef();
            _slotRefs[slot] = slotRef;
        }

        return slotRef;
    }

    private DateTime trackedDate = DateTime.MinValue;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Only reset the roving tabindex when the displayed day actually changes (including on
        // first render) - not on every render, which would otherwise fight OnGridKeyDownAsync's own
        // updates to focusedSlot as the user arrows around within the same day.
        if (Date.Date != trackedDate)
        {
            trackedDate = Date.Date;
            focusedSlot = 0;
            _slotRefs.Clear();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("twTabs.registerKeydownGuard", gridRef);
                keydownGuardRegistered = true;
            }
            catch (JSDisconnectedException)
            {
                // The circuit disconnected before the script could run; nothing to register.
            }
        }
    }

    /// <summary>
    /// Moves the roving tabindex vertically within this day's 48 slots: ArrowDown/ArrowUp by one
    /// half-hour, Home/End to midnight/23:30. ArrowLeft/ArrowRight are deliberately left unhandled
    /// (and left to bubble) so a parent composing several columns side by side - see
    /// <see cref="TwScheduleWeekView{T}"/> - can move focus to the adjacent day at the same time
    /// instead.
    /// </summary>
    private async Task OnGridKeyDownAsync(KeyboardEventArgs e)
    {
        int? target = e.Key switch
        {
            "ArrowDown" => focusedSlot + 1,
            "ArrowUp" => focusedSlot - 1,
            "Home" => 0,
            "End" => slotsPerDay - 1,
            _ => null
        };

        if (target is null)
        {
            return;
        }

        var clamped = Math.Clamp(target.Value, 0, slotsPerDay - 1);
        if (clamped == focusedSlot)
        {
            return;
        }

        focusedSlot = clamped;
        StateHasChanged();

        if (_slotRefs.TryGetValue(focusedSlot, out var slotRef))
        {
            await slotRef.Element.FocusAsync();
        }
    }

    /// <summary>
    /// Moves the roving tabindex to the given slot and focuses it - used by
    /// <see cref="TwScheduleWeekView{T}"/> to continue a horizontal ArrowLeft/ArrowRight move into
    /// this column at the same time row the user was on.
    /// </summary>
    internal async Task FocusSlotAsync(int slotIndex)
    {
        var clamped = Math.Clamp(slotIndex, 0, slotsPerDay - 1);
        focusedSlot = clamped;
        StateHasChanged();

        if (_slotRefs.TryGetValue(focusedSlot, out var slotRef))
        {
            await slotRef.Element.FocusAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!keydownGuardRegistered)
        {
            GC.SuppressFinalize(this);
            return;
        }

        try
        {
            await JSRuntime.InvokeVoidAsync("twTabs.unregisterKeydownGuard", gridRef);
        }
        catch (JSDisconnectedException)
        {
            // The circuit is already gone; nothing left to clean up.
        }
        catch (InvalidOperationException)
        {
            // JS interop unavailable during teardown (e.g. prerendering); safe to ignore.
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }
}
