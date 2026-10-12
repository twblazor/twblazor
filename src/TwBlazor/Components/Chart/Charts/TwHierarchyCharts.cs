// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Charting;
using TwBlazor.Localization;
using TwBlazor.Models;

namespace TwBlazor.Components;

/// <summary>
/// A Marimekko chart: a column for each category whose width shows the category's total, split into segments
/// showing each series' share of it, so both the size of every category and its make-up are visible at once.
/// </summary>
/// <remarks>
/// Missing and negative values count as zero, because they have no share to draw.
/// </remarks>
public class TwMarimekkoChart : TwCategoryChartBase
{
    // A column narrower than this has no room for its name underneath.
    private const double minLabelWidth = 6;

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder, IReadOnlyList<ChartSeriesSlot> series)
    {
        var totals = Enumerable.Range(0, Categories.Count).Select(category => series.Sum(slot => GetShareValue(slot, category))).ToList();
        var grandTotal = totals.Sum();
        if (grandTotal <= 0)
        {
            return;
        }

        builder.ValueAxis([.. Enumerable.Range(0, 5).Select(quarter => new ChartTick(quarter * 25, FormatShare(quarter, 4)))]);

        List<ChartTick> ticks = [];
        double x = 0;
        for (var category = 0; category < Categories.Count; category++)
        {
            var width = totals[category] / grandTotal * 100;
            if (width <= 0)
            {
                continue;
            }

            double y = 100;
            foreach (var slot in series)
            {
                var value = GetShareValue(slot, category);
                if (value <= 0)
                {
                    continue;
                }

                var height = value / totals[category] * 100;
                y -= height;
                builder.Rect(x, y, width, height, slot.Color);

                if (width >= 10 && height >= 10)
                {
                    builder.Text(x + width / 2, y + height / 2, FormatShare(value, totals[category]), tone: ChartTextTone.Inverse);
                }
            }

            if (width >= minLabelWidth)
            {
                ticks.Add(new ChartTick(x + width / 2, Categories[category]));
            }

            builder.Datum(x + width / 2, 50, width, 100, Categories[category],
                [.. GetRows(series, category), new ChartDatumRow(localizer[TwStrings.Chart_Total], FormatValue(totals[category])), new ChartDatumRow(localizer[TwStrings.Chart_Share], FormatShare(totals[category], grandTotal))]);
            x += width;
        }

        builder.XAxis = new ChartAxis(ticks);
    }

    private static double GetShareValue(ChartSeriesSlot slot, int category) => slot.Value(category) is { } value && double.IsFinite(value) ? Math.Max(0, value) : 0;

    private static string FormatShare(double value, double total) => (value / total).ToString("0.#%");
}

/// <summary>
/// The shared definition of the charts that draw a tree of <see cref="ChartHierarchyNode"/> values.
/// </summary>
public abstract class TwHierarchyChartBase : TwChartBase
{
    /// <summary>
    /// Gets or sets the top-level nodes of the tree.
    /// </summary>
    [Parameter] public IReadOnlyList<ChartHierarchyNode> Roots { get; set; } = [];

    /// <summary>
    /// Gets or sets the heading of the node column in the table view.
    /// Defaults to the localized <see cref="TwStrings.Chart_Category"/> text ("Category").
    /// </summary>
    [Parameter] public string? LabelHeader { get; set; }

    private protected string effectiveLabelHeader => LabelHeader ?? localizer[TwStrings.Chart_Category];

    /// <summary>
    /// Gets or sets the heading of the value column in the table view. It also labels the value in tooltips.
    /// Defaults to the localized <see cref="TwStrings.Chart_Value"/> text ("Value").
    /// </summary>
    [Parameter] public string? ValueHeader { get; set; }

    private protected string effectiveValueHeader => ValueHeader ?? localizer[TwStrings.Chart_Value];

    /// <summary>
    /// Gets the value of a node: its own when it is a leaf, otherwise the sum of its leaves. Negative values count as zero.
    /// </summary>
    /// <param name="node">The node.</param>
    private protected static double GetTotal(ChartHierarchyNode node) => node.Children is { Count: > 0 } children
        ? children.Sum(GetTotal)
        : double.IsFinite(node.Value) ? Math.Max(0, node.Value) : 0;

