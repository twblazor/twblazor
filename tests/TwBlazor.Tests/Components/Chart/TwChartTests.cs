using Bunit;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Models;

namespace TwBlazor.Tests.Components.Chart;

public class TwChartTests : TwBlazorTestBase
{
    private static readonly string[] _categories = ["North", "South", "East"];

    private static readonly ChartSeries[] _series = [new("2024", [10, 20, 30]), new("2025", [15, 25, 35])];

    private TwChartTheme theme => Theme.Components.Require<TwChartTheme>();

    private IRenderedComponent<TwColumnChart> RenderColumns(Action<ComponentParameterCollectionBuilder<TwColumnChart>>? configure = null, ChartSeries[]? series = null) =>
        TestContext.Render<TwColumnChart>(parameters =>
        {
            parameters.Add(p => p.Categories, _categories).Add(p => p.Series, series ?? _series);
            configure?.Invoke(parameters);
        });

    #region Container

    [Fact]
    public void TwChart_RendersFigureWithTitleAndDescription()
    {
        var cut = TestContext.Render<TwChart>(p => p.Add(x => x.Id, "sales").Add(x => x.Title, "Sales").Add(x => x.Description, "Last quarter"));

        var figure = cut.Find("figure");
        Assert.Equal("sales-title", figure.GetAttribute("aria-labelledby"));
        Assert.Equal("Sales", cut.Find("#sales-title").TextContent);
        Assert.Contains("Last quarter", cut.Find("figcaption").TextContent);
        Assert.Contains(theme.Container, figure.GetAttribute("class"));
    }

