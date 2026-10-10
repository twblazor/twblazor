// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

// Design and API shape inspired by MudBlazor's MudDialogProvider
// (https://github.com/MudBlazor/MudBlazor/tree/dev/src/MudBlazor/Components/Dialog), MIT License.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Models;
using TwBlazor.Services;

namespace TwBlazor.Components;

/// <summary>
/// Hosts and renders dialogs shown through <see cref="ITwDialogService"/>.
/// </summary>
/// <remarks>
/// Add a single instance of this component to your layout, typically alongside <see cref="TwToastProvider"/>.
/// </remarks>
public sealed partial class TwDialogProvider : IDisposable
{
    [Inject] private ITwDialogService dialogService { get; set; } = null!;

    [Inject] private IJSRuntime? jsRuntime { get; set; }

    private readonly List<ITwDialogReference> _dialogs = [];

    /// <summary>
    /// The provider's root element, excluded when the rest of the page is marked <c>inert</c> while a
    /// dialog is open.
    /// </summary>
    private ElementReference containerRef;

    // Names this provider's claim on the page's inert state, so a picker closing inside a dialog lifts
    // only what the picker set (see twDialog.setBackgroundInert).
    private const string inertOwner = "dialog";

    /// <summary>
    /// Opaque focus-restore tokens captured (via JS interop) at the moment each dialog was shown, keyed
    /// by dialog id, so focus can be returned to the triggering element once that dialog closes.
    /// </summary>
    private readonly Dictionary<Guid, string> _focusRestoreTokens = [];

    /// <summary>
    /// Subscribes to dialog service events.
    /// </summary>
    protected override void OnInitialized()
    {
        if (dialogService is null)
            return;

        dialogService.DialogInstanceAddedAsync += OnDialogInstanceAddedAsync;
        dialogService.OnDialogCloseRequested += OnDialogCloseRequested;
    }

    private async Task OnDialogInstanceAddedAsync(ITwDialogReference reference)
    {
        // Capture whatever has focus right now (almost always the element that triggered the dialog,
        // e.g. a button click) *before* the dialog renders and steals focus, so it can be restored
        // once this dialog closes.
        var wasEmpty = _dialogs.Count == 0;
        _dialogs.Add(reference);

        if (jsRuntime is not null)
        {
            try
            {
                var token = await jsRuntime.InvokeAsync<string?>("twDialog.captureFocus");
                if (!string.IsNullOrEmpty(token))
                {
                    _focusRestoreTokens[reference.Id] = token;
                }

                // Only the outermost dialog needs to inert the background - nested dialogs already
                // render inside this same provider container, which is already excluded.
                if (wasEmpty)
                {
                    await jsRuntime.InvokeVoidAsync("twDialog.setBackgroundInert", containerRef, inertOwner);
                }
            }
            catch (JSDisconnectedException)
            {
                // The circuit disconnected mid-call; nothing more to do.
            }
            catch (InvalidOperationException)
            {
                // JS interop unavailable (e.g. dialog shown during prerendering or before the circuit
                // is established); nothing more to do.
            }
        }

        await InvokeAsync(StateHasChanged);
    }

    private void OnDialogCloseRequested(ITwDialogReference reference, TwDialogResult? result)
    {
        if (!_dialogs.Remove(reference))
            return;

        reference.Dismiss(result);

        _ = FinalizeDialogCloseAsync(reference, wasLastDialog: _dialogs.Count == 0);

        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Restores focus to the element that triggered <paramref name="reference"/>'s dialog and, once the
    /// last open dialog has closed, removes the background <c>inert</c> marking.
    /// </summary>
    private async Task FinalizeDialogCloseAsync(ITwDialogReference reference, bool wasLastDialog)
    {
        if (jsRuntime is null)
            return;

        try
        {
            if (wasLastDialog)
            {
                await jsRuntime.InvokeVoidAsync("twDialog.clearBackgroundInert", inertOwner);
            }

            if (_focusRestoreTokens.Remove(reference.Id, out var token) && !string.IsNullOrEmpty(token))
            {
                await jsRuntime.InvokeVoidAsync("twDialog.restoreFocus", token);
            }
        }
        catch (JSDisconnectedException)
        {
            // The circuit disconnected mid-call; nothing more to do.
        }
    }

    /// <summary>
    /// Marks any newly-added dialogs as rendered so pending Show calls can complete.
    /// </summary>
    protected override void OnAfterRender(bool firstRender)
    {
        foreach (var reference in _dialogs)
        {
            reference.RenderCompleteTaskCompletionSource.TrySetResult(true);
        }
    }

    /// <summary>
    /// Unsubscribes from dialog service events and, if any dialogs were still open, best-effort clears
    /// the background <c>inert</c> marking left by <see cref="OnDialogInstanceAddedAsync"/>.
    /// </summary>
    public void Dispose()
    {
        if (dialogService is null)
            return;

        dialogService.DialogInstanceAddedAsync -= OnDialogInstanceAddedAsync;
        dialogService.OnDialogCloseRequested -= OnDialogCloseRequested;

        if (_dialogs.Count > 0 && jsRuntime is not null)
        {
            // Dispose() is synchronous but the clean up call is JS interop (async), so it's fired and
            // forgotten here on a best-effort basis: each still-open TwDialog releases its own focus
            // trap via its own IAsyncDisposable.DisposeAsync as it's torn down, but the background
            // inert marking is owned by this provider and would otherwise be left in place permanently
            // if the provider itself is disposed while dialogs remain open.
            _ = ClearBackgroundInertBestEffortAsync();
        }

        GC.SuppressFinalize(this);
    }

    private async Task ClearBackgroundInertBestEffortAsync()
    {
        try
        {
            await jsRuntime!.InvokeVoidAsync("twDialog.clearBackgroundInert", inertOwner);
        }
        catch (JSDisconnectedException)
        {
            // The circuit disconnected mid-call; nothing more to do.
        }
        catch (InvalidOperationException)
        {
            // JS interop unavailable during teardown (e.g. prerendering); safe to ignore.
        }
    }
}
