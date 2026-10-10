// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TwBlazor.Components;

namespace TwBlazor;

/// <summary>
/// Shared focus-management and JS-interop plumbing for text-editable "combobox" pickers that open a
/// popover panel on focus (<see cref="TwDatePicker"/>, <see cref="TwTimePicker"/>): a Tab focus trap and
/// background inert-ing while the panel is open, an outside-click handler that closes it, and restoring
/// focus to the trigger once it does. Each derived picker still owns its own value parsing/formatting and
/// <c>OnAfterRenderAsync</c> override (their native-vs-custom picker detection differs slightly in what
/// happens afterward), but the open/close mechanics themselves are identical, so they live here once
/// instead of being copy-pasted per picker.
/// </summary>
public abstract class TwPopoverPickerComponentBase : TwBlazorTextInputComponentBase, IAsyncDisposable
{
    /// <summary>
    /// Gets or sets the JavaScript runtime instance used for interop operations.
    /// </summary>
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Overrides automatic device detection for whether the browser's native picker should be used
    /// instead of the custom popover. Leave unset (<see langword="null"/>) to auto-detect based on the
    /// client platform (iOS and Android use the native picker by default).
    /// </summary>
    [Parameter] public bool? PreferNativePicker { get; set; }

    /// <summary>
    /// Determines whether the popover panel is currently shown.
    /// </summary>
    protected bool isFocused { get; set; }

    /// <summary>
    /// Indicates whether the browser's native picker UI is being used instead of the custom popover,
    /// either because <see cref="PreferNativePicker"/> was explicitly set or because the client platform
    /// (iOS/Android) was detected via JS interop.
    /// </summary>
    protected bool UseNativePicker;

    /// <summary>
    /// Reference to the TwInputRoot component instance, used to access the root DOM element for JS interop.
    /// </summary>
    protected TwInputRoot? InputRoot;

    /// <summary>
    /// Reference to the popover panel element, used to move focus into it and to trap Tab navigation
    /// while it's open.
    /// </summary>
    protected ElementReference PanelRef;

    /// <summary>
    /// Opaque token (captured via JS interop from the element focused just before the panel opened,
    /// almost always the trigger textfield) used to restore focus there once the panel closes.
    /// </summary>
    protected string? FocusReturnToken;

    /// <summary>
    /// Set when the panel opens so the next <c>OnAfterRenderAsync</c> moves focus into it once
    /// (<see cref="TwColorPicker"/>, <see cref="TwSelect{T}"/>). Pickers whose trigger stays a
    /// text-editable combobox (<see cref="TwDatePicker"/>, <see cref="TwTimePicker"/>, and their
    /// range/date-time variants) don't use this - focus has to stay on the input for typing to work,
    /// so users move into the panel explicitly (Tab, a click, or an arrow key) instead.
    /// </summary>
    /// <remarks>
    /// Not used to gate the Tab focus trap, background inert-ing, or panel positioning - those must
    /// re-run on every render while the panel is open rather than only once, since a stray render
    /// triggered by unrelated state (e.g. the previous close's own trailing re-render still in
    /// flight) could otherwise consume a one-shot flag against a stale <c>PanelRef</c> before the
    /// render that actually mounts the new panel gets a chance to run it.
    /// </remarks>
    protected bool PendingOpenFocus;

    /// <summary>
    /// .NET object reference used for JavaScript interop callbacks.
    /// </summary>
    private DotNetObjectReference<TwPopoverPickerComponentBase>? dotNetRef;

    /// <summary>
    /// Indicates whether the outside click handler has been registered with JavaScript.
    /// </summary>
    private bool registeredOutsideHandler;

    /// <summary>
    /// Set when the panel should take focus on the next render: after it was opened from the keyboard
    /// (Arrow Down on the trigger) or from the trigger's icon button. Opening with a click leaves focus
    /// in the text field so the user can keep typing.
    /// </summary>
    protected bool PendingPanelFocus;

    /// <summary>
    /// The id of the popover panel element, referenced by the trigger's <c>aria-controls</c> while the
    /// panel is open.
    /// </summary>
    protected string panelId => $"{Id}-panel";

    /// <summary>
    /// Names this picker's claim on the page's inert state, so closing it lifts only what it set and
    /// never what an enclosing dialog set (see <c>twDialog.setBackgroundInert</c>).
    /// </summary>
    private string inertOwner => $"picker-{Id}";