    /// <summary>
    /// Gets the nodes that have a value to draw.
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    private protected static List<ChartHierarchyNode> GetSized(IReadOnlyList<ChartHierarchyNode>? nodes) => [.. (nodes ?? []).Where(node => GetTotal(node) > 0)];

    /// <summary>
    /// Gets the name of a node with the names of its ancestors in front, so that nodes sharing a name stay distinct.
    /// </summary>
    /// <param name="parent">The path of the parent, or an empty string for a top-level node.</param>
    /// <param name="node">The node.</param>
    private protected static string GetPath(string parent, ChartHierarchyNode node) => parent.Length == 0 ? node.Label : $"{parent} / {node.Label}";

    /// <summary>
    /// Formats a share of a whole as a percentage.
    /// </summary>
    /// <param name="value">The part.</param>
    /// <param name="total">The whole.</param>
    private protected static string FormatShare(double value, double total) => total == 0 ? "0%" : (value / total).ToString("0.#%");

    /// <summary>
    /// Adds a legend entry for each top-level node when there is more than one, and returns their colors.
    /// </summary>
    /// <param name="builder">The builder that collects the scene.</param>
    /// <param name="roots">The top-level nodes being drawn.</param>
    private protected static List<ChartColor> AddRootLegend(ChartSceneBuilder builder, IReadOnlyList<ChartHierarchyNode> roots)
    {
        List<ChartColor> colors = [.. roots.Select((_, index) => ChartMetrics.Cycle(index))];
        for (var i = 0; i < roots.Count && roots.Count > 1; i++)
        {
            builder.Legend(roots[i].Label, colors[i]);
        }

        return colors;
    }
}

/// <summary>
/// A circular treemap (circle packing): a circle for every leaf of a tree, with an area that follows its value,
/// packed inside a circle for its parent, so the grouping is visible as well as the sizes.
/// </summary>
/// <remarks>
/// Circles leave gaps, so sizes are harder to compare than on <see cref="TwConvexTreemapChart"/>; in return the
/// nesting is easier to see. Nodes with no value are left out.
/// </remarks>
public class TwCircularTreemapChart : TwHierarchyChartBase
{
    // Circles are packed a little larger than they are drawn, which leaves a gap between neighbors.
    private const double spacing = 1.08;

    // The smallest circle, in pixels across, that has room for its name.
    private const double minLabelSize = 36;

    /// <inheritdoc />
    protected internal override void BuildScene(ChartSceneBuilder builder)
    {
        var roots = GetSized(Roots);
        if (roots.Count == 0)
        {
            return;
        }

        builder.Shape = ChartShape.Square;
        var colors = AddRootLegend(builder, roots);
        var total = roots.Sum(GetTotal);
        var packed = Pack(new ChartHierarchyNode(string.Empty, Children: roots));
        var scale = 48 / packed.Radius;
        var pixels = ChartMetrics.SquareSize(effectiveHeight) / 100;
        List<IReadOnlyList<string>> rows = [];

        void Draw(PackedCircle circle, double centerX, double centerY, string parent, ChartColor color)
        {
            var x = centerX + circle.X * scale;
            var y = centerY + circle.Y * scale;
            var radius = circle.Radius / spacing * scale;
            var path = GetPath(parent, circle.Node);

            if (circle.Children.Count > 0)
            {
                builder.Path(ChartGeometry.Circle(x, y, radius), ChartColor.Track, filled: true, outline: true, title: path);
                circle.Children.ForEach(child => Draw(child, x, y, path, color));
                return;
            }

            var value = GetTotal(circle.Node);
            builder.Path(ChartGeometry.Circle(x, y, radius), color, filled: true, outline: true);

            if (radius * 2 * pixels >= minLabelSize)
            {
                builder.Add(new ChartText(x, y, circle.Node.Label, Tone: ChartTextTone.Inverse, Strong: true, MaxWidth: radius * 1.7));
            }

            builder.SectorDatum(new ChartSector(x, y, 0, radius, 0, 360), path,
                [new ChartDatumRow(effectiveValueHeader, FormatValue(value), color), new ChartDatumRow(localizer[TwStrings.Chart_Share], FormatShare(value, total))], anchorAngle: 0);
            rows.Add([path, FormatValue(value)]);
        }

        for (var i = 0; i < packed.Children.Count; i++)
        {
            Draw(packed.Children[i], 50, 50, string.Empty, colors[i]);
        }

        builder.Table = new ChartTable([effectiveLabelHeader, effectiveValueHeader], rows);
    }

