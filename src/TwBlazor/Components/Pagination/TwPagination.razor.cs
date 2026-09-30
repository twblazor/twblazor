// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

public partial class TwPagination : TwBlazorComponentBase
{
    private TwPaginationTheme theme => options.Theme.Components.Require<TwPaginationTheme>();

    /// <summary>
    /// Gets and sets the current active page.
    /// </summary>
    [Parameter] public int ActivePage { get; set; } = 1;

    /// <summary>
    /// The total count of pages displayed.
    /// </summary>
    [Parameter] public int TotalPages { get; set; } = 1;

    /// <summary>
    /// The bound event callback when the active page is changed.
    /// </summary>
    [Parameter] public EventCallback<int> ActivePageChanged { get; set; }

    /// <summary>
    /// Whether to show an optional "items per page" dropdown alongside the pagination controls.
    /// </summary>
    [Parameter] public bool ShowPageSize { get; set; } = false;

    /// <summary>
    /// The number of items displayed per page. Only used when <see cref="ShowPageSize"/> is true.
    /// </summary>
    [Parameter] public int PageSize { get; set; } = 10;


    /// <summary>
    /// The bound event callback when the page size is changed e.g. from displaying 5 per page to 10 etc.
    /// </summary>
    [Parameter] public EventCallback<int> PageSizeChanged { get; set; }

    /// <summary>
    /// The selectable options for <see cref="PageSize"/>.
    /// </summary>
    [Parameter] public int[] PageSizeOptions { get; set; } = [5, 10, 25, 50, 100];

    /// <summary>
    /// Whether to render smaller, more compact buttons.
    /// </summary>
    [Parameter] public bool Dense { get; set; }

    private string buttonBaseClasses => new ClassBuilder(theme.Base)
        .AddClass(Dense ? theme.DenseSize : theme.Size)
        .Build();

    private string rootClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Align.Center)
        .AddClass(options.Theme.Spacing.Gap.Lg)
        .AddClass(Class).Build();

    /// <summary>
    /// Gets the classes for the joined button group. The group clips its buttons to a single rounded,
    /// bordered container (the same construction as the table's container) so the buttons themselves
    /// need no rounding or borders of their own.
    /// </summary>
    private string listClasses => new ClassBuilder(theme.List)
        .AddClass(roundedBuilder.GetRounded())
        .Build();

    // The active page is distinguished by more than color alone (font-weight + background tint),
    // so low-vision/color-deficient sighted users have a cue beyond the purple/gray hue difference.
    private string IsActivePage(int page) =>
        new ClassBuilder(buttonBaseClasses)
        .AddClass(theme.ActiveButton, page == ActivePage)
        .AddClass(theme.Buttons, page != ActivePage)
        .Build();

    /// <summary>
    /// Whether the Previous button is at the lower boundary and should be disabled.
    /// </summary>
    private bool isPreviousDisabled => ActivePage <= 1;

    /// <summary>
    /// Whether the Next button is at the upper boundary and should be disabled.
    /// </summary>
    private bool isNextDisabled => ActivePage >= TotalPages;

    private string NavButtonClass(bool disabled) =>
        new ClassBuilder(buttonBaseClasses)
        .AddClass(theme.Buttons, !disabled)
        .AddClass(options.Theme.Interaction.DisabledOpacity, disabled)
        .Build();

    /// <summary>
    /// Gets up to 3 page numbers centred on <see cref="ActivePage"/>, clamped to stay within
    /// [1, TotalPages] so the window slides toward the nearest edge instead of leaving a gap.
    /// </summary>
    private IEnumerable<int> VisiblePages()
    {
        var windowSize = Math.Min(3, TotalPages);
        var start = Math.Clamp(ActivePage - 1, 1, TotalPages - windowSize + 1);

        return Enumerable.Range(start, windowSize);
    }

    private async Task OnPageClick(int page)
    {
        page = Math.Clamp(page, 1, TotalPages);

        if (page == ActivePage)
        {
            return;
        }

        ActivePage = page;

        if (ActivePageChanged.HasDelegate)
        {
            await ActivePageChanged.InvokeAsync(page);
        }
    }

    private async Task OnPageSizeSelected(int selected)
    {
        if (selected != PageSize)
        {
            PageSize = selected;

            if (PageSizeChanged.HasDelegate)
            {
                await PageSizeChanged.InvokeAsync(selected);
            }
        }
    }
}
