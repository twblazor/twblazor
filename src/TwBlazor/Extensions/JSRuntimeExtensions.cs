// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.JSInterop;

namespace TwBlazor.Extensions;

/// <summary>
/// JS interop calls that skip quietly when the script cannot run: the circuit has disconnected or is being torn down, or
/// the component is prerendering and JS interop is not available yet. A <see cref="JSException"/> thrown by the script
/// itself still reaches the caller.
/// </summary>
internal static class JSRuntimeExtensions
{
    /// <summary>
    /// Calls a JavaScript function that returns nothing, doing nothing when JS interop is unavailable.
    /// </summary>
    /// <param name="jsRuntime">The JS runtime.</param>
    /// <param name="identifier">The function to call, relative to the global scope, such as <c>twDialog.focusPanel</c>.</param>
    /// <param name="args">The arguments, each of which must be JSON serializable.</param>
    /// <returns><see langword="true"/> when the call ran; <see langword="false"/> when JS interop was unavailable.</returns>
    public static async ValueTask<bool> TryInvokeVoidAsync(this IJSRuntime jsRuntime, string identifier, params object?[]? args)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync(identifier, args);
            return true;
        }
        catch (JSDisconnectedException)
        {
            return false;
        }
        catch (OperationCanceledException)
        {
            // The circuit is being torn down, so the call was cancelled before it ran.
            return false;
        }
        catch (InvalidOperationException)
        {
            // JS interop cannot run while the component is prerendering.
            return false;
        }
    }

    /// <summary>
    /// Calls a JavaScript function and returns its result, or <paramref name="fallback"/> when JS interop is
    /// unavailable.
    /// </summary>
    /// <typeparam name="TValue">The JSON-serializable type of the result.</typeparam>
    /// <param name="jsRuntime">The JS runtime.</param>
    /// <param name="identifier">The function to call, relative to the global scope, such as <c>twDialog.captureFocus</c>.</param>
    /// <param name="fallback">The value returned when the call could not run.</param>
    /// <param name="args">The arguments, each of which must be JSON serializable.</param>
    /// <returns>The function's result, or <paramref name="fallback"/>.</returns>
    public static async ValueTask<TValue> TryInvokeAsync<TValue>(this IJSRuntime jsRuntime, string identifier, TValue fallback, params object?[]? args)
    {
        try
        {
            return await jsRuntime.InvokeAsync<TValue>(identifier, args);
        }
        catch (JSDisconnectedException)
        {
            return fallback;
        }
        catch (OperationCanceledException)
        {
            // The circuit is being torn down, so the call was cancelled before it ran.
            return fallback;
        }
        catch (InvalidOperationException)
        {
            // JS interop cannot run while the component is prerendering.
            return fallback;
        }
    }
}
