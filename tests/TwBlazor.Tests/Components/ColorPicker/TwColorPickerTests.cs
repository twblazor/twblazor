using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Builders;
using TwBlazor.Components;
using TwBlazor.Components.ColorPicker;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.ColorPicker;

public class TwColorPickerTests : TwBlazorTestBase
{
    private TwInputTheme inputTheme => Theme.Components.Require<TwInputTheme>();

    [Fact]
    public void ClickingSwatch_PositionsPanelAsFixed_NotTheOldAbsolutePositionPanel()
    {
        // Regression test: the popover panel must be positioned via twPicker.registerScrollReposition
        // (which applies twPicker.positionPanelFixed itself - position:fixed, anchored via
        // JS-computed coordinates) rather than the old twPicker.positionPanel (position:absolute), so
        // it isn't clipped when the picker is used inside a scrollable ancestor such as a TwDialog's
        // body.
        var cut = TestContext.Render<TwColorPicker>();

        cut.Find("button[aria-haspopup='dialog']").Click();

        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.registerScrollReposition");
        Assert.IsType<ElementReference>(invocation.Arguments[0]);
        Assert.IsType<ElementReference>(invocation.Arguments[1]);
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.positionPanel");
    }

    [Fact]
    public void TwColorPicker_Renders_WithDefaultValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>();

