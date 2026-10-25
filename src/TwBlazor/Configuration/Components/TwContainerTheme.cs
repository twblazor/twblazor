// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Enums;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for <see cref="TwBlazor.Components.TwContainer"/>.
/// Override any property to customize container styles globally.
/// </summary>
/// <remarks>
/// The maximum widths and the numeric padding steps come from the shared
/// <see cref="TwBlazorSizing.MaxWidth"/> and <see cref="TwBlazorSpacing.Steps"/> scales.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwContainerTheme
{
    /// <summary>
    /// Gets or sets the classes every container has, such as its full width.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the classes that center the container in its parent.
    /// </summary>
    public required string Centered { get; set; }

    /// <summary>
    /// Gets or sets the start and end padding used when no <c>Gutter</c> is set, typically growing with the screen.
    /// </summary>
    public required string Gutter { get; set; }

    /// <summary>
    /// Gets or sets the maximum width used when no <c>MaxWidth</c> is set.
    /// </summary>
    public ContainerSize DefaultMaxWidth { get; set; } = ContainerSize.Xl7;

    /// <summary>
    /// Gets or sets the class that makes the container a query container, so its children can respond to its
    /// width with Tailwind's <c>@sm:</c>, <c>@md:</c> and similar prefixes.
    /// </summary>
    public required string QueryContainer { get; set; }

    /// <summary>
    /// Gets or sets the classes for <see cref="ContainerSurface.Default"/>.
    /// </summary>
    public required string SurfaceDefault { get; set; }

    /// <summary>
    /// Gets or sets the classes for <see cref="ContainerSurface.Subtle"/>.
    /// </summary>
    public required string SurfaceSubtle { get; set; }

    /// <summary>
    /// Gets or sets the classes for <see cref="ContainerSurface.Elevated"/>.
    /// </summary>
    public required string SurfaceElevated { get; set; }
}
