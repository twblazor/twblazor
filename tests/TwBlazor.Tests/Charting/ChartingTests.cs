using TwBlazor.Charting;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Charting;

public class ChartScaleTests
{
    [Fact]
    public void Nice_IncludesZero_AndRoundsOutToTickValues()
    {
        var scale = ChartScale.Nice(12, 87);

        Assert.Equal(0, scale.Min);
        Assert.Equal(100, scale.Max);
        Assert.Equal([0, 20, 40, 60, 80, 100], scale.Ticks);
    }

    [Fact]
    public void Nice_WithoutZero_HugsTheData()
    {
        var scale = ChartScale.Nice(102, 148, includeZero: false);

        Assert.Equal(100, scale.Min);
        Assert.Equal(150, scale.Max);
    }

    [Fact]
    public void Nice_CoversNegativeValues()
    {
        var scale = ChartScale.Nice(-35, 60);

        Assert.True(scale.Min <= -35);
        Assert.True(scale.Max >= 60);
        Assert.Contains(0, scale.Ticks);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(5, 5)]
    [InlineData(double.NaN, double.NaN)]
    [InlineData(double.NegativeInfinity, double.PositiveInfinity)]
    public void Nice_WithNoSpread_StillProducesAUsableScale(double min, double max)
    {
        var scale = ChartScale.Nice(min, max, includeZero: false);

        Assert.True(scale.Max > scale.Min);
        Assert.True(scale.Ticks.Count >= 2);
    }

    [Fact]
    public void Nice_FromValues_IgnoresNonFiniteValues_AndHandlesNone()
    {
        Assert.Equal(10, ChartScale.Nice([2, double.NaN, 9]).Max);
        Assert.Equal(1, ChartScale.Nice([]).Max);
    }

    [Fact]
    public void Map_ReturnsPercentageAlongTheAxis()
    {
        var scale = ChartScale.Nice(0, 100);

        Assert.Equal(0, scale.Map(0));
        Assert.Equal(25, scale.Map(25));
        Assert.Equal(100, scale.Map(100));
    }

    [Theory]
    [InlineData(0.7, 1)]
    [InlineData(1.2, 2)]
    [InlineData(3, 5)]
    [InlineData(7, 10)]
    [InlineData(23, 50)]
    [InlineData(0, 1)]
    [InlineData(-4, 1)]
    public void NiceStep_RoundsUpToOneTwoOrFiveTimesAPowerOfTen(double raw, double expected)
    {
        Assert.Equal(expected, ChartScale.NiceStep(raw));
    }

    [Fact]
    public void Band_DividesTheAxisIntoEqualSlots()
    {
        var band = new ChartBand(4);

        Assert.Equal(25, band.Step);
        Assert.Equal(12.5, band.Center(0));
        Assert.Equal(87.5, band.Center(3));
    }

    [Fact]
    public void Band_RespectsItsStartAndEnd_AndAnEmptyBandHasNoStep()
    {
        var band = new ChartBand(2, 20, 80);

        Assert.Equal(30, band.Step);
        Assert.Equal(35, band.Center(0));
        Assert.Equal(0, new ChartBand(0).Step);
    }
}

public class ChartGeometryTests
{
    [Theory]
    [InlineData(0, 50, 40)]
    [InlineData(90, 60, 50)]
    [InlineData(180, 50, 60)]
    [InlineData(270, 40, 50)]
    public void Polar_MeasuresAnglesClockwiseFromTwelveOClock(double angle, double expectedX, double expectedY)
    {
        var point = ChartGeometry.Polar(50, 50, 10, angle);

        Assert.Equal(expectedX, point.X, 6);
        Assert.Equal(expectedY, point.Y, 6);
    }

    [Fact]
    public void Sector_WithNoInnerRadius_IsAWedgeThroughTheCenter()
    {
        var path = ChartGeometry.Sector(50, 50, 0, 40, 0, 90);

        Assert.StartsWith("M50 10", path);
        Assert.Contains("L50 50", path);
        Assert.EndsWith("Z", path);
    }

    [Fact]
    public void Sector_WithInnerRadius_HasTwoArcs_AndFlagsLargeArcs()
    {
        var small = ChartGeometry.Sector(50, 50, 20, 40, 0, 90);
        var large = ChartGeometry.Sector(50, 50, 20, 40, 0, 270);

        Assert.Equal(2, small.Count(c => c == 'A'));
        Assert.Contains("A40 40 0 0 1", small);
        Assert.Contains("A40 40 0 1 1", large);
    }

