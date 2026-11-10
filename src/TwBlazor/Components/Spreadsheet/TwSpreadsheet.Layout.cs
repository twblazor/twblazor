// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using System.Text;
using TwBlazor.Localization;
using TwBlazor.Models;
using TwBlazor.Spreadsheet;
using TwBlazor.Utilities;
using TwBlazor.Extensions;

namespace TwBlazor.Components;

/// <summary>
/// How the grid is laid out: zoom, frozen panes, column widths, each cell's own style and the selection summary.
/// </summary>
public partial class TwSpreadsheet
{
    /// <summary>
    /// The height of every row and of the column headings, in pixels, matching the theme's <c>h-8</c>.
    /// </summary>
    private const int rowHeight = 32;

    /// <summary>
    /// How much <see cref="WidenColumnAsync"/> and <see cref="NarrowColumnAsync"/> change a column, in pixels.
    /// </summary>
    private const int widthStep = 16;

    private const int minColumnWidth = 24;

    private const int maxColumnWidth = 1000;

    private static readonly int[] _zoomSteps = [50, 75, 90, 100, 110, 125, 150, 175, 200];

    /// <summary>
    /// Gets or sets the zoom level of the grid, in percent, between 25 and 400. Defaults to 100.
    /// </summary>
    [Parameter] public int Zoom { get; set; } = 100;

    /// <summary>
    /// Gets or sets the callback invoked with the new zoom level after the user zooms.
    /// </summary>
    [Parameter] public EventCallback<int> ZoomChanged { get; set; }

