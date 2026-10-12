// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;
using TwBlazor.Localization;
using TwBlazor.Models;

namespace TwBlazor.Components;

/// <summary>
/// A link between two nodes of a graph chart, by their positions in the list of nodes being drawn.
/// </summary>
/// <param name="Source">The index of the node the link starts at.</param>
/// <param name="Target">The index of the node the link points to.</param>
/// <param name="Value">The weight of the link.</param>
internal readonly record struct ChartGraphEdge(int Source, int Target, double Value);

/// <summary>
/// The shared definition of the charts that draw <see cref="ChartGraphNode"/> values joined by
/// <see cref="ChartGraphLink"/> values. It supplies the table view and the tooltip of each node.
/// </summary>
public abstract class TwGraphChartBase : TwChartBase
{
    /// <summary>
    /// Gets or sets the nodes. Each needs a unique <see cref="ChartGraphNode.Id"/>; repeats are left out.
    /// </summary>
    [Parameter] public IReadOnlyList<ChartGraphNode> Nodes { get; set; } = [];

    /// <summary>
    /// Gets or sets the links between nodes, by node identifier. Links that name a missing node, or join a node
    /// to itself, are left out.
    /// </summary>
    [Parameter] public IReadOnlyList<ChartGraphLink> Links { get; set; } = [];

    /// <summary>
    /// Gets or sets the heading of the column naming where each link starts in the table view.
    /// Defaults to the localized <see cref="TwStrings.Chart_Source"/> text ("Source").
    /// </summary>
    [Parameter] public string? SourceHeader { get; set; }

    private string effectiveSourceHeader => SourceHeader ?? localizer[TwStrings.Chart_Source];

    /// <summary>
    /// Gets or sets the heading of the column naming where each link ends in the table view.
    /// Defaults to the localized <see cref="TwStrings.Chart_Destination"/> text ("Target").
    /// </summary>
    [Parameter] public string? TargetHeader { get; set; }

    private string effectiveTargetHeader => TargetHeader ?? localizer[TwStrings.Chart_Destination];

    /// <summary>
    /// Gets or sets the heading of the value column in the table view. It also labels a link's value in tooltips.
    /// Defaults to the localized <see cref="TwStrings.Chart_Value"/> text ("Value").
    /// </summary>
    [Parameter] public string? ValueHeader { get; set; }

    private protected string effectiveValueHeader => ValueHeader ?? localizer[TwStrings.Chart_Value];

    /// <summary>
    /// Gets whether the value of each link is part of what the chart shows. A chart that only shows which nodes
    /// are joined turns this off, and values are then left out of the tooltips and the table view.
    /// </summary>
    private protected virtual bool hasValues => true;

    /// <summary>
    /// Adds the marks and data points for the graph.
    /// </summary>
    /// <param name="builder">The builder that collects the scene.</param>
    /// <param name="nodes">The nodes to draw. Never empty.</param>
    /// <param name="edges">The links between them.</param>
    private protected abstract void BuildGraph(ChartSceneBuilder builder, IReadOnlyList<ChartGraphNode> nodes, IReadOnlyList<ChartGraphEdge> edges);

    /// <inheritdoc />
    protected internal sealed override void BuildScene(ChartSceneBuilder builder)
    {
        var nodes = Nodes.Where(node => !string.IsNullOrWhiteSpace(node.Id)).DistinctBy(node => node.Id).ToList();
        if (nodes.Count == 0)
        {
            return;
        }

        var indexes = nodes.Select((node, index) => (node.Id, index)).ToDictionary(item => item.Id, item => item.index, StringComparer.Ordinal);
        List<ChartGraphEdge> edges = [.. Links.Where(link => link.Source != link.Target && double.IsFinite(link.Value) && indexes.ContainsKey(link.Source) && indexes.ContainsKey(link.Target))
            .Select(link => new ChartGraphEdge(indexes[link.Source], indexes[link.Target], link.Value))];

        string[] headers = [effectiveSourceHeader, effectiveTargetHeader];
        builder.Table = new ChartTable(hasValues ? [.. headers, effectiveValueHeader] : headers, [.. edges.Select(GetTableRow)]);
        BuildGraph(builder, nodes, edges);

        IReadOnlyList<string> GetTableRow(ChartGraphEdge edge) => hasValues
            ? [nodes[edge.Source].Label, nodes[edge.Target].Label, FormatValue(edge.Value)]
            : [nodes[edge.Source].Label, nodes[edge.Target].Label];
    }