    /// <summary>
    /// Extra ARIA attributes forwarded onto the trigger textfield's rendered &lt;input&gt; so assistive
    /// technology knows it opens a popover dialog and whether that dialog is currently open. Omitted when
    /// the native browser picker is in use, since no custom dialog will appear.
    /// </summary>
    protected Dictionary<string, object> triggerAttributes
    {
        get
        {
            if (UseNativePicker)
            {
                return [];
            }

            var attributes = new Dictionary<string, object>
            {
                ["role"] = "combobox",
                ["aria-haspopup"] = "dialog",
                ["aria-expanded"] = isFocused ? "true" : "false"
            };

            if (isFocused)
            {
                attributes["aria-controls"] = panelId;
            }

            return attributes;
        }
    }

    /// <summary>
    /// Releases the Tab focus trap, clears background inert-ing, and unregisters the scroll/resize
    /// listener that kept the panel glued to its trigger while open (registered, along with the
    /// panel's initial position, by each derived picker's own <c>OnAfterRenderAsync</c> via
    /// <c>twPicker.registerScrollReposition</c>; safe to call even for a picker that never
    /// registered one). Must be called (and awaited) while the
    /// panel is still mounted - i.e. before <see cref="isFocused"/> is set to false - since it needs
    /// <see cref="PanelRef"/> to still resolve to a live DOM node.
    /// </summary>
    protected async Task ReleasePanelTrapAsync()
    {
        await JSRuntime.InvokeVoidAsync("twPicker.unregisterScrollReposition", PanelRef);
        await JSRuntime.InvokeVoidAsync("twDialog.releaseFocusTrap", PanelRef);
        await JSRuntime.InvokeVoidAsync("twDialog.clearBackgroundInert", inertOwner);
    }

    /// <summary>
    /// Whether <see cref="ApplyPanelTrapAsync"/> has armed the Tab focus trap and background inert-ing
    /// since <see cref="OnAfterRenderAsync"/> last confirmed it lifted once the panel closed. Not reset by
    /// <see cref="ReleasePanelTrapAsync"/> itself, since a render can re-arm the trap while that is still
    /// awaiting - see <see cref="OnAfterRenderAsync"/>.
    /// </summary>
    private bool panelTrapApplied;

    /// <summary>
    /// Arms the Tab focus trap on <paramref name="panel"/> and makes everything outside it inert.
    /// Every picker calls this from its own <c>OnAfterRenderAsync</c> whenever its panel is open.
    /// </summary>
    protected async Task ApplyPanelTrapAsync(ElementReference panel)
    {
        panelTrapApplied = true;
        await JSRuntime.InvokeVoidAsync("twDialog.trapFocus", panel);
        await JSRuntime.InvokeVoidAsync("twDialog.setBackgroundInert", InputRoot?.RootRef, inertOwner);
    }

    /// <summary>
    /// Lifts the background inert-ing if it is still on after the panel has closed.
    /// </summary>
    /// <remarks>
    /// Picking a value closes the panel through <see cref="ReleasePanelTrapAsync"/>, which awaits
    /// several JS calls. The component re-renders while those are in flight, still with the panel
    /// open, and that render's <c>OnAfterRenderAsync</c> arms the trap again after the release has
    /// already cleared it. Nothing runs after the panel is gone to undo that, so the rest of the page
    /// (inside a dialog, its Save and Cancel buttons) stayed inert and unclickable. The render that
    /// removes the panel always follows, so checking here catches it.
    /// </remarks>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!isFocused && panelTrapApplied)
        {
            panelTrapApplied = false;
            await JSRuntime.InvokeVoidAsync("twDialog.clearBackgroundInert", inertOwner);
        }

