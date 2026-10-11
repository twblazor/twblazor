using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Builders;
using TwBlazor.Components.TimePicker;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.TimePicker;

public class TwTimePickerBodyTests : TwBlazorTestBase
{
    private TwInputTheme inputTheme => Theme.Components.Require<TwInputTheme>();

    [Fact]
    public void NumberInputs_UseGlobalDefaultVariant_WhenNotSet()
    {
        // Arrange - the hour/minute number inputs used to be styled with a hardcoded
        // transparent-background/bottom-border look, completely independent of
        // TwInputTheme.DefaultInputVariant, so they never looked "Filled" even when that was
        // the configured global default. They must now follow it like every other text input.
        inputTheme.DefaultInputVariant = InputVariant.Filled;

        // Act
        var cut = TestContext.Render<TwTimePickerBody>();

        // Assert
        var hourInput = cut.FindAll("input[type='text']")[0];
        var minuteInput = cut.FindAll("input[type='text']")[1];
        var expectedClasses = InputVariantBuilder.GetClasses(InputVariant.Filled, inputTheme);
        Assert.Contains(expectedClasses, hourInput.GetAttribute("class"));
        Assert.Contains(expectedClasses, minuteInput.GetAttribute("class"));

        // A conflicting "bg-transparent" baked into TwTimePickerTheme.NumberInput regressed once
        // before without any test catching it, because asserting the Filled classes were merely
        // *present* doesn't notice an extra class silently fighting them for the background - assert
        // it's actually absent too.
        Assert.DoesNotContain("bg-transparent", hourInput.GetAttribute("class"));
        Assert.DoesNotContain("bg-transparent", minuteInput.GetAttribute("class"));
    }

    [Fact]
    public void NumberInputs_UseGlobalDefaultVariant_WhenNotSet_Outlined()
    {
        // Arrange - same as above, but for a non-Filled global default, proving the inputs track
        // whatever TwInputTheme.DefaultInputVariant is configured to rather than one hardcoded look.
        inputTheme.DefaultInputVariant = InputVariant.Outlined;

        // Act
        var cut = TestContext.Render<TwTimePickerBody>();

        // Assert
        var hourInput = cut.FindAll("input[type='text']")[0];
        var expectedClasses = InputVariantBuilder.GetClasses(InputVariant.Outlined, inputTheme);
        Assert.Contains(expectedClasses, hourInput.GetAttribute("class"));
    }

