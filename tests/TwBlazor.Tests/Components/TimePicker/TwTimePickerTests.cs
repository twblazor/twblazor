using Bunit;
using Microsoft.AspNetCore.Components;
using System.Reflection;
using TwBlazor.Builders;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.TimePicker;

public class TwTimePickerTests : TwBlazorTestBase
{
    private TwInputTheme inputTheme => Theme.Components.Require<TwInputTheme>();

    public TwTimePickerTests()
    {
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        TestContext.JSInterop.SetupVoid("jPicker.registerOutsideClick");
        TestContext.JSInterop.SetupVoid("jPicker.unregisterOutsideClick");
    }

    [Fact]
    public void FocusingInput_PositionsPanelAsFixed_AnchoredToTheInputRoot()
    {
        // Regression test: the popover panel must be positioned via twPicker.registerScrollReposition
        // (which applies twPicker.positionPanelFixed itself - position:fixed, anchored via
        // JS-computed coordinates), so it isn't clipped when the picker is used inside a scrollable
        // ancestor such as a TwDialog's body - previously this panel had no dynamic positioning at
        // all, just static CSS.
        var cut = TestContext.Render<TwTimePicker>();

        cut.Find("input[type='text']").Click();

        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.registerScrollReposition");
        Assert.IsType<ElementReference>(invocation.Arguments[0]);
        Assert.IsType<ElementReference>(invocation.Arguments[1]);
    }

    [Fact]
    public void TwTimePicker_RendersWithDefaultValues()
    {
        // Act
        var cut = TestContext.Render<TwTimePicker>();

        // Assert
        Assert.Contains("type=\"text\"", cut.Markup);
        Assert.Contains("Select a time", cut.Markup); // Default placeholder
    }

