// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Configuration.Components;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Represents a customizable table component for Blazor applications that enables rendering of table headers, body, and
/// footers with configurable styling and behaviour.
/// </summary>
/// <remarks>The TwTable component provides flexibility for displaying tabular data by allowing developers to
/// supply custom content for the header, body, and footer sections using RenderFragment parameters. It supports
/// additional styling options such as striped rows, hover effects, and borders, which can be controlled through
/// component parameters. Additional CSS classes can be applied to each section for further customization. This
/// component is intended to be used as a building block for creating accessible and visually consistent tables in
/// Blazor projects.</remarks>
public partial class TwTable : TwBlazorComponentBase
{
    // The wrapper scrolls when the table is wider than its container. A keyboard can only scroll it once it
    // is focusable, and a focusable region needs a name.
    private string scrollRegionLabel => string.IsNullOrWhiteSpace(AriaLabel) ? effectiveScrollRegionLabel : $"{AriaLabel}, {effectiveScrollRegionLabel}";

    /// <summary>
    /// Gets or sets the accessible name of the scrollable region that wraps the table.
    /// Defaults to the localized <see cref="TwStrings.Table_ScrollRegion"/> text ("Scrollable table").
    /// </summary>
    [Parameter] public string? ScrollRegionLabel { get; set; }

    private string effectiveScrollRegionLabel => ScrollRegionLabel ?? localizer[TwStrings.Table_ScrollRegion];

    private ElementReference wrapperRef;

    [Inject] private IJSRuntime jsRuntime { get; set; } = null!;

