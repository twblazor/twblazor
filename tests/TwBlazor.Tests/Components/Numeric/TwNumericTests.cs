using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;

namespace TwBlazor.Tests.Components.Numeric;

public class TwNumericTests : TwBlazorTestBase
{
    private TwNumericTheme numericTheme => Theme.Components.Require<TwNumericTheme>();

    private TwInputTheme inputTheme => Theme.Components.Require<TwInputTheme>();

    private static ChangeEventArgs Text(string value) => new() { Value = value };

    private static KeyboardEventArgs Key(string key) => new() { Key = key };

    private string increaseLabel => Localizer[TwStrings.Numeric_Increase];

    private string decreaseLabel => Localizer[TwStrings.Numeric_Decrease];

    private string clearLabel => Localizer[TwStrings.Textfield_Clear];

    #region Rendering

    [Fact]
    public void Renders_ATextInputWithTheSpinbuttonRole()
    {
        // Act
        var cut = TestContext.Render<TwNumeric<int>>(p => p.Add(x => x.Value, 42));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("text", input.GetAttribute("type"));
        Assert.Equal("spinbutton", input.GetAttribute("role"));
        Assert.Equal("42", input.GetAttribute("value"));
        Assert.Equal("42", input.GetAttribute("aria-valuenow"));
        Assert.Equal("off", input.GetAttribute("autocomplete"));
    }

    [Fact]
    public void ReusesTwTextfield()
    {
        // Act
        var cut = TestContext.Render<TwNumeric<int>>(p => p
            .Add(x => x.Variant, InputVariant.Outlined)
            .Add(x => x.Dense, true)
            .Add(x => x.Placeholder, "How many?"));

        // Assert
        var textfield = cut.FindComponent<TwTextfield<string>>();
        Assert.Equal(InputVariant.Outlined, textfield.Instance.Variant);
        Assert.True(textfield.Instance.Dense);
        Assert.Equal("How many?", cut.Find("input").GetAttribute("placeholder"));
        Assert.Contains(inputTheme.DenseSize, cut.Find("input").GetAttribute("class"));
    }

    [Fact]
    public void Label_IsTiedToTheInput()
    {
        // Act
        var cut = TestContext.Render<TwNumeric<int>>(p => p.Add(x => x.Id, "quantity").Add(x => x.Label, "Quantity"));

        // Assert
        Assert.Equal("quantity", cut.Find("label").GetAttribute("for"));
        Assert.Equal("quantity", cut.Find("input").Id);
    }

    [Theory]
    [InlineData(typeof(int), "numeric")]
    [InlineData(typeof(long), "numeric")]
    [InlineData(typeof(byte), "numeric")]
    [InlineData(typeof(decimal), "decimal")]
    [InlineData(typeof(double), "decimal")]
    [InlineData(typeof(float), "decimal")]
    [InlineData(typeof(int?), "numeric")]
    [InlineData(typeof(decimal?), "decimal")]
    public void InputMode_MatchesTheType(Type type, string expected)
    {
        // Act
        var cut = TestContext.Render(builder =>
        {
            builder.OpenComponent(0, typeof(TwNumeric<>).MakeGenericType(type));
            builder.CloseComponent();
        });

        // Assert
        Assert.Equal(expected, cut.Find("input").GetAttribute("inputmode"));
    }

    [Fact]
    public void UnsupportedType_Throws()
    {
        // Act & Assert
        var exception = Assert.Throws<TypeInitializationException>(() => TestContext.Render<TwNumeric<string>>());
        Assert.IsType<NotSupportedException>(exception.GetBaseException());
    }

    [Fact]
    public void AriaValueMinAndMax_AreOnlySetForExplicitLimits()
    {
        // Act
        var unbounded = TestContext.Render<TwNumeric<int>>();
        var bounded = TestContext.Render<TwNumeric<int>>(p => p.Add(x => x.Min, 1).Add(x => x.Max, 8));

        // Assert
        Assert.Null(unbounded.Find("input").GetAttribute("aria-valuemin"));
        Assert.Null(unbounded.Find("input").GetAttribute("aria-valuemax"));
        Assert.Equal("1", bounded.Find("input").GetAttribute("aria-valuemin"));
        Assert.Equal("8", bounded.Find("input").GetAttribute("aria-valuemax"));
    }

