// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// A panel that slides in from an edge of the screen over the page, for a form, a set of filters, the details of an
/// item or navigation on a small screen. From the bottom edge it is a bottom sheet.
/// </summary>
/// <remarks>
/// <para>
/// Open and close it with <c>@bind-Open</c>, or from code with <see cref="OpenAsync"/> and <see cref="CloseAsync"/>.
/// The content is first rendered when the drawer first opens and is kept after that, so what was typed into it
/// survives closing it again.
/// </para>
/// <para>
/// Accessibility: the panel is a dialog named by its <see cref="Title"/> and described by its
/// <see cref="Description"/>. Opening moves focus into it: onto an element marked <c>data-tw-autofocus</c>,
/// otherwise its first control, otherwise the panel itself. A <see cref="Modal"/> drawer keeps Tab inside it, makes
/// the page behind it inert and stops the page from scrolling. Escape, the close button and a click on the backdrop
/// close it, and focus goes back to the element that had it before the drawer opened.
/// </para>
/// </remarks>
public partial class TwDrawer : TwBlazorComponentBase, IAsyncDisposable
{
    private bool isOpen;
    private bool hasRendered;
    private bool pendingActivate;
    private bool active;
    private ElementReference rootRef;
    private ElementReference panelRef;
    private DotNetObjectReference<TwDrawer>? selfReference;

    [Inject] private IJSRuntime jsRuntime { get; set; } = null!;

    private TwDrawerTheme theme => options.Theme.Components.Require<TwDrawerTheme>();

    /// <summary>
    /// Gets or sets the drawer's body content.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets content shown in a footer below the body, such as the drawer's actions. The footer stays in view
    /// while the body scrolls.
    /// </summary>
    [Parameter] public RenderFragment? FooterContent { get; set; }

    /// <summary>
    /// Gets or sets the heading at the top of the drawer, which also names it for assistive technology.
    /// </summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// Gets or sets a line of text under the <see cref="Title"/>, which screen readers announce with the name.
    /// </summary>
    [Parameter] public string? Description { get; set; }

    /// <summary>
    /// Gets or sets whether the drawer is open. Use with <see cref="OpenChanged"/> (<c>@bind-Open</c>).
    /// </summary>
    [Parameter] public bool Open { get; set; }

