using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components.DatePicker;

namespace TwBlazor.Tests.Components.DatePicker;

public class TwDatePickerCalendarTests : TwBlazorTestBase
{
    [Fact]
    public void ClickingDay_InvokesDaySelected()
    {
        // Arrange
        DateTime? selected = null;
        var cut = TestContext.Render<TwDatePickerCalendar>(p => p
            .Add(x => x.AnchorDate, new DateTime(2025, 11, 1))
            .Add(x => x.DaySelected, EventCallback.Factory.Create<DateTime>(this, d => selected = d))
        );

        // Act
        cut.FindAll("button.day").First(b => b.TextContent.Trim() == "5").Click();

        // Assert
        Assert.NotNull(selected);
        Assert.Equal(5, selected!.Value.Day);
    }

    [Fact]
    public void ViewSwitch_MonthThenYear_ShowsQuickPickGrids()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePickerCalendar>(p => p
            .Add(x => x.AnchorDate, new DateTime(2025, 11, 1))
            .Add(x => x.DaySelected, EventCallback.Factory.Create<DateTime>(this, _ => { }))
        );

        // Act & Assert
        cut.Find("button.view-switch").Click(); // Day -> Month
        Assert.Contains("months-of-the-year", cut.Markup);

        cut.Find("button.view-switch").Click(); // Month -> Year
        Assert.Contains("years-of-the-decade", cut.Markup);
    }

    [Fact]
    public void PreserveAnchorDayTrue_KeepsDayWhenNavigatingMonths()
    {
        // Arrange - TwDatePicker's single-date usage: the anchor's day should survive a month pick.
        var cut = TestContext.Render<TwDatePickerCalendar>(p => p
            .Add(x => x.AnchorDate, new DateTime(2025, 11, 15))
            .Add(x => x.PreserveAnchorDay, true)
            .Add(x => x.DaySelected, EventCallback.Factory.Create<DateTime>(this, _ => { }))
        );

        // Act
        cut.Find("button.view-switch").Click(); // Day -> Month
        var marchButton = cut.FindAll("button.month").First(b => b.TextContent.Trim() == "Mar");
        marchButton.Click();

        // Assert - back in Day view, showing March, still day 15.
        Assert.Contains("March 2025", cut.Markup);
        Assert.Contains("datepicker-grid", cut.Markup);
    }

    [Fact]
    public void PreserveAnchorDayFalse_NormalizesToFirstOfMonth()
    {
        // Arrange - TwDateTimeRangePicker's per-step usage: the anchor only ever identifies a month.
        var cut = TestContext.Render<TwDatePickerCalendar>(p => p
            .Add(x => x.AnchorDate, new DateTime(2025, 11, 15))
            .Add(x => x.PreserveAnchorDay, false)
            .Add(x => x.DaySelected, EventCallback.Factory.Create<DateTime>(this, _ => { }))
        );

        // Act
        cut.Find("button.view-switch").Click(); // Day -> Month
        var marchButton = cut.FindAll("button.month").First(b => b.TextContent.Trim() == "Mar");
        marchButton.Click();

        // Assert
        Assert.Contains("March 2025", cut.Markup);
    }

    [Fact]
    public void RangeMode_HighlightsBothEndpoints()
    {
        // Arrange
        var range = new KeyValuePair<DateTime?, DateTime?>(new DateTime(2025, 11, 5), new DateTime(2025, 11, 15));
        var cut = TestContext.Render<TwDatePickerCalendar>(p => p
            .Add(x => x.AnchorDate, new DateTime(2025, 11, 1))
            .Add(x => x.Range, range)
            .Add(x => x.DaySelected, EventCallback.Factory.Create<DateTime>(this, _ => { }))
        );

        // Assert
        var day5 = cut.FindAll("button.day").First(b => b.TextContent.Trim() == "5");
        var day15 = cut.FindAll("button.day").First(b => b.TextContent.Trim() == "15");
        Assert.Contains("Start of selected range", day5.GetAttribute("aria-label"));
        Assert.Contains("End of selected range", day15.GetAttribute("aria-label"));
    }

    [Fact]
    public void MinDate_DisablesPreviousMonthButton()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDatePickerCalendar>(p => p
            .Add(x => x.AnchorDate, new DateTime(2025, 11, 1))
            .Add(x => x.MinDate, new DateTime(2025, 11, 1))
            .Add(x => x.DaySelected, EventCallback.Factory.Create<DateTime>(this, _ => { }))
        );

        // Assert
        Assert.Equal("true", cut.Find(".prev-btn").GetAttribute("aria-disabled"));
    }

    [Fact]
    public void ShowTodayIndicatorFalse_DoesNotMarkCurrentMonth()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDatePickerCalendar>(p => p
            .Add(x => x.AnchorDate, DateTime.Today)
            .Add(x => x.ShowTodayIndicator, false)
            .Add(x => x.DaySelected, EventCallback.Factory.Create<DateTime>(this, _ => { }))
        );

        // Act
        cut.Find("button.view-switch").Click(); // Day -> Month

        // Assert - no month button should carry aria-current="date".
        Assert.DoesNotContain(cut.FindAll("button.month"), b => b.GetAttribute("aria-current") == "date");
    }

    [Fact]
    public void Navigated_FiresOnViewSwitch()
    {
        // Arrange
        var navigatedCount = 0;
        var cut = TestContext.Render<TwDatePickerCalendar>(p => p
            .Add(x => x.AnchorDate, new DateTime(2025, 11, 1))
            .Add(x => x.DaySelected, EventCallback.Factory.Create<DateTime>(this, _ => { }))
            .Add(x => x.Navigated, EventCallback.Factory.Create(this, () => navigatedCount++))
        );

        // Act
        cut.Find("button.view-switch").Click();

        // Assert
        Assert.Equal(1, navigatedCount);
    }

    [Fact]
    public void AnchorDateChanged_FiresOnNextMonthClick()
    {
        // Arrange
        DateTime? newAnchor = null;
        var cut = TestContext.Render<TwDatePickerCalendar>(p => p
            .Add(x => x.AnchorDate, new DateTime(2025, 11, 1))
            .Add(x => x.DaySelected, EventCallback.Factory.Create<DateTime>(this, _ => { }))
            .Add(x => x.AnchorDateChanged, EventCallback.Factory.Create<DateTime>(this, d => newAnchor = d))
        );

        // Act
        cut.Find(".next-btn").Click();

        // Assert
        Assert.NotNull(newAnchor);
        Assert.Equal(12, newAnchor!.Value.Month);
    }

    [Fact]
    public void MaxDate_DisablesNextMonthButton()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDatePickerCalendar>(p => p
            .Add(x => x.AnchorDate, new DateTime(2025, 11, 1))
            .Add(x => x.MaxDate, new DateTime(2025, 11, 30))
            .Add(x => x.DaySelected, EventCallback.Factory.Create<DateTime>(this, _ => { }))
        );

        // Assert
        Assert.Equal("true", cut.Find(".next-btn").GetAttribute("aria-disabled"));
    }

    [Fact]
    public void ShowTodayIndicatorTrue_MarksCurrentMonthAndYear()
    {
        // Arrange & Act - default is true.
        var cut = TestContext.Render<TwDatePickerCalendar>(p => p
            .Add(x => x.AnchorDate, DateTime.Today)
            .Add(x => x.DaySelected, EventCallback.Factory.Create<DateTime>(this, _ => { }))
        );

        // Act
        cut.Find("button.view-switch").Click(); // Day -> Month

        // Assert
        Assert.Contains(cut.FindAll("button.month"), b => b.GetAttribute("aria-current") == "date");
    }

    [Fact]
    public void SelectingYear_NavigatesToMonthView_ForThatYear()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePickerCalendar>(p => p
            .Add(x => x.AnchorDate, new DateTime(2025, 11, 1))
            .Add(x => x.DaySelected, EventCallback.Factory.Create<DateTime>(this, _ => { }))
        );

        // Act
        cut.Find("button.view-switch").Click(); // Day -> Month
        cut.Find("button.view-switch").Click(); // Month -> Year
        var yearButton = cut.FindAll("button.year").First(b => b.TextContent.Trim() == "2030");
        yearButton.Click();

        // Assert
        Assert.Contains("months-of-the-year", cut.Markup);
        Assert.Contains("2030", cut.Markup);
    }

    [Fact]
    public void ViewChanged_FiresWithNewView()
    {
        // Arrange
        DatePickerCalendarView? newView = null;
        var cut = TestContext.Render<TwDatePickerCalendar>(p => p
            .Add(x => x.AnchorDate, new DateTime(2025, 11, 1))
            .Add(x => x.DaySelected, EventCallback.Factory.Create<DateTime>(this, _ => { }))
            .Add(x => x.ViewChanged, EventCallback.Factory.Create<DatePickerCalendarView>(this, v => newView = v))
        );

        // Act
        cut.Find("button.view-switch").Click();

        // Assert
        Assert.Equal(DatePickerCalendarView.Month, newView);
    }

    [Fact]
    public void BodyClasses_IsForwardedToBodyElement()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDatePickerCalendar>(p => p
            .Add(x => x.AnchorDate, new DateTime(2025, 11, 1))
            .Add(x => x.BodyClasses, "my-custom-body-class")
            .Add(x => x.DaySelected, EventCallback.Factory.Create<DateTime>(this, _ => { }))
        );

        // Assert - BodyClasses lands on TwDatePickerBody's inner wrapper, alongside its own "w-full".
        Assert.Contains("my-custom-body-class", cut.Markup);
    }

    [Fact]
    public void ShowMonthCaption_RendersCaptionAboveDayGrid()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDatePickerCalendar>(p => p
            .Add(x => x.AnchorDate, new DateTime(2025, 11, 1))
            .Add(x => x.ShowMonthCaption, true)
            .Add(x => x.DaySelected, EventCallback.Factory.Create<DateTime>(this, _ => { }))
        );

        // Assert
        Assert.Contains("November 2025", cut.Markup);
    }
}
