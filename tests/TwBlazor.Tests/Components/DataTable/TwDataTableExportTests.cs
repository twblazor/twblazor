using System.Globalization;
using System.IO.Compression;
using System.Xml.Linq;
using Bunit;
using TwBlazor.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.DataTable;

public class TwDataTableExportTests : TwBlazorTestBase
{
    private static readonly XNamespace _main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

    public TwDataTableExportTests()
    {
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(true);
    }

    private static List<Order> Orders() =>
    [
        new("Kettle", "Kitchen", 1234.5m, new DateTime(2026, 10, 1), true),
        new("Armchair", "Furniture", 250m, new DateTime(2026, 9, 14, 13, 30, 0), false),
        new("Blender", "Kitchen", 60m, new DateTime(2026, 8, 2), true)
    ];

    private static List<TwDataTableColumn<Order>> Columns() =>
    [
        new() { Name = "name", Title = "Name", PropertySelector = o => o.Name, IsSortable = true },
        new() { Name = "category", Title = "Category", PropertySelector = o => o.Category },
        new() { Name = "total", Title = "Total", PropertySelector = o => o.Total, CellFormatter = o => $"£{o.Total:N2}" },
        new() { Name = "placed", Title = "Placed", PropertySelector = o => o.Placed },
        new() { Name = "paid", Title = "Paid", PropertySelector = o => o.Paid }
    ];

    private static XDocument Sheet(byte[] workbook, string part = "xl/worksheets/sheet1.xml")
    {
        using var zip = new ZipArchive(new MemoryStream(workbook));
        using var stream = zip.GetEntry(part)!.Open();
        return XDocument.Load(stream);
    }

    private static List<List<string>> CsvLines(string csv) =>
        [.. csv.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Select(line => line.Split(',').ToList())];

    [Fact]
    public void ToCsv_WritesAHeadingRowThenARowPerItem()
    {
        // Act
        var csv = TwDataTableExport.ToCsv(Orders(), Columns());

        // Assert
        var lines = CsvLines(csv);
        Assert.Equal(["Name", "Category", "Total", "Placed", "Paid"], lines[0]);
        Assert.Equal(["Kettle", "Kitchen", "1234.5", "2026-10-01", "TRUE"], lines[1]);
        Assert.Equal(["Armchair", "Furniture", "250", "2026-09-14 13:30:00", "FALSE"], lines[2]);
        Assert.EndsWith("\r\n", csv);
    }

    [Fact]
    public void ToCsv_WritesNumbersTheSameWay_WhateverTheCulture()
    {
        // Arrange
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
        try
        {
            // Act
            var csv = TwDataTableExport.ToCsv(Orders(), Columns());

            // Assert
            Assert.Contains(",1234.5,", csv);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Theory]
    [InlineData("Smith, John", "\"Smith, John\"")]
    [InlineData("12\" ruler", "\"12\"\" ruler\"")]
    [InlineData("Line one\nLine two", "\"Line one\nLine two\"")]
    [InlineData("plain", "plain")]
    public void ToCsv_QuotesFieldsThatNeedIt(string value, string expected)
    {
        // Arrange
        List<TwDataTableColumn<string>> columns = [new() { Title = "Value", PropertySelector = s => s }];

        // Act
        var csv = TwDataTableExport.ToCsv([value], columns);

        // Assert
        Assert.Equal($"Value\r\n{expected}\r\n", csv);
    }

    [Theory]
    [InlineData("=SUM(A1:A2)")]
    [InlineData("+44 20 7946 0000")]
    [InlineData("-cmd")]
    [InlineData("@import")]
    public void ToCsv_GuardsTextThatASpreadsheetWouldRunAsAFormula(string value)
    {
        // Arrange
        List<TwDataTableColumn<string>> columns = [new() { Title = "Value", PropertySelector = s => s }];

        // Act
        var csv = TwDataTableExport.ToCsv([value], columns);

        // Assert
        Assert.StartsWith($"Value\r\n'{value[0]}", csv);
    }

    [Fact]
    public void ToCsv_LeavesNegativeNumbersAlone()
    {
        // Arrange
        List<TwDataTableColumn<int>> columns = [new() { Title = "Change", PropertySelector = n => n }];

        // Act
        var csv = TwDataTableExport.ToCsv([-5], columns);

        // Assert
        Assert.Equal("Change\r\n-5\r\n", csv);
    }

    [Fact]
    public void ToCsv_WritesOtherValueTypesInAnInvariantForm()
    {
        // Arrange
        List<TwDataTableColumn<Order>> columns =
        [
            new() { Title = "Offset", ExportFormatter = _ => new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.FromHours(1)) },
            new() { Title = "Date", ExportFormatter = _ => new DateOnly(2026, 1, 2) },
            new() { Title = "Time", ExportFormatter = _ => new TimeOnly(14, 5) },
            new() { Title = "Missing", ExportFormatter = _ => null },
            new() { Title = "Guid", ExportFormatter = _ => Guid.Empty },
            new() { Title = "Kind", ExportFormatter = _ => DayOfWeek.Monday }
        ];

        // Act
        var lines = CsvLines(TwDataTableExport.ToCsv(Orders().Take(1), columns));

        // Assert
        Assert.Equal(["2026-01-02 03:04:05+01:00", "2026-01-02", "14:05:00", "", Guid.Empty.ToString(), "Monday"], lines[1]);
    }

