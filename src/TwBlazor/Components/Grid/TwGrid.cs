// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Lays out its children on a CSS grid: column and row counts that change at any breakpoint, columns that fit
/// themselves to the space, custom track templates, named areas, subgrid, auto flow, alignment and gaps.
/// </summary>
/// <remarks>
/// <para>
/// Columns are chosen in this order: <see cref="SubgridColumns"/>, then <see cref="ColumnTemplate"/>, then
/// <see cref="MinColumnWidth"/>, then <see cref="Columns"/> and its breakpoint variants. Rows work the same way
/// with <see cref="SubgridRows"/>, <see cref="RowTemplate"/> and <see cref="Rows"/>.
/// </para>
/// <para>
/// A grid is a visual layout, not a data grid: it adds no ARIA grid role and no arrow key navigation. Placing
/// items out of source order changes only where they are drawn, so keep the markup in reading order.
/// </para>
/// </remarks>
public class TwGrid : TwLayoutComponentBase
{
    private TwGridTheme theme => options.Theme.Components.Require<TwGridTheme>();

    /// <summary>
    /// Gets or sets whether the grid is inline, sitting in a line of text instead of taking a line of its own.
    /// </summary>
    [Parameter] public bool Inline { get; set; }

    /// <summary>
    /// Gets or sets the number of equal columns at every width, from 1 to 12.
    /// </summary>
    [Parameter] public int? Columns { get; set; }

    /// <summary>
    /// Gets or sets the number of columns from the <c>sm</c> breakpoint (40rem) upwards.
    /// </summary>
    [Parameter] public int? SmColumns { get; set; }

    /// <summary>
    /// Gets or sets the number of columns from the <c>md</c> breakpoint (48rem) upwards.
    /// </summary>
    [Parameter] public int? MdColumns { get; set; }

    /// <summary>
    /// Gets or sets the number of columns from the <c>lg</c> breakpoint (64rem) upwards.
    /// </summary>
    [Parameter] public int? LgColumns { get; set; }

    /// <summary>
    /// Gets or sets the number of columns from the <c>xl</c> breakpoint (80rem) upwards.
    /// </summary>
    [Parameter] public int? XlColumns { get; set; }

    /// <summary>
    /// Gets or sets the number of columns from the <c>2xl</c> breakpoint (96rem) upwards.
    /// </summary>
    [Parameter] public int? Xl2Columns { get; set; }

    /// <summary>
    /// Gets or sets the number of equal rows at every width, from 1 to 6.
    /// </summary>
    [Parameter] public int? Rows { get; set; }

    /// <summary>
    /// Gets or sets the number of rows from the <c>sm</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? SmRows { get; set; }

    /// <summary>
    /// Gets or sets the number of rows from the <c>md</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? MdRows { get; set; }

    /// <summary>
    /// Gets or sets the number of rows from the <c>lg</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? LgRows { get; set; }

    /// <summary>
    /// Gets or sets the number of rows from the <c>xl</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? XlRows { get; set; }

    /// <summary>
    /// Gets or sets the number of rows from the <c>2xl</c> breakpoint upwards.
    /// </summary>
    [Parameter] public int? Xl2Rows { get; set; }

    /// <summary>
    /// Gets or sets the narrowest a column may be, as a CSS length such as <c>16rem</c>. The grid then makes as many
    /// columns as fit, with no breakpoints needed, and drops to a single column when even one does not fit.
    /// </summary>
    [Parameter] public string? MinColumnWidth { get; set; }

    /// <summary>
    /// Gets or sets whether a grid with <see cref="MinColumnWidth"/> keeps empty columns when there are too few
    /// items to fill a row, so items keep the same width instead of stretching to fill it.
    /// </summary>
    [Parameter] public bool AutoFill { get; set; }

    /// <summary>
    /// Gets or sets a custom column template, as the value of CSS <c>grid-template-columns</c>, such as
    /// <c>16rem 1fr</c> for a fixed sidebar beside a flexible main column.
    /// </summary>
    [Parameter] public string? ColumnTemplate { get; set; }

    /// <summary>
    /// Gets or sets a custom row template, as the value of CSS <c>grid-template-rows</c>, such as <c>auto 1fr auto</c>.
    /// </summary>
    [Parameter] public string? RowTemplate { get; set; }

    /// <summary>
    /// Gets or sets named areas, one string per row with area names separated by spaces, such as
    /// <c>["header header", "sidebar main"]</c>. Place a <see cref="TwGridItem"/> in an area with its
    /// <see cref="TwGridItem.Area"/>. Use a full stop for an empty cell.
    /// </summary>
    [Parameter] public IReadOnlyList<string>? Areas { get; set; }

    /// <summary>
    /// Gets or sets whether this grid, nested in a grid item, lines its columns up with its parent grid's columns.
    /// </summary>
    [Parameter] public bool SubgridColumns { get; set; }

    /// <summary>
    /// Gets or sets whether this grid, nested in a grid item, lines its rows up with its parent grid's rows, so
    /// headings and buttons in neighbouring cards line up.
    /// </summary>
    [Parameter] public bool SubgridRows { get; set; }

