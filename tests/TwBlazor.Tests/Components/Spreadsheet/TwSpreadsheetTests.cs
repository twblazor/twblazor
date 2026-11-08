using Bunit;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;
using TwBlazor.Models;

namespace TwBlazor.Tests.Components.Spreadsheet;

public class TwSpreadsheetTests : TwBlazorTestBase
{
    private TwSpreadsheetTheme theme => Theme.Components.Require<TwSpreadsheetTheme>();

    private static List<Sheet> Book() =>
    [
        new Sheet
        {
            Name = "Budget",
            Cells =
            {
                ["A1"] = new() { Value = "Item" },
                ["B1"] = new() { Value = "Cost" },
                ["A2"] = new() { Value = "Rent" },
                ["B2"] = new() { Value = "1200" },
                ["A3"] = new() { Value = "Food" },
                ["B3"] = new() { Value = "300" },
                ["B4"] = new() { Value = "=SUM(B2:B3)" },
            },
        },
        new Sheet { Name = "Notes", Cells = { ["A1"] = new() { Value = "=Budget!B4/2" } } },
    ];

    private IRenderedComponent<TwSpreadsheet> Render(List<Sheet>? sheets = null, Action<ComponentParameterCollectionBuilder<TwSpreadsheet>>? configure = null) =>
        TestContext.Render<TwSpreadsheet>(p =>
        {
            p.Add(x => x.Id, "sh");
            p.Add(x => x.Sheets, sheets ?? Book());
            p.Add(x => x.Rows, 10);
            p.Add(x => x.Columns, 5);
            configure?.Invoke(p);
        });

    private static AngleSharp.Dom.IElement Grid(IRenderedComponent<TwSpreadsheet> cut) => cut.Find("#sh");

    private static string? Active(IRenderedComponent<TwSpreadsheet> cut) => Grid(cut).GetAttribute("aria-activedescendant");

    private static AngleSharp.Dom.IElement Cell(IRenderedComponent<TwSpreadsheet> cut, string address) => cut.Find($"#sh-{address}");

    private static Task Key(IRenderedComponent<TwSpreadsheet> cut, string key, bool shift = false, bool ctrl = false) =>
        cut.InvokeAsync(() => Grid(cut).KeyDown(new KeyboardEventArgs { Key = key, ShiftKey = shift, CtrlKey = ctrl }));

    private static AngleSharp.Dom.IElement ButtonNamed(IRenderedComponent<TwSpreadsheet> cut, string name) =>
        cut.FindAll("button").Single(b => b.GetAttribute("aria-label") == name);

    private static string Status(IRenderedComponent<TwSpreadsheet> cut) => cut.Find("[role='status']").TextContent;

    private async Task TypeIntoCellAsync(IRenderedComponent<TwSpreadsheet> cut, string text)
    {
        await cut.InvokeAsync(() => cut.Instance.BeginTypingAsync());
        await cut.InvokeAsync(() => cut.Find("#sh-editor").Input(text));
    }

    #region Rendering

    [Fact]
    public void Renders_AnAriaGrid_WithHeadingsAndCells()
    {
        var cut = Render();

        var grid = Grid(cut);
        Assert.Equal("grid", grid.GetAttribute("role"));
        Assert.Equal("0", grid.GetAttribute("tabindex"));
        Assert.Equal("true", grid.GetAttribute("aria-multiselectable"));
        Assert.Equal("11", grid.GetAttribute("aria-rowcount"));
        Assert.Equal("6", grid.GetAttribute("aria-colcount"));
        Assert.Equal(Localizer[TwStrings.Spreadsheet_Label], grid.GetAttribute("aria-label"));
        Assert.Equal("sh-hint", grid.GetAttribute("aria-describedby"));
        Assert.Equal(Localizer[TwStrings.Spreadsheet_GridHint], cut.Find("#sh-hint").TextContent);
        Assert.Equal("sh-A1", Active(cut));

        Assert.Equal(["A", "B", "C", "D", "E"], cut.FindAll("th[scope='col']").Select(h => h.TextContent.Trim()));
        Assert.Equal(10, cut.FindAll("th[role='rowheader']").Count);
        Assert.Equal(50, cut.FindAll("td[role='gridcell']").Count);
        Assert.Equal("true", Cell(cut, "A1").GetAttribute("aria-selected"));
        Assert.Equal("false", Cell(cut, "B1").GetAttribute("aria-selected"));
    }

    [Fact]
    public void Cells_ShowValues_AndFormulaResults()
    {
        var cut = Render();

        Assert.Equal("Rent", Cell(cut, "A2").TextContent.Trim());
        Assert.Equal("1500", Cell(cut, "B4").TextContent.Trim());
        Assert.Contains(theme.AlignEnd, Cell(cut, "B4").ClassList);
        Assert.Contains(theme.CellNumber, Cell(cut, "B4").ClassList);
        Assert.Contains(theme.AlignStart, Cell(cut, "A2").ClassList);
        Assert.Contains(theme.CellActive.Split(' ')[0], Cell(cut, "A1").ClassList);
    }

    [Fact]
    public void Cells_ShowErrors_WithTheErrorClass()
    {
        var cut = Render([new Sheet { Cells = { ["A1"] = new() { Value = "=1/0" } } }]);

        Assert.Equal("#DIV/0!", Cell(cut, "A1").TextContent.Trim());
        Assert.Contains(theme.CellError.Split(' ')[0], Cell(cut, "A1").ClassList);
    }

    [Fact]
    public void Cells_ShowTheirFormatting()
    {
        var cut = Render([new Sheet
        {
            Cells =
            {
                ["A1"] = new() { Value = "x", Bold = true, Italic = true, Underline = true, Alignment = SheetCellAlignment.Center },
                ["A2"] = new() { Value = "1234.5", Format = SheetNumberFormat.Currency },
            },
        }]);

        var cell = Cell(cut, "A1");
        Assert.Contains(theme.Bold, cell.ClassList);
        Assert.Contains("italic", cell.ClassList);
        Assert.Contains("underline", cell.ClassList);
        Assert.Contains(theme.AlignCenter, cell.ClassList);
        Assert.Equal("£1,234.50", Cell(cut, "A2").TextContent.Trim());
    }

    [Fact]
    public void AnEmptyWorkbook_GetsOneSheet()
    {
        var sheets = new List<Sheet>();
        var cut = Render(sheets);

        Assert.Single(sheets);
        Assert.Equal(Localizer[TwStrings.Spreadsheet_DefaultSheetName, 1], sheets[0].Name);
        Assert.Single(cut.FindAll("[role='tab']"));
    }

    [Fact]
    public void Parts_CanBeHidden()
    {
        var cut = Render(configure: p => p.Add(x => x.ShowToolbar, false).Add(x => x.ShowFormulaBar, false).Add(x => x.ShowSheetTabs, false));

        Assert.Empty(cut.FindAll("[role='toolbar']"));
        Assert.Empty(cut.FindAll("#sh-formula"));
        Assert.Empty(cut.FindAll("[role='tablist']"));
        Assert.Null(cut.Find("#sh-panel").GetAttribute("role"));
    }

    [Fact]
    public void Size_IsSetByColumnWidthAndHeight()
    {
        var cut = Render(configure: p => p.Add(x => x.ColumnWidth, 80).Add(x => x.Height, "10rem").Add(x => x.Class, "my-sheet"));

        Assert.Equal("width:448px;zoom:1", Grid(cut).GetAttribute("style"));
        Assert.StartsWith("height:10rem;scroll-padding-top:32px;scroll-padding-inline-start:48px", cut.Find("#sh-panel").GetAttribute("style"));
        Assert.Contains("my-sheet", cut.Find("#sh-root").ClassList);
    }

    [Fact]
    public void AriaLabel_AndAriaLabelledBy_NameTheGrid()
    {
        var named = Render(configure: p => p.Add(x => x.AriaLabel, "Budget"));
        Assert.Equal("Budget", Grid(named).GetAttribute("aria-label"));

        var labelled = Render(configure: p => p.Add(x => x.AriaLabelledBy, "heading"));
        Assert.Equal("heading", Grid(labelled).GetAttribute("aria-labelledby"));
        Assert.Null(Grid(labelled).GetAttribute("aria-label"));
    }

    [Fact]
    public void Attach_IsCalledAfterTheFirstRender_AndDetachOnDispose()
    {
        var cut = Render();

        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twSpreadsheet.attach");
        TestContext.Dispose();
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twSpreadsheet.detach");
    }

    #endregion

    #region Keyboard navigation

    [Theory]
    [InlineData("ArrowRight", "sh-B1")]
    [InlineData("ArrowDown", "sh-A2")]
    [InlineData("ArrowLeft", "sh-A1")]
    [InlineData("ArrowUp", "sh-A1")]
    [InlineData("PageDown", "sh-A10")]
    [InlineData("End", "sh-B1")]
    public async Task Keys_MoveTheActiveCell(string key, string expected)
    {
        var cut = Render();

        await Key(cut, key);

        Assert.Equal(expected, Active(cut));
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twSpreadsheet.scrollToCell");
    }

    [Fact]
    public async Task CtrlHome_AndCtrlEnd_GoToTheCornersOfTheData()
    {
        var cut = Render();

        await Key(cut, "End", ctrl: true);
        Assert.Equal("sh-B4", Active(cut));

        await Key(cut, "Home");
        Assert.Equal("sh-A4", Active(cut));

        await Key(cut, "Home", ctrl: true);
        Assert.Equal("sh-A1", Active(cut));
    }

    [Fact]
    public async Task CtrlArrow_JumpsAcrossBlocksOfFilledCells()
    {
        var cut = Render();

        await Key(cut, "ArrowDown", ctrl: true);
        Assert.Equal("sh-A3", Active(cut));

        await Key(cut, "ArrowDown", ctrl: true);
        Assert.Equal("sh-A10", Active(cut));

        await Key(cut, "ArrowUp", ctrl: true);
        Assert.Equal("sh-A3", Active(cut));

        await Key(cut, "ArrowRight", ctrl: true);
        Assert.Equal("sh-B3", Active(cut));

        await Key(cut, "ArrowRight", ctrl: true);
        Assert.Equal("sh-E3", Active(cut));
    }

    [Fact]
    public async Task ShiftArrow_ExtendsTheSelection_AndAnnouncesIt()
    {
        var events = new List<SheetSelectionEventArgs>();
        var cut = Render(configure: p => p.Add(x => x.OnSelectionChange, e => events.Add(e)));

        await Key(cut, "ArrowRight", shift: true);
        await Key(cut, "ArrowDown", shift: true);

        Assert.Equal("sh-A1", Active(cut));
        Assert.Equal("A1:B2", cut.Instance.SelectedRange);
        Assert.Equal(4, cut.FindAll("td[aria-selected='true']").Count);
        Assert.Contains(theme.CellSelected.Split(' ')[0], Cell(cut, "B2").ClassList);
        Assert.Contains(Localizer[TwStrings.Spreadsheet_RangeSelected, "A1:B2"], Status(cut));
        Assert.Equal("A1:B2", events[^1].Range);
        Assert.Equal("A1", events[^1].ActiveCell);
        Assert.Contains(theme.HeaderSelected.Split(' ')[0], cut.FindAll("th[scope='col']")[1].ClassList);

        await Key(cut, "Escape");
        Assert.Equal("A1", cut.Instance.SelectedRange);
    }

    [Fact]
    public async Task SelectAll_Row_AndColumn()
    {
        var cut = Render();
        await Key(cut, "ArrowDown");

        await Key(cut, "a", ctrl: true);
        Assert.Equal("A1:E10", cut.Instance.SelectedRange);

        await Key(cut, "ArrowRight");
        await Key(cut, " ", shift: true);
        Assert.Equal("A1:E1", cut.Instance.SelectedRange);
        Assert.Equal("B1", cut.Instance.ActiveCell);

        await Key(cut, " ", ctrl: true);
        Assert.Equal("B1:B10", cut.Instance.SelectedRange);
        Assert.Equal("B1", cut.Instance.ActiveCell);
    }

