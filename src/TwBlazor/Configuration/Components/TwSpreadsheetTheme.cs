// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for the spreadsheet (<see cref="TwBlazor.Components.TwSpreadsheet"/>).
/// Override any property to customize spreadsheet styles globally.
/// </summary>
/// <remarks>
/// The toolbar is made of other components with their own themes: <see cref="TwToggleTheme"/> for the toggles, toggle
/// groups and toolbar buttons, <see cref="TwInputTheme"/> for the number format list and <see cref="TwSeparatorTheme"/>
/// for the lines between groups. This theme holds the layout of the spreadsheet and the look of its grid.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwSpreadsheetTheme
{
    /// <summary>
    /// Gets or sets the classes for the box around the toolbar, the formula bar, the grid and the sheet tabs.
    /// </summary>
    public required string Root { get; set; }

    /// <summary>
    /// Gets or sets the classes for the toolbar: a row at the top that holds the controls (<see cref="ToolbarItems"/>)
    /// and, at its end, the More tools button (<see cref="ToolbarMore"/>). It has the <c>data-expanded</c> attribute
    /// while the user has expanded it, and the <c>data-overflowing</c> attribute while its controls do not fit on one
    /// line, so give it a named group (such as <c>group/sheettoolbar</c>) for <see cref="ToolbarMore"/> to use.
    /// </summary>
    public required string Toolbar { get; set; }

    /// <summary>
    /// Gets or sets the classes for the box holding the toolbar's controls. By default it stays on one line and scrolls
    /// sideways, without a scrollbar, when the controls do not fit.
    /// </summary>
    public string ToolbarItems { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes added to <see cref="ToolbarItems"/> while the user has expanded the toolbar with the
    /// More tools button, such as letting the controls wrap onto more lines.
    /// </summary>
    public string ToolbarItemsExpanded { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the box around the More tools button at the end of the toolbar. By default it is
    /// hidden until the controls do not fit on one line, or while the toolbar is expanded.
    /// </summary>
    public string ToolbarMore { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes added to each <see cref="TwBlazor.Components.TwToggleGroup{T}"/> in the toolbar, which
    /// take away the group's own border and padding so the buttons sit directly on the toolbar. They need the important
    /// modifier (<c>!</c>), since they replace classes the toggle group sets itself.
    /// </summary>
    public string ToolbarGroup { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the line drawn between groups of toolbar controls.
    /// </summary>
    public string ToolbarSeparator { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes added to the root of the number format list in the toolbar, such as its width.
    /// </summary>
    public string FormatSelect { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the bar under the toolbar that holds the cell reference and the cell's contents.
    /// </summary>
    public required string FormulaBar { get; set; }

    /// <summary>
    /// Gets or sets the classes for the cell reference at the start of the formula bar, such as <c>B3</c>.
    /// </summary>
    public required string NameBox { get; set; }

    /// <summary>
    /// Gets or sets the classes for the <c>fx</c> mark between the cell reference and the cell's contents.
    /// </summary>
    public string FormulaMark { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the field that shows and edits the active cell's contents.
    /// </summary>
    public required string FormulaInput { get; set; }

    /// <summary>
    /// Gets or sets the classes for the scrolling area around the grid. Its scroll padding should match the height of
    /// the column headings and the width of the row headings, so the active cell is never scrolled under them.
    /// </summary>
    public required string Viewport { get; set; }

    /// <summary>
    /// Gets or sets the classes for the grid's table, which takes focus while the user moves between cells.
    /// </summary>
    /// <remarks>The grid's focus ring needs the 3:1 contrast ratio against the page.</remarks>
    public required string Grid { get; set; }

    /// <summary>
    /// Gets or sets the classes for the empty heading at the top left corner, which selects every cell.
    /// </summary>
    public required string Corner { get; set; }

    /// <summary>
    /// Gets or sets the classes for a column heading, such as <c>A</c>.
    /// </summary>
    public required string ColumnHeader { get; set; }

    /// <summary>
    /// Gets or sets the classes for a row heading, such as <c>1</c>.
    /// </summary>
    public required string RowHeader { get; set; }

    /// <summary>
    /// Gets or sets the classes added to the column and row headings of the selection.
    /// </summary>
    public required string HeaderSelected { get; set; }

    /// <summary>
    /// Gets or sets the classes for every cell.
    /// </summary>
    public required string Cell { get; set; }

    /// <summary>
    /// Gets or sets the classes added to the cells in the selected range.
    /// </summary>
    public required string CellSelected { get; set; }

    /// <summary>
    /// Gets or sets the classes added to the active cell, the one typing goes into. It should not rely on colour alone,
    /// such as a thick outline. The grid has the <c>group/sheet</c> class, so the outline can change while the grid
    /// has focus with <c>group-focus-within/sheet:</c>, which is how keyboard users see where focus is.
    /// </summary>
    /// <remarks>The outline needs the 3:1 contrast ratio against the cell, since it is the grid's focus
    /// indicator.</remarks>
    public required string CellActive { get; set; }

    /// <summary>
    /// Gets or sets the classes added to a cell that holds a number, such as tabular figures.
    /// </summary>
    public string CellNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes added to a cell that holds an error, such as <c>#DIV/0!</c>.
    /// </summary>
    /// <remarks>The text needs the 4.5:1 contrast ratio against the cell. The error code itself says what went
    /// wrong, so the colour is not the only sign.</remarks>
    public string CellError { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes that line a cell's content up at its start.
    /// </summary>
    public required string AlignStart { get; set; }

    /// <summary>
    /// Gets or sets the classes that line a cell's content up in its middle.
    /// </summary>
    public required string AlignCenter { get; set; }

    /// <summary>
    /// Gets or sets the classes that line a cell's content up at its end.
    /// </summary>
    public required string AlignEnd { get; set; }

    /// <summary>
    /// Gets or sets the classes for bold cell text.
    /// </summary>
    public string Bold { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for italic cell text.
    /// </summary>
    public string Italic { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for underlined cell text.
    /// </summary>
    public string Underline { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the field that opens in a cell while it is edited.
    /// </summary>
    public required string CellEditor { get; set; }

    /// <summary>
    /// Gets or sets the classes added to cells in frozen rows and columns, which stay in view while the rest scrolls
    /// under them, so they need a solid background.
    /// </summary>
    public required string CellFrozen { get; set; }

    /// <summary>
    /// Gets or sets the classes added to the last frozen row, such as a line along its bottom marking where the frozen
    /// rows end.
    /// </summary>
    public string FrozenRowEdge { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes added to the last frozen column, such as a line along its end.
    /// </summary>
    public string FrozenColumnEdge { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the handle at the end of each column heading, dragged to resize the column.
    /// </summary>
    /// <remarks>Keyboard users resize the active column from the context menu instead.</remarks>
    public required string ResizeHandle { get; set; }

    /// <summary>
    /// Gets or sets the classes added to the root of the font list in the toolbar, such as its width.
    /// </summary>
    public string FontSelect { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes added to the root of the font size list in the toolbar, such as its width.
    /// </summary>
    public string FontSizeSelect { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the colour swatch shown beside each colour in the text color and fill color menus.
    /// </summary>
    public required string ColorSwatch { get; set; }

    /// <summary>
    /// Gets or sets the classes for a colour in the text color and fill color menus, which line its swatch up with its
    /// name.
    /// </summary>
    public required string ColorItem { get; set; }

    /// <summary>
    /// Gets or sets the classes added to <see cref="Root"/> in full screen mode, when the spreadsheet covers the
    /// window.
    /// </summary>
    public required string FullScreen { get; set; }

    /// <summary>
    /// Gets or sets the classes added to the grid's scrolling area in full screen mode, so it fills the height left
    /// over by the toolbar and the footer.
    /// </summary>
    public required string ViewportFullScreen { get; set; }

    /// <summary>
    /// Gets or sets the classes for the bar at the bottom that holds the sheet tabs, the selection summary and the zoom
    /// controls.
    /// </summary>
    public required string Footer { get; set; }

    /// <summary>
    /// Gets or sets the classes for the summary of the selected numbers, such as their sum and average.
    /// </summary>
    /// <remarks>The text needs the 4.5:1 contrast ratio against the footer.</remarks>
    public required string StatusText { get; set; }

    /// <summary>
    /// Gets or sets the classes for the group of zoom controls in the footer.
    /// </summary>
    public required string ZoomControls { get; set; }

    /// <summary>
    /// Gets or sets the classes for the button between the zoom buttons that shows the zoom level and resets it.
    /// </summary>
    public required string ZoomLabel { get; set; }

    /// <summary>
    /// Gets or sets the classes for the group of sheet tabs inside the footer.
    /// </summary>
    public required string SheetTabs { get; set; }

    /// <summary>
    /// Gets or sets the classes for a sheet tab.
    /// </summary>
    public required string SheetTab { get; set; }

    /// <summary>
    /// Gets or sets the classes added to the tab of the sheet being shown. It should not rely on colour alone.
    /// </summary>
    public required string SheetTabActive { get; set; }

    /// <summary>
    /// Gets or sets the classes for the field that opens in a sheet tab to rename it.
    /// </summary>
    public required string SheetTabInput { get; set; }

    /// <summary>
    /// Gets or sets the classes for the button after the sheet tabs that adds a sheet.
    /// </summary>
    public required string AddSheetButton { get; set; }

    /// <summary>
    /// Gets or sets the classes for the handle at the bottom of each row heading, dragged to resize the row.
    /// </summary>
    /// <remarks>Keyboard users resize the active row from the context menu instead.</remarks>
    public required string RowResizeHandle { get; set; }

    /// <summary>
    /// Gets or sets the classes for the heading row of a table. It needs a solid background and the 4.5:1 contrast ratio for its text, and room at the end for the filter button.
    /// </summary>
    public required string TableHeader { get; set; }

    /// <summary>
    /// Gets or sets the classes for every other row of a banded table.
    /// </summary>
    public required string TableBand { get; set; }

    /// <summary>
    /// Gets or sets the classes for the filter button in each table heading. Drawing it in the heading's own text
    /// colour (<c>text-inherit</c>, <c>bg-current/15</c>) keeps it readable when the heading is given a fill, since a
    /// filled cell gets black or white text, whichever reads better.
    /// </summary>
    /// <remarks>It needs the 3:1 contrast ratio against the heading.</remarks>
    public required string FilterButton { get; set; }

    /// <summary>
    /// Gets or sets the classes for the filter button of a column that is filtered. Its icon changes too, so the state does not rely on colour alone.
    /// </summary>
    public required string FilterButtonActive { get; set; }

    /// <summary>
    /// Gets or sets the classes for the layer behind a dialog, such as the confirmation before a sheet is deleted. On small screens it holds the dialog at the bottom, as a sheet.
    /// </summary>
    public required string DialogBackdrop { get; set; }

    /// <summary>
    /// Gets or sets the classes for the dialog panel.
    /// </summary>
    public required string Dialog { get; set; }

    /// <summary>
    /// Gets or sets the classes for the dialog title.
    /// </summary>
    public required string DialogTitle { get; set; }

    /// <summary>
    /// Gets or sets the classes for the dialog message.
    /// </summary>
    /// <remarks>It needs the 4.5:1 contrast ratio against the panel.</remarks>
    public required string DialogText { get; set; }

    /// <summary>
    /// Gets or sets the classes for the area holding the fields of a dialog.
    /// </summary>
    public required string DialogBody { get; set; }

    /// <summary>
    /// Gets or sets the classes for the row of buttons at the bottom of a dialog. On small screens the buttons stack, full width.
    /// </summary>
    public required string DialogActions { get; set; }

    /// <summary>
    /// Gets or sets the classes for each dialog button, such as a taller height on small screens so it is easy to tap.
    /// </summary>
    public required string DialogButton { get; set; }

    /// <summary>
    /// Gets or sets the classes for the scrolling list of values in a column filter.
    /// </summary>
    public required string FilterList { get; set; }

    /// <summary>
    /// Gets or sets the classes for the row of sort buttons at the top of a column filter.
    /// </summary>
    public required string FilterSort { get; set; }

    /// <summary>
    /// Gets or sets the classes for the label of a field in a dialog.
    /// </summary>
    public required string FieldLabel { get; set; }

    /// <summary>
    /// Gets or sets the classes for a text field in a dialog. Text at least 16 pixels high keeps mobile browsers from zooming in.
    /// </summary>
    public required string FieldInput { get; set; }

    /// <summary>
    /// Gets or sets the classes for the message under a field in a dialog that is not valid.
    /// </summary>
    /// <remarks>It needs the 4.5:1 contrast ratio against the panel.</remarks>
    public required string FieldError { get; set; }

    /// <summary>
    /// Gets or sets the classes for the top of a dialog: its title and its close button.
    /// </summary>
    public required string DialogHeader { get; set; }

    /// <summary>
    /// Gets or sets the classes for the close button at the top of a dialog. It needs a target of at least 24 pixels and a visible focus ring.
    /// </summary>
    public required string DialogClose { get; set; }

    /// <summary>
    /// Gets or sets the classes for the filter dialog panel, on top of <see cref="Dialog"/>, such as a wider maximum width.
    /// </summary>
    public required string FilterDialog { get; set; }

    /// <summary>
    /// Gets or sets the classes for each section of a column filter: sort, conditions and values.
    /// </summary>
    public required string FilterSection { get; set; }

    /// <summary>
    /// Gets or sets the classes for the small heading of each section of a column filter. It needs the 4.5:1 contrast ratio against the panel.
    /// </summary>
    public required string FilterSectionTitle { get; set; }

    /// <summary>
    /// Gets or sets the classes for the conditions editor in a column filter, such as putting each comparison and its value side by side on wider screens.
    /// </summary>
    public required string FilterConditions { get; set; }

    /// <summary>
    /// Gets or sets the classes for the field that narrows the list of values in a column filter.
    /// </summary>
    public required string FilterSearch { get; set; }

    /// <summary>
    /// Gets or sets the classes for each value in the list of a column filter, added to the root of its checkbox.
    /// </summary>
    public required string FilterItem { get; set; }

    /// <summary>
    /// Gets or sets the classes for the buttons at the bottom of a column filter.
    /// </summary>
    public required string FilterFooter { get; set; }

    /// <summary>
    /// Gets or sets the classes for the Clear filter button, on top of <see cref="DialogButton"/>, such as pushing it to the start of the row.
    /// </summary>
    public required string FilterClear { get; set; }
}
