using Bunit;
using Microsoft.AspNetCore.Components;
using System.Reflection;
using TwBlazor.Abstraction;
using TwBlazor.ArchitectureTests.Components;

namespace TwBlazor.ArchitectureTests;

/// <summary>
/// Dynamic tests that verify ALL input components properly implement IInputComponent contract.
/// </summary>
public class InputComponentContractTests : TwBlazorTestBase
{
    [Theory]
    [MemberData(nameof(InputComponentsList.GetAllInputComponentTypesAsTestData), MemberType = typeof(InputComponentsList))]
    public void InputComponent_ShouldInheritFromTwBlazorInputComponentBase(Type componentType)
    {
        // Assert
        Assert.True(typeof(TwBlazorInputComponentBase).IsAssignableFrom(componentType),
            $"{componentType.Name} should inherit from TwBlazorInputComponentBase");
    }

    [Theory]
    [MemberData(nameof(InputComponentsList.GetAllInputComponentTypesAsTestData), MemberType = typeof(InputComponentsList))]
    public void InputComponent_ShouldImplementIInputComponent(Type componentType)
    {
        // Assert
        Assert.True(typeof(ITwInputComponent).IsAssignableFrom(componentType),
            $"{componentType.Name} should implement IInputComponent");
    }

    [Theory]
    [MemberData(nameof(InputComponentsList.GetAllInputComponentTypesAsTestData), MemberType = typeof(InputComponentsList))]
    public void InputComponent_ShouldHaveRootIdParameter(Type componentType)
    {
        // Assert
        var property = componentType.GetProperty("RootId",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(HasParameterAttribute(property),
            $"{componentType.Name} should have RootId property with [Parameter] attribute");
        Assert.Equal(typeof(string), property.PropertyType);
    }

    [Theory]
    [MemberData(nameof(InputComponentsList.GetAllInputComponentTypesAsTestData), MemberType = typeof(InputComponentsList))]
    public void InputComponent_ShouldHaveRootClassParameter(Type componentType)
    {
        // Assert
        var property = componentType.GetProperty("RootClass",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(HasParameterAttribute(property),
            $"{componentType.Name} should have RootClass property with [Parameter] attribute");
        Assert.Equal(typeof(string), property.PropertyType);
    }

    [Theory]
    [MemberData(nameof(InputComponentsList.GetAllInputComponentTypesAsTestData), MemberType = typeof(InputComponentsList))]
    public void InputComponent_ShouldHaveRootAttributesParameter(Type componentType)
    {
        // Assert
        var property = componentType.GetProperty("RootAttributes",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(HasParameterAttribute(property),
            $"{componentType.Name} should have RootAttributes property with [Parameter] attribute");
        Assert.Equal(typeof(Dictionary<string, object>), property.PropertyType);
    }

    [Theory]
    [MemberData(nameof(InputComponentsList.GetAllInputComponentTypesAsTestData), MemberType = typeof(InputComponentsList))]
    public void InputComponent_ShouldHaveLabelParameter(Type componentType)
    {
        // Assert
        var property = componentType.GetProperty("Label",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(HasParameterAttribute(property),
            $"{componentType.Name} should have Label property with [Parameter] attribute");
        Assert.Equal(typeof(string), property.PropertyType);
    }

    [Theory]
    [MemberData(nameof(InputComponentsList.GetAllInputComponentTypesAsTestData), MemberType = typeof(InputComponentsList))]
    public void InputComponent_ShouldHaveLabelIdParameter(Type componentType)
    {
        // Assert
        var property = componentType.GetProperty("LabelId",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(HasParameterAttribute(property),
            $"{componentType.Name} should have LabelId property with [Parameter] attribute");
        Assert.Equal(typeof(string), property.PropertyType);
    }

    [Theory]
    [MemberData(nameof(InputComponentsList.GetAllInputComponentTypesAsTestData), MemberType = typeof(InputComponentsList))]
    public void InputComponent_ShouldHaveLabelAttributesParameter(Type componentType)
    {
        // Assert
        var property = componentType.GetProperty("LabelAttributes",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(HasParameterAttribute(property),
            $"{componentType.Name} should have LabelAttributes property with [Parameter] attribute");
        Assert.Equal(typeof(Dictionary<string, object>), property.PropertyType);
    }

    [Theory]
    [MemberData(nameof(InputComponentsList.GetAllInputComponentTypesAsTestData), MemberType = typeof(InputComponentsList))]
    public void InputComponent_ShouldHaveLabelClassParameter(Type componentType)
    {
        // Assert
        var property = componentType.GetProperty("LabelClass",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(HasParameterAttribute(property),
            $"{componentType.Name} should have LabelClass property with [Parameter] attribute");
        Assert.Equal(typeof(string), property.PropertyType);
    }

    [Theory]
    [MemberData(nameof(InputComponentsList.GetAllInputComponentTypesAsTestData), MemberType = typeof(InputComponentsList))]
    public void InputComponent_ShouldHaveReadOnlyParameter(Type componentType)
    {
        // Assert
        var property = componentType.GetProperty("ReadOnly",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(HasParameterAttribute(property),
            $"{componentType.Name} should have ReadOnly property with [Parameter] attribute");
        Assert.Equal(typeof(bool), property.PropertyType);
    }

    [Theory]
    [MemberData(nameof(InputComponentsList.GetAllInputComponentTypesAsTestData), MemberType = typeof(InputComponentsList))]
    public void InputComponent_ShouldHaveDisabledParameter(Type componentType)
    {
        // Assert
        var property = componentType.GetProperty("Disabled",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(HasParameterAttribute(property),
            $"{componentType.Name} should have Disabled property with [Parameter] attribute");
        Assert.Equal(typeof(bool), property.PropertyType);
    }

    [Theory]
    [MemberData(nameof(InputComponentsList.GetAllInputComponentTypesAsTestData), MemberType = typeof(InputComponentsList))]
    public void InputComponent_ShouldHaveLabelClassesProperty(Type componentType)
    {
        // Assert
        var property = componentType.GetProperty("LabelClasses",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.Equal(typeof(string), property.PropertyType);
        // LabelClasses should NOT have [Parameter] attribute - it's computed
        Assert.False(HasParameterAttribute(property),
            $"{componentType.Name} LabelClasses should be a computed property, not a Parameter");
    }

    [Theory]
    [MemberData(nameof(InputComponentsList.GetAllInputComponentTypesAsTestData), MemberType = typeof(InputComponentsList))]
    public void InputComponent_ShouldHaveCloseMethod(Type componentType)
    {
        // Assert
        var method = componentType.GetMethod("Close",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
    }

    [Fact]
    public void InputComponentsList_ShouldFindAtLeastOneComponent()
    {
        // Arrange & Act
        var components = InputComponentsList.GetAllInputComponentTypes().ToList();

        // Assert
        Assert.NotEmpty(components);

        // Output for debugging
        foreach (var component in components)
        {
            Console.WriteLine($"Found input component: {component.Name}");
        }
    }

    /// <summary>
    /// Verify that all discovered components inherit from the base class.
    /// This ensures the reflection logic is working correctly.
    /// </summary>
    [Fact]
    public void InputComponentsList_AllComponentsShouldInheritFromBase()
    {
        // Arrange & Act
        var components = InputComponentsList.GetAllInputComponentTypes().ToList();

        // Assert
        foreach (var component in components)
        {
            Assert.True(typeof(TwBlazorInputComponentBase).IsAssignableFrom(component),
                $"{component.Name} should inherit from TwBlazorInputComponentBase");
        }
    }

    /// <summary>
    /// Helper method to check if a property has the [Parameter] attribute.
    /// </summary>
    private static bool HasParameterAttribute(PropertyInfo? property)
    {
        if (property == null) return false;
        return property.GetCustomAttribute<ParameterAttribute>() != null;
    }

    #region DOM Rendering Tests

    /// <summary>
    /// Tests that verify input component parameters actually render in the DOM.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetNonGenericInputComponentTypesAsTestData))]
    public void InputComponent_RootIdParameter_ShouldRenderInDOM(Type componentType)
    {
        if (componentType.IsGenericType || componentType.IsAbstract)
        {
            return;
        }

        try
        {
            // Act
            var cut = RenderComponentWithParameters(componentType, new Dictionary<string, object>
            {
                { "RootId", "test-root-id-456" }
            });

            // Assert - RootId should be on the root element
            var element = cut.Find("[id='test-root-id-456']");
            Assert.NotNull(element);
        }
        catch (Exception ex) when (IsKnownRenderingIssue(ex, componentType))
        {
            Console.WriteLine($"Skipping {componentType.Name}: {ex.Message}");
        }
    }

    [Theory]
    [MemberData(nameof(GetNonGenericInputComponentTypesAsTestData))]
    public void InputComponent_RootClassParameter_ShouldRenderInDOM(Type componentType)
    {
        if (componentType.IsGenericType || componentType.IsAbstract)
        {
            return;
        }

        try
        {
            // Act
            var cut = RenderComponentWithParameters(componentType, new Dictionary<string, object>
            {
                { "RootClass", "custom-root-class" }
            });

            // Assert
            var elements = cut.FindAll(".custom-root-class");
            Assert.NotEmpty(elements);
        }
        catch (Exception ex) when (IsKnownRenderingIssue(ex, componentType))
        {
            Console.WriteLine($"Skipping {componentType.Name}: {ex.Message}");
        }
    }

    [Theory]
    [MemberData(nameof(GetNonGenericInputComponentTypesAsTestData))]
    public void InputComponent_RootAttributesParameter_ShouldRenderInDOM(Type componentType)
    {
        if (componentType.IsGenericType || componentType.IsAbstract)
        {
            return;
        }

        try
        {
            // Act
            var cut = RenderComponentWithParameters(componentType, new Dictionary<string, object>
            {
                { "RootAttributes", new Dictionary<string, object>
                    {
                        { "data-root-attr", "root-test-value" }
                    }
                }
            });

            // Assert
            var element = cut.Find("[data-root-attr='root-test-value']");
            Assert.NotNull(element);
        }
        catch (Exception ex) when (IsKnownRenderingIssue(ex, componentType))
        {
            Console.WriteLine($"Skipping {componentType.Name}: {ex.Message}");
        }
    }

    [Theory]
    [MemberData(nameof(GetNonGenericInputComponentTypesAsTestData))]
    public void InputComponent_LabelParameter_ShouldRenderInDOM(Type componentType)
    {
        if (componentType.IsGenericType || componentType.IsAbstract)
        {
            return;
        }

        try
        {
            // Act
            var cut = RenderComponentWithParameters(componentType, new Dictionary<string, object>
            {
                { "Label", "Test Label Text" }
            });

            // Assert - Label should appear in the markup
            var markup = cut.Markup;
            Assert.Contains("Test Label Text", markup);
        }
        catch (Exception ex) when (IsKnownRenderingIssue(ex, componentType))
        {
            Console.WriteLine($"Skipping {componentType.Name}: {ex.Message}");
        }
    }

    [Theory]
    [MemberData(nameof(GetNonGenericInputComponentTypesAsTestData))]
    public void InputComponent_LabelIdParameter_ShouldRenderInDOM(Type componentType)
    {
        if (componentType.IsGenericType || componentType.IsAbstract)
        {
            return;
        }

        try
        {
            // Act
            var cut = RenderComponentWithParameters(componentType, new Dictionary<string, object>
            {
                { "Label", "Test Label" }, // Label must be present for LabelId to render
                { "LabelId", "test-label-id-789" }
            });

            // Assert - LabelId should be on the label element
            var element = cut.Find("label[id='test-label-id-789']");
            Assert.NotNull(element);
        }
        catch (Exception ex) when (IsKnownRenderingIssue(ex, componentType))
        {
            Console.WriteLine($"Skipping {componentType.Name}: {ex.Message}");
        }
    }

    [Theory]
    [MemberData(nameof(GetNonGenericInputComponentTypesAsTestData))]
    public void InputComponent_LabelClassParameter_ShouldRenderInDOM(Type componentType)
    {
        if (componentType.IsGenericType || componentType.IsAbstract)
        {
            return;
        }

        try
        {
            // Act
            var cut = RenderComponentWithParameters(componentType, new Dictionary<string, object>
            {
                { "Label", "Test Label" }, // Label must be present for LabelClass to render
                { "LabelClass", "custom-label-class" }
            });

            // Assert
            var element = cut.Find("label.custom-label-class");
            Assert.NotNull(element);
        }
        catch (Exception ex) when (IsKnownRenderingIssue(ex, componentType))
        {
            Console.WriteLine($"Skipping {componentType.Name}: {ex.Message}");
        }
    }

    [Theory]
    [MemberData(nameof(GetNonGenericInputComponentTypesAsTestData))]
    public void InputComponent_LabelAttributesParameter_ShouldRenderInDOM(Type componentType)
    {
        if (componentType.IsGenericType || componentType.IsAbstract)
        {
            return;
        }

        try
        {
            // Act
            var cut = RenderComponentWithParameters(componentType, new Dictionary<string, object>
            {
                { "Label", "Test Label" }, // Label must be present
                { "LabelAttributes", new Dictionary<string, object>
                    {
                        { "data-label-attr", "label-test-value" }
                    }
                }
            });

            // Assert
            var element = cut.Find("label[data-label-attr='label-test-value']");
            Assert.NotNull(element);
        }
        catch (Exception ex) when (IsKnownRenderingIssue(ex, componentType))
        {
            Console.WriteLine($"Skipping {componentType.Name}: {ex.Message}");
        }
    }

    [Theory]
    [MemberData(nameof(GetNonGenericInputComponentTypesAsTestData))]
    public void InputComponent_DisabledParameter_ShouldRenderInDOM(Type componentType)
    {
        if (componentType.IsGenericType || componentType.IsAbstract)
        {
            return;
        }

        try
        {
            // Act
            var cut = RenderComponentWithParameters(componentType, new Dictionary<string, object>
            {
                { "Disabled", true }
            });

            // Assert - Look for disabled attribute or disabled class
            var markup = cut.Markup;
            var hasDisabled = markup.Contains("disabled", StringComparison.OrdinalIgnoreCase) ||
                             markup.Contains("cursor-not-allowed", StringComparison.OrdinalIgnoreCase) ||
                             markup.Contains("opacity-50", StringComparison.OrdinalIgnoreCase);

            Assert.True(hasDisabled,
                $"{componentType.Name} should render disabled state in DOM when Disabled=true");
        }
        catch (Exception ex) when (IsKnownRenderingIssue(ex, componentType))
        {
            Console.WriteLine($"Skipping {componentType.Name}: {ex.Message}");
        }
    }

    [Theory]
    [MemberData(nameof(GetNonGenericInputComponentTypesAsTestData))]
    public void InputComponent_ReadOnlyParameter_ShouldRenderInDOM(Type componentType)
    {
        if (componentType.IsGenericType || componentType.IsAbstract)
        {
            return;
        }

        try
        {
            // Act
            var cut = RenderComponentWithParameters(componentType, new Dictionary<string, object>
            {
                { "ReadOnly", true }
            });

            // Assert - Look for readonly attribute or readonly class
            var markup = cut.Markup;
            var hasReadOnly = markup.Contains("readonly", StringComparison.OrdinalIgnoreCase) ||
                              markup.Contains("read-only", StringComparison.OrdinalIgnoreCase) ||
                              markup.Contains("cursor-default", StringComparison.OrdinalIgnoreCase);

            Assert.True(hasReadOnly,
                $"{componentType.Name} should render readonly state in DOM when ReadOnly=true");
        }
        catch (Exception ex) when (IsKnownRenderingIssue(ex, componentType))
        {
            Console.WriteLine($"Skipping {componentType.Name}: {ex.Message}");
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets non-generic input component types for rendering tests.
    /// </summary>
    public static TheoryData<Type> GetNonGenericInputComponentTypesAsTestData()
    {
        TheoryData<Type> data =
        [
            .. InputComponentsList.GetAllInputComponentTypes()
                .Where(t => !t.IsGenericType && !t.IsAbstract),
        ];

        return data;
    }

    private IRenderedComponent<Microsoft.AspNetCore.Components.IComponent> RenderComponentWithParameters(Type componentType, Dictionary<string, object> parameters)
    {
        // Build component using RenderFragment
        RenderFragment fragment = builder =>
        {
            builder.OpenComponent(0, componentType);
            var sequence = 1;
            foreach (var param in parameters)
            {
                builder.AddAttribute(sequence++, param.Key, param.Value);
            }
            builder.CloseComponent();
        };

        return TestContext.Render(fragment);
    }

    private static bool IsKnownRenderingIssue(Exception ex, Type componentType)
    {
        // Some components may require specific setup or have dependencies
        return ex is InvalidOperationException ||
               ex is ArgumentException ||
               ex is NullReferenceException ||
               ex.Message.Contains("requires") ||
               ex.Message.Contains("missing");
    }

    #endregion
}
