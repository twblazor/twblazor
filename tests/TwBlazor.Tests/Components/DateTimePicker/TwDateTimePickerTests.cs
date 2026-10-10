using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Builders;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.DateTimePicker;

public class TwDateTimePickerTests : TwBlazorTestBase
{
    private TwInputTheme inputTheme => Theme.Components.Require<TwInputTheme>();

    public TwDateTimePickerTests()
    {
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        TestContext.JSInterop.SetupVoid("jDatePicker.registerOutsideClick");
        TestContext.JSInterop.SetupVoid("jDatePicker.unregisterOutsideClick");
    }

    [Fact]
    public void FocusingDateInput_ShowsCalendarAndTimeControls()
    {
        // Arrange
        var start = new DateTime(2025, 11, 24, 11, 30, 0);

        // Act
        var cut = TestContext.Render<TwDateTimePicker>(p => p
            .Add(x => x.SelectedDateTime, start)
        );

        cut.Find("input").Click(); // Date input

        // Assert
        Assert.Contains("datepicker-grid", cut.Markup);
        // Verify time picker controls are rendered (4 buttons for hour/minute up/down)
        var buttons = cut.FindAll("[data-tw-popover] button[type='button']");
        Assert.True(buttons.Count >= 4, "Expected at least 4 control buttons for time picker");
    }

    [Fact]
    public void ClickingCalendarDay_InvokesSelectedDateTimeChanged_AndValueChanged()
    {
        // Arrange - a calendar day click flows through TwDatePicker's own SelectedDateChanged/
        // ValueChanged callbacks, which TwDateTimePicker must explicitly relay to its own consumers
        // (a plain @bind-SelectedDate/@bind-Value only updates TwDateTimePicker's own local field,
        // it doesn't also invoke TwDateTimePicker's SelectedDateTimeChanged/ValueChanged for callers
        // bound to this component).
        DateTime? callbackValue = null;
        string? valueChanged = null;
        var start = new DateTime(2025, 11, 24, 11, 30, 0);

        var cut = TestContext.Render<TwDateTimePicker>(p => p
            .Add(x => x.SelectedDateTime, start)
            .Add(x => x.SelectedDateTimeChanged, EventCallback.Factory.Create<DateTime>(this, d => callbackValue = d))
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<string>(this, v => valueChanged = v))
        );

        // Act
        cut.Find("input").Click();
        cut.FindAll("button.day").First(b => b.TextContent.Trim() == "5").Click();

