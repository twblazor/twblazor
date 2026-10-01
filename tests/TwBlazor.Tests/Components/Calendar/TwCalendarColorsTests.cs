// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Calendar;

public class TwCalendarColorsTests
{
    public static readonly TheoryData<string?> InvalidColors =
    [
        (string?)null,
        "",
        "   ",
        "purple", // named color, not hex
        "9333ea", // missing '#'
        "#ggg", // not hex digits
        "#12", // too short
        "#1234567", // 7 digits - not a valid RGB/RGBA length
        "#9333ea; } body { display: none", // CSS-injection attempt
    ];

    [Theory]
    [InlineData("#9333ea", "background-color:#9333ea26;border-left-color:#9333ea;")]
    [InlineData("#9333EA", "background-color:#9333EA26;border-left-color:#9333EA;")]
    [InlineData("#9333ea80", "background-color:#9333ea80;border-left-color:#9333ea80;")] // already has alpha - used as-is, not re-tinted
    [InlineData("#fff", "background-color:#ffffff26;border-left-color:#ffffff;")] // shorthand is expanded before tinting
    public void GetEventCardStyle_ValidHexColor_ReturnsTintedBackgroundAndAccentBorder(string color, string expectedPrefix)
    {
        Assert.StartsWith(expectedPrefix + "color:light-dark(", TwCalendarColors.GetEventCardStyle(color));
    }

    [Theory]
    [InlineData("#f59e0b")]
    [InlineData("#e03c3c")]
    [InlineData("#3b73ed")]
    [InlineData("#9333ea")]
    [InlineData("#ffffff")]
    [InlineData("#000000")]
    public void GetEventCardStyle_TextColor_IsReadableOnTheTintInLightAndDarkMode(string color)
    {
        var style = TwCalendarColors.GetEventCardStyle(color);
        var match = System.Text.RegularExpressions.Regex.Match(style, @"light-dark\((#[0-9a-f]{6}),(#[0-9a-f]{6})\)");
        Assert.True(match.Success, style);

        var rgb = Channels(color);
        var light = Blend(Channels("#ffffff"), rgb, 0x26 / 255d);
        var dark = Blend(Channels("#111827"), rgb, 0x26 / 255d);

        Assert.True(Contrast(Channels(match.Groups[1].Value), light) >= 4.5);
        Assert.True(Contrast(Channels(match.Groups[2].Value), dark) >= 4.5);
    }

    private static double[] Channels(string hex) =>
        [Convert.ToInt32(hex[1..3], 16), Convert.ToInt32(hex[3..5], 16), Convert.ToInt32(hex[5..7], 16)];

    private static double[] Blend(double[] bottom, double[] top, double amount) =>
        [.. bottom.Zip(top, (b, t) => b + ((t - b) * amount))];

    private static double Contrast(double[] a, double[] b)
    {
        var la = Luminance(a);
        var lb = Luminance(b);
        return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
    }

    private static double Luminance(double[] c)
    {
        var linear = c.Select(v => v / 255d).Select(v => v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4)).ToArray();
        return (0.2126 * linear[0]) + (0.7152 * linear[1]) + (0.0722 * linear[2]);
    }

    [Theory]
    [MemberData(nameof(InvalidColors))]
    public void GetEventCardStyle_InvalidColor_ReturnsEmptyString(string? color)
    {
        Assert.Equal(string.Empty, TwCalendarColors.GetEventCardStyle(color));
    }

    [Theory]
    [InlineData("#9333ea", "background-color:#9333ea26;border-left-color:#9333ea;")]
    [InlineData("#fff", "background-color:#ffffff26;border-left-color:#ffffff;")]
    [InlineData("#9333ea80", "background-color:#9333ea80;border-left-color:#9333ea80;")]
    public void GetAccentCardStyle_ValidHexColor_ReturnsTintAndAccentBorderWithoutTextColor(string color, string expected)
    {
        Assert.Equal(expected, TwCalendarColors.GetAccentCardStyle(color));
    }

    [Theory]
    [InlineData("#9333ea", "background-color:#9333ea;")]
    [InlineData("#fff", "background-color:#ffffff;")]
    public void GetSolidBackgroundStyle_ValidHexColor_ReturnsSolidBackground(string color, string expected)
    {
        Assert.Equal(expected, TwCalendarColors.GetSolidBackgroundStyle(color));
    }

    [Theory]
    [InlineData("#9333ea", "color:#9333ea;")]
    [InlineData("#abc", "color:#aabbcc;")]
    public void GetSolidTextStyle_ValidHexColor_ReturnsTextColor(string color, string expected)
    {
        Assert.Equal(expected, TwCalendarColors.GetSolidTextStyle(color));
    }

    [Theory]
    [MemberData(nameof(InvalidColors))]
    public void AccentStyles_InvalidColor_ReturnEmptyString(string? color)
    {
        Assert.Equal(string.Empty, TwCalendarColors.GetAccentCardStyle(color));
        Assert.Equal(string.Empty, TwCalendarColors.GetSolidBackgroundStyle(color));
        Assert.Equal(string.Empty, TwCalendarColors.GetSolidTextStyle(color));
    }
}