    [Fact]
    public void TwTimePicker_DisplaysProvidedTime()
    {
        // Arrange
        var selectedTime = new TimeOnly(14, 30); // 2:30 PM

        // Act
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.SelectedTime, selectedTime)
        );

        // Assert
        var input = cut.Find("input[type='text']");
        Assert.Equal("14:30", input.GetAttribute("value"));
    }

    [Fact]
    public void TwTimePicker_DisplaysTime_In12HourFormat()
    {
        // Arrange
        var selectedTime = new TimeOnly(14, 30); // 2:30 PM

        // Act
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.SelectedTime, selectedTime)
            .Add(x => x.Is12HourFormat, true)
        );

        // Assert
        var input = cut.Find("input[type='text']");
        Assert.Contains("02:30 PM", input.GetAttribute("value"));
    }

    [Fact]
    public void TwTimePicker_ShowsTimePickerBody_OnFocus()
    {
        // Arrange
        var selectedTime = new TimeOnly(10, 15);

        // Act
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.SelectedTime, selectedTime)
        );

        cut.Find("input[type='text']").Click();

        // Assert
        Assert.Contains("Choose time", cut.Markup);
        // Verify time picker controls are rendered (4 buttons for hour/minute up/down)
        var buttons = cut.FindAll("button[type='button']");
        Assert.True(buttons.Count >= 4, "Expected at least 4 control buttons for time picker");
    }

    [Fact]
    public void PanelNumberInputs_UseGlobalDefaultVariant_WhenNotSet()
    {
        // Arrange - the hour/minute number inputs inside the popover used to be styled with a
        // hardcoded transparent-background look regardless of TwInputTheme.DefaultInputVariant,
        // so they never looked "Filled" even when that was the configured global default.
        inputTheme.DefaultInputVariant = InputVariant.Filled;

        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.SelectedTime, new TimeOnly(10, 15))
        );

        // Act
        cut.Find("input[type='text']").Click();

        // Assert
        var numberInputs = cut.FindAll("input[type='text']").Skip(1).ToList();
        Assert.Equal(2, numberInputs.Count); // hour, minute
        var expectedClasses = InputVariantBuilder.GetClasses(InputVariant.Filled, inputTheme);
        foreach (var input in numberInputs)
        {
            Assert.Contains(expectedClasses, input.GetAttribute("class"));
        }
    }

    [Fact]
    public void TwTimePicker_RendersLabel_WhenProvided()
    {
        // Act
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.Label, "Select your time")
        );

        // Assert
        Assert.Contains("Select your time", cut.Markup);
    }

    [Fact]
    public void TwTimePicker_RendersCustomPlaceholder()
    {
        // Act
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.Placeholder, "Pick a time")
        );

        // Assert
        Assert.Contains("Pick a time", cut.Markup);
    }

    [Fact]
    public void TwTimePicker_UpdatesValue_OnTimeChange()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        string? callbackValue = null;
        var initialTime = new TimeOnly(10, 30);

        // Act
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<string>(this, v => callbackValue = v))
        );

        cut.Find("input[type='text']").Click();

        var buttons = cut.FindAll("button[type='button']");
        buttons[0].Click(); // Hour increment button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(11, callbackTime.Value.Hour);
        Assert.Equal(30, callbackTime.Value.Minute);
        Assert.NotNull(callbackValue);
        Assert.Equal("11:30", callbackValue);
    }

    [Fact]
    public void TwTimePicker_ParsesTextInput_AndUpdatesTime()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(10, 30);

        // Act
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        var input = cut.Find("input[type='text']");
        input.Change("15:45");

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(15, callbackTime.Value.Hour);
        Assert.Equal(45, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePicker_RendersClockIcon()
    {
        // Act
        var cut = TestContext.Render<TwTimePicker>();

        // Assert
        // Clock icon is present in the component
        Assert.Contains("<svg", cut.Markup);
    }

    [Fact]
    public void TwTimePicker_AppliesCustomClass()
    {
        // Act
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.RootClass, "custom-class")
        );

        // Assert
        Assert.Contains("custom-class", cut.Markup);
    }

    [Fact]
    public void TwTimePicker_SupportsBindValue()
    {
        // Arrange
        var selectedTime = new TimeOnly(14, 30);

        // Act
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.SelectedTime, selectedTime)
            .Add(x => x.Value, "14:30")
        );

        // Assert
        var input = cut.Find("input[type='text']");
        Assert.Equal("14:30", input.GetAttribute("value"));
    }

    [Fact]
    public void TwTimePicker_IncrementsMinute_AndUpdatesTime()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(10, 30);

        // Act
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        cut.Find("input[type='text']").Click();

        var buttons = cut.FindAll("button[type='button']");
        buttons[2].Click(); // Minute increment button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(10, callbackTime.Value.Hour);
        Assert.Equal(31, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePicker_DecrementsHour_AndUpdatesTime()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var initialTime = new TimeOnly(10, 30);

        // Act
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.SelectedTime, initialTime)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
        );

        cut.Find("input[type='text']").Click();

        var buttons = cut.FindAll("button[type='button']");
        buttons[1].Click(); // Hour decrement button

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(9, callbackTime.Value.Hour);
        Assert.Equal(30, callbackTime.Value.Minute);
    }

    [Fact]
    public void TwTimePicker_OnFocus_DoesNotShowBody_WhenReadOnly()
    {
        // Act
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.ReadOnly, true));

        cut.Find("input[type='text']").Click();

        // Assert
        Assert.DoesNotContain("Choose time", cut.Markup);
    }

    [Fact]
    public void TwTimePicker_OnFocus_DoesNotShowBody_WhenDisabled()
    {
        // Act
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.Disabled, true));

        cut.Find("input[type='text']").Click();

        // Assert
        Assert.DoesNotContain("Choose time", cut.Markup);
    }

    [Fact]
    public void TwTimePicker_Close_HidesTimePickerBody()
    {
        // Arrange
        var cut = TestContext.Render<TwTimePicker>();
        cut.Find("input[type='text']").Click();
        Assert.Contains("Choose time", cut.Markup);

        // Act
        cut.InvokeAsync(() => cut.Instance.Close());

        // Assert
        Assert.DoesNotContain("Choose time", cut.Markup);
    }

    [Fact]
    public void TwTimePicker_TextValueChanged_DoesNothing_WhenReadOnly()
    {
        // Arrange
        string? callbackValue = null;
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.ReadOnly, true)
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<string>(this, v => callbackValue = v)));

        var input = cut.Find("input[type='text']");

        // Act
        input.Change("15:45");

        // Assert
        Assert.Null(callbackValue);
    }

    [Fact]
    public void TwTimePicker_TextValueChanged_DoesNothing_WhenDisabled()
    {
        // Arrange
        string? callbackValue = null;
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.Disabled, true)
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<string>(this, v => callbackValue = v)));

        var input = cut.Find("input[type='text']");

        // Act
        input.Change("15:45");

        // Assert
        Assert.Null(callbackValue);
    }

    [Fact]
    public void TwTimePicker_TextValueChanged_InvalidTime_DoesNotInvokeSelectedTimeChanged()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        string? callbackValue = null;
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<string>(this, v => callbackValue = v)));

        var input = cut.Find("input[type='text']");

        // Act
        input.Change("not-a-time");

        // Assert
        Assert.Null(callbackTime);
        Assert.Equal("not-a-time", callbackValue);
    }

    [Fact]
    public async Task TwTimePicker_DisposeAsync_CompletesSuccessfully()
    {
        // Arrange
        var cut = TestContext.Render<TwTimePicker>();
        cut.Find("input[type='text']").Click();

        // Act
        var exception = await Record.ExceptionAsync(() => cut.Instance.DisposeAsync().AsTask());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void TwTimePicker_Focus_TwiceInARow_OnlyRegistersOutsideClickOnce()
    {
        // Arrange
        var cut = TestContext.Render<TwTimePicker>();

        // Act
        cut.Find("input[type='text']").Click();
        cut.Find("input[type='text']").Click();

        // Assert
        Assert.Contains("Choose time", cut.Markup);
    }

    [Fact]
    public void PreferNativePickerTrue_RendersNativeTimeInput_With24HourValue()
    {
        // Arrange & Act — Is12HourFormat is ignored while native: the browser renders its own localized UI.
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.SelectedTime, new TimeOnly(14, 30))
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.PreferNativePicker, true)
        );

        var input = cut.Find("input");

        // Assert
        Assert.Equal("time", input.GetAttribute("type"));
        Assert.Equal("14:30", input.GetAttribute("value"));
    }

    [Fact]
    public void PreferNativePickerTrue_ThenParameterChange_KeepsFormat24Hour_EvenWithIs12HourFormat()
    {
        // Arrange - OnParametersSetAsync's `!UseNativePicker && Is12HourFormat` ternary reads
        // UseNativePicker as it stood at the time parameters were last set. On the very first render
        // UseNativePicker is still false (OnAfterRenderAsync hasn't flipped it to true yet), so that
        // branch only ever sees UseNativePicker=true once OnAfterRenderAsync has already run and a
        // later parameter change re-invokes OnParametersSetAsync - exercised here by re-rendering
        // after the first render instead of setting everything up-front.
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.SelectedTime, new TimeOnly(14, 30))
            .Add(x => x.PreferNativePicker, true)
        );

        // Act - re-render with Is12HourFormat now true; UseNativePicker is already true from the
        // first render's OnAfterRenderAsync, so OnParametersSetAsync must keep the native 24-hour format.
        cut.Render(p => p
            .Add(x => x.SelectedTime, new TimeOnly(14, 30))
            .Add(x => x.PreferNativePicker, true)
            .Add(x => x.Is12HourFormat, true)
        );

        // Assert
        var input = cut.Find("input");
        Assert.Equal("14:30", input.GetAttribute("value"));
    }

    [Fact]
    public void PreferNativePickerTrue_FocusingInput_DoesNotShowCustomBody()
    {
        // Arrange
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.PreferNativePicker, true)
        );

        // Act
        cut.Find("input").Click();

        // Assert
        Assert.DoesNotContain("Choose time", cut.Markup);
    }

    [Fact]
    public void PreferNativePickerTrue_ChangingNativeInput_ParsesValue_AndInvokesCallbacks()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        string? callbackValue = null;

        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.SelectedTime, new TimeOnly(10, 30))
            .Add(x => x.PreferNativePicker, true)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t))
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<string>(this, v => callbackValue = v))
        );

        // Act — native <input type="time"> always sends a 24-hour "HH:mm" value on change.
        cut.Find("input").Change("15:45");

        // Assert
        Assert.NotNull(callbackTime);
        Assert.Equal(15, callbackTime!.Value.Hour);
        Assert.Equal(45, callbackTime.Value.Minute);
        Assert.Equal("15:45", callbackValue);
    }

    [Fact]
    public void PreferNativePickerFalse_RendersTextInput_AndOpensCustomBodyOnFocus()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.PreferNativePicker, false)
        );

        var input = cut.Find("input");
        Assert.Equal("text", input.GetAttribute("type"));

        // Act
        input.Click();

        // Assert
        Assert.Contains("Choose time", cut.Markup);
    }

    [Fact]
    public void NativePickerNotSpecified_DetectsViaJsInterop_AndSwitchesToTimeInput()
    {
        // Arrange
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(true);

        // Act
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.SelectedTime, new TimeOnly(9, 5))
        );

        // Assert
        var input = cut.Find("input");
        Assert.Equal("time", input.GetAttribute("type"));
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDevice.prefersNativePicker");
    }

    [Fact]
    public void NativePickerNotSpecified_Is12HourFormat_KeepsCustomBody_EvenWhenDeviceWouldPreferNative()
    {
        // Arrange - iOS/Android renders <input type="time"> using the device's own 24-hour-time
        // setting and cannot be forced to show AM/PM, so auto-detection must not switch to it when
        // the caller has explicitly asked for a 12-hour display (issue #159).
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(true);

        // Act
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.SelectedTime, new TimeOnly(14, 30))
            .Add(x => x.Is12HourFormat, true)
        );

        // Assert
        var input = cut.Find("input");
        Assert.Equal("text", input.GetAttribute("type"));
        Assert.Contains("02:30 PM", input.GetAttribute("value"));

        input.Click();
        Assert.Contains("Choose time", cut.Markup);
    }

    [Fact]
    public void PreferNativePickerTrue_Is12HourFormat_StillUsesNativeInput()
    {
        // Arrange & Act - an explicit PreferNativePicker="true" is a deliberate opt-in that accepts
        // the native control's device-driven format, unlike plain auto-detection.
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.SelectedTime, new TimeOnly(14, 30))
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.PreferNativePicker, true)
        );

        // Assert
        var input = cut.Find("input");
        Assert.Equal("time", input.GetAttribute("type"));
    }

    [Fact]
    public void NativePickerNotSpecified_JsInteropReturnsFalse_KeepsCustomBody()
    {
        // Arrange
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(false);

        // Act
        var cut = TestContext.Render<TwTimePicker>();

        var input = cut.Find("input");
        Assert.Equal("text", input.GetAttribute("type"));

        input.Click();

        // Assert
        Assert.Contains("Choose time", cut.Markup);
    }

    [Fact]
    public void TriggerAttributes_HasAriaHaspopupDialog_AndAriaExpandedFalse_WhenClosed()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.PreferNativePicker, false));

        // Assert
        var input = cut.Find("input[type='text']");
        Assert.Equal("dialog", input.GetAttribute("aria-haspopup"));
        Assert.Equal("false", input.GetAttribute("aria-expanded"));
    }

    [Fact]
    public void TriggerAttributes_AriaExpandedTrue_WhenOpen()
    {
        // Arrange
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.PreferNativePicker, false));

        // Act
        cut.Find("input[type='text']").Click();

        // Assert
        var input = cut.Find("input[type='text']");
        Assert.Equal("true", input.GetAttribute("aria-expanded"));
    }

    [Fact]
    public void TriggerAttributes_OmitsAriaHaspopup_WhenUsingNativePicker()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.PreferNativePicker, true));

        // Assert
        var input = cut.Find("input");
        Assert.Null(input.GetAttribute("aria-haspopup"));
        Assert.Null(input.GetAttribute("aria-expanded"));
    }

    [Fact]
    public void OnTextValueChanged_SetsInvalid_AndErrorMessage_OnUnparseableInput()
    {
        // Arrange
        var cut = TestContext.Render<TwTimePicker>();
        var input = cut.Find("input[type='text']");

        // Act
        input.Change("not-a-time");

        // Assert
        Assert.True(cut.Instance.Invalid);
        Assert.Equal("Enter a valid time", cut.Instance.ErrorMessage);
    }

    [Fact]
    public void OnTextValueChanged_ClearsInvalid_OnValidInput()
    {
        // Arrange - start from an invalid state left over from a previous bad entry.
        var cut = TestContext.Render<TwTimePicker>();
        var input = cut.Find("input[type='text']");
        input.Change("not-a-time");
        Assert.True(cut.Instance.Invalid);

        // Act
        input.Change("15:45");

        // Assert
        Assert.False(cut.Instance.Invalid);
        Assert.Equal(string.Empty, cut.Instance.ErrorMessage);
        Assert.Equal(new TimeOnly(15, 45), cut.Instance.SelectedTime);
    }

    [Fact]
    public void OnTextValueChanged_WhitespaceInput_DoesNotInvokeSelectedTimeChanged_OrSetInvalid()
    {
        // Arrange
        TimeOnly? callbackTime = null;
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => callbackTime = t)));
        var input = cut.Find("input[type='text']");

        // Act
        input.Change("   ");

        // Assert
        Assert.Null(callbackTime);
        Assert.False(cut.Instance.Invalid);
    }

    [Fact]
    public void OnFocusAsync_CapturesFocusToken()
    {
        // Arrange
        TestContext.JSInterop.Setup<string?>("twDialog.captureFocus").SetResult("tw-focus-token");
        var cut = TestContext.Render<TwTimePicker>();

        // Act
        cut.Find("input[type='text']").Click();

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.captureFocus");
    }

    [Fact]
    public void OnAfterRender_TrapsFocus_WhenPanelOpens()
    {
        // Arrange
        var cut = TestContext.Render<TwTimePicker>();

        // Act
        cut.Find("input[type='text']").Click();

        // Assert - unlike TwDatePicker/TwColorPicker, focus deliberately stays on the input (a
        // typeable combobox) rather than moving into the panel, so twDialog.focusSurface is not
        // invoked here; it's only called from the clock icon's click handler.
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.trapFocus");
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.setBackgroundInert");
    }

    [Fact]
    public void Close_ReleasesPanelTrap_AndRestoresFocus_WhenPanelWasOpen()
    {
        // Arrange
        TestContext.JSInterop.Setup<string?>("twDialog.captureFocus").SetResult("tw-focus-token");
        var cut = TestContext.Render<TwTimePicker>();
        cut.Find("input[type='text']").Click();

        // Act
        cut.InvokeAsync(() => cut.Instance.Close());

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.releaseFocusTrap");
        Assert.Contains(TestContext.JSInterop.Invocations,
            i => i.Identifier == "twDialog.restoreFocus" && (string?)i.Arguments[0] == "tw-focus-token");
    }

    [Fact]
    public void OnPanelKeyDown_Escape_ClosesPanel_AndRestoresFocus()
    {
        // Arrange
        TestContext.JSInterop.Setup<string?>("twDialog.captureFocus").SetResult("tw-focus-token");
        var cut = TestContext.Render<TwTimePicker>();
        cut.Find("input[type='text']").Click();
        var panel = cut.Find("div[role='dialog']");

        // Act
        panel.KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Escape" });

        // Assert
        Assert.DoesNotContain("Choose time", cut.Markup);
        Assert.Contains(TestContext.JSInterop.Invocations,
            i => i.Identifier == "twDialog.restoreFocus" && (string?)i.Arguments[0] == "tw-focus-token");
    }

    [Fact]
    public void OnPanelKeyDown_NonEscapeKey_DoesNotClosePanel()
    {
        // Arrange
        var cut = TestContext.Render<TwTimePicker>();
        cut.Find("input[type='text']").Click();
        var panel = cut.Find("div[role='dialog']");

        // Act
        panel.KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });

        // Assert
        Assert.Contains("Choose time", cut.Markup);
    }

    #region Clock Icon Tests

    [Fact]
    public void OnIconClick_FocusesSurface_WhenNotDisabled()
    {
        // Arrange - covers OnIconClickAsync's non-disabled path (the twDialog.focusSurface JS call),
        // previously unreached since no existing test interacted with the decorative clock icon.
        var cut = TestContext.Render<TwTimePicker>(p => p.Add(x => x.PreferNativePicker, true));
        var icon = cut.Find("div[role='button']");

        // Act
        icon.Click();

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.focusSurface");
    }

    [Fact]
    public void OnIconClick_FocusesTriggerInput_NotTheRootOrTheIconItself()
    {
        // Arrange - regression test (reported: the clock icon didn't open the panel on click).
        // twDialog.focusSurface used to be called with the whole InputRoot, but the icon is rendered
        // ahead of the trigger <input> in DOM order and (role="button" tabindex="0") is itself
        // focusable - so scanning the root for the first focusable descendant found the icon that was
        // just clicked and refocused it, a no-op that never fired the input's focus event, so the
        // panel never opened. The JS call must target the trigger's actual <input> element instead.
        var cut = TestContext.Render<TwTimePicker>(p => p.Add(x => x.PreferNativePicker, true));
        var icon = cut.Find("div[role='button']");

        // Act
        icon.Click();

        // Assert
        var invocation = TestContext.JSInterop.Invocations.Single(i => i.Identifier == "twDialog.focusSurface");
        var passedRef = Assert.IsType<ElementReference>(invocation.Arguments[0]);

        var baseType = typeof(TwPopoverPickerComponentBase);
        var triggerInputRefProperty = baseType.GetProperty("triggerInputRef", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var inputRootField = baseType.GetField("InputRoot", BindingFlags.NonPublic | BindingFlags.Instance)!;

        var expectedRef = (ElementReference?)triggerInputRefProperty.GetValue(cut.Instance);
        var rootRef = ((TwInputRoot?)inputRootField.GetValue(cut.Instance))?.RootRef;

        Assert.NotNull(expectedRef);
        Assert.Equal(expectedRef!.Value.Id, passedRef.Id);
        Assert.NotEqual(rootRef?.Id, passedRef.Id);
    }

    [Fact]
    public void OnIconClick_DoesNothing_WhenDisabled()
    {
        // Arrange - covers OnIconClickAsync's `if (Disabled) return;` guard.
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.PreferNativePicker, true)
            .Add(x => x.Disabled, true));
        var icon = cut.Find("div[role='button']");

        // Act
        icon.Click();

        // Assert
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.focusSurface");
    }

    [Fact]
    public void OnIconKeyDown_Enter_FocusesSurface()
    {
        // Arrange - covers OnIconKeyDownAsync's `e.Key is "Enter" or " "` match branch (Enter case).
        var cut = TestContext.Render<TwTimePicker>(p => p.Add(x => x.PreferNativePicker, true));
        var icon = cut.Find("div[role='button']");

        // Act
        icon.KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.focusSurface");
    }

    [Fact]
    public void OnIconKeyDown_Space_FocusesSurface()
    {
        // Arrange - covers OnIconKeyDownAsync's match branch (Space case).
        var cut = TestContext.Render<TwTimePicker>(p => p.Add(x => x.PreferNativePicker, true));
        var icon = cut.Find("div[role='button']");

        // Act
        icon.KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = " " });

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.focusSurface");
    }

    [Fact]
    public void OnIconKeyDown_OtherKey_DoesNothing()
    {
        // Arrange - covers OnIconKeyDownAsync's non-matching-key branch (no forwarded call).
        var cut = TestContext.Render<TwTimePicker>(p => p.Add(x => x.PreferNativePicker, true));
        var icon = cut.Find("div[role='button']");

        // Act
        icon.KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Tab" });

        // Assert
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.focusSurface");
    }

    #endregion

    #region Focus Suppression And Text-Change-While-Open Tests

    [Fact]
    public void Focus_Alone_DoesNotOpenPanel_ButAClickDoes_BeforeAndAfterClosing()
    {
        // Arrange - the panel makes the rest of the page unreachable while it is open, so it must not open
        // just because focus arrived (tabbing through a form, or focus being restored after a close).
        TestContext.JSInterop.Setup<string?>("twDialog.captureFocus").SetResult("tw-focus-token");
        var cut = TestContext.Render<TwTimePicker>();

        // Act & Assert - the trigger does not even listen for focus
        Assert.Throws<MissingEventHandlerException>(() => cut.Find("input[type='text']").Focus());
        Assert.DoesNotContain("Choose time", cut.Markup);

        // Act - a click opens it
        cut.Find("input[type='text']").Click();
        Assert.Contains("Choose time", cut.Markup);

        // Act - closing restores focus to the trigger, which must leave the panel closed
        cut.InvokeAsync(() => cut.Instance.Close());
        Assert.DoesNotContain("Choose time", cut.Markup);

        // Act - and the next click opens it again
        cut.Find("input[type='text']").Click();

        // Assert
        Assert.Contains("Choose time", cut.Markup);
    }

    [Fact]
    public void OnTextValueChanged_WhileFocused_ReleasesPanelTrap_AndClosesPanel()
    {
        // Arrange - covers OnTextValueChanged's `if (isFocused) { await ReleasePanelTrapAsync(); }` branch,
        // previously unreached since existing text-change tests never focused the field first.
        var cut = TestContext.Render<TwTimePicker>();
        var input = cut.Find("input[type='text']");
        input.Click();
        Assert.Contains("Choose time", cut.Markup);

        // Act
        input.Change("15:45");

        // Assert
        Assert.DoesNotContain("Choose time", cut.Markup);
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.releaseFocusTrap");
    }

    #endregion

    #region Variant Tests

    [Fact]
    public void UsesGlobalDefaultVariant_WhenNotSet()
    {
        // Arrange - no Variant set on the component, so it must follow TwInputTheme.DefaultInputVariant
        // (inherited via TwBlazorInputComponentBase.effectiveVariant), even after the theme changes.
        inputTheme.DefaultInputVariant = InputVariant.Outlined;

        // Act
        var cut = TestContext.Render<TwTimePicker>();

        // Assert
        var input = cut.Find("input[type='text']");
        Assert.Contains(InputVariantBuilder.GetClasses(InputVariant.Outlined, inputTheme), input.GetAttribute("class"));
    }

    [Fact]
    public void ExplicitVariant_OverridesGlobalDefault()
    {
        // Arrange - the global default is Outlined, but this instance explicitly asks for Filled.
        inputTheme.DefaultInputVariant = InputVariant.Outlined;

        // Act
        var cut = TestContext.Render<TwTimePicker>(p => p
            .Add(x => x.Variant, InputVariant.Filled));

        // Assert
        var input = cut.Find("input[type='text']");
        Assert.Contains(InputVariantBuilder.GetClasses(InputVariant.Filled, inputTheme), input.GetAttribute("class"));
    }

    #endregion
}
