// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.Xml;
using TwBlazor.Components;
using TwBlazor.Enums;
using TwBlazor.Export.Packaging;
using TwBlazor.Models;
using TwBlazor.Spreadsheet;

namespace TwBlazor.Export.Spreadsheet;

/// <summary>
/// Writes a <see cref="TwSpreadsheet"/> workbook as an Excel file: one worksheet per sheet, formulas with their last
/// results so the file shows them before Excel recalculates, and each cell's bold, italic, underline, alignment and
/// number format.
/// </summary>
internal static class XlsxWorkbookWriter
{
    private const string spreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string countAttribute = "count";
    private const string currencyFormatId = "164";

    // The error codes Excel can store; any other result of a formula is left for Excel to work out again.
    private static readonly HashSet<string> _excelErrors = new(StringComparer.Ordinal)
    {
        SheetValue.DivideByZero, SheetValue.ValueError, SheetValue.ReferenceError, SheetValue.NameError,
        SheetValue.NumberError, SheetValue.NotAvailable,
    };

    /// <summary>
    /// The look of a cell, which picks its entry in the styles part.
    /// </summary>
    private readonly record struct CellStyle(bool Bold, bool Italic, bool Underline, SheetCellAlignment Alignment, SheetNumberFormat Format, string? Fill, string? FontColor, string? FontFamily, double? FontSize)
    {
        public FontKey Font => new(Bold, Italic, Underline, FontColor, FontFamily, FontSize);
    }

    /// <summary>
    /// The parts of a cell's look that make up its font entry in the styles part.
    /// </summary>
    private readonly record struct FontKey(bool Bold, bool Italic, bool Underline, string? Color, string? Family, double? Size);

    /// <summary>
    /// Writes the workbook.
    /// </summary>
    /// <param name="sheets">The sheets, in tab order.</param>
    /// <param name="culture">The culture whose currency symbol currency cells are saved with.</param>
    /// <returns>The contents of an <c>.xlsx</c> file.</returns>
    public static byte[] Write(IReadOnlyList<Sheet> sheets, CultureInfo culture)
    {
        var calculator = new SheetCalculator(() => sheets);
        var names = UniqueSheetNames(sheets);
        var styles = new List<CellStyle> { default };

        // The worksheets are written first, so every style they use is known when the styles part is written.
        var worksheets = new List<Action<XmlWriter>>();
        for (var i = 0; i < sheets.Count; i++)
        {
            var index = i;
            var cells = SortedCells(sheets[index]).Select(c => (c.Address, c.Cell, Style: StyleIndex(styles, c.Cell))).ToList();
            worksheets.Add(writer => WriteWorksheet(writer, sheets[index], cells, address => calculator.GetValue(index, address)));
        }

        using var package = new ZipPackage();
        package.AddXml("[Content_Types].xml", writer => OpenXmlParts.WriteContentTypes(writer,
        [
            ("/xl/workbook.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"),
            ("/xl/styles.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"),
            .. Enumerable.Range(1, sheets.Count).Select(n => ($"/xl/worksheets/sheet{n}.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml")),
        ]));
        package.AddXml("_rels/.rels", writer => OpenXmlParts.WriteRelationships(writer,
            [new("rId1", OpenXmlParts.OfficeDocumentRelationship, "xl/workbook.xml")]));
        package.AddXml("xl/workbook.xml", writer => WriteWorkbook(writer, names));
        package.AddXml("xl/_rels/workbook.xml.rels", writer => OpenXmlParts.WriteRelationships(writer,
        [
            .. Enumerable.Range(1, sheets.Count).Select(n => new OpenXmlRelationship($"rId{n}", OpenXmlParts.WorksheetRelationship, $"worksheets/sheet{n}.xml")),
            new($"rId{sheets.Count + 1}", OpenXmlParts.StylesRelationship, "styles.xml"),
        ]));

        for (var i = 0; i < worksheets.Count; i++)
        {
            package.AddXml($"xl/worksheets/sheet{i + 1}.xml", worksheets[i]);
        }

        package.AddXml("xl/styles.xml", writer => WriteStyles(writer, styles, culture));
        return package.ToArray();
    }

