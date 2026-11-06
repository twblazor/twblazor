// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;

namespace TwBlazor.Components;

/// <summary>
/// A group of <see cref="TwMenuRadioItem{TValue}"/> options in a <see cref="TwMenu"/> or <see cref="TwContextMenu"/>,
/// of which one is chosen at a time, such as a sort order or a zoom level. Bind the chosen option with
/// <c>@bind-Value</c>.
/// </summary>
/// <typeparam name="TValue">The type of the options' values.</typeparam>
public partial class TwMenuRadioGroup<TValue> : TwBlazorComponentBase
{
    private TwMenuTheme theme => options.Theme.Components.Require<TwMenuTheme>();

    /// <summary>
    /// Gets or sets the group's heading, which assistive technology also reads as the group's name.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the value of the chosen option. Bind it with <c>@bind-Value</c>.
    /// </summary>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>
    /// Raised with the new value when a person chooses an option.
    /// </summary>
    [Parameter]
    public EventCallback<TValue?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the group's <see cref="TwMenuRadioItem{TValue}"/> options.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string labelId => $"{Id}-label";

    internal bool IsChosen(TValue? value)
    {
        // An item with no value is never the chosen one, even when the group's own value is also unset.
        if (value is null)
        {
            return false;
        }

        return EqualityComparer<TValue?>.Default.Equals(Value, value);
    }

    internal async Task ChooseAsync(TValue? value)
    {
        if (IsChosen(value))
        {
            return;
        }

        Value = value;
        await ValueChanged.InvokeAsync(value);

        // The group cascades itself without IsFixed, so re-rendering it re-renders every option with its new state.
        StateHasChanged();
    }
}
