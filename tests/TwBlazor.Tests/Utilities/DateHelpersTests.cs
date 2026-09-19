// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Utilities;

namespace TwBlazor.Tests.Utilities;

public class DateHelpersTests
{
    [Theory]
    [InlineData(2026, 3, 18, 12)] // Wednesday
    [InlineData(2026, 1, 1, 1)] // Thursday, ISO week 1
    [InlineData(2025, 12, 29, 1)] // Monday, belongs to 2026's ISO week 1
    public void GetWeekOfYear_ReturnsIsoWeekNumber(int year, int month, int day, int expectedWeek)
    {
        var date = new DateTime(year, month, day);

        Assert.Equal(expectedWeek, DateHelpers.GetWeekOfYear(date));
    }

    [Theory]
    [InlineData(2026, 3, 18, 2026, 3, 16)] // Wednesday -> Monday same week
    [InlineData(2026, 3, 16, 2026, 3, 16)] // Monday -> itself
    [InlineData(2026, 3, 22, 2026, 3, 16)] // Sunday -> Monday same week
    public void GetStartOfWeek_ReturnsMonday(int year, int month, int day, int expectedYear, int expectedMonth, int expectedDay)
    {
        var date = new DateTime(year, month, day, 14, 30, 0);

        var startOfWeek = DateHelpers.GetStartOfWeek(date);

        Assert.Equal(new DateTime(expectedYear, expectedMonth, expectedDay), startOfWeek);
        Assert.Equal(DayOfWeek.Monday, startOfWeek.DayOfWeek);
    }

    [Fact]
    public void GetStartOfWeek_DropsTimeOfDay()
    {
        var date = new DateTime(2026, 3, 18, 23, 59, 59);

        var startOfWeek = DateHelpers.GetStartOfWeek(date);

        Assert.Equal(TimeSpan.Zero, startOfWeek.TimeOfDay);
    }

    [Theory]
    [InlineData(2026, 3, 1, 1)] // March 2026: 1st is a Sunday, in the grid's first (lead-in) week
    [InlineData(2026, 3, 2, 2)] // first Monday of March, second grid row
    [InlineData(2026, 3, 18, 4)] // matches the Month view's 4th row for this date
    [InlineData(2026, 3, 31, 6)] // March 2026 spans 6 grid rows - the max, not an ISO week-of-year number
    [InlineData(2026, 4, 1, 1)] // April 2026: 1st is a Wednesday, still the grid's first row
    [InlineData(2026, 4, 30, 5)] // April 2026 spans exactly 5 grid rows
    public void GetWeekOfMonth_MatchesMonthViewsGridRow(int year, int month, int day, int expectedWeek)
    {
        var date = new DateTime(year, month, day);

        Assert.Equal(expectedWeek, DateHelpers.GetWeekOfMonth(date));
    }

    [Fact]
    public void GetWeekOfMonth_NeverExceedsSix()
    {
        for (var month = 1; month <= 12; month++)
        {
            var daysInMonth = DateTime.DaysInMonth(2026, month);
            for (var day = 1; day <= daysInMonth; day++)
            {
                var week = DateHelpers.GetWeekOfMonth(new DateTime(2026, month, day));
                Assert.InRange(week, 1, 6);
            }
        }
    }
}