        // Assert
        Assert.NotNull(callbackValue);
        Assert.Equal(5, callbackValue!.Value.Day);
        Assert.NotNull(valueChanged);
        Assert.Equal(callbackValue.Value.ToString("dd/MM/yyyy HH:mm"), valueChanged);
    }

    [Fact]
    public void IncrementHour_UpdatesSelectedDateTime_AndValue()
    {
        // Arrange
        DateTime? callbackValue = null;
        string? valueChanged = null;

        var start = new DateTime(2025, 11, 24, 23, 15, 0);

        // Act
        var cut = TestContext.Render<TwDateTimePicker>(p => p
            .Add(x => x.SelectedDateTime, start)
            .Add(x => x.SelectedDateTimeChanged, EventCallback.Factory.Create<DateTime>(this, d => callbackValue = d))
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<string>(this, s => valueChanged = s))
        );

        cut.Find("input").Click();

        var buttons = cut.FindAll("[data-tw-popover] button[type='button']");
        // Find the hour increment button - it's among the time picker buttons
        // After date picker buttons, we have: hour up (0), hour down (1), minute up (2), minute down (3)
        // But there are also date navigation buttons, so we need to find the time buttons
        // The simplest approach is to get all buttons and find the ones after the calendar
        var timeButtons = buttons.Skip(buttons.Count - 4).ToList();
        timeButtons[0].Click(); // Hour increment button

        // Assert
        Assert.Equal((start.Hour + 1) % 24, callbackValue!.Value.Hour);
        Assert.NotNull(valueChanged);
        Assert.Equal(callbackValue.Value.ToString("dd/MM/yyyy HH:mm"), valueChanged);
    }

    [Fact]
    public void IncrementMinute_RollsOverHour()
    {
        // Arrange
        DateTime? callbackValue = null;
        var start = new DateTime(2025, 11, 24, 10, 59, 0);

        // Act
        var cut = TestContext.Render<TwDateTimePicker>(p => p
            .Add(x => x.SelectedDateTime, start)
            .Add(x => x.SelectedDateTimeChanged, EventCallback.Factory.Create<DateTime>(this, d => callbackValue = d))
        );

        cut.Find("input").Click();
        var buttons = cut.FindAll("[data-tw-popover] button[type='button']");
        var timeButtons = buttons.Skip(buttons.Count - 4).ToList();
        timeButtons[2].Click(); // Minute increment button

        // Assert
        Assert.Equal(11, callbackValue!.Value.Hour);
        Assert.Equal(0, callbackValue.Value.Minute);
    }

    [Fact]
    public void DecrementMinute_RollsBackHour()
    {
        // Arrange
        DateTime? callbackValue = null;
        var start = new DateTime(2025, 11, 24, 10, 0, 0);

        // Act
        var cut = TestContext.Render<TwDateTimePicker>(p => p
            .Add(x => x.SelectedDateTime, start)
            .Add(x => x.SelectedDateTimeChanged, EventCallback.Factory.Create<DateTime>(this, d => callbackValue = d))
        );

        cut.Find("input").Click();
        var buttons = cut.FindAll("[data-tw-popover] button[type='button']");
        var timeButtons = buttons.Skip(buttons.Count - 4).ToList();
        timeButtons[3].Click(); // Minute decrement button

        // Assert
        Assert.Equal(9, callbackValue!.Value.Hour);
        Assert.Equal(59, callbackValue.Value.Minute);
    }

    [Fact]
    public void ManualHourEntry_12HourFormat_NormalizesTo24Hour()
    {
        // Arrange
        DateTime? callbackValue = null;
        string? valueChanged = null;

        var start = new DateTime(2025, 11, 24, 11, 30, 0); // AM

        // Act
        var cut = TestContext.Render<TwDateTimePicker>(p => p
            .Add(x => x.SelectedDateTime, start)
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedDateTimeChanged, EventCallback.Factory.Create<DateTime>(this, d => callbackValue = d))
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<string>(this, s => valueChanged = s))
        );

        cut.Find("input").Click();
        var hourInput = cut.FindAll("input")[1]; // Hour input (second input: date, hour, minute)
        hourInput.Change("12"); // 12 AM -> should become 00 internally


        // Assert
        Assert.NotNull(callbackValue);
        // Because initial time was 11 (AM), entering 12 should map to 00
        // UpdateTime logic converts 12 AM to 0
        Assert.Equal(0, callbackValue!.Value.Hour);
        Assert.Equal(callbackValue.Value.ToString("dd/MM/yyyy hh:mm tt"), valueChanged);
    }

    [Fact]
    public async Task ManualMinuteEntry_ClampsAboveSixty()
    {
        // Arrange
        DateTime? callbackValue = null;

        var start = new DateTime(2025, 11, 24, 10, 10, 0);

        // Act
        var cut = TestContext.Render<TwDateTimePicker>(p => p
            .Add(x => x.SelectedDateTime, start)
            .Add(x => x.SelectedDateTimeChanged, EventCallback.Factory.Create<DateTime>(this, d => callbackValue = d))
        );

        cut.Find("input").Click();
        // Minute input is the third input: date input, hour input, minute input
        var minuteInput = cut.FindAll("input")[2];
        await minuteInput.ChangeAsync(new ChangeEventArgs()
        {
            Value = "75"
        });

        // Assert
        Assert.NotNull(callbackValue);
        Assert.Equal(59, callbackValue!.Value.Minute);
    }

    [Fact]
    public void PreferNativePickerTrue_RendersNativeDateTimeLocalInput()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDateTimePicker>(p => p
            .Add(x => x.SelectedDateTime, new DateTime(2025, 11, 24, 14, 30, 0))
            .Add(x => x.PreferNativePicker, true)
        );

        var input = cut.Find("input");

        // Assert
        Assert.Equal("datetime-local", input.GetAttribute("type"));
        Assert.Equal("2025-11-24T14:30", input.GetAttribute("value"));
    }

    [Fact]
    public void PreferNativePickerTrue_FocusingInput_DoesNotShowCalendarOrTimeControls()
    {
        // Arrange
        var cut = TestContext.Render<TwDateTimePicker>(p => p
            .Add(x => x.SelectedDateTime, new DateTime(2025, 11, 24, 11, 30, 0))
            .Add(x => x.PreferNativePicker, true)
        );

        // Act
        cut.Find("input").Click();

        // Assert
        Assert.DoesNotContain("datepicker-grid", cut.Markup);
        Assert.Single(cut.FindAll("input")); // no time-picker inputs rendered alongside the native input
    }

    [Fact]
    public void PreferNativePickerFalse_StillShowsCalendarAndTimeControls_OnFocus()
    {
        // Arrange
        var cut = TestContext.Render<TwDateTimePicker>(p => p
            .Add(x => x.SelectedDateTime, new DateTime(2025, 11, 24, 11, 30, 0))
            .Add(x => x.PreferNativePicker, false)
        );

        // Act
        cut.Find("input").Click();

        // Assert
        Assert.Contains("datepicker-grid", cut.Markup);
        var buttons = cut.FindAll("[data-tw-popover] button[type='button']");
        Assert.True(buttons.Count >= 4, "Expected at least 4 control buttons for time picker");
    }

    [Fact]
    public void UsesGlobalDefaultVariant_WhenNotSet()
    {
        // Arrange - Variant used to be declared as a non-nullable InputVariant on TwDateTimePicker,
        // which meant it always defaulted to InputVariant.Default (enum value 0) and was forwarded
        // explicitly to the inner TwDatePicker/TwTextfield, permanently shadowing whatever
        // TwInputTheme.DefaultInputVariant was configured to. Now that Variant is inherited (nullable)
        // from TwBlazorInputComponentBase, leaving it unset must let the theme's global default through.
        inputTheme.DefaultInputVariant = InputVariant.Outlined;

        // Act
        var cut = TestContext.Render<TwDateTimePicker>(p => p
            .Add(x => x.SelectedDateTime, new DateTime(2025, 11, 24, 11, 30, 0))
        );

        // Assert
        var input = cut.Find("input");
        Assert.Contains(InputVariantBuilder.GetClasses(InputVariant.Outlined, inputTheme), input.GetAttribute("class"));
    }

    [Fact]
    public void ExplicitVariant_OverridesGlobalDefault()
    {
        // Arrange - the global default is Outlined, but this instance explicitly asks for Filled.
        inputTheme.DefaultInputVariant = InputVariant.Outlined;

        // Act
        var cut = TestContext.Render<TwDateTimePicker>(p => p
            .Add(x => x.SelectedDateTime, new DateTime(2025, 11, 24, 11, 30, 0))
            .Add(x => x.Variant, InputVariant.Filled)
        );

        // Assert
        var input = cut.Find("input");
        Assert.Contains(InputVariantBuilder.GetClasses(InputVariant.Filled, inputTheme), input.GetAttribute("class"));
    }

    [Fact]
    public void PanelNumberInputs_UseGlobalDefaultVariant_WhenNotSet()
    {
        // Arrange - the embedded TwTimePickerBody's hour/minute inputs used to be styled with a
        // hardcoded transparent-background look regardless of TwInputTheme.DefaultInputVariant, so
        // they never looked "Filled" even when that was the configured global default.
        inputTheme.DefaultInputVariant = InputVariant.Filled;

        var cut = TestContext.Render<TwDateTimePicker>(p => p
            .Add(x => x.SelectedDateTime, new DateTime(2025, 11, 24, 11, 30, 0))
        );

        // Act
        cut.Find("input").Click();

        // Assert - date input, hour input, minute input
        var hourInput = cut.FindAll("input")[1];
        var minuteInput = cut.FindAll("input")[2];
        var expectedClasses = InputVariantBuilder.GetClasses(InputVariant.Filled, inputTheme);
        Assert.Contains(expectedClasses, hourInput.GetAttribute("class"));
        Assert.Contains(expectedClasses, minuteInput.GetAttribute("class"));
    }

    [Fact]
    public void UnsetFormat_UsesThemeDefault24HourFormat()
    {
        // Arrange
        var datePickerTheme = Theme.Components.Require<TwDatePickerTheme>();
        datePickerTheme.DefaultDateTimeFormat = "MM/dd/yyyy HH:mm";

        // Act
        var cut = TestContext.Render<TwDateTimePicker>(p => p
            .Add(x => x.SelectedDateTime, new DateTime(2025, 11, 24, 14, 30, 0))
        );

        // Assert
        Assert.Equal("11/24/2025 14:30", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void UnsetFormat_Is12HourFormat_UsesThemeDefault12HourFormat()
    {
        // Arrange
        var datePickerTheme = Theme.Components.Require<TwDatePickerTheme>();
        datePickerTheme.DefaultDateTimeFormat12Hour = "MM/dd/yyyy hh:mm tt";

        // Act
        var cut = TestContext.Render<TwDateTimePicker>(p => p
            .Add(x => x.SelectedDateTime, new DateTime(2025, 11, 24, 14, 30, 0))
            .Add(x => x.Is12HourFormat, true)
        );

        // Assert
        Assert.Equal("11/24/2025 02:30 PM", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void ExplicitFormat_OverridesThemeDefault_AndIs12HourFormat()
    {
        // Arrange - an explicit Format wins regardless of Is12HourFormat.
        var cut = TestContext.Render<TwDateTimePicker>(p => p
            .Add(x => x.SelectedDateTime, new DateTime(2025, 11, 24, 14, 30, 0))
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.Format, "yyyy-MM-dd HH:mm")
        );

        // Assert
        Assert.Equal("2025-11-24 14:30", cut.Find("input").GetAttribute("value"));
    }
}