    private static PackedCircle Pack(ChartHierarchyNode node)
    {
        var children = GetSized(node.Children).Select(Pack).ToList();
        if (children.Count == 0)
        {
            return new PackedCircle(node, []) { Radius = Math.Sqrt(GetTotal(node)) * spacing };
        }

        // Largest first, each circle goes in the free spot nearest the middle that touches two already placed.
        List<PackedCircle> placed = [];
        foreach (var circle in children.OrderByDescending(child => child.Radius))
        {
            if (placed.Count == 1)
            {
                circle.X = placed[0].Radius + circle.Radius;
            }
            else if (placed.Count > 1)
            {
                (circle.X, circle.Y) = placed.SelectMany((first, index) => placed.Skip(index + 1).SelectMany(second => GetTouching(first, second, circle.Radius)))
                    .Where(spot => placed.All(other => Math.Sqrt(Math.Pow(other.X - spot.X, 2) + Math.Pow(other.Y - spot.Y, 2)) >= other.Radius + circle.Radius - 1e-6))
                    .DefaultIfEmpty((X: placed.Max(other => other.X + other.Radius) + circle.Radius, Y: 0.0))
                    .MinBy(spot => spot.X * spot.X + spot.Y * spot.Y);
            }

            placed.Add(circle);
        }

        // Walk the center toward whichever circle reaches furthest until the enclosing circle stops shrinking.
        var (x, y) = (placed.Average(circle => circle.X), placed.Average(circle => circle.Y));
        for (var step = 1; step <= 200; step++)
        {
            var furthest = placed.MaxBy(circle => GetReach(circle, x, y))!;
            x += (furthest.X - x) / (step + 1);
            y += (furthest.Y - y) / (step + 1);
        }

        var radius = placed.Max(circle => GetReach(circle, x, y));
        placed.ForEach(circle => (circle.X, circle.Y) = (circle.X - x, circle.Y - y));
        return new PackedCircle(node, children) { Radius = (children.Count == 1 ? radius * 1.1 : radius) * spacing };
    }

    private static double GetReach(PackedCircle circle, double x, double y) => Math.Sqrt(Math.Pow(circle.X - x, 2) + Math.Pow(circle.Y - y, 2)) + circle.Radius;

    /// <summary>
    /// Gets the two positions at which a circle of a given radius touches both of two others, if it can.
    /// </summary>
    private static IEnumerable<(double X, double Y)> GetTouching(PackedCircle first, PackedCircle second, double radius)
    {
        var (toFirst, toSecond) = (first.Radius + radius, second.Radius + radius);
        var (dx, dy) = (second.X - first.X, second.Y - first.Y);
        var distance = Math.Sqrt(dx * dx + dy * dy);
        if (distance <= 0 || distance > toFirst + toSecond)
        {
            yield break;
        }

        var along = (toFirst * toFirst - toSecond * toSecond + distance * distance) / (2 * distance);
        var across = Math.Sqrt(Math.Max(0, toFirst * toFirst - along * along));
        var (baseX, baseY) = (first.X + dx / distance * along, first.Y + dy / distance * along);
        yield return (baseX - dy / distance * across, baseY + dx / distance * across);
        yield return (baseX + dy / distance * across, baseY - dx / distance * across);
    }

    /// <summary>
    /// A node's circle, positioned relative to the center of its parent's.
    /// </summary>
    private sealed class PackedCircle(ChartHierarchyNode node, List<PackedCircle> children)
    {
        public ChartHierarchyNode Node { get; } = node;

        public List<PackedCircle> Children { get; } = children;

        public double X { get; set; }

        public double Y { get; set; }

        public double Radius { get; init; }
    }
}

/// <summary>
/// A convex treemap: the leaves of a tree drawn as tiles with cut corners that fill the plot, each with an area
/// that follows its value and grouped with the other leaves of its parent.
/// </summary>
/// <remarks>
/// Tiles are laid out largest first and kept as close to square as possible. Nodes with no value are left out.
/// </remarks>
public class TwConvexTreemapChart : TwHierarchyChartBase
{
    // The gap around the tiles of one parent, which sets them apart from the next group.
    private const double groupInset = 0.8;

