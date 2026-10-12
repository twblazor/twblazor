// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using TwBlazor.Configuration;

namespace TwBlazor.Localization;

/// <summary>
/// The date and time formats components render with, resolved for one culture from
/// <see cref="TwDateTimeFormatOptions"/>, the resource files and the culture itself.
/// </summary>
public sealed class TwDateTimeFormats
{
    /// <summary>
    /// Gets the culture that supplies month names, day names and AM/PM designators.
    /// </summary>
    public required CultureInfo Culture { get; init; }

    /// <summary>
    /// Gets the day a week starts on.
    /// </summary>
    public required DayOfWeek FirstDayOfWeek { get; init; }

    /// <summary>
    /// Gets whether displayed times use a 12 hour clock.
    /// </summary>
    public required bool Use12HourClock { get; init; }

    /// <summary>
    /// Gets whether the time pickers use a 12 hour clock when their <c>Is12HourFormat</c> parameter is not set.
    /// </summary>
    public required bool Use12HourInput { get; init; }

    /// <inheritdoc cref="TwDateTimeFormatOptions.ShortTime" />
    public required string ShortTime { get; init; }

    /// <inheritdoc cref="TwDateTimeFormatOptions.Hour" />
    public required string Hour { get; init; }

    /// <inheritdoc cref="TwDateTimeFormatOptions.LongDate" />
    public required string LongDate { get; init; }

    /// <inheritdoc cref="TwDateTimeFormatOptions.MediumDate" />
    public required string MediumDate { get; init; }

    /// <inheritdoc cref="TwDateTimeFormatOptions.MonthYear" />
    public required string MonthYear { get; init; }

    /// <inheritdoc cref="TwDateTimeFormatOptions.MonthDay" />
    public required string MonthDay { get; init; }

    /// <inheritdoc cref="TwDateTimeFormatOptions.Month" />
    public required string Month { get; init; }

    /// <inheritdoc cref="TwDateTimeFormatOptions.ShortMonth" />
    public required string ShortMonth { get; init; }

    /// <inheritdoc cref="TwDateTimeFormatOptions.Year" />
    public required string Year { get; init; }

    /// <inheritdoc cref="TwDateTimeFormatOptions.WeekdayDay" />
    public required string WeekdayDay { get; init; }

    /// <summary>
    /// Gets the format the date pickers type and parse, or <c>null</c> to use the theme's default.
    /// </summary>
    public string? DateInput { get; init; }

    /// <summary>
    /// Gets the format the time pickers type and parse, or <c>null</c> to derive it from the clock style.
    /// </summary>
    public string? TimeInput { get; init; }

    /// <summary>
    /// Gets the format the date time pickers type and parse, or <c>null</c> to use the theme's default.
    /// </summary>
    public string? DateTimeInput { get; init; }

    /// <summary>
    /// Gets the text shown for the morning half of a 12 hour clock.
    /// </summary>
    public string AmDesignator => string.IsNullOrEmpty(Culture.DateTimeFormat.AMDesignator) ? "AM" : Culture.DateTimeFormat.AMDesignator;

    /// <summary>
    /// Gets the text shown for the afternoon half of a 12 hour clock.
    /// </summary>
    public string PmDesignator => string.IsNullOrEmpty(Culture.DateTimeFormat.PMDesignator) ? "PM" : Culture.DateTimeFormat.PMDesignator;

    /// <summary>
    /// Formats a date with one of the formats on this object, using <see cref="Culture"/>.
    /// </summary>
    /// <param name="value">The date to format.</param>
    /// <param name="format">The format string, usually one of this object's properties.</param>
    /// <returns>The formatted date.</returns>
    public string Format(DateTime value, string format) => value.ToString(Normalize(format), Culture);

    /// <inheritdoc cref="Format(DateTime, string)" />
    public string Format(TimeOnly value, string format) => value.ToString(Normalize(format), Culture);

