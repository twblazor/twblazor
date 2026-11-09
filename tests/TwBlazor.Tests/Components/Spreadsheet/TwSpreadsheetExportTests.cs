using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using TwBlazor.Components;
using TwBlazor.Enums;
using TwBlazor.Models;

namespace TwBlazor.Tests.Components.Spreadsheet;

public class TwSpreadsheetExportTests
{
    private static readonly XNamespace _main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

    private static List<Sheet> Book() =>
    [
        new Sheet
        {
            Name = "Costs",
            Cells =
            {
                ["A1"] = new() { Value = "Item", Bold = true },
                ["B1"] = new() { Value = "Price", Bold = true, Alignment = SheetCellAlignment.End },
                ["A2"] = new() { Value = "Tea, green" },
                ["B2"] = new() { Value = "2.5", Format = SheetNumberFormat.Currency },
                ["A3"] = new() { Value = "=1/0" },
                ["B3"] = new() { Value = "=SUM(B2:B2)*2", Italic = true, Format = SheetNumberFormat.Percent },
                ["C3"] = new() { Value = "TRUE" },
                ["D4"] = new() { Value = "=A1&\"!\"" },
                ["A5"] = new() { Value = "=C3" },
            },
        },
        new Sheet { Name = "Costs", Cells = { ["A1"] = new() { Value = "=Costs!B2" }, ["B1"] = new() { Value = "=A1/0" }, ["C1"] = new() { Value = "=C1" } } },
    ];

    private static Dictionary<string, XDocument> Unzip(byte[] bytes)
    {
        using var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
        return archive.Entries.ToDictionary(e => e.FullName, e =>
        {
            using var stream = e.Open();
            return XDocument.Load(stream);
        });
    }

    private static XElement CellOf(XDocument sheet, string address) =>
        sheet.Descendants(_main + "c").Single(c => (string?)c.Attribute("r") == address);

    [Fact]
    public void ToCsv_WritesResults_FromA1ToTheLastFilledCell()
    {
        var csv = TwSpreadsheetExport.ToCsv(Book());

        var lines = csv.Split("\r\n");
        Assert.Equal("Item,Price,,", lines[0]);
        Assert.Equal("\"Tea, green\",2.5,,", lines[1]);
        Assert.Equal("#DIV/0!,5,TRUE,", lines[2]);
        Assert.Equal(",,,Item!", lines[3]);
        Assert.Equal("TRUE,,,", lines[4]);
        Assert.Equal(string.Empty, lines[5]);
    }

    [Fact]
    public void ToCsv_GuardsTextThatLooksLikeAFormula_AndHandlesEmptySheets()
    {
        List<Sheet> book = [new Sheet { Cells = { ["A1"] = new() { Value = "'=HYPERLINK(1)" }, ["B1"] = new() { Value = "say \"hi\"" } } }];

        Assert.Equal("'=HYPERLINK(1),\"say \"\"hi\"\"\"\r\n", TwSpreadsheetExport.ToCsv(book));
        Assert.Equal(string.Empty, TwSpreadsheetExport.ToCsv([new Sheet()]));
        Assert.Throws<ArgumentOutOfRangeException>(() => TwSpreadsheetExport.ToCsv(Book(), 5));
    }

    [Fact]
    public void ToCsv_ReadsOtherSheets()
    {
        Assert.Equal("2.5,#DIV/0!,#CIRC!\r\n", TwSpreadsheetExport.ToCsv(Book(), 1));
    }

    [Fact]
    public void ToExcel_WritesEverySheet_WithUniqueNames()
    {
        var parts = Unzip(TwSpreadsheetExport.ToExcel(Book(), CultureInfo.GetCultureInfo("en-GB")));

        var sheets = parts["xl/workbook.xml"].Descendants(_main + "sheet").Select(s => (string?)s.Attribute("name")).ToList();
        Assert.Equal(["Costs", "Costs (2)"], sheets);
        Assert.Contains("xl/worksheets/sheet1.xml", parts.Keys);
        Assert.Contains("xl/worksheets/sheet2.xml", parts.Keys);
        Assert.Equal("1", (string?)parts["xl/workbook.xml"].Descendants(_main + "calcPr").Single().Attribute("fullCalcOnLoad"));
    }