    /// <inheritdoc />
    protected internal override void BuildScene(ChartSceneBuilder builder)
    {
        var roots = GetSized(Roots);
        if (roots.Count == 0)
        {
            return;
        }

        builder.Shape = ChartShape.Square;
        var colors = AddRootLegend(builder, roots);
        var total = roots.Sum(GetTotal);
        List<IReadOnlyList<string>> rows = [];

        void Tile(List<ChartHierarchyNode> nodes, (double X, double Y, double Width, double Height) area, string parent, ChartColor? groupColor)
        {
            var ordered = nodes.Select((node, index) => (Node: node, Color: groupColor ?? colors[index])).OrderByDescending(item => GetTotal(item.Node)).ToList();
            var tiles = Squarify([.. ordered.Select(item => GetTotal(item.Node))], area);

            for (var i = 0; i < ordered.Count; i++)
            {
                var (x, y, width, height) = tiles[i];
                var (node, color) = ordered[i];
                var path = GetPath(parent, node);

                if (GetSized(node.Children) is { Count: > 0 } children)
                {
                    var inset = Math.Min(groupInset, Math.Min(width, height) / 4);
                    Tile(children, (x + inset, y + inset, width - inset * 2, height - inset * 2), path, color);
                    continue;
                }

                var value = GetTotal(node);
                var cut = Math.Min(width, height) * 0.18;
                (double X, double Y)[] corners =
                [
                    (x + cut, y), (x + width - cut, y), (x + width, y + cut), (x + width, y + height - cut),
                    (x + width - cut, y + height), (x + cut, y + height), (x, y + height - cut), (x, y + cut)
                ];

                builder.Path(ChartGeometry.Line(corners, close: true), color, filled: true, outline: true);

                if (width >= 14 && height >= 10)
                {
                    builder.Add(new ChartText(x + width / 2, y + height / 2, node.Label, Tone: ChartTextTone.Inverse, Strong: true, MaxWidth: width - cut * 2));
                }

                builder.ShapeDatum(corners, path, [new ChartDatumRow(effectiveValueHeader, FormatValue(value), color), new ChartDatumRow(localizer[TwStrings.Chart_Share], FormatShare(value, total))]);
                rows.Add([path, FormatValue(value)]);
            }
        }

        Tile(roots, (0, 0, 100, 100), string.Empty, null);
        builder.Table = new ChartTable([effectiveLabelHeader, effectiveValueHeader], rows);
    }

    /// <summary>
    /// Divides an area into one rectangle per value, sized by the value and kept as close to square as possible.
    /// </summary>
    /// <param name="values">The values, largest first.</param>
    /// <param name="area">The area to divide.</param>
    private static List<(double X, double Y, double Width, double Height)> Squarify(List<double> values, (double X, double Y, double Width, double Height) area)
    {
        var (x, y, width, height) = area;
        var areaPerUnit = width * height / values.Sum();
        List<(double X, double Y, double Width, double Height)> tiles = [];

        while (tiles.Count < values.Count)
        {
            var shortSide = Math.Min(width, height);
            List<double> row = [values[tiles.Count] * areaPerUnit];

            while (tiles.Count + row.Count < values.Count)
            {
                var candidate = values[tiles.Count + row.Count] * areaPerUnit;
                if (WorstRatio([.. row, candidate], shortSide) > WorstRatio(row, shortSide))
                {
                    break;
                }

                row.Add(candidate);
            }

            var depth = row.Sum() / shortSide;
            double offset = 0;

            foreach (var length in row.Select(tile => tile / depth))
            {
                tiles.Add(width >= height ? (x, y + offset, depth, length) : (x + offset, y, length, depth));
                offset += length;
            }

            if (width >= height)
            {
                x += depth;
                width -= depth;
            }
            else
            {
                y += depth;
                height -= depth;
            }
        }

        return tiles;
    }

    private static double WorstRatio(List<double> row, double side)
    {
        var sum = row.Sum();
        return Math.Max(side * side * row.Max() / (sum * sum), sum * sum / (side * side * row.Min()));
    }
}