    [Fact]
    public async Task ArrowKeys_AreMirrored_InARightToLeftLayout()
    {
        var provider = TestContext.Render<TwDirectionProvider>(p => p
            .Add(x => x.Direction, TextDirection.RightToLeft)
            .AddChildContent<TwSpreadsheet>(s => s.Add(x => x.Id, "sh").Add(x => x.Sheets, Book()).Add(x => x.Rows, 5).Add(x => x.Columns, 5)));
        var cut = provider.FindComponent<TwSpreadsheet>();

        await cut.InvokeAsync(() => cut.Find("#sh").KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" }));

        Assert.Equal("B1", cut.Instance.ActiveCell);
        Assert.NotNull(cut.FindAll("button").SingleOrDefault(b => b.GetAttribute("aria-label") == Localizer[TwStrings.Spreadsheet_AlignRight]));
    }

    #endregion

    #region Editing

    [Fact]
    public async Task Typing_ReplacesTheCell_AndEnterKeepsItAndMovesDown()
    {
        var changes = new List<SheetChangeEventArgs>();
        var sheets = Book();
        var cut = Render(sheets, p => p.Add(x => x.OnCellsChange, e => changes.Add(e)));

        await TypeIntoCellAsync(cut, "Total");

        var focus = TestContext.JSInterop.Invocations.Last(i => i.Identifier == "twSpreadsheet.focusEditor");
        Assert.Equal("sh-editor", focus.Arguments[1]);
        Assert.Equal(true, focus.Arguments[2]);
        Assert.Equal("enter", Grid(cut).GetAttribute("data-editing"));
        Assert.Equal(Localizer[TwStrings.Spreadsheet_EditCell, "A1"], cut.Find("#sh-editor").GetAttribute("aria-label"));
        Assert.Equal("Total", cut.Find("#sh-formula").GetAttribute("value"));

        await Key(cut, "Enter");

        Assert.Equal("Total", sheets[0].Cells["A1"].Value);
        Assert.Equal("sh-A2", Active(cut));
        Assert.Empty(cut.FindAll("#sh-editor"));
        Assert.Equal(["A1"], changes.Single().Addresses);
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twSpreadsheet.focus");
    }

    [Theory]
    [InlineData("Tab", false, "B1")]
    [InlineData("Tab", true, "A1")]
    [InlineData("Enter", true, "A1")]
    [InlineData("ArrowRight", false, "B1")]
    public async Task KeysThatFinishAnEdit_MoveAfterwards(string key, bool shift, string expected)
    {
        var sheets = Book();
        var cut = Render(sheets);

        await TypeIntoCellAsync(cut, "x");
        await Key(cut, key, shift);

        Assert.Equal("x", sheets[0].Cells["A1"].Value);
        Assert.Equal(expected, cut.Instance.ActiveCell);
    }

    [Fact]
    public async Task F2_EditsWithTheCurrentContents_AndArrowKeysMoveTheCaret()
    {
        var sheets = Book();
        var cut = Render(sheets);
        await Key(cut, "ArrowDown");
        await Key(cut, "ArrowDown");
        await Key(cut, "ArrowDown");
        await Key(cut, "ArrowRight");

        await Key(cut, "F2");

        Assert.Equal("=SUM(B2:B3)", cut.Find("#sh-editor").GetAttribute("value"));
        Assert.Equal("edit", Grid(cut).GetAttribute("data-editing"));
        await Key(cut, "ArrowLeft");
        Assert.NotEmpty(cut.FindAll("#sh-editor"));

        await Key(cut, "F2");
        Assert.Equal("enter", Grid(cut).GetAttribute("data-editing"));
    }

    [Fact]
    public async Task Enter_EditsTheCell_AndEscapeThrowsTheChangeAway()
    {
        var sheets = Book();
        var cut = Render(sheets);

        await Key(cut, "Enter");
        await cut.InvokeAsync(() => cut.Find("#sh-editor").Input("Changed"));
        await Key(cut, "Escape");

        Assert.Equal("Item", sheets[0].Cells["A1"].Value);
        Assert.Empty(cut.FindAll("#sh-editor"));
    }

    [Fact]
    public async Task Blur_KeepsTheChange()
    {
        var sheets = Book();
        var cut = Render(sheets);

        await TypeIntoCellAsync(cut, "Kept");
        await cut.InvokeAsync(() => cut.Find("#sh-editor").Blur());

        Assert.Equal("Kept", sheets[0].Cells["A1"].Value);
    }

    [Fact]
    public async Task AFormula_IsWorkedOutWhenItIsKept_AndDependentCellsUpdate()
    {
        var sheets = Book();
        var cut = Render(sheets);
        await cut.InvokeAsync(() => cut.Instance.SelectAsync("B2"));

        await TypeIntoCellAsync(cut, "2000");
        await Key(cut, "Enter");

        Assert.Equal("2300", Cell(cut, "B4").TextContent.Trim());
        Assert.Equal("1150", cut.Instance.GetDisplayText("A1", 1));
    }

    [Fact]
    public async Task EmptyingACell_RemovesIt()
    {
        var sheets = Book();
        var cut = Render(sheets);

        await TypeIntoCellAsync(cut, string.Empty);
        await Key(cut, "Enter");

        Assert.False(sheets[0].Cells.ContainsKey("A1"));
    }

    [Fact]
    public async Task Delete_ClearsTheSelection_KeepingFormatting()
    {
        var sheets = Book();
        sheets[0].Cells["A2"].Bold = true;
        var cut = Render(sheets);
        await Key(cut, "ArrowDown", shift: true);

        await Key(cut, "Delete");

        Assert.False(sheets[0].Cells.ContainsKey("A1"));
        Assert.Null(sheets[0].Cells["A2"].Value);
        Assert.True(sheets[0].Cells["A2"].Bold);
        Assert.Contains(Localizer[TwStrings.Spreadsheet_Cleared, "A1:A2"], Status(cut));
    }

    [Fact]
    public async Task UndoAndRedo_RestoreChanges()
    {
        var sheets = Book();
        var cut = Render(sheets);
        await TypeIntoCellAsync(cut, "New");
        await Key(cut, "Enter");

        await Key(cut, "z", ctrl: true);
        Assert.Equal("Item", sheets[0].Cells["A1"].Value);
        Assert.Equal("A1", cut.Instance.SelectedRange);

        await Key(cut, "y", ctrl: true);
        Assert.Equal("New", sheets[0].Cells["A1"].Value);

        await Key(cut, "z", ctrl: true);
        await Key(cut, "Z", shift: true, ctrl: true);
        Assert.Equal("New", sheets[0].Cells["A1"].Value);
    }

    [Fact]
    public async Task UndoButtons_AreDisabledUntilThereIsSomethingToUndo()
    {
        var cut = Render();
        Assert.True(ButtonNamed(cut, Localizer[TwStrings.TextEditor_Undo]).HasAttribute("disabled"));
        Assert.True(ButtonNamed(cut, Localizer[TwStrings.TextEditor_Redo]).HasAttribute("disabled"));

        await TypeIntoCellAsync(cut, "1");
        await Key(cut, "Enter");
        Assert.False(ButtonNamed(cut, Localizer[TwStrings.TextEditor_Undo]).HasAttribute("disabled"));

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.TextEditor_Undo]).Click());
        Assert.False(ButtonNamed(cut, Localizer[TwStrings.TextEditor_Redo]).HasAttribute("disabled"));
    }

    #endregion

    #region Formula bar and name box

    [Fact]
    public async Task FormulaBar_ShowsTheRawContents_AndEditsTheCell()
    {
        var sheets = Book();
        var cut = Render(sheets);
        await cut.InvokeAsync(() => cut.Instance.SelectAsync("B4"));

        var bar = cut.Find("#sh-formula");
        Assert.Equal("=SUM(B2:B3)", bar.GetAttribute("value"));
        Assert.Equal(Localizer[TwStrings.Spreadsheet_FormulaBar], bar.GetAttribute("aria-label"));

        await cut.InvokeAsync(() => cut.Find("#sh-formula").Focus());
        await cut.InvokeAsync(() => cut.Find("#sh-formula").Input("=B2*2"));
        await cut.InvokeAsync(() => cut.Find("#sh-formula").KeyDown(new KeyboardEventArgs { Key = "Enter" }));

        Assert.Equal("=B2*2", sheets[0].Cells["B4"].Value);
        Assert.Equal("2400", Cell(cut, "B4").TextContent.Trim());
        Assert.Equal("B5", cut.Instance.ActiveCell);
    }

    [Fact]
    public async Task FormulaBar_EscapeThrowsTheChangeAway()
    {
        var sheets = Book();
        var cut = Render(sheets);

        await cut.InvokeAsync(() => cut.Find("#sh-formula").Focus());
        await cut.InvokeAsync(() => cut.Find("#sh-formula").Input("nope"));
        await cut.InvokeAsync(() => cut.Find("#sh-formula").KeyDown(new KeyboardEventArgs { Key = "Escape" }));

        Assert.Equal("Item", sheets[0].Cells["A1"].Value);
    }

    [Fact]
    public async Task NameBox_SelectsTheRangeTyped()
    {
        var cut = Render();
        var box = cut.Find($"input[aria-label='{Localizer[TwStrings.Spreadsheet_NameBox]}']");
        Assert.Equal("A1", box.GetAttribute("value"));

        await cut.InvokeAsync(() => box.Focus());
        await cut.InvokeAsync(() => cut.Find($"input[aria-label='{Localizer[TwStrings.Spreadsheet_NameBox]}']").Input("c3:d4"));
        await cut.InvokeAsync(() => cut.Find($"input[aria-label='{Localizer[TwStrings.Spreadsheet_NameBox]}']").KeyDown(new KeyboardEventArgs { Key = "Enter" }));

        Assert.Equal("C3:D4", cut.Instance.SelectedRange);
        Assert.Equal("C3", cut.Instance.ActiveCell);
    }

    #endregion

    #region Pointer

    [Fact]
    public async Task PointerDown_SelectsACell_AndDragExtendsIt()
    {
        var cut = Render();

        await cut.InvokeAsync(() => cut.Instance.PointerDownAsync(1, 1, false));
        await cut.InvokeAsync(() => cut.Instance.PointerExtendAsync(3, 2));

        Assert.Equal("B2", cut.Instance.ActiveCell);
        Assert.Equal("B2:C4", cut.Instance.SelectedRange);

        await cut.InvokeAsync(() => cut.Instance.PointerDownAsync(0, 0, true));
        Assert.Equal("A1:B2", cut.Instance.SelectedRange);
        Assert.Equal("B2", cut.Instance.ActiveCell);
    }

    [Fact]
    public async Task PointerDown_OnHeadings_SelectsColumnsRowsOrEverything()
    {
        var cut = Render();

        await cut.InvokeAsync(() => cut.Instance.PointerDownAsync(-1, 2, false));
        Assert.Equal("C1:C10", cut.Instance.SelectedRange);

        await cut.InvokeAsync(() => cut.Instance.PointerExtendAsync(-1, 3));
        Assert.Equal("C1:D10", cut.Instance.SelectedRange);

        await cut.InvokeAsync(() => cut.Instance.PointerDownAsync(4, -1, false));
        Assert.Equal("A5:E5", cut.Instance.SelectedRange);

        await cut.InvokeAsync(() => cut.Instance.PointerDownAsync(-1, -1, false));
        Assert.Equal("A1:E10", cut.Instance.SelectedRange);
    }

    [Fact]
    public async Task DoubleClick_EditsTheCell()
    {
        var cut = Render();

        await cut.InvokeAsync(() => cut.Instance.PointerDoubleClickAsync(1, 0));

        Assert.Equal("Rent", cut.Find("#sh-editor").GetAttribute("value"));
        Assert.Equal("A2", cut.Instance.ActiveCell);
    }

    [Fact]
    public async Task WhileTypingAFormula_ClickingCellsPutsTheirReferenceIn()
    {
        var sheets = Book();
        var cut = Render(sheets);
        await cut.InvokeAsync(() => cut.Instance.SelectAsync("C2"));

        await TypeIntoCellAsync(cut, "=SUM(");
        Assert.Equal("true", Grid(cut).GetAttribute("data-pointing"));

        await cut.InvokeAsync(() => cut.Instance.PointerDownAsync(1, 1, false));
        await cut.InvokeAsync(() => cut.Instance.PointerExtendAsync(2, 1));
        Assert.Equal("=SUM(B2:B3", cut.Find("#sh-editor").GetAttribute("value"));
        Assert.Equal("C2", cut.Instance.ActiveCell);

        await cut.InvokeAsync(() => cut.Find("#sh-editor").Input("=SUM(B2:B3)"));
        await Key(cut, "Enter");

        Assert.Equal("=SUM(B2:B3)", sheets[0].Cells["C2"].Value);
        Assert.Equal("1500", Cell(cut, "C2").TextContent.Trim());
    }

    [Fact]
    public async Task PointerDown_WhileEditingText_KeepsTheEditAndMoves()
    {
        var sheets = Book();
        var cut = Render(sheets);

        await TypeIntoCellAsync(cut, "Hello");
        await cut.InvokeAsync(() => cut.Instance.PointerDownAsync(2, 2, false));

        Assert.Equal("Hello", sheets[0].Cells["A1"].Value);
        Assert.Equal("C3", cut.Instance.ActiveCell);
    }

    #endregion

    #region Clipboard

    [Fact]
    public async Task Copy_PutsTabSeparatedTextOnTheClipboard()
    {
        var cut = Render();
        await Key(cut, "ArrowRight", shift: true);
        await Key(cut, "ArrowDown", shift: true);

        await Key(cut, "c", ctrl: true);

        var copy = TestContext.JSInterop.Invocations.Single(i => i.Identifier == "twSpreadsheet.copyText");
        Assert.Equal("Item\tCost\nRent\t1200", copy.Arguments[0]);
        Assert.Contains(Localizer[TwStrings.Spreadsheet_Copied, "A1:B2"], Status(cut));
    }

    [Fact]
    public async Task PastingCopiedCells_MovesTheirFormulas_AndKeepsFormatting()
    {
        var sheets = Book();
        sheets[0].Cells["B4"].Bold = true;
        var cut = Render(sheets);
        await cut.InvokeAsync(() => cut.Instance.SelectAsync("B4"));
        await Key(cut, "c", ctrl: true);
        await cut.InvokeAsync(() => cut.Instance.SelectAsync("C4"));

        await cut.InvokeAsync(() => cut.Instance.PasteTextAsync("1500"));

        Assert.Equal("=SUM(C2:C3)", sheets[0].Cells["C4"].Value);
        Assert.True(sheets[0].Cells["C4"].Bold);
        Assert.Contains(Localizer[TwStrings.Spreadsheet_Pasted, "C4"], Status(cut));
    }

    [Fact]
    public async Task PastingOneCopiedCell_FillsTheSelection()
    {
        var sheets = Book();
        var cut = Render(sheets);
        await cut.InvokeAsync(() => cut.Instance.SelectAsync("B4"));
        await Key(cut, "c", ctrl: true);
        await cut.InvokeAsync(() => cut.Instance.SelectAsync("C4:D4"));

        await cut.InvokeAsync(() => cut.Instance.PasteTextAsync("1500\r\n"));

        Assert.Equal("=SUM(C2:C3)", sheets[0].Cells["C4"].Value);
        Assert.Equal("=SUM(D2:D3)", sheets[0].Cells["D4"].Value);
    }

    [Fact]
    public async Task PastingText_SplitsItIntoCells()
    {
        var sheets = Book();
        var cut = Render(sheets);
        await cut.InvokeAsync(() => cut.Instance.SelectAsync("C1"));

        await cut.InvokeAsync(() => cut.Instance.PasteTextAsync("a\tb\r\n1\t\n"));

        Assert.Equal("a", sheets[0].Cells["C1"].Value);
        Assert.Equal("b", sheets[0].Cells["D1"].Value);
        Assert.Equal("1", sheets[0].Cells["C2"].Value);
        Assert.False(sheets[0].Cells.ContainsKey("D2"));
        Assert.Equal("C1:D2", cut.Instance.SelectedRange);
    }

    [Fact]
    public async Task PastingOneValue_FillsTheSelection()
    {
        var sheets = Book();
        var cut = Render(sheets);
        await cut.InvokeAsync(() => cut.Instance.SelectAsync("C1:C3"));

        await cut.InvokeAsync(() => cut.Instance.PasteTextAsync("7"));

        Assert.All(new[] { "C1", "C2", "C3" }, a => Assert.Equal("7", sheets[0].Cells[a].Value));
    }

    [Fact]
    public async Task Cut_CopiesAndClears()
    {
        var sheets = Book();
        var cut = Render(sheets);

        await Key(cut, "x", ctrl: true);

        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twSpreadsheet.copyText");
        Assert.False(sheets[0].Cells.ContainsKey("A1"));
    }

    #endregion

    #region Formatting

    [Fact]
    public async Task CtrlB_IAndU_ToggleFormattingOnTheSelection()
    {
        var sheets = Book();
        var cut = Render(sheets);
        await Key(cut, "ArrowDown", shift: true);

        await Key(cut, "b", ctrl: true);
        await Key(cut, "i", ctrl: true);
        await Key(cut, "u", ctrl: true);

        Assert.True(sheets[0].Cells["A2"].Bold);
        Assert.True(sheets[0].Cells["A2"].Italic);
        Assert.True(sheets[0].Cells["A2"].Underline);
        Assert.Equal("true", ButtonNamed(cut, Localizer[TwStrings.TextEditor_Bold]).GetAttribute("aria-pressed"));

        await Key(cut, "b", ctrl: true);
        Assert.False(sheets[0].Cells["A1"].Bold);
    }

    [Fact]
    public async Task Toolbar_SetsBoldAlignmentAndNumberFormat()
    {
        var sheets = Book();
        var cut = Render(sheets);
        await cut.InvokeAsync(() => cut.Instance.SelectAsync("B2"));

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.TextEditor_Bold]).Click());
        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_AlignCenter]).Click());

        Assert.True(sheets[0].Cells["B2"].Bold);
        Assert.Equal(SheetCellAlignment.Center, sheets[0].Cells["B2"].Alignment);
        Assert.Equal("true", ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_AlignCenter]).GetAttribute("aria-checked"));

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_AlignCenter]).Click());
        Assert.Equal(SheetCellAlignment.General, sheets[0].Cells["B2"].Alignment);

        var format = cut.Find($"[role='combobox'][aria-label='{Localizer[TwStrings.Spreadsheet_NumberFormat]}']");
        Assert.Contains(Localizer[TwStrings.Spreadsheet_FormatGeneral], format.TextContent);
        await cut.InvokeAsync(() => format.Click());
        await cut.InvokeAsync(() => cut.FindAll("[role='option']").Single(o => o.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_FormatCurrency]).Click());

        Assert.Equal(SheetNumberFormat.Currency, sheets[0].Cells["B2"].Format);
        Assert.Equal("£1,200.00", Cell(cut, "B2").TextContent.Trim());
    }

    [Fact]
    public void Toolbar_IsAnAriaToolbar_WithLabelledGroups()
    {
        var cut = Render();

        var toolbar = cut.Find("[role='toolbar']");
        Assert.Equal(Localizer[TwStrings.Spreadsheet_Toolbar], toolbar.GetAttribute("aria-label"));
        Assert.Equal("sh", toolbar.GetAttribute("aria-controls"));
        Assert.Equal("Control+B", ButtonNamed(cut, Localizer[TwStrings.TextEditor_Bold]).GetAttribute("aria-keyshortcuts"));
        Assert.Equal("radiogroup", cut.Find("[aria-label='" + Localizer[TwStrings.Spreadsheet_Alignment] + "']").GetAttribute("role"));
    }

    [Fact]
    public void Toolbar_HoldsItsControlsInAScrollingBox_WithAMoreToolsButtonAfterIt()
    {
        var cut = Render();

        var toolbar = cut.Find("[role='toolbar']");
        var items = cut.Find("#sh-toolbar-items");
        var more = ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_MoreTools]);
        Assert.Equal("toolbar", items.ParentElement!.GetAttribute("role"));
        Assert.Contains(theme.ToolbarItems.Split(' ')[0], items.ClassList);
        Assert.Contains("[scrollbar-width:none]", items.ClassList);
        Assert.Null(more.Closest("#sh-toolbar-items"));
        var moreBox = toolbar.Children.Last();
        Assert.Contains(theme.ToolbarMore.Split(' ')[0], moreBox.ClassList);
        Assert.Contains(more, moreBox.QuerySelectorAll("button"));
        Assert.Equal("false", more.GetAttribute("aria-expanded"));
        Assert.Equal("sh-toolbar-items", more.GetAttribute("aria-controls"));
        Assert.Null(toolbar.GetAttribute("data-expanded"));
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twSpreadsheet.observeToolbar");
    }

    [Fact]
    public async Task MoreTools_ExpandsTheToolbarOntoMoreLines_AndCollapsesItAgain()
    {
        var cut = Render();

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_MoreTools]).Click());

        Assert.Equal("true", ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_MoreTools]).GetAttribute("aria-expanded"));
        Assert.Equal("true", cut.Find("[role='toolbar']").GetAttribute("data-expanded"));
        Assert.Contains("flex-wrap", cut.Find("#sh-toolbar-items").ClassList);
        Assert.DoesNotContain("flex-nowrap", cut.Find("#sh-toolbar-items").ClassList);

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_MoreTools]).Click());

        Assert.Equal("false", ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_MoreTools]).GetAttribute("aria-expanded"));
        Assert.Null(cut.Find("[role='toolbar']").GetAttribute("data-expanded"));
        Assert.Contains("flex-nowrap", cut.Find("#sh-toolbar-items").ClassList);
    }

    [Fact]
    public void Toolbar_IsOnlyWatchedForOverflow_OnceWhileItIsShown()
    {
        var cut = Render();
        cut.Render();
        Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twSpreadsheet.observeToolbar");

        cut.Render(p => p.Add(x => x.ShowToolbar, false));
        cut.Render(p => p.Add(x => x.ShowToolbar, true));

        Assert.Equal(2, TestContext.JSInterop.Invocations.Count(i => i.Identifier == "twSpreadsheet.observeToolbar"));
    }

    #endregion

    #region Sheets

    [Fact]
    public void SheetTabs_AreATablist_ControllingTheGrid()
    {
        var cut = Render();

        var tabs = cut.FindAll("[role='tab']");
        Assert.Equal(["Budget", "Notes"], tabs.Select(t => t.TextContent));
        Assert.Equal("true", tabs[0].GetAttribute("aria-selected"));
        Assert.Equal("0", tabs[0].GetAttribute("tabindex"));
        Assert.Equal("-1", tabs[1].GetAttribute("tabindex"));
        Assert.Equal("sh-panel", tabs[0].GetAttribute("aria-controls"));
        Assert.Equal("tabpanel", cut.Find("#sh-panel").GetAttribute("role"));
        Assert.Equal("sh-tab-0", cut.Find("#sh-panel").GetAttribute("aria-labelledby"));
        Assert.Equal(Localizer[TwStrings.Spreadsheet_SheetTabs], cut.Find("[role='tablist']").GetAttribute("aria-label"));
    }

    [Fact]
    public async Task ClickingATab_ShowsTheSheet_AndReportsIt()
    {
        var index = -1;
        var cut = Render(configure: p => p.Add(x => x.ActiveSheetIndexChanged, i => index = i));

        await cut.InvokeAsync(() => cut.Find("#sh-tab-1").Click());

        Assert.Equal(1, index);
        Assert.Equal("Notes", cut.Instance.ActiveSheet.Name);
        Assert.Equal("750", Cell(cut, "A1").TextContent.Trim());
    }

    [Fact]
    public void ActiveSheetIndex_PicksTheSheetShown()
    {
        var cut = Render(configure: p => p.Add(x => x.ActiveSheetIndex, 1));

        Assert.Equal("Notes", cut.Instance.ActiveSheet.Name);
        Assert.Equal("true", cut.Find("#sh-tab-1").GetAttribute("aria-selected"));
    }

    [Theory]
    [InlineData("ArrowRight", 1)]
    [InlineData("ArrowLeft", 1)]
    [InlineData("End", 1)]
    [InlineData("Home", 0)]
    public async Task ArrowKeys_MoveBetweenTabs(string key, int expected)
    {
        var cut = Render();

        await cut.InvokeAsync(() => cut.Find("#sh-tab-0").KeyDown(new KeyboardEventArgs { Key = key }));

        Assert.Equal(expected, cut.FindAll("[role='tab']").ToList().FindIndex(t => t.GetAttribute("aria-selected") == "true"));
    }

    [Fact]
    public async Task AddSheet_AddsTheNextFreeName_AndShowsIt()
    {
        var sheets = Book();
        var cut = Render(sheets);

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_AddSheet]).Click());

        Assert.Equal(3, sheets.Count);
        Assert.Equal(Localizer[TwStrings.Spreadsheet_DefaultSheetName, 3], sheets[2].Name);
        Assert.Equal("true", cut.Find("#sh-tab-2").GetAttribute("aria-selected"));
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twFocus.focusById" && (string?)i.Arguments[0] == "sh-tab-2");
    }

    [Fact]
    public async Task AddSheetAsync_KeepsNamesUnique()
    {
        var sheets = Book();
        var cut = Render(sheets);

        await cut.InvokeAsync(() => cut.Instance.AddSheetAsync("budget"));

        Assert.Equal("budget (2)", sheets[^1].Name);
    }

    [Fact]
    public async Task F2OnATab_RenamesTheSheet_AndTheFormulasThatReferToIt()
    {
        var sheets = Book();
        var cut = Render(sheets);

        await cut.InvokeAsync(() => cut.Find("#sh-tab-0").KeyDown(new KeyboardEventArgs { Key = "F2" }));
        Assert.Empty(cut.FindAll("[role='tablist']"));
        var input = cut.Find("#sh-rename");
        Assert.Equal("Budget", input.GetAttribute("value"));
        Assert.Equal(Localizer[TwStrings.Spreadsheet_RenameSheet], input.GetAttribute("aria-label"));

        await cut.InvokeAsync(() => cut.Find("#sh-rename").Input("Costs 2026"));
        await cut.InvokeAsync(() => cut.Find("#sh-rename").KeyDown(new KeyboardEventArgs { Key = "Enter" }));

        Assert.Equal("Costs 2026", sheets[0].Name);
        Assert.Equal("='Costs 2026'!B4/2", sheets[1].Cells["A1"].Value);
        Assert.Equal("750", cut.Instance.GetDisplayText("A1", 1));
        Assert.NotEmpty(cut.FindAll("[role='tablist']"));
    }

    [Fact]
    public async Task Rename_EscapeKeepsTheName_AndDuplicatesAreRefused()
    {
        var sheets = Book();
        var cut = Render(sheets);

        await cut.InvokeAsync(() => cut.Find("#sh-tab-0").DoubleClick());
        await cut.InvokeAsync(() => cut.Find("#sh-rename").Input("Other"));
        await cut.InvokeAsync(() => cut.Find("#sh-rename").KeyDown(new KeyboardEventArgs { Key = "Escape" }));
        Assert.Equal("Budget", sheets[0].Name);

        Assert.False(await cut.InvokeAsync(() => cut.Instance.RenameSheetAsync(0, "notes")));
        Assert.False(await cut.InvokeAsync(() => cut.Instance.RenameSheetAsync(0, "  ")));
        Assert.True(await cut.InvokeAsync(() => cut.Instance.RenameSheetAsync(0, "Money")));
        Assert.Equal("Money", sheets[0].Name);
    }

    [Fact]
    public async Task RemoveSheetAsync_RemovesASheet_ButNotTheLast()
    {
        var sheets = Book();
        var cut = Render(sheets, p => p.Add(x => x.ActiveSheetIndex, 1));

        Assert.True(await cut.InvokeAsync(() => cut.Instance.RemoveSheetAsync(0)));
        Assert.Single(sheets);
        Assert.Equal("Notes", cut.Instance.ActiveSheet.Name);
        Assert.Equal(refError, Cell(cut, "A1").TextContent.Trim());
        Assert.False(await cut.InvokeAsync(() => cut.Instance.RemoveSheetAsync(0)));
    }

    private const string refError = "#REF!";

    [Fact]
    public void AddSheetButton_CanBeHidden()
    {
        var cut = Render(configure: p => p.Add(x => x.AllowAddSheet, false));

        Assert.DoesNotContain(cut.FindAll("button"), b => b.GetAttribute("aria-label") == Localizer[TwStrings.Spreadsheet_AddSheet]);
    }

    #endregion

    #region Read only

    [Fact]
    public async Task ReadOnly_AllowsMovingAndCopying_ButNotChanging()
    {
        var sheets = Book();
        var cut = Render(sheets, p => p.Add(x => x.ReadOnly, true));

        Assert.Equal("true", Grid(cut).GetAttribute("aria-readonly"));
        Assert.Equal("true", Grid(cut).GetAttribute("data-readonly"));
        Assert.Null(Grid(cut).GetAttribute("aria-describedby"));
        Assert.Empty(cut.FindAll("#sh-hint"));
        Assert.True(cut.Find("#sh-formula").HasAttribute("readonly"));
        Assert.DoesNotContain(cut.FindAll("button"), b => b.GetAttribute("aria-label") == Localizer[TwStrings.Spreadsheet_AddSheet]);

        await Key(cut, "Enter");
        await cut.InvokeAsync(() => cut.Instance.BeginTypingAsync());
        Assert.Empty(cut.FindAll("#sh-editor"));

        await Key(cut, "Delete");
        await Key(cut, "b", ctrl: true);
        await cut.InvokeAsync(() => cut.Instance.PasteTextAsync("x"));
        Assert.Equal("Item", sheets[0].Cells["A1"].Value);
        Assert.False(sheets[0].Cells["A1"].Bold);

        await Key(cut, "ArrowDown");
        Assert.Equal("A2", cut.Instance.ActiveCell);
        await Key(cut, "c", ctrl: true);
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twSpreadsheet.copyText");

        await cut.InvokeAsync(() => cut.Find("#sh-tab-0").KeyDown(new KeyboardEventArgs { Key = "F2" }));
        Assert.Empty(cut.FindAll("#sh-rename"));
    }

    #endregion

    #region Public methods and export

    [Fact]
    public async Task SetCellAsync_IsUndoable_AndReported()
    {
        var changes = new List<SheetChangeEventArgs>();
        var sheets = Book();
        var cut = Render(sheets, p => p.Add(x => x.OnCellsChange, e => changes.Add(e)));

        Assert.True(await cut.InvokeAsync(() => cut.Instance.SetCellAsync("b3", "700")));
        Assert.Equal("1900", cut.Instance.GetDisplayText("B4"));
        Assert.Equal(["B3"], changes.Single().Addresses);

        Assert.True(await cut.InvokeAsync(() => cut.Instance.SetCellAsync("A1", "Hi", 1)));
        Assert.Equal("Hi", sheets[1].Cells["A1"].Value);

        Assert.False(await cut.InvokeAsync(() => cut.Instance.SetCellAsync("nope", "1")));
        Assert.False(await cut.InvokeAsync(() => cut.Instance.SetCellAsync("A1", "1", 9)));
    }

    [Fact]
    public async Task SelectAsync_RejectsWhatIsNotARange()
    {
        var cut = Render();

        Assert.False(await cut.InvokeAsync(() => cut.Instance.SelectAsync("x")));
        Assert.False(await cut.InvokeAsync(() => cut.Instance.SelectAsync("A1:?")));
        Assert.True(await cut.InvokeAsync(() => cut.Instance.SelectAsync("Z99")));
        Assert.Equal("E10", cut.Instance.ActiveCell);
    }

    [Fact]
    public void GetDisplayText_IsEmpty_ForAnythingNotValid()
    {
        var cut = Render();

        Assert.Equal(string.Empty, cut.Instance.GetDisplayText("A1", 7));
        Assert.Equal(string.Empty, cut.Instance.GetDisplayText("nope"));
        Assert.Equal(string.Empty, cut.Instance.GetDisplayText("E9"));
    }

    [Fact]
    public async Task Refresh_WorksFormulasOutAfterChangesFromCode()
    {
        var sheets = Book();
        var cut = Render(sheets);

        sheets[0].Cells["B2"].Value = "100";
        Assert.Equal("1500", cut.Instance.GetDisplayText("B4"));

        await cut.InvokeAsync(cut.Instance.Refresh);
        Assert.Equal("400", Cell(cut, "B4").TextContent.Trim());
    }

    [Fact]
    public void ExportMenu_ListsTheFormats()
    {
        var cut = Render(configure: p => p.Add(x => x.ExportFormats, [SpreadsheetExportFormat.Excel, SpreadsheetExportFormat.Csv]));

        cut.FindAll("button").Single(b => b.GetAttribute("aria-label") == Localizer[TwStrings.TextEditor_Export]).Click();

        var items = cut.FindAll("[role='menuitem']").Select(i => i.TextContent.Trim()).ToList();
        Assert.Equal([$"{Localizer[TwStrings.Spreadsheet_ExportExcel]} (.xlsx)", $"{Localizer[TwStrings.Spreadsheet_ExportCsv]} (.csv)"], items);
    }

    [Fact]
    public async Task ExportAsync_SavesTheFile_AndAnnouncesIt()
    {
        var cut = Render(configure: p => p.Add(x => x.ExportFileName, "Costs"));

        var file = await cut.InvokeAsync(() => cut.Instance.ExportAsync(SpreadsheetExportFormat.Csv));

        Assert.Equal("Costs.csv", file.FileName);
        var save = TestContext.JSInterop.Invocations.Single(i => i.Identifier == "twDownload.save");
        Assert.Equal("Costs.csv", save.Arguments[0]);
        Assert.Contains(Localizer[TwStrings.TextEditor_Exported, "Costs.csv"], Status(cut));
    }

    [Fact]
    public async Task JumpTarget_StaysPut_AtTheEdge()
    {
        var cut = Render();

        await Key(cut, "ArrowUp", ctrl: true);
        await Key(cut, "ArrowLeft", ctrl: true);

        Assert.Equal("A1", cut.Instance.ActiveCell);
    }

    #endregion

    #region Zoom and status bar

    [Fact]
    public async Task ZoomButtons_StepThroughLevels_AndTheLabelResets()
    {
        var zooms = new List<int>();
        var cut = Render(configure: p => p.Add(x => x.ZoomChanged, z => zooms.Add(z)));

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_ZoomIn]).Click());
        Assert.Equal(110, cut.Instance.Zoom);
        Assert.Contains("zoom:1.1", Grid(cut).GetAttribute("style"));
        Assert.Equal("1.1", Grid(cut).GetAttribute("data-zoom"));
        Assert.Contains(Localizer[TwStrings.Spreadsheet_ZoomChanged, 110], Status(cut));

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_ZoomOut]).Click());
        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_ZoomOut]).Click());
        Assert.Equal(90, cut.Instance.Zoom);

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_ZoomReset, 90]).Click());
        Assert.Equal(100, cut.Instance.Zoom);
        Assert.Equal([110, 100, 90, 100], zooms);
        Assert.Equal("100%", ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_ZoomReset, 100]).TextContent.Trim());
    }

    [Fact]
    public async Task Zoom_IsKeptWithinItsLimits()
    {
        var cut = Render(configure: p => p.Add(x => x.Zoom, 1000));
        Assert.Equal(400, cut.Instance.Zoom);
        Assert.True(ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_ZoomIn]).HasAttribute("disabled"));

        await cut.InvokeAsync(() => cut.Instance.SetZoomAsync(5));
        Assert.Equal(25, cut.Instance.Zoom);
        Assert.True(ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_ZoomOut]).HasAttribute("disabled"));
    }

    [Fact]
    public async Task StatusBar_SumsTheSelectedNumbers()
    {
        var cut = Render();
        var footer = cut.Find("[aria-label='" + Localizer[TwStrings.Spreadsheet_Zoom] + "']").ParentElement!;
        Assert.DoesNotContain(Localizer[TwStrings.Spreadsheet_StatusCount, "1"], footer.TextContent);

        await cut.InvokeAsync(() => cut.Instance.SelectAsync("A1:B4"));

        var text = cut.Find("[aria-label='" + Localizer[TwStrings.Spreadsheet_Zoom] + "']").ParentElement!.TextContent;
        Assert.Contains(Localizer[TwStrings.Spreadsheet_StatusSum, "3000"], text);
        Assert.Contains(Localizer[TwStrings.Spreadsheet_StatusAverage, "1000"], text);
        Assert.Contains(Localizer[TwStrings.Spreadsheet_StatusCount, "7"], text);
    }

    [Fact]
    public void StatusBar_AndFooter_CanBeHidden()
    {
        var cut = Render(configure: p => p.Add(x => x.ShowStatusBar, false).Add(x => x.ShowSheetTabs, false));

        Assert.DoesNotContain(cut.FindAll("button"), b => b.GetAttribute("aria-label") == Localizer[TwStrings.Spreadsheet_ZoomIn]);
        Assert.Empty(cut.FindAll("[aria-label='" + Localizer[TwStrings.Spreadsheet_Zoom] + "']"));
    }

    #endregion

    #region Freeze panes and column widths

    [Fact]
    public async Task FreezePanes_MakesRowsAndColumnsSticky_AndKeepsTheActiveCellClear()
    {
        var sheets = Book();
        var cut = Render(sheets);
        await cut.InvokeAsync(() => cut.Instance.SelectAsync("C3"));

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_Freeze]).Click());
        await cut.InvokeAsync(() => cut.FindAll("[role='menuitem']").Single(i => i.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_FreezePanes]).Click());

        Assert.Equal(2, sheets[0].FrozenRows);
        Assert.Equal(2, sheets[0].FrozenColumns);
        Assert.Contains(Localizer[TwStrings.Spreadsheet_FrozenStatus, 2, 2], Status(cut));
        Assert.Equal("position:sticky;top:32px;inset-inline-start:48px;z-index:4", Cell(cut, "A1").GetAttribute("style"));
        Assert.Equal("position:sticky;top:64px;z-index:3", Cell(cut, "D2").GetAttribute("style"));
        Assert.Equal("position:sticky;inset-inline-start:152px;z-index:3", Cell(cut, "B5").GetAttribute("style"));
        Assert.Null(Cell(cut, "C3").GetAttribute("style"));
        Assert.Contains(theme.FrozenRowEdge.Split(' ')[0], Cell(cut, "D2").ClassList);
        Assert.Contains(theme.FrozenColumnEdge.Split(' ')[0], Cell(cut, "B5").ClassList);
        Assert.Contains(theme.CellFrozen.Split(' ')[0], Cell(cut, "A5").ClassList);
        Assert.Contains("scroll-padding-top:96px;scroll-padding-inline-start:256px", cut.Find("#sh-panel").GetAttribute("style"));
    }

    [Fact]
    public async Task FrozenPanes_ComeFromTheSheet_AndCanBeCleared()
    {
        var sheets = Book();
        sheets[0].FrozenRows = 1;
        var cut = Render(sheets);
        Assert.Contains("top:32px", Cell(cut, "E1").GetAttribute("style"));

        await cut.InvokeAsync(() => cut.Instance.FreezePanes(0, 0));

        Assert.Null(Cell(cut, "E1").GetAttribute("style"));
    }

    [Fact]
    public async Task Columns_AreResized_FromTheBrowserTheContextMenuAndCode()
    {
        var sheets = Book();
        var cut = Render(sheets);

        await cut.InvokeAsync(() => cut.Instance.ResizeColumn(1, 150));
        Assert.Equal(150, sheets[0].ColumnWidths["B"]);
        Assert.Equal("width:150px", cut.FindAll("col")[2].GetAttribute("style"));
        Assert.StartsWith("width:614px", Grid(cut).GetAttribute("style"));

        await cut.InvokeAsync(() => cut.Instance.ResizeColumn(1, 104));
        Assert.False(sheets[0].ColumnWidths.ContainsKey("B"));

        Assert.True(await cut.InvokeAsync(() => cut.Instance.SetColumnWidth("c", 5)));
        Assert.Equal(24, sheets[0].ColumnWidths["C"]);
        Assert.False(await cut.InvokeAsync(() => cut.Instance.SetColumnWidth("Z", 50)));
        Assert.False(await cut.InvokeAsync(() => cut.Instance.SetColumnWidth("?", 50)));

        await cut.InvokeAsync(() => cut.Instance.ResizeColumn(9, 50));
        Assert.Equal(5, cut.FindAll("[data-tw-spreadsheet-resize]").Count);
    }

    #endregion

    #region Fonts and colours

    [Fact]
    public async Task ColorMenus_ShadeAndColorTheSelection()
    {
        var sheets = Book();
        var cut = Render(sheets);
        await cut.InvokeAsync(() => cut.Instance.SelectAsync("A1:A2"));

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_FillColor]).Click());
        await cut.InvokeAsync(() => cut.FindAll("[role='menuitem']").Single(i => i.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_ColorYellow]).Click());
        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_TextColor]).Click());
        await cut.InvokeAsync(() => cut.FindAll("[role='menuitem']").Single(i => i.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_ColorRed]).Click());

        Assert.Equal("#FEF9C3", sheets[0].Cells["A2"].Background);
        Assert.Equal("#B91C1C", sheets[0].Cells["A2"].Color);
        Assert.Equal("background-color:#FEF9C3;color:#B91C1C", Cell(cut, "A1").GetAttribute("style"));

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_FillColor]).Click());
        await cut.InvokeAsync(() => cut.FindAll("[role='menuitem']").Single(i => i.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_NoFill]).Click());
        Assert.Null(sheets[0].Cells["A1"].Background);
    }

    [Fact]
    public void Fills_GetReadableText_AndUnsafeValuesAreIgnored()
    {
        var cut = Render([new Sheet
        {
            Cells =
            {
                ["A1"] = new() { Value = "a", Background = "#334155" },
                ["A2"] = new() { Value = "b", Background = "#fff" },
                ["A3"] = new() { Value = "c", Background = "red;position:fixed", Color = "url(x)", FontFamily = "x;}body{", FontSize = 500 },
                ["A4"] = new() { Value = "d", FontFamily = "Georgia", FontSize = 14 },
            },
        }]);

        Assert.Equal("background-color:#334155;color:#F9FAFB", Cell(cut, "A1").GetAttribute("style"));
        Assert.Equal("background-color:#fff;color:#111827", Cell(cut, "A2").GetAttribute("style"));
        Assert.Equal("font-size:72pt", Cell(cut, "A3").GetAttribute("style"));
        Assert.Equal("font-family:Georgia;font-size:14pt", Cell(cut, "A4").GetAttribute("style"));
    }

    [Fact]
    public async Task FontLists_OfferTheFontsGiven_AndSetTheSelection()
    {
        var sheets = Book();
        var cut = Render(sheets, p => p.Add(x => x.Fonts, ["Inter", "Bad;Font", "inter"]).Add(x => x.FontSizes, [12, 10, 500]));

        await cut.InvokeAsync(() => cut.Find("[role='combobox'][aria-label='" + Localizer[TwStrings.Spreadsheet_Font] + "']").Click());
        var fonts = cut.FindAll("[role='option']").Select(o => o.TextContent.Trim()).Where(t => t.Length > 0).ToList();
        Assert.Equal([Localizer[TwStrings.Spreadsheet_DefaultFont], "Inter"], fonts);
        await cut.InvokeAsync(() => cut.FindAll("[role='option']").Single(o => o.TextContent.Trim() == "Inter").Click());
        Assert.Equal("Inter", sheets[0].Cells["A1"].FontFamily);

        await cut.InvokeAsync(() => cut.Find("[role='combobox'][aria-label='" + Localizer[TwStrings.Spreadsheet_FontSize] + "']").Click());
        var sizes = cut.FindAll("[role='option']").Select(o => o.TextContent.Trim()).Where(t => t.Length > 0).ToList();
        Assert.Equal([Localizer[TwStrings.Spreadsheet_DefaultFont], "10", "12", "72"], sizes);
        await cut.InvokeAsync(() => cut.FindAll("[role='option']").Single(o => o.TextContent.Trim() == "12").Click());
        Assert.Equal(12, sheets[0].Cells["A1"].FontSize);
    }

    [Fact]
    public async Task CustomColors_AreOffered_WithTheirNames()
    {
        var cut = Render(configure: p => p.Add(x => x.FillColors, [new SpreadsheetColor { Name = "Brand", Value = "#123456" }]));

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_FillColor]).Click());

        var items = cut.FindAll("[role='menuitem']").Select(i => i.TextContent.Trim()).ToList();
        Assert.Equal([Localizer[TwStrings.Spreadsheet_NoFill], "Brand"], items);
    }

    #endregion

    #region Rows, columns and sorting

    [Fact]
    public async Task InsertRows_MovesCellsAndFormulas_OnEverySheet_AndCanBeUndone()
    {
        var sheets = Book();
        sheets[0].FrozenRows = 3;
        var cut = Render(sheets);

        await cut.InvokeAsync(() => cut.Instance.InsertRowsAsync(1));

        Assert.Equal("Item", sheets[0].Cells["A1"].Value);
        Assert.Equal("Rent", sheets[0].Cells["A3"].Value);
        Assert.Equal("=SUM(B3:B4)", sheets[0].Cells["B5"].Value);
        Assert.Equal("=Budget!B5/2", sheets[1].Cells["A1"].Value);
        Assert.Equal(4, sheets[0].FrozenRows);
        Assert.Equal("1500", Cell(cut, "B5").TextContent.Trim());

        await Key(cut, "z", ctrl: true);
        Assert.Equal("=SUM(B2:B3)", sheets[0].Cells["B4"].Value);
        Assert.Equal("=Budget!B4/2", sheets[1].Cells["A1"].Value);
        Assert.Equal(3, sheets[0].FrozenRows);

        await Key(cut, "y", ctrl: true);
        Assert.Equal("=SUM(B3:B4)", sheets[0].Cells["B5"].Value);
    }

    [Fact]
    public async Task DeleteRows_ShrinksRanges_AndBreaksReferencesToDeletedCells()
    {
        var sheets = Book();
        sheets[0].Cells["C1"] = new() { Value = "=B2" };
        var cut = Render(sheets);

        await cut.InvokeAsync(() => cut.Instance.DeleteRowsAsync(1));

        Assert.Equal("Food", sheets[0].Cells["A2"].Value);
        Assert.Equal("=SUM(B2:B2)", sheets[0].Cells["B3"].Value);
        Assert.Equal("=#REF!", sheets[0].Cells["C1"].Value);
        Assert.Equal("300", Cell(cut, "B3").TextContent.Trim());
    }

    [Fact]
    public async Task InsertAndDeleteColumns_MoveCellsWidthsAndFormulas()
    {
        var sheets = Book();
        sheets[0].ColumnWidths["B"] = 150;
        var cut = Render(sheets);

        await cut.InvokeAsync(() => cut.Instance.InsertColumnsAsync(1, 2));
        Assert.Equal("Cost", sheets[0].Cells["D1"].Value);
        Assert.Equal("=SUM(D2:D3)", sheets[0].Cells["D4"].Value);
        Assert.Equal(150, sheets[0].ColumnWidths["D"]);

        await cut.InvokeAsync(() => cut.Instance.DeleteColumnsAsync(0));
        Assert.Equal("Cost", sheets[0].Cells["C1"].Value);
        Assert.Equal("=Budget!C4/2", sheets[1].Cells["A1"].Value);
    }

    [Fact]
    public async Task RightClick_SelectsTheCell_UnlessItIsAlreadySelected()
    {
        var cut = Render();
        await cut.InvokeAsync(() => cut.Instance.SelectAsync("A2:A3"));

        await cut.InvokeAsync(() => cut.Instance.PointerContextAsync(2, 0));
        Assert.Equal("A2:A3", cut.Instance.SelectedRange);

        await cut.InvokeAsync(() => cut.Instance.PointerContextAsync(5, 3));
        Assert.Equal("D6", cut.Instance.SelectedRange);

        await cut.InvokeAsync(() => cut.Instance.PointerDownAsync(-1, 1, false));
        await cut.InvokeAsync(() => cut.Instance.PointerContextAsync(-1, 1));
        Assert.Equal("B1:B10", cut.Instance.SelectedRange);
    }

    [Fact]
    public async Task Sort_OrdersRowsByTheActiveColumn_KeepingHeadings()
    {
        List<Sheet> sheets =
        [
            new Sheet
            {
                Cells =
                {
                    ["A1"] = new() { Value = "Name" }, ["B1"] = new() { Value = "Score" },
                    ["A2"] = new() { Value = "Cy" }, ["B2"] = new() { Value = "20" },
                    ["A3"] = new() { Value = "Al" }, ["B3"] = new() { Value = "35" },
                    ["A4"] = new() { Value = "Bo" }, ["B4"] = new() { Value = "=5*2" },
                },
            },
        ];
        var cut = Render(sheets);
        await cut.InvokeAsync(() => cut.Instance.SelectAsync("B1"));

        await cut.InvokeAsync(() => cut.Instance.SortAsync());

        Assert.Equal("Name", sheets[0].Cells["A1"].Value);
        Assert.Equal(["Bo", "Cy", "Al"], new[] { "A2", "A3", "A4" }.Select(a => sheets[0].Cells[a].Value));
        Assert.Equal("=5*2", sheets[0].Cells["B2"].Value);

        await cut.InvokeAsync(() => cut.Instance.SortAsync(descending: true));
        Assert.Equal("Al", sheets[0].Cells["A2"].Value);

        await Key(cut, "z", ctrl: true);
        Assert.Equal("Bo", sheets[0].Cells["A2"].Value);
    }

    [Fact]
    public async Task Sort_OfASelection_PutsEmptyCellsLast()
    {
        List<Sheet> sheets = [new Sheet { Cells = { ["A1"] = new() { Value = "b" }, ["A3"] = new() { Value = "a" } } }];
        var cut = Render(sheets);
        await cut.InvokeAsync(() => cut.Instance.SelectAsync("A1:A3"));

        await cut.InvokeAsync(() => cut.Instance.SortAsync());

        Assert.Equal("a", sheets[0].Cells["A1"].Value);
        Assert.Equal("b", sheets[0].Cells["A2"].Value);
        Assert.False(sheets[0].Cells.ContainsKey("A3"));
    }

    [Fact]
    public async Task StructureChanges_AreBlocked_WhenReadOnly()
    {
        var sheets = Book();
        var cut = Render(sheets, p => p.Add(x => x.ReadOnly, true));

        await cut.InvokeAsync(() => cut.Instance.InsertRowsAsync(0));
        await cut.InvokeAsync(() => cut.Instance.DeleteColumnsAsync(0));
        await cut.InvokeAsync(() => cut.Instance.SortAsync());

        Assert.Equal("Item", sheets[0].Cells["A1"].Value);
    }

    #endregion

    #region Full screen

    [Fact]
    public async Task FullScreen_CoversTheWindow_AndEscapeLeavesIt()
    {
        var states = new List<bool>();
        var cut = Render(configure: p => p.Add(x => x.FullScreenChanged, on => states.Add(on)));
        var toggle = ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_FullScreen]);
        Assert.Equal("false", toggle.GetAttribute("aria-pressed"));

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_FullScreen]).Click());

        Assert.True(cut.Instance.FullScreen);
        Assert.Equal("true", ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_FullScreen]).GetAttribute("aria-pressed"));
        Assert.Contains(theme.FullScreen.Split(' ')[0], cut.Find("#sh-root").ClassList);
        Assert.Equal("true", cut.Find("#sh-root").GetAttribute("data-fullscreen"));
        Assert.StartsWith("height:auto", cut.Find("#sh-panel").GetAttribute("style"));
        Assert.Contains(Localizer[TwStrings.Spreadsheet_FullScreenOn], Status(cut));
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twSpreadsheet.setFullScreen" && (bool)i.Arguments[1]!);

        await Key(cut, "Escape");

        Assert.False(cut.Instance.FullScreen);
        Assert.Contains(Localizer[TwStrings.Spreadsheet_FullScreenOff], Status(cut));
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twSpreadsheet.setFullScreen" && !(bool)i.Arguments[1]!);
        Assert.Equal([true, false], states);
    }

    [Fact]
    public async Task FullScreen_EscapeFirstCollapsesASelection_AndExitWorksFromTheBrowser()
    {
        var cut = Render(configure: p => p.Add(x => x.FullScreen, true));
        await Key(cut, "ArrowRight", shift: true);

        await Key(cut, "Escape");
        Assert.True(cut.Instance.FullScreen);
        Assert.Equal("A1", cut.Instance.SelectedRange);

        await cut.InvokeAsync(() => cut.Instance.ExitFullScreenAsync());
        Assert.False(cut.Instance.FullScreen);
    }

    [Fact]
    public void FullScreenButton_CanBeHidden_AndColorItemsLineUp()
    {
        var cut = Render(configure: p => p.Add(x => x.AllowFullScreen, false));

        Assert.DoesNotContain(cut.FindAll("button"), b => b.GetAttribute("aria-label") == Localizer[TwStrings.Spreadsheet_FullScreen]);
        ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_FillColor]).Click();
        Assert.NotEmpty(cut.FindAll("[role='menuitem'] ." + theme.ColorItem.Split(' ')[0]));
    }

    #endregion

    #region Row heights and headings

    [Fact]
    public async Task Rows_AreResized_FromTheBrowserTheContextMenuAndCode()
    {
        var sheets = Book();
        var cut = Render(sheets);
        Assert.Equal("height:32px", cut.FindAll("tbody tr")[1].GetAttribute("style"));
        Assert.Equal(10, cut.FindAll("[data-tw-spreadsheet-row-resize]").Count);

        await cut.InvokeAsync(() => cut.Instance.ResizeRow(1, 60));
        Assert.Equal(60, sheets[0].RowHeights[2]);
        Assert.Equal("height:60px", cut.FindAll("tbody tr")[1].GetAttribute("style"));

        await cut.InvokeAsync(() => cut.Instance.SelectAsync("A2"));
        await cut.InvokeAsync(() => cut.Instance.SetRowHeight(2, 32));
        Assert.False(sheets[0].RowHeights.ContainsKey(2));

        Assert.True(await cut.InvokeAsync(() => cut.Instance.SetRowHeight(3, 1)));
        Assert.Equal(16, sheets[0].RowHeights[3]);
        Assert.False(await cut.InvokeAsync(() => cut.Instance.SetRowHeight(0, 50)));
        Assert.False(await cut.InvokeAsync(() => cut.Instance.SetRowHeight(11, 50)));
    }

    [Fact]
    public async Task FrozenRows_UseTheirHeights()
    {
        var sheets = Book();
        sheets[0].FrozenRows = 2;
        sheets[0].RowHeights[1] = 50;
        var cut = Render(sheets);

        Assert.Contains("top:82px", Cell(cut, "A2").GetAttribute("style"));
        Assert.Contains("scroll-padding-top:114px", cut.Find("#sh-panel").GetAttribute("style"));
        await Task.CompletedTask;
    }

    [Fact]
    public async Task LargerText_MakesTheRowTaller()
    {
        var sheets = Book();
        var cut = Render(sheets, p => p.Add(x => x.FontSizes, [24]));

        await cut.InvokeAsync(() => cut.Find("[role='combobox'][aria-label='" + Localizer[TwStrings.Spreadsheet_FontSize] + "']").Click());
        await cut.InvokeAsync(() => cut.FindAll("[role='option']").Single(o => o.TextContent.Trim() == "24").Click());

        Assert.Equal(48, sheets[0].RowHeights[1]);
    }

    [Fact]
    public async Task InsertingRows_MovesRowHeights()
    {
        var sheets = Book();
        sheets[0].RowHeights[3] = 70;
        var cut = Render(sheets);

        await cut.InvokeAsync(() => cut.Instance.InsertRowsAsync(0, 2));

        Assert.Equal(70, sheets[0].RowHeights[5]);
        Assert.False(sheets[0].RowHeights.ContainsKey(3));
    }

    [Fact]
    public void SelectedHeadings_AreSolid_SoCellsDoNotShowThrough()
    {
        Assert.DoesNotContain(theme.HeaderSelected.Split(' '), c => c.StartsWith("bg-", StringComparison.Ordinal) && c.Contains('/', StringComparison.Ordinal));
        Assert.DoesNotContain(theme.HeaderSelected.Split(' '), c => c.StartsWith("dark:bg-", StringComparison.Ordinal) && c.Contains('/', StringComparison.Ordinal));
    }

    #endregion

    #region Sheet menu and deleting sheets

    [Fact]
    public async Task SheetMenu_DeletesTheSheet_AfterConfirming_AndUndoBringsItBack()
    {
        var sheets = Book();
        var cut = Render(sheets, p => p.Add(x => x.ActiveSheetIndex, 0));

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_SheetMenu]).Click());
        await cut.InvokeAsync(() => cut.FindAll("[role='menuitem']").Single(i => i.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_DeleteSheet]).Click());

        var dialog = cut.Find("#sh-dialog");
        Assert.Equal("alertdialog", dialog.GetAttribute("role"));
        Assert.Equal("true", dialog.GetAttribute("aria-modal"));
        Assert.Equal(Localizer[TwStrings.Spreadsheet_DeleteSheetTitle, "Budget"], cut.Find("#sh-dialog-title").TextContent.Trim());
        Assert.Equal("sh-dialog-text", dialog.GetAttribute("aria-describedby"));
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twSpreadsheet.openDialog");
        Assert.Equal("true", cut.FindAll("#sh-dialog button").First(b => b.TextContent.Trim() == Localizer[TwStrings.Common_Cancel]).GetAttribute("data-autofocus"));

        await cut.InvokeAsync(() => cut.FindAll("#sh-dialog button").Single(b => b.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_DeleteSheet]).Click());

        Assert.Single(sheets);
        Assert.Empty(cut.FindAll("#sh-dialog"));
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twSpreadsheet.closeDialog");
        Assert.Contains(Localizer[TwStrings.Spreadsheet_SheetDeleted, "Budget"], Status(cut));
        Assert.Equal(refError, Cell(cut, "A1").TextContent.Trim());

        await Key(cut, "z", ctrl: true);
        Assert.Equal(["Budget", "Notes"], sheets.Select(s => s.Name));
        Assert.Equal("Budget", cut.Instance.ActiveSheet.Name);

        await Key(cut, "y", ctrl: true);
        Assert.Equal(["Notes"], sheets.Select(s => s.Name));
    }

    [Fact]
    public async Task DeleteDialog_CancelAndEscape_KeepTheSheet()
    {
        var sheets = Book();
        var cut = Render(sheets);

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_SheetMenu]).Click());
        await cut.InvokeAsync(() => cut.FindAll("[role='menuitem']").Single(i => i.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_DeleteSheet]).Click());
        await cut.InvokeAsync(() => cut.FindAll("#sh-dialog button").First(b => b.TextContent.Trim() == Localizer[TwStrings.Common_Cancel]).Click());
        Assert.Equal(2, sheets.Count);
        Assert.Empty(cut.FindAll("#sh-dialog"));

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_SheetMenu]).Click());
        await cut.InvokeAsync(() => cut.FindAll("[role='menuitem']").Single(i => i.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_DeleteSheet]).Click());
        await cut.InvokeAsync(() => cut.Instance.CloseDialogAsync());
        Assert.Equal(2, sheets.Count);
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twFocus.focusById" && (string?)i.Arguments[0] == "sh-tab-0");
    }

    [Fact]
    public async Task SheetMenu_MovesAndRenamesSheets_AndCannotDeleteTheLast()
    {
        var sheets = Book();
        var cut = Render(sheets);

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_SheetMenu]).Click());
        var left = cut.FindAll("[role='menuitem']").Single(i => i.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_MoveSheetLeft]);
        Assert.True(left.HasAttribute("disabled") || left.GetAttribute("aria-disabled") == "true");
        await cut.InvokeAsync(() => cut.FindAll("[role='menuitem']").Single(i => i.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_MoveSheetRight]).Click());

        Assert.Equal(["Notes", "Budget"], sheets.Select(s => s.Name));
        Assert.Equal("Budget", cut.Instance.ActiveSheet.Name);
        Assert.Equal("true", cut.Find("#sh-tab-1").GetAttribute("aria-selected"));

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_SheetMenu]).Click());
        await cut.InvokeAsync(() => cut.FindAll("[role='menuitem']").Single(i => i.TextContent.Trim().StartsWith(Localizer[TwStrings.Spreadsheet_RenameSheetAction], StringComparison.Ordinal)).Click());
        Assert.NotEmpty(cut.FindAll("#sh-rename"));

        var single = Render([new Sheet { Name = "Only" }]);
        await single.InvokeAsync(() => single.FindAll("button").Single(b => b.GetAttribute("aria-label") == Localizer[TwStrings.Spreadsheet_SheetMenu]).Click());
        var delete = single.FindAll("[role='menuitem']").Single(i => i.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_DeleteSheet]);
        Assert.True(delete.HasAttribute("disabled") || delete.GetAttribute("aria-disabled") == "true");
    }

    [Fact]
    public void SheetTabs_HaveAContextMenu_ThatIsATablist()
    {
        var cut = Render();

        var tablist = cut.Find("[role='tablist']");
        Assert.Equal("enabled", tablist.GetAttribute("data-tw-context-menu"));
        Assert.Equal(Localizer[TwStrings.Spreadsheet_SheetTabs], tablist.GetAttribute("aria-label"));
        Assert.Equal(2, tablist.QuerySelectorAll("[role='tab']").Length);
    }

    #endregion

    #region Tables and filters

    private static List<Sheet> Orders() =>
    [
        new Sheet
        {
            Cells =
            {
                ["A1"] = new() { Value = "Name" }, ["B1"] = new() { Value = "Status" },
                ["A2"] = new() { Value = "Ada" }, ["B2"] = new() { Value = "Open" },
                ["A3"] = new() { Value = "Bo" }, ["B3"] = new() { Value = "Closed" },
                ["A4"] = new() { Value = "Cy" }, ["B4"] = new() { Value = "Open" },
                ["A5"] = new() { Value = "Di" },
            },
        },
    ];

    [Fact]
    public async Task InsertTableDialog_CreatesATable_FromTheSelection()
    {
        var sheets = Orders();
        var cut = Render(sheets);
        await cut.InvokeAsync(() => cut.Instance.SelectAsync("A1:B5"));

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_InsertTable]).Click());

        Assert.Equal("dialog", cut.Find("#sh-dialog").GetAttribute("role"));
        Assert.Equal("A1:B5", cut.Find("#sh-table-range").GetAttribute("value"));
        await cut.InvokeAsync(() => cut.Find("#sh-dialog form").Submit());

        var table = Assert.Single(sheets[0].Tables);
        Assert.Equal("A1:B5", table.Range);
        Assert.Equal(Localizer[TwStrings.Spreadsheet_DefaultTableName, 1], table.Name);
        Assert.Contains(theme.TableHeader.Split(' ')[0], Cell(cut, "B1").ClassList);
        Assert.Contains(theme.TableBand.Split(' ')[0], Cell(cut, "A3").ClassList);
        Assert.DoesNotContain(theme.TableBand.Split(' ')[0], Cell(cut, "A2").ClassList);
        Assert.Equal(Localizer[TwStrings.Spreadsheet_FilterColumn, "Status"], Cell(cut, "B1").QuerySelector("[data-tw-spreadsheet-filter]")!.GetAttribute("aria-label"));
        Assert.Contains(Localizer[TwStrings.Spreadsheet_TableCreated, table.Name], Status(cut));
    }

    [Fact]
    public void FilterButton_TakesTheHeadingsTextColour_SoItStaysReadableOnALightFill()
    {
        var sheets = Orders();
        sheets[0].Tables.Add(new SheetTable { Range = "A1:B5" });
        sheets[0].Cells["B1"].Background = "#F3E8FF";
        var cut = Render(sheets);

        var heading = Cell(cut, "B1");
        var button = heading.QuerySelector("[data-tw-spreadsheet-filter]")!;
        Assert.Contains("color:#111827", heading.GetAttribute("style"));
        Assert.Contains("text-inherit", button.ClassList);
        Assert.Contains("bg-current/15", button.ClassList);
        Assert.DoesNotContain("text-white", button.ClassList);
        Assert.DoesNotContain("bg-white/20", button.ClassList);
    }

    [Fact]
    public void ActiveFilterButton_IsOutlinedInTheHeadingsTextColour()
    {
        var sheets = Orders();
        sheets[0].Tables.Add(new SheetTable { Range = "A1:B5", Filters = { ["B"] = ["Open"] } });
        sheets[0].Cells["B1"].Background = "#F3E8FF";
        var cut = Render(sheets);

        var button = Cell(cut, "B1").QuerySelector("[data-tw-spreadsheet-filter]")!;
        Assert.Contains("ring-current", button.ClassList);
        Assert.DoesNotContain("bg-white", button.ClassList);
    }

    [Fact]
    public async Task InsertTableDialog_ShowsAnError_ForABadOrOverlappingRange()
    {
        var sheets = Orders();
        sheets[0].Tables.Add(new SheetTable { Range = "A1:B3" });
        var cut = Render(sheets);

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_InsertTable]).Click());
        await cut.InvokeAsync(() => cut.Find("#sh-table-range").Input("nope"));
        await cut.InvokeAsync(() => cut.Find("#sh-dialog form").Submit());

        Assert.Equal("true", cut.Find("#sh-table-range").GetAttribute("aria-invalid"));
        Assert.Equal(Localizer[TwStrings.Spreadsheet_TableRangeInvalid], cut.Find("#sh-table-range-error").TextContent.Trim());

        await cut.InvokeAsync(() => cut.Find("#sh-table-range").Input("B2:C4"));
        Assert.Null(cut.Find("#sh-table-range").GetAttribute("aria-invalid"));
        await cut.InvokeAsync(() => cut.Find("#sh-dialog form").Submit());
        Assert.Single(sheets[0].Tables);
        Assert.Null(await cut.InvokeAsync(() => cut.Instance.AddTableAsync("A2:A4")));
    }

    [Fact]
    public async Task ATableWithoutHeaders_GetsAHeadingRowInserted()
    {
        var sheets = Orders();
        var cut = Render(sheets);

        var table = await cut.InvokeAsync(() => cut.Instance.AddTableAsync("A2:B4", hasHeaders: false));

        Assert.NotNull(table);
        Assert.Equal("A2:B5", table.Range);
        Assert.Equal(Localizer[TwStrings.Spreadsheet_DefaultColumnName, 1], sheets[0].Cells["A2"].Value);
        Assert.Equal(Localizer[TwStrings.Spreadsheet_DefaultColumnName, 2], sheets[0].Cells["B2"].Value);
        Assert.Equal("Ada", sheets[0].Cells["A3"].Value);

        await Key(cut, "z", ctrl: true);
        Assert.Empty(sheets[0].Tables);
        Assert.Equal("Ada", sheets[0].Cells["A2"].Value);
    }

    [Fact]
    public async Task Filter_HidesRows_AndClearShowsThemAgain()
    {
        var sheets = Orders();
        sheets[0].Tables.Add(new SheetTable { Range = "A1:B5" });
        var cut = Render(sheets);

        await cut.InvokeAsync(() => Cell(cut, "B1").QuerySelector("[data-tw-spreadsheet-filter]")!.Click());
        Assert.Equal(Localizer[TwStrings.Spreadsheet_FilterColumn, "Status"], cut.Find("#sh-dialog-title").TextContent.Trim());
        var boxes = cut.FindAll("#sh-dialog input[type='checkbox']");
        Assert.Equal(4, boxes.Count);
        Assert.Contains(Localizer[TwStrings.Spreadsheet_Blanks], cut.Find("#sh-dialog").TextContent);

        await cut.InvokeAsync(() => cut.FindAll("#sh-dialog label").Single(l => l.TextContent.Trim() == "Closed").QuerySelector("input")!.Change(false));
        await cut.InvokeAsync(() => cut.FindAll("#sh-dialog label").Single(l => l.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_Blanks]).QuerySelector("input")!.Change(false));
        await cut.InvokeAsync(() => cut.FindAll("#sh-dialog button").Single(b => b.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_ApplyFilter]).Click());

        Assert.Equal(["Open"], sheets[0].Tables[0].Filters["B"]);
        var rows = cut.FindAll("tbody tr");
        Assert.False(rows[1].HasAttribute("hidden"));
        Assert.True(rows[2].HasAttribute("hidden"));
        Assert.True(rows[4].HasAttribute("hidden"));
        Assert.Contains(Localizer[TwStrings.Spreadsheet_RowsShown, 2, 4], Status(cut));
        Assert.Equal(Localizer[TwStrings.Spreadsheet_FilterColumnActive, "Status"], Cell(cut, "B1").QuerySelector("[data-tw-spreadsheet-filter]")!.GetAttribute("aria-label"));

        await cut.InvokeAsync(() => cut.Instance.SelectAsync("A2"));
        await Key(cut, "ArrowDown");
        Assert.Equal("A4", cut.Instance.ActiveCell);

        await cut.InvokeAsync(() => cut.Instance.SelectAsync("B1"));
        await cut.InvokeAsync(() => Grid(cut).KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowDown", AltKey = true }));
        await cut.InvokeAsync(() => cut.FindAll("#sh-dialog button").Single(b => b.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_ClearFilter]).Click());

        Assert.Empty(sheets[0].Tables[0].Filters);
        Assert.DoesNotContain(cut.FindAll("tbody tr"), r => r.HasAttribute("hidden"));
    }

    [Fact]
    public async Task Filter_SelectAll_AndSortTheColumn()
    {
        var sheets = Orders();
        sheets[0].Tables.Add(new SheetTable { Range = "A1:B5", Filters = { ["B"] = ["Open"] } });
        var cut = Render(sheets);

        await cut.InvokeAsync(() => cut.Instance.SelectAsync("B1"));
        await cut.InvokeAsync(() => Grid(cut).KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowDown", AltKey = true }));
        await cut.InvokeAsync(() => cut.FindAll("#sh-dialog label").Single(l => l.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_SelectAllValues]).QuerySelector("input")!.Change(true));
        await cut.InvokeAsync(() => cut.FindAll("#sh-dialog button").Single(b => b.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_ApplyFilter]).Click());
        Assert.Empty(sheets[0].Tables[0].Filters);

        await cut.InvokeAsync(() => cut.Instance.SelectAsync("A1"));
        await cut.InvokeAsync(() => Grid(cut).KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowDown", AltKey = true }));
        await cut.InvokeAsync(() => cut.FindAll("#sh-dialog button").Single(b => b.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_SortDescending]).Click());

        Assert.Equal("Name", sheets[0].Cells["A1"].Value);
        Assert.Equal(["Di", "Cy", "Bo", "Ada"], new[] { "A2", "A3", "A4", "A5" }.Select(a => sheets[0].Cells[a].Value));
        Assert.Empty(cut.FindAll("#sh-dialog"));
    }

    [Fact]
    public async Task Tables_MoveWithInsertedColumns_AndCanBeRemoved()
    {
        var sheets = Orders();
        sheets[0].Tables.Add(new SheetTable { Range = "A1:B5", Filters = { ["B"] = ["Open"] } });
        var cut = Render(sheets);

        await cut.InvokeAsync(() => cut.Instance.InsertColumnsAsync(0));
        Assert.Equal("B1:C5", sheets[0].Tables[0].Range);
        Assert.Equal(["Open"], sheets[0].Tables[0].Filters["C"]);

        await cut.InvokeAsync(() => cut.Instance.DeleteRowsAsync(0));
        Assert.Empty(sheets[0].Tables);

        await Key(cut, "z", ctrl: true);
        Assert.Single(sheets[0].Tables);
        Assert.True(await cut.InvokeAsync(() => cut.Instance.RemoveTable(sheets[0].Tables[0])));
        Assert.Empty(sheets[0].Tables);
        Assert.Equal("Ada", sheets[0].Cells["B2"].Value);
    }

    [Fact]
    public void ParseRange_ReadsRangesAndSingleCells()
    {
        Assert.Equal("C3", TwSpreadsheet.ParseRange("c3").ToString());
        Assert.Equal("A1:B4", TwSpreadsheet.ParseRange("B4:A1").ToString());
        Assert.Null(TwSpreadsheet.ParseRange("A1:B2:C3"));
        Assert.Null(TwSpreadsheet.ParseRange("?"));
    }

    #endregion

    #region Condition filters, fitting and the footer

    [Fact]
    public async Task ConditionFilters_HideRowsThatDoNotMeetThem()
    {
        List<Sheet> sheets =
        [
            new Sheet
            {
                Tables = { new SheetTable { Range = "A1:B5" } },
                Cells =
                {
                    ["A1"] = new() { Value = "Item" }, ["B1"] = new() { Value = "Price" },
                    ["A2"] = new() { Value = "Tea" }, ["B2"] = new() { Value = "4" },
                    ["A3"] = new() { Value = "Cake" }, ["B3"] = new() { Value = "12" },
                    ["A4"] = new() { Value = "Coffee" }, ["B4"] = new() { Value = "6" },
                    ["A5"] = new() { Value = "Pie" }, ["B5"] = new() { Value = "20" },
                },
            },
        ];
        var cut = Render(sheets);

        await cut.InvokeAsync(() => cut.Instance.SelectAsync("B1"));
        await cut.InvokeAsync(() => Grid(cut).KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowDown", AltKey = true }));
        Assert.NotEmpty(cut.FindAll("#sh-dialog-conditions"));
        await cut.InvokeAsync(() => cut.Find("#sh-dialog-conditions-first-operator").Click());
        await cut.InvokeAsync(() => cut.FindAll("[role='option']").Single(o => o.TextContent.Trim() == Localizer["DataTable_OperatorIsGreaterThan"]).Click());
        await cut.InvokeAsync(() => cut.Find("#sh-dialog-conditions-first-value").Input("5"));
        await cut.InvokeAsync(() => cut.FindAll("#sh-dialog button").Single(b => b.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_ApplyFilter]).Click());

        var condition = sheets[0].Tables[0].Conditions["B"];
        Assert.Equal(FilterOperator.IsGreaterThan, condition.First.Operator);
        Assert.Null(condition.Second);
        var rows = cut.FindAll("tbody tr");
        Assert.True(rows[1].HasAttribute("hidden"));
        Assert.False(rows[2].HasAttribute("hidden"));
        Assert.Contains(Localizer[TwStrings.Spreadsheet_RowsShown, 3, 4], Status(cut));
        Assert.Equal(Localizer[TwStrings.Spreadsheet_FilterColumnActive, "Price"], Cell(cut, "B1").QuerySelector("[data-tw-spreadsheet-filter]")!.GetAttribute("aria-label"));

        await cut.InvokeAsync(() => cut.Instance.InsertColumnsAsync(0));
        Assert.True(sheets[0].Tables[0].Conditions.ContainsKey("C"));
    }

    [Fact]
    public async Task ConditionFilters_ComeFromTheTable_TextColumnsCompareText_AndTheFilterPartsCanBeHidden()
    {
        List<Sheet> sheets =
        [
            new Sheet
            {
                Tables = { new SheetTable { Range = "A1:A4", Conditions = { ["A"] = new ColumnFilter { First = new() { Operator = FilterOperator.StartsWith, Value = "c" } } } } },
                Cells = { ["A1"] = new() { Value = "Item" }, ["A2"] = new() { Value = "Tea" }, ["A3"] = new() { Value = "Cake" }, ["A4"] = new() { Value = "Coffee" } },
            },
        ];
        var cut = Render(sheets, p => p.Add(x => x.ShowFilterValues, false).Add(x => x.FilterOperators, [FilterOperator.StartsWith, FilterOperator.EndsWith]));

        Assert.True(cut.FindAll("tbody tr")[1].HasAttribute("hidden"));
        Assert.False(cut.FindAll("tbody tr")[2].HasAttribute("hidden"));

        await cut.InvokeAsync(() => Cell(cut, "A1").QuerySelector("[data-tw-spreadsheet-filter]")!.Click());
        Assert.Empty(cut.FindAll("#sh-dialog fieldset"));
        await cut.InvokeAsync(() => cut.Find("#sh-dialog-conditions-first-operator").Click());
        Assert.Equal(2, cut.FindAll("[role='option']").Count);
        await cut.InvokeAsync(() => cut.Instance.CloseDialogAsync());

        var noConditions = Render(Orders(), p => p.Add(x => x.ShowFilterConditions, false));
        Assert.True(await noConditions.InvokeAsync(() => noConditions.Instance.AddTableAsync("A1:B5")) is not null);
        await noConditions.InvokeAsync(() => noConditions.Instance.SelectAsync("B1"));
        await noConditions.InvokeAsync(() => noConditions.Find("#sh").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowDown", AltKey = true }));
        Assert.Empty(noConditions.FindAll("#sh-dialog-conditions"));
        Assert.NotEmpty(noConditions.FindAll("#sh-dialog fieldset"));
    }

    [Fact]
    public async Task FitToContents_UsesTheMeasuredSizes()
    {
        TestContext.JSInterop.Setup<int>("twSpreadsheet.fitColumn", _ => true).SetResult(180);
        TestContext.JSInterop.Setup<int>("twSpreadsheet.fitRow", _ => true).SetResult(48);
        var sheets = Book();
        var cut = Render(sheets);
        await cut.InvokeAsync(() => cut.Instance.SelectAsync("B2:C3"));

        var grid = cut.FindComponents<TwContextMenu>().Single(c => c.Instance.Id == "sh-panel");
        await cut.InvokeAsync(() => grid.Instance.OpenAtAsync(10, 10));
        var menu = cut.FindComponents<TwMenuItem>();
        await cut.InvokeAsync(() => menu.Single(m => m.Instance.Label == Localizer[TwStrings.Spreadsheet_FitColumn]).Instance.OnClick.InvokeAsync());
        await cut.InvokeAsync(() => grid.Instance.OpenAtAsync(10, 10));
        menu = cut.FindComponents<TwMenuItem>();
        await cut.InvokeAsync(() => menu.Single(m => m.Instance.Label == Localizer[TwStrings.Spreadsheet_FitRow]).Instance.OnClick.InvokeAsync());

        Assert.Equal(180, sheets[0].ColumnWidths["B"]);
        Assert.Equal(180, sheets[0].ColumnWidths["C"]);
        Assert.Equal(48, sheets[0].RowHeights[2]);
        Assert.Equal(48, sheets[0].RowHeights[3]);
    }

    [Fact]
    public void FullScreenButton_IsInTheFooter_EvenWithoutTabsOrStatusBar()
    {
        var cut = Render(configure: p => p.Add(x => x.ShowSheetTabs, false).Add(x => x.ShowStatusBar, false));

        var toggle = ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_FullScreen]);
        Assert.Null(toggle.Closest("[role='toolbar']"));
        Assert.NotEmpty(cut.FindAll("[role='toolbar']"));
    }

    #endregion

    #region Filter dialog layout

    [Fact]
    public async Task FilterSearch_NarrowsTheValues_AndSelectAllOnlyTouchesWhatIsShown()
    {
        var sheet = new Sheet { Tables = { new SheetTable { Range = "A1:A9" } }, Cells = { ["A1"] = new() { Value = "Fruit" } } };
        string[] fruit = ["Apple", "Apricot", "Banana", "Cherry", "Date", "Fig", "Grape", "Kiwi"];
        for (var i = 0; i < fruit.Length; i++)
        {
            sheet.Cells[$"A{i + 2}"] = new() { Value = fruit[i] };
        }

        var cut = Render([sheet]);
        await cut.InvokeAsync(() => Cell(cut, "A1").QuerySelector("[data-tw-spreadsheet-filter]")!.Click());

        Assert.Equal(Localizer[TwStrings.Spreadsheet_SearchValues], cut.Find("#sh-filter-search").GetAttribute("placeholder"));
        await cut.InvokeAsync(() => cut.Find("#sh-filter-search").Input("ap"));
        var labels = cut.FindAll("#sh-dialog input[type='checkbox']").Select(i => i.Closest("label")!.TextContent.Trim()).ToList();
        Assert.Equal([Localizer[TwStrings.Spreadsheet_SelectAllValues], "Apple", "Apricot", "Grape"], labels);

        await cut.InvokeAsync(() => cut.FindAll("#sh-dialog label").Single(l => l.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_SelectAllValues]).QuerySelector("input")!.Change(false));
        await cut.InvokeAsync(() => cut.FindAll("#sh-dialog button").Single(b => b.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_ApplyFilter]).Click());

        Assert.Equal(["Banana", "Cherry", "Date", "Fig", "Kiwi"], sheet.Tables[0].Filters["A"]);
    }

    [Fact]
    public async Task FilterDialog_HasLabelledSections_AndACloseButton()
    {
        var sheets = Orders();
        sheets[0].Tables.Add(new SheetTable { Range = "A1:B5" });
        var cut = Render(sheets);

        await cut.InvokeAsync(() => Cell(cut, "B1").QuerySelector("[data-tw-spreadsheet-filter]")!.Click());

        Assert.Empty(cut.FindAll("#sh-filter-search"));
        var headings = cut.FindAll("#sh-dialog h3, #sh-dialog legend").Select(h => h.TextContent.Trim()).ToList();
        Assert.Equal([Localizer[TwStrings.Spreadsheet_SortHeading], Localizer[TwStrings.Spreadsheet_FilterConditionHeading], Localizer[TwStrings.Spreadsheet_FilterValuesHeading]], headings);

        await cut.InvokeAsync(() => cut.FindAll("#sh-dialog button").Single(b => b.GetAttribute("aria-label") == Localizer[TwStrings.Common_Close]).Click());
        Assert.Empty(cut.FindAll("#sh-dialog"));
    }

    [Fact]
    public async Task DeleteDialog_IsPlain_WithoutAnIcon()
    {
        var cut = Render();

        await cut.InvokeAsync(() => ButtonNamed(cut, Localizer[TwStrings.Spreadsheet_SheetMenu]).Click());
        await cut.InvokeAsync(() => cut.FindAll("[role='menuitem']").Single(i => i.TextContent.Trim() == Localizer[TwStrings.Spreadsheet_DeleteSheet]).Click());

        Assert.Empty(cut.FindAll("#sh-dialog i, #sh-dialog svg"));
    }

    #endregion
}
