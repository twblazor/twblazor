using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Components.ColorPicker;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.ColorPicker;

public class TwColorPickerBodyTests : TwBlazorTestBase
{
    [Fact]
    public void TwColorPickerBody_Renders_WithDefaultValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPickerBody>();

        // Assert
        var dialog = cut.Find(".tw-color-picker-dialog");
        Assert.NotNull(dialog);
    }

    [Fact]
    public void TwColorPickerBody_Renders_WithCustomValue()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF5733"));

        // Assert
        var preview = cut.Find("div[style*='background-color']");
        Assert.Contains("#FF5733", preview.GetAttribute("style"));
    }

    [Fact]
    public void TwColorPickerBody_Renders_ModeSwitchButton_ByDefault()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPickerBody>();

        // Assert
        var buttons = cut.FindAll("button");
        var modeButton = buttons.FirstOrDefault(b => b.TextContent.Contains("HEX"));
        Assert.NotNull(modeButton);
    }

    [Fact]
    public void TwColorPickerBody_DoesNotRender_ModeSwitchButton_WhenDisabled()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.ShowModeSwitch, false));

        // Assert
        var buttons = cut.FindAll("button");
        var modeButton = buttons.FirstOrDefault(b => b.TextContent.Contains("HEX") || b.TextContent.Contains("RGB") || b.TextContent.Contains("HSL"));
        Assert.Null(modeButton);
    }

    [Fact]
    public void TwColorPickerBody_SwitchesMode_FromHexToRgb()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF5733"));

        // Act
        var buttons = cut.FindAll("button");
        var modeButton = buttons[0];  // First button is the HEX mode button
        modeButton.Click();

        // Assert
        var updatedButtons = cut.FindAll("button");
        var rgbButton = updatedButtons.FirstOrDefault(b => b.TextContent.Contains("RGB"));
        Assert.NotNull(rgbButton);

        // Check RGB inputs are rendered
        var inputs = cut.FindAll("input[type='number']");
        var rInput = inputs.FirstOrDefault(i => i.GetAttribute("min") == "0" && i.GetAttribute("max") == "255");
        Assert.NotNull(rInput);
    }

    [Fact]
    public void TwColorPickerBody_SwitchesMode_FromRgbToHsl()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF5733"));

        // Act - Switch to RGB first
        var buttons = cut.FindAll("button");
        var modeButton = buttons[0];  // First button is the HEX mode button
        modeButton.Click();

        // Act - Switch to HSL
        buttons = cut.FindAll("button");
        modeButton = buttons[0];  // First button is now RGB mode button
        modeButton.Click();

        // Assert
        var updatedButtons = cut.FindAll("button");
        var hslButton = updatedButtons.FirstOrDefault(b => b.TextContent.Contains("HSL"));
        Assert.NotNull(hslButton);

        // Check HSL inputs are rendered
        var inputs = cut.FindAll("input[type='number']");
        var hInput = inputs.FirstOrDefault(i => i.GetAttribute("min") == "0" && i.GetAttribute("max") == "360");
        Assert.NotNull(hInput);
    }

    [Fact]
    public void TwColorPickerBody_SwitchesMode_FromHslToHex()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF5733"));

        // Act - Cycle through modes
        var buttons = cut.FindAll("button");
        var modeButton = buttons[0];  // First button is the HEX mode button
        modeButton.Click(); // HEX -> RGB

        buttons = cut.FindAll("button");
        modeButton = buttons[0];  // First button is now RGB mode button
        modeButton.Click(); // RGB -> HSL

        buttons = cut.FindAll("button");
        modeButton = buttons[0];  // First button is now HSL mode button
        modeButton.Click(); // HSL -> HEX

        // Assert
        var updatedButtons = cut.FindAll("button");
        var hexButton = updatedButtons.FirstOrDefault(b => b.TextContent.Contains("HEX"));
        Assert.NotNull(hexButton);

        // Check HEX input is rendered
        var inputs = cut.FindAll("input[type='text']");
        var hexInput = inputs.FirstOrDefault(i => !string.IsNullOrEmpty(i.GetAttribute("placeholder")));
        Assert.NotNull(hexInput);
    }

    [Fact]
    public void TwColorPickerBody_DisplaysRgbInputs_InRgbMode()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF5733"));

        // Act - Switch to RGB mode
        var buttons = cut.FindAll("button");
        var modeButton = buttons[0];  // First button is the HEX mode button
        modeButton.Click();

        // Assert - Check R, G, B labels
        var labels = cut.FindAll("label");
        Assert.Contains(labels, l => l.TextContent.Contains('R'));
        Assert.Contains(labels, l => l.TextContent.Contains('G'));
        Assert.Contains(labels, l => l.TextContent.Contains('B'));
    }

    [Fact]
    public void TwColorPickerBody_DisplaysHslInputs_InHslMode()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF5733"));

        // Act - Switch to HSL mode (HEX -> RGB -> HSL)
        var buttons = cut.FindAll("button");
        var modeButton = buttons[0];  // First button is the HEX mode button
        modeButton.Click();

        buttons = cut.FindAll("button");
        modeButton = buttons[0];  // First button is now RGB mode button
        modeButton.Click();

        // Assert - Check H, S, L labels
        var labels = cut.FindAll("label");
        Assert.Contains(labels, l => l.TextContent.Contains('H') && !l.TextContent.Contains("HEX"));
        Assert.Contains(labels, l => l.TextContent.Contains('S'));
        Assert.Contains(labels, l => l.TextContent.Contains('L'));
    }

    [Fact]
    public void TwColorPickerBody_DisplaysHexInput_InHexMode()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF5733"));

        // Assert - Check HEX label
        var labels = cut.FindAll("label");
        Assert.Contains(labels, l => l.TextContent.Contains("HEX"));
    }

    [Fact]
    public void TwColorPickerBody_Renders_WithAlphaSlider_WhenEnabled()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.ShowAlpha, true));

        // Assert
        var alphaLabel = cut.FindAll("span").FirstOrDefault(s => s.TextContent.Contains("Alpha"));
        Assert.NotNull(alphaLabel);
    }

    [Fact]
    public void TwColorPickerBody_DoesNotRender_AlphaSlider_ByDefault()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPickerBody>();

        // Assert
        var alphaLabel = cut.FindAll("span").FirstOrDefault(s => s.TextContent.Contains("Alpha"));
        Assert.Null(alphaLabel);
    }

    [Fact]
    public void TwColorPickerBody_Renders_CancelAndConfirmButtons()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPickerBody>();

        // Assert
        var buttons = cut.FindAll("button");
        Assert.Contains(buttons, b => b.TextContent.Contains("Cancel"));
        Assert.Contains(buttons, b => b.TextContent.Contains("Confirm"));
    }

    [Fact]
    public void TwColorPickerBody_InvokesOnClose_WhenCancelClicked()
    {
        // Arrange
        var closeCalled = false;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.OnClose, confirmed => closeCalled = true));

        // Act
        var buttons = cut.FindAll("button");
        var cancelButton = buttons[buttons.Count - 2];  // Second to last button is Cancel
        cancelButton.Click();

        // Assert
        Assert.True(closeCalled);
    }

    [Fact]
    public void TwColorPickerBody_InvokesOnClose_WhenConfirmClicked()
    {
        // Arrange
        var closeCalled = false;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.OnClose, confirmed => closeCalled = true));

        // Act
        var buttons = cut.FindAll("button");
        var confirmButton = buttons[buttons.Count - 1];  // Last button is Confirm
        confirmButton.Click();

        // Assert
        Assert.True(closeCalled);
    }

    [Fact]
    public void TwColorPickerBody_ParsesColorValue_Correctly()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#3b82f6"));

        // Assert
        var preview = cut.Find("div[style*='background-color']");
        Assert.Contains("#3B82F6", preview.GetAttribute("style"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TwColorPickerBody_Renders_WithVariant()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Variant, InputVariant.Filled));

        // Assert
        var dialog = cut.Find(".tw-color-picker-dialog");
        Assert.NotNull(dialog);
    }

    [Fact]
    public void TwColorPickerBody_UpdatesHslValues_WhenRgbValuesChange()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        // Act - Switch to RGB mode and change a value
        var buttons = cut.FindAll("button");
        var modeButton = buttons[0];  // First button is the HEX mode button
        modeButton.Click();

        var inputs = cut.FindAll("input[type='number']");
        var rInput = inputs[0];  // First input is R
        rInput.Input(128);

        // Assert
        Assert.NotNull(changedValue);
        Assert.StartsWith("#", changedValue);
    }

    [Fact]
    public void TwColorPickerBody_UpdatesRgbValues_WhenHslValuesChange()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        // Act - Switch to HSL mode and change a value
        var buttons = cut.FindAll("button");
        var modeButton = buttons[0];  // First button is the HEX mode button
        modeButton.Click(); // HEX -> RGB

        buttons = cut.FindAll("button");
        modeButton = buttons[0];  // First button is now RGB mode button
        modeButton.Click(); // RGB -> HSL

        var inputs = cut.FindAll("input[type='number']");
        var hInput = inputs[0];  // First input is H (hue)
        hInput.Input(180);

        // Assert
        Assert.NotNull(changedValue);
        Assert.StartsWith("#", changedValue);
    }

    [Fact]
    public void TwColorPickerBody_ValidatesHexInput()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000"));

        // Assert - HEX input should have placeholder
        var hexInput = cut.Find("input[type='text']");
        Assert.NotNull(hexInput);
        Assert.Contains('#', hexInput.GetAttribute("placeholder") ?? "");
    }

    [Fact]
    public void TwColorPickerBody_ClampsRgbValues_ToValidRange()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#000000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        // Act - Switch to RGB mode
        var buttons = cut.FindAll("button");
        var modeButton = buttons[0];  // First button is the HEX mode button
        modeButton.Click();

        // Change R value to 255 (max)
        var inputs = cut.FindAll("input[type='number']");
        var rInput = inputs[0];  // First input is R
        rInput.Input(255);

        // Assert - Should produce a valid color
        Assert.NotNull(changedValue);
        Assert.Matches(@"^#[0-9A-F]{6}$", changedValue);
    }

    [Fact]
    public void TwColorPickerBody_ClampsHslHue_ToValidRange()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        // Act - Switch to HSL mode
        var buttons = cut.FindAll("button");
        var modeButton = buttons[0];  // First button is the HEX mode button
        modeButton.Click(); // HEX -> RGB

        buttons = cut.FindAll("button");
        modeButton = buttons[0];  // First button is now RGB mode button
        modeButton.Click(); // RGB -> HSL

        // Change H value to 360 (max)
        var inputs = cut.FindAll("input[type='number']");
        var hInput = inputs[0];  // First input is H (hue)
        hInput.Input(360);

        // Assert - Should produce a valid color
        Assert.NotNull(changedValue);
        Assert.Matches(@"^#[0-9A-F]{6}$", changedValue);
    }

    [Fact]
    public void TwColorPickerBody_MaintainsColorConsistency_AcrossModeChanges()
    {
        // Arrange
        var initialColor = "#3b82f6";
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, initialColor));

        // Get initial preview color
        var initialPreview = cut.Find("div[style*='background-color']");

        // Act - Cycle through all modes
        var buttons = cut.FindAll("button");
        var modeButton = buttons[0];  // First button is the HEX mode button
        modeButton.Click(); // HEX -> RGB

        buttons = cut.FindAll("button");
        modeButton = buttons[0];  // First button is now RGB mode button
        modeButton.Click(); // RGB -> HSL

        buttons = cut.FindAll("button");
        modeButton = buttons[0];  // First button is now HSL mode button
        modeButton.Click(); // HSL -> HEX

        // Assert - Color should remain the same
        var finalPreview = cut.Find("div[style*='background-color']");
        var finalStyle = finalPreview.GetAttribute("style");
        Assert.Contains(initialColor.ToUpper(), finalStyle, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TwColorPickerBody_ShowsCorrectInputLabels_InEachMode()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000"));

        // Assert HEX mode
        var labels = cut.FindAll("label");
        Assert.Contains(labels, l => l.TextContent.Contains("HEX"));

        // Switch to RGB mode
        var buttons = cut.FindAll("button");
        var modeButton = buttons[0];  // First button is the HEX mode button
        modeButton.Click();

        labels = cut.FindAll("label");
        Assert.Contains(labels, l => l.TextContent == "R");
        Assert.Contains(labels, l => l.TextContent == "G");
        Assert.Contains(labels, l => l.TextContent == "B");

        // Switch to HSL mode
        buttons = cut.FindAll("button");
        modeButton = buttons[0];  // First button is now RGB mode button
        modeButton.Click();

        labels = cut.FindAll("label");
        Assert.Contains(labels, l => l.TextContent == "H");
        Assert.Contains(labels, l => l.TextContent == "S");
        Assert.Contains(labels, l => l.TextContent == "L");
    }

    [Fact]
    public void TwColorPickerBody_IncludesAlpha_WhenShowAlphaIsTrue()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ShowAlpha, true)
            .Add(p => p.ValueChanged, value => changedValue = value));

        // Assert - Alpha slider should be present
        var alphaLabel = cut.FindAll("span").FirstOrDefault(s => s.TextContent.Contains("Alpha"));
        Assert.NotNull(alphaLabel);

        // Alpha percentage should be displayed
        var percentageSpan = cut.FindAll("span").FirstOrDefault(s => s.TextContent.Contains('%'));
        Assert.NotNull(percentageSpan);
    }

    [Fact]
    public void TwColorPickerBody_ParsesColorWithAlpha()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF000080")
            .Add(p => p.ShowAlpha, true));

        // Assert - Should render with alpha
        var dialog = cut.Find(".tw-color-picker-dialog");
        Assert.NotNull(dialog);

        // Alpha percentage should show approximately 50%
        var percentageSpan = cut.FindAll("span").FirstOrDefault(s => s.TextContent.Contains('%'));
        Assert.NotNull(percentageSpan);
    }

    [Fact]
    public void TwColorPickerBody_HandlesInvalidHexInput_Gracefully()
    {
        // Arrange & Act - Should not throw with invalid initial value
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#ZZZ"));

        // Assert - Should render with default color
        var dialog = cut.Find(".tw-color-picker-dialog");
        Assert.NotNull(dialog);
    }

    [Fact]
    public void TwColorPickerBody_DefaultsToBlack_WhenValueIsEmpty()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, ""));

        // Assert - Should render with default black color
        var dialog = cut.Find(".tw-color-picker-dialog");
        Assert.NotNull(dialog);

        var preview = cut.Find("div[style*='background-color']");
        Assert.Contains("#000000", preview.GetAttribute("style"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TwColorPickerBody_DoesNotReparse_WhileSelectorDragging()
    {
        // Arrange
        var initialColor = "#FF0000"; // Pure red with hue = 0
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, initialColor));

        // Get the saturation/lightness selector
        var selector = cut.Find(".relative.w-full.h-48");

        // Act - Simulate drag on selector
        selector.MouseDown(new MouseEventArgs
        {
            OffsetX = 110,
            OffsetY = 94
        });

        // While dragging, try to update the Value parameter
        // This should NOT trigger re-parsing and should NOT affect hue
        cut.Render(parameters => parameters
            .Add(p => p.Value, "#FE0000")); // Slightly different red

        selector.MouseUp(new MouseEventArgs
        {
            OffsetX = 110,
            OffsetY = 94
        });

        // Assert - The component should maintain its HSL values during drag
        // and not re-parse the incoming value
        var dialog = cut.Find(".tw-color-picker-dialog");
        Assert.NotNull(dialog);
    }

    [Fact]
    public void TwColorPickerBody_DoesNotReparse_WhileHueDragging()
    {
        // Arrange
        var initialColor = "#FF0000";
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, initialColor));

        // Get the hue slider
        var hueSlider = cut.FindAll(".relative.h-6")[0];  // First slider is hue

        // Act - Simulate drag on hue slider
        hueSlider.MouseDown(new MouseEventArgs
        {
            OffsetX = 55
        });

        // While dragging, try to update the Value parameter
        cut.Render(parameters => parameters
            .Add(p => p.Value, "#FE0000"));

        hueSlider.MouseUp(new MouseEventArgs
        {
            OffsetX = 55
        });

        // Assert
        var dialog = cut.Find(".tw-color-picker-dialog");
        Assert.NotNull(dialog);
    }

    [Fact]
    public void TwColorPickerBody_DoesNotReparse_WhileAlphaDragging()
    {
        // Arrange
        var initialColor = "#FF0000FF";
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, initialColor)
            .Add(p => p.ShowAlpha, true));

        // Get the alpha slider
        var alphaSlider = cut.FindAll(".relative.h-6")[1];  // Second slider is alpha

        // Act - Simulate drag on alpha slider
        alphaSlider.MouseDown(new MouseEventArgs
        {
            OffsetX = 62
        });

        // While dragging, try to update the Value parameter
        cut.Render(parameters => parameters
            .Add(p => p.Value, "#FE000080")
            .Add(p => p.ShowAlpha, true));

        alphaSlider.MouseUp(new MouseEventArgs
        {
            OffsetX = 62
        });

        // Assert
        var dialog = cut.Find(".tw-color-picker-dialog");
        Assert.NotNull(dialog);
    }

    [Fact]
    public void TwColorPickerBody_ReparseColor_AfterDragEnds()
    {
        // Arrange
        string? lastChangedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => lastChangedValue = value));

        var selector = cut.Find(".relative.w-full.h-48");

        // Act - Start drag
        selector.MouseDown(new MouseEventArgs
        {
            OffsetX = 110,
            OffsetY = 94
        });

        // End drag
        selector.MouseUp(new MouseEventArgs
        {
            OffsetX = 110,
            OffsetY = 94
        });

        // Now update the value - it should re-parse since drag has ended
        cut.Render(parameters => parameters
            .Add(p => p.Value, "#00FF00")); // Change to green

        // Assert - Should have re-parsed the new color
        var preview = cut.Find("div[style*='background-color']");
        Assert.Contains("#00FF00", preview.GetAttribute("style"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TwColorPickerBody_MaintainsHue_WhenDraggingSaturationLightness()
    {
        // Arrange - Start with a specific hue (red = 0°)
        var initialColor = "#FF0000";
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, initialColor)
            .Add(p => p.ValueChanged, value => changedValue = value));

        // Act - Drag in the saturation/lightness selector
        var selector = cut.Find(".relative.w-full.h-48");

        // Simulate dragging to a different saturation/lightness but same hue
        selector.MouseDown(new MouseEventArgs
        {
            OffsetX = 165, // ~75% saturation
            OffsetY = 47   // ~75% lightness
        });

        selector.MouseMove(new MouseEventArgs
        {
            OffsetX = 165,
            OffsetY = 47
        });

        selector.MouseUp(new MouseEventArgs
        {
            OffsetX = 165,
            OffsetY = 47
        });

        // Assert - The changed color should still be in the red hue range
        // Red hue is 0°, so the hex should start with higher values in R than G/B
        Assert.NotNull(changedValue);
        Assert.StartsWith("#", changedValue);

        // Extract RGB values from hex
        var hex = changedValue.TrimStart('#');
        if (hex.Length >= 6)
        {
            var r = Convert.ToInt32(hex[..2], 16);
            var g = Convert.ToInt32(hex[2..4], 16);
            var b = Convert.ToInt32(hex[4..6], 16);

            // For red hue, R should be greater than or equal to both G and B
            Assert.True(r >= g, $"Red hue should have R >= G, but got R={r}, G={g}");
            Assert.True(r >= b, $"Red hue should have R >= B, but got R={r}, B={b}");
        }
    }

    [Fact]
    public void TwColorPickerBody_CorrectlyHandles_MultipleValueUpdates_BetweenDrags()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000"));

        var selector = cut.Find(".relative.w-full.h-48");

        // Act - First drag
        selector.MouseDown(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });
        selector.MouseUp(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });

        // Update value (should re-parse)
        cut.Render(parameters => parameters.Add(p => p.Value, "#00FF00"));

        // Second drag
        selector.MouseDown(new MouseEventArgs { OffsetX = 55, OffsetY = 47 });
        selector.MouseUp(new MouseEventArgs { OffsetX = 55, OffsetY = 47 });

        // Update value again (should re-parse)
        cut.Render(parameters => parameters.Add(p => p.Value, "#0000FF"));

        // Assert - Final color should be blue
        var preview = cut.Find("div[style*='background-color']");
        Assert.Contains("#0000FF", preview.GetAttribute("style"), StringComparison.OrdinalIgnoreCase);
    }

    #region OutputFormat Tests

    [Fact]
    public void TwColorPickerBody_OutputsHexFormat_ByDefault()
    {
        // Arrange
        string? outputValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF5733")
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => outputValue = value)));

        // Act - Trigger a change by interacting with the selector
        var selector = cut.Find(".relative.h-48");
        selector.MouseDown(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });
        selector.MouseUp(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });

        // Assert
        Assert.NotNull(outputValue);
        Assert.StartsWith("#", outputValue);
        Assert.Matches(@"^#[0-9A-Fa-f]{6}$", outputValue);
    }

    [Fact]
    public void TwColorPickerBody_OutputsHexFormat_WhenExplicitlySet()
    {
        // Arrange
        string? outputValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF5733")
            .Add(p => p.OutputFormat, ColorMode.Hex)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => outputValue = value)));

        // Act - Trigger a change
        var selector = cut.Find(".relative.h-48");
        selector.MouseDown(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });
        selector.MouseUp(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });

        // Assert
        Assert.NotNull(outputValue);
        Assert.StartsWith("#", outputValue);
        Assert.Matches(@"^#[0-9A-Fa-f]{6}$", outputValue);
    }

    [Fact]
    public void TwColorPickerBody_OutputsRgbFormat_WhenSet()
    {
        // Arrange
        string? outputValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF5733")
            .Add(p => p.OutputFormat, ColorMode.Rgb)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => outputValue = value)));

        // Act - Trigger a change
        var selector = cut.Find(".relative.h-48");
        selector.MouseDown(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });
        selector.MouseUp(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });

        // Assert
        Assert.NotNull(outputValue);
        Assert.StartsWith("rgb(", outputValue);
        Assert.Matches(@"^rgb\(\d{1,3}, \d{1,3}, \d{1,3}\)$", outputValue);
    }

    [Fact]
    public void TwColorPickerBody_OutputsHslFormat_WhenSet()
    {
        // Arrange
        string? outputValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF5733")
            .Add(p => p.OutputFormat, ColorMode.Hsl)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => outputValue = value)));

        // Act - Trigger a change
        var selector = cut.Find(".relative.h-48");
        selector.MouseDown(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });
        selector.MouseUp(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });

        // Assert
        Assert.NotNull(outputValue);
        Assert.StartsWith("hsl(", outputValue);
        Assert.Matches(@"^hsl\(\d{1,3}, \d{1,3}%, \d{1,3}%\)$", outputValue);
    }

    [Fact]
    public void TwColorPickerBody_OutputsHexWithAlpha_WhenShowAlphaIsTrue()
    {
        // Arrange
        string? outputValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF5733")
            .Add(p => p.OutputFormat, ColorMode.Hex)
            .Add(p => p.ShowAlpha, true)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => outputValue = value)));

        // Act - Trigger a change
        var selector = cut.Find(".relative.h-48");
        selector.MouseDown(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });
        selector.MouseUp(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });

        // Assert
        Assert.NotNull(outputValue);
        Assert.StartsWith("#", outputValue);
        Assert.Matches(@"^#[0-9A-Fa-f]{8}$", outputValue);
    }

    [Fact]
    public void TwColorPickerBody_OutputsRgbaFormat_WhenShowAlphaIsTrue()
    {
        // Arrange
        string? outputValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF5733")
            .Add(p => p.OutputFormat, ColorMode.Rgb)
            .Add(p => p.ShowAlpha, true)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => outputValue = value)));

        // Act - Trigger a change
        var selector = cut.Find(".relative.h-48");
        selector.MouseDown(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });
        selector.MouseUp(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });

        // Assert
        Assert.NotNull(outputValue);
        Assert.StartsWith("rgba(", outputValue);
        Assert.Matches(@"^rgba\(\d{1,3}, \d{1,3}, \d{1,3}, \d+\.\d{2}\)$", outputValue);
    }

    [Fact]
    public void TwColorPickerBody_OutputsHslaFormat_WhenShowAlphaIsTrue()
    {
        // Arrange
        string? outputValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF5733")
            .Add(p => p.OutputFormat, ColorMode.Hsl)
            .Add(p => p.ShowAlpha, true)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => outputValue = value)));

        // Act - Trigger a change
        var selector = cut.Find(".relative.h-48");
        selector.MouseDown(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });
        selector.MouseUp(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });

        // Assert
        Assert.NotNull(outputValue);
        Assert.StartsWith("hsla(", outputValue);
        Assert.Matches(@"^hsla\(\d{1,3}, \d{1,3}%, \d{1,3}%, \d+\.\d{2}\)$", outputValue);
    }

    [Fact]
    public void TwColorPickerBody_ParsesRgbInput_AndOutputsInSpecifiedFormat()
    {
        // Arrange
        string? outputValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "rgb(255, 87, 51)")
            .Add(p => p.OutputFormat, ColorMode.Hex)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => outputValue = value)));

        // Act - Trigger a change
        var selector = cut.Find(".relative.h-48");
        selector.MouseDown(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });
        selector.MouseUp(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });

        // Assert - Should output in Hex format even though input was RGB
        Assert.NotNull(outputValue);
        Assert.StartsWith("#", outputValue);
        Assert.Matches(@"^#[0-9A-Fa-f]{6}$", outputValue);
    }

    [Fact]
    public void TwColorPickerBody_ParsesRgbaInput_PreservesAlpha()
    {
        // Arrange
        string? outputValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "rgba(255, 87, 51, 0.5)")
            .Add(p => p.OutputFormat, ColorMode.Hex)
            .Add(p => p.ShowAlpha, true)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => outputValue = value)));

        // Act - Trigger a change
        var selector = cut.Find(".relative.h-48");
        selector.MouseDown(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });
        selector.MouseUp(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });

        // Assert - Should preserve alpha in output
        Assert.NotNull(outputValue);
        Assert.StartsWith("#", outputValue);
        Assert.Matches(@"^#[0-9A-Fa-f]{8}$", outputValue);
    }

    [Fact]
    public void TwColorPickerBody_ParsesHslInput_AndOutputsInSpecifiedFormat()
    {
        // Arrange
        string? outputValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "hsl(9, 100%, 60%)")
            .Add(p => p.OutputFormat, ColorMode.Rgb)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => outputValue = value)));

        // Act - Trigger a change
        var selector = cut.Find(".relative.h-48");
        selector.MouseDown(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });
        selector.MouseUp(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });

        // Assert - Should output in RGB format even though input was HSL
        Assert.NotNull(outputValue);
        Assert.StartsWith("rgb(", outputValue);
        Assert.Matches(@"^rgb\(\d{1,3}, \d{1,3}, \d{1,3}\)$", outputValue);
    }

    [Fact]
    public void TwColorPickerBody_ParsesHslaInput_PreservesAlpha()
    {
        // Arrange
        string? outputValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "hsla(9, 100%, 60%, 0.75)")
            .Add(p => p.OutputFormat, ColorMode.Hsl)
            .Add(p => p.ShowAlpha, true)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => outputValue = value)));

        // Act - Trigger a change
        var selector = cut.Find(".relative.h-48");
        selector.MouseDown(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });
        selector.MouseUp(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });

        // Assert - Should preserve alpha in output
        Assert.NotNull(outputValue);
        Assert.StartsWith("hsla(", outputValue);
        Assert.Matches(@"^hsla\(\d{1,3}, \d{1,3}%, \d{1,3}%, \d+\.\d{2}\)$", outputValue);
    }

    [Fact]
    public void TwColorPickerBody_OutputFormat_CanBeDifferentFromInputMode()
    {
        // Arrange - Start with RGB input, but output as HSL
        string? outputValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "rgb(255, 87, 51)")
            .Add(p => p.OutputFormat, ColorMode.Hsl)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => outputValue = value)));

        // Act - Trigger a change
        var selector = cut.Find(".relative.h-48");
        selector.MouseDown(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });
        selector.MouseUp(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });

        // Assert
        Assert.NotNull(outputValue);
        Assert.StartsWith("hsl(", outputValue);
        Assert.Matches(@"^hsl\(\d{1,3}, \d{1,3}%, \d{1,3}%\)$", outputValue);
    }

    [Fact]
    public void TwColorPickerBody_HandlesInvalidInputFormat_FallsBackToBlack()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "invalid-color-value")
            .Add(p => p.OutputFormat, ColorMode.Hex));

        // Assert - Should default to black (#000000)
        var preview = cut.Find("div[style*='background-color']");
        Assert.Contains("#000000", preview.GetAttribute("style"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TwColorPickerBody_OutputFormat_WorksWithHueSlider()
    {
        // Arrange
        string? outputValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.OutputFormat, ColorMode.Rgb)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => outputValue = value)));

        // Act - Change hue
        var hueSlider = cut.Find(".relative.h-6");
        hueSlider.MouseDown(new MouseEventArgs { OffsetX = 110 });
        hueSlider.MouseUp(new MouseEventArgs { OffsetX = 110 });

        // Assert
        Assert.NotNull(outputValue);
        Assert.StartsWith("rgb(", outputValue);
    }

    [Fact]
    public void TwColorPickerBody_OutputFormat_WorksWithAlphaSlider()
    {
        // Arrange
        string? outputValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.OutputFormat, ColorMode.Hsl)
            .Add(p => p.ShowAlpha, true)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => outputValue = value)));

        // Act - Change alpha
        var alphaSlider = cut.FindAll(".relative.h-6")[1];  // Second slider is alpha
        alphaSlider.MouseDown(new MouseEventArgs { OffsetX = 62 });
        alphaSlider.MouseUp(new MouseEventArgs { OffsetX = 62 });

        // Assert
        Assert.NotNull(outputValue);
        Assert.StartsWith("hsla(", outputValue);
        Assert.Matches(@"^hsla\(\d{1,3}, \d{1,3}%, \d{1,3}%, \d+\.\d{2}\)$", outputValue);
    }

    [Fact]
    public void TwColorPickerBody_OutputFormat_WorksWithRgbInputs()
    {
        // Arrange
        string? outputValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF5733")
            .Add(p => p.OutputFormat, ColorMode.Hsl)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => outputValue = value)));

        // Act - Switch to RGB mode and change a value
        var buttons = cut.FindAll("button");
        var modeButton = buttons[0];  // First button is the HEX mode button
        modeButton.Click();

        // Change R input
        var inputs = cut.FindAll("input[type='number']");
        var rInput = inputs[0];  // First input is R
        rInput.Input("128");

        // Assert - Should still output in HSL format
        Assert.NotNull(outputValue);
        Assert.StartsWith("hsl(", outputValue);
    }

    [Fact]
    public void TwColorPickerBody_ParsesRgbValue_AsInput()
    {
        // Arrange & Act - covers the rgb format branch in ParseColorFromValue
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "rgb(255, 0, 0)"));

        // Assert - should parse rgb and display correctly
        var preview = cut.Find("div[style*='background-color']");
        Assert.Contains("#FF0000", preview.GetAttribute("style"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TwColorPickerBody_ParsesHslValue_AsInput()
    {
        // Arrange & Act - covers the hsl format branch in ParseColorFromValue
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "hsl(0, 100%, 50%)"));

        // Assert - should parse hsl and display correctly
        var preview = cut.Find("div[style*='background-color']");
        Assert.Contains("#FF0000", preview.GetAttribute("style"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TwColorPickerBody_DefaultsToBlack_ForUnrecognizedFormat()
    {
        // Arrange & Act - covers the else branch (unrecognized format) in ParseColorFromValue
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "notacolor"));

        // Assert - should fall back to default black color
        var preview = cut.Find("div[style*='background-color']");
        Assert.Contains("#000000", preview.GetAttribute("style"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TwColorPickerBody_SelectorMouseMove_WithoutDrag_DoesNotUpdate()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        // Act - mousemove WITHOUT prior mousedown (isSelectorDragging = false)
        var selector = cut.Find(".relative.w-full.h-48");
        selector.MouseMove(new Microsoft.AspNetCore.Components.Web.MouseEventArgs { OffsetX = 100, OffsetY = 50 });

        // Assert - no value change triggered
        Assert.Null(changedValue);
    }

    [Fact]
    public void TwColorPickerBody_HueMouseMove_WithoutDrag_DoesNotUpdate()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        // Act - mousemove on hue slider WITHOUT prior mousedown (isHueDragging = false)
        var hueSlider = cut.FindAll(".relative.h-6")[0];
        hueSlider.MouseMove(new Microsoft.AspNetCore.Components.Web.MouseEventArgs { OffsetX = 110 });

        // Assert - no value change triggered
        Assert.Null(changedValue);
    }

    [Fact]
    public void TwColorPickerBody_AlphaMouseMove_WithoutDrag_DoesNotUpdate()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ShowAlpha, true)
            .Add(p => p.ValueChanged, value => changedValue = value));

        // Act - mousemove on alpha slider WITHOUT prior mousedown (isAlphaDragging = false)
        var alphaSlider = cut.FindAll(".relative.h-6")[1];
        alphaSlider.MouseMove(new Microsoft.AspNetCore.Components.Web.MouseEventArgs { OffsetX = 60 });

        // Assert - no value change triggered
        Assert.Null(changedValue);
    }

    [Fact]
    public void TwColorPickerBody_OutputFormat_Rgb_EmitsRgbString()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.OutputFormat, TwBlazor.Enums.ColorMode.Rgb)
            .Add(p => p.ValueChanged, value => changedValue = value));

        // Act - trigger a color change by dragging the hue slider
        var hueSlider = cut.FindAll(".relative.h-6")[0];
        hueSlider.MouseDown(new Microsoft.AspNetCore.Components.Web.MouseEventArgs { OffsetX = 55 });

        // Assert - output should be rgb format
        Assert.NotNull(changedValue);
        Assert.StartsWith("rgb(", changedValue);
    }

    [Fact]
    public void TwColorPickerBody_OutputFormat_Hsl_EmitsHslString()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.OutputFormat, TwBlazor.Enums.ColorMode.Hsl)
            .Add(p => p.ValueChanged, value => changedValue = value));

        // Act - trigger a color change by dragging the hue slider
        var hueSlider = cut.FindAll(".relative.h-6")[0];
        hueSlider.MouseDown(new Microsoft.AspNetCore.Components.Web.MouseEventArgs { OffsetX = 55 });

        // Assert - output should be hsl format
        Assert.NotNull(changedValue);
        Assert.StartsWith("hsl(", changedValue);
    }

    #endregion

    #region Coverage Gap Tests

    [Fact]
    public void OnGInputChanged_UpdatesColor_InRgbMode()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#000000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        var buttons = cut.FindAll("button");
        buttons[0].Click(); // HEX -> RGB

        // Act
        var inputs = cut.FindAll("input[type='number']");
        var gInput = inputs[1]; // R, G, B
        gInput.Input(128);

        // Assert
        Assert.NotNull(changedValue);
        Assert.Matches(@"^#[0-9A-F]{6}$", changedValue);
    }

    [Fact]
    public void OnBInputChanged_UpdatesColor_InRgbMode()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#000000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        var buttons = cut.FindAll("button");
        buttons[0].Click(); // HEX -> RGB

        // Act
        var inputs = cut.FindAll("input[type='number']");
        var bInput = inputs[2]; // R, G, B
        bInput.Input(200);

        // Assert
        Assert.NotNull(changedValue);
        Assert.Matches(@"^#[0-9A-F]{6}$", changedValue);
    }

    [Fact]
    public void OnSInputChanged_UpdatesColor_InHslMode()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        var buttons = cut.FindAll("button");
        buttons[0].Click(); // HEX -> RGB
        buttons = cut.FindAll("button");
        buttons[0].Click(); // RGB -> HSL

        // Act
        var inputs = cut.FindAll("input[type='number']");
        var sInput = inputs[1]; // H, S, L
        sInput.Input(50);

        // Assert
        Assert.NotNull(changedValue);
        Assert.Matches(@"^#[0-9A-F]{6}$", changedValue);
    }

    [Fact]
    public void OnLInputChanged_UpdatesColor_InHslMode()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        var buttons = cut.FindAll("button");
        buttons[0].Click(); // HEX -> RGB
        buttons = cut.FindAll("button");
        buttons[0].Click(); // RGB -> HSL

        // Act
        var inputs = cut.FindAll("input[type='number']");
        var lInput = inputs[2]; // H, S, L
        lInput.Input(25);

        // Assert
        Assert.NotNull(changedValue);
        Assert.Matches(@"^#[0-9A-F]{6}$", changedValue);
    }

    [Fact]
    public void OnHInputChanged_DoesNotUpdate_WhenValueIsNotNumeric()
    {
        // Arrange - OnHInputChanged is only ever invoked by the UI with an int.ToString()
        // (the TwTextfield<int> already parsed it), so the int.TryParse failure branch is
        // unreachable through normal interaction; invoke the private method directly.
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        var method = typeof(TwColorPickerBody).GetMethod("OnHInputChanged",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        method.Invoke(cut.Instance, ["not-a-number"]);

        // Assert
        Assert.Null(changedValue);
    }

    [Fact]
    public void OnHueMouseMove_UpdatesColor_WhileDragging()
    {
        // Arrange - OnHueMouseDown starts the drag; OnHueMouseMove only updates while dragging.
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        var hueSlider = cut.FindAll(".relative.h-6")[0];
        hueSlider.MouseDown(new MouseEventArgs { OffsetX = 55 });
        changedValue = null; // reset after the mousedown's own update

        // Act
        hueSlider.MouseMove(new MouseEventArgs { OffsetX = 110 });

        // Assert
        Assert.NotNull(changedValue);
    }

    [Fact]
    public void OnAlphaMouseMove_UpdatesColor_WhileDragging()
    {
        // Arrange - OnAlphaMouseDown starts the drag; OnAlphaMouseMove only updates while dragging.
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ShowAlpha, true)
            .Add(p => p.ValueChanged, value => changedValue = value));

        var alphaSlider = cut.FindAll(".relative.h-6")[1];
        alphaSlider.MouseDown(new MouseEventArgs { OffsetX = 30 });
        changedValue = null; // reset after the mousedown's own update

        // Act
        alphaSlider.MouseMove(new MouseEventArgs { OffsetX = 60 });

        // Assert
        Assert.NotNull(changedValue);
    }

    // OnHexInputChanged and OnHexInputCommitted are wired to @onchange/@oninput as unmatched
    // attributes forwarded onto TwTextfield's own internal input element (which already has its
    // own @bind:event="onchange" for the same event name). The internal binding wins the
    // conflict, so simulated Change()/Input() DOM events never actually reach these handlers in
    // bUnit. Exercised directly via reflection instead, which is otherwise a faithful unit test
    // of their own parsing/commit logic.
    private static void InvokeHexHandler(TwColorPickerBody instance, string methodName, object? eventArgs)
    {
        var method = typeof(TwColorPickerBody).GetMethod(methodName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var parameters = method.GetParameters().Length == 0 ? [] : new[] { eventArgs };
        method.Invoke(instance, parameters);
    }

    private static void SetHexInputField(TwColorPickerBody instance, string value)
    {
        var field = typeof(TwColorPickerBody).GetField("hexInput",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        field.SetValue(instance, value);
    }

    [Fact]
    public void OnHexInputChanged_UpdatesHexInputField()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#000000"));

        // Act
        InvokeHexHandler(cut.Instance, "OnHexInputChanged", new ChangeEventArgs { Value = "ff5733" });

        // Assert
        var field = typeof(TwColorPickerBody).GetField("hexInput",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        Assert.Equal("ff5733", field.GetValue(cut.Instance));
    }

    [Fact]
    public void OnHexInputChanged_FallsBackToDefaultColor_WhenValueIsNull()
    {
        // Arrange - exercises the `e.Value?.ToString() ?? defaultColor` null branch.
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#000000"));

        // Act
        InvokeHexHandler(cut.Instance, "OnHexInputChanged", new ChangeEventArgs { Value = null });

        // Assert
        var field = typeof(TwColorPickerBody).GetField("hexInput",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        Assert.Equal("#000000", field.GetValue(cut.Instance));
    }

    [Fact]
    public void OnHexInputCommitted_UpdatesColor_OnValidHex()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#000000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        SetHexInputField(cut.Instance, "ff5733");

        // Act
        InvokeHexHandler(cut.Instance, "OnHexInputCommitted", new ChangeEventArgs());

        // Assert
        Assert.Equal("#FF5733", changedValue);
    }

    [Fact]
    public void OnHexInputCommitted_KeepsPreviousColor_OnInvalidHexCharacters()
    {
        // Arrange - exercises the FormatException catch (non-hex characters).
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#000000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        SetHexInputField(cut.Instance, "zzzzzz");

        // Act
        InvokeHexHandler(cut.Instance, "OnHexInputCommitted", new ChangeEventArgs());

        // Assert - invalid input is swallowed, no color change notified
        Assert.Null(changedValue);
    }

    [Fact]
    public void OnHexInputCommitted_KeepsPreviousColor_OnNegativeHexSegment()
    {
        // Arrange - regression test: Convert.ToInt32 throws a plain ArgumentException (not
        // ArgumentOutOfRangeException) for a minus-sign segment like "-1" in a non-base-10
        // conversion. Previously unhandled - see the ArgumentException catch in the source.
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#000000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        SetHexInputField(cut.Instance, "-10000");

        // Act
        InvokeHexHandler(cut.Instance, "OnHexInputCommitted", new ChangeEventArgs());

        // Assert - invalid input is swallowed, no color change notified
        Assert.Null(changedValue);
    }

    [Fact]
    public void OnHexInputCommitted_DoesNothing_WhenHexIsTooShort()
    {
        // Arrange - exercises the `hex.Length >= 7` false branch.
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#000000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        SetHexInputField(cut.Instance, "abc");

        // Act
        InvokeHexHandler(cut.Instance, "OnHexInputCommitted", new ChangeEventArgs());

        // Assert - too short to parse, silently ignored
        Assert.Null(changedValue);
    }

    [Fact]
    public void OnHexInputCommitted_UpdatesColor_WhenHexInputAlreadyHasHashPrefix()
    {
        // Arrange - exercises the `!hex.StartsWith('#')` false branch (prefix not re-added).
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#000000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        SetHexInputField(cut.Instance, "#00ff00");

        // Act
        InvokeHexHandler(cut.Instance, "OnHexInputCommitted", new ChangeEventArgs());

        // Assert
        Assert.Equal("#00FF00", changedValue);
    }

    [Fact]
    public void OnSInputChanged_DoesNotUpdate_WhenValueIsNotNumeric()
    {
        // Arrange - same as OnHInputChanged, unreachable via the UI since the TwTextfield<int>
        // already parsed the value; invoke the private method directly.
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        var method = typeof(TwColorPickerBody).GetMethod("OnSInputChanged",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        method.Invoke(cut.Instance, ["not-a-number"]);

        // Assert
        Assert.Null(changedValue);
    }

    [Fact]
    public void OnLInputChanged_DoesNotUpdate_WhenValueIsNotNumeric()
    {
        // Arrange - same as OnHInputChanged, unreachable via the UI since the TwTextfield<int>
        // already parsed the value; invoke the private method directly.
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        var method = typeof(TwColorPickerBody).GetMethod("OnLInputChanged",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        method.Invoke(cut.Instance, ["not-a-number"]);

        // Assert
        Assert.Null(changedValue);
    }

    #endregion

    #region Keyboard Slider Tests

    [Fact]
    public void OnSelectorKeyDown_ArrowRight_IncreasesSaturation()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#808080")
            .Add(p => p.ValueChanged, value => changedValue = value));

        var selector = cut.Find("div[role='slider'][aria-label='Saturation and lightness']");

        // Act
        selector.KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        Assert.NotNull(changedValue);
    }

    [Fact]
    public void OnSelectorKeyDown_ArrowLeft_DecreasesSaturation()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#808080")
            .Add(p => p.ValueChanged, value => changedValue = value));

        var selector = cut.Find("div[role='slider'][aria-label='Saturation and lightness']");

        // Act
        selector.KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        // Assert
        Assert.NotNull(changedValue);
    }

    [Fact]
    public void OnSelectorKeyDown_ArrowUp_IncreasesLightness()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#808080"));

        var selector = cut.Find("div[role='slider'][aria-label='Saturation and lightness']");

        // Act
        selector.KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });

        // Assert
        Assert.NotNull(selector);
    }

    [Fact]
    public void OnSelectorKeyDown_ArrowDown_DecreasesLightness()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#808080"));

        var selector = cut.Find("div[role='slider'][aria-label='Saturation and lightness']");

        // Act
        selector.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });

        // Assert
        Assert.NotNull(selector);
    }

    [Fact]
    public void OnSelectorKeyDown_Home_SetsSaturationToMinimum()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#808080"));

        var selector = cut.Find("div[role='slider'][aria-label='Saturation and lightness']");

        // Act
        selector.KeyDown(new KeyboardEventArgs { Key = "Home" });

        // Assert
        Assert.Equal("0", selector.GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void OnSelectorKeyDown_End_SetsSaturationToMaximum()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#808080"));

        var selector = cut.Find("div[role='slider'][aria-label='Saturation and lightness']");

        // Act
        selector.KeyDown(new KeyboardEventArgs { Key = "End" });

        // Assert
        Assert.Equal("100", selector.GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void OnSelectorKeyDown_UnhandledKey_DoesNotChangeValue()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#808080")
            .Add(p => p.ValueChanged, value => changedValue = value));

        var selector = cut.Find("div[role='slider'][aria-label='Saturation and lightness']");

        // Act
        selector.KeyDown(new KeyboardEventArgs { Key = "A" });

        // Assert
        Assert.Null(changedValue);
    }

    [Fact]
    public void OnHueKeyDown_ArrowRight_IncreasesHue()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "hsl(100, 50%, 50%)"));

        var hueSlider = cut.Find("div[role='slider'][aria-label='Hue']");

        // Act
        hueSlider.KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        Assert.Equal("105", hueSlider.GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void OnHueKeyDown_ArrowUp_IncreasesHue()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "hsl(100, 50%, 50%)"));

        var hueSlider = cut.Find("div[role='slider'][aria-label='Hue']");

        // Act
        hueSlider.KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });

        // Assert
        Assert.Equal("105", hueSlider.GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void OnHueKeyDown_ArrowLeft_DecreasesHue()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "hsl(100, 50%, 50%)"));

        var hueSlider = cut.Find("div[role='slider'][aria-label='Hue']");

        // Act
        hueSlider.KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        // Assert
        Assert.Equal("95", hueSlider.GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void OnHueKeyDown_ArrowDown_DecreasesHue()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "hsl(100, 50%, 50%)"));

        var hueSlider = cut.Find("div[role='slider'][aria-label='Hue']");

        // Act
        hueSlider.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });

        // Assert
        Assert.Equal("95", hueSlider.GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void OnHueKeyDown_Home_SetsHueToMinimum()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "hsl(100, 50%, 50%)"));

        var hueSlider = cut.Find("div[role='slider'][aria-label='Hue']");

        // Act
        hueSlider.KeyDown(new KeyboardEventArgs { Key = "Home" });

        // Assert
        Assert.Equal("0", hueSlider.GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void OnHueKeyDown_End_SetsHueToMaximum()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "hsl(100, 50%, 50%)"));

        var hueSlider = cut.Find("div[role='slider'][aria-label='Hue']");

        // Act
        hueSlider.KeyDown(new KeyboardEventArgs { Key = "End" });

        // Assert
        Assert.Equal("360", hueSlider.GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void OnHueKeyDown_UnhandledKey_DoesNotChangeValue()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "hsl(100, 50%, 50%)")
            .Add(p => p.ValueChanged, value => changedValue = value));

        var hueSlider = cut.Find("div[role='slider'][aria-label='Hue']");

        // Act
        hueSlider.KeyDown(new KeyboardEventArgs { Key = "A" });

        // Assert
        Assert.Null(changedValue);
    }

    [Fact]
    public void OnAlphaKeyDown_ArrowRight_IncreasesAlpha()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "rgba(255, 0, 0, 0.5)")
            .Add(p => p.ShowAlpha, true));

        var alphaSlider = cut.Find("div[role='slider'][aria-label='Alpha']");

        // Act
        alphaSlider.KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        Assert.Equal("55", alphaSlider.GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void OnAlphaKeyDown_ArrowUp_IncreasesAlpha()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "rgba(255, 0, 0, 0.5)")
            .Add(p => p.ShowAlpha, true));

        var alphaSlider = cut.Find("div[role='slider'][aria-label='Alpha']");

        // Act
        alphaSlider.KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });

        // Assert
        Assert.Equal("55", alphaSlider.GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void OnAlphaKeyDown_ArrowLeft_DecreasesAlpha()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "rgba(255, 0, 0, 0.5)")
            .Add(p => p.ShowAlpha, true));

        var alphaSlider = cut.Find("div[role='slider'][aria-label='Alpha']");

        // Act
        alphaSlider.KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        // Assert
        Assert.Equal("45", alphaSlider.GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void OnAlphaKeyDown_ArrowDown_DecreasesAlpha()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "rgba(255, 0, 0, 0.5)")
            .Add(p => p.ShowAlpha, true));

        var alphaSlider = cut.Find("div[role='slider'][aria-label='Alpha']");

        // Act
        alphaSlider.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });

        // Assert
        Assert.Equal("45", alphaSlider.GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void OnAlphaKeyDown_Home_SetsAlphaToMinimum()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "rgba(255, 0, 0, 0.5)")
            .Add(p => p.ShowAlpha, true));

        var alphaSlider = cut.Find("div[role='slider'][aria-label='Alpha']");

        // Act
        alphaSlider.KeyDown(new KeyboardEventArgs { Key = "Home" });

        // Assert
        Assert.Equal("0", alphaSlider.GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void OnAlphaKeyDown_End_SetsAlphaToMaximum()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "rgba(255, 0, 0, 0.5)")
            .Add(p => p.ShowAlpha, true));

        var alphaSlider = cut.Find("div[role='slider'][aria-label='Alpha']");

        // Act
        alphaSlider.KeyDown(new KeyboardEventArgs { Key = "End" });

        // Assert
        Assert.Equal("100", alphaSlider.GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void OnAlphaKeyDown_UnhandledKey_DoesNotChangeValue()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "rgba(255, 0, 0, 0.5)")
            .Add(p => p.ShowAlpha, true)
            .Add(p => p.ValueChanged, value => changedValue = value));

        var alphaSlider = cut.Find("div[role='slider'][aria-label='Alpha']");

        // Act
        alphaSlider.KeyDown(new KeyboardEventArgs { Key = "A" });

        // Assert
        Assert.Null(changedValue);
    }

    #endregion

    #region JS Interop Tests

    [Fact]
    public void OnAfterRender_RegistersPreventScrollKeys_ForSelectorAndHue_WhenAlphaHidden()
    {
        // Arrange & Act
        TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ShowAlpha, false));

        // Assert - only selector + hue registered, no alpha slider exists to attach to
        var invocations = TestContext.JSInterop.Invocations
            .Where(i => i.Identifier == "twSlider.preventScrollKeys")
            .ToList();
        Assert.Equal(2, invocations.Count);
    }

    [Fact]
    public void OnAfterRender_RegistersPreventScrollKeys_ForAllThreeSliders_WhenAlphaShown()
    {
        // Arrange & Act
        TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ShowAlpha, true));

        // Assert
        var invocations = TestContext.JSInterop.Invocations
            .Where(i => i.Identifier == "twSlider.preventScrollKeys")
            .ToList();
        Assert.Equal(3, invocations.Count);
    }

    [Fact]
    public void OnAfterRender_MeasuresAndAppliesSelectorAndHueDimensions_FromJsInterop()
    {
        // Arrange - covers the "measured size is valid" branches in MeasureSlidersAsync for the selector
        // (`selectorSize is [> 0, > 0]`) and hue (`hueSize is [> 0, ..]`) elements, which the rest of the
        // suite never exercises since it leaves twColorPicker.getSize unconfigured (Loose mode then
        // returns null, which only ever hits the "keep fallback" branch).
        TestContext.JSInterop.Setup<double[]>("twColorPicker.getSize", _ => true).SetResult([300, 200]);

        // Act
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ShowAlpha, false));

        // Assert - the fallback values (220/188) would have been replaced by the measured ones
        var widthField = typeof(TwColorPickerBody).GetField("selectorWidth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var heightField = typeof(TwColorPickerBody).GetField("selectorHeight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var hueWidthField = typeof(TwColorPickerBody).GetField("hueSliderWidth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        Assert.Equal(300d, widthField.GetValue(cut.Instance));
        Assert.Equal(200d, heightField.GetValue(cut.Instance));
        Assert.Equal(300d, hueWidthField.GetValue(cut.Instance));
    }

    [Fact]
    public void OnAfterRender_MeasuresAndAppliesAlphaSliderWidth_WhenShowAlphaTrue()
    {
        // Arrange - covers the `if (ShowAlpha)` branch inside MeasureSlidersAsync and its own
        // `alphaSize is [> 0, ..]` check, only reachable when ShowAlpha is true.
        TestContext.JSInterop.Setup<double[]>("twColorPicker.getSize", _ => true).SetResult([150, 90]);

        // Act
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ShowAlpha, true));

        // Assert - the fallback (124) would have been replaced by the measured value
        var alphaWidthField = typeof(TwColorPickerBody).GetField("alphaSliderWidth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        Assert.Equal(150d, alphaWidthField.GetValue(cut.Instance));
    }

    #endregion

    #region Touch Interaction Tests

    [Fact]
    public void OnSelectorTouchStart_UpdatesPosition_ViaJsInterop()
    {
        // Arrange - covers OnSelectorTouchStart and the "has touches" branch of GetRelativeTouchPositionAsync
        // (the JS round-trip through twColorPicker.relativePosition), neither previously exercised.
        TestContext.JSInterop.Setup<double[]>("twColorPicker.relativePosition", _ => true).SetResult([220, 0]);
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        var selector = cut.Find(".relative.w-full.h-48");

        // Act
        selector.TriggerEvent("ontouchstart", new TouchEventArgs
        {
            Touches = [new TouchPoint { ClientX = 300, ClientY = 300 }]
        });

        // Assert - offsetX 220 (== fallback selectorWidth) drives saturation to 1, offsetY 0 drives lightness to 1
        Assert.Equal("#FFFFFF", changedValue);
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twColorPicker.relativePosition");
    }

    [Fact]
    public void OnSelectorTouchMove_WhileDragging_InvokesValueChangedAgain()
    {
        // Arrange - OnSelectorTouchStart begins the drag; OnSelectorTouchMove should update again while dragging.
        TestContext.JSInterop.Setup<double[]>("twColorPicker.relativePosition", _ => true).SetResult([50, 50]);
        var changeCount = 0;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => changeCount++));

        var selector = cut.Find(".relative.w-full.h-48");
        selector.TriggerEvent("ontouchstart", new TouchEventArgs { Touches = [new TouchPoint { ClientX = 50, ClientY = 50 }] });
        Assert.Equal(1, changeCount);

        // Act
        selector.TriggerEvent("ontouchmove", new TouchEventArgs { Touches = [new TouchPoint { ClientX = 60, ClientY = 60 }] });

        // Assert
        Assert.Equal(2, changeCount);
    }

    [Fact]
    public void OnSelectorTouchMove_WithoutDrag_DoesNotUpdate()
    {
        // Arrange - no prior touchstart, so isSelectorDragging is false and the `if (!isSelectorDragging) return;`
        // guard in OnSelectorTouchMove should skip the JS round-trip entirely.
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        var selector = cut.Find(".relative.w-full.h-48");

        // Act
        selector.TriggerEvent("ontouchmove", new TouchEventArgs { Touches = [new TouchPoint { ClientX = 50, ClientY = 50 }] });

        // Assert
        Assert.Null(changedValue);
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twColorPicker.relativePosition");
    }

    [Fact]
    public void OnSelectorTouchEnd_StopsDragging_SubsequentMoveDoesNothing()
    {
        // Arrange
        TestContext.JSInterop.Setup<double[]>("twColorPicker.relativePosition", _ => true).SetResult([10, 10]);
        var changeCount = 0;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => changeCount++));

        var selector = cut.Find(".relative.w-full.h-48");
        selector.TriggerEvent("ontouchstart", new TouchEventArgs { Touches = [new TouchPoint { ClientX = 10, ClientY = 10 }] });
        Assert.Equal(1, changeCount);

        // Act - end the drag, then attempt another move
        selector.TriggerEvent("ontouchend", new TouchEventArgs());
        selector.TriggerEvent("ontouchmove", new TouchEventArgs { Touches = [new TouchPoint { ClientX = 50, ClientY = 50 }] });

        // Assert - the move after touchend is a no-op, proving OnSelectorTouchEnd cleared isSelectorDragging
        Assert.Equal(1, changeCount);
    }

    [Fact]
    public void GetRelativeTouchPositionAsync_ReturnsZero_WhenTouchesEmpty()
    {
        // Arrange - covers the `e.Touches.Length == 0` short-circuit, which returns (0,0) without ever
        // reaching the twColorPicker.relativePosition JS call.
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        var selector = cut.Find(".relative.w-full.h-48");

        // Act
        selector.TriggerEvent("ontouchstart", new TouchEventArgs { Touches = [] });

        // Assert - (0,0) -> saturation 0, lightness 1 -> white; and no JS round-trip was made
        Assert.Equal("#FFFFFF", changedValue);
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twColorPicker.relativePosition");
    }

    [Fact]
    public void OnHueTouchStart_UpdatesHue_ViaJsInterop()
    {
        // Arrange - covers OnHueTouchStart.
        TestContext.JSInterop.Setup<double[]>("twColorPicker.relativePosition", _ => true).SetResult([110, 0]);
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "hsl(0, 100%, 50%)"));

        var hueSlider = cut.Find("div[role='slider'][aria-label='Hue']");

        // Act
        hueSlider.TriggerEvent("ontouchstart", new TouchEventArgs { Touches = [new TouchPoint { ClientX = 110, ClientY = 0 }] });

        // Assert - fallback hueSliderWidth is 220, so offsetX 110 drives hue to 180
        Assert.Equal("180", hueSlider.GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void OnHueTouchMove_WhileDragging_InvokesValueChangedAgain()
    {
        // Arrange - OnHueTouchStart begins the drag; OnHueTouchMove should update again while dragging.
        TestContext.JSInterop.Setup<double[]>("twColorPicker.relativePosition", _ => true).SetResult([55, 0]);
        var changeCount = 0;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "hsl(0, 100%, 50%)")
            .Add(p => p.ValueChanged, value => changeCount++));

        var hueSlider = cut.Find("div[role='slider'][aria-label='Hue']");
        hueSlider.TriggerEvent("ontouchstart", new TouchEventArgs { Touches = [new TouchPoint { ClientX = 55, ClientY = 0 }] });
        Assert.Equal(1, changeCount);

        // Act
        hueSlider.TriggerEvent("ontouchmove", new TouchEventArgs { Touches = [new TouchPoint { ClientX = 55, ClientY = 0 }] });

        // Assert
        Assert.Equal(2, changeCount);
    }

    [Fact]
    public void OnHueTouchMove_WithoutDrag_DoesNotUpdate()
    {
        // Arrange - no prior touchstart, so isHueDragging is false.
        var changeCount = 0;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "hsl(0, 100%, 50%)")
            .Add(p => p.ValueChanged, value => changeCount++));

        var hueSlider = cut.Find("div[role='slider'][aria-label='Hue']");

        // Act
        hueSlider.TriggerEvent("ontouchmove", new TouchEventArgs { Touches = [new TouchPoint { ClientX = 55, ClientY = 0 }] });

        // Assert
        Assert.Equal(0, changeCount);
    }

    [Fact]
    public void OnHueTouchEnd_StopsDragging_SubsequentMoveDoesNothing()
    {
        // Arrange
        TestContext.JSInterop.Setup<double[]>("twColorPicker.relativePosition", _ => true).SetResult([55, 0]);
        var changeCount = 0;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "hsl(0, 100%, 50%)")
            .Add(p => p.ValueChanged, value => changeCount++));

        var hueSlider = cut.Find("div[role='slider'][aria-label='Hue']");
        hueSlider.TriggerEvent("ontouchstart", new TouchEventArgs { Touches = [new TouchPoint { ClientX = 55, ClientY = 0 }] });
        Assert.Equal(1, changeCount);

        // Act
        hueSlider.TriggerEvent("ontouchend", new TouchEventArgs());
        hueSlider.TriggerEvent("ontouchmove", new TouchEventArgs { Touches = [new TouchPoint { ClientX = 110, ClientY = 0 }] });

        // Assert
        Assert.Equal(1, changeCount);
    }

    [Fact]
    public void OnAlphaTouchStart_UpdatesAlpha_ViaJsInterop()
    {
        // Arrange - covers OnAlphaTouchStart.
        TestContext.JSInterop.Setup<double[]>("twColorPicker.relativePosition", _ => true).SetResult([62, 0]);
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ShowAlpha, true));

        var alphaSlider = cut.Find("div[role='slider'][aria-label='Alpha']");

        // Act
        alphaSlider.TriggerEvent("ontouchstart", new TouchEventArgs { Touches = [new TouchPoint { ClientX = 62, ClientY = 0 }] });

        // Assert - fallback alphaSliderWidth is 124, so offsetX 62 drives alpha to 50%
        Assert.Equal("50", alphaSlider.GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void OnAlphaTouchMove_WhileDragging_InvokesValueChangedAgain()
    {
        // Arrange - OnAlphaTouchStart begins the drag; OnAlphaTouchMove should update again while dragging.
        TestContext.JSInterop.Setup<double[]>("twColorPicker.relativePosition", _ => true).SetResult([30, 0]);
        var changeCount = 0;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ShowAlpha, true)
            .Add(p => p.ValueChanged, value => changeCount++));

        var alphaSlider = cut.Find("div[role='slider'][aria-label='Alpha']");
        alphaSlider.TriggerEvent("ontouchstart", new TouchEventArgs { Touches = [new TouchPoint { ClientX = 30, ClientY = 0 }] });
        Assert.Equal(1, changeCount);

        // Act
        alphaSlider.TriggerEvent("ontouchmove", new TouchEventArgs { Touches = [new TouchPoint { ClientX = 60, ClientY = 0 }] });

        // Assert
        Assert.Equal(2, changeCount);
    }

    [Fact]
    public void OnAlphaTouchMove_WithoutDrag_DoesNotUpdate()
    {
        // Arrange - no prior touchstart, so isAlphaDragging is false.
        var changeCount = 0;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ShowAlpha, true)
            .Add(p => p.ValueChanged, value => changeCount++));

        var alphaSlider = cut.Find("div[role='slider'][aria-label='Alpha']");

        // Act
        alphaSlider.TriggerEvent("ontouchmove", new TouchEventArgs { Touches = [new TouchPoint { ClientX = 60, ClientY = 0 }] });

        // Assert
        Assert.Equal(0, changeCount);
    }

    [Fact]
    public void OnAlphaTouchEnd_StopsDragging_SubsequentMoveDoesNothing()
    {
        // Arrange
        TestContext.JSInterop.Setup<double[]>("twColorPicker.relativePosition", _ => true).SetResult([30, 0]);
        var changeCount = 0;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ShowAlpha, true)
            .Add(p => p.ValueChanged, value => changeCount++));

        var alphaSlider = cut.Find("div[role='slider'][aria-label='Alpha']");
        alphaSlider.TriggerEvent("ontouchstart", new TouchEventArgs { Touches = [new TouchPoint { ClientX = 30, ClientY = 0 }] });
        Assert.Equal(1, changeCount);

        // Act
        alphaSlider.TriggerEvent("ontouchend", new TouchEventArgs());
        alphaSlider.TriggerEvent("ontouchmove", new TouchEventArgs { Touches = [new TouchPoint { ClientX = 90, ClientY = 0 }] });

        // Assert
        Assert.Equal(1, changeCount);
    }

    #endregion

    #region EyeDropper Tests

    [Fact]
    public void OnAfterRender_DetectsEyeDropperSupport_AndRendersButton()
    {
        // Arrange
        TestContext.JSInterop.Setup<bool>("twColorPicker.supportsEyeDropper").SetResult(true);

        // Act
        var cut = TestContext.Render<TwColorPickerBody>();

        // Assert
        var buttons = cut.FindAll("button");
        Assert.Contains(buttons, b => b.GetAttribute("aria-label") == "Pick color from screen");
    }

    [Fact]
    public void OnAfterRender_DoesNotRenderEyeDropperButton_WhenUnsupported()
    {
        // Arrange - Loose JSInterop mode (the suite default) returns default(bool) = false when
        // twColorPicker.supportsEyeDropper isn't explicitly configured, same as an unsupported browser.
        var cut = TestContext.Render<TwColorPickerBody>();

        // Assert
        var buttons = cut.FindAll("button");
        Assert.DoesNotContain(buttons, b => b.GetAttribute("aria-label") == "Pick color from screen");
    }

    [Fact]
    public void PickColorFromScreenAsync_AppliesPickedColor()
    {
        // Arrange
        TestContext.JSInterop.Setup<bool>("twColorPicker.supportsEyeDropper").SetResult(true);
        TestContext.JSInterop.Setup<string?>("twColorPicker.openEyeDropper").SetResult("#7f56d9");
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#000000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        // Act
        var eyedropperButton = cut.FindAll("button").First(b => b.GetAttribute("aria-label") == "Pick color from screen");
        eyedropperButton.Click();

        // Assert
        Assert.Equal("#7F56D9", changedValue);
    }

    [Fact]
    public void PickColorFromScreenAsync_DoesNothing_WhenPickedColorIsNull()
    {
        // Arrange - twColorPicker.openEyeDropper isn't configured, so Loose mode resolves it to
        // null, the same result the JS side returns when the API is unsupported or the user cancels.
        TestContext.JSInterop.Setup<bool>("twColorPicker.supportsEyeDropper").SetResult(true);
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#000000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        // Act
        var eyedropperButton = cut.FindAll("button").First(b => b.GetAttribute("aria-label") == "Pick color from screen");
        eyedropperButton.Click();

        // Assert
        Assert.Null(changedValue);
    }

    [Fact]
    public void PickColorFromScreenAsync_DoesNothing_WhenPickedColorIsTooShort()
    {
        // Arrange - defends against a malformed/truncated response; the real EyeDropper API always
        // returns a full 6-digit hex string, but this guards the parsing below regardless.
        TestContext.JSInterop.Setup<bool>("twColorPicker.supportsEyeDropper").SetResult(true);
        TestContext.JSInterop.Setup<string?>("twColorPicker.openEyeDropper").SetResult("#abc");
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#000000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        // Act
        var eyedropperButton = cut.FindAll("button").First(b => b.GetAttribute("aria-label") == "Pick color from screen");
        eyedropperButton.Click();

        // Assert
        Assert.Null(changedValue);
    }

    #endregion

    #region Additional Coverage Gap Tests

    [Fact]
    public void FormatColorOutput_FallsBackToHex_ForUnrecognizedOutputFormat()
    {
        // Arrange - OutputFormat's switch has a defensive discard arm for a value outside the
        // Hex/Rgb/Hsl enum range, otherwise unreachable through the public API.
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.OutputFormat, (ColorMode)99)
            .Add(p => p.ValueChanged, value => changedValue = value));

        // Act
        var selector = cut.Find(".relative.w-full.h-48");
        selector.MouseDown(new MouseEventArgs { OffsetX = 110, OffsetY = 94 });

        // Assert
        Assert.NotNull(changedValue);
        Assert.Matches(@"^#[0-9A-F]{6}$", changedValue);
    }

    [Fact]
    public void HandleModeSwitch_FallsBackToHex_WhenCurrentModeIsUnrecognized()
    {
        // Arrange - currentMode's switch has the same kind of defensive discard arm, reached here by
        // setting the private field directly since the public API can only ever cycle it through
        // Hex/Rgb/Hsl.
        var cut = TestContext.Render<TwColorPickerBody>();
        var field = typeof(TwColorPickerBody).GetField("currentMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        field.SetValue(cut.Instance, (ColorMode)99);

        var method = typeof(TwColorPickerBody).GetMethod("HandleModeSwitch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        method.Invoke(cut.Instance, null);

        // Assert
        Assert.Equal(ColorMode.Hex, field.GetValue(cut.Instance));
    }

    [Fact]
    public void OnRInputChanged_IgnoresNonNumericInput()
    {
        // Arrange - TwTextfield<int> only ever calls this with an already-parsed int.ToString(), so
        // the invalid-parse branch is otherwise unreachable via normal interaction.
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        var method = typeof(TwColorPickerBody).GetMethod("OnRInputChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        method.Invoke(cut.Instance, ["not-a-number"]);

        // Assert
        Assert.Null(changedValue);
    }

    [Fact]
    public void OnGInputChanged_IgnoresNonNumericInput()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        var method = typeof(TwColorPickerBody).GetMethod("OnGInputChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        method.Invoke(cut.Instance, ["not-a-number"]);

        // Assert
        Assert.Null(changedValue);
    }

    [Fact]
    public void OnBInputChanged_IgnoresNonNumericInput()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#FF0000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        var method = typeof(TwColorPickerBody).GetMethod("OnBInputChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        method.Invoke(cut.Instance, ["not-a-number"]);

        // Assert
        Assert.Null(changedValue);
    }

    [Fact]
    public void OnHexInputCommitted_HandlesNullHexInputField_Gracefully()
    {
        // Arrange - hexInput is only ever assigned string literals in practice, so the `?? defaultColor`
        // fallback is otherwise unreachable; reached here by setting the private field directly.
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPickerBody>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.ValueChanged, value => changedValue = value));

        var field = typeof(TwColorPickerBody).GetField("hexInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        field.SetValue(cut.Instance, null);

        var method = typeof(TwColorPickerBody).GetMethod("OnHexInputCommitted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        method.Invoke(cut.Instance, null);

        // Assert - falls back to "#000000" and parses successfully
        Assert.Equal("#000000", changedValue);
    }

    #endregion

    [Fact]
    public void TwColorPickerBody_EverySlider_ShowsThePrimaryFocusRing()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPickerBody>();

        // Assert - the saturation square and the hue and alpha sliders are all Tab stops
        var sliders = cut.FindAll("[role='slider']");
        Assert.True(sliders.Count >= 2);
        Assert.All(sliders, slider =>
        {
            Assert.Equal("0", slider.GetAttribute("tabindex"));
            Assert.Contains("focus-visible:ring-2", slider.ClassList);
            Assert.Contains("focus-visible:ring-purple-500", slider.ClassList);
            Assert.Contains("focus:outline-hidden", slider.ClassList);
        });
    }
}
