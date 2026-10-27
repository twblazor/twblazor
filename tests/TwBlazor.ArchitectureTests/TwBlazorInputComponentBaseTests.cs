using Bunit;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.ArchitectureTests;

public class TwBlazorInputComponentBaseTests : TwBlazorTestBase
{
    [Fact]
    public void TwBlazorInputComponentBase_GeneratesId_WhenNotProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestInputComponent>();

        // Assert
        var input = cut.Find("input");
        var id = input.GetAttribute("id");
        Assert.NotNull(id);
        Assert.StartsWith("testinputcomponent-", id);
    }

    [Fact]
    public void TwBlazorInputComponentBase_UsesProvidedId_WhenSpecified()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestInputComponent>(parameters => parameters
            .Add(p => p.Id, "custom-input-id"));

        // Assert
        var input = cut.Find("input");
        var id = input.GetAttribute("id");
        Assert.Equal("custom-input-id", id);
    }

    [Fact]
    public void TwBlazorInputComponentBase_RendersLabel_WhenLabelProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestInputComponent>(parameters => parameters
            .Add(p => p.Label, "Username"));

        // Assert
        var label = cut.Find("label");
        Assert.NotNull(label);
        Assert.Equal("Username", label.TextContent);
    }

    [Fact]
    public void TwBlazorInputComponentBase_DoesNotRenderLabel_WhenLabelEmpty()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestInputComponent>(parameters => parameters
            .Add(p => p.Label, string.Empty));

        // Assert
        Assert.Throws<Bunit.ElementNotFoundException>(() => cut.Find("label"));
    }

    [Fact]
    public void TwBlazorInputComponentBase_LabelClasses_IncludesCustomClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestInputComponent>(parameters => parameters
            .Add(p => p.Label, "Email")
            .Add(p => p.LabelClass, "text-blue-600"));

        // Assert
        var label = cut.Find("label");
        var classes = label.GetAttribute("class");

        Assert.Contains("text-blue-600", classes);
    }

    [Fact]
    public void TwBlazorInputComponentBase_LabelId_IsSetCorrectly()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestInputComponent>(parameters => parameters
            .Add(p => p.Label, "Password")
            .Add(p => p.LabelId, "password-label"));

        // Assert
        var label = cut.Find("label");
        Assert.Equal("password-label", label.GetAttribute("id"));
    }

    [Fact]
    public void TwBlazorInputComponentBase_LabelFor_MatchesInputId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestInputComponent>(parameters => parameters
            .Add(p => p.Label, "Email")
            .Add(p => p.Id, "email-input"));

        // Assert
        var label = cut.Find("label");
        var input = cut.Find("input");
        Assert.Equal("email-input", label.GetAttribute("for"));
        Assert.Equal("email-input", input.GetAttribute("id"));
    }

    [Fact]
    public void TwBlazorInputComponentBase_LabelAttributes_AreApplied()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestInputComponent>(parameters => parameters
            .Add(p => p.Label, "Test")
            .Add(p => p.LabelAttributes, new Dictionary<string, object>
            {
                { "data-test", "label-attr" },
                { "title", "Test Title" }
            }));

        // Assert
        var label = cut.Find("label");
        Assert.Equal("label-attr", label.GetAttribute("data-test"));
        Assert.Equal("Test Title", label.GetAttribute("title"));
    }

    [Fact]
    public void TwBlazorInputComponentBase_Readonly_IsFalseByDefault()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestInputComponent>();

        // Assert
        var input = cut.Find("input");
        Assert.False(input.HasAttribute("readonly"));
    }

    [Fact]
    public void TwBlazorInputComponentBase_Readonly_IsAppliedWhenTrue()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestInputComponent>(parameters => parameters
            .Add(p => p.ReadOnly, true));

        // Assert
        var input = cut.Find("input");
        Assert.True(input.HasAttribute("readonly"));
    }

    [Fact]
    public void TwBlazorInputComponentBase_Disabled_IsFalseByDefault()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestInputComponent>();

        // Assert
        var input = cut.Find("input");
        Assert.False(input.HasAttribute("disabled"));
    }

    [Fact]
    public void TwBlazorInputComponentBase_Disabled_IsAppliedWhenTrue()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestInputComponent>(parameters => parameters
            .Add(p => p.Disabled, true));

        // Assert
        var input = cut.Find("input");
        Assert.True(input.HasAttribute("disabled"));
    }

    [Fact]
    public void TwBlazorInputComponentBase_SupportsMultipleProperties()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestInputComponent>(parameters => parameters
            .Add(p => p.Id, "full-input")
            .Add(p => p.Label, "Full Name")
            .Add(p => p.LabelClass, "font-bold")
            .Add(p => p.LabelId, "full-name-label")
            .Add(p => p.ReadOnly, true));

        // Assert
        var label = cut.Find("label");
        var input = cut.Find("input");

        Assert.Equal("Full Name", label.TextContent);
        Assert.Equal("full-name-label", label.GetAttribute("id"));
        Assert.Contains("font-bold", label.GetAttribute("class"));
        Assert.Equal("full-input", label.GetAttribute("for"));
        Assert.Equal("full-input", input.GetAttribute("id"));
        Assert.True(input.HasAttribute("readonly"));
    }

    [Fact]
    public void TwBlazorInputComponentBase_GeneratesUniqueIds_ForMultipleInstances()
    {
        // Arrange & Act
        var cut1 = TestContext.Render<TestInputComponent>();
        var cut2 = TestContext.Render<TestInputComponent>();

        // Assert
        var id1 = cut1.Find("input").GetAttribute("id");
        var id2 = cut2.Find("input").GetAttribute("id");
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void TwBlazorInputComponentBase_InheritsFromTwBlazorComponentBase()
    {
        // Arrange & Act
        var component = new TestInputComponent();

        // Assert
        Assert.IsType<TwBlazorComponentBase>(component, exactMatch: false);
    }

    [Fact]
    public void TwBlazorInputComponentBase_SupportsAriaLabel()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestInputComponent>(parameters => parameters
            .Add(p => p.AriaLabel, "Enter your username"));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("Enter your username", input.GetAttribute("aria-label"));
    }

    [Fact]
    public void TwBlazorInputComponentBase_SupportsAriaLabelledBy()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestInputComponent>(parameters => parameters
            .Add(p => p.AriaLabelledBy, "custom-label-id"));

        // Assert
        var input = cut.Find("input");
        Assert.Equal("custom-label-id", input.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void TwBlazorInputComponentBase_BothReadonlyAndDisabled_CanBeSetTogether()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestInputComponent>(parameters => parameters
            .Add(p => p.ReadOnly, true)
            .Add(p => p.Disabled, true));

        // Assert
        var input = cut.Find("input");
        Assert.True(input.HasAttribute("readonly"));
        Assert.True(input.HasAttribute("disabled"));
    }

    [Fact]
    public void TwBlazorInputComponentBase_ForwardsAttributes_ToRootAttributes_WhenRootAttributesEmpty()
    {
        // Arrange
        var attributes = new Dictionary<string, object> { { "data-test", "forwarded" } };

        // Act
        var cut = TestContext.Render<TestInputComponent>(parameters => parameters
            .Add(p => p.Attributes, attributes));

        // Assert
        Assert.Same(attributes, cut.Instance.RootAttributes);
    }

    [Fact]
    public void TwBlazorInputComponentBase_DoesNotOverrideRootAttributes_WhenExplicitlyProvided()
    {
        // Arrange
        var attributes = new Dictionary<string, object> { { "data-test", "attrs" } };
        var rootAttributes = new Dictionary<string, object> { { "data-root", "root-attrs" } };

        // Act
        var cut = TestContext.Render<TestInputComponent>(parameters => parameters
            .Add(p => p.Attributes, attributes)
            .Add(p => p.RootAttributes, rootAttributes));

        // Assert
        Assert.Same(rootAttributes, cut.Instance.RootAttributes);
        Assert.NotSame(attributes, cut.Instance.RootAttributes);
    }

    [Fact]
    public void TwBlazorInputComponentBase_RootAttributes_RemainsEmpty_WhenAttributesEmpty()
    {
        // Arrange & Act
        var cut = TestContext.Render<TestInputComponent>();

        // Assert
        Assert.Empty(cut.Instance.RootAttributes);
    }

    [Fact]
    public async Task TwBlazorInputComponentBase_Close_CompletesSuccessfully()
    {
        // Arrange
        var component = new TestInputComponent();

        // Act
        var exception = await Record.ExceptionAsync(() => component.Close());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void TwBlazorTextInputComponentBase_EffectiveVariant_UsesGlobalDefault_WhenNotSet()
    {
        // Arrange
        Theme.Components.Require<TwInputTheme>().DefaultInputVariant = InputVariant.Outlined;

        // Act
        var cut = TestContext.Render<TestInputComponent>();

        // Assert - no Variant was set on the component, so it falls back to the theme's default,
        // the same "global default => component-declared override" pattern as effectiveShadow/effectiveRounded.
        Assert.Equal(InputVariant.Outlined, cut.Instance.EffectiveVariant);
    }

    [Fact]
    public void TwBlazorTextInputComponentBase_EffectiveVariant_UsesComponentValue_WhenSet()
    {
        // Arrange
        Theme.Components.Require<TwInputTheme>().DefaultInputVariant = InputVariant.Outlined;

        // Act - the component explicitly declares a variant, which must win over the global default.
        var cut = TestContext.Render<TestInputComponent>(parameters => parameters
            .Add(p => p.Variant, InputVariant.Filled));

        // Assert
        Assert.Equal(InputVariant.Filled, cut.Instance.EffectiveVariant);
    }

    [Fact]
    public void TwBlazorTextInputComponentBase_EffectiveVariant_FollowsGlobalDefault_WhenThemeChanges()
    {
        // Arrange & Act - effectiveVariant is a live-computed property (not snapshotted at render
        // time), so each instance is asserted immediately after its own render, before the theme is
        // mutated again for the next case - otherwise the second mutation would be visible through
        // the first instance's getter too.
        Theme.Components.Require<TwInputTheme>().DefaultInputVariant = InputVariant.Default;
        var defaultCut = TestContext.Render<TestInputComponent>();
        Assert.Equal(InputVariant.Default, defaultCut.Instance.EffectiveVariant);

        Theme.Components.Require<TwInputTheme>().DefaultInputVariant = InputVariant.Filled;
        var filledCut = TestContext.Render<TestInputComponent>();
        Assert.Equal(InputVariant.Filled, filledCut.Instance.EffectiveVariant);
    }
}

