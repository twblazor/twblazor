// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// The container for a chart: it frames any chart component (such as <see cref="TwBarChart"/> or
/// <see cref="TwLineChart"/>) with a title, a description and a loading state.
/// </summary>
/// <remarks>
/// The container renders a <c>figure</c> whose caption is the title, and the chart inside it is labelled by that
/// title. Each chart draws its own legend, tooltips and table view, so the container stays the same whichever
/// chart it holds.
/// </remarks>
public partial class TwChart : TwBlazorComponentBase
{
    private TwChartTheme theme => options.Theme.Components.Require<TwChartTheme>();

    /// <summary>
    /// Gets or sets the chart to display, for example a <see cref="TwColumnChart"/>.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the title shown above the chart. It also names the chart for assistive technology.
    /// </summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// Gets or sets a short description shown under the title, for example the unit or the period covered.
    /// </summary>
    [Parameter] public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the height of the plot area for the charts inside the container.
    /// </summary>
    /// <remarks>
    /// Default is <see cref="ChartHeight.Medium"/>. A chart can override it with its own <c>Height</c>.
    /// </remarks>
    [Parameter] public ChartHeight Height { get; set; } = ChartHeight.Medium;

    /// <summary>
    /// Gets or sets whether the data is still loading. A placeholder is shown in place of the chart.
    /// </summary>
    [Parameter] public bool Loading { get; set; }

    /// <summary>
    /// Gets or sets the text announced by assistive technology while <see cref="Loading"/> is set.
    /// </summary>
    [Parameter] public string LoadingLabel { get; set; } = "Loading chart";

    /// <summary>
    /// Gets the id of the title element, or <see langword="null"/> when there is no title. Charts inside the
    /// container use it as their accessible name.
    /// </summary>
    internal string? titleId => string.IsNullOrWhiteSpace(Title) ? null : $"{Id}-title";

    private string classes => new ClassBuilder(theme.Container).AddClass(Class).Build();

    private string skeletonHeight => Height switch
    {
        ChartHeight.Small => "10rem",
        ChartHeight.Large => "24rem",
        _ => "16rem"
    };
}