    /// <summary>
    /// Gets the cells of a sheet that hold something, row by row, skipping keys that are not addresses.
    /// </summary>
    internal static IEnumerable<(CellAddress Address, SheetCell Cell)> SortedCells(Sheet sheet) =>
        sheet.Cells
            .Select(pair => (Valid: CellAddress.TryParse(pair.Key, out var address), Address: address, Cell: pair.Value))
            .Where(c => c.Valid && c.Cell is not null)
            .OrderBy(c => c.Address.Row)
            .ThenBy(c => c.Address.Column)
            .Select(c => (c.Address, c.Cell));

    /// <summary>
    /// Makes the sheet names Excel accepts: no <c>[ ] : * ? / \</c>, at most 31 characters, and no two the same
    /// without regard to case.
    /// </summary>
    internal static List<string> UniqueSheetNames(IReadOnlyList<Sheet> sheets)
    {
        var names = new List<string>();
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < sheets.Count; i++)
        {
            var name = TwDataTableExport.SanitizeSheetName(string.IsNullOrWhiteSpace(sheets[i].Name) ? $"Sheet{i + 1}" : sheets[i].Name);
            var candidate = name;
            var n = 2;
            while (!used.Add(candidate))
            {
                var suffix = $" ({n})";
                candidate = (name.Length + suffix.Length > 31 ? name[..(31 - suffix.Length)] : name) + suffix;
                n++;
            }

            names.Add(candidate);
        }

