using Bunit;
using System.Globalization;
using TwBlazor.Charting;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Models;

namespace TwBlazor.Tests.Components.Chart;

public class TwGanttChartTests : TwBlazorTestBase
{
    private static readonly DateTime _jan1 = new(2026, 1, 1);

    private static DateTime Day(int day) => _jan1.AddDays(day - 1);

    // A plan with a summary over two tasks and a milestone at the end.
    private static readonly ChartTask[] _plan =
    [
        new("Build", Day(1), Day(1)) { Id = "build" },
        new("Design", Day(1), Day(5)) { ParentId = "build", Progress = 1 },
        new("Code", Day(5), Day(15)) { ParentId = "build", Progress = 0.25 },
        new("Launch", Day(15), Day(15))
    ];

    private TwGanttTheme ganttTheme => Theme.Components.Require<TwGanttTheme>();

    private IRenderedComponent<TwGanttChart> Render(Action<ComponentParameterCollectionBuilder<TwGanttChart>>? configure = null, IReadOnlyList<ChartTask>? tasks = null, DateTime? today = null) =>
        TestContext.Render<TwGanttChart>(p =>
        {
            p.Add(x => x.Tasks, tasks ?? _plan);
            p.Add(x => x.Today, today ?? new DateTime(2030, 1, 1));
            configure?.Invoke(p);
        });

    private ChartScene Scene(Action<ComponentParameterCollectionBuilder<TwGanttChart>>? configure = null, IReadOnlyList<ChartTask>? tasks = null, DateTime? today = null) =>
        Render(configure, tasks, today).Instance.CreateScene();

    #region Plan

    [Fact]
    public void Plan_ListsTasksUnderTheirSummary_AndRollsUpDatesAndProgress()
    {
        var plan = GanttPlan.Build(_plan, new HashSet<string>(), rollUpSummaries: true);

        Assert.Equal(["Build", "Design", "Code", "Launch"], plan.Visible.Select(node => node.Task.Label));
        var summary = plan.All[0];
        Assert.True(summary.IsSummary);
        Assert.Equal((Day(1), Day(15)), (summary.Start, summary.End));
        // Design is 4 days done, Code a quarter of 10 days: (4 + 2.5) / 14.
        Assert.Equal(6.5 / 14, summary.Progress!.Value, 6);
        Assert.Equal([0, 1, 1, 0], plan.All.Select(node => node.Depth));
    }

    [Fact]
    public void Plan_CollapsedSummary_HidesItsTasks_AndStandsInForThem()
    {
        var plan = GanttPlan.Build(_plan, new HashSet<string> { "build" }, rollUpSummaries: true);

        Assert.Equal(["Build", "Launch"], plan.Visible.Select(node => node.Task.Label));
        Assert.False(plan.All[0].IsExpanded);
        Assert.Equal(0, plan.VisibleRowOf("Code"));
        Assert.Equal(1, plan.VisibleRowOf("Launch"));
        Assert.Equal(-1, plan.VisibleRowOf("missing"));
    }

    [Fact]
    public void Plan_MissingParentOrLoop_LeavesTheTaskAtTheTopLevel()
    {
        ChartTask[] tasks =
        [
            new("A", Day(1), Day(2)) { ParentId = "B" },
            new("B", Day(1), Day(2)) { ParentId = "A" },
            new("C", Day(1), Day(2)) { ParentId = "nowhere" }
        ];

        var plan = GanttPlan.Build(tasks, new HashSet<string>(), rollUpSummaries: true);

        Assert.Equal(3, plan.All.Count);
        Assert.Contains(plan.All, node => node.Task.Label == "C" && node.Depth == 0);
    }

    [Fact]
    public void Plan_WithoutRollUp_KeepsASummarysOwnDates()
    {
        ChartTask[] tasks = [new("Phase", Day(1), Day(30)), new("Task", Day(3), Day(4)) { ParentId = "Phase" }];

        var plan = GanttPlan.Build(tasks, new HashSet<string>(), rollUpSummaries: false);

        Assert.Equal((Day(1), Day(30)), (plan.All[0].Start, plan.All[0].End));
    }

