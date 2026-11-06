// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// An item in a <see cref="TwMenu"/> or <see cref="TwContextMenu"/> that opens a further menu beside it, such as
/// "Share" opening a list of places to share to. Put the same items in it as in a menu, including further submenus.
/// </summary>
/// <remarks>The item follows the ARIA menu pattern for submenus: it says that it opens a menu and whether that menu is
/// open. Hovering the item, clicking it, or pressing Enter, Space or Arrow Right on it opens the submenu; Arrow Right
/// and Enter also move focus to its first item. Arrow Left or Escape close the submenu and return focus to the item.
/// Arrow Right and Arrow Left swap in a right to left layout, where the submenu opens on the left. Choosing an action in
/// the submenu closes the whole menu.</remarks>
public partial class TwSubmenu : TwBlazorComponentBase
{
    private TwMenuTheme theme => options.Theme.Components.Require<TwMenuTheme>();

    /// <summary>
    /// Gets or sets the text of the item that opens the submenu. Also names the submenu for assistive technology.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the content of the item that opens the submenu, in place of <see cref="Label"/>. Keep it to text.
    /// </summary>
    [Parameter]
    public RenderFragment? LabelContent { get; set; }

    /// <summary>
    /// Gets or sets an icon shown before the item's text.
    /// </summary>
    [Parameter]
    public Icon? StartIcon { get; set; }

    /// <summary>
    /// Gets or sets the submenu's items.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Whether the item is disabled. It is skipped by the arrow keys and its submenu cannot open.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Whether the item's text is indented to line up with items that have an icon or a tick.
    /// </summary>
    [Parameter]
    public bool Inset { get; set; }

    private string panelId => $"{Id}-menu";

    private string triggerClasses => new ClassBuilder(theme.Item)
        .AddClass(theme.SubmenuTrigger)
        .AddClass(theme.ItemInset, Inset && StartIcon is null)
        .AddClass(theme.ItemDisabled, Disabled)
        .AddClass(Class).Build();
}
