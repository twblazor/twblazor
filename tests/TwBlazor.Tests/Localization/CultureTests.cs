// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using System.Globalization;
using TwBlazor.Components;
using TwBlazor.Configuration;
using TwBlazor.Enums;
using TwBlazor.Localization;
using ThemeBase = TwBlazor.Theme.Theme;

namespace TwBlazor.Tests.Localization;

[Collection("ServiceCollectionExtensions")]
public class CultureTests
{
    public static TheoryData<CultureInfo> Cultures => new(TestCultures.All);

    private static BunitContext CreateContext(CultureInfo culture)
    {
        var context = new BunitContext();
        context.Services.AddTwBlazor(options => options.Localization.Culture = culture, ThemeBase.CreateDefaultTheme());
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    private static TwLocalizer CreateLocalizer(CultureInfo culture)
    {
        var options = new TwBlazorOptions { Theme = ThemeBase.CreateDefaultTheme() };
        options.Localization.Culture = culture;
        return new TwLocalizer(options);
    }

    [Theory]
    [MemberData(nameof(Cultures))]
    public void Numeric_FormatsWithTheCulturesSeparators(CultureInfo culture)
    {
        using var context = CreateContext(culture);

        var cut = context.Render<TwNumeric<decimal>>(p => p.Add(x => x.Value, 1234.5m));

        // The general format writes the number with the culture's decimal separator ("1234,5" in de-DE, "1234٫5"
        // in ar), which is exactly what the field shows when no Format parameter is set.
        Assert.Equal(1234.5m.ToString(culture), cut.Find("input").GetAttribute("value"));
    }

    [Theory]
    [MemberData(nameof(Cultures))]
    public void Attachment_FileSize_UsesTheCulturesSeparator(CultureInfo culture)
    {
        // FormatFileSize rounds 1536 bytes down to "1.5 KB", and its "0.#" format uses the culture's decimal
        // separator, which for a single decimal place matches the culture's general format of 1.5.
        Assert.Equal((1.5).ToString(culture) + " KB", TwAttachment.FormatFileSize(1536, culture));
    }

    [Theory]
    [MemberData(nameof(Cultures))]
    public void Localizer_DerivesTheCulturesDateFormats(CultureInfo culture)
    {
        var formats = CreateLocalizer(culture).Formats;

        Assert.Equal(culture.DateTimeFormat.LongDatePattern, formats.LongDate);
        Assert.Equal(culture.DateTimeFormat.YearMonthPattern, formats.MonthYear);
        Assert.Equal(culture.DateTimeFormat.ShortTimePattern, formats.ShortTime);
        Assert.Equal(culture.DateTimeFormat.FirstDayOfWeek, formats.FirstDayOfWeek);
    }

    [Theory]
    [MemberData(nameof(Cultures))]
    public void Direction_FollowsTheLanguage(CultureInfo culture)
    {
        var expected = culture.TextInfo.IsRightToLeft ? TextDirection.RightToLeft : TextDirection.LeftToRight;

        Assert.Equal(expected, CreateLocalizer(culture).Direction);
    }
}
