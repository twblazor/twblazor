// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// A button that opens a menu of actions and options, such as a column's options in a table or a row's actions.
/// </summary>
/// <remarks>The menu follows the ARIA menu button pattern. The button says that it opens a menu and whether the menu
/// is open. Enter, Space or Arrow Down open the menu on its first item and Arrow Up on its last. In the menu the arrow
/// keys move between the items and wrap round, Home and End move to the first and last, and typing a letter moves to the
/// next item starting with it. Escape closes the menu and returns focus to the button, and Tab closes it and moves on.
/// Put <see cref="TwMenuItem"/>, <see cref="TwMenuCheckboxItem"/>, <see cref="TwMenuDivider"/> and
/// <see cref="TwMenuGroup"/> in it.</remarks>
public partial class TwMenu : TwBlazorComponentBase, IAsyncDisposable
{
    private bool isOpen;
    private string? pendingFocus;
    private bool registered;
    private ElementReference rootRef;
    private ElementReference triggerRef;
    private ElementReference menuRef;
    private DotNetObjectReference<TwMenu>? selfReference;

    [Inject] private IJSRuntime jsRuntime { get; set; } = null!;

    private TwMenuTheme theme => options.Theme.Components.Require<TwMenuTheme>();

    /// <summary>
    /// Gets or sets the menu's items.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the text of the button that opens the menu. Also names the menu for assistive technology.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets an icon for the button: before the <see cref="Label"/>, or on its own with
    /// <see cref="IconOnly"/>.
    /// </summary>
    [Parameter]
    public Icon? Icon { get; set; }

    /// <summary>
    /// Whether the button shows only its <see cref="Icon"/>. Give it an <see cref="TwBlazorComponentBase.AriaLabel"/>,
    /// which is also shown as a tooltip; without one it is named with the localized
    /// <see cref="TwStrings.Menu_Options"/> text ("Options").
    /// </summary>
    [Parameter]
    public bool IconOnly { get; set; }

    /// <summary>
    /// Gets or sets the look of the button. Defaults to the button theme's own default.
    /// </summary>
    [Parameter]
    public ButtonVariant? Variant { get; set; }

    /// <summary>
    /// Whether the button uses its compact size.
    /// </summary>
    [Parameter]
    public bool Dense { get; set; }

    /// <summary>
    /// Gets or sets extra classes for the button.
    /// </summary>
    [Parameter]
    public string? ButtonClass { get; set; }

    /// <summary>
    /// Whether the button is disabled, so the menu cannot open.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Raised when the menu opens or closes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    /// <summary>
    /// Gets whether the menu is open.
    /// </summary>
    public bool IsOpen => isOpen;

    private string triggerId => $"{Id}-trigger";

    private string menuId => $"{Id}-menu";

    private string effectiveAriaLabel => AriaLabel ?? Label ?? localizer[TwStrings.Menu_Options];

    private string rootClasses => new ClassBuilder(theme.Root).AddClass(Class).Build();

    private Dictionary<string, object> triggerAttributes => new()
    {
        ["aria-haspopup"] = "menu",
        ["aria-expanded"] = isOpen ? "true" : "false",
        ["aria-controls"] = menuId,
        ["onkeydown"] = EventCallback.Factory.Create<KeyboardEventArgs>(this, OnTriggerKeyDownAsync)
    };

    /// <summary>
    /// Opens the menu and moves focus to its first item.
    /// </summary>
    /// <returns>A task that completes once the menu is open.</returns>
    public Task OpenAsync() => OpenAsync("first");

    private async Task OpenAsync(string focus)
    {
        if (Disabled || isOpen)
        {
            return;
        }

        isOpen = true;
        pendingFocus = focus;
        StateHasChanged();
        await OpenChanged.InvokeAsync(true);
    }

    /// <summary>
    /// Closes the menu and returns focus to its button. The Escape key and a click outside the menu call this too.
    /// </summary>
    /// <returns>A task that completes once the menu is closed.</returns>
    [JSInvokable("Close")]
    public Task CloseAsync() => CloseAsync(returnFocus: true);

    /// <summary>
    /// Closes the menu without moving focus, because Tab is already moving it on.
    /// </summary>
    /// <returns>A task that completes once the menu is closed.</returns>
    [JSInvokable]
    public Task CloseFromTab() => CloseAsync(returnFocus: false);

    internal async Task CloseAsync(bool returnFocus)
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;
        pendingFocus = null;
        await UnregisterAsync();

        if (returnFocus)
        {
            await InvokeScriptAsync("twMenu.focus", triggerRef);
        }

        await InvokeAsync(StateHasChanged);
        await OpenChanged.InvokeAsync(false);
    }

    private async Task ToggleAsync()
    {
        if (isOpen)
        {
            await CloseAsync();
        }
        else
        {
            await OpenAsync("first");
        }
    }

    private async Task OnTriggerKeyDownAsync(KeyboardEventArgs args)
    {
        // Enter and Space reach the button as a click, so only the arrows need handling here.
        if (args.Key == "ArrowDown")
        {
            await OpenAsync("first");
        }
        else if (args.Key == "ArrowUp")
        {
            await OpenAsync("last");
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (isOpen && pendingFocus is { } focus)
        {
            pendingFocus = null;
            selfReference ??= DotNetObjectReference.Create(this);
            registered = true;
            await InvokeScriptAsync("twPicker.registerOutsideClick", rootRef, selfReference, menuId);
            await InvokeScriptAsync("twMenu.open", menuRef, triggerRef, selfReference, focus);
        }
    }

    private async Task UnregisterAsync()
    {
        if (!registered)
        {
            return;
        }

        registered = false;
        await InvokeScriptAsync("twMenu.close", menuRef);
        await InvokeScriptAsync("twPicker.unregisterOutsideClick", rootRef, menuId);
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
    /// Removes the menu's listeners and releases the reference the script held to it.
    /// </summary>
    /// <returns>A task that completes once the menu is cleaned up.</returns>
    public async ValueTask DisposeAsync()
    {
        await UnregisterAsync();
        selfReference?.Dispose();
        GC.SuppressFinalize(this);
    }
}
