// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

// Design and API shape inspired by MudBlazor's MudDialogContainer
// (https://github.com/MudBlazor/MudBlazor/tree/dev/src/MudBlazor/Components/Dialog), MIT License.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Diagnostics;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Models;
using TwBlazor.Services;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Renders the backdrop, positioning, and chrome (header, title, close button) around dialog content
/// shown via <see cref="ITwDialogService"/>.
/// </summary>
/// <remarks>
/// This component is rendered internally by <see cref="TwDialogProvider"/> for each active dialog and is not
/// typically instantiated directly by consumers.
/// </remarks>
public partial class TwDialog : TwBlazorComponentBase, IAsyncDisposable
{
    [Inject] private DialogBuilder dialogBuilder { get; set; } = null!;

    [Inject] private IJSRuntime jSRuntime { get; set; } = null!;

    private TwDialogTheme theme => options.Theme.Components.Require<TwOverlayTheme>().Dialog;

    private ElementReference surfaceRef;
    private bool hasFocused;
    private bool trapRegistered;

    /// <summary>
    /// Gets or sets the dialog reference this chrome renders content for.
    /// </summary>
    [Parameter, EditorRequired] public ITwDialogReference Reference { get; set; } = null!;

    private TwDialogOptions dialogOptions => Reference.Options ?? TwDialogOptions._default;

    private bool noHeader => dialogOptions.NoHeader ?? false;

    private bool showCloseButton => dialogOptions.CloseButton ?? true;

    private bool backdropClickEnabled => dialogOptions.BackdropClick ?? true;

    private bool closeOnEscapeKeyEnabled => dialogOptions.CloseOnEscapeKey ?? true;

    private string titleId => $"{Id}-title";

    /// <summary>
    /// Gets the accessible name to apply as <c>aria-label</c> on the dialog surface.
    /// </summary>
    /// <remarks>
    /// When <see cref="TwDialogOptions.NoHeader"/> is set, the header title (and its <c>aria-labelledby</c>
    /// wiring) is suppressed. If the consumer didn't separately supply <see cref="TwBlazorComponentBase.AriaLabel"/>
    /// or <see cref="TwBlazorComponentBase.AriaLabelledBy"/> in that case, fall back to the dialog's
    /// <see cref="ITwDialogReference.Title"/> (if any). If even that isn't available, fall back to a generic
    /// label as a last resort so the dialog never ships with no accessible name at all - the <c>#if DEBUG</c>
    /// warning in <see cref="OnParametersSet"/> exists to help developers notice and fix this case during
    /// development, but this fallback keeps Release builds accessible too.
    /// </remarks>
    private string? effectiveAriaLabel
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(AriaLabel))
            {
                return AriaLabel;
            }

            if (noHeader && string.IsNullOrWhiteSpace(AriaLabelledBy))
            {
                return !string.IsNullOrWhiteSpace(Reference?.Title) ? Reference.Title : "Dialog";
            }

            return AriaLabel;
        }
    }

    private string backdropClasses => dialogBuilder.GetBackdropClasses(dialogOptions.Position, dialogOptions.BackdropClass);

    private string surfaceClasses => new ClassBuilder(
            dialogBuilder.GetSurfaceClasses(
                dialogOptions.MaxWidth,
                dialogOptions.FullWidth ?? false,
                dialogOptions.FullScreen ?? false,
                dialogOptions.Rounded,
                dialogOptions.Shadow))
        .AddClass(dialogOptions.Class ?? string.Empty, !string.IsNullOrWhiteSpace(dialogOptions.Class))
        .AddClass(Class)
        .Build();

    private string headerClasses => theme.Header;

    private string titleClasses => theme.Title;

    private string closeButtonClasses => theme.CloseButton;

    private string contentClasses => theme.Content;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

#if DEBUG
        if (noHeader
            && string.IsNullOrWhiteSpace(AriaLabel)
            && string.IsNullOrWhiteSpace(AriaLabelledBy)
            && string.IsNullOrWhiteSpace(Reference?.Title))
        {
            Debug.WriteLine(
                $"[TwBlazor] TwDialog '{Id}' has NoHeader set but no AriaLabel, AriaLabelledBy, or Title was " +
                "supplied, so it will render with no accessible name for assistive technology. Set AriaLabel, " +
                "AriaLabelledBy, or a Title when showing the dialog.");
        }
#endif
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !hasFocused)
        {
            hasFocused = true;

            try
            {
                // Trap Tab/Shift+Tab within the dialog surface and move initial focus onto it (or its
                // first focusable descendant) rather than the backdrop, so assistive technology lands
                // inside the dialog instead of on inert chrome.
                await jSRuntime.InvokeVoidAsync("twDialog.trapFocus", surfaceRef);
                trapRegistered = true;
                await jSRuntime.InvokeVoidAsync("twDialog.focusSurface", surfaceRef);
            }
            catch (JSDisconnectedException)
            {
                // The circuit disconnected before the script could run; nothing to focus.
            }
        }
    }

    /// <summary>
    /// Releases the JS-side Tab focus trap registered for this dialog's surface.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (trapRegistered)
        {
            try
            {
                await jSRuntime.InvokeVoidAsync("twDialog.releaseFocusTrap", surfaceRef);
            }
            catch (JSDisconnectedException)
            {
                // The circuit is already gone; nothing left to clean up.
            }
            catch (InvalidOperationException)
            {
                // JS interop unavailable during teardown (e.g. prerendering); safe to ignore.
            }
        }

        GC.SuppressFinalize(this);
    }

    private Task HandleBackdropClickAsync()
    {
        if (backdropClickEnabled)
        {
            Reference.Close(TwDialogResult.Cancel());
        }

        return Task.CompletedTask;
    }

    private Task HandleCloseButtonClickAsync()
    {
        Reference.Close(TwDialogResult.Cancel());
        return Task.CompletedTask;
    }

    private Task HandleKeyDownAsync(KeyboardEventArgs e)
    {
        if (closeOnEscapeKeyEnabled && e.Key == "Escape")
        {
            Reference.Close(TwDialogResult.Cancel());
        }

        return Task.CompletedTask;
    }
}
