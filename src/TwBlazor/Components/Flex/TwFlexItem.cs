// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// A child of a <see cref="TwFlex"/> that controls how it grows, shrinks and aligns: flex size, basis, its own
/// cross-axis alignment, visual order and whether it pushes the items after it to the end.
/// </summary>
/// <remarks>
/// Any element can be a child of a <see cref="TwFlex"/>; use <see cref="TwFlexItem"/> only when a child needs one of
/// these settings.
/// </remarks>
public class TwFlexItem : TwLayoutComponentBase
{
    private TwFlexTheme theme => options.Theme.Components.Require<TwFlexTheme>();

    [CascadingParameter] private TwFlex? parentFlex { get; set; }

    /// <summary>
    /// Gets or sets how the item grows and shrinks, such as <see cref="FlexSize.Fill"/> to take an equal share of
    /// the free space.
    /// </summary>
    [Parameter] public FlexSize? Flex { get; set; }

    /// <summary>
    /// Gets or sets whether the item may grow into free space. Leave it unset to keep the browser's choice.
    /// </summary>
    [Parameter] public bool? Grow { get; set; }

    /// <summary>
    /// Gets or sets whether the item may shrink when space runs out. Leave it unset to keep the browser's choice.
    /// </summary>
    [Parameter] public bool? Shrink { get; set; }

    /// <summary>
    /// Gets or sets the item's starting size along the main axis, before free space is shared out.
    /// </summary>
    [Parameter] public FlexBasis? Basis { get; set; }

    /// <summary>
    /// Gets or sets the item's own alignment on the cross axis, overriding the layout's.
    /// </summary>
    [Parameter] public AlignSelf? AlignSelf { get; set; }

    /// <summary>
    /// Gets or sets where the item is drawn, from 1 to 12, or <see cref="ItemOrder.First"/> or
    /// <see cref="ItemOrder.Last"/>. Only the visual order changes, so screen readers and the Tab key still
    /// follow the markup.
    /// </summary>
    [Parameter] public int? Order { get; set; }

    /// <summary>
    /// Gets or sets whether the item, and every item after it, is pushed to the end of the layout: to the end of a
    /// row, or to the bottom of a column. The way it pushes follows the enclosing <see cref="TwFlex"/>'s
    /// <see cref="TwFlex.Direction"/>.
    /// </summary>
    [Parameter] public bool PushEnd { get; set; }

    /// <summary>
    /// Gets or sets whether the item may shrink below the width of its content, so long text inside it can wrap
    /// or truncate instead of overflowing.
    /// </summary>
    [Parameter] public bool MinWidthZero { get; set; }

    /// <inheritdoc />
    protected override string rootClasses => new ClassBuilder(theme.Item)
        .AddClass(flexSizeClass)
        .AddClass(Grow switch { true => flexbox.Grow, false => flexbox.GrowNone, _ => string.Empty })
        .AddClass(Shrink switch { true => flexbox.Shrink, false => flexbox.ShrinkNone, _ => string.Empty })
        .AddClass(flexbox.Basis.Get(Basis))
        .AddClass(AlignSelfClass(AlignSelf))
        .AddClass(OrderClass(Order))
        .AddClass(parentFlex?.isColumn == true ? theme.ItemPushEndColumn : theme.ItemPushEnd, PushEnd)
        .AddClass(theme.ItemMinWidthZero, MinWidthZero)
        .AddClass(decorationClasses)
        .AddClass(Class)
        .Build();

    private string flexSizeClass => Flex switch
    {
        FlexSize.Auto => flexbox.FlexAuto,
        FlexSize.Initial => flexbox.FlexInitial,
        FlexSize.None => flexbox.FlexNone,
        FlexSize.Fill => flexbox.Flex1,
        _ => string.Empty
    };
}
