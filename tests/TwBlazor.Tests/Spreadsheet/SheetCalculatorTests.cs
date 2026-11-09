using System.Globalization;
using TwBlazor.Enums;
using TwBlazor.Models;
using TwBlazor.Spreadsheet;

namespace TwBlazor.Tests.Spreadsheet;

public class SheetCalculatorTests
{
    private static List<Sheet> Book(params (string Address, string Value)[] cells)
    {
        var sheet = new Sheet { Name = "Main" };
        foreach (var (address, value) in cells)
        {
            sheet.Cells[address] = new SheetCell { Value = value };
        }

        return [sheet];
    }

    private static SheetValue Evaluate(string formula, params (string Address, string Value)[] cells)
    {
        var book = Book([.. cells, ("Z99", formula)]);
        return new SheetCalculator(() => book).GetValue(0, CellAddress.Parse("Z99"));
    }

    private static double Number(string formula, params (string Address, string Value)[] cells)
    {
        var value = Evaluate(formula, cells);
        Assert.Equal(SheetValueKind.Number, value.Kind);
        return value.Number;
    }

    private static string? Error(string formula, params (string Address, string Value)[] cells)
    {
        var value = Evaluate(formula, cells);
        Assert.True(value.IsError, $"Expected an error but got {value.Kind} {value.AsText()}");
        return value.Text;
    }

    #region Addresses

    [Theory]
    [InlineData("A1", 0, 0)]
    [InlineData("b3", 2, 1)]
    [InlineData("$C$12", 11, 2)]
    [InlineData("AA10", 9, 26)]
    [InlineData("XFD1048576", 1048575, 16383)]
    public void CellAddress_ParsesA1Notation(string text, int row, int column)
    {
        Assert.True(CellAddress.TryParse(text, out var address));
        Assert.Equal(new CellAddress(row, column), address);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("1")]
    [InlineData("A0")]
    [InlineData("ABCD1")]
    [InlineData("XFE1")]
    [InlineData("A1048577")]
    [InlineData("A1B")]
    public void CellAddress_RejectsWhatIsNotAnAddress(string text)
    {
        Assert.False(CellAddress.TryParse(text, out _));
    }

    [Theory]
    [InlineData(0, "A")]
    [InlineData(25, "Z")]
    [InlineData(26, "AA")]
    [InlineData(701, "ZZ")]
    [InlineData(702, "AAA")]
    public void ColumnName_UsesSpreadsheetLetters(int column, string expected)
    {
        Assert.Equal(expected, CellAddress.ColumnName(column));
    }

    [Fact]
    public void CellRange_IsNormalized_AndCountsItsCells()
    {
        var range = new CellRange(CellAddress.Parse("C4"), CellAddress.Parse("A2"));

        Assert.Equal("A2:C4", range.ToString());
        Assert.Equal(3, range.RowCount);
        Assert.Equal(3, range.ColumnCount);
        Assert.Equal(9, range.Cells().Count());
        Assert.True(range.Contains(CellAddress.Parse("B3")));
        Assert.False(range.Contains(CellAddress.Parse("D3")));
        Assert.Equal("B2", new CellRange(CellAddress.Parse("B2"), CellAddress.Parse("B2")).ToString());
    }

    #endregion

    #region Literals

    [Theory]
    [InlineData("42", 42)]
    [InlineData("-3.5", -3.5)]
    [InlineData("1,234.5", 1234.5)]
    [InlineData("12.5%", 0.125)]
    [InlineData("1e3", 1000)]
    public void Literal_ReadsNumbers(string raw, double expected)
    {
        var value = SheetCalculator.ParseLiteral(raw);

        Assert.Equal(SheetValueKind.Number, value.Kind);
        Assert.Equal(expected, value.Number, 10);
    }

    [Fact]
    public void Literal_ReadsBooleansTextAndEmpty()
    {
        Assert.Equal(SheetValueKind.Boolean, SheetCalculator.ParseLiteral("true").Kind);
        Assert.Equal(0, SheetCalculator.ParseLiteral("FALSE").Number);
        Assert.Equal("hello", SheetCalculator.ParseLiteral("hello").Text);
        Assert.Equal("0123", SheetCalculator.ParseLiteral("'0123").Text);
        Assert.Equal(SheetValueKind.Empty, SheetCalculator.ParseLiteral(null).Kind);
        Assert.Equal(SheetValueKind.Empty, SheetCalculator.ParseLiteral(string.Empty).Kind);
    }

