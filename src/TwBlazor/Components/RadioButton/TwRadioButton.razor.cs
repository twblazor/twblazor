// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Builders;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a generic radio button input component for Blazor that supports two-way data binding and customizable value
/// types.
/// </summary>
/// <remarks>Use <see cref="TwRadioButton{T}"/> to create radio button inputs that bind to values of type <typeparamref
/// name="T"/>. The component supports two-way binding via the <see cref="SelectedValue"/> and <see cref="SelectedValueChanged"/>
/// parameters. Radio buttons with the same <see cref="Name"/> property form a radio button group where only one can be selected at 
/// a time. A value of the checkbox can be set using <see cref="Value"/>.</remarks>
/// <typeparam name="T">The type of value associated with the radio button. Determines the data type used for binding and value changes.</typeparam>
public partial class TwRadioButton<T> : TwBlazorInputComponentBase
{
    private TwRadioButtonTheme theme => options.Theme.Components.Require<TwRadioButtonTheme>();

    /// <summary>
    /// Gets or sets the name associated with the radio button group.
    /// </summary>
    /// <remarks>Radio buttons with the same name form a mutually exclusive group.</remarks>
    [Parameter] public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name from a parent RadioGroup (cascaded value).
    /// </summary>
    [CascadingParameter(Name = "GroupName")] public string? GroupName { get; set; }

    /// <summary>
    /// Gets the effective name to use, preferring the explicit Name parameter over the cascaded GroupName.
    /// </summary>
    private string effectiveName => !string.IsNullOrWhiteSpace(Name) ? Name : GroupName ?? string.Empty;

    /// <summary>
    /// Gets or sets the color of the radio button.
    /// </summary>
    [Parameter] public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets the current value of the parameter.
    /// </summary>
    [Parameter] public T Value { get; set; } = default!;

    /// <summary>
    /// Gets or sets the selected value for the radio button group.
    /// </summary>
    /// <remarks>When this value matches the radio button's <see cref="Value"/>, the radio button is checked.</remarks>
    [Parameter] public T? SelectedValue { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when the selected value changes.
    /// </summary>
    /// <remarks>Use this property to handle value change events in two-way data binding scenarios. The
    /// callback is triggered when the component's value is updated, allowing parent components to respond to the
    /// change.</remarks>
    [Parameter] public EventCallback<T> SelectedValueChanged { get; set; } = default!;

    private string classes =>
        new ClassBuilder(theme.Base)
        .AddClass(GetRadioButtonColor(Color))
        .AddClass(Disabled ? theme.Disabled : theme.Hover)
        .AddClass(Class).Build();

    private string labelClasses =>
        new ClassBuilder(theme.LabelBase)
        .AddClass(ReadOnly || Disabled ? theme.LabelNonInteractiveCursor : theme.LabelInteractiveCursor)
        .AddClass(Disabled ? theme.LabelDisabled : string.Empty)
        .AddClass(LabelClass).Build();

    private string radioClasses =>
        new ClassBuilder(theme.IconWrapper)
        .AddClass(options.Theme.Colors.TextColors.Dark.Dark, Color == Enums.Color.Light)
        .AddClass(options.Theme.Colors.TextColors.Medium.Light, Color != Enums.Color.Light)
        .Build();

    private bool isChecked => EqualityComparer<T>.Default.Equals(Value, SelectedValue);

    private string GetRadioButtonColor(Color? color) => ColorBuilder.GetPaletteColor(color, theme.Colors, theme.Colors.Primary);

    private async Task HandleChange(ChangeEventArgs e)
    {
        if (ReadOnly || Disabled)
            return;

        await SelectedValueChanged.InvokeAsync(Value);
    }
}
