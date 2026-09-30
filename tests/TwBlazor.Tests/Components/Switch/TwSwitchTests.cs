using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Switch;

public class TwSwitchTests : TwBlazorTestBase
{
    [Fact]
    public void ShouldRender_Checkbox_Input()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Value, false)
        );

        // Assert
        var input = cut.Find("input[type='checkbox']");
        Assert.NotNull(input);
    }

    [Fact]
    public void ShouldRender_WithLabel()
    {
        // Arrange
        var labelText = "Enable notifications";

        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Label, labelText)
            .Add(x => x.Value, false)
        );

        // Assert
        var label = cut.Find("label");
        Assert.NotNull(label);
        Assert.Contains(labelText, cut.Markup);
    }

    [Fact]
    public void ShouldRender_WithGeneratedId_WhenIdNotProvided()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Value, false)
        );

        // Assert
        var input = cut.Find("input");
        var id = input.GetAttribute("id");
        Assert.NotNull(id);
        Assert.StartsWith("switch-", id);
    }

    [Fact]
    public void ShouldRender_WithCustomId_WhenIdProvided()
    {
        // Arrange
        var customId = "my-switch";

        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Id, customId)
            .Add(x => x.Value, false)
        );

        // Assert
        var input = cut.Find("input");
        Assert.Equal(customId, input.GetAttribute("id"));
    }

    [Fact]
    public void ShouldRender_WithName()
    {
        // Arrange
        var name = "toggle";

        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Name, name)
            .Add(x => x.Value, false)
        );

        // Assert
        var input = cut.Find("input");
        Assert.Equal(name, input.GetAttribute("name"));
    }

    [Fact]
    public void ShouldRender_Checked_WhenValueIsTrue()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Value, true)
        );

        // Assert
        var input = cut.Find("input");
        Assert.True(input.HasAttribute("checked"));
    }

    [Fact]
    public void ShouldRender_Unchecked_WhenValueIsFalse()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Value, false)
        );

        // Assert
        var input = cut.Find("input");
        Assert.False(input.HasAttribute("checked"));
    }

    [Fact]
    public void ClickingSwitch_TogglesValue_And_InvokesCallback()
    {
        // Arrange
        bool? valueFromCallback = null;
        var initialValue = false;

        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Value, initialValue)
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<bool>(this, v => valueFromCallback = v))
        );

        var input = cut.Find("input");
        input.Change(true);

        // Assert
        Assert.NotNull(valueFromCallback);
        Assert.True(valueFromCallback.Value);
    }

    [Fact]
    public void ShouldRender_WithCustomClass()
    {
        // Arrange
        var customClass = "my-custom-class";

        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Class, customClass)
            .Add(x => x.Value, false)
        );

        // Assert
        var input = cut.Find("input");
        Assert.Contains(customClass, input.GetAttribute("class"));
    }

    [Fact]
    public void ShouldRender_WithCustomLabelClass()
    {
        // Arrange
        var customLabelClass = "my-label-class";

        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.LabelClass, customLabelClass)
            .Add(x => x.Label, "Test Label")
            .Add(x => x.Value, false)
        );

        // Assert
        var label = cut.Find("label");
        Assert.Contains(customLabelClass, label.GetAttribute("class"));
    }

    [Fact]
    public void ShouldRender_WithLabelOnRight()
    {
        // Arrange
        var labelText = "Settings";

        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Label, labelText)
            .Add(x => x.Value, false)
        );

        // Assert
        var label = cut.Find("label");
        Assert.Contains("inline-flex", label.GetAttribute("class"));
        Assert.Contains("items-center", label.GetAttribute("class"));
        Assert.Contains("gap-2", label.GetAttribute("class"));
    }

    [Fact]
    public void ShouldRender_Label_WithoutConflictingDisplayClass()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Label, "Settings")
            .Add(x => x.Value, false)
        );

        // Assert
        var classList = cut.Find("label").GetAttribute("class")!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Assert.DoesNotContain("block", classList);
    }

    [Fact]
    public void ShouldRender_SwitchVisuals_WithCorrectStructure()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Value, false)
            .Add(x => x.Color, Color.Primary));

        // Assert
        var spans = cut.FindAll("span");
        Assert.True(spans.Count >= 3); // Container + background track + toggle circle

        // Check for track (background)
        Assert.Contains(spans, s => s.GetAttribute("class")?.Contains("bg-gray-300") == true);
        Assert.Contains(spans, s => s.GetAttribute("class")?.Contains("peer-checked:bg-purple-600") == true);

        // Check for toggle circle
        Assert.Contains(spans, s => s.GetAttribute("class")?.Contains("bg-white") == true);
        Assert.Contains(spans, s => s.GetAttribute("class")?.Contains("peer-checked:translate-x-full") == true);
    }

    [Fact]
    public void ShouldRender_DarkModeClasses()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Value, false)
            .Add(x => x.Color, Color.Primary));

        // Assert
        var markup = cut.Markup;
        Assert.Contains("dark:bg-[oklch(21.15%_0.012_254.09)]", markup); // Track background
        Assert.Contains("dark:peer-checked:bg-purple-500", markup); // Checked state color
    }

    [Fact]
    public void ShouldRender_WithDisabledClasses()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Value, false)
        );

        // Assert
        var markup = cut.Markup;
        Assert.Contains("peer-disabled:opacity-40", markup);
        Assert.Contains("peer-disabled:pointer-events-none", markup);
    }

    [Fact]
    public void SwitchContainer_HasCorrectDimensions()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Value, false)
        );

        // Assert
        var container = cut.Find("span.relative.inline-block");
        Assert.Contains("w-10", container.GetAttribute("class"));
        Assert.Contains("h-6", container.GetAttribute("class"));
    }

    [Fact]
    public void Input_HasPeerClass_ForPeerModifiers()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Value, false)
        );

        // Assert
        var input = cut.Find("input");
        Assert.Contains("peer", input.GetAttribute("class"));
        Assert.Contains("sr-only", input.GetAttribute("class"));
    }

    [Fact]
    public void Label_HasCursorPointer()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Label, "Test")
            .Add(x => x.Value, false)
        );

        // Assert
        var label = cut.Find("label");
        Assert.Contains("cursor-pointer", label.GetAttribute("class"));
    }

    [Fact]
    public void ShouldRender_WithDefaultPurpleColor_WhenColorNotSpecified()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Value, false)
        );

        // Assert - When Color is not specified (null), GetSwitchColor falls back to the Primary
        // (purple) color rather than omitting a color class - see TwSwitch.GetSwitchColor.
        var markup = cut.Markup;
        Assert.Contains("peer-checked:bg-purple-600", markup);
    }

    [Fact]
    public void ShouldRender_WithGreenColor_WhenColorIsGreen()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Color, Color.Success)
            .Add(x => x.Value, false)
        );

        // Assert
        var markup = cut.Markup;
        Assert.Contains("peer-checked:bg-green-600", markup);
        Assert.Contains("dark:peer-checked:bg-green-500", markup);
    }

    [Fact]
    public void ShouldRender_WithFuchsiaColor_WhenColorIsAccent()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Color, Color.Accent)
            .Add(x => x.Value, false)
        );

        // Assert
        var markup = cut.Markup;
        Assert.Contains("peer-checked:bg-fuchsia-600", markup);
        Assert.Contains("dark:peer-checked:bg-fuchsia-500", markup);
    }

    [Fact]
    public void ShouldRender_WithRedColor_WhenColorIsRed()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Color, Color.Danger)
            .Add(x => x.Value, false)
        );

        // Assert
        var markup = cut.Markup;
        Assert.Contains("peer-checked:bg-red-600", markup);
        Assert.Contains("dark:peer-checked:bg-red-500", markup);
    }

    [Theory]
    [InlineData(Color.Danger, "red")]
    [InlineData(Color.Accent, "fuchsia")]
    [InlineData(Color.Success, "green")]
    [InlineData(Color.Primary, "purple")]
    [InlineData(Color.Warning, "yellow")]
    [InlineData(Color.Info, "blue")]
    public void ShouldRender_WithCorrectColor_ForAllColors(Color color, string colorName)
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Color, color)
            .Add(x => x.Value, false)
        );

        // Assert
        var markup = cut.Markup;
        Assert.Contains($"peer-checked:bg-{colorName}-", markup);
    }

    [Fact]
    public void ShouldRender_WithPointerEventsNone_WhenReadonly()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.ReadOnly, true)
            .Add(x => x.Label, "Test")
            .Add(x => x.Value, false)
        );

        // Assert
        var label = cut.Find("label");
        Assert.Contains("pointer-events-none", label.GetAttribute("class"));
        Assert.DoesNotContain("opacity-40", label.GetAttribute("class"));
    }

    [Fact]
    public void ReadonlySwitch_PreventsClickEvents()
    {
        // Arrange
        bool? valueFromCallback = null;
        var initialValue = false;

        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.ReadOnly, true)
            .Add(x => x.Value, initialValue)
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<bool>(this, v => valueFromCallback = v))
        );

        var input = cut.Find("input");

        // Try to click the input (should be prevented by readonly)
        try
        {
            input.Click();
        }
        catch (Exception)
        {
            // Click may be prevented
        }

        // Assert - callback should not have been invoked
        Assert.Null(valueFromCallback);
    }

    [Fact]
    public void ShouldRender_WithReadonlyAndChecked()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.ReadOnly, true)
            .Add(x => x.Value, true)
        );

        // Assert
        var input = cut.Find("input");
        Assert.True(input.HasAttribute("checked"));

        // Readonly switch should have pointer-events-none on label
        var markup = cut.Markup;
        Assert.Contains("pointer-events-none", markup);
    }

    [Fact]
    public void ReadonlySwitch_WithLabel_ShowsCurrentState()
    {
        // Arrange
        var labelText = "Readonly setting";

        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.ReadOnly, true)
            .Add(x => x.Label, labelText)
            .Add(x => x.Value, true)
        );

        // Assert
        Assert.Contains(labelText, cut.Markup);
        var input = cut.Find("input");
        Assert.True(input.HasAttribute("checked"));

        // Verify readonly behavior through pointer-events-none
        var label = cut.Find("label");
        Assert.Contains("pointer-events-none", label.GetAttribute("class"));
    }

    [Fact]
    public void ShouldRender_WithDisabledAttribute_WhenDisabledIsTrue()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Disabled, true)
            .Add(x => x.Value, false)
        );

        // Assert
        var input = cut.Find("input");
        Assert.True(input.HasAttribute("disabled"));
    }

    [Fact]
    public void ShouldRender_WithoutDisabledAttribute_WhenDisabledIsFalse()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Disabled, false)
            .Add(x => x.Value, false)
        );

        // Assert
        var input = cut.Find("input");
        Assert.False(input.HasAttribute("disabled"));
    }

    [Fact]
    public void ShouldRender_WithOpacityAndPointerEventsNone_WhenDisabled()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Disabled, true)
            .Add(x => x.Label, "Test")
            .Add(x => x.Value, false)
        );

        // Assert
        var label = cut.Find("label");
        Assert.Contains("pointer-events-none", label.GetAttribute("class"));
        Assert.Contains("opacity-40", label.GetAttribute("class"));
    }

    [Fact]
    public void DisabledSwitch_DoesNotInvokeCallback_OnChange()
    {
        // Arrange
        bool? valueFromCallback = null;
        var initialValue = false;

        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Disabled, true)
            .Add(x => x.Value, initialValue)
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<bool>(this, v => valueFromCallback = v))
        );

        var input = cut.Find("input");

        // Try to change the value (should be blocked by disabled)
        try
        {
            input.Change(true);
        }
        catch (Exception)
        {
            // Disabled inputs may throw or ignore changes
        }

        // Assert - callback should not have been invoked
        Assert.Null(valueFromCallback);
    }

    [Fact]
    public void ShouldRender_WithDisabledAndChecked()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Disabled, true)
            .Add(x => x.Value, true)
        );

        // Assert
        var input = cut.Find("input");
        Assert.True(input.HasAttribute("disabled"));
        Assert.True(input.HasAttribute("checked"));
    }

    [Fact]
    public void DisabledSwitch_WithLabel_ShowsDimmedState()
    {
        // Arrange
        var labelText = "Disabled setting";

        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Disabled, true)
            .Add(x => x.Label, labelText)
            .Add(x => x.Value, true)
        );

        // Assert
        Assert.Contains(labelText, cut.Markup);
        var label = cut.Find("label");
        Assert.Contains("opacity-40", label.GetAttribute("class"));
        var input = cut.Find("input");
        Assert.True(input.HasAttribute("checked"));
        Assert.True(input.HasAttribute("disabled"));
    }

    [Fact]
    public void ReadonlySwitch_MaintainsFullOpacity()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.ReadOnly, true)
            .Add(x => x.Label, "Test")
            .Add(x => x.Value, true)
        );

        // Assert - Readonly should NOT have opacity-40
        var label = cut.Find("label");
        Assert.DoesNotContain("opacity-40", label.GetAttribute("class"));
        Assert.Contains("pointer-events-none", label.GetAttribute("class"));
    }

    [Fact]
    public void DisabledSwitch_ShowsPeerDisabledStyles()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Disabled, true)
            .Add(x => x.Value, false)
        );

        // Assert - Track should have peer-disabled styles
        var markup = cut.Markup;
        Assert.Contains("peer-disabled:opacity-40", markup);
        Assert.Contains("peer-disabled:pointer-events-none", markup);
    }

    [Fact]
    public void ShouldGenerate_UniqueIds_ForMultipleInstances()
    {
        // Act
        var cut1 = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Value, false)
        );
        var cut2 = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Value, false)
        );

        // Assert
        var id1 = cut1.Find("input").GetAttribute("id");
        var id2 = cut2.Find("input").GetAttribute("id");
        Assert.NotEqual(id1, id2);
        Assert.DoesNotContain("`", id1); // Should not contain generic type indicator
        Assert.DoesNotContain("`", id2);
    }

    [Fact]
    public void GeneratedId_DoesNotContain_GenericTypeIndicator()
    {
        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Value, false)
        );

        // Assert
        var id = cut.Find("input").GetAttribute("id");
        Assert.NotNull(id);
        Assert.DoesNotContain("`", id);
        Assert.StartsWith("switch-", id); // Should not have backtick in the prefix
        Assert.DoesNotContain("switch`1", id); // Should not contain the generic type indicator
    }

    [Fact]
    public void ShouldRender_Unchecked_WhenValueIsNotBool()
    {
        // Act - isChecked's `Value is bool boolValue` pattern match should fail gracefully
        // for a non-bool TValue rather than throwing.
        var cut = TestContext.Render<TwSwitch<string>>(p => p
            .Add(x => x.Value, "not-a-bool")
        );

        // Assert
        var input = cut.Find("input");
        Assert.False(input.HasAttribute("checked"));
    }

    [Fact]
    public void ReadonlySwitch_ChangeEvent_DoesNotInvokeCallback()
    {
        // Arrange - exercises the ReadOnly branch of HandleChange via a real change event
        // (Click() never reaches HandleChange since there's no @onclick handler wired up).
        bool? valueFromCallback = null;

        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.ReadOnly, true)
            .Add(x => x.Value, false)
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<bool>(this, v => valueFromCallback = v))
        );

        var input = cut.Find("input");

        // Act
        input.Change(true);

        // Assert
        Assert.Null(valueFromCallback);
    }

    [Fact]
    public void HandleChange_DoesNotInvokeCallback_WhenEventValueIsNotBool()
    {
        // Arrange - exercises the `e.Value is bool newValue` false branch of HandleChange.
        var callbackInvoked = false;

        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Value, false)
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<bool>(this, _ => callbackInvoked = true))
        );

        var input = cut.Find("input");

        // Act
        input.Change("not-a-bool");

        // Assert
        Assert.False(callbackInvoked);
    }

    [Fact]
    public void TwSwitch_AppliesAriaInvalid_WhenInvalid()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Invalid, true)
            .Add(x => x.Value, false));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
    }

    [Fact]
    public void TwSwitch_DoesNotApplyAriaInvalid_WhenNotInvalid()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Value, false));

        // Assert
        var input = cut.Find("input");
        Assert.False(input.HasAttribute("aria-invalid"));
    }

    [Fact]
    public void TwSwitch_SetsAriaDescribedBy_ToErrorId_WhenInvalidWithErrorMessage()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Id, "notify-switch")
            .Add(x => x.Invalid, true)
            .Add(x => x.ErrorMessage, "Something went wrong")
            .Add(x => x.Value, false));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("notify-switch-error", input.GetAttribute("aria-describedby"));

        var error = cut.Find("p[role='alert']");
        Assert.Contains("Something went wrong", error.TextContent);
    }

    [Fact]
    public void TwSwitch_SetsAriaLabel_WhenProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.AriaLabel, "Enable notifications")
            .Add(x => x.Value, false));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("Enable notifications", input.GetAttribute("aria-label"));
    }

    [Theory]
    [InlineData(Color.Light)]
    [InlineData(Color.Dark)]
    public void GetSwitchColor_ReturnsThemeColor_ForLightAndDarkColors(Color color)
    {
        // Arrange - GetSwitchColor's switch expression has a distinct branch per Color value; the
        // Theory above (ShouldRender_WithCorrectColor_ForAllColors) only covers
        // Danger/Accent/Success/Primary/Warning/Info, leaving Light/Dark untested.
        var switchTheme = Theme.Components.Require<TwSwitchTheme>();
        var expected = color == Color.Light ? switchTheme.Colors.Light : switchTheme.Colors.Dark;

        // Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Color, color)
            .Add(x => x.Value, false));

        // Assert
        Assert.Contains(expected, cut.Markup);
    }

    [Fact]
    public void TwSwitch_SetsAriaLabelledBy_WhenProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.AriaLabelledBy, "notify-label")
            .Add(x => x.Value, false));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("notify-label", input.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void ShouldOutlineToggleAndTrack_SoTheCheckedKnobEdgeStaysVisible()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p
            .Add(x => x.Value, true)
            .Add(x => x.Color, Color.Primary));

        // Assert - regression: a large soft shadow-lg on the knob smudged its edge against the colored track.
        var spans = cut.FindAll("span");
        var toggle = Assert.Single(spans, s => s.GetAttribute("class")?.Contains("peer-checked:translate-x-full") == true);
        var toggleClasses = toggle.GetAttribute("class")!;
        Assert.Contains("ring-1", toggleClasses);
        Assert.DoesNotContain("shadow-lg", toggleClasses);

        var track = Assert.Single(spans, s => s.GetAttribute("class")?.Contains("ring-inset") == true);
        Assert.Contains("ring-1", track.GetAttribute("class"));
    }
}
