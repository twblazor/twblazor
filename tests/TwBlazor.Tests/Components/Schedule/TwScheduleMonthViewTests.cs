// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Schedule;

public class TwScheduleMonthViewTests : TwBlazorTestBase
{
    private static Schedule<string> Event(string name, DateTime day, int startHour, int endHour) => new()
    {
        Name = name,
        DateTimeStart = day.Date.AddHours(startHour),
        DateTimeEnd = day.Date.AddHours(endHour)
    };

    [Fact]
    public void RendersSixWeekRows_ForAMonthThatSpansSixWeeks()
    {
        // March 2026: 1st is a Sunday and the 31st is a Tuesday, so the Monday-start grid needs 6 rows.
        var cut = TestContext.Render<TwScheduleMonthView<string>>(p => p
            .Add(x => x.Date, new DateTime(2026, 3, 15)));

        var rows = cut.FindAll("tbody tr[role='row']");

        Assert.Equal(6, rows.Count);
    }

    [Fact]
    public void RendersFiveWeekRows_ForAMonthThatSpansFiveWeeks()
    {
        // April 2026: 1st is a Wednesday and the 30th is a Thursday, so the Monday-start grid fits 5 rows.
        var cut = TestContext.Render<TwScheduleMonthView<string>>(p => p
            .Add(x => x.Date, new DateTime(2026, 4, 15)));

        var rows = cut.FindAll("tbody tr[role='row']");

        Assert.Equal(5, rows.Count);
    }

    [Fact]
    public void RendersSevenEqualWidthColumns_ViaColgroup()
    {
        // Regression test: without an explicit <colgroup>, the browser's table auto-layout sizes
        // columns by their widest cell's content across ALL rows (not just the header), so a column
        // that happens to hold more/longer event chips in a later week renders wider than the rest -
        // an explicit equal-width <col> per column is what keeps every column the same width
        // regardless of how much content lands in it.
        var cut = TestContext.Render<TwScheduleMonthView<string>>(p => p
            .Add(x => x.Date, new DateTime(2026, 3, 15)));

        var cols = cut.FindAll("colgroup col");

        Assert.Equal(7, cols.Count);
        var widths = cols.Select(c => c.GetAttribute("style")).ToList();
        Assert.All(widths, w => Assert.Equal(widths[0], w));
        Assert.All(widths, w => Assert.Contains("width:14.2857%", w));
    }

    [Fact]
    public void EveryRow_HasSevenDayCells()
    {
        var cut = TestContext.Render<TwScheduleMonthView<string>>(p => p
            .Add(x => x.Date, new DateTime(2026, 3, 15)));

        foreach (var row in cut.FindAll("tbody tr[role='row']"))
        {
            Assert.Equal(7, row.QuerySelectorAll("td[role='gridcell']").Length);
        }
    }

    [Fact]
    public void EventChip_UsesCustomColor_WhenSet()
    {
        var day = new DateTime(2026, 3, 10);
        var evt = Event("Design review", day, 10, 11);
        evt.Color = "#2563eb";

        var cut = TestContext.Render<TwScheduleMonthView<string>>(p => p
            .Add(x => x.Date, new DateTime(2026, 3, 15))
            .Add(x => x.Schedules, [evt]));

        var row = cut.Find("button[aria-label^='Design review']");
        var style = row.GetAttribute("style");

        Assert.Contains("background-color:#2563eb26;", style); // tinted background
        Assert.Contains("border-left-color:#2563eb;", style);
        Assert.Contains("color:#2563eb;", style);
    }

    [Fact]
    public void RendersEventChip_ForADayWithAnEvent()
    {
        var day = new DateTime(2026, 3, 10);
        var evt = Event("Design review", day, 10, 11);

        var cut = TestContext.Render<TwScheduleMonthView<string>>(p => p
            .Add(x => x.Date, new DateTime(2026, 3, 15))
            .Add(x => x.Schedules, [evt]));

        Assert.Contains("Design review", cut.Markup);
    }

    [Fact]
    public void ClickingDayNumber_InvokesOnDayClickWithThatDate()
    {
        DateTime? clicked = null;
        var cut = TestContext.Render<TwScheduleMonthView<string>>(p => p
            .Add(x => x.Date, new DateTime(2026, 3, 15))
            .Add(x => x.OnDayClick, EventCallback.Factory.Create<DateTime>(this, d => clicked = d)));

        var button = cut.FindAll("button").First(b => b.TextContent.Trim() == "10");
        button.Click();

        Assert.NotNull(clicked);
        Assert.Equal(10, clicked!.Value.Day);
        Assert.Equal(3, clicked.Value.Month);
    }

    [Fact]
    public void ClickingEventChip_InvokesOnEventClick_NotOnDayClick()
    {
        DateTime? dayClicked = null;
        Schedule<string>? eventClicked = null;
        var day = new DateTime(2026, 3, 10);
        var evt = Event("Design review", day, 10, 11);

        var cut = TestContext.Render<TwScheduleMonthView<string>>(p => p
            .Add(x => x.Date, new DateTime(2026, 3, 15))
            .Add(x => x.Schedules, [evt])
            .Add(x => x.OnDayClick, EventCallback.Factory.Create<DateTime>(this, d => dayClicked = d))
            .Add(x => x.OnEventClick, EventCallback.Factory.Create<Schedule<string>>(this, e => eventClicked = e)));

        cut.Find("button[aria-label^='Design review']").Click();

        Assert.Same(evt, eventClicked);
        Assert.Null(dayClicked);
    }

    [Fact]
    public void ShowsOverflowLabel_WhenMoreThanThreeEventsOnOneDay()
    {
        var day = new DateTime(2026, 3, 10);
        var events = new List<Schedule<string>>
        {
            Event("Event 1", day, 9, 10),
            Event("Event 2", day, 10, 11),
            Event("Event 3", day, 11, 12),
            Event("Event 4", day, 12, 13)
        };

        var cut = TestContext.Render<TwScheduleMonthView<string>>(p => p
            .Add(x => x.Date, new DateTime(2026, 3, 15))
            .Add(x => x.Schedules, events));

        Assert.Contains("+1 more", cut.Markup);
    }

    [Fact]
    public void DoesNotShowOverflowLabel_WhenThreeOrFewerEventsOnOneDay()
    {
        var day = new DateTime(2026, 3, 10);
        var events = new List<Schedule<string>>
        {
            Event("Event 1", day, 9, 10),
            Event("Event 2", day, 10, 11),
            Event("Event 3", day, 11, 12)
        };

        var cut = TestContext.Render<TwScheduleMonthView<string>>(p => p
            .Add(x => x.Date, new DateTime(2026, 3, 15))
            .Add(x => x.Schedules, events));

        Assert.DoesNotContain("more", cut.Markup);
    }
}
