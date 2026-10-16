// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Utilities;

namespace TwBlazor.Tests.Utilities;

public class TimeStepHelpersTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(120)]
    [InlineData(180)]
    [InlineData(720)]
    public void IsValidStep_AcceptsStepsThatLineUpWithTheClock(int step)
    {
        Assert.True(TimeStepHelpers.IsValidStep(step));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-15)]
    [InlineData(7)]
    [InlineData(45)]
    [InlineData(90)]
    [InlineData(300)]
    [InlineData(1440)]
    public void IsValidStep_RejectsStepsThatDriftFromTheClock(int step)
    {
        Assert.False(TimeStepHelpers.IsValidStep(step));
    }

    [Fact]
    public void ValidateStep_Throws_ForUnsupportedStep()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => TimeStepHelpers.ValidateStep(90));

        Assert.Equal("step", exception.ParamName);
    }

    [Theory]
    [InlineData(10, 30, 1, 10, 31)]
    [InlineData(10, 30, 15, 10, 45)]
    [InlineData(10, 37, 15, 10, 45)] // off the step: moves to the next one
    [InlineData(10, 45, 15, 11, 0)] // rolls into the next hour
    [InlineData(23, 45, 15, 0, 0)] // wraps past midnight
    [InlineData(10, 30, 120, 12, 0)]
    [InlineData(10, 0, 120, 12, 0)]
    [InlineData(22, 0, 120, 0, 0)]
    public void StepUp_MovesToTheNextStep(int hour, int minute, int step, int expectedHour, int expectedMinute)
    {
        var result = TimeStepHelpers.StepUp(new TimeOnly(hour, minute), step);

        Assert.Equal(new TimeOnly(expectedHour, expectedMinute), result);
    }

    [Theory]
    [InlineData(10, 30, 1, 10, 29)]
    [InlineData(10, 30, 15, 10, 15)]
    [InlineData(10, 37, 15, 10, 30)] // off the step: moves to the one before it
    [InlineData(10, 0, 15, 9, 45)] // rolls into the previous hour
    [InlineData(0, 0, 15, 23, 45)] // wraps past midnight
    [InlineData(10, 30, 120, 10, 0)]
    [InlineData(10, 0, 120, 8, 0)]
    [InlineData(0, 0, 120, 22, 0)]
    public void StepDown_MovesToThePreviousStep(int hour, int minute, int step, int expectedHour, int expectedMinute)
    {
        var result = TimeStepHelpers.StepDown(new TimeOnly(hour, minute), step);

        Assert.Equal(new TimeOnly(expectedHour, expectedMinute), result);
    }

    [Fact]
    public void StepUpAndDown_KeepSeconds()
    {
        var time = new TimeOnly(10, 30, 42);

        Assert.Equal(42, TimeStepHelpers.StepUp(time, 15).Second);
        Assert.Equal(42, TimeStepHelpers.StepDown(time, 15).Second);
    }
}