    [Fact]
    public void Sector_FullCircle_StopsShortSoItStillDraws()
    {
        var path = ChartGeometry.Sector(50, 50, 0, 40, 0, 360);

        Assert.DoesNotContain("A40 40 0 1 1 50 10", path);
    }

    [Fact]
    public void Line_JoinsPointsAccordingToTheCurve()
    {
        (double X, double Y)[] points = [(0, 10), (50, 40), (100, 20)];

        Assert.Equal("M0 10L50 40L100 20", ChartGeometry.Line(points));
        Assert.Equal("M0 10L50 10L50 40L100 40L100 20", ChartGeometry.Line(points, ChartCurve.Step));
        Assert.Equal(2, ChartGeometry.Line(points, ChartCurve.Smooth).Count(c => c == 'C'));
        Assert.EndsWith("Z", ChartGeometry.Line(points, close: true));
        Assert.Equal(string.Empty, ChartGeometry.Line([]));
    }

    [Fact]
    public void Line_Smooth_DoesNotOvershootTheData()
    {
        (double X, double Y)[] points = [(0, 50), (50, 50), (100, 0)];

        var path = ChartGeometry.Line(points, ChartCurve.Smooth);

        Assert.StartsWith("M0 50C8.33 50 33.33 50 50 50", path);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(0.5, 3)]
    [InlineData(1, 5)]
    [InlineData(0.25, 2)]
    public void Percentile_InterpolatesBetweenNeighbors(double percentile, double expected)
    {
        Assert.Equal(expected, ChartGeometry.Percentile([1, 2, 3, 4, 5], percentile));
    }

    [Fact]
    public void Percentile_OfNothing_IsZero()
    {
        Assert.Equal(0, ChartGeometry.Percentile([], 0.5));
    }
}

public class ChartSceneBuilderTests
{
    [Fact]
    public void Build_OfNothing_IsEmpty()
    {
        Assert.True(new ChartSceneBuilder().Build().IsEmpty);
        Assert.True(ChartScene.Empty.IsEmpty);
    }

    [Fact]
    public void Bar_Vertical_GrowsUpFromTheBaseline_AndRoundsItsTop()
    {
        var builder = new ChartSceneBuilder();

        builder.Bar(50, 10, 0, 40, ChartColor.Series(0));

        var rect = Assert.IsType<ChartRect>(Assert.Single(builder.Build().Marks));
        Assert.Equal((45, 60, 10, 40), (rect.X, rect.Y, rect.Width, rect.Height));
        Assert.Equal(ChartCorner.Top, rect.Corner);
    }

    [Fact]
    public void Bar_Horizontal_GrowsRightFromTheBaseline_AndRoundsItsRight()
    {
        var builder = new ChartSceneBuilder { Horizontal = true };

        builder.Bar(50, 10, 0, 40, ChartColor.Series(0));

        var rect = Assert.IsType<ChartRect>(Assert.Single(builder.Build().Marks));
        Assert.Equal((0, 45, 40, 10), (rect.X, rect.Y, rect.Width, rect.Height));
        Assert.Equal(ChartCorner.Right, rect.Corner);
    }

    [Theory]
    [InlineData(false, ChartCorner.Bottom)]
    [InlineData(true, ChartCorner.Left)]
    public void Bar_Falling_RoundsTheOppositeEnd(bool horizontal, ChartCorner expected)
    {
        var builder = new ChartSceneBuilder { Horizontal = horizontal };

        builder.Bar(50, 10, 50, 20, ChartColor.Series(0));

        Assert.Equal(expected, Assert.IsType<ChartRect>(Assert.Single(builder.Build().Marks)).Corner);
    }

    [Fact]
    public void Bar_UsesAnExplicitCorner_WhenGiven()
    {
        var builder = new ChartSceneBuilder();

        builder.Bar(50, 10, 0, 40, ChartColor.Series(0), ChartCorner.All);

        Assert.Equal(ChartCorner.All, Assert.IsType<ChartRect>(Assert.Single(builder.Build().Marks)).Corner);
    }

