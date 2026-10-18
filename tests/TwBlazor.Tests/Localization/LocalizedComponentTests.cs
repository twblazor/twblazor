// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using System.Globalization;
using TwBlazor.Components;
using TwBlazor.Components.DatePicker;
using TwBlazor.Configuration;
using ThemeBase = TwBlazor.Theme.Theme;

namespace TwBlazor.Tests.Localization;

[Collection("ServiceCollectionExtensions")]
public class LocalizedComponentTests
{
    private static BunitContext CreateContext(Action<TwLocalizationOptions> configure)
    {
        var context = new BunitContext();
        context.Services.AddTwBlazor(options => configure(options.Localization), ThemeBase.CreateDefaultTheme());
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    private static BunitContext CreateContext(string culture) =>
        CreateContext(localization => localization.Culture = CultureInfo.GetCultureInfo(culture));

    [Fact]
    public void Pagination_RendersItsLabelsInTheConfiguredLanguage()
    {
        using var context = CreateContext("es");

        var cut = context.Render<TwPagination>(p => p
            .Add(x => x.TotalPages, 5)
            .Add(x => x.ActivePage, 2));

        Assert.NotNull(cut.Find("[aria-label='Página anterior']"));
        Assert.NotNull(cut.Find("[aria-label='Página siguiente']"));
        Assert.Equal("paginación", cut.Find("nav").GetAttribute("aria-label"));
    }

    [Fact]
    public void Alert_CloseButton_IsTranslated()
    {
        using var context = CreateContext("ja");

        var cut = context.Render<TwAlert>(p => p.Add(x => x.Dismissible, true));

        Assert.NotNull(cut.Find("button[aria-label='閉じる']"));
    }

    [Fact]
    public void Select_Placeholder_ComesFromTheLanguage_UnlessTheParameterIsSet()
    {
        using var context = CreateContext("pt");

        var localized = context.Render<TwSelect<string>>(p => p
            .Add(x => x.PreferNativePicker, true)
            .Add(x => x.Values, ["One", "Two"]));
        var explicitPlaceholder = context.Render<TwSelect<string>>(p => p
            .Add(x => x.PreferNativePicker, true)
            .Add(x => x.Placeholder, "Pick one")
            .Add(x => x.Values, ["One", "Two"]));

        Assert.Contains("Selecione uma opção...", localized.Markup);
        Assert.Contains("Pick one", explicitPlaceholder.Markup);
        Assert.DoesNotContain("Selecione uma opção...", explicitPlaceholder.Markup);
    }

    [Fact]
    public void Spinner_Label_IsTranslated()
    {
        using var context = CreateContext("tr");

        var cut = context.Render<TwSpinner>();

        Assert.Contains("Yükleniyor...", cut.Markup);
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, 23)]
    [InlineData(DayOfWeek.Sunday, 1)]
    public void CalendarMonthView_StartsTheWeekOnTheConfiguredDay(DayOfWeek firstDayOfWeek, int expectedFirstDay)
    {
        // March 2026 starts on a Sunday, so a Monday-first grid opens on 23 February.
        using var context = CreateContext(localization =>
        {
            localization.Culture = CultureInfo.GetCultureInfo("en-GB");
            localization.Formats.FirstDayOfWeek = firstDayOfWeek;
        });

        var cut = context.Render<TwCalendarMonthView<string>>(p => p.Add(x => x.Date, new DateTime(2026, 3, 15)));

        var headers = cut.FindAll("th[role='columnheader']");
        var formats = CultureInfo.GetCultureInfo("en-GB").DateTimeFormat;
        Assert.Equal(formats.GetShortestDayName(firstDayOfWeek), headers[0].TextContent.Trim());
        Assert.StartsWith(expectedFirstDay.ToString(CultureInfo.InvariantCulture), cut.Find("tbody td button").TextContent.Trim());
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, 6)]
    [InlineData(DayOfWeek.Sunday, 0)]
    public void DatePickerDayView_StartsTheWeekOnTheConfiguredDay(DayOfWeek firstDayOfWeek, int expectedLeadInDays)
    {
        using var context = CreateContext(localization =>
        {
            localization.Culture = CultureInfo.GetCultureInfo("en-GB");
            localization.Formats.FirstDayOfWeek = firstDayOfWeek;
        });

        var cut = context.Render<TwDatePickerDayView>(p => p.Add(x => x.Value, new DateTime(2026, 3, 15)));

        Assert.Equal(expectedLeadInDays, cut.FindAll("span.day.prev").Count);
        Assert.Equal(CultureInfo.GetCultureInfo("en-GB").DateTimeFormat.GetDayName(firstDayOfWeek), cut.Find("th abbr").GetAttribute("title"));
    }

    [Fact]
    public void DatePickerDayView_NamesDaysWithTheLongDateFormat()
    {
        using var context = CreateContext(localization =>
        {
            localization.Culture = CultureInfo.GetCultureInfo("en-GB");
            localization.Formats.LongDate = "d MMMM yyyy";
        });

        var cut = context.Render<TwDatePickerDayView>(p => p.Add(x => x.Value, new DateTime(2026, 3, 15)));

        Assert.NotNull(cut.Find("button[aria-label='15 March 2026']"));
        Assert.NotNull(cut.Find("button[aria-label='16 March 2026']"));
    }

    [Fact]
    public void DatePicker_UsesTheConfiguredInputFormat_UnlessTheParameterIsSet()
    {
        using var context = CreateContext(localization =>
        {
            localization.Culture = CultureInfo.GetCultureInfo("en-GB");
            localization.Formats.DateInput = "yyyy.MM.dd";
        });

        var configured = context.Render<TwDatePicker>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.SelectedDate, new DateTime(2026, 3, 18)));
        var explicitFormat = context.Render<TwDatePicker>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Format, "dd-MM-yyyy")
            .Add(x => x.SelectedDate, new DateTime(2026, 3, 18)));

        Assert.Equal("2026.03.18", configured.Find("input").GetAttribute("value"));
        Assert.Equal("18-03-2026", explicitFormat.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void TimePicker_Uses12Hours_WhenConfigured_UnlessTheParameterSaysOtherwise()
    {
        using var context = CreateContext(localization =>
        {
            localization.Culture = CultureInfo.GetCultureInfo("en-US");
            localization.Formats.Use12HourClock = true;
        });

        var configured = context.Render<TwTimePicker>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.SelectedTime, new TimeOnly(14, 30)));
        var explicit24Hour = context.Render<TwTimePicker>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Is12HourFormat, false)
            .Add(x => x.SelectedTime, new TimeOnly(14, 30)));

        Assert.Equal("02:30 PM", configured.Find("input").GetAttribute("value"));
        Assert.Equal("14:30", explicit24Hour.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void TimePicker_StaysOn24Hours_ByDefault_EvenForA12HourCulture()
    {
        using var context = CreateContext("en-US");

        var cut = context.Render<TwTimePicker>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.SelectedTime, new TimeOnly(14, 30)));

        Assert.Equal("14:30", cut.Find("input").GetAttribute("value"));
    }

    [Theory]
    [InlineData("en-GB", "09:00")]
    [InlineData("en-US", "9:00 AM")]
    public void CalendarDayColumn_LabelsSlotsWithTheCulturesTimeFormat(string culture, string expected)
    {
        using var context = CreateContext(culture);

        var cut = context.Render<TwCalendarDayColumn<string>>(p => p
            .Add(x => x.Date, new DateTime(2026, 3, 18))
            .Add(x => x.Editable, true));

        Assert.Equal(expected, cut.FindAll("button")[18].GetAttribute("aria-label")?.Replace(' ', ' '));
    }

    [Fact]
    public void Progress_KeepsItsShimmerWidthInvariant_WhenTheCultureUsesADecimalComma()
    {
        using var context = CreateContext("ru");

        var cut = context.Render<TwProgress<double>>(p => p
            .Add(x => x.Value, 45.5)
            .Add(x => x.Max, 100)
            .Add(x => x.Animated, true));

        Assert.Contains("width: 45.5%", cut.Find("span[aria-hidden='true']").GetAttribute("style"));
        Assert.Contains("45,5%", cut.Find("progress").TextContent);
    }

    [Fact]
    public void Progress_IndeterminateFallbackText_IsTranslated()
    {
        using var context = CreateContext("es");

        var cut = context.Render<TwProgress<int>>(p => p
            .Add(x => x.Max, 100)
            .Add(x => x.Indeterminate, true));

        Assert.Contains("Cargando...", cut.Find("progress").TextContent);
    }
}
