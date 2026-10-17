using Bunit;
using System.Globalization;
using TwBlazor.Charting;
using TwBlazor.Components;
using TwBlazor.Enums;
using TwBlazor.Models;

namespace TwBlazor.Tests.Components.Chart;

/// <summary>
/// Tests the scene each chart type builds: its marks, data points, axes, legend and table.
/// </summary>
public class ChartTypeTests : TwBlazorTestBase
{
    private static readonly string[] _categories = ["A", "B", "C"];

    private static readonly ChartSeries[] _series = [new("One", [10, 20, 30]), new("Two", [5, -5, 15])];

    private static readonly ChartValue[] _values = [new("Search", 50), new("Direct", 30), new("Social", 20)];

    private static readonly ChartSample[] _samples = [new("Fast", [1, 2, 3, 4, 5, 6, 7, 8, 9, 40]), new("Slow", [20, 22, 24])];

    private ChartScene Scene<T>(Action<ComponentParameterCollectionBuilder<T>>? configure = null) where T : TwChartBase =>
        TestContext.Render(configure).Instance.CreateScene();

    private ChartScene CategoryScene<T>(Action<ComponentParameterCollectionBuilder<T>>? configure = null) where T : TwCategoryChartBase =>
        Scene<T>(p =>
        {
            p.Add(x => x.Categories, _categories).Add(x => x.Series, _series);
            configure?.Invoke(p);
        });

    private ChartScene ValueScene<T>(Action<ComponentParameterCollectionBuilder<T>>? configure = null) where T : TwValueChartBase =>
        Scene<T>(p =>
        {
            p.Add(x => x.Data, _values);
            configure?.Invoke(p);
        });

    private static List<T> Marks<T>(ChartScene scene) where T : IChartMark => [.. scene.Marks.OfType<T>()];

    private static List<ChartRect> Bars(ChartScene scene, int slot) => [.. scene.Marks.OfType<ChartRect>().Where(rect => rect.Color == ChartColor.Series(slot))];

    #region Every chart

    public static TheoryData<Type> ChartTypes => [.. typeof(TwChartBase).Assembly.GetTypes().Where(type => !type.IsAbstract && typeof(TwChartBase).IsAssignableFrom(type))];

    [Theory]
    [MemberData(nameof(ChartTypes))]
    public void EveryChart_WithNoData_ShowsTheEmptyState(Type type)
    {
        var cut = TestContext.Render(builder =>
        {
            builder.OpenComponent(0, type);
            builder.CloseComponent();
        });

        // A gauge always has a value to show, so it is the one chart that is never empty.
        if (type == typeof(TwGaugeChart))
        {
            Assert.NotEmpty(cut.FindAll("svg"));
        }
        else
        {
            Assert.Contains("No data to display", cut.Markup);
        }
    }

    [Fact]
    public void EveryChart_WithData_HasMarksDataPointsAndATable()
    {
        ChartPointSeries[] points = [new("P", [new(1, 2, "a", 3), new(4, 5)])];
        ChartScene[] scenes =
        [
            CategoryScene<TwBarChart>(), CategoryScene<TwColumnChart>(), CategoryScene<TwLollipopChart>(), CategoryScene<TwDotPlotChart>(),
            CategoryScene<TwDumbbellChart>(), CategoryScene<TwLineChart>(), CategoryScene<TwAreaChart>(), CategoryScene<TwSlopeChart>(),
            CategoryScene<TwBumpChart>(), CategoryScene<TwParallelCoordinatesChart>(), CategoryScene<TwHeatmapChart>(), CategoryScene<TwMatrixChart>(),
            CategoryScene<TwRadarChart>(), CategoryScene<TwSmallMultiplesChart>(),
            ValueScene<TwWaterfallChart>(), ValueScene<TwPieChart>(), ValueScene<TwNightingaleChart>(), ValueScene<TwRadialBarChart>(),
            ValueScene<TwFunnelChart>(), ValueScene<TwWaffleChart>(), ValueScene<TwTreemapChart>(), ValueScene<TwIconChart>(), ValueScene<TwWordCloudChart>(),
            Scene<TwRangeChart>(p => p.Add(x => x.Ranges, [new ChartRange("A", 1, 5)])),
            Scene<TwBulletChart>(p => p.Add(x => x.Items, [new ChartBullet("A", 5, 6)])),
            Scene<TwScatterChart>(p => p.Add(x => x.Series, points)),
            Scene<TwQuadrantChart>(p => p.Add(x => x.Series, points)),
            Scene<TwCategoricalScatterChart>(p => p.Add(x => x.Points, [new("A", "X", 2), new("B", "Y", 4)])),
            Scene<TwHexbinChart>(p => p.Add(x => x.Series, points)),
            Scene<TwContourChart>(p => p.Add(x => x.Series, [new ChartPointSeries("Surface", [new(0, 0, Size: 1), new(1, 0, Size: 2), new(0, 1, Size: 3)])])),
            CategoryScene<TwMarimekkoChart>(),
            Scene<TwCircularTreemapChart>(p => p.Add(x => x.Roots, [new ChartHierarchyNode("Root", Children: [new("A", 3), new("B", 2)])])),
            Scene<TwConvexTreemapChart>(p => p.Add(x => x.Roots, [new ChartHierarchyNode("Root", Children: [new("A", 3), new("B", 2)])])),
            Scene<TwDendrogramChart>(p => p.Add(x => x.Roots, [new ChartHierarchyNode("Root", Children: [new("A", 3), new("B", 2)])])),
            Scene<TwVennDiagramChart>(p => p.Add(x => x.Sets, [new("A", 5), new("B", 4)])),
            Scene<TwEulerDiagramChart>(p => p.Add(x => x.Sets, [new("A", 5), new("B", 4)])),
            Scene<TwSunburstChart>(p => p.Add(x => x.Roots, [new ChartHierarchyNode("Root", Children: [new("A", 3), new("B", 2)])])),
            CategoryScene<TwStreamGraphChart>(),
            CategoryScene<TwBumpAreaChart>(),
            Scene<TwBarcodeChart>(p => p.Add(x => x.Values, [1, 2, 3, 4])),
            Scene<TwDensityPlotChart>(p => p.Add(x => x.Samples, _samples)),
            Scene<TwRidgelinePlotChart>(p => p.Add(x => x.Samples, _samples)),
            CategoryScene<TwHorizonChart>(),
            Scene<TwRadialHistogramChart>(p => p.Add(x => x.Values, [1, 2, 2, 3, 9])),
            Scene<TwBeeswarmChart>(p => p.Add(x => x.Samples, _samples)),
            Scene<TwViolinPlotChart>(p => p.Add(x => x.Samples, _samples)),
            Scene<TwChordDiagramChart>(p => p.Add(x => x.Nodes, [new("a", "A"), new("b", "B")]).Add(x => x.Links, [new("a", "b", 2)])),
            Scene<TwArcDiagramChart>(p => p.Add(x => x.Nodes, [new("a", "A"), new("b", "B")]).Add(x => x.Links, [new("a", "b", 2)])),
            Scene<TwSankeyChart>(p => p.Add(x => x.Nodes, [new("a", "A"), new("b", "B", 1)]).Add(x => x.Links, [new("a", "b", 2)])),
            Scene<TwNetworkDiagramChart>(p => p.Add(x => x.Nodes, [new("a", "A"), new("b", "B")]).Add(x => x.Links, [new("a", "b", 2)])),
            Scene<TwFlowchart>(p => p.Add(x => x.Nodes, [new("a", "A"), new("b", "B")]).Add(x => x.Links, [new("a", "b", 2)])),
            Scene<TwGaugeChart>(p => p.Add(x => x.Value, 40)),
            Scene<TwHistogramChart>(p => p.Add(x => x.Values, [1, 2, 2, 3, 9])),
            Scene<TwBoxPlotChart>(p => p.Add(x => x.Samples, _samples)),
            Scene<TwStripPlotChart>(p => p.Add(x => x.Samples, _samples)),
            Scene<TwCandlestickChart>(p => p.Add(x => x.Items, [new ChartOhlc("Mon", 1, 4, 0, 3)])),
            Scene<TwGanttChart>(p => p.Add(x => x.Tasks, [new ChartTask("A", new DateTime(2026, 1, 1), new DateTime(2026, 1, 9))]))
        ];

        Assert.Equal(ChartTypes.Count, scenes.Length);
        Assert.All(scenes, scene =>
        {
            Assert.NotEmpty(scene.Marks);
            Assert.NotEmpty(scene.Datums);
            Assert.NotNull(scene.Table);
            Assert.NotEmpty(scene.Table.Rows);
            Assert.All(scene.Datums, datum => Assert.False(string.IsNullOrWhiteSpace(datum.AriaLabel)));
        });
    }

    #endregion

    #region Shared layout rules

    private static readonly ChartGraphNode[] _nodes = [new("in", "Input"), new("solar", "Solar"), new("grid", "Grid"), new("homes", "Homes")];

    private static readonly ChartGraphLink[] _links = [new("in", "solar", 25), new("in", "grid", 75), new("solar", "homes", 25), new("grid", "homes", 75)];

    private static readonly ChartHierarchyNode[] _tree = [new("Media", Children: [new("Images", 9), new("Audio", 4)]), new("Notes", 1)];

    private static void AssertInsidePlot(ChartScene scene)
    {
        static void InPlot(double value) => Assert.InRange(value, -0.001, 100.001);

        Assert.All(Marks<ChartRect>(scene), rect =>
        {
            InPlot(rect.X);
            InPlot(rect.Y);
            InPlot(rect.X + rect.Width);
            InPlot(rect.Y + rect.Height);
        });
        Assert.All(Marks<ChartDot>(scene), dot =>
        {
            InPlot(dot.X);
            InPlot(dot.Y);
        });
        Assert.All(Marks<ChartText>(scene), text =>
        {
            InPlot(text.X);
            InPlot(text.Y);
        });
        Assert.All(Marks<ChartLine>(scene), line =>
        {
            InPlot(line.X1);
            InPlot(line.Y1);
            InPlot(line.X2);
            InPlot(line.Y2);
        });
        Assert.All(scene.Datums, datum =>
        {
            InPlot(datum.X - datum.Width / 2);
            InPlot(datum.X + datum.Width / 2);
            InPlot(datum.Y - datum.Height / 2);
            InPlot(datum.Y + datum.Height / 2);
        });
    }

    [Fact]
    public void NewCharts_KeepEveryMarkAndDataPointInsideThePlot()
    {
        ChartSample[] crowded = [new("A", [.. Enumerable.Repeat(5.0, 40)]), new("B", [1, 2, 3]), new("C", [9, 9, 9, 9]), new("D", [4]), new("E", [6, 7]), new("F", [2, 2])];
        ChartPointSeries[] corners = [new("P", [new(0, 0, Size: 1), new(10, 0, Size: 2), new(0, 10, Size: 3), new(10, 10, Size: 4), new(5, 5, Size: 9)])];
        ChartSeries[] many = [.. Enumerable.Range(0, 7).Select(index => new ChartSeries($"S{index}", [index + 1, 9 - index, 5]))];
        ChartGraphNode[] nodes = [.. Enumerable.Range(0, 9).Select(index => new ChartGraphNode($"n{index}", $"Node {index}"))];
        ChartGraphLink[] links = [.. Enumerable.Range(0, 8).Select(index => new ChartGraphLink($"n{index}", $"n{index + 1}", index + 1)), new("n0", "n8", 4)];
        ChartValue[] words = [.. Enumerable.Range(0, 80).Select(index => new ChartValue($"Word{index}", 100 - index))];

        ChartScene[] scenes =
        [
            Scene<TwBeeswarmChart>(p => p.Add(x => x.Samples, crowded)),
            Scene<TwViolinPlotChart>(p => p.Add(x => x.Samples, crowded)),
            Scene<TwRidgelinePlotChart>(p => p.Add(x => x.Samples, crowded)),
            Scene<TwDensityPlotChart>(p => p.Add(x => x.Samples, crowded)),
            Scene<TwHexbinChart>(p => p.Add(x => x.Series, corners)),
            Scene<TwContourChart>(p => p.Add(x => x.Series, corners)),
            Scene<TwBumpAreaChart>(p => p.Add(x => x.Categories, _categories).Add(x => x.Series, many)),
            Scene<TwStreamGraphChart>(p => p.Add(x => x.Categories, _categories).Add(x => x.Series, many)),
            Scene<TwHorizonChart>(p => p.Add(x => x.Categories, _categories).Add(x => x.Series, many)),
            Scene<TwMarimekkoChart>(p => p.Add(x => x.Categories, _categories).Add(x => x.Series, many)),
            Scene<TwCircularTreemapChart>(p => p.Add(x => x.Roots, _tree)),
            Scene<TwConvexTreemapChart>(p => p.Add(x => x.Roots, _tree)),
            Scene<TwDendrogramChart>(p => p.Add(x => x.Roots, _tree)),
            Scene<TwSunburstChart>(p => p.Add(x => x.Roots, _tree)),
            Scene<TwVennDiagramChart>(p => p.Add(x => x.Sets, [new("A", 5), new("B", 4), new("C", 3)])),
            Scene<TwEulerDiagramChart>(p => p.Add(x => x.Sets, [new("A", 50), new("B", 4), new("C", 3)]).Add(x => x.Intersections, [new("A", "B", 4)])),
            Scene<TwChordDiagramChart>(p => p.Add(x => x.Nodes, nodes).Add(x => x.Links, links)),
            Scene<TwArcDiagramChart>(p => p.Add(x => x.Nodes, nodes).Add(x => x.Links, links)),
            Scene<TwSankeyChart>(p => p.Add(x => x.Nodes, nodes).Add(x => x.Links, links)),
            Scene<TwNetworkDiagramChart>(p => p.Add(x => x.Nodes, nodes).Add(x => x.Links, links)),
            Scene<TwFlowchart>(p => p.Add(x => x.Nodes, nodes).Add(x => x.Links, links)),
            Scene<TwWordCloudChart>(p => p.Add(x => x.Data, words))
        ];

        Assert.All(scenes, scene =>
        {
            Assert.NotEmpty(scene.Marks);
            AssertInsidePlot(scene);
        });
    }