    [Fact]
    public void ValueAxis_Vertical_LabelsTheLeft_AndFlipsPositions()
    {
        var builder = new ChartSceneBuilder();

        builder.ValueAxis(ChartScale.Nice(0, 100), value => $"{value}u", "Units");
        var scene = builder.Build();

        Assert.Null(scene.XAxis);
        Assert.Equal("Units", scene.YAxis!.Title);
        Assert.Equal(new ChartTick(100, "0u"), scene.YAxis.Ticks[0]);
        Assert.Equal(new ChartTick(0, "100u"), scene.YAxis.Ticks[^1]);
        Assert.Equal(6, scene.Marks.OfType<ChartLine>().Count(line => line.Color == ChartColor.Grid));
    }

    [Fact]
    public void ValueAxis_Horizontal_LabelsTheBottom()
    {
        var builder = new ChartSceneBuilder { Horizontal = true };

        builder.ValueAxis(ChartScale.Nice(0, 100), value => value.ToString());
        var scene = builder.Build();

        Assert.Null(scene.YAxis);
        Assert.Equal(new ChartTick(0, "0"), scene.XAxis!.Ticks[0]);
    }

    [Fact]
    public void BandAxis_Vertical_ThinsOutCrowdedLabels()
    {
        var builder = new ChartSceneBuilder();
        string[] labels = [.. Enumerable.Range(1, 30).Select(i => $"Day {i}")];

        builder.BandAxis(labels, new ChartBand(labels.Length));

        var ticks = builder.Build().XAxis!.Ticks;
        Assert.Equal(10, ticks.Count);
        Assert.Equal(["Day 1", "Day 4"], ticks.Take(2).Select(tick => tick.Label));
    }

    [Fact]
    public void BandAxis_Horizontal_KeepsEveryLabel()
    {
        var builder = new ChartSceneBuilder { Horizontal = true };
        string[] labels = [.. Enumerable.Range(1, 30).Select(i => $"Row {i}")];

        builder.BandAxis(labels, new ChartBand(labels.Length));

        Assert.Equal(30, builder.Build().YAxis!.Ticks.Count);
    }

    [Theory]
    [InlineData(false, 50, 50, 100)]
    [InlineData(true, 50, 100, 50)]
    public void BandDatum_CoversTheWholeSlot(bool horizontal, double expectedY, double expectedWidth, double expectedHeight)
    {
        var builder = new ChartSceneBuilder { Horizontal = horizontal };

        builder.BandDatum(50, 50, "A", []);

        var datum = Assert.Single(builder.Build().Datums);
        Assert.Equal((expectedY, expectedWidth, expectedHeight), (datum.Y, datum.Width, datum.Height));
    }

    [Theory]
    [InlineData(false, true, ChartAnchor.Middle, ChartBaseline.Bottom)]
    [InlineData(false, false, ChartAnchor.Middle, ChartBaseline.Top)]
    [InlineData(true, true, ChartAnchor.Start, ChartBaseline.Middle)]
    [InlineData(true, false, ChartAnchor.End, ChartBaseline.Middle)]
    public void TipLabel_SitsJustPastTheDataEnd(bool horizontal, bool rising, ChartAnchor anchor, ChartBaseline baseline)
    {
        var builder = new ChartSceneBuilder { Horizontal = horizontal };

        builder.TipLabel(50, 40, "40", rising);

        var text = Assert.IsType<ChartText>(Assert.Single(builder.Build().Marks));
        Assert.Equal((anchor, baseline, ChartTextTone.Primary), (text.Anchor, text.Baseline, text.Tone));
    }

    [Fact]
    public void Path_IgnoresEmptyData()
    {
        var builder = new ChartSceneBuilder();

        builder.Path(string.Empty, ChartColor.Series(0));

        Assert.Empty(builder.Build().Marks);
    }

    [Fact]
    public void Build_CarriesEveryPartOfTheScene()
    {
        var builder = new ChartSceneBuilder { Shape = ChartShape.Square, Table = new ChartTable(["A"], [["1"]]) };
        builder.Add(new ChartDot(1, 2, ChartColor.Ink));
        builder.Glyph(1, 2, Icon.Person, ChartColor.Series(1));
        builder.Marker(10, 20, ChartColor.Series(0));
        builder.Baseline(0);
        builder.Legend("One", ChartColor.Series(0), 0, true);

        var scene = builder.Build();

        Assert.Equal(ChartShape.Square, scene.Shape);
        Assert.Equal(4, scene.Marks.Count);
        Assert.Equal(new ChartLegendItem("One", ChartColor.Series(0), 0, true), Assert.Single(scene.Legend));
        Assert.NotNull(scene.Table);
        Assert.False(scene.IsEmpty);
    }