    [Fact]
    public void Plan_TaskEndingWhenItStarts_IsAMilestone()
    {
        var plan = GanttPlan.Build(_plan, new HashSet<string>(), rollUpSummaries: true);

        Assert.True(plan.All[^1].IsMilestone);
        Assert.False(plan.All[0].IsMilestone);
    }

    #endregion

    #region Timeline

    [Fact]
    public void Timeline_DayZoom_WidensToWholeDays_AndGivesEachDayTheSameWidth()
    {
        var axis = GanttTimeline.Create(GanttZoom.Day, Day(1).AddHours(9), Day(4).AddHours(3), CultureInfo.InvariantCulture);

        Assert.Equal((Day(1), Day(5)), (axis.Start, axis.End));
        Assert.Equal(4, axis.SlotCount);
        Assert.Equal(37.5, axis.Position(Day(2).AddHours(12)), 6);
        Assert.Equal(Day(2).AddHours(12), axis.At(37.5));
        Assert.Equal(Day(3), axis.Snap(Day(2).AddHours(13)));
    }

    [Fact]
    public void Timeline_MonthZoom_GivesShortAndLongMonthsTheSameWidth()
    {
        var axis = GanttTimeline.Create(GanttZoom.Month, new DateTime(2026, 2, 10), new DateTime(2026, 3, 20), CultureInfo.InvariantCulture);

        Assert.Equal(2, axis.SlotCount);
        Assert.Equal(50, axis.Position(new DateTime(2026, 3, 1)), 6);
    }

    [Fact]
    public void Timeline_WeekZoom_StartsOnTheCulturesFirstDay()
    {
        // 1 January 2026 is a Thursday.
        var british = GanttTimeline.Create(GanttZoom.Week, Day(1), Day(2), CultureInfo.GetCultureInfo("en-GB"));
        var american = GanttTimeline.Create(GanttZoom.Week, Day(1), Day(2), CultureInfo.GetCultureInfo("en-US"));

        Assert.Equal(DayOfWeek.Monday, british.Start.DayOfWeek);
        Assert.Equal(DayOfWeek.Sunday, american.Start.DayOfWeek);
    }

    [Fact]
    public void Timeline_Auto_RunsEvenlyFromFirstToLastDate()
    {
        var axis = GanttTimeline.Create(GanttZoom.Auto, Day(1), Day(11), CultureInfo.InvariantCulture);

        Assert.Equal(0, axis.SlotCount);
        Assert.Equal(50, axis.Position(Day(6)), 6);
        Assert.Equal(Day(6), axis.Snap(Day(6).AddHours(5)));
    }

    #endregion

    #region Arrows

    [Fact]
    public void ArrowPath_EndsThatFaceEachOther_TakeThreeSegments()
    {
        var path = GanttArrow.Path(new(10, 10, 1), new(50, 30, 1), 2, 20);

        Assert.Equal(4, path.Count(c => c is 'M' or 'L'));
    }

    [Fact]
    public void ArrowPath_EndsThatOverlap_GoRoundTheTaskInFiveSegments()
    {
        var path = GanttArrow.Path(new(50, 10, 1), new(20, 30, 1), 2, 20);

        Assert.Equal(6, path.Count(c => c is 'M' or 'L'));
    }

    #endregion

    #region Scene

    [Fact]
    public void Scene_TaskWithProgress_DrawsATrackAndAFill()
    {
        var scene = Scene();

        var code = scene.Marks.OfType<ChartRect>().Where(rect => rect.Color == ChartColor.Series(0)).ToList();
        Assert.Contains(code, rect => rect.Opacity < 1);
        Assert.Contains(scene.Datums, datum => datum.AriaLabel == "Code: Start 5 Jan, End 15 Jan, Done 25%");
    }

    [Fact]
    public void Scene_Milestone_IsADiamond_WithASingleDate()
    {
        var scene = Scene();

        Assert.Contains(scene.Marks.OfType<ChartGlyph>(), glyph => glyph.Icon == TwBlazor.Enums.Icon.Diamond_Fill);
        Assert.Equal("Launch: Date 15 Jan", scene.Datums[^1].AriaLabel);
    }