        return names;
    }

    private static int StyleIndex(List<CellStyle> styles, SheetCell cell)
    {
        var style = new CellStyle(cell.Bold, cell.Italic, cell.Underline, cell.Alignment, cell.Format,
            Argb(cell.Background), Argb(cell.Color), SheetFormatting.SafeFontFamily(cell.FontFamily), SheetFormatting.SafeFontSize(cell.FontSize));
        var index = styles.IndexOf(style);
        if (index < 0)
        {
            styles.Add(style);
            index = styles.Count - 1;
        }

        return index;
    }

    private static void WriteWorkbook(XmlWriter writer, List<string> names)
    {
        writer.WriteStartElement("workbook", spreadsheetNamespace);
        writer.WriteAttributeString("xmlns", "r", null, OpenXmlParts.RelationshipsNamespace);

        writer.WriteStartElement("sheets", spreadsheetNamespace);
        for (var i = 0; i < names.Count; i++)
        {
            writer.WriteStartElement("sheet", spreadsheetNamespace);
            writer.WriteAttributeString("name", names[i]);
            writer.WriteAttributeString("sheetId", (i + 1).ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("id", OpenXmlParts.RelationshipsNamespace, $"rId{i + 1}");
            writer.WriteEndElement();
        }

        writer.WriteEndElement();

        // Excel works every formula out again on opening, since the saved results come from twblazor's own engine.
        writer.WriteStartElement("calcPr", spreadsheetNamespace);
        writer.WriteAttributeString("fullCalcOnLoad", "1");
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteWorksheet(XmlWriter writer, Sheet sheet, List<(CellAddress Address, SheetCell Cell, int Style)> cells, Func<CellAddress, SheetValue> valueOf)
    {
        writer.WriteStartElement("worksheet", spreadsheetNamespace);
        WriteFrozenPanes(writer, Math.Max(0, sheet.FrozenRows), Math.Max(0, sheet.FrozenColumns));
        WriteColumnWidths(writer, sheet.ColumnWidths);
        writer.WriteStartElement("sheetData", spreadsheetNamespace);

        var rows = cells.GroupBy(c => c.Address.Row).ToDictionary(g => g.Key, g => g.ToList());

        // Excel measures row heights in points, three quarters of a pixel each.
        var heights = (sheet.RowHeights ?? []).Where(p => p.Key >= 1).ToDictionary(p => p.Key - 1, p => Math.Clamp(p.Value, 16, 400) * 0.75);
        foreach (var row in rows.Keys.Union(heights.Keys).Order())
        {
            writer.WriteStartElement("row", spreadsheetNamespace);
            writer.WriteAttributeString("r", (row + 1).ToString(CultureInfo.InvariantCulture));
            if (heights.TryGetValue(row, out var height))
            {
                writer.WriteAttributeString("ht", height.ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("customHeight", "1");
            }

            foreach (var (address, cell, style) in rows.GetValueOrDefault(row) ?? [])
            {
                WriteCell(writer, address, cell, style, valueOf(address));
            }

            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteFrozenPanes(XmlWriter writer, int rows, int columns)
    {
        if (rows == 0 && columns == 0)
        {
            return;
        }

        writer.WriteStartElement("sheetViews", spreadsheetNamespace);
        writer.WriteStartElement("sheetView", spreadsheetNamespace);
        writer.WriteAttributeString("workbookViewId", "0");
        writer.WriteStartElement("pane", spreadsheetNamespace);
        if (columns > 0)
        {
            writer.WriteAttributeString("xSplit", columns.ToString(CultureInfo.InvariantCulture));
        }

        if (rows > 0)
        {
            writer.WriteAttributeString("ySplit", rows.ToString(CultureInfo.InvariantCulture));
        }

        writer.WriteAttributeString("topLeftCell", new CellAddress(rows, columns).ToString());
        writer.WriteAttributeString("activePane", (rows > 0, columns > 0) switch
        {
            (true, true) => "bottomRight",
            (true, false) => "bottomLeft",
            _ => "topRight",
        });
        writer.WriteAttributeString("state", "frozen");
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    /// <summary>
    /// Writes the columns that have their own width. Excel measures widths in characters of its default font, about
    /// seven pixels each plus five pixels of padding.
    /// </summary>
    private static void WriteColumnWidths(XmlWriter writer, Dictionary<string, int>? widths)
    {
        var columns = (widths ?? [])
            .Select(pair => (Valid: CellAddress.TryParse($"{pair.Key}1", out var address), Column: address.Column, Width: Math.Clamp(pair.Value, 24, 1000)))
            .Where(c => c.Valid)
            .OrderBy(c => c.Column)
            .ToList();
        if (columns.Count == 0)
        {
            return;
        }

        writer.WriteStartElement("cols", spreadsheetNamespace);
        foreach (var (_, column, width) in columns)
        {
            var number = (column + 1).ToString(CultureInfo.InvariantCulture);
            writer.WriteStartElement("col", spreadsheetNamespace);
            writer.WriteAttributeString("min", number);
            writer.WriteAttributeString("max", number);
            writer.WriteAttributeString("width", Math.Round((width - 5) / 7d, 2).ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("customWidth", "1");
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Turns a hex colour into the ARGB form Excel stores, such as <c>FFFEF3C7</c>, or <see langword="null"/> when it
    /// is not a hex colour.
    /// </summary>
    internal static string? Argb(string? color)
    {
        var hex = SheetFormatting.SafeColor(color)?[1..].ToUpperInvariant();
        return hex?.Length switch
        {
            3 => "FF" + string.Concat(hex.Select(c => $"{c}{c}")),
            6 => "FF" + hex,
            8 => hex[6..] + hex[..6],
            _ => null,
        };
    }

    private static void WriteCell(XmlWriter writer, CellAddress address, SheetCell cell, int style, SheetValue value)
    {
        var isFormula = SheetCalculator.IsFormula(cell.Value);
        if (!isFormula && value.Kind == SheetValueKind.Empty && style == 0)
        {
            return;
        }

        writer.WriteStartElement("c", spreadsheetNamespace);
        writer.WriteAttributeString("r", address.ToString());
        if (style != 0)
        {
            writer.WriteAttributeString("s", style.ToString(CultureInfo.InvariantCulture));
        }

        if (isFormula)
        {
            WriteFormula(writer, cell.Value![1..], value);
        }
        else
        {
            WriteLiteral(writer, value);
        }

        writer.WriteEndElement();
    }

    private static void WriteFormula(XmlWriter writer, string formula, SheetValue value)
    {
        var (type, cached) = value.Kind switch
        {
            SheetValueKind.Number => (null, value.Number.ToString("R", CultureInfo.InvariantCulture)),
            SheetValueKind.Boolean => ("b", value.Number != 0 ? "1" : "0"),
            SheetValueKind.Text => ("str", value.Text),
            SheetValueKind.Error when _excelErrors.Contains(value.Text ?? string.Empty) => ("e", value.Text),
            _ => ((string?)null, (string?)null),
        };

        if (type is not null)
        {
            writer.WriteAttributeString("t", type);
        }

        writer.WriteElementString("f", spreadsheetNamespace, XmlCharacters.Clean(formula));
        if (cached is not null)
        {
            writer.WriteElementString("v", spreadsheetNamespace, XmlCharacters.Clean(cached));
        }
    }

    private static void WriteLiteral(XmlWriter writer, SheetValue value)
    {
        switch (value.Kind)
        {
            case SheetValueKind.Number:
                writer.WriteElementString("v", spreadsheetNamespace, value.Number.ToString("R", CultureInfo.InvariantCulture));
                break;
            case SheetValueKind.Boolean:
                writer.WriteAttributeString("t", "b");
                writer.WriteElementString("v", spreadsheetNamespace, value.Number != 0 ? "1" : "0");
                break;
            case SheetValueKind.Text:
                writer.WriteAttributeString("t", "inlineStr");
                writer.WriteStartElement("is", spreadsheetNamespace);
                writer.WriteStartElement("t", spreadsheetNamespace);
                writer.WriteAttributeString("xml", "space", null, "preserve");
                writer.WriteString(XmlCharacters.Clean(value.Text ?? string.Empty));
                writer.WriteEndElement();
                writer.WriteEndElement();
                break;
        }
    }

    private static void WriteStyles(XmlWriter writer, List<CellStyle> styles, CultureInfo culture)
    {
        var fonts = styles.Select(s => s.Font).Distinct().ToList();
        if (fonts[0] != default)
        {
            fonts.Insert(0, default);
        }

        // Excel needs the two built in fills first; each cell fill comes after them.
        var fills = new List<string?> { null, null };
        fills.AddRange(styles.Select(s => s.Fill).Where(f => f is not null).Distinct());

        writer.WriteStartElement("styleSheet", spreadsheetNamespace);

        writer.WriteStartElement("numFmts", spreadsheetNamespace);
        writer.WriteAttributeString(countAttribute, "1");
        writer.WriteStartElement("numFmt", spreadsheetNamespace);
        writer.WriteAttributeString("numFmtId", currencyFormatId);
        var symbol = culture.NumberFormat.CurrencySymbol.Replace("\"", string.Empty, StringComparison.Ordinal);
        writer.WriteAttributeString("formatCode", $"\"{symbol}\"#,##0.00;-\"{symbol}\"#,##0.00");
        writer.WriteEndElement();
        writer.WriteEndElement();

        writer.WriteStartElement("fonts", spreadsheetNamespace);
        writer.WriteAttributeString(countAttribute, fonts.Count.ToString(CultureInfo.InvariantCulture));
        foreach (var font in fonts)
        {
            WriteFont(writer, font);
        }

        writer.WriteEndElement();

        // Excel needs the two built in fills and a border to be present before any cell format refers to them.
        writer.WriteStartElement("fills", spreadsheetNamespace);
        writer.WriteAttributeString(countAttribute, fills.Count.ToString(CultureInfo.InvariantCulture));
        for (var i = 0; i < fills.Count; i++)
        {
            writer.WriteStartElement("fill", spreadsheetNamespace);
            writer.WriteStartElement("patternFill", spreadsheetNamespace);
            writer.WriteAttributeString("patternType", i switch { 0 => "none", 1 => "gray125", _ => "solid" });
            if (i > 1)
            {
                writer.WriteStartElement("fgColor", spreadsheetNamespace);
                writer.WriteAttributeString("rgb", fills[i]);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        writer.WriteEndElement();

        writer.WriteStartElement("borders", spreadsheetNamespace);
        writer.WriteAttributeString(countAttribute, "1");
        writer.WriteStartElement("border", spreadsheetNamespace);
        writer.WriteEndElement();
        writer.WriteEndElement();

        writer.WriteStartElement("cellStyleXfs", spreadsheetNamespace);
        writer.WriteAttributeString(countAttribute, "1");
        writer.WriteStartElement("xf", spreadsheetNamespace);
        writer.WriteAttributeString("numFmtId", "0");
        writer.WriteAttributeString("fontId", "0");
        writer.WriteAttributeString("fillId", "0");
        writer.WriteAttributeString("borderId", "0");
        writer.WriteEndElement();
        writer.WriteEndElement();

        writer.WriteStartElement("cellXfs", spreadsheetNamespace);
        writer.WriteAttributeString(countAttribute, styles.Count.ToString(CultureInfo.InvariantCulture));
        foreach (var style in styles)
        {
            WriteCellFormat(writer, style, fonts.IndexOf(style.Font), style.Fill is null ? 0 : fills.IndexOf(style.Fill));
        }

        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteFlag(XmlWriter writer, string name, bool on)
    {
        if (on)
        {
            writer.WriteStartElement(name, spreadsheetNamespace);
            writer.WriteEndElement();
        }
    }

    private static void WriteFont(XmlWriter writer, FontKey font)
    {
        writer.WriteStartElement("font", spreadsheetNamespace);
        WriteFlag(writer, "b", font.Bold);
        WriteFlag(writer, "i", font.Italic);
        WriteFlag(writer, "u", font.Underline);
        if (font.Size is { } size)
        {
            writer.WriteStartElement("sz", spreadsheetNamespace);
            writer.WriteAttributeString("val", size.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }

        if (font.Color is not null)
        {
            writer.WriteStartElement("color", spreadsheetNamespace);
            writer.WriteAttributeString("rgb", font.Color);
            writer.WriteEndElement();
        }

        if (font.Family is not null)
        {
            writer.WriteStartElement("name", spreadsheetNamespace);
            writer.WriteAttributeString("val", font.Family);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private static void WriteCellFormat(XmlWriter writer, CellStyle style, int font, int fill)
    {
        var numberFormat = style.Format switch
        {
            SheetNumberFormat.Number => "4",
            SheetNumberFormat.Currency => currencyFormatId,
            SheetNumberFormat.Percent => "10",
            _ => "0",
        };

        writer.WriteStartElement("xf", spreadsheetNamespace);
        writer.WriteAttributeString("numFmtId", numberFormat);
        writer.WriteAttributeString("fontId", font.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("fillId", fill.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("borderId", "0");
        writer.WriteAttributeString("xfId", "0");
        if (numberFormat != "0")
        {
            writer.WriteAttributeString("applyNumberFormat", "1");
        }

        if (font > 0)
        {
            writer.WriteAttributeString("applyFont", "1");
        }

        if (fill > 0)
        {
            writer.WriteAttributeString("applyFill", "1");
        }

        var horizontal = style.Alignment switch
        {
            SheetCellAlignment.Start => "left",
            SheetCellAlignment.Center => "center",
            SheetCellAlignment.End => "right",
            _ => null,
        };

        if (horizontal is not null)
        {
            writer.WriteAttributeString("applyAlignment", "1");
            writer.WriteStartElement("alignment", spreadsheetNamespace);
            writer.WriteAttributeString("horizontal", horizontal);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }
}
