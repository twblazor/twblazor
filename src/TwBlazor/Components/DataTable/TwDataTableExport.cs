// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace TwBlazor.Components;

/// <summary>
/// Writes rows out as CSV or as an Excel workbook, using <see cref="TwDataTableColumn{TItem}"/> definitions for the
/// headings and the values. <see cref="TwDataTable{TItem}"/> uses it for its export buttons, and it can be called on
/// its own, for example to build a file on the server.
/// </summary>
/// <remarks>Each cell takes the column's <see cref="TwDataTableColumn{TItem}.ExportFormatter"/> when it has one,
/// then its <see cref="TwDataTableColumn{TItem}.PropertySelector"/>, then its
/// <see cref="TwDataTableColumn{TItem}.CellFormatter"/>. Columns whose <see cref="TwDataTableColumn{TItem}.Exportable"/>
/// is <c>false</c> are left out.</remarks>
public static class TwDataTableExport
{
    /// <summary>
    /// The media type of a CSV file.
    /// </summary>
    public const string CsvContentType = "text/csv";

    /// <summary>
    /// The media type of an Excel workbook.
    /// </summary>
    public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private const string spreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string relationshipsNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private const string packageRelationshipsNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";
    private const string countAttribute = "count";

    // Indexes into the cellXfs list written by WriteStyles.
    private const int headerStyle = 1;
    private const int dateStyle = 2;
    private const int dateTimeStyle = 3;
    private const int timeStyle = 4;

    private static readonly char[] _csvSpecialCharacters = [',', '"', '\r', '\n'];

    /// <summary>
    /// Writes <paramref name="rows"/> as CSV text, with a heading row first. Fields are quoted when they need to be,
    /// numbers and dates are written in an invariant format so any spreadsheet reads them the same way, and text that
    /// a spreadsheet would run as a formula is prefixed with an apostrophe.
    /// </summary>
    /// <typeparam name="TItem">The type of the rows.</typeparam>
    /// <param name="rows">The rows to write.</param>
    /// <param name="columns">The columns to write, in order.</param>
    /// <returns>The CSV text, with a line break after every row.</returns>
    public static string ToCsv<TItem>(IEnumerable<TItem> rows, IEnumerable<TwDataTableColumn<TItem>> columns)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(columns);

        var exported = ExportedColumns(columns);
        var csv = new StringBuilder();

        AppendCsvLine(csv, exported.Select(c => (object?)c.Title));
        foreach (var row in rows)
        {
            AppendCsvLine(csv, exported.Select(c => ValueOf(c, row)));
        }

