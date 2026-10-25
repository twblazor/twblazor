// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Lays out its children in a row or a column with CSS flexbox: direction (changing at any breakpoint),
/// wrapping, alignment, gaps and dividers between children.
/// </summary>
/// <remarks>
/// Reversing the direction changes only where children are drawn. Screen readers and the Tab key still follow
/// the markup, so keep the markup in reading order.
/// </remarks>
public class TwFlex : TwLayoutComponentBase
{
    private TwFlexTheme theme => options.Theme.Components.Require<TwFlexTheme>();

    /// <summary>
    /// Gets or sets whether the layout is inline, sitting in a line of text instead of taking a line of its own.
    /// </summary>
    [Parameter] public bool Inline { get; set; }

    /// <summary>
    /// Gets or sets the direction at every width. Leave it unset for the browser default, a row.
    /// </summary>
    [Parameter] public FlexDirection? Direction { get; set; }

    /// <summary>
    /// Gets or sets the direction from the <c>sm</c> breakpoint (40rem) upwards.
    /// </summary>
    [Parameter] public FlexDirection? SmDirection { get; set; }

    /// <summary>
    /// Gets or sets the direction from the <c>md</c> breakpoint (48rem) upwards.
    /// </summary>
    [Parameter] public FlexDirection? MdDirection { get; set; }

    /// <summary>
    /// Gets or sets the direction from the <c>lg</c> breakpoint (64rem) upwards.
    /// </summary>
    [Parameter] public FlexDirection? LgDirection { get; set; }

    /// <summary>
    /// Gets or sets the direction from the <c>xl</c> breakpoint (80rem) upwards.
    /// </summary>
    [Parameter] public FlexDirection? XlDirection { get; set; }

    /// <summary>
    /// Gets or sets the direction from the <c>2xl</c> breakpoint (96rem) upwards.
    /// </summary>
    [Parameter] public FlexDirection? Xl2Direction { get; set; }

    /// <summary>
    /// Gets or sets whether children wrap onto new lines. Leave it unset for the browser default, no wrapping.
    /// </summary>
    [Parameter] public FlexWrap? Wrap { get; set; }

    /// <summary>
    /// Gets or sets how children are spaced along the main axis.
    /// </summary>
    [Parameter] public JustifyContent? Justify { get; set; }

    /// <summary>
    /// Gets or sets how children are aligned on the cross axis.
    /// </summary>
    [Parameter] public AlignItems? Align { get; set; }

    /// <summary>
    /// Gets or sets how wrapped lines are spaced on the cross axis. Only has an effect when children wrap.
    /// </summary>
    [Parameter] public AlignContent? AlignContent { get; set; }

    /// <summary>
    /// Gets or sets the space between children as a step on Tailwind's spacing scale, such as <c>4</c> for <c>gap-4</c>.
    /// </summary>
    [Parameter] public double? Gap { get; set; }

    /// <summary>
    /// Gets or sets the space between columns as a step on Tailwind's spacing scale, such as <c>5</c> for
    /// <c>gap-x-5</c>. It takes the place of <see cref="Gap"/> between columns.
    /// </summary>
    [Parameter] public double? ColumnGap { get; set; }

    /// <summary>
    /// Gets or sets the space between rows as a step on Tailwind's spacing scale, such as <c>1</c> for
    /// <c>gap-y-1</c>. It takes the place of <see cref="Gap"/> between rows.
    /// </summary>
    [Parameter] public double? RowGap { get; set; }

    /// <summary>
    /// Gets or sets whether the layout fills its parent's width.
    /// </summary>
    [Parameter] public bool FullWidth { get; set; }

    /// <summary>
    /// Gets or sets whether the layout fills its parent's height, for example to push a button to the bottom of a card.
    /// </summary>
    [Parameter] public bool FullHeight { get; set; }

    /// <summary>
    /// Gets or sets whether a line is drawn between children: across a column, or down a row. The line follows
    /// <see cref="Direction"/>, not the responsive directions.
    /// </summary>
    [Parameter] public bool Divided { get; set; }

    /// <inheritdoc />
    protected override string rootClasses => new ClassBuilder(Inline ? theme.Inline : theme.Base)
        .AddClass(JoinClasses(
            flexbox.ResponsiveDirection.Get(Direction, Breakpoint.Base),
            flexbox.ResponsiveDirection.Get(SmDirection, Breakpoint.Sm),
            flexbox.ResponsiveDirection.Get(MdDirection, Breakpoint.Md),
            flexbox.ResponsiveDirection.Get(LgDirection, Breakpoint.Lg),
            flexbox.ResponsiveDirection.Get(XlDirection, Breakpoint.Xl),
            flexbox.ResponsiveDirection.Get(Xl2Direction, Breakpoint.Xl2)))
        .AddClass(wrapClass)
        .AddClass(JustifyContentClass(Justify))
        .AddClass(AlignItemsClass(Align))
        .AddClass(AlignContentClass(AlignContent))
        .AddClass(steps.Gap.Get(Gap))
        .AddClass(steps.GapX.Get(ColumnGap))
        .AddClass(steps.GapY.Get(RowGap))
        .AddClass(theme.FullWidth, FullWidth)
        .AddClass(theme.FullHeight, FullHeight)
        .AddClass(dividerClasses, Divided)
        .AddClass(decorationClasses)
        .AddClass(Class)
        .Build();

    /// <summary>
    /// Gets whether the layout stacks its children at every width, which decides the way
    /// <see cref="TwFlexItem.PushEnd"/> pushes.
    /// </summary>
    internal bool isColumn => Direction is FlexDirection.Column or FlexDirection.ColumnReverse;

    /// <inheritdoc />
    protected override void BuildContent(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<TwFlex>>(8);
        builder.AddAttribute(9, nameof(CascadingValue<TwFlex>.Value), this);
        builder.AddAttribute(10, nameof(CascadingValue<TwFlex>.ChildContent), ChildContent);
        builder.CloseComponent();
    }

    private string wrapClass => Wrap switch
    {
        FlexWrap.NoWrap => flexbox.NoWrap,
        FlexWrap.Wrap => flexbox.Wrap,
        FlexWrap.WrapReverse => flexbox.WrapReverse,
        _ => string.Empty
    };

    private string dividerClasses => Direction switch
    {
        FlexDirection.Column => JoinClasses(theme.DivideY, theme.DivideColor),
        FlexDirection.ColumnReverse => JoinClasses(theme.DivideY, theme.DivideYReverse, theme.DivideColor),
        FlexDirection.RowReverse => JoinClasses(theme.DivideX, theme.DivideXReverse, theme.DivideColor),
        _ => JoinClasses(theme.DivideX, theme.DivideColor)
    };
}