    [Fact]
    public void IsFormula_NeedsSomethingAfterTheEqualsSign()
    {
        Assert.True(SheetCalculator.IsFormula("=1"));
        Assert.False(SheetCalculator.IsFormula("="));
        Assert.False(SheetCalculator.IsFormula("1"));
        Assert.False(SheetCalculator.IsFormula(null));
    }

    #endregion

    #region Operators

    [Theory]
    [InlineData("=1+2*3", 7)]
    [InlineData("=(1+2)*3", 9)]
    [InlineData("=2^3^2", 64)]
    [InlineData("=-2^2", 4)]
    [InlineData("=10/4", 2.5)]
    [InlineData("=50%", 0.5)]
    [InlineData("=200*10%", 20)]
    [InlineData("=1.5e2", 150)]
    [InlineData("=+5", 5)]
    [InlineData("=7-2-1", 4)]
    public void Arithmetic_FollowsSpreadsheetPrecedence(string formula, double expected)
    {
        Assert.Equal(expected, Number(formula), 10);
    }

    [Fact]
    public void Arithmetic_ReadsCellsAndNumbersStoredAsText()
    {
        Assert.Equal(15, Number("=A1*B1", ("A1", "3"), ("B1", "5")));
        Assert.Equal(3, Number("=A1+B1", ("A1", "3")));
        Assert.Equal(4, Number("=A1+1", ("A1", "=\"3\"")));
        Assert.Equal(2, Number("=A1+1", ("A1", "TRUE")));
    }

    [Fact]
    public void Concatenation_JoinsText()
    {
        var value = Evaluate("=A1&\" \"&B1&\"!\"", ("A1", "Hello"), ("B1", "World"));

        Assert.Equal("Hello World!", value.Text);
        Assert.Equal("A\"B", Evaluate("=\"A\"\"B\"").Text);
        Assert.Equal("1.5TRUE", Evaluate("=1.5&TRUE").Text);
    }

    [Theory]
    [InlineData("=1<2", true)]
    [InlineData("=2<=2", true)]
    [InlineData("=3>4", false)]
    [InlineData("=\"apple\"=\"APPLE\"", true)]
    [InlineData("=\"a\"<\"b\"", true)]
    [InlineData("=1<\"a\"", true)]
    [InlineData("=\"z\"<TRUE", true)]
    [InlineData("=A1=0", true)]
    [InlineData("=A1=\"\"", true)]
    [InlineData("=1<>1", false)]
    [InlineData("=5>=6", false)]
    public void Comparison_OrdersNumbersTextAndBooleans(string formula, bool expected)
    {
        var value = Evaluate(formula);

        Assert.Equal(SheetValueKind.Boolean, value.Kind);
        Assert.Equal(expected, value.Number != 0);
    }

    [Fact]
    public void FormulaPointingAtAnEmptyCell_ShowsZero()
    {
        Assert.Equal(0, Number("=A1"));
    }

    #endregion

    #region Functions

    [Theory]
    [InlineData("=SUM(A1:A3)", 6)]
    [InlineData("=SUM(A1:A3, 10)", 16)]
    [InlineData("=sum(A1, A2)", 3)]
    [InlineData("=AVERAGE(A1:A3)", 2)]
    [InlineData("=MIN(A1:A3)", 1)]
    [InlineData("=MAX(A1:A3)", 3)]
    [InlineData("=PRODUCT(A1:A3)", 6)]
    [InlineData("=COUNT(A1:A5)", 3)]
    [InlineData("=COUNTA(A1:A5)", 4)]
    [InlineData("=COUNTIF(A1:A3, \">1\")", 2)]
    [InlineData("=COUNTIF(A1:A5, \"t*\")", 1)]
    [InlineData("=SUMIF(A1:A3, \">=2\")", 5)]
    [InlineData("=SUMIF(A1:A3, \"<>2\", B1:B3)", 40)]
    [InlineData("=SUMIF(A1:A3, 2, B1:B3)", 20)]
    public void Aggregates_SkipTextInCells(string formula, double expected)
    {
        Assert.Equal(expected, Number(formula, ("A1", "1"), ("A2", "2"), ("A3", "3"), ("A4", "text"), ("B1", "10"), ("B2", "20"), ("B3", "30")));
    }