    [Fact]
    public void NullValue_RendersEmpty_WithoutAriaValueNow()
    {
        // Act
        var cut = TestContext.Render<TwNumeric<int?>>();

        // Assert
        var input = cut.Find("input");
        Assert.Equal(string.Empty, input.GetAttribute("value") ?? string.Empty);
        Assert.Null(input.GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void Format_IsAppliedInTheCulture()
    {
        // Act
        var cut = TestContext.Render<TwNumeric<decimal>>(p => p.Add(x => x.Value, 1250.5m).Add(x => x.Format, "N2"));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("1,250.50", input.GetAttribute("value"));
        Assert.Equal("1250.5", input.GetAttribute("aria-valuenow"));
        Assert.Equal("1,250.50", input.GetAttribute("aria-valuetext"));
    }

    [Fact]
    public void Culture_ChangesTheSeparators()
    {
        // Act
        var cut = TestContext.Render<TwNumeric<decimal>>(p => p
            .Add(x => x.Value, 1250.5m)
            .Add(x => x.Format, "N2")
            .Add(x => x.Culture, CultureInfo.GetCultureInfo("de-DE")));

        // Assert
        Assert.Equal("1.250,50", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void AriaValueText_IsOmitted_WhenItMatchesTheNumber()
    {
        // Act
        var cut = TestContext.Render<TwNumeric<int>>(p => p.Add(x => x.Value, 7));

        // Assert
        Assert.Null(cut.Find("input").GetAttribute("aria-valuetext"));
    }

    [Fact]
    public void PrefixAndSuffix_AreShown_AndReadOutWithTheValue()
    {
        // Act
        var cut = TestContext.Render<TwNumeric<int>>(p => p.Add(x => x.Value, 75).Add(x => x.Prefix, "£").Add(x => x.Suffix, "kg"));

        // Assert
        var adornments = cut.FindAll($"span[aria-hidden='true']").Where(span => span.ClassName == numericTheme.Adornment).Select(span => span.TextContent).ToArray();
        Assert.Equal(["£", "kg"], adornments);
        Assert.Equal("£ 75 kg", cut.Find("input").GetAttribute("aria-valuetext"));
    }

    [Fact]
    public void PrefixAndSuffix_WidenThePadding()
    {
        // Act
        var cut = TestContext.Render<TwNumeric<int>>(p => p.Add(x => x.Prefix, "£").Add(x => x.Suffix, "kg"));

        // Assert
        var style = cut.Find("input").GetAttribute("style");
        Assert.Contains("padding-inline-start: calc(0.75rem + 1ch + 0.25rem)", style);
        Assert.Contains("padding-inline-end: calc(2ch + 2 * 2rem + 3 * 0.25rem + 1px)", style);
    }

    [Fact]
    public void ConsumerAttributes_OverrideTheDefaults()
    {
        // Act
        var cut = TestContext.Render<TwNumeric<int>>(p => p.AddUnmatched("inputmode", "text").AddUnmatched("data-test", "x"));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("text", input.GetAttribute("inputmode"));
        Assert.Equal("x", input.GetAttribute("data-test"));
    }

    [Fact]
    public void InputAttributes_AreNotCopiedOntoTheTextfieldWrapper()
    {
        // Act
        var cut = TestContext.Render<TwNumeric<int>>();

        // Assert
        Assert.Single(cut.FindAll("[role='spinbutton']"));
    }

    #endregion

    #region Spin buttons

    [Fact]
    public void SpinButtons_AreRendered_OutOfTheTabOrder()
    {
        // Act
        var cut = TestContext.Render<TwNumeric<int>>(p => p.Add(x => x.Id, "count"));

        // Assert
        var decrease = cut.Find($"button[aria-label='{decreaseLabel}']");
        var increase = cut.Find($"button[aria-label='{increaseLabel}']");
        Assert.Equal("-1", decrease.GetAttribute("tabindex"));
        Assert.Equal("-1", increase.GetAttribute("tabindex"));
        Assert.Equal("count", increase.GetAttribute("aria-controls"));
        Assert.Equal("button", increase.GetAttribute("type"));
    }

    [Fact]
    public void HideSpinButtons_RemovesThem()
    {
        // Act
        var cut = TestContext.Render<TwNumeric<int>>(p => p.Add(x => x.HideSpinButtons, true));

        // Assert
        Assert.Empty(cut.FindAll($"button[aria-label='{increaseLabel}']"));
        Assert.Empty(cut.FindAll($"button[aria-label='{decreaseLabel}']"));
    }

    [Fact]
    public async Task IncreaseButton_StepsUp()
    {
        // Arrange
        var value = 5;
        var cut = TestContext.Render<TwNumeric<int>>(p => p
            .Add(x => x.Value, value)
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find($"button[aria-label='{increaseLabel}']").ClickAsync(new MouseEventArgs());

        // Assert
        Assert.Equal(6, value);
        Assert.Equal("6", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task DecreaseButton_StepsDown_ByStep()
    {
        // Arrange
        var value = 1m;
        var cut = TestContext.Render<TwNumeric<decimal>>(p => p
            .Add(x => x.Value, value)
            .Add(x => x.Step, 0.25m)
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find($"button[aria-label='{decreaseLabel}']").ClickAsync(new MouseEventArgs());

        // Assert
        Assert.Equal(0.75m, value);
    }

    [Fact]
    public void Buttons_AreDisabled_AtTheLimits()
    {
        // Act
        var atMax = TestContext.Render<TwNumeric<int>>(p => p.Add(x => x.Value, 8).Add(x => x.Max, 8));
        var atMin = TestContext.Render<TwNumeric<int>>(p => p.Add(x => x.Value, 1).Add(x => x.Min, 1));

        // Assert
        Assert.True(atMax.Find($"button[aria-label='{increaseLabel}']").HasAttribute("disabled"));
        Assert.False(atMax.Find($"button[aria-label='{decreaseLabel}']").HasAttribute("disabled"));
        Assert.True(atMin.Find($"button[aria-label='{decreaseLabel}']").HasAttribute("disabled"));
        Assert.False(atMin.Find($"button[aria-label='{increaseLabel}']").HasAttribute("disabled"));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Buttons_AreDisabled_WhenDisabledOrReadOnly(bool disabled, bool readOnly)
    {
        // Act
        var cut = TestContext.Render<TwNumeric<int>>(p => p.Add(x => x.Disabled, disabled).Add(x => x.ReadOnly, readOnly));

        // Assert
        Assert.All(cut.FindAll("button"), button => Assert.True(button.HasAttribute("disabled")));
    }

    #endregion

    #region Keyboard and wheel

    [Theory]
    [InlineData("ArrowUp", 11)]
    [InlineData("ArrowDown", 9)]
    [InlineData("PageUp", 20)]
    [InlineData("PageDown", 0)]
    public async Task Keys_StepTheValue(string key, int expected)
    {
        // Arrange
        var value = 10;
        var cut = TestContext.Render<TwNumeric<int>>(p => p
            .Add(x => x.Value, value)
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find("input").KeyDownAsync(Key(key));

        // Assert
        Assert.Equal(expected, value);
    }

    [Fact]
    public async Task Keys_StopAtTheLimits()
    {
        // Arrange
        var value = 95;
        var cut = TestContext.Render<TwNumeric<int>>(p => p
            .Add(x => x.Value, value)
            .Add(x => x.Max, 100)
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find("input").KeyDownAsync(Key("PageUp"));

        // Assert
        Assert.Equal(100, value);
    }

    [Fact]
    public async Task Keys_WithModifiers_DoNotStep()
    {
        // Arrange
        var value = 10;
        var cut = TestContext.Render<TwNumeric<int>>(p => p
            .Add(x => x.Value, value)
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find("input").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowUp", CtrlKey = true });

        // Assert
        Assert.Equal(10, value);
    }

    [Fact]
    public async Task Keys_DoNotStep_WhenReadOnly()
    {
        // Arrange
        var changed = false;
        var cut = TestContext.Render<TwNumeric<int>>(p => p
            .Add(x => x.ReadOnly, true)
            .Add(x => x.ValueChanged, _ => changed = true));

        // Act
        await cut.Find("input").KeyDownAsync(Key("ArrowUp"));

        // Assert
        Assert.False(changed);
    }

    [Fact]
    public async Task OnKeyDown_IsStillInvoked()
    {
        // Arrange
        string? pressed = null;
        var cut = TestContext.Render<TwNumeric<int>>(p => p.Add(x => x.OnKeyDown, args => pressed = args.Key));

        // Act
        await cut.Find("input").KeyDownAsync(Key("ArrowUp"));

        // Assert
        Assert.Equal("ArrowUp", pressed);
    }

    [Fact]
    public async Task NullValue_StepsFromZero()
    {
        // Arrange
        int? value = null;
        var cut = TestContext.Render<TwNumeric<int?>>(p => p
            .Add(x => x.Value, value)
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find("input").KeyDownAsync(Key("ArrowUp"));

        // Assert
        Assert.Equal(1, value);
    }

    [Fact]
    public async Task DoubleSteps_AvoidBinaryRoundingErrors()
    {
        // Arrange
        var value = 0.1;
        var cut = TestContext.Render<TwNumeric<double>>(p => p
            .Add(x => x.Value, value)
            .Add(x => x.Step, 0.2)
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find("input").KeyDownAsync(Key("ArrowUp"));

        // Assert
        Assert.Equal(0.3, value);
        Assert.Equal("0.3", cut.Find("input").GetAttribute("value"));
    }

    [Theory]
    [InlineData(false, -100, 11)]
    [InlineData(false, 100, 9)]
    [InlineData(true, -100, 9)]
    public async Task Wheel_StepsTheFocusedValue(bool invert, double deltaY, int expected)
    {
        // Arrange
        var value = 10;
        var cut = TestContext.Render<TwNumeric<int>>(p => p
            .Add(x => x.Value, value)
            .Add(x => x.InvertMouseWheel, invert)
            .Add(x => x.ValueChanged, v => value = v));
        var input = cut.Find("input");
        await input.FocusAsync(new FocusEventArgs());

        // Act
        await cut.Find("input").WheelAsync(new WheelEventArgs { DeltaY = deltaY });

        // Assert
        Assert.Equal(expected, value);
    }

    [Fact]
    public async Task Wheel_IsIgnored_WhenNotFocused()
    {
        // Arrange
        var value = 10;
        var cut = TestContext.Render<TwNumeric<int>>(p => p
            .Add(x => x.Value, value)
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find("input").WheelAsync(new WheelEventArgs { DeltaY = -100 });

        // Assert
        Assert.Equal(10, value);
    }

    [Fact]
    public async Task Wheel_IsIgnored_WhenDisabled()
    {
        // Arrange
        var value = 10;
        var cut = TestContext.Render<TwNumeric<int>>(p => p
            .Add(x => x.Value, value)
            .Add(x => x.DisableMouseWheel, true)
            .Add(x => x.ValueChanged, v => value = v));
        await cut.Find("input").FocusAsync(new FocusEventArgs());

        // Act
        await cut.Find("input").WheelAsync(new WheelEventArgs { DeltaY = -100 });

        // Assert
        Assert.Equal(10, value);
    }

    [Fact]
    public void Script_IsAttached_WithTheDefaultPattern()
    {
        // Act
        var cut = TestContext.Render<TwNumeric<decimal>>(p => p.Add(x => x.Min, 0m));

        // Assert
        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twNumeric.attach");
        Assert.Equal(@"[0-9\u{002C}\u{002E}]", invocation.Arguments[1]);
        Assert.False(Assert.IsType<bool>(invocation.Arguments[2]));
    }

    [Fact]
    public void DefaultPattern_AllowsTheMinusSign_OnlyBelowZero()
    {
        // Act
        TestContext.Render<TwNumeric<int>>();

        // Assert
        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twNumeric.attach");
        Assert.Equal(@"[0-9\u{002C}\u{002D}]", invocation.Arguments[1]);
    }

    [Fact]
    public void Pattern_ReplacesTheDefault()
    {
        // Act
        TestContext.Render<TwNumeric<int>>(p => p.Add(x => x.Pattern, "[0-9]"));

        // Assert
        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twNumeric.attach");
        Assert.Equal("[0-9]", invocation.Arguments[1]);
    }

    [Fact]
    public async Task Script_IsDetached_OnDispose()
    {
        // Arrange
        var cut = TestContext.Render<TwNumeric<int>>();

        // Act
        await cut.Instance.DisposeAsync();

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twNumeric.detach");
    }

    #endregion

    #region Typing

    [Fact]
    public async Task TypedText_IsParsed_AndReformatted()
    {
        // Arrange
        var value = 0m;
        var cut = TestContext.Render<TwNumeric<decimal>>(p => p
            .Add(x => x.Format, "N2")
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find("input").ChangeAsync(Text("1,234.5"));

        // Assert
        Assert.Equal(1234.5m, value);
        Assert.Equal("1,234.50", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task TypedText_IsParsedInTheCulture()
    {
        // Arrange
        var value = 0m;
        var cut = TestContext.Render<TwNumeric<decimal>>(p => p
            .Add(x => x.Culture, CultureInfo.GetCultureInfo("de-DE"))
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find("input").ChangeAsync(Text("12,5"));

        // Assert
        Assert.Equal(12.5m, value);
    }

    [Fact]
    public async Task TypedText_OutsideTheLimits_IsClamped()
    {
        // Arrange
        var value = 5;
        var cut = TestContext.Render<TwNumeric<int>>(p => p
            .Add(x => x.Value, value)
            .Add(x => x.Min, 1)
            .Add(x => x.Max, 10)
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find("input").ChangeAsync(Text("150"));

        // Assert
        Assert.Equal(10, value);
        Assert.Equal("10", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task TextThatIsNotANumber_ShowsAnError_AndKeepsTheValue()
    {
        // Arrange
        var changed = false;
        var cut = TestContext.Render<TwNumeric<int>>(p => p
            .Add(x => x.Value, 5)
            .Add(x => x.ValueChanged, _ => changed = true));

        // Act
        await cut.Find("input").ChangeAsync(Text("abc"));

        // Assert
        Assert.False(changed);
        var input = cut.Find("input");
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Equal(Localizer[TwStrings.Numeric_Invalid], cut.Find("[role='alert']").TextContent);
        Assert.Equal(cut.Find("[role='alert']").Id, input.GetAttribute("aria-describedby"));
    }

    [Fact]
    public async Task FractionsInAnIntegerField_AreNotANumber()
    {
        // Arrange
        var cut = TestContext.Render<TwNumeric<int>>();

        // Act
        await cut.Find("input").ChangeAsync(Text("1.5"));

        // Assert
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-invalid"));
    }

    [Fact]
    public async Task TheError_Clears_OnceTheTextIsANumber()
    {
        // Arrange
        var cut = TestContext.Render<TwNumeric<int>>();
        await cut.Find("input").ChangeAsync(Text("abc"));

        // Act
        await cut.Find("input").ChangeAsync(Text("4"));

        // Assert
        Assert.Null(cut.Find("input").GetAttribute("aria-invalid"));
        Assert.Empty(cut.FindAll("[role='alert']"));
    }

    [Fact]
    public async Task Stepping_ClearsTheError()
    {
        // Arrange
        var cut = TestContext.Render<TwNumeric<int>>();
        await cut.Find("input").ChangeAsync(Text("abc"));

        // Act
        await cut.Find("input").KeyDownAsync(Key("ArrowUp"));

        // Assert
        Assert.Null(cut.Find("input").GetAttribute("aria-invalid"));
        Assert.Equal("1", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task EmptyText_SetsANullableValueToNull()
    {
        // Arrange
        int? value = 5;
        var cut = TestContext.Render<TwNumeric<int?>>(p => p
            .Add(x => x.Value, value)
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find("input").ChangeAsync(Text(string.Empty));

        // Assert
        Assert.Null(value);
    }

    [Fact]
    public async Task EmptyText_RestoresANonNullableValue()
    {
        // Arrange
        var changed = false;
        var cut = TestContext.Render<TwNumeric<int>>(p => p
            .Add(x => x.Value, 5)
            .Add(x => x.ValueChanged, _ => changed = true));

        // Act
        await cut.Find("input").ChangeAsync(Text(string.Empty));

        // Assert
        Assert.False(changed);
        Assert.Equal("5", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task OnInput_UpdatesTheValueOnEveryKeystroke_WithoutReformatting()
    {
        // Arrange
        var value = 0m;
        var cut = TestContext.Render<TwNumeric<decimal>>(p => p
            .Add(x => x.BindEvent, "oninput")
            .Add(x => x.Format, "N2")
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find("input").InputAsync(Text("12"));

        // Assert
        Assert.Equal(12m, value);
        Assert.Equal("12", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task OnInput_FlagsTextThatIsNotANumber_OnlyOnBlur()
    {
        // Arrange
        var cut = TestContext.Render<TwNumeric<int>>(p => p.Add(x => x.BindEvent, "oninput"));
        await cut.Find("input").FocusAsync(new FocusEventArgs());
        await cut.Find("input").InputAsync(Text("-"));
        Assert.Null(cut.Find("input").GetAttribute("aria-invalid"));

        // Act
        await cut.Find("input").BlurAsync(new FocusEventArgs());

        // Assert
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-invalid"));
    }

    [Fact]
    public async Task Blur_TidiesTheTextIntoTheFormat()
    {
        // Arrange
        var cut = TestContext.Render<TwNumeric<decimal>>(p => p.Add(x => x.BindEvent, "oninput").Add(x => x.Format, "N2"));
        await cut.Find("input").InputAsync(Text("3"));

        // Act
        await cut.Find("input").BlurAsync(new FocusEventArgs());

        // Assert
        Assert.Equal("3.00", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void ExternalValueChanges_UpdateTheText()
    {
        // Arrange
        var cut = TestContext.Render<TwNumeric<int>>(p => p.Add(x => x.Value, 1));

        // Act
        cut.Render(p => p.Add(x => x.Value, 99));

        // Assert
        Assert.Equal("99", cut.Find("input").GetAttribute("value"));
    }

    #endregion

    #region Validation

    [Fact]
    public async Task Validator_RunsOnChange_AndShowsItsMessage()
    {
        // Arrange
        var cut = TestContext.Render<TwNumeric<int>>(p => p
            .Add(x => x.Validator, v => Task.FromResult<string?>(v % 2 == 0 ? null : "Enter an even number.")));

        // Act
        await cut.Find("input").ChangeAsync(Text("3"));

        // Assert
        Assert.Equal("Enter an even number.", cut.Find("[role='alert']").TextContent);
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-invalid"));

        // Act
        await cut.Find("input").KeyDownAsync(Key("ArrowUp"));

        // Assert
        Assert.Empty(cut.FindAll("[role='alert']"));
    }

    [Fact]
    public async Task Validator_RunsAfterValueChanged()
    {
        // Arrange
        var calls = new List<string>();
        var cut = TestContext.Render<TwNumeric<int>>(p => p
            .Add(x => x.ValueChanged, _ => calls.Add("changed"))
            .Add(x => x.Validator, _ =>
            {
                calls.Add("validated");
                return Task.FromResult<string?>(null);
            }));

        // Act
        await cut.Find("input").ChangeAsync(Text("2"));

        // Assert
        Assert.Equal(["changed", "validated"], calls);
    }

    [Fact]
    public void InvalidAndErrorMessage_AreShown()
    {
        // Act
        var cut = TestContext.Render<TwNumeric<int>>(p => p.Add(x => x.Invalid, true).Add(x => x.ErrorMessage, "Too many."));

        // Assert
        Assert.Equal("Too many.", cut.Find("[role='alert']").TextContent);
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-invalid"));
    }

    [Fact]
    public void ErrorMessage_IsHidden_WhileNotInvalid()
    {
        // Act
        var cut = TestContext.Render<TwNumeric<int>>(p => p.Add(x => x.Invalid, false).Add(x => x.ErrorMessage, "You can buy up to 4 tickets."));

        // Assert
        Assert.Empty(cut.FindAll("[role='alert']"));
        Assert.Null(cut.Find("input").GetAttribute("aria-invalid"));
    }

    [Fact]
    public async Task ErrorMessage_Clears_OnceAStepMakesTheBoundValueValid()
    {
        // Arrange - mirrors Invalid="@(tickets > 4)" with @bind-Value, re-rendering with the new value as the parent would
        var tickets = 5;
        IRenderedComponent<TwNumeric<int>> cut = null!;
        cut = TestContext.Render<TwNumeric<int>>(p => p
            .Add(x => x.Value, tickets)
            .Add(x => x.Invalid, tickets > 4)
            .Add(x => x.ErrorMessage, "You can buy up to 4 tickets.")
            .Add(x => x.ValueChanged, v =>
            {
                tickets = v;
                cut.Render(r => r.Add(x => x.Value, tickets).Add(x => x.Invalid, tickets > 4));
            }));
        Assert.Equal("You can buy up to 4 tickets.", cut.Find("[role='alert']").TextContent);
        await cut.Find("input").FocusAsync(new FocusEventArgs());

        // Act
        await cut.Find("input").WheelAsync(new WheelEventArgs { DeltaY = 100 });

        // Assert
        Assert.Equal(4, tickets);
        Assert.Empty(cut.FindAll("[role='alert']"));
    }

    [Fact]
    public void Required_IsPassedToTheInput()
    {
        // Act
        var cut = TestContext.Render<TwNumeric<int?>>(p => p.Add(x => x.Required, true));

        // Assert
        var input = cut.Find("input");
        Assert.True(input.HasAttribute("required"));
        Assert.Equal("true", input.GetAttribute("aria-required"));
    }

    #endregion

    #region Clearing

    [Fact]
    public void Clearable_ShowsTheTextfieldClearButton_WhileThereIsAValue()
    {
        // Act
        var empty = TestContext.Render<TwNumeric<int?>>(p => p.Add(x => x.Clearable, true));
        var filled = TestContext.Render<TwNumeric<int?>>(p => p.Add(x => x.Clearable, true).Add(x => x.Value, 3));

        // Assert
        Assert.True(empty.Find($"button[aria-label='{clearLabel}']").HasAttribute("disabled"));
        var button = filled.Find($"button[aria-label='{clearLabel}']");
        Assert.False(button.HasAttribute("disabled"));
        Assert.Equal("-1", button.GetAttribute("tabindex"));
        Assert.True(filled.FindComponent<TwTextfield<string>>().Instance.Clearable);
    }

    [Fact]
    public async Task Clearing_SetsANullableValueToNull()
    {
        // Arrange
        int? value = 3;
        var cut = TestContext.Render<TwNumeric<int?>>(p => p
            .Add(x => x.Value, value)
            .Add(x => x.Clearable, true)
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find($"button[aria-label='{clearLabel}']").ClickAsync(new MouseEventArgs());

        // Assert
        Assert.Null(value);
        Assert.Equal(string.Empty, cut.Find("input").GetAttribute("value") ?? string.Empty);
    }

    [Fact]
    public async Task Clearing_SetsANonNullableValueToZero_WithinTheLimits()
    {
        // Arrange
        var value = 7;
        var cut = TestContext.Render<TwNumeric<int>>(p => p
            .Add(x => x.Value, value)
            .Add(x => x.Min, 1)
            .Add(x => x.Clearable, true)
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.Find($"button[aria-label='{clearLabel}']").ClickAsync(new MouseEventArgs());

        // Assert
        Assert.Equal(1, value);
        Assert.Equal("1", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task ClearAsync_ClearsFromCode()
    {
        // Arrange
        decimal? value = 2.5m;
        var cut = TestContext.Render<TwNumeric<decimal?>>(p => p
            .Add(x => x.Value, value)
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.InvokeAsync(cut.Instance.ClearAsync);

        // Assert
        Assert.Null(value);
    }

    [Fact]
    public async Task IncrementAndDecrementAsync_StepFromCode()
    {
        // Arrange
        var value = 5;
        var cut = TestContext.Render<TwNumeric<int>>(p => p
            .Add(x => x.Value, value)
            .Add(x => x.Step, 2)
            .Add(x => x.ValueChanged, v => value = v));

        // Act
        await cut.InvokeAsync(cut.Instance.IncrementAsync);
        await cut.InvokeAsync(cut.Instance.IncrementAsync);
        await cut.InvokeAsync(cut.Instance.DecrementAsync);

        // Assert
        Assert.Equal(7, value);
    }

    #endregion
}
