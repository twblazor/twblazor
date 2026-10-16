// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Components.TimePicker;

namespace TwBlazor.Tests.Components.TimePicker;

public class TwTimePickerBodyKeyboardTests : TwBlazorTestBase
{
    private TimeOnly? changedTo;

    private IRenderedComponent<TwTimePickerBody> Render(int step = 1, TimeOnly? time = null) =>
        TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, time ?? new TimeOnly(10, 30))
            .Add(x => x.Step, step)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, time => changedTo = time)));

    [Theory]
    [InlineData(0, "ArrowUp", 11, 30)]
    [InlineData(0, "ArrowDown", 9, 30)]
    [InlineData(1, "ArrowUp", 10, 31)]
    [InlineData(1, "ArrowDown", 10, 29)]
    public void ArrowUpAndDown_StepTheFocusedField(int fieldIndex, string key, int expectedHour, int expectedMinute)
    {
        // Arrange
        var cut = Render();

        // Act
        cut.FindAll("input[type='text']")[fieldIndex].KeyDown(new KeyboardEventArgs { Key = key });

        // Assert
        Assert.Equal(new TimeOnly(expectedHour, expectedMinute), changedTo);
    }

    [Theory]
    [InlineData(0, "ArrowUp", 11, 37)] // hours still move one hour, keeping the minutes
    [InlineData(0, "ArrowDown", 9, 37)]
    [InlineData(1, "ArrowUp", 10, 45)]
    [InlineData(1, "ArrowDown", 10, 30)]
    public void ArrowUpAndDown_WithMinuteStep_MoveToTheNextStep(int fieldIndex, string key, int expectedHour, int expectedMinute)
    {
        // Arrange
        var cut = Render(step: 15, time: new TimeOnly(10, 37));

        // Act
        cut.FindAll("input[type='text']")[fieldIndex].KeyDown(new KeyboardEventArgs { Key = key });

        // Assert
        Assert.Equal(new TimeOnly(expectedHour, expectedMinute), changedTo);
    }

    [Theory]
    [InlineData("ArrowUp", 12)]
    [InlineData("ArrowDown", 10)]
    public void ArrowUpAndDown_WithHourStep_MoveTheHourByTheStep(string key, int expectedHour)
    {
        // Arrange
        var cut = Render(step: 120, time: new TimeOnly(10, 30));

        // Act
        cut.FindAll("input[type='text']")[0].KeyDown(new KeyboardEventArgs { Key = key });

        // Assert
        Assert.Equal(new TimeOnly(expectedHour, 0), changedTo);
    }

    [Theory]
    [InlineData("ArrowUp")]
    [InlineData("ArrowDown")]
    public void ArrowUpAndDown_WithHourStep_LeaveTheLockedMinuteAlone(string key)
    {
        // Arrange
        var cut = Render(step: 120);

        // Act
        cut.FindAll("input[type='text']")[1].KeyDown(new KeyboardEventArgs { Key = key });

        // Assert
        Assert.Null(changedTo);
    }

    [Theory]
    [InlineData(0, "ArrowLeft")]
    [InlineData(0, "a")]
    [InlineData(1, "ArrowRight")]
    [InlineData(1, "5")]
    public void OtherKeys_LeaveTheTimeAlone(int fieldIndex, string key)
    {
        // Arrange
        var cut = Render();

        // Act
        cut.FindAll("input[type='text']")[fieldIndex].KeyDown(new KeyboardEventArgs { Key = key });

        // Assert
        Assert.Null(changedTo);
    }
}
