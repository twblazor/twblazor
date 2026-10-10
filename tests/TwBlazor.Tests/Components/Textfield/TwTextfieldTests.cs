using Bunit;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Textfield;

public class TwTextfieldTests : TwBlazorTestBase
{
    private TwInputTheme inputTheme => Theme.Components.Require<TwInputTheme>();

    [Fact]
    public void TwTextfield_Renders_WithDefaultValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>();

        // Assert
        var input = cut.Find("input");
        Assert.NotNull(input);
        Assert.Equal("text", input.GetAttribute("type"));
        Assert.Empty(input.GetAttribute("placeholder") ?? string.Empty);
    }

    [Fact]
    public void TwTextfield_Renders_WithLabel()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Label, "Username"));

        // Assert
        var label = cut.Find("label");
        Assert.NotNull(label);
        Assert.Equal("Username", label.TextContent);
        Assert.Contains("block mb-2 text-xs font-normal tracking-wide", label.GetAttribute("class"));
    }

    [Fact]
    public void TwTextfield_DoesNotRender_LabelWhenEmpty()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Label, string.Empty));

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find("label"));
    }

    [Fact]
    public void TwTextfield_Renders_WithPlaceholder()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Placeholder, "Enter your email"));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("Enter your email", input.GetAttribute("placeholder"));
    }

    [Fact]
    public void TwTextfield_Renders_WithCustomInputType()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.InputType, "password"));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("password", input.GetAttribute("type"));
    }

    [Fact]
    public void TwTextfield_Renders_WithEmailType()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.InputType, "email"));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("email", input.GetAttribute("type"));
    }

    [Fact]
    public void TwTextfield_Renders_WithNumberType()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<int>>(parameters => parameters
            .Add(p => p.InputType, "number")
            .Add(p => p.Value, 42));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("number", input.GetAttribute("type"));
    }

    [Fact]
    public void TwTextfield_Renders_WithId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Id, "custom-id"));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("custom-id", input.GetAttribute("id"));
    }

    [Fact]
    public void TwTextfield_Renders_WithLabelAndId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Id, "username-input")
            .Add(p => p.Label, "Username"));

        // Assert
        var label = cut.Find("label");
        var input = cut.Find("input");
        Assert.Equal("username-input", label.GetAttribute("for"));
        Assert.Equal("username-input", input.GetAttribute("id"));
    }

    [Fact]
    public void TwTextfield_Renders_WithCustomClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Class, "custom-class"));

        // Assert
        var input = cut.Find("input");
        Assert.Contains("custom-class", input.GetAttribute("class"));
        Assert.Contains(inputTheme.TextfieldBase, input.GetAttribute("class")); // Default class should still be present
    }

    [Fact]
    public void TwTextfield_Renders_WithCustomLabelClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Label, "Email")
            .Add(p => p.LabelClass, "text-blue-600"));

        // Assert
        var label = cut.Find("label");
        Assert.Contains("text-blue-600", label.GetAttribute("class"));
        Assert.Contains("tracking-wide", label.GetAttribute("class")); // Default classes that don't conflict should still be present
        Assert.DoesNotContain(inputTheme.LabelBase, label.GetAttribute("class")); // The theme's own text color is replaced
    }

    [Fact]
    public void TwTextfield_Renders_WithInitialValue()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Value, "Initial Value"));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("Initial Value", input.GetAttribute("value"));
    }

    [Fact]
    public void TwTextfield_TriggersValueChanged_OnInput()
    {
        // Arrange
        var newValue = string.Empty;
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Value, "")
            .Add(p => p.ValueChanged, value => newValue = value));

        // Act
        var input = cut.Find("input");
        input.Change("Test Input");

        // Assert
        Assert.Equal("Test Input", newValue);
    }

    [Fact]
    public void TwTextfield_TriggersValueChanged_WithIntType()
    {
        // Arrange
        var newValue = 0;
        var cut = TestContext.Render<TwTextfield<int>>(parameters => parameters
            .Add(p => p.InputType, "number")
            .Add(p => p.Value, 0)
            .Add(p => p.ValueChanged, value => newValue = value));

        // Act
        var input = cut.Find("input");
        input.Change(123);

        // Assert
        Assert.Equal(123, newValue);
    }

    [Fact]
    public void TwTextfield_UpdatesValue_OnChange()
    {
        // Arrange
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Value, "Old Value")
            .Add(p => p.ValueChanged, _ => { }));

        // Act
        var input = cut.Find("input");
        input.Change("New Value");

        // Assert
        Assert.Equal("New Value", input.GetAttribute("value"));
    }

    [Fact]
    public void TwTextfield_Renders_WithCustomBindEvent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.BindEvent, "oninput")
            .Add(p => p.Value, "")
            .Add(p => p.ValueChanged, _ => { }));

        // Assert - This tests that the component accepts the parameter without error
        var input = cut.Find("input");
        Assert.NotNull(input);
    }

    [Fact]
    public void TwTextfield_Renders_WithAttributes()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Attributes, new Dictionary<string, object>
            {
                { "data-test", "test-value" },
                { "disabled", true }
            }));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("test-value", input.GetAttribute("data-test"));
        Assert.True(input.HasAttribute("disabled"));
    }

    [Fact]
    public void TwTextfield_Renders_WithMultipleParameters()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Id, "full-example")
            .Add(p => p.Label, "Full Name")
            .Add(p => p.Placeholder, "Enter your full name")
            .Add(p => p.Value, "John Doe")
            .Add(p => p.Class, "rounded-lg")
            .Add(p => p.LabelClass, "font-bold"));

        // Assert
        var label = cut.Find("label");
        var input = cut.Find("input");

        Assert.Equal("Full Name", label.TextContent);
        Assert.Contains("font-bold", label.GetAttribute("class"));
        Assert.Equal("full-example", input.GetAttribute("id"));
        Assert.Equal("Enter your full name", input.GetAttribute("placeholder"));
        Assert.Equal("John Doe", input.GetAttribute("value"));
        Assert.Contains("rounded-lg", input.GetAttribute("class"));
    }

    [Fact]
    public void TwTextfield_Renders_WithDecimalType()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<decimal>>(parameters => parameters
            .Add(p => p.InputType, "number")
            .Add(p => p.Value, 99.99m));

        // Assert
        var input = cut.Find("input");
        Assert.NotNull(input);
    }

    [Fact]
    public void TwTextfield_HasDefaultClasses()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>();

        // Assert
        var input = cut.Find("input");
        var classes = input.GetAttribute("class");

        Assert.Contains(inputTheme.TextfieldBase, classes);
    }

    [Fact]
    public void TwTextfield_DoesNotTriggerValueChanged_WhenNotProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Value, "Initial"));

        var input = cut.Find("input");

        // Act - Should not throw exception
        var exception = Record.Exception(() => input.Change("Changed"));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void TwTextfield_GeneratesId_WhenNotProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>();

        // Assert
        var input = cut.Find("input");
        var id = input.GetAttribute("id");
        Assert.NotNull(id);
        Assert.StartsWith("textfield-", id);
        Assert.DoesNotContain("`", id); // Should not contain generic type indicator
    }

    [Fact]
    public void TwTextfield_GeneratesUniqueIds_ForMultipleInstances()
    {
        // Arrange & Act
        var cut1 = TestContext.Render<TwTextfield<string>>();
        var cut2 = TestContext.Render<TwTextfield<string>>();

        // Assert
        var id1 = cut1.Find("input").GetAttribute("id");
        var id2 = cut2.Find("input").GetAttribute("id");
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void TwTextfield_AssociatesLabelWithGeneratedId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Label, "Email"));

        // Assert
        var label = cut.Find("label");
        var input = cut.Find("input");
        var inputId = input.GetAttribute("id");
        var labelFor = label.GetAttribute("for");

        Assert.NotNull(inputId);
        Assert.Equal(inputId, labelFor);
        Assert.StartsWith("textfield-", inputId);
    }

    [Fact]
    public void TwTextfield_Renders_WithDisabled()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Disabled, true));

        // Assert
        var input = cut.Find("input");
        Assert.True(input.HasAttribute("disabled"));
        Assert.Contains("opacity-40 cursor-not-allowed", input.GetAttribute("class"));
    }

    [Fact]
    public void TwTextfield_Renders_WithoutDisabled_WhenFalse()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Disabled, false));

        // Assert
        var input = cut.Find("input");
        Assert.False(input.HasAttribute("disabled"));
    }

    [Fact]
    public void TwTextfield_Renders_WithReadonly()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.ReadOnly, true));

        // Assert
        var input = cut.Find("input");
        Assert.True(input.HasAttribute("readonly"));
    }

    [Fact]
    public void TwTextfield_Renders_WithoutReadonly_WhenFalse()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.ReadOnly, false));

        // Assert
        var input = cut.Find("input");
        Assert.False(input.HasAttribute("readonly"));
    }

    [Fact]
    public void TwTextfield_Renders_WithBothDisabledAndReadonly()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.ReadOnly, true));

        // Assert
        var input = cut.Find("input");
        Assert.True(input.HasAttribute("disabled"));
        Assert.True(input.HasAttribute("readonly"));
    }

    [Theory]
    [InlineData("tel")]
    [InlineData("url")]
    public void TwTextfield_SetsAutocompleteAndInputMode_ForTelAndUrlTypes(string inputType)
    {
        // Arrange & Act - GetAutoCompleteForInputType and GetInputModeForInputType both have a
        // dedicated "tel"/"url" branch (in addition to the already-covered "email" and "number" ones).
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.InputType, inputType));

        // Assert
        var input = cut.Find("input");
        Assert.Equal(inputType, input.GetAttribute("autocomplete"));
        Assert.Equal(inputType, input.GetAttribute("inputmode"));
    }

    [Fact]
    public void TwTextfield_SetsInputMode_ForSearchType_WithoutAutocomplete()
    {
        // Arrange & Act - "search" has a branch in GetInputModeForInputType but not in
        // GetAutoCompleteForInputType (which falls through to its default null case for it).
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.InputType, "search"));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("search", input.GetAttribute("inputmode"));
        Assert.False(input.HasAttribute("autocomplete"));
    }

    [Fact]
    public void TwTextfield_UsesGlobalDefaultVariant_WhenNotSet()
    {
        // Arrange - no Variant set on the component, so it must follow TwInputTheme.DefaultInputVariant
        // (inherited via TwBlazorInputComponentBase.effectiveVariant).
        inputTheme.DefaultInputVariant = InputVariant.Outlined;

        // Act
        var cut = TestContext.Render<TwTextfield<string>>();

        // Assert
        var classes = cut.Find("input").GetAttribute("class");
        Assert.Contains(InputVariantBuilder.GetClasses(InputVariant.Outlined, inputTheme), classes);
    }

    [Fact]
    public void TwTextfield_ExplicitVariant_OverridesGlobalDefault()
    {
        // Arrange - the global default is Outlined, but this instance explicitly asks for Filled.
        inputTheme.DefaultInputVariant = InputVariant.Outlined;

        // Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Variant, InputVariant.Filled));

        // Assert
        var classes = cut.Find("input").GetAttribute("class");
        Assert.Contains(InputVariantBuilder.GetClasses(InputVariant.Filled, inputTheme), classes);
    }

    [Fact]
    public void TwTextfield_Renders_ErrorMessage_WhenInvalid()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Invalid, true)
            .Add(p => p.ErrorMessage, "This field is required."));

        // Assert
        var error = cut.Find("p[role='alert']");
        Assert.Equal("This field is required.", error.TextContent);
        Assert.Contains(inputTheme.ErrorMessage, error.GetAttribute("class"));
    }

    [Fact]
    public void TwTextfield_DoesNotAssociateErrorMessage_WhenNotInvalid()
    {
        // Arrange & Act - TwInputRoot renders the error text whenever ErrorMessage is set, but the
        // component only wires up the id/aria-describedby association (via errorId) when Invalid is
        // also true - so the paragraph exists here but isn't announced against the input.
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.ErrorMessage, "This field is required."));

        // Assert
        var error = cut.Find("p[role='alert']");
        Assert.Empty(error.GetAttribute("id") ?? string.Empty);
        Assert.False(cut.Find("input").HasAttribute("aria-invalid"));
        Assert.False(cut.Find("input").HasAttribute("aria-describedby"));
    }

    [Fact]
    public void TwTextfield_DoesNotRender_ErrorMessage_WhenNeitherInvalidNorErrorMessageSet()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>();

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find("p[role='alert']"));
    }

    [Fact]
    public void TwTextfield_SetsAriaInvalidAndDescribedBy_WhenInvalid()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Id, "email-field")
            .Add(p => p.Invalid, true)
            .Add(p => p.ErrorMessage, "Enter a valid email address."));

        // Assert
        var input = cut.Find("input");
        var error = cut.Find("p[role='alert']");
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Equal("email-field-error", input.GetAttribute("aria-describedby"));
        Assert.Equal("email-field-error", error.GetAttribute("id"));
    }

    [Fact]
    public void TwTextfield_DoesNotSetAriaInvalid_ByDefault()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>();

        // Assert
        var input = cut.Find("input");
        Assert.False(input.HasAttribute("aria-invalid"));
        Assert.False(input.HasAttribute("aria-describedby"));
    }

    [Fact]
    public void TwTextfield_Validator_MarksInvalid_WhenValidatorReturnsErrorMessage()
    {
        // Arrange
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Value, string.Empty)
            .Add(p => p.ValueChanged, _ => { })
            .Add(p => p.Validator, (string value) => Task.FromResult<string?>(
                string.IsNullOrWhiteSpace(value) ? "Username is required." : null)));

        // Act
        cut.Find("input").Change(string.Empty);

        // Assert
        var error = cut.Find("p[role='alert']");
        Assert.Equal("Username is required.", error.TextContent);
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-invalid"));
    }

    [Fact]
    public void TwTextfield_Validator_MarksValid_WhenValidatorReturnsNull()
    {
        // Arrange - starts invalid, then a passing value should clear both Invalid and ErrorMessage.
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Value, string.Empty)
            .Add(p => p.ValueChanged, _ => { })
            .Add(p => p.Invalid, true)
            .Add(p => p.ErrorMessage, "Username is required.")
            .Add(p => p.Validator, (string value) => Task.FromResult<string?>(
                string.IsNullOrWhiteSpace(value) ? "Username is required." : null)));

        // Act
        cut.Find("input").Change("validusername");

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find("p[role='alert']"));
        Assert.False(cut.Find("input").HasAttribute("aria-invalid"));
    }

    [Fact]
    public void TwTextfield_Validator_ReceivesTheChangedValue()
    {
        // Arrange
        string? receivedValue = null;
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Value, "old")
            .Add(p => p.ValueChanged, _ => { })
            .Add(p => p.Validator, (string value) =>
            {
                receivedValue = value;
                return Task.FromResult<string?>(null);
            }));

        // Act
        cut.Find("input").Change("new");

        // Assert
        Assert.Equal("new", receivedValue);
    }

    [Fact]
    public void TwTextfield_Validator_NotInvoked_WhenNotProvided()
    {
        // Arrange & Act - should not throw and should leave Invalid/ErrorMessage untouched.
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Value, string.Empty)
            .Add(p => p.ValueChanged, _ => { }));

        var exception = Record.Exception(() => cut.Find("input").Change("Changed"));

        // Assert
        Assert.Null(exception);
        Assert.Throws<ElementNotFoundException>(() => cut.Find("p[role='alert']"));
    }

    [Fact]
    public void TwTextfield_Validator_SupportsAsynchronousValidation()
    {
        // Arrange - a validator that doesn't complete synchronously (e.g. simulating a server call)
        // should still be awaited and its result reflected once it completes.
        var validationCompletionSource = new TaskCompletionSource<string?>();
        var cut = TestContext.Render<TwTextfield<string>>(parameters => parameters
            .Add(p => p.Value, string.Empty)
            .Add(p => p.ValueChanged, _ => { })
            .Add(p => p.Validator, (string _) => validationCompletionSource.Task));

        // Act
        cut.Find("input").Change("taken-username");
        Assert.Throws<ElementNotFoundException>(() => cut.Find("p[role='alert']"));
        validationCompletionSource.SetResult("'taken-username' is already taken.");

        // Assert
        cut.WaitForState(() => cut.FindAll("p[role='alert']").Count > 0);
        Assert.Equal("'taken-username' is already taken.", cut.Find("p[role='alert']").TextContent);
    }
}
