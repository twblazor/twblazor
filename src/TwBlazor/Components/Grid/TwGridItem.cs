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
    [Parameter] public int? ColSpan { get; set; }

    /// <summary>
    /// Gets or sets how many columns the item spans from the <c>sm</c> breakpoint (40rem) upwards.
    /// </summary>
    [Parameter] public int? ColSpanSm { get; set; }

    /// <summary>
    /// Gets or sets how many columns the item spans from the <c>md</c> breakpoint (48rem) upwards.
    /// </summary>
    [Parameter] public int? ColSpanMd { get; set; }

    /// <summary>
    /// Gets or sets how many columns the item spans from the <c>lg</c> breakpoint (64rem) upwards.
    /// </summary>
    [Parameter] public int? ColSpanLg { get; set; }

    /// <summary>
    /// Gets or sets how many columns the item spans from the <c>xl</c> breakpoint (80rem) upwards.
    /// </summary>
    [Parameter] public int? ColSpanXl { get; set; }

    /// <summary>
    /// Gets or sets how many columns the item spans from the <c>2xl</c> breakpoint (96rem) upwards.
    /// </summary>
    [Parameter] public int? ColSpanXl2 { get; set; }

    /// <summary>
    /// Gets or sets how many rows the item spans at every width, from 1 to 6, or <see cref="GridSpan.Full"/> for every row.
    /// </summary>
    [Parameter] public int? RowSpan { get; set; }

    /// <summary>
    /// Gets or sets how many rows the item spans from the <c>sm</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? RowSpanSm { get; set; }

    /// <summary>
    /// Gets or sets how many rows the item spans from the <c>md</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? RowSpanMd { get; set; }

    /// <summary>
    /// Gets or sets how many rows the item spans from the <c>lg</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? RowSpanLg { get; set; }

    /// <summary>
    /// Gets or sets how many rows the item spans from the <c>xl</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? RowSpanXl { get; set; }

    /// <summary>
    /// Gets or sets how many rows the item spans from the <c>2xl</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? RowSpanXl2 { get; set; }

    /// <summary>
    /// Gets or sets the column line the item starts on at every width, from 1 to 13. Use it to leave empty
    /// columns before the item.
    /// </summary>
    [Parameter] public int? ColStart { get; set; }

    /// <summary>
    /// Gets or sets the column line the item starts on from the <c>sm</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? ColStartSm { get; set; }

    /// <summary>
    /// Gets or sets the column line the item starts on from the <c>md</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? ColStartMd { get; set; }

    /// <summary>
    /// Gets or sets the column line the item starts on from the <c>lg</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? ColStartLg { get; set; }

    /// <summary>
    /// Gets or sets the column line the item starts on from the <c>xl</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? ColStartXl { get; set; }

    /// <summary>
    /// Gets or sets the column line the item starts on from the <c>2xl</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? ColStartXl2 { get; set; }

    /// <summary>
    /// Gets or sets the row line the item starts on at every width, from 1 to 7.
    /// </summary>
    [Parameter] public int? RowStart { get; set; }

    /// <summary>
    /// Gets or sets the row line the item starts on from the <c>sm</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? RowStartSm { get; set; }

    /// <summary>
    /// Gets or sets the row line the item starts on from the <c>md</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? RowStartMd { get; set; }

    /// <summary>
    /// Gets or sets the row line the item starts on from the <c>lg</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? RowStartLg { get; set; }

    /// <summary>
    /// Gets or sets the row line the item starts on from the <c>xl</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? RowStartXl { get; set; }

    /// <summary>
    /// Gets or sets the row line the item starts on from the <c>2xl</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? RowStartXl2 { get; set; }

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
        .AddClass(ResponsiveClasses(grid.ColSpan, ColSpan, ColSpanSm, ColSpanMd, ColSpanLg, ColSpanXl, ColSpanXl2))
        .AddClass(ResponsiveClasses(grid.RowSpan, RowSpan, RowSpanSm, RowSpanMd, RowSpanLg, RowSpanXl, RowSpanXl2))
        .AddClass(ResponsiveClasses(grid.ColStart, ColStart, ColStartSm, ColStartMd, ColStartLg, ColStartXl, ColStartXl2))
        .AddClass(ResponsiveClasses(grid.RowStart, RowStart, RowStartSm, RowStartMd, RowStartLg, RowStartXl, RowStartXl2))
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