    [Fact]
    public void Aggregates_OfNothing()
    {
        Assert.Equal(0, Number("=SUM(A1:A3)"));
        Assert.Equal(0, Number("=MAX(A1:A3)"));
        Assert.Equal(SheetValue.DivideByZero, Error("=AVERAGE(A1:A3)"));
        Assert.Equal(SheetValue.ValueError, Error("=SUM(\"abc\")"));
    }

    [Theory]
    [InlineData("=IF(A1>1, \"big\", \"small\")", "big")]
    [InlineData("=IF(A1>5, \"big\", \"small\")", "small")]
    [InlineData("=IFERROR(1/0, \"oops\")", "oops")]
    [InlineData("=UPPER(\"abc\")", "ABC")]
    [InlineData("=LOWER(\"ABC\")", "abc")]
    [InlineData("=TRIM(\"  a   b  \")", "a b")]
    [InlineData("=LEFT(\"spreadsheet\", 6)", "spread")]
    [InlineData("=LEFT(\"abc\")", "a")]
    [InlineData("=RIGHT(\"spreadsheet\", 5)", "sheet")]
    [InlineData("=MID(\"spreadsheet\", 7, 3)", "she")]
    [InlineData("=CONCAT(\"a\", A1, \"b\")", "a2b")]
    [InlineData("=CONCATENATE(B1:C1)", "xy")]
    public void TextAndConditionFunctions(string formula, string expected)
    {
        Assert.Equal(expected, Evaluate(formula, ("A1", "2"), ("B1", "x"), ("C1", "y")).AsText());
    }

    [Theory]
    [InlineData("=ROUND(2.675, 2)", 2.68)]
    [InlineData("=ROUND(-2.5)", -3)]
    [InlineData("=ROUND(1234, -2)", 1200)]
    [InlineData("=ROUNDUP(3.21, 1)", 3.3)]
    [InlineData("=ROUNDUP(-3.21, 0)", -4)]
    [InlineData("=ROUNDDOWN(3.29, 1)", 3.2)]
    [InlineData("=ROUNDDOWN(-3.9)", -3)]
    [InlineData("=ABS(-4)", 4)]
    [InlineData("=INT(-2.5)", -3)]
    [InlineData("=SQRT(16)", 4)]
    [InlineData("=POWER(2, 10)", 1024)]
    [InlineData("=MOD(10, 3)", 1)]
    [InlineData("=MOD(-10, 3)", 2)]
    [InlineData("=MOD(10, -3)", -2)]
    [InlineData("=LEN(\"hello\")", 5)]
    public void MathFunctions(string formula, double expected)
    {
        Assert.Equal(expected, Number(formula), 10);
    }

    [Fact]
    public void Pi_AndLogicalFunctions()
    {
        Assert.Equal(Math.PI, Number("=PI()"), 10);
        Assert.Equal(1, Evaluate("=AND(TRUE, 1, A1:A2)", ("A1", "TRUE"), ("A2", "text")).Number);
        Assert.Equal(0, Evaluate("=AND(TRUE, FALSE)").Number);
        Assert.Equal(1, Evaluate("=OR(FALSE, 1)").Number);
        Assert.Equal(1, Evaluate("=NOT(FALSE)").Number);
        Assert.Equal(SheetValueKind.Boolean, Evaluate("=IF(FALSE, 1)").Kind);
        Assert.Equal(SheetValue.ValueError, Error("=AND(A1:A2)"));
    }

    [Fact]
    public void If_OnlyWorksOutTheBranchItTakes()
    {
        Assert.Equal(1, Number("=IF(TRUE, 1, 1/0)"));
    }

