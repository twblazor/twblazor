// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for <see cref="TwBlazor.Components.TwGrid"/> and <see cref="TwBlazor.Components.TwGridItem"/>.
/// Override any property to customize grid layout styles globally.
/// </summary>
/// <remarks>
/// Column and row counts, spans, start lines, auto flow and cell alignment come from the shared
/// <see cref="TwBlazorGrid"/> utilities, and the numeric gaps from <see cref="TwBlazorSpacing"/>.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwGridTheme
{
    /// <summary>
    /// Gets or sets the classes for a block-level grid.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the classes for an inline grid, which sits in a line of text.
    /// </summary>
    public required string Inline { get; set; }

    /// <summary>
    /// Gets or sets the columns used with <c>MinColWidth</c>: as many columns as fit, stretched to share the
    /// row. The minimum width is read from the <c>--tw-grid-min</c> custom property.
    /// </summary>
    public required string AutoFit { get; set; }

    /// <summary>
    /// Gets or sets the columns used with <c>MinColWidth</c> and <c>AutoFill</c>: as many columns as fit,
    /// keeping empty tracks so a short last row does not stretch. The minimum width is read from the
    /// <c>--tw-grid-min</c> custom property.
    /// </summary>
    public required string AutoFill { get; set; }

    /// <summary>
    /// Gets or sets the classes every <see cref="TwBlazor.Components.TwGridItem"/> has. By default it lets an item
    /// shrink below its content, so a long word or a wide child cannot stretch its column.
    /// </summary>
    public required string Item { get; set; }
}