    [Fact]
    public void TwChart_WithoutTitleOrDescription_RendersNoCaption()
    {
        var cut = TestContext.Render<TwChart>();

        Assert.Empty(cut.FindAll("figcaption"));
        Assert.Null(cut.Find("figure").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void TwChart_DescriptionOnly_RendersCaptionWithoutTitle()
    {
        var cut = TestContext.Render<TwChart>(p => p.Add(x => x.Description, "Last quarter"));

        Assert.Equal("Last quarter", cut.Find("figcaption").TextContent.Trim());
    }

    [Fact]
    public void TwChart_AriaLabelledBy_OverridesTheTitle()
    {
        var cut = TestContext.Render<TwChart>(p => p.Add(x => x.Title, "Sales").Add(x => x.AriaLabelledBy, "heading"));

        Assert.Equal("heading", cut.Find("figure").GetAttribute("aria-labelledby"));
    }

    [Theory]
    [InlineData(ChartHeight.Small, "10rem")]
    [InlineData(ChartHeight.Medium, "16rem")]
    [InlineData(ChartHeight.Large, "24rem")]
    public void TwChart_Loading_ShowsPlaceholderInsteadOfContent(ChartHeight height, string expected)
    {
        var cut = TestContext.Render<TwChart>(p => p
            .Add(x => x.Loading, true)
            .Add(x => x.Height, height)
            .AddChildContent("<p id=\"content\">Chart</p>"));

        Assert.Empty(cut.FindAll("#content"));
        Assert.Equal("true", cut.Find("[aria-busy]").GetAttribute("aria-busy"));
        Assert.Contains(expected, cut.Markup);
    }

    [Fact]
    public void TwChart_NamesTheChartInsideIt_AndSetsItsHeight()
    {
        var cut = TestContext.Render<TwChart>(p => p
            .Add(x => x.Id, "sales")
            .Add(x => x.Title, "Sales")
            .Add(x => x.Height, ChartHeight.Large)
            .AddChildContent<TwColumnChart>(c => c.Add(x => x.Categories, _categories).Add(x => x.Series, _series)));

        var chart = cut.Find("[role='group']");
        Assert.Equal("sales-title", chart.GetAttribute("aria-labelledby"));
        Assert.Null(chart.GetAttribute("aria-label"));
        Assert.Contains(theme.Large, cut.Find("svg").ParentElement!.GetAttribute("class"));
    }

    #endregion

    #region Chart root

    [Fact]
    public void Chart_Standalone_IsANamedGroup()
    {
        var cut = RenderColumns();

        var root = cut.Find("[role='group']");
        Assert.Equal("Chart", root.GetAttribute("aria-label"));
        Assert.StartsWith("columnchart-", root.Id);
        Assert.Contains(theme.Chart, root.GetAttribute("class"));
    }

    [Fact]
    public void Chart_AriaLabel_OverridesTheContainerTitle()
    {
        var cut = TestContext.Render<TwChart>(p => p
            .Add(x => x.Title, "Sales")
            .AddChildContent<TwColumnChart>(c => c.Add(x => x.AriaLabel, "Custom").Add(x => x.Categories, _categories).Add(x => x.Series, _series)));

        var chart = cut.Find("[role='group']");
        Assert.Equal("Custom", chart.GetAttribute("aria-label"));
        Assert.Null(chart.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void Chart_WithNoData_ShowsEmptyText()
    {
        var cut = TestContext.Render<TwColumnChart>(p => p.Add(x => x.EmptyText, "Nothing yet"));

        Assert.Equal("Nothing yet", cut.Find("p").TextContent);
        Assert.Empty(cut.FindAll("svg"));
        Assert.Empty(cut.FindAll("button"));
    }

    [Theory]
    [InlineData(ChartHeight.Small)]
    [InlineData(ChartHeight.Medium)]
    [InlineData(ChartHeight.Large)]
    public void Chart_Height_SetsPlotHeightClasses(ChartHeight height)
    {
        var cut = RenderColumns(p => p.Add(x => x.Height, height));

        var expected = height switch { ChartHeight.Small => theme.Small, ChartHeight.Large => theme.Large, _ => theme.Medium };
        Assert.Contains(expected, cut.Find("svg").ParentElement!.ClassList);
    }

    [Theory]
    [InlineData(ChartHeight.Small)]
    [InlineData(ChartHeight.Medium)]
    [InlineData(ChartHeight.Large)]
    public void Chart_SquareShape_UsesSquareClasses(ChartHeight height)
    {
        var cut = TestContext.Render<TwPieChart>(p => p.Add(x => x.Data, [new ChartValue("A", 1)]).Add(x => x.Height, height));

        var classes = cut.Find("svg").ParentElement!.GetAttribute("class")!;
        var expected = height switch { ChartHeight.Small => theme.SquareSmall, ChartHeight.Large => theme.SquareLarge, _ => theme.SquareMedium };
        Assert.Contains(theme.Square, classes);
        Assert.Contains(expected, classes);
        Assert.DoesNotContain(theme.Medium, classes.Split(' '));
    }

    #endregion

    #region Marks

    [Fact]
    public void Chart_DrawsBarsWithSeriesColorsFromTheTheme()
    {
        var cut = RenderColumns();

        var first = cut.FindAll("div").Where(e => e.ClassList.Contains("bg-purple-600")).ToList();
        Assert.Equal(3, first.Count);
        Assert.All(first, bar => Assert.Equal("true", bar.GetAttribute("aria-hidden")));
        Assert.Contains(theme.RoundedTop, first[0].GetAttribute("class"));
        Assert.Contains(theme.SurfaceRing, first[0].GetAttribute("class"));
        Assert.Contains("left:", first[0].GetAttribute("style"));
    }

    [Fact]
    public void Chart_DrawsGridlinesInADecorativeSvgLayer()
    {
        var cut = RenderColumns();

        var svg = cut.Find("svg");
        Assert.Equal("true", svg.GetAttribute("aria-hidden"));
        Assert.Equal("none", svg.GetAttribute("preserveAspectRatio"));
        Assert.NotEmpty(svg.QuerySelectorAll("line"));
        Assert.All(svg.QuerySelectorAll("line"), line => Assert.Equal("non-scaling-stroke", line.GetAttribute("vector-effect")));
    }

    [Fact]
    public void Chart_DrawsPaths_Dots_Text_AndIcons()
    {
        var line = TestContext.Render<TwLineChart>(p => p.Add(x => x.Categories, _categories).Add(x => x.Series, _series));
        Assert.Equal(2, line.FindAll("path").Count);
        Assert.Contains(theme.FillNone, line.Find("path").GetAttribute("class"));
        Assert.Equal(6, line.FindAll("div").Count(e => e.GetAttribute("class")?.StartsWith(theme.Dot) == true));

        var pie = TestContext.Render<TwPieChart>(p => p.Add(x => x.Data, [new ChartValue("A", 3), new ChartValue("B", 1)]));
        Assert.Contains(theme.SurfaceStroke, pie.Find("path").GetAttribute("class"));
        Assert.Equal("A: 3 (75%)", pie.Find("path title").TextContent);
        Assert.Contains(pie.FindAll("span"), e => e.TextContent == "75%" && e.ClassList.Contains("text-white"));

        var icons = TestContext.Render<TwIconChart>(p => p.Add(x => x.Data, [new ChartValue("A", 3)]).Add(x => x.UnitValue, 1).Add(x => x.ShowTableToggle, false));
        Assert.Equal(3, icons.FindAll("i").Count);
    }

    [Fact]
    public void Chart_Opacity_IsOnlyWrittenWhenBelowOne()
    {
        var cut = TestContext.Render<TwAreaChart>(p => p.Add(x => x.Categories, _categories).Add(x => x.Series, _series));

        var paths = cut.FindAll("path");
        Assert.Contains(paths, path => path.GetAttribute("style") == "opacity:0.12");
        Assert.Contains(paths, path => path.GetAttribute("style") == null);
    }

    [Fact]
    public void Chart_RendersAxisLabels_HiddenFromAssistiveTechnology()
    {
        var cut = RenderColumns();

        var labels = cut.FindAll("span").Where(e => e.ClassList.Contains("truncate")).Select(e => e.TextContent).ToList();
        Assert.Contains("North", labels);
        Assert.Contains("40", labels);
        Assert.All(cut.FindAll("span").Where(e => e.ClassList.Contains("truncate")), e => Assert.NotNull(e.Closest("[aria-hidden='true']")));
    }

    [Fact]
    public void Chart_RendersAxisTitles()
    {
        var cut = TestContext.Render<TwScatterChart>(p => p
            .Add(x => x.Series, [new ChartPointSeries("A", [new ChartPoint(1, 2), new ChartPoint(3, 4)])])
            .Add(x => x.XTitle, "Price")
            .Add(x => x.YTitle, "Rating"));

        var titles = cut.FindAll("div").Where(e => e.GetAttribute("class")?.StartsWith(theme.AxisTitle) == true).Select(e => e.TextContent).ToList();
        Assert.Equal(["Rating", "Price"], titles);
    }

    [Fact]
    public void Chart_ValueFormat_AppliesToAxisTooltipsAndTable()
    {
        var cut = RenderColumns(p => p.Add(x => x.ValueFormat, "0.0"));

        Assert.Contains("10.0", cut.Find("[role='img']").GetAttribute("aria-label"));
        Assert.Contains(cut.FindAll("span"), e => e.TextContent == "40.0");
    }

    [Fact]
    public void Chart_ValueFormatter_TakesPrecedenceOverValueFormat()
    {
        var cut = RenderColumns(p => p.Add(x => x.ValueFormat, "0.0").Add(x => x.ValueFormatter, value => $"{value}k"));

        Assert.Equal("North: 2024 10k, 2025 15k", cut.Find("[role='img']").GetAttribute("aria-label"));
    }

    #endregion

    #region Legend

    [Fact]
    public void Chart_WithSeveralSeries_ShowsALegendOfToggleButtons()
    {
        var cut = RenderColumns();

        var legend = cut.Find("ul");
        Assert.Equal("Legend", legend.GetAttribute("aria-label"));
        var buttons = legend.QuerySelectorAll("button");
        Assert.Equal(["2024", "2025"], buttons.Select(b => b.TextContent.Trim()));
        Assert.All(buttons, b => Assert.Equal("true", b.GetAttribute("aria-pressed")));
        Assert.Contains("bg-orange-600", buttons[1].QuerySelector("span")!.ClassList);
    }

    [Fact]
    public void Chart_WithOneSeries_HasNoLegend()
    {
        var cut = RenderColumns(series: [_series[0]]);

        Assert.Empty(cut.FindAll("ul"));
    }

    [Fact]
    public void Chart_ShowLegend_False_HidesTheLegend()
    {
        var cut = RenderColumns(p => p.Add(x => x.ShowLegend, false));

        Assert.Empty(cut.FindAll("ul"));
    }

    [Fact]
    public void Chart_InteractiveLegend_False_RendersPlainEntries()
    {
        var cut = RenderColumns(p => p.Add(x => x.InteractiveLegend, false));

        Assert.Empty(cut.FindAll("ul button"));
        Assert.Equal(2, cut.FindAll("ul li").Count);
    }

    [Fact]
    public void Chart_ClickingALegendEntry_HidesTheSeries_AndKeepsOtherColors()
    {
        var cut = RenderColumns();

        cut.FindAll("ul button")[0].Click();

        var button = cut.FindAll("ul button")[0];
        Assert.Equal("false", button.GetAttribute("aria-pressed"));
        Assert.Contains(theme.LegendItemHidden, button.GetAttribute("class"));
        Assert.Equal("North: 2025 15", cut.Find("[role='img']").GetAttribute("aria-label"));
        Assert.Empty(cut.FindAll("div").Where(e => e.ClassList.Contains("bg-purple-600") && e.HasAttribute("style")));
        Assert.Equal(3, cut.FindAll("div").Count(e => e.ClassList.Contains("bg-orange-600")));
    }

    [Fact]
    public void Chart_HidingEverySeries_KeepsTheLegendSoTheyCanBeShownAgain()
    {
        var cut = RenderColumns();

        cut.FindAll("ul button")[0].Click();
        cut.FindAll("ul button")[1].Click();
        Assert.Empty(cut.FindAll("[role='img']"));
        Assert.Equal(2, cut.FindAll("ul button").Count);

        cut.FindAll("ul button")[0].Click();
        Assert.Equal(3, cut.FindAll("[role='img']").Count);
    }

    #endregion

    #region Data points

    [Fact]
    public void Chart_EachDataPoint_IsALabelledImageWithATooltip()
    {
        var cut = RenderColumns();

        var points = cut.FindAll("[role='img']");
        Assert.Equal(3, points.Count);
        Assert.Equal("North: 2024 10, 2025 15", points[0].GetAttribute("aria-label"));

        var tooltips = cut.FindAll("[role='tooltip']");
        Assert.Equal(3, tooltips.Count);
        Assert.Contains("North", tooltips[0].TextContent);
        Assert.Contains("15", tooltips[0].TextContent);
        Assert.Equal(2, tooltips[0].QuerySelectorAll("[aria-hidden='true']").Count(e => e.ClassList.Contains("h-0.5")));
        Assert.Contains("position:absolute", points[0].ParentElement!.GetAttribute("style"));
        Assert.Contains("min-w-6", points[0].ParentElement!.ClassList);
    }

    [Fact]
    public void Chart_Plot_IsASingleTabStop()
    {
        var cut = RenderColumns();

        Assert.Equal(["0", "-1", "-1"], cut.FindAll("[role='img']").Select(e => e.GetAttribute("tabindex")));
        Assert.All(cut.FindAll("[role='img']"), e => Assert.Null(e.ParentElement!.GetAttribute("tabindex")));
    }

    [Theory]
    [InlineData("ArrowRight", 1)]
    [InlineData("ArrowDown", 1)]
    [InlineData("End", 2)]
    [InlineData("ArrowLeft", 0)]
    [InlineData("Home", 0)]
    [InlineData("a", 0)]
    public async Task Chart_ArrowKeys_MoveBetweenDataPoints(string key, int expected)
    {
        var cut = RenderColumns();

        await cut.Find("svg").ParentElement!.KeyDownAsync(new KeyboardEventArgs { Key = key });

        Assert.Equal("0", cut.FindAll("[role='img']")[expected].GetAttribute("tabindex"));
    }

    [Fact]
    public async Task Chart_ArrowKeys_StopAtTheEnds()
    {
        var cut = RenderColumns();
        var plot = () => cut.Find("svg").ParentElement!;

        await plot().KeyDownAsync(new KeyboardEventArgs { Key = "End" });
        await plot().KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" });
        Assert.Equal("0", cut.FindAll("[role='img']")[2].GetAttribute("tabindex"));

        await plot().KeyDownAsync(new KeyboardEventArgs { Key = "ArrowUp" });
        Assert.Equal("0", cut.FindAll("[role='img']")[1].GetAttribute("tabindex"));
    }

    [Fact]
    public void Chart_FocusingADataPoint_MakesItTheTabStop()
    {
        var cut = RenderColumns();

        cut.FindAll("[role='img']")[2].Focus();

        Assert.Equal(["-1", "-1", "0"], cut.FindAll("[role='img']").Select(e => e.GetAttribute("tabindex")));
    }

    [Fact]
    public void Chart_RegistersAndReleasesTheKeydownGuard()
    {
        var cut = RenderColumns();

        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twTabs.registerKeydownGuard");

        TestContext.Dispose();
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twTabs.unregisterKeydownGuard");
    }

    [Fact]
    public void Chart_HoverAffordance_FollowsTheChartType()
    {
        Assert.Contains(theme.HitHighlight, RenderColumns().Find("[role='img'] span").GetAttribute("class"));

        var line = TestContext.Render<TwLineChart>(p => p.Add(x => x.Categories, _categories).Add(x => x.Series, _series));
        Assert.Contains(theme.Crosshair, line.Find("[role='img'] span").GetAttribute("class"));

        var pie = TestContext.Render<TwPieChart>(p => p.Add(x => x.Data, [new ChartValue("A", 1)]));
        Assert.Empty(pie.FindAll("[role='img'] span"));
    }

    #endregion

    #region Table view

    [Fact]
    public void Chart_TableToggle_SwitchesToATableOfTheSameData_AndBack()
    {
        var cut = RenderColumns();
        Assert.Empty(cut.FindAll("table"));

        cut.FindAll("button").Single(b => b.TextContent.Contains("View as table")).Click();

        var table = cut.Find("table");
        Assert.Equal("Chart data", table.GetAttribute("aria-label"));
        Assert.Equal(["Category", "2024", "2025"], table.QuerySelectorAll("thead th").Select(e => e.TextContent));
        Assert.Equal("North", table.QuerySelector("tbody th[scope='row']")!.TextContent);
        Assert.Equal(["10", "15"], table.QuerySelectorAll("tbody tr")[0].QuerySelectorAll("td").Select(e => e.TextContent));
        Assert.Empty(cut.FindAll("svg"));

        cut.FindAll("button").Single(b => b.TextContent.Contains("View as chart")).Click();
        Assert.Empty(cut.FindAll("table"));
        Assert.NotEmpty(cut.FindAll("svg"));
    }

    [Fact]
    public void Chart_ShowTableToggle_False_RemovesTheButton()
    {
        var cut = RenderColumns(p => p.Add(x => x.ShowTableToggle, false), [_series[0]]);

        Assert.Empty(cut.FindAll("button"));
    }

    [Fact]
    public void Chart_Table_LeavesMissingValuesBlank()
    {
        var cut = RenderColumns(series: [new ChartSeries("Only", [1, null])]);

        cut.Find("button").Click();

        Assert.Equal(["1", "", ""], cut.FindAll("tbody td").Select(e => e.TextContent));
    }

    #endregion
}