    /// <summary>
    /// Builds the tooltip rows for a node: one for every link that leaves or reaches it, naming the node at the other end.
    /// </summary>
    /// <param name="node">The index of the node.</param>
    /// <param name="nodes">The nodes being drawn.</param>
    /// <param name="edges">The links between them.</param>
    private protected IReadOnlyList<ChartDatumRow> GetLinkRows(int node, IReadOnlyList<ChartGraphNode> nodes, IReadOnlyList<ChartGraphEdge> edges) =>
    [
        .. edges.Where(edge => edge.Source == node).Select(edge => new ChartDatumRow($"→ {nodes[edge.Target].Label}", hasValues ? FormatValue(edge.Value) : string.Empty)),
        .. edges.Where(edge => edge.Target == node).Select(edge => new ChartDatumRow($"← {nodes[edge.Source].Label}", hasValues ? FormatValue(edge.Value) : string.Empty))
    ];

    /// <summary>
    /// Works out which column each node belongs in for a left-to-right layout: at least its own
    /// <see cref="ChartGraphNode.Stage"/>, and always to the right of every node that links to it.
    /// </summary>
    /// <param name="nodes">The nodes being drawn.</param>
    /// <param name="edges">The links between them.</param>
    /// <returns>The zero-based column of each node, with no empty columns.</returns>
    private protected static int[] GetColumns(IReadOnlyList<ChartGraphNode> nodes, IReadOnlyList<ChartGraphEdge> edges)
    {
        var stages = nodes.Select(node => Math.Max(0, node.Stage)).ToArray();

        // A loop of links has no rightmost node, so stop after as many passes as it takes to settle a graph without one.
        for (var pass = 0; pass < nodes.Count; pass++)
        {
            foreach (var edge in edges.Where(edge => stages[edge.Target] <= stages[edge.Source]))
            {
                stages[edge.Target] = Math.Min(nodes.Count - 1, stages[edge.Source] + 1);
            }
        }

        var used = stages.Distinct().Order().ToList();
        return [.. stages.Select(stage => used.IndexOf(stage))];
    }
}

/// <summary>
/// A chord diagram: nodes arranged as arcs around a circle, each as long as the total of its links, with a ribbon
/// across the circle for every link, as thick as the link's value.
/// </summary>
/// <remarks>
/// Each ribbon takes the color of the node it starts at. Nodes are named in the legend, and each arc's tooltip
/// lists its links.
/// </remarks>
public class TwChordDiagramChart : TwGraphChartBase
{
    private const double outerRadius = 48;

    private const double innerRadius = 43;

    private const double ribbonRadius = 42;

    private protected override void BuildGraph(ChartSceneBuilder builder, IReadOnlyList<ChartGraphNode> nodes, IReadOnlyList<ChartGraphEdge> edges)
    {
        builder.Shape = ChartShape.Square;

        // Without any weight to share out, every node gets an equal arc so the chart still names them.
        var weights = nodes.Select((_, index) => edges.Where(edge => edge.Source == index || edge.Target == index).Sum(edge => Math.Abs(edge.Value))).ToList();
        var isWeighted = weights.Sum() > 0;
        var arcs = nodes.Select((_, index) => index).Where(index => !isWeighted || weights[index] > 0).ToList();
        var gap = arcs.Count > 1 ? Math.Min(4, 90.0 / arcs.Count) : 0;
        var degreesPerUnit = (360 - gap * arcs.Count) / (isWeighted ? weights.Sum() : arcs.Count);

        var starts = new double[nodes.Count];
        var spans = new double[nodes.Count];
        double angle = 0;
        foreach (var index in arcs)
        {
            starts[index] = angle;
            spans[index] = (isWeighted ? weights[index] : 1) * degreesPerUnit;
            angle += spans[index] + gap;
        }

        var used = (double[])starts.Clone();
        foreach (var edge in edges.Where(edge => edge.Value != 0))
        {
            var span = Math.Abs(edge.Value) * degreesPerUnit;
            var (from, to) = (used[edge.Source], used[edge.Target]);
            used[edge.Source] += span;
            used[edge.Target] += span;

            builder.Path(GetRibbon(from, to, span), ChartMetrics.Cycle(edge.Source), filled: true, opacity: 0.55, outline: true,
                title: $"{nodes[edge.Source].Label} → {nodes[edge.Target].Label}: {FormatValue(edge.Value)}");
        }

        foreach (var index in arcs)
        {
            var color = ChartMetrics.Cycle(index);
            var arc = new ChartSector(50, 50, innerRadius, outerRadius, starts[index], starts[index] + spans[index]);

            builder.Path(ChartGeometry.Sector(arc), color, filled: true, outline: true);
            builder.Legend(nodes[index].Label, color);
            builder.SectorDatum(arc, nodes[index].Label, [new ChartDatumRow(localizer[TwStrings.Chart_Total], FormatValue(weights[index]), color), .. GetLinkRows(index, nodes, edges)]);
        }
    }

