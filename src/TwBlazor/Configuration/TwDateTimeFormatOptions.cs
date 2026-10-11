// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using TwBlazor.Localization;

namespace TwBlazor.Configuration;

/// <summary>
/// The date and time formats TwBlazor components render with. Every value is optional and is a .NET custom or
/// standard date and time format string.
/// </summary>
/// <remarks>
/// A value set here always wins. Otherwise the matching <c>Format_</c> entry of a resource file is used when it has
/// a value, and failing that the culture's own pattern.
/// </remarks>
public class TwDateTimeFormatOptions
{
    /// <summary>
    /// Gets or sets the day calendars and date pickers start the week on. Defaults to the culture's
    /// <see cref="DateTimeFormatInfo.FirstDayOfWeek"/>.
    /// </summary>
    public DayOfWeek? FirstDayOfWeek { get; set; }

    /// <summary>
    /// Gets or sets whether times use a 12 hour clock with an AM/PM designator.
    /// </summary>
    /// <remarks>
    /// Left unset, displayed times (such as the calendar's) follow the culture, and the time pickers stay on a
    /// 24 hour clock unless their own <c>Is12HourFormat</c> parameter says otherwise.
    /// </remarks>
    public bool? Use12HourClock { get; set; }

    /// <summary>
    /// Gets or sets the format of a time of day, for example <c>HH:mm</c>. Defaults to the culture's short time pattern.
    /// </summary>
    public string? ShortTime { get; set; }

    /// <summary>
    /// Gets or sets the format of an hour label in the calendar's time gutter. Defaults to <c>h tt</c> on a 12 hour
    /// clock and <c>HH:mm</c> on a 24 hour one.
    /// </summary>
    public string? Hour { get; set; }

    /// <summary>
    /// Gets or sets the format of a full date, used for the accessible name of a day. Defaults to the culture's long
    /// date pattern.
    /// </summary>
    public string? LongDate { get; set; }

    /// <summary>
    /// Gets or sets the format of an abbreviated date. Defaults to <c>d MMM yyyy</c>.
    /// </summary>
    public string? MediumDate { get; set; }

    /// <summary>
    /// Gets or sets the format of a month and year heading. Defaults to the culture's year month pattern.
    /// </summary>
    public string? MonthYear { get; set; }

    /// <summary>
    /// Gets or sets the format of a day and month without the year. Defaults to the culture month day pattern.
    /// </summary>
    public string? MonthDay { get; set; }

    /// <summary>
    /// Gets or sets the format of a month on its own. Defaults to <c>MMMM</c>.
    /// </summary>
    public string? Month { get; set; }

    /// <summary>
    /// Gets or sets the format of an abbreviated month. Defaults to <c>MMM</c>.
    /// </summary>
    public string? ShortMonth { get; set; }

    /// <summary>
    /// Gets or sets the format of a year. Defaults to <c>yyyy</c>.
    /// </summary>
    public string? Year { get; set; }

    /// <summary>
    /// Gets or sets the format of a week view column heading. Defaults to <c>ddd d</c>.
    /// </summary>
    public string? WeekdayDay { get; set; }

    /// <summary>
    /// Gets or sets the format the date pickers type and parse. Defaults to the theme's
    /// <see cref="Components.TwDatePickerTheme.DefaultFormat"/>.
    /// </summary>
    public string? DateInput { get; set; }

    /// <summary>
    /// Gets or sets the format the time pickers type and parse. Defaults to <c>hh:mm tt</c> on a 12 hour clock and
    /// <c>HH:mm</c> on a 24 hour one.
    /// </summary>
    public string? TimeInput { get; set; }

    /// <summary>
    /// Gets or sets the format the date time pickers type and parse. Defaults to the theme's
    /// <see cref="Components.TwDatePickerTheme.DefaultDateTimeFormat"/> or
    /// <see cref="Components.TwDatePickerTheme.DefaultDateTimeFormat12Hour"/>.
    /// </summary>
    public string? DateTimeInput { get; set; }

    /// <summary>
    /// Copies the culture's own patterns into the picker input formats, so the date and time pickers type and parse
    /// the way the culture writes dates (for example <c>M/d/yyyy</c> and <c>h:mm tt</c> for <c>en-US</c>).
    /// </summary>
    /// <param name="culture">The culture to take the patterns from.</param>
    /// <returns>These options, so calls can be chained.</returns>
    public TwDateTimeFormatOptions UseCultureInputFormats(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        var format = culture.DateTimeFormat;
        DateInput = format.ShortDatePattern;
        TimeInput = format.ShortTimePattern;
        DateTimeInput = $"{format.ShortDatePattern} {format.ShortTimePattern}";
        Use12HourClock ??= TwDateTimeFormats.Is12HourPattern(format.ShortTimePattern);
        return this;
    }
}