    /// <summary>
    /// Gets or sets whether the zoom controls and the summary of the selected numbers are shown under the grid.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter] public bool ShowStatusBar { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the spreadsheet covers the whole window. Bind it with <c>@bind-FullScreen</c> to know when
    /// the user toggles it.
    /// </summary>
    /// <remarks>The page behind stops scrolling while it is on, and <kbd>Escape</kbd> turns it off.</remarks>
    [Parameter] public bool FullScreen { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the user turns full screen mode on or off.
    /// </summary>
    [Parameter] public EventCallback<bool> FullScreenChanged { get; set; }

    /// <summary>
    /// Gets or sets whether the toolbar has a button that turns full screen mode on and off. Defaults to
    /// <see langword="true"/>.
    /// </summary>
    [Parameter] public bool AllowFullScreen { get; set; } = true;

    private bool? appliedFullScreen;

    private bool toolbarExpanded;

    private bool toolbarObserved;

    private double zoomFactor => Zoom / 100d;

    private string toolbarItemsId => $"{Id}-toolbar-items";

    private string toolbarItemsClasses => new ClassBuilder(theme.ToolbarItems)
        .AddClass(theme.ToolbarItemsExpanded, toolbarExpanded)
        .Build();

    /// <summary>
    /// The More tools button, drawn like the toolbar's toggles and shown pressed while the toolbar is expanded.
    /// </summary>
    private string toolbarMoreButtonClasses => new ClassBuilder(toggleTheme.Base)
        .AddClass(toggleTheme.Sm)
        .AddClass(roundedBuilder.GetRounded(Rounded ?? toggleTheme.ToggleRounded ?? options.Theme.Rounded.DefaultRounded))
        .AddClass(toolbarExpanded ? toggleTheme.Pressed : toggleTheme.Unpressed)
        .AddClass(toggleTheme.PressedForcedColors, toolbarExpanded)
        .AddClass(toggleTheme.Hover, !toolbarExpanded)
        .AddClass(colorBuilder.GetFocusRing(null))
        .Build();

    private Dictionary<string, object> toolbarMoreAttributes => new()
    {
        ["aria-expanded"] = toolbarExpanded ? "true" : "false",
        ["aria-controls"] = toolbarItemsId,
    };

    /// <summary>
    /// Shows every toolbar control on as many lines as they need, or puts them back on one line.
    /// </summary>
    private void ToggleToolbar() => toolbarExpanded = !toolbarExpanded;

    /// <summary>
    /// Starts watching the toolbar once it is shown, so the More tools button only appears when the controls do not
    /// fit on one line.
    /// </summary>
    private async Task SyncToolbarAsync()
    {
        if (!ShowToolbar)
        {
            toolbarObserved = false;
            return;
        }

        if (toolbarObserved)
        {
            return;
        }

        toolbarObserved = true;
        await jsRuntime.TryInvokeVoidAsync("twSpreadsheet.observeToolbar", gridRef);
    }

    /// <summary>
    /// Turns full screen mode on or off, announces it and reports it through <see cref="FullScreenChanged"/>.
    /// </summary>
    /// <param name="on">Whether the spreadsheet should cover the window.</param>
    /// <returns>A task that completes once the mode has changed.</returns>
    public async Task SetFullScreenAsync(bool on)
    {
        if (on == FullScreen)
        {
            return;
        }

        FullScreen = on;
        statusMessage = localizer[on ? TwStrings.Spreadsheet_FullScreenOn : TwStrings.Spreadsheet_FullScreenOff];
        scrollToActive = true;
        // Also reached from script through ExitFullScreenAsync and from app code, where nothing renders for us.
#pragma warning disable BL0012
        StateHasChanged();
#pragma warning restore BL0012
        await FullScreenChanged.InvokeAsync(on);
    }

    /// <summary>
    /// Leaves full screen mode, called by <c>twSpreadsheet.attach</c> when <kbd>Escape</kbd> is pressed outside the
    /// grid, such as on the toolbar.
    /// </summary>
    /// <returns>A task that completes once full screen mode is off.</returns>
    [JSInvokable("ExitFullScreen")]
    public Task ExitFullScreenAsync() => SetFullScreenAsync(false);

    /// <summary>
    /// Stops the page behind from scrolling while in full screen mode, and lets it scroll again afterwards.
    /// </summary>
    private async Task SyncFullScreenAsync()
    {
        if (appliedFullScreen == FullScreen || (appliedFullScreen is null && !FullScreen))
        {
            appliedFullScreen = FullScreen;
            return;
        }

        appliedFullScreen = FullScreen;
        await jsRuntime.TryInvokeVoidAsync("twSpreadsheet.setFullScreen", gridRef, FullScreen);
    }

    private Sheet sheet => Sheets[activeSheet];

    private int frozenRows => Math.Clamp(sheet.FrozenRows, 0, Rows - 1);

    private int frozenColumns => Math.Clamp(sheet.FrozenColumns, 0, Columns - 1);

    #region Public methods

    /// <summary>
    /// Keeps rows at the top and columns at the start of the sheet being shown in view while the rest scrolls.
    /// </summary>
    /// <param name="rows">The number of rows to freeze, or 0 for none.</param>
    /// <param name="columns">The number of columns to freeze, or 0 for none.</param>
    public void FreezePanes(int rows, int columns)
    {
        sheet.FrozenRows = Math.Clamp(rows, 0, Rows - 1);
        sheet.FrozenColumns = Math.Clamp(columns, 0, Columns - 1);
        statusMessage = localizer[TwStrings.Spreadsheet_FrozenStatus, sheet.FrozenRows, sheet.FrozenColumns];
        StateHasChanged();
    }

    /// <summary>
    /// Sets the zoom level and reports it through <see cref="ZoomChanged"/>.
    /// </summary>
    /// <param name="zoom">The zoom level in percent, kept between 25 and 400.</param>
    /// <returns>A task that completes once the zoom has changed.</returns>
    public async Task SetZoomAsync(int zoom)
    {
        var clamped = Math.Clamp(zoom, 25, 400);
        if (clamped == Zoom)
        {
            return;
        }

        Zoom = clamped;
        statusMessage = localizer[TwStrings.Spreadsheet_ZoomChanged, Zoom];
        scrollToActive = true;
        StateHasChanged();
        await ZoomChanged.InvokeAsync(Zoom);
    }

    /// <summary>
    /// Sets the width of a column on the sheet being shown.
    /// </summary>
    /// <param name="column">The column's letters, such as <c>B</c>.</param>
    /// <param name="width">The width in pixels, kept between 24 and 1000.</param>
    /// <returns><see langword="true"/> when the column was valid.</returns>
    public bool SetColumnWidth(string column, int width)
    {
        if (!CellAddress.TryParse($"{column}1", out var address) || address.Column >= Columns)
        {
            return false;
        }

        WriteWidth(address.Column, width);
        StateHasChanged();
        return true;
    }

    #endregion

    private Task ZoomOutAsync() => SetZoomAsync(_zoomSteps.LastOrDefault(z => z < Zoom, Math.Max(25, Zoom - 25)));

    private Task ZoomInAsync() => SetZoomAsync(_zoomSteps.FirstOrDefault(z => z > Zoom, Math.Min(400, Zoom + 25)));

    private void FreezeAtActiveCell() => FreezePanes(anchor.Row, anchor.Column);

    private int WidthOf(int column) =>
        sheet.ColumnWidths.TryGetValue(CellAddress.ColumnName(column), out var width) ? Math.Clamp(width, minColumnWidth, maxColumnWidth) : ColumnWidth;

    private void WriteWidth(int column, int width)
    {
        var letters = CellAddress.ColumnName(column);
        var clamped = Math.Clamp(width, minColumnWidth, maxColumnWidth);
        if (clamped == ColumnWidth)
        {
            sheet.ColumnWidths.Remove(letters);
        }
        else
        {
            sheet.ColumnWidths[letters] = clamped;
        }
    }

    private int ColumnOffset(int column)
    {
        var offset = rowHeaderWidth;
        for (var c = 0; c < column; c++)
        {
            offset += WidthOf(c);
        }

        return offset;
    }

    private int totalWidth => ColumnOffset(Columns);

    private const int minRowHeight = 16;

    private const int maxRowHeight = 400;

    private const int heightStep = 8;

    private HashSet<int>? hiddenRowsCache;

    /// <summary>
    /// Works the formulas out again and forgets which rows the table filters hide, after anything changed.
    /// </summary>
    private void Recalculate()
    {
        calculator?.Invalidate();
        hiddenRowsCache = null;
    }

    /// <summary>
    /// Gets the zero based rows of the sheet being shown that a table filter hides.
    /// </summary>
    private HashSet<int> hiddenRows => hiddenRowsCache ??= HiddenRowsOf(activeSheet);

    private bool IsHidden(int row) => hiddenRows.Contains(row);

    private int HeightOf(int row) =>
        sheet.RowHeights.TryGetValue(row + 1, out var height) ? Math.Clamp(height, minRowHeight, maxRowHeight) : rowHeight;

    private void WriteHeight(int row, int height)
    {
        var clamped = Math.Clamp(height, minRowHeight, maxRowHeight);
        if (clamped == rowHeight)
        {
            sheet.RowHeights.Remove(row + 1);
        }
        else
        {
            sheet.RowHeights[row + 1] = clamped;
        }
    }

    /// <summary>
    /// The distance from the top of the grid to the top of a row: the column headings, then every row above that a
    /// filter does not hide.
    /// </summary>
    private int RowOffset(int row)
    {
        var offset = rowHeight;
        for (var r = 0; r < row; r++)
        {
            if (!IsHidden(r))
            {
                offset += HeightOf(r);
            }
        }

        return offset;
    }

    private string RowStyle(int row) => string.Create(CultureInfo.InvariantCulture, $"height:{HeightOf(row)}px");

    /// <summary>
    /// Sets the height of a row on the sheet being shown.
    /// </summary>
    /// <param name="row">The row number, starting at 1.</param>
    /// <param name="height">The height in pixels, kept between 16 and 400.</param>
    /// <returns><see langword="true"/> when the row was valid.</returns>
    public bool SetRowHeight(int row, int height)
    {
        if (row < 1 || row > Rows)
        {
            return false;
        }

        WriteHeight(row - 1, height);
        StateHasChanged();
        return true;
    }

    /// <summary>
    /// Sets a row's height after the user dragged its heading's edge, called by <c>twSpreadsheet.attach</c>.
    /// </summary>
    /// <param name="row">The zero based row.</param>
    /// <param name="height">The new height in pixels.</param>
    [JSInvokable("ResizeRow")]
    public void ResizeRow(int row, int height) => SetRowHeight(row + 1, height);

    /// <summary>
    /// Makes the selected columns as wide as their widest content, as a double-click on a column edge does.
    /// </summary>
    private async Task FitColumnsAsync()
    {
        for (var column = selection.Start.Column; column <= selection.End.Column; column++)
        {
            var width = await jsRuntime.TryInvokeAsync<int>("twSpreadsheet.fitColumn", 0, gridRef, column);
            if (width > 0)
            {
                WriteWidth(column, width);
            }
        }
    }

    /// <summary>
    /// Makes the selected rows as tall as their tallest text, but never shorter than the standard height, as a
    /// double-click on a row edge does.
    /// </summary>
    private async Task FitRowsAsync()
    {
        for (var row = selection.Start.Row; row <= selection.End.Row; row++)
        {
            var height = await jsRuntime.TryInvokeAsync<int>("twSpreadsheet.fitRow", 0, gridRef, row);
            if (height > 0)
            {
                WriteHeight(row, height);
            }
        }
    }

    private Task IncreaseRowHeightAsync() => ResizeRowsBy(heightStep);

    private Task DecreaseRowHeightAsync() => ResizeRowsBy(-heightStep);

    private Task ResizeRowsBy(int delta)
    {
        for (var row = selection.Start.Row; row <= selection.End.Row; row++)
        {
            WriteHeight(row, HeightOf(row) + delta);
        }

        StateHasChanged();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Makes the selected rows tall enough for a text size, so larger text is not cut off. Rows are never made
    /// shorter.
    /// </summary>
    /// <param name="points">The text size in points.</param>
    private void FitRowsToFont(double points)
    {
        var needed = (int)Math.Ceiling(points * 4 / 3 * 1.25) + 8;
        for (var row = selection.Start.Row; row <= selection.End.Row; row++)
        {
            if (needed > HeightOf(row))
            {
                WriteHeight(row, needed);
            }
        }
    }

    private Task WidenColumnAsync() => ResizeColumnsBy(widthStep);

    private Task NarrowColumnAsync() => ResizeColumnsBy(-widthStep);

    private Task ResizeColumnsBy(int delta)
    {
        for (var column = selection.Start.Column; column <= selection.End.Column; column++)
        {
            WriteWidth(column, WidthOf(column) + delta);
        }

        StateHasChanged();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets a column's width after the user dragged its heading's edge, called by <c>twSpreadsheet.attach</c>.
    /// </summary>
    /// <param name="column">The zero based column.</param>
    /// <param name="width">The new width in pixels.</param>
    [JSInvokable("ResizeColumn")]
    public void ResizeColumn(int column, int width)
    {
        if (column < 0 || column >= Columns)
        {
            return;
        }

        WriteWidth(column, width);
        StateHasChanged();
    }

    private string gridStyle => string.Create(CultureInfo.InvariantCulture, $"width:{totalWidth}px;zoom:{zoomFactor}");

    private string viewportStyle
    {
        get
        {
            var top = RowOffset(frozenRows) * zoomFactor;
            var start = ColumnOffset(frozenColumns) * zoomFactor;
            var height = FullScreen ? "auto" : Height;
            return string.Create(CultureInfo.InvariantCulture, $"height:{height};scroll-padding-top:{top:0.##}px;scroll-padding-inline-start:{start:0.##}px");
        }
    }

    private string ColumnColStyle(int column) => string.Create(CultureInfo.InvariantCulture, $"width:{WidthOf(column)}px");

    /// <summary>
    /// The sticky offsets of a cell or heading in the frozen panes, or <see langword="null"/> when it scrolls freely.
    /// </summary>
    private string? FrozenStyle(int row, int column)
    {
        var frozenRow = row >= 0 && row < frozenRows;
        var frozenColumn = column >= 0 && column < frozenColumns;
        if (!frozenRow && !frozenColumn)
        {
            return null;
        }

        var style = new StringBuilder("position:sticky;");
        if (frozenRow)
        {
            style.Append(CultureInfo.InvariantCulture, $"top:{RowOffset(row)}px;");
        }

        if (frozenColumn)
        {
            style.Append(CultureInfo.InvariantCulture, $"inset-inline-start:{ColumnOffset(column)}px;");
        }

        // Frozen cells sit over the cells scrolling under them, and the corner where both meet sits over both.
        style.Append(CultureInfo.InvariantCulture, $"z-index:{(frozenRow && frozenColumn ? 4 : 3)}");
        return style.ToString();
    }

    private string? ColumnHeaderStyle(int column) =>
        column < frozenColumns ? string.Create(CultureInfo.InvariantCulture, $"inset-inline-start:{ColumnOffset(column)}px;z-index:15") : null;

    private string? RowHeaderStyle(int row) =>
        row < frozenRows ? string.Create(CultureInfo.InvariantCulture, $"top:{RowOffset(row)}px;z-index:15") : null;

    /// <summary>
    /// A cell's inline style: its sticky offsets in the frozen panes, and its own fill, text colour, font and size,
    /// each checked before it is written so a stored value cannot break out of the style.
    /// </summary>
    private string? CellStyle(CellAddress address, SheetCell? cell)
    {
        var frozen = FrozenStyle(address.Row, address.Column);
        if (cell is null)
        {
            return frozen;
        }

        var style = new StringBuilder(frozen ?? string.Empty);
        void Add(string part)
        {
            if (style.Length > 0)
            {
                style.Append(';');
            }

            style.Append(part);
        }

        var background = SheetFormatting.SafeColor(cell.Background);
        if (background is not null)
        {
            Add($"background-color:{background}");
        }

        if (SheetFormatting.SafeColor(cell.Color) is { } color)
        {
            Add($"color:{color}");
        }
        else if (background is not null)
        {
            Add($"color:{ReadableTextOn(background)}");
        }

        if (SheetFormatting.SafeFontFamily(cell.FontFamily) is { } family)
        {
            Add($"font-family:{family}");
        }

        if (SheetFormatting.SafeFontSize(cell.FontSize) is { } size)
        {
            Add(string.Create(CultureInfo.InvariantCulture, $"font-size:{size}pt"));
        }

        return style.Length == 0 ? null : style.ToString();
    }

    /// <summary>
    /// The sum, average and count shown under the grid while more than one cell is selected, or
    /// <see langword="null"/> for a single cell. Very large selections are not summed, to keep moving around quick.
    /// </summary>
    private (string? Sum, string? Average, string Count)? selectionSummary
    {
        get
        {
            var range = selection;
            if (range.IsSingleCell || calculator is null || (long)range.RowCount * range.ColumnCount > 10_000)
            {
                return null;
            }

            var count = 0;
            var numbers = new List<double>();
            foreach (var pair in sheet.Cells)
            {
                if (!CellAddress.TryParse(pair.Key, out var address) || !range.Contains(address) || string.IsNullOrEmpty(pair.Value?.Value))
                {
                    continue;
                }

                count++;
                var value = calculator.GetValue(activeSheet, address);
                if (value.Kind == SheetValueKind.Number)
                {
                    numbers.Add(value.Number);
                }
            }

            if (count == 0)
            {
                return null;
            }

            var countText = localizer[TwStrings.Spreadsheet_StatusCount, count.ToString("N0", culture)];
            if (numbers.Count == 0)
            {
                return (null, null, countText);
            }

            return (
                localizer[TwStrings.Spreadsheet_StatusSum, numbers.Sum().ToString("G10", culture)],
                localizer[TwStrings.Spreadsheet_StatusAverage, numbers.Average().ToString("G10", culture)],
                countText);
        }
    }
}