    private static string GetRibbon(double from, double to, double span)
    {
        var (fromStart, fromEnd) = (ChartGeometry.Polar(50, 50, ribbonRadius, from), ChartGeometry.Polar(50, 50, ribbonRadius, from + span));
        var (toStart, toEnd) = (ChartGeometry.Polar(50, 50, ribbonRadius, to), ChartGeometry.Polar(50, 50, ribbonRadius, to + span));

        // Each side bends two thirds of the way to the middle, which is the curve a single control point at the center would draw.
        static (double X, double Y) Bend((double X, double Y) point) => (point.X + (50 - point.X) * 2 / 3, point.Y + (50 - point.Y) * 2 / 3);

        return new ChartPathBuilder()
            .MoveTo(fromStart.X, fromStart.Y)
            .ArcTo(ribbonRadius, span > 180, true, fromEnd.X, fromEnd.Y)
            .CurveTo(Bend(fromEnd).X, Bend(fromEnd).Y, Bend(toStart).X, Bend(toStart).Y, toStart.X, toStart.Y)
            .ArcTo(ribbonRadius, span > 180, true, toEnd.X, toEnd.Y)
            .CurveTo(Bend(toEnd).X, Bend(toEnd).Y, Bend(fromStart).X, Bend(fromStart).Y, fromStart.X, fromStart.Y)
            .Close()
            .ToString();
    }
}

/// <summary>
/// An arc diagram: nodes set out along a line in the order given, with an arc above the line for every link, the
/// arc reaching higher the further apart its two nodes are.
/// </summary>
/// <remarks>
/// Order the nodes so that linked ones sit close together and the short arcs show the clusters. Each node's
/// tooltip lists its links.
/// </remarks>
public class TwArcDiagramChart : TwGraphChartBase
{
    private const double baseline = 95;

    // The longest arc stops short of the top of the plot.
    private const double maxHeight = 88;

    private protected override void BuildGraph(ChartSceneBuilder builder, IReadOnlyList<ChartGraphNode> nodes, IReadOnlyList<ChartGraphEdge> edges)
    {
        var band = new ChartBand(nodes.Count);
        var color = ChartColor.Series(0);
        var longest = edges.Select(edge => Math.Abs(edge.Target - edge.Source)).DefaultIfEmpty(1).Max();
        builder.XAxis = new ChartAxis([.. nodes.Select((node, index) => new ChartTick(band.Center(index), node.Label))]);

        foreach (var edge in edges)
        {
            var (from, to) = (band.Center(edge.Source), band.Center(edge.Target));

            // A curve rises three quarters of the way to its control points, so they sit a third higher than the arc should reach.
            var control = baseline - maxHeight * Math.Abs(edge.Target - edge.Source) / longest * 4 / 3;
            builder.Path(new ChartPathBuilder().MoveTo(from, baseline).CurveTo(from, control, to, control, to, baseline).ToString(), color, opacity: 0.5,
                title: $"{nodes[edge.Source].Label} → {nodes[edge.Target].Label}: {FormatValue(edge.Value)}");
        }

        for (var i = 0; i < nodes.Count; i++)
        {
            builder.Dot(band.Center(i), baseline, color, 10);
            builder.Datum(band.Center(i), baseline, 0, 0, nodes[i].Label, GetLinkRows(i, nodes, edges), ChartHover.None);
        }
    }
}

/// <summary>
/// A Sankey diagram: nodes in columns joined by ribbons as thick as the flow between them, for showing how a
/// quantity divides and recombines as it moves through stages.
/// </summary>
/// <remarks>
/// A node sits to the right of every node that flows into it; set <see cref="ChartGraphNode.Stage"/> to move one
/// further right. Each node is as tall as the larger of what flows in and out of it. Links without a positive
/// value are left out.
/// </remarks>
public class TwSankeyChart : TwGraphChartBase
{
    private const double nodeWidth = 2.5;

    private const double nodeGap = 4;

    // A node with nothing flowing through it is still drawn, this tall.
    private const double minNodeHeight = 1.5;

    private const int curveSteps = 12;

