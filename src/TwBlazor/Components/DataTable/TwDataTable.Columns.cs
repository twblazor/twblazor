// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

public partial class TwDataTable<TItem> : IAsyncDisposable
{
    /// <summary>
    /// The narrowest a person can make a column by resizing it, in pixels.
    /// </summary>
    public const int MinimumColumnWidth = 48;

    private const int maximumColumnWidth = 2000;
    private const int resizeStep = 32;

    private List<TwDataTableColumnState> columnStates = [];
    private List<TwDataTableColumn<TItem>> visibleColumns = [];
    private Dictionary<string, TwDataTableColumn<TItem>> columnsByName = [];
    private string? columnSignature;
    private bool columnPanelOpen;
    private string? pendingFocusId;
    private bool resizeAttached;
    private ElementReference rootRef;
    private DotNetObjectReference<TwDataTable<TItem>>? selfReference;

    /// <summary>
    /// Whether to show a Columns button that opens a panel for showing, hiding and reordering the columns.
    /// Only applies when <see cref="Columns"/> is used. Defaults to <c>false</c>.
    /// </summary>
    [Parameter]
    public bool ColumnPanel { get; set; }

    /// <summary>
    /// Gets or sets the text of the button that opens the column panel. Defaults to the localized
    /// <see cref="TwStrings.DataTable_Columns"/> text ("Columns").
    /// </summary>
    [Parameter]
    public string? ColumnPanelLabel { get; set; }

    /// <summary>
    /// Whether people can resize the columns by dragging the edge of a column header. The column panel gains
    /// Narrower and Wider buttons for each column too, so the Columns button is shown and resizing works from the
    /// keyboard. Only applies when <see cref="Columns"/> is used, and turns on <see cref="FixedLayout"/> so a
    /// column keeps the width it is given. Defaults to <c>false</c>.
    /// </summary>
    [Parameter]
    public bool ResizableColumns { get; set; }

    private string effectiveColumnPanelLabel => ColumnPanelLabel ?? localizer[TwStrings.DataTable_Columns];

    private bool showColumnPanelButton => (ColumnPanel || ResizableColumns) && hasColumns;

    private bool effectiveFixedLayout => FixedLayout || (ResizableColumns && hasColumns);

    private string columnPanelToggleId => $"{Id}-columns-toggle";

    private string columnPanelId => $"{Id}-columns";

    private Dictionary<string, object> columnPanelToggleAttributes => new()
    {
        ["aria-expanded"] = columnPanelOpen ? "true" : "false",
        ["aria-controls"] = columnPanelId
    };

