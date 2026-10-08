// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace TwBlazor.Configuration.Components;

/// <summary>
/// One chart color, declared once per place it can be painted so Tailwind can see and compile every class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TwChartColor
{
    /// <summary>
    /// Gets or sets the background classes, used by bars, dots, legend swatches and tooltip keys.
    /// </summary>
    public required string Background { get; set; }

    /// <summary>
    /// Gets or sets the SVG fill classes, used by areas, sectors and other filled paths.
    /// </summary>
    public required string Fill { get; set; }

    /// <summary>
    /// Gets or sets the SVG stroke classes, used by lines.
    /// </summary>
    public required string Stroke { get; set; }

    /// <summary>
    /// Gets or sets the text color classes, used by icons. Labels never use it: chart text stays in neutral ink.
    /// </summary>
    public required string Text { get; set; }
}

/// <summary>
/// Theme configuration for <see cref="TwBlazor.Components.TwChart"/> and every chart drawn inside it.
/// Override any property to customize chart styles globally.
/// </summary>
/// <remarks>
/// Chart geometry (positions and sizes) is written as inline styles because it is computed from data. Everything
/// else, including every color, comes from here.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TwChartTheme
{
    /// <summary>
    /// Gets or sets the classes for the <see cref="TwBlazor.Components.TwChart"/> container.
    /// </summary>
    public required string Container { get; set; }

    /// <summary>
    /// Gets or sets the classes for the container's caption, which holds the title and description.
    /// </summary>
    public required string Header { get; set; }

    /// <summary>
    /// Gets or sets the classes for the container's title.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the classes for the container's description.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the classes for the container's body, which holds the charts.
    /// </summary>
    public required string Body { get; set; }

    /// <summary>
    /// Gets or sets the classes for the root element of a chart.
    /// </summary>
    public required string Chart { get; set; }

    /// <summary>
    /// Gets or sets the classes for the row above the plot that holds the legend and the table toggle.
    /// </summary>
    public required string Toolbar { get; set; }

    /// <summary>
    /// Gets or sets the classes for the legend list.
    /// </summary>
    public required string Legend { get; set; }

    /// <summary>
    /// Gets or sets the classes for a legend entry.
    /// </summary>
    public required string LegendItem { get; set; }

    /// <summary>
    /// Gets or sets the extra classes for a legend entry that can be clicked to hide its series.
    /// </summary>
    public required string LegendButton { get; set; }

    /// <summary>
    /// Gets or sets the extra classes for a legend entry whose series is hidden.
    /// </summary>
    public required string LegendItemHidden { get; set; }

    /// <summary>
    /// Gets or sets the classes for a legend entry's color swatch. The color itself comes from the series.
    /// </summary>
    public required string LegendSwatch { get; set; }

    /// <summary>
    /// Gets or sets the classes for the button that switches between the plot and the table view.
    /// </summary>
    public required string TableToggle { get; set; }

    /// <summary>
    /// Gets or sets the classes for an axis title.
    /// </summary>
    public required string AxisTitle { get; set; }

    /// <summary>
    /// Gets or sets the extra classes for the title under the horizontal axis.
    /// </summary>
    public required string XAxisTitle { get; set; }

    /// <summary>
    /// Gets or sets the classes for the element wrapping the plot and its axes when there is no vertical axis.
    /// </summary>
    public required string Frame { get; set; }

    /// <summary>
    /// Gets or sets the classes for the element wrapping the plot and its axes when there is a vertical axis:
    /// a two-column grid whose first column sizes itself to the widest label.
    /// </summary>
    public required string FrameWithYAxis { get; set; }

    /// <summary>
    /// Gets or sets the classes for the vertical axis column.
    /// </summary>
    public required string YAxis { get; set; }

    /// <summary>
    /// Gets or sets the classes for the invisible, zero-height copy of the vertical axis labels that gives the
    /// axis column its width. The visible labels are absolutely positioned, so they cannot size it themselves.
    /// </summary>
    public required string AxisSizer { get; set; }

    /// <summary>
    /// Gets or sets the typography classes shared by every axis label.
    /// </summary>
    public required string Tick { get; set; }

    /// <summary>
    /// Gets or sets the extra classes for a vertical axis label.
    /// </summary>
    public required string YTick { get; set; }

    /// <summary>
    /// Gets or sets the classes for the horizontal axis row.
    /// </summary>
    public required string XAxis { get; set; }

    /// <summary>
    /// Gets or sets the extra classes for a horizontal axis label.
    /// </summary>
    public required string XTick { get; set; }

    /// <summary>
    /// Gets or sets the classes for the plot area, the positioning context for every mark.
    /// </summary>
    public required string Plot { get; set; }

    /// <summary>
    /// Gets or sets the plot height classes for <see cref="Enums.ChartHeight.Small"/>.
    /// </summary>
    public required string Small { get; set; }

    /// <summary>
    /// Gets or sets the plot height classes for <see cref="Enums.ChartHeight.Medium"/>.
    /// </summary>
    public required string Medium { get; set; }

    /// <summary>
    /// Gets or sets the plot height classes for <see cref="Enums.ChartHeight.Large"/>.
    /// </summary>
    public required string Large { get; set; }

    /// <summary>
    /// Gets or sets the classes that make the plot area a centered square, used by radial charts.
    /// </summary>
    public required string Square { get; set; }

    /// <summary>
    /// Gets or sets the maximum width classes for a square plot at <see cref="Enums.ChartHeight.Small"/>.
    /// </summary>
    public required string SquareSmall { get; set; }

    /// <summary>
    /// Gets or sets the maximum width classes for a square plot at <see cref="Enums.ChartHeight.Medium"/>.
    /// </summary>
    public required string SquareMedium { get; set; }

    /// <summary>
    /// Gets or sets the maximum width classes for a square plot at <see cref="Enums.ChartHeight.Large"/>.
    /// </summary>
    public required string SquareLarge { get; set; }

    /// <summary>
    /// Gets or sets the classes for the SVG layer that draws lines and paths.
    /// </summary>
    public required string Canvas { get; set; }

    /// <summary>
    /// Gets or sets the classes shared by every rectangle mark.
    /// </summary>
    public required string Rect { get; set; }

    /// <summary>
    /// Gets or sets the classes that round the top corners of a rectangle (the data end of a rising column).
    /// </summary>
    public required string RoundedTop { get; set; }

    /// <summary>
    /// Gets or sets the classes that round the bottom corners of a rectangle.
    /// </summary>
    public required string RoundedBottom { get; set; }

    /// <summary>
    /// Gets or sets the classes that round the left corners of a rectangle.
    /// </summary>
    public required string RoundedLeft { get; set; }

    /// <summary>
    /// Gets or sets the classes that round the right corners of a rectangle (the data end of a rising bar).
    /// </summary>
    public required string RoundedRight { get; set; }

    /// <summary>
    /// Gets or sets the classes that round every corner of a rectangle.
    /// </summary>
    public required string RoundedAll { get; set; }

    /// <summary>
    /// Gets or sets the classes that draw a thin ring in the surface color around a rectangle, separating
    /// touching marks with a gap instead of a border. Must match the background the chart sits on.
    /// </summary>
    public required string SurfaceRing { get; set; }

    /// <summary>
    /// Gets or sets the classes shared by every dot mark, including its surface-colored ring.
    /// </summary>
    public required string Dot { get; set; }

    /// <summary>
    /// Gets or sets the classes for the element positioning an icon mark.
    /// </summary>
    public required string Glyph { get; set; }

    /// <summary>
    /// Gets or sets the classes shared by every text mark.
    /// </summary>
    public required string Text { get; set; }

    /// <summary>
    /// Gets or sets the color classes for quiet text marks such as secondary labels.
    /// </summary>
    public required string TextMuted { get; set; }

    /// <summary>
    /// Gets or sets the color classes for prominent text marks such as value labels.
    /// </summary>
    public required string TextPrimary { get; set; }

    /// <summary>
    /// Gets or sets the color classes for text marks drawn on top of a colored fill.
    /// </summary>
    public required string TextInverse { get; set; }

    /// <summary>
    /// Gets or sets the extra classes for an emphasized text mark.
    /// </summary>
    public required string TextStrong { get; set; }

    /// <summary>
    /// Gets or sets the extra classes for a headline text mark such as a gauge's value.
    /// </summary>
    public required string TextLarge { get; set; }

    /// <summary>
    /// Gets or sets the classes that remove the fill from a stroked SVG path.
    /// </summary>
    public required string FillNone { get; set; }

    /// <summary>
    /// Gets or sets the classes for a hairline stroke, used by gridlines and baselines.
    /// </summary>
    public required string StrokeHairline { get; set; }

    /// <summary>
    /// Gets or sets the classes for a standard stroke, used by data lines.
    /// </summary>
    public required string StrokeNormal { get; set; }

    /// <summary>
    /// Gets or sets the classes for a heavy stroke, used by connectors such as a dumbbell's bar.
    /// </summary>
    public required string StrokeThick { get; set; }

    /// <summary>
    /// Gets or sets the classes that draw a surface-colored outline around a filled path, separating touching
    /// shapes such as pie slices. Must match the background the chart sits on.
    /// </summary>
    public required string SurfaceStroke { get; set; }

    /// <summary>
    /// Gets or sets the extra classes for the <see cref="TwBlazor.Components.TwTooltip"/> wrapper around each
    /// data point, including its minimum hit size.
    /// </summary>
    public required string HitWrapper { get; set; }

    /// <summary>
    /// Gets or sets the classes added to the wrapper of a data point that has its own outline, such as a pie
    /// slice. They stop the wrapper's rectangle taking the pointer, so only <see cref="HitShape"/> does.
    /// </summary>
    public string HitWrapperShaped { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the element cut to the outline of a shaped data point. It is the only part
    /// of the data point that takes the pointer, so hovering or tapping anywhere on the shape shows its tooltip.
    /// </summary>
    public string HitShape { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the classes for the focusable element inside each data point.
    /// </summary>
    public required string Hit { get; set; }

    /// <summary>
    /// Gets or sets the classes for the wash shown over a data point while it is hovered or focused.
    /// </summary>
    public required string HitHighlight { get; set; }

    /// <summary>
    /// Gets or sets the classes for the vertical hairline shown through a data point while it is hovered or focused.
    /// </summary>
    public required string Crosshair { get; set; }

    /// <summary>
    /// Gets or sets the classes for the title line of a data point's tooltip.
    /// </summary>
    public required string TooltipTitle { get; set; }

    /// <summary>
    /// Gets or sets the classes for a row of a data point's tooltip.
    /// </summary>
    public required string TooltipRow { get; set; }

    /// <summary>
    /// Gets or sets the classes for the short color key at the start of a tooltip row.
    /// </summary>
    public required string TooltipKey { get; set; }

    /// <summary>
    /// Gets or sets the classes for the label in a tooltip row.
    /// </summary>
    public required string TooltipLabel { get; set; }

    /// <summary>
    /// Gets or sets the classes for the value in a tooltip row.
    /// </summary>
    public required string TooltipValue { get; set; }

    /// <summary>
    /// Gets or sets the classes for the message shown when a chart has no data.
    /// </summary>
    public required string Empty { get; set; }

    /// <summary>
    /// Gets or sets the extra classes for a row header cell in the table view.
    /// </summary>
    public required string TableRowHeader { get; set; }

    /// <summary>
    /// Gets or sets the categorical colors, assigned to series in this fixed order. A series beyond the last
    /// color reuses the last one, so keep charts to this many series or fewer.
    /// </summary>
    /// <remarks>
    /// The order is part of the design: neighboring colors must stay distinguishable for color-blind readers.
    /// </remarks>
    public required IReadOnlyList<TwChartColor> Series { get; set; }

    /// <summary>
    /// Gets or sets the color for a favourable change.
    /// </summary>
    public required TwChartColor Positive { get; set; }

    /// <summary>
    /// Gets or sets the color for an unfavourable change.
    /// </summary>
    public required TwChartColor Negative { get; set; }

    /// <summary>
    /// Gets or sets the color for a neutral summary value.
    /// </summary>
    public required TwChartColor Total { get; set; }

    /// <summary>
    /// Gets or sets the single hue whose opacity shows magnitude, used by heatmaps.
    /// </summary>
    public required TwChartColor Sequential { get; set; }

    /// <summary>
    /// Gets or sets the color for an unfilled track.
    /// </summary>
    public required TwChartColor Track { get; set; }

    /// <summary>
    /// Gets or sets the color for gridlines.
    /// </summary>
    public required TwChartColor Grid { get; set; }

    /// <summary>
    /// Gets or sets the color for baselines and axis lines.
    /// </summary>
    public required TwChartColor Axis { get; set; }

    /// <summary>
    /// Gets or sets the high-contrast ink color, used by target markers.
    /// </summary>
    public required TwChartColor Ink { get; set; }
}
