// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Schedule;

public class TwScheduleColorsTests
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
        Assert.Equal(expected, TwScheduleColors.GetEventCardStyle(color));
    }

    [Fact]
    public void GetEventCardStyle_ShorthandHex_IsExpandedToSixDigitsBeforeTinting()
    {
        Assert.Equal("background-color:#ffffff26;border-left-color:#ffffff;color:#ffffff;", TwScheduleColors.GetEventCardStyle("#fff"));
    }

    [Theory]
    [MemberData(nameof(InvalidColors))]
    public void GetEventCardStyle_InvalidColor_ReturnsEmptyString(string? color)
    {
        Assert.Equal(string.Empty, TwScheduleColors.GetEventCardStyle(color));
    }
}
