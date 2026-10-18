// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;

namespace TwBlazor.Components;

/// <summary>
/// A labelled group of items in a <see cref="TwMenu"/>, such as the columns that can be shown or hidden. Assistive
/// technology reads the <see cref="Label"/> as the group's name.
/// </summary>
public partial class TwMenuGroup : TwBlazorComponentBase
{
    private TwMenuTheme theme => options.Theme.Components.Require<TwMenuTheme>();

    /// <summary>
    /// Gets or sets the group's heading.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the group's items.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string labelId => $"{Id}-label";
}