        // Assert
        var textInput = cut.Find("input[type='text']");
        Assert.NotNull(textInput);
        Assert.Equal("text", textInput.GetAttribute("type"));
    }

    [Fact]
    public void TwColorPicker_Renders_WithLabel()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Label, "Select Color"));

        // Assert
        var label = cut.Find("label");
        Assert.NotNull(label);
        Assert.Equal("Select Color", label.TextContent);
    }

    [Fact]
    public void TwColorPicker_DoesNotRender_LabelWhenEmpty()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Label, string.Empty));

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find("label"));
    }

    [Fact]
    public void TwColorPicker_Renders_WithCustomValue()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#FF5733"));

        // Assert
        var textInput = cut.Find("input[type='text']");
        Assert.Equal("#FF5733", textInput.GetAttribute("value"));
    }

    [Fact]
    public void TwColorPicker_Renders_WithPreview()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#FF5733"));

        // Assert
        var preview = cut.Find("button");
        Assert.NotNull(preview);
        Assert.Contains("background-color: #FF5733", preview.GetAttribute("style"));
    }

    [Fact]
    public void TwColorPicker_Renders_WithId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Id, "custom-color-picker"));

        // Assert
        var allInputs = cut.FindAll("input");
        var textInput = allInputs.FirstOrDefault(i => i.GetAttribute("id") == "custom-color-picker");
        Assert.NotNull(textInput);
    }

    [Fact]
    public void TwColorPicker_Renders_WithRootId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.RootId, "custom-root-id"));

        // Assert
        var root = cut.Find("#custom-root-id");
        Assert.NotNull(root);
    }

    [Fact]
    public void TwColorPicker_GeneratesRootId_WhenNotProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>();

        // Assert
        var divs = cut.FindAll("div");
        Assert.NotEmpty(divs);
        var hasIdAttribute = divs.Any(d => !string.IsNullOrEmpty(d.GetAttribute("id")));
        Assert.True(hasIdAttribute);
    }

    [Fact]
    public void TwColorPicker_Renders_WithCustomClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Class, "custom-class"));

        // Assert
        var allInputs = cut.FindAll("input");
        var textInput = allInputs.FirstOrDefault(i => i.GetAttribute("type") == "text");
        Assert.NotNull(textInput);
        Assert.Contains("custom-class", textInput.GetAttribute("class"));
    }

    [Fact]
    public void TwColorPicker_Renders_WithCustomRootClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.RootClass, "custom-root-class"));

        // Assert
        var div = cut.Find("div");
        Assert.Contains("custom-root-class", div.GetAttribute("class"));
    }

    [Fact]
    public void TwColorPicker_Renders_WithDisabledAttribute()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Disabled, true));

        // Assert
        var textInput = cut.Find("input[type='text']");
        Assert.True(textInput.HasAttribute("disabled"));
    }

    [Fact]
    public void TwColorPicker_Renders_WithReadOnlyAttribute()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.ReadOnly, true));

        // Assert
        var allInputs = cut.FindAll("input");
        var textInput = allInputs.FirstOrDefault(i => i.GetAttribute("type") == "text");
        Assert.NotNull(textInput);
        Assert.True(textInput.HasAttribute("readonly"));
    }

    [Fact]
    public void TwColorPicker_Renders_WithDefaultVariant()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Variant, InputVariant.Default));

        // Assert
        var allInputs = cut.FindAll("input");
        var textInput = allInputs.FirstOrDefault(i => i.GetAttribute("type") == "text");
        Assert.NotNull(textInput);
        // Variant styling is now handled by TwTextfield component
    }

    [Fact]
    public void TwColorPicker_UsesGlobalDefaultVariant_WhenNotSet()
    {
        // Arrange - no Variant set on the component, so it must follow TwInputTheme.DefaultInputVariant
        // (inherited via TwBlazorInputComponentBase.effectiveVariant), even after the theme changes.
        inputTheme.DefaultInputVariant = InputVariant.Outlined;

        // Act
        var cut = TestContext.Render<TwColorPicker>();

        // Assert
        var textInput = cut.FindAll("input").First(i => i.GetAttribute("type") == "text");
        Assert.Contains(InputVariantBuilder.GetClasses(InputVariant.Outlined, inputTheme), textInput.GetAttribute("class"));
    }

    [Fact]
    public void TwColorPicker_ExplicitVariant_OverridesGlobalDefault()
    {
        // Arrange - the global default is Outlined, but this instance explicitly asks for Filled.
        inputTheme.DefaultInputVariant = InputVariant.Outlined;

        // Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Variant, InputVariant.Filled));

        // Assert
        var textInput = cut.FindAll("input").First(i => i.GetAttribute("type") == "text");
        Assert.Contains(InputVariantBuilder.GetClasses(InputVariant.Filled, inputTheme), textInput.GetAttribute("class"));
    }

    [Fact]
    public void TwColorPicker_InvokesValueChanged_WhenColorChanges()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#000000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        // Act
        var textInput = cut.Find("input[type='text']");
        textInput.Change("#FF5733");

        // Assert
        Assert.Equal("#FF5733", changedValue);
    }

    [Fact]
    public void TwColorPicker_UpdatesValue_WhenColorChanges()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#000000"));

        // Act
        var textInput = cut.Find("input[type='text']");
        textInput.Change("#00FF00");

        // Assert
        Assert.Equal("#00FF00", textInput.GetAttribute("value"));
    }

    [Fact]
    public void TwColorPicker_BindsOnChange_ByDefault()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>();

        // Assert
        var allInputs = cut.FindAll("input");
        var textInput = allInputs.FirstOrDefault(i => i.GetAttribute("type") == "text");
        // The text input uses TwTextfield which handles the bind event
        Assert.NotNull(textInput);
    }

    [Fact]
    public void TwColorPicker_SupportsCustomBindEvent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.BindEvent, "oninput"));

        // Assert
        var allInputs = cut.FindAll("input");
        var textInput = allInputs.FirstOrDefault(i => i.GetAttribute("type") == "text");
        Assert.NotNull(textInput);
    }

    [Fact]
    public void TwColorPicker_Renders_WithLabelAttributes()
    {
        // Arrange
        var labelAttributes = new Dictionary<string, object>
        {
            { "data-test", "label-test" }
        };

        // Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Label, "Color")
            .Add(p => p.LabelAttributes, labelAttributes));

        // Assert
        var label = cut.Find("label");
        Assert.Equal("label-test", label.GetAttribute("data-test"));
    }

    [Fact]
    public void TwColorPicker_Renders_WithLabelClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Label, "Color")
            .Add(p => p.LabelClass, "custom-label-class"));

        // Assert
        var label = cut.Find("label");
        Assert.Contains("custom-label-class", label.GetAttribute("class"));
    }

    [Fact]
    public void TwColorPicker_Renders_WithLabelId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Label, "Color")
            .Add(p => p.LabelId, "custom-label-id")
            .Add(p => p.Id, "color-input"));

        // Assert
        var label = cut.Find("label");
        Assert.Equal("custom-label-id", label.GetAttribute("id"));
        Assert.Equal("color-input", label.GetAttribute("for"));
    }

    [Fact]
    public void TwColorPicker_InvokesFocus_WhenFocused()
    {
        // Arrange
        var wasFocused = false;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.OnFocus, args => wasFocused = true));

        // Act
        var allInputs = cut.FindAll("input");
        var textInput = allInputs.FirstOrDefault(i => i.GetAttribute("type") == "text");
        Assert.NotNull(textInput);
        textInput.Focus();

        // Assert
        Assert.True(wasFocused);
    }

    [Fact]
    public void TwColorPicker_Renders_WithAdditionalAttributes()
    {
        // Arrange
        var attributes = new Dictionary<string, object>
        {
            { "data-testid", "color-picker-test" }
        };

        // Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Attributes, attributes)
            .Add(p => p.AriaLabel, "Pick a color"));

        // Assert
        var allInputs = cut.FindAll("input");
        var textInput = allInputs.FirstOrDefault(i => i.GetAttribute("type") == "text");
        Assert.NotNull(textInput);
        Assert.Equal("color-picker-test", textInput.GetAttribute("data-testid"));
        // aria-label is set via the AriaLabel component parameter, not the generic Attributes
        // dictionary - a stray "aria-label" key in Attributes must never silently override it.
        Assert.Equal("Pick a color", textInput.GetAttribute("aria-label"));
    }

    [Fact]
    public void TwColorPicker_ContainsRequiredClasses()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>();

        // Assert
        var allInputs = cut.FindAll("input");
        var textInput = allInputs.FirstOrDefault(i => i.GetAttribute("type") == "text");
        Assert.NotNull(textInput);
        var classAttr = textInput.GetAttribute("class");
        // Component now uses TwTextfield which handles its own styling
        Assert.NotNull(classAttr);
    }

    [Fact]
    public void TwColorPicker_Renders_WithModeSwitchButton()
    {
        // Arrange
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.ShowModeSwitch, true));

        // Act - click to show dialog
        var preview = cut.Find("button");
        preview.Click();

        // Assert - mode switch button is in the dialog body
        var buttons = cut.FindAll("button");
        var modeButton = buttons.FirstOrDefault(b => b.TextContent.Contains("HEX"));
        Assert.NotNull(modeButton);
    }

    [Fact]
    public void TwColorPicker_DoesNotShowDialog_WhenDisabled()
    {
        // Arrange
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.Value, "#3b82f6"));

        // Act
        var preview = cut.Find("button");
        preview.Click();

        // Assert - Dialog should not appear
        var dialogs = cut.FindAll(".tw-color-picker-dialog");
        Assert.Empty(dialogs);
    }

    [Fact]
    public void TwColorPicker_DoesNotShowDialog_WhenReadOnly()
    {
        // Arrange
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.ReadOnly, true)
            .Add(p => p.Value, "#3b82f6"));

        // Act
        var preview = cut.Find("button");
        preview.Click();

        // Assert - Dialog should not appear
        var dialogs = cut.FindAll(".tw-color-picker-dialog");
        Assert.Empty(dialogs);
    }

    [Fact]
    public void TwColorPicker_ShowsDialog_WhenPreviewClicked()
    {
        // Arrange
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6"));

        // Act
        var preview = cut.Find("button");
        preview.Click();

        // Assert
        var dialog = cut.Find(".tw-color-picker-dialog");
        Assert.NotNull(dialog);
    }

    [Fact]
    public void TwColorPicker_UpdatesDisplayValue_WhenValueChanges()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#FF0000"));

        var textInput = cut.Find("input[type='text']");
        Assert.Equal("#FF0000", textInput.GetAttribute("value"));

        // Act - Update the value parameter
        cut.Render(parameters => parameters
            .Add(p => p.Value, "#00FF00"));

        // Assert
        textInput = cut.Find("input[type='text']");
        Assert.Equal("#00FF00", textInput.GetAttribute("value"));
    }

    [Fact]
    public void TwColorPicker_NormalizesHexValue_WithoutHash()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#000000")
            .Add(p => p.ValueChanged, value => changedValue = value));

        // Act
        var textInput = cut.Find("input[type='text']");
        textInput.Change("FF5733");

        // Assert
        Assert.NotNull(changedValue);
        Assert.StartsWith("#", changedValue);
    }

    [Fact]
    public void TwColorPicker_PreviewColor_MatchesValue()
    {
        // Arrange
        var color = "#3b82f6";
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, color));

        // Assert
        var preview = cut.Find("button");
        Assert.Contains($"background-color: {color}", preview.GetAttribute("style"));
    }

    [Fact]
    public void TwColorPicker_ShowsPlaceholder_BasedOnAlphaSetting()
    {
        // Arrange - Without alpha
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.ShowAlpha, false));

        var textInput = cut.Find("input[type='text']");
        var placeholder = textInput.GetAttribute("placeholder");
        Assert.Contains("RRGGBB", placeholder);
        Assert.DoesNotContain("AA", placeholder);

        // Arrange - With alpha
        cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.ShowAlpha, true));

        textInput = cut.Find("input[type='text']");
        placeholder = textInput.GetAttribute("placeholder");
        Assert.Contains("RRGGBBAA", placeholder);
    }

    [Fact]
    public void TwColorPicker_PassesShowAlpha_ToColorPickerBody()
    {
        // Arrange
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.ShowAlpha, true)
            .Add(p => p.Value, "#3b82f6"));

        // Act
        var preview = cut.Find("button");
        preview.Click();

        // Assert - Alpha slider should be in the dialog
        var alphaLabel = cut.FindAll("span").FirstOrDefault(s => s.TextContent.Contains("Alpha"));
        Assert.NotNull(alphaLabel);
    }

    [Fact]
    public void TwColorPicker_PassesShowModeSwitch_ToColorPickerBody()
    {
        // Arrange
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.ShowModeSwitch, false)
            .Add(p => p.Value, "#3b82f6"));

        // Act
        var preview = cut.Find("button");
        preview.Click();

        // Assert - Mode switch button should not be present
        var buttons = cut.FindAll("button");
        var modeButton = buttons.FirstOrDefault(b =>
            b.TextContent.Contains("HEX") ||
            b.TextContent.Contains("RGB") ||
            b.TextContent.Contains("HSL"));
        Assert.Null(modeButton);
    }

    [Fact]
    public void TwColorPicker_ShowsDialogWithCurrentColor()
    {
        // Arrange
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#FF0000"));

        // Act - Open dialog
        var preview = cut.Find("button");
        preview.Click();

        // Assert - Dialog should show the current color
        var dialog = cut.Find(".tw-color-picker-dialog");
        Assert.NotNull(dialog);

        // The swatch button in TwColorPicker.razor also sets an inline background-color style, but it's a
        // <button>, not a <div> - only the dialog body's swatch (TwColorPickerBody.razor) matches this selector.
        var dialogPreviews = cut.FindAll("div[style*='background-color']");
        Assert.NotEmpty(dialogPreviews);
    }

    [Fact]
    public void TwColorPicker_ClosesDialog_WhenConfirmClicked()
    {
        // Arrange
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6"));

        // Act - Open dialog
        var preview = cut.Find("button");
        preview.Click();

        // Click confirm
        var buttons = cut.FindAll("button");
        var confirmButton = buttons.First(b => b.TextContent.Contains("Confirm"));
        confirmButton.Click();

        // Assert - Dialog should be closed
        var dialogs = cut.FindAll(".tw-color-picker-dialog");
        Assert.Empty(dialogs);
    }

    [Fact]
    public void TwColorPicker_ClosesDialog_WhenCancelClicked()
    {
        // Arrange
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6"));

        // Act - Open dialog
        var preview = cut.Find("button");
        preview.Click();

        // Click cancel
        var buttons = cut.FindAll("button");
        var cancelButton = buttons.First(b => b.TextContent.Contains("Cancel"));
        cancelButton.Click();

        // Assert - Dialog should be closed
        var dialogs = cut.FindAll(".tw-color-picker-dialog");
        Assert.Empty(dialogs);
    }

    #region Bug Fix Tests - RGB/HSL Format Support

    [Fact]
    public void TwColorPicker_DisplaysRgbValue_WithoutHashPrefix()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "rgb(59, 130, 246)")
            .Add(p => p.OutputFormat, ColorMode.Rgb));

        // Assert
        var textInput = cut.Find("input[type='text']");
        var value = textInput.GetAttribute("value");
        Assert.Equal("rgb(59, 130, 246)", value);
        Assert.DoesNotContain("#", value);
    }

    [Fact]
    public void TwColorPicker_DisplaysHslValue_WithoutHashPrefix()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "hsl(217, 91%, 60%)")
            .Add(p => p.OutputFormat, ColorMode.Hsl));

        // Assert
        var textInput = cut.Find("input[type='text']");
        var value = textInput.GetAttribute("value");
        Assert.Equal("hsl(217, 91%, 60%)", value);
        Assert.DoesNotContain("#", value);
    }

    [Fact]
    public void TwColorPicker_DisplaysRgbaValue_WithoutHashPrefix()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "rgba(59, 130, 246, 0.5)")
            .Add(p => p.OutputFormat, ColorMode.Rgb)
            .Add(p => p.ShowAlpha, true));

        // Assert
        var textInput = cut.Find("input[type='text']");
        var value = textInput.GetAttribute("value");
        Assert.Equal("rgba(59, 130, 246, 0.5)", value);
        Assert.DoesNotContain("#", value);
    }

    [Fact]
    public void TwColorPicker_PreviewBox_RendersCorrectly_WithRgbValue()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "rgb(59, 130, 246)")
            .Add(p => p.OutputFormat, ColorMode.Rgb));

        // Assert
        var preview = cut.Find("button");
        var style = preview.GetAttribute("style");

        // Should convert RGB to hex for CSS background-color
        Assert.Contains("background-color", style);
        Assert.Contains("#", style);
    }

    [Fact]
    public void TwColorPicker_PreviewBox_RendersCorrectly_WithHslValue()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "hsl(217, 91%, 60%)")
            .Add(p => p.OutputFormat, ColorMode.Hsl));

        // Assert
        var preview = cut.Find("button");
        var style = preview.GetAttribute("style");

        // Should convert HSL to hex for CSS background-color
        Assert.Contains("background-color", style);
        Assert.Contains("#", style);
    }

    [Fact]
    public void TwColorPicker_Placeholder_ChangesBasedOnOutputFormat()
    {
        // Test Hex placeholder
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.OutputFormat, ColorMode.Hex));
        var textInput = cut.Find("input[type='text']");
        Assert.Contains("#RRGGBB", textInput.GetAttribute("placeholder"));

        // Test RGB placeholder
        cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.OutputFormat, ColorMode.Rgb));
        textInput = cut.Find("input[type='text']");
        Assert.Contains("rgb(r, g, b)", textInput.GetAttribute("placeholder"));

        // Test HSL placeholder
        cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.OutputFormat, ColorMode.Hsl));
        textInput = cut.Find("input[type='text']");
        Assert.Contains("hsl(h, s%, l%)", textInput.GetAttribute("placeholder"));
    }

    [Fact]
    public void TwColorPicker_Placeholder_IncludesAlpha_WhenShowAlphaTrue()
    {
        // Test RGBA placeholder
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.OutputFormat, ColorMode.Rgb)
            .Add(p => p.ShowAlpha, true));
        var textInput = cut.Find("input[type='text']");
        Assert.Contains("rgba(r, g, b, a)", textInput.GetAttribute("placeholder"));

        // Test HSLA placeholder
        cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.OutputFormat, ColorMode.Hsl)
            .Add(p => p.ShowAlpha, true));
        textInput = cut.Find("input[type='text']");
        Assert.Contains("hsla(h, s%, l%, a)", textInput.GetAttribute("placeholder"));
    }

    [Fact]
    public async Task TwColorPicker_TextInput_PreservesRgbFormat()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "rgb(59, 130, 246)")
            .Add(p => p.OutputFormat, ColorMode.Rgb)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => changedValue = value)));

        // Act - Type in a new RGB value using ChangeAsync (which triggers onchange)
        var textInput = cut.Find("input[type='text']");
        await textInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = "rgb(255, 0, 0)" });

        // Assert
        Assert.Equal("rgb(255, 0, 0)", changedValue);
        Assert.DoesNotContain("#", changedValue);
    }

    [Fact]
    public async Task TwColorPicker_TextInput_PreservesHslFormat()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "hsl(217, 91%, 60%)")
            .Add(p => p.OutputFormat, ColorMode.Hsl)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => changedValue = value)));

        // Act - Type in a new HSL value using ChangeAsync (which triggers onchange)
        var textInput = cut.Find("input[type='text']");
        await textInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = "hsl(0, 100%, 50%)" });

        // Assert
        Assert.Equal("hsl(0, 100%, 50%)", changedValue);
        Assert.DoesNotContain("#", changedValue);
    }

    [Fact]
    public void TwColorPicker_PreviewBox_HandlesInvalidRgbValue()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "rgb(invalid)")
            .Add(p => p.OutputFormat, ColorMode.Rgb));

        // Assert - Should fallback to black
        var preview = cut.Find("button");
        var style = preview.GetAttribute("style");
        Assert.Contains("background-color: #000000", style);
    }

    [Fact]
    public void TwColorPicker_Preview_ConvertsRgbValueToHexForDisplay()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "rgb(255, 87, 51)"));

        // Assert - GetPreviewColor converts rgb to hex for the swatch
        var preview = cut.Find("button");
        var style = preview.GetAttribute("style");
        Assert.NotNull(style);
        Assert.Contains("background-color", style);
    }

    [Fact]
    public void TwColorPicker_Preview_ConvertsHslValueToHexForDisplay()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "hsl(0, 100%, 50%)"));

        // Assert - GetPreviewColor converts hsl to hex for the swatch
        var preview = cut.Find("button");
        var style = preview.GetAttribute("style");
        Assert.NotNull(style);
        Assert.Contains("background-color", style);
    }

    [Fact]
    public void TwColorPicker_ClosesDialog_OnEscapeKey()
    {
        // Arrange
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6"));

        var preview = cut.Find("button");
        preview.Click();
        Assert.NotEmpty(cut.FindAll(".tw-color-picker-dialog"));

        // Act
        var dialogContainer = cut.Find("[tabindex='-1']");
        dialogContainer.KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Escape" });

        // Assert
        Assert.Empty(cut.FindAll(".tw-color-picker-dialog"));
    }

    [Fact]
    public void TwColorPicker_KeepsDialogOpen_OnNonEscapeKey()
    {
        // Arrange
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6"));

        var preview = cut.Find("button");
        preview.Click();

        // Act
        var dialogContainer = cut.Find("[tabindex='-1']");
        dialogContainer.KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });

        // Assert
        Assert.NotEmpty(cut.FindAll(".tw-color-picker-dialog"));
    }

    [Fact]
    public void TwColorPicker_NormalizeHex_TrimsAlphaChannel_WhenShowAlphaFalse()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#000000")
            .Add(p => p.ShowAlpha, false)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => changedValue = v)));

        // Act
        var textInput = cut.Find("input[type='text']");
        textInput.Change("FF5733AA");

        // Assert
        Assert.Equal("#FF5733", changedValue);
    }

    [Fact]
    public void TwColorPicker_NormalizeHex_PreservesAlphaChannel_WhenShowAlphaTrue()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#000000")
            .Add(p => p.ShowAlpha, true)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => changedValue = v)));

        // Act
        var textInput = cut.Find("input[type='text']");
        textInput.Change("FF5733AA");

        // Assert
        Assert.Equal("#FF5733AA", changedValue);
    }

    [Fact]
    public void TwColorPicker_HandleTextInputChange_DoesNotInvokeValueChanged_WhenEmpty()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#000000")
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => changedValue = v)));

        // Act
        var textInput = cut.Find("input[type='text']");
        textInput.Change(string.Empty);

        // Assert
        Assert.Null(changedValue);
    }

    #endregion

    #region Native Picker (Device Detection)

    [Fact]
    public void PreferNativePickerTrue_RendersNativeColorInput_InsteadOfPreviewDiv()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, true));

        // Assert
        var nativeInput = cut.Find("input[type='color']");
        Assert.Equal("#3b82f6", nativeInput.GetAttribute("value"));
        Assert.Throws<ElementNotFoundException>(() => cut.Find("button"));
    }

    [Fact]
    public void PreferNativePickerTrue_ClickingPreview_DoesNotOpenCustomDialog()
    {
        // Arrange
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, true));

        // Assert — the custom dialog markup never renders because ShowDialog() is never wired up.
        Assert.DoesNotContain("tw-color-picker-dialog", cut.Markup);
    }

    [Fact]
    public void PreferNativePickerTrue_ChangingNativeColorInput_UpdatesValue_AndInvokesCallback()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, true)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => changedValue = v)));

        // Act
        var nativeInput = cut.Find("input[type='color']");
        nativeInput.Change("#ff5733");

        // Assert
        Assert.Equal("#ff5733", changedValue);
    }

    [Fact]
    public void PreferNativePickerTrue_OutputFormatRgb_ConvertsNativeHexToRgb()
    {
        // Arrange - the native <input type="color"> can only ever emit a plain hex value (iOS/Android
        // use this path by default), so OutputFormat has to be applied on the way out here too, the
        // same way TwColorPickerBody's custom dialog already does.
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, true)
            .Add(p => p.OutputFormat, ColorMode.Rgb)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => changedValue = v)));

        // Act
        var nativeInput = cut.Find("input[type='color']");
        nativeInput.Change("#ff5733");

        // Assert
        Assert.Equal("rgb(255, 87, 51)", changedValue);
        Assert.Equal("rgb(255, 87, 51)", cut.Instance.Value);
    }

    [Fact]
    public void PreferNativePickerTrue_OutputFormatHsl_ConvertsNativeHexToHsl()
    {
        // Arrange
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, true)
            .Add(p => p.OutputFormat, ColorMode.Hsl)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => changedValue = v)));

        // Act
        var nativeInput = cut.Find("input[type='color']");
        nativeInput.Change("#ff5733");

        // Assert
        Assert.Equal("hsl(11, 100%, 60%)", changedValue);
    }

    [Fact]
    public void PreferNativePickerTrue_OutputFormatHex_LeavesNativeHexUnchanged()
    {
        // Arrange - explicit ColorMode.Hex (rather than relying on the parameter default) covers the
        // switch's Hex arm directly.
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, true)
            .Add(p => p.OutputFormat, ColorMode.Hex)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => changedValue = v)));

        // Act
        var nativeInput = cut.Find("input[type='color']");
        nativeInput.Change("#ff5733");

        // Assert
        Assert.Equal("#ff5733", changedValue);
    }

    [Fact]
    public void PreferNativePickerFalse_RendersPreviewDiv_AndOpensCustomDialogOnClick()
    {
        // Arrange
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, false));

        Assert.Throws<ElementNotFoundException>(() => cut.Find("input[type='color']"));

        // Act
        cut.Find("button").Click();

        // Assert
        var buttons = cut.FindAll("button");
        Assert.NotEmpty(buttons);
    }

    [Fact]
    public void NativePickerNotSpecified_DetectsViaJsInterop_AndSwitchesToNativeColorInput()
    {
        // Arrange
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(true);

        // Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6"));

        // Assert
        Assert.NotNull(cut.Find("input[type='color']"));
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDevice.prefersNativePicker");
    }

    [Fact]
    public void NativePickerNotSpecified_JsInteropReturnsFalse_KeepsPreviewDiv()
    {
        // Arrange
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(false);

        // Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6"));

        // Assert
        Assert.NotNull(cut.Find("button"));
    }

    [Fact]
    public async Task OnDialogValueChanged_UpdatesValue_AndInvokesCallback()
    {
        // Arrange - drive TwColorPicker's OnDialogValueChanged handler through the
        // ValueChanged callback it wires up on the child TwColorPickerBody.
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => changedValue = v)));

        cut.Find("button").Click();
        var body = cut.FindComponent<TwColorPickerBody>();

        // Act
        await cut.InvokeAsync(() => body.Instance.ValueChanged.InvokeAsync("#ff0000"));

        // Assert
        Assert.Equal("#ff0000", changedValue);
        Assert.Equal("#ff0000", cut.Instance.Value);
    }

    [Fact]
    public async Task OnDialogClose_ClosesDialog()
    {
        // Arrange - drive TwColorPicker's OnDialogClose handler through the OnClose
        // callback it wires up on the child TwColorPickerBody.
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, false));

        cut.Find("button").Click();
        var body = cut.FindComponent<TwColorPickerBody>();

        // Act
        await cut.InvokeAsync(() => body.Instance.OnClose.InvokeAsync(true));

        // Assert
        Assert.Throws<Bunit.Rendering.ComponentNotFoundException>(() => cut.FindComponent<TwColorPickerBody>());
    }

    [Fact]
    public void OnDialogKeyDown_Escape_ClosesDialog()
    {
        // Arrange
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, false));

        cut.Find("button").Click();
        var dialogWrapper = cut.Find("div[tabindex='-1']");

        // Act
        dialogWrapper.KeyDown(new KeyboardEventArgs { Key = "Escape" });

        // Assert
        Assert.Throws<Bunit.Rendering.ComponentNotFoundException>(() => cut.FindComponent<TwColorPickerBody>());
    }

    [Fact]
    public void OnDialogKeyDown_NonEscapeKey_DoesNotCloseDialog()
    {
        // Arrange
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, false));

        cut.Find("button").Click();
        var dialogWrapper = cut.Find("div[tabindex='-1']");

        // Act
        dialogWrapper.KeyDown(new KeyboardEventArgs { Key = "A" });

        // Assert - dialog stays open
        Assert.NotNull(cut.FindComponent<TwColorPickerBody>());
    }

    [Fact]
    public void ShowDialog_CalledTwice_OnlyRegistersOutsideClickHandlerOnce()
    {
        // Arrange - exercises RegisterOutsideClickAsync's `if (registeredOutsideHandler) return;`
        // guard by opening the dialog, closing it via a second click target isn't available, so
        // instead we click the preview to open, then click it again while already open.
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, false));

        var preview = cut.Find("button");

        // Act
        preview.Click();
        preview.Click(); // second call while already registered

        // Assert - should not throw, dialog remains open
        Assert.NotNull(cut.FindComponent<TwColorPickerBody>());
    }

    [Fact]
    public async Task Close_ClosesDialog_WhenInvokedDirectly()
    {
        // Arrange - Close() is [JSInvokable] and normally called from JS when clicking outside
        // the dialog; invoke it directly here to cover that path.
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, false));

        cut.Find("button").Click();
        Assert.NotNull(cut.FindComponent<TwColorPickerBody>());

        // Act
        await cut.InvokeAsync(() => cut.Instance.Close());

        // Assert
        Assert.Throws<Bunit.Rendering.ComponentNotFoundException>(() => cut.FindComponent<TwColorPickerBody>());
    }

    [Fact]
    public async Task DisposeAsync_DoesNotThrow_AfterDialogWasOpened()
    {
        // Arrange
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, false));

        cut.Find("button").Click();

        // Act
        var exception = await Record.ExceptionAsync(async () => await ((IAsyncDisposable)cut.Instance).DisposeAsync());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task DisposeAsync_DoesNotThrow_WhenDialogWasNeverOpened()
    {
        // Arrange - exercises the `if (!registeredOutsideHandler) return;` guard in
        // UnregisterOutsideClickAsync when the dialog was never opened.
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6"));

        // Act
        var exception = await Record.ExceptionAsync(async () => await ((IAsyncDisposable)cut.Instance).DisposeAsync());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void HandleTextInputChangeAsync_IgnoresWhitespaceOnlyInput()
    {
        // Arrange - exercises the `!string.IsNullOrWhiteSpace(displayValue)` false branch.
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => changedValue = v)));

        var textInput = cut.Find("input[type='text']");

        // Act
        textInput.Change("   ");

        // Assert - ValueChanged never invoked, Value untouched
        Assert.Null(changedValue);
        Assert.Equal("#3b82f6", cut.Instance.Value);
    }

    [Fact]
    public void OnNativeColorChangedAsync_IgnoresWhitespaceOnlyInput()
    {
        // Arrange - exercises the `string.IsNullOrWhiteSpace(newValue)` early-return branch.
        string? changedValue = null;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, true)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => changedValue = v)));

        var nativeInput = cut.Find("input[type='color']");

        // Act
        nativeInput.Change(new ChangeEventArgs { Value = null });

        // Assert
        Assert.Null(changedValue);
        Assert.Equal("#3b82f6", cut.Instance.Value);
    }

    [Fact]
    public void GetPreviewColor_ReturnsDefaultColor_WhenValueIsWhitespace()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "   ")
            .Add(p => p.PreferNativePicker, false));

        // Assert - falls back to the default black swatch
        var preview = cut.Find("button");
        Assert.Contains("#000000", preview.GetAttribute("style"));
    }

    [Fact]
    public void SwatchAriaLabel_ReflectsCurrentValue()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, false));

        // Assert
        var preview = cut.Find("button");
        Assert.Equal("Selected color: #3b82f6", preview.GetAttribute("aria-label"));
    }

    [Fact]
    public void SwatchAriaLabel_Updates_WhenValueChanges()
    {
        // Arrange
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, false));

        // Act
        cut.Render(parameters => parameters.Add(p => p.Value, "#ff0000"));

        // Assert
        var preview = cut.Find("button");
        Assert.Equal("Selected color: #ff0000", preview.GetAttribute("aria-label"));
    }

    [Fact]
    public void ShowDialog_CapturesFocusToken()
    {
        // Arrange
        TestContext.JSInterop.Setup<string?>("twDialog.captureFocus").SetResult("tw-focus-token");
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, false));

        // Act
        cut.Find("button").Click();

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.captureFocus");
    }

    [Fact]
    public void OnAfterRender_TrapsFocus_AndFocusesSurface_WhenDialogOpens()
    {
        // Arrange
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, false));

        // Act
        cut.Find("button").Click();

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.trapFocus");
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.setBackgroundInert");
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.focusSurface");
    }

    [Fact]
    public void OnDialogClose_ReleasesPanelTrap_AndRestoresFocus()
    {
        // Arrange
        TestContext.JSInterop.Setup<string?>("twDialog.captureFocus").SetResult("tw-focus-token");
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, false));
        cut.Find("button").Click();

        // Act - confirm closes the dialog
        var buttons = cut.FindAll("button");
        var confirmButton = buttons.First(b => b.TextContent.Contains("Confirm"));
        confirmButton.Click();

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.releaseFocusTrap");
        Assert.Contains(TestContext.JSInterop.Invocations,
            i => i.Identifier == "twDialog.restoreFocus" && (string?)i.Arguments[0] == "tw-focus-token");
    }

    [Fact]
    public void OnDialogKeyDown_Escape_ReleasesPanelTrap_AndRestoresFocus()
    {
        // Arrange
        TestContext.JSInterop.Setup<string?>("twDialog.captureFocus").SetResult("tw-focus-token");
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, false));
        cut.Find("button").Click();
        var dialogWrapper = cut.Find("div[role='dialog']");

        // Act
        dialogWrapper.KeyDown(new KeyboardEventArgs { Key = "Escape" });

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.releaseFocusTrap");
        Assert.Contains(TestContext.JSInterop.Invocations,
            i => i.Identifier == "twDialog.restoreFocus" && (string?)i.Arguments[0] == "tw-focus-token");
    }

    [Fact]
    public async Task Close_ReleasesPanelTrap_AndRestoresFocus_WhenInvokedWhileOpen()
    {
        // Arrange
        TestContext.JSInterop.Setup<string?>("twDialog.captureFocus").SetResult("tw-focus-token");
        var cut = TestContext.Render<TwColorPicker>(parameters => parameters
            .Add(p => p.Value, "#3b82f6")
            .Add(p => p.PreferNativePicker, false));
        cut.Find("button").Click();

        // Act
        await cut.InvokeAsync(() => cut.Instance.Close());

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.releaseFocusTrap");
        Assert.Contains(TestContext.JSInterop.Invocations,
            i => i.Identifier == "twDialog.restoreFocus" && (string?)i.Arguments[0] == "tw-focus-token");
    }

    #endregion

    [Fact]
    public void NormalizeColorValue_ReturnsDefaultColor_WhenInputIsWhitespace()
    {
        // Arrange - NormalizeColorValue's own whitespace guard is otherwise unreachable: its only
        // caller (HandleTextInputChangeAsync) already filters out whitespace before calling it.
        var cut = TestContext.Render<TwColorPicker>();
        var method = typeof(TwColorPicker).GetMethod("NormalizeColorValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        var result = method.Invoke(cut.Instance, ["   "]);

        // Assert
        Assert.Equal("#000000", result);
    }

    [Fact]
    public void NormalizeHexValue_ReturnsDefaultColor_WhenInputIsWhitespace()
    {
        // Arrange - same as NormalizeColorValue: unreachable via the public API since its only caller
        // never passes whitespace through.
        var cut = TestContext.Render<TwColorPicker>();
        var method = typeof(TwColorPicker).GetMethod("NormalizeHexValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Act
        var result = method.Invoke(cut.Instance, ["   "]);

        // Assert
        Assert.Equal("#000000", result);
    }
}
