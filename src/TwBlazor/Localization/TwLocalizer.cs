// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Globalization;
using System.Resources;
using TwBlazor.Configuration;

namespace TwBlazor.Localization;

/// <summary>
/// The default <see cref="ITwLocalizer"/>. It reads the resource files registered on
/// <see cref="TwLocalizationOptions.Resources"/> first and falls back to TwBlazor's built in strings.
/// </summary>
public class TwLocalizer : ITwLocalizer
{
    private static readonly ResourceManager _builtInResources = new("TwBlazor.Resources.TwBlazorStrings", typeof(TwLocalizer).Assembly);

    private readonly TwLocalizationOptions _localization;
    private readonly ConcurrentDictionary<(string Text, string Format), TwDateTimeFormats> _formatsByCulture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TwLocalizer"/> class.
    /// </summary>
    /// <param name="options">The options <c>AddTwBlazor</c> was configured with.</param>
    public TwLocalizer(TwBlazorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _localization = options.Localization;
    }

    /// <summary>
    /// Gets the culture text is looked up for.
    /// </summary>
    protected virtual CultureInfo textCulture => _localization.Culture ?? CultureInfo.CurrentUICulture;

    /// <summary>
    /// Gets the culture dates, times and numbers are formatted with.
    /// </summary>
    protected virtual CultureInfo formatCulture => _localization.Culture ?? CultureInfo.CurrentCulture;

    /// <inheritdoc />
    public virtual string this[string key] => GetString(key, textCulture) ?? key;

    /// <inheritdoc />
    public virtual string this[string key, params object?[] arguments] => string.Format(formatCulture, this[key], arguments);

    /// <inheritdoc />
    public virtual TwDateTimeFormats Formats
    {
        get
        {
            var text = textCulture;
            var format = formatCulture;
            return _formatsByCulture.GetOrAdd((text.Name, format.Name), _ => CreateFormats(text, format));
        }
    }

    /// <summary>
    /// Looks a key up in the registered resource files, then in the built in strings.
    /// </summary>
    /// <param name="key">One of the <see cref="TwStrings"/> keys.</param>
    /// <param name="culture">The culture to look the key up for.</param>
    /// <returns>The text, or <c>null</c> when no resource file has a value for the key.</returns>
    protected virtual string? GetString(string key, CultureInfo culture)
    {
        for (var i = _localization.Resources.Count - 1; i >= 0; i--)
        {
            var value = _localization.Resources[i].GetString(key, culture);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
        }

        return _builtInResources.GetString(key, culture);
    }

    /// <summary>
    /// Resolves the date and time formats for a culture: the configured options first, then the <c>Format_</c>
    /// entries of the resource files, then the culture's own patterns.
    /// </summary>
    /// <param name="text">The culture resource files are read for.</param>
    /// <param name="format">The culture dates and times are formatted with.</param>
    /// <returns>The resolved formats.</returns>
    protected virtual TwDateTimeFormats CreateFormats(CultureInfo text, CultureInfo format)
    {
        var configured = _localization.Formats;
        var pattern = format.DateTimeFormat;

        var timeInput = configured.TimeInput ?? Resource(TwStrings.Format_TimeInput);
        var configuredShortTime = configured.ShortTime ?? Resource(TwStrings.Format_ShortTime);
        var shortTime = configuredShortTime ?? configured.Use12HourClock switch
        {
            true => "h:mm tt",
            false => "HH:mm",
            null => pattern.ShortTimePattern
        };
        var use12HourClock = configured.Use12HourClock ?? TwDateTimeFormats.Is12HourPattern(shortTime);

        return new TwDateTimeFormats
        {
            Culture = format,
            FirstDayOfWeek = configured.FirstDayOfWeek ?? pattern.FirstDayOfWeek,
            Use12HourClock = use12HourClock,
            Use12HourInput = configured.Use12HourClock ?? (timeInput is not null && TwDateTimeFormats.Is12HourPattern(timeInput)),
            ShortTime = shortTime,
            Hour = configured.Hour ?? Resource(TwStrings.Format_Hour) ?? (use12HourClock ? "h tt" : "HH:mm"),
            LongDate = configured.LongDate ?? Resource(TwStrings.Format_LongDate) ?? pattern.LongDatePattern,
            MediumDate = configured.MediumDate ?? Resource(TwStrings.Format_MediumDate) ?? "d MMM yyyy",
            MonthYear = configured.MonthYear ?? Resource(TwStrings.Format_MonthYear) ?? pattern.YearMonthPattern,
            MonthDay = configured.MonthDay ?? Resource(TwStrings.Format_MonthDay) ?? pattern.MonthDayPattern,
            Month = configured.Month ?? Resource(TwStrings.Format_Month) ?? "MMMM",
            ShortMonth = configured.ShortMonth ?? Resource(TwStrings.Format_ShortMonth) ?? "MMM",
            Year = configured.Year ?? Resource(TwStrings.Format_Year) ?? "yyyy",
            WeekdayDay = configured.WeekdayDay ?? Resource(TwStrings.Format_WeekdayDay) ?? "ddd d",
            DateInput = configured.DateInput ?? Resource(TwStrings.Format_DateInput),
            TimeInput = timeInput,
            DateTimeInput = configured.DateTimeInput ?? Resource(TwStrings.Format_DateTimeInput)
        };

        string? Resource(string key)
        {
            var value = GetString(key, text);
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}