    private protected override void BuildGraph(ChartSceneBuilder builder, IReadOnlyList<ChartGraphNode> nodes, IReadOnlyList<ChartGraphEdge> edges)
    {
        List<ChartGraphEdge> flows = [.. edges.Where(edge => edge.Value > 0)];
        var columns = GetColumns(nodes, flows);
        var columnCount = columns.Max() + 1;
        var sizes = nodes.Select((_, index) => Math.Max(flows.Where(flow => flow.Source == index).Sum(flow => flow.Value), flows.Where(flow => flow.Target == index).Sum(flow => flow.Value))).ToList();
        var members = Enumerable.Range(0, columnCount).Select(column => Enumerable.Range(0, nodes.Count).Where(index => columns[index] == column).ToList()).ToList();

        // One scale for every column, set by the column with the least room to spare.
        var scale = members.Where(column => column.Sum(index => sizes[index]) > 0)
            .Select(column => (100 - nodeGap * (column.Count - 1) - minNodeHeight * column.Count) / column.Sum(index => sizes[index]))
            .DefaultIfEmpty(0).Min();

        var lefts = new double[nodes.Count];
        var tops = new double[nodes.Count];
        var heights = sizes.Select(size => Math.Max(minNodeHeight, size * scale)).ToList();
        for (var column = 0; column < columnCount; column++)
        {
            var top = (100 - members[column].Sum(index => heights[index]) - nodeGap * (members[column].Count - 1)) / 2;
            foreach (var index in members[column])
            {
                lefts[index] = columnCount == 1 ? 50 - nodeWidth / 2 : column * (100 - nodeWidth) / (columnCount - 1);
                tops[index] = top;
                top += heights[index] + nodeGap;
            }
        }

        // Ribbons leave and arrive in the order of the nodes at their far ends, so they cross as little as possible.
        var leaving = (double[])tops.Clone();
        var arriving = (double[])tops.Clone();
        var starts = new double[flows.Count];
        var ends = new double[flows.Count];
        foreach (var index in Enumerable.Range(0, flows.Count).OrderBy(index => tops[flows[index].Target]))
        {
            starts[index] = leaving[flows[index].Source];
            leaving[flows[index].Source] += flows[index].Value * scale;
        }

        foreach (var index in Enumerable.Range(0, flows.Count).OrderBy(index => tops[flows[index].Source]))
        {
            ends[index] = arriving[flows[index].Target];
            arriving[flows[index].Target] += flows[index].Value * scale;
        }

        List<(List<(double X, double Y)> Outline, (double X, double Y) Anchor, string Title, ChartDatumRow Row)> ribbons = [];
        for (var i = 0; i < flows.Count; i++)
        {
            var flow = flows[i];
            var color = ChartMetrics.Cycle(flow.Source);
            var thickness = flow.Value * scale;
            var upper = GetCurve(lefts[flow.Source] + nodeWidth, starts[i], lefts[flow.Target], ends[i]);
            List<(double X, double Y)> outline = [.. upper, .. Enumerable.Reverse(upper).Select(point => (point.X, point.Y + thickness))];

            builder.Path(ChartGeometry.Line(outline, close: true), color, filled: true, opacity: 0.4);
            ribbons.Add((outline, upper[curveSteps / 2], $"{nodes[flow.Source].Label} → {nodes[flow.Target].Label}", new ChartDatumRow(effectiveValueHeader, FormatValue(flow.Value), color)));
        }

        for (var i = 0; i < nodes.Count; i++)
        {
            var color = ChartMetrics.Cycle(i);
            var isLast = columnCount > 1 && columns[i] == columnCount - 1;
            var middle = tops[i] + heights[i] / 2;

            builder.Add(new ChartRect(lefts[i], tops[i], nodeWidth, heights[i], color, ChartCorner.All, Gap: false));
            builder.Add(new ChartText(isLast ? lefts[i] : lefts[i] + nodeWidth, middle, nodes[i].Label, isLast ? ChartAnchor.End : ChartAnchor.Start, Tone: ChartTextTone.Primary, OffsetX: isLast ? -6 : 6, Strong: true));
            builder.Datum(lefts[i] + nodeWidth / 2, middle, nodeWidth, heights[i], nodes[i].Label,
                [new ChartDatumRow(localizer[TwStrings.Chart_Total], FormatValue(sizes[i]), color), .. GetLinkRows(i, nodes, flows)]);
        }

        foreach (var (outline, anchor, title, row) in ribbons)
        {
            builder.ShapeDatum(outline, title, [row], anchor);
        }
    }