    [Fact]
    public void ToExcel_KeepsFormulasWithTheirResults()
    {
        var parts = Unzip(TwSpreadsheetExport.ToExcel(Book(), CultureInfo.GetCultureInfo("en-GB")));
        var sheet = parts["xl/worksheets/sheet1.xml"];

        var sum = CellOf(sheet, "B3");
        Assert.Equal("SUM(B2:B2)*2", sum.Element(_main + "f")?.Value);
        Assert.Equal("5", sum.Element(_main + "v")?.Value);

        var error = CellOf(sheet, "A3");
        Assert.Equal("e", (string?)error.Attribute("t"));
        Assert.Equal("#DIV/0!", error.Element(_main + "v")?.Value);

        var text = CellOf(sheet, "D4");
        Assert.Equal("str", (string?)text.Attribute("t"));
        Assert.Equal("Item!", text.Element(_main + "v")?.Value);

        var boolean = CellOf(sheet, "A5");
        Assert.Equal("b", (string?)boolean.Attribute("t"));
        Assert.Equal("1", boolean.Element(_main + "v")?.Value);

        var circular = CellOf(parts["xl/worksheets/sheet2.xml"], "C1");
        Assert.Null(circular.Element(_main + "v"));
        Assert.Null(circular.Attribute("t"));
    }

    [Fact]
    public void ToExcel_WritesLiterals_AsNumbersBooleansAndText()
    {
        var sheet = Unzip(TwSpreadsheetExport.ToExcel(Book()))["xl/worksheets/sheet1.xml"];

        Assert.Equal("2.5", CellOf(sheet, "B2").Element(_main + "v")?.Value);
        Assert.Equal("b", (string?)CellOf(sheet, "C3").Attribute("t"));
        Assert.Equal("inlineStr", (string?)CellOf(sheet, "A2").Attribute("t"));
        Assert.Equal("Tea, green", CellOf(sheet, "A2").Descendants(_main + "t").Single().Value);
        Assert.Equal(["1", "2", "3", "4", "5"], sheet.Descendants(_main + "row").Select(r => (string?)r.Attribute("r")));
    }

    [Fact]
    public void ToExcel_WritesTheFormatting_AsStyles()
    {
        var parts = Unzip(TwSpreadsheetExport.ToExcel(Book(), CultureInfo.GetCultureInfo("en-GB")));
        var styles = parts["xl/styles.xml"];
        var sheet = parts["xl/worksheets/sheet1.xml"];
        var formats = styles.Descendants(_main + "cellXfs").Single().Elements(_main + "xf").ToList();
        var fonts = styles.Descendants(_main + "fonts").Single().Elements(_main + "font").ToList();

        XElement FormatOf(string address) => formats[int.Parse((string)CellOf(sheet, address).Attribute("s")!, CultureInfo.InvariantCulture)];

        var heading = FormatOf("B1");
        Assert.NotNull(fonts[int.Parse((string)heading.Attribute("fontId")!, CultureInfo.InvariantCulture)].Element(_main + "b"));
        Assert.Equal("right", (string?)heading.Element(_main + "alignment")?.Attribute("horizontal"));

        Assert.Equal("164", (string?)FormatOf("B2").Attribute("numFmtId"));
        Assert.Contains("£", (string?)styles.Descendants(_main + "numFmt").Single().Attribute("formatCode"));

        var percent = FormatOf("B3");
        Assert.Equal("10", (string?)percent.Attribute("numFmtId"));
        Assert.NotNull(fonts[int.Parse((string)percent.Attribute("fontId")!, CultureInfo.InvariantCulture)].Element(_main + "i"));
        Assert.Null(CellOf(sheet, "A2").Attribute("s"));
    }

