using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;

namespace TwBlazor.Tests.Components.MaskedInput;

public class TwMaskedInputTests : TwBlazorTestBase
{
    private const string phoneMask = "(999) 999-9999";

    private const string codeMask = "AAA-9999";

    private TwInputTheme inputTheme => Theme.Components.Require<TwInputTheme>();

    private static ChangeEventArgs Text(string value) => new() { Value = value };

    private static KeyboardEventArgs Key(string key) => new() { Key = key };

    private string clearLabel => Localizer[TwStrings.Textfield_Clear];

    #region Rendering

    [Fact]
    public void Renders_ATextInput_BackedByTwTextfield()
    {
        // Act
        var cut = TestContext.Render<TwMaskedInput>(p => p.Add(x => x.Mask, phoneMask));

        // Assert
        Assert.NotNull(cut.FindComponent<TwTextfield<string>>());
        var input = cut.Find("input");
        Assert.Equal("text", input.GetAttribute("type"));
    }

    [Fact]
    public void ReusesTwTextfield_AndForwardsItsAppearance()
    {
        // Act
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.Variant, InputVariant.Outlined)
            .Add(x => x.Dense, true)
            .Add(x => x.Placeholder, "Phone number"));

        // Assert
        var textfield = cut.FindComponent<TwTextfield<string>>();
        Assert.Equal(InputVariant.Outlined, textfield.Instance.Variant);
        Assert.True(textfield.Instance.Dense);
        Assert.Equal("Phone number", cut.Find("input").GetAttribute("placeholder"));
        Assert.Contains(inputTheme.DenseSize, cut.Find("input").GetAttribute("class"));
    }

    [Fact]
    public void EmptyField_ShowsTheMask_ByDefault()
    {
        // Act
        var cut = TestContext.Render<TwMaskedInput>(p => p.Add(x => x.Mask, phoneMask));

        // Assert
        Assert.Equal("(___) ___-____", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void PlaceholderChar_ChangesTheUnfilledCharacters()
    {
        // Act
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.PlaceholderChar, '#'));

        // Assert
        Assert.Equal("(###) ###-####", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void ShowMaskFalse_LeavesAnEmptyFieldEmpty()
    {
        // Act
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.ShowMask, false));

        // Assert
        Assert.Equal(string.Empty, cut.Find("input").GetAttribute("value") ?? string.Empty);
    }

    [Fact]
    public void ShowMaskFalse_ShowsOnlyUpToTheLastTypedCharacter()
    {
        // Act
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.ShowMask, false)
            .Add(x => x.Value, "123"));

        // Assert
        Assert.Equal("(123", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void Value_IsRenderedThroughTheMask()
    {
        // Act
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.Value, "1234567890"));

        // Assert
        Assert.Equal("(123) 456-7890", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void Label_IsTiedToTheInput()
    {
        // Act
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.Id, "phone")
            .Add(x => x.Label, "Phone"));

        // Assert
        Assert.Equal("phone", cut.Find("label").GetAttribute("for"));
        Assert.Equal("phone", cut.Find("input").Id);
    }

    [Fact]
    public void Autocomplete_IsOff()
    {
        // Act
        var cut = TestContext.Render<TwMaskedInput>(p => p.Add(x => x.Mask, phoneMask));

        // Assert
        Assert.Equal("off", cut.Find("input").GetAttribute("autocomplete"));
    }

    [Theory]
    [InlineData(phoneMask, "numeric")]
    [InlineData("9999", "numeric")]
    [InlineData(codeMask, "text")]
    [InlineData("***", "text")]
    [InlineData("999-A", "text")]
    [InlineData("()", "text")]
    public void InputMode_IsNumeric_OnlyForADigitsOnlyMask(string mask, string expected)
    {
        // Act
        var cut = TestContext.Render<TwMaskedInput>(p => p.Add(x => x.Mask, mask));

        // Assert
        Assert.Equal(expected, cut.Find("input").GetAttribute("inputmode"));
    }

    [Fact]
    public void ConsumerAttributes_OverrideTheDefaults()
    {
        // Act
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .AddUnmatched("inputmode", "text")
            .AddUnmatched("data-test", "x"));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("text", input.GetAttribute("inputmode"));
        Assert.Equal("x", input.GetAttribute("data-test"));
    }

    [Fact]
    public void InputAttributes_AreNotCopiedOntoTheTextfieldWrapper()
    {
        // Act
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .AddUnmatched("data-test", "x"));

        // Assert - the input itself carries the input attributes...
        var input = cut.Find("input");
        Assert.Single(cut.FindAll("input"));
        Assert.Equal("off", input.GetAttribute("autocomplete"));
        Assert.Equal("numeric", input.GetAttribute("inputmode"));
        Assert.Equal("x", input.GetAttribute("data-test"));

        // ...but the text field's wrapper only has the marker the masked input put on it, so neither the input's
        // defaults nor the consumer's attributes are duplicated onto it.
        var wrapper = cut.Find("[data-tw-masked-input-field]");
        Assert.Equal("DIV", wrapper.TagName);
        Assert.False(wrapper.HasAttribute("autocomplete"));
        Assert.False(wrapper.HasAttribute("inputmode"));
        Assert.False(wrapper.HasAttribute("data-test"));
    }

    #endregion

    #region Value round-trip

    [Fact]
    public async Task ChangingTheField_SetsTheRawValue()
    {
        // Arrange
        var value = string.Empty;
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find("input").ChangeAsync(Text("(123) 456-7890"));

        // Assert
        Assert.Equal("1234567890", value);
        Assert.Equal("(123) 456-7890", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task ChangingTheField_DropsThePlaceholdersAndLiterals()
    {
        // Arrange
        var value = "x";
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find("input").ChangeAsync(Text("(12_) ___-____"));

        // Assert
        Assert.Equal("12", value);
        Assert.Equal("(12_) ___-____", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task ChangingTheField_WithACustomMask_SetsTheRawValue()
    {
        // Arrange
        var value = string.Empty;
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, codeMask)
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find("input").ChangeAsync(Text("ABC-1234"));

        // Assert
        Assert.Equal("ABC1234", value);
        Assert.Equal("ABC-1234", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task ChangingTheField_IsIgnored_WhenTheRawValueIsUnchanged()
    {
        // Arrange
        var changes = 0;
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.Value, "12")
            .Add(x => x.ValueChanged, _ => changes++));

        // Act
        await cut.Find("input").ChangeAsync(Text("(12_) ___-____"));

        // Assert
        Assert.Equal(0, changes);
    }

    [Fact]
    public void ExternalValueChanges_UpdateTheText()
    {
        // Arrange
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.Value, "1234567890"));
        Assert.Equal("(123) 456-7890", cut.Find("input").GetAttribute("value"));

        // Act
        cut.Render(p => p.Add(x => x.Value, "9876543210"));

        // Assert
        Assert.Equal("(987) 654-3210", cut.Find("input").GetAttribute("value"));
    }

    #endregion

    #region Validation

    [Fact]
    public async Task Validator_RunsWithTheRawValue_AndShowsItsMessage()
    {
        // Arrange
        string? validated = null;
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.Validator, v =>
            {
                validated = v;
                return Task.FromResult<string?>(v.Length < 10 ? "Enter the whole number." : null);
            }));

        // Act
        await cut.Find("input").ChangeAsync(Text("(123) 456-7890"));

        // Assert
        Assert.Equal("1234567890", validated);
        Assert.Empty(cut.FindAll("[role='alert']"));
        Assert.Null(cut.Find("input").GetAttribute("aria-invalid"));

        // Act
        await cut.Find("input").ChangeAsync(Text("(12_) ___-____"));

        // Assert
        Assert.Equal("12", validated);
        var alert = cut.Find("[role='alert']");
        Assert.Equal("Enter the whole number.", alert.TextContent);
        var input = cut.Find("input");
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Equal(alert.Id, input.GetAttribute("aria-describedby"));
    }

    [Fact]
    public void InvalidAndErrorMessage_AreShown()
    {
        // Act
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.Invalid, true)
            .Add(x => x.ErrorMessage, "That number is not in service."));

        // Assert
        Assert.Equal("That number is not in service.", cut.Find("[role='alert']").TextContent);
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-invalid"));
    }

    [Fact]
    public void ErrorMessage_IsHidden_WhileNotInvalid()
    {
        // Act
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.Invalid, false)
            .Add(x => x.ErrorMessage, "That number is not in service."));

        // Assert
        Assert.Empty(cut.FindAll("[role='alert']"));
        Assert.Null(cut.Find("input").GetAttribute("aria-invalid"));
    }

    [Fact]
    public void Required_IsPassedToTheInput()
    {
        // Act
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.Required, true));

        // Assert
        var input = cut.Find("input");
        Assert.True(input.HasAttribute("required"));
        Assert.Equal("true", input.GetAttribute("aria-required"));
    }

    #endregion

    #region Clearing

    [Fact]
    public void Clearable_ShowsTheTextfieldClearButton()
    {
        // Act
        var empty = TestContext.Render<TwMaskedInput>(p => p.Add(x => x.Mask, phoneMask).Add(x => x.Clearable, true));
        var filled = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.Value, "1234567890")
            .Add(x => x.Clearable, true));

        // Assert - an empty field still shows the mask, so the clear button follows the raw value (HasValue) rather
        // than the text in the field.
        Assert.False(empty.FindComponent<TwTextfield<string>>().Instance.HasValue);
        Assert.True(empty.Find($"button[aria-label='{clearLabel}']").HasAttribute("disabled"));

        Assert.True(filled.FindComponent<TwTextfield<string>>().Instance.HasValue);
        Assert.True(filled.FindComponent<TwTextfield<string>>().Instance.Clearable);
        Assert.False(filled.Find($"button[aria-label='{clearLabel}']").HasAttribute("disabled"));
    }

    [Fact]
    public async Task ClickingTheClearButton_OnAnEmptyField_LeavesTheMaskAlone()
    {
        // Arrange
        string? value = null;
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.Value, string.Empty)
            .Add(x => x.Clearable, true)
            .Add(x => x.ValueChanged, v => value = v));

        // Act - a browser never dispatches a click on a disabled button, so this is the harsher case
        await cut.Find($"button[aria-label='{clearLabel}']").ClickAsync(new MouseEventArgs());

        // Assert - there is nothing to clear, so neither the drawn mask nor the value moves
        Assert.Equal("(___) ___-____", cut.Find("input").GetAttribute("value"));
        Assert.Equal(string.Empty, cut.Instance.Value);
        Assert.Null(value);
    }

    [Fact]
    public async Task ClickingTheDisabledClearButton_StillReachesOnClear_InBUnit()
    {
        // This pins down where the guard lives: the clear button is only *disabled*, and a browser never dispatches a
        // click on it, but TwTextfield.ClearAsync does not itself check that there is a value. So anything that does
        // reach the handler - bUnit's synthetic click here, or a programmatic ClearAsync - still runs OnClear, even
        // though the value and the drawn mask are untouched. TwNumeric behaves the same way.
        // If this starts failing because OnClear no longer runs, the guard has moved into C# for every field.
        var cleared = false;
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.Clearable, true)
            .Add(x => x.OnClear, () => cleared = true));

        // Act
        await cut.Find($"button[aria-label='{clearLabel}']").ClickAsync(new MouseEventArgs());

        // Assert
        Assert.True(cleared);
        Assert.Equal(string.Empty, cut.Instance.Value);
        Assert.Equal("(___) ___-____", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task ClickingTheClearButton_EmptiesTheValue_AndInvokesOnClear()
    {
        // Arrange
        var value = "1234567890";
        var cleared = false;
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.Value, value)
            .Add(x => x.Clearable, true)
            .Add(x => x.ValueChanged, v => value = v)
            .Add(x => x.OnClear, () => cleared = true));

        // Act
        await cut.Find($"button[aria-label='{clearLabel}']").ClickAsync(new MouseEventArgs());

        // Assert
        Assert.Equal(string.Empty, value);
        Assert.True(cleared);
        Assert.Equal(string.Empty, cut.Instance.Value);
        Assert.Equal("(___) ___-____", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task ClearAsync_EmptiesTheValue_AndInvokesOnClear()
    {
        // Arrange
        var value = "1234567890";
        var cleared = false;
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.Value, value)
            .Add(x => x.ValueChanged, v => value = v)
            .Add(x => x.OnClear, () => cleared = true));

        // Act
        await cut.InvokeAsync(cut.Instance.ClearAsync);

        // Assert - same outcome as the button, and the mask is re-drawn
        Assert.Equal(string.Empty, value);
        Assert.True(cleared);
        Assert.Equal(string.Empty, cut.Instance.Value);
        Assert.Equal("(___) ___-____", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task ClearAsync_RestoresTheMask_EvenWhenTheValueIsAlreadyEmpty()
    {
        // Arrange - the field is empty, so ClearAsync has no value to report but must still redraw the mask
        var changes = 0;
        var cleared = false;
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.ValueChanged, _ => changes++)
            .Add(x => x.OnClear, () => cleared = true));

        // Act
        await cut.InvokeAsync(cut.Instance.ClearAsync);

        // Assert
        Assert.Equal("(___) ___-____", cut.Find("input").GetAttribute("value"));
        Assert.Equal(0, changes);
        Assert.True(cleared);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task ClearAsync_DoesNothing_WhenDisabledOrReadOnly(bool disabled, bool readOnly)
    {
        // Arrange
        var cleared = false;
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.Value, "1234567890")
            .Add(x => x.Disabled, disabled)
            .Add(x => x.ReadOnly, readOnly)
            .Add(x => x.OnClear, () => cleared = true));

        // Act
        await cut.InvokeAsync(cut.Instance.ClearAsync);

        // Assert
        Assert.Equal("1234567890", cut.Instance.Value);
        Assert.Equal("(123) 456-7890", cut.Find("input").GetAttribute("value"));
        Assert.False(cleared);
    }

    #endregion

    #region Mask and value consistency

    [Fact]
    public void ShrinkingTheMask_TruncatesTheValue_AndReformatsTheText()
    {
        // Arrange
        var changes = new List<string>();
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.Value, "1234567890")
            .Add(x => x.ValueChanged, v => changes.Add(v)));
        Assert.Equal("(123) 456-7890", cut.Find("input").GetAttribute("value"));

        // Act
        cut.Render(p => p.Add(x => x.Mask, "999-999"));

        // Assert - the new mask holds six characters, so both the value and the display shrink to that prefix
        Assert.Equal("123456", cut.Instance.Value);
        Assert.Equal("123-456", cut.Find("input").GetAttribute("value"));
        Assert.Equal(["123456"], changes);
    }

    [Fact]
    public void GrowingTheMask_KeepsTheValue_WithoutReportingAChange()
    {
        // Arrange
        var changes = 0;
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, "999-999")
            .Add(x => x.Value, "123456")
            .Add(x => x.ValueChanged, _ => changes++));

        // Act
        cut.Render(p => p.Add(x => x.Mask, phoneMask));

        // Assert - the wider mask only adds placeholders, so the value still fits and is left alone
        Assert.Equal("123456", cut.Instance.Value);
        Assert.Equal("(123) 456-____", cut.Find("input").GetAttribute("value"));
        Assert.Equal(0, changes);
    }

    [Theory]
    [InlineData("9999", "12A4", "12", "12__")]
    [InlineData("9999", "123456", "1234", "1234")]
    [InlineData("9999", "A123", "", "____")]
    [InlineData("9999", "1234A", "1234", "1234")]
    [InlineData("AAAA", "AB1C", "AB", "AB__")]
    public void ValueWithCharactersTheMaskRejects_IsTruncatedAndReported(string mask, string value, string expectedValue, string expectedText)
    {
        // Arrange
        var changes = new List<string>();
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, mask)
            .Add(x => x.Value, value)
            .Add(x => x.ValueChanged, v => changes.Add(v)));

        // Assert - the model and the display agree, the rejected character is never drawn, and the truncation is
        // reported once so a bound model follows the field
        Assert.Equal(expectedValue, cut.Instance.Value);
        Assert.Equal(expectedText, cut.Find("input").GetAttribute("value"));
        Assert.Equal([expectedValue], changes);
    }

    [Fact]
    public void AValueTheMaskAccepts_IsNotReportedAsAChange()
    {
        // Arrange
        var changes = 0;

        // Act
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, "9999")
            .Add(x => x.Value, "1234")
            .Add(x => x.ValueChanged, _ => changes++));

        // Assert
        Assert.Equal("1234", cut.Instance.Value);
        Assert.Equal("1234", cut.Find("input").GetAttribute("value"));
        Assert.Equal(0, changes);
    }

    [Theory]
    [InlineData("9999", "١٢٣٤", "", "____")]
    [InlineData("AAAA", "ÄBC", "", "____")]
    [InlineData("****", "abä", "ab", "ab__")]
    public void MaskTokens_AcceptAsciiLettersAndDigitsOnly(string mask, string value, string expectedValue, string expectedText)
    {
        // Arrange
        var changes = new List<string>();
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, mask)
            .Add(x => x.Value, value)
            .Add(x => x.ValueChanged, v => changes.Add(v)));

        // Assert - a non-ASCII digit or letter is not a mask token match, so filling stops there
        Assert.Equal(expectedValue, cut.Instance.Value);
        Assert.Equal(expectedText, cut.Find("input").GetAttribute("value"));
        Assert.Equal([expectedValue], changes);
    }

    [Fact]
    public void AnEmptyMask_RendersAPlainEmptyField()
    {
        // Act
        var cut = TestContext.Render<TwMaskedInput>(p => p.Add(x => x.Mask, string.Empty));

        // Assert - no tokens means nothing to draw and nothing to enforce: a plain, empty text field
        var input = cut.Find("input");
        Assert.Equal(string.Empty, input.GetAttribute("value") ?? string.Empty);
        Assert.Equal(string.Empty, cut.Instance.Value);
        Assert.Equal("text", input.GetAttribute("inputmode"));
    }

    [Fact]
    public async Task AMaskWithNoTokens_DrawsItsLiterals_AndHoldsNoValue()
    {
        // Arrange
        var changes = 0;
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, "()-")
            .Add(x => x.ValueChanged, _ => changes++));

        // The literals are drawn, but there is no fillable position, so the field can never hold a value.
        Assert.Equal("()-", cut.Find("input").GetAttribute("value"));
        Assert.Equal(string.Empty, cut.Instance.Value);

        // Act - committing the drawn text strips back to an empty raw value, which is already the value
        await cut.Find("input").ChangeAsync(Text("()-"));

        // Assert
        Assert.Equal(0, changes);
        Assert.Equal(string.Empty, cut.Instance.Value);
    }

    #endregion

    #region Committing

    [Fact]
    public async Task OnInput_UpdatesTheValueOnEveryKeystroke()
    {
        // Arrange
        var value = string.Empty;
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.BindEvent, "oninput")
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find("input").InputAsync(Text("1234"));

        // Assert - the raw value is committed without waiting for the field to lose focus, and the mask is redrawn
        Assert.Equal("1234", value);
        Assert.Equal("1234", cut.Instance.Value);
        Assert.Equal("(123) 4__-____", cut.Find("input").GetAttribute("value"));
    }

    #endregion

    #region Script

    [Fact]
    public void Script_IsAttached_Once_WithTheMask()
    {
        // Act
        TestContext.Render<TwMaskedInput>(p => p.Add(x => x.Mask, phoneMask));

        // Assert
        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twMaskedInput.attach");
        Assert.IsType<ElementReference>(invocation.Arguments[0]);
        Assert.Equal(phoneMask, invocation.Arguments[1]);
        Assert.Equal("_", invocation.Arguments[2]);
        Assert.True(Assert.IsType<bool>(invocation.Arguments[3]));
    }

    [Fact]
    public void Script_IsAttached_WithThePlaceholderCharAndShowMask()
    {
        // Act
        TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.PlaceholderChar, '#')
            .Add(x => x.ShowMask, false));

        // Assert
        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twMaskedInput.attach");
        Assert.Equal("#", invocation.Arguments[2]);
        Assert.False(Assert.IsType<bool>(invocation.Arguments[3]));
    }

    [Fact]
    public void Attaching_ExposesTheInputReference()
    {
        // Act
        var cut = TestContext.Render<TwMaskedInput>(p => p.Add(x => x.Mask, phoneMask));

        // Assert
        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twMaskedInput.attach");
        Assert.Equal(Assert.IsType<ElementReference>(invocation.Arguments[0]), cut.Instance.InputRef);
    }

    [Fact]
    public void ChangingTheMask_UpdatesTheScript()
    {
        // Arrange
        var cut = TestContext.Render<TwMaskedInput>(p => p.Add(x => x.Mask, phoneMask));

        // Act
        cut.Render(p => p.Add(x => x.Mask, codeMask));

        // Assert
        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twMaskedInput.update");
        Assert.Equal(codeMask, invocation.Arguments[1]);
    }

    [Fact]
    public void UnchangedParameters_DoNotUpdateTheScript()
    {
        // Arrange
        var cut = TestContext.Render<TwMaskedInput>(p => p.Add(x => x.Mask, phoneMask));

        // Act
        cut.Render(p => p.Add(x => x.Mask, phoneMask).Add(x => x.Value, "1234567890"));

        // Assert
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twMaskedInput.update");
    }

    [Fact]
    public async Task Script_IsDetached_OnDispose()
    {
        // Arrange
        var cut = TestContext.Render<TwMaskedInput>(p => p.Add(x => x.Mask, phoneMask));

        // Act
        await cut.Instance.DisposeAsync();

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twMaskedInput.detach");
    }

    #endregion

    #region Callbacks

    [Fact]
    public async Task Focus_EtAl_AreForwardedToTheInput()
    {
        // Arrange
        string? pressed = null;
        var focused = false;
        var blurred = false;
        var clicked = false;
        var cut = TestContext.Render<TwMaskedInput>(p => p
            .Add(x => x.Mask, phoneMask)
            .Add(x => x.OnFocus, _ => focused = true)
            .Add(x => x.OnBlur, _ => blurred = true)
            .Add(x => x.OnClick, _ => clicked = true)
            .Add(x => x.OnKeyDown, args => pressed = args.Key));

        // Act
        await cut.Find("input").FocusAsync(new FocusEventArgs());
        await cut.Find("input").ClickAsync(new MouseEventArgs());
        await cut.Find("input").KeyDownAsync(Key("Tab"));
        await cut.Find("input").BlurAsync(new FocusEventArgs());

        // Assert
        Assert.True(focused);
        Assert.True(blurred);
        Assert.True(clicked);
        Assert.Equal("Tab", pressed);
    }

    [Fact]
    public async Task FocusAsync_MovesFocusToTheInput()
    {
        // Arrange
        var cut = TestContext.Render<TwMaskedInput>(p => p.Add(x => x.Mask, phoneMask));

        // Act
        await cut.InvokeAsync(cut.Instance.FocusAsync);

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "Blazor._internal.domWrapper.focus");
    }

    #endregion
}
