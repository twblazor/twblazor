// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Schedule;

public class TwScheduleTests : TwBlazorTestBase
{
    [Fact]
    public void RendersHeaderDateLabels_ForSelectedDate()
    {
        var selectedDate = new DateTime(2026, 3, 18); // Wednesday, 4th Monday-start week of March 2026

        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.SelectedDate, selectedDate));

        Assert.Contains("March 2026", cut.Markup); // title
        Assert.Contains("Week 4", cut.Markup); // week-number chip
        Assert.Contains("16 Mar 2026", cut.Markup); // subtitle range start (Monday)
        Assert.Contains("22 Mar 2026", cut.Markup); // subtitle range end (Sunday)
    }

    [Fact]
    public void RootContainer_SpansFullWidth()
    {
        var cut = TestContext.Render<TwSchedule<string>>();

        Assert.Contains("w-full", cut.Find($"#{cut.Instance.Id}").GetAttribute("class"));
    }

    [Theory]
    [InlineData(TwScheduleView.Day)]
    [InlineData(TwScheduleView.Week)]
    public void ScrollToTime_DefaultsToEightAm_AndScrollsOnFirstRenderOnly(TwScheduleView view)
    {
        var cut = TestContext.Render<TwSchedule<string>>(p => p.Add(x => x.View, view));

        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twSchedule.scrollToFraction");
        var fraction = Assert.IsType<double>(invocation.Arguments[1]);
        Assert.Equal(8.0 * 60 / (24 * 60), fraction, precision: 6);

        // Navigating within the same view (Previous/Next) shouldn't re-trigger the scroll - the
        // component instance persists, so OnAfterRenderAsync's firstRender only fires once.
        cut.Find("[aria-label='Next']").Click();
        Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twSchedule.scrollToFraction");
    }

    [Theory]
    [InlineData(TwScheduleView.Day)]
    [InlineData(TwScheduleView.Week)]
    public void ScrollToTime_CanBeCustomized(TwScheduleView view)
    {
        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.View, view)
            .Add(x => x.ScrollToTime, TimeSpan.FromHours(13.5)));

        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twSchedule.scrollToFraction");
        var fraction = Assert.IsType<double>(invocation.Arguments[1]);
        Assert.Equal(13.5 * 60 / (24 * 60), fraction, precision: 6);
    }

    [Theory]
    [InlineData(TwScheduleView.Day, -1, 0, 0)]
    [InlineData(TwScheduleView.Week, 0, -7, 0)]
    [InlineData(TwScheduleView.Month, 0, 0, -1)]
    public void PreviousButton_StepsByViewsUnit(TwScheduleView view, int expectedDayDelta, int expectedWeekDayDelta, int expectedMonthDelta)
    {
        var selectedDate = new DateTime(2026, 3, 18);
        DateTime? result = null;

        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.SelectedDate, selectedDate)
            .Add(x => x.View, view)
            .Add(x => x.SelectedDateChanged, EventCallback.Factory.Create<DateTime>(this, d => result = d)));

        cut.Find("[aria-label='Previous']").Click();

        var expected = view switch
        {
            TwScheduleView.Day => selectedDate.AddDays(expectedDayDelta),
            TwScheduleView.Week => selectedDate.AddDays(expectedWeekDayDelta),
            TwScheduleView.Month => selectedDate.AddMonths(expectedMonthDelta),
            _ => selectedDate
        };

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(TwScheduleView.Day)]
    [InlineData(TwScheduleView.Week)]
    [InlineData(TwScheduleView.Month)]
    public void NextButton_StepsByViewsUnit(TwScheduleView view)
    {
        var selectedDate = new DateTime(2026, 3, 18);
        DateTime? result = null;

        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.SelectedDate, selectedDate)
            .Add(x => x.View, view)
            .Add(x => x.SelectedDateChanged, EventCallback.Factory.Create<DateTime>(this, d => result = d)));

        cut.Find("[aria-label='Next']").Click();

        var expected = view switch
        {
            TwScheduleView.Day => selectedDate.AddDays(1),
            TwScheduleView.Week => selectedDate.AddDays(7),
            TwScheduleView.Month => selectedDate.AddMonths(1),
            _ => selectedDate
        };

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TodayButton_SetsSelectedDateToToday()
    {
        DateTime? result = null;

        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.SelectedDate, new DateTime(2020, 1, 1))
            .Add(x => x.SelectedDateChanged, EventCallback.Factory.Create<DateTime>(this, d => result = d)));

        cut.FindAll("button").First(b => b.TextContent.Trim() == "Today").Click();

        Assert.Equal(DateTime.Today, result);
    }

    [Fact]
    public void TodayButton_IsARealButtonElement_WithVisibleLabel_NotAnIcon()
    {
        var cut = TestContext.Render<TwSchedule<string>>();

        var todayButton = cut.FindAll("button").First(b => b.TextContent.Trim() == "Today");

        Assert.Equal("button", todayButton.TagName.ToLowerInvariant());
        Assert.Empty(todayButton.QuerySelectorAll("i"));
    }

    [Theory]
    [InlineData(null, "40rem")]
    [InlineData("20rem", "20rem")]
    [InlineData("600px", "600px")]
    public void MaxHeight_AppliesToScrollableTimeGrid_InDayView(string? maxHeight, string expected)
    {
        var cut = TestContext.Render<TwSchedule<string>>(p =>
        {
            p.Add(x => x.View, TwScheduleView.Day);
            if (maxHeight is not null)
            {
                p.Add(x => x.MaxHeight, maxHeight);
            }
        });

        var style = cut.Find("div.overflow-y-auto").GetAttribute("style");
        Assert.Contains($"max-height:{expected}", style);
    }

    [Theory]
    [InlineData(null, "40rem")]
    [InlineData("20rem", "20rem")]
    public void MaxHeight_AppliesToScrollableTimeGrid_InWeekView(string? maxHeight, string expected)
    {
        var cut = TestContext.Render<TwSchedule<string>>(p =>
        {
            p.Add(x => x.View, TwScheduleView.Week);
            if (maxHeight is not null)
            {
                p.Add(x => x.MaxHeight, maxHeight);
            }
        });

        var style = cut.Find("div.overflow-y-auto").GetAttribute("style");
        Assert.Contains($"max-height:{expected}", style);
    }

    [Theory]
    [InlineData(TwScheduleView.Day)]
    [InlineData(TwScheduleView.Week)]
    public void ScrollContainer_UsesItemsStart_NotDefaultStretch(TwScheduleView view)
    {
        // Regression test: flexbox's default align-items:stretch resizes the gutter/day-column flex
        // children to the (max-height-clamped) scroll container's cross size instead of their full
        // content height - corrupting the scrollable area and pushing event chips (positioned via an
        // absolute "top" measured from the day column's own top edge) down to wherever that shrunk
        // box ends up, which visually reads as events clustering near the bottom of the day regardless
        // of their real time. items-start keeps the children at their natural (144rem) height instead.
        var cut = TestContext.Render<TwSchedule<string>>(p => p.Add(x => x.View, view));

        var scrollContainer = cut.Find("div.overflow-y-auto");

        Assert.Contains("items-start", scrollContainer.GetAttribute("class"));
    }

    [Theory]
    [InlineData(0, 0, "Midnight event")]
    [InlineData(9, 0, "Morning event")]
    [InlineData(23, 30, "Late night event")]
    public void EventChip_TopOffset_ScalesLinearlyWithTimeOfDay_InDayView(int hour, int minute, string label)
    {
        // Regression test guarding the actual visual bug reported: events must be positioned
        // proportionally to their time of day (a 9am event roughly a third of the way down a
        // midnight-to-midnight column), not bunched near the bottom regardless of their real time.
        var day = new DateTime(2026, 3, 18);
        var start = day.AddHours(hour).AddMinutes(minute);
        var evt = new Schedule<string> { Name = label, DateTimeStart = start, DateTimeEnd = start.AddMinutes(30) };

        var cut = TestContext.Render<TwScheduleDayColumn<string>>(p => p
            .Add(x => x.Date, day)
            .Add(x => x.Events, new List<Schedule<string>> { evt }));

        var chip = cut.Find($"button[aria-label^='{label}']");
        var style = chip.GetAttribute("style")!;

        var expectedTopRem = (hour * 60 + minute) / 30m * 3m; // minutesPerSlot=30, slotHeightRem=3
        Assert.Contains($"top:{expectedTopRem.ToString(System.Globalization.CultureInfo.InvariantCulture)}rem", style);
    }

    [Fact]
    public void EventChip_InDayView_UsesCustomColor_WhenSet()
    {
        var day = new DateTime(2026, 3, 18);
        var evt = new Schedule<string> { Name = "Design review", DateTimeStart = day.AddHours(9), DateTimeEnd = day.AddHours(10), Color = "#2563eb" };

        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.SelectedDate, day)
            .Add(x => x.View, TwScheduleView.Day)
            .Add(x => x.Schedules, [evt]));

        var chip = cut.Find("button[aria-label^='Design review']");
        var style = chip.GetAttribute("style");

        Assert.Contains("background-color:#2563eb26;", style); // tinted background
        Assert.Contains("border-left-color:#2563eb;", style);
        Assert.Contains("color:#2563eb;", style);
    }

    [Fact]
    public void EventChip_InDayView_HasNoInlineBackground_WhenColorNotSet()
    {
        var day = new DateTime(2026, 3, 18);
        var evt = new Schedule<string> { Name = "Design review", DateTimeStart = day.AddHours(9), DateTimeEnd = day.AddHours(10) };

        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.SelectedDate, day)
            .Add(x => x.View, TwScheduleView.Day)
            .Add(x => x.Schedules, [evt]));

        var chip = cut.Find("button[aria-label^='Design review']");

        Assert.DoesNotContain("background-color", chip.GetAttribute("style"));
    }

    private static Schedule<string> Event(string name, DateTime start, DateTime end, bool readOnly = false) => new()
    {
        Name = name,
        DateTimeStart = start,
        DateTimeEnd = end,
        ReadOnly = readOnly
    };

    [Fact]
    public void DragEvent_InDayView_MovesEvent_PreservingDuration()
    {
        var day = new DateTime(2026, 3, 18);
        var evt = Event("Design review", day.AddHours(9), day.AddHours(10));
        List<Schedule<string>>? updatedList = null;

        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.SelectedDate, day)
            .Add(x => x.View, TwScheduleView.Day)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [evt])
            .Add(x => x.SchedulesChanged, EventCallback.Factory.Create<List<Schedule<string>>>(this, list => updatedList = list)));

        cut.Find("button[aria-label^='Design review']").DragStart(new DragEventArgs());
        cut.Find("button[aria-label='11:00 AM']").Drop(new DragEventArgs());

        var moved = Assert.Single(Assert.IsType<List<Schedule<string>>>(updatedList));
        Assert.Equal(new DateTimeOffset(day.AddHours(11)), moved.DateTimeStart);
        Assert.Equal(new DateTimeOffset(day.AddHours(12)), moved.DateTimeEnd);
    }

    [Fact]
    public void DragEvent_InWeekView_CanMoveEventToADifferentDay()
    {
        var wednesday = new DateTime(2026, 3, 18);
        var evt = Event("Design review", wednesday.AddHours(9), wednesday.AddHours(10));
        List<Schedule<string>>? updatedList = null;

        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.SelectedDate, wednesday)
            .Add(x => x.View, TwScheduleView.Week)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [evt])
            .Add(x => x.SchedulesChanged, EventCallback.Factory.Create<List<Schedule<string>>>(this, list => updatedList = list)));

        cut.Find("button[aria-label^='Design review']").DragStart(new DragEventArgs());

        // Columns render Monday(0)..Sunday(6); Wednesday is index 2, Thursday index 3.
        var thursdayColumn = cut.FindComponents<TwScheduleDayColumn<string>>()[3];
        thursdayColumn.Find("button[aria-label='9:00 AM']").Drop(new DragEventArgs());

        var moved = Assert.Single(Assert.IsType<List<Schedule<string>>>(updatedList));
        Assert.Equal(new DateTimeOffset(wednesday.AddDays(1).AddHours(9)), moved.DateTimeStart);
        Assert.Equal(new DateTimeOffset(wednesday.AddDays(1).AddHours(10)), moved.DateTimeEnd);
    }

    [Fact]
    public void DragEvent_InMonthView_ChangesDateOnly_PreservingTimeOfDayAndDuration()
    {
        var evt = Event("Design review", new DateTime(2026, 3, 10, 9, 30, 0), new DateTime(2026, 3, 10, 10, 15, 0));
        List<Schedule<string>>? updatedList = null;

        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.SelectedDate, new DateTime(2026, 3, 15))
            .Add(x => x.View, TwScheduleView.Month)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [evt])
            .Add(x => x.SchedulesChanged, EventCallback.Factory.Create<List<Schedule<string>>>(this, list => updatedList = list)));

        cut.Find("button[aria-label^='Design review']").DragStart(new DragEventArgs());

        var dayButton = cut.FindAll("button").First(b => b.TextContent.Trim() == "20");
        var cell = dayButton.Closest("td");
        Assert.NotNull(cell);
        cell!.Drop(new DragEventArgs());

        var moved = Assert.Single(Assert.IsType<List<Schedule<string>>>(updatedList));
        Assert.Equal(new DateTimeOffset(2026, 3, 20, 9, 30, 0, TimeSpan.Zero), moved.DateTimeStart);
        Assert.Equal(new DateTimeOffset(2026, 3, 20, 10, 15, 0, TimeSpan.Zero), moved.DateTimeEnd);
    }

    [Fact]
    public void DragEvent_ReadOnlyEvent_IsNotDraggable()
    {
        var day = new DateTime(2026, 3, 18);
        var evt = Event("Locked", day.AddHours(9), day.AddHours(10), readOnly: true);

        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.SelectedDate, day)
            .Add(x => x.View, TwScheduleView.Day)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [evt]));

        var chip = cut.Find("button[aria-label^='Locked']");

        Assert.Equal("false", chip.GetAttribute("draggable"));
    }

    [Fact]
    public void DragEvent_WhenNotEditable_EventIsNotDraggable()
    {
        var day = new DateTime(2026, 3, 18);
        var evt = Event("Design review", day.AddHours(9), day.AddHours(10));

        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.SelectedDate, day)
            .Add(x => x.View, TwScheduleView.Day)
            .Add(x => x.Editable, false)
            .Add(x => x.Schedules, [evt]));

        var chip = cut.Find("button[aria-label^='Design review']");

        Assert.Equal("false", chip.GetAttribute("draggable"));
    }

    [Fact]
    public void ViewSwitch_InvokesViewChanged_AndSwapsRenderedViewComponent()
    {
        TwScheduleView? result = null;

        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.View, TwScheduleView.Week)
            .Add(x => x.ViewChanged, EventCallback.Factory.Create<TwScheduleView>(this, v => result = v)));

        Assert.NotNull(cut.FindComponent<TwScheduleWeekView<string>>());

        cut.Find("[aria-label='Month view']").Click();

        Assert.Equal(TwScheduleView.Month, result);
    }

    [Fact]
    public void ViewSwitcher_MarksActiveViewButton_WithAriaPressed()
    {
        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.View, TwScheduleView.Week));

        Assert.Equal("false", cut.Find("[aria-label='Day view']").GetAttribute("aria-pressed"));
        Assert.Equal("true", cut.Find("[aria-label='Week view']").GetAttribute("aria-pressed"));
        Assert.Equal("false", cut.Find("[aria-label='Month view']").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void RendersDayView_WhenViewIsDay()
    {
        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.View, TwScheduleView.Day));

        Assert.NotNull(cut.FindComponent<TwScheduleDayView<string>>());
    }

    [Fact]
    public void RendersMonthView_WhenViewIsMonth()
    {
        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.View, TwScheduleView.Month));

        Assert.NotNull(cut.FindComponent<TwScheduleMonthView<string>>());
    }

    [Fact]
    public void AddEventButton_HiddenByDefault_ShownWhenEditable()
    {
        var cut = TestContext.Render<TwSchedule<string>>();

        Assert.DoesNotContain("Add event", cut.Markup);

        cut.Render(p => p.Add(x => x.Editable, true));

        Assert.Contains("Add event", cut.Markup);
    }

    [Fact]
    public void SearchButton_HiddenByDefault_ShownWhenOnSearchHasDelegate()
    {
        var cut = TestContext.Render<TwSchedule<string>>();

        Assert.Empty(cut.FindAll("[aria-label='Search events']"));

        cut.Render(p => p
            .Add(x => x.OnSearch, EventCallback.Factory.Create(this, () => { })));

        Assert.NotEmpty(cut.FindAll("[aria-label='Search events']"));
    }

    #region Now indicator

    [Theory]
    [InlineData(TwScheduleView.Day)]
    [InlineData(TwScheduleView.Week)]
    public void NowIndicator_RendersLineAndGutterLabel_RegardlessOfViewedDate(TwScheduleView view)
    {
        // The indicator is a "what time is it right now" reference bar, not scoped to today - it must
        // still render even when browsing a date far from today.
        var before = DateTime.Now;
        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.SelectedDate, DateTime.Today.AddDays(30))
            .Add(x => x.View, view));
        var after = DateTime.Now;

        var line = cut.Find("div.border-t-2");
        var label = cut.Find("div.right-2");

        var minTop = TwScheduleTimeGrid.GetOffsetRem(before.TimeOfDay.TotalMinutes);
        var maxTop = TwScheduleTimeGrid.GetOffsetRem(after.TimeOfDay.TotalMinutes);
        var lineStyle = line.GetAttribute("style")!;
        var actualTop = decimal.Parse(lineStyle.Split("top:")[1].Split("rem")[0], System.Globalization.CultureInfo.InvariantCulture);

        Assert.InRange(actualTop, minTop, maxTop);
        Assert.NotEmpty(label.TextContent);
    }

    [Fact]
    public void NowIndicator_Dot_AlwaysPresent_AtLeftEdge_InDayView()
    {
        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.SelectedDate, DateTime.Today.AddDays(30))
            .Add(x => x.View, TwScheduleView.Day));

        var dot = cut.Find("div.border-t-2").QuerySelector("span");

        Assert.NotNull(dot);
        Assert.Contains("left:0", dot!.GetAttribute("style"));
    }

    [Fact]
    public void NowIndicator_Dot_Present_InWeekView_WhenTodayIsInTheDisplayedWeek()
    {
        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.SelectedDate, DateTime.Today)
            .Add(x => x.View, TwScheduleView.Week));

        var dot = cut.Find("div.border-t-2").QuerySelector("span");
        var weekStart = TwBlazor.Utilities.DateHelpers.GetStartOfWeek(DateTime.Today);
        var expectedLeft = 100m / 7 * (DateTime.Today - weekStart).Days;

        Assert.NotNull(dot);
        Assert.Contains($"left:{expectedLeft.ToString(System.Globalization.CultureInfo.InvariantCulture)}%", dot!.GetAttribute("style"));
    }

    [Fact]
    public void NowIndicator_Dot_Absent_InWeekView_WhenTodayIsNotInTheDisplayedWeek()
    {
        var cut = TestContext.Render<TwSchedule<string>>(p => p
            .Add(x => x.SelectedDate, DateTime.Today.AddDays(30))
            .Add(x => x.View, TwScheduleView.Week));

        var dot = cut.Find("div.border-t-2").QuerySelector("span");

        Assert.Null(dot);
    }

    #endregion
}