    [Theory]
    [InlineData("=1/0", SheetValue.DivideByZero)]
    [InlineData("=MOD(1, 0)", SheetValue.DivideByZero)]
    [InlineData("=\"a\"+1", SheetValue.ValueError)]
    [InlineData("=NOPE(1)", SheetValue.NameError)]
    [InlineData("=nope", SheetValue.NameError)]
    [InlineData("=SQRT(-1)", SheetValue.NumberError)]
    [InlineData("=NA()", SheetValue.NotAvailable)]
    [InlineData("=#N/A+1", SheetValue.NotAvailable)]
    [InlineData("=SUM()", SheetValue.ValueError)]
    [InlineData("=ABS(1, 2)", SheetValue.ValueError)]
    [InlineData("=1+", SheetValue.SyntaxError)]
    [InlineData("=(1", SheetValue.SyntaxError)]
    [InlineData("=\"open", SheetValue.SyntaxError)]
    [InlineData("=1 2", SheetValue.SyntaxError)]
    [InlineData("=A1:B2", SheetValue.ValueError)]
    [InlineData("=Other!A1", SheetValue.ReferenceError)]
    [InlineData("=SUM(Other!A1:A2)", SheetValue.ReferenceError)]
    [InlineData("=SUM(A1:Z9999)", SheetValue.NumberError)]
    [InlineData("=LEFT(\"a\", -1)", SheetValue.ValueError)]
    [InlineData("=10^1000", SheetValue.NumberError)]
    public void Errors(string formula, string expected)
    {
        Assert.Equal(expected, Error(formula));
    }

    [Fact]
    public void Errors_SpreadThroughFormulas()
    {
        Assert.Equal(SheetValue.DivideByZero, Error("=A1+1", ("A1", "=1/0")));
        Assert.Equal(SheetValue.DivideByZero, Error("=SUM(A1:A2)", ("A1", "=1/0")));
        Assert.Equal(SheetValue.DivideByZero, Error("=A1&\"x\"", ("A1", "=1/0")));
    }

    [Fact]
    public void CircularReferences_ShowCircError()
    {
        var book = Book(("A1", "=B1+1"), ("B1", "=A1+1"), ("C1", "=C1"));
        var calculator = new SheetCalculator(() => book);

        Assert.Equal(SheetValue.CircularError, calculator.GetValue(0, CellAddress.Parse("A1")).Text);
        Assert.Equal(SheetValue.CircularError, calculator.GetValue(0, CellAddress.Parse("C1")).Text);
    }

    [Fact]
    public void References_CanPointAtOtherSheets_ByNameWithOrWithoutQuotes()
    {
        List<Sheet> book =
        [
            new Sheet { Name = "Main", Cells = { ["A1"] = new() { Value = "=Data!B2*2" }, ["A2"] = new() { Value = "='My data'!A1+SUM(data!B1:B2)" } } },
            new Sheet { Name = "Data", Cells = { ["B1"] = new() { Value = "1" }, ["B2"] = new() { Value = "21" } } },
            new Sheet { Name = "My data", Cells = { ["A1"] = new() { Value = "100" } } },
        ];
        var calculator = new SheetCalculator(() => book);

        Assert.Equal(42, calculator.GetValue(0, CellAddress.Parse("A1")).Number);
        Assert.Equal(122, calculator.GetValue(0, CellAddress.Parse("A2")).Number);
    }

    [Fact]
    public void Invalidate_WorksResultsOutAgain()
    {
        var book = Book(("A1", "1"), ("A2", "=A1*10"));
        var calculator = new SheetCalculator(() => book);
        Assert.Equal(10, calculator.GetValue(0, CellAddress.Parse("A2")).Number);

        book[0].Cells["A1"].Value = "5";
        Assert.Equal(10, calculator.GetValue(0, CellAddress.Parse("A2")).Number);

        calculator.Invalidate();
        Assert.Equal(50, calculator.GetValue(0, CellAddress.Parse("A2")).Number);
    }

    [Fact]
    public void GetValue_ForASheetThatDoesNotExist_IsARefError()
    {
        var book = Book();

        Assert.Equal(SheetValue.ReferenceError, new SheetCalculator(() => book).GetValue(3, new CellAddress(0, 0)).Text);
    }

