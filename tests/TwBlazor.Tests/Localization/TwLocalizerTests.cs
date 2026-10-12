// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using TwBlazor.Configuration;
using TwBlazor.Localization;
using ThemeBase = TwBlazor.Theme.Theme;

namespace TwBlazor.Tests.Localization;

[Collection("ServiceCollectionExtensions")]
public partial class TwLocalizerTests
{
    private static readonly ResourceManager _builtIn = new("TwBlazor.Resources.TwBlazorStrings", typeof(TwLocalizer).Assembly);

    public static TheoryData<string> Languages => ["zh-Hans", "es", "hi", "pt", "bn", "ru", "ja", "pa-Arab", "tr", "vi", "zh-Hant", "ar", "mr", "te", "ko", "ta", "ur", "id", "de", "fr", "jv", "fa", "it", "pl"];

    private static TwLocalizer Create(Action<TwLocalizationOptions>? configure = null)
    {
        var options = new TwBlazorOptions { Theme = ThemeBase.CreateDefaultTheme() };
        configure?.Invoke(options.Localization);
        return new TwLocalizer(options);
    }

    private static TwLocalizer CreateFor(string culture, Action<TwLocalizationOptions>? configure = null) =>
        Create(localization =>
        {
            localization.Culture = CultureInfo.GetCultureInfo(culture);
            configure?.Invoke(localization);
        });

    private static IEnumerable<string> Keys() =>
        typeof(TwStrings).GetFields(BindingFlags.Public | BindingFlags.Static).Select(field => (string)field.GetRawConstantValue()!);

    private static string[] Placeholders(string value) =>
        [.. PlaceholderRegex().Matches(value).Select(match => match.Value).Distinct().Order()];

    [GeneratedRegex(@"\{\d+\}")]
    private static partial Regex PlaceholderRegex();

    [Fact]
    public void Indexer_ReturnsTheEnglishText_ByDefault()
    {
        var localizer = CreateFor("en-GB");

        Assert.Equal("Close", localizer[TwStrings.Common_Close]);
        Assert.Equal("Next page", localizer[TwStrings.Pagination_NextPage]);
    }

    [Fact]
    public void Indexer_FillsInPlaceholders()
    {
        var localizer = CreateFor("en-GB");

        Assert.Equal("Page 2 of 5", localizer[TwStrings.Pagination_PageStatus, 2, 5]);
    }

    [Fact]
    public void Indexer_ReturnsTheKey_WhenNothingIsRegisteredForIt()
    {
        var localizer = CreateFor("en-GB");

        Assert.Equal("Not_A_Key", localizer["Not_A_Key"]);
    }

    [Theory]
    [InlineData("es", "Cerrar")]
    [InlineData("pt", "Fechar")]
    [InlineData("ru", "Закрыть")]
    [InlineData("tr", "Kapat")]
    [InlineData("ja", "閉じる")]
    [InlineData("zh-Hans", "关闭")]
    [InlineData("hi", "बंद करें")]
    [InlineData("bn", "বন্ধ করুন")]
    [InlineData("pa-Arab", "بند کرو")]
    [InlineData("vi", "Đóng")]
    [InlineData("zh-Hant", "關閉")]
    [InlineData("ar", "إغلاق")]
    [InlineData("mr", "बंद करा")]
    [InlineData("te", "మూసివేయి")]
    [InlineData("ko", "닫기")]
    [InlineData("ta", "மூடு")]
    [InlineData("ur", "بند کریں")]
    [InlineData("id", "Tutup")]
    [InlineData("de", "Schließen")]
    [InlineData("fr", "Fermer")]
    [InlineData("jv", "Tutup")]
    [InlineData("fa", "بستن")]
    [InlineData("it", "Chiudi")]
    [InlineData("pl", "Zamknij")]
    public void Indexer_ReturnsTheTranslation_ForEachShippedLanguage(string culture, string expected)
    {
        var localizer = CreateFor(culture);

        Assert.Equal(expected, localizer[TwStrings.Common_Close]);
    }

    [Theory]
    [InlineData("es-MX", "Cerrar")]
    [InlineData("pt-BR", "Fechar")]
    [InlineData("zh-CN", "关闭")]
    [InlineData("pa-Arab-PK", "بند کرو")]
    [InlineData("zh-HK", "關閉")]
    [InlineData("zh-TW", "關閉")]
    [InlineData("ar-EG", "إغلاق")]
    [InlineData("de-AT", "Schließen")]
    [InlineData("fr-CA", "Fermer")]
    public void Indexer_FallsBackToTheParentCulture(string culture, string expected)
    {
        var localizer = CreateFor(culture);

        Assert.Equal(expected, localizer[TwStrings.Common_Close]);
    }

