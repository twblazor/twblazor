using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Reflection;
using TwBlazor.Builders;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;

namespace TwBlazor.Tests.Components.DatePicker;

public class TwDatePickerTests : TwBlazorTestBase
{
    private TwInputTheme inputTheme => Theme.Components.Require<TwInputTheme>();

    public TwDatePickerTests()
    {
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
        TestContext.JSInterop.SetupVoid("twPicker.registerOutsideClick");
        TestContext.JSInterop.SetupVoid("twPicker.unregisterOutsideClick");
    }

    [Fact]
    public void ShouldRender_Input_WithPlaceholderAndIcon()
    {
        // Arrange
        var placeholder = "Pick a day";

        // Act
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.Placeholder, placeholder)
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
        );

        var input = cut.Find("input");

        // Assert
        Assert.NotNull(input);
        Assert.Equal(placeholder, input.GetAttribute("placeholder"));
        Assert.NotNull(cut.Find("svg"));
    }

    [Fact]
    public void FocusingInput_ShowsDatePicker()
    {
        // Arrange
        var date = new DateTime(2025, 11, 1);


        // Act
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, date)
        );

        cut.Find("input").Click();

        // Assert
        Assert.Contains("datepicker-grid", cut.Markup);
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.registerOutsideClick");
    }

    [Fact]
    public void FocusingInput_PositionsPanelAsFixed_AnchoredToTheInputRoot()
    {
        // Regression test: the popover panel must be positioned via twPicker.registerScrollReposition
        // (which applies twPicker.positionPanelFixed itself - position:fixed, anchored to the input
        // root via JS-computed coordinates) rather than the old twPicker.positionPanel
        // (position:absolute), so it isn't clipped when the picker is used inside a scrollable
        // ancestor such as a TwDialog's body.
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1)));

        cut.Find("input").Click();

        var invocation = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.registerScrollReposition");
        Assert.IsType<ElementReference>(invocation.Arguments[0]);
        Assert.IsType<ElementReference>(invocation.Arguments[1]);
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.positionPanel");
    }

    [Fact]
    public async Task Closing_UnregistersScrollReposition()
    {
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1)));

        cut.Find("input").Click();
        await cut.Instance.Close();

        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.unregisterScrollReposition");
    }

    [Fact]
    public void ClickingDay_SelectsDate_And_InvokesCallbacks()
    {
        // Arrange
        DateTime? selectedFromCallback = null;
        string? valueFromCallback = null;

        var selectedCallback = EventCallback.Factory.Create(this, (DateTime d) => selectedFromCallback = d);
        var valueCallback = EventCallback.Factory.Create(this, (string s) => valueFromCallback = s);

        var startDate = new DateTime(2025, 11, 1);

        // Act & Assert
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, startDate)
            .Add(x => x.SelectedDateChanged, selectedCallback)
            .Add(x => x.ValueChanged, valueCallback)
        );

        cut.Find("input").Click();

        var dayButton = cut.FindAll("button.day").FirstOrDefault(b => b.TextContent.Trim() == "15");

        Assert.NotNull(dayButton);

        dayButton!.Click();

        Assert.Equal(new DateTime(2025, 11, 15), selectedFromCallback);
        Assert.Equal("15/11/2025", valueFromCallback); // Format default: dd/MM/yyyy
    }

    [Fact]
    public void TypingValidDate_InInput_ParsesAndSelectsDate()
    {
        // Arrange
        DateTime? selectedFromCallback = null;
        string? valueFromCallback = null;

        var selectedCallback = EventCallback.Factory.Create(this, (DateTime d) => selectedFromCallback = d);
        var valueCallback = EventCallback.Factory.Create(this, (string s) => valueFromCallback = s);

        // Act
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDateChanged, selectedCallback)
            .Add(x => x.ValueChanged, valueCallback)
        );

        cut.Find("input").Change("18/11/2025");

        // Assert
        Assert.NotNull(selectedFromCallback);
        Assert.Equal(new DateTime(2025, 11, 18).Date, selectedFromCallback!.Value.Date);
        Assert.Equal("18/11/2025", valueFromCallback); // dd/MM/yyyy
    }

    [Fact]
    public void CustomUsStyleFormat_DisplaysAndParsesMonthFirst()
    {
        // Arrange — Format is a plain .NET custom date format string, so any pattern works, e.g.
        // US-style month-first dates.
        DateTime? selectedFromCallback = null;
        string? valueFromCallback = null;

        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2026, 12, 24))
            .Add(x => x.Format, "MM/dd/yyyy")
            .Add(x => x.SelectedDateChanged, EventCallback.Factory.Create<DateTime>(this, d => selectedFromCallback = d))
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<string>(this, v => valueFromCallback = v))
        );

        // Assert initial display
        Assert.Equal("12/24/2026", cut.Find("input").GetAttribute("value"));

        // Act — type a different date in the same US pattern
        cut.Find("input").Change("01/05/2027");

        // Assert
        Assert.Equal(new DateTime(2027, 1, 5).Date, selectedFromCallback!.Value.Date);
        Assert.Equal("01/05/2027", valueFromCallback);
    }

    [Fact]
    public void CustomDashSeparatedFormat_DisplaysAndParses()
    {
        // Arrange — a dash-separated pattern, e.g. for locales that don't use "/".
        DateTime? selectedFromCallback = null;

        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 12, 20))
            .Add(x => x.Format, "dd-MM-yyyy")
            .Add(x => x.SelectedDateChanged, EventCallback.Factory.Create<DateTime>(this, d => selectedFromCallback = d))
        );

        // Assert initial display
        Assert.Equal("20-12-2025", cut.Find("input").GetAttribute("value"));

        // Act
        cut.Find("input").Change("05-01-2026");

        // Assert
        Assert.Equal(new DateTime(2026, 1, 5).Date, selectedFromCallback!.Value.Date);
    }

    [Fact]
    public void TypingInvalidDate_InInput_DoesNotSilentlySelectToday_AndShowsError()
    {
        // Arrange
        DateTime? selectedFromCallback = null;
        string? valueFromCallback = null;

        var selectedCallback = EventCallback.Factory.Create(this, (DateTime d) => selectedFromCallback = d);
        var valueCallback = EventCallback.Factory.Create(this, (string s) => valueFromCallback = s);

        // Act
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDateChanged, selectedCallback)
            .Add(x => x.ValueChanged, valueCallback)
        );

        cut.Find("input").Change("not-a-date");

        // Assert: an unparsable date must not be silently swapped for today - neither callback fires,
        // and the field surfaces an accessible (role="alert") error instead.
        Assert.Null(selectedFromCallback);
        Assert.Null(valueFromCallback);

        var input = cut.Find("input");
        Assert.Equal("true", input.GetAttribute("aria-invalid"));

        var error = cut.Find("[role='alert']");
        Assert.Equal("Enter a valid date", error.TextContent);
    }

    [Fact]
    public void SwitchingViews_Month_Then_Day_RendersExpectedSections()
    {
        // Arrange
        var start = new DateTime(2025, 11, 3);
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, start)
        );

        cut.Find("input").Click();
        cut.Find("button.view-switch").Click(); // Day -> Month


        // Act & Assert
        Assert.Contains("months-of-the-year", cut.Markup);

        var monthButton = cut.FindAll("button.month").First(b => b.TextContent.Trim() == "Jan");
        monthButton.Click();

        Assert.Contains("datepicker-grid", cut.Markup);
    }

    [Fact]
    public void SwitchingToYearView_ShowsDecadeGrid_WithTenYears()
    {
        // Arrange
        var start = new DateTime(2025, 11, 1);
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, start)
        );

        // Act & Assert
        cut.Find("input").Click();
        cut.Find("button.view-switch").Click(); // Day -> Month
        cut.Find("button.view-switch").Click(); // Month -> Year

        Assert.Contains("years-of-the-decade", cut.Markup);
        var yearButtons = cut.FindAll("button.year");
        Assert.Equal(10, yearButtons.Count);
        Assert.Equal("2025", yearButtons[0].TextContent.Trim());
    }

    [Fact]
    public void YearView_HighlightsCurrentYear_DistinctFromSelectedYear()
    {
        // Arrange — a SelectedDate several years before today so both the selected year and
        // today's year fall within the same displayed decade, but remain distinct: the selected
        // year should keep the existing solid highlight, while today's year gets its own separate
        // ActiveClass indicator (mirroring the day grid's today-vs-selected distinction), which
        // didn't exist for the year grid before.
        var today = DateTime.Today;
        var selectedYear = today.Year - 5;
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(selectedYear, 6, 15))
        );
        var datePickerTheme = Theme.Components.Require<TwBlazor.Configuration.Components.TwDatePickerTheme>();

        // Act
        cut.Find("input").Click();
        cut.Find("button.view-switch").Click(); // Day -> Month
        cut.Find("button.view-switch").Click(); // Month -> Year

        var currentYearButton = cut.FindAll("button.year").First(b => b.TextContent.Trim() == today.Year.ToString());
        var selectedYearButton = cut.FindAll("button.year").First(b => b.TextContent.Trim() == selectedYear.ToString());

        // Assert
        Assert.Equal("date", currentYearButton.GetAttribute("aria-current"));
        Assert.Contains(datePickerTheme.ActiveClass, currentYearButton.GetAttribute("class"));
        Assert.DoesNotContain(Theme.Components.Require<TwBlazor.Configuration.Components.TwDatePickerTheme>().SelectedClass, currentYearButton.GetAttribute("class"));

        Assert.Null(selectedYearButton.GetAttribute("aria-current"));
        Assert.Contains(Theme.Components.Require<TwBlazor.Configuration.Components.TwDatePickerTheme>().SelectedClass, selectedYearButton.GetAttribute("class"));
    }

    [Fact]
    public void MonthView_HighlightsCurrentMonth_DistinctFromSelectedMonth()
    {
        // Arrange — SelectedDate uses today's year but a different month, so both the current
        // month and the selected month appear in the same 12-month grid, distinctly highlighted.
        var today = DateTime.Today;
        var selectedMonth = today.Month == 1 ? 3 : 1;
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(today.Year, selectedMonth, 15))
        );
        var datePickerTheme = Theme.Components.Require<TwBlazor.Configuration.Components.TwDatePickerTheme>();

        // Act
        cut.Find("input").Click();
        cut.Find("button.view-switch").Click(); // Day -> Month

        var currentMonthButton = cut.FindAll("button.month").First(b => b.TextContent.Trim() == today.ToString("MMM"));
        var selectedMonthButton = cut.FindAll("button.month").First(b => b.TextContent.Trim() == new DateTime(today.Year, selectedMonth, 1).ToString("MMM"));

        // Assert
        Assert.Equal("date", currentMonthButton.GetAttribute("aria-current"));
        Assert.Contains(datePickerTheme.ActiveClass, currentMonthButton.GetAttribute("class"));
        Assert.DoesNotContain(Theme.Components.Require<TwBlazor.Configuration.Components.TwDatePickerTheme>().SelectedClass, currentMonthButton.GetAttribute("class"));

        Assert.Null(selectedMonthButton.GetAttribute("aria-current"));
        Assert.Contains(Theme.Components.Require<TwBlazor.Configuration.Components.TwDatePickerTheme>().SelectedClass, selectedMonthButton.GetAttribute("class"));
    }

    [Fact]
    public void NavigationButtons_WorkAcrossMonthYearDecade()
    {
        // Arrange
        var start = new DateTime(2025, 11, 1);
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, start)
        );

        // Act & Assert
        cut.Find("input").Click();

        // Next month
        cut.Find("button.next-btn").Click();
        Assert.Contains(start.AddMonths(1).ToString("MMMM yyyy"), cut.Markup);

        // Previous twice
        cut.Find("button.prev-btn").Click();
        cut.Find("button.prev-btn").Click();
        Assert.Contains(start.AddMonths(-1).ToString("MMMM yyyy"), cut.Markup);

        // Month -> Year
        cut.Find("button.view-switch").Click(); // Day->Month
        cut.Find("button.view-switch").Click(); // Month->Year
        Assert.Contains("years-of-the-decade", cut.Markup);

        cut.Find("button.next-btn").Click();
        Assert.Contains((start.Year + 10).ToString(), cut.Markup);
    }

    [Fact]
    public void NavigatingMonths_DoesNotChangeSelectedDate_UntilADayIsPicked()
    {
        // Arrange — browsing with Next/Previous must not silently change what's actually selected;
        // only clicking a day (or typing a valid one) should ever fire SelectedDateChanged.
        var selectedDateChangedInvoked = false;
        var start = new DateTime(2025, 11, 1);
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, start)
            .Add(x => x.SelectedDateChanged, EventCallback.Factory.Create<DateTime>(this, _ => selectedDateChangedInvoked = true))
        );

        // Act
        cut.Find("input").Click();
        cut.Find("button.next-btn").Click();
        cut.Find("button.next-btn").Click();

        // Assert
        Assert.False(selectedDateChangedInvoked);
    }

    [Fact]
    public void NavigatingToADecadeWithoutTheSelectedYear_DoesNotHighlightAnyYearAsSelected()
    {
        // Arrange — this is the bug: navigation used to write directly into SelectedDate, so
        // whichever decade you paged to always showed its own first year as "selected" (since the
        // grid's own generation and the isSelectedYear check both read the same, now-drifted,
        // SelectedDate). With navigation and selection properly separated, paging to a decade that
        // doesn't contain the real selected year should highlight nothing.
        var start = new DateTime(2025, 11, 1);
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, start)
        );

        // Act
        cut.Find("input").Click();
        cut.Find("button.view-switch").Click(); // Day -> Month
        cut.Find("button.view-switch").Click(); // Month -> Year
        cut.Find("button.next-btn").Click(); // 2025-2034 -> 2035-2044, well past the real selection

        // Assert
        var yearButtons = cut.FindAll("button.year");
        Assert.All(yearButtons, b => Assert.Equal("false", b.GetAttribute("aria-pressed")));
    }

    [Fact]
    public void NavigatingToAYearWithoutTheSelectedMonth_DoesNotHighlightAnyMonthAsSelected()
    {
        // Arrange — same bug as the decade case above, one level down: paging the month grid to a
        // different year must not make that year's months falsely show one as selected.
        var start = new DateTime(2025, 11, 1);
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, start)
        );

        // Act
        cut.Find("input").Click();
        cut.Find("button.view-switch").Click(); // Day -> Month
        cut.Find("button.next-btn").Click(); // 2025 -> 2026

        // Assert
        var monthButtons = cut.FindAll("button.month");
        Assert.All(monthButtons, b => Assert.Equal("false", b.GetAttribute("aria-pressed")));
    }

    [Fact]
    public void ClickingDayAfterNavigatingMonths_SelectsTheDayInTheNavigatedMonth()
    {
        // Arrange — confirms navigation still correctly feeds into which month a day gets picked
        // from, despite no longer sharing storage with SelectedDate.
        DateTime? selectedFromCallback = null;
        var start = new DateTime(2025, 11, 1);
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, start)
            .Add(x => x.SelectedDateChanged, EventCallback.Factory.Create<DateTime>(this, d => selectedFromCallback = d))
        );

        // Act
        cut.Find("input").Click();
        cut.Find("button.next-btn").Click(); // -> December 2025
        cut.FindAll("button.day").First(b => b.TextContent.Trim() == "10").Click();

        // Assert
        Assert.Equal(new DateTime(2025, 12, 10), selectedFromCallback);
    }

    [Fact]
    public async Task ReopeningPanel_ResetsNavigationBackToSelectedDatesMonth()
    {
        // Arrange — browsing away without picking anything shouldn't linger into the next time the
        // picker is opened; it should always start back at the actual selection's own month.
        var start = new DateTime(2025, 11, 1);
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, start)
        );

        // Act
        cut.Find("input").Click();
        cut.Find("button.next-btn").Click();
        Assert.Contains("December 2025", cut.Markup);

        await cut.Instance.Close();
        cut.Find("input").Click();

        // Assert
        Assert.Contains("November 2025", cut.Markup);
    }

    [Fact]
    public void PreviousDecadeButton_MovesDisplayedDecadeBackTenYears()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
        );

        // Act
        cut.Find("input").Click();
        cut.Find("button.view-switch").Click(); // Day -> Month
        cut.Find("button.view-switch").Click(); // Month -> Year
        cut.Find("button.prev-btn").Click();

        // Assert
        var yearButtons = cut.FindAll("button.year");
        Assert.Equal("2015", yearButtons[0].TextContent.Trim());
    }

    [Fact]
    public void PreviousYearButton_MovesDisplayedYearBackOne()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
        );

        // Act
        cut.Find("input").Click();
        cut.Find("button.view-switch").Click(); // Day -> Month
        cut.Find("button.prev-btn").Click();

        // Assert
        Assert.Contains("2024", cut.Markup);
    }

    [Fact]
    public void SelectingYear_NavigatesToMonthView_ForThatYear()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
        );

        // Act
        cut.Find("input").Click();
        cut.Find("button.view-switch").Click(); // Day -> Month
        cut.Find("button.view-switch").Click(); // Month -> Year
        var yearButton = cut.FindAll("button.year").First(b => b.TextContent.Trim() == "2027");
        yearButton.Click();

        // Assert — switched to Month view, now showing 2027's months.
        Assert.Contains("months-of-the-year", cut.Markup);
        Assert.Contains("2027", cut.Markup);
    }

    [Fact]
    public void Today_UsesUtcNow_WhenSelectedDateKindIsUtc()
    {
        // Arrange — SelectedDate.Kind determines whether "today" is computed from DateTime.UtcNow
        // or DateTime.Now; most other tests use an Unspecified-kind date, so this covers the Utc
        // branch specifically. The displayed year (from anchorDate, seeded from SelectedDate) must
        // match today's real UTC year for the current-month indicator to be able to appear at all.
        var utcToday = DateTime.UtcNow.Date;
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, DateTime.SpecifyKind(new DateTime(utcToday.Year, 1, 1), DateTimeKind.Utc))
        );

        // Act
        cut.Find("input").Click();
        cut.Find("button.view-switch").Click(); // Day -> Month

        // Assert — the month matching today's actual UTC month/year is marked current.
        var monthButtons = cut.FindAll("button.month");
        var currentMonthButton = monthButtons[utcToday.Month - 1];
        Assert.Equal("date", currentMonthButton.GetAttribute("aria-current"));
    }

    [Fact]
    public async Task Close_UnregistersJS_And_HidesPopup()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
        );

        // Act & Assert
        cut.Find("input").Click();
        Assert.Contains("datepicker-grid", cut.Markup);

        await cut.Instance.Close();

        Assert.DoesNotContain("datepicker-grid", cut.Markup);
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.unregisterOutsideClick");
    }

    [Fact]
    public void DefaultVariant_RendersCorrectClasses()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
            .Add(x => x.Variant, TwBlazor.Enums.InputVariant.Default)
        );

        var input = cut.Find("input");

        // Assert
        Assert.NotNull(input);
        Assert.Contains("border-b", input.GetAttribute("class"));
    }

    [Fact]
    public void OutlinedVariant_RendersCorrectClasses()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
            .Add(x => x.Variant, TwBlazor.Enums.InputVariant.Outlined)
        );

        var input = cut.Find("input");

        // Assert
        Assert.NotNull(input);
        Assert.Contains("border", input.GetAttribute("class"));
        Assert.Contains("rounded", input.GetAttribute("class"));
    }

    [Fact]
    public void FilledVariant_RendersCorrectClasses()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
            .Add(x => x.Variant, TwBlazor.Enums.InputVariant.Filled)
        );

        var input = cut.Find("input");

        // Assert
        Assert.NotNull(input);
        Assert.Contains("bg-[oklch(97%_0_0)]", input.GetAttribute("class"));
    }

    [Fact]
    public void UsesGlobalDefaultVariant_WhenNotSet()
    {
        // Arrange - no Variant set on the component, so it must follow TwInputTheme.DefaultInputVariant
        // (inherited via TwBlazorInputComponentBase.effectiveVariant), even after the theme changes.
        inputTheme.DefaultInputVariant = TwBlazor.Enums.InputVariant.Outlined;

        // Act
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
        );

        // Assert
        var input = cut.Find("input");
        Assert.Contains(InputVariantBuilder.GetClasses(TwBlazor.Enums.InputVariant.Outlined, inputTheme), input.GetAttribute("class"));
    }

    [Fact]
    public async Task DisposeAsync_UnregistersOutsideClick()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
        );

        cut.Find("input").Click(); // registers outside click handler

        // Act
        await cut.Instance.DisposeAsync();

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations,
            i => i.Identifier == "twPicker.unregisterOutsideClick");
    }

    [Fact]
    public void ReadOnly_FocusingInput_DoesNotShowDatePicker()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
            .Add(x => x.ReadOnly, true)
        );

        // Act
        cut.Find("input").Click();

        // Assert
        Assert.DoesNotContain("datepicker-grid", cut.Markup);
    }

    [Fact]
    public void Disabled_FocusingInput_DoesNotShowDatePicker()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
            .Add(x => x.Disabled, true)
        );

        // Act
        cut.Find("input").Click();

        // Assert
        Assert.DoesNotContain("datepicker-grid", cut.Markup);
    }

    [Fact]
    public void ReadOnly_TypingDate_DoesNotUpdateSelectedDate()
    {
        // Arrange
        DateTime? selectedFromCallback = null;
        var selectedCallback = EventCallback.Factory.Create(this, (DateTime d) => selectedFromCallback = d);

        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
            .Add(x => x.SelectedDateChanged, selectedCallback)
            .Add(x => x.ReadOnly, true)
        );

        // Act
        cut.Find("input").Change("18/11/2025");

        // Assert
        Assert.Null(selectedFromCallback);
    }

    [Fact]
    public void TypingEmptyDate_InInput_DoesNotUpdateSelectedDate()
    {
        // Arrange
        DateTime? selectedFromCallback = null;
        var selectedCallback = EventCallback.Factory.Create(this, (DateTime d) => selectedFromCallback = d);

        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
            .Add(x => x.SelectedDateChanged, selectedCallback)
        );

        // Act
        cut.Find("input").Change(string.Empty);

        // Assert
        Assert.Null(selectedFromCallback);
    }

    [Fact]
    public void Disabled_TypingDate_DoesNotUpdateSelectedDate()
    {
        // Arrange
        DateTime? selectedFromCallback = null;
        var selectedCallback = EventCallback.Factory.Create(this, (DateTime d) => selectedFromCallback = d);

        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
            .Add(x => x.SelectedDateChanged, selectedCallback)
            .Add(x => x.Disabled, true)
        );

        // Act
        cut.Find("input").Change("18/11/2025");

        // Assert
        Assert.Null(selectedFromCallback);
    }

    [Fact]
    public void PreferNativePickerTrue_RendersNativeDateInput_WithIsoValue()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 18))
            .Add(x => x.PreferNativePicker, true)
        );

        var input = cut.Find("input");

        // Assert
        Assert.Equal("date", input.GetAttribute("type"));
        Assert.Equal("2025-11-18", input.GetAttribute("value"));
    }

    [Fact]
    public void PreferNativePickerTrue_FocusingInput_DoesNotShowCustomPopup()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
            .Add(x => x.PreferNativePicker, true)
        );

        // Act
        cut.Find("input").Click();

        // Assert
        Assert.DoesNotContain("datepicker-grid", cut.Markup);
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.registerOutsideClick");
    }

    [Fact]
    public void PreferNativePickerTrue_ChangingNativeInput_ParsesIsoValue_AndInvokesCallbacks()
    {
        // Arrange
        DateTime? selectedFromCallback = null;
        string? valueFromCallback = null;

        var selectedCallback = EventCallback.Factory.Create(this, (DateTime d) => selectedFromCallback = d);
        var valueCallback = EventCallback.Factory.Create(this, (string s) => valueFromCallback = s);

        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
            .Add(x => x.PreferNativePicker, true)
            .Add(x => x.SelectedDateChanged, selectedCallback)
            .Add(x => x.ValueChanged, valueCallback)
        );

        // Act — native <input type="date"> always sends an ISO "yyyy-MM-dd" value on change.
        cut.Find("input").Change("2025-12-25");

        // Assert
        Assert.Equal(new DateTime(2025, 12, 25), selectedFromCallback);
        Assert.Equal("2025-12-25", valueFromCallback);
    }

    [Fact]
    public void PreferNativePickerTrue_ChangingNativeDateTimeInput_WithSecondsSuffix_StillParses()
    {
        // Arrange - some WebKit (Safari/iOS) versions append a ":00" seconds component to the
        // datetime-local value on change even though no `step` attribute requests it, which doesn't
        // exactly match NativeFormat's "yyyy-MM-ddTHH:mm" (issue #158). The native path should
        // tolerate this instead of surfacing "Enter a valid date" for a value the browser itself sent.
        DateTime? selectedFromCallback = null;
        string? valueFromCallback = null;
        string? errorMessage = null;

        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1, 14, 30, 0))
            .Add(x => x.PreferNativePicker, true)
            .Add(x => x.NativeInputType, "datetime-local")
            .Add(x => x.NativeFormat, "yyyy-MM-ddTHH:mm")
            .Add(x => x.SelectedDateChanged, EventCallback.Factory.Create<DateTime>(this, d => selectedFromCallback = d))
            .Add(x => x.ValueChanged, EventCallback.Factory.Create<string>(this, s => valueFromCallback = s))
        );

        // Act
        cut.Find("input").Change("2025-12-25T13:45:00");
        errorMessage = cut.Instance.ErrorMessage;

        // Assert
        Assert.False(cut.Instance.Invalid);
        Assert.True(string.IsNullOrEmpty(errorMessage));
        Assert.Equal(new DateTime(2025, 12, 25, 13, 45, 0), selectedFromCallback);
        Assert.NotNull(valueFromCallback);
    }

    [Fact]
    public void PreferNativePickerFalse_TypedText_WithExtraSeconds_StaysInvalid()
    {
        // Arrange - the leniency added for native-picker values must not weaken validation of text
        // the user actually typed into the custom popover's trigger; that still has to match Format
        // exactly, so an unexpected trailing seconds component is correctly rejected here.
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.PreferNativePicker, false)
        );

        var input = cut.Find("input[type='text']");

        // Act
        input.Change("01/11/2025 14:30:00");

        // Assert
        Assert.True(cut.Instance.Invalid);
        Assert.Equal("Enter a valid date", cut.Instance.ErrorMessage);
    }

    [Fact]
    public void PreferNativePickerFalse_RendersTextInput_AndOpensCustomPopupOnFocus()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
            .Add(x => x.PreferNativePicker, false)
        );

        var input = cut.Find("input");
        Assert.Equal("text", input.GetAttribute("type"));

        // Act
        input.Click();

        // Assert
        Assert.Contains("datepicker-grid", cut.Markup);
    }

    [Fact]
    public void NativePickerNotSpecified_DetectsViaJsInterop_AndSwitchesToDateInput()
    {
        // Arrange
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(true);

        // Act
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 18))
        );

        // Assert
        var input = cut.Find("input");
        Assert.Equal("date", input.GetAttribute("type"));
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDevice.prefersNativePicker");
    }

    [Fact]
    public void NativePickerNotSpecified_JsInteropReturnsFalse_KeepsCustomPopup()
    {
        // Arrange
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(false);

        // Act
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
        );

        var input = cut.Find("input");
        Assert.Equal("text", input.GetAttribute("type"));

        input.Click();

        // Assert
        Assert.Contains("datepicker-grid", cut.Markup);
    }

    [Fact]
    public void PreferNativePickerTrue_CustomNativeInputTypeAndFormat_AreRespected()
    {
        // Arrange & Act — this is how TwDateTimePicker configures the inner TwDatePicker for datetime-local.
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 18, 14, 30, 0))
            .Add(x => x.PreferNativePicker, true)
            .Add(x => x.NativeInputType, "datetime-local")
            .Add(x => x.NativeFormat, "yyyy-MM-ddTHH:mm")
        );

        var input = cut.Find("input");

        // Assert
        Assert.Equal("datetime-local", input.GetAttribute("type"));
        Assert.Equal("2025-11-18T14:30", input.GetAttribute("value"));
    }

    [Fact]
    public void FocusedChildContent_WhenSet_IsRenderedInsideOpenPanel()
    {
        // Arrange — TwDateTimePicker composes TwDatePicker with a time picker via this render
        // fragment; the .razor's "@if (FocusedChildContent is not null)" branch is otherwise never
        // exercised because no existing test supplies it.
        const string marker = "focused-child-marker";
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
            .Add(x => x.FocusedChildContent, RenderFragmentBuilder(marker))
        );

        // Act
        cut.Find("input").Click();

        // Assert
        Assert.Contains("datepicker-grid", cut.Markup);
        Assert.Contains(marker, cut.Markup);
    }

    [Fact]
    public void ClickingCalendarIcon_WhenEnabled_FocusesTriggerViaJsInterop()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.PreferNativePicker, true)
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
        );

        // Act
        cut.Find("[aria-label='Open date picker']").Click();

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.focusSurface");
    }

    [Fact]
    public void ClickingCalendarIcon_FocusesTriggerInput_NotTheRootOrTheIconItself()
    {
        // Arrange - regression test (reported: the calendar icon didn't open the panel on click).
        // twDialog.focusSurface used to be called with the whole InputRoot, but the icon is rendered
        // ahead of the trigger <input> in DOM order and (role="button" tabindex="0") is itself
        // focusable - so scanning the root for the first focusable descendant found the icon that was
        // just clicked and refocused it, a no-op that never fired the input's focus event, so the
        // panel never opened. The JS call must target the trigger's actual <input> element instead.
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.PreferNativePicker, true)
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
        );

        // Act
        cut.Find("[aria-label='Open date picker']").Click();

        // Assert
        var invocation = TestContext.JSInterop.Invocations.Single(i => i.Identifier == "twDialog.focusSurface");
        var passedRef = Assert.IsType<ElementReference>(invocation.Arguments[0]);

        var baseType = typeof(TwPopoverPickerComponentBase);
        var triggerInputRefProperty = baseType.GetProperty("triggerInputRef", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var inputRootField = baseType.GetField("InputRoot", BindingFlags.NonPublic | BindingFlags.Instance)!;

        var expectedRef = (ElementReference?)triggerInputRefProperty.GetValue(cut.Instance);
        var rootRef = ((TwInputRoot?)inputRootField.GetValue(cut.Instance))?.RootRef;

        Assert.NotNull(expectedRef);
        Assert.Equal(expectedRef!.Value.Id, passedRef.Id);
        Assert.NotEqual(rootRef?.Id, passedRef.Id);
    }

    [Fact]
    public void ClickingCalendarIcon_WhenDisabled_DoesNotInvokeJsInterop()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.PreferNativePicker, true)
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
            .Add(x => x.Disabled, true)
        );

        // Act
        cut.Find("[aria-label='Open date picker']").Click();

        // Assert
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.focusSurface");
    }

    [Theory]
    [InlineData("Enter")]
    [InlineData(" ")]
    public void CalendarIconKeyDown_EnterOrSpace_FocusesTriggerViaJsInterop(string key)
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.PreferNativePicker, true)
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
        );

        // Act
        cut.Find("[aria-label='Open date picker']").KeyDown(new KeyboardEventArgs { Key = key });

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.focusSurface");
    }

    [Fact]
    public void CalendarIconKeyDown_OtherKey_DoesNotInvokeJsInterop()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.PreferNativePicker, true)
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
        );

        // Act
        cut.Find("[aria-label='Open date picker']").KeyDown(new KeyboardEventArgs { Key = "Tab" });

        // Assert
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.focusSurface");
    }

    [Fact]
    public void PanelKeyDown_Escape_ClosesPanelAndReleasesFocusTrap()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
        );
        cut.Find("input").Click();
        Assert.Contains("datepicker-grid", cut.Markup);

        // Act
        cut.Find("[role='dialog']").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        // Assert
        Assert.DoesNotContain("datepicker-grid", cut.Markup);
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.releaseFocusTrap");
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.unregisterOutsideClick");
    }

    [Fact]
    public void PanelKeyDown_NonEscapeKey_KeepsPanelOpen()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
        );
        cut.Find("input").Click();

        // Act
        cut.Find("[role='dialog']").KeyDown(new KeyboardEventArgs { Key = "a" });

        // Assert
        Assert.Contains("datepicker-grid", cut.Markup);
    }

    [Fact]
    public void SelectingDay_WithCapturedFocusToken_RestoresFocusViaJsInterop()
    {
        // Arrange — simulate the trigger having genuinely had focus captured before the panel
        // opened, so SelectDateAsync's RestoreFocusAsync call actually reaches the JS restore-focus
        // path instead of no-oping on a null token (the default in every other test here).
        TestContext.JSInterop.Setup<string?>("twDialog.captureFocus").SetResult("captured-token");

        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
        );
        cut.Find("input").Click();

        // Act
        var dayButton = cut.FindAll("button.day").First(b => b.TextContent.Trim() == "15");
        dayButton.Click();

        // Assert
        var invocation = TestContext.JSInterop.Invocations.Single(i => i.Identifier == "twDialog.restoreFocus");
        Assert.Equal("captured-token", invocation.Arguments[0]);
    }

    [Fact]
    public void TypingValidDate_WhilePanelIsOpen_ReleasesFocusTrapBeforeClosing()
    {
        // Arrange — every other typed-date test types without ever opening the panel first, so
        // OnTextValueChanged's "if (isFocused) await ReleasePanelTrapAsync()" branch is otherwise
        // never taken.
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
        );
        cut.Find("input").Click();
        Assert.Contains("datepicker-grid", cut.Markup);

        // Act
        cut.Find("input").Change("18/11/2025");

        // Assert
        Assert.DoesNotContain("datepicker-grid", cut.Markup);
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twDialog.releaseFocusTrap");
    }

    [Fact]
    public void EmptyFormat_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            TestContext.Render<TwDatePicker>(p => p
                .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
                .Add(x => x.Format, string.Empty)));
    }

    [Fact]
    public void UnsetFormat_UsesThemeDefaultFormat()
    {
        // Arrange - changing the shared theme default (rather than passing Format per instance)
        // must be picked up when Format is left unset.
        var datePickerTheme = Theme.Components.Require<TwDatePickerTheme>();
        datePickerTheme.DefaultFormat = "MM/dd/yyyy";

        // Act
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 24))
        );

        // Assert
        Assert.Equal("11/24/2025", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void ExplicitFormat_OverridesThemeDefault()
    {
        // Arrange
        var datePickerTheme = Theme.Components.Require<TwDatePickerTheme>();
        datePickerTheme.DefaultFormat = "MM/dd/yyyy";

        // Act
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 24))
            .Add(x => x.Format, "dd-MM-yyyy")
        );

        // Assert
        Assert.Equal("24-11-2025", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public async Task Close_WhenPanelNeverOpened_SkipsUnregisterAndDoesNotThrow()
    {
        // Arrange — Close() is reachable even if the panel was never focused open (e.g. called
        // defensively by a composing component); registeredOutsideHandler is still false here, so
        // UnregisterOutsideClickAsync's guard clause should short-circuit rather than call JS.
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.SelectedDate, new DateTime(2025, 11, 1))
        );

        // Act
        await cut.Instance.Close();

        // Assert
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.unregisterOutsideClick");
    }
}