/// <summary>
/// A dendrogram: a tree drawn from its root at the top down to its leaves along the bottom, with every branch
/// joined to its parent by a bracket, for showing how items group into clusters.
/// </summary>
/// <remarks>
/// Leaves are named along the axis under the plot and line up whatever their depth. Branches are named beside
/// the point where they split.
/// </remarks>
public class TwDendrogramChart : TwHierarchyChartBase
{
    private const double top = 6;

    private const double bottom = 95;

    /// <inheritdoc />
    protected internal override void BuildScene(ChartSceneBuilder builder)
    {
        if (Roots.Count == 0)
        {
            return;
        }

        var leaves = new ChartBand(Roots.Sum(CountLeaves));
        var depth = Roots.Max(GetDepth);
        var color = ChartColor.Series(0);
        List<ChartTick> ticks = [];
        List<(double X, double Y, string Path, string Value)> nodes = [];

        (double X, double Y) Place(ChartHierarchyNode node, int level, string parent)
        {
            var path = GetPath(parent, node);
            var index = nodes.Count;
            nodes.Add(default);

            (double X, double Y) position;
            if (node.Children is { Count: > 0 } children)
            {
                List<(double X, double Y)> branches = [.. children.Select(child => Place(child, level + 1, path))];
                position = ((branches[0].X + branches[^1].X) / 2, top + (bottom - top) * level / depth);

                builder.Line(branches[0].X, position.Y, branches[^1].X, position.Y, ChartColor.Axis, ChartStroke.Normal);
                branches.ForEach(branch => builder.Line(branch.X, position.Y, branch.X, branch.Y, ChartColor.Axis, ChartStroke.Normal));

                // Beside the split, not above it: the bracket from the parent arrives from straight above.
                var onRight = position.X > 70;
                builder.Add(new ChartText(position.X, position.Y, node.Label, onRight ? ChartAnchor.End : ChartAnchor.Start, ChartBaseline.Bottom, ChartTextTone.Primary, onRight ? -8 : 8, -6, Strong: true, MaxWidth: 40));
            }
            else
            {
                position = (leaves.Center(ticks.Count), depth == 0 ? 50 : bottom);
                ticks.Add(new ChartTick(position.X, node.Label));
            }

            nodes[index] = (position.X, position.Y, path, FormatValue(GetTotal(node)));
            return position;
        }

        foreach (var root in Roots)
        {
            Place(root, 0, string.Empty);
        }

        foreach (var (x, y, path, value) in nodes)
        {
            builder.Dot(x, y, color, 8);
            builder.Datum(x, y, 0, 0, path, [new ChartDatumRow(effectiveValueHeader, value, color)], ChartHover.None);
        }

        builder.XAxis = new ChartAxis(ticks);
        builder.Table = new ChartTable([effectiveLabelHeader, effectiveValueHeader], [.. nodes.Select(node => (IReadOnlyList<string>)[node.Path, node.Value])]);
    }

    private static int CountLeaves(ChartHierarchyNode node) => node.Children is { Count: > 0 } children ? children.Sum(CountLeaves) : 1;

    private static int GetDepth(ChartHierarchyNode node) => node.Children is { Count: > 0 } children ? 1 + children.Max(GetDepth) : 0;
}

/// <summary>
/// A sunburst chart: a tree drawn as rings, with the top-level nodes in the middle and each node's children
/// around it, every segment sized by its share of the whole.
/// </summary>
/// <remarks>
/// Each top-level node has its own color and its descendants are lighter tints of it. Nodes with no value are left out.
/// </remarks>
public class TwSunburstChart : TwHierarchyChartBase
{
    private const double holeRadius = 10;

    private const double outerRadius = 48;