    [Fact]
    public void Datum_AriaLabel_StatesTitleAndEveryRow()
    {
        var datum = new ChartDatum(0, 0, 0, 0, "North", [new("2024", "360"), new("2025", "420")]);

        Assert.Equal("North: 2024 360, 2025 420", datum.AriaLabel);
        Assert.Equal("North", (datum with { Rows = [] }).AriaLabel);
    }

    [Fact]
    public void Color_Series_NeverGoesBelowTheFirstSlot()
    {
        Assert.Equal(0, ChartColor.Series(-3).Slot);
        Assert.Equal(ChartColorRole.Total, ChartColor.Total.Role);
        Assert.Equal(ChartColorRole.Sequential, ChartColor.Sequential.Role);
        Assert.Equal(ChartColorRole.Track, ChartColor.Track.Role);
        Assert.Equal(ChartColorRole.Axis, ChartColor.Axis.Role);
    }
}

public class ChartSectorDatumTests
{
    private static ChartDatum Sector(double innerRadius, double outerRadius, double startAngle, double endAngle)
    {
        var builder = new ChartSceneBuilder();
        builder.SectorDatum(50, 50, innerRadius, outerRadius, startAngle, endAngle, "Slice", []);
        return Assert.Single(builder.Build().Datums);
    }

    private static string[] Points(ChartDatum datum) => datum.Clip!["polygon(".Length..^1].Split(", ");

    [Fact]
    public void SectorDatum_Wedge_IsBoxedAroundTheWedge_AndTracedBackToTheCenter()
    {
        // The top-right quarter of a circle of radius 40 centered on 50,50.
        var datum = Sector(0, 40, 0, 90);

        Assert.Equal(70, datum.X, 3);
        Assert.Equal(30, datum.Y, 3);
        Assert.Equal(40, datum.Width, 3);
        Assert.Equal(40, datum.Height, 3);
        Assert.Equal(ChartHover.None, datum.Hover);

        var points = Points(datum);
        Assert.Equal("0% 0%", points[0]);
        Assert.Contains("100% 100%", points);
        Assert.Equal("0% 100%", points[^1]);
    }

    [Fact]
    public void SectorDatum_RingSegment_LeavesOutTheHole()
    {
        var points = Points(Sector(20, 40, 0, 90));

        Assert.DoesNotContain("0% 100%", points);
        Assert.Equal("0% 50%", points[^1]);
        Assert.Contains("50% 100%", points);
    }

    [Fact]
    public void SectorDatum_FullCircle_CoversTheWholeCircle()
    {
        var datum = Sector(0, 48, 0, 360);

        Assert.Equal(50, datum.X, 3);
        Assert.Equal(50, datum.Y, 3);
        Assert.Equal(96, datum.Width, 3);
        Assert.Equal(96, datum.Height, 3);
    }

    [Fact]
    public void SectorDatum_WritesNumbersInvariantly()
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
            Assert.DoesNotContain(Points(Sector(0, 40, 0, 50)), point => point.Contains(','));
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = culture;
        }
    }

    [Fact]
    public void SectorDatum_Wedge_PointsItsTooltipPartWayOutAlongItsMiddle()
    {
        // Straight right of center (90 degrees), 65% of the way out on a radius of 40.
        var datum = Sector(0, 40, 0, 180);

        Assert.Equal(76, datum.AnchorX!.Value, 3);
        Assert.Equal(50, datum.AnchorY!.Value, 3);
    }

    [Fact]
    public void SectorDatum_RingSegment_PointsItsTooltipAtTheMiddleOfTheRing()
    {
        var datum = Sector(20, 40, 0, 180);

        Assert.Equal(80, datum.AnchorX!.Value, 3);
        Assert.Equal(50, datum.AnchorY!.Value, 3);
    }

    [Fact]
    public void SectorDatum_AnchorAngle_MovesTheTooltipRoundTheSegment()
    {
        var builder = new ChartSceneBuilder();
        builder.SectorDatum(50, 50, 20, 40, 0, 270, "Ring", [], anchorAngle: 180);

        var datum = Assert.Single(builder.Build().Datums);
        Assert.Equal(50, datum.AnchorX!.Value, 3);
        Assert.Equal(80, datum.AnchorY!.Value, 3);
    }
}
