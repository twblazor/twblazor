// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Models;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a generic select dropdown component that supports two-way data binding.
/// </summary>
/// <typeparam name="T">The type of values in the dropdown options.</typeparam>
/// <remarks>
/// The TwSelect component provides a flexible dropdown control that can bind to collections of any type.
/// It supports customizable styling, placeholder text, required validation, and property-based display text.
/// The component implements two-way binding through the <see cref="SelectedValue"/> and <see cref="SelectedValueChanged"/> parameters.
/// </remarks>
public partial class TwSelect<T> : TwPopoverPickerComponentBase
{
    [Inject] private InputVariantBuilder inputVariantBuilder { get; set; } = null!;

    private TwInputTheme theme => options.Theme.Components.Require<TwInputTheme>();

    /// <summary>
    /// Gets or sets the collection of values to display in the dropdown.
    /// </summary>
    [Parameter] public IEnumerable<T> Values { get; set; } = [];

    /// <summary>
    /// Gets or sets the currently selected value.
    /// </summary>
    /// <remarks>
    /// Not used when <see cref="Multiple"/> is <see langword="true"/> - bind <see cref="SelectedValues"/> instead.
    /// </remarks>
    [Parameter] public T SelectedValue { get; set; } = default!;

    /// <summary>
    /// Gets or sets the callback that is invoked when the selected value changes.
    /// </summary>
    [Parameter] public EventCallback<T> SelectedValueChanged { get; set; } = default!;

    /// <summary>
    /// Gets or sets whether more than one option can be selected at once.
    /// </summary>
    /// <remarks>
    /// A closed trigger (matching the single-select look) is always shown. On a device that prefers its
    /// own native picker (see <see cref="TwPopoverPickerComponentBase.PreferNativePicker"/>), tapping it
    /// opens a real, invisible <c>&lt;select multiple&gt;</c> layered on top - so the platform's own
    /// multi-select UI (e.g. iOS/Android's full-screen sheet) still handles the interaction. Everywhere
    /// else, it opens a custom checkbox-list popover instead, since a native multi-select can't render as
    /// a closed, single-row trigger the way a single-select can. Bind
    /// <see cref="SelectedValues"/>/<see cref="SelectedValuesChanged"/> rather than
    /// <see cref="SelectedValue"/>/<see cref="SelectedValueChanged"/> when this is <see langword="true"/>.
    /// </remarks>
    [Parameter] public bool Multiple { get; set; }

    /// <summary>
    /// Gets or sets the currently selected values when <see cref="Multiple"/> is <see langword="true"/>.
    /// </summary>
    [Parameter] public IEnumerable<T> SelectedValues { get; set; } = [];

