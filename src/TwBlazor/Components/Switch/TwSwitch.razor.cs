// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a toggle switch component that allows users to select between two states.
/// </summary>
/// <remarks>Supports two-way data binding through the Value and ValueChanged parameters, enabling parent
/// components to react to changes in the switch's state. The component can be customized with a name and color, and is
/// typically used in forms or settings panels where a binary choice is required.</remarks>
public partial class TwSwitch<T> : TwBlazorInputComponentBase
{
    private TwSwitchTheme theme => options.Theme.Components.Require<TwSwitchTheme>();

    /// <summary>
    /// Gets or sets the name associated with the component.
    /// </summary>
    [Parameter] public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the color of the switch.
    /// </summary>
    [Parameter] public Color? Color { get; set; }

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

    private string wrapperClasses =>
        new ClassBuilder(options.Theme.Position.Relative)
        .AddClass(options.Theme.Display.InlineBlock)
        .AddClass(theme.WrapperSize).Build();

    private string classes =>
        new ClassBuilder(theme.Base)
        .AddClass(Class).Build();

    private string labelClasses =>
        new ClassBuilder(theme.LabelBase)
        .AddClass(ReadOnly || Disabled ? theme.LabelNonInteractiveCursor : theme.LabelInteractiveCursor)
        .AddClass(Disabled ? theme.LabelDisabled : string.Empty)
        .AddClass(LabelClass).Build();

    private string trackClasses =>
        new ClassBuilder(theme.Track)
        .AddClass(GetSwitchColor(Color))
        .AddClass(colorBuilder.GetPeerFocusRing(Color))
        .Build();

    private string GetSwitchColor(Color? color) => ColorBuilder.GetPaletteColor(color, theme.Colors, theme.Colors.Primary);


    private bool isChecked => Value is bool boolValue && boolValue;

    private async Task HandleChange(ChangeEventArgs e)
    {
        if (ReadOnly || Disabled)
            return;

        if (e.Value is bool newValue)
        {
            Value = (T)(object)newValue;
            await ValueChanged.InvokeAsync(Value);
        }
    }
}