    /// <summary>
    /// Gets or sets the space between rows and columns as a step on Tailwind's spacing scale, such as <c>6</c> for <c>gap-6</c>.
    /// </summary>
    [Parameter] public double? Gap { get; set; }

    /// <summary>
    /// Gets or sets the space between columns as a step on Tailwind's spacing scale. It takes the place of
    /// <see cref="Gap"/> between columns.
    /// </summary>
    [Parameter] public double? ColumnGap { get; set; }

    /// <summary>
    /// Gets or sets the space between rows as a step on Tailwind's spacing scale. It takes the place of
    /// <see cref="Gap"/> between rows.
    /// </summary>
    [Parameter] public double? RowGap { get; set; }

    /// <summary>
    /// Gets or sets how children without a position are placed. Leave it unset for the browser default, row by row.
    /// </summary>
    [Parameter] public GridAutoFlow? AutoFlow { get; set; }

    /// <summary>
    /// Gets or sets the size of columns the browser adds on its own, for example with <see cref="GridAutoFlow.Column"/>.
    /// </summary>
    [Parameter] public GridAutoSize? AutoColumns { get; set; }

    /// <summary>
    /// Gets or sets the size of rows the browser adds on its own. <see cref="GridAutoSize.Fr"/> makes every row as
    /// tall as the tallest.
    /// </summary>
    [Parameter] public GridAutoSize? AutoRows { get; set; }

    /// <summary>
    /// Gets or sets how every item is aligned inside its cell on the row axis.
    /// </summary>
    [Parameter] public JustifyItems? JustifyItems { get; set; }

    /// <summary>
    /// Gets or sets how every item is aligned inside its cell on the column axis.
    /// </summary>
    [Parameter] public AlignItems? AlignItems { get; set; }

    /// <summary>
    /// Gets or sets how the columns are spaced when they are narrower than the grid.
    /// </summary>
    [Parameter] public JustifyContent? JustifyContent { get; set; }

    /// <summary>
    /// Gets or sets how the rows are spaced when they are shorter than the grid.
    /// </summary>
    [Parameter] public AlignContent? AlignContent { get; set; }

    /// <inheritdoc />
    protected override string rootClasses => new ClassBuilder(Inline ? theme.Inline : theme.Base)
        .AddClass(columnClasses)
        .AddClass(rowClasses)
        .AddClass(spacing.Gap.Get(Gap))
        .AddClass(spacing.GapX.Get(ColumnGap))
        .AddClass(spacing.GapY.Get(RowGap))
        .AddClass(grid.AutoFlow.Get(AutoFlow))
        .AddClass(grid.AutoColumns.Get(AutoColumns))
        .AddClass(grid.AutoRows.Get(AutoRows))
        .AddClass(justifyItemsClass)
        .AddClass(AlignItemsClass(AlignItems))
        .AddClass(JustifyContentClass(JustifyContent))
        .AddClass(AlignContentClass(AlignContent))
        .AddClass(decorationClasses)
        .AddClass(Class)
        .Build();

    /// <inheritdoc />
    protected override string? rootStyle => JoinStyles(
        usesMinColumnWidth ? $"--tw-grid-min: {CssValue(MinColumnWidth!)}" : null,
        HasText(ColumnTemplate) ? $"grid-template-columns: {CssValue(ColumnTemplate!)}" : null,
        HasText(RowTemplate) ? $"grid-template-rows: {CssValue(RowTemplate!)}" : null,
        areasStyle,
        Style);

    private bool usesMinColumnWidth => !SubgridColumns && !HasText(ColumnTemplate) && HasText(MinColumnWidth);

    private string columnClasses
    {
        get
        {
            if (SubgridColumns)
            {
                return grid.ColumnsSubgrid;
            }

            if (HasText(ColumnTemplate))
            {
                return string.Empty;
            }

            if (usesMinColumnWidth)
            {
                return AutoFill ? theme.AutoFill : theme.AutoFit;
            }

            return ResponsiveClasses(grid.Columns, Columns, SmColumns, MdColumns, LgColumns, XlColumns, Xl2Columns);
        }
    }

    private string rowClasses
    {
        get
        {
            if (SubgridRows)
            {
                return grid.RowsSubgrid;
            }

            return HasText(RowTemplate)
                ? string.Empty
                : ResponsiveClasses(grid.Rows, Rows, SmRows, MdRows, LgRows, XlRows, Xl2Rows);
        }
    }

    private string justifyItemsClass => JustifyItems switch
    {
        Enums.JustifyItems.Start => grid.JustifyItems.Start,
        Enums.JustifyItems.Center => grid.JustifyItems.Center,
        Enums.JustifyItems.End => grid.JustifyItems.End,
        Enums.JustifyItems.Stretch => grid.JustifyItems.Stretch,
        _ => string.Empty
    };

    private string? areasStyle
    {
        get
        {
            var rows = Areas?
                .Select(row => CssValue(row))
                .Where(row => row.Length > 0)
                .Select(row => $"\"{row}\"")
                .ToList();

            return rows is { Count: > 0 } ? $"grid-template-areas: {string.Join(' ', rows)}" : null;
        }
    }

    private static bool HasText(string? value) => !string.IsNullOrWhiteSpace(value);
}
