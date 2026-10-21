// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Models;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// A button that opens a floating panel of rich, interactive content next to it, such as a short form, a set of
/// filters or details about an item.
/// </summary>
/// <remarks>
/// <para>
/// The panel is a non-modal dialog positioned against the button. It opens on the side set by
/// <see cref="Placement"/>, flips to the other side when there is no room, and stays inside the viewport as the page
/// scrolls. Use <see cref="TriggerContent"/> for a trigger of your own.
/// </para>
/// <para>
/// Accessibility: the button says that it opens a dialog and whether it is open. Opening moves focus into the panel,
/// and Escape closes it and returns focus to the button. Tabbing out of the panel or clicking outside it closes it
/// and leaves focus where it went. The panel is named by its <see cref="Title"/>, or else by the button.
/// </para>
/// </remarks>
public partial class TwPopover : TwBlazorComponentBase, IAsyncDisposable
{
    private bool isOpen;
    private bool pendingOpen;
    private bool registered;
    private ElementReference rootRef;
    private ElementReference triggerRef;
    private ElementReference panelRef;
    private DotNetObjectReference<TwPopover>? selfReference;

    [Inject] private IJSRuntime jsRuntime { get; set; } = null!;

    private TwPopoverTheme theme => options.Theme.Components.Require<TwOverlayTheme>().Popover;

    /// <summary>
    /// Gets or sets the panel's content.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets a trigger of your own in place of the built-in button. Splat the context's
    /// <see cref="PopoverTriggerContext.Attributes"/> onto it and call <see cref="PopoverTriggerContext.ToggleAsync"/>
    /// from its click handler.
    /// </summary>
    [Parameter] public RenderFragment<PopoverTriggerContext>? TriggerContent { get; set; }

    /// <summary>
    /// Gets or sets the text of the button that opens the panel.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// Gets or sets an icon for the button: before the <see cref="Label"/>, or on its own with <see cref="IconOnly"/>.
    /// </summary>
    [Parameter] public Icon? Icon { get; set; }

    /// <summary>
    /// Gets or sets whether the button shows only its <see cref="Icon"/>. Give it an
    /// <see cref="TwBlazorComponentBase.AriaLabel"/>, which is also shown as a tooltip.
    /// </summary>
    [Parameter] public bool IconOnly { get; set; }

    /// <summary>
    /// Gets or sets the look of the button. Defaults to the button theme's own default.
    /// </summary>
    [Parameter] public ButtonVariant? Variant { get; set; }

    /// <summary>
    /// Gets or sets whether the button uses its compact size.
    /// </summary>
    [Parameter] public bool Dense { get; set; }

    /// <summary>
    /// Gets or sets extra classes for the button.
    /// </summary>
    [Parameter] public string? ButtonClass { get; set; }

    /// <summary>
    /// Gets or sets whether the button is disabled, so the panel cannot open.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets a heading shown at the top of the panel, which also names it for assistive technology.
    /// </summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// Gets or sets whether the panel has a close button in its top corner, which helps touch and screen reader
    /// users close it.
    /// </summary>
    [Parameter] public bool ShowCloseButton { get; set; }

    /// <summary>
    /// Gets or sets which side of the trigger the panel opens on. Defaults to <see cref="PopoverPlacement.Bottom"/>.
    /// </summary>
    [Parameter] public PopoverPlacement Placement { get; set; } = PopoverPlacement.Bottom;

    /// <summary>
    /// Gets or sets how the panel lines up with the trigger. Defaults to <see cref="PopoverAlign.Start"/>.
    /// </summary>
    [Parameter] public PopoverAlign Align { get; set; } = PopoverAlign.Start;

    /// <summary>
    /// Gets or sets the gap between the trigger and the panel, in pixels. Defaults to 8.
    /// </summary>
    [Parameter] public int Offset { get; set; } = 8;

    /// <summary>
    /// Gets or sets whether opening the panel moves focus into it: onto an element marked
    /// <c>data-tw-autofocus</c>, otherwise its first control, otherwise the panel itself. Defaults to
    /// <see langword="true"/>. When off, the panel's content follows the button in the Tab order.
    /// </summary>
    [Parameter] public bool AutoFocus { get; set; } = true;

    /// <summary>
    /// Gets or sets whether clicking outside the panel closes it. Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter] public bool CloseOnOutsideClick { get; set; } = true;

    /// <summary>
    /// Gets or sets extra classes for the panel.
    /// </summary>
    [Parameter] public string? PanelClass { get; set; }

    /// <summary>
    /// Gets or sets whether the panel is open. Use with <see cref="OpenChanged"/> (<c>@bind-Open</c>) to open and
    /// close it from code.
    /// </summary>
    [Parameter] public bool Open { get; set; }

