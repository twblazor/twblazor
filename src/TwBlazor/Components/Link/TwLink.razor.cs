// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

public partial class TwLink : TwBlazorComponentBase
{
    private TwLinkTheme theme => options.Theme.Components.Require<TwLinkTheme>();

    /// <summary>
    /// Gets or sets the URL that the hyperlink points to.
    /// </summary>
    [Parameter] public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the target frame or window for the navigation request.
    /// </summary>
    /// <remarks>Set this property to specify where the linked content will be displayed. Common values
    /// include "_blank" to open in a new window or tab, "_self" to open in the same frame, and "_parent" or "_top" for
    /// parent or top-level frames. If not set, the default behaviour depends on the browser and context.</remarks>
    [Parameter] public string? Target { get; set; }

    /// <summary>
    /// Gets or sets the color of the link.
    /// </summary>
    /// <remarks>
    /// When not specified, defaults to blue. Can be customized via theme configuration.
    /// </remarks>
    [Parameter] public Color? Color { get; set; }

    /// <summary>
    /// The child content of the link element.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the relationship between the linked resource and the current document.
    /// </summary>
    /// <remarks>
    /// When not set and <see cref="Target"/> is "_blank", defaults to "noopener" so a new-tab link
    /// can't access <c>window.opener</c> on the page it opened. This property overrides that default.
    /// </remarks>
    [Parameter] public string? Rel { get; set; }

    private string? effectiveRel => Rel ?? (Target == "_blank" ? "noopener" : null);

    private string? classes => string.IsNullOrEmpty(Class)
        ? new ClassBuilder()
            .AddClass(colorBuilder.GetTextColor(Color ?? Enums.Color.Primary))
            .AddClass(theme.Default)
            .Build()
        : new ClassBuilder(colorBuilder.GetFocusRing(Color))
            .AddClass(options.Theme.Interaction.FocusOutlineNone)
            .AddClass(Class)
            .Build();
}