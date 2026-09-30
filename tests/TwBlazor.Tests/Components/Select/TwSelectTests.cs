using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Builders;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Select;

public class TwSelectTests : TwBlazorTestBase
{
    private TwInputTheme inputTheme => Theme.Components.Require<TwInputTheme>();

    private static readonly string[] _twoStringOptions = ["Option1", "Option2"];
    private static readonly string[] _threeStringOptions = ["Option1", "Option2", "Option3"];
    private static readonly string[] _countryOptions = ["USA", "UK", "Canada"];
    private static readonly string[] _fruitOptions = ["Apple", "Banana", "Cherry"];
    private static readonly string[] _singleStringOption = ["Option1"];
    private static readonly int[] _intOptions = [1, 2, 3];
    private static readonly int[] _zeroOneTwoIntOptions = [0, 1, 2];
    private static readonly string[] _option1AndOption3Selected = ["Option1", "Option3"];
    private static readonly string[] _option1Selected = ["Option1"];
    private static readonly string[] _selectedIds1And3 = ["1", "3"];
    private static readonly string[] _selectedId1 = ["1"];
    private static readonly string[] _selectedIds1And2 = ["1", "2"];
    private class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    [Fact]
    public void TwSelect_Renders_WithDefaultValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Values, _threeStringOptions));

        // Assert
        var select = cut.Find("select");
        Assert.NotNull(select);
        var options = cut.FindAll("option");
        Assert.Equal(4, options.Count); // 1 placeholder + 3 values
    }

    [Fact]
    public void TwSelect_OptsIntoCustomizableSelect_WhereTheBrowserSupportsIt()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Values, _threeStringOptions));

        // Assert
        var selectClass = cut.Find("select").GetAttribute("class");
        Assert.Contains("supports-[appearance:base-select]:[appearance:base-select]", selectClass);
        Assert.Contains("[&::picker(select)]:[appearance:base-select]", selectClass);
        Assert.Contains("appearance-none", selectClass);
        var optionClass = cut.Find("option").GetAttribute("class");
        Assert.Contains("supports-[appearance:base-select]:checked:", optionClass);
        Assert.Contains("supports-[appearance:base-select]:py-2", optionClass);
    }

    [Fact]
    public void TwSelect_UsesDefaultSize_WhenNotDense()
    {
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Values, _threeStringOptions));

        var selectClass = cut.Find("select").GetAttribute("class");
        Assert.Contains(inputTheme.Size, selectClass);
        Assert.DoesNotContain(inputTheme.DenseSize, selectClass);
    }

    [Fact]
    public void TwSelect_UsesDenseSize_WhenDense()
    {
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Values, _threeStringOptions)
            .Add(p => p.Dense, true));

        var selectClass = cut.Find("select").GetAttribute("class");
        Assert.Contains(inputTheme.DenseSize, selectClass);
        Assert.DoesNotContain(inputTheme.Size, selectClass);
    }

    [Fact]
    public void TwSelect_Renders_WithLabel()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Label, "Choose an option")
            .Add(p => p.Values, _twoStringOptions));

        // Assert
        var label = cut.Find("label");
        Assert.NotNull(label);
        Assert.Equal("Choose an option", label.TextContent);
        Assert.Equal(inputTheme.LabelBase, label.GetAttribute("class"));
    }

    [Fact]
    public void TwSelect_DoesNotRender_LabelWhenEmpty()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Label, string.Empty)
            .Add(p => p.Values, _twoStringOptions));

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find("label"));
    }

    [Fact]
    public void TwSelect_Renders_WithPlaceholder()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Placeholder, "Select a value...")
            .Add(p => p.Values, _twoStringOptions));

        // Assert
        var placeholderOption = cut.Find("option[value='0']");
        Assert.NotNull(placeholderOption);
        Assert.Equal("Select a value...", placeholderOption.TextContent);
    }

    [Fact]
    public void TwSelect_DoesNotRender_PlaceholderWhenRequired()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Required, true)
            .Add(p => p.Values, _twoStringOptions));

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find("option[value='0']"));
    }

    [Fact]
    public void TwSelect_Renders_WithStringValues()
    {
        // Arrange
        var values = _fruitOptions;

        // Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Values, values));

        // Assert
        var options = cut.FindAll("option").Skip(1).ToList(); // Skip placeholder
        Assert.Equal(3, options.Count);
        Assert.Equal("Apple", options[0].TextContent);
        Assert.Equal("Banana", options[1].TextContent);
        Assert.Equal("Cherry", options[2].TextContent);
    }

    [Fact]
    public void TwSelect_Renders_WithIntValues()
    {
        // Arrange
        var values = _intOptions;

        // Act
        var cut = TestContext.Render<TwSelect<int>>(parameters => parameters
            .Add(p => p.Values, values));

        // Assert
        var options = cut.FindAll("option").Skip(1).ToList(); // Skip placeholder
        Assert.Equal(3, options.Count);
        Assert.Equal("1", options[0].TextContent);
        Assert.Equal("2", options[1].TextContent);
        Assert.Equal("3", options[2].TextContent);
    }

    [Fact]
    public void TwSelect_Renders_WithComplexObjects()
    {
        // Arrange
        var values = new[]
        {
            new TestModel { Id = 1, Name = "First" },
            new TestModel { Id = 2, Name = "Second" },
            new TestModel { Id = 3, Name = "Third" }
        };

        // Act
        var cut = TestContext.Render<TwSelect<TestModel>>(parameters => parameters
            .Add(p => p.Values, values)
            .Add(p => p.PropertyName, "Name"));

        // Assert
        var options = cut.FindAll("option").Skip(1).ToList(); // Skip placeholder
        Assert.Equal(3, options.Count);
        Assert.Equal("First", options[0].TextContent);
        Assert.Equal("Second", options[1].TextContent);
        Assert.Equal("Third", options[2].TextContent);
    }

    [Fact]
    public void TwSelect_Renders_ComplexObjectsWithDifferentProperty()
    {
        // Arrange
        var values = new[]
        {
            new TestModel { Id = 1, Name = "First", Description = "Desc1" },
            new TestModel { Id = 2, Name = "Second", Description = "Desc2" },
            new TestModel { Id = 3, Name = "Third", Description = "Desc3" }
        };

        // Act
        var cut = TestContext.Render<TwSelect<TestModel>>(parameters => parameters
            .Add(p => p.Values, values)
            .Add(p => p.PropertyName, "Description"));

        // Assert
        var options = cut.FindAll("option").Skip(1).ToList(); // Skip placeholder
        Assert.Equal("Desc1", options[0].TextContent);
        Assert.Equal("Desc2", options[1].TextContent);
        Assert.Equal("Desc3", options[2].TextContent);
    }

    [Fact]
    public void TwSelect_SelectsValue_WhenInitiallySet()
    {
        // Arrange
        var values = _threeStringOptions;

        // Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Values, values)
            .Add(p => p.SelectedValue, "Option2"));

        // Assert
        var options = cut.FindAll("option");
        var selectedOption = options.FirstOrDefault(o => o.HasAttribute("selected") && o.GetAttribute("value") != "0");
        Assert.NotNull(selectedOption);
        Assert.Equal("Option2", selectedOption.TextContent);
    }

    [Fact]
    public void TwSelect_SelectsValue_WhenSelectedValueEqualsDefault()
    {
        // Arrange - regression test for a bug where PopulateValues unconditionally treated
        // any SelectedValue equal to default(T) as unselectable, even when that value was
        // present in Values. 0 is both a legitimate option and default(int).
        var cut = TestContext.Render<TwSelect<int>>(parameters => parameters
            .Add(p => p.Values, _zeroOneTwoIntOptions)
            .Add(p => p.SelectedValue, 0));

        // Assert
        var options = cut.FindAll("option");
        var selectedOption = options.FirstOrDefault(o => o.HasAttribute("selected") && o.GetAttribute("value") != "0");
        Assert.NotNull(selectedOption);
        Assert.Equal("0", selectedOption.TextContent);
    }

    [Fact]
    public void TwSelect_InvokesSelectedValueChanged_OnChange()
    {
        // Arrange
        var values = _threeStringOptions;
        string? selectedValue = null;

        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Values, values)
            .Add(p => p.SelectedValueChanged, EventCallback.Factory.Create<string>(this, v => selectedValue = v)));

        // Act
        var select = cut.Find("select");
        select.Change("2"); // Index of Option2

        // Assert
        Assert.Equal("Option2", selectedValue);
    }

    [Fact]
    public void TwSelect_InvokesSelectedValueChanged_WithComplexObject()
    {
        // Arrange
        var values = new[]
        {
            new TestModel { Id = 1, Name = "First" },
            new TestModel { Id = 2, Name = "Second" },
            new TestModel { Id = 3, Name = "Third" }
        };
        TestModel? selectedModel = null;

        var cut = TestContext.Render<TwSelect<TestModel>>(parameters => parameters
            .Add(p => p.Values, values)
            .Add(p => p.PropertyName, "Name")
            .Add(p => p.SelectedValueChanged, EventCallback.Factory.Create<TestModel>(this, v => selectedModel = v)));

        // Act
        var select = cut.Find("select");
        select.Change("2");

        // Assert
        Assert.NotNull(selectedModel);
        Assert.Equal(2, selectedModel.Id);
        Assert.Equal("Second", selectedModel.Name);
    }

    [Fact]
    public void TwSelect_Renders_WithId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Id, "custom-select-id")
            .Add(p => p.Values, _twoStringOptions));

        // Assert
        var select = cut.Find("select");
        Assert.Equal("custom-select-id", select.GetAttribute("id"));
    }

    [Fact]
    public void TwSelect_Renders_WithLabelAndId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Id, "country-select")
            .Add(p => p.Label, "Country")
            .Add(p => p.Values, _countryOptions));

        // Assert
        var label = cut.Find("label");
        var select = cut.Find("select");
        Assert.Equal("country-select", label.GetAttribute("for"));
        Assert.Equal("country-select", select.GetAttribute("id"));
    }

    [Fact]
    public void TwSelect_Renders_WithCustomClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Class, "custom-select-class")
            .Add(p => p.Values, _twoStringOptions));

        // Assert
        var select = cut.Find("select");
        Assert.Contains("custom-select-class", select.GetAttribute("class"));
        Assert.Contains(inputTheme.SelectBase, select.GetAttribute("class")); // Default class should still be present
    }

    [Fact]
    public void TwSelect_Renders_WithCustomLabelClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Label, "Select")
            .Add(p => p.LabelClass, "text-blue-600")
            .Add(p => p.Values, _twoStringOptions));

        // Assert
        var label = cut.Find("label");
        Assert.Contains("text-blue-600", label.GetAttribute("class"));
        Assert.Contains(inputTheme.LabelBase, label.GetAttribute("class")); // Default class should still be present
    }

    [Fact]
    public void TwSelect_HasDefaultClasses()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Values, _twoStringOptions));

        // Assert
        var select = cut.Find("select");
        var classes = select.GetAttribute("class");
        // TwSelect rewrites the shared "focus:" variant to "focus-visible:" so a native <select>
        // (which focuses on click same as keyboard) only shows the border on keyboard focus.
        var defaultVariantClasses = InputVariantBuilder.GetClasses(inputTheme.DefaultInputVariant, inputTheme)
            .Replace("focus:", "focus-visible:", StringComparison.Ordinal);
        Assert.Contains(inputTheme.SelectBase, classes);
        Assert.Contains(defaultVariantClasses, classes);
    }

    [Fact]
    public void TwSelect_UsesGlobalDefaultVariant_WhenNotSet()
    {
        // Arrange - no Variant set on the component, so it must follow TwInputTheme.DefaultInputVariant
        // (inherited via TwBlazorInputComponentBase.effectiveVariant), even after the theme changes.
        inputTheme.DefaultInputVariant = InputVariant.Outlined;

        // Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Values, _twoStringOptions));

        // Assert
        var classes = cut.Find("select").GetAttribute("class");
        var expected = InputVariantBuilder.GetClasses(InputVariant.Outlined, inputTheme)
            .Replace("focus:", "focus-visible:", StringComparison.Ordinal);
        Assert.Contains(expected, classes);
    }

    [Fact]
    public void TwSelect_ExplicitVariant_OverridesGlobalDefault()
    {
        // Arrange - the global default is Outlined, but this instance explicitly asks for Filled.
        inputTheme.DefaultInputVariant = InputVariant.Outlined;

        // Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Values, _twoStringOptions)
            .Add(p => p.Variant, InputVariant.Filled));

        // Assert
        var classes = cut.Find("select").GetAttribute("class");
        var expected = InputVariantBuilder.GetClasses(InputVariant.Filled, inputTheme)
            .Replace("focus:", "focus-visible:", StringComparison.Ordinal);
        Assert.Contains(expected, classes);
    }

    [Fact]
    public void TwSelect_HasAppearanceNone()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Values, _twoStringOptions));

        // Assert
        var select = cut.Find("select");
        var classes = select.GetAttribute("class");
        Assert.Contains(inputTheme.SelectBase, classes);
    }

    [Fact]
    public void TwSelect_HasCustomDropdownArrow()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Values, _twoStringOptions));

        // Assert - Verify the custom SVG background is applied via SelectBaseClasses
        var select = cut.Find("select");
        var classes = select.GetAttribute("class");
        Assert.Contains(inputTheme.SelectBase, classes);
    }

    [Fact]
    public void TwSelect_ReadOnly_RemovesDropdownArrow()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.ReadOnly, true)
            .Add(p => p.Values, _twoStringOptions));

        // Assert - Verify the background is removed for readonly
        var select = cut.Find("select");
        var classes = select.GetAttribute("class");
        Assert.Contains("!bg-none", classes);
    }

    [Fact]
    public void TwSelect_NotReadOnly_HasDropdownArrow()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.ReadOnly, false)
            .Add(p => p.Values, _twoStringOptions));

        // Assert - Verify the background is present when not readonly
        var select = cut.Find("select");
        var classes = select.GetAttribute("class");
        Assert.Contains(inputTheme.SelectBase, classes);
        Assert.DoesNotContain("!bg-none", classes);
    }

    [Fact]
    public void TwSelect_Disabled_KeepsDropdownArrow()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.Values, _twoStringOptions));

        // Assert - Disabled state should keep the arrow, only readonly removes it
        var select = cut.Find("select");
        var classes = select.GetAttribute("class");
        Assert.Contains(inputTheme.SelectBase, classes);
        Assert.DoesNotContain("!bg-none", classes);
        Assert.Contains("opacity-40", classes);
        Assert.Contains("cursor-not-allowed", classes);
    }

    [Fact]
    public void TwSelect_Renders_WithRootId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.RootId, "root-container")
            .Add(p => p.Values, _twoStringOptions));

        // Assert
        var rootDiv = cut.Find("div[id='root-container']");
        Assert.NotNull(rootDiv);
    }

    [Fact]
    public void TwSelect_GeneratesRootId_WhenNotProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Values, _twoStringOptions));

        // Assert
        var rootDiv = cut.Find("div");
        var id = rootDiv.GetAttribute("id");
        Assert.NotNull(id);
        Assert.NotEmpty(id);
    }

    [Fact]
    public void TwSelect_Renders_WithRootClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.RootClass, "custom-root-class")
            .Add(p => p.Values, _twoStringOptions));

        // Assert
        var rootDiv = cut.Find("div");
        Assert.Contains("custom-root-class", rootDiv.GetAttribute("class"));
    }

    [Fact]
    public void TwSelect_Renders_WithDisabled()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.Values, _twoStringOptions));

        // Assert
        var select = cut.Find("select");
        Assert.True(select.HasAttribute("disabled"));
        Assert.Contains("opacity-40", select.GetAttribute("class"));
        Assert.Contains("cursor-not-allowed", select.GetAttribute("class"));
    }

    [Fact]
    public void TwSelect_Renders_WithReadOnly()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.ReadOnly, true)
            .Add(p => p.Values, _twoStringOptions));

        // Assert
        var select = cut.Find("select");
        // ReadOnly must stay focusable/in the tab order - using disabled here would remove it
        // from the tab order and drop its value from submission, so it's conveyed via
        // aria-readonly instead (HandleChange blocks the actual value change).
        Assert.False(select.HasAttribute("disabled"));
        Assert.Equal("true", select.GetAttribute("aria-readonly"));
        // Readonly should NOT have opacity-40 (only disabled has that)
        Assert.DoesNotContain("opacity-40", select.GetAttribute("class"));
        // Readonly should remove the dropdown arrow
        Assert.Contains("!bg-none", select.GetAttribute("class"));
    }

    [Fact]
    public void TwSelect_DoesNotInvokeCallback_WhenReadonly()
    {
        // Arrange
        string? selectedValue = null;
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.ReadOnly, true)
            .Add(p => p.Values, _twoStringOptions)
            .Add(p => p.SelectedValueChanged, EventCallback.Factory.Create<string>(this, v => selectedValue = v)));

        // Act
        var select = cut.Find("select");
        select.Change("1");

        // Assert - Event handler should not be invoked when readonly
        Assert.Null(selectedValue);
    }

    [Fact]
    public void TwSelect_DoesNotInvokeCallback_WhenDisabled()
    {
        // Arrange
        string? selectedValue = null;
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.Values, _twoStringOptions)
            .Add(p => p.SelectedValueChanged, EventCallback.Factory.Create<string>(this, v => selectedValue = v)));

        // Act
        var select = cut.Find("select");
        select.Change("1");

        // Assert - Event handler should not be invoked when disabled
        Assert.Null(selectedValue);
    }

    [Fact]
    public void TwSelect_Renders_WithAttributes()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Attributes, new Dictionary<string, object>
            {
                { "data-test", "test-value" }
            })
            .Add(p => p.AriaLabel, "Select option")
            .Add(p => p.Values, _twoStringOptions));

        // Assert
        var select = cut.Find("select");
        Assert.Equal("test-value", select.GetAttribute("data-test"));
        // aria-label is set via the AriaLabel component parameter, not the generic Attributes
        // dictionary - a stray "aria-label" key in Attributes must never silently override it.
        Assert.Equal("Select option", select.GetAttribute("aria-label"));
    }

    [Fact]
    public void TwSelect_Renders_WithEmptyValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Values, Array.Empty<string>()));

        // Assert
        var options = cut.FindAll("option");
        Assert.Single(options); // Only placeholder
    }

    [Fact]
    public void TwSelect_Renders_WithLabelAttributes()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Label, "Test Label")
            .Add(p => p.LabelAttributes, new Dictionary<string, object>
            {
                { "data-label-test", "label-value" }
            })
            .Add(p => p.Values, _singleStringOption));

        // Assert
        var label = cut.Find("label");
        Assert.Equal("label-value", label.GetAttribute("data-label-test"));
    }

    [Fact]
    public void TwSelect_MaintainsSelection_AcrossMultipleChanges()
    {
        // Arrange
        var values = _threeStringOptions;
        string? selectedValue = null;

        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Values, values)
            .Add(p => p.SelectedValueChanged, EventCallback.Factory.Create<string>(this, v => selectedValue = v)));

        var select = cut.Find("select");

        // Act - First change
        select.Change("1");
        Assert.Equal("Option1", selectedValue);

        // Act - Second change
        select.Change("3");
        Assert.Equal("Option3", selectedValue);

        // Act - Third change
        select.Change("2");
        Assert.Equal("Option2", selectedValue);
    }

    [Fact]
    public void TwSelect_HandlesNullPropertyName_WithComplexObjects()
    {
        // Arrange
        var values = new[]
        {
            new TestModel { Id = 1, Name = "First" },
            new TestModel { Id = 2, Name = "Second" }
        };

        // Act - PropertyName is null or empty, should use ToString()
        var cut = TestContext.Render<TwSelect<TestModel>>(parameters => parameters
            .Add(p => p.Values, values)
            .Add(p => p.PropertyName, string.Empty));

        // Assert
        var options = cut.FindAll("option").Skip(1).ToList();
        Assert.Equal(2, options.Count);
        // Should contain ToString() representation
        Assert.NotEmpty(options[0].TextContent);
    }

    [Fact]
    public void TwSelect_Renders_WithMultipleParameters()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Id, "full-example")
            .Add(p => p.Label, "Select Country")
            .Add(p => p.Placeholder, "Choose a country...")
            .Add(p => p.Class, "custom-class")
            .Add(p => p.LabelClass, "custom-label")
            .Add(p => p.RootClass, "custom-root")
            .Add(p => p.Values, _countryOptions)
            .Add(p => p.SelectedValue, "USA"));

        // Assert
        var rootDiv = cut.Find("div");
        var label = cut.Find("label");
        var select = cut.Find("select");

        Assert.Contains("custom-root", rootDiv.GetAttribute("class"));
        Assert.Equal("Select Country", label.TextContent);
        Assert.Contains("custom-label", label.GetAttribute("class"));
        Assert.Equal("full-example", select.GetAttribute("id"));
        Assert.Contains("custom-class", select.GetAttribute("class"));

        var placeholderOption = cut.Find("option[value='0']");
        Assert.Equal("Choose a country...", placeholderOption.TextContent);
    }

    [Fact]
    public void TwSelect_ReadOnly_DoesNotApplyDisabledAttribute()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.ReadOnly, true)
            .Add(p => p.Values, _twoStringOptions)
            .Add(p => p.SelectedValue, "Option1"));

        // Assert - ReadOnly must not disable the control (that would remove it from the tab
        // order/AT); HTML select doesn't support a native readonly attribute either, so the
        // read-only state is conveyed via aria-readonly instead.
        var select = cut.Find("select");
        Assert.False(select.HasAttribute("disabled"));
        Assert.DoesNotContain("readonly", select.Attributes.Select(a => a.Name));
        Assert.Equal("true", select.GetAttribute("aria-readonly"));
    }

    [Fact]
    public void TwSelect_ReadOnly_PreventsValueChange()
    {
        // Arrange
        var initialValue = "Option1";
        var changedValue = initialValue;
        var callbackInvoked = false;

        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.ReadOnly, true)
            .Add(p => p.Values, _threeStringOptions)
            .Add(p => p.SelectedValue, initialValue)
            .Add(p => p.SelectedValueChanged, EventCallback.Factory.Create<string>(this, v =>
            {
                callbackInvoked = true;
                changedValue = v;
            })));

        // Act - Try to change the value
        var select = cut.Find("select");
        select.Change("2"); // Try to change to Option2

        // Assert - Value should not change and callback should not be invoked
        Assert.False(callbackInvoked);
        Assert.Equal(initialValue, changedValue);
    }

    [Fact]
    public void TwSelect_Disabled_PreventsValueChange()
    {
        // Arrange
        var initialValue = "Option1";
        var changedValue = initialValue;
        var callbackInvoked = false;

        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.Values, _threeStringOptions)
            .Add(p => p.SelectedValue, initialValue)
            .Add(p => p.SelectedValueChanged, EventCallback.Factory.Create<string>(this, v =>
            {
                callbackInvoked = true;
                changedValue = v;
            })));

        // Act - Try to change the value
        var select = cut.Find("select");
        select.Change("2"); // Try to change to Option2

        // Assert - Value should not change and callback should not be invoked
        Assert.False(callbackInvoked);
        Assert.Equal(initialValue, changedValue);
    }

    [Fact]
    public void TwSelect_ReadOnly_AndDisabled_BothApplyDisabledAttribute()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.ReadOnly, true)
            .Add(p => p.Disabled, true)
            .Add(p => p.Values, _twoStringOptions));

        // Assert
        var select = cut.Find("select");
        Assert.True(select.HasAttribute("disabled"));
        Assert.Contains("opacity-40", select.GetAttribute("class"));
        Assert.Contains("cursor-not-allowed", select.GetAttribute("class"));
    }

    [Fact]
    public void TwSelect_Renders_ActualText_ForValueTypeDefault()
    {
        // Arrange & Act - regression test: GetDisplayText previously blanked out any value
        // equal to default(T), including 0, which is a legitimate value for a non-nullable
        // value type like int (not an "absent" value the way null is).
        var cut = TestContext.Render<TwSelect<int>>(parameters => parameters
            .Add(p => p.Values, _zeroOneTwoIntOptions));

        // Assert
        var options = cut.FindAll("option").Skip(1).ToList(); // Skip placeholder
        Assert.Equal("0", options[0].TextContent);
        Assert.Equal("1", options[1].TextContent);
    }

    [Fact]
    public void TwSelect_Renders_EmptyText_ForNullValue()
    {
        // Arrange & Act - GetDisplayText still returns string.Empty for a genuinely null value.
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Values, new[] { null!, "Option1" }));

        // Assert
        var options = cut.FindAll("option").Skip(1).ToList(); // Skip placeholder
        Assert.Equal(string.Empty, options[0].TextContent);
        Assert.Equal("Option1", options[1].TextContent);
    }

    [Fact]
    public void TwSelect_FallsBackToToString_WhenPropertyNameDoesNotExist()
    {
        // Arrange
        var values = new[] { new TestModel { Id = 1, Name = "First" } };

        // Act - PropertyName doesn't match any property on TestModel, so GetProperty
        // returns null and display text falls back to value.ToString().
        var cut = TestContext.Render<TwSelect<TestModel>>(parameters => parameters
            .Add(p => p.Values, values)
            .Add(p => p.PropertyName, "NoSuchProperty"));

        // Assert
        var option = cut.FindAll("option").Skip(1).First();
        Assert.Equal(values[0].ToString(), option.TextContent);
    }

    [Fact]
    public void TwSelect_HandleChange_DoesNothing_WhenValueIsNotNumeric()
    {
        // Arrange
        var callbackInvoked = false;
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Values, _twoStringOptions)
            .Add(p => p.SelectedValueChanged, EventCallback.Factory.Create<string>(this, _ => callbackInvoked = true)));

        // Act
        var select = cut.Find("select");
        select.Change("not-a-number");

        // Assert
        Assert.False(callbackInvoked);
    }

    [Fact]
    public void TwSelect_HandleChange_DoesNothing_WhenEventValueIsNull()
    {
        // Arrange
        var callbackInvoked = false;
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Values, _twoStringOptions)
            .Add(p => p.SelectedValueChanged, EventCallback.Factory.Create<string>(this, _ => callbackInvoked = true)));

        // Act
        var select = cut.Find("select");
        select.Change(new ChangeEventArgs { Value = null });

        // Assert
        Assert.False(callbackInvoked);
    }

    [Fact]
    public void TwSelect_NotMultiple_DoesNotRenderMultipleAttribute()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Values, _threeStringOptions));

        // Assert
        var select = cut.Find("select");
        Assert.False(select.HasAttribute("multiple"));
    }

    // --- Multiple: custom popover trigger (PreferNativePicker=false, e.g. desktop) ---

    [Fact]
    public void TwSelect_Multiple_Custom_RendersTriggerMatchingSingleSelectStyle()
    {
        // Arrange & Act - the closed trigger should look like a normal TwSelect (same base classes,
        // including the dropdown arrow), not a plain always-expanded native multi-select listbox.
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Values, _threeStringOptions));

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find("select"));
        var button = cut.Find("button[aria-haspopup='listbox']");
        var trigger = button.ParentElement!;
        Assert.Contains(inputTheme.SelectBase, trigger.GetAttribute("class"));
    }

    [Fact]
    public void TwSelect_Multiple_Custom_PanelOptions_HideCheckboxesAndShowTickForSelectedRows()
    {
        // Arrange
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Values, _threeStringOptions)
            .Add(p => p.SelectedValues, _option1Selected));

        // Act
        cut.Find("button[aria-haspopup='listbox']").Click();

        // Assert - the checkboxes stay in the DOM (focusable, accessible) but are visually hidden,
        // and selected rows are marked by a tick and tint rather than a checked box.
        var surface = cut.Find("div[role='dialog']").FirstElementChild!.GetAttribute("class");
        Assert.Contains("[&_input]:sr-only", surface);
        Assert.Contains("[&_label:has(input:checked)::before]:visible", surface);
        Assert.Contains("[&_label:has(input:checked)]:bg-purple-50", surface);
        Assert.Equal(3, cut.FindAll("div[role='dialog'] input[type='checkbox']").Count);
    }

    [Fact]
    public void TwSelect_Multiple_Custom_ShowsPlaceholder_WhenNothingSelected()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Placeholder, "Pick some options...")
            .Add(p => p.Values, _threeStringOptions));

        // Assert
        var button = cut.Find("button[aria-haspopup='listbox']");
        var trigger = button.ParentElement!;
        Assert.Contains("Pick some options...", trigger.TextContent);
    }

    [Fact]
    public void TwSelect_Multiple_Custom_ShowsChip_PerSelectedValue()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Values, _threeStringOptions)
            .Add(p => p.SelectedValues, _option1AndOption3Selected));

        // Assert
        var button = cut.Find("button[aria-haspopup='listbox']");
        var trigger = button.ParentElement!;
        Assert.Contains("Option1", trigger.TextContent);
        Assert.Contains("Option3", trigger.TextContent);
        Assert.DoesNotContain("Option2", trigger.TextContent);
        Assert.DoesNotContain("Pick", trigger.TextContent);
    }

    [Fact]
    public void TwSelect_Multiple_Custom_ChipsUseLargeSize()
    {
        // Arrange & Act - larger than TwChip's Small/Medium defaults so selected-option chips stay
        // legible without growing the trigger box itself.
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Values, _threeStringOptions)
            .Add(p => p.SelectedValues, _option1Selected));

        // Assert - TwChipTheme.Lg is the only size that sets h-8. The chip itself is the inner <span>,
        // nested inside the @onclick:stopPropagation wrapper <span> that keeps chip removal from also
        // toggling the popover.
        var button = cut.Find("button[aria-haspopup='listbox']");
        var trigger = button.ParentElement!;
        var chip = trigger.QuerySelector("span span");
        Assert.NotNull(chip);
        Assert.Contains("h-8", chip.GetAttribute("class"));
    }

    [Fact]
    public void TwSelect_Multiple_Custom_OpensPanel_OnTriggerClick()
    {
        // Arrange
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Values, _threeStringOptions));

        // Act
        cut.Find("button[aria-haspopup='listbox']").Click();

        // Assert - one checkbox per option, inside a dialog-role popover panel
        var panel = cut.Find("div[role='dialog']");
        Assert.Equal(3, panel.QuerySelectorAll("input[type='checkbox']").Length);
    }

    [Fact]
    public void OpeningPanel_PositionsItAsFixed_MatchingTheTriggersWidth()
    {
        // Regression test: the options panel must be positioned via twPicker.registerScrollReposition
        // with matchAnchorWidth=true (which applies twPicker.positionPanelFixed itself -
        // position:fixed, sized/anchored via JS-computed coordinates), so it spans the trigger's
        // actual width and isn't clipped inside a scrollable ancestor such as a TwDialog's body - the
        // old CSS-only "absolute top-full left-0 w-full" never handled either.
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Values, _threeStringOptions));

        cut.Find("button[aria-haspopup='listbox']").Click();

        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.registerScrollReposition");
        Assert.IsType<ElementReference>(invocation.Arguments[0]);
        Assert.IsType<ElementReference>(invocation.Arguments[1]);
        Assert.True((bool)invocation.Arguments[2]!);
    }

    [Fact]
    public void TwSelect_Multiple_Custom_OpensPanel_OnTriggerClick_WhenAlreadyHasSelectedValues()
    {
        // Arrange - regression test: once at least one option is selected the open button has no text
        // content of its own (the chips take its place), so it needs an explicit minimum height (see
        // TwInputTheme.SelectMultiOpenButton) or it collapses to zero height and stops being clickable.
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Values, _threeStringOptions)
            .Add(p => p.SelectedValues, _option1Selected));

        // Act
        cut.Find("button[aria-haspopup='listbox']").Click();

        // Assert
        var panel = cut.Find("div[role='dialog']");
        Assert.Equal(3, panel.QuerySelectorAll("input[type='checkbox']").Length);
    }

    [Fact]
    public void TwSelect_Multiple_Custom_PanelForcesReadableLabelTextColor()
    {
        // Arrange & Act - TwCheckboxTheme.LabelBase uses a muted caption color that's too low-contrast
        // in dark mode once it's a whole option list's primary text, so the panel surface forces it via
        // SelectPanelItemText (see TwInputTheme.SelectPanelItemText for why).
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Values, _threeStringOptions));
        cut.Find("button[aria-haspopup='listbox']").Click();

        // Assert
        var panel = cut.Find("div[role='dialog']");
        var surface = panel.QuerySelector("div");
        Assert.NotNull(surface);
        Assert.Contains(inputTheme.SelectPanelItemText, surface.GetAttribute("class"));
    }

    [Fact]
    public void TwSelect_Multiple_Custom_DoesNotOpenPanel_WhenDisabled()
    {
        // Arrange
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Disabled, true)
            .Add(p => p.Values, _threeStringOptions));

        // Act
        cut.Find("button[aria-haspopup='listbox']").Click();

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find("div[role='dialog']"));
    }

    [Fact]
    public void TwSelect_Multiple_Custom_DoesNotOpenPanel_WhenReadOnly()
    {
        // Arrange
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.ReadOnly, true)
            .Add(p => p.Values, _threeStringOptions));

        // Act
        cut.Find("button[aria-haspopup='listbox']").Click();

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find("div[role='dialog']"));
    }

    [Fact]
    public void TwSelect_Multiple_Custom_TogglingCheckbox_InvokesSelectedValuesChanged()
    {
        // Arrange
        IEnumerable<string>? selectedValues = null;
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Values, _threeStringOptions)
            .Add(p => p.SelectedValuesChanged, EventCallback.Factory.Create<IEnumerable<string>>(this, v => selectedValues = v)));
        cut.Find("button[aria-haspopup='listbox']").Click();

        // Act - checks the first checkbox (Option1)
        cut.FindAll("input[type='checkbox']")[0].Change(true);

        // Assert
        Assert.NotNull(selectedValues);
        Assert.Equal(["Option1"], selectedValues);
    }

    [Fact]
    public void TwSelect_Multiple_Custom_RemovingChip_InvokesSelectedValuesChanged()
    {
        // Arrange
        IEnumerable<string>? selectedValues = null;
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Values, _threeStringOptions)
            .Add(p => p.SelectedValues, _option1AndOption3Selected)
            .Add(p => p.SelectedValuesChanged, EventCallback.Factory.Create<IEnumerable<string>>(this, v => selectedValues = v)));

        // Act - removes the Option1 chip via its close button, without opening the popover
        cut.Find("button[aria-label='Remove Option1']").Click();

        // Assert
        Assert.NotNull(selectedValues);
        Assert.Equal(["Option3"], selectedValues);
    }

    [Fact]
    public void TwSelect_Multiple_Custom_WithComplexObjects_TogglingCheckbox_InvokesSelectedValuesChanged()
    {
        // Arrange
        var values = new[]
        {
            new TestModel { Id = 1, Name = "First" },
            new TestModel { Id = 2, Name = "Second" },
            new TestModel { Id = 3, Name = "Third" }
        };
        IEnumerable<TestModel>? selectedModels = null;

        var cut = TestContext.Render<TwSelect<TestModel>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Values, values)
            .Add(p => p.PropertyName, "Name")
            .Add(p => p.SelectedValuesChanged, EventCallback.Factory.Create<IEnumerable<TestModel>>(this, v => selectedModels = v)));
        cut.Find("button[aria-haspopup='listbox']").Click();

        // Act
        cut.FindAll("input[type='checkbox']")[1].Change(true);

        // Assert
        Assert.NotNull(selectedModels);
        Assert.Equal(["Second"], selectedModels.Select(m => m.Name));
    }

    // --- Multiple: native overlay <select multiple> (PreferNativePicker=true, e.g. mobile) ---

    [Fact]
    public void TwSelect_Multiple_Native_RendersMultipleAttribute()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, true)
            .Add(p => p.Values, _threeStringOptions));

        // Assert
        var select = cut.Find("select");
        Assert.True(select.HasAttribute("multiple"));
    }

    [Fact]
    public void TwSelect_Multiple_Native_DoesNotRenderPlaceholderOption()
    {
        // Arrange & Act - a multi-select has no need for a "nothing selected" placeholder option,
        // since simply selecting nothing already represents that.
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, true)
            .Add(p => p.Values, _threeStringOptions));

        // Assert
        var options = cut.FindAll("option");
        Assert.Equal(3, options.Count);
        Assert.Throws<ElementNotFoundException>(() => cut.Find("option[value='0']"));
    }

    [Fact]
    public void TwSelect_Multiple_Native_SelectsInitialValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, true)
            .Add(p => p.Values, _threeStringOptions)
            .Add(p => p.SelectedValues, _option1AndOption3Selected));

        // Assert
        var options = cut.FindAll("option");
        var selected = options.Where(o => o.HasAttribute("selected")).Select(o => o.TextContent.Trim()).ToList();
        Assert.Equal(["Option1", "Option3"], selected);
    }

    [Fact]
    public void TwSelect_Multiple_Native_ReadOnly_BlocksPointerInteraction()
    {
        // Arrange & Act - ReadOnly can't use the native "readonly" attribute (invalid on <select>), so
        // it's conveyed via aria-readonly plus disabling pointer interaction, same as the single-select.
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, true)
            .Add(p => p.ReadOnly, true)
            .Add(p => p.Values, _threeStringOptions));

        // Assert
        var select = cut.Find("select");
        Assert.False(select.HasAttribute("disabled"));
        Assert.Equal("true", select.GetAttribute("aria-readonly"));
        Assert.Contains(Theme.Interaction.PointerEventsNone, select.GetAttribute("class"));
    }

    [Fact]
    public void TwSelect_Multiple_Native_ShowsChip_PerSelectedValue()
    {
        // Arrange & Act - the decorative trigger behind the invisible native select shows the same
        // chip-per-selection look as the custom trigger.
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, true)
            .Add(p => p.Values, _threeStringOptions)
            .Add(p => p.SelectedValues, _option1AndOption3Selected));

        // Assert
        var decorative = cut.Find("div[aria-hidden='true']");
        Assert.Contains("Option1", decorative.TextContent);
        Assert.Contains("Option3", decorative.TextContent);
    }

    [Fact]
    public void TwSelect_Multiple_Native_InvokesSelectedValuesChanged_OnChange()
    {
        // Arrange
        IEnumerable<string>? selectedValues = null;
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, true)
            .Add(p => p.Values, _threeStringOptions)
            .Add(p => p.SelectedValuesChanged, EventCallback.Factory.Create<IEnumerable<string>>(this, v => selectedValues = v)));

        // Act - selects Option1 (id 1) and Option3 (id 3)
        var select = cut.Find("select");
        select.Change(new ChangeEventArgs { Value = _selectedIds1And3 });

        // Assert
        Assert.NotNull(selectedValues);
        Assert.Equal(["Option1", "Option3"], selectedValues);
    }

    [Fact]
    public void TwSelect_Multiple_Native_InvokesSelectedValuesChanged_WithEmptyCollection_WhenDeselectingAll()
    {
        // Arrange
        IEnumerable<string>? selectedValues = null;
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, true)
            .Add(p => p.Values, _threeStringOptions)
            .Add(p => p.SelectedValues, _option1Selected)
            .Add(p => p.SelectedValuesChanged, EventCallback.Factory.Create<IEnumerable<string>>(this, v => selectedValues = v)));

        // Act
        var select = cut.Find("select");
        select.Change(new ChangeEventArgs { Value = Array.Empty<string>() });

        // Assert
        Assert.NotNull(selectedValues);
        Assert.Empty(selectedValues);
    }

    [Fact]
    public void TwSelect_Multiple_Native_DoesNotInvokeCallback_WhenReadonly()
    {
        // Arrange
        var callbackInvoked = false;
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, true)
            .Add(p => p.ReadOnly, true)
            .Add(p => p.Values, _threeStringOptions)
            .Add(p => p.SelectedValuesChanged, EventCallback.Factory.Create<IEnumerable<string>>(this, _ => callbackInvoked = true)));

        // Act
        var select = cut.Find("select");
        select.Change(new ChangeEventArgs { Value = _selectedId1 });

        // Assert
        Assert.False(callbackInvoked);
    }

    [Fact]
    public void TwSelect_Multiple_Native_DoesNotInvokeCallback_WhenDisabled()
    {
        // Arrange
        var callbackInvoked = false;
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, true)
            .Add(p => p.Disabled, true)
            .Add(p => p.Values, _threeStringOptions)
            .Add(p => p.SelectedValuesChanged, EventCallback.Factory.Create<IEnumerable<string>>(this, _ => callbackInvoked = true)));

        // Act
        var select = cut.Find("select");
        select.Change(new ChangeEventArgs { Value = _selectedId1 });

        // Assert
        Assert.False(callbackInvoked);
    }

    [Fact]
    public void TwSelect_Multiple_Native_IgnoresNonArrayEventValue()
    {
        // Arrange - defensive: a plain scalar change value should never crash the multi-select handler.
        var callbackInvoked = false;
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, true)
            .Add(p => p.Values, _threeStringOptions)
            .Add(p => p.SelectedValuesChanged, EventCallback.Factory.Create<IEnumerable<string>>(this, _ => callbackInvoked = true)));

        // Act
        var select = cut.Find("select");
        select.Change("1");

        // Assert
        Assert.False(callbackInvoked);
    }

    [Fact]
    public void TwSelect_Multiple_Native_WithComplexObjects_SelectsAndChangesCorrectly()
    {
        // Arrange
        var values = new[]
        {
            new TestModel { Id = 1, Name = "First" },
            new TestModel { Id = 2, Name = "Second" },
            new TestModel { Id = 3, Name = "Third" }
        };
        IEnumerable<TestModel>? selectedModels = null;

        var cut = TestContext.Render<TwSelect<TestModel>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, true)
            .Add(p => p.Values, values)
            .Add(p => p.PropertyName, "Name")
            .Add(p => p.SelectedValuesChanged, EventCallback.Factory.Create<IEnumerable<TestModel>>(this, v => selectedModels = v)));

        // Act
        var select = cut.Find("select");
        select.Change(new ChangeEventArgs { Value = _selectedIds1And2 });

        // Assert
        Assert.NotNull(selectedModels);
        Assert.Equal(["First", "Second"], selectedModels.Select(m => m.Name));
    }
}
