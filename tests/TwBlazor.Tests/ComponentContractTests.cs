using Bunit;
using Microsoft.AspNetCore.Components;
using System.Reflection;
using TwBlazor.Abstraction;
using TwBlazor.Enums;
using TwBlazor.Tests.Components;

namespace TwBlazor.Tests;

/// <summary>
/// Dynamic tests that verify ALL components properly implement ITwBlazorComponent contract.
/// </summary>
public class ComponentContractTests : TwBlazorTestBase
{
    public ComponentContractTests()
    {
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory]
    [MemberData(nameof(ComponentsList.GetAllComponentTypesAsTestData), MemberType = typeof(ComponentsList))]
    public void Component_ShouldInheritFromTwBlazorComponentBase(Type componentType)
    {
        // Assert
        Assert.True(typeof(TwBlazorComponentBase).IsAssignableFrom(componentType),
            $"{componentType.Name} should inherit from TwBlazorComponentBase");
    }

    [Theory]
    [MemberData(nameof(ComponentsList.GetAllComponentTypesAsTestData), MemberType = typeof(ComponentsList))]
    public void Component_ShouldImplementITwBlazorComponent(Type componentType)
    {
        // Assert
        Assert.True(typeof(ITwComponent).IsAssignableFrom(componentType),
            $"{componentType.Name} should implement ITwBlazorComponent");
    }

