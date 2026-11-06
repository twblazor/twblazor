// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// One option of a <see cref="TwMenuRadioGroup{TValue}"/>. Choosing it makes it the group's value, and by default
/// leaves the menu open, the same as a <see cref="TwMenuCheckboxItem"/>. Assistive technology announces whether it is
/// the chosen option.
/// </summary>
/// <typeparam name="TValue">The type of the option's value, the same as its group's.</typeparam>
public partial class TwMenuRadioItem<TValue> : TwBlazorComponentBase
{
    [CascadingParameter] private ITwMenuHost? menu { get; set; }

    [CascadingParameter] private TwMenuRadioGroup<TValue>? group { get; set; }

    private TwMenuTheme theme => options.Theme.Components.Require<TwMenuTheme>();

    /// <summary>
    /// Gets or sets the value the group takes when this option is chosen.
    /// </summary>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>
    /// Gets or sets the option's text. Use <see cref="ChildContent"/> instead for richer content.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the option's content, in place of <see cref="Label"/>. Keep it to text.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets a keyboard shortcut shown at the end of the option as a hint. It is only a hint: the app handles
    /// the shortcut itself.
    /// </summary>
    [Parameter]
    public string? Shortcut { get; set; }

    /// <summary>
    /// Whether the option is disabled. It is skipped by the arrow keys and cannot be chosen.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Whether choosing the option closes the menu. Defaults to <c>false</c>.
    /// </summary>
    [Parameter]
    public bool CloseOnClick { get; set; }

    private bool isChecked => Value is not null && (group?.IsChosen(Value) ?? false);

    private string itemClasses => new ClassBuilder(theme.Item)
        .AddClass(theme.ItemDisabled, Disabled)
        .AddClass(Class).Build();

    private string indicatorClasses => new ClassBuilder(theme.CheckIcon)
        .AddClass(theme.CheckIconHidden, !isChecked).Build();

    private async Task ChooseAsync()
    {
        if (Disabled)
        {
            return;
        }

        if (group is not null)
        {
            await group.ChooseAsync(Value);
        }

        if (CloseOnClick && menu is not null)
        {
            await menu.CloseMenuAsync(returnFocus: true);
        }
    }
}