    /// <summary>
    /// Gets or sets the callback that is invoked when the selection changes while <see cref="Multiple"/> is <see langword="true"/>.
    /// </summary>
    [Parameter] public EventCallback<IEnumerable<T>> SelectedValuesChanged { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text displayed when no value is selected.
    /// </summary>
    /// <remarks>
    /// When <see cref="Multiple"/> is <see langword="true"/>, this is shown on the closed trigger only
    /// while nothing is selected - it's replaced by a chip per selected option once at least one is picked.
    /// </remarks>
    [Parameter] public string Placeholder { get; set; } = "Select an option...";

    /// <summary>
    /// Gets or sets whether the select is required (no empty option will be shown).
    /// </summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>
    /// Gets or sets the name of the property to display for complex objects.
    /// </summary>
    /// <remarks>
    /// When Values contains complex objects, specify the property name to display as the option text.
    /// If null or empty, the object's ToString() method will be used.
    /// </remarks>
    [Parameter] public string PropertyName { get; set; } = string.Empty;

    private Dictionary<int, T> parsedValues { get; set; } = [];

    private int selectedValueId;

    private HashSet<int> selectedValueIds { get; set; } = [];

    /// <summary>
    /// Gets the shared box classes (background, border, padding, disabled/readonly treatment) behind
    /// both <see cref="classes"/> and <see cref="triggerClasses"/>, parameterized on which "focus" variant
    /// shows the border - the two differ only in that.
    /// </summary>
    private string GetBoxClasses(string focusVariant) => new ClassBuilder(theme.SelectBase)
        .AddClass(inputSizeClasses, !Multiple)
        .AddClass(inputVariantBuilder.GetClasses(effectiveVariant, theme).Replace("focus:", focusVariant, StringComparison.Ordinal))
        .AddClass(theme.SelectDefaultPadding, effectiveVariant == InputVariant.Default)
        // Default/Outlined variants make the field's own background bg-transparent so it blends
        // with the surrounding page - fine for <input>, but a native <select> popup renders using
        // the element's own background/text colors, so a transparent one falls back to the OS's
        // native (often light) popup surface and can pair unreadable white dark-mode text onto it.
        // The Filled variant already sets a real background, so it's left alone here.
        .AddClass(theme.SelectNativeBackground, effectiveVariant != InputVariant.Filled)
        .AddClass(Disabled ? $"{options.Theme.Interaction.DisabledOpacity} {options.Theme.Interaction.DisabledCursor}" : string.Empty)
        .AddClass(ReadOnly ? theme.SelectReadOnlyBackground : string.Empty)
        .AddClass(ReadOnly && !Disabled ? options.Theme.Interaction.PointerEventsNone : string.Empty)
        .Build();

    /// <summary>
    /// Gets the CSS classes applied to the select element.
    /// </summary>
    private string classes => new ClassBuilder(GetBoxClasses("focus-visible:"))
        // A native <select> focuses on mouse click same as keyboard, so theme.FocusBorder's plain
        // "focus:" variant (shared with text inputs, where showing the border on click is fine)
        // is rewritten to "focus-visible:" here so the border only appears on keyboard focus.
        .AddClass(Class)
        .Build();

    /// <summary>
    /// Gets the classes for <see cref="Multiple"/>'s decorative closed trigger (the div shown behind the
    /// invisible native overlay select, or holding the custom-popover's chips/open button) - the same box
    /// look as <see cref="classes"/>, plus the flex-wrap layout needed for a row of chips.
    /// </summary>
    /// <remarks>
    /// The box itself is never the focusable/interactive element here (its content is - see
    /// <see cref="TwInputTheme.SelectMultiOpenButton"/> and each chip's own close button), so its
    /// focus-ring border uses <c>focus-within:</c> rather than <c>focus:</c>/<c>focus-visible:</c> - it
    /// shows whenever a descendant has focus, the same way a real &lt;select&gt;'s border shows when it
    /// itself is focused.
    /// </remarks>
    private string triggerClasses => new ClassBuilder(GetBoxClasses("focus-within:"))
        .AddClass(theme.SelectMultiTriggerLayout)
        .AddClass(Class)
        .Build();

    /// <summary>
    /// Gets the classes for the real, invisible <c>&lt;select multiple&gt;</c> layered over the decorative
    /// trigger when <see cref="TwPopoverPickerComponentBase.UseNativePicker"/> is <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// ReadOnly can't use the native <c>readonly</c> attribute (not valid on &lt;select&gt;, same as the
    /// single-select and custom-popover triggers), so it's blocked from changing the value the same way
    /// those are - by disabling pointer interaction here, on top of <see cref="HandleChange"/> also
    /// ignoring any change event that does get through (e.g. via keyboard).
    /// </remarks>
    private string nativeOverlayClasses => new ClassBuilder(theme.SelectNativeMultiOverlay)
        .AddClass(ReadOnly && !Disabled ? options.Theme.Interaction.PointerEventsNone : string.Empty)
        .Build();

    /// <summary>
    /// Gets the positioning classes for the custom checkbox-list popover panel's outer wrapper.
    /// </summary>
    private string panelPositionClasses => new ClassBuilder(theme.SelectPanelPosition).Build();

    /// <summary>
    /// Gets the surface (background/border/rounded/shadow) classes for the custom checkbox-list popover
    /// panel, plus its own padding/scroll behavior and the label text color override documented on
    /// <see cref="TwInputTheme.SelectPanelItemText"/>.
    /// </summary>
    private string panelSurfaceClasses => new ClassBuilder(popoverBuilder.GetSurfaceClasses(Rounded, Shadow, theme.SelectPanelSurface))
        .AddClass(theme.SelectPanelItemText)
        .Build();

    /// <summary>
    /// Gets the currently selected option ids in <see cref="Values"/> order, for rendering one chip per
    /// selected option on <see cref="Multiple"/>'s closed trigger.
    /// </summary>
    private IEnumerable<int> orderedSelectedValueIds => selectedValueIds.OrderBy(id => id);

    /// <summary>
    /// Gets the items shown in <see cref="Multiple"/>'s custom checkbox-list popover, rebuilt from
    /// <see cref="parsedValues"/> each render - cheap for the option counts a select realistically has,
    /// and <see cref="TwCheckboxGroup{TValue}"/> resyncs each item's checked state from
    /// <see cref="SelectedValues"/> every render regardless of whether this list instance changed.
    /// </summary>
    private List<CheckboxGroupItem<T>> checkboxItems =>
        [.. parsedValues.Select(kv => new CheckboxGroupItem<T> { Label = GetDisplayText(kv.Value), Value = kv.Value })];

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (string.IsNullOrEmpty(RootId))
        {
            RootId = Guid.NewGuid().ToString("N");
        }

        PopulateValues();

        // Only Disabled maps to the native disabled attribute. ReadOnly is not a valid attribute
        // for select elements in HTML, so it must stay focusable/announced via aria-readonly - see
        // the aria-readonly attribute on the <select> markup and the ReadOnly guard in HandleChange,
        // which together keep the control in the tab order while blocking value changes.
        if (Disabled && !Attributes.ContainsKey("disabled"))
        {
            Attributes["disabled"] = true;
        }
        else if (!Disabled)
        {
            Attributes.Remove("disabled");
        }

        // A <select> with no "name" is omitted from form submissions; default to Id (already unique
        // per instance) unless the consumer supplied their own, same as TwTextfield.
        if (!Attributes.ContainsKey("name") && !string.IsNullOrEmpty(Id))
        {
            Attributes["name"] = Id;
        }
    }