    [Fact]
    public void Export_PrefersTheExportFormatter_ThenThePropertySelector_ThenTheCellFormatter()
    {
        // Arrange
        List<TwDataTableColumn<Order>> columns =
        [
            new() { Title = "A", PropertySelector = o => o.Name, ExportFormatter = o => o.Name.ToUpperInvariant() },
            new() { Title = "B", PropertySelector = o => o.Name, CellFormatter = o => "formatted" },
            new() { Title = "C", CellFormatter = o => $"<{o.Name}>" }
        ];

        // Act
        var lines = CsvLines(TwDataTableExport.ToCsv(Orders().Take(1), columns));

        // Assert
        Assert.Equal(["KETTLE", "Kettle", "<Kettle>"], lines[1]);
    }

    [Fact]
    public void Export_LeavesOutColumnsThatAreNotExportable()
    {
        // Arrange
        var columns = Columns();
        columns[1].Exportable = false;

        // Act
        var lines = CsvLines(TwDataTableExport.ToCsv(Orders(), columns));

        // Assert
        Assert.Equal(["Name", "Total", "Placed", "Paid"], lines[0]);
    }

    [Fact]
    public void Export_RejectsNullArguments()
    {
        Assert.Throws<ArgumentNullException>(() => TwDataTableExport.ToCsv<Order>(null!, Columns()));
        Assert.Throws<ArgumentNullException>(() => TwDataTableExport.ToCsv(Orders(), null!));
        Assert.Throws<ArgumentNullException>(() => TwDataTableExport.ToExcel<Order>(null!, Columns()));
        Assert.Throws<ArgumentNullException>(() => TwDataTableExport.ToExcel(Orders(), null!));
    }

    [Fact]
    public void ToExcel_WritesAValidPackage()
    {
        // Act
        var workbook = TwDataTableExport.ToExcel(Orders(), Columns(), "Orders");

        // Assert
        using var zip = new ZipArchive(new MemoryStream(workbook));
        Assert.Equal(
            ["[Content_Types].xml", "_rels/.rels", "xl/_rels/workbook.xml.rels", "xl/styles.xml", "xl/workbook.xml", "xl/worksheets/sheet1.xml"],
            zip.Entries.Select(e => e.FullName).Order(StringComparer.Ordinal));
        Assert.Equal("Orders", Sheet(workbook, "xl/workbook.xml").Descendants(_main + "sheet").Single().Attribute("name")!.Value);
    }

    [Fact]
    public void ToExcel_WritesABoldHeadingRow_ThatStaysInView()
    {
        // Act
        var sheet = Sheet(TwDataTableExport.ToExcel(Orders(), Columns()));

        // Assert
        var headings = sheet.Descendants(_main + "row").First().Elements(_main + "c").ToList();
        Assert.Equal(["Name", "Category", "Total", "Placed", "Paid"], headings.Select(c => c.Value));
        Assert.All(headings, c => Assert.Equal("1", c.Attribute("s")!.Value));
        Assert.Equal("frozen", sheet.Descendants(_main + "pane").Single().Attribute("state")!.Value);
    }

    [Fact]
    public void ToExcel_KeepsNumbersDatesAndTrueOrFalseValuesAsTheirOwnTypes()
    {
        // Act
        var sheet = Sheet(TwDataTableExport.ToExcel(Orders(), Columns()));

        // Assert
        var cells = sheet.Descendants(_main + "row").ElementAt(1).Elements(_main + "c").ToDictionary(c => c.Attribute("r")!.Value);
        Assert.Equal("inlineStr", cells["A2"].Attribute("t")!.Value);
        Assert.Equal("Kettle", cells["A2"].Value);
        Assert.Null(cells["C2"].Attribute("t"));
        Assert.Equal("1234.5", cells["C2"].Value);
        Assert.Equal(new DateTime(2026, 10, 1).ToOADate().ToString(CultureInfo.InvariantCulture), cells["D2"].Value);
        Assert.Equal("2", cells["D2"].Attribute("s")!.Value);
        Assert.Equal("b", cells["E2"].Attribute("t")!.Value);
        Assert.Equal("1", cells["E2"].Value);

        var timed = sheet.Descendants(_main + "row").ElementAt(2).Elements(_main + "c").Single(c => c.Attribute("r")!.Value == "D3");
        Assert.Equal("3", timed.Attribute("s")!.Value);
    }