    [Fact]
    public void NewCharts_WritePathsTheSameWay_WhateverTheCulture()
    {
        var culture = CultureInfo.CurrentCulture;
        try
        {
            // German writes one and a half as "1,5", which is not a number in an SVG path.
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            ChartScene[] scenes =
            [
                Scene<TwHexbinChart>(p => p.Add(x => x.Series, [new ChartPointSeries("P", [new(1.5, 2.5), new(7.25, 8.75)])])),
                Scene<TwConvexTreemapChart>(p => p.Add(x => x.Roots, _tree)),
                Scene<TwChordDiagramChart>(p => p.Add(x => x.Nodes, _nodes).Add(x => x.Links, _links)),
                Scene<TwArcDiagramChart>(p => p.Add(x => x.Nodes, _nodes).Add(x => x.Links, _links)),
                Scene<TwSankeyChart>(p => p.Add(x => x.Nodes, _nodes).Add(x => x.Links, _links)),
                Scene<TwFlowchart>(p => p.Add(x => x.Nodes, _nodes).Add(x => x.Links, _links))
            ];

            Assert.All(scenes.SelectMany(Marks<ChartPath>), path => Assert.DoesNotContain(',', path.Data));
            Assert.All(scenes.SelectMany(scene => scene.Datums).Where(datum => datum.Clip != null), datum => Assert.DoesNotMatch(@"\d,\d", datum.Clip!));
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    [Fact]
    public void ShapeDatum_CoversItsOutline_AndPointsTheTooltipAtItsAnchor()
    {
        var builder = new ChartSceneBuilder();
        builder.ShapeDatum([(10, 20), (30, 20), (20, 60)], "Shape", [], (20, 20));
        var datum = Assert.Single(builder.Build().Datums);

        Assert.Equal((20, 40, 20, 40), (datum.X, datum.Y, datum.Width, datum.Height));
        Assert.Equal("polygon(0% 0%, 100% 0%, 50% 100%)", datum.Clip);
        Assert.Equal((20, 20), (datum.AnchorX, datum.AnchorY));
        Assert.Equal(ChartHover.None, datum.Hover);
    }

    [Fact]
    public void TextWithAMaximumWidth_IsCutShortInsteadOfSpillingOverItsMark()
    {
        var cut = TestContext.Render<TwFlowchart>(p => p.Add(x => x.Nodes, [new ChartGraphNode("a", "A step with a very long name")]));
        var label = cut.FindAll("span[aria-hidden='true']").Single(span => span.TextContent == "A step with a very long name");

        Assert.Contains("truncate", label.ClassName);
        Assert.Contains("max-width:", label.GetAttribute("style"));
    }

    #endregion

    #region Word cloud

    [Fact]
    public void WordCloud_SizesWordsByValue_AndKeepsEveryWordAccessible()
    {
        var scene = ValueScene<TwWordCloudChart>();
        var text = Marks<ChartText>(scene);

        Assert.Equal(_values.Length, text.Count);
        Assert.Equal(_values.Select(value => value.Label), text.Select(word => word.Text));
        Assert.True(text[0].FontSize > text[^1].FontSize);
        Assert.Equal(_values.Length, scene.Datums.Count);
        Assert.NotNull(scene.Table);
    }

    [Fact]
    public void WordCloud_NeverOverlapsWords_AndLeavesOutOnlyThoseThatDoNotFit()
    {
        ChartValue[] words = [.. Enumerable.Range(0, 120).Select(index => new ChartValue($"Keyword{index}", 500 - index))];
        var scene = Scene<TwWordCloudChart>(p => p.Add(x => x.Data, words));
        var boxes = scene.Datums;

        Assert.InRange(boxes.Count, 1, words.Length - 1);
        Assert.Equal(words.Length, scene.Table!.Rows.Count);
        Assert.Equal("Keyword0", boxes[0].Title);

        for (var first = 0; first < boxes.Count; first++)
        {
            for (var second = first + 1; second < boxes.Count; second++)
            {
                var apartX = Math.Abs(boxes[first].X - boxes[second].X) >= (boxes[first].Width + boxes[second].Width) / 2 - 0.001;
                var apartY = Math.Abs(boxes[first].Y - boxes[second].Y) >= (boxes[first].Height + boxes[second].Height) / 2 - 0.001;
                Assert.True(apartX || apartY, $"{boxes[first].Title} overlaps {boxes[second].Title}");
            }
        }
    }

    [Fact]
    public void WordCloud_ScalesItsWordsWithTheHeightOfTheChart()
    {
        var small = Marks<ChartText>(ValueScene<TwWordCloudChart>(p => p.Add(x => x.Height, ChartHeight.Small)));
        var large = Marks<ChartText>(ValueScene<TwWordCloudChart>(p => p.Add(x => x.Height, ChartHeight.Large)));

        Assert.True(small[0].FontSize < large[0].FontSize);
    }

    #endregion

    #region Comparison and correlation

    [Fact]
    public void CategoricalScatter_UsesCategoriesForAxes_AndValueForPointSize()
    {
        var scene = Scene<TwCategoricalScatterChart>(p => p.Add(x => x.Points, [new("A", "X", 0.2), new("B", "Y", 0.8)]));
        var dots = Marks<ChartDot>(scene);

        Assert.Equal(["A", "B"], scene.XAxis!.Ticks.Select(tick => tick.Label));
        Assert.Equal(["X", "Y"], scene.YAxis!.Ticks.Select(tick => tick.Label));
        Assert.Equal(2, dots.Count);
        Assert.True(dots[0].Diameter < dots[1].Diameter);
        Assert.Equal(32, dots[1].Diameter);
    }

    [Fact]
    public void CategoricalScatter_PointsEachTooltipAtItsDot()
    {
        var scene = Scene<TwCategoricalScatterChart>(p => p.Add(x => x.Points, [new("A", "X", 2), new("B", "Y", 4, "Peak")]));
        var dots = Marks<ChartDot>(scene);

        Assert.Equal(dots.Select(dot => (dot.X, dot.Y)), scene.Datums.Select(datum => (datum.X, datum.Y)));
        Assert.All(scene.Datums, datum => Assert.Equal((0, 0), (datum.Width, datum.Height)));
        Assert.Equal(["A, X", "Peak"], scene.Datums.Select(datum => datum.Title));
        Assert.Equal(["Label", "X", "Y", "Value"], scene.Table!.Headers);
    }

    [Fact]
    public void Hexbin_AggregatesPointsIntoAccessibleBins()
    {
        ChartPointSeries[] points = [new("P", [new(1, 1), new(1, 1), new(9, 9)])];
        var scene = Scene<TwHexbinChart>(p => p.Add(x => x.Series, points));

        Assert.Equal(2, Marks<ChartPath>(scene).Count);
        Assert.Equal(2, scene.Datums.Count);
        Assert.All(scene.Datums, datum => Assert.StartsWith("polygon(", datum.Clip));
        Assert.Equal(["1", "2"], scene.Datums.Select(datum => datum.Rows[0].Value).Order());
        Assert.Equal(["X", "Y", "Count"], scene.Table!.Headers);
    }

    [Fact]
    public void Hexbin_ShadesFullerBinsDarker()
    {
        ChartPointSeries[] points = [new("P", [new(1, 1), new(1, 1), new(1, 1), new(1, 1), new(9, 9)])];
        var hexagons = Marks<ChartPath>(Scene<TwHexbinChart>(p => p.Add(x => x.Series, points)));

        Assert.Equal(1, hexagons.Max(hexagon => hexagon.Opacity));
        Assert.True(hexagons.Min(hexagon => hexagon.Opacity) < 0.5);
    }

    [Fact]
    public void Contour_DrawsOneLinePerLevel_AndADataPointPerSample()
    {
        ChartPointSeries[] points = [new("P", [new(0, 0, Size: 0), new(1, 0, Size: 1), new(0, 1, Size: 2), new(1, 1, Size: 3), new(2, 2)])];
        var scene = Scene<TwContourChart>(p => p.Add(x => x.Series, points).Add(x => x.Levels, 4));
        var lines = Marks<ChartPath>(scene);

        Assert.Equal(4, lines.Count);
        Assert.All(lines, line => Assert.False(line.Filled));
        Assert.True(lines[0].Opacity < lines[^1].Opacity);
        Assert.Equal(4, scene.Datums.Count);
        Assert.Equal(4, Marks<ChartDot>(scene).Count);
        Assert.Equal(4, scene.Table!.Rows.Count);
    }

    [Fact]
    public void Contour_WithFewerThanThreeSamples_IsEmpty()
    {
        var scene = Scene<TwContourChart>(p => p.Add(x => x.Series, [new ChartPointSeries("P", [new(0, 0, Size: 1), new(1, 1, Size: 2)])]));

        Assert.True(scene.IsEmpty);
    }

    #endregion

    #region Hierarchies and sets

    [Fact]
    public void Marimekko_SizesColumnsByTotal_AndSegmentsByShare()
    {
        ChartSeries[] series = [new("Core", [30, 10]), new("Plus", [30, 30])];
        var scene = Scene<TwMarimekkoChart>(p => p.Add(x => x.Categories, ["North", "South"]).Add(x => x.Series, series));
        var rects = Marks<ChartRect>(scene);

        Assert.Equal([60, 60, 40, 40], rects.Select(rect => rect.Width));
        Assert.Equal([50, 50, 25, 75], rects.Select(rect => rect.Height));
        Assert.Equal([30, 80], scene.XAxis!.Ticks.Select(tick => tick.Position));
        Assert.Equal(["0%", "25%", "50%", "75%", "100%"], scene.YAxis!.Ticks.Select(tick => tick.Label));
        Assert.Equal(2, scene.Datums.Count);
    }

    [Fact]
    public void CircularTreemap_PacksCirclesByArea_WithoutOverlap()
    {
        var scene = Scene<TwCircularTreemapChart>(p => p.Add(x => x.Roots, _tree));
        var leaves = scene.Datums;

        Assert.Equal(["Media / Images", "Media / Audio", "Notes"], leaves.Select(leaf => leaf.Title));
        Assert.Equal(1.5, leaves[0].Width / leaves[1].Width, 2);
        Assert.Equal(3, leaves[0].Width / leaves[2].Width, 2);

        for (var first = 0; first < leaves.Count; first++)
        {
            for (var second = first + 1; second < leaves.Count; second++)
            {
                var distance = Math.Sqrt(Math.Pow(leaves[first].X - leaves[second].X, 2) + Math.Pow(leaves[first].Y - leaves[second].Y, 2));
                Assert.True(distance >= (leaves[first].Width + leaves[second].Width) / 2);
            }
        }

        Assert.Equal(["Media", "Notes"], scene.Legend.Select(item => item.Label));
        Assert.Equal(ChartShape.Square, scene.Shape);
    }

    [Fact]
    public void ConvexTreemap_TilesThePlotByValue()
    {
        ChartHierarchyNode[] roots = [new("A", 6), new("B", 3), new("C", 1)];
        var scene = Scene<TwConvexTreemapChart>(p => p.Add(x => x.Roots, roots));
        var areas = scene.Datums.Select(datum => datum.Width * datum.Height).ToList();

        Assert.Equal(3, Marks<ChartPath>(scene).Count);
        Assert.Equal([6000, 3000, 1000], areas.Select(area => Math.Round(area)));
        Assert.All(scene.Datums, datum => Assert.StartsWith("polygon(", datum.Clip));
        Assert.All(Marks<ChartText>(scene), text => Assert.NotNull(text.MaxWidth));
    }

    [Fact]
    public void Dendrogram_NamesLeavesAlongTheAxis_AndMakesEveryNodeADataPoint()
    {
        var scene = Scene<TwDendrogramChart>(p => p.Add(x => x.Roots, _tree));

        Assert.Equal(["Images", "Audio", "Notes"], scene.XAxis!.Ticks.Select(tick => tick.Label));
        Assert.Equal(["Media", "Media / Images", "Media / Audio", "Notes"], scene.Datums.Select(datum => datum.Title));
        Assert.Equal(["Media"], Marks<ChartText>(scene).Select(text => text.Text));
        Assert.Single(scene.Datums.Where(datum => datum.Title != "Media").Select(datum => datum.Y).Distinct());
        Assert.Equal(scene.XAxis.Ticks.Select(tick => tick.Position), scene.Datums.Where(datum => datum.Title != "Media").Select(datum => datum.X));
        Assert.Equal("13", scene.Datums[0].Rows[0].Value);
    }

    [Fact]
    public void Sunburst_SizesSegmentsByShare_AndKeepsABranchInOneColor()
    {
        var scene = Scene<TwSunburstChart>(p => p.Add(x => x.Roots, _tree));
        var segments = Marks<ChartPath>(scene);

        Assert.Equal(["Media", "Media / Images", "Media / Audio", "Notes"], scene.Datums.Select(datum => datum.Title));
        Assert.Equal([ChartColor.Series(0), ChartColor.Series(0), ChartColor.Series(0), ChartColor.Series(1)], segments.Select(segment => segment.Color));
        Assert.True(segments[1].Opacity < segments[0].Opacity);
        Assert.Equal(["92.9%", "64.3%", "28.6%", "7.1%"], scene.Datums.Select(datum => datum.Rows[1].Value));
    }

    [Fact]
    public void VennDiagram_DrawsEqualCircles_AndKeepsTheSharedCountsApart()
    {
        ChartSet[] sets = [new("A", 50), new("B", 5), new("C", 20)];
        ChartSetIntersection[] shared = [new("A", "B", 3), new("C", "B", 2), new("A", "C", 9)];
        var scene = Scene<TwVennDiagramChart>(p => p.Add(x => x.Sets, sets).Add(x => x.Intersections, shared));
        var text = Marks<ChartText>(scene);
        var counts = text.Skip(3).ToList();

        Assert.Single(scene.Datums.Select(datum => Math.Round(datum.Width, 3)).Distinct());
        Assert.Equal(["A", "B", "C", "3", "9", "2"], text.Select(item => item.Text));
        Assert.Equal([ChartBaseline.Bottom, ChartBaseline.Bottom, ChartBaseline.Top], text.Take(3).Select(item => item.Baseline));

        for (var first = 0; first < counts.Count; first++)
        {
            for (var second = first + 1; second < counts.Count; second++)
            {
                Assert.True(Math.Sqrt(Math.Pow(counts[first].X - counts[second].X, 2) + Math.Pow(counts[first].Y - counts[second].Y, 2)) > 15);
            }
        }

        Assert.Equal(["A", "B", "C", "A ∩ B", "A ∩ C", "B ∩ C"], scene.Table!.Rows.Select(row => row[0]));
    }

    [Fact]
    public void EulerDiagram_SizesCirclesBySet_AndOverlapsOnlyWhatIsShared()
    {
        ChartSet[] sets = [new("A", 40), new("B", 10)];

        static double Gap(ChartScene scene) => Math.Abs(scene.Datums[0].X - scene.Datums[1].X) - (scene.Datums[0].Width + scene.Datums[1].Width) / 2;

        var apart = Scene<TwEulerDiagramChart>(p => p.Add(x => x.Sets, sets));
        var overlapping = Scene<TwEulerDiagramChart>(p => p.Add(x => x.Sets, sets).Add(x => x.Intersections, [new("B", "A", 4)]));
        var inside = Scene<TwEulerDiagramChart>(p => p.Add(x => x.Sets, sets).Add(x => x.Intersections, [new("A", "B", 10)]));

        Assert.Equal(2, apart.Datums[0].Width / apart.Datums[1].Width, 2);
        Assert.True(Gap(apart) > 0);
        Assert.Empty(Marks<ChartText>(apart).Skip(2));
        Assert.InRange(Gap(overlapping), -overlapping.Datums[1].Width, 0);
        Assert.True(Gap(inside) < -inside.Datums[1].Width);
        Assert.Equal("4", Marks<ChartText>(overlapping)[2].Text);
    }

    #endregion

    #region Temporal variants

    [Fact]
    public void StreamGraph_CentersStackedSeriesAroundTheMidline()
    {
        var scene = CategoryScene<TwStreamGraphChart>();

        Assert.Equal(_series.Length, Marks<ChartPath>(scene).Count);
        Assert.Equal(_categories.Length, scene.Datums.Count);
        Assert.All(scene.Datums, datum => Assert.Equal(ChartHover.Crosshair, datum.Hover));
        Assert.Equal(_categories, scene.XAxis!.Ticks.Select(tick => tick.Label));
        Assert.Equal(scene.XAxis.Ticks.Select(tick => tick.Position), scene.Datums.Select(datum => datum.X));
    }

    [Fact]
    public void BumpArea_StacksBandsByRank_AndReportsTheRankInEachTooltip()
    {
        ChartSeries[] series = [new("A", [1, 5]), new("B", [3, 2])];
        var scene = Scene<TwBumpAreaChart>(p => p.Add(x => x.Categories, ["2024", "2025"]).Add(x => x.Series, series));

        Assert.Equal(2, Marks<ChartPath>(scene).Count);
        Assert.Equal(["B, 2024", "A, 2024", "A, 2025", "B, 2025"], scene.Datums.Select(datum => datum.Title));
        Assert.Equal(["1", "2", "1", "2"], scene.Datums.Select(datum => datum.Rows[0].Value));
        Assert.Equal(["3", "1", "5", "2"], scene.Datums.Select(datum => datum.Rows[1].Value));

        // Within a category the bands never overlap, and a larger value makes a thicker band.
        Assert.True(scene.Datums[0].Y + scene.Datums[0].Height / 2 < scene.Datums[1].Y - scene.Datums[1].Height / 2);
        Assert.Equal(3, scene.Datums[0].Height / scene.Datums[1].Height, 3);
    }

    [Fact]
    public void BumpArea_BreaksABand_WhereItsSeriesHasNoValue()
    {
        ChartSeries[] series = [new("A", [1, null, 5]), new("B", [3, 2, 4])];
        var scene = Scene<TwBumpAreaChart>(p => p.Add(x => x.Categories, _categories).Add(x => x.Series, series));

        Assert.Equal(3, Marks<ChartPath>(scene).Count);
        Assert.Equal(5, scene.Datums.Count);
    }

    [Fact]
    public void Barcode_DrawsAVerticalLinePerValue_AlongAHorizontalAxis()
    {
        var scene = Scene<TwBarcodeChart>(p => p.Add(x => x.Values, [1, 2, 2, double.NaN, 4]));
        var marks = Marks<ChartLine>(scene).Where(line => line.Color == ChartColor.Series(0)).ToList();

        Assert.NotNull(scene.XAxis);
        Assert.Null(scene.YAxis);
        Assert.Equal(3, marks.Count);
        Assert.All(marks, line => Assert.Equal(line.X1, line.X2));
        Assert.Equal(marks.Select(line => line.X1), scene.Datums.Select(datum => datum.X));
        Assert.Equal(["1", "2", "1"], scene.Datums.Select(datum => datum.Rows[0].Value));
        Assert.Equal(3, scene.Table!.Rows.Count);
    }

    #endregion

    #region Distribution variants

    [Fact]
    public void DensityPlot_DrawsOneFilledCurvePerGroup_AndPointsItsTooltipAtThePeak()
    {
        var scene = Scene<TwDensityPlotChart>(p => p.Add(x => x.Samples, _samples));

        Assert.Equal(_samples.Length, Marks<ChartPath>(scene).Count(path => path.Filled));
        Assert.Equal(_samples.Select(sample => sample.Label), scene.Legend.Select(item => item.Label));
        Assert.NotNull(scene.XAxis);
        Assert.Null(scene.YAxis);
        Assert.Equal(_samples.Length, scene.Datums.Count);
        Assert.All(scene.Datums, datum =>
        {
            Assert.StartsWith("polygon(", datum.Clip);
            Assert.InRange(datum.AnchorY!.Value, datum.Y - datum.Height / 2 - 0.001, datum.Y - datum.Height / 2 + 0.001);
        });
        Assert.Equal(["Group", "Count", "Minimum", "Median", "Maximum"], scene.Table!.Headers);
    }

    [Fact]
    public void DensityPlot_DrawsATighterGroupTaller()
    {
        ChartSample[] samples = [new("Tight", [10, 10.5, 11, 11.5, 12]), new("Loose", [0, 5, 10, 15, 20])];
        var scene = Scene<TwDensityPlotChart>(p => p.Add(x => x.Samples, samples));

        Assert.True(scene.Datums[0].Height > scene.Datums[1].Height * 2);
    }

    [Fact]
    public void Ridgeline_GivesEachGroupItsOwnRow()
    {
        var scene = Scene<TwRidgelinePlotChart>(p => p.Add(x => x.Samples, _samples));

        Assert.Equal(_samples.Select(sample => sample.Label), scene.YAxis!.Ticks.Select(tick => tick.Label));
        Assert.NotNull(scene.XAxis);
        Assert.Equal(_samples.Length, scene.Datums.Count);
        Assert.Equal(_samples.Length, Marks<ChartPath>(scene).Count(path => path.Filled));
        Assert.Empty(Marks<ChartText>(scene));
    }

    [Fact]
    public void Horizon_GivesEachSeriesARow_AndLayersMoreBandsForLargerValues()
    {
        ChartSeries[] series = [new("One", [10, 20, 30]), new("Two", [5, -30, null])];
        var scene = Scene<TwHorizonChart>(p => p.Add(x => x.Categories, _categories).Add(x => x.Series, series).Add(x => x.BandCount, 3));
        var rects = Marks<ChartRect>(scene);
        var column = new ChartBand(_categories.Length);

        Assert.Equal(["One", "Two"], scene.YAxis!.Ticks.Select(tick => tick.Label));
        Assert.Empty(scene.Legend);
        Assert.Equal([1, 2, 3], Enumerable.Range(0, 3).Select(index => rects.Count(rect => rect.Y < 50 && rect.X == column.Step * index)));
        Assert.Equal(3, rects.Count(rect => rect.Color == ChartColor.Negative));
        Assert.Equal(_categories.Length, scene.Datums.Count);
        Assert.All(scene.Datums, datum => Assert.Equal(100, datum.Height));
    }

    [Fact]
    public void RadialHistogram_DrawsABarForEachBinThatHasValues()
    {
        var scene = Scene<TwRadialHistogramChart>(p => p.Add(x => x.Values, [1, 2, 2, 3, 9, double.NaN]).Add(x => x.BinCount, 8));
        var bars = Marks<ChartPath>(scene).Where(path => path.Color == ChartColor.Series(0)).ToList();

        Assert.Equal(4, bars.Count);
        Assert.Single(Marks<ChartPath>(scene), path => path.Color == ChartColor.Track);
        Assert.Equal(8, scene.Datums.Count);
        Assert.Equal("1 to 2", scene.Datums[0].Title);
        Assert.Equal(["1", "2", "1", "0", "0", "0", "0", "1"], scene.Datums.Select(datum => datum.Rows[0].Value));
    }

    [Fact]
    public void Beeswarm_DrawsEveryObservation_WithoutLeavingItsGroup()
    {
        ChartSample[] samples = [new("A", [.. Enumerable.Repeat(5.0, 60)]), new("B", [1, 2, 3]), new("C", [4, 4, 4])];
        var scene = Scene<TwBeeswarmChart>(p => p.Add(x => x.Samples, samples));
        var dots = Marks<ChartDot>(scene);
        var band = new ChartBand(samples.Length);

        Assert.Equal(66, dots.Count);
        Assert.Equal(samples.Length, scene.Datums.Count);
        Assert.All(dots.Take(60), dot => Assert.InRange(dot.X, 0, band.Step));
        Assert.Equal(3, dots.Skip(63).Select(dot => dot.X).Distinct().Count());
        Assert.Single(dots.Skip(60).Take(3).Select(dot => dot.X).Distinct());
    }

    [Fact]
    public void ViolinPlot_DrawsOneSilhouettePerGroup_WithItsMedianMarked()
    {
        var scene = Scene<TwViolinPlotChart>(p => p.Add(x => x.Samples, [.. _samples, new ChartSample("Flat", [7, 7])]));

        Assert.Equal(_samples.Length, Marks<ChartPath>(scene).Count);
        Assert.Equal(_samples.Length + 1, Marks<ChartDot>(scene).Count);
        Assert.Equal(_samples.Length + 1, scene.Datums.Count);
    }

    #endregion

    #region Graph diagrams

    [Fact]
    public void GraphDiagrams_ListEveryLinkInTheTable_AndInTheTooltipsOfItsNodes()
    {
        ChartScene[] scenes =
        [
            Scene<TwChordDiagramChart>(p => p.Add(x => x.Nodes, _nodes).Add(x => x.Links, _links)),
            Scene<TwArcDiagramChart>(p => p.Add(x => x.Nodes, _nodes).Add(x => x.Links, _links)),
            Scene<TwSankeyChart>(p => p.Add(x => x.Nodes, _nodes).Add(x => x.Links, _links)),
            Scene<TwNetworkDiagramChart>(p => p.Add(x => x.Nodes, _nodes).Add(x => x.Links, _links))
        ];

        Assert.All(scenes, scene =>
        {
            Assert.Equal(["Source", "Target", "Value"], scene.Table!.Headers);
            Assert.Equal(["Input", "Solar", "25"], scene.Table.Rows[0]);
            Assert.Equal(_links.Length, scene.Table.Rows.Count);

            var solar = scene.Datums.Single(datum => datum.Title == "Solar");
            Assert.Contains(solar.Rows, row => row is { Label: "→ Homes", Value: "25" });
            Assert.Contains(solar.Rows, row => row is { Label: "← Input", Value: "25" });
        });
    }

    [Fact]
    public void GraphDiagrams_LeaveOutLinksToMissingNodesAndRepeatedNodes()
    {
        ChartGraphNode[] nodes = [new("a", "A"), new("a", "Again"), new("b", "B"), new(" ", "Blank")];
        ChartGraphLink[] links = [new("a", "b", 2), new("a", "missing", 5), new("a", "a", 1), new("b", "a", double.NaN)];
        var scene = Scene<TwNetworkDiagramChart>(p => p.Add(x => x.Nodes, nodes).Add(x => x.Links, links));

        Assert.Equal(["A", "B"], scene.Datums.Select(datum => datum.Title));
        Assert.Single(scene.Table!.Rows);
        Assert.Single(Marks<ChartLine>(scene));
    }

    [Fact]
    public void ChordDiagram_SizesArcsAndRibbonsByValue_AndNamesNodesInTheLegend()
    {
        var scene = Scene<TwChordDiagramChart>(p => p.Add(x => x.Nodes, _nodes).Add(x => x.Links, _links));
        var paths = Marks<ChartPath>(scene);

        Assert.Equal(ChartShape.Square, scene.Shape);
        Assert.Equal(_nodes.Select(node => node.Label), scene.Legend.Select(item => item.Label));
        Assert.Equal(_links.Length + _nodes.Length, paths.Count);
        Assert.All(paths, path => Assert.True(path.Filled));
        Assert.Empty(Marks<ChartText>(scene));
        Assert.Empty(Marks<ChartDot>(scene));
        Assert.Equal(["100", "50", "150", "100"], scene.Datums.Select(datum => datum.Rows[0].Value));
        Assert.True(scene.Datums[2].Width > scene.Datums[1].Width);
    }

    [Fact]
    public void ArcDiagram_NamesEachNodeOnceUnderItsDot_AndRaisesLongerLinksHigher()
    {
        ChartGraphLink[] links = [new("in", "solar"), new("in", "homes")];
        var scene = Scene<TwArcDiagramChart>(p => p.Add(x => x.Nodes, _nodes).Add(x => x.Links, links));
        var dots = Marks<ChartDot>(scene);

        Assert.Equal(_nodes.Select(node => node.Label), scene.XAxis!.Ticks.Select(tick => tick.Label));
        Assert.Equal(scene.XAxis.Ticks.Select(tick => tick.Position), dots.Select(dot => dot.X));
        Assert.Empty(Marks<ChartText>(scene));
        Assert.Equal(2, Marks<ChartPath>(scene).Count);
        Assert.Equal(dots.Select(dot => (dot.X, dot.Y)), scene.Datums.Select(datum => (datum.X, datum.Y)));
    }

    [Fact]
    public void Sankey_SizesNodesAndRibbonsByFlow_AndNamesLinksByLabel()
    {
        var scene = Scene<TwSankeyChart>(p => p.Add(x => x.Nodes, _nodes).Add(x => x.Links, [.. _links, new ChartGraphLink("homes", "in", -5)]));
        var rects = Marks<ChartRect>(scene);
        var ribbons = scene.Datums.Skip(_nodes.Length).ToList();

        Assert.Equal(_nodes.Length, rects.Count);
        Assert.Equal(rects[0].Height, rects[3].Height, 3);
        Assert.Equal(3, rects[2].Height / rects[1].Height, 3);
        Assert.True(rects[0].X < rects[1].X && rects[1].X == rects[2].X && rects[2].X < rects[3].X);
        Assert.Equal(["Input → Solar", "Input → Grid", "Solar → Homes", "Grid → Homes"], ribbons.Select(ribbon => ribbon.Title));
        Assert.All(ribbons, ribbon => Assert.StartsWith("polygon(", ribbon.Clip));
        Assert.Equal([ChartAnchor.Start, ChartAnchor.Start, ChartAnchor.Start, ChartAnchor.End], Marks<ChartText>(scene).Select(text => text.Anchor));
    }

    [Fact]
    public void Sankey_DrawsRepeatedLinksSideBySide()
    {
        ChartGraphLink[] links = [new("in", "solar", 10), new("in", "solar", 10)];
        var scene = Scene<TwSankeyChart>(p => p.Add(x => x.Nodes, _nodes.Take(2).ToArray()).Add(x => x.Links, links));
        var ribbons = scene.Datums.Skip(2).ToList();

        Assert.Equal(2, ribbons.Count);
        Assert.Equal(ribbons[0].Y + ribbons[0].Height, ribbons[1].Y, 3);
    }

    [Fact]
    public void NetworkDiagram_ArrangesNodesTheSameWayEveryTime_AndApart()
    {
        ChartScene Build() => Scene<TwNetworkDiagramChart>(p => p.Add(x => x.Nodes, _nodes).Add(x => x.Links, _links));
        var dots = Marks<ChartDot>(Build());

        Assert.Equal(dots.Select(dot => (dot.X, dot.Y)), Marks<ChartDot>(Build()).Select(dot => (dot.X, dot.Y)));
        Assert.Equal(_nodes.Length, dots.Select(dot => (Math.Round(dot.X), Math.Round(dot.Y))).Distinct().Count());
        Assert.Equal(_nodes.Select(node => node.Label), Marks<ChartText>(Build()).Select(text => text.Text));
        Assert.All(Marks<ChartText>(Build()), text => Assert.Equal(ChartBaseline.Bottom, text.Baseline));
    }

    [Fact]
    public void Flowchart_SetsStepsOutLeftToRight_AndStopsEachArrowAtTheEdgeOfItsBox()
    {
        ChartGraphNode[] nodes = [new("submit", "Submit"), new("review", "Review"), new("publish", "Publish")];
        ChartGraphLink[] links = [new("submit", "review"), new("review", "publish")];
        var scene = Scene<TwFlowchart>(p => p.Add(x => x.Nodes, nodes).Add(x => x.Links, links));
        var boxes = Marks<ChartRect>(scene);
        var arrows = Marks<ChartLine>(scene);

        Assert.True(boxes[0].X < boxes[1].X && boxes[1].X < boxes[2].X);
        Assert.Single(boxes.Select(box => box.Y).Distinct());
        Assert.Equal(2, arrows.Count);
        Assert.Equal(boxes[0].X + boxes[0].Width, arrows[0].X1, 3);
        Assert.True(arrows[0].X2 < boxes[1].X);
        Assert.Equal(2, Marks<ChartPath>(scene).Count(path => path.Filled));
        Assert.Equal(["Source", "Target"], scene.Table!.Headers);
        Assert.Equal("→ Review", scene.Datums[0].Rows[0].Label);
    }

    #endregion

    #region Edge cases of the newer charts

    [Fact]
    public void ShapeDatum_WithNoOutline_AddsNothing()
    {
        var builder = new ChartSceneBuilder();
        builder.ShapeDatum([], "Shape", []);

        Assert.Empty(builder.Build().Datums);
    }

    [Theory]
    [InlineData(ChartHeight.Small, false)]
    [InlineData(ChartHeight.Large, false)]
    [InlineData(ChartHeight.Medium, true)]
    public void Beeswarm_KeepsItsDotsApart_AtEveryHeightAndOrientation(ChartHeight height, bool horizontal)
    {
        ChartSample[] samples = [new("A", [5, 5, 5, 5, 5]), new("B", [1, 9])];
        var scene = Scene<TwBeeswarmChart>(p => p.Add(x => x.Samples, samples).Add(x => x.Height, height).Add(x => x.Horizontal, horizontal));
        var swarm = Marks<ChartDot>(scene).Take(5).ToList();

        // Identical values share a position on the value axis, so each needs its own place across the group's slot.
        Assert.Equal(5, swarm.Select(dot => horizontal ? dot.Y : dot.X).Distinct().Count());
        Assert.Single(swarm.Select(dot => horizontal ? dot.X : dot.Y).Distinct());
        AssertInsidePlot(scene);
    }

    [Fact]
    public void Marimekko_LeavesOutEmptyCategories_AndLabelsOnlyWhatHasRoom()
    {
        ChartSeries[] series = [new("One", [60, 0, 3, null]), new("Two", [36, -5, 1, double.NaN])];
        var scene = Scene<TwMarimekkoChart>(p => p.Add(x => x.Categories, ["A", "B", "C", "D"]).Add(x => x.Series, series));

        Assert.Equal([96, 96, 4, 4], Marks<ChartRect>(scene).Select(rect => rect.Width));
        Assert.Equal(["A"], scene.XAxis!.Ticks.Select(tick => tick.Label));
        Assert.Equal(["62.5%", "37.5%"], Marks<ChartText>(scene).Select(text => text.Text));
        Assert.Equal(["A", "C"], scene.Datums.Select(datum => datum.Title));
        Assert.Equal("96%", scene.Datums[0].Rows[^1].Value);
    }

    [Fact]
    public void Marimekko_WithNothingToShare_DrawsNothing()
    {
        var scene = Scene<TwMarimekkoChart>(p => p.Add(x => x.Categories, ["A", "B"]).Add(x => x.Series, [new ChartSeries("One", [0, -4])]));

        Assert.Empty(scene.Marks);
        Assert.NotNull(scene.Table);
    }

    [Fact]
    public void CircularTreemap_PacksManySiblings_WithoutOverlap_AndIgnoresWhatHasNoValue()
    {
        ChartHierarchyNode[] roots =
        [
            new("Group", Children: [new("A", 30), new("B", 20), new("C", 12), new("D", 8), new("E", 5), new("F", 3), new("G", 2), new("Lost", double.NaN), new("Gone", -4)]),
            new("Solo", Children: [new("Only", 10)]),
            new("Empty", Children: [new("Nothing", 0)])
        ];
        var scene = Scene<TwCircularTreemapChart>(p => p.Add(x => x.Roots, roots).Add(x => x.LabelHeader, "Folder").Add(x => x.ValueHeader, "Size"));
        var leaves = scene.Datums;

        Assert.Equal(8, leaves.Count);
        Assert.Equal(["Group", "Solo"], scene.Legend.Select(item => item.Label));
        Assert.Equal(["Folder", "Size"], scene.Table!.Headers);
        Assert.Equal("Size", leaves[0].Rows[0].Label);
        Assert.Equal(Math.Sqrt(30.0 / 20), leaves[0].Width / leaves[1].Width, 2);
        AssertInsidePlot(scene);

        for (var first = 0; first < leaves.Count; first++)
        {
            for (var second = first + 1; second < leaves.Count; second++)
            {
                var distance = Math.Sqrt(Math.Pow(leaves[first].X - leaves[second].X, 2) + Math.Pow(leaves[first].Y - leaves[second].Y, 2));
                Assert.True(distance >= (leaves[first].Width + leaves[second].Width) / 2 - 0.01, $"{leaves[first].Title} overlaps {leaves[second].Title}");
            }
        }
    }

    [Fact]
    public void ConvexTreemap_SharesARowBetweenEqualTiles_AndGroupsChildrenInsideTheirParent()
    {
        ChartHierarchyNode[] roots = [new("Group", Children: [new("A", 1), new("B", 1), new("C", 1), new("D", 1)]), new("Other", 4)];
        var scene = Scene<TwConvexTreemapChart>(p => p.Add(x => x.Roots, roots).Add(x => x.LabelHeader, "Team").Add(x => x.ValueHeader, "Budget"));
        var group = scene.Datums.Where(datum => datum.Title.StartsWith("Group / ")).ToList();

        Assert.Equal(4, group.Count);
        Assert.Single(group.Select(tile => Math.Round(tile.Width * tile.Height, 3)).Distinct());
        Assert.Equal(4, group.Select(tile => (Math.Round(tile.X, 3), Math.Round(tile.Y, 3))).Distinct().Count());
        Assert.Single(group.Select(tile => Math.Round(tile.Y, 3)).Distinct());
        Assert.Equal(5000, scene.Datums.Single(datum => datum.Title == "Other").Width * scene.Datums.Single(datum => datum.Title == "Other").Height, 3);
        Assert.Equal(["Team", "Budget"], scene.Table!.Headers);
        Assert.Equal("50%", scene.Datums.Single(datum => datum.Title == "Other").Rows[1].Value);
    }

    [Fact]
    public void Dendrogram_OfLeavesAlone_SetsThemInARow()
    {
        var scene = Scene<TwDendrogramChart>(p => p.Add(x => x.Roots, [new ChartHierarchyNode("A", 1), new ChartHierarchyNode("B", 2, [])]));

        Assert.Equal(["A", "B"], scene.XAxis!.Ticks.Select(tick => tick.Label));
        Assert.All(scene.Datums, datum => Assert.Equal(50, datum.Y));
        Assert.Empty(Marks<ChartLine>(scene));
        Assert.Empty(Marks<ChartText>(scene));
    }

    [Fact]
    public void Dendrogram_NamesABranchNearTheRightEdge_OnItsLeft()
    {
        ChartHierarchyNode[] roots = [new("Left", Children: [new("A", 1), new("B", 1)]), new("Right", Children: [new("C", 1), new("D", 1)])];
        var names = Marks<ChartText>(Scene<TwDendrogramChart>(p => p.Add(x => x.Roots, roots)));

        Assert.Equal([ChartAnchor.Start, ChartAnchor.End], names.Select(name => name.Anchor));
        Assert.True(names[0].OffsetX > 0 && names[1].OffsetX < 0);
    }

    [Fact]
    public void Sunburst_EndsABranch_WhereItsChildrenHaveNoValue()
    {
        ChartHierarchyNode[] roots = [new("Kept", 5, [new("Nothing", 0)]), new("Deep", Children: [new("Leaf", 5)])];
        var scene = Scene<TwSunburstChart>(p => p.Add(x => x.Roots, roots).Add(x => x.Height, ChartHeight.Large));

        Assert.Equal(["Deep", "Deep / Leaf"], scene.Datums.Select(datum => datum.Title));
        Assert.Empty(scene.Legend);
        Assert.Equal(["Deep", "Leaf"], Marks<ChartText>(scene).Select(text => text.Text));
    }

    [Fact]
    public void SetDiagrams_DrawASingleSet_AsOneCircle()
    {
        var venn = Scene<TwVennDiagramChart>(p => p.Add(x => x.Sets, [new ChartSet("A", 5), new ChartSet("None", 0)]).Add(x => x.LabelHeader, "Set").Add(x => x.ValueHeader, "Members"));
        var euler = Scene<TwEulerDiagramChart>(p => p.Add(x => x.Sets, [new ChartSet("A", 5)]));

        Assert.All((ChartScene[])[venn, euler], scene =>
        {
            var circle = Assert.Single(scene.Datums);
            Assert.Equal((50, 50), (circle.X, circle.Y));
            Assert.Equal(["A"], Marks<ChartText>(scene).Select(text => text.Text));
        });
        Assert.Equal(["Set", "Members"], venn.Table!.Headers);
        Assert.Equal("Members", venn.Datums[0].Rows[0].Label);
    }

    [Fact]
    public void EulerDiagram_StacksSetsThatShareEverything()
    {
        ChartSet[] sets = [new("A", 40), new("B", 40), new("C", 40)];
        ChartSetIntersection[] shared = [new("A", "B", 40), new("A", "C", 40), new("B", "C", 40)];
        var scene = Scene<TwEulerDiagramChart>(p => p.Add(x => x.Sets, sets).Add(x => x.Intersections, shared));

        Assert.Single(scene.Datums.Select(datum => (Math.Round(datum.X, 3), Math.Round(datum.Y, 3), Math.Round(datum.Width, 3))).Distinct());
        Assert.Equal(6, Marks<ChartText>(scene).Count);
        AssertInsidePlot(scene);
    }

    [Fact]
    public void EulerDiagram_LeavesOutTheCount_WhereAnOverlapIsTooSmallToHoldIt()
    {
        ChartSet[] sets = [new("A", 50), new("B", 20)];
        var scene = Scene<TwEulerDiagramChart>(p => p.Add(x => x.Sets, sets).Add(x => x.Intersections, [new("A", "B", 0.0001)]));

        Assert.Equal(["A", "B"], Marks<ChartText>(scene).Select(text => text.Text));
        Assert.Equal(3, scene.Table!.Rows.Count);
    }

    [Fact]
    public void ChordDiagram_WithoutLinks_GivesEveryNodeAnEqualArc()
    {
        var scene = Scene<TwChordDiagramChart>(p => p.Add(x => x.Nodes, _nodes).Add(x => x.SourceHeader, "From").Add(x => x.TargetHeader, "To").Add(x => x.ValueHeader, "Flow"));
        var alone = Scene<TwChordDiagramChart>(p => p.Add(x => x.Nodes, _nodes.Take(1).ToArray()));

        Assert.Equal(_nodes.Length, Marks<ChartPath>(scene).Count);
        Assert.Equal(_nodes.Length, scene.Datums.Count);
        Assert.Equal(["From", "To", "Flow"], scene.Table!.Headers);
        Assert.Empty(scene.Table.Rows);
        Assert.Equal((96, 96), (Math.Round(Assert.Single(alone.Datums).Width), Math.Round(alone.Datums[0].Height)));
    }

    [Fact]
    public void Sankey_WithoutFlows_StacksItsNodesInOneColumn()
    {
        var scene = Scene<TwSankeyChart>(p => p.Add(x => x.Nodes, _nodes).Add(x => x.ValueHeader, "Flow"));
        var rects = Marks<ChartRect>(scene);

        Assert.Equal(_nodes.Length, rects.Count);
        Assert.Single(rects.Select(rect => rect.X).Distinct());
        Assert.Equal(_nodes.Length, scene.Datums.Count);
        Assert.All(Marks<ChartText>(scene), text => Assert.Equal(ChartAnchor.Start, text.Anchor));
        AssertInsidePlot(scene);
    }

    [Fact]
    public void NetworkDiagram_WithASingleNode_PutsItInTheMiddle()
    {
        var scene = Scene<TwNetworkDiagramChart>(p => p.Add(x => x.Nodes, _nodes.Take(1).ToArray()));
        var dot = Assert.Single(Marks<ChartDot>(scene));

        Assert.Equal((50, 50), (dot.X, dot.Y));
        Assert.Equal(ChartAnchor.Middle, Assert.Single(Marks<ChartText>(scene)).Anchor);
    }

    [Fact]
    public void DensityPlot_WithOneGroup_HasNoLegend_AndHidesTheCurvesOfHiddenGroups()
    {
        var single = Scene<TwDensityPlotChart>(p => p.Add(x => x.Samples, [_samples[0], new ChartSample("Nothing usable", [double.NaN])]).Add(x => x.LabelHeader, "Service"));
        Assert.Empty(single.Legend);
        Assert.Single(single.Datums);
        Assert.Equal("Service", single.Table!.Headers[0]);

        var cut = TestContext.Render<TwDensityPlotChart>(p => p.Add(x => x.Samples, _samples));
        foreach (var index in Enumerable.Range(0, _samples.Length))
        {
            cut.FindAll("button[aria-pressed]")[index].Click();
        }

        var hidden = cut.Instance.CreateScene();
        Assert.Empty(hidden.Marks);
        Assert.All(hidden.Legend, item => Assert.True(item.Hidden));
        Assert.Equal(_samples.Length, hidden.Table!.Rows.Count);
    }

    [Fact]
    public void RadialHistogram_OfOneRepeatedValue_FillsASingleBin()
    {
        var scene = Scene<TwRadialHistogramChart>(p => p.Add(x => x.Values, [7, 7, 7]).Add(x => x.BinCount, 4).Add(x => x.RangeLabel, "Hour").Add(x => x.CountLabel, "Calls"));

        Assert.Equal(["3", "0", "0", "0"], scene.Datums.Select(datum => datum.Rows[0].Value));
        Assert.Equal("Calls", scene.Datums[0].Rows[0].Label);
        Assert.Equal(["Hour", "Calls"], scene.Table!.Headers);
    }

    [Fact]
    public void CategoricalScatter_WithNothingToSizeBy_DrawsEqualDots_AndLeavesOutValuesThatAreNotNumbers()
    {
        ChartCategoricalPoint[] points = [new("A", "X", 0), new("B", "Y", -3), new("C", "Z", double.NaN)];
        var scene = Scene<TwCategoricalScatterChart>(p => p.Add(x => x.Points, points).Add(x => x.ValueTitle, "Orders"));

        Assert.Equal([8, 8], Marks<ChartDot>(scene).Select(dot => dot.Diameter));
        Assert.Equal(["A", "B"], scene.XAxis!.Ticks.Select(tick => tick.Label));
        Assert.Equal(["X", "Y", "Orders"], scene.Table!.Headers);
        Assert.Equal("Orders", scene.Datums[0].Rows[2].Label);
    }

    [Fact]
    public void PointSurfaceCharts_LeaveOutPointsWithoutAPosition_AndUseTheirOwnLabels()
    {
        ChartPointSeries[] points = [new("Sensors", [new(0, 0, "Door", 1), new(4, 0, Size: 2), new(0, 4, Size: 3), new(double.NaN, 1, Size: 4), new(1, double.PositiveInfinity, Size: 5), new(2, 2, Size: double.NaN)])];
        var hexbin = Scene<TwHexbinChart>(p => p.Add(x => x.Series, points).Add(x => x.CountLabel, "Readings"));
        var contour = Scene<TwContourChart>(p => p.Add(x => x.Series, points).Add(x => x.ValueTitle, "Heat"));

        Assert.Equal(4, hexbin.Datums.Sum(datum => int.Parse(datum.Rows[0].Value)));
        Assert.Equal(["X", "Y", "Readings"], hexbin.Table!.Headers);
        Assert.Equal(["Door", "Sensors", "Sensors"], contour.Datums.Select(datum => datum.Title));
        Assert.Equal(["X", "Y", "Heat"], contour.Table!.Headers);
        Assert.Equal("Heat", contour.Datums[0].Rows[2].Label);
    }

    [Fact]
    public void StreamGraph_CountsMissingValuesAsZero_AndDrawsNothingWithoutATotal()
    {
        var scene = Scene<TwStreamGraphChart>(p => p.Add(x => x.Categories, _categories).Add(x => x.Series, [new ChartSeries("One", [4, null, double.NaN]), new ChartSeries("Two", [4, 8, 2])]));
        var empty = Scene<TwStreamGraphChart>(p => p.Add(x => x.Categories, _categories).Add(x => x.Series, [new ChartSeries("One", [0, null, -2])]));

        Assert.Equal(2, Marks<ChartPath>(scene).Count);
        Assert.Equal(_categories.Length, scene.Datums.Count);
        Assert.Empty(empty.Marks);
        Assert.NotNull(empty.Table);
    }

    [Fact]
    public void BumpArea_DrawsALoneSeriesAndLateArrivals_AndNothingWithoutATotal()
    {
        var alone = Scene<TwBumpAreaChart>(p => p.Add(x => x.Categories, _categories).Add(x => x.Series, [new ChartSeries("One", [2, 4, 3])]));
        var late = Scene<TwBumpAreaChart>(p => p.Add(x => x.Categories, _categories).Add(x => x.Series, [new ChartSeries("One", [2, 4, 3]), new ChartSeries("Late", [null, null, 5])]));
        var empty = Scene<TwBumpAreaChart>(p => p.Add(x => x.Categories, _categories).Add(x => x.Series, [new ChartSeries("One", [0, null, -1])]));

        Assert.Single(Marks<ChartPath>(alone));
        Assert.All(alone.Datums, datum => Assert.Equal(50, datum.Y, 3));
        Assert.Equal(2, Marks<ChartPath>(late).Count);
        Assert.Equal(["One, A", "One, B", "Late, C", "One, C"], late.Datums.Select(datum => datum.Title));
        Assert.Empty(empty.Marks);
        AssertInsidePlot(late);
    }

    [Fact]
    public void Barcode_NamesItsColumns_FromItsLabels()
    {
        var scene = Scene<TwBarcodeChart>(p => p.Add(x => x.Values, [3, 3, 8]).Add(x => x.ValueLabel, "Items").Add(x => x.CountLabel, "Orders"));

        Assert.Equal(["Items", "Orders"], scene.Table!.Headers);
        Assert.Equal("Orders", scene.Datums[0].Rows[0].Label);
        Assert.Equal(["3", "2"], scene.Table.Rows[0]);
    }

    [Fact]
    public void WordCloud_WithNoPositiveValues_DrawsNothing()
    {
        var scene = Scene<TwWordCloudChart>(p => p.Add(x => x.Data, [new ChartValue("zero", 0), new ChartValue("minus", -3), new ChartValue(" ", 5)]));

        Assert.Empty(scene.Marks);
        Assert.Equal(3, scene.Table!.Rows.Count);
    }

    #endregion

    #region Bar and column

    [Fact]
    public void Column_Grouped_DrawsOneBarPerValue_SideBySide()
    {
        var scene = CategoryScene<TwColumnChart>();

        Assert.Equal(3, Bars(scene, 0).Count);
        Assert.Equal(3, Bars(scene, 1).Count);
        Assert.True(Bars(scene, 0)[0].X < Bars(scene, 1)[0].X);
        Assert.Equal(["A", "B", "C"], scene.XAxis!.Ticks.Select(tick => tick.Label));
        Assert.Equal(3, scene.Datums.Count);
        Assert.Equal(["One", "Two"], scene.Legend.Select(item => item.Label));
    }

    [Fact]
    public void Column_NegativeValue_HangsBelowTheBaseline()
    {
        var scene = CategoryScene<TwColumnChart>();

        var negative = Bars(scene, 1)[1];
        var positive = Bars(scene, 0)[1];
        Assert.Equal(ChartCorner.Bottom, negative.Corner);
        Assert.Equal(positive.Y + positive.Height, negative.Y, 6);
    }

    [Fact]
    public void Bar_IsHorizontal_WithCategoriesOnTheLeft()
    {
        var scene = CategoryScene<TwBarChart>();

        Assert.Equal(["A", "B", "C"], scene.YAxis!.Ticks.Select(tick => tick.Label));
        Assert.Equal(ChartCorner.Right, Bars(scene, 0)[0].Corner);
        Assert.Equal(100, scene.Datums[0].Width);
    }

    [Fact]
    public void Column_Stacked_PilesSeriesOnTopOfEachOther_AndRoundsOnlyTheDataEnd()
    {
        var scene = CategoryScene<TwColumnChart>(p => p.Add(x => x.Stacking, ChartStacking.Stacked));

        var bottom = Bars(scene, 0)[0];
        var top = Bars(scene, 1)[0];
        Assert.Equal(bottom.Y, top.Y + top.Height, 6);
        Assert.Equal(bottom.X, top.X, 6);
        Assert.Equal(ChartCorner.None, bottom.Corner);
        Assert.Equal(ChartCorner.Top, top.Corner);
        Assert.Equal(ChartCorner.Bottom, Bars(scene, 1)[1].Corner);
    }

    [Fact]
    public void Column_Percent_FillsEveryBar_AndLabelsTheAxisInPercent()
    {
        ChartSeries[] series = [new("One", [10, 30]), new("Two", [30, 30])];
        var scene = Scene<TwColumnChart>(p => p.Add(x => x.Categories, ["A", "B"]).Add(x => x.Series, series).Add(x => x.Stacking, ChartStacking.Percent));

        Assert.Equal("100%", scene.YAxis!.Ticks[^1].Label);
        Assert.Equal(25, Bars(scene, 0)[0].Height, 6);
        Assert.Equal(75, Bars(scene, 1)[0].Height, 6);
        Assert.Equal("10", scene.Datums[0].Rows[0].Value);
    }

    [Fact]
    public void Column_Percent_WithAnEmptyCategory_DoesNotDivideByZero()
    {
        var scene = Scene<TwColumnChart>(p => p.Add(x => x.Categories, ["A"]).Add(x => x.Series, [new ChartSeries("One", [0])]).Add(x => x.Stacking, ChartStacking.Percent));

        Assert.Empty(Bars(scene, 0));
    }

    [Fact]
    public void Column_ShowValues_LabelsEachBar()
    {
        var scene = CategoryScene<TwColumnChart>(p => p.Add(x => x.ShowValues, true));

        Assert.Equal(["10", "5", "20", "-5", "30", "15"], Marks<ChartText>(scene).Select(text => text.Text));
    }

    [Fact]
    public void Column_MissingValues_AreSkipped()
    {
        var scene = Scene<TwColumnChart>(p => p.Add(x => x.Categories, _categories).Add(x => x.Series, [new ChartSeries("One", [1, null])]));

        Assert.Single(Bars(scene, 0));
        Assert.Empty(scene.Legend);
        Assert.Empty(scene.Datums[2].Rows);
    }

    #endregion

    #region Lollipop, dot plot, dumbbell, range, bullet

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Lollipop_DrawsAStemAndADotPerValue(bool horizontal)
    {
        var scene = CategoryScene<TwLollipopChart>(p => p.Add(x => x.Horizontal, horizontal).Add(x => x.ShowValues, true));

        Assert.Equal(6, Marks<ChartDot>(scene).Count);
        Assert.Equal(6, Marks<ChartLine>(scene).Count(line => line.Color.Role == ChartColorRole.Series));
        Assert.Equal(6, Marks<ChartText>(scene).Count);
        Assert.Equal(horizontal, scene.YAxis!.Ticks[0].Label == "A");
    }

    [Fact]
    public void DotPlot_HasNoConnector_AndDumbbellDoes()
    {
        var dots = CategoryScene<TwDotPlotChart>();
        var dumbbell = CategoryScene<TwDumbbellChart>();

        Assert.Equal(6, Marks<ChartDot>(dots).Count);
        Assert.DoesNotContain(Marks<ChartLine>(dots), line => line.Stroke == ChartStroke.Thick);
        Assert.Equal(3, Marks<ChartLine>(dumbbell).Count(line => line.Stroke == ChartStroke.Thick && line.Color == ChartColor.Track));
    }

    [Fact]
    public void DotPlot_Vertical_PutsCategoriesAlongTheBottom()
    {
        Assert.Equal("A", CategoryScene<TwDotPlotChart>(p => p.Add(x => x.Vertical, true)).XAxis!.Ticks[0].Label);
        Assert.Equal("A", CategoryScene<TwDotPlotChart>().YAxis!.Ticks[0].Label);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Range_DrawsAFloatingBarFromLowToHigh(bool vertical)
    {
        var scene = Scene<TwRangeChart>(p => p.Add(x => x.Ranges, [new ChartRange("A", 20, 60), new ChartRange("B", 0, 100)]).Add(x => x.Vertical, vertical));

        var bar = Bars(scene, 0)[0];
        Assert.Equal(ChartCorner.All, bar.Corner);
        Assert.Equal(40, vertical ? bar.Height : bar.Width, 6);
        Assert.Equal(["Low", "High"], scene.Datums[0].Rows.Select(row => row.Label));
        Assert.Equal(["A", "20", "60"], scene.Table!.Rows[0]);
    }

    [Fact]
    public void Bullet_DrawsRangesBarAndTarget_OnItsOwnScale()
    {
        var scene = Scene<TwBulletChart>(p => p.Add(x => x.Items, [new ChartBullet("Revenue", 50, 80, [40, 70, 100]), new ChartBullet("Profit", 5, 4)]));

        Assert.Equal(4, Marks<ChartRect>(scene).Count(rect => rect.Color == ChartColor.Track));
        Assert.Equal(2, Marks<ChartRect>(scene).Count(rect => rect.Color == ChartColor.Ink));
        Assert.Equal(50, Bars(scene, 0)[0].Width, 6);
        Assert.Equal(100, Bars(scene, 0)[1].Width, 6);
        Assert.Equal(["Actual", "Target"], scene.Legend.Select(item => item.Label));
        Assert.Equal("Revenue: Actual 50, Target 80", scene.Datums[0].AriaLabel);
    }

    #endregion

    #region Waterfall

    [Fact]
    public void Waterfall_FloatsEachChange_AndEndsWithTheTotal()
    {
        ChartValue[] data = [new("Sales", 100), new("Costs", -40)];
        var scene = Scene<TwWaterfallChart>(p => p.Add(x => x.Data, data).Add(x => x.ShowValues, true));

        var rects = Marks<ChartRect>(scene);
        Assert.Equal([ChartColor.Positive, ChartColor.Negative, ChartColor.Total], rects.Select(rect => rect.Color));
        Assert.Equal(rects[0].Y, rects[1].Y, 6);
        Assert.Equal(60, rects[2].Height, 6);
        Assert.Equal(["Sales", "Costs", "Total"], scene.XAxis!.Ticks.Select(tick => tick.Label));
        Assert.Equal("Total: Total 60", scene.Datums[^1].AriaLabel);
        Assert.Equal(["+100", "-40"], Marks<ChartText>(scene).Select(text => text.Text));
        Assert.Equal(["Increase", "Decrease", "Total"], scene.Legend.Select(item => item.Label));
    }

    [Fact]
    public void Waterfall_ShowTotal_False_OmitsTheTotal()
    {
        var scene = ValueScene<TwWaterfallChart>(p => p.Add(x => x.ShowTotal, false));

        Assert.Equal(3, scene.Datums.Count);
        Assert.DoesNotContain(Marks<ChartRect>(scene), rect => rect.Color == ChartColor.Total);
        Assert.Equal(2, scene.Legend.Count);
    }

    #endregion

    #region Line and area

    [Fact]
    public void Line_DrawsAPathAndMarkersPerSeries_WithACrosshairPerCategory()
    {
        var scene = CategoryScene<TwLineChart>();

        Assert.Equal(2, Marks<ChartPath>(scene).Count);
        Assert.All(Marks<ChartPath>(scene), path => Assert.False(path.Filled));
        Assert.Equal(6, Marks<ChartDot>(scene).Count);
        Assert.All(scene.Datums, datum => Assert.Equal(ChartHover.Crosshair, datum.Hover));
    }

    [Fact]
    public void Line_MissingValue_BreaksTheLine()
    {
        ChartSeries[] series = [new("One", [1, 2, null, 4, 5])];
        var scene = Scene<TwLineChart>(p => p.Add(x => x.Categories, ["A", "B", "C", "D", "E"]).Add(x => x.Series, series));

        Assert.Equal(2, Marks<ChartPath>(scene).Count);
        Assert.Equal(4, Marks<ChartDot>(scene).Count);
    }

    [Theory]
    [InlineData(ChartCurve.Linear, 'L')]
    [InlineData(ChartCurve.Smooth, 'C')]
    public void Line_Curve_ChangesHowPointsAreJoined(ChartCurve curve, char command)
    {
        var scene = CategoryScene<TwLineChart>(p => p.Add(x => x.Curve, curve));

        Assert.Contains(command, Marks<ChartPath>(scene)[0].Data);
    }

    [Fact]
    public void Line_ShowMarkers_DefaultsOffForLongSeries_AndCanBeForced()
    {
        string[] categories = [.. Enumerable.Range(0, 20).Select(i => i.ToString())];
        ChartSeries[] series = [new("One", [.. Enumerable.Range(0, 20).Select(i => (double?)i)])];

        Assert.Empty(Marks<ChartDot>(Scene<TwLineChart>(p => p.Add(x => x.Categories, categories).Add(x => x.Series, series))));
        Assert.Equal(20, Marks<ChartDot>(Scene<TwLineChart>(p => p.Add(x => x.Categories, categories).Add(x => x.Series, series).Add(x => x.ShowMarkers, true))).Count);
        Assert.Empty(Marks<ChartDot>(CategoryScene<TwLineChart>(p => p.Add(x => x.ShowMarkers, false))));
    }

    [Fact]
    public void Area_AddsAFaintFillUnderEachLine()
    {
        var scene = CategoryScene<TwAreaChart>();

        var fills = Marks<ChartPath>(scene).Where(path => path.Filled).ToList();
        Assert.Equal(2, fills.Count);
        Assert.All(fills, fill => Assert.Equal(0.12, fill.Opacity));
        Assert.Equal(2, Marks<ChartPath>(scene).Count(path => !path.Filled));
    }

    [Fact]
    public void Area_Stacked_FillsSolidLayersSeparatedByAGap_ScaledToTheTotal()
    {
        var scene = CategoryScene<TwAreaChart>(p => p.Add(x => x.Stacked, true));

        var paths = Marks<ChartPath>(scene);
        Assert.Equal(2, paths.Count);
        Assert.All(paths, path => Assert.True(path.Filled && path.Outline));
        Assert.Empty(Marks<ChartDot>(scene));
        Assert.Equal("50", scene.YAxis!.Ticks[^1].Label);
    }

    #endregion

    #region Slope, bump, parallel coordinates

    [Fact]
    public void Slope_LabelsBothEndsOfEachLine()
    {
        var scene = Scene<TwSlopeChart>(p => p.Add(x => x.Categories, ["2020", "2025"]).Add(x => x.Series, [new ChartSeries("Aster", [34, 41]), new ChartSeries("Birch", [28, null])]));

        Assert.Equal(["Aster 34", "41", "Birch 28"], Marks<ChartText>(scene).Select(text => text.Text));
        Assert.Equal([25, 75], scene.XAxis!.Ticks.Select(tick => tick.Position));
        Assert.Equal(2, Marks<ChartPath>(scene).Count);
        Assert.Equal(3, scene.Datums.Count);
    }

    [Fact]
    public void Slope_WithOneCategory_CentersIt()
    {
        var scene = Scene<TwSlopeChart>(p => p.Add(x => x.Categories, ["2020"]).Add(x => x.Series, [new ChartSeries("Aster", [34])]));

        Assert.Equal(50, scene.XAxis!.Ticks[0].Position);
    }

    [Fact]
    public void Bump_PlacesTheHighestValueFirst()
    {
        var scene = CategoryScene<TwBumpChart>();

        Assert.Equal(["1", "2"], scene.YAxis!.Ticks.Select(tick => tick.Label));
        Assert.Equal("One, A: Rank 1, One 10", scene.Datums[0].AriaLabel);
        Assert.Equal("Two, A: Rank 2, Two 5", scene.Datums[1].AriaLabel);
        Assert.Equal(["One", "Two"], Marks<ChartText>(scene).Select(text => text.Text));
        Assert.True(scene.Datums[0].Y < scene.Datums[1].Y);
    }

    [Fact]
    public void ParallelCoordinates_GivesEachVariableItsOwnScale()
    {
        var scene = CategoryScene<TwParallelCoordinatesChart>();

        Assert.Equal(3, Marks<ChartLine>(scene).Count(line => line.Color == ChartColor.Axis));
        Assert.Equal(2, Marks<ChartPath>(scene).Count);
        Assert.Equal(["10", "5", "20", "-5", "30", "15"], Marks<ChartText>(scene).Select(text => text.Text));
    }

    #endregion

    #region Heatmap and matrix

    [Fact]
    public void Heatmap_ShadesCellsByValue_WithSeriesAsRows()
    {
        var scene = CategoryScene<TwHeatmapChart>(p => p.Add(x => x.ShowValues, true));

        var cells = Marks<ChartRect>(scene);
        Assert.Equal(6, cells.Count);
        Assert.All(cells, cell => Assert.Equal(ChartColor.Sequential, cell.Color));
        Assert.Equal(1, cells.Max(cell => cell.Opacity), 6);
        Assert.Equal(0.1, cells.Min(cell => cell.Opacity), 6);
        Assert.Equal(["One", "Two"], scene.YAxis!.Ticks.Select(tick => tick.Label));
        Assert.Empty(scene.Legend);
        Assert.Equal(ChartTextTone.Inverse, Marks<ChartText>(scene).Single(text => text.Text == "30").Tone);
        Assert.Equal(ChartTextTone.Primary, Marks<ChartText>(scene).Single(text => text.Text == "-5").Tone);
        Assert.Equal("One, A: Value 10", scene.Datums[0].AriaLabel);
    }

    [Fact]
    public void Heatmap_WithIdenticalValues_UsesFullIntensity()
    {
        var scene = Scene<TwHeatmapChart>(p => p.Add(x => x.Categories, ["A"]).Add(x => x.Series, [new ChartSeries("One", [4]), new ChartSeries("Two", [null])]));

        Assert.Equal(1, Assert.Single(Marks<ChartRect>(scene)).Opacity);
    }

    [Fact]
    public void Matrix_SizesADotByValue()
    {
        var dots = Marks<ChartDot>(CategoryScene<TwMatrixChart>());

        Assert.Equal(6, dots.Count);
        Assert.Equal(32, dots.Max(dot => dot.Diameter), 6);
        Assert.Equal(8, dots.Min(dot => dot.Diameter), 6);
    }

    #endregion

    #region Radar and small multiples

    [Fact]
    public void Radar_DrawsAShapePerSeries_OnASquarePlot()
    {
        var scene = CategoryScene<TwRadarChart>();

        Assert.Equal(ChartShape.Square, scene.Shape);
        Assert.Equal(2, Marks<ChartPath>(scene).Count(path => path.Filled));
        Assert.Equal(6, Marks<ChartDot>(scene).Count);
        Assert.Equal(["A", "B", "C"], Marks<ChartText>(scene).Select(text => text.Text));
        Assert.Equal(6, scene.Datums.Count);
    }

    [Fact]
    public void Radar_WithFewerThanThreeCategories_DrawsNothing()
    {
        var scene = Scene<TwRadarChart>(p => p.Add(x => x.Categories, ["A", "B"]).Add(x => x.Series, _series));

        Assert.Empty(scene.Marks);
    }

    [Theory]
    [InlineData(SmallMultipleKind.Column, 6, 0)]
    [InlineData(SmallMultipleKind.Line, 0, 2)]
    [InlineData(SmallMultipleKind.Area, 0, 4)]
    public void SmallMultiples_DrawsOnePanelPerSeries(SmallMultipleKind kind, int rects, int paths)
    {
        var scene = CategoryScene<TwSmallMultiplesChart>(p => p.Add(x => x.Kind, kind));

        Assert.Equal(rects, Marks<ChartRect>(scene).Count);
        Assert.Equal(paths, Marks<ChartPath>(scene).Count);
        Assert.Equal(["One", "Two"], Marks<ChartText>(scene).Where(text => text.Strong).Select(text => text.Text));
        Assert.Empty(scene.Legend);
        Assert.Equal(6, scene.Datums.Count);
    }

    [Fact]
    public void SmallMultiples_Columns_WrapsPanelsOntoRows()
    {
        var scene = CategoryScene<TwSmallMultiplesChart>(p => p.Add(x => x.Columns, 1));

        var titles = Marks<ChartText>(scene).Where(text => text.Strong).ToList();
        Assert.Equal(titles[0].X, titles[1].X);
        Assert.True(titles[1].Y > titles[0].Y);
    }

    #endregion

    #region Scatter and quadrant

    private static readonly ChartPointSeries[] _points =
    [
        new("Audio", [new(10, 20, "Buds", 100), new(30, 40, null, 25)]),
        new("Wearables", [new(20, 60)])
    ];

    [Fact]
    public void Scatter_DrawsADotPerPoint_AndSizesBubblesByArea()
    {
        var scene = Scene<TwScatterChart>(p => p.Add(x => x.Series, _points).Add(x => x.XTitle, "Price").Add(x => x.YTitle, "Rating"));

        var dots = Marks<ChartDot>(scene);
        Assert.Equal([40, 25, 10], dots.Select(dot => dot.Diameter));
        Assert.Equal("Price", scene.XAxis!.Title);
        Assert.Equal("Rating", scene.YAxis!.Title);
        Assert.Equal("Buds: Price 10, Rating 20, Size 100", scene.Datums[0].AriaLabel);
        Assert.Equal("Wearables: Price 20, Rating 60", scene.Datums[2].AriaLabel);
        Assert.Equal(["Series", "Label", "Price", "Rating", "Size"], scene.Table!.Headers);
        Assert.Empty(Marks<ChartPath>(scene));
        Assert.Empty(Marks<ChartText>(scene));
    }

    [Fact]
    public void Scatter_Connect_JoinsEachSeries_AndShowLabels_NamesPoints()
    {
        var scene = Scene<TwScatterChart>(p => p.Add(x => x.Series, _points).Add(x => x.Connect, true).Add(x => x.ShowLabels, true));

        Assert.Equal(2, Marks<ChartPath>(scene).Count);
        Assert.Equal("Buds", Assert.Single(Marks<ChartText>(scene)).Text);
    }

    [Fact]
    public void Scatter_WithoutSizes_OmitsTheSizeColumn()
    {
        var scene = Scene<TwScatterChart>(p => p.Add(x => x.Series, [new ChartPointSeries("A", [new ChartPoint(1, 2)])]));

        Assert.Equal(4, scene.Table!.Headers.Count);
        Assert.Empty(scene.Legend);
    }

    [Fact]
    public void Quadrant_AddsDividersAndCornerLabels_AndLabelsPointsByDefault()
    {
        var scene = Scene<TwQuadrantChart>(p => p
            .Add(x => x.Series, _points)
            .Add(x => x.XThreshold, 20)
            .Add(x => x.YThreshold, 40)
            .Add(x => x.TopLeftLabel, "Quick wins")
            .Add(x => x.TopRightLabel, "Major")
            .Add(x => x.BottomLeftLabel, "Fill-ins")
            .Add(x => x.BottomRightLabel, "Reconsider"));

        var dividers = Marks<ChartLine>(scene).Where(line => line.Color == ChartColor.Axis).ToList();
        Assert.Equal(2, dividers.Count);
        Assert.Equal(50, dividers[0].X1, 6);
        Assert.Equal(["Quick wins", "Major", "Fill-ins", "Reconsider", "Buds"], Marks<ChartText>(scene).Select(text => text.Text));
    }

    [Fact]
    public void Quadrant_WithoutThresholds_DividesInTheMiddle()
    {
        var dividers = Marks<ChartLine>(Scene<TwQuadrantChart>(p => p.Add(x => x.Series, _points))).Where(line => line.Color == ChartColor.Axis).ToList();

        Assert.Equal((50, 50), (dividers[0].X1, dividers[1].Y1));
    }

    #endregion

    #region Parts of a whole

    [Fact]
    public void Pie_DrawsASliceAndLegendEntryPerValue_WithShares()
    {
        var scene = ValueScene<TwPieChart>();

        Assert.Equal(ChartShape.Square, scene.Shape);
        Assert.Equal(3, Marks<ChartPath>(scene).Count);
        Assert.All(Marks<ChartPath>(scene), path => Assert.True(path.Filled && path.Outline));
        Assert.Equal(["50%", "30%", "20%"], Marks<ChartText>(scene).Select(text => text.Text));
        Assert.Equal(["Search", "Direct", "Social"], scene.Legend.Select(item => item.Label));
        Assert.Equal("Search: Value 50, Share 50%", scene.Datums[0].AriaLabel);
    }

    [Fact]
    public void Pie_FoldsSmallValuesIntoOther_SoColorsAreNeverReused()
    {
        ChartValue[] data = [.. Enumerable.Range(1, 12).Select(i => new ChartValue($"Item {i}", i))];
        var scene = Scene<TwPieChart>(p => p.Add(x => x.Data, data).Add(x => x.OtherLabel, "Rest"));

        Assert.Equal(8, scene.Legend.Count);
        Assert.Equal("Rest", scene.Legend[^1].Label);
        Assert.Equal("Rest: Value 15, Share 19.2%", scene.Datums[^1].AriaLabel);
        Assert.Equal(12, scene.Table!.Rows.Count);
    }

    [Fact]
    public void Pie_SkipsValuesWithNoShare_AndHidesLabelsOnThinSlices()
    {
        var scene = Scene<TwPieChart>(p => p.Add(x => x.Data, [new ChartValue("A", 96), new ChartValue("B", 4), new ChartValue("C", 0), new ChartValue("D", -3)]));

        Assert.Equal(2, Marks<ChartPath>(scene).Count);
        Assert.Equal("96%", Assert.Single(Marks<ChartText>(scene)).Text);
        Assert.Empty(Scene<TwPieChart>(p => p.Add(x => x.Data, [new ChartValue("C", 0)])).Marks);
    }

    [Fact]
    public void Pie_Donut_ShowsCenterText_AndSemicircleSweepsHalf()
    {
        var donut = ValueScene<TwPieChart>(p => p.Add(x => x.InnerRadius, 0.6).Add(x => x.CenterText, "100").Add(x => x.ShowPercentages, false));
        Assert.Equal("100", Assert.Single(Marks<ChartText>(donut)).Text);
        Assert.Equal(2, Marks<ChartPath>(donut)[0].Data.Count(c => c == 'A'));

        var half = ValueScene<TwPieChart>(p => p.Add(x => x.Semicircle, true).Add(x => x.InnerRadius, 0.6).Add(x => x.CenterText, "100"));
        Assert.StartsWith("M2 74", Marks<ChartPath>(half)[0].Data);
        Assert.Equal(ChartBaseline.Bottom, Marks<ChartText>(half)[^1].Baseline);
    }

    [Fact]
    public void Nightingale_UsesEqualAngles_AndAreaForValue()
    {
        var scene = ValueScene<TwNightingaleChart>();

        Assert.Equal(3, Marks<ChartPath>(scene).Count);
        Assert.StartsWith("M50 4", Marks<ChartPath>(scene)[0].Data);
        Assert.Equal(3, scene.Legend.Count);
        Assert.Empty(Scene<TwNightingaleChart>(p => p.Add(x => x.Data, [new ChartValue("A", 0)])).Marks);
    }

    [Fact]
    public void RadialBar_DrawsATrackAndAnArcPerValue()
    {
        var scene = Scene<TwRadialBarChart>(p => p.Add(x => x.Data, [new ChartValue("A", 100), new ChartValue("B", 0)]));

        Assert.Equal(2, Marks<ChartPath>(scene).Count(path => path.Color == ChartColor.Track));
        Assert.Single(Marks<ChartPath>(scene), path => path.Color == ChartColor.Series(0));
        Assert.Equal(["A", "B"], Marks<ChartText>(scene).Select(text => text.Text));
    }

    [Fact]
    public void Waffle_AlwaysFillsOneHundredCells()
    {
        var scene = Scene<TwWaffleChart>(p => p.Add(x => x.Data, [new ChartValue("A", 1), new ChartValue("B", 1), new ChartValue("C", 1)]));

        var cells = Marks<ChartRect>(scene);
        Assert.Equal(100, cells.Count);
        Assert.Equal([34, 33, 33], Enumerable.Range(0, 3).Select(slot => cells.Count(cell => cell.Color == ChartColor.Series(slot))));
        Assert.Equal("A (33.3%)", scene.Legend[0].Label);
        Assert.Equal(3, scene.Datums.Count);
        Assert.Empty(Scene<TwWaffleChart>(p => p.Add(x => x.Data, [new ChartValue("A", 0)])).Marks);
    }

    [Fact]
    public void Waffle_TinyShare_GetsALegendEntryButNoDataPoint()
    {
        var scene = Scene<TwWaffleChart>(p => p.Add(x => x.Data, [new ChartValue("A", 999), new ChartValue("B", 1)]));

        Assert.Equal(2, scene.Legend.Count);
        Assert.Single(scene.Datums);
    }

    [Fact]
    public void Treemap_TilesThePlotInProportionToValue()
    {
        var scene = Scene<TwTreemapChart>(p => p.Add(x => x.Data, [new ChartValue("A", 60), new ChartValue("B", 25), new ChartValue("C", 10), new ChartValue("D", 5), new ChartValue("E", 0)]));

        var tiles = Marks<ChartRect>(scene);
        Assert.Equal(4, tiles.Count);
        Assert.Equal(10000, tiles.Sum(tile => tile.Width * tile.Height), 3);
        Assert.Equal(6000, tiles[0].Width * tiles[0].Height, 3);
        Assert.All(tiles, tile => Assert.True(tile.X >= 0 && tile.Y >= 0 && tile.X + tile.Width <= 100.001 && tile.Y + tile.Height <= 100.001));
        Assert.Contains(Marks<ChartText>(scene), text => text.Text == "A");
        Assert.Equal("A: Value 60, Share 60%", scene.Datums[0].AriaLabel);
        Assert.Empty(Scene<TwTreemapChart>(p => p.Add(x => x.Data, [new ChartValue("E", 0)])).Marks);
    }

    [Theory]
    [InlineData(false, "Search")]
    [InlineData(true, "Social")]
    public void Funnel_NarrowsThroughTheStages_AndInvertedFlipsTheOrder(bool inverted, string top)
    {
        var scene = ValueScene<TwFunnelChart>(p => p.Add(x => x.Inverted, inverted));

        var stages = Marks<ChartPath>(scene);
        Assert.Equal(3, stages.Count);
        Assert.Equal([1, 0.725, 0.45], stages.Select(stage => Math.Round(stage.Opacity, 3)));
        Assert.Equal(top, scene.YAxis!.Ticks.MinBy(tick => tick.Position)!.Label);
        Assert.Equal("Direct: Value 30, Of first stage 60%", scene.Datums[1].AriaLabel);
        Assert.Empty(Scene<TwFunnelChart>(p => p.Add(x => x.Data, [new ChartValue("A", 0)])).Marks);
    }

    [Fact]
    public void Funnel_SingleStage_IsFullyOpaque()
    {
        Assert.Equal(1, Assert.Single(Marks<ChartPath>(Scene<TwFunnelChart>(p => p.Add(x => x.Data, [new ChartValue("A", 5)])))).Opacity);
    }

    [Theory]
    [InlineData(40, 2)]
    [InlineData(0, 1)]
    [InlineData(250, 2)]
    public void Gauge_FillsTheArcInProportion_AndClampsToItsRange(double value, int arcs)
    {
        var scene = Scene<TwGaugeChart>(p => p.Add(x => x.Value, value).Add(x => x.Label, "Used"));

        Assert.Equal(arcs, Marks<ChartPath>(scene).Count);
        Assert.Contains(Marks<ChartText>(scene), text => text.Large && text.Text == value.ToString());
        Assert.Equal($"Used: Used {value}, Range 0 to 100", Assert.Single(scene.Datums).AriaLabel);
    }

    [Fact]
    public void Gauge_WithAnInvalidRange_DrawsNothing()
    {
        Assert.True(Scene<TwGaugeChart>(p => p.Add(x => x.Min, 10).Add(x => x.Max, 10)).IsEmpty);
    }

    #endregion

    #region Icon chart

    [Fact]
    public void Icon_Count_RepeatsTheIconPerUnit()
    {
        var scene = Scene<TwIconChart>(p => p.Add(x => x.Data, [new ChartValue("A", 40), new ChartValue("B", 20)]).Add(x => x.Icon, TwBlazor.Enums.Icon.Person));

        var glyphs = Marks<ChartGlyph>(scene);
        Assert.Equal(12, glyphs.Count);
        Assert.All(glyphs, glyph => Assert.Equal(TwBlazor.Enums.Icon.Person, glyph.Icon));
        Assert.Equal("1 icon = 5", Assert.Single(scene.Legend).Label);
        Assert.Equal(["A", "B"], scene.YAxis!.Ticks.Select(tick => tick.Label));
    }

    [Fact]
    public void Icon_UnitValue_SetsWhatOneIconIsWorth()
    {
        var scene = Scene<TwIconChart>(p => p.Add(x => x.Data, [new ChartValue("A", 40)]).Add(x => x.UnitValue, 10));

        Assert.Equal(4, Marks<ChartGlyph>(scene).Count);
    }

    [Fact]
    public void Icon_Size_ScalesOneIconPerValue()
    {
        var scene = Scene<TwIconChart>(p => p.Add(x => x.Data, [new ChartValue("A", 40), new ChartValue("B", 10)]).Add(x => x.Mode, IconChartMode.Size));

        var glyphs = Marks<ChartGlyph>(scene);
        Assert.Equal([72, 44], glyphs.Select(glyph => glyph.Size));
        Assert.Empty(scene.Legend);
        Assert.Equal("A", scene.XAxis!.Ticks[0].Label);
        Assert.Empty(Scene<TwIconChart>(p => p.Add(x => x.Data, [new ChartValue("A", 0)])).Marks);
    }

    #endregion

    #region Distributions

    [Fact]
    public void Histogram_CountsValuesIntoEqualBins()
    {
        var scene = Scene<TwHistogramChart>(p => p.Add(x => x.Values, [0, 1, 2, 3, 4, 10, double.NaN]).Add(x => x.BinCount, 2));

        Assert.Equal(2, Bars(scene, 0).Count);
        Assert.Equal([["0 to 5", "5"], ["5 to 10", "1"]], scene.Table!.Rows);
        Assert.Equal(["0", "5", "10"], scene.XAxis!.Ticks.Select(tick => tick.Label));
        Assert.Equal("0 to 5: Count 5", scene.Datums[0].AriaLabel);
    }

    [Fact]
    public void Histogram_PicksABinCount_AndCopesWithIdenticalValues()
    {
        Assert.Equal(4, Scene<TwHistogramChart>(p => p.Add(x => x.Values, [1, 2, 3, 4, 5, 6, 7, 8])).Datums.Count);
        Assert.Equal("3", Scene<TwHistogramChart>(p => p.Add(x => x.Values, [7, 7, 7]).Add(x => x.BinCount, 1)).Table!.Rows[0][1]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BoxPlot_SummarisesEachGroup_AndMarksOutliers(bool horizontal)
    {
        var scene = Scene<TwBoxPlotChart>(p => p.Add(x => x.Samples, _samples).Add(x => x.Horizontal, horizontal));

        Assert.Equal(2, Marks<ChartRect>(scene).Count(rect => rect.Color == ChartColor.Track));
        Assert.Single(Marks<ChartDot>(scene));
        Assert.Equal(["Fast", "10", "1", "3.25", "5.5", "7.75", "40"], scene.Table!.Rows[0]);
        Assert.Equal("Median", scene.Datums[0].Rows[3].Label);
        Assert.Equal("Fast", (horizontal ? scene.YAxis : scene.XAxis)!.Ticks[0].Label);
    }

    [Fact]
    public void BoxPlot_SkipsEmptyGroups()
    {
        var scene = Scene<TwBoxPlotChart>(p => p.Add(x => x.Samples, [new ChartSample("Empty", []), new ChartSample("One", [5])]));

        Assert.Equal("One", Assert.Single(scene.Datums).Title);
    }

    [Fact]
    public void StripPlot_DrawsEveryObservation_AndJitterIsRepeatable()
    {
        var plain = Marks<ChartDot>(Scene<TwStripPlotChart>(p => p.Add(x => x.Samples, _samples)));
        var jittered = Marks<ChartDot>(Scene<TwStripPlotChart>(p => p.Add(x => x.Samples, _samples).Add(x => x.Jitter, true)));
        var again = Marks<ChartDot>(Scene<TwStripPlotChart>(p => p.Add(x => x.Samples, _samples).Add(x => x.Jitter, true)));

        Assert.Equal(13, plain.Count);
        Assert.Single(plain.Take(10).Select(dot => dot.X).Distinct());
        Assert.True(jittered.Take(10).Select(dot => dot.X).Distinct().Count() > 5);
        Assert.Equal(jittered, again);
    }

    #endregion

    #region Candlestick and Gantt

    private static readonly ChartOhlc[] _prices = [new("Mon", 10, 16, 8, 14), new("Tue", 14, 15, 9, 11), new("Wed", 11, 12, 10, 11)];

    [Fact]
    public void Candlestick_DrawsABodyAndWick_ColoredByDirection()
    {
        var scene = Scene<TwCandlestickChart>(p => p.Add(x => x.Items, _prices));

        var bodies = Marks<ChartRect>(scene);
        Assert.Equal([ChartColor.Positive, ChartColor.Negative, ChartColor.Positive], bodies.Select(body => body.Color));
        Assert.True(bodies[2].Height > 0);
        Assert.Equal(3, Marks<ChartLine>(scene).Count(line => line.Color.Role is ChartColorRole.Positive or ChartColorRole.Negative));
        Assert.Equal("Tue (Falling): Open 14, High 15, Low 9, Close 11", scene.Datums[1].AriaLabel);
        Assert.Equal(["Rising", "Falling"], scene.Legend.Select(item => item.Label));
    }

    [Fact]
    public void Candlestick_Ohlc_DrawsTicksInsteadOfBodies()
    {
        var scene = Scene<TwCandlestickChart>(p => p.Add(x => x.Items, _prices).Add(x => x.Kind, CandlestickStyle.Ohlc));

        Assert.Empty(Marks<ChartRect>(scene));
        Assert.Equal(9, Marks<ChartLine>(scene).Count(line => line.Color.Role is ChartColorRole.Positive or ChartColorRole.Negative));
    }

    [Fact]
    public void Gantt_PlacesEachTaskBetweenItsDates()
    {
        ChartTask[] tasks = [new("Design", new DateTime(2026, 1, 1), new DateTime(2026, 1, 11)), new("Build", new DateTime(2026, 1, 6), new DateTime(2026, 1, 21))];
        var scene = Scene<TwGanttChart>(p => p.Add(x => x.Tasks, tasks).Add(x => x.DateFormat, "yyyy-MM-dd"));

        var bars = Bars(scene, 0);
        Assert.Equal((0, 50), (bars[0].X, bars[0].Width));
        Assert.Equal((25, 75), (bars[1].X, bars[1].Width));
        Assert.Equal(["Design", "Build"], scene.YAxis!.Ticks.Select(tick => tick.Label));
        Assert.Equal("2026-01-01", scene.XAxis!.Ticks[0].Label);
        Assert.Equal("2026-01-21", scene.XAxis.Ticks[^1].Label);
        Assert.Equal("Build: Start 2026-01-06, End 2026-01-21", scene.Datums[1].AriaLabel);
    }

    #endregion

    [Fact]
    public void Pie_DataPoints_CoverTheirWholeSlice()
    {
        // Search is half the pie: the right-hand semicircle, 48 wide and 96 tall.
        var scene = ValueScene<TwPieChart>();

        Assert.All(scene.Datums, datum => Assert.StartsWith("polygon(", datum.Clip));
        Assert.Equal(48, scene.Datums[0].Width, 3);
        Assert.Equal(96, scene.Datums[0].Height, 3);
        Assert.Equal(74, scene.Datums[0].X, 3);
        Assert.Equal(50, scene.Datums[0].Y, 3);
    }

    [Fact]
    public void Nightingale_And_RadialBar_DataPoints_CoverTheirWholeShape()
    {
        Assert.All(ValueScene<TwNightingaleChart>().Datums, datum => Assert.StartsWith("polygon(", datum.Clip));

        // Every ring's hit area spans its whole track, so a short bar is as easy to point at as a long one.
        var rings = ValueScene<TwRadialBarChart>().Datums;
        Assert.All(rings, datum => Assert.StartsWith("polygon(", datum.Clip));
        Assert.Equal(96, rings[0].Width, 3);
    }
}
