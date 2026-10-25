// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for <see cref="TwBlazor.Components.TwFlex"/> and <see cref="TwBlazor.Components.TwFlexItem"/>.
/// Override any property to customize flex layout styles globally.
/// </summary>
/// <remarks>
/// Direction, wrapping, alignment, basis and order come from the shared <see cref="TwBlazorFlexbox"/> utilities,
/// and the numeric gaps from <see cref="TwBlazorSpacing"/>.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwFlexTheme
{
    /// <summary>
    /// Gets or sets the classes for a block-level flex layout.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the classes for an inline flex layout, which sits in a line of text.
    /// </summary>
    public required string Inline { get; set; }

    /// <summary>
    /// Gets or sets the classes that make the layout fill its parent's width.
    /// </summary>
    public required string FullWidth { get; set; }

    /// <summary>
    /// Gets or sets the classes that make the layout fill its parent's height.
    /// </summary>
    public required string FullHeight { get; set; }

    /// <summary>
    /// Gets or sets the classes that draw a line between children laid out in a row.
    /// </summary>
    public required string DivideX { get; set; }

    /// <summary>
    /// Gets or sets the classes that draw a line between children stacked in a column.
    /// </summary>
    public required string DivideY { get; set; }

    /// <summary>
    /// Gets or sets the classes added to <see cref="DivideX"/> when the row is reversed, so each line stays between two children.
    /// </summary>
    public required string DivideXReverse { get; set; }

    /// <summary>
    /// Gets or sets the classes added to <see cref="DivideY"/> when the column is reversed, so each line stays between two children.
    /// </summary>
    public required string DivideYReverse { get; set; }

    /// <summary>
    /// Gets or sets the color of the lines drawn between children.
    /// </summary>
    public required string DivideColor { get; set; }

    /// <summary>
    /// Gets or sets the classes every <see cref="TwBlazor.Components.TwFlexItem"/> has.
    /// </summary>
    public required string Item { get; set; }

    /// <summary>
    /// Gets or sets the classes that let an item shrink below its content, so long text inside it can truncate.
    /// </summary>
    public required string ItemMinWidthZero { get; set; }

    /// <summary>
    /// Gets or sets the classes that push an item, and everything after it, to the end of a row.
    /// </summary>
    public required string ItemPushEnd { get; set; }

    /// <summary>
    /// Gets or sets the classes that push an item, and everything after it, to the bottom of a column.
    /// </summary>
    public required string ItemPushEndColumn { get; set; }
}