    [Fact]
    public void NumberInputs_ExplicitVariant_OverridesGlobalDefault()
    {
        // Arrange - the global default is Outlined, but this instance explicitly asks for Filled.
        inputTheme.DefaultInputVariant = InputVariant.Outlined;

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Variant, InputVariant.Filled)
        );

        // Assert
        var hourInput = cut.FindAll("input[type='text']")[0];
        var expectedClasses = InputVariantBuilder.GetClasses(InputVariant.Filled, inputTheme);
        Assert.Contains(expectedClasses, hourInput.GetAttribute("class"));
    }

    [Fact]
    public void TwTimePickerBody_RendersWithDefaultTime()
    {
        // Act
        var cut = TestContext.Render<TwTimePickerBody>();

        // Assert - auto-generated id prefix (TwBlazorComponentBase strips the "Tw" prefix and
        // lower-cases the type name), confirming the component's root element actually rendered.
        Assert.Contains("timepickerbody-", cut.Markup);

        // Verify we have hour and minute inputs
        var inputs = cut.FindAll("input[type='text']");
        Assert.Equal(2, inputs.Count); // Hour and minute inputs

        // Verify we have increment/decrement buttons (4 TwIcon buttons for 24h format)
        var buttons = cut.FindAll("button[type='button']");
        Assert.Equal(4, buttons.Count); // 4 buttons: hour up/down, minute up/down
    }

    [Fact]
    public void TwTimePickerBody_DisplaysTime_In24HourFormat()
    {
        // Arrange
        var selectedTime = new TimeOnly(14, 30);

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, selectedTime)
            .Add(x => x.Is12HourFormat, false)
        );

        // Assert
        var hourInput = cut.FindAll("input[type='text']")[0];
        var minuteInput = cut.FindAll("input[type='text']")[1];

        Assert.Equal("14", hourInput.GetAttribute("value"));
        Assert.Equal("30", minuteInput.GetAttribute("value"));
    }

    [Fact]
    public void TwTimePickerBody_DisplaysTime_In12HourFormat()
    {
        // Arrange
        var selectedTime = new TimeOnly(14, 30); // 2:30 pm

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, selectedTime)
            .Add(x => x.Is12HourFormat, true)
        );

        // Assert
        var hourInput = cut.FindAll("input[type='text']")[0];
        Assert.Equal("02", hourInput.GetAttribute("value")); // Shows as 02 in 12-hour format
    }

    [Fact]
    public void TwTimePickerBody_IncrementHour_UpdatesTime()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(10, 30);

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var buttons = cut.FindAll("button[type='button']");
        buttons[0].Click(); // Hour increment button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(11, callbackTime.Value.Hour);
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_DecrementHour_UpdatesTime()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(10, 30);

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var buttons = cut.FindAll("button[type='button']");
        buttons[1].Click(); // Hour decrement button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(9, callbackTime.Value.Hour);
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_IncrementMinute_UpdatesTime()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(10, 30);

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var buttons = cut.FindAll("button[type='button']");
        buttons[2].Click(); // Minute increment button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(10, callbackTime.Value.Hour);
        Assert.Equal(31, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_DecrementMinute_UpdatesTime()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(10, 30);

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var buttons = cut.FindAll("button[type='button']");
        buttons[3].Click(); // Minute decrement button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(10, callbackTime.Value.Hour);
        Assert.Equal(29, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_IncrementHour_RollsOverAt23()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(23, 30);

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var buttons = cut.FindAll("button[type='button']");
        buttons[0].Click(); // Hour increment button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(0, callbackTime.Value.Hour); // Rolls over to 00:30
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_DecrementHour_RollsBackAt0()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(0, 30);

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var buttons = cut.FindAll("button[type='button']");
        buttons[1].Click(); // Hour decrement button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(23, callbackTime.Value.Hour); // Rolls back to 23:30
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_IncrementMinute_RollsOverHourAt59()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(10, 59);

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var buttons = cut.FindAll("button[type='button']");
        buttons[2].Click(); // Minute increment button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(11, callbackTime.Value.Hour); // Hour increments
        Assert.Equal(0, callbackTime.Value.Minute); // Minute rolls to 00
    }

    [Fact]
    public void TwTimePickerBody_DecrementMinute_RollsBackHourAt0()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(10, 0);

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var buttons = cut.FindAll("button[type='button']");
        buttons[3].Click(); // Minute decrement button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(9, callbackTime.Value.Hour); // Hour decrements
        Assert.Equal(59, callbackTime.Value.Minute); // Minute rolls to 59
    }

    [Fact]
    public void TwTimePickerBody_ManualHourInput_UpdatesTime()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(10, 30);

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var hourInput = cut.FindAll("input[type='text']")[0];
        hourInput.Change("15");

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(15, callbackTime.Value.Hour);
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_ManualMinuteInput_UpdatesTime()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(10, 30);

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var minuteInput = cut.FindAll("input[type='text']")[1];
        minuteInput.Change("45");

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(10, callbackTime.Value.Hour);
        Assert.Equal(45, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_ClampsHourTo23_In24HourFormat()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(10, 30);

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.Is12HourFormat, false)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var hourInput = cut.FindAll("input[type='text']")[0];
        hourInput.Change("25"); // Invalid hour

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(23, callbackTime.Value.Hour); // Clamped to max
    }

    [Fact]
    public void TwTimePickerBody_ClampsMinuteTo59()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(10, 30);

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var minuteInput = cut.FindAll("input[type='text']")[1];
        minuteInput.Change("65"); // Invalid minute

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(59, callbackTime.Value.Minute); // Clamped to max
    }

    [Fact]
    public void TwTimePickerBody_RendersColonSeparator()
    {
        // Act
        var cut = TestContext.Render<TwTimePickerBody>();

        // Assert
        Assert.Contains(":", cut.Markup);
    }

    [Fact]
    public void TwTimePickerBody_AppliesCustomClass()
    {
        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Class, "custom-time-class")
        );

        // Assert
        Assert.Contains("custom-time-class", cut.Markup);
    }

    [Fact]
    public void TwTimePickerBody_UsesEventCallbacks_WhenProvided()
    {
        // Arrange
        var hourIncreased = false;
        var initialTime = new TimeOnly(10, 30);

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.OnHourIncreased, EventCallback.Factory.Create(this, () => hourIncreased = true))
        );

        var buttons = cut.FindAll("button[type='button']");
        buttons[0].Click(); // Hour increment button

        // Assert
        Assert.True(hourIncreased);
    }

    [Fact]
    public void TwTimePickerBody_Preserves_AM_PM_In12HourFormat()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(14, 30); // 2:30 pm

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var hourInput = cut.FindAll("input[type='text']")[0];
        hourInput.Change("3"); // Change to 3 (should stay PM)

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(15, callbackTime.Value.Hour); // 3 PM = 15 in 24h
    }

    #region AM/PM Toggle Button Tests

    [Fact]
    public void TwTimePickerBody_ShowsAmPmButton_When12HourFormat()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, new TimeOnly(10, 30))
        );

        // Assert
        var buttons = cut.FindAll("button");
        var amPmButton = buttons.FirstOrDefault(b => b.TextContent.Contains("AM") || b.TextContent.Contains("PM"));
        Assert.NotNull(amPmButton);
    }

    [Fact]
    public void TwTimePickerBody_HidesAmPmButton_When24HourFormat()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, false)
            .Add(x => x.SelectedTime, new TimeOnly(10, 30))
        );

        // Assert
        var buttons = cut.FindAll("button");
        var amPmButton = buttons.FirstOrDefault(b => b.TextContent.Contains("AM") || b.TextContent.Contains("PM"));
        Assert.Null(amPmButton);
    }

    [Fact]
    public void TwTimePickerBody_AmPmButton_DisplaysAM_ForMorningHours()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, new TimeOnly(10, 30)) // 10:30 am
        );

        // Assert
        var buttons = cut.FindAll("button");
        var amPmButton = buttons.First(b => b.TextContent.Contains("AM") || b.TextContent.Contains("PM"));
        Assert.Contains("AM", amPmButton.TextContent);
    }

    [Fact]
    public void TwTimePickerBody_AmPmButton_DisplaysPM_ForAfternoonHours()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, new TimeOnly(14, 30)) // 2:30 pm
        );

        // Assert
        var buttons = cut.FindAll("button");
        var amPmButton = buttons.First(b => b.TextContent.Contains("AM") || b.TextContent.Contains("PM"));
        Assert.Contains("PM", amPmButton.TextContent);
    }

    [Fact]
    public void TwTimePickerBody_ToggleAmPm_AM_To_PM()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(10, 30); // 10:30 am

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button");
        var amPmButton = buttons.First(b => b.TextContent.Contains("AM") || b.TextContent.Contains("PM"));
        amPmButton.Click();

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(22, callbackTime.Value.Hour); // 10:30 am + 12 hours = 22:30 (10:30 pm)
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_ToggleAmPm_PM_To_AM()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(14, 30); // 2:30 pm

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button");
        var amPmButton = buttons.First(b => b.TextContent.Contains("AM") || b.TextContent.Contains("PM"));
        amPmButton.Click();

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(2, callbackTime.Value.Hour); // 2:30 pm - 12 hours = 02:30 (2:30 am)
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_ToggleAmPm_PreservesMinutes()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(10, 45); // 10:45 am

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button");
        var amPmButton = buttons.First(b => b.TextContent.Contains("AM") || b.TextContent.Contains("PM"));
        amPmButton.Click();

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(45, callbackTime.Value.Minute); // Minutes preserved
    }

    #endregion

    #region 12-Hour Format Cycling Tests

    [Fact]
    public void TwTimePickerBody_12Hour_IncrementHour_11AM_To_12PM()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(11, 30); // 11:30 am

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[0].Click(); // Hour increment button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(12, callbackTime.Value.Hour); // 12:30 pm
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_12Hour_IncrementHour_12PM_To_1PM()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(12, 30); // 12:30 pm

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[0].Click(); // Hour increment button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(13, callbackTime.Value.Hour); // 1:30 pm (13:30 in 24h)
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_12Hour_IncrementHour_11PM_To_12AM()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(23, 30); // 11:30 pm

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[0].Click(); // Hour increment button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(0, callbackTime.Value.Hour); // 12:30 am (00:30 in 24h)
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_12Hour_IncrementHour_12AM_To_1AM()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(0, 30); // 12:30 am

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[0].Click(); // Hour increment button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(1, callbackTime.Value.Hour); // 1:30 am
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_12Hour_DecrementHour_12PM_To_11AM()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(12, 30); // 12:30 pm

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[1].Click(); // Hour decrement button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(11, callbackTime.Value.Hour); // 11:30 am
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_12Hour_DecrementHour_1PM_To_12PM()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(13, 30); // 1:30 pm

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[1].Click(); // Hour decrement button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(12, callbackTime.Value.Hour); // 12:30 pm
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_12Hour_DecrementHour_12AM_To_11PM()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(0, 30); // 12:30 am

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[1].Click(); // Hour decrement button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(23, callbackTime.Value.Hour); // 11:30 pm (23:30 in 24h)
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_12Hour_DecrementHour_1AM_To_12AM()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(1, 30); // 1:30 am

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[1].Click(); // Hour decrement button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(0, callbackTime.Value.Hour); // 12:30 am (00:30 in 24h)
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_12Hour_IncrementMinute_CyclesHourCorrectly()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(11, 59); // 11:59 am

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[2].Click(); // Minute increment button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(12, callbackTime.Value.Hour); // Rolls to 12:00 pm
        Assert.Equal(0, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_12Hour_DecrementMinute_CyclesHourCorrectly()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(12, 0); // 12:00 pm

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[3].Click(); // Minute decrement button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(11, callbackTime.Value.Hour); // Rolls back to 11:59 am
        Assert.Equal(59, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_12Hour_FullCycle_Through24Hours()
    {
        // This test ensures we can cycle through all 24 hours correctly
        // Arrange
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, new TimeOnly(0, 0)) // Start at 12:00 am
        );

        var buttons = cut.FindAll("button[type='button']");
        var increaseHourButton = buttons[0]; // Hour increment button

        // Act & Assert - Click through all 24 hours
        for (var expectedHour = 1; expectedHour < 24; expectedHour++)
        {
            increaseHourButton.Click();
            cut.Render(); // Force re-render

            var hourInput = cut.FindAll("input[type='text']")[0];
            var displayedHour = int.Parse(hourInput.GetAttribute("value") ?? "0");

            // Verify the displayed hour is correct for 12-hour format
            var expected12Hour = expectedHour % 12;
            if (expected12Hour == 0) expected12Hour = 12;
            Assert.Equal(expected12Hour, displayedHour);
        }

        // One more click should return to 12:00 am
        increaseHourButton.Click();
        cut.Render();
        var finalHourInput = cut.FindAll("input[type='text']")[0];
        Assert.Equal("12", finalHourInput.GetAttribute("value"));
    }

    #endregion

    #region 24-Hour Format Cycling Tests

    [Fact]
    public void TwTimePickerBody_24Hour_IncrementHour_22_To_23()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(22, 30); // 22:30

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, false)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[0].Click(); // Hour increment button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(23, callbackTime.Value.Hour); // 23:30
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_24Hour_IncrementHour_23_To_0()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(23, 30); // 23:30

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, false)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[0].Click(); // Hour increment button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(0, callbackTime.Value.Hour); // Rolls over to 00:30
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_24Hour_IncrementHour_0_To_1()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(0, 30); // 00:30

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, false)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[0].Click(); // Hour increment button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(1, callbackTime.Value.Hour); // 01:30
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_24Hour_DecrementHour_1_To_0()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(1, 30); // 01:30

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, false)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[1].Click(); // Hour decrement button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(0, callbackTime.Value.Hour); // 00:30
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_24Hour_DecrementHour_0_To_23()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(0, 30); // 00:30

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, false)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[1].Click(); // Hour decrement button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(23, callbackTime.Value.Hour); // Rolls back to 23:30
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_24Hour_DecrementHour_23_To_22()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(23, 30); // 23:30

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, false)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[1].Click(); // Hour decrement button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(22, callbackTime.Value.Hour); // 22:30
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_24Hour_IncrementMinute_At_23_59()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(23, 59); // 23:59

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, false)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[2].Click(); // Minute increment button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(0, callbackTime.Value.Hour); // Rolls to 00:00
        Assert.Equal(0, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_24Hour_DecrementMinute_At_0_0()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(0, 0); // 00:00

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, false)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[3].Click(); // Minute decrement button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(23, callbackTime.Value.Hour); // Rolls back to 23:59
        Assert.Equal(59, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_24Hour_IncrementMinute_CyclesHourCorrectly()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(14, 59); // 14:59

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, false)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[2].Click(); // Minute increment button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(15, callbackTime.Value.Hour); // Rolls to 15:00
        Assert.Equal(0, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_24Hour_DecrementMinute_CyclesHourCorrectly()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(15, 0); // 15:00

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, false)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        // Act
        var buttons = cut.FindAll("button[type='button']");
        buttons[3].Click(); // Minute decrement button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(14, callbackTime.Value.Hour); // Rolls back to 14:59
        Assert.Equal(59, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_24Hour_FullCycle_Through24Hours()
    {
        // This test ensures we can cycle through all 24 hours correctly
        // Arrange
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, false)
            .Add(x => x.SelectedTime, new TimeOnly(0, 0)) // Start at 00:00
        );

        var buttons = cut.FindAll("button[type='button']");
        var increaseHourButton = buttons[0]; // Hour increment button

        // Act & Assert - Click through all 24 hours
        for (var expectedHour = 1; expectedHour < 24; expectedHour++)
        {
            increaseHourButton.Click();
            cut.Render(); // Force re-render

            var hourInput = cut.FindAll("input[type='text']")[0];
            var displayedHour = int.Parse(hourInput.GetAttribute("value") ?? "0");

            // Verify the displayed hour is correct for 24-hour format
            Assert.Equal(expectedHour, displayedHour);
        }

        // One more click should return to 00:00
        increaseHourButton.Click();
        cut.Render();
        var finalHourInput = cut.FindAll("input[type='text']")[0];
        Assert.Equal("00", finalHourInput.GetAttribute("value"));
    }

    [Fact]
    public void TwTimePickerBody_24Hour_FullCycle_Backward_Through24Hours()
    {
        // This test ensures we can cycle backwards through all 24 hours correctly
        // Arrange
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, false)
            .Add(x => x.SelectedTime, new TimeOnly(0, 0)) // Start at 00:00
        );

        var buttons = cut.FindAll("button[type='button']");
        var decreaseHourButton = buttons[1]; // Hour decrement button

        // Act & Assert - Click backwards through all 24 hours
        for (var expectedHour = 23; expectedHour > 0; expectedHour--)
        {
            decreaseHourButton.Click();
            cut.Render(); // Force re-render

            var hourInput = cut.FindAll("input[type='text']")[0];
            var displayedHour = int.Parse(hourInput.GetAttribute("value") ?? "0");

            // Verify the displayed hour is correct for 24-hour format
            Assert.Equal(expectedHour, displayedHour);
        }

        // One more click should return to 00:00
        decreaseHourButton.Click();
        cut.Render();
        var finalHourInput = cut.FindAll("input[type='text']")[0];
        Assert.Equal("00", finalHourInput.GetAttribute("value"));
    }

    [Fact]
    public void TwTimePickerBody_24Hour_MultipleIncrements_MaintainsCorrectState()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(20, 30); // 20:30

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, false)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var buttons = cut.FindAll("button[type='button']");

        // Act - Increment 5 hours (should wrap around)
        for (var i = 0; i < 5; i++)
        {
            buttons[0].Click(); // Hour increment button
        }

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(1, callbackTime.Value.Hour); // 20 + 5 = 25, which wraps to 01:30
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_24Hour_MultipleDecrements_MaintainsCorrectState()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(3, 30); // 03:30

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, false)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var buttons = cut.FindAll("button[type='button']");

        // Act - Decrement 5 hours (should wrap around)
        for (var i = 0; i < 5; i++)
        {
            buttons[1].Click(); // Hour decrement button
        }

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(22, callbackTime.Value.Hour); // 3 - 5 = -2, which wraps to 22:30
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    #endregion

    #region Normalize12HourTo24Hour Edge Case Tests

    [Fact]
    public void TwTimePickerBody_12Hour_ManualInput_12_WhileAM_Sets_Midnight()
    {
        // Arrange - hour == 12 and currentIsPm == false → should return 0 (12 AM = midnight)
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(10, 30); // 10:30 am

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var hourInput = cut.FindAll("input[type='text']")[0];
        hourInput.Change("12"); // Typing 12 while in AM period

        // Assert: 12 AM = 00:30 in 24h
        Assert.NotNull(callbackTime);
        Assert.Equal(0, callbackTime.Value.Hour);
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_12Hour_ManualInput_12_WhilePM_Sets_Noon()
    {
        // Arrange - hour == 12 and currentIsPm == true → should return 12 (12 PM = noon)
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(14, 30); // 2:30 pm

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var hourInput = cut.FindAll("input[type='text']")[0];
        hourInput.Change("12"); // Typing 12 while in PM period

        // Assert: 12 PM = 12:30 in 24h
        Assert.NotNull(callbackTime);
        Assert.Equal(12, callbackTime.Value.Hour);
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    #endregion

    #region Custom Event Callback Coverage

    [Fact]
    public void TwTimePickerBody_UsesOnHourDecreased_WhenProvided()
    {
        // Arrange
        var hourDecreased = false;

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, new TimeOnly(10, 30))
            .Add(x => x.OnHourDecreased, EventCallback.Factory.Create(this, () => hourDecreased = true))
        );

        var buttons = cut.FindAll("button[type='button']");
        buttons[1].Click(); // Hour decrement button

        // Assert
        Assert.True(hourDecreased);
    }

    [Fact]
    public void TwTimePickerBody_UsesOnMinuteIncreased_WhenProvided()
    {
        // Arrange
        var minuteIncreased = false;

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, new TimeOnly(10, 30))
            .Add(x => x.OnMinuteIncreased, EventCallback.Factory.Create(this, () => minuteIncreased = true))
        );

        var buttons = cut.FindAll("button[type='button']");
        buttons[2].Click(); // Minute increment button

        // Assert
        Assert.True(minuteIncreased);
    }

    [Fact]
    public void TwTimePickerBody_UsesOnMinuteDecreased_WhenProvided()
    {
        // Arrange
        var minuteDecreased = false;

        // Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, new TimeOnly(10, 30))
            .Add(x => x.OnMinuteDecreased, EventCallback.Factory.Create(this, () => minuteDecreased = true))
        );

        var buttons = cut.FindAll("button[type='button']");
        buttons[3].Click(); // Minute decrement button

        // Assert
        Assert.True(minuteDecreased);
    }

    [Fact]
    public void TwTimePickerBody_UsesOnHourValueChanged_WhenProvided()
    {
        // Arrange - when OnHourValueChanged has a delegate, SetHourValue defers entirely to
        // the caller instead of parsing/clamping and updating SelectedTime itself.
        string? rawHourValue = null;
        TimeOnly? callbackTime = null;

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, new TimeOnly(10, 30))
            .Add(x => x.OnHourValueChanged, EventCallback.Factory.Create<string>(this, v => rawHourValue = v))
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var hourInput = cut.FindAll("input[type='text']")[0];

        // Act
        hourInput.Change("99");

        // Assert - raw value forwarded to the caller, SelectedTime untouched
        Assert.Equal("99", rawHourValue);
        Assert.Null(callbackTime);
    }

    [Fact]
    public void TwTimePickerBody_UsesOnMinuteValueChanged_WhenProvided()
    {
        // Arrange - same as OnHourValueChanged, but for the minute input.
        string? rawMinuteValue = null;
        TimeOnly? callbackTime = null;

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, new TimeOnly(10, 30))
            .Add(x => x.OnMinuteValueChanged, EventCallback.Factory.Create<string>(this, v => rawMinuteValue = v))
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var minuteInput = cut.FindAll("input[type='text']")[1];

        // Act
        minuteInput.Change("99");

        // Assert
        Assert.Equal("99", rawMinuteValue);
        Assert.Null(callbackTime);
    }

    [Fact]
    public void TwTimePickerBody_ManualHourInput_DoesNotUpdateTime_WhenEventValueIsNull()
    {
        // Arrange - exercises the `e.Value?.ToString() ?? string.Empty` null branch on the
        // hour input's onchange handler.
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(10, 30);

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var hourInput = cut.FindAll("input[type='text']")[0];

        // Act
        hourInput.Change(new ChangeEventArgs { Value = null });

        // Assert - empty string fails int.TryParse, so hour resets to 0 but minute is preserved
        Assert.NotNull(callbackTime);
        Assert.Equal(0, callbackTime.Value.Hour);
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_ManualMinuteInput_DoesNotUpdateTime_WhenEventValueIsNull()
    {
        // Arrange - same as above, for the minute input.
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(10, 30);

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var minuteInput = cut.FindAll("input[type='text']")[1];

        // Act
        minuteInput.Change(new ChangeEventArgs { Value = null });

        // Assert - empty string fails int.TryParse, so minute resets to 0 but hour is preserved
        Assert.NotNull(callbackTime);
        Assert.Equal(10, callbackTime.Value.Hour);
        Assert.Equal(0, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePickerBody_12Hour_ManualInput_NonTwelve_WhileAM_StaysSameHour()
    {
        // Arrange - hour != 12 and currentIsPm == false → Normalize12HourTo24Hour's
        // fallthrough `return currentIsPm ? hour + 12 : hour;` with currentIsPm false.
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(10, 30); // 10:30 am

        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var hourInput = cut.FindAll("input[type='text']")[0];

        // Act
        hourInput.Change("5"); // Typing 5 while in AM period

        // Assert: stays 5 AM = 05:30 in 24h (not shifted by +12)
        Assert.NotNull(callbackTime);
        Assert.Equal(5, callbackTime.Value.Hour);
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    #endregion
}