    /// <summary>
    /// Hands the scroll wrapper to the script that keeps it a Tab stop only while the table overflows it.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            // If the script cannot run (prerendering, or the circuit has gone) the wrapper stays focusable.
            await InvokeScriptAsync("twScrollRegion.observe", wrapperRef);
        }

        await SyncResizeScriptAsync();
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        SyncPagingParameters();
    }

    private TwTableTheme theme => options.Theme.Components.Require<TwTableTheme>();

    /// <summary>
    /// Optional caption content for the table, rendered as a <c>&lt;caption&gt;</c> element. Provides an accessible
    /// name/description for the table that is programmatically associated with it. Visually hidden by default
    /// (via the <c>sr-only</c> class) unless <see cref="CaptionVisible"/> is set to <c>true</c>.
    /// </summary>
    [Parameter]
    public RenderFragment? Caption { get; set; }

    /// <summary>
    /// Whether the <see cref="Caption"/> should be visible. Defaults to <c>false</c>, rendering the caption
    /// visually hidden (but still available to assistive technology).
    /// </summary>
    [Parameter]
    public bool CaptionVisible { get; set; } = false;

    /// <summary>
    /// Optional column definitions, rendered inside a <c>&lt;colgroup&gt;</c> before the table sections. Use
    /// <see cref="TwTableColumn"/> to give columns a width, so long or breakable content (such as inline code)
    /// cannot squeeze a column narrower than its content needs.
    /// </summary>
    [Parameter]
    public RenderFragment? ColumnGroup { get; set; }

    /// <summary>
    /// Whether column widths come from the <see cref="ColumnGroup"/> and the header row instead of the cell
    /// content (CSS <c>table-layout: fixed</c>). Columns keep the same width however their content changes,
    /// which suits tables whose rows are paged, filtered or loaded on demand. Defaults to <c>false</c>.
    /// </summary>
    [Parameter]
    public bool FixedLayout { get; set; }

    /// <summary>
    /// Gets or sets the tallest the table can grow before its rows scroll inside it, as any CSS length such as
    /// <c>24rem</c> or <c>60vh</c>. Leave it unset for the table to be as tall as its rows. Pair it with
    /// <see cref="StickyHeader"/> to keep the column headings in view.
    /// </summary>
    [Parameter]
    public string? MaxHeight { get; set; }

    /// <summary>
    /// Whether the header row stays at the top of the table while its rows scroll beneath it. Only has an effect
    /// when the table scrolls, for example with <see cref="MaxHeight"/>. Defaults to <c>false</c>.
    /// </summary>
    [Parameter]
    public bool StickyHeader { get; set; }

    private string? wrapperStyle => string.IsNullOrWhiteSpace(effectiveMaxHeight) ? null : $"max-height:{effectiveMaxHeight.Trim()}";

    /// <summary>
    /// Content to render in the table header (thead).
    /// </summary>
    [Parameter]
    public RenderFragment? TableHeader { get; set; }

    /// <summary>
    /// Content to render in the table body (tbody).
    /// </summary>
    [Parameter]
    public RenderFragment? TableBody { get; set; }

    /// <summary>
    /// Content made of several <c>&lt;tbody&gt;</c> elements, one for each group of rows, rendered in place of
    /// <see cref="TableBody"/>. The context is the body's CSS classes (including <see cref="Striped"/>,
    /// <see cref="Hoverable"/> and <see cref="Bordered"/>), so put it on every <c>&lt;tbody&gt;</c> to style the groups
    /// like a single body. Start each group with a header cell that has <c>scope="rowgroup"</c>, and assistive
    /// technology announces it with every row in the group.
    /// </summary>
    [Parameter]
    public RenderFragment<string>? RowGroups { get; set; }

    /// <summary>
    /// Content to render in the table footer (tfoot).
    /// </summary>
    [Parameter]
    public RenderFragment? TableFooter { get; set; }

    /// <summary>
    /// Additional CSS classes for the header section.
    /// </summary>
    [Parameter]
    public string? HeaderClass { get; set; }

    /// <summary>
    /// Additional CSS classes for the body section.
    /// </summary>
    [Parameter]
    public string? BodyClass { get; set; }

    /// <summary>
    /// Additional CSS classes for the footer section.
    /// </summary>
    [Parameter]
    public string? FooterClass { get; set; }

    /// <summary>
    /// Whether to show striped rows.
    /// </summary>
    [Parameter]
    public bool Striped { get; set; }

    /// <summary>
    /// Whether to show hover effect on rows.
    /// </summary>
    [Parameter]
    public bool Hoverable { get; set; }

    /// <summary>
    /// Whether to show borders on all cells.
    /// </summary>
    [Parameter]
    public bool Bordered { get; set; } = false;

    /// <summary>
    /// Whether to hide the outer table border.
    /// </summary>
    [Parameter]
    public bool NoBorder { get; set; } = false;

    /// <summary>
    /// Gets the classes for the outer wrapping &lt;div&gt; - rounding, clipping, and (unless <see cref="NoBorder"/>)
    /// the container border/shadow. Kept on this wrapper rather than the &lt;table&gt; itself, and separate from
    /// the inner scroll &lt;div&gt;, since a &lt;table&gt; element doesn't reliably clip its own rounded corners
    /// (the header's background bleeds past them), and combining scrolling with clipping on a single element
    /// would fight over the same overflow behavior.
    /// </summary>
    /// <remarks>
    /// Also carries <see cref="TwTableTheme.Body"/>'s background (the container itself is otherwise
    /// transparent, so any slack between the table and the rounded clip boundary - e.g. the horizontal
    /// scrollbar gutter the inner scroll &lt;div&gt; can reserve along the bottom edge - would reveal
    /// the page behind it there instead of matching the table) and <see cref="TwBlazorComponentBase.Class"/>:
    /// this wrapper is the component's actual visual box, so a caller's margin/width/etc. needs to land
    /// here rather than on the inner &lt;table&gt;, where it would be trapped inside the rounded clip
    /// instead of creating space around the whole component.
    /// </remarks>
    private string containerClasses => new ClassBuilder()
        .AddClass(roundedBuilder.GetRounded(effectiveRounded))
        .AddClass(theme.ContainerClip)
        .AddClass(theme.Body)
        .AddClass(theme.Bordered, !NoBorder)
        .AddClass(Class)
        .Build();

    private string tableClasses => new ClassBuilder()
        .AddClass(theme.Base)
        .AddClass(theme.FixedLayout, effectiveFixedLayout)
        .Build();

    private string headerClasses => new ClassBuilder()
        .AddClass(theme.Header)
        .AddClass(theme.StickyHeader, effectiveStickyHeader)
        .AddClass(HeaderClass ?? string.Empty)
        .Build();

    private string bodyClasses => new ClassBuilder()
        .AddClass(theme.Body)
        .AddClass(theme.BodyStriped, Striped)
        .AddClass(theme.BodyHoverable, Hoverable)
        .AddClass(theme.RowDivider, Bordered)
        .AddClass(BodyClass ?? string.Empty)
        .Build();

    private string captionClasses => new ClassBuilder()
        .AddClass(options.Theme.Display.ScreenReaderOnly, !CaptionVisible)
        .Build();

    private string footerClasses => new ClassBuilder()
        .AddClass(theme.Footer, Bordered)
        .AddClass(FooterClass ?? string.Empty)
        .Build();
}