    [Fact]
    public void FunctionNames_AreListed()
    {
        Assert.Contains("SUM", FormulaFunctions.Names);
        Assert.Contains("COUNTIF", FormulaFunctions.Names);
    }

    #endregion

    #region Formatting

    [Theory]
    [InlineData(1234.5, SheetNumberFormat.General, "1234.5")]
    [InlineData(1234.5, SheetNumberFormat.Number, "1,234.50")]
    [InlineData(1234.5, SheetNumberFormat.Currency, "£1,234.50")]
    [InlineData(0.125, SheetNumberFormat.Percent, "12.50%")]
    [InlineData(0.1 + 0.2, SheetNumberFormat.General, "0.3")]
    public void Display_FormatsNumbersInTheCulture(double number, SheetNumberFormat format, string expected)
    {
        Assert.Equal(expected, SheetFormatting.Display(SheetValue.FromNumber(number), format, CultureInfo.GetCultureInfo("en-GB")));
    }

    [Fact]
    public void Display_ShowsBooleansErrorsAndText()
    {
        var culture = CultureInfo.InvariantCulture;

        Assert.Equal("TRUE", SheetFormatting.Display(SheetValue.FromBoolean(true), SheetNumberFormat.Currency, culture));
        Assert.Equal("#DIV/0!", SheetFormatting.Display(SheetValue.Error(SheetValue.DivideByZero), SheetNumberFormat.General, culture));
        Assert.Equal("hi", SheetFormatting.Display(SheetValue.FromText("hi"), SheetNumberFormat.Number, culture));
        Assert.Equal(string.Empty, SheetFormatting.Display(SheetValue.Empty, SheetNumberFormat.General, culture));
    }

    [Fact]
    public void GeneralAlignment_FollowsTheKindOfValue()
    {
        Assert.Equal(SheetCellAlignment.End, SheetFormatting.GeneralAlignment(SheetValue.FromNumber(1)));
        Assert.Equal(SheetCellAlignment.Center, SheetFormatting.GeneralAlignment(SheetValue.FromBoolean(true)));
        Assert.Equal(SheetCellAlignment.Center, SheetFormatting.GeneralAlignment(SheetValue.Error(SheetValue.NameError)));
        Assert.Equal(SheetCellAlignment.Start, SheetFormatting.GeneralAlignment(SheetValue.FromText("a")));
    }

    [Theory]
    [InlineData("=A1+B1", 1, 0, "=A2+B2")]
    [InlineData("=A1+B1", 0, 2, "=C1+D1")]
    [InlineData("=$A$1+A$1+$A1", 1, 1, "=$A$1+B$1+$A2")]
    [InlineData("=SUM(A1:A3)*2", 2, 0, "=SUM(A3:A5)*2")]
    [InlineData("=A1&\"B1\"", 1, 0, "=A2&\"B1\"")]
    [InlineData("=Data!A1", 1, 0, "=Data!A2")]
    [InlineData("=A1", -1, 0, "=#REF!")]
    [InlineData("=A1", 0, 0, "=A1")]
    [InlineData("plain", 1, 1, "plain")]
    [InlineData("=\"broken", 1, 1, "=\"broken")]
    public void ShiftFormula_MovesRelativeReferences(string raw, int rows, int columns, string expected)
    {
        Assert.Equal(expected, SheetFormatting.ShiftFormula(raw, rows, columns));
    }

    [Theory]
    [InlineData("=Data!A1+data!B2", "Data", "Facts", "=Facts!A1+Facts!B2")]
    [InlineData("='Old name'!A1", "Old name", "New", "=New!A1")]
    [InlineData("=Data!A1", "Data", "My data", "='My data'!A1")]
    [InlineData("=Data!A1", "Data", "Q1", "='Q1'!A1")]
    [InlineData("=Data!A1", "Data", "Bob's", "='Bob''s'!A1")]
    [InlineData("=Other!A1", "Data", "New", "=Other!A1")]
    [InlineData("text", "Data", "New", "text")]
    public void RenameSheet_ChangesSheetNamesInReferences(string raw, string oldName, string newName, string expected)
    {
        Assert.Equal(expected, SheetFormatting.RenameSheet(raw, oldName, newName));
    }

    #endregion

    #region Structure and styles