    [Fact]
    public void Indexer_FallsBackToEnglish_ForALanguageThatIsNotShipped()
    {
        var localizer = CreateFor("sv-SE");

        Assert.Equal("Close", localizer[TwStrings.Common_Close]);
    }

    [Fact]
    public void Indexer_FollowsTheCurrentUICulture_WhenNoCultureIsConfigured()
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("es");

            Assert.Equal("Cerrar", Create()[TwStrings.Common_Close]);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void RegisteredResources_WinOverTheBuiltInStrings_AndFallBackForOtherKeys()
    {
        var localizer = CreateFor("en-GB", localization => localization.AddResource(new StubResourceManager(new() { [TwStrings.Common_Close] = "Dismiss" })));

        Assert.Equal("Dismiss", localizer[TwStrings.Common_Close]);
        Assert.Equal("Cancel", localizer[TwStrings.Common_Cancel]);
    }

    [Fact]
    public void RegisteredResources_AreSearchedLastAddedFirst()
    {
        var localizer = CreateFor("en-GB", localization => localization
            .AddResource(new StubResourceManager(new() { [TwStrings.Common_Close] = "First", [TwStrings.Common_Save] = "Keep" }))
            .AddResource(new StubResourceManager(new() { [TwStrings.Common_Close] = "Second" })));

        Assert.Equal("Second", localizer[TwStrings.Common_Close]);
        Assert.Equal("Keep", localizer[TwStrings.Common_Save]);
    }