    private string columnPanelHeaderClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Align.Center)
        .AddClass(options.Theme.Flexbox.Justify.Between)
        .AddClass(options.Theme.Spacing.Gap.Md)
        .AddClass(options.Theme.Spacing.MarginBottom.Md).Build();

    private string columnListClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Col)
        .AddClass(options.Theme.Spacing.Gap.Sm).Build();

    private string columnListItemClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Align.Center)
        .AddClass(options.Theme.Flexbox.Justify.Between)
        .AddClass(options.Theme.Spacing.Gap.Md).Build();

    private string columnMoveButtonsClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Spacing.Gap.Sm).Build();

    private string HeaderCellClasses(TwDataTableColumn<TItem> column) => new ClassBuilder(theme.HeaderCellPadding)
        .AddClass(options.Theme.Position.Relative, ResizableColumns && column.Resizable)
        .AddClass(column.HeaderClass ?? string.Empty).Build();

    private RenderFragment? effectiveColumnGroup =>
        hasColumns && (ResizableColumns || visibleColumns.Any(c => !string.IsNullOrWhiteSpace(ColumnWidth(c))))
            ? builder =>
            {
                foreach (var column in visibleColumns)
                {
                    builder.OpenComponent<TwTableColumn>(0);
                    builder.SetKey(column.Name);
                    builder.AddComponentParameter(1, nameof(TwTableColumn.Width), ColumnWidth(column));
                    builder.AddComponentParameter(2, "data-column", column.Name);
                    builder.CloseComponent();
                }
            }
            : ColumnGroup;

    private string? ColumnWidth(TwDataTableColumn<TItem> column) =>
        columnStates.FirstOrDefault(s => s.Name == column.Name)?.Width ?? column.Width;

    /// <summary>
    /// Rebuilds the column state when the set of column definitions changes, keeping what a person already
    /// changed for the columns that are still there.
    /// </summary>
    private void SyncColumns()
    {
        if (!hasColumns)
        {
            columnsByName = [];
            columnStates = [];
            visibleColumns = [];
            columnSignature = null;
            return;
        }

        columnsByName = Columns!
            .GroupBy(c => c.Name)
            .ToDictionary(g => g.Key, g => g.First());

        var signature = string.Join('\u001f', Columns!.Select(c => c.Name));
        if (signature != columnSignature)
        {
            columnSignature = signature;
            columnStates = MergeColumnStates(columnStates);
        }

        RebuildVisibleColumns();
    }

    private List<TwDataTableColumnState> DefaultColumnStates() =>
        [.. columnsByName.Values.Select(c => new TwDataTableColumnState { Name = c.Name, Visible = c.Visible, Width = c.Width })];

    /// <summary>
    /// Lines saved column settings up with the column definitions: settings for columns that are gone are
    /// dropped, new columns are added at the end with their defaults, and at least one column stays visible.
    /// </summary>
    private List<TwDataTableColumnState> MergeColumnStates(IEnumerable<TwDataTableColumnState> saved)
    {
        var merged = saved
            .Where(s => s is not null && columnsByName.ContainsKey(s.Name))
            .DistinctBy(s => s.Name)
            .Select(s => s.Clone())
            .ToList();

        foreach (var column in columnsByName.Values.Where(c => merged.All(s => s.Name != c.Name)))
        {
            merged.Add(new TwDataTableColumnState { Name = column.Name, Visible = column.Visible, Width = column.Width });
        }

        foreach (var state in merged.Where(s => !columnsByName[s.Name].Hideable))
        {
            state.Visible = true;
        }

        if (merged.Count > 0 && !merged.Any(s => s.Visible))
        {
            merged[0].Visible = true;
        }

        return merged;
    }

    private void RebuildVisibleColumns()
    {
        visibleColumns = [.. columnStates.Where(s => s.Visible).Select(s => columnsByName[s.Name])];
    }

    private bool CanToggleColumn(TwDataTableColumnState state) =>
        columnsByName[state.Name].Hideable && (!state.Visible || columnStates.Count(s => s.Visible) > 1);

    private void ToggleColumnPanel()
    {
        columnPanelOpen = !columnPanelOpen;
    }

    private void OnColumnPanelKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Escape")
        {
            columnPanelOpen = false;
            pendingFocusId = columnPanelToggleId;
        }
    }

    private async Task SetColumnVisibleAsync(string name, bool visible)
    {
        var state = columnStates.FirstOrDefault(s => s.Name == name);
        if (state is null || state.Visible == visible || !CanToggleColumn(state))
        {
            return;
        }

        state.Visible = visible;
        RebuildVisibleColumns();

        var title = columnsByName[name].Title;
        sortStatusMessage = visible
            ? localizer[TwStrings.DataTable_ColumnShown, title]
            : localizer[TwStrings.DataTable_ColumnHidden, title];

        await NotifyStateChangedAsync();
    }

    /// <summary>
    /// Moves a column one place earlier or later. Focus follows the column to its new place, moving to the other
    /// button when the one that was pressed can go no further.
    /// </summary>
    private async Task MoveColumnAsync(string name, int offset)
    {
        var from = columnStates.FindIndex(s => s.Name == name);
        var to = from + offset;
        if (from < 0 || to < 0 || to >= columnStates.Count)
        {
            return;
        }

        var state = columnStates[from];
        columnStates.RemoveAt(from);
        columnStates.Insert(to, state);
        RebuildVisibleColumns();

        var atEdge = offset < 0 ? to == 0 : to == columnStates.Count - 1;
        var direction = (offset < 0) != atEdge ? "earlier" : "later";
        pendingFocusId = ColumnMoveButtonId(to, direction);

        sortStatusMessage = localizer[TwStrings.DataTable_ColumnMoved, columnsByName[name].Title, to + 1, columnStates.Count];

        await NotifyStateChangedAsync();
    }

    private string ColumnMoveButtonId(int index, string direction) => $"{Id}-column-{index}-{direction}";

    private async Task ResetColumnsAsync()
    {
        columnStates = MergeColumnStates(DefaultColumnStates());
        RebuildVisibleColumns();
        sortStatusMessage = localizer[TwStrings.DataTable_ColumnsReset];
        await NotifyStateChangedAsync();
    }

    private int? ColumnPixelWidth(TwDataTableColumn<TItem> column)
    {
        var width = ColumnWidth(column);
        return width is not null
            && width.EndsWith("px", StringComparison.OrdinalIgnoreCase)
            && double.TryParse(width[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var pixels)
            ? (int)Math.Round(pixels)
            : null;
    }

    /// <summary>
    /// Makes a column narrower or wider from the column panel, which is how keyboard and single pointer users
    /// resize a column without dragging its edge.
    /// </summary>
    private async Task ResizeColumnAsync(TwDataTableColumn<TItem> column, int change)
    {
        var current = ColumnPixelWidth(column) ?? await MeasureColumnAsync(column);
        await CommitColumnWidth(column.Name, current + change);
        sortStatusMessage = localizer[TwStrings.DataTable_ColumnResized, column.Title, ColumnPixelWidth(column) ?? current];
    }

    private string ResizeButtonId(int index, string direction) => $"{Id}-column-{index}-{direction}";

    private async Task<int> MeasureColumnAsync(TwDataTableColumn<TItem> column)
    {
        try
        {
            return await jsRuntime.InvokeAsync<int>("twDataTable.measureColumn", rootRef, column.Name);
        }
        catch (JSDisconnectedException)
        {
            return MinimumColumnWidth;
        }
        catch (InvalidOperationException)
        {
            return MinimumColumnWidth;
        }
    }

    /// <summary>
    /// Sets a column's width once a person has finished resizing it, by dragging its edge or from the column panel.
    /// </summary>
    /// <param name="name">The <see cref="TwDataTableColumn{TItem}.Name"/> of the resized column.</param>
    /// <param name="width">The new width in pixels.</param>
    /// <returns>A task that completes once the width is applied.</returns>
    [JSInvokable]
    public async Task CommitColumnWidth(string name, int width)
    {
        var state = columnStates.FirstOrDefault(s => s.Name == name);
        if (state is null || !columnsByName[name].Resizable)
        {
            return;
        }

        var clamped = Math.Clamp(width, MinimumColumnWidth, maximumColumnWidth);
        state.Width = $"{clamped.ToString(CultureInfo.InvariantCulture)}px";
        StateHasChanged();
        await NotifyStateChangedAsync();
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender && usesStorage)
        {
            await LoadStoredStateAsync();
        }

        await LoadIfRequestedAsync(firstRender);

        if (ResizableColumns && hasColumns && !resizeAttached)
        {
            resizeAttached = true;
            selfReference ??= DotNetObjectReference.Create(this);
            await InvokeScriptAsync("twDataTable.attachResize", rootRef, selfReference, MinimumColumnWidth, maximumColumnWidth);
        }

        if (pendingFocusId is { } focusId)
        {
            pendingFocusId = null;
            await InvokeScriptAsync("twDataTable.focus", focusId);
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
    /// Detaches the column resize script and releases the reference it held to the table.
    /// </summary>
    /// <returns>A task that completes once the table is cleaned up.</returns>
    public async ValueTask DisposeAsync()
    {
        if (resizeAttached)
        {
            await InvokeScriptAsync("twDataTable.detachResize", rootRef);
        }

        selfReference?.Dispose();
        loadCancellation?.Cancel();
        loadCancellation?.Dispose();
        GC.SuppressFinalize(this);
    }
}
