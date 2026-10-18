// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// Theme configuration for table components (<see cref="TwBlazor.Components.TwTable"/> and
/// <see cref="TwBlazor.Components.TwDataTable{TItem}"/>).
/// Override any property to customize table styles globally.
/// </summary>
/// <remarks>
/// <see cref="TwBlazor.Components.TwDataTable{TItem}"/> does not define its own table theme - its
/// auto-generated header row (used when rendering <c>Columns</c>) reuses <see cref="Header"/> so both
/// components stay visually consistent from a single configuration point.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwTableTheme
{
    /// <summary>
    /// Gets or sets the classes for the scrollable wrapper around the table element.
    /// </summary>
    public required string Wrapper { get; set; }

    /// <summary>
    /// Gets or sets the classes that clip the outer container to its rounded corners.
    /// </summary>
    public required string ContainerClip { get; set; }

    /// <summary>
    /// Gets or sets the base classes applied to the table element itself.
    /// </summary>
    public required string Base { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to the outer wrapping <c>&lt;div&gt;</c> when the table's
    /// <c>NoBorder</c> parameter is <c>false</c> (the default) - the container's border and shadow.
    /// Rounding and clipping live on that same wrapper regardless of <c>NoBorder</c>, via
    /// <see cref="TwBlazor.TwBlazorComponentBase.Rounded"/>, so the table's corners (including the
    /// header's background) are always clipped cleanly instead of the raw square corners a
    /// <c>&lt;table&gt;</c> element renders even with a rounded class applied directly to it.
    /// </summary>
    public required string Bordered { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to the table element when the table's <c>FixedLayout</c> parameter
    /// is <c>true</c> (e.g. <c>table-fixed</c>).
    /// </summary>
    public string FixedLayout { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes applied to the table header, controlling its background, text colour,
    /// typography, and the permanent divider separating it from the body.
    /// </summary>
    public required string Header { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to the table header when the table's <c>StickyHeader</c> parameter is
    /// <c>true</c>, which keep it at the top of the scrolling table (e.g. <c>sticky top-0 z-10</c>).
    /// </summary>
    public string StickyHeader { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base background classes applied to the table body.
    /// </summary>
    public required string Body { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to the table body when its <c>Striped</c> parameter is <c>true</c>.
    /// </summary>
    public required string BodyStriped { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to the table body when its <c>Hoverable</c> parameter is <c>true</c>.
    /// </summary>
    public required string BodyHoverable { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to the table body when its <c>Bordered</c> parameter is <c>true</c> -
    /// a single hairline between consecutive rows (e.g. via <c>divide-y</c>). Deliberately not a full
    /// per-cell grid: a border around every cell reads as a busy, dated spreadsheet grid, especially
    /// combined with <c>Striped</c> - a thin row divider gives the same "this is bordered" affordance
    /// without the clash.
    /// </summary>
    public required string RowDivider { get; set; }

    /// <summary>
    /// Gets or sets the classes applied to the table footer when the table's <c>Bordered</c> parameter
    /// is <c>true</c> - a single divider separating it from the body.
    /// </summary>
    public required string Footer { get; set; }

    /// <summary>
    /// Gets or sets the width classes for <see cref="TwBlazor.Components.TwDataTable{TItem}"/>'s search
    /// field column, when <c>Searchable</c> is enabled.
    /// </summary>
    public required string SearchColumn { get; set; }

    /// <summary>
    /// Gets or sets the color classes for an auto-generated column header's sort icon.
    /// </summary>
    public required string SortIcon { get; set; }

    /// <summary>
    /// Gets or sets the padding classes for an auto-generated column header cell.
    /// </summary>
    public required string HeaderCellPadding { get; set; }

    /// <summary>
    /// Gets or sets the padding classes for an auto-generated body cell.
    /// </summary>
    public required string BodyCellPadding { get; set; }

    /// <summary>
    /// Gets or sets the classes for the "no data" row shown when there are no rows to display.
    /// </summary>
    public required string EmptyState { get; set; }


    /// <summary>
    /// Gets or sets the classes for the separator on a resizable column's border, which a person drags or focuses
    /// and moves with the arrow keys to resize the column (<see cref="TwBlazor.Components.TwTable.ResizableColumns"/>).
    /// The resize script positions it, so it needs absolute positioning; centre it on the border, give it a column
    /// resize cursor and a visible focus state.
    /// </summary>
    public string ResizeHandle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the width classes for <see cref="TwBlazor.Components.TwDataTable{TItem}"/>'s Group by list.
    /// </summary>
    public string GroupByColumn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the header row at the top of each group when the rows are grouped.
    /// </summary>
    public string GroupRow { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the button in a group header that expands or collapses the group, including
    /// its focus ring.
    /// </summary>
    public string GroupToggle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the row count beside a group's name.
    /// </summary>
    public string GroupCount { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the cell shown in place of a row while
    /// <see cref="TwBlazor.Components.TwDataTable{TItem}"/> loads it.
    /// </summary>
    public string LoadingCell { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for <see cref="TwBlazor.Components.TwDataTable{TItem}"/>'s Actions column, which
    /// holds each row's Edit button, or its Save and Cancel buttons while it is edited.
    /// </summary>
    public string ActionsCell { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the row being edited, which set it apart from the rows around it.
    /// </summary>
    public string EditingRow { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for <see cref="TwBlazor.Components.TwDataTable{TItem}"/>'s column filter, a dialog
    /// positioned under the column's filter button. The filter script places it with fixed positioning.
    /// </summary>
    public string FilterPanel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the heading at the top of a column filter.
    /// </summary>
    public string FilterHeading { get; set; } = string.Empty;
}
