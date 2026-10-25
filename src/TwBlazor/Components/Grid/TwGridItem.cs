// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// A child of a <see cref="TwGrid"/> that sets where it sits: how many columns and rows it spans and where it
/// starts (both changing at any breakpoint), a named area, its alignment inside its cell and its visual order.
/// </summary>
/// <remarks>
/// Any element can be a child of a <see cref="TwGrid"/>; use <see cref="TwGridItem"/> only when a child needs one of
/// these settings. Placing an item away from its place in the markup changes only where it is drawn, so screen
/// readers and the Tab key still follow the markup.
/// </remarks>
public class TwGridItem : TwLayoutComponentBase
{
    private TwGridTheme theme => options.Theme.Components.Require<TwGridTheme>();

    /// <summary>
    /// Gets or sets how many columns the item spans at every width, from 1 to 12, or <see cref="GridSpan.Full"/>
    /// for every column.
    /// </summary>
    [Parameter] public int? ColumnSpan { get; set; }

    /// <summary>
    /// Gets or sets how many columns the item spans from the <c>sm</c> breakpoint (40rem) upwards.
    /// </summary>
    [Parameter] public int? SmColumnSpan { get; set; }

    /// <summary>
    /// Gets or sets how many columns the item spans from the <c>md</c> breakpoint (48rem) upwards.
    /// </summary>
    [Parameter] public int? MdColumnSpan { get; set; }

    /// <summary>
    /// Gets or sets how many columns the item spans from the <c>lg</c> breakpoint (64rem) upwards.
    /// </summary>
    [Parameter] public int? LgColumnSpan { get; set; }

    /// <summary>
    /// Gets or sets how many columns the item spans from the <c>xl</c> breakpoint (80rem) upwards.
    /// </summary>
    [Parameter] public int? XlColumnSpan { get; set; }

    /// <summary>
    /// Gets or sets how many columns the item spans from the <c>2xl</c> breakpoint (96rem) upwards.
    /// </summary>
    [Parameter] public int? Xl2ColumnSpan { get; set; }

    /// <summary>
    /// Gets or sets how many rows the item spans at every width, from 1 to 6, or <see cref="GridSpan.Full"/> for every row.
    /// </summary>
    [Parameter] public int? RowSpan { get; set; }

    /// <summary>
    /// Gets or sets how many rows the item spans from the <c>sm</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? SmRowSpan { get; set; }

    /// <summary>
    /// Gets or sets how many rows the item spans from the <c>md</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? MdRowSpan { get; set; }

    /// <summary>
    /// Gets or sets how many rows the item spans from the <c>lg</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? LgRowSpan { get; set; }

    /// <summary>
    /// Gets or sets how many rows the item spans from the <c>xl</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? XlRowSpan { get; set; }

    /// <summary>
    /// Gets or sets how many rows the item spans from the <c>2xl</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? Xl2RowSpan { get; set; }

    /// <summary>
    /// Gets or sets the column line the item starts on at every width, from 1 to 13. Use it to leave empty
    /// columns before the item.
    /// </summary>
    [Parameter] public int? ColumnStart { get; set; }

    /// <summary>
    /// Gets or sets the column line the item starts on from the <c>sm</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? SmColumnStart { get; set; }

    /// <summary>
    /// Gets or sets the column line the item starts on from the <c>md</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? MdColumnStart { get; set; }

    /// <summary>
    /// Gets or sets the column line the item starts on from the <c>lg</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? LgColumnStart { get; set; }

    /// <summary>
    /// Gets or sets the column line the item starts on from the <c>xl</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? XlColumnStart { get; set; }

    /// <summary>
    /// Gets or sets the column line the item starts on from the <c>2xl</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? Xl2ColumnStart { get; set; }

    /// <summary>
    /// Gets or sets the row line the item starts on at every width, from 1 to 7.
    /// </summary>
    [Parameter] public int? RowStart { get; set; }

    /// <summary>
    /// Gets or sets the row line the item starts on from the <c>sm</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? SmRowStart { get; set; }

    /// <summary>
    /// Gets or sets the row line the item starts on from the <c>md</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? MdRowStart { get; set; }

    /// <summary>
    /// Gets or sets the row line the item starts on from the <c>lg</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? LgRowStart { get; set; }

    /// <summary>
    /// Gets or sets the row line the item starts on from the <c>xl</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? XlRowStart { get; set; }

    /// <summary>
    /// Gets or sets the row line the item starts on from the <c>2xl</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? Xl2RowStart { get; set; }

    /// <summary>
    /// Gets or sets the named area the item fills, one of the names in its grid's <see cref="TwGrid.Areas"/>.
    /// </summary>
    [Parameter] public string? Area { get; set; }

    /// <summary>
    /// Gets or sets the item's alignment inside its cell on the row axis, overriding the grid's.
    /// </summary>
    [Parameter] public JustifySelf? JustifySelf { get; set; }

    /// <summary>
    /// Gets or sets the item's alignment inside its cell on the column axis, overriding the grid's.
    /// </summary>
    [Parameter] public AlignSelf? AlignSelf { get; set; }

    /// <summary>
    /// Gets or sets where the item is drawn among items that have no position, from 1 to 12, or
    /// <see cref="ItemOrder.First"/> or <see cref="ItemOrder.Last"/>.
    /// </summary>
    [Parameter] public int? Order { get; set; }

    /// <inheritdoc />
    protected override string rootClasses => new ClassBuilder(theme.Item)
        .AddClass(ResponsiveClasses(grid.ColumnSpan, ColumnSpan, SmColumnSpan, MdColumnSpan, LgColumnSpan, XlColumnSpan, Xl2ColumnSpan))
        .AddClass(ResponsiveClasses(grid.RowSpan, RowSpan, SmRowSpan, MdRowSpan, LgRowSpan, XlRowSpan, Xl2RowSpan))
        .AddClass(ResponsiveClasses(grid.ColumnStart, ColumnStart, SmColumnStart, MdColumnStart, LgColumnStart, XlColumnStart, Xl2ColumnStart))
        .AddClass(ResponsiveClasses(grid.RowStart, RowStart, SmRowStart, MdRowStart, LgRowStart, XlRowStart, Xl2RowStart))
        .AddClass(justifySelfClass)
        .AddClass(AlignSelfClass(AlignSelf))
        .AddClass(OrderClass(Order))
        .AddClass(decorationClasses)
        .AddClass(Class)
        .Build();

    /// <inheritdoc />
    protected override string? rootStyle => JoinStyles(
        string.IsNullOrWhiteSpace(Area) ? null : $"grid-area: {CssValue(Area)}",
        Style);

    private string justifySelfClass => JustifySelf switch
    {
        Enums.JustifySelf.Start => grid.JustifySelf.Start,
        Enums.JustifySelf.Center => grid.JustifySelf.Center,
        Enums.JustifySelf.End => grid.JustifySelf.End,
        Enums.JustifySelf.Stretch => grid.JustifySelf.Stretch,
        _ => string.Empty
    };
}