    /// <summary>
    /// Traces the S-shaped edge of a ribbon from one node to another as a run of points.
    /// </summary>
    private static List<(double X, double Y)> GetCurve(double fromX, double fromY, double toX, double toY) =>
    [
        .. Enumerable.Range(0, curveSteps + 1).Select(step =>
        {
            // A cubic curve whose control points sit level with each end, halfway across.
            var t = step / (double)curveSteps;
            var (start, bend, end) = ((1 - t) * (1 - t) * (1 - t), 3 * (1 - t) * t, t * t * t);
            return (start * fromX + bend * (fromX + toX) / 2 + end * toX, (start + bend * (1 - t)) * fromY + (bend * t + end) * toY);
        })
    ];
}

/// <summary>
/// A network diagram: nodes drawn as dots and joined by a line for every link, arranged so that linked nodes sit
/// close together and unrelated ones drift apart.
/// </summary>
/// <remarks>
/// The arrangement is worked out from the links alone and is the same every time for the same data. Each node's
/// tooltip lists its links.
/// </remarks>
public class TwNetworkDiagramChart : TwGraphChartBase
{
    private const int iterations = 200;

    private protected override void BuildGraph(ChartSceneBuilder builder, IReadOnlyList<ChartGraphNode> nodes, IReadOnlyList<ChartGraphEdge> edges)
    {
        var points = Arrange(nodes.Count, edges);
        var color = ChartColor.Series(0);

        foreach (var edge in edges)
        {
            builder.Line(points[edge.Source].X, points[edge.Source].Y, points[edge.Target].X, points[edge.Target].Y, ChartColor.Axis, ChartStroke.Normal);
        }

        for (var i = 0; i < nodes.Count; i++)
        {
            var (x, y) = points[i];
            var anchor = x < 20 ? ChartAnchor.Start : x > 80 ? ChartAnchor.End : ChartAnchor.Middle;

            builder.Dot(x, y, color, 12);
            builder.Add(new ChartText(x, y, nodes[i].Label, anchor, ChartBaseline.Bottom, ChartTextTone.Primary, OffsetY: -10, Strong: true, MaxWidth: 30));
            builder.Datum(x, y, 0, 0, nodes[i].Label, GetLinkRows(i, nodes, edges), ChartHover.None);
        }
    }

    /// <summary>
    /// Positions the nodes with a force-directed layout: every pair of nodes pushes apart, every link pulls its
    /// two nodes together, and each step moves the nodes a little less than the last until they settle.
    /// </summary>
    private static (double X, double Y)[] Arrange(int count, IReadOnlyList<ChartGraphEdge> edges)
    {
        // Starting on a circle, not at random, is what makes the result repeatable.
        var points = Enumerable.Range(0, count).Select(index => ChartGeometry.Polar(0, 0, 0.5, index * 360.0 / count)).ToArray();
        var ideal = 1 / Math.Sqrt(count);

        for (var iteration = 0; iteration < iterations; iteration++)
        {
            var moves = new (double X, double Y)[count];
            void Push(int node, double x, double y) => moves[node] = (moves[node].X + x, moves[node].Y + y);

            for (var first = 0; first < count; first++)
            {
                // A gentle pull to the middle keeps unlinked nodes from flying off.
                Push(first, -points[first].X * 0.1, -points[first].Y * 0.1);

                for (var second = first + 1; second < count; second++)
                {
                    var (dx, dy) = (points[first].X - points[second].X, points[first].Y - points[second].Y);
                    var distance = Math.Max(0.01, Math.Sqrt(dx * dx + dy * dy));
                    var force = ideal * ideal / distance;
                    Push(first, dx / distance * force, dy / distance * force);
                    Push(second, -dx / distance * force, -dy / distance * force);
                }
            }

            foreach (var edge in edges)
            {
                var (dx, dy) = (points[edge.Target].X - points[edge.Source].X, points[edge.Target].Y - points[edge.Source].Y);
                var distance = Math.Sqrt(dx * dx + dy * dy);
                Push(edge.Source, dx * distance / ideal, dy * distance / ideal);
                Push(edge.Target, -dx * distance / ideal, -dy * distance / ideal);
            }

            var limit = 0.1 * (1 - iteration / (double)iterations);
            for (var index = 0; index < count; index++)
            {
                var length = Math.Sqrt(moves[index].X * moves[index].X + moves[index].Y * moves[index].Y);
                if (length > 0)
                {
                    points[index] = (points[index].X + moves[index].X / length * Math.Min(length, limit), points[index].Y + moves[index].Y / length * Math.Min(length, limit));
                }
            }
        }

        // Stretch the result to fill the plot, inside a margin that leaves room for the dots and the names above them.
        var (left, right) = (points.Min(point => point.X), points.Max(point => point.X));
        var (top, bottom) = (points.Min(point => point.Y), points.Max(point => point.Y));
        return [.. points.Select(point => (
            right > left ? 8 + (point.X - left) / (right - left) * 84 : 50,
            bottom > top ? 16 + (point.Y - top) / (bottom - top) * 74 : 50))];
    }
}

