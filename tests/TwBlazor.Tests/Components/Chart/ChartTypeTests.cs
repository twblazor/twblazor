using Bunit;
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
            ValueScene<TwFunnelChart>(), ValueScene<TwWaffleChart>(), ValueScene<TwTreemapChart>(), ValueScene<TwIconChart>(),
            Scene<TwRangeChart>(p => p.Add(x => x.Ranges, [new ChartRange("A", 1, 5)])),
            Scene<TwBulletChart>(p => p.Add(x => x.Items, [new ChartBullet("A", 5, 6)])),
            Scene<TwScatterChart>(p => p.Add(x => x.Series, points)),
            Scene<TwQuadrantChart>(p => p.Add(x => x.Series, points)),
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