    /// <summary>
    /// Raised when the drawer opens or closes.
    /// </summary>
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }

    /// <summary>
    /// Gets or sets the edge of the screen the drawer slides in from. Defaults to <see cref="DrawerPosition.End"/>.
    /// </summary>
    [Parameter] public DrawerPosition Position { get; set; } = DrawerPosition.End;

    /// <summary>
    /// Gets or sets how far the drawer reaches into the screen: its width on the start or end edge, its height on
    /// the top or bottom edge. Defaults to <see cref="DrawerSize.Medium"/>.
    /// </summary>
    [Parameter] public DrawerSize Size { get; set; } = DrawerSize.Medium;

    /// <summary>
    /// Gets or sets whether the drawer blocks the page behind it with a backdrop. Defaults to
    /// <see langword="true"/>. When off, the page stays usable while the drawer is open, as a side sheet.
    /// </summary>
    [Parameter] public bool Modal { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the header has a close button. Defaults to <see langword="true"/>. Keep it unless the
    /// drawer has another visible way to close it, as touch and screen reader users rely on it.
    /// </summary>
    [Parameter] public bool ShowCloseButton { get; set; } = true;

    /// <summary>
    /// Gets or sets whether a click on the backdrop closes a modal drawer. Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter] public bool CloseOnBackdropClick { get; set; } = true;

    /// <summary>
    /// Gets or sets whether Escape closes the drawer. Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter] public bool CloseOnEscape { get; set; } = true;

    /// <summary>
    /// Gets or sets extra classes for the backdrop.
    /// </summary>
    [Parameter] public string? BackdropClass { get; set; }

    /// <summary>
    /// Gets or sets extra classes for the body.
    /// </summary>
    [Parameter] public string? ContentClass { get; set; }

    /// <summary>
    /// Gets whether the drawer is open.
    /// </summary>
    public bool IsOpen => isOpen;

    private string state => isOpen ? "open" : "closed";

    private string titleId => $"{Id}-title";

    private string descriptionId => $"{Id}-description";

    private bool hasTitle => !string.IsNullOrWhiteSpace(Title);

    private bool hasDescription => !string.IsNullOrWhiteSpace(Description);

    private string? effectiveAriaLabel =>
        !string.IsNullOrWhiteSpace(AriaLabel) ? AriaLabel
        : !hasTitle && string.IsNullOrWhiteSpace(AriaLabelledBy) ? localizer[TwStrings.Dialog_Label]
        : null;

    private string? effectiveAriaLabelledBy =>
        !string.IsNullOrWhiteSpace(AriaLabelledBy) ? AriaLabelledBy
        : hasTitle && string.IsNullOrWhiteSpace(AriaLabel) ? titleId
        : null;

    private bool isVertical => Position is DrawerPosition.Top or DrawerPosition.Bottom;

    private string positionClasses => Position switch
    {
        DrawerPosition.Start => theme.Start,
        DrawerPosition.Top => theme.Top,
        DrawerPosition.Bottom => theme.Bottom,
        _ => theme.End
    };

    private string sizeClasses => (Size, isVertical) switch
    {
        (DrawerSize.Small, false) => theme.SmallWidth,
        (DrawerSize.Large, false) => theme.LargeWidth,
        (DrawerSize.Full, false) => theme.FullWidth,
        (_, false) => theme.MediumWidth,
        (DrawerSize.Small, true) => theme.SmallHeight,
        (DrawerSize.Large, true) => theme.LargeHeight,
        (DrawerSize.Full, true) => theme.FullHeight,
        (_, true) => theme.MediumHeight
    };

    private string backdropClasses => new ClassBuilder(theme.Backdrop)
        .AddClass(BackdropClass ?? string.Empty)
        .Build();

    private string panelClasses => new ClassBuilder(theme.Panel)
        .AddClass(positionClasses)
        .AddClass(sizeClasses)
        .AddClass(Class)
        .Build();

    private string contentClasses => new ClassBuilder(theme.Content)
        .AddClass(ContentClass ?? string.Empty)
        .Build();

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (Open && !isOpen)
        {
            await OpenCoreAsync(notify: false);
        }
        else if (!Open && isOpen)
        {
            await CloseCoreAsync(notify: false);
        }
    }

    /// <summary>
    /// Opens the drawer.
    /// </summary>
    /// <returns>A task that completes once the drawer is open.</returns>
    public Task OpenAsync() => OpenCoreAsync(notify: true);

    private async Task OpenCoreAsync(bool notify)
    {
        if (isOpen)
        {
            return;
        }

        isOpen = true;
        Open = true;
        hasRendered = true;
        pendingActivate = true;
        StateHasChanged();

        if (notify)
        {
            await OpenChanged.InvokeAsync(true);
        }
    }

    /// <summary>
    /// Closes the drawer and returns focus to the element that had it before the drawer opened.
    /// </summary>
    /// <returns>A task that completes once the drawer is closed.</returns>
    public Task CloseAsync() => CloseCoreAsync(notify: true);

    /// <summary>
    /// Opens the drawer when it is closed, and closes it when it is open.
    /// </summary>
    /// <returns>A task that completes once the drawer has opened or closed.</returns>
    public Task ToggleAsync() => isOpen ? CloseAsync() : OpenAsync();

    /// <summary>
    /// Closes the drawer when Escape is pressed. Invoked from JavaScript, which listens on the document and only
    /// calls the most recently opened dialog, drawer or popover.
    /// </summary>
    /// <returns>A task that completes once the drawer is closed.</returns>
    [JSInvokable("CloseFromEscape")]
    public Task CloseFromEscapeAsync() => CloseOnEscape ? CloseAsync() : Task.CompletedTask;

    private Task HandleBackdropClickAsync() => CloseOnBackdropClick ? CloseAsync() : Task.CompletedTask;

    private async Task CloseCoreAsync(bool notify)
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;
        Open = false;
        pendingActivate = false;
        await DeactivateAsync(returnFocus: true);
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

        if (isOpen && pendingActivate)
        {
            pendingActivate = false;
            selfReference ??= DotNetObjectReference.Create(this);
            active = true;
            await InvokeScriptAsync("twDrawer.open", rootRef, panelRef, selfReference, new
            {
                modal = Modal,
                closeOnEscape = CloseOnEscape
            });
        }
    }

    private async Task DeactivateAsync(bool returnFocus)
    {
        if (!active)
        {
            return;
        }

        active = false;
        await InvokeScriptAsync("twDrawer.close", panelRef, returnFocus);
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
    /// Releases the drawer's focus trap, background inert-ing, scroll lock and Escape listener, and the reference the
    /// script held to it.
    /// </summary>
    /// <returns>A task that completes once the drawer is cleaned up.</returns>
    public async ValueTask DisposeAsync()
    {
        await DeactivateAsync(returnFocus: false);
        selfReference?.Dispose();
        GC.SuppressFinalize(this);
    }
}
