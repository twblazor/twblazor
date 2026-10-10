using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Builders;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.DateTimeRangePicker;

public class TwDateTimeRangePickerTests : TwBlazorTestBase
{
    private TwInputTheme inputTheme => Theme.Components.Require<TwInputTheme>();

    public TwDateTimeRangePickerTests()
    {
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        TestContext.JSInterop.SetupVoid("twPicker.registerOutsideClick");
        TestContext.JSInterop.SetupVoid("twPicker.unregisterOutsideClick");
    }

    private static EventCallback<KeyValuePair<DateTime?, DateTime?>> NoOpRangeCallback(TwDateTimeRangePickerTests owner) =>
        EventCallback.Factory.Create<KeyValuePair<DateTime?, DateTime?>>(owner, _ => { });

    private static bool IsStageActive(IRenderedComponent<TwDateTimeRangePicker> cut, string label) =>
        cut.FindAll("[role='group'][aria-label='Range step'] button").First(b => b.TextContent.Trim() == label).GetAttribute("aria-pressed") == "true";

    [Fact]
    public void FocusingInput_PositionsPanelAsFixed_AnchoredToTheInputRoot()
    {
        // Regression test - see the identical test on TwDatePickerTests for the full rationale.
        var cut = TestContext.Render<TwDateTimeRangePicker>();

        cut.Find("input").Click();

        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.registerScrollReposition");
        Assert.IsType<ElementReference>(invocation.Arguments[0]);
        Assert.IsType<ElementReference>(invocation.Arguments[1]);
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.positionPanel");
    }

    [Fact]
    public void RendersOneMergedInput()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDateTimeRangePicker>();

