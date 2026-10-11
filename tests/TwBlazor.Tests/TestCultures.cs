using System.Globalization;

namespace TwBlazor.Tests;

/// <summary>
/// Cultures for tests whose date and time patterns are spelled out, so assertions do not depend on the
/// locale data of the machine running them.
/// </summary>
public static class TestCultures
{
    /// <summary>
    /// Gets British English with every pattern components read set explicitly. The operating system's own
    /// <c>en-GB</c> data differs between platforms (for example whether the long date has a comma, and the
    /// case of the AM and PM designators), which would otherwise make text assertions platform specific.
    /// </summary>
    public static CultureInfo British { get; } = CreateBritish();

    private static CultureInfo CreateBritish()
    {
        var culture = (CultureInfo)CultureInfo.GetCultureInfo("en-GB").Clone();
        var format = culture.DateTimeFormat;
        format.FirstDayOfWeek = DayOfWeek.Monday;
        format.LongDatePattern = "dddd, d MMMM yyyy";
        format.ShortDatePattern = "dd/MM/yyyy";
        format.YearMonthPattern = "MMMM yyyy";
        format.MonthDayPattern = "d MMMM";
        format.ShortTimePattern = "HH:mm";
        format.LongTimePattern = "HH:mm:ss";
        format.AMDesignator = "am";
        format.PMDesignator = "pm";
        return CultureInfo.ReadOnly(culture);
    }
}
