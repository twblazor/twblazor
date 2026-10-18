// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Localization;

namespace TwBlazor.Components;

public partial class TwTable : IAsyncDisposable
{
    private readonly Dictionary<string, TwTableColumn> _columnsById = [];
    private DotNetObjectReference<TwTable>? selfReference;
    private bool resizeAttached;

    /// <summary>
    /// Whether people can resize the columns defined in <see cref="ColumnGroup"/> by dragging the border at the end
    /// of a column's header, or by focusing that border and pressing the arrow keys. Turns on
    /// <see cref="FixedLayout"/> so a column keeps the width it is given. Defaults to <c>false</c>.
    /// </summary>
    /// <remarks>Each border is a focusable separator that reports the column's width. Use
    /// <see cref="TwTableColumn.Label"/> to name it after the column, and <see cref="TwTableColumn.Resizable"/> to keep
    /// a column at its width. A resize gives every column its current width in pixels, so the others stay put.</remarks>
    [Parameter]
    public bool ResizableColumns { get; set; }

    /// <summary>
    /// Raised once a person finishes resizing, with the new width of every column, keyed by each
    /// <see cref="TwTableColumn"/>'s <see cref="TwBlazorComponentBase.Id"/>.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyDictionary<string, string>> ColumnsResized { get; set; }

    private bool effectiveFixedLayout => FixedLayout || ResizableColumns;

    private string resizeLabelFormat => localizer[TwStrings.Table_ResizeColumn, "{0}"];

    private string resizeValueFormat => localizer[TwStrings.Table_ColumnWidthValue, "{0}"];

    internal void RegisterColumn(TwTableColumn column) => _columnsById[column.Id!] = column;

    internal void UnregisterColumn(TwTableColumn column)
    {
        if (column.Id is { } id && _columnsById.TryGetValue(id, out var registered) && ReferenceEquals(registered, column))
        {
            _columnsById.Remove(id);
        }
    }

    /// <summary>
    /// Applies the widths the resize script measured once a person finishes resizing, by dragging a border or from the
    /// keyboard.
    /// </summary>
    /// <param name="widths">The width in pixels of each column, keyed by the column's id.</param>
    /// <returns>A task that completes once the widths are applied.</returns>
    [JSInvokable]
    public async Task CommitColumnWidths(Dictionary<string, int> widths)
    {
        ArgumentNullException.ThrowIfNull(widths);

        var applied = new Dictionary<string, string>();
        foreach (var (id, pixels) in widths)
        {
            if (!_columnsById.TryGetValue(id, out var column))
            {
                continue;
            }

            var width = $"{Math.Max(0, pixels).ToString(CultureInfo.InvariantCulture)}px";
            applied[id] = width;
            await column.ApplyResizedWidthAsync(width);
        }

        if (applied.Count > 0)
        {
            await ColumnsResized.InvokeAsync(applied);
        }
    }

    private async Task SyncResizeScriptAsync()
    {
        if (ResizableColumns && !resizeAttached)
        {
            resizeAttached = true;
            selfReference ??= DotNetObjectReference.Create(this);
            await InvokeScriptAsync("twTable.attachResize", wrapperRef, selfReference);
        }
        else if (!ResizableColumns && resizeAttached)
        {
            resizeAttached = false;
            await InvokeScriptAsync("twTable.detachResize", wrapperRef);
        }
        else if (resizeAttached)
        {
            // Columns may have been added, hidden, reordered or resized by the parent.
            await InvokeScriptAsync("twTable.layoutResize", wrapperRef);
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
    /// Detaches the resize script and releases the reference it held to the table.
    /// </summary>
    /// <returns>A task that completes once the table is cleaned up.</returns>
    public async ValueTask DisposeAsync()
    {
        if (resizeAttached)
        {
            resizeAttached = false;
            await InvokeScriptAsync("twTable.detachResize", wrapperRef);
        }

        selfReference?.Dispose();
        GC.SuppressFinalize(this);
    }
}
