using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Builders;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.TimeRangePicker;

public class TwTimeRangePickerTests : TwBlazorTestBase
{
    private TwInputTheme inputTheme => Theme.Components.Require<TwInputTheme>();

    public TwTimeRangePickerTests()
    {
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        TestContext.JSInterop.SetupVoid("twPicker.registerOutsideClick");
        TestContext.JSInterop.SetupVoid("twPicker.unregisterOutsideClick");
    }

    private static EventCallback<KeyValuePair<TimeOnly?, TimeOnly?>> NoOpRangeCallback(TwTimeRangePickerTests owner) =>
        EventCallback.Factory.Create<KeyValuePair<TimeOnly?, TimeOnly?>>(owner, _ => { });

    private static bool IsStageActive(IRenderedComponent<TwTimeRangePicker> cut, string label) =>
        cut.FindAll("[role='group'][aria-label='Range step'] button").First(b => b.TextContent.Trim() == label).GetAttribute("aria-pressed") == "true";

    [Fact]
    public void FocusingInput_PositionsPanelAsFixed_AnchoredToTheInputRoot()
    {
        // Regression test - see the identical test on TwTimePickerTests for the full rationale.
        var cut = TestContext.Render<TwTimeRangePicker>();

        cut.Find("input").Click();

        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.registerScrollReposition");
        Assert.IsType<ElementReference>(invocation.Arguments[0]);
        Assert.IsType<ElementReference>(invocation.Arguments[1]);
    }

    [Fact]
    public void RendersOneMergedInput()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTimeRangePicker>();