        // Assert
        Assert.Single(cut.FindAll("input"));
    }

    [Fact]
    public void FocusingInput_StartsOnStepOneOfTwo()
    {
        // Arrange
        var cut = TestContext.Render<TwDateTimeRangePicker>();

        // Act
        cut.Find("input").Click();

        // Assert
        Assert.True(IsStageActive(cut, "Start"));
        Assert.Contains("datepicker-grid", cut.Markup);
    }

    [Fact]
    public void PanelNumberInputs_UseGlobalDefaultVariant_WhenNotSet()
    {
        // Arrange - the embedded TwTimePickerBody's hour/minute inputs used to be styled with a
        // hardcoded transparent-background look regardless of TwInputTheme.DefaultInputVariant, so
        // they never looked "Filled" even when that was the configured global default.
        inputTheme.DefaultInputVariant = InputVariant.Filled;

        var cut = TestContext.Render<TwDateTimeRangePicker>();

        // Act
        cut.Find("input").Click();

        // Assert - merged trigger, then hour/minute (day-view calendar has no <input> elements)
        var numberInputs = cut.FindAll("input").Skip(1).ToList();
        Assert.Equal(2, numberInputs.Count);
        var expectedClasses = InputVariantBuilder.GetClasses(InputVariant.Filled, inputTheme);
        foreach (var input in numberInputs)
        {
            Assert.Contains(expectedClasses, input.GetAttribute("class"));
        }
    }

    [Fact]
    public void PickingStartDay_AdvancesToStepTwo_WithoutClosingPanel()
    {
        // Arrange
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<DateTime?, DateTime?>(new DateTime(2025, 11, 1), null))
            .Add(x => x.SelectedRangeChanged, NoOpRangeCallback(this))
        );

        // Act
        cut.Find("input").Click();
        cut.FindAll("button.day").First(b => b.TextContent.Trim() == "5").Click();

        // Assert - the panel stays open, now on the end step.
        Assert.Contains("datepicker-grid", cut.Markup);
        Assert.True(IsStageActive(cut, "End"));
    }

    [Fact]
    public void PickingStartThenEndDay_CompletesRange_AndClosesPanel()
    {
        // Arrange
        KeyValuePair<DateTime?, DateTime?>? rangeFromCallback = null;
        string? valueFromCallback = null;

        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<DateTime?, DateTime?>(new DateTime(2025, 11, 1), null))
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<DateTime?, DateTime?>>(this, r => rangeFromCallback = r))
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<string>(this, v => valueFromCallback = v))
        );

        // Act
        cut.Find("input").Click();
        cut.FindAll("button.day").First(b => b.TextContent.Trim() == "5").Click();
        cut.FindAll("button.day").First(b => b.TextContent.Trim() == "15").Click();

        // Assert
        Assert.NotNull(rangeFromCallback);
        Assert.Equal(5, rangeFromCallback!.Value.Key!.Value.Day);
        Assert.Equal(15, rangeFromCallback.Value.Value!.Value.Day);
        Assert.NotNull(valueFromCallback);
        Assert.Contains(" - ", valueFromCallback);
        Assert.DoesNotContain("datepicker-grid", cut.Markup);
    }

    [Fact]
    public void EndStep_DisablesDaysBeforeTheStart()
    {
        // Arrange - the start is the 10th; while picking the end, days before it must render disabled.
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<DateTime?, DateTime?>(new DateTime(2025, 11, 1), null))
            .Add(x => x.SelectedRangeChanged, NoOpRangeCallback(this))
        );

        cut.Find("input").Click();
        cut.FindAll("button.day").First(b => b.TextContent.Trim() == "10").Click();

        // Act
        var day5 = cut.FindAll("button.day").First(b => b.TextContent.Trim() == "5");

        // Assert
        Assert.Equal("true", day5.GetAttribute("aria-disabled"));
    }

    [Fact]
    public void EndStep_ClickingDayBeforeStart_DoesNotCompleteRange()
    {
        // Arrange
        KeyValuePair<DateTime?, DateTime?>? rangeFromCallback = null;
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<DateTime?, DateTime?>(new DateTime(2025, 11, 1), null))
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<DateTime?, DateTime?>>(this, r => rangeFromCallback = r))
        );

        cut.Find("input").Click();
        cut.FindAll("button.day").First(b => b.TextContent.Trim() == "10").Click();
        rangeFromCallback = null; // reset - only care about what happens next

        // Act - clicking a disabled day before the start should no-op.
        cut.FindAll("button.day").First(b => b.TextContent.Trim() == "5").Click();

        // Assert
        Assert.Null(rangeFromCallback);
        Assert.True(IsStageActive(cut, "End"));
    }

    [Fact]
    public void AdjustingTimeThenPickingDay_CombinesBoth()
    {
        // Arrange - the existing start (2025-11-01) has no time component, so the panel seeds
        // pendingTime from its midnight time-of-day, making the expected result deterministic.
        KeyValuePair<DateTime?, DateTime?>? rangeFromCallback = null;
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<DateTime?, DateTime?>(new DateTime(2025, 11, 1), null))
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<DateTime?, DateTime?>>(this, r => rangeFromCallback = r))
        );

        // Act
        cut.Find("input").Click();
        var timeButtons = cut.FindAll("button[type='button']").TakeLast(4).ToList();
        timeButtons[0].Click(); // hour increment: 00:00 -> 01:00
        cut.FindAll("button.day").First(b => b.TextContent.Trim() == "5").Click();

        // Assert
        Assert.NotNull(rangeFromCallback);
        Assert.Equal(5, rangeFromCallback!.Value.Key!.Value.Day);
        Assert.Equal(1, rangeFromCallback.Value.Key!.Value.Hour);
    }

    [Fact]
    public void AdjustingStartTime_AfterStartAlreadyPicked_UpdatesImmediately()
    {
        // Arrange - reopening with an existing start already set.
        KeyValuePair<DateTime?, DateTime?>? rangeFromCallback = null;
        var start = new DateTime(2025, 11, 5, 9, 0, 0);

        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<DateTime?, DateTime?>(start, null))
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<DateTime?, DateTime?>>(this, r => rangeFromCallback = r))
        );

        // Act - focusing re-seeds the start step; adjusting the hour should apply immediately since
        // the start date is already set, without needing to reclick the day.
        cut.Find("input").Click();
        var timeButtons = cut.FindAll("button[type='button']").TakeLast(4).ToList();
        timeButtons[0].Click(); // hour increment

        // Assert
        Assert.NotNull(rangeFromCallback);
        Assert.Equal(10, rangeFromCallback!.Value.Key!.Value.Hour);
        Assert.Equal(5, rangeFromCallback.Value.Key!.Value.Day);
    }

    [Fact]
    public void ReopeningAfterCompleteRange_StartsBackOnStepOne()
    {
        // Arrange
        var start = new DateTime(2025, 11, 5);
        var end = new DateTime(2025, 11, 15);
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<DateTime?, DateTime?>(start, end))
            .Add(x => x.SelectedRangeChanged, NoOpRangeCallback(this))
        );

        // Act
        cut.Find("input").Click();

        // Assert
        Assert.True(IsStageActive(cut, "Start"));
    }

    [Fact]
    public void ClickingEndTab_BeforeStartIsPicked_SwitchesStepWithoutClosingPanel()
    {
        // Arrange - the Start/End tabs let a user jump directly to either step rather than being
        // locked into picking strictly start-then-end.
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRangeChanged, NoOpRangeCallback(this))
        );

        // Act
        cut.Find("input").Click();
        cut.FindAll("[role='group'][aria-label='Range step'] button").First(b => b.TextContent.Trim() == "End").Click();

        // Assert
        Assert.True(IsStageActive(cut, "End"));
        Assert.Contains("datepicker-grid", cut.Markup);
    }

    [Fact]
    public void PickingEndFirst_ViaTab_SwitchesBackToStart_WithoutCompletingOrClosing()
    {
        // Arrange - picking an end with no start yet can't complete the range, so it should bounce
        // back to the start step instead of closing the panel.
        KeyValuePair<DateTime?, DateTime?>? rangeFromCallback = null;
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<DateTime?, DateTime?>(null, new DateTime(2025, 11, 1)))
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<DateTime?, DateTime?>>(this, r => rangeFromCallback = r))
        );

        // Act
        cut.Find("input").Click();
        cut.FindAll("[role='group'][aria-label='Range step'] button").First(b => b.TextContent.Trim() == "End").Click();
        cut.FindAll("button.day").First(b => b.TextContent.Trim() == "20").Click();

        // Assert
        Assert.NotNull(rangeFromCallback);
        Assert.Null(rangeFromCallback!.Value.Key);
        Assert.Equal(20, rangeFromCallback.Value.Value!.Value.Day);
        Assert.True(IsStageActive(cut, "Start"));
        Assert.Contains("datepicker-grid", cut.Markup);
    }

    [Fact]
    public void SwitchingBackToStartTab_AfterAdvancingToEnd_ReanchorsToExistingStart()
    {
        // Arrange
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<DateTime?, DateTime?>(new DateTime(2025, 11, 1), null))
            .Add(x => x.SelectedRangeChanged, NoOpRangeCallback(this))
        );

        cut.Find("input").Click();
        cut.FindAll("button.day").First(b => b.TextContent.Trim() == "10").Click(); // advances to End, jumps to November

        // Act
        cut.FindAll("[role='group'][aria-label='Range step'] button").First(b => b.TextContent.Trim() == "Start").Click();

        // Assert - back on the Start step, still showing the picked start's month.
        Assert.True(IsStageActive(cut, "Start"));
        Assert.Contains("November 2025", cut.Markup);
    }

    [Fact]
    public void TypingValidRangeText_ParsesAndSelectsRange()
    {
        // Arrange
        KeyValuePair<DateTime?, DateTime?>? rangeFromCallback = null;

        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<DateTime?, DateTime?>>(this, r => rangeFromCallback = r))
        );

        // Act
        cut.Find("input").Change("05/11/2025 09:00 - 15/11/2025 17:30");

        // Assert
        Assert.NotNull(rangeFromCallback);
        Assert.Equal(new DateTime(2025, 11, 5, 9, 0, 0), rangeFromCallback!.Value.Key);
        Assert.Equal(new DateTime(2025, 11, 15, 17, 30, 0), rangeFromCallback.Value.Value);
    }

    [Fact]
    public void TypingReversedRangeText_SwapsStartAndEnd()
    {
        // Arrange
        KeyValuePair<DateTime?, DateTime?>? rangeFromCallback = null;

        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<DateTime?, DateTime?>>(this, r => rangeFromCallback = r))
        );

        // Act
        cut.Find("input").Change("15/11/2025 17:30 - 05/11/2025 09:00");

        // Assert
        Assert.NotNull(rangeFromCallback);
        Assert.Equal(new DateTime(2025, 11, 5, 9, 0, 0), rangeFromCallback!.Value.Key);
        Assert.Equal(new DateTime(2025, 11, 15, 17, 30, 0), rangeFromCallback.Value.Value);
    }

    [Fact]
    public void TypingInvalidRangeText_ShowsError_DoesNotSelect()
    {
        // Arrange
        var callbackInvoked = false;
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<DateTime?, DateTime?>>(this, _ => callbackInvoked = true))
        );

        // Act
        cut.Find("input").Change("not a range");

        // Assert
        Assert.False(callbackInvoked);
        Assert.True(cut.Instance.Invalid);
        Assert.Equal("Enter a valid date range", cut.Instance.ErrorMessage);
    }

    [Fact]
    public void Is12HourFormat_AppliesToDisplayedValue()
    {
        // Arrange & Act
        var start = new DateTime(2025, 11, 5, 9, 0, 0);
        var end = new DateTime(2025, 11, 20, 14, 0, 0);

        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<DateTime?, DateTime?>(start, end))
            .Add(x => x.Is12HourFormat, true)
        );

        // Assert
        Assert.Equal("05/11/2025 09:00 AM - 20/11/2025 02:00 PM", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void NothingSelected_ProducesEmptyValue()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDateTimeRangePicker>();

        // Assert
        Assert.Equal(string.Empty, cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void OnlyStartSelected_ValueShowsJustTheStart()
    {
        // Arrange & Act
        var start = new DateTime(2025, 11, 5, 9, 0, 0);

        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<DateTime?, DateTime?>(start, null))
        );

        // Assert
        Assert.Equal("05/11/2025 09:00", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void ClickingDayBeforeMinDate_DoesNotSelect()
    {
        // Arrange
        var callbackInvoked = false;
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, default(KeyValuePair<DateTime?, DateTime?>))
            .Add(x => x.MinDate, new DateTime(2025, 11, 10))
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<DateTime?, DateTime?>>(this, _ => callbackInvoked = true))
        );

        // Act
        cut.Find("input").Click();
        cut.FindAll("button.day").First(b => b.TextContent.Trim() == "5").Click();

        // Assert
        Assert.False(callbackInvoked);
    }

    [Fact]
    public void ReadOnly_PreventsTypedChanges()
    {
        // Arrange
        var callbackInvoked = false;
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.ReadOnly, true)
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<DateTime?, DateTime?>>(this, _ => callbackInvoked = true))
        );

        // Act
        cut.Find("input").Change("05/11/2025 09:00 - 15/11/2025 17:30");

        // Assert
        Assert.False(callbackInvoked);
        Assert.True(cut.Find("input").HasAttribute("readonly"));
    }

    [Fact]
    public void Disabled_RendersDisabledInput()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.Disabled, true)
        );

        // Assert
        Assert.True(cut.Find("input").HasAttribute("disabled"));
    }

    [Fact]
    public async Task Close_UnregistersJS_And_HidesPopup()
    {
        // Arrange
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, default(KeyValuePair<DateTime?, DateTime?>))
            .Add(x => x.SelectedRangeChanged, NoOpRangeCallback(this))
        );

        // Act
        cut.Find("input").Click();
        Assert.Contains("datepicker-grid", cut.Markup);
        await cut.Instance.Close();

        // Assert
        Assert.DoesNotContain("datepicker-grid", cut.Markup);
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.unregisterOutsideClick");
    }

    [Fact]
    public void UnsetFormat_UsesThemeDefault24HourFormat()
    {
        // Arrange
        var datePickerTheme = Theme.Components.Require<TwDatePickerTheme>();
        datePickerTheme.DefaultDateTimeFormat = "MM/dd/yyyy HH:mm";

        // Act
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<DateTime?, DateTime?>(new DateTime(2025, 11, 5, 9, 0, 0), new DateTime(2025, 11, 15, 17, 0, 0)))
        );

        // Assert
        Assert.Equal("11/05/2025 09:00 - 11/15/2025 17:00", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void ExplicitFormat_OverridesThemeDefault_AndIs12HourFormat()
    {
        // Arrange - an explicit Format wins regardless of Is12HourFormat.
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<DateTime?, DateTime?>(new DateTime(2025, 11, 5, 9, 0, 0), new DateTime(2025, 11, 15, 17, 0, 0)))
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.Format, "yyyy-MM-dd HH:mm")
        );

        // Assert
        Assert.Equal("2025-11-05 09:00 - 2025-11-15 17:00", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void UnsetRangeSeparator_UsesThemeDefaultSeparator()
    {
        // Arrange
        var datePickerTheme = Theme.Components.Require<TwDatePickerTheme>();
        datePickerTheme.DefaultRangeSeparator = " to ";

        // Act
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<DateTime?, DateTime?>(new DateTime(2025, 11, 5, 9, 0, 0), new DateTime(2025, 11, 15, 17, 0, 0)))
        );

        // Assert
        Assert.Equal("05/11/2025 09:00 to 15/11/2025 17:00", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void ClickingDayAfterMaxDate_DoesNotSelect()
    {
        // Arrange - violates MaxDate while picking the start.
        var callbackInvoked = false;
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, default(KeyValuePair<DateTime?, DateTime?>))
            .Add(x => x.MaxDate, new DateTime(2025, 11, 20))
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<DateTime?, DateTime?>>(this, _ => callbackInvoked = true))
        );

        // Act
        cut.Find("input").Click();
        cut.FindAll("button.day").First(b => b.TextContent.Trim() == "25").Click();

        // Assert
        Assert.False(callbackInvoked);
    }

    [Fact]
    public void TypingRangeOutsideMinMaxBounds_ShowsError_DoesNotSelect()
    {
        // Arrange
        var callbackInvoked = false;
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.MinDate, new DateTime(2025, 11, 10))
            .Add(x => x.MaxDate, new DateTime(2025, 11, 20))
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<DateTime?, DateTime?>>(this, _ => callbackInvoked = true))
        );

        // Act
        cut.Find("input").Change("01/11/2025 09:00 - 15/11/2025 17:00");

        // Assert
        Assert.False(callbackInvoked);
        Assert.True(cut.Instance.Invalid);
    }

    [Fact]
    public void TypingWhitespace_DoesNothing()
    {
        // Arrange
        var callbackInvoked = false;
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<DateTime?, DateTime?>>(this, _ => callbackInvoked = true))
        );

        // Act
        cut.Find("input").Change("   ");

        // Assert
        Assert.False(callbackInvoked);
        Assert.False(cut.Instance.Invalid);
    }

    [Fact]
    public void EffectivePlaceholder_UsesFormatAndDefaultSeparator()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDateTimeRangePicker>();

        // Assert
        Assert.Equal("dd/mm/yyyy hh:mm - dd/mm/yyyy hh:mm", cut.Find("input").GetAttribute("placeholder"));
    }

    [Fact]
    public void ExplicitPlaceholder_OverridesTheComputedDefault()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.Placeholder, "Pick a stay")
        );

        // Assert
        Assert.Equal("Pick a stay", cut.Find("input").GetAttribute("placeholder"));
    }

    [Fact]
    public void ReadOnly_DisablesTimeStepperButtons()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDateTimeRangePicker>(p => p
            .Add(x => x.ReadOnly, true)
        );

        // Assert - the calendar/time panel never opens for a readonly field.
        cut.Find("input").Click();
        Assert.DoesNotContain("datepicker-grid", cut.Markup);
    }
}
