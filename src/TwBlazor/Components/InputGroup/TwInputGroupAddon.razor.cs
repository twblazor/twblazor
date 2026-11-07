// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Holds text, icons or buttons beside the input of a <see cref="TwInputGroup"/>.
/// </summary>
/// <remarks>
/// Put static text in a <see cref="TwInputGroupText"/>, so it is read with the input, and actions in a
/// <see cref="TwInputGroupButton"/>, so they fit the field. A decorative <see cref="TwIcon"/> is hidden from screen
/// readers unless it has an <c>AriaLabel</c>.
/// </remarks>
public partial class TwInputGroupAddon : TwBlazorComponentBase
{
    private TwInputGroupTheme theme => options.Theme.Components.Require<TwInputGroupTheme>();

    /// <summary>
    /// Gets or sets the content of the addon, such as a <see cref="TwInputGroupText"/>, a <see cref="TwIcon"/> or a
    /// <see cref="TwInputGroupButton"/>.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets where the addon sits in the group. Defaults to <see cref="InputGroupAlign.InlineStart"/>.
    /// </summary>
    /// <remarks>
    /// The addon is placed here whatever its position in the markup. Addons with the same alignment keep their order.
    /// </remarks>
    [Parameter] public InputGroupAlign Align { get; set; } = InputGroupAlign.InlineStart;

    private string alignValue => Align switch
    {
        InputGroupAlign.InlineEnd => "inline-end",
        InputGroupAlign.BlockStart => "block-start",
        InputGroupAlign.BlockEnd => "block-end",
        _ => "inline-start"
    };

    private string alignClasses => Align switch
    {
        InputGroupAlign.InlineEnd => theme.AddonInlineEnd,
        InputGroupAlign.BlockStart => theme.AddonBlockStart,
        InputGroupAlign.BlockEnd => theme.AddonBlockEnd,
        _ => theme.AddonInlineStart
    };

    private string classes => new ClassBuilder(theme.Addon).AddClass(alignClasses).AddClass(Class).Build();
}