/// <summary>
/// A flowchart: the steps of a process drawn as boxes in columns from left to right, with an arrow for every
/// link from one step to the next.
/// </summary>
/// <remarks>
/// A step sits to the right of every step that leads to it; set <see cref="ChartGraphNode.Stage"/> to move one
/// further right. Steps that share a column are stacked in the order given. Link values are not shown.
/// </remarks>
public class TwFlowchart : TwGraphChartBase
{
    // The length and half-width of an arrowhead, in pixels.
    private const double headLength = 9;

    private const double headWidth = 4;

    /// <inheritdoc />
    private protected override bool hasValues => false;

    private protected override void BuildGraph(ChartSceneBuilder builder, IReadOnlyList<ChartGraphNode> nodes, IReadOnlyList<ChartGraphEdge> edges)
    {
        var columns = GetColumns(nodes, edges);
        var columnBand = new ChartBand(columns.Max() + 1);
        var members = Enumerable.Range(0, columnBand.Count).Select(column => Enumerable.Range(0, nodes.Count).Where(index => columns[index] == column).ToList()).ToList();
        var width = Math.Min(columnBand.Step * 0.6, 24);
        var height = Math.Min(100.0 / members.Max(column => column.Count) * 0.5, 16);
        var color = ChartColor.Series(0);

        var centers = new (double X, double Y)[nodes.Count];
        for (var column = 0; column < members.Count; column++)
        {
            var rows = new ChartBand(members[column].Count);
            for (var row = 0; row < members[column].Count; row++)
            {
                centers[members[column][row]] = (columnBand.Center(column), rows.Center(row));
            }
        }

        // Where the straight line between two box centers leaves a box, measured from its center.
        (double X, double Y) Edge((double X, double Y) from, (double X, double Y) toward)
        {
            var (dx, dy) = (toward.X - from.X, toward.Y - from.Y);
            var reach = Math.Min(dx == 0 ? double.MaxValue : width / 2 / Math.Abs(dx), dy == 0 ? double.MaxValue : height / 2 / Math.Abs(dy));
            return (from.X + dx * reach, from.Y + dy * reach);
        }

        // Plot percentages are wider than they are tall, so the arrowhead is shaped in pixels and converted back.
        var pixelsY = ChartMetrics.PlotHeight(effectiveHeight) / 100;
        var pixelsX = pixelsY * ChartMetrics.WideAspect;
        foreach (var edge in edges)
        {
            var start = Edge(centers[edge.Source], centers[edge.Target]);
            var tip = Edge(centers[edge.Target], centers[edge.Source]);
            var (dx, dy) = ((tip.X - start.X) * pixelsX, (tip.Y - start.Y) * pixelsY);
            var length = Math.Sqrt(dx * dx + dy * dy);
            var (alongX, alongY) = (dx / length * headLength / pixelsX, dy / length * headLength / pixelsY);
            var (acrossX, acrossY) = (-dy / length * headWidth / pixelsX, dx / length * headWidth / pixelsY);
            builder.Line(start.X, start.Y, tip.X - alongX, tip.Y - alongY, ChartColor.Ink, ChartStroke.Normal);
            builder.Path(ChartGeometry.Line([tip, (tip.X - alongX + acrossX, tip.Y - alongY + acrossY), (tip.X - alongX - acrossX, tip.Y - alongY - acrossY)], close: true), ChartColor.Ink, filled: true);
        }

        for (var i = 0; i < nodes.Count; i++)
        {
            var (x, y) = centers[i];
            builder.Add(new ChartRect(x - width / 2, y - height / 2, width, height, color, ChartCorner.All, Gap: false));
            builder.Add(new ChartText(x, y, nodes[i].Label, Tone: ChartTextTone.Inverse, Strong: true, MaxWidth: width * 0.9));
            builder.Datum(x, y, width, height, nodes[i].Label, GetLinkRows(i, nodes, edges));
        }
    }
}