    /// <summary>
    /// Raised when the panel opens or closes.
    /// </summary>
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }

    /// <summary>
    /// Gets whether the panel is open.
    /// </summary>
    public bool IsOpen => isOpen;

    private string triggerId => $"{Id}-trigger";

    private string panelId => $"{Id}-panel";

    private string titleId => $"{Id}-title";

    private bool hasTitle => !string.IsNullOrWhiteSpace(Title);

    private string effectiveAriaLabel => AriaLabel ?? Label ?? string.Empty;

    private string rootClasses => new ClassBuilder(theme.Root).AddClass(Class).Build();

    private string panelClasses => new ClassBuilder(theme.Panel)
        .AddClass(popoverBuilder.GetSurfaceClasses(Rounded ?? theme.Rounded, Shadow ?? theme.Shadow))
        .AddClass(PanelClass ?? string.Empty)
        .Build();

    private Dictionary<string, object> triggerAttributes => new()
    {
        ["aria-haspopup"] = "dialog",
        ["aria-expanded"] = isOpen ? "true" : "false",
        ["aria-controls"] = panelId
    };

    private PopoverTriggerContext triggerContext => new(
        new Dictionary<string, object>(triggerAttributes) { ["id"] = triggerId, ["disabled"] = Disabled },
        isOpen,
        ToggleAsync);

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (Open && !isOpen)
        {
            await OpenAsync();
        }
        else if (!Open && isOpen)
        {
            await CloseAsync(returnFocus: false, notify: false);
        }
    }

    /// <summary>
    /// Opens the panel.
    /// </summary>
    /// <returns>A task that completes once the panel is open.</returns>
    public async Task OpenAsync()
    {
        if (Disabled || isOpen)
        {
            return;
        }

        isOpen = true;
        Open = true;
        pendingOpen = true;
        StateHasChanged();
        await OpenChanged.InvokeAsync(true);
    }

    /// <summary>
    /// Closes the panel and returns focus to the button.
    /// </summary>
    /// <returns>A task that completes once the panel is closed.</returns>
    public Task CloseAsync() => CloseAsync(returnFocus: true, notify: true);

    /// <summary>
    /// Opens the panel when it is closed, and closes it when it is open.
    /// </summary>
    /// <returns>A task that completes once the panel has opened or closed.</returns>
    public Task ToggleAsync() => isOpen ? CloseAsync() : OpenAsync();

    /// <summary>
    /// Closes the panel after a click outside it, leaving focus where the click put it.
    /// </summary>
    /// <returns>A task that completes once the panel is closed.</returns>
    [JSInvokable("Close")]
    public Task CloseFromOutsideClick() => CloseAsync(returnFocus: false, notify: true);

    /// <summary>
    /// Closes the panel when Escape is pressed, returning focus to the button.
    /// </summary>
    /// <returns>A task that completes once the panel is closed.</returns>
    [JSInvokable]
    public Task CloseFromEscape() => CloseAsync(returnFocus: true, notify: true);

    /// <summary>
    /// Closes the panel when focus moves out of it, leaving focus where it went.
    /// </summary>
    /// <returns>A task that completes once the panel is closed.</returns>
    [JSInvokable]
    public Task CloseFromFocusOut() => CloseAsync(returnFocus: false, notify: true);

    private async Task CloseAsync(bool returnFocus, bool notify)
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;
        Open = false;
        pendingOpen = false;
        await UnregisterAsync();

        if (returnFocus)
        {
            await InvokeScriptAsync("twMenu.focus", triggerRef);
        }

        await InvokeAsync(StateHasChanged);

        if (notify)
        {
            await OpenChanged.InvokeAsync(false);
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (isOpen && pendingOpen)
        {
            pendingOpen = false;
            selfReference ??= DotNetObjectReference.Create(this);
            registered = true;
            await InvokeScriptAsync("twPopover.open", rootRef, triggerRef, panelRef, selfReference, new
            {
                placement = Placement.ToString().ToLowerInvariant(),
                align = Align.ToString().ToLowerInvariant(),
                offset = Offset,
                autoFocus = AutoFocus,
                closeOnOutsideClick = CloseOnOutsideClick
            });
        }
    }

    private async Task UnregisterAsync()
    {
        if (!registered)
        {
            return;
        }

        registered = false;
        await InvokeScriptAsync("twPopover.close", rootRef, panelRef);
    }

    private async Task InvokeScriptAsync(string identifier, params object?[] args)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync(identifier, args);
        }
        catch (JSDisconnectedException)
        {
            // The circuit disconnected before the script could run.
        }
        catch (InvalidOperationException)
        {
            // JS interop is not available (prerendering).
        }
    }

    /// <summary>
    /// Removes the popover's listeners and releases the reference the script held to it.
    /// </summary>
    /// <returns>A task that completes once the popover is cleaned up.</returns>
    public async ValueTask DisposeAsync()
    {
        await UnregisterAsync();
        selfReference?.Dispose();
        GC.SuppressFinalize(this);
    }
}
