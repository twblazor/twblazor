// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using System.Globalization;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a generic slider component that enables users to select a value within a specified range.
/// </summary>
/// <remarks>This component provides a customizable slider interface for selecting values of type T. It supports
/// two-way data binding and allows precise control over the value selection through configurable minimum, maximum, and
/// step values. The component is suitable for scenarios where users need to select numeric or comparable values within
/// defined bounds.</remarks>
/// <typeparam name="T">The type of the value represented by the slider. Must support comparison operations to allow range enforcement.</typeparam>
public partial class TwSlider<T> : TwBlazorInputComponentBase
{
    private TwSliderTheme theme => options.Theme.Components.Require<TwSliderTheme>();

    /// <summary>
    /// Gets or sets the current value of the parameter.
    /// </summary>
    [Parameter] public T Value { get; set; } = default!;

    /// <summary>
    /// Gets or sets the callback that is invoked when the value changes.
    /// </summary>
    /// <remarks>Use this property to handle value change events in two-way data binding scenarios. The
    /// callback is triggered when the component's value is updated, allowing parent components to respond to the
    /// change.</remarks>
    [Parameter] public EventCallback<T> ValueChanged { get; set; } = default!;

    /// <summary>
    /// Gets or sets the minimum allowed value for the parameter of type T.
    /// </summary>
    /// <remarks>The Min property must be set to a valid value before use. It defines the lower bound for the
    /// associated parameter, ensuring that values do not fall below this threshold.</remarks>
    [Parameter] public required T Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowable value for the parameter of type T.
    /// </summary>
    /// <remarks>This property is required and must be set to a valid value before use. Ensure that the value
    /// assigned to Max adheres to any constraints defined by the type T.</remarks>
    [Parameter] public required T Max { get; set; }

    /// <summary>
    /// Gets or sets the step value used to increment or decrement the slider's value.
    /// </summary>
    /// <remarks>The step value must be a positive number. It determines the amount by which the slider's
    /// value increases or decreases with each adjustment. Use this property to control the precision of value changes
    /// when interacting with the slider.</remarks>
    [Parameter] public required T Step { get; set; }

    /// <summary>
    /// Gets or sets the name associated with the component.
    /// </summary>
    /// <remarks>The name is used to identify the component in various contexts, such as when binding data or
    /// displaying information to the user.</remarks>
    [Parameter] public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the color of the slider.
    /// </summary>
    /// <remarks>
    /// When specified, applies an accent color class to the slider. If null, no color class is applied.
    /// </remarks>
    [Parameter] public Color? Color { get; set; }

    /// <summary>
    /// Gets the classes for the outer wrapper that hosts the interactive input and the custom visual track.
    /// /// </summary>
    private string wrapperClasses => new ClassBuilder(theme.Wrapper)
        .AddClass(RootClass)
        .Build();

    /// <summary>
    /// Gets the classes for the native range input. It is stretched over the full wrapper and made
    /// invisible so that pointer, keyboard and touch interaction (and accessibility semantics) are still
    /// handled natively, while the visible track/fill/thumb are drawn separately for full styling control.
    /// </summary>
    private string classes => new ClassBuilder(theme.Base)
        .AddClass(Disabled ? "opacity-40 cursor-not-allowed" : string.Empty)
        .AddClass(ReadOnly && !Disabled ? "pointer-events-none" : string.Empty)
        .AddClass(Class)
        .Build();

    /// <summary>
    /// Gets the classes for the background track behind the filled portion of the slider.
    /// </summary>
    private string trackClasses => new ClassBuilder(theme.Track)
        .AddClass(Disabled ? "opacity-40" : string.Empty)
        .Build();

    /// <summary>
    /// Gets the classes for the filled portion of the track, from the start up to the current value.
    /// /// </summary>
    /// <remarks>
    /// Width is intentionally not transitioned: it is driven by every "input" event while dragging, and
    /// animating it would make the fill visibly lag behind the (instantly-positioned) thumb on fast drags.
    /// </remarks>
    private string fillClasses => new ClassBuilder(theme.Fill)
        .AddClass(roundedBuilder.GetRounded())
        .AddClass(GetSliderColor(Color))
        .Build();

    /// <summary>
    /// Gets the classes for the draggable thumb positioned at the current value.
    /// </summary>
    private string thumbClasses => new ClassBuilder(theme.Thumb)
        .AddClass(colorBuilder.GetBorderColor(Color))
        .AddClass(colorBuilder.GetPeerFocusRing(Color))
        .AddClass(Disabled ? "opacity-40" : string.Empty)
        .Build();

    /// <summary>
    /// Gets the classes for the floating value tooltip shown above the thumb on hover/focus.
    /// </summary>
    /// <remarks>
    /// /// Only opacity/scale are transitioned (not the "left" position) so the tooltip tracks the thumb
    /// instantly while dragging and only animates its show/hide.
    /// </remarks>
    private string bubbleClasses => theme.Bubble;


    private string GetSliderColor(Color? color) => ColorBuilder.GetPaletteColor(color, theme.Colors, theme.Colors.Primary);

    /// <summary>
    /// Gets the current value as a percentage (0-100) between <see cref="Min"/> and <see cref="Max"/>, used to
    /// position the fill, thumb and value tooltip.
    /// </summary>
    private double percentage
    {
        get
        {
            try
            {
                var min = Convert.ToDouble(Min, CultureInfo.InvariantCulture);
                var max = Convert.ToDouble(Max, CultureInfo.InvariantCulture);
                var value = Convert.ToDouble(Value, CultureInfo.InvariantCulture);

                if (max <= min)
                    return 0;

                return Math.Clamp((value - min) / (max - min) * 100, 0, 100);
            }
            catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
            {
                return 0;
            }
        }
    }

    private string percentageText => percentage.ToString("0.###", CultureInfo.InvariantCulture);

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Only apply disabled attribute when actually disabled, not for readonly
        // Readonly behavior is handled by pointer-events-none CSS class to prevent interaction
        // This allows readonly sliders to maintain their color while disabled sliders are greyed out
        if (Disabled)
        {
            if (!Attributes.ContainsKey("disabled"))
            {
                Attributes["disabled"] = true;
            }
        }
        else
        {
            Attributes.Remove("disabled");
        }
    }

    private async Task HandleInput(ChangeEventArgs e)
    {
        if (ReadOnly || Disabled || e.Value is null)
            return;

        try
        {
            var stringValue = e.Value.ToString();
            if (!string.IsNullOrEmpty(stringValue))
            {
                var convertedValue = (T)Convert.ChangeType(stringValue, typeof(T));
                Value = convertedValue;
                await ValueChanged.InvokeAsync(Value);
            }
        }
        catch (InvalidCastException)
        {
            // If conversion fails, keep the current value
        }
        catch (FormatException)
        {
            // If format is invalid, keep the current value
        }
        catch (OverflowException)
        {
            // If value is out of range, keep the current value
        }
    }
}