        return csv.ToString();
    }

    /// <summary>
    /// Writes <paramref name="rows"/> as an Excel workbook with one worksheet. The heading row is bold and stays in
    /// view as the sheet scrolls, numbers and true or false values keep their types, and dates are real dates.
    /// </summary>
    /// <typeparam name="TItem">The type of the rows.</typeparam>
    /// <param name="rows">The rows to write.</param>
    /// <param name="columns">The columns to write, in order.</param>
    /// <param name="sheetName">The name of the worksheet. Characters Excel does not allow are replaced, and the
    /// name is cut to 31 characters.</param>
    /// <returns>The contents of an <c>.xlsx</c> file.</returns>
    public static byte[] ToExcel<TItem>(IEnumerable<TItem> rows, IEnumerable<TwDataTableColumn<TItem>> columns, string sheetName = "Sheet1")
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(columns);

        var exported = ExportedColumns(columns);

        using var output = new MemoryStream();
        using (var package = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(package, "[Content_Types].xml", WriteContentTypes);
            WriteEntry(package, "_rels/.rels", WritePackageRelationships);
            WriteEntry(package, "xl/workbook.xml", writer => WriteWorkbook(writer, SanitizeSheetName(sheetName)));
            WriteEntry(package, "xl/_rels/workbook.xml.rels", WriteWorkbookRelationships);
            WriteEntry(package, "xl/styles.xml", WriteStyles);
            WriteEntry(package, "xl/worksheets/sheet1.xml", writer => WriteWorksheet(writer, rows, exported));
        }

        return output.ToArray();
    }

    private static List<TwDataTableColumn<TItem>> ExportedColumns<TItem>(IEnumerable<TwDataTableColumn<TItem>> columns) =>
        [.. columns.Where(c => c is { Exportable: true })];

    private static object? ValueOf<TItem>(TwDataTableColumn<TItem> column, TItem row)
    {
        if (column.ExportFormatter is { } exportFormatter)
        {
            return exportFormatter(row);
        }

        if (column.PropertySelector is { } selector)
        {
            return selector(row);
        }

        return column.CellFormatter?.Invoke(row);
    }

    private static void AppendCsvLine(StringBuilder csv, IEnumerable<object?> values)
    {
        csv.AppendJoin(',', values.Select(CsvField));
        csv.Append("\r\n");
    }

    private static string CsvField(object? value)
    {
        var text = value switch
        {
            null => string.Empty,
            string s => GuardFormula(s),
            DateTime d => d.TimeOfDay == TimeSpan.Zero ? d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : d.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            DateTimeOffset d => d.ToString("yyyy-MM-dd HH:mm:sszzz", CultureInfo.InvariantCulture),
            DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            TimeOnly t => t.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
            bool b => b ? "TRUE" : "FALSE",
            IFormattable f when IsNumber(value) => f.ToString(null, CultureInfo.InvariantCulture),
            IFormattable f => GuardFormula(f.ToString(null, CultureInfo.InvariantCulture)),
            _ => GuardFormula(value.ToString() ?? string.Empty)
        };

        return text.IndexOfAny(_csvSpecialCharacters) >= 0
            ? $"\"{text.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
            : text;
    }

    /// <summary>
    /// Stops text that starts like a formula from running when the file is opened in a spreadsheet (CSV injection).
    /// </summary>
    private static string GuardFormula(string text) =>
        text.Length > 0 && text[0] is '=' or '+' or '-' or '@' or '\t' or '\r' ? $"'{text}" : text;

    private static bool IsNumber(object value) => value is byte or sbyte or short or ushort or int or uint or long or ulong
        or float or double or decimal;

    private static void WriteEntry(ZipArchive package, string path, Action<XmlWriter> write)
    {
        var entry = package.CreateEntry(path, CompressionLevel.Optimal);
        using var stream = entry.Open();
        using var writer = XmlWriter.Create(stream, new XmlWriterSettings { Encoding = new UTF8Encoding(false) });
        writer.WriteStartDocument(true);
        write(writer);
        writer.WriteEndDocument();
    }

    private static void WriteContentTypes(XmlWriter writer)
    {
        const string ns = "http://schemas.openxmlformats.org/package/2006/content-types";
        writer.WriteStartElement("Types", ns);
        WriteDefault("rels", "application/vnd.openxmlformats-package.relationships+xml");
        WriteDefault("xml", "application/xml");
        WriteOverride("/xl/workbook.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml");
        WriteOverride("/xl/worksheets/sheet1.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml");
        WriteOverride("/xl/styles.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml");
        writer.WriteEndElement();

        void WriteDefault(string extension, string contentType)
        {
            writer.WriteStartElement("Default", ns);
            writer.WriteAttributeString("Extension", extension);
            writer.WriteAttributeString("ContentType", contentType);
            writer.WriteEndElement();
        }

        void WriteOverride(string part, string contentType)
        {
            writer.WriteStartElement("Override", ns);
            writer.WriteAttributeString("PartName", part);
            writer.WriteAttributeString("ContentType", contentType);
            writer.WriteEndElement();
        }
    }

    private static void WritePackageRelationships(XmlWriter writer)
    {
        writer.WriteStartElement("Relationships", packageRelationshipsNamespace);
        WriteRelationship(writer, "rId1", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument", "xl/workbook.xml");
        writer.WriteEndElement();
    }

    private static void WriteWorkbookRelationships(XmlWriter writer)
    {
        writer.WriteStartElement("Relationships", packageRelationshipsNamespace);
        WriteRelationship(writer, "rId1", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet", "worksheets/sheet1.xml");
        WriteRelationship(writer, "rId2", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles", "styles.xml");
        writer.WriteEndElement();
    }

    private static void WriteRelationship(XmlWriter writer, string id, string type, string target)
    {
        writer.WriteStartElement("Relationship", packageRelationshipsNamespace);
        writer.WriteAttributeString("Id", id);
        writer.WriteAttributeString("Type", type);
        writer.WriteAttributeString("Target", target);
        writer.WriteEndElement();
    }

    private static void WriteWorkbook(XmlWriter writer, string sheetName)
    {
        writer.WriteStartElement("workbook", spreadsheetNamespace);
        writer.WriteAttributeString("xmlns", "r", null, relationshipsNamespace);
        writer.WriteStartElement("sheets", spreadsheetNamespace);
        writer.WriteStartElement("sheet", spreadsheetNamespace);
        writer.WriteAttributeString("name", sheetName);
        writer.WriteAttributeString("sheetId", "1");
        writer.WriteAttributeString("id", relationshipsNamespace, "rId1");
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteStyles(XmlWriter writer)
    {
        writer.WriteStartElement("styleSheet", spreadsheetNamespace);

        writer.WriteStartElement("fonts", spreadsheetNamespace);
        writer.WriteAttributeString(countAttribute, "2");
        writer.WriteStartElement("font", spreadsheetNamespace);
        writer.WriteEndElement();
        writer.WriteStartElement("font", spreadsheetNamespace);
        writer.WriteStartElement("b", spreadsheetNamespace);
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();

        // Excel needs the two built in fills and a border to be present before any cell format refers to them.
        writer.WriteStartElement("fills", spreadsheetNamespace);
        writer.WriteAttributeString(countAttribute, "2");
        foreach (var pattern in new[] { "none", "gray125" })
        {
            writer.WriteStartElement("fill", spreadsheetNamespace);
            writer.WriteStartElement("patternFill", spreadsheetNamespace);
            writer.WriteAttributeString("patternType", pattern);
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
        WriteCellFormat(writer, numberFormat: 0, font: 0, applyFormat: false);
        writer.WriteEndElement();

        // In order: normal, header (bold), date (d/m/yyyy), date and time, time. The numbers are Excel's built in
        // formats, which follow the reader's own regional settings.
        writer.WriteStartElement("cellXfs", spreadsheetNamespace);
        writer.WriteAttributeString(countAttribute, "5");
        WriteCellFormat(writer, numberFormat: 0, font: 0, applyFormat: false);
        WriteCellFormat(writer, numberFormat: 0, font: 1, applyFormat: false);
        WriteCellFormat(writer, numberFormat: 14, font: 0, applyFormat: true);
        WriteCellFormat(writer, numberFormat: 22, font: 0, applyFormat: true);
        WriteCellFormat(writer, numberFormat: 21, font: 0, applyFormat: true);
        writer.WriteEndElement();

        writer.WriteEndElement();
    }

    private static void WriteCellFormat(XmlWriter writer, int numberFormat, int font, bool applyFormat)
    {
        writer.WriteStartElement("xf", spreadsheetNamespace);
        writer.WriteAttributeString("numFmtId", numberFormat.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("fontId", font.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("fillId", "0");
        writer.WriteAttributeString("borderId", "0");
        if (applyFormat)
        {
            writer.WriteAttributeString("applyNumberFormat", "1");
        }
        if (font > 0)
        {
            writer.WriteAttributeString("applyFont", "1");
        }
        writer.WriteEndElement();
    }

    private static void WriteWorksheet<TItem>(XmlWriter writer, IEnumerable<TItem> rows, List<TwDataTableColumn<TItem>> columns)
    {
        writer.WriteStartElement("worksheet", spreadsheetNamespace);

        writer.WriteStartElement("sheetViews", spreadsheetNamespace);
        writer.WriteStartElement("sheetView", spreadsheetNamespace);
        writer.WriteAttributeString("workbookViewId", "0");
        writer.WriteStartElement("pane", spreadsheetNamespace);
        writer.WriteAttributeString("ySplit", "1");
        writer.WriteAttributeString("topLeftCell", "A2");
        writer.WriteAttributeString("activePane", "bottomLeft");
        writer.WriteAttributeString("state", "frozen");
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();

        writer.WriteStartElement("sheetData", spreadsheetNamespace);

        var rowNumber = 1;
        WriteRow(writer, rowNumber, columns.Select(c => (object?)c.Title), headerStyle);
        foreach (var row in rows)
        {
            rowNumber++;
            WriteRow(writer, rowNumber, columns.Select(c => ValueOf(c, row)), null);
        }

        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void WriteRow(XmlWriter writer, int rowNumber, IEnumerable<object?> values, int? style)
    {
        var rowText = rowNumber.ToString(CultureInfo.InvariantCulture);
        writer.WriteStartElement("row", spreadsheetNamespace);
        writer.WriteAttributeString("r", rowText);

        var columnIndex = 0;
        foreach (var value in values)
        {
            var reference = ColumnLetters(columnIndex) + rowText;
            columnIndex++;
            if (value is null)
            {
                continue;
            }

            WriteCell(writer, reference, value, style);
        }

        writer.WriteEndElement();
    }

    private static void WriteCell(XmlWriter writer, string reference, object value, int? style)
    {
        writer.WriteStartElement("c", spreadsheetNamespace);
        writer.WriteAttributeString("r", reference);

        switch (value)
        {
            case bool b:
                writer.WriteAttributeString("t", "b");
                WriteStyle(style);
                writer.WriteElementString("v", spreadsheetNamespace, b ? "1" : "0");
                break;
            case double d when !double.IsFinite(d):
                WriteInlineString(writer, d.ToString(CultureInfo.InvariantCulture), style);
                break;
            case float f when !float.IsFinite(f):
                WriteInlineString(writer, f.ToString(CultureInfo.InvariantCulture), style);
                break;
            case var number when IsNumber(number):
                WriteStyle(style);
                writer.WriteElementString("v", spreadsheetNamespace, ((IFormattable)number).ToString(null, CultureInfo.InvariantCulture));
                break;
            case DateTime dateTime:
                WriteStyle(style ?? (dateTime.TimeOfDay == TimeSpan.Zero ? dateStyle : dateTimeStyle));
                writer.WriteElementString("v", spreadsheetNamespace, dateTime.ToOADate().ToString(CultureInfo.InvariantCulture));
                break;
            case DateTimeOffset offset:
                WriteStyle(style ?? dateTimeStyle);
                writer.WriteElementString("v", spreadsheetNamespace, offset.DateTime.ToOADate().ToString(CultureInfo.InvariantCulture));
                break;
            case DateOnly date:
                WriteStyle(style ?? dateStyle);
                writer.WriteElementString("v", spreadsheetNamespace, date.ToDateTime(TimeOnly.MinValue).ToOADate().ToString(CultureInfo.InvariantCulture));
                break;
            case TimeOnly time:
                WriteStyle(style ?? timeStyle);
                writer.WriteElementString("v", spreadsheetNamespace, time.ToTimeSpan().TotalDays.ToString(CultureInfo.InvariantCulture));
                break;
            default:
                WriteInlineString(writer, value is IFormattable formattable
                    ? formattable.ToString(null, CultureInfo.InvariantCulture)
                    : value.ToString() ?? string.Empty, style);
                break;
        }

        writer.WriteEndElement();

        void WriteStyle(int? index)
        {
            if (index is { } s)
            {
                writer.WriteAttributeString("s", s.ToString(CultureInfo.InvariantCulture));
            }
        }
    }

    private static void WriteInlineString(XmlWriter writer, string text, int? style)
    {
        writer.WriteAttributeString("t", "inlineStr");
        if (style is { } s)
        {
            writer.WriteAttributeString("s", s.ToString(CultureInfo.InvariantCulture));
        }

        writer.WriteStartElement("is", spreadsheetNamespace);
        writer.WriteStartElement("t", spreadsheetNamespace);
        writer.WriteAttributeString("xml", "space", null, "preserve");
        writer.WriteString(RemoveInvalidXmlCharacters(text));
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static string RemoveInvalidXmlCharacters(string text)
    {
        if (text.All(XmlConvert.IsXmlChar))
        {
            return text;
        }

        var cleaned = new StringBuilder(text.Length);
        var i = 0;
        while (i < text.Length)
        {
            if (XmlConvert.IsXmlChar(text[i]))
            {
                cleaned.Append(text[i]);
                i++;
            }
            else if (i + 1 < text.Length && XmlConvert.IsXmlSurrogatePair(text[i + 1], text[i]))
            {
                // A character outside the basic plane, such as an emoji, takes two chars.
                cleaned.Append(text, i, 2);
                i += 2;
            }
            else
            {
                i++;
            }
        }

        return cleaned.ToString();
    }

    /// <summary>
    /// Turns a zero based column index into Excel's column letters: 0 is A, 25 is Z and 26 is AA.
    /// </summary>
    internal static string ColumnLetters(int index)
    {
        var letters = new StringBuilder();
        for (var n = index + 1; n > 0; n = (n - 1) / 26)
        {
            letters.Insert(0, (char)('A' + ((n - 1) % 26)));
        }

        return letters.ToString();
    }

    /// <summary>
    /// Makes a worksheet name Excel accepts: no <c>[ ] : * ? / \</c>, not blank, and at most 31 characters.
    /// </summary>
    internal static string SanitizeSheetName(string? name)
    {
        var cleaned = new string([.. (name ?? string.Empty).Select(c => c is '[' or ']' or ':' or '*' or '?' or '/' or '\\' ? '_' : c)]).Trim('\'').Trim();
        if (cleaned.Length == 0)
        {
            cleaned = "Sheet1";
        }

        return cleaned.Length > 31 ? cleaned[..31] : cleaned;
    }
}