// Test component
// Derives from TwBlazorTextInputComponentBase (rather than TwBlazorInputComponentBase directly) so it
// also carries Variant/effectiveVariant - see the EffectiveVariant tests below.
public class TestInputComponent : TwBlazorTextInputComponentBase
{
    /// <summary>
    /// Exposes the protected <c>effectiveVariant</c> so tests can assert on the global-default-vs-
    /// component-declared cascade without needing a real rendered class list to inspect.
    /// </summary>
    public InputVariant EffectiveVariant => effectiveVariant;

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        var sequence = 0;

        // Render label if provided
        if (!string.IsNullOrEmpty(Label))
        {
            builder.OpenElement(sequence++, "label");
            builder.AddAttribute(sequence++, "for", Id);

            if (!string.IsNullOrEmpty(LabelId))
            {
                builder.AddAttribute(sequence++, "id", LabelId);
            }

            builder.AddAttribute(sequence++, "class", LabelClasses);

            // Add label attributes
            if (LabelAttributes != null && LabelAttributes.Count > 0)
            {
                foreach (var attr in LabelAttributes)
                {
                    builder.AddAttribute(sequence++, attr.Key, attr.Value);
                }
            }

            builder.AddContent(sequence++, Label);
            builder.CloseElement();
        }

        // Render input
        builder.OpenElement(sequence++, "input");
        builder.AddAttribute(sequence++, "id", Id);
        builder.AddAttribute(sequence++, "class", Class);

        if (ReadOnly)
        {
            builder.AddAttribute(sequence++, "readonly", true);
        }

        if (Disabled)
        {
            builder.AddAttribute(sequence++, "disabled", true);
        }

        if (!string.IsNullOrEmpty(AriaLabel))
        {
            builder.AddAttribute(sequence++, "aria-label", AriaLabel);
        }

        if (!string.IsNullOrEmpty(AriaLabelledBy))
        {
            builder.AddAttribute(sequence++, "aria-labelledby", AriaLabelledBy);
        }

        builder.CloseElement();
    }
}
