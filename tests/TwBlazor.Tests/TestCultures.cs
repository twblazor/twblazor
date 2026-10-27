using System.Globalization;

namespace TwBlazor.Tests;

/// <summary>
/// Cultures for tests whose date and time patterns are spelled out, so assertions do not depend on the
/// locale data of the machine running them. The set spans 12 and 24 hour clocks, left to right and right to
/// left scripts, and first days of week of Sunday, Monday and Saturday, so the localization tests cover more
/// than a single culture.
/// </summary>
public static class TestCultures
{
    /// <summary>
    /// Gets British English with every pattern components read set explicitly. The operating system's own
    /// <c>en-GB</c> data differs between platforms (for example whether the long date has a comma, and the
    /// case of the AM and PM designators), which would otherwise make text assertions platform specific.
    /// </summary>
    public static CultureInfo British { get; } = CreateBritish();

    /// <summary>
    /// Gets American English: Sunday first day of week, a <c>M/d/yyyy</c> short date and a 12 hour
    /// <c>h:mm tt</c> clock with uppercase AM and PM.
    /// </summary>
    public static CultureInfo American { get; } =
        Create("en-US", DayOfWeek.Sunday, "dddd, MMMM d, yyyy", "M/d/yyyy", "MMMM yyyy", "MMMM d", "h:mm tt", "h:mm:ss tt", "AM", "PM");

    /// <summary>
    /// Gets German: Monday first day of week, a <c>dd.MM.yyyy</c> short date and a 24 hour <c>HH:mm</c> clock.
    /// </summary>
    public static CultureInfo German { get; } =
        Create("de-DE", DayOfWeek.Monday, "dddd, d. MMMM yyyy", "dd.MM.yyyy", "MMMM yyyy", "d. MMMM", "HH:mm", "HH:mm:ss", "", "");

    /// <summary>
    /// Gets French: Monday first day of week, a <c>dd/MM/yyyy</c> short date and a 24 hour <c>HH:mm</c> clock.
    /// </summary>
    public static CultureInfo French { get; } =
        Create("fr-FR", DayOfWeek.Monday, "dddd d MMMM yyyy", "dd/MM/yyyy", "MMMM yyyy", "d MMMM", "HH:mm", "HH:mm:ss", "", "");

    /// <summary>
    /// Gets Spanish: Monday first day of week, a <c>dd/MM/yyyy</c> short date and a 24 hour <c>H:mm</c> clock
    /// (the hour has no leading zero, as CLDR writes Spanish times).
    /// </summary>
    public static CultureInfo Spanish { get; } =
        Create("es-ES", DayOfWeek.Monday, "dddd, d 'de' MMMM 'de' yyyy", "dd/MM/yyyy", "MMMM 'de' yyyy", "d 'de' MMMM", "H:mm", "H:mm:ss", "", "");

    /// <summary>
    /// Gets Turkish: Monday first day of week, a <c>dd.MM.yyyy</c> short date and a 24 hour <c>HH:mm</c> clock.
    /// Turkish is the dotless-i culture, whose case rules differ from the Latin default.
    /// </summary>
    public static CultureInfo Turkish { get; } =
        Create("tr-TR", DayOfWeek.Monday, "d MMMM yyyy dddd", "dd.MM.yyyy", "MMMM yyyy", "d MMMM", "HH:mm", "HH:mm:ss", "", "");

    /// <summary>
    /// Gets Arabic: Saturday first day of week per CLDR, a right to left script, Eastern Arabic numerals and a
    /// 12 hour <c>h:mm tt</c> clock.
    /// </summary>
    public static CultureInfo Arabic { get; } =
        Create("ar", DayOfWeek.Saturday, "dddd، d MMMM yyyy", "dd/MM/yyyy", "MMMM yyyy", "d MMMM", "h:mm tt", "h:mm:ss tt", "ص", "م");

    /// <summary>
    /// Gets Japanese: Sunday first day of week, a <c>yyyy/MM/dd</c> short date and a 24 hour <c>H:mm</c> clock
    /// (the hour has no leading zero, as CLDR writes Japanese times).
    /// </summary>
    public static CultureInfo Japanese { get; } =
        Create("ja-JP", DayOfWeek.Sunday, "yyyy'年'M'月'd'日'dddd", "yyyy/MM/dd", "yyyy'年'M'月'", "M'月'd'日'", "H:mm", "H:mm:ss", "", "");

    /// <summary>
    /// Gets every pinned culture, for tests that assert the same behaviour across all of them.
    /// </summary>
    public static IReadOnlyList<CultureInfo> All { get; } =
        [British, American, German, French, Spanish, Turkish, Arabic, Japanese];

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

    private static CultureInfo Create(
        string name,
        DayOfWeek firstDayOfWeek,
        string longDate,
        string shortDate,
        string yearMonth,
        string monthDay,
        string shortTime,
        string longTime,
        string amDesignator,
        string pmDesignator)
    {
        var culture = (CultureInfo)CultureInfo.GetCultureInfo(name).Clone();
        var format = culture.DateTimeFormat;
        format.FirstDayOfWeek = firstDayOfWeek;
        format.LongDatePattern = longDate;
        format.ShortDatePattern = shortDate;
        format.YearMonthPattern = yearMonth;
        format.MonthDayPattern = monthDay;
        format.ShortTimePattern = shortTime;
        format.LongTimePattern = longTime;
        format.AMDesignator = amDesignator;
        format.PMDesignator = pmDesignator;
        return CultureInfo.ReadOnly(culture);
    }
}
