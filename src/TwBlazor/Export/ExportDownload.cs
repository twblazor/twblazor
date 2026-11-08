// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.JSInterop;
using TwBlazor.Models;

namespace TwBlazor.Export;

/// <summary>
/// Hands an exported file to the browser to save, the same way for every component that exports, such as
/// <see cref="Components.TwDataTable{TItem}"/> and <see cref="Components.TwTextEditor"/>.
/// </summary>
/// <remarks>The bytes are streamed to the browser, so a large file does not have to fit in one interop message on
/// Blazor Server.</remarks>
internal static class ExportDownload
{
    /// <summary>
    /// Saves a file in the browser.
    /// </summary>
    /// <param name="jsRuntime">The JS runtime of the component exporting the file.</param>
    /// <param name="file">The file.</param>
    /// <returns><see langword="true"/> when the browser was handed the file; <see langword="false"/> when the circuit
    /// or the JS runtime was gone.</returns>
    public static async Task<bool> SaveAsync(IJSRuntime jsRuntime, ExportFile file)
    {
        using var stream = new MemoryStream(file.Content);
        using var streamReference = new DotNetStreamReference(stream);

        try
        {
            await jsRuntime.InvokeVoidAsync("twDownload.save", file.FileName, file.ContentType, streamReference);
            return true;
        }
        catch (JSDisconnectedException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}
