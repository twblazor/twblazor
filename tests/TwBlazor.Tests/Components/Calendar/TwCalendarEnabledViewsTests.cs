// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Calendar;

public class TwCalendarEnabledViewsTests : TwBlazorTestBase
{
    private static readonly DateTime wednesday = new(2026, 3, 18);

    private IRenderedComponent<TwCalendar<string>> Render(TwCalendarView view, IReadOnlyCollection<TwCalendarView>? enabled, Action<ComponentParameterCollectionBuilder<TwCalendar<string>>>? more = null) =>
        TestContext.Render<TwCalendar<string>>(p =>
        {
            p.Add(x => x.SelectedDate, wednesday).Add(x => x.View, view);
            if (enabled is not null)
            {
                p.Add(x => x.EnabledViews, enabled);
            }

            more?.Invoke(p);
        });

    private static IReadOnlyList<string> ViewButtons(IRenderedComponent<TwCalendar<string>> cut) =>
        [.. cut.FindAll("[role='radiogroup'] button").Select(b => b.GetAttribute("aria-label")!)];

    [Fact]
    public void ByDefault_AllThreeViews_AreAvailable_InSwitcherOrder()
    {
        var cut = Render(TwCalendarView.Week, null);

        Assert.Equal(["Day view", "Week view", "Month view"], ViewButtons(cut));
    }

    [Fact]
    public void EmptyEnabledViews_MeansAllViews()
    {
        var cut = Render(TwCalendarView.Week, []);

        Assert.Equal(["Day view", "Week view", "Month view"], ViewButtons(cut));
    }

    [Theory]
    [InlineData(new[] { TwCalendarView.Week, TwCalendarView.Month }, new[] { "Week view", "Month view" })]
    [InlineData(new[] { TwCalendarView.Day, TwCalendarView.Month }, new[] { "Day view", "Month view" })]
    [InlineData(new[] { TwCalendarView.Day, TwCalendarView.Week }, new[] { "Day view", "Week view" })]
    public void OnlyEnabledViews_GetASwitcherButton(TwCalendarView[] enabled, string[] expected)
    {
        var cut = Render(enabled[0], enabled);

        Assert.Equal(expected, ViewButtons(cut));
    }

    [Fact]
    public void SwitcherOrder_IgnoresTheOrderOfTheArray()
    {
        var cut = Render(TwCalendarView.Day, [TwCalendarView.Month, TwCalendarView.Day]);

        Assert.Equal(["Day view", "Month view"], ViewButtons(cut));
    }

    [Fact]
    public void DuplicateEntries_AreHarmless()
    {
        var cut = Render(TwCalendarView.Day, [TwCalendarView.Day, TwCalendarView.Day, TwCalendarView.Week]);

        Assert.Equal(["Day view", "Week view"], ViewButtons(cut));
    }

    [Theory]
    [InlineData(TwCalendarView.Day)]
    [InlineData(TwCalendarView.Week)]
    [InlineData(TwCalendarView.Month)]
    public void ASingleEnabledView_HidesTheSwitcherEntirely(TwCalendarView only)
    {
        var cut = Render(only, [only]);

        Assert.Empty(cut.FindAll("[role='radiogroup']"));
        Assert.Empty(cut.FindAll("button[aria-label$=' view']"));
    }

    [Fact]
    public void ASingleEnabledView_StillShowsThatView_AndNavigation()
    {
        var cut = Render(TwCalendarView.Month, [TwCalendarView.Month]);

        Assert.NotNull(cut.FindComponent<TwCalendarMonthView<string>>());
        Assert.NotNull(cut.Find("button[aria-label='Previous']"));
        Assert.NotNull(cut.Find("button[aria-label='Next']"));
        Assert.Contains(cut.FindAll("button"), b => b.TextContent.Trim() == "Today");
    }

    [Theory]
    [InlineData(TwCalendarView.Day, new[] { TwCalendarView.Week, TwCalendarView.Month }, TwCalendarView.Week)]
    [InlineData(TwCalendarView.Month, new[] { TwCalendarView.Day, TwCalendarView.Week }, TwCalendarView.Day)]
    [InlineData(TwCalendarView.Week, new[] { TwCalendarView.Month, TwCalendarView.Day }, TwCalendarView.Day)] // first in switcher order, not array order
    public void WhenViewIsNotEnabled_TheFirstEnabledViewIsShownInstead(TwCalendarView requested, TwCalendarView[] enabled, TwCalendarView expected)
    {
        var cut = Render(requested, enabled);

        var pressed = cut.FindAll("[role='radiogroup'] button").Single(b => b.GetAttribute("aria-pressed") == "true");
        Assert.Equal($"{expected} view", pressed.GetAttribute("aria-label"));
        Assert.Equal(expected == TwCalendarView.Day, cut.FindComponents<TwCalendarDayView<string>>().Count == 1);
        Assert.Equal(expected == TwCalendarView.Week, cut.FindComponents<TwCalendarWeekView<string>>().Count == 1);
        Assert.Equal(expected == TwCalendarView.Month, cut.FindComponents<TwCalendarMonthView<string>>().Count == 1);
    }