    /// <inheritdoc />
    protected internal override void BuildScene(ChartSceneBuilder builder)
    {
        var roots = GetSized(Roots);
        if (roots.Count == 0)
        {
            return;
        }

        builder.Shape = ChartShape.Square;
        var colors = AddRootLegend(builder, roots);
        var total = roots.Sum(GetTotal);
        var ring = (outerRadius - holeRadius) / (roots.Max(GetDepth) + 1);
        var pixels = ChartMetrics.SquareSize(effectiveHeight) / 100;
        List<IReadOnlyList<string>> rows = [];

        void Draw(ChartHierarchyNode node, int level, double start, double end, string parent, ChartColor color)
        {
            var value = GetTotal(node);
            var path = GetPath(parent, node);
            var sector = new ChartSector(50, 50, holeRadius + ring * level, holeRadius + ring * (level + 1), start, end);
            builder.Path(ChartGeometry.Sector(sector), color, filled: true, opacity: Math.Max(0.4, 1 - level * 0.2), outline: true);

            // The name sits across the middle of the segment, so it needs that much of a chord and a ring tall enough for a line of text.
            var middle = (sector.InnerRadius + sector.OuterRadius) / 2;
            var chord = 2 * middle * Math.Sin(Math.Min(end - start, 180) / 2 * Math.PI / 180);
            if (ring * pixels >= 18 && chord * pixels >= Math.Min(ChartMetrics.TextWidth(node.Label), 40))
            {
                var center = ChartGeometry.Polar(50, 50, middle, (start + end) / 2);
                builder.Add(new ChartText(center.X, center.Y, node.Label, Tone: ChartTextTone.Inverse, Strong: true, MaxWidth: chord * 0.9));
            }

            builder.SectorDatum(sector, path, [new ChartDatumRow(effectiveValueHeader, FormatValue(value), color), new ChartDatumRow(localizer[TwStrings.Chart_Share], FormatShare(value, total))]);
            rows.Add([path, FormatValue(value)]);

            var angle = start;
            foreach (var child in GetSized(node.Children))
            {
                var span = (end - start) * GetTotal(child) / value;
                Draw(child, level + 1, angle, angle + span, path, color);
                angle += span;
            }
        }

        double cursor = 0;
        for (var i = 0; i < roots.Count; i++)
        {
            var span = 360 * GetTotal(roots[i]) / total;
            Draw(roots[i], 0, cursor, cursor + span, string.Empty, colors[i]);
            cursor += span;
        }

        builder.Table = new ChartTable([effectiveLabelHeader, effectiveValueHeader], rows);
    }

    private static int GetDepth(ChartHierarchyNode node) => GetSized(node.Children) is { Count: > 0 } children ? 1 + children.Max(GetDepth) : 0;
}

/// <summary>
/// The shared definition of the charts that draw sets as overlapping circles:
/// <see cref="TwVennDiagramChart"/> and <see cref="TwEulerDiagramChart"/>.
/// </summary>
public abstract class TwSetDiagramChartBase : TwChartBase
{
    private const int maxSets = 3;

    // How finely the plot is sampled to find the middle of the region two sets share.
    private const int samples = 60;

    /// <summary>
    /// Gets or sets the sets to draw. Up to three are shown; sets with no size are left out.
    /// </summary>
    [Parameter] public IReadOnlyList<ChartSet> Sets { get; set; } = [];

    /// <summary>
    /// Gets or sets how many elements each pair of sets shares. Each is named by the labels of its two sets.
    /// </summary>
    [Parameter] public IReadOnlyList<ChartSetIntersection> Intersections { get; set; } = [];

    /// <summary>
    /// Gets or sets the heading of the set column in the table view.
    /// Defaults to the localized <see cref="TwStrings.Chart_Category"/> text ("Category").
    /// </summary>
    [Parameter] public string? LabelHeader { get; set; }

    private string effectiveLabelHeader => LabelHeader ?? localizer[TwStrings.Chart_Category];

    /// <summary>
    /// Gets or sets the heading of the size column in the table view. It also labels a set's size in tooltips.
    /// Defaults to the localized <see cref="TwStrings.Chart_Size"/> text ("Size").
    /// </summary>
    [Parameter] public string? ValueHeader { get; set; }

    private string effectiveValueHeader => ValueHeader ?? localizer[TwStrings.Chart_Size];

    /// <summary>
    /// Positions a circle for each set. Any units will do: the result is scaled to fit the plot.
    /// </summary>
    /// <param name="sets">The sets to draw. Between one and three.</param>
    /// <param name="shared">Gets how many elements two sets share, by their indexes, or zero when none were given.</param>
    private protected abstract IReadOnlyList<(double X, double Y, double Radius)> Arrange(IReadOnlyList<ChartSet> sets, Func<int, int, double> shared);