    [Fact]
    public void Scene_Summary_IsASlimBar_AndListedInTheLegend()
    {
        var scene = Scene();

        var bars = scene.Marks.OfType<ChartRect>().ToList();
        var task = bars.Max(rect => rect.Height);
        Assert.Contains(bars, rect => rect.Height < task);
        Assert.DoesNotContain(scene.Marks.OfType<ChartPath>(), path => path.Filled);
        Assert.Contains(scene.Legend, item => item.Label == "Summary");
        Assert.Contains(scene.Legend, item => item.Label == "Milestone");
    }

    [Fact]
    public void TaskLabels_NearTheEnd_SitBeforeTheBar_SoTheyStayInsideThePlot()
    {
        var scene = Scene(p => p.Add(x => x.ShowTaskLabels, true));

        var labels = scene.Marks.OfType<ChartText>().ToList();
        Assert.All(labels, label => Assert.NotNull(label.MaxWidth));
        var last = labels.Single(label => label.Text == "Launch");
        Assert.Equal(ChartAnchor.End, last.Anchor);
        Assert.True(last.OffsetX < 0);
        Assert.Equal(ChartAnchor.Start, labels.First().Anchor);
    }

    [Fact]
    public void Datum_PointsItsTooltipAtTheBar()
    {
        var scene = Scene();

        var datum = scene.Datums[^1];
        Assert.Equal(datum.Y, datum.AnchorY);
        Assert.True(datum.AnchorX > 90);
    }

    [Fact]
    public void ScrollingPlot_OpensTooltipsInTheTopHalfDownwards()
    {
        var cut = Render(p => p.Add(x => x.Zoom, GanttZoom.Day));

        var tooltips = cut.FindAll("[role='tooltip']");
        Assert.Equal("bottom", tooltips[0].GetAttribute("data-tw-placement"));
        Assert.StartsWith("left:", tooltips[0].GetAttribute("style"));
        Assert.Contains(";top:", tooltips[0].GetAttribute("style"));
        Assert.Equal("top", tooltips[^1].GetAttribute("data-tw-placement"));
    }

    [Fact]
    public void Scene_Dependency_DrawsAnArrowWithItsMeaning()
    {
        var scene = Scene(p => p.Add(x => x.Dependencies, [new ChartTaskDependency("Design", "Code")]));

        var arrow = Assert.Single(scene.Marks.OfType<ChartPath>(), path => path.Title != null);
        Assert.Equal("Code starts after Design finishes", arrow.Title);
        Assert.Contains(scene.Marks.OfType<ChartGlyph>(), glyph => glyph.Icon == TwBlazor.Enums.Icon.Caret_Right_Fill);
    }

    [Fact]
    public void Scene_DependencyInsideACollapsedSummary_IsNotDrawn()
    {
        var scene = Scene(p => p
            .Add(x => x.Dependencies, [new ChartTaskDependency("Design", "Code"), new ChartTaskDependency("Code", "Launch")])
            .Add(x => x.CollapsedIds, new HashSet<string> { "build" }));

        // Design to Code both fold into Build; Code to Launch now runs from Build.
        Assert.Single(scene.Marks.OfType<ChartPath>(), path => path.Title != null);
    }

    [Fact]
    public void Scene_Today_IsMarkedOnlyInsideTheRange()
    {
        var inside = Scene(today: Day(8));
        var outside = Scene();

        Assert.Contains(inside.Marks.OfType<ChartLine>(), line => line.Color == ChartColor.Negative);
        Assert.Contains(inside.Legend, item => item.Label == "Today");
        Assert.DoesNotContain(outside.Marks.OfType<ChartLine>(), line => line.Color == ChartColor.Negative);
    }

    [Fact]
    public void TodayLegendEntry_HidesAndShowsTheLine()
    {
        var cut = Render(today: Day(8));
        bool HasLine() => cut.Instance.CreateScene().Marks.OfType<ChartLine>().Any(line => line.Color == ChartColor.Negative);
        AngleSharp.Dom.IElement TodayButton() => cut.FindAll("button[aria-pressed]").Single(b => b.TextContent.Trim() == "Today");

        Assert.Equal("true", TodayButton().GetAttribute("aria-pressed"));

        TodayButton().Click();

        Assert.False(HasLine());
        Assert.Equal("false", TodayButton().GetAttribute("aria-pressed"));
        Assert.Contains(cut.Instance.CreateScene().Legend, item => item.Label == "Today" && item.Hidden);

        TodayButton().Click();

        Assert.True(HasLine());
    }

