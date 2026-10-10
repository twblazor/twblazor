// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Models;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a container component for grouping multiple <see cref="TwCheckbox{T}"/> components together.
/// </summary>
/// <typeparam name="TValue">The type of values in the checkbox group when using Items binding.</typeparam>
/// <remarks>
/// TwCheckboxGroup provides two ways to use checkbox groups:
/// 1. Manual: Add TwCheckbox components as children (ChildContent)
/// 2. Automatic: Provide Items and bind to SelectedValues for automatic checkbox generation
/// The component supports cascading the Name property to child checkboxes.
/// </remarks>
public partial class TwCheckboxGroup<TValue> : TwBlazorComponentBase
{
    private TwGroupsTheme groupsTheme => options.Theme.Components.Require<TwGroupsTheme>();
    private TwInputTheme inputTheme => options.Theme.Components.Require<TwInputTheme>();

    /// <summary>
    /// Gets or sets the content to display within the checkbox group.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the legend text for the checkbox group.
    /// </summary>
    [Parameter] public string? Legend { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for the legend element.
    /// </summary>
    [Parameter] public string? LegendClass { get; set; }

    /// <summary>
    /// Gets or sets the orientation of the checkbox group.
    /// </summary>
    /// <remarks>
    /// Default is vertical. Set to true for horizontal orientation.
    /// </remarks>
    [Parameter] public bool Horizontal { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the entire checkbox group is disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the name for all checkboxes in the group.
    /// </summary>
    /// <remarks>
    /// This value is cascaded to child checkboxes unless they specify their own name.
    /// </remarks>
    [Parameter] public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the collection of items to display as checkboxes (for automatic mode).
    /// </summary>
    /// <remarks>
    /// When provided, checkboxes are automatically generated from the items.
    /// If not provided, use ChildContent to manually add checkboxes.
    /// </remarks>
    [Parameter] public IEnumerable<CheckboxGroupItem<TValue>>? Items { get; set; }

    /// <summary>
    /// Gets or sets the collection of selected values (for automatic mode with Items).
    /// </summary>
    [Parameter] public IEnumerable<TValue>? SelectedValues { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the selected values change (for automatic mode with Items).
    /// </summary>
    [Parameter] public EventCallback<IEnumerable<TValue>> SelectedValuesChanged { get; set; }

    /// <summary>
    /// Gets or sets the color for automatically generated checkboxes.
    /// </summary>
    [Parameter] public Color? ItemColor { get; set; }

    /// <summary>
    /// Gets or sets whether the group as a whole is in an error state.
    /// </summary>
    [Parameter] public bool Invalid { get; set; }

    /// <summary>
    /// Gets or sets the error message for the group. It is shown under the options when
    /// <see cref="Invalid"/> is set, announced when it appears, and read as the group's description.
    /// </summary>
    [Parameter] public string ErrorMessage { get; set; } = string.Empty;

    private string? errorId => Invalid && !string.IsNullOrWhiteSpace(ErrorMessage) ? $"{Id}-error" : null;


    private string classes =>
        new ClassBuilder(groupsTheme.FieldsetBase)
        .AddClass(Horizontal ? groupsTheme.HorizontalLayout : groupsTheme.VerticalLayout)
        .AddClass(Disabled ? groupsTheme.CheckboxGroupDisabled : string.Empty)
        .AddClass(groupsTheme.Gap)
        .AddClass(Class)
        .Build();

    private string legendClasses =>
        new ClassBuilder(inputTheme.InputLegendBase)
        .AddClass(LegendClass ?? string.Empty)
        .Build();

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Sync IsSelected state with SelectedValues
        if (Items != null && SelectedValues != null)
        {
            HashSet<TValue> selectedSet = [.. SelectedValues];
            foreach (var item in Items)
            {
                item.IsSelected = selectedSet.Contains(item.Value!);
            }
        }
    }

    private async Task HandleItemChanged(CheckboxGroupItem<TValue> item, bool isSelected)
    {
        item.IsSelected = isSelected;

        if (Items != null)
        {
            var newSelectedValues = Items
                .Where(i => i.IsSelected)
                .Select(i => i.Value)
                .ToList();

            await SelectedValuesChanged.InvokeAsync(newSelectedValues);
        }
    }
}