        // Assert
        Assert.Single(cut.FindAll("input"));
    }

    [Fact]
    public void FocusingInput_OpensPanel_StartingOnStartTab()
    {
        // Arrange
        var cut = TestContext.Render<TwTimeRangePicker>();

        // Act
        cut.Find("input").Click();

        // Assert
        Assert.True(IsStageActive(cut, "Start"));
        Assert.Contains("role=\"dialog\"", cut.Markup);
    }

    [Fact]
    public void PanelNumberInputs_UseGlobalDefaultVariant_WhenNotSet()
    {
        // Arrange - the embedded TwTimePickerBody's hour/minute inputs used to be styled with a
        // hardcoded transparent-background look regardless of TwInputTheme.DefaultInputVariant, so
        // they never looked "Filled" even when that was the configured global default.
        inputTheme.DefaultInputVariant = InputVariant.Filled;

        var cut = TestContext.Render<TwTimeRangePicker>();

        // Act
        cut.Find("input").Click();

        // Assert
        var numberInputs = cut.FindAll("input").Skip(1).ToList(); // merged trigger, then hour/minute
        Assert.Equal(2, numberInputs.Count);
        var expectedClasses = InputVariantBuilder.GetClasses(InputVariant.Filled, inputTheme);
        foreach (var input in numberInputs)
        {
            Assert.Contains(expectedClasses, input.GetAttribute("class"));
        }
    }

    [Fact]
    public void AdjustingStartTime_UpdatesImmediately()
    {
        // Arrange
        KeyValuePair<TimeOnly?, TimeOnly?>? rangeFromCallback = null;
        var cut = TestContext.Render<TwTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<TimeOnly?, TimeOnly?>(new TimeOnly(9, 0), null))
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<TimeOnly?, TimeOnly?>>(this, r => rangeFromCallback = r))
        );

        // Act - editing while the Start tab is active applies straight to the start time.
        cut.Find("input").Click();
        var timeButtons = cut.FindAll("[data-tw-popover] button[type='button']").TakeLast(4).ToList();
        timeButtons[0].Click(); // hour increment: 09:00 -> 10:00

        // Assert
        Assert.NotNull(rangeFromCallback);
        Assert.Equal(new TimeOnly(10, 0), rangeFromCallback!.Value.Key);
        Assert.Null(rangeFromCallback.Value.Value);
    }

    [Fact]
    public void SwitchingToEndTab_AdjustingTime_UpdatesEndOnly()
    {
        // Arrange
        KeyValuePair<TimeOnly?, TimeOnly?>? rangeFromCallback = null;
        var cut = TestContext.Render<TwTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<TimeOnly?, TimeOnly?>(new TimeOnly(9, 0), new TimeOnly(17, 0)))
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<TimeOnly?, TimeOnly?>>(this, r => rangeFromCallback = r))
        );

        // Act
        cut.Find("input").Click();
        cut.FindAll("[role='group'][aria-label='Range step'] button").First(b => b.TextContent.Trim() == "End").Click();
        var timeButtons = cut.FindAll("[data-tw-popover] button[type='button']").TakeLast(4).ToList();
        timeButtons[2].Click(); // minute increment: 17:00 -> 17:01

        // Assert
        Assert.NotNull(rangeFromCallback);
        Assert.Equal(new TimeOnly(9, 0), rangeFromCallback!.Value.Key);
        Assert.Equal(new TimeOnly(17, 1), rangeFromCallback.Value.Value);
        Assert.True(IsStageActive(cut, "End"));
    }

    [Fact]
    public void ReopeningAfterEditingEnd_StartsBackOnStartTab()
    {
        // Arrange
        var cut = TestContext.Render<TwTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<TimeOnly?, TimeOnly?>(new TimeOnly(9, 0), new TimeOnly(17, 0)))
            .Add(x => x.SelectedRangeChanged, NoOpRangeCallback(this))
        );

        cut.Find("input").Click();
        cut.FindAll("[role='group'][aria-label='Range step'] button").First(b => b.TextContent.Trim() == "End").Click();
        cut.InvokeAsync(() => cut.Instance.Close());

        // Act
        cut.Find("input").Click();

        // Assert
        Assert.True(IsStageActive(cut, "Start"));
    }

    [Fact]
    public void TypingValidRangeText_ParsesAndSelectsRange()
    {
        // Arrange
        KeyValuePair<TimeOnly?, TimeOnly?>? rangeFromCallback = null;

        var cut = TestContext.Render<TwTimeRangePicker>(p => p
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<TimeOnly?, TimeOnly?>>(this, r => rangeFromCallback = r))
        );

        // Act
        cut.Find("input").Change("09:00 - 17:30");

        // Assert
        Assert.NotNull(rangeFromCallback);
        Assert.Equal(new TimeOnly(9, 0), rangeFromCallback!.Value.Key);
        Assert.Equal(new TimeOnly(17, 30), rangeFromCallback.Value.Value);
    }

    [Fact]
    public void TypingOvernightRangeText_IsNotSwapped()
    {
        // Arrange - an end earlier than the start is a legitimate overnight range here, unlike the
        // date-based range pickers, so it must not be reordered.
        KeyValuePair<TimeOnly?, TimeOnly?>? rangeFromCallback = null;

        var cut = TestContext.Render<TwTimeRangePicker>(p => p
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<TimeOnly?, TimeOnly?>>(this, r => rangeFromCallback = r))
        );

        // Act
        cut.Find("input").Change("22:00 - 06:00");

        // Assert
        Assert.NotNull(rangeFromCallback);
        Assert.Equal(new TimeOnly(22, 0), rangeFromCallback!.Value.Key);
        Assert.Equal(new TimeOnly(6, 0), rangeFromCallback.Value.Value);
    }

    [Fact]
    public void TypingInvalidRangeText_ShowsError_DoesNotSelect()
    {
        // Arrange
        var callbackInvoked = false;
        var cut = TestContext.Render<TwTimeRangePicker>(p => p
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<TimeOnly?, TimeOnly?>>(this, _ => callbackInvoked = true))
        );

        // Act
        cut.Find("input").Change("not a range");

        // Assert
        Assert.False(callbackInvoked);
        Assert.True(cut.Instance.Invalid);
        Assert.StartsWith("Enter a valid time range, for example ", cut.Instance.ErrorMessage);
    }

    [Fact]
    public void TypingWhitespace_DoesNothing()
    {
        // Arrange
        var callbackInvoked = false;
        var cut = TestContext.Render<TwTimeRangePicker>(p => p
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<TimeOnly?, TimeOnly?>>(this, _ => callbackInvoked = true))
        );

        // Act
        cut.Find("input").Change("   ");

        // Assert
        Assert.False(callbackInvoked);
        Assert.False(cut.Instance.Invalid);
    }

    [Fact]
    public void Is12HourFormat_AppliesToDisplayedValue()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<TimeOnly?, TimeOnly?>(new TimeOnly(9, 0), new TimeOnly(14, 0)))
            .Add(x => x.Is12HourFormat, true)
        );

        // Assert
        Assert.Equal("09:00 AM - 02:00 PM", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void ExplicitFormat_OverridesIs12HourFormat()
    {
        // Arrange & Act - an explicit Format wins regardless of Is12HourFormat.
        var cut = TestContext.Render<TwTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<TimeOnly?, TimeOnly?>(new TimeOnly(9, 0), new TimeOnly(14, 0)))
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.Format, "HH:mm")
        );

        // Assert
        Assert.Equal("09:00 - 14:00", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void NothingSelected_ProducesEmptyValue()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTimeRangePicker>();

        // Assert
        Assert.Equal(string.Empty, cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void OnlyStartSelected_ValueShowsJustTheStart()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<TimeOnly?, TimeOnly?>(new TimeOnly(9, 0), null))
        );

        // Assert
        Assert.Equal("09:00", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void CustomRangeSeparator_AppliesToDisplayedValue_AndParsing()
    {
        // Arrange
        KeyValuePair<TimeOnly?, TimeOnly?>? rangeFromCallback = null;
        var cut = TestContext.Render<TwTimeRangePicker>(p => p
            .Add(x => x.SelectedRange, new KeyValuePair<TimeOnly?, TimeOnly?>(new TimeOnly(9, 0), new TimeOnly(17, 0)))
            .Add(x => x.RangeSeparator, " to ")
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<TimeOnly?, TimeOnly?>>(this, r => rangeFromCallback = r))
        );

        // Assert - display uses the custom separator.
        Assert.Equal("09:00 to 17:00", cut.Find("input").GetAttribute("value"));

        // Act - and it also parses back using that same separator.
        cut.Find("input").Change("08:00 to 16:00");

        // Assert
        Assert.NotNull(rangeFromCallback);
        Assert.Equal(new TimeOnly(8, 0), rangeFromCallback!.Value.Key);
        Assert.Equal(new TimeOnly(16, 0), rangeFromCallback.Value.Value);
    }

    [Fact]
    public void EffectivePlaceholder_UsesFormatAndDefaultSeparator()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTimeRangePicker>();

        // Assert
        Assert.Equal("hh:mm - hh:mm", cut.Find("input").GetAttribute("placeholder"));
    }

    [Fact]
    public void ExplicitPlaceholder_OverridesTheComputedDefault()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTimeRangePicker>(p => p
            .Add(x => x.Placeholder, "Pick a shift")
        );

        // Assert
        Assert.Equal("Pick a shift", cut.Find("input").GetAttribute("placeholder"));
    }

    [Fact]
    public void ReadOnly_PreventsTypedChanges()
    {
        // Arrange
        var callbackInvoked = false;
        var cut = TestContext.Render<TwTimeRangePicker>(p => p
            .Add(x => x.ReadOnly, true)
            .Add(x => x.SelectedRangeChanged, EventCallback.Factory.Create<KeyValuePair<TimeOnly?, TimeOnly?>>(this, _ => callbackInvoked = true))
        );

        // Act
        cut.Find("input").Change("09:00 - 17:00");

        // Assert
        Assert.False(callbackInvoked);
        Assert.True(cut.Find("input").HasAttribute("readonly"));
    }

    [Fact]
    public void ReadOnly_DoesNotOpenPanel()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTimeRangePicker>(p => p
            .Add(x => x.ReadOnly, true)
        );

        // Act
        cut.Find("input").Click();

        // Assert - the picker panel never opens for a readonly field.
        Assert.DoesNotContain("role=\"dialog\"", cut.Markup);
    }

    [Fact]
    public void Disabled_RendersDisabledInput()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTimeRangePicker>(p => p
            .Add(x => x.Disabled, true)
        );

        // Assert
        Assert.True(cut.Find("input").HasAttribute("disabled"));
    }

    [Fact]
    public async Task Close_UnregistersJS_And_HidesPopup()
    {
        // Arrange
        var cut = TestContext.Render<TwTimeRangePicker>(p => p
            .Add(x => x.SelectedRangeChanged, NoOpRangeCallback(this))
        );

        // Act
        cut.Find("input").Click();
        Assert.Contains("role=\"dialog\"", cut.Markup);
        await cut.Instance.Close();

        // Assert
        Assert.DoesNotContain("role=\"dialog\"", cut.Markup);
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.unregisterOutsideClick");
    }
}
