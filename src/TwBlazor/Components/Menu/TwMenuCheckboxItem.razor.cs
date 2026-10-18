// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// An option in a <see cref="TwMenu"/> that is on or off, such as whether a column is shown. Choosing it flips
/// <see cref="Checked"/>, and by default leaves the menu open so several options can be changed in a row.
/// </summary>
public partial class TwMenuCheckboxItem : TwBlazorComponentBase
{
    [CascadingParameter] private TwMenu? menu { get; set; }

    private TwMenuTheme theme => options.Theme.Components.Require<TwMenuTheme>();

    /// <summary>
    /// Gets or sets the item's text. Use <see cref="ChildContent"/> instead for richer content.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the item's content, in place of <see cref="Label"/>. Keep it to text.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets whether the option is on. Bind it with <c>@bind-Checked</c>.
    /// </summary>
    [Parameter]
    public bool Checked { get; set; }

    /// <summary>
    /// Raised with the new value when a person chooses the item.
    /// </summary>
    [Parameter]
    public EventCallback<bool> CheckedChanged { get; set; }

    /// <summary>
    /// Whether the item is disabled. It is skipped by the arrow keys and cannot be changed.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Whether choosing the item closes the menu. Defaults to <c>false</c>.
    /// </summary>
    [Parameter]
    public bool CloseOnClick { get; set; }

    private string itemClasses => new ClassBuilder(theme.Item)
        .AddClass(theme.ItemDisabled, Disabled)
        .AddClass(Class).Build();

    private string checkClasses => new ClassBuilder(theme.CheckIcon)
        .AddClass(theme.CheckIconHidden, !Checked).Build();

    private async Task ToggleAsync()
    {
        if (Disabled)
        {
            return;
        }

        Checked = !Checked;
        await CheckedChanged.InvokeAsync(Checked);

        if (CloseOnClick && menu is not null)
        {
            await menu.CloseAsync(returnFocus: true);
        }
    }
}
