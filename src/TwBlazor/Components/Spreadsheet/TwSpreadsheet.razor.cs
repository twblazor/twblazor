// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Export;
using TwBlazor.Localization;
using TwBlazor.Models;
using TwBlazor.Spreadsheet;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a spreadsheet: a grid of cells that hold numbers, text and formulas such as <c>=SUM(B2:B6)</c>, with a
/// formula bar, a formatting toolbar and tabs for several sheets, in the manner of Excel.
/// </summary>
/// <remarks>
/// <para>
/// Pass the workbook as <see cref="Sheets"/>: a list of <see cref="Sheet"/>, each holding its cells by address. The
/// spreadsheet edits the list and its cells in place, and reports every change through <see cref="OnCellsChange"/>, so
/// the workbook can be saved as it changes. Formulas are worked out in .NET, with no other package, and can refer to
/// cells on other sheets, as in <c>=Budget!B4</c>. The workbook exports as an Excel file that keeps the formulas and the
/// formatting, or as CSV, through <see cref="ExportFormats"/> or <see cref="TwSpreadsheetExport"/>.
/// </para>
/// <para>
/// The grid follows the ARIA grid pattern: it is a single Tab stop, the arrow keys move the active cell, and
/// <kbd>Shift</kbd> with an arrow key extends the selection. Typing replaces the active cell, <kbd>Enter</kbd> or
/// <kbd>F2</kbd> edits it, and the toolbar, formula bar and sheet tabs are reached with <kbd>Tab</kbd>.
/// </para>
/// </remarks>
public partial class TwSpreadsheet : TwBlazorComponentBase, IAsyncDisposable
{
    /// <summary>
    /// The width of the row headings, in pixels.
    /// </summary>
    private const int rowHeaderWidth = 48;

    /// <summary>
    /// How many rows <kbd>Page Up</kbd> and <kbd>Page Down</kbd> move.
    /// </summary>
    private const int pageRows = 10;

    [Inject] private IJSRuntime jsRuntime { get; set; } = null!;

    private TwSpreadsheetTheme theme => options.Theme.Components.Require<TwSpreadsheetTheme>();

    private TwToggleTheme toggleTheme => options.Theme.Components.Require<TwToggleTheme>();

    /// <summary>
    /// Gets or sets the workbook: the sheets in tab order. An empty list gets one empty sheet.
    /// </summary>
    /// <remarks>The spreadsheet edits the list and the cells in place. After changing them from code, call
    /// <see cref="Refresh"/> so the formulas are worked out again.</remarks>
    [Parameter] public List<Sheet> Sheets { get; set; } = [];

