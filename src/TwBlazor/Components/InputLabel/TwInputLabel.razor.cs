// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

public partial class TwInputLabel : TwBlazorComponentBase
{
    private TwInputTheme theme => options.Theme.Components.Require<TwInputTheme>();

    /// <summary>
    /// Gets or sets the identifier for the target element associated with this component.
    /// </summary>
    /// <remarks>This property is typically used to specify the context or purpose of the component, allowing
    /// for better integration with other elements or components in the application.</remarks>
    [Parameter] public string For { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the label text displayed for the associated input element.
    /// </summary>
    /// <remarks>This property allows customization of the label shown to users, enhancing accessibility and
    /// usability. The default value is an empty string, which means no label will be displayed unless explicitly
    /// set.</remarks>
    [Parameter] public string Label { get; set; } = string.Empty;
    /// <summary>
    /// Renders instead of <see cref="Label"/> if not null and inside of the label html element.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Allows you to completely override the input label theme classes.
    /// </summary>
    [Parameter] public string OverrideClass { get; set; } = string.Empty;

    private string classes =>
        new ClassBuilder(string.IsNullOrWhiteSpace(OverrideClass) ? theme.LabelBase : OverrideClass)
        .AddClass(Class)
        .Build();
}
