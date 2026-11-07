// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Configuration.Components;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// An area that opens a menu of actions where a person right-clicks it, such as the actions for a file, a row or a
/// canvas. The menu takes the same items as a <see cref="TwMenu"/>.
/// </summary>
/// <remarks>The menu opens at the pointer on a right-click (or a long press, on touch screens that support it) and is
/// kept inside the window. Keyboard users open it with Shift+F10 or the Menu key while focus is in the area; it then
/// opens against the focused element with focus on its first item. Inside the menu the keys are the same as in a
/// <see cref="TwMenu"/>. Escape closes it and returns focus to where it was; a click elsewhere, a scroll or choosing an
/// action close it too. Outside the area the browser keeps its own context menu. Put <see cref="TwMenuItem"/>,
/// <see cref="TwMenuCheckboxItem"/>, <see cref="TwMenuRadioGroup{TValue}"/>, <see cref="TwSubmenu"/>,
/// <see cref="TwMenuDivider"/> and <see cref="TwMenuGroup"/> in <see cref="MenuContent"/>.</remarks>
public partial class TwContextMenu : TwBlazorComponentBase, IAsyncDisposable, ITwMenuHost
{
    private bool isOpen;
    private bool attached;
    private bool registered;
    private PendingOpen? pendingOpen;
    private ElementReference areaRef;
    private ElementReference menuRef;
    private DotNetObjectReference<TwContextMenu>? selfReference;

    [Inject] private IJSRuntime jsRuntime { get; set; } = null!;

    private TwMenuTheme theme => options.Theme.Components.Require<TwMenuTheme>();

    /// <summary>
    /// Gets or sets the area that opens the menu when it is right-clicked.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the menu's items.
    /// </summary>
    [Parameter]
    public RenderFragment? MenuContent { get; set; }

    /// <summary>
    /// Whether the area is a Tab stop of its own, so keyboard users can reach it and open the menu with Shift+F10 or
    /// the Menu key. Set it when nothing inside the area can take focus. The area then tells screen readers how to open
    /// the menu; give it a name with an <c>aria-label</c> attribute. Defaults to <c>false</c>.
    /// </summary>
    [Parameter]
    public bool Focusable { get; set; }

    /// <summary>
    /// Whether the menu is turned off, so a right-click shows the browser's own context menu instead.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets extra classes for the menu panel.
    /// </summary>
    [Parameter]
    public string? MenuClass { get; set; }

    /// <summary>
    /// Raised when the menu opens or closes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    /// <summary>
    /// Gets whether the menu is open.
    /// </summary>
    public bool IsOpen => isOpen;

    private string menuId => $"{Id}-menu";

    private string hintId => $"{Id}-hint";

    // The menu is named by AriaLabel, or else the localized "Context menu".
    private string effectiveMenuLabel => AriaLabel ?? localizer[TwStrings.ContextMenu_Label];

    private string areaClasses => new ClassBuilder(theme.ContextArea).AddClass(Class).Build();

    private string panelClasses => new ClassBuilder(theme.Panel).AddClass(MenuClass ?? string.Empty).Build();

    /// <summary>
    /// Opens the menu at a point in the window, or moves it there when it is already open. A right-click, Shift+F10
    /// and the Menu key call this.
    /// </summary>
    /// <param name="clientX">The horizontal position, in CSS pixels from the left of the window.</param>
    /// <param name="clientY">The vertical position, in CSS pixels from the top of the window.</param>
    /// <param name="fromKeyboard">Whether the menu is opened from the keyboard, which moves focus to its first item.
    /// Opened by a pointer, the menu itself takes focus, so the arrow keys still reach the items.</param>
    /// <returns>A task that completes once the menu is open.</returns>
    [JSInvokable("OpenAt")]
    public async Task OpenAtAsync(double clientX, double clientY, bool fromKeyboard = false)
    {
        if (Disabled)
        {
            return;
        }

        pendingOpen = new PendingOpen(clientX, clientY, fromKeyboard ? "first" : "menu");

        if (isOpen)
        {
            // Already open: move it to the new point rather than closing and opening it again.
            StateHasChanged();
            return;
        }

        isOpen = true;
        StateHasChanged();
        await OpenChanged.InvokeAsync(true);
    }

    /// <summary>
    /// Closes the menu and returns focus to where it was before the menu opened. Escape and choosing an item call
    /// this too.
    /// </summary>
    /// <returns>A task that completes once the menu is closed.</returns>
    [JSInvokable("Close")]
    public Task CloseAsync() => CloseAsync(returnFocus: true);

    /// <summary>
    /// Closes the menu without moving focus, because a click elsewhere, a scroll or Tab is already moving it on.
    /// </summary>
    /// <returns>A task that completes once the menu is closed.</returns>
    [JSInvokable]
    public Task Dismiss() => CloseAsync(returnFocus: false);

    Task ITwMenuHost.CloseMenuAsync(bool returnFocus) => CloseAsync(returnFocus);

    private async Task CloseAsync(bool returnFocus)
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;
        pendingOpen = null;

        if (registered)
        {
            registered = false;
            await InvokeScriptAsync("twContextMenu.close", areaRef, menuRef, returnFocus);
        }

        await InvokeAsync(StateHasChanged);
        await OpenChanged.InvokeAsync(false);
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            selfReference ??= DotNetObjectReference.Create(this);
            attached = true;
            await InvokeScriptAsync("twContextMenu.attach", areaRef, selfReference);
        }

        if (isOpen && pendingOpen is { } open)
        {
            pendingOpen = null;
            registered = true;
            await InvokeScriptAsync("twContextMenu.open", areaRef, menuRef, selfReference, open.X, open.Y, open.Focus);
        }
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
    /// Removes the area's and the menu's listeners and releases the reference the script held to the component.
    /// </summary>
    /// <returns>A task that completes once the component is cleaned up.</returns>
    public async ValueTask DisposeAsync()
    {
        if (attached)
        {
            attached = false;
            await InvokeScriptAsync("twContextMenu.detach", areaRef);
        }

        selfReference?.Dispose();
        GC.SuppressFinalize(this);
    }

    private sealed record PendingOpen(double X, double Y, string Focus);
}