    /// <summary>
    /// Gets or sets the index of the sheet being shown. Defaults to the first.
    /// </summary>
    [Parameter] public int ActiveSheetIndex { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked with the index of the sheet being shown after the user switches sheet.
    /// </summary>
    [Parameter] public EventCallback<int> ActiveSheetIndexChanged { get; set; }

    /// <summary>
    /// Gets or sets the number of rows shown. Defaults to 100.
    /// </summary>
    [Parameter] public int Rows { get; set; } = 100;

    /// <summary>
    /// Gets or sets the number of columns shown. Defaults to 26, <c>A</c> to <c>Z</c>.
    /// </summary>
    [Parameter] public int Columns { get; set; } = 26;

    /// <summary>
    /// Gets or sets the width of every column, in pixels. Defaults to 104.
    /// </summary>
    [Parameter] public int ColumnWidth { get; set; } = 104;

    /// <summary>
    /// Gets or sets the height of the grid's scrolling area, as a CSS length. Defaults to <c>24rem</c>.
    /// </summary>
    [Parameter] public string Height { get; set; } = "24rem";

    /// <summary>
    /// Gets or sets whether the cells can only be read, selected and copied. The toolbar's formatting controls are
    /// disabled, and sheets cannot be added or renamed.
    /// </summary>
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>
    /// Gets or sets whether the formatting toolbar is shown. Defaults to <see langword="true"/>.
    /// </summary>
    /// <remarks>The keyboard shortcuts, such as <kbd>Ctrl</kbd> + <kbd>B</kbd>, work without it.</remarks>
    [Parameter] public bool ShowToolbar { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the formula bar, with the cell reference and the active cell's contents, is shown.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter] public bool ShowFormulaBar { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the sheet tabs are shown under the grid. Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter] public bool ShowSheetTabs { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the user can add sheets with the button after the tabs. Defaults to
    /// <see langword="true"/>.
    /// </summary>
    [Parameter] public bool AllowAddSheet { get; set; } = true;

    /// <summary>
    /// Gets or sets the formats offered by an export menu at the end of the toolbar, in the order listed. Empty (the
    /// default) shows no export menu.
    /// </summary>
    [Parameter] public IReadOnlyList<SpreadsheetExportFormat> ExportFormats { get; set; } = [];

    /// <summary>
    /// Gets or sets the name exported files are saved with, without the extension. Defaults to <c>workbook</c>.
    /// </summary>
    [Parameter] public string ExportFileName { get; set; } = "workbook";

    /// <summary>
    /// Gets or sets the callback invoked after cells changed, through typing, pasting, clearing, formatting, undo or
    /// redo.
    /// </summary>
    [Parameter] public EventCallback<SheetChangeEventArgs> OnCellsChange { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the active cell or the selected range changes.
    /// </summary>
    [Parameter] public EventCallback<SheetSelectionEventArgs> OnSelectionChange { get; set; }

    private SheetCalculator? calculator;

    private ElementReference gridRef;

    private DotNetObjectReference<TwSpreadsheet>? dotNetRef;

    private bool attached;

    private int activeSheet;

    private CellAddress anchor;

    private CellAddress extent;

    private CellAddress rangeStart;

    private string statusMessage = string.Empty;

    private bool scrollToActive;

    private bool focusGrid;

    private string? focusElementId;

    private List<FormatOption> formatOptions = [];

    private string? nameBoxText;

    private readonly Dictionary<int, string> _cellClassCache = [];

    private TwSpreadsheetTheme? cachedTheme;

    /// <summary>
    /// A number format in the toolbar's list, shown by its localized name.
    /// </summary>
    private sealed record FormatOption(SheetNumberFormat Format, string Name)
    {
        /// <inheritdoc />
        public override string ToString() => Name;
    }

    /// <summary>
    /// Gets the sheet being shown.
    /// </summary>
    public Sheet ActiveSheet => Sheets[activeSheet];

    /// <summary>
    /// Gets the address of the active cell, the one typing goes into, such as <c>B3</c>.
    /// </summary>
    public string ActiveCell => anchor.ToString();

    /// <summary>
    /// Gets the selected range, such as <c>B3:D8</c>, or the active cell's address when only one cell is selected.
    /// </summary>
    public string SelectedRange => selection.ToString();

    private CellRange selection => new(rangeStart, extent);

    private bool canEdit => !ReadOnly;

    private string panelId => $"{Id}-panel";

    private string hintId => $"{Id}-hint";

    private string formulaInputId => $"{Id}-formula";

    private string editorId => $"{Id}-editor";

    private string exportMenuId => $"{Id}-export";

    private string TabId(int index) => $"{Id}-tab-{index}";

    private string CellId(CellAddress address) => $"{Id}-{address}";

    private CultureInfo culture => localizer.Formats.Culture;

    private string gridLabel => AriaLabel ?? localizer[TwStrings.Spreadsheet_Label];

    private static string rowHeaderColStyle => string.Create(CultureInfo.InvariantCulture, $"width:{rowHeaderWidth}px");


    private string rootClasses => new ClassBuilder(theme.Root)
        .AddClass(Class)
        .AddClass(theme.FullScreen, FullScreen)
        .Build();

    private string viewportClasses => new ClassBuilder(theme.Viewport)
        .AddClass(theme.ViewportFullScreen, FullScreen)
        .Build();

    /// <summary>
    /// A toolbar button drawn the same way as the toolbar's toggles while they are not pressed.
    /// </summary>
    private string ToolbarButtonClasses(bool disabled) => new ClassBuilder(toggleTheme.Base)
        .AddClass(toggleTheme.Sm)
        .AddClass(roundedBuilder.GetRounded(Rounded ?? toggleTheme.ToggleRounded ?? options.Theme.Rounded.DefaultRounded))
        .AddClass(toggleTheme.Unpressed)
        .AddClass(toggleTheme.Hover, !disabled)
        .AddClass(toggleTheme.Disabled, disabled)
        .AddClass(colorBuilder.GetFocusRing(null))
        .Build();

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        Sheets ??= [];
        if (Sheets.Count == 0)
        {
            Sheets.Add(new Sheet { Name = localizer[TwStrings.Spreadsheet_DefaultSheetName, 1] });
        }

        Rows = Math.Clamp(Rows, 1, CellAddress.MaxRows);
        Columns = Math.Clamp(Columns, 1, CellAddress.MaxColumns);
        ColumnWidth = Math.Clamp(ColumnWidth, minColumnWidth, maxColumnWidth);
        Zoom = Math.Clamp(Zoom, 25, 400);

        calculator ??= new SheetCalculator(() => Sheets);
        Recalculate();

        var requested = Math.Clamp(ActiveSheetIndex, 0, Sheets.Count - 1);
        if (requested != activeSheet)
        {
            CancelEdit();
            activeSheet = requested;
        }

        anchor = Clamp(anchor);
        extent = Clamp(extent);
        rangeStart = Clamp(rangeStart);

        formatOptions =
        [
            new(SheetNumberFormat.Number, localizer[TwStrings.Spreadsheet_FormatNumber]),
            new(SheetNumberFormat.Currency, localizer[TwStrings.Spreadsheet_FormatCurrency]),
            new(SheetNumberFormat.Percent, localizer[TwStrings.Spreadsheet_FormatPercent]),
        ];

        BuildFormattingOptions();

        if (ReadOnly)
        {
            CancelEdit();
            renamingSheet = null;
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        try
        {
            if (firstRender)
            {
                dotNetRef = DotNetObjectReference.Create(this);
                await jsRuntime.InvokeVoidAsync("twSpreadsheet.attach", gridRef, dotNetRef);
                attached = true;
            }

            await SyncFullScreenAsync();
            await SyncToolbarAsync();
            await OpenPendingDialogAsync();
            await RunPendingFocusAsync();
        }
        catch (JSDisconnectedException)
        {
            // The circuit disconnected before the script could run; nothing to set up.
        }
    }

    private async Task RunPendingFocusAsync()
    {
        if (focusEditor)
        {
            focusEditor = false;
            await jsRuntime.InvokeVoidAsync("twSpreadsheet.focusEditor", gridRef, editorTarget, takeTyped);
            takeTyped = false;
        }
        else if (focusGrid)
        {
            focusGrid = false;
            await jsRuntime.InvokeVoidAsync("twSpreadsheet.focus", gridRef);
        }
        else if (focusElementId is not null)
        {
            var id = focusElementId;
            focusElementId = null;
            await jsRuntime.InvokeVoidAsync("twFocus.focusById", id);
        }

        if (scrollToActive)
        {
            scrollToActive = false;
            await jsRuntime.InvokeVoidAsync("twSpreadsheet.scrollToCell", CellId(anchor));
        }
    }

    #region Public methods

    /// <summary>
    /// Moves focus to the grid, on the active cell.
    /// </summary>
    /// <returns>A task that completes once focus has moved.</returns>
    public async Task FocusAsync() => await jsRuntime.InvokeVoidAsync("twSpreadsheet.focus", gridRef);

    /// <summary>
    /// Works the formulas out again and redraws the grid, after the cells or the sheets were changed from code.
    /// </summary>
    public void Refresh()
    {
        Recalculate();
        StateHasChanged();
    }

    /// <summary>
    /// Selects a cell or a range on the sheet being shown and scrolls it into view.
    /// </summary>
    /// <param name="range">A cell, such as <c>B3</c>, or a range, such as <c>B3:D8</c>, whose first corner becomes the
    /// active cell.</param>
    /// <returns><see langword="true"/> when the address was valid.</returns>
    public async Task<bool> SelectAsync(string range)
    {
        var parts = (range ?? string.Empty).Split(':', StringSplitOptions.TrimEntries);
        if (parts.Length is < 1 or > 2 || !CellAddress.TryParse(parts[0], out var first))
        {
            return false;
        }

        var second = first;
        if (parts.Length == 2 && !CellAddress.TryParse(parts[1], out second))
        {
            return false;
        }

        await CommitEditAsync();
        await SetSelectionAsync(first, second);
        return true;
    }

    /// <summary>
    /// Gets the text a cell shows: the result of its formula, formatted with its number format.
    /// </summary>
    /// <param name="address">The cell, such as <c>B3</c>.</param>
    /// <param name="sheetIndex">The index of the sheet, or <see langword="null"/> for the sheet being shown.</param>
    /// <returns>The text, or an empty string for an empty cell or an address that is not valid.</returns>
    public string GetDisplayText(string address, int? sheetIndex = null)
    {
        var index = sheetIndex ?? activeSheet;
        if (!CellAddress.TryParse(address, out var cell) || index < 0 || index >= Sheets.Count || calculator is null)
        {
            return string.Empty;
        }

        var format = CellAt(index, cell)?.Format ?? SheetNumberFormat.General;
        return SheetFormatting.Display(calculator.GetValue(index, cell), format, culture);
    }

    /// <summary>
    /// Sets what a cell holds, as if the user had typed it, so it can be undone and is reported through
    /// <see cref="OnCellsChange"/>.
    /// </summary>
    /// <param name="address">The cell, such as <c>B3</c>.</param>
    /// <param name="value">A number, text or a formula starting with <c>=</c>, or <see langword="null"/> to empty the
    /// cell.</param>
    /// <param name="sheetIndex">The index of the sheet, or <see langword="null"/> for the sheet being shown.</param>
    /// <returns><see langword="true"/> when the address and sheet were valid.</returns>
    public async Task<bool> SetCellAsync(string address, string? value, int? sheetIndex = null)
    {
        var index = sheetIndex ?? activeSheet;
        if (!CellAddress.TryParse(address, out var cell) || index < 0 || index >= Sheets.Count)
        {
            return false;
        }

        await ApplyAsync(index, [Change(index, cell, c => c.Value = value)]);
        return true;
    }

    /// <summary>
    /// Exports the workbook as a file and has the browser save it. The export is announced to screen readers.
    /// </summary>
    /// <param name="format">The file format. CSV holds the sheet being shown.</param>
    /// <param name="fileName">The file name without its extension, or <see langword="null"/> to use
    /// <see cref="ExportFileName"/>.</param>
    /// <returns>The exported file, for example to send it somewhere else as well.</returns>
    public async Task<ExportFile> ExportAsync(SpreadsheetExportFormat format, string? fileName = null)
    {
        await CommitEditAsync();
        var file = TwSpreadsheetExport.Export(Sheets, format, fileName ?? ExportFileName, activeSheet, culture);

        if (await ExportDownload.SaveAsync(jsRuntime, file))
        {
            statusMessage = localizer[TwStrings.TextEditor_Exported, file.FileName];
            StateHasChanged();
        }

        return file;
    }

    #endregion

    /// <summary>
    /// Selects the cell or range typed into the cell reference box when <kbd>Enter</kbd> is pressed, and moves focus to
    /// the grid; <kbd>Escape</kbd> goes back to the grid without moving.
    /// </summary>
    private async Task OnNameBoxKeyDownAsync(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await SelectAsync(nameBoxText ?? string.Empty);
            nameBoxText = null;
            focusGrid = true;
        }
        else if (e.Key == "Escape")
        {
            nameBoxText = null;
            focusGrid = true;
        }
    }

    #region Rendering helpers

    private SheetCell? CellAt(int sheet, CellAddress address) =>
        Sheets[sheet].Cells.TryGetValue(address.ToString(), out var cell) ? cell : null;

    private CellAddress Clamp(CellAddress address) =>
        new(Math.Clamp(address.Row, 0, Rows - 1), Math.Clamp(address.Column, 0, Columns - 1));

    /// <summary>
    /// What a cell shows and how, worked out once per cell per render.
    /// </summary>
    private readonly record struct CellView(string Text, string Classes, string? Style, bool Selected, bool Active, bool TableHeader = false);

    private CellView ViewOf(CellAddress address, CellRange range)
    {
        var selected = range.Contains(address);
        var active = address == anchor;
        var cell = CellAt(activeSheet, address);

        var frozen = Frozen(address);
        var table = TableRole(address);
        if (cell is null)
        {
            return new CellView(string.Empty, CellClasses(selected, active, SheetCellAlignment.Start, null, SheetValueKind.Empty, frozen, table), CellStyle(address, null), selected, active, table.Header);
        }

        var value = calculator!.GetValue(activeSheet, address);
        var alignment = cell.Alignment == SheetCellAlignment.General ? SheetFormatting.GeneralAlignment(value) : cell.Alignment;
        var text = SheetFormatting.Display(value, cell.Format, culture);
        return new CellView(text, CellClasses(selected, active, alignment, cell, value.Kind, frozen, table), CellStyle(address, cell), selected, active, table.Header);
    }

    /// <summary>
    /// Builds a cell's classes, kept per combination of states, since a grid has thousands of cells and only a few
    /// combinations.
    /// </summary>
    private (bool Cell, bool RowEdge, bool ColumnEdge) Frozen(CellAddress address) =>
        (address.Row < frozenRows || address.Column < frozenColumns, address.Row == frozenRows - 1, address.Column == frozenColumns - 1);

    private string CellClasses(bool selected, bool active, SheetCellAlignment alignment, SheetCell? cell, SheetValueKind kind, (bool Cell, bool RowEdge, bool ColumnEdge) frozen, (bool Header, bool Band) table)
    {
        if (!ReferenceEquals(cachedTheme, theme))
        {
            _cellClassCache.Clear();
            cachedTheme = theme;
        }

        var bold = cell?.Bold == true;
        var italic = cell?.Italic == true;
        var underline = cell?.Underline == true;
        var key = (selected ? 1 : 0) | (active ? 2 : 0) | (bold ? 4 : 0) | (italic ? 8 : 0) | (underline ? 16 : 0)
            | ((int)alignment << 5) | ((int)kind << 8) | (frozen.Cell ? 1 << 11 : 0) | (frozen.RowEdge ? 1 << 12 : 0) | (frozen.ColumnEdge ? 1 << 13 : 0) | (table.Header ? 1 << 14 : 0) | (table.Band ? 1 << 15 : 0);

        if (!_cellClassCache.TryGetValue(key, out var classes))
        {
            classes = new ClassBuilder(theme.Cell)
                .AddClass(theme.CellSelected, selected)
                .AddClass(theme.CellActive, active)
                .AddClass(alignment switch
                {
                    SheetCellAlignment.Center => theme.AlignCenter,
                    SheetCellAlignment.End => theme.AlignEnd,
                    _ => theme.AlignStart,
                })
                .AddClass(theme.CellNumber, kind == SheetValueKind.Number)
                .AddClass(theme.CellError, kind == SheetValueKind.Error)
                .AddClass(theme.Bold, bold)
                .AddClass(theme.Italic, italic)
                .AddClass(theme.Underline, underline)
                .AddClass(theme.CellFrozen, frozen.Cell)
                .AddClass(theme.FrozenRowEdge, frozen.RowEdge)
                .AddClass(theme.FrozenColumnEdge, frozen.ColumnEdge)
                .AddClass(theme.TableBand, table.Band)
                .AddClass(theme.TableHeader, table.Header)
                .Build();
            _cellClassCache[key] = classes;
        }

        return classes;
    }

    private string ColumnHeaderClasses(int column, CellRange range) => new ClassBuilder(theme.ColumnHeader)
        .AddClass(theme.HeaderSelected, column >= range.Start.Column && column <= range.End.Column)
        .AddClass(theme.FrozenColumnEdge, column == frozenColumns - 1)
        .Build();

    private string RowHeaderClasses(int row, CellRange range) => new ClassBuilder(theme.RowHeader)
        .AddClass(theme.HeaderSelected, row >= range.Start.Row && row <= range.End.Row)
        .AddClass(theme.FrozenRowEdge, row == frozenRows - 1)
        .Build();

    private string SheetTabClasses(int index) => new ClassBuilder(theme.SheetTab)
        .AddClass(theme.SheetTabActive, index == activeSheet)
        .Build();

    private string formulaBarText => editing ? editText : CellAt(activeSheet, anchor)?.Value ?? string.Empty;

    private string WithShortcut(string labelKey, string shortcut) => $"{localizer[labelKey]} ({shortcut})";

    private static Dictionary<string, object> KeyShortcuts(string shortcut) => new() { ["aria-keyshortcuts"] = shortcut };

    private string ExportLabel(SpreadsheetExportFormat format)
    {
        var name = format == SpreadsheetExportFormat.Excel ? localizer[TwStrings.Spreadsheet_ExportExcel] : localizer[TwStrings.Spreadsheet_ExportCsv];
        return $"{name} ({TwSpreadsheetExport.ExtensionOf(format)})";
    }

    private static Icon ExportIcon(SpreadsheetExportFormat format) => format == SpreadsheetExportFormat.Excel ? Icon.File_Earmark_Excel : Icon.Filetype_Csv;

    #endregion

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        try
        {
            if (attached)
            {
                if (FullScreen)
                {
                    await jsRuntime.InvokeVoidAsync("twSpreadsheet.setFullScreen", gridRef, false);
                }

                await jsRuntime.InvokeVoidAsync("twSpreadsheet.detach", gridRef);
            }
        }
        catch (JSDisconnectedException)
        {
            // The circuit is already gone; nothing left to clean up.
        }
        catch (InvalidOperationException)
        {
            // JS interop unavailable during teardown (e.g. prerendering); safe to ignore.
        }

        dotNetRef?.Dispose();
        GC.SuppressFinalize(this);
    }
}
