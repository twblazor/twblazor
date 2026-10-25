// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Centers content in a column with a maximum width and side padding, the outer frame of a page or of a band
/// across it.
/// </summary>
/// <remarks>
/// By default the container is centered, as wide as <see cref="TwContainerTheme.DefaultMaxWidth"/> allows, and
/// padded by <see cref="TwContainerTheme.Gutter"/>. Set <see cref="TwLayoutComponentBase.Element"/> to render a
/// landmark such as <c>main</c> or a named <c>section</c>.
/// </remarks>
public class TwContainer : TwLayoutComponentBase
{
    private TwContainerTheme theme => options.Theme.Components.Require<TwContainerTheme>();

    /// <summary>
    /// Gets or sets the widest the container grows. Defaults to the theme's <see cref="TwContainerTheme.DefaultMaxWidth"/>.
    /// </summary>
    [Parameter] public ContainerSize? MaxWidth { get; set; }

    /// <summary>
    /// Gets or sets whether the container is centered in its parent. Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter] public bool Centered { get; set; } = true;

    /// <summary>
    /// Gets or sets the start and end padding as a step on Tailwind's spacing scale, such as <c>6</c> for
    /// <c>px-6</c>. Leave it unset for the theme's <see cref="TwContainerTheme.Gutter"/>, which grows with the screen.
    /// </summary>
    [Parameter] public double? Gutter { get; set; }

    /// <summary>
    /// Gets or sets the top and bottom padding as a step on Tailwind's spacing scale, such as <c>12</c> for <c>py-12</c>.
    /// </summary>
    [Parameter] public double? PaddingY { get; set; }

    /// <summary>
    /// Gets or sets the top padding as a step on Tailwind's spacing scale, such as <c>14</c> for <c>pt-14</c>.
    /// It takes the place of <see cref="PaddingY"/> at the top.
    /// </summary>
    [Parameter] public double? PaddingTop { get; set; }

    /// <summary>
    /// Gets or sets the bottom padding as a step on Tailwind's spacing scale, such as <c>12</c> for <c>pb-12</c>.
    /// It takes the place of <see cref="PaddingY"/> at the bottom.
    /// </summary>
    [Parameter] public double? PaddingBottom { get; set; }

    /// <summary>
    /// Gets or sets the background painted behind the content. Defaults to <see cref="ContainerSurface.None"/>.
    /// </summary>
    [Parameter] public ContainerSurface Surface { get; set; } = ContainerSurface.None;

    /// <summary>
    /// Gets or sets whether the container is a query container, so its children can respond to its width with
    /// Tailwind's <c>@sm:</c>, <c>@md:</c> and similar prefixes instead of the width of the screen.
    /// </summary>
    [Parameter] public bool QueryContainer { get; set; }

    /// <inheritdoc />
    protected override string rootClasses => new ClassBuilder(theme.Base)
        .AddClass(theme.Centered, Centered)
        .AddClass(options.Theme.Sizing.MaxWidth.Get(MaxWidth ?? theme.DefaultMaxWidth))
        .AddClass(Gutter is null ? theme.Gutter : steps.PaddingX.Get(Gutter))
        .AddClass(steps.PaddingY.Get(PaddingY))
        .AddClass(steps.PaddingTop.Get(PaddingTop))
        .AddClass(steps.PaddingBottom.Get(PaddingBottom))
        .AddClass(surfaceClasses)
        .AddClass(theme.QueryContainer, QueryContainer)
        .AddClass(decorationClasses)
        .AddClass(Class)
        .Build();

    private string surfaceClasses => Surface switch
    {
        ContainerSurface.Default => theme.SurfaceDefault,
        ContainerSurface.Subtle => theme.SurfaceSubtle,
        ContainerSurface.Elevated => theme.SurfaceElevated,
        _ => string.Empty
    };
}
