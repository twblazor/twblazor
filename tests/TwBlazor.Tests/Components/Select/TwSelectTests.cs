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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
            .Add(p => p.Values, _threeStringOptions));

        var selectClass = cut.Find("select").GetAttribute("class");
        Assert.Contains(inputTheme.Size, selectClass);
        Assert.DoesNotContain(inputTheme.DenseSize, selectClass);
    }

    [Fact]
    public void TwSelect_UsesDenseSize_WhenDense()
    {
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
            .Add(x => x.PreferNativePicker, true)
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
        Assert.Contains(inputTheme.SelectMultiTriggerBase, trigger.GetAttribute("class"));
    }

    [Fact]
    public void TwSelect_Multiple_Custom_PanelOptions_ShowTickForSelectedRows()
    {
        // Arrange
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Values, _threeStringOptions)
            .Add(p => p.SelectedValues, _option1Selected));

        // Act
        cut.Find("button[aria-haspopup='listbox']").Click();

        // Assert - the same listbox rows as the single select, with a tick on the selected one
        var listbox = cut.Find("[role='listbox']");
        Assert.Equal("true", listbox.GetAttribute("aria-multiselectable"));
        var options = listbox.QuerySelectorAll("[role='option']");
        Assert.Equal(3, options.Length);
        Assert.Equal("true", options[0].GetAttribute("aria-selected"));
        Assert.Equal("false", options[1].GetAttribute("aria-selected"));
        Assert.NotNull(options[0].QuerySelector("i"));
        Assert.Null(options[1].QuerySelector("i"));
        Assert.Contains(inputTheme.SelectCustomOption, options[0].GetAttribute("class"));
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
        var panel = cut.Find("[role='listbox']");
        Assert.Equal(3, panel.QuerySelectorAll("[role='option']").Length);
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
        var panel = cut.Find("[role='listbox']");
        Assert.Equal(3, panel.QuerySelectorAll("[role='option']").Length);
    }

    [Fact]
    public void TwSelect_Multiple_Custom_UsesTheSameOptionStyleAsTheSingleSelect()
    {
        // Arrange - one shared option style keeps the two controls looking identical.
        var single = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Required, true)
            .Add(p => p.Values, _threeStringOptions));
        var multiple = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Values, _threeStringOptions));
        single.Find("[role='combobox']").Click();
        multiple.Find("[role='combobox']").Click();

        // Assert
        var singleClass = single.Find("[role='option']").GetAttribute("class");
        var multipleClass = multiple.Find("[role='option']").GetAttribute("class");
        Assert.Equal(singleClass, multipleClass);
    }

    [Fact]
    public void TwSelect_Multiple_Custom_ClickingAnOption_TogglesItAndKeepsTheListboxOpen()
    {
        // Arrange
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Values, _threeStringOptions));
        cut.Find("button[aria-haspopup='listbox']").Click();

        // Act
        cut.FindAll("[role='option']")[1].Click();
        cut.FindAll("[role='option']")[2].Click();
        cut.FindAll("[role='option']")[1].Click();

        // Assert
        Assert.NotEmpty(cut.FindAll("[role='listbox']"));
        Assert.Equal("false", cut.FindAll("[role='option']")[1].GetAttribute("aria-selected"));
        Assert.Equal("true", cut.FindAll("[role='option']")[2].GetAttribute("aria-selected"));
    }

    [Fact]
    public void TwSelect_Multiple_Custom_TriggerShowsAChevron_LikeTheSingleSelect()
    {
        var cut = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Values, _threeStringOptions));

        var trigger = cut.Find("button[aria-haspopup='listbox']").ParentElement!;

        Assert.NotNull(trigger.QuerySelector("i.bi-chevron-down"));
    }

    [Fact]
    public void TwSelect_Multiple_Native_TriggerMatchesTheDesktopTrigger()
    {
        // Arrange - on iOS and Android the decorative trigger is what people see behind the native overlay.
        var native = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, true)
            .Add(p => p.Values, _threeStringOptions));
        var desktop = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Values, _threeStringOptions));

        // Assert
        var nativeTrigger = native.Find("div[aria-hidden='true']");
        Assert.Contains(inputTheme.SelectMultiTriggerBase, nativeTrigger.GetAttribute("class"));
        Assert.NotNull(nativeTrigger.QuerySelector("i.bi-chevron-down"));
        Assert.Equal(
            desktop.Find("button[aria-haspopup='listbox']").ParentElement!.GetAttribute("class"),
            nativeTrigger.GetAttribute("class"));
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
        cut.Find("button[aria-haspopup='listbox']").ParentElement!.Click();

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find("[role='listbox']"));
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
        Assert.Throws<ElementNotFoundException>(() => cut.Find("[role='listbox']"));
    }

    [Fact]
    public void TwSelect_Multiple_Custom_TogglingAnOption_InvokesSelectedValuesChanged()
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
        cut.FindAll("[role='option']")[0].Click();

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
    public void TwSelect_Multiple_Custom_WithComplexObjects_TogglingAnOption_InvokesSelectedValuesChanged()
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
        cut.FindAll("[role='option']")[1].Click();

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

    // --- Single: custom listbox (desktop) ---

    [Fact]
    public void TwSelect_Single_RendersCustomCombobox_OnDesktop()
    {
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Id, "country")
            .Add(x => x.Values, _countryOptions)
            .Add(x => x.Placeholder, "Pick a country"));

        Assert.Empty(cut.FindAll("select"));
        var trigger = cut.Find("button#country");
        Assert.Equal("combobox", trigger.GetAttribute("role"));
        Assert.Equal("listbox", trigger.GetAttribute("aria-haspopup"));
        Assert.Equal("false", trigger.GetAttribute("aria-expanded"));
        Assert.Contains("Pick a country", trigger.TextContent);
    }

    [Fact]
    public void TwSelect_Single_RendersNativeSelect_WhenNativePickerPreferred()
    {
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.PreferNativePicker, true)
            .Add(x => x.Values, _countryOptions));

        Assert.NotNull(cut.Find("select"));
        Assert.Empty(cut.FindAll("[role='combobox']"));
    }

    [Fact]
    public void TwSelect_Single_ShowsSelectedValue_OnTrigger()
    {
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Values, _countryOptions)
            .Add(x => x.SelectedValue, "UK"));

        Assert.Equal("UK", cut.Find("[role='combobox']").TextContent.Trim());
    }

    [Fact]
    public void TwSelect_Single_OpensListboxWithOptions_OnClick()
    {
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Id, "country")
            .Add(x => x.Required, true)
            .Add(x => x.Values, _countryOptions)
            .Add(x => x.SelectedValue, "UK"));

        cut.Find("[role='combobox']").Click();

        Assert.Equal("true", cut.Find("[role='combobox']").GetAttribute("aria-expanded"));
        var options = cut.FindAll("[role='listbox'] [role='option']");
        Assert.Equal(["USA", "UK", "Canada"], options.Select(o => o.TextContent.Trim()));
        Assert.Equal("true", options[1].GetAttribute("aria-selected"));
        Assert.Equal("false", options[0].GetAttribute("aria-selected"));
    }

    [Fact]
    public void TwSelect_Single_OffersThePlaceholderAsAnOption_WhenNotRequired()
    {
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Placeholder, "None")
            .Add(x => x.Values, _countryOptions));

        cut.Find("[role='combobox']").Click();

        var options = cut.FindAll("[role='option']");
        Assert.Equal(4, options.Count);
        Assert.Equal("None", options[0].TextContent.Trim());
    }

    [Fact]
    public void TwSelect_Single_ClickingAnOption_SelectsItAndClosesTheListbox()
    {
        string? selected = null;
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Required, true)
            .Add(x => x.Values, _countryOptions)
            .Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<string>(this, v => selected = v)));

        cut.Find("[role='combobox']").Click();
        cut.FindAll("[role='option']")[2].Click();

        Assert.Equal("Canada", selected);
        Assert.Empty(cut.FindAll("[role='listbox']"));
        Assert.Equal("Canada", cut.Find("[role='combobox']").TextContent.Trim());
        Assert.Equal("false", cut.Find("[role='combobox']").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void TwSelect_Single_ClickingTheSelectedOption_DoesNotInvokeCallback()
    {
        var calls = 0;
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Required, true)
            .Add(x => x.Values, _countryOptions)
            .Add(x => x.SelectedValue, "USA")
            .Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<string>(this, _ => calls++)));

        cut.Find("[role='combobox']").Click();
        cut.FindAll("[role='option']")[0].Click();

        Assert.Equal(0, calls);
    }

    [Fact]
    public void TwSelect_Single_ArrowDown_OpensTheListbox()
    {
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Values, _countryOptions));

        cut.Find("[role='combobox']").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowDown" });

        Assert.NotEmpty(cut.FindAll("[role='listbox']"));
    }

    [Fact]
    public void TwSelect_Single_Escape_ClosesTheListbox()
    {
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Values, _countryOptions));

        cut.Find("[role='combobox']").Click();
        cut.Find("[role='listbox']").ParentElement!.ParentElement!.KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Escape" });

        Assert.Empty(cut.FindAll("[role='listbox']"));
    }

    [Fact]
    public void TwSelect_Single_Disabled_DisablesTheTrigger_AndDoesNotOpen()
    {
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Disabled, true)
            .Add(x => x.Values, _countryOptions));

        Assert.True(cut.Find("[role='combobox']").HasAttribute("disabled"));
    }

    [Fact]
    public void TwSelect_Single_ReadOnly_DoesNotOpen()
    {
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.ReadOnly, true)
            .Add(x => x.Values, _countryOptions));

        cut.Find("[role='combobox']").Click();

        Assert.Empty(cut.FindAll("[role='listbox']"));
        Assert.Equal("true", cut.Find("[role='combobox']").GetAttribute("aria-readonly"));
    }

    [Fact]
    public void TwSelect_Single_UsesPropertyName_ForOptionText()
    {
        var values = new[] { new TestModel { Id = 1, Name = "First" }, new TestModel { Id = 2, Name = "Second" } };
        var cut = TestContext.Render<TwSelect<TestModel>>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Required, true)
            .Add(x => x.Values, values)
            .Add(x => x.PropertyName, "Name"));

        cut.Find("[role='combobox']").Click();

        Assert.Equal(["First", "Second"], cut.FindAll("[role='option']").Select(o => o.TextContent.Trim()));
    }

    [Fact]
    public void TwSelect_Close_IsInvokableFromJavaScript()
    {
        // JS interop resolves [JSInvokable] methods on the runtime type, so the override must carry the
        // attribute itself; without it a scroll or outside click throws instead of closing the listbox.
        var method = typeof(TwSelect<string>).GetMethod(nameof(TwSelect<string>.Close), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);

        Assert.NotNull(method);
        var attribute = method.GetCustomAttributes(typeof(Microsoft.JSInterop.JSInvokableAttribute), inherit: false).Cast<Microsoft.JSInterop.JSInvokableAttribute>().Single();
        Assert.Equal("Close", attribute.Identifier);
    }

    [Fact]
    public void TwSelect_SelectOption_IsInvokableFromJavaScript()
    {
        var method = typeof(TwSelect<string>).GetMethod(nameof(TwSelect<string>.SelectOptionAsync));

        var attribute = method!.GetCustomAttributes(typeof(Microsoft.JSInterop.JSInvokableAttribute), inherit: false).Cast<Microsoft.JSInterop.JSInvokableAttribute>().Single();
        Assert.Equal("SelectOption", attribute.Identifier);
    }

    [Fact]
    public async Task TwSelect_Dispose_DoesNotThrow_WhenTheCircuitHasDisconnected()
    {
        TestContext.JSInterop.SetupVoid("twPicker.unregisterOutsideClick", _ => true)
            .SetException(new Microsoft.JSInterop.JSDisconnectedException("circuit gone"));
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Values, _countryOptions));
        cut.Find("[role='combobox']").Click();

        var exception = await Record.ExceptionAsync(async () => await cut.Instance.DisposeAsync());

        Assert.Null(exception);
    }

    [Fact]
    public async Task TwSelect_Dispose_DoesNotThrow_WhenNeverOpened()
    {
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Values, _countryOptions));

        var exception = await Record.ExceptionAsync(async () => await cut.Instance.DisposeAsync());

        Assert.Null(exception);
    }

    [Fact]
    public void TwSelect_Multiple_Custom_TightensStartPadding_OnlyWhileShowingChips()
    {
        var empty = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Values, _threeStringOptions));
        var withChips = TestContext.Render<TwSelect<string>>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.PreferNativePicker, false)
            .Add(p => p.Values, _threeStringOptions)
            .Add(p => p.SelectedValues, _option1Selected));

        // The first chip should sit as far from the start edge as from the top and bottom.
        Assert.DoesNotContain(inputTheme.SelectMultiChipsPadding, empty.Find("button[aria-haspopup='listbox']").ParentElement!.GetAttribute("class"));
        Assert.Contains(inputTheme.SelectMultiChipsPadding, withChips.Find("button[aria-haspopup='listbox']").ParentElement!.GetAttribute("class"));
    }

    // --- Coverage: guards, keyboard and detection paths of the desktop listbox ---

    private IRenderedComponent<TwSelect<string>> RenderSingle(Action<ComponentParameterCollectionBuilder<TwSelect<string>>>? configure = null, bool required = true) =>
        TestContext.Render<TwSelect<string>>(p =>
        {
            p.Add(x => x.PreferNativePicker, false)
             .Add(x => x.Required, required)
             .Add(x => x.Values, _countryOptions);
            configure?.Invoke(p);
        });

    [Fact]
    public async Task TwSelect_SelectOption_IgnoresAnUnknownId()
    {
        var calls = 0;
        var cut = RenderSingle(p => p.Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<string>(this, _ => calls++)));

        await cut.InvokeAsync(() => cut.Instance.SelectOptionAsync(99));

        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task TwSelect_SelectOption_Placeholder_ClearsTheDisplayWithoutInvokingTheCallback()
    {
        var calls = 0;
        var cut = RenderSingle(p => p
            .Add(x => x.Placeholder, "None")
            .Add(x => x.SelectedValue, "UK")
            .Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<string>(this, _ => calls++)), required: false);
        Assert.Equal("UK", cut.Find("[role='combobox']").TextContent.Trim());

        await cut.InvokeAsync(() => cut.Instance.SelectOptionAsync(0));

        Assert.Equal(0, calls);
        Assert.Equal("None", cut.Find("[role='combobox']").TextContent.Trim());
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task TwSelect_SelectOption_IsIgnored_WhenDisabledOrReadOnly(bool disabled, bool readOnly)
    {
        var calls = 0;
        var cut = RenderSingle(p => p
            .Add(x => x.Disabled, disabled)
            .Add(x => x.ReadOnly, readOnly)
            .Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<string>(this, _ => calls++)));

        await cut.InvokeAsync(() => cut.Instance.SelectOptionAsync(2));

        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData("ArrowUp", true)]
    [InlineData("ArrowDown", true)]
    [InlineData("a", false)]
    [InlineData("Enter", false)]
    public void TwSelect_Single_TriggerKeyDown_OpensOnlyForArrowKeys(string key, bool opens)
    {
        var cut = RenderSingle();

        cut.Find("[role='combobox']").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = key });

        Assert.Equal(opens, cut.FindAll("[role='listbox']").Count == 1);
    }

    [Fact]
    public void TwSelect_Single_ArrowKey_DoesNotReopenOrCloseAnOpenListbox()
    {
        var cut = RenderSingle();
        cut.Find("[role='combobox']").Click();

        cut.Find("[role='combobox']").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowDown" });

        Assert.Single(cut.FindAll("[role='listbox']"));
    }

    [Fact]
    public async Task TwSelect_Close_ClosesAnOpenListbox_AndCanBeCalledAgain()
    {
        var cut = RenderSingle();
        cut.Find("[role='combobox']").Click();

        await cut.InvokeAsync(() => cut.Instance.Close());
        await cut.InvokeAsync(() => cut.Instance.Close());

        Assert.Empty(cut.FindAll("[role='listbox']"));
    }

    [Fact]
    public async Task TwSelect_Dispose_WhileOpen_DoesNotThrow()
    {
        var cut = RenderSingle();
        cut.Find("[role='combobox']").Click();

        var exception = await Record.ExceptionAsync(async () => await cut.Instance.DisposeAsync());

        Assert.Null(exception);
    }

    [Fact]
    public void TwSelect_Single_OpeningAttachesTheListbox_WithTheSelectedOptionAndSelectCommit()
    {
        var cut = RenderSingle(p => p.Add(x => x.SelectedValue, "UK"));

        cut.Find("[role='combobox']").Click();

        var attach = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twSelect.attachListbox");
        Assert.EndsWith("-option-2", (string)attach.Arguments[2]!);
        Assert.Equal("SelectOption", attach.Arguments[3]);
    }

    [Fact]
    public void TwSelect_Multiple_OpeningAttachesTheListbox_WithTheFirstSelectedOptionAndToggleCommit()
    {
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.Multiple, true)
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Values, _threeStringOptions)
            .Add(x => x.SelectedValues, _option1AndOption3Selected));

        cut.Find("[role='combobox']").Click();

        var attach = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twSelect.attachListbox");
        Assert.EndsWith("-option-1", (string)attach.Arguments[2]!);
        Assert.Equal("ToggleOption", attach.Arguments[3]);
    }

    [Fact]
    public void TwSelect_Multiple_ArrowDown_OpensTheListbox()
    {
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.Multiple, true)
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Values, _threeStringOptions));

        cut.Find("[role='combobox']").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowDown" });

        Assert.Single(cut.FindAll("[role='listbox']"));
    }

    [Fact]
    public async Task TwSelect_ToggleOption_SelectsThenDeselects_AndReportsEachChange()
    {
        var reported = new List<string[]>();
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.Multiple, true)
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Values, _threeStringOptions)
            .Add(x => x.SelectedValuesChanged, EventCallback.Factory.Create<IEnumerable<string>>(this, v => reported.Add([.. v]))));

        await cut.InvokeAsync(() => cut.Instance.ToggleOptionAsync(3));
        await cut.InvokeAsync(() => cut.Instance.ToggleOptionAsync(1));
        await cut.InvokeAsync(() => cut.Instance.ToggleOptionAsync(3));

        Assert.Equal(["Option3"], reported[0]);
        Assert.Equal(["Option1", "Option3"], reported[1]);
        Assert.Equal(["Option1"], reported[2]);
    }

    [Fact]
    public async Task TwSelect_ToggleOption_IgnoresUnknownIds_AndDisabledOrReadOnly()
    {
        var calls = 0;
        var callback = EventCallback.Factory.Create<IEnumerable<string>>(this, _ => calls++);
        var enabled = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.Multiple, true).Add(x => x.PreferNativePicker, false)
            .Add(x => x.Values, _threeStringOptions).Add(x => x.SelectedValuesChanged, callback));
        var disabled = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.Multiple, true).Add(x => x.PreferNativePicker, false).Add(x => x.Disabled, true)
            .Add(x => x.Values, _threeStringOptions).Add(x => x.SelectedValuesChanged, callback));
        var readOnly = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.Multiple, true).Add(x => x.PreferNativePicker, false).Add(x => x.ReadOnly, true)
            .Add(x => x.Values, _threeStringOptions).Add(x => x.SelectedValuesChanged, callback));

        await enabled.InvokeAsync(() => enabled.Instance.ToggleOptionAsync(42));
        await disabled.InvokeAsync(() => disabled.Instance.ToggleOptionAsync(1));
        await readOnly.InvokeAsync(() => readOnly.Instance.ToggleOptionAsync(1));

        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TwSelect_UsesTheNativePicker_WhenTheDeviceReportsAMobilePlatform(bool multiple)
    {
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(true);

        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.Multiple, multiple)
            .Add(x => x.Values, _countryOptions));

        Assert.NotEmpty(cut.FindAll("select"));
        Assert.Empty(cut.FindAll("[role='combobox']"));
    }

    [Fact]
    public void TwSelect_UsesTheListbox_WhenTheDeviceReportsADesktopPlatform()
    {
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(false);

        var cut = TestContext.Render<TwSelect<string>>(p => p.Add(x => x.Values, _countryOptions));

        Assert.Empty(cut.FindAll("select"));
        Assert.NotEmpty(cut.FindAll("[role='combobox']"));
    }

    // --- Coverage: selecting without a bound callback, and the defensive release path ---

    [Fact]
    public async Task TwSelect_SelectOption_WorksWithoutABoundCallback()
    {
        var cut = RenderSingle();

        await cut.InvokeAsync(() => cut.Instance.SelectOptionAsync(2));

        Assert.Equal("UK", cut.Find("[role='combobox']").TextContent.Trim());
    }

    [Fact]
    public async Task TwSelect_SelectOption_Placeholder_WhenAlreadyOnThePlaceholder_ChangesNothing()
    {
        var calls = 0;
        var cut = RenderSingle(p => p
            .Add(x => x.Placeholder, "None")
            .Add(x => x.SelectedValueChanged, EventCallback.Factory.Create<string>(this, _ => calls++)), required: false);

        await cut.InvokeAsync(() => cut.Instance.SelectOptionAsync(0));

        Assert.Equal(0, calls);
        Assert.Equal("None", cut.Find("[role='combobox']").TextContent.Trim());
    }

    [Fact]
    public async Task TwSelect_ToggleOption_WorksWithoutABoundCallback()
    {
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.Multiple, true)
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Values, _threeStringOptions));

        await cut.InvokeAsync(() => cut.Instance.ToggleOptionAsync(2));

        Assert.Contains("Option2", cut.Find("button[aria-haspopup='listbox']").ParentElement!.TextContent);
    }

    [Fact]
    public async Task TwSelect_Dispose_ToleratesTheOutsideClickHandleAlreadyBeingGone()
    {
        // The release path is defensive about the handle being null (for example if a close raced a
        // dispose), so it must neither throw nor skip clearing the registered flag.
        var cut = RenderSingle();
        cut.Find("[role='combobox']").Click();
        typeof(TwPopoverPickerComponentBase)
            .GetField("dotNetRef", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(cut.Instance, null);

        var exception = await Record.ExceptionAsync(async () => await cut.Instance.DisposeAsync());

        Assert.Null(exception);
    }
}