    /// <summary>
    /// Determines, via <see cref="TwPopoverPickerComponentBase.PreferNativePicker"/> or JS-based device
    /// detection, whether <see cref="Multiple"/>'s closed trigger should hand taps/clicks to a real
    /// invisible native <c>&lt;select multiple&gt;</c> instead of opening the custom checkbox popover.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender && Multiple)
        {
            UseNativePicker = PreferNativePicker ?? await DeviceDetector.PrefersNativePickerAsync(JSRuntime);
            if (UseNativePicker)
            {
                StateHasChanged();
            }
        }

        if (isFocused && PanelRef.Context != null)
        {
            // Re-run on every render rather than gating behind a one-shot "just opened" flag - see
            // the matching remarks on TwDatePicker.OnAfterRenderAsync for why these three specifically
            // need to. matchAnchorWidth: true - the options panel should span exactly the trigger's
            // width, not its own natural content width (see the remarks on TwInputTheme.SelectPanelPosition).
            await RegisterPanelScrollBehaviorAsync(PanelRef, matchAnchorWidth: true);
            await JSRuntime.InvokeVoidAsync("twDialog.trapFocus", PanelRef);
            await JSRuntime.InvokeVoidAsync("twDialog.setBackgroundInert", InputRoot?.RootRef);

            // Unlike the trap/inert/positioning above, moving focus into the panel must stay a
            // one-shot action gated on PendingOpenFocus - repeating it on every render would yank
            // focus back to the first option whenever anything else re-renders this component while
            // the panel is open (e.g. while the user is navigating options with the keyboard).
            if (PendingOpenFocus)
            {
                PendingOpenFocus = false;
                await JSRuntime.InvokeVoidAsync("twDialog.focusSurface", PanelRef);
            }
        }
    }

    private void PopulateValues()
    {
        parsedValues = [];
        var valueId = 1;

        var selectedSet = Multiple ? new HashSet<T>(SelectedValues, EqualityComparer<T>.Default) : null;
        var newSelectedIds = Multiple ? new HashSet<int>() : null;

        foreach (var value in Values)
        {
            parsedValues.Add(valueId, value);

            if (Multiple)
            {
                if (selectedSet!.Contains(value))
                {
                    newSelectedIds!.Add(valueId);
                }
            }
            else if (EqualityComparer<T>.Default.Equals(SelectedValue, value))
            {
                selectedValueId = valueId;
            }

            valueId++;
        }

        if (Multiple)
        {
            selectedValueIds = newSelectedIds!;
        }
    }

    private string GetDisplayText(T value)
    {
        // Only a genuinely absent (null) value should render as empty - default(T) is a
        // legitimate, selectable value for non-nullable value types (e.g. 0 for int).
        if (value is null)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(PropertyName))
        {
            var property = value.GetType().GetProperty(PropertyName);
            if (property != null)
            {
                var propertyValue = property.GetValue(value);
                return propertyValue?.ToString() ?? string.Empty;
            }
        }

        return value.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Opens or closes the custom checkbox-list popover from a click on <see cref="Multiple"/>'s
    /// decorative trigger button. Unlike the text-editable combobox pickers (<see cref="TwDatePicker"/>/
    /// <see cref="TwTimePicker"/>), this is a plain button rather than a focusable trigger textfield, so
    /// it drives the shared <c>isFocused</c>/<c>PendingOpenFocus</c> state directly - the same approach
    /// <see cref="TwColorPicker"/> uses for its swatch button - and toggles closed again on a second click.
    /// </summary>
    private async Task ToggleOrOpenPanelAsync()
    {
        if (Disabled || ReadOnly)
            return;

        if (isFocused)
        {
            await Close();
            return;
        }

        FocusReturnToken = await JSRuntime.InvokeAsync<string?>("twDialog.captureFocus");
        isFocused = true;
        PendingOpenFocus = true;
        await RegisterOutsideClickAsync();
    }

    /// <summary>
    /// Removes one selected option from <see cref="Multiple"/>'s selection via its chip's close button on
    /// the custom-popover trigger. Not offered on the native-overlay trigger (see
    /// <see cref="TwPopoverPickerComponentBase.UseNativePicker"/>) - its chips sit behind the real,
    /// invisible <c>&lt;select multiple&gt;</c>, which always receives the tap/click instead.
    /// </summary>
    private async Task RemoveSelectionAsync(int id)
    {
        if (Disabled || ReadOnly || !selectedValueIds.Contains(id))
            return;

        var newIds = new HashSet<int>(selectedValueIds);
        newIds.Remove(id);

        selectedValueIds = newIds;
        SelectedValues = [.. orderedSelectedValueIds.Select(x => parsedValues[x])];

        if (SelectedValuesChanged.HasDelegate)
        {
            await SelectedValuesChanged.InvokeAsync(SelectedValues);
        }
    }

    /// <summary>
    /// Handles a selection change from the custom checkbox-list popover's <see cref="TwCheckboxGroup{TValue}"/>.
    /// </summary>
    private async Task HandleCheckboxGroupChanged(IEnumerable<T> newValues)
    {
        var newValuesSet = new HashSet<T>(newValues, EqualityComparer<T>.Default);
        var newIds = new HashSet<int>();

        foreach (var (key, value) in parsedValues)
        {
            if (newValuesSet.Contains(value))
            {
                newIds.Add(key);
            }
        }

        selectedValueIds = newIds;
        SelectedValues = newValues;

        if (SelectedValuesChanged.HasDelegate)
        {
            await SelectedValuesChanged.InvokeAsync(SelectedValues);
        }
    }

    private async Task HandleChange(ChangeEventArgs e)
    {
        if (ReadOnly || Disabled)
            return;

        if (Multiple)
        {
            await HandleMultipleChangeAsync(e);
        }
        else
        {
            await HandleSingleChangeAsync(e);
        }
    }

    /// <summary>
    /// Handles a change event from the native, invisible <c>&lt;select multiple&gt;</c> overlay.
    /// </summary>
    private async Task HandleMultipleChangeAsync(ChangeEventArgs e)
    {
        // ChangeEventArgs.Value for a native <select multiple> is the array of selected option
        // values (see the @onchange "Multiple option selection" binding support in Blazor docs),
        // not a single scalar - so it's read as a string[] here rather than parsed as one int.
        if (e.Value is not string[] selectedIdStrings)
            return;

        var newIds = new HashSet<int>();
        var newValues = new List<T>();

        foreach (var idString in selectedIdStrings)
        {
            if (int.TryParse(idString, out var id) && parsedValues.TryGetValue(id, out var value))
            {
                newIds.Add(id);
                newValues.Add(value);
            }
        }

        selectedValueIds = newIds;
        SelectedValues = newValues;

        if (SelectedValuesChanged.HasDelegate)
        {
            await SelectedValuesChanged.InvokeAsync(SelectedValues);
        }
    }

    /// <summary>
    /// Handles a change event from the single-select <c>&lt;select&gt;</c> element.
    /// </summary>
    private async Task HandleSingleChangeAsync(ChangeEventArgs e)
    {
        if (!int.TryParse(e.Value?.ToString(), out var newValueId))
            return;

        selectedValueId = newValueId;
        if (!parsedValues.TryGetValue(selectedValueId, out var selectedItem))
            return;

        SelectedValue = selectedItem;
        if (SelectedValueChanged.HasDelegate)
        {
            await SelectedValueChanged.InvokeAsync(SelectedValue);
        }
    }
}