    /// <summary>
    /// Formats a date and time as <see cref="MediumDate"/> followed by <see cref="ShortTime"/>.
    /// </summary>
    /// <param name="value">The date and time to format.</param>
    /// <returns>The formatted date and time, for example <c>1 Oct 2026 09:05</c>.</returns>
    public string FormatDateTime(DateTime value) => $"{Format(value, MediumDate)} {Format(value, ShortTime)}";

    /// <summary>
    /// Gets the first day of the week containing <paramref name="date"/>.
    /// </summary>
    /// <param name="date">Any date in the week.</param>
    /// <returns>Midnight on the week's first day.</returns>
    public DateTime GetStartOfWeek(DateTime date) => date.Date.AddDays(-GetDayOffset(date.DayOfWeek));

    /// <summary>
    /// Gets how many days <paramref name="day"/> is after <see cref="FirstDayOfWeek"/> (0 to 6), which is the
    /// column it sits in on a week grid.
    /// </summary>
    /// <param name="day">The day of the week.</param>
    /// <returns>The zero based column.</returns>
    public int GetDayOffset(DayOfWeek day) => ((int)day - (int)FirstDayOfWeek + 7) % 7;

    /// <summary>
    /// Gets the 1-based week of the month <paramref name="date"/> falls in, counting the rows of a month grid
    /// that starts on <see cref="FirstDayOfWeek"/>.
    /// </summary>
    /// <param name="date">The date to place.</param>
    /// <returns>The week of the month, from 1 to 6.</returns>
    public int GetWeekOfMonth(DateTime date)
    {
        var firstOfMonth = new DateTime(date.Year, date.Month, 1, 0, 0, 0, date.Kind);
        return ((date.Date - GetStartOfWeek(firstOfMonth)).Days / 7) + 1;
    }

    /// <summary>
    /// Gets the days of the week in display order, starting from <see cref="FirstDayOfWeek"/>.
    /// </summary>
    /// <returns>The seven days of the week.</returns>
    public IReadOnlyList<DayOfWeek> GetWeekdays() =>
        [.. Enumerable.Range(0, 7).Select(i => (DayOfWeek)(((int)FirstDayOfWeek + i) % 7))];

    /// <summary>
    /// Gets the shortest weekday names in display order, starting from <see cref="FirstDayOfWeek"/>.
    /// </summary>
    /// <returns>The seven column headings of a week grid.</returns>
    public IReadOnlyList<string> GetWeekdayHeaders() =>
        [.. GetWeekdays().Select(day => Culture.DateTimeFormat.GetShortestDayName(day))];

    /// <summary>
    /// Gets the full weekday names in display order, starting from <see cref="FirstDayOfWeek"/>.
    /// </summary>
    /// <returns>The seven weekday names.</returns>
    public IReadOnlyList<string> GetWeekdayNames() =>
        [.. GetWeekdays().Select(day => Culture.DateTimeFormat.GetDayName(day))];

    /// <summary>
    /// Works out whether a time format string describes a 12 hour clock.
    /// </summary>
    /// <param name="pattern">A .NET time format string.</param>
    /// <returns><c>true</c> when the pattern has a 12 hour specifier outside quoted text.</returns>
    public static bool Is12HourPattern(string pattern)
    {
        var quote = '\0';
        var escaped = false;
        foreach (var current in pattern)
        {
            if (escaped)
            {
                escaped = false;
            }
            else if (quote != '\0')
            {
                if (current == quote)
                {
                    quote = '\0';
                }
            }
            else if (current is '\'' or '"')
            {
                quote = current;
            }
            else if (current == '\\')
            {
                escaped = true;
            }
            else if (current is 'h' or 't')
            {
                return true;
            }
        }

        return false;
    }

    // A one character custom specifier such as "d" or "t" would otherwise be read as a standard format.
    private static string Normalize(string format) =>
        format.Length == 1 && !IsStandardFormat(format[0]) ? $"%{format}" : format;

    private static bool IsStandardFormat(char format) => "dDfFgGmMoOrRstTuUyY".Contains(format);
}
