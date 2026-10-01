// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Calendar;

public class TwCalendarColorsTests
{
    public static readonly TheoryData<string?> InvalidColors = new()
    {
        (string?)null,
        "",
        "   ",
        "purple", // named color, not hex
        "9333ea", // missing '#'
        "#ggg", // not hex digits
        "#12", // too short
        "#1234567", // 7 digits - not a valid RGB/RGBA length
        "#9333ea; } body { display: none", // CSS-injection attempt
    };

    [Theory]
    [InlineData("#9333ea", "background-color:#9333ea26;border-left-color:#9333ea;color:#9333ea;")]
    [InlineData("#9333EA", "background-color:#9333EA26;border-left-color:#9333EA;color:#9333EA;")]
    [InlineData("#9333ea80", "background-color:#9333ea80;border-left-color:#9333ea80;color:#9333ea80;")] // already has alpha - used as-is, not re-tinted
    public void GetEventCardStyle_ValidHexColor_ReturnsBackgroundBorderAndTextDeclarations(string color, string expected)
    {
        Assert.Equal(expected, TwCalendarColors.GetEventCardStyle(color));
    }

    [Fact]
    public void GetEventCardStyle_ShorthandHex_IsExpandedToSixDigitsBeforeTinting()
    {
        Assert.Equal("background-color:#ffffff26;border-left-color:#ffffff;color:#ffffff;", TwCalendarColors.GetEventCardStyle("#fff"));
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