    [Fact]
    public void ToExcel_WritesOtherValueTypes()
    {
        // Arrange
        List<TwDataTableColumn<Order>> columns =
        [
            new() { Title = "Offset", ExportFormatter = _ => new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero) },
            new() { Title = "Date", ExportFormatter = _ => new DateOnly(2026, 1, 2) },
            new() { Title = "Time", ExportFormatter = _ => new TimeOnly(12, 0) },
            new() { Title = "Missing", ExportFormatter = _ => null },
            new() { Title = "Infinite", ExportFormatter = _ => double.PositiveInfinity },
            new() { Title = "NaN", ExportFormatter = _ => float.NaN },
            new() { Title = "Kind", ExportFormatter = _ => DayOfWeek.Monday }
        ];

        // Act
        var sheet = Sheet(TwDataTableExport.ToExcel(Orders().Take(1), columns));

        // Assert
        var cells = sheet.Descendants(_main + "row").ElementAt(1).Elements(_main + "c").ToDictionary(c => c.Attribute("r")!.Value);
        Assert.Equal("3", cells["A2"].Attribute("s")!.Value);
        Assert.Equal("2", cells["B2"].Attribute("s")!.Value);
        Assert.Equal("4", cells["C2"].Attribute("s")!.Value);
        Assert.Equal("0.5", cells["C2"].Value);
        Assert.False(cells.ContainsKey("D2"));
        Assert.Equal("Infinity", cells["E2"].Value);
        Assert.Equal("NaN", cells["F2"].Value);
        Assert.Equal("Monday", cells["G2"].Value);
    }

    [Fact]
    public void ToExcel_DropsCharactersThatXmlCannotHold()
    {
        // Arrange
        List<TwDataTableColumn<string>> columns = [new() { Title = "Value", PropertySelector = s => s }];

        // Act
        var sheet = Sheet(TwDataTableExport.ToExcel(["bell\u0007 and 😀"], columns));

        // Assert
        Assert.Equal("bell and 😀", sheet.Descendants(_main + "row").ElementAt(1).Value);
    }

    [Theory]
    [InlineData(0, "A")]
    [InlineData(25, "Z")]
    [InlineData(26, "AA")]
    [InlineData(701, "ZZ")]
    [InlineData(702, "AAA")]
    public void ColumnLetters_FollowExcelsNaming(int index, string expected)
    {
        Assert.Equal(expected, TwDataTableExport.ColumnLetters(index));
    }

    [Theory]
    [InlineData("Q3 [draft]: sales/returns?", "Q3 _draft__ sales_returns_")]
    [InlineData("'quoted'", "quoted")]
    [InlineData("   ", "Sheet1")]
    [InlineData(null, "Sheet1")]
    [InlineData("A very long worksheet name that keeps going", "A very long worksheet name that")]
    public void SanitizeSheetName_MakesANameExcelAccepts(string? name, string expected)
    {
        Assert.Equal(expected, TwDataTableExport.SanitizeSheetName(name));
    }

    private IRenderedComponent<TwDataTable<Order>> Render(Action<ComponentParameterCollectionBuilder<TwDataTable<Order>>>? configure = null) =>
        TestContext.Render<TwDataTable<Order>>(parameters =>
        {
            parameters
                .Add(p => p.Id, "orders")
                .Add(p => p.Items, Orders())
                .Add(p => p.Columns, Columns())
                .Add(p => p.Searchable, true)
                .Add(p => p.RowsPerPage, 2)
                .Add(p => p.ExportFileName, "orders");
            configure?.Invoke(parameters);
        });

    [Fact]
    public void ExportButtons_AreHidden_ByDefault()
    {
        // Act
        var cut = Render();

        // Assert
        Assert.Empty(cut.FindAll("#orders-export-csv, #orders-export-excel"));
    }

    [Fact]
    public void ExportButtons_AreShown_WhenExportable()
    {
        // Act
        var cut = Render(p => p.Add(x => x.Exportable, true));

        // Assert
        Assert.Contains("Export CSV", cut.Find("#orders-export-csv").TextContent);
        Assert.Contains("Export Excel", cut.Find("#orders-export-excel").TextContent);
    }

    [Theory]
    [InlineData(TwDataTableExportFormats.Csv, true, false)]
    [InlineData(TwDataTableExportFormats.Excel, false, true)]
    [InlineData(TwDataTableExportFormats.None, false, false)]
    public void ExportFormats_ChoosesTheButtons(TwDataTableExportFormats formats, bool csv, bool excel)
    {
        // Act
        var cut = Render(p => p.Add(x => x.Exportable, true).Add(x => x.ExportFormats, formats));

        // Assert
        Assert.Equal(csv, cut.FindAll("#orders-export-csv").Count == 1);
        Assert.Equal(excel, cut.FindAll("#orders-export-excel").Count == 1);
    }

    [Fact]
    public async Task GetCsv_UsesTheSearchSortAndColumnsOnScreen()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.State, new TwDataTableState
        {
            Columns = [new() { Name = "total" }, new() { Name = "name" }, new() { Name = "category", Visible = false }, new() { Name = "placed", Visible = false }, new() { Name = "paid", Visible = false }]
        }));
        cut.Find("[aria-label='Sort by Name']").Click();
        await cut.InvokeAsync(() => cut.Find("#orders-search").Change("Kitchen"));

        // Act
        var lines = CsvLines(cut.Instance.GetCsv());

        // Assert
        Assert.Equal([["Total", "Name"], ["60", "Blender"], ["1234.5", "Kettle"]], lines);
    }

    [Fact]
    public void GetCsv_CurrentPage_OnlyWritesTheRowsOnScreen()
    {
        // Arrange
        var cut = Render();

        // Act
        var lines = CsvLines(cut.Instance.GetCsv(TwDataTableExportScope.CurrentPage));

        // Assert
        Assert.Equal(3, lines.Count);
    }

    [Fact]
    public async Task GetCsv_All_IgnoresTheSearch()
    {
        // Arrange
        var cut = Render();
        await cut.InvokeAsync(() => cut.Find("#orders-search").Change("Kitchen"));

        // Act
        var lines = CsvLines(cut.Instance.GetCsv(TwDataTableExportScope.All));

        // Assert
        Assert.Equal(4, lines.Count);
    }

    [Fact]
    public void GetExcel_NamesTheWorksheetAfterTheFile()
    {
        // Arrange
        var cut = Render();

        // Act
        var workbook = cut.Instance.GetExcel();

        // Assert
        Assert.Equal("orders", Sheet(workbook, "xl/workbook.xml").Descendants(_main + "sheet").Single().Attribute("name")!.Value);
        Assert.Equal(4, Sheet(workbook).Descendants(_main + "row").Count());
    }

    [Fact]
    public void ExportCsvButton_DownloadsTheFile_AndAnnouncesHowManyRows()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.Exportable, true));

        // Act
        cut.Find("#orders-export-csv").Click();

        // Assert
        var download = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twDataTable.download");
        Assert.Equal("orders.csv", download.Arguments[0]);
        Assert.Equal(TwDataTableExport.CsvContentType, download.Arguments[1]);
        Assert.Equal("Exported 3 rows", cut.Find("[role='status']").TextContent.Trim());
    }

    [Fact]
    public void ExportExcelButton_DownloadsAWorkbook()
    {
        // Arrange
        var cut = Render(p => p.Add(x => x.Exportable, true).Add(x => x.ExportScope, TwDataTableExportScope.CurrentPage));

        // Act
        cut.Find("#orders-export-excel").Click();

        // Assert
        var download = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twDataTable.download");
        Assert.Equal("orders.xlsx", download.Arguments[0]);
        Assert.Equal(TwDataTableExport.ExcelContentType, download.Arguments[1]);
        Assert.Equal("Exported 2 rows", cut.Find("[role='status']").TextContent.Trim());
    }

    [Fact]
    public async Task ExportCsvAsync_WritesAByteOrderMark_SoExcelReadsUnicode()
    {
        // Arrange
        byte[]? downloaded = null;
        var cut = Render();
        TestContext.JSInterop.SetupVoid("twDataTable.download", invocation =>
        {
            var reference = (Microsoft.JSInterop.DotNetStreamReference)invocation.Arguments[2]!;
            using var copy = new MemoryStream();
            reference.Stream.CopyTo(copy);
            downloaded = copy.ToArray();
            return true;
        }).SetVoidResult();

        // Act
        await cut.InvokeAsync(() => cut.Instance.ExportCsvAsync());

        // Assert
        Assert.NotNull(downloaded);
        Assert.Equal([0xEF, 0xBB, 0xBF], downloaded.Take(3));
    }

    public record Order(string Name, string Category, decimal Total, DateTime Placed, bool Paid);
}