    [Fact]
    public void Scene_DayZoom_LabelsEachDay_AndShadesTheWeekend()
    {
        var scene = Scene(p => p.Add(x => x.Zoom, GanttZoom.Day));

        Assert.Equal(15, scene.XAxis!.Ticks.Count);
        Assert.Equal("1 Jan", scene.XAxis.Ticks[0].Label);
        Assert.Equal("2", scene.XAxis.Ticks[1].Label);
        // 3-4 and 10-11 January 2026 are weekends: one shaded block for each.
        var shading = Assert.Single(scene.Marks.OfType<ChartPath>(), path => path.Color == ChartColor.Track);
        Assert.Equal(2, shading.Data.Count(c => c == 'M'));
        Assert.Contains(scene.Legend, item => item.Label == "Non-working day");
    }

    [Fact]
    public void Scene_NonWorkingDays_CanBeChosen()
    {
        var scene = Scene(p => p.Add(x => x.Zoom, GanttZoom.Day).Add(x => x.NonWorkingDays, [DayOfWeek.Friday]));

        Assert.Equal(2, Assert.Single(scene.Marks.OfType<ChartPath>(), path => path.Color == ChartColor.Track).Data.Count(c => c == 'M'));
    }

    [Fact]
    public void Scene_TodayOnANonWorkingDay_IsDrawnAboveTheShading()
    {
        // 3 January 2026 is a Saturday.
        var scene = Scene(p => p.Add(x => x.Zoom, GanttZoom.Day), today: Day(3).AddHours(12));

        var marks = scene.Marks.ToList();
        var shading = marks.FindIndex(mark => mark is ChartPath { Color: var color } && color == ChartColor.Track);
        var today = marks.FindIndex(mark => mark is ChartLine { Color: var color } && color == ChartColor.Negative);
        Assert.InRange(shading, 0, today - 1);
        Assert.DoesNotContain(marks, mark => mark is ChartRect { Color: var color } && color == ChartColor.Track);
    }

    [Fact]
    public void Scene_Table_ListsEveryTask_IncludingCollapsedOnes()
    {
        var scene = Scene(p => p.Add(x => x.CollapsedIds, new HashSet<string> { "build" }));

        Assert.Equal(2, scene.Datums.Count);
        Assert.Equal(4, scene.Table!.Rows.Count);
        Assert.Equal(["Task", "Start", "End", "Done"], scene.Table.Headers);
    }

    [Fact]
    public void Scene_ColorIndex_PicksTheSeriesColor()
    {
        ChartTask[] tasks = [new("A", Day(1), Day(3)) { ColorIndex = 2 }];

        var scene = Scene(tasks: tasks);

        Assert.Single(scene.Marks.OfType<ChartRect>(), rect => rect.Color == ChartColor.Series(2));
    }

    [Fact]
    public void Scene_TaskLabels_AreDrawnAfterEachBar()
    {
        var scene = Scene(p => p.Add(x => x.ShowTaskLabels, true));

        Assert.Equal(4, scene.Marks.OfType<ChartText>().Count());
    }

    #endregion

    #region Rendering

    [Fact]
    public void Tree_RendersAnExpandButtonPerSummary_ThatCollapsesIt()
    {
        IReadOnlySet<string>? reported = null;
        var cut = Render(p => p.Add(x => x.CollapsedIdsChanged, value => reported = value));

        var toggle = cut.FindAll($"button.{ganttTheme.Toggle.Split(' ')[0]}[aria-expanded]").First(b => b.GetAttribute("aria-label") == "Hide the tasks under Build");
        Assert.Equal("true", toggle.GetAttribute("aria-expanded"));

        toggle.Click();

        Assert.Equal(["build"], reported!);
        Assert.Contains(cut.FindAll("button[aria-expanded='false']"), b => b.GetAttribute("aria-label") == "Show the tasks under Build");
        Assert.Equal(2, cut.Instance.CreateScene().Datums.Count);
    }

