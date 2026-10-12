// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Calendar;

public class TwCalendarTimingsTests
{
    [Fact]
    public void Timings_HaveTheDocumentedValues()
    {
        Assert.Equal(TimeSpan.FromSeconds(30), TwCalendarTimings.nowTickInterval);
        Assert.Equal(TimeSpan.FromSeconds(2), TwCalendarTimings.highlightDuration);
        Assert.Equal(TimeSpan.FromMilliseconds(25), TwCalendarTimings.dragActivationDelay);
    }

    [Fact]
    public void AllViews_ListsEveryViewInSwitcherOrder()
    {
        Assert.Equal([TwCalendarView.Day, TwCalendarView.Week, TwCalendarView.Month], TwCalendarTimings.allViews);
    }
}

public class TwCalendarTimeGridTests
{
    [Fact]
    public void SlotHeightRem_IsThreeRem_ForAThirtyMinuteSlot()
    {
        // A decimal constant is inlined wherever it is used, so its declaration is only read through reflection.
        var slotHeight = typeof(TwCalendarTimeGrid).GetField(nameof(TwCalendarTimeGrid.SlotHeightRem))!.GetValue(null);

        Assert.Equal(3m, slotHeight);
        Assert.Equal(30, TwCalendarTimeGrid.MinutesPerSlot);
        Assert.Equal(TwCalendarTimeGrid.SlotHeightRem, TwCalendarTimeGrid.GetOffsetRem(TwCalendarTimeGrid.MinutesPerSlot));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(30, 3)]
    [InlineData(60, 6)]
    [InlineData(90, 9)]
    [InlineData(1440, 144)]
    public void GetOffsetRem_ConvertsMinutesToRem(double minutes, double expectedRem)
    {
        Assert.Equal((decimal)expectedRem, TwCalendarTimeGrid.GetOffsetRem(minutes));
    }

    [Theory]
    [InlineData(-45)]
    [InlineData(-0.5)]
    public void GetOffsetRem_ClampsNegativeTimesToTheTop(double minutes)
    {
        Assert.Equal(0m, TwCalendarTimeGrid.GetOffsetRem(minutes));
    }

    [Fact]
    public void GetOffsetRem_ClampsTimesPastMidnightToTheBottom()
    {
        Assert.Equal(144m, TwCalendarTimeGrid.GetOffsetRem(5000));
    }

    [Fact]
    public void Constants_DescribeAThirtyMinuteThreeRemRow()
    {
        Assert.Equal(30, TwCalendarTimeGrid.MinutesPerSlot);
        Assert.Equal(3m, TwCalendarTimeGrid.SlotHeightRem);
    }
}