    /// <inheritdoc />
    protected internal sealed override void BuildScene(ChartSceneBuilder builder)
    {
        var sets = Sets.Where(set => double.IsFinite(set.Value) && set.Value > 0).Take(maxSets).ToList();
        if (sets.Count == 0)
        {
            return;
        }

        List<(int First, int Second, double Value)> overlaps = [];
        for (var first = 0; first < sets.Count; first++)
        {
            for (var second = first + 1; second < sets.Count; second++)
            {
                var value = Intersections.Where(item => (item.First == sets[first].Label && item.Second == sets[second].Label) || (item.First == sets[second].Label && item.Second == sets[first].Label))
                    .Select(item => item.Value).FirstOrDefault(item => double.IsFinite(item) && item > 0);
                if (value > 0)
                {
                    overlaps.Add((first, second, value));
                }
            }
        }

        builder.Shape = ChartShape.Square;
        var circles = Fit(Arrange(sets, (first, second) => overlaps.FirstOrDefault(overlap => (overlap.First, overlap.Second) == (Math.Min(first, second), Math.Max(first, second))).Value));
        var middle = circles.Average(circle => circle.Y);
        string Name((int First, int Second, double Value) overlap) => $"{sets[overlap.First].Label} ∩ {sets[overlap.Second].Label}";

        for (var i = 0; i < sets.Count; i++)
        {
            var (x, y, radius) = circles[i];
            var color = ChartColor.Series(i);
            builder.Path(ChartGeometry.Circle(x, y, radius), color, filled: true, opacity: 0.35);

            // Names go outside the circles, under those in the lower half so they stay clear of the overlaps.
            var below = y > middle + 1;
            builder.Add(new ChartText(x, below ? y + radius : y - radius, sets[i].Label, Baseline: below ? ChartBaseline.Top : ChartBaseline.Bottom, Tone: ChartTextTone.Primary, OffsetY: below ? 6 : -6, Strong: true));
            builder.SectorDatum(new ChartSector(x, y, 0, radius, 0, 360), sets[i].Label,
                [new ChartDatumRow(effectiveValueHeader, FormatValue(sets[i].Value), color), .. overlaps.Where(overlap => overlap.First == i || overlap.Second == i).Select(overlap => new ChartDatumRow(Name(overlap), FormatValue(overlap.Value)))],
                anchorAngle: 0);
        }

        foreach (var overlap in overlaps)
        {
            if (GetSharedCenter(circles, overlap.First, overlap.Second) is { } center)
            {
                builder.Add(new ChartText(center.X, center.Y, FormatValue(overlap.Value), Tone: ChartTextTone.Primary, Strong: true));
            }
        }

        builder.Table = new ChartTable([effectiveLabelHeader, effectiveValueHeader],
            [.. sets.Select(set => (IReadOnlyList<string>)[set.Label, FormatValue(set.Value)]), .. overlaps.Select(overlap => (IReadOnlyList<string>)[Name(overlap), FormatValue(overlap.Value)])]);
    }

    /// <summary>
    /// Scales and centers the circles to fill the plot, leaving room above and below for the names.
    /// </summary>
    private static List<(double X, double Y, double Radius)> Fit(IReadOnlyList<(double X, double Y, double Radius)> circles)
    {
        var (left, right) = (circles.Min(circle => circle.X - circle.Radius), circles.Max(circle => circle.X + circle.Radius));
        var (high, low) = (circles.Min(circle => circle.Y - circle.Radius), circles.Max(circle => circle.Y + circle.Radius));
        var scale = Math.Min(92 / (right - left), 76 / (low - high));
        return [.. circles.Select(circle => (50 + (circle.X - (left + right) / 2) * scale, 50 + (circle.Y - (high + low) / 2) * scale, circle.Radius * scale))];
    }

    /// <summary>
    /// Gets the middle of the region two circles share, preferring the part of it no other circle covers.
    /// Returns <see langword="null"/> when the circles do not overlap.
    /// </summary>
    private static (double X, double Y)? GetSharedCenter(List<(double X, double Y, double Radius)> circles, int first, int second)
    {
        bool Covers(int circle, double x, double y) => Math.Pow(x - circles[circle].X, 2) + Math.Pow(y - circles[circle].Y, 2) < circles[circle].Radius * circles[circle].Radius;

        List<(double X, double Y, bool Exclusive)> shared = [];
        for (var row = 0; row <= samples; row++)
        {
            for (var column = 0; column <= samples; column++)
            {
                var (x, y) = (column * 100.0 / samples, row * 100.0 / samples);
                if (Covers(first, x, y) && Covers(second, x, y))
                {
                    shared.Add((x, y, !Enumerable.Range(0, circles.Count).Any(other => other != first && other != second && Covers(other, x, y))));
                }
            }
        }

        var region = shared.Any(point => point.Exclusive) ? shared.Where(point => point.Exclusive).ToList() : shared;
        return region.Count == 0 ? null : (region.Average(point => point.X), region.Average(point => point.Y));
    }
}

