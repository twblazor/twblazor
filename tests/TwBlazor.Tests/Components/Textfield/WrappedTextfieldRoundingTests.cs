using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Textfield;

/// <summary>
/// Inputs that render a <see cref="TwTextfield{T}"/> inside them must pass their own Rounded through to it,
/// rather than leaving the field on the theme's default.
/// </summary>
public class WrappedTextfieldRoundingTests : TwBlazorTestBase
{
    public static TheoryData<Type> WrappingInputs =>
    [
        typeof(TwNumeric<int>),
        typeof(TwDatePicker),
        typeof(TwDateTimePicker),
        typeof(TwDateRangePicker),
        typeof(TwDateTimeRangePicker),
        typeof(TwTimePicker),
        typeof(TwTimeRangePicker),
        typeof(TwColorPicker),
    ];

    [Theory]
    [MemberData(nameof(WrappingInputs))]
    public void WrappedTextfield_UsesTheComponentsRounded(Type component)
    {
        // Arrange - the theme default differs from the instance value, so a field ignoring it would show rounded-lg
        Theme.Rounded.DefaultRounded = Rounded.Lg;

        // Act
        var cut = TestContext.Render(builder =>
        {
            builder.OpenComponent(0, component);
            builder.AddAttribute(1, nameof(TwBlazorComponentBase.Rounded), (Rounded?)Rounded.Full);
            builder.AddAttribute(2, nameof(TwBlazorTextInputComponentBase.Variant), (InputVariant?)InputVariant.Outlined);
            builder.CloseComponent();
        });

        // Assert
        var input = cut.Find("input");
        Assert.Contains("rounded-full", input.ClassList);
        Assert.DoesNotContain("rounded-lg", input.ClassList);
    }
}