    [Fact]
    public void FlatPlan_KeepsPlainTaskNames()
    {
        ChartTask[] tasks = [new("A", Day(1), Day(3)), new("B", Day(2), Day(4))];

        var cut = Render(tasks: tasks);

        Assert.Empty(cut.FindAll("button[aria-expanded]"));
    }

    [Fact]
    public void DefaultExpandAll_False_StartsWithSummariesCollapsed()
    {
        var cut = Render(p => p.Add(x => x.DefaultExpandAll, false));

        Assert.Equal(2, cut.Instance.CreateScene().Datums.Count);
    }

    [Fact]
    public async Task ExpandAllAndCollapseAll_ChangeEverySummary()
    {
        var cut = Render();

        await cut.InvokeAsync(cut.Instance.CollapseAllAsync);
        Assert.Equal(2, cut.Instance.CreateScene().Datums.Count);

        await cut.InvokeAsync(cut.Instance.ExpandAllAsync);
        Assert.Equal(4, cut.Instance.CreateScene().Datums.Count);
    }

    [Fact]
    public void ZoomControls_ChangeTheZoom_AndScrollTheWiderPlot()
    {
        GanttZoom? reported = null;
        var cut = Render(p => p.Add(x => x.ZoomChanged, value => reported = value));

        Assert.Empty(cut.FindAll("[style^='min-width']"));

        cut.FindAll("button").First(b => b.TextContent.Trim() == "Day").Click();

        Assert.Equal(GanttZoom.Day, reported);
        // 1 to 15 January is 15 days of 40 pixels.
        Assert.Equal("min-width:600px", cut.Find("[style^='min-width']").GetAttribute("style"));
    }

    [Fact]
    public void ScrollingPlot_GivesTheTaskLabelsThePlotHeight()
    {
        var cut = Render(p => p.Add(x => x.Zoom, GanttZoom.Day));

        var plot = cut.Find("[style^='min-width']").FirstElementChild!;
        var labels = cut.Find("[style^='min-width']").ParentElement!.PreviousElementSibling!;
        var heightClass = plot.ClassList.Single(c => c.StartsWith("h-"));

        Assert.Contains(heightClass, labels.ClassList);
    }

    [Fact]
    public void ShowToolbar_False_HidesTheControls()
    {
        var cut = Render(p => p.Add(x => x.ShowToolbar, false));

        Assert.DoesNotContain(cut.FindAll("button"), b => b.TextContent.Trim() is "Day" or "Expand all");
    }

    [Fact]
    public void RowHeight_SizesThePlotToItsRows()
    {
        var cut = Render(p => p.Add(x => x.RowHeight, 30));

        Assert.NotEmpty(cut.FindAll("[style='height:120px']"));
    }

    [Fact]
    public void OnTaskClick_IsRaisedForTheClickedRow()
    {
        ChartTask? clicked = null;
        var cut = Render(p => p.Add(x => x.OnTaskClick, task => clicked = task));

        cut.FindAll("[data-chart-datum]")[2].Click();

        Assert.Equal("Code", clicked?.Label);
    }

    [Fact]
    public void Dragging_DrawsAHandleOutlinePerBar_ButNotForRolledUpSummaries()
    {
        var cut = Render(p => p.Add(x => x.AllowDrag, true).Add(x => x.AllowResize, true));

        var bars = cut.FindAll("[data-gantt-bar]");
        Assert.Equal(["Design", "Code", "Launch"], bars.Select(bar => bar.GetAttribute("data-key")));
        Assert.Equal("move", bars[2].GetAttribute("data-flags"));
        Assert.Equal("move resize", bars[0].GetAttribute("data-flags"));
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twGanttChart.attach");
    }

    [Fact]
    public void WithoutGestures_NoOverlayOrScriptIsUsed()
    {
        var cut = Render();

        Assert.Empty(cut.FindAll("[data-gantt-overlay]"));
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twGanttChart.attach");
    }

    #endregion

    #region Gestures