    [Fact]
    public void AddResource_RejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new TwLocalizationOptions().AddResource(null!));
    }

    [Fact]
    public void Plural_PicksTheSingularFormOnlyForOne()
    {
        var localizer = CreateFor("en-GB");

        Assert.Equal("1 result", localizer.Plural(1, TwStrings.Sidebar_ResultsOne, TwStrings.Sidebar_ResultsOther, 1));
        Assert.Equal("3 results", localizer.Plural(3, TwStrings.Sidebar_ResultsOne, TwStrings.Sidebar_ResultsOther, 3));
        Assert.Equal("0 results", localizer.Plural(0, TwStrings.Sidebar_ResultsOne, TwStrings.Sidebar_ResultsOther, 0));
    }

    [Fact]
    public void EveryKeyConstant_HasAnEnglishEntry_AndEveryEntryHasAConstant()
    {
        var resources = _builtIn.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
        var entries = resources.Cast<System.Collections.DictionaryEntry>().Select(entry => (string)entry.Key).ToHashSet();
        var keys = Keys().ToHashSet();

        Assert.Empty(keys.Except(entries));
        Assert.Empty(entries.Except(keys));
    }

    [Fact]
    public void EveryKeyConstant_IsNamedAfterItsValue()
    {
        Assert.All(
            typeof(TwStrings).GetFields(BindingFlags.Public | BindingFlags.Static),
            field => Assert.Equal(field.Name, (string)field.GetRawConstantValue()!));
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void LanguageFile_OnlyUsesKnownKeys_AndKeepsEveryPlaceholder(string culture)
    {
        var resources = _builtIn.GetResourceSet(CultureInfo.GetCultureInfo(culture), true, false);
        Assert.NotNull(resources);

        var entries = resources.Cast<System.Collections.DictionaryEntry>().ToList();
        Assert.NotEmpty(entries);
        Assert.All(entries, entry =>
        {
            var english = _builtIn.GetString((string)entry.Key, CultureInfo.InvariantCulture);
            Assert.NotNull(english);
            Assert.False(string.IsNullOrWhiteSpace((string?)entry.Value));
            Assert.Equal(Placeholders(english), Placeholders((string)entry.Value!));
        });
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void LanguageFile_TranslatesNearlyEveryString(string culture)
    {
        var resources = _builtIn.GetResourceSet(CultureInfo.GetCultureInfo(culture), true, false)!;
        var translated = resources.Cast<System.Collections.DictionaryEntry>().Select(entry => (string)entry.Key).ToHashSet();
        var missing = Keys().Where(key => !key.StartsWith("Format_", StringComparison.Ordinal) && !translated.Contains(key)).ToList();

        // A few strings are only punctuation around placeholders and read the same in some languages.
        Assert.True(missing.Count <= 5, $"{culture} is missing: {string.Join(", ", missing)}");
    }

    [Fact]
    public void Formats_FollowTheCulture()
    {
        var british = CreateFor("en-GB").Formats;
        var american = CreateFor("en-US").Formats;

        Assert.Equal(DayOfWeek.Monday, british.FirstDayOfWeek);
        Assert.False(british.Use12HourClock);
        Assert.Equal("HH:mm", british.Hour);
        Assert.Equal(DayOfWeek.Sunday, american.FirstDayOfWeek);
        Assert.True(american.Use12HourClock);
        Assert.Equal("h tt", american.Hour);
        Assert.Equal(CultureInfo.GetCultureInfo("en-US").DateTimeFormat.LongDatePattern, american.LongDate);
    }

    [Fact]
    public void Formats_AreBritish_WhenTheApplicationHasNoCulture()
    {
        var formats = Create(localization => localization.Culture = CultureInfo.InvariantCulture).Formats;
        var date = new DateTime(2026, 3, 18, 14, 5, 0);

        Assert.Equal(DayOfWeek.Monday, formats.FirstDayOfWeek);
        Assert.False(formats.Use12HourClock);
        Assert.Equal("14:05", formats.Format(date, formats.ShortTime));
        Assert.Equal("Wednesday, 18 March 2026", formats.Format(date, formats.LongDate));
        Assert.Equal("March 2026", formats.Format(date, formats.MonthYear));
        Assert.Equal("18 Mar 2026", formats.Format(date, formats.MediumDate));
        Assert.Equal("18 Mar 2026 14:05", formats.FormatDateTime(date));
    }

    [Fact]
    public void Formats_FollowTheCurrentCulture_WhenNoCultureIsConfigured()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            Assert.Equal(DayOfWeek.Sunday, Create().Formats.FirstDayOfWeek);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void Formats_ConfiguredValues_WinOverTheCulture()
    {
        var formats = CreateFor("en-GB", localization =>
        {
            localization.Formats.FirstDayOfWeek = DayOfWeek.Saturday;
            localization.Formats.ShortTime = "H.mm";
            localization.Formats.LongDate = "d MMMM yyyy";
            localization.Formats.MonthYear = "MM/yyyy";
            localization.Formats.DateInput = "yyyy-MM-dd";
            localization.Formats.DateTimeInput = "yyyy-MM-dd HH:mm";
        }).Formats;
        var date = new DateTime(2026, 3, 18, 9, 5, 0);

        Assert.Equal(DayOfWeek.Saturday, formats.FirstDayOfWeek);
        Assert.Equal("9.05", formats.Format(date, formats.ShortTime));
        Assert.Equal("18 March 2026", formats.Format(date, formats.LongDate));
        Assert.Equal("03/2026", formats.Format(date, formats.MonthYear));
        Assert.Equal("yyyy-MM-dd", formats.DateInput);
        Assert.Equal("yyyy-MM-dd HH:mm", formats.DateTimeInput);
    }

    [Fact]
    public void Formats_ResourceEntries_WinOverTheCulture_ButNotOverConfiguredValues()
    {
        var resource = new StubResourceManager(new() { [TwStrings.Format_ShortTime] = "HH'h'mm", [TwStrings.Format_LongDate] = "yyyy/MM/dd" });

        var fromResource = CreateFor("en-GB", localization => localization.AddResource(resource)).Formats;
        var configured = CreateFor("en-GB", localization =>
        {
            localization.AddResource(resource);
            localization.Formats.ShortTime = "HH:mm:ss";
        }).Formats;

        Assert.Equal("HH'h'mm", fromResource.ShortTime);
        Assert.Equal("yyyy/MM/dd", fromResource.LongDate);
        Assert.Equal("HH:mm:ss", configured.ShortTime);
    }

    [Theory]
    [InlineData(true, "h:mm tt", "h tt")]
    [InlineData(false, "HH:mm", "HH:mm")]
    public void Formats_Use12HourClock_DrivesTheTimeFormats(bool use12HourClock, string expectedShortTime, string expectedHour)
    {
        var formats = CreateFor("en-GB", localization => localization.Formats.Use12HourClock = use12HourClock).Formats;

        Assert.Equal(use12HourClock, formats.Use12HourClock);
        Assert.Equal(use12HourClock, formats.Use12HourInput);
        Assert.Equal(expectedShortTime, formats.ShortTime);
        Assert.Equal(expectedHour, formats.Hour);
    }

    [Fact]
    public void Formats_PickersStayOn24Hours_UnlessConfigured()
    {
        Assert.False(CreateFor("en-US").Formats.Use12HourInput);
        Assert.True(CreateFor("en-GB", localization => localization.Formats.TimeInput = "hh:mm tt").Formats.Use12HourInput);
    }

    [Fact]
    public void UseCultureInputFormats_CopiesTheCulturesPatterns()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        var options = new TwDateTimeFormatOptions().UseCultureInputFormats(culture);

        Assert.Equal(culture.DateTimeFormat.ShortDatePattern, options.DateInput);
        Assert.Equal(culture.DateTimeFormat.ShortTimePattern, options.TimeInput);
        Assert.Equal($"{culture.DateTimeFormat.ShortDatePattern} {culture.DateTimeFormat.ShortTimePattern}", options.DateTimeInput);
        Assert.True(options.Use12HourClock);
    }

    [Theory]
    [InlineData("h:mm tt", true)]
    [InlineData("hh:mm", true)]
    [InlineData("HH:mm", false)]
    [InlineData("H.mm", false)]
    [InlineData("HH'h'mm", false)]
    [InlineData("HH \\h mm", false)]
    public void Is12HourPattern_IgnoresQuotedAndEscapedText(string pattern, bool expected)
    {
        Assert.Equal(expected, TwDateTimeFormats.Is12HourPattern(pattern));
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, 2026, 3, 16)]
    [InlineData(DayOfWeek.Sunday, 2026, 3, 15)]
    [InlineData(DayOfWeek.Saturday, 2026, 3, 14)]
    public void GetStartOfWeek_UsesTheFirstDayOfWeek(DayOfWeek firstDayOfWeek, int year, int month, int day)
    {
        var formats = CreateFor("en-GB", localization => localization.Formats.FirstDayOfWeek = firstDayOfWeek).Formats;

        Assert.Equal(new DateTime(year, month, day), formats.GetStartOfWeek(new DateTime(2026, 3, 18, 13, 30, 0)));
    }

    [Fact]
    public void WeekHelpers_StartFromTheFirstDayOfWeek()
    {
        var formats = CreateFor("en-GB", localization => localization.Formats.FirstDayOfWeek = DayOfWeek.Sunday).Formats;

        Assert.Equal(0, formats.GetDayOffset(DayOfWeek.Sunday));
        Assert.Equal(6, formats.GetDayOffset(DayOfWeek.Saturday));
        Assert.Equal(DayOfWeek.Sunday, formats.GetWeekdays()[0]);
        Assert.Equal(DayOfWeek.Saturday, formats.GetWeekdays()[6]);
        Assert.Equal("Sunday", formats.GetWeekdayNames()[0]);
        Assert.Equal(7, formats.GetWeekdayHeaders().Count);
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, 1, 1)]
    [InlineData(DayOfWeek.Monday, 2, 2)]
    [InlineData(DayOfWeek.Monday, 31, 6)]
    [InlineData(DayOfWeek.Sunday, 1, 1)]
    [InlineData(DayOfWeek.Sunday, 31, 5)]
    public void GetWeekOfMonth_CountsRowsOfTheMonthGrid(DayOfWeek firstDayOfWeek, int day, int expectedWeek)
    {
        // March 2026 starts on a Sunday.
        var formats = CreateFor("en-GB", localization => localization.Formats.FirstDayOfWeek = firstDayOfWeek).Formats;

        Assert.Equal(expectedWeek, formats.GetWeekOfMonth(new DateTime(2026, 3, day)));
    }

    [Fact]
    public void Format_TreatsASingleCustomSpecifierAsCustom()
    {
        var formats = CreateFor("en-GB").Formats;

        Assert.Equal("9", formats.Format(new DateTime(2026, 3, 18, 9, 5, 0), "h"));
        Assert.Equal("09:05", formats.Format(new TimeOnly(9, 5), "HH:mm"));
    }

    [Fact]
    public void Formats_UseTheLanguagesOwnMonthNames()
    {
        var formats = CreateFor("es").Formats;

        Assert.Contains("marzo", formats.Format(new DateTime(2026, 3, 18), formats.LongDate));
    }

    [Fact]
    public void AddTwBlazor_RegistersTheDefaultLocalizer()
    {
        var services = new ServiceCollection();
        services.AddTwBlazor(options => options.Localization.Culture = CultureInfo.GetCultureInfo("es"), ThemeBase.CreateDefaultTheme());

        var localizer = services.BuildServiceProvider().GetRequiredService<ITwLocalizer>();

        Assert.IsType<TwLocalizer>(localizer);
        Assert.Equal("Cerrar", localizer[TwStrings.Common_Close]);
    }

    [Fact]
    public void AddTwBlazor_KeepsALocalizerRegisteredBeforeIt()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITwLocalizer, StubLocalizer>();
        services.AddTwBlazor(ThemeBase.CreateDefaultTheme());

        Assert.IsType<StubLocalizer>(services.BuildServiceProvider().GetRequiredService<ITwLocalizer>());
    }

    private sealed class StubResourceManager(Dictionary<string, string> values) : ResourceManager
    {
        public override string? GetString(string name, CultureInfo? culture) => values.GetValueOrDefault(name);
    }

    private sealed class StubLocalizer : ITwLocalizer
    {
        public string this[string key] => key;

        public string this[string key, params object?[] arguments] => key;

        public TwDateTimeFormats Formats => throw new NotSupportedException();
    }
}
