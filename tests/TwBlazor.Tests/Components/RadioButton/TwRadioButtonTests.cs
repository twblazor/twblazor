using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.RadioButton;

public class TwRadioButtonTests : TwBlazorTestBase
{
    [Fact]
    public void TwRadioButton_Renders_WithDefaultValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var input = cut.Find("input[type='radio']");
        Assert.NotNull(input);
        Assert.False(input.HasAttribute("checked"));
    }

    [Fact]
    public void TwRadioButton_GeneratesId_WhenNotProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var input = cut.Find("input");
        var id = input.GetAttribute("id");
        Assert.NotNull(id);
        Assert.StartsWith("radiobutton-", id);
        Assert.DoesNotContain("`", id);
    }

    [Fact]
    public void TwRadioButton_UsesProvidedId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Id, "custom-radio-id")
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("custom-radio-id", input.GetAttribute("id"));
    }

    [Fact]
    public void TwRadioButton_GeneratesUniqueIds_ForMultipleInstances()
    {
        // Arrange & Act
        var cut1 = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));
        var cut2 = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Value, "option2")
            .Add(p => p.SelectedValue, null));

        // Assert
        var id1 = cut1.Find("input").GetAttribute("id");
        var id2 = cut2.Find("input").GetAttribute("id");
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void TwRadioButton_Renders_AsChecked_WhenValueMatchesSelectedValue()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, "option1"));

        // Assert
        var input = cut.Find("input");
        Assert.True(input.HasAttribute("checked"));
    }

    [Fact]
    public void TwRadioButton_Renders_AsUnchecked_WhenValueDoesNotMatchSelectedValue()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, "option2"));

        // Assert
        var input = cut.Find("input");
        Assert.False(input.HasAttribute("checked"));
    }

    [Fact]
    public void TwRadioButton_RendersLabel_WhenLabelProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Label, "Option 1")
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var label = cut.Find("label");
        Assert.NotNull(label);
        Assert.Contains("Option 1", label.TextContent);
    }

    [Fact]
    public void TwRadioButton_LabelFor_MatchesRadioButtonId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Id, "option1-radio")
            .Add(p => p.Label, "Option 1")
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var label = cut.Find("label");
        var input = cut.Find("input");
        Assert.Equal("option1-radio", label.GetAttribute("for"));
        Assert.Equal("option1-radio", input.GetAttribute("id"));
    }

    [Fact]
    public void TwRadioButton_InvokesSelectedValueChanged_WhenSelected()
    {
        // Arrange
        string? valueFromCallback = null;
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null)
            .Add(p => p.SelectedValueChanged, EventCallback.Factory.Create<string>(this, v => valueFromCallback = v)));

        // Act
        var input = cut.Find("input");
        input.Change(true);

        // Assert
        Assert.NotNull(valueFromCallback);
        Assert.Equal("option1", valueFromCallback);
    }

    [Fact]
    public void TwRadioButton_SetsName_WhenProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Name, "options")
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("options", input.GetAttribute("name"));
    }

    [Fact]
    public void TwRadioButton_AppliesCustomClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Class, "custom-radio-class")
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var input = cut.Find("input");
        Assert.Contains("custom-radio-class", input.GetAttribute("class"));
    }

    [Fact]
    public void TwRadioButton_HasDefaultClasses()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var input = cut.Find("input");
        var classes = input.GetAttribute("class");
        Assert.Contains("peer", classes);
        Assert.Contains("size-5", classes);
        Assert.Contains("cursor-pointer", classes);
        Assert.Contains("appearance-none", classes);
        Assert.Contains("rounded-full", classes);
        Assert.Contains("border", classes);
        Assert.Contains("border-[oklch(21%_0.006_285.885)]/50", classes);
    }

    [Fact]
    public void TwRadioButton_AppliesCustomLabelClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Label, "Test Label")
            .Add(p => p.LabelClass, "text-blue-600")
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var label = cut.Find("label");
        Assert.Contains("text-blue-600", label.GetAttribute("class"));
        Assert.Contains("flex", label.GetAttribute("class"));
        Assert.Contains("items-center", label.GetAttribute("class"));
    }

    [Fact]
    public void TwRadioButton_AppliesAriaReadonly_WhenReadonly()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.ReadOnly, true)
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("true", input.GetAttribute("aria-readonly"));
    }

    [Fact]
    public void TwRadioButton_DoesNotApplyAriaReadonly_WhenNotReadonly()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.ReadOnly, false)
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var input = cut.Find("input");
        Assert.False(input.HasAttribute("aria-readonly"));
    }

    [Fact]
    public void TwRadioButton_DoesNotInvokeCallback_WhenReadonly()
    {
        // Arrange
        string? valueFromCallback = null;
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.ReadOnly, true)
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null)
            .Add(p => p.SelectedValueChanged, EventCallback.Factory.Create<string>(this, v => valueFromCallback = v)));

        // Act
        var input = cut.Find("input");
        input.Change(true);

        // Assert
        Assert.Null(valueFromCallback);
    }

    [Fact]
    public void TwRadioButton_AppliesDisabledState()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var input = cut.Find("input");
        Assert.True(input.HasAttribute("disabled"));
    }

    [Fact]
    public void TwRadioButton_DoesNotInvokeCallback_WhenDisabled()
    {
        // Arrange
        string? valueFromCallback = null;
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null)
            .Add(p => p.SelectedValueChanged, EventCallback.Factory.Create<string>(this, v => valueFromCallback = v)));

        // Act
        var input = cut.Find("input");
        input.Change(true);

        // Assert
        Assert.Null(valueFromCallback);
    }

    [Fact]
    public void TwRadioButton_AppliesDisabledClasses()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var input = cut.Find("input");
        var classes = input.GetAttribute("class");
        Assert.Contains("opacity-40", classes);
        Assert.Contains("cursor-not-allowed", classes);
    }

    [Fact]
    public void TwRadioButton_LabelHasPointerEventsNone_WhenDisabled()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Label, "Disabled Radio Button")
            .Add(p => p.Disabled, true)
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var label = cut.Find("label");
        Assert.Contains("pointer-events-none", label.GetAttribute("class"));
        Assert.Contains("opacity-40", label.GetAttribute("class"));
    }

    [Fact]
    public void TwRadioButton_LabelHasPointerEventsNone_WhenReadonly()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Label, "Readonly Radio Button")
            .Add(p => p.ReadOnly, true)
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var label = cut.Find("label");
        Assert.Contains("pointer-events-none", label.GetAttribute("class"));
    }

    [Fact]
    public void TwRadioButton_DefaultsToPrimaryColor_WhenNoColorIsSet()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, "option1"));

        // Assert
        Assert.Contains("checked:bg-purple-600", cut.Find("input").ClassName);
    }

    [Fact]
    public void TwRadioButton_AppliesPurpleColor_WhenColorIsPrimary()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Color, Color.Primary)
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var markup = cut.Markup;
        Assert.Contains("checked:bg-purple-600", markup);
    }

    [Fact]
    public void TwRadioButton_AppliesGreenColor_WhenColorIsGreen()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Color, Color.Success)
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var markup = cut.Markup;
        Assert.Contains("checked:bg-green-600", markup);
    }

    [Fact]
    public void TwRadioButton_AppliesRedColor_WhenColorIsRed()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Color, Color.Danger)
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var markup = cut.Markup;
        Assert.Contains("checked:bg-red-600", markup);
    }

    [Fact]
    public void TwRadioButton_AppliesFuchsiaColor_WhenColorIsAccent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Color, Color.Accent)
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var markup = cut.Markup;
        Assert.Contains("checked:bg-fuchsia-600", markup);
    }

    [Theory]
    [InlineData(Color.Primary, "purple")]
    [InlineData(Color.Success, "green")]
    [InlineData(Color.Danger, "red")]
    [InlineData(Color.Accent, "fuchsia")]
    [InlineData(Color.Warning, "yellow")]
    [InlineData(Color.Info, "blue")]
    public void TwRadioButton_AppliesCorrectColor_ForEachColorEnum(Color color, string colorName)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Color, color)
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var markup = cut.Markup;
        Assert.Contains($"checked:bg-{colorName}-", markup);
    }

    [Fact]
    public void TwRadioButton_WithIntValue_WorksCorrectly()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<int>>(parameters => parameters
            .Add(p => p.Value, 1)
            .Add(p => p.SelectedValue, 1));

        // Assert
        var input = cut.Find("input");
        Assert.True(input.HasAttribute("checked"));
    }

    [Fact]
    public void TwRadioButton_WithIntValue_UpdatesSelectedValue()
    {
        // Arrange
        int? valueFromCallback = null;
        var cut = TestContext.Render<TwRadioButton<int>>(parameters => parameters
            .Add(p => p.Value, 42)
            .Add(p => p.SelectedValue, 0)
            .Add(p => p.SelectedValueChanged, EventCallback.Factory.Create<int>(this, v => valueFromCallback = v)));

        // Act
        var input = cut.Find("input");
        input.Change(true);

        // Assert
        Assert.NotNull(valueFromCallback);
        Assert.Equal(42, valueFromCallback.Value);
    }

    [Fact]
    public void TwRadioButton_MultipleButtonsInGroup_OnlyOneChecked()
    {
        // Arrange & Act
        var cut1 = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Name, "group1")
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, "option1"));

        var cut2 = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Name, "group1")
            .Add(p => p.Value, "option2")
            .Add(p => p.SelectedValue, "option1"));

        // Assert
        var input1 = cut1.Find("input");
        var input2 = cut2.Find("input");
        Assert.True(input1.HasAttribute("checked"));
        Assert.False(input2.HasAttribute("checked"));
    }

    [Fact]
    public void TwRadioButton_WithEnumValue_WorksCorrectly()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<Color>>(parameters => parameters
            .Add(p => p.Value, Color.Primary)
            .Add(p => p.SelectedValue, Color.Primary));

        // Assert
        var input = cut.Find("input");
        Assert.True(input.HasAttribute("checked"));
    }

    [Fact]
    public void TwRadioButton_WithEnumValue_UpdatesSelectedValue()
    {
        // Arrange
        Color? valueFromCallback = null;
        var cut = TestContext.Render<TwRadioButton<Color>>(parameters => parameters
            .Add(p => p.Value, Color.Success)
            .Add(p => p.SelectedValue, Color.Danger)
            .Add(p => p.SelectedValueChanged, EventCallback.Factory.Create<Color>(this, v => valueFromCallback = v)));

        // Act
        var input = cut.Find("input");
        input.Change(true);

        // Assert
        Assert.NotNull(valueFromCallback);
        Assert.Equal(Color.Success, valueFromCallback.Value);
    }

    [Fact]
    public void TwRadioButton_SupportsLabelAttributes()
    {
        // Arrange
        var labelAttributes = new Dictionary<string, object>
        {
            { "data-testid", "test-label" }
        };

        // Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Label, "Test")
            .Add(p => p.LabelAttributes, labelAttributes)
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var label = cut.Find("label");
        Assert.Equal("test-label", label.GetAttribute("data-testid"));
    }

    [Fact]
    public void TwRadioButton_SupportsLabelId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Label, "Test")
            .Add(p => p.LabelId, "custom-label-id")
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var label = cut.Find("label");
        Assert.Equal("custom-label-id", label.GetAttribute("id"));
    }

    [Fact]
    public void TwRadioButton_AppliesAriaInvalid_WhenInvalid()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Invalid, true)
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
    }

    [Fact]
    public void TwRadioButton_DoesNotApplyAriaInvalid_WhenNotInvalid()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var input = cut.Find("input");
        Assert.False(input.HasAttribute("aria-invalid"));
    }

    [Fact]
    public void TwRadioButton_SetsAriaDescribedBy_ToErrorId_WhenInvalidWithErrorMessage()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.Id, "plan-radio")
            .Add(p => p.Invalid, true)
            .Add(p => p.ErrorMessage, "Pick one")
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("plan-radio-error", input.GetAttribute("aria-describedby"));

        var error = cut.Find("p[role='alert']");
        Assert.Contains("Pick one", error.TextContent);
    }

    [Fact]
    public void TwRadioButton_SetsAriaLabel_WhenProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.AriaLabel, "Choose plan")
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("Choose plan", input.GetAttribute("aria-label"));
    }

    [Fact]
    public void TwRadioButton_SetsAriaLabelledBy_WhenProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioButton<string>>(parameters => parameters
            .Add(p => p.AriaLabelledBy, "plan-label")
            .Add(p => p.Value, "option1")
            .Add(p => p.SelectedValue, null));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("plan-label", input.GetAttribute("aria-labelledby"));
    }
}