        if (isFocused && PendingPanelFocus && PanelRef.Context != null)
        {
            PendingPanelFocus = false;
            await JSRuntime.InvokeVoidAsync("twDialog.focusPanel", PanelRef);
        }
    }

    /// <summary>
    /// Positions <paramref name="panel"/> against <see cref="InputRoot"/> and keeps it correct for
    /// as long as it stays open (see <c>twPicker.registerScrollReposition</c>): on desktop, a page
    /// scroll or window resize closes the panel instead of chasing the trigger around the viewport,
    /// since scrolling away from the field being edited reads as the user moving on; on mobile, the
    /// panel keeps repositioning itself to stay glued to the trigger instead, since there a page
    /// scroll is how touch users reveal more of a panel taller than the viewport. Call once per open
    /// from <c>OnAfterRenderAsync</c>, alongside the Tab focus trap and background inert-ing.
    /// </summary>
    /// <param name="panel">The popover panel element to position and track.</param>
    /// <param name="matchAnchorWidth">
    /// Whether the panel's width should be set to match <see cref="InputRoot"/>'s trigger width in
    /// pixels, rather than keeping its own natural/CSS width - see <see cref="TwSelect{T}"/>.
    /// </param>
    protected async Task RegisterPanelScrollBehaviorAsync(ElementReference panel, bool matchAnchorWidth = false)
    {
        dotNetRef ??= DotNetObjectReference.Create(this);
        await JSRuntime.InvokeVoidAsync("twPicker.registerScrollReposition", InputRoot?.RootRef, panel, matchAnchorWidth, dotNetRef);
    }

    /// <summary>
    /// Reference to the trigger textfield's actual &lt;input&gt; element, supplied by derived pickers
    /// (<see cref="TwDatePicker"/>, <see cref="TwTimePicker"/>) that render a <see cref="TwTextfield{T}"/>
    /// trigger. Used by <see cref="OnIconClickAsync"/> to focus that element directly when the browser's
    /// native picker is in use.
    /// </summary>
    protected virtual ElementReference? triggerInputRef => null;

    /// <summary>
    /// Handles the trigger's icon button: opens the panel and moves focus into it, or closes the panel
    /// when it is already open.
    /// </summary>
    /// <remarks>
    /// With the browser's native picker there is no panel to open, so the button moves focus to the
    /// trigger input instead. It focuses <see cref="triggerInputRef"/> directly when a derived picker
    /// supplies one, rather than scanning the whole <see cref="InputRoot"/> for its first focusable
    /// element, which would find the icon button itself.
    /// </remarks>
    protected async Task OnIconClickAsync()
    {
        if (Disabled)
            return;

        if (UseNativePicker || ReadOnly)
        {
            object surface = triggerInputRef is { } inputRef ? inputRef : InputRoot?.RootRef ?? default;
            await JSRuntime.InvokeVoidAsync("twDialog.focusSurface", surface);
            return;
        }

        if (isFocused)
        {
            await Close();
            return;
        }

        await OpenPanelAsync();
        PendingPanelFocus = isFocused;
    }

    /// <summary>
    /// Handles keydown events on the trigger icon so it's operable from the keyboard (Enter and Space
    /// act as a click), since it's a &lt;div&gt; rather than a native button.
    /// </summary>
    protected async Task OnIconKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key is "Enter" or " ")
        {
            await OnIconClickAsync();
        }
    }

    /// <summary>
    /// Handles a click on the trigger input, opening the popover panel. Focus stays in the input so
    /// the user can still type a value.
    /// </summary>
    protected async Task OnTriggerClickAsync()
    {
        if (!isFocused)
        {
            await OpenPanelAsync();
        }
    }

    /// <summary>
    /// Handles keydown events on the trigger input. Arrow Down opens the panel and moves focus into
    /// it, and Escape closes an open panel.
    /// </summary>
    /// <remarks>
    /// The panel does not open when the input merely receives focus: that would block the rest of the
    /// page for anyone tabbing through a form.
    /// </remarks>
    protected async Task OnTriggerKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            if (isFocused)
            {
                await ClosePanelAsync();
            }

            return;
        }

        if (e.Key != "ArrowDown")
            return;

        if (!isFocused)
        {
            await OpenPanelAsync();
        }

        PendingPanelFocus = isFocused;
    }

    /// <summary>
    /// Opens the popover panel.
    /// </summary>
    /// <remarks>
    /// Sets <see cref="isFocused"/> to true and registers an outside click handler to detect clicks
    /// outside the component. If the component is readonly, disabled, or the native picker is in use,
    /// the custom popover panel will not be shown.
    /// </remarks>
    protected virtual async Task OpenPanelAsync()
    {
        if (ReadOnly || Disabled || UseNativePicker)
            return;

        // Capture whatever currently has focus (the trigger textfield or its icon button) so it can
        // be restored once the panel closes.
        FocusReturnToken = await JSRuntime.InvokeAsync<string?>("twDialog.captureFocus");
        isFocused = true;
        PendingOpenFocus = true;
        await RegisterOutsideClickAsync();
    }

    /// <summary>
    /// Handles keydown events on the popover panel, closing it (and restoring focus to the trigger)
    /// when Escape is pressed.
    /// </summary>
    protected async Task OnPanelKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key != "Escape") return;

        await ClosePanelAsync();
    }

    /// <summary>
    /// Closes the panel after the user committed text typed into the trigger (Enter, or leaving the
    /// field).
    /// </summary>
    /// <remarks>
    /// When the panel was open, focus goes back to the trigger. Tab from the trigger moves into the
    /// panel, which this call is about to remove, so leaving focus where it was would drop it on the
    /// page body. When the panel was already closed, focus has moved on by itself and is left alone.
    /// </remarks>
    protected async Task ClosePanelAfterTextCommitAsync()
    {
        var wasOpen = isFocused;

        if (wasOpen)
        {
            await ReleasePanelTrapAsync();
        }

        isFocused = false;
        PendingPanelFocus = false;

        if (wasOpen)
        {
            await RestoreFocusAsync();
        }
        else
        {
            FocusReturnToken = null;
        }
    }

    /// <summary>
    /// Closes the panel and returns focus to whatever opened it.
    /// </summary>
    private async Task ClosePanelAsync()
    {
        await ReleasePanelTrapAsync();
        isFocused = false;
        PendingPanelFocus = false;
        await UnregisterOutsideClickAsync();
        await RestoreFocusAsync();
    }

    /// <summary>
    /// Restores focus to whatever element was focused (captured via <see cref="FocusReturnToken"/>) right
    /// before the panel opened, typically this component's own trigger textfield. No-ops if no token was
    /// captured (e.g. the panel is being closed a second time).
    /// </summary>
    protected async Task RestoreFocusAsync()
    {
        if (string.IsNullOrEmpty(FocusReturnToken)) return;

        var token = FocusReturnToken;
        FocusReturnToken = null;
        await JSRuntime.InvokeVoidAsync("twDialog.restoreFocus", token);
    }

    /// <summary>
    /// Registers a JavaScript event handler to detect clicks outside the picker component.
    /// </summary>
    /// <remarks>
    /// This method is called when the picker is focused. It ensures the handler is only registered once
    /// by checking the <see cref="registeredOutsideHandler"/> flag.
    /// </remarks>
    protected async Task RegisterOutsideClickAsync()
    {
        if (registeredOutsideHandler) return;
        dotNetRef ??= DotNetObjectReference.Create(this);
        await JSRuntime.InvokeVoidAsync("twPicker.registerOutsideClick", InputRoot?.RootRef, dotNetRef);
        registeredOutsideHandler = true;
    }

    /// <summary>
    /// Unregisters the JavaScript outside click handler and disposes of the .NET object reference.
    /// </summary>
    /// <remarks>
    /// This method should be called when the picker is closed to prevent memory leaks and remove event listeners.
    /// </remarks>
    protected async Task UnregisterOutsideClickAsync()
    {
        if (!registeredOutsideHandler) return;
        await JSRuntime.InvokeVoidAsync("twPicker.unregisterOutsideClick", InputRoot?.RootRef);
        ReleaseOutsideClickHandle();
    }

    /// <summary>
    /// Releases the .NET reference handed to JavaScript and marks the outside click handler as unregistered.
    /// </summary>
    private void ReleaseOutsideClickHandle()
    {
        dotNetRef?.Dispose();
        dotNetRef = null;
        registeredOutsideHandler = false;
    }

    /// <summary>
    /// Closes the picker's popover panel and cleans up JavaScript event handlers.
    /// </summary>
    /// <remarks>
    /// This method is invoked from JavaScript when a click outside the picker is detected.
    /// It sets <see cref="isFocused"/> to false, unregisters the outside click handler, and triggers a UI refresh.
    /// </remarks>
    [JSInvokable("Close")]
    public override async Task Close()
    {
        if (isFocused)
        {
            await ReleasePanelTrapAsync();
        }
        isFocused = false;
        await UnregisterOutsideClickAsync();
        await RestoreFocusAsync();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Disposes of the component's resources asynchronously.
    /// </summary>
    /// <remarks>
    /// This method ensures that JavaScript event handlers are unregistered and the .NET object reference is
    /// disposed, even if the component is removed from the DOM without <see cref="Close"/> being called.
    /// This prevents memory leaks and orphaned event listeners.
    /// </remarks>
    public virtual async ValueTask DisposeAsync()
    {
        try
        {
            await UnregisterOutsideClickAsync();
        }
        catch (JSDisconnectedException)
        {
            // The circuit is already gone, so there is nothing left on the client to unregister;
            // release what is held here instead of letting disposal throw.
            ReleaseOutsideClickHandle();
        }

        GC.SuppressFinalize(this);
    }
}
