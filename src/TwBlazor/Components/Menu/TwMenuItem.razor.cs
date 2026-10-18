// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// An action in a <see cref="TwMenu"/>. Choosing it runs <see cref="OnClick"/> and closes the menu.
/// </summary>
public partial class TwMenuItem : TwBlazorComponentBase
{
    [CascadingParameter] private TwMenu? menu { get; set; }

    private TwMenuTheme theme => options.Theme.Components.Require<TwMenuTheme>();

    /// <summary>
    /// Gets or sets the item's text. Use <see cref="ChildContent"/> instead for richer content.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the item's content, in place of <see cref="Label"/>. Keep it to text: an item is a single control,
    /// so it cannot hold links or buttons of its own.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets an icon shown before the item's text.
    /// </summary>
    [Parameter]
    public Icon? StartIcon { get; set; }

    /// <summary>
    /// Raised when a person chooses the item, by clicking it or pressing Enter or Space on it.
    /// </summary>
    [Parameter]
    public EventCallback OnClick { get; set; }

    /// <summary>
    /// Whether the item is disabled. It is skipped by the arrow keys and cannot be chosen.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Whether choosing the item closes the menu and returns focus to its button. Defaults to <c>true</c>.
    /// </summary>
    [Parameter]
    public bool CloseOnClick { get; set; } = true;

    private string itemClasses => new ClassBuilder(theme.Item)
        .AddClass(theme.ItemDisabled, Disabled)
        .AddClass(Class).Build();

    private async Task ActivateAsync()
    {
        if (Disabled)
        {
            return;
        }

        await OnClick.InvokeAsync();

        if (CloseOnClick && menu is not null)
        {
            await menu.CloseAsync(returnFocus: true);
        }
    }
}