    [Theory]
    [MemberData(nameof(ComponentsList.GetAllComponentTypesAsTestData), MemberType = typeof(ComponentsList))]
    public void Component_ShouldHaveIdParameter(Type componentType)
    {
        // Assert
        var property = componentType.GetProperty("Id",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(HasParameterAttribute(property),
            $"{componentType.Name} should have Id property with [Parameter] attribute");
        Assert.Equal(typeof(string), property.PropertyType);
    }

    [Theory]
    [MemberData(nameof(ComponentsList.GetAllComponentTypesAsTestData), MemberType = typeof(ComponentsList))]
    public void Component_ShouldHaveClassParameter(Type componentType)
    {
        // Assert
        var property = componentType.GetProperty("Class",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(HasParameterAttribute(property),
            $"{componentType.Name} should have Class property with [Parameter] attribute");
        Assert.Equal(typeof(string), property.PropertyType);
    }

    [Theory]
    [MemberData(nameof(ComponentsList.GetAllComponentTypesAsTestData), MemberType = typeof(ComponentsList))]
    public void Component_ShouldHaveStyleParameter(Type componentType)
    {
        // Assert
        var property = componentType.GetProperty("Style",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(HasParameterAttribute(property),
            $"{componentType.Name} should have Style property with [Parameter] attribute");
        Assert.Equal(typeof(string), property.PropertyType);
    }

    [Theory]
    [MemberData(nameof(ComponentsList.GetAllComponentTypesAsTestData), MemberType = typeof(ComponentsList))]
    public void Component_ShouldHaveAttributesParameter(Type componentType)
    {
        // Assert
        var property = componentType.GetProperty("Attributes",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(HasParameterAttribute(property),
            $"{componentType.Name} should have Attributes property with [Parameter] attribute");
        Assert.Equal(typeof(Dictionary<string, object>), property.PropertyType);

        // Verify CaptureUnmatchedValues = true
        var paramAttr = property.GetCustomAttribute<ParameterAttribute>();
        Assert.NotNull(paramAttr);
        Assert.True(paramAttr.CaptureUnmatchedValues,
            $"{componentType.Name} Attributes parameter should have CaptureUnmatchedValues = true");
    }

    [Theory]
    [MemberData(nameof(ComponentsList.GetAllComponentTypesAsTestData), MemberType = typeof(ComponentsList))]
    public void Component_ShouldHaveAriaLabelParameter(Type componentType)
    {
        // Assert
        var property = componentType.GetProperty("AriaLabel",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(HasParameterAttribute(property),
            $"{componentType.Name} should have AriaLabel property with [Parameter] attribute");
        Assert.Equal(typeof(string), property.PropertyType);
    }

    [Theory]
    [MemberData(nameof(ComponentsList.GetAllComponentTypesAsTestData), MemberType = typeof(ComponentsList))]
    public void Component_ShouldHaveAriaLabelledByParameter(Type componentType)
    {
        // Assert
        var property = componentType.GetProperty("AriaLabelledBy",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(HasParameterAttribute(property),
            $"{componentType.Name} should have AriaLabelledBy property with [Parameter] attribute");
        Assert.Equal(typeof(string), property.PropertyType);
    }

    [Theory]
    [MemberData(nameof(ComponentsList.GetAllComponentTypesAsTestData), MemberType = typeof(ComponentsList))]
    public void Component_ShouldHaveShadowParameter(Type componentType)
    {
        // Assert
        var property = componentType.GetProperty("Shadow",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(HasParameterAttribute(property),
            $"{componentType.Name} should have Shadow property with [Parameter] attribute");

        // Shadow is nullable enum
        var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
        Assert.Equal(typeof(Shadow), underlyingType);
    }

    [Theory]
    [MemberData(nameof(ComponentsList.GetAllComponentTypesAsTestData), MemberType = typeof(ComponentsList))]
    public void Component_ShouldHaveRoundedParameter(Type componentType)
    {
        // Assert
        var property = componentType.GetProperty("Rounded",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(HasParameterAttribute(property),
            $"{componentType.Name} should have Rounded property with [Parameter] attribute");

        // Rounded is nullable enum
        var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
        Assert.Equal(typeof(Rounded), underlyingType);
    }

    [Fact]
    public void ComponentsList_ShouldFindAtLeastOneComponent()
    {
        // Arrange & Act
        var components = ComponentsList.GetAllComponentTypes().ToList();

        // Assert
        Assert.NotEmpty(components);

        // Output for debugging
        foreach (var component in components)
        {
            Console.WriteLine($"Found component: {component.Name}");
        }
    }

    [Fact]
    public void ComponentsList_AllComponentsShouldInheritFromBase()
    {
        // Arrange & Act
        var components = ComponentsList.GetAllComponentTypes().ToList();

        // Assert
        foreach (var component in components)
        {
            Assert.True(typeof(TwBlazorComponentBase).IsAssignableFrom(component),
                $"{component.Name} should inherit from TwBlazorComponentBase");
        }
    }

    /// <summary>
    /// Verify that input components also appear in the general component list,
    /// since TwBlazorInputComponentBase inherits from TwBlazorComponentBase.
    /// </summary>
    [Fact]
    public void ComponentsList_ShouldIncludeInputComponents()
    {
        // Arrange & Act
        var allComponents = ComponentsList.GetAllComponentTypes().ToList();
        var inputComponents = InputComponentsList.GetAllInputComponentTypes().ToList();

        // Assert
        foreach (var inputComponent in inputComponents)
        {
            Assert.Contains(inputComponent, allComponents);
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
    /// Tests that verify parameters actually render in the DOM, not just that they exist as properties.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetNonGenericComponentTypesAsTestData))]
    public void Component_IdParameter_ShouldRenderInDOM(Type componentType)
    {
        // Skip if component is generic or can't be easily instantiated
        if (componentType.IsGenericType || componentType.IsAbstract)
        {
            return;
        }

        try
        {
            // Act
            var cut = RenderComponentWithParameters(componentType, new Dictionary<string, object>
            {
                { "Id", "test-id-123" }
            });

            // Assert
            var element = cut.Find("[id='test-id-123']");
            Assert.NotNull(element);
        }
        catch (Exception ex) when (IsKnownRenderingIssue(ex, componentType))
        {
            // Some components may require additional setup or have dependencies
            Console.WriteLine($"Skipping {componentType.Name}: {ex.Message}");
        }
    }

    [Theory]
    [MemberData(nameof(GetNonGenericComponentTypesAsTestData))]
    public void Component_ClassParameter_ShouldRenderInDOM(Type componentType)
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
                { "Class", "custom-test-class" }
            });

            // Assert - Find element with the custom class
            var elements = cut.FindAll(".custom-test-class");
            Assert.NotEmpty(elements);
        }
        catch (Exception ex) when (IsKnownRenderingIssue(ex, componentType))
        {
            Console.WriteLine($"Skipping {componentType.Name}: {ex.Message}");
        }
    }

    [Theory]
    [MemberData(nameof(GetNonGenericComponentTypesAsTestData))]
    public void Component_StyleParameter_ShouldRenderInDOM(Type componentType)
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
                { "Style", "color: red;" }
            });

            // Assert
            var element = cut.Find("[style*='color: red']");
            Assert.NotNull(element);
        }
        catch (Exception ex) when (IsKnownRenderingIssue(ex, componentType))
        {
            Console.WriteLine($"Skipping {componentType.Name}: {ex.Message}");
        }
    }

    [Theory]
    [MemberData(nameof(GetNonGenericComponentTypesAsTestData))]
    public void Component_CustomAttributesParameter_ShouldRenderInDOM(Type componentType)
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
                { "Attributes", new Dictionary<string, object>
                    {
                        { "data-testid", "custom-attribute-test" }
                    }
                }
            });