/// <summary>
/// A Venn diagram: up to three sets drawn as equal circles that all overlap, with the number of shared elements
/// written where each pair meets.
/// </summary>
/// <remarks>
/// Every possible overlap is drawn whether or not the sets share anything, and the circles do not follow the
/// sizes of the sets. To show sizes and only the overlaps that exist, use <see cref="TwEulerDiagramChart"/>.
/// </remarks>
public class TwVennDiagramChart : TwSetDiagramChartBase
{
    private protected override IReadOnlyList<(double X, double Y, double Radius)> Arrange(IReadOnlyList<ChartSet> sets, Func<int, int, double> shared) => sets.Count switch
    {
        1 => [(0, 0, 1)],
        2 => [(-0.5, 0, 1), (0.5, 0, 1)],
        _ => [(-0.5, 0, 1), (0.5, 0, 1), (0, Math.Sqrt(3) / 2, 1)]
    };
}

/// <summary>
/// An Euler diagram: up to three sets drawn as circles whose areas follow their sizes, overlapping only where
/// they share elements and by an area that follows how many.
/// </summary>
/// <remarks>
/// Sets with no intersection given are drawn apart, and a set that lies wholly within another is drawn inside it.
/// With three sets the overlaps are as close as circles allow, so read exact numbers from the labels.
/// </remarks>
public class TwEulerDiagramChart : TwSetDiagramChartBase
{
    private protected override IReadOnlyList<(double X, double Y, double Radius)> Arrange(IReadOnlyList<ChartSet> sets, Func<int, int, double> shared)
    {
        List<double> radii = [.. sets.Select(set => Math.Sqrt(set.Value / Math.PI))];
        double Distance(int first, int second) => GetDistance(radii[first], radii[second], shared(first, second));

        List<(double X, double Y, double Radius)> circles = [(0, 0, radii[0])];
        if (sets.Count > 1)
        {
            circles.Add((Distance(0, 1), 0, radii[1]));
        }

        if (sets.Count > 2)
        {
            // The third center lies where its distances from the first two meet, or in line with them when they cannot.
            var (apart, toFirst, toSecond) = (Distance(0, 1), Distance(0, 2), Distance(1, 2));
            var along = apart > 0 ? (toFirst * toFirst - toSecond * toSecond + apart * apart) / (2 * apart) : toFirst;
            circles.Add((along, Math.Sqrt(Math.Max(0, toFirst * toFirst - along * along)), radii[2]));
        }

        return circles;
    }

    /// <summary>
    /// Gets how far apart the centers of two circles must be for the area they share to equal <paramref name="shared"/>.
    /// </summary>
    private static double GetDistance(double first, double second, double shared)
    {
        var (small, large) = (Math.Min(first, second), Math.Max(first, second));
        if (shared <= 0)
        {
            return (first + second) * 1.1;
        }

        // Sharing all of the smaller set, give or take the rounding of its radius, puts it inside the larger.
        if (shared >= Math.PI * small * small * 0.999999)
        {
            return (large - small) / 2;
        }

        // The shared area only shrinks as the circles move apart, so halve the range until it matches.
        var (near, far) = (large - small, large + small);
        for (var step = 0; step < 50; step++)
        {
            var distance = (near + far) / 2;
            var area = small * small * Math.Acos((distance * distance + small * small - large * large) / (2 * distance * small))
                + large * large * Math.Acos((distance * distance + large * large - small * small) / (2 * distance * large))
                - Math.Sqrt((-distance + small + large) * (distance + small - large) * (distance - small + large) * (distance + small + large)) / 2;
            (near, far) = area > shared ? (distance, far) : (near, distance);
        }

        return (near + far) / 2;
    }
}