    [Fact]
    public void FallbackView_DrivesTheHeaderTitle_AndNavigationStep()
    {
        DateTime? result = null;
        var cut = Render(TwCalendarView.Day, [TwCalendarView.Month], p => p
            .Add(x => x.SelectedDateChanged, EventCallback.Factory.Create<DateTime>(this, d => result = d)));

        Assert.Equal("March 2026", cut.Find("span.font-semibold").TextContent.Trim());

        cut.Find("button[aria-label='Next']").Click();

        Assert.Equal(wednesday.AddMonths(1), result);
    }

    [Fact]
    public void SwitchingTo_AnEnabledView_Works_AndReportsIt()
    {
        TwCalendarView? changed = null;
        var cut = Render(TwCalendarView.Week, [TwCalendarView.Week, TwCalendarView.Month], p => p
            .Add(x => x.ViewChanged, EventCallback.Factory.Create<TwCalendarView>(this, v => changed = v)));

        cut.Find("button[aria-label='Month view']").Click();

        Assert.Equal(TwCalendarView.Month, changed);
    }

    [Fact]
    public void WeekDayHeader_JustPicksTheDate_WhenTheDayViewIsDisabled()
    {
        DateTime? date = null;
        TwCalendarView? view = null;
        var cut = Render(TwCalendarView.Week, [TwCalendarView.Week, TwCalendarView.Month], p => p
            .Add(x => x.SelectedDateChanged, EventCallback.Factory.Create<DateTime>(this, d => date = d))
            .Add(x => x.ViewChanged, EventCallback.Factory.Create<TwCalendarView>(this, v => view = v)));

        cut.FindAll("button").First(b => b.TextContent.Trim() == "Fri 20").Click();

        Assert.Equal(new DateTime(2026, 3, 20), date);
        Assert.Null(view);
        Assert.NotNull(cut.FindComponent<TwCalendarWeekView<string>>());
    }

    [Fact]
    public void WeekDayHeader_StillDrillsIntoTheDay_WhenTheDayViewIsEnabled()
    {
        TwCalendarView? view = null;
        var cut = Render(TwCalendarView.Week, null, p => p
            .Add(x => x.ViewChanged, EventCallback.Factory.Create<TwCalendarView>(this, v => view = v)));

        cut.FindAll("button").First(b => b.TextContent.Trim() == "Fri 20").Click();

        Assert.Equal(TwCalendarView.Day, view);
    }

    [Fact]
    public void MonthDayNumber_JustPicksTheDate_WhenTheDayViewIsDisabled()
    {
        DateTime? date = null;
        TwCalendarView? view = null;
        var cut = Render(TwCalendarView.Month, [TwCalendarView.Week, TwCalendarView.Month], p => p
            .Add(x => x.SelectedDateChanged, EventCallback.Factory.Create<DateTime>(this, d => date = d))
            .Add(x => x.ViewChanged, EventCallback.Factory.Create<TwCalendarView>(this, v => view = v)));

        cut.Find("button[aria-label='March 25, 2026']").Click();

        Assert.Equal(new DateTime(2026, 3, 25), date);
        Assert.Null(view);
        Assert.NotNull(cut.FindComponent<TwCalendarMonthView<string>>());
    }

    [Fact]
    public void EnabledViews_CanBeAList()
    {
        List<TwCalendarView> enabled = [TwCalendarView.Week, TwCalendarView.Month];
        var cut = Render(TwCalendarView.Week, enabled);

        Assert.Equal(["Week view", "Month view"], ViewButtons(cut));
    }

    [Fact]
    public void ChangingEnabledViews_UpdatesTheSwitcher()
    {
        var cut = Render(TwCalendarView.Week, null);
        Assert.Equal(3, ViewButtons(cut).Count);

        cut.Render(p => p.Add(x => x.EnabledViews, [TwCalendarView.Week]));

        Assert.Empty(cut.FindAll("[role='radiogroup']"));
    }

    [Fact]
    public void TodayButton_StillWorks_WithRestrictedViews()
    {
        DateTime? result = null;
        var cut = Render(TwCalendarView.Month, [TwCalendarView.Month], p => p
            .Add(x => x.SelectedDateChanged, EventCallback.Factory.Create<DateTime>(this, d => result = d)));

        cut.FindAll("button").First(b => b.TextContent.Trim() == "Today").Click();

        Assert.Equal(DateTime.Today, result);
    }
}