    [Fact]
    public async Task CommitBarDrag_Move_KeepsTheLength_AndSnaps()
    {
        ChartTaskChange? change = null;
        var cut = Render(p => p.Add(x => x.AllowDrag, true).Add(x => x.OnTaskChange, c => change = c));

        // The plot runs 1 to 15 January at Auto zoom: 50% is 8 January, plus a few hours that snap away.
        var accepted = await cut.InvokeAsync(() => cut.Instance.CommitBarDrag("Design", "move", 50.5, 80));

        Assert.True(accepted);
        Assert.Equal(ChartTaskChangeKind.Move, change!.Kind);
        Assert.Equal((Day(8), Day(12)), (change.Start, change.End));
    }

    [Fact]
    public async Task CommitBarDrag_ResizeEnd_ChangesOnlyTheEnd()
    {
        ChartTaskChange? change = null;
        var cut = Render(p => p.Add(x => x.AllowResize, true).Add(x => x.OnTaskChange, c => change = c));

        // 80% of 1 to 15 January is the 12th, plus a few hours that snap away.
        await cut.InvokeAsync(() => cut.Instance.CommitBarDrag("Code", "resize-end", 0, 80));

        Assert.Equal((Day(5), Day(12)), (change!.Start, change.End));
        Assert.Equal(ChartTaskChangeKind.ResizeEnd, change.Kind);
    }

    [Fact]
    public async Task CommitBarDrag_Cancelled_ReportsTheRefusal()
    {
        var cut = Render(p => p.Add(x => x.AllowDrag, true).Add(x => x.OnTaskChange, c => c.Cancel = true));

        var accepted = await cut.InvokeAsync(() => cut.Instance.CommitBarDrag("Design", "move", 50, 80));

        Assert.False(accepted);
    }

    [Fact]
    public async Task CommitProgressDrag_ReportsTheNewProgress()
    {
        ChartTaskChange? change = null;
        var cut = Render(p => p.Add(x => x.AllowProgressDrag, true).Add(x => x.OnTaskChange, c => change = c));

        await cut.InvokeAsync(() => cut.Instance.CommitProgressDrag("Code", 0.6049));

        Assert.Equal(ChartTaskChangeKind.Progress, change!.Kind);
        Assert.Equal(0.6, change.Progress);
        Assert.Equal((Day(5), Day(15)), (change.Start, change.End));
    }

    [Theory]
    [InlineData("end", "start", ChartDependencyType.FinishToStart)]
    [InlineData("start", "start", ChartDependencyType.StartToStart)]
    [InlineData("end", "end", ChartDependencyType.FinishToFinish)]
    [InlineData("start", "end", ChartDependencyType.StartToFinish)]
    public async Task CommitLink_TheEndsPickedDecideTheType(string fromAnchor, string toAnchor, ChartDependencyType expected)
    {
        ChartDependencyRequest? request = null;
        var cut = Render(p => p.Add(x => x.AllowLinking, true).Add(x => x.OnDependencyCreate, r => request = r));

        var accepted = await cut.InvokeAsync(() => cut.Instance.CommitLink("Design", fromAnchor, "Code", toAnchor));

        Assert.True(accepted);
        Assert.Equal(new ChartTaskDependency("Design", "Code", expected), request!.Dependency);
        Assert.Equal("Code", request.To.Label);
    }

    [Fact]
    public async Task CommitLink_ToTheSameTask_IsRefused()
    {
        var raised = false;
        var cut = Render(p => p.Add(x => x.AllowLinking, true).Add(x => x.OnDependencyCreate, _ => raised = true));

        var accepted = await cut.InvokeAsync(() => cut.Instance.CommitLink("Code", "end", "Code", "start"));

        Assert.False(accepted);
        Assert.False(raised);
    }

    [Fact]
    public void DependencyClick_RaisesTheDependency()
    {
        ChartTaskDependency? clicked = null;
        var cut = Render(p => p
            .Add(x => x.Dependencies, [new ChartTaskDependency("Design", "Code")])
            .Add(x => x.AllowLinking, true)
            .Add(x => x.OnDependencyClick, d => clicked = d));

        cut.Find("[data-gantt-link]").Click();

        Assert.Equal(new ChartTaskDependency("Design", "Code"), clicked);
    }

    #endregion
}