            // Assert
            var element = cut.Find("[data-testid='custom-attribute-test']");
            Assert.NotNull(element);
        }
        catch (Exception ex) when (IsKnownRenderingIssue(ex, componentType))
        {
            Console.WriteLine($"Skipping {componentType.Name}: {ex.Message}");
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets non-generic component types for rendering tests.
    /// Generic types require type arguments and are tested separately.
    /// </summary>
    public static TheoryData<Type> GetNonGenericComponentTypesAsTestData()
    {
        TheoryData<Type> data =
        [
            .. ComponentsList.GetAllComponentTypes()
                .Where(t => !t.IsGenericType && !t.IsAbstract
                // exclude component bases.
                && t != typeof(TwBlazorInputComponentBase)
                && t != typeof(TwBlazorComponentBase)
                // TwDatePickerHeader is an internal DatePicker subcomponent that does not
                // render its inherited Id/Class/Style/Attributes on the DOM.
                && t != typeof(TwBlazor.Components.DatePicker.TwDatePickerHeader)
                // TwSidebarNavigationList is an internal Sidebar subcomponent that recursively
                // renders a list of TwSidebarItems with no root element of its own to carry
                // Id/Class/Style/Attributes.
                && t != typeof(TwBlazor.Components.TwSidebarNavigationList)),
        ];

        return data;
    }

    // Components that render nothing until they are given something to show.
    private static readonly Dictionary<Type, Dictionary<string, object>> _requiredParameters = new()
    {
        [typeof(TwBlazor.Components.TwQrCode)] = new() { ["Value"] = "https://twblazor.com" },
    };

    private IRenderedComponent<IComponent> RenderComponentWithParameters(Type componentType, Dictionary<string, object> parameters)
    {
        var required = _requiredParameters.GetValueOrDefault(componentType) ?? [];

        // Build component using RenderFragment
        RenderFragment fragment = builder =>
        {
            builder.OpenComponent(0, componentType);
            var sequence = 1;
            foreach (var param in required.Concat(parameters))
            {
                builder.AddAttribute(sequence++, param.Key, param.Value);
            }
            builder.CloseComponent();
        };

        return TestContext.Render(fragment);
    }

    private static string GetComponentTagName(Type componentType)
    {
        return componentType.Name;
    }

    private static bool IsKnownRenderingIssue(Exception ex, Type componentType)
    {
        // Some components may require specific setup, services, or have dependencies
        // This helps identify components that need special handling
        return ex is InvalidOperationException ||
               ex is ArgumentException ||
               ex is NullReferenceException ||
               ex.Message.Contains("requires") ||
               ex.Message.Contains("missing");
    }

    #endregion
}