    [Theory]
    [InlineData("=A1+A3", true, 1, 1, "=A1+A4")]
    [InlineData("=SUM(A1:A5)", true, 2, 2, "=SUM(A1:A7)")]
    [InlineData("=$B$3", true, 0, 1, "=$B$4")]
    [InlineData("=A3", true, 1, -1, "=A2")]
    [InlineData("=A2", true, 1, -1, "=#REF!")]
    [InlineData("=SUM(A1:A5)", true, 1, -2, "=SUM(A1:A3)")]
    [InlineData("=SUM(A2:A3)", true, 1, -2, "=SUM(#REF!)")]
    [InlineData("=SUM(A2:A4)", true, 0, -2, "=SUM(A1:A2)")]
    [InlineData("=C1", false, 1, 1, "=D1")]
    [InlineData("=C1", false, 1, -1, "=B1")]
    [InlineData("=SUM(A1:D1)", false, 1, -2, "=SUM(A1:B1)")]
    [InlineData("=Main!A3+Other!A3", true, 0, 1, "=Main!A4+Other!A3")]
    [InlineData("=\"A3\"&A3", true, 0, 1, "=\"A3\"&A4")]
    [InlineData("text", true, 0, 1, "text")]
    public void AdjustForStructure_FollowsMovedCells(string raw, bool rows, int index, int count, string expected)
    {
        Assert.Equal(expected, SheetFormatting.AdjustForStructure(raw, onChangedSheet: true, "Main", rows, index, count));
    }

    [Fact]
    public void AdjustForStructure_OnAnotherSheet_OnlyChangesReferencesNamingTheChangedSheet()
    {
        Assert.Equal("=Main!A4+A3", SheetFormatting.AdjustForStructure("=Main!A3+A3", onChangedSheet: false, "main", rows: true, 0, 1));
        Assert.Equal("=\"broken", SheetFormatting.AdjustForStructure("=\"broken", true, "Main", true, 0, 1));
        Assert.Equal("=A1", SheetFormatting.AdjustForStructure("=A1", true, "Main", true, 0, 0));
    }

    [Theory]
    [InlineData("#abc", "#abc")]
    [InlineData("#AABBCC", "#AABBCC")]
    [InlineData("#AABBCC80", "#AABBCC80")]
    [InlineData("red", null)]
    [InlineData("#GGGGGG", null)]
    [InlineData("#12345", null)]
    [InlineData("#fff;position:fixed", null)]
    [InlineData(null, null)]
    public void SafeColor_OnlyAllowsHex(string? color, string? expected)
    {
        Assert.Equal(expected, SheetFormatting.SafeColor(color));
    }

    [Theory]
    [InlineData("Times New Roman", "Times New Roman")]
    [InlineData(" 'Segoe UI', sans-serif ", "'Segoe UI', sans-serif")]
    [InlineData("Arial;color:red", null)]
    [InlineData("x}body{", null)]
    [InlineData("", null)]
    public void SafeFontFamily_RejectsAnythingThatCouldLeaveTheStyle(string family, string? expected)
    {
        Assert.Equal(expected, SheetFormatting.SafeFontFamily(family));
    }

    [Fact]
    public void SafeFontSize_IsKeptBetweenSixAndSeventyTwo()
    {
        Assert.Equal(6, SheetFormatting.SafeFontSize(1));
        Assert.Equal(72, SheetFormatting.SafeFontSize(100));
        Assert.Equal(11, SheetFormatting.SafeFontSize(11));
        Assert.Null(SheetFormatting.SafeFontSize(double.NaN));
        Assert.Null(SheetFormatting.SafeFontSize(null));
    }

    [Theory]
    [InlineData("#FFFFFF", "#111827")]
    [InlineData("#FEF9C3", "#111827")]
    [InlineData("#000", "#F9FAFB")]
    [InlineData("#334155", "#F9FAFB")]
    [InlineData("#1D4ED8CC", "#F9FAFB")]
    public void ReadableTextOn_PicksTheBetterContrast(string background, string expected)
    {
        Assert.Equal(expected, TwBlazor.Components.TwSpreadsheet.ReadableTextOn(background));
    }

    #endregion
}
