// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Calendar;

public class TwCalendarTests : TwBlazorTestBase
{
    [Fact]
    public void RendersHeaderDateLabels_ForSelectedDate()
    {
        var selectedDate = new DateTime(2026, 3, 18); // Wednesday, 4th Monday-start week of March 2026

        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, selectedDate)
            .Add(x => x.View, TwCalendarView.Month));

        Assert.Contains("March 2026", cut.Markup); // title
        Assert.Contains("Week 4", cut.Markup); // week-number chip
        Assert.Contains("16 Mar 2026", cut.Markup); // subtitle range start (Monday)
        Assert.Contains("22 Mar 2026", cut.Markup); // subtitle range end (Sunday)
    }

    [Fact]
    public void RootContainer_SpansFullWidth()
    {
        var cut = TestContext.Render<TwCalendar<string>>();

        Assert.Contains("w-full", cut.Find($"#{cut.Instance.Id}").GetAttribute("class"));
    }

    [Theory]
    [InlineData(TwCalendarView.Day)]
    [InlineData(TwCalendarView.Week)]
    public void ScrollToTime_DefaultsToEightAm_AndScrollsOnFirstRenderOnly(TwCalendarView view)
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p.Add(x => x.View, view));

        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twCalendar.scrollToFraction");
        var fraction = Assert.IsType<double>(invocation.Arguments[1]);
        Assert.Equal(8.0 * 60 / (24 * 60), fraction, precision: 6);

        // Navigating within the same view (Previous/Next) shouldn't re-trigger the scroll - the
        // component instance persists, so OnAfterRenderAsync's firstRender only fires once.
        cut.Find("[aria-label='Next']").Click();
        Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twCalendar.scrollToFraction");
    }

    [Theory]
    [InlineData(TwCalendarView.Day)]
    [InlineData(TwCalendarView.Week)]
    public void ScrollToTime_CanBeCustomized(TwCalendarView view)
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.View, view)
            .Add(x => x.ScrollToTime, TimeSpan.FromHours(13.5)));

        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twCalendar.scrollToFraction");
        var fraction = Assert.IsType<double>(invocation.Arguments[1]);
        Assert.Equal(13.5 * 60 / (24 * 60), fraction, precision: 6);
    }

    [Theory]
    [InlineData(TwCalendarView.Day, -1, 0, 0)]
    [InlineData(TwCalendarView.Week, 0, -7, 0)]
    [InlineData(TwCalendarView.Month, 0, 0, -1)]
    public void PreviousButton_StepsByViewsUnit(TwCalendarView view, int expectedDayDelta, int expectedWeekDayDelta, int expectedMonthDelta)
    {
        var selectedDate = new DateTime(2026, 3, 18);
        DateTime? result = null;

        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, selectedDate)
            .Add(x => x.View, view)
            .Add(x => x.SelectedDateChanged, EventCallback.Factory.Create<DateTime>(this, d => result = d)));

        cut.Find("[aria-label='Previous']").Click();

        var expected = view switch
        {
            TwCalendarView.Day => selectedDate.AddDays(expectedDayDelta),
            TwCalendarView.Week => selectedDate.AddDays(expectedWeekDayDelta),
            TwCalendarView.Month => selectedDate.AddMonths(expectedMonthDelta),
            _ => selectedDate
        };

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(TwCalendarView.Day)]
    [InlineData(TwCalendarView.Week)]
    [InlineData(TwCalendarView.Month)]
    public void NextButton_StepsByViewsUnit(TwCalendarView view)
    {
        var selectedDate = new DateTime(2026, 3, 18);
        DateTime? result = null;

        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, selectedDate)
            .Add(x => x.View, view)
            .Add(x => x.SelectedDateChanged, EventCallback.Factory.Create<DateTime>(this, d => result = d)));

        cut.Find("[aria-label='Next']").Click();

        var expected = view switch
        {
            TwCalendarView.Day => selectedDate.AddDays(1),
            TwCalendarView.Week => selectedDate.AddDays(7),
            TwCalendarView.Month => selectedDate.AddMonths(1),
            _ => selectedDate
        };

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TodayButton_SetsSelectedDateToToday()
    {
        DateTime? result = null;

        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, new DateTime(2020, 1, 1))
            .Add(x => x.SelectedDateChanged, EventCallback.Factory.Create<DateTime>(this, d => result = d)));

        FindTodayButton(cut).Click();

        Assert.Equal(DateTime.Today, result);
    }

    private static AngleSharp.Dom.IElement FindTodayButton(IRenderedComponent<TwCalendar<string>> cut) =>
        cut.FindAll("button").Single(b => b.TextContent.Trim() == "Today");

    [Fact]
    public void TodayButton_IsAFilledPrimaryTextButton()
    {
        var cut = TestContext.Render<TwCalendar<string>>();

        var today = FindTodayButton(cut);

        Assert.Contains("bg-purple-600", today.GetAttribute("class")); // filled primary, not outlined
        Assert.DoesNotContain("border", today.GetAttribute("class")!.Split(' ')); // not outlined
        Assert.Null(today.GetAttribute("aria-describedby")); // its visible label needs no tooltip
    }

    [Fact]
    public void TodayButton_IsDense_HasAPinIcon_AndFollowsTheTitle()
    {
        var cut = TestContext.Render<TwCalendar<string>>();

        var today = FindTodayButton(cut);

        Assert.Contains(Theme.Components.Require<TwBlazor.Configuration.Components.TwButtonTheme>().DensePadding, today.GetAttribute("class"));
        Assert.NotNull(today.QuerySelector("i.bi-pin"));

        var lead = today.ParentElement!;
        var children = lead.Children.ToList();
        Assert.True(children.IndexOf(today) > children.IndexOf(lead.QuerySelector("span.font-semibold")!.ParentElement!.ParentElement!));
        Assert.Equal(today, children.Last());
    }

    [Theory]
    [InlineData(TwCalendarView.Week)]
    [InlineData(TwCalendarView.Month)]
    public void PressingToday_PulsesTodaysColumnOrCell_AndOnlyThat(TwCalendarView view)
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, new DateTime(2020, 1, 1))
            .Add(x => x.View, view));
        Assert.Empty(cut.FindAll(".animate-pulse"));

        FindTodayButton(cut).Click();

        var pulsing = cut.FindAll(".animate-pulse");
        var element = Assert.Single(pulsing);
        var expectedDay = DateTime.Today.ToString("MMMM d, yyyy");
        if (view == TwCalendarView.Month)
        {
            Assert.Equal("td", element.TagName.ToLowerInvariant());
            Assert.NotNull(element.QuerySelector($"button[aria-label='{expectedDay}']"));
        }
        else
        {
            Assert.Equal(DateTime.Today.ToString("dddd, MMMM d"), element.GetAttribute("aria-label"));
        }
    }

    [Fact]
    public void PressingToday_InDayView_DoesNotPulseAnything()
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, new DateTime(2020, 1, 1))
            .Add(x => x.View, TwCalendarView.Day));

        FindTodayButton(cut).Click();

        Assert.Empty(cut.FindAll(".animate-pulse"));
    }

    [Fact]
    public void TodayHighlight_ClearsItselfAfterAFewSeconds()
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, DateTime.Today)
            .Add(x => x.View, TwCalendarView.Week));

        FindTodayButton(cut).Click();
        Assert.Single(cut.FindAll(".animate-pulse"));

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll(".animate-pulse")), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void PressingTodayAgain_RestartsTheHighlight_InsteadOfClearingItEarly()
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, DateTime.Today)
            .Add(x => x.View, TwCalendarView.Month));

        FindTodayButton(cut).Click();
        Thread.Sleep(1500);
        FindTodayButton(cut).Click();
        Thread.Sleep(1000); // 2.5s after the first press, 1s after the second

        Assert.Single(cut.FindAll(".animate-pulse"));
        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll(".animate-pulse")), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void DisposingTheCalendar_WhileHighlighted_DoesNotThrow()
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p.Add(x => x.View, TwCalendarView.Week));
        FindTodayButton(cut).Click();

        TestContext.Dispose();
        Thread.Sleep(100);
    }

    [Fact]
    public void TodayButton_SitsOnTheLeft_AndPreviousNextOnTheRight()
    {
        var cut = TestContext.Render<TwCalendar<string>>();

        var header = cut.Find("button[aria-label='Previous']").Closest("div")!.ParentElement!;
        var lead = header.Children.First();
        var controls = header.Children.Last();

        Assert.Contains(lead.QuerySelectorAll("button"), b => b.TextContent.Trim() == "Today");
        Assert.Empty(lead.QuerySelectorAll("button[aria-label='Previous'], button[aria-label='Next']"));
        Assert.NotNull(controls.QuerySelector("button[aria-label='Previous']"));
        Assert.NotNull(controls.QuerySelector("button[aria-label='Next']"));
        Assert.DoesNotContain(controls.QuerySelectorAll("button"), b => b.TextContent.Trim() == "Today");
    }

    [Fact]
    public void PreviousAndNext_AreAdjacent_AheadOfTheViewSwitcher()
    {
        var cut = TestContext.Render<TwCalendar<string>>();

        var labels = cut.Find("button[aria-label='Previous']").Closest("div")!.QuerySelectorAll("button")
            .Select(b => b.GetAttribute("aria-label") ?? b.TextContent.Trim()).ToList();

        Assert.True(labels.IndexOf("Previous") + 1 == labels.IndexOf("Next"));
        Assert.True(labels.IndexOf("Next") < labels.IndexOf("Day view"));
    }

    [Theory]
    [InlineData(TwCalendarView.Day, "Wed 18 March 2026")]
    [InlineData(TwCalendarView.Week, "16 Mar 2026 - 22 Mar 2026")]
    [InlineData(TwCalendarView.Month, "March 2026")]
    public void HeaderTitle_NamesWhatTheViewShows(TwCalendarView view, string expected)
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, new DateTime(2026, 3, 18))
            .Add(x => x.View, view));

        Assert.Equal(expected, cut.Find("span.font-semibold").TextContent.Trim());
    }

    [Fact]
    public void HeaderTitle_ZeroPadsDayNumbers()
    {
        var day = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, new DateTime(2026, 12, 3))
            .Add(x => x.View, TwCalendarView.Day));
        var week = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, new DateTime(2026, 12, 3))
            .Add(x => x.View, TwCalendarView.Week));

        Assert.Equal("Thu 03 December 2026", day.Find("span.font-semibold").TextContent.Trim());
        Assert.Equal("30 Nov 2026 - 06 Dec 2026", week.Find("span.font-semibold").TextContent.Trim());
    }

    [Theory]
    [InlineData(TwCalendarView.Day, "16 Mar 2026 - 22 Mar 2026")]
    [InlineData(TwCalendarView.Week, "March 2026")]
    [InlineData(TwCalendarView.Month, "16 Mar 2026 - 22 Mar 2026")]
    public void HeaderSubtitle_IsTheWeekRange_ExceptInWeekViewWhichShowsTheMonth(TwCalendarView view, string expected)
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, new DateTime(2026, 3, 18))
            .Add(x => x.View, view));

        Assert.Equal(expected, HeaderSubtitle(cut));
    }

    [Theory]
    [InlineData(2026, 10, 1, "September - October 2026")] // week of 28 Sep - 4 Oct
    [InlineData(2026, 12, 30, "December 2026 - January 2027")] // week of 28 Dec - 3 Jan
    [InlineData(2026, 3, 18, "March 2026")]
    public void WeekViewSubtitle_NamesEveryMonthTheWeekTouches(int year, int month, int day, string expected)
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, new DateTime(year, month, day))
            .Add(x => x.View, TwCalendarView.Week));

        Assert.Equal(expected, HeaderSubtitle(cut));
    }

    [Theory]
    [InlineData(TwCalendarView.Day)]
    [InlineData(TwCalendarView.Week)]
    [InlineData(TwCalendarView.Month)]
    public void Header_HasATitleAndASubtitle_InEveryView_SoItsHeightNeverChanges(TwCalendarView view)
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, new DateTime(2026, 3, 18))
            .Add(x => x.View, view));

        var group = cut.Find("span.font-semibold").Closest("div")!.ParentElement!;

        Assert.Equal(2, group.Children.Length); // title row + subtitle
        Assert.False(string.IsNullOrWhiteSpace(group.Children[1].TextContent));
    }

    private static string HeaderSubtitle(IRenderedComponent<TwCalendar<string>> cut) =>
        cut.Find("span.font-semibold").Closest("div")!.ParentElement!.Children[1].TextContent.Trim();

    [Theory]
    [InlineData(TwCalendarView.Day)]
    [InlineData(TwCalendarView.Week)]
    [InlineData(TwCalendarView.Month)]
    public void WeekChip_IsShownInEveryView(TwCalendarView view)
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, new DateTime(2026, 3, 18))
            .Add(x => x.View, view));

        Assert.Contains("Week 4", cut.Markup);
    }

    [Theory]
    [InlineData(TwCalendarView.Day, "day")]
    [InlineData(TwCalendarView.Week, "week")]
    [InlineData(TwCalendarView.Month, "month")]
    public void NavigationButtons_HaveViewSpecificTooltips(TwCalendarView view, string unit)
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p.Add(x => x.View, view));

        Assert.Equal($"Previous {unit}", TooltipFor(cut, "Previous"));
        Assert.Equal($"Next {unit}", TooltipFor(cut, "Next"));
    }

    [Theory]
    [InlineData("Day view")]
    [InlineData("Week view")]
    [InlineData("Month view")]
    public void ViewSwitcherButtons_HaveTooltips(string label)
    {
        var cut = TestContext.Render<TwCalendar<string>>();

        Assert.Equal(label, TooltipFor(cut, label));
    }

    [Fact]
    public void SearchButton_HasTooltip()
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.OnSearch, EventCallback.Factory.Create(this, () => { })));

        Assert.Equal("Search events", TooltipFor(cut, "Search events"));
    }

    private static string TooltipFor(IRenderedComponent<TwCalendar<string>> cut, string ariaLabel)
    {
        var button = cut.Find($"button[aria-label='{ariaLabel}']");
        return cut.Find($"#{button.GetAttribute("aria-describedby")}").TextContent;
    }

    [Theory]
    [InlineData(TwCalendarView.Day)]
    [InlineData(TwCalendarView.Week)]
    [InlineData(TwCalendarView.Month)]
    public void NoView_RendersASideCalendar(TwCalendarView view)
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p.Add(x => x.View, view));

        Assert.Empty(cut.FindComponents<TwBlazor.Components.DatePicker.TwDatePickerCalendar>());
    }

    [Theory]
    [InlineData(null, "40rem")]
    [InlineData("20rem", "20rem")]
    [InlineData("600px", "600px")]
    public void MaxHeight_AppliesToScrollableTimeGrid_InDayView(string? maxHeight, string expected)
    {
        var cut = TestContext.Render<TwCalendar<string>>(p =>
        {
            p.Add(x => x.View, TwCalendarView.Day);
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
        var cut = TestContext.Render<TwCalendar<string>>(p =>
        {
            p.Add(x => x.View, TwCalendarView.Week);
            if (maxHeight is not null)
            {
                p.Add(x => x.MaxHeight, maxHeight);
            }
        });

        var style = cut.Find("div.overflow-y-auto").GetAttribute("style");
        Assert.Contains($"max-height:{expected}", style);
    }

    [Theory]
    [InlineData(TwCalendarView.Day)]
    [InlineData(TwCalendarView.Week)]
    public void ScrollContainer_UsesItemsStart_NotDefaultStretch(TwCalendarView view)
    {
        // Regression test: flexbox's default align-items:stretch resizes the gutter/day-column flex
        // children to the (max-height-clamped) scroll container's cross size instead of their full
        // content height - corrupting the scrollable area and pushing event chips (positioned via an
        // absolute "top" measured from the day column's own top edge) down to wherever that shrunk
        // box ends up, which visually reads as events clustering near the bottom of the day regardless
        // of their real time. items-start keeps the children at their natural (144rem) height instead.
        var cut = TestContext.Render<TwCalendar<string>>(p => p.Add(x => x.View, view));

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

        var cut = TestContext.Render<TwCalendarDayColumn<string>>(p => p
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

        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, day)
            .Add(x => x.View, TwCalendarView.Day)
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

        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, day)
            .Add(x => x.View, TwCalendarView.Day)
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

        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, day)
            .Add(x => x.View, TwCalendarView.Day)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [evt])
            .Add(x => x.SchedulesChanged, EventCallback.Factory.Create<List<Schedule<string>>>(this, list => updatedList = list)));

        cut.Find("button[aria-label^='Design review']").DragStart(new DragEventArgs());
        cut.Find("button[aria-label='11:00 AM']").Drop(new DragEventArgs());

        cut.WaitForAssertion(() => Assert.NotNull(updatedList)); // the drop handler finishes asynchronously
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

        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, wednesday)
            .Add(x => x.View, TwCalendarView.Week)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [evt])
            .Add(x => x.SchedulesChanged, EventCallback.Factory.Create<List<Schedule<string>>>(this, list => updatedList = list)));

        cut.Find("button[aria-label^='Design review']").DragStart(new DragEventArgs());

        // Columns render Monday(0)..Sunday(6); Wednesday is index 2, Thursday index 3.
        var thursdayColumn = cut.FindComponents<TwCalendarDayColumn<string>>()[3];
        thursdayColumn.Find("button[aria-label='9:00 AM']").Drop(new DragEventArgs());

        cut.WaitForAssertion(() => Assert.NotNull(updatedList)); // the drop handler finishes asynchronously
        var moved = Assert.Single(Assert.IsType<List<Schedule<string>>>(updatedList));
        Assert.Equal(new DateTimeOffset(wednesday.AddDays(1).AddHours(9)), moved.DateTimeStart);
        Assert.Equal(new DateTimeOffset(wednesday.AddDays(1).AddHours(10)), moved.DateTimeEnd);
    }

    [Fact]
    public void DragEvent_InMonthView_ChangesDateOnly_PreservingTimeOfDayAndDuration()
    {
        var evt = Event("Design review", new DateTime(2026, 3, 10, 9, 30, 0), new DateTime(2026, 3, 10, 10, 15, 0));
        List<Schedule<string>>? updatedList = null;

        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, new DateTime(2026, 3, 15))
            .Add(x => x.View, TwCalendarView.Month)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [evt])
            .Add(x => x.SchedulesChanged, EventCallback.Factory.Create<List<Schedule<string>>>(this, list => updatedList = list)));

        cut.Find("button[aria-label^='Design review']").DragStart(new DragEventArgs());

        var dayButton = cut.FindAll("button").First(b => b.TextContent.Trim() == "20");
        var cell = dayButton.Closest("td");
        Assert.NotNull(cell);
        cell!.Drop(new DragEventArgs());

        cut.WaitForAssertion(() => Assert.NotNull(updatedList)); // the drop handler finishes asynchronously
        var moved = Assert.Single(Assert.IsType<List<Schedule<string>>>(updatedList));
        Assert.Equal(new DateTimeOffset(2026, 3, 20, 9, 30, 0, TimeSpan.Zero), moved.DateTimeStart);
        Assert.Equal(new DateTimeOffset(2026, 3, 20, 10, 15, 0, TimeSpan.Zero), moved.DateTimeEnd);
    }

    /// <summary>
    /// Starts dragging the named event and waits for the schedule to enter drag mode, which it does a
    /// moment after dragstart (see TwCalendar's drag activation delay).
    /// </summary>
    private static void StartDrag(IRenderedComponent<TwCalendar<string>> cut, string eventName)
    {
        cut.Find($"button[aria-label^='{eventName}']").DragStart(new DragEventArgs());
        cut.WaitForAssertion(() => Assert.Contains("pointer-events-none", cut.Find($"button[aria-label^='{eventName}']").GetAttribute("class")));
    }

    [Fact]
    public void DragEvent_InDayView_ShowsPlaceholderAtHoveredSlot_SizedToEventDuration()
    {
        var day = new DateTime(2026, 3, 18);
        var dragged = Event("Design review", day.AddHours(9), day.AddHours(10.5));
        var other = Event("Existing", day.AddHours(14), day.AddHours(15));

        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, day)
            .Add(x => x.View, TwCalendarView.Day)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [dragged, other]));

        Assert.Empty(cut.FindAll("[aria-hidden='true'][style*='width:100%']"));

        StartDrag(cut, "Design review");
        cut.Find("button[aria-label='2:00 PM']").DragEnter(new DragEventArgs());

        var placeholder = cut.WaitForElement("div[aria-hidden='true'][style*='width:100%']");
        var style = placeholder.GetAttribute("style")!;
        Assert.Contains("top:84rem", style); // 14:00 = slot 28 * 3rem
        Assert.Contains("height:9rem", style); // 90 minutes = 3 slots * 3rem
        Assert.Contains("pointer-events-none", placeholder.GetAttribute("class"));
    }

    [Fact]
    public void DragEvent_InWeekView_ShowsPlaceholderOnlyInHoveredDayColumn()
    {
        var wednesday = new DateTime(2026, 3, 18);
        var evt = Event("Design review", wednesday.AddHours(9), wednesday.AddHours(10));

        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, wednesday)
            .Add(x => x.View, TwCalendarView.Week)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [evt]));

        StartDrag(cut, "Design review");
        var columns = cut.FindComponents<TwCalendarDayColumn<string>>();
        columns[3].Find("button[aria-label='9:00 AM']").DragEnter(new DragEventArgs());

        cut.WaitForAssertion(() =>
        {
            columns = cut.FindComponents<TwCalendarDayColumn<string>>();
            for (var i = 0; i < columns.Count; i++)
            {
                var hasPlaceholder = columns[i].FindAll("div[aria-hidden='true']").Count > 0;
                Assert.Equal(i == 3, hasPlaceholder);
            }
        });
    }

    [Fact]
    public void DragEvent_InDayView_ChipsIgnorePointerEvents_SoOccupiedSlotsCanReceiveTheDrop()
    {
        var day = new DateTime(2026, 3, 18);
        var dragged = Event("Design review", day.AddHours(13), day.AddHours(14));
        var other = Event("Existing", day.AddHours(14), day.AddHours(15));
        List<Schedule<string>>? updatedList = null;

        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, day)
            .Add(x => x.View, TwCalendarView.Day)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [dragged, other])
            .Add(x => x.SchedulesChanged, EventCallback.Factory.Create<List<Schedule<string>>>(this, list => updatedList = list)));

        Assert.DoesNotContain("pointer-events-none", cut.Find("button[aria-label^='Existing']").GetAttribute("class"));

        StartDrag(cut, "Design review");

        Assert.Contains("pointer-events-none", cut.Find("button[aria-label^='Existing']").GetAttribute("class"));

        cut.Find("button[aria-label='2:00 PM']").Drop(new DragEventArgs());

        cut.WaitForAssertion(() =>
        {
            var moved = Assert.Single(Assert.IsType<List<Schedule<string>>>(updatedList), e => e.Name == "Design review");
            Assert.Equal(new DateTimeOffset(day.AddHours(14)), moved.DateTimeStart);
            Assert.DoesNotContain("pointer-events-none", cut.Find("button[aria-label^='Existing']").GetAttribute("class"));
        });
    }

    [Fact]
    public void DragEvent_DragEnd_ClearsPlaceholderAndRestoresChips()
    {
        var day = new DateTime(2026, 3, 18);
        var evt = Event("Design review", day.AddHours(9), day.AddHours(10));

        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, day)
            .Add(x => x.View, TwCalendarView.Day)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [evt]));

        StartDrag(cut, "Design review");
        cut.Find("button[aria-label='11:00 AM']").DragEnter(new DragEventArgs());
        cut.WaitForElement("div[aria-hidden='true'][style*='width:100%']");

        cut.Find("button[aria-label^='Design review']").DragEnd(new DragEventArgs());

        cut.WaitForAssertion(() =>
        {
            Assert.Empty(cut.FindAll("div[aria-hidden='true'][style*='width:100%']"));
            Assert.DoesNotContain("pointer-events-none", cut.Find("button[aria-label^='Design review']").GetAttribute("class"));
        });
    }

    [Fact]
    public void DragEvent_InMonthView_ShowsPlaceholderInHoveredDay()
    {
        var evt = Event("Design review", new DateTime(2026, 3, 10, 9, 30, 0), new DateTime(2026, 3, 10, 10, 15, 0));

        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, new DateTime(2026, 3, 15))
            .Add(x => x.View, TwCalendarView.Month)
            .Add(x => x.Editable, true)
            .Add(x => x.Schedules, [evt]));

        cut.Find("button[aria-label^='Design review']").DragStart(new DragEventArgs());

        // The Month view's chips keep pointer events (drops bubble up to the cell), so there's no chip
        // class to wait on; re-entering the cell until the drag goes live is idempotent.
        cut.WaitForAssertion(() =>
        {
            cut.FindAll("button").First(b => b.TextContent.Trim() == "20").Closest("td")!.DragEnter(new DragEventArgs());

            var hovered = cut.FindAll("button").First(b => b.TextContent.Trim() == "20").Closest("td")!;
            var placeholder = Assert.Single(hovered.QuerySelectorAll("div[aria-hidden='true']"));
            Assert.Contains("Design review", placeholder.TextContent);
            Assert.Single(cut.FindAll("td div[aria-hidden='true']"));
        });
    }

    [Fact]
    public void DragEvent_ReadOnlyEvent_IsNotDraggable()
    {
        var day = new DateTime(2026, 3, 18);
        var evt = Event("Locked", day.AddHours(9), day.AddHours(10), readOnly: true);

        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, day)
            .Add(x => x.View, TwCalendarView.Day)
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

        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, day)
            .Add(x => x.View, TwCalendarView.Day)
            .Add(x => x.Editable, false)
            .Add(x => x.Schedules, [evt]));

        var chip = cut.Find("button[aria-label^='Design review']");

        Assert.Equal("false", chip.GetAttribute("draggable"));
    }

    [Fact]
    public void ViewSwitch_InvokesViewChanged_AndSwapsRenderedViewComponent()
    {
        TwCalendarView? result = null;

        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.View, TwCalendarView.Week)
            .Add(x => x.ViewChanged, EventCallback.Factory.Create<TwCalendarView>(this, v => result = v)));

        Assert.NotNull(cut.FindComponent<TwCalendarWeekView<string>>());

        cut.Find("[aria-label='Month view']").Click();

        Assert.Equal(TwCalendarView.Month, result);
    }

    [Fact]
    public void ViewSwitcher_MarksActiveViewButton_WithAriaPressed()
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.View, TwCalendarView.Week));

        Assert.Equal("false", cut.Find("[aria-label='Day view']").GetAttribute("aria-pressed"));
        Assert.Equal("true", cut.Find("[aria-label='Week view']").GetAttribute("aria-pressed"));
        Assert.Equal("false", cut.Find("[aria-label='Month view']").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void RendersDayView_WhenViewIsDay()
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.View, TwCalendarView.Day));

        Assert.NotNull(cut.FindComponent<TwCalendarDayView<string>>());
    }

    [Fact]
    public void RendersMonthView_WhenViewIsMonth()
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.View, TwCalendarView.Month));

        Assert.NotNull(cut.FindComponent<TwCalendarMonthView<string>>());
    }

    [Fact]
    public void AddEventButton_HiddenByDefault_ShownWhenEditable()
    {
        var cut = TestContext.Render<TwCalendar<string>>();

        Assert.DoesNotContain("Add event", cut.Markup);

        cut.Render(p => p.Add(x => x.Editable, true));

        Assert.Contains("Add event", cut.Markup);
    }

    [Fact]
    public void SearchButton_HiddenByDefault_ShownWhenOnSearchHasDelegate()
    {
        var cut = TestContext.Render<TwCalendar<string>>();

        Assert.Empty(cut.FindAll("[aria-label='Search events']"));

        cut.Render(p => p
            .Add(x => x.OnSearch, EventCallback.Factory.Create(this, () => { })));

        Assert.NotEmpty(cut.FindAll("[aria-label='Search events']"));
    }

    #region Now indicator

    [Theory]
    [InlineData(TwCalendarView.Day)]
    [InlineData(TwCalendarView.Week)]
    public void NowIndicator_RendersLineAndGutterLabel_RegardlessOfViewedDate(TwCalendarView view)
    {
        // The indicator is a "what time is it right now" reference bar, not scoped to today - it must
        // still render even when browsing a date far from today.
        var before = DateTime.Now;
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, DateTime.Today.AddDays(30))
            .Add(x => x.View, view));
        var after = DateTime.Now;

        var line = cut.Find("div.border-t-2");
        var label = cut.Find("div.right-2");

        var minTop = TwCalendarTimeGrid.GetOffsetRem(before.TimeOfDay.TotalMinutes);
        var maxTop = TwCalendarTimeGrid.GetOffsetRem(after.TimeOfDay.TotalMinutes);
        var lineStyle = line.GetAttribute("style")!;
        var actualTop = decimal.Parse(lineStyle.Split("top:")[1].Split("rem")[0], System.Globalization.CultureInfo.InvariantCulture);

        Assert.InRange(actualTop, minTop, maxTop);
        Assert.NotEmpty(label.TextContent);
    }

    [Fact]
    public void NowIndicator_Dot_AlwaysPresent_AtLeftEdge_InDayView()
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, DateTime.Today.AddDays(30))
            .Add(x => x.View, TwCalendarView.Day));

        var dot = cut.Find("div.border-t-2").QuerySelector("span");

        Assert.NotNull(dot);
        Assert.Contains("left:0", dot!.GetAttribute("style"));
    }

    [Fact]
    public void NowIndicator_Dot_Present_InWeekView_WhenTodayIsInTheDisplayedWeek()
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, DateTime.Today)
            .Add(x => x.View, TwCalendarView.Week));

        var dot = cut.Find("div.border-t-2").QuerySelector("span");
        var weekStart = TwBlazor.Utilities.DateHelpers.GetStartOfWeek(DateTime.Today);
        var expectedLeft = 100m / 7 * (DateTime.Today - weekStart).Days;

        Assert.NotNull(dot);
        Assert.Contains($"left:{expectedLeft.ToString(System.Globalization.CultureInfo.InvariantCulture)}%", dot!.GetAttribute("style"));
    }

    [Fact]
    public void NowIndicator_Dot_Absent_InWeekView_WhenTodayIsNotInTheDisplayedWeek()
    {
        var cut = TestContext.Render<TwCalendar<string>>(p => p
            .Add(x => x.SelectedDate, DateTime.Today.AddDays(30))
            .Add(x => x.View, TwCalendarView.Week));

        var dot = cut.Find("div.border-t-2").QuerySelector("span");

        Assert.Null(dot);
    }

    #endregion
}