    [Fact]
    public void Export_NamesTheFile_AndPicksTheMediaType()
    {
        var excel = TwSpreadsheetExport.Export(Book(), SpreadsheetExportFormat.Excel, "Q3: report");
        var csv = TwSpreadsheetExport.Export(Book(), SpreadsheetExportFormat.Csv, " ");

        Assert.Equal("Q3_ report.xlsx", excel.FileName);
        Assert.Equal(TwSpreadsheetExport.ExcelContentType, excel.ContentType);
        Assert.Equal("workbook.csv", csv.FileName);
        Assert.Equal(TwSpreadsheetExport.CsvContentType, csv.ContentType);
        Assert.Equal(Encoding.UTF8.GetPreamble(), csv.Content[..3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => TwSpreadsheetExport.Export(Book(), (SpreadsheetExportFormat)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => TwSpreadsheetExport.ExtensionOf((SpreadsheetExportFormat)99));
        Assert.Equal(".xlsx", TwSpreadsheetExport.ExtensionOf(SpreadsheetExportFormat.Excel));
    }

    [Fact]
    public void ToExcel_SkipsKeysThatAreNotAddresses()
    {
        List<Sheet> book = [new Sheet { Cells = { ["Total"] = new() { Value = "1" }, ["A1"] = new() { Value = "2" } } }];

        var sheet = Unzip(TwSpreadsheetExport.ToExcel(book))["xl/worksheets/sheet1.xml"];

        Assert.Single(sheet.Descendants(_main + "c"));
    }

    [Fact]
    public void ToExcel_WritesFillsFontsFrozenPanesAndWidths()
    {
        List<Sheet> book =
        [
            new Sheet
            {
                FrozenRows = 1,
                FrozenColumns = 2,
                ColumnWidths = { ["B"] = 152, ["?!"] = 10 },
                Cells =
                {
                    ["A1"] = new() { Value = "x", Background = "#fef3c7", Color = "#B91C1C", FontFamily = "Georgia", FontSize = 14 },
                    ["A2"] = new() { Value = "y", Background = "#1D4ED880" },
                },
            },
            new Sheet { FrozenRows = 3, Cells = { ["A1"] = new() { Value = "z" } } },
            new Sheet { FrozenColumns = 1, Cells = { ["A1"] = new() { Value = "w" } } },
        ];

        var parts = Unzip(TwSpreadsheetExport.ToExcel(book));
        var sheet = parts["xl/worksheets/sheet1.xml"];
        var styles = parts["xl/styles.xml"];

        var pane = sheet.Descendants(_main + "pane").Single();
        Assert.Equal("2", (string?)pane.Attribute("xSplit"));
        Assert.Equal("1", (string?)pane.Attribute("ySplit"));
        Assert.Equal("C2", (string?)pane.Attribute("topLeftCell"));
        Assert.Equal("bottomRight", (string?)pane.Attribute("activePane"));
        Assert.Equal("bottomLeft", (string?)parts["xl/worksheets/sheet2.xml"].Descendants(_main + "pane").Single().Attribute("activePane"));
        Assert.Equal("topRight", (string?)parts["xl/worksheets/sheet3.xml"].Descendants(_main + "pane").Single().Attribute("activePane"));

        var col = sheet.Descendants(_main + "col").Single();
        Assert.Equal("2", (string?)col.Attribute("min"));
        Assert.Equal("21", (string?)col.Attribute("width"));

        var fills = styles.Descendants(_main + "fill").ToList();
        Assert.Equal(4, fills.Count);
        Assert.Equal("FFFEF3C7", (string?)fills[2].Descendants(_main + "fgColor").Single().Attribute("rgb"));
        Assert.Equal("801D4ED8", (string?)fills[3].Descendants(_main + "fgColor").Single().Attribute("rgb"));

        var formats = styles.Descendants(_main + "cellXfs").Single().Elements(_main + "xf").ToList();
        var format = formats[int.Parse((string)CellOf(sheet, "A1").Attribute("s")!, CultureInfo.InvariantCulture)];
        Assert.Equal("2", (string?)format.Attribute("fillId"));
        Assert.Equal("1", (string?)format.Attribute("applyFill"));
        var font = styles.Descendants(_main + "font").ToList()[int.Parse((string)format.Attribute("fontId")!, CultureInfo.InvariantCulture)];
        Assert.Equal("14", (string?)font.Element(_main + "sz")?.Attribute("val"));
        Assert.Equal("FFB91C1C", (string?)font.Element(_main + "color")?.Attribute("rgb"));
        Assert.Equal("Georgia", (string?)font.Element(_main + "name")?.Attribute("val"));
        Assert.Empty(parts["xl/worksheets/sheet2.xml"].Descendants(_main + "cols"));
    }
}
