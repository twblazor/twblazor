using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Components;
using TwBlazor.Components.DatePicker;
using TwBlazor.Components.TimePicker;
using TwBlazor.Enums;
using TwBlazor.Models;
using TwBlazor.Services;

namespace TwBlazor.Tests.Components;

/// <summary>
/// Pins the keyboard and screen reader behaviour the components share: the same keys mean the same thing
/// everywhere, nothing opens just because it received focus, and a control never removes itself from under
/// the keyboard.
/// </summary>
public class AccessibilityBehaviourTests : TwBlazorTestBase
{
    private static KeyboardEventArgs Key(string key) => new() { Key = key };

    private bool WasInvoked(string identifier) => TestContext.JSInterop.Invocations.Any(i => i.Identifier == identifier);

    #region Popover pickers

    [Fact]
    public void DatePicker_ArrowDownOnTheField_OpensThePanel_AndMovesFocusIntoIt()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p.Add(x => x.SelectedDate, new DateTime(2026, 3, 10)));

        // Act
        cut.Find("input").KeyDown(Key("ArrowDown"));

        // Assert
        Assert.NotNull(cut.Find("[role='dialog']"));
        Assert.True(WasInvoked("twDialog.focusPanel"));
    }

    [Fact]
    public void DatePicker_ClickOnTheField_OpensThePanel_ButLeavesFocusInTheField()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p.Add(x => x.SelectedDate, new DateTime(2026, 3, 10)));

        // Act
        cut.Find("input").Click();

        // Assert - typing a date is a first-class way to use the field, so a click must not take focus away
        Assert.NotNull(cut.Find("[role='dialog']"));
        Assert.False(WasInvoked("twDialog.focusPanel"));
    }

    [Fact]
    public void DatePicker_EscapeOnTheField_ClosesTheOpenPanel()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p.Add(x => x.SelectedDate, new DateTime(2026, 3, 10)));
        cut.Find("input").Click();

        // Act
        cut.Find("input").KeyDown(Key("Escape"));

        // Assert
        Assert.Empty(cut.FindAll("[role='dialog']"));
        Assert.Equal("false", cut.Find("input").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void DatePicker_EscapeOnTheField_DoesNothing_WhenThePanelIsClosed()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p.Add(x => x.SelectedDate, new DateTime(2026, 3, 10)));

        // Act
        cut.Find("input").KeyDown(Key("Escape"));

        // Assert
        Assert.False(WasInvoked("twDialog.restoreFocus"));
    }

    [Fact]
    public void DatePicker_OpenPanel_IsReferencedByTheField_AndMarkedAsAPopover()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p.Add(x => x.SelectedDate, new DateTime(2026, 3, 10)));
        Assert.Null(cut.Find("input").GetAttribute("aria-controls"));

        // Act
        cut.Find("input").Click();

        // Assert - data-tw-popover is what tells an enclosing dialog that Escape belongs to this panel
        var panel = cut.Find("[role='dialog']");
        Assert.Equal(panel.Id, cut.Find("input").GetAttribute("aria-controls"));
        Assert.True(panel.HasAttribute("data-tw-popover"));
    }

    [Fact]
    public void DatePicker_IconButton_OpensThePanelAndMovesFocusIn_ThenClosesIt()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p.Add(x => x.SelectedDate, new DateTime(2026, 3, 10)));

        // Act
        cut.Find("div[role='button']").Click();

        // Assert
        Assert.NotNull(cut.Find("[role='dialog']"));
        Assert.True(WasInvoked("twDialog.focusPanel"));

        // Act
        cut.Find("div[role='button']").Click();

        // Assert
        Assert.Empty(cut.FindAll("[role='dialog']"));
    }

    [Fact]
    public void DatePicker_CommittingTypedText_WhileThePanelIsOpen_ReturnsFocusToTheField()
    {
        // Arrange - Tab from the field moves into the panel, which this commit removes. Without the hand-back
        // focus would be dropped on the page body.
        TestContext.JSInterop.Setup<string?>("twDialog.captureFocus").SetResult("tw-focus-token");
        var cut = TestContext.Render<TwDatePicker>(p => p.Add(x => x.SelectedDate, new DateTime(2026, 3, 10)));
        cut.Find("input").Click();

        // Act
        cut.Find("input").Change("not a date");

        // Assert
        Assert.Empty(cut.FindAll("[role='dialog']"));
        Assert.True(WasInvoked("twDialog.restoreFocus"));
    }

    [Fact]
    public void Picker_InertOwner_IsScopedToThePicker()
    {
        // Arrange
        var cut = TestContext.Render<TwDatePicker>(p => p
            .Add(x => x.Id, "start")
            .Add(x => x.SelectedDate, new DateTime(2026, 3, 10)));

        // Act
        cut.Find("input").Click();
        cut.Find("input").KeyDown(Key("Escape"));

        // Assert - a picker inside a dialog must lift only its own inert-ing, never the dialog's
        var set = TestContext.JSInterop.Invocations.Last(i => i.Identifier == "twDialog.setBackgroundInert");
        var clear = TestContext.JSInterop.Invocations.Last(i => i.Identifier == "twDialog.clearBackgroundInert");
        Assert.Equal("picker-start", set.Arguments[1]);
        Assert.Equal("picker-start", clear.Arguments[0]);
    }

    [Theory]
    [InlineData("PageDown", false, 2026, 4, 10)]
    [InlineData("PageUp", false, 2026, 2, 10)]
    [InlineData("PageDown", true, 2027, 3, 10)]
    [InlineData("ArrowRight", false, 2026, 3, 11)]
    public void DayGrid_KeysThatLeaveTheMonth_AskTheOwnerToTurnThePage(string key, bool shift, int year, int month, int day)
    {
        // Arrange - March 2026; ArrowRight from the 31st is the only arrow case that leaves the month
        var start = key == "ArrowRight" ? new DateTime(2026, 3, 31) : new DateTime(2026, 3, 10);
        var expected = key == "ArrowRight" ? new DateTime(2026, 4, 1) : new DateTime(year, month, day);
        DateTime? requested = null;
        var cut = TestContext.Render<TwDatePickerDayView>(p => p
            .Add(x => x.Value, start)
            .Add(x => x.NavigationRequested, EventCallback.Factory.Create<DateTime>(this, d => requested = d)));

        // Act
        cut.Find("table").KeyDown(new KeyboardEventArgs { Key = key, ShiftKey = shift });

        // Assert
        Assert.Equal(expected, requested);
    }

    [Fact]
    public void DayGrid_ArrowWithinTheMonth_DoesNotAskForAPageTurn()
    {
        // Arrange
        var requested = false;
        var cut = TestContext.Render<TwDatePickerDayView>(p => p
            .Add(x => x.Value, new DateTime(2026, 3, 10))
            .Add(x => x.NavigationRequested, EventCallback.Factory.Create<DateTime>(this, _ => requested = true)));

        // Act
        cut.Find("table").KeyDown(Key("ArrowRight"));

        // Assert
        Assert.False(requested);
        Assert.Equal("0", cut.Find("button[aria-label='March 11, 2026']").GetAttribute("tabindex"));
    }

    [Fact]
    public void Calendar_PageDown_ShowsTheNextMonth()
    {
        // Arrange
        var anchor = new DateTime(2026, 3, 10);
        var cut = TestContext.Render<TwDatePickerCalendar>(p => p
            .Add(x => x.AnchorDate, anchor)
            .Add(x => x.AnchorDateChanged, EventCallback.Factory.Create<DateTime>(this, d => anchor = d))
            .Add(x => x.DaySelected, EventCallback.Factory.Create<DateTime>(this, _ => { })));

        // Act
        cut.Find("table").KeyDown(Key("PageDown"));

        // Assert
        Assert.Equal(new DateTime(2026, 4, 10), anchor);
    }

    [Fact]
    public void Calendar_PageUp_IsStoppedByMinDate()
    {
        // Arrange
        var anchor = new DateTime(2026, 3, 10);
        var cut = TestContext.Render<TwDatePickerCalendar>(p => p
            .Add(x => x.AnchorDate, anchor)
            .Add(x => x.MinDate, new DateTime(2026, 3, 1))
            .Add(x => x.AnchorDateChanged, EventCallback.Factory.Create<DateTime>(this, d => anchor = d))
            .Add(x => x.DaySelected, EventCallback.Factory.Create<DateTime>(this, _ => { })));

        // Act
        cut.Find("table").KeyDown(Key("PageUp"));

        // Assert
        Assert.Equal(new DateTime(2026, 3, 10), anchor);
    }

    [Fact]
    public void TimeFields_AreSpinButtons_SteppedWithTheArrowKeys()
    {
        // Arrange
        var time = new TimeOnly(9, 30);
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Id, "t")
            .Add(x => x.SelectedTime, time)
            .Add(x => x.SelectedTimeChanged, EventCallback.Factory.Create<TimeOnly>(this, t => time = t)));
        var hour = cut.Find("#t-hour");

        // Assert
        Assert.Equal("spinbutton", hour.GetAttribute("role"));
        Assert.Equal("9", hour.GetAttribute("aria-valuenow"));
        Assert.Equal("23", hour.GetAttribute("aria-valuemax"));

        // Act
        hour.KeyDown(Key("ArrowUp"));
        cut.Find("#t-minute").KeyDown(Key("ArrowDown"));

        // Assert
        Assert.Equal(new TimeOnly(10, 29), time);
    }

    [Fact]
    public void AmPmButton_SaysWhichHalfOfTheDayIsSelected()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTimePickerBody>(p => p
            .Add(x => x.Is12HourFormat, true)
            .Add(x => x.SelectedTime, new TimeOnly(15, 0)));

        // Assert - the accessible name starts with the visible text, so it is announced and voice control can say it
        Assert.NotNull(cut.Find("button[aria-label='PM, switch to AM']"));
        Assert.Equal("3", cut.Find("input[aria-label='Hours']").GetAttribute("aria-valuenow"));
        Assert.Equal("12", cut.Find("input[aria-label='Hours']").GetAttribute("aria-valuemax"));
    }

    [Fact]
    public void TimeRangePicker_StageButtons_AreToggleButtons_NotHalfATabsPattern()
    {
        // Arrange
        var cut = TestContext.Render<TwTimeRangePicker>();

        // Act
        cut.Find("input").Click();

        // Assert
        var buttons = cut.FindAll("[role='group'][aria-label='Range step'] button");
        Assert.Equal(2, buttons.Count);
        Assert.Equal("true", buttons[0].GetAttribute("aria-pressed"));
        Assert.Equal("false", buttons[1].GetAttribute("aria-pressed"));
        Assert.Empty(cut.FindAll("[role='tab'], [role='tablist']"));
    }

    [Fact]
    public void TimePickerBody_DoesNotShareTheFieldsId()
    {
        // Arrange
        var cut = TestContext.Render<TwTimePicker>(p => p.Add(x => x.Id, "meeting"));

        // Act
        cut.Find("input").Click();

        // Assert
        Assert.Single(cut.FindAll("#meeting"));
        Assert.NotNull(cut.Find("#meeting-time-hour"));
    }

    #endregion

    #region Dialog

    [Fact]
    public void Dialog_ListensForEscapeOnTheDocument_AndClosesWhenTold()
    {
        // Arrange
        var dialogService = TestContext.Services.GetRequiredService<ITwDialogService>();
        var reference = dialogService.CreateReference();
        reference.InjectOptions(new TwDialogOptions());
        reference.InjectTitle("Confirm");
        reference.InjectRenderFragment(builder => builder.AddContent(0, "Body"));
        TwDialogResult? result = null;
        dialogService.OnDialogCloseRequested += (_, r) => result = r;

        // Act
        var cut = TestContext.Render<TwDialog>(p => p.Add(x => x.Reference, reference));

        // Assert - no key handler on the element: one that only fires while focus is inside the dialog is
        // what made a second Escape necessary
        Assert.True(WasInvoked("twDialog.registerEscape"));
        Assert.Throws<MissingEventHandlerException>(() => cut.Find("div[tabindex='-1']").KeyDown(Key("Escape")));

        // Act
        cut.InvokeAsync(() => cut.Instance.CloseFromEscapeAsync());

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Canceled);
    }

    [Fact]
    public void DialogProvider_ClaimsTheInertState_AsItsOwn()
    {
        // Arrange
        var dialogService = TestContext.Services.GetRequiredService<ITwDialogService>();
        var cut = TestContext.Render<TwDialogProvider>();

        // Act
        cut.InvokeAsync(() => dialogService.ShowAsync<TwSpinner>("Loading"));

        // Assert
        var invocation = TestContext.JSInterop.Invocations.Single(i => i.Identifier == "twDialog.setBackgroundInert");
        Assert.Equal("dialog", invocation.Arguments[1]);
    }

    #endregion

    #region Sidebar

    [Fact]
    public void Sidebar_SkipLink_TargetsTheCurrentPage_AndMovesFocusItself()
    {
        // Arrange
        var navigation = TestContext.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("button#usage");
        var cut = TestContext.Render<TwSidebar>(p => p.AddChildContent("Main area"));

        // Act
        var link = cut.Find("a.sr-only");
        link.Click();

        // Assert - a bare "#main-content" is resolved against <base href> and lands on the site root
        Assert.Equal("button#main-content", link.GetAttribute("href"));
        Assert.Equal("Skip to main content", link.TextContent);
        Assert.True(WasInvoked("twSidebar.focusMain"));
    }

    [Fact]
    public void Sidebar_MainContent_IsAMainLandmark()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSidebar>(p => p.AddChildContent("Main area"));

        // Assert
        Assert.Equal("MAIN", cut.Find("#main-content").TagName);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Sidebar_ClosedNavigation_IsInert(bool isOpen)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSidebar>(p => p.Add(x => x.IsSidebarOpen, isOpen));

        // Assert - moved off screen is not enough: its links would still be in the Tab order, invisibly
        Assert.Equal(!isOpen, cut.Find("nav[aria-label='sidebar navigation']").HasAttribute("inert"));
    }

    [Fact]
    public void Sidebar_Toggle_ReportsItsStateAndWhatItControls()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSidebar>(p => p.Add(x => x.IsSidebarOpen, true));

        // Assert
        var toggle = cut.Find("button[aria-label='Close sidebar']");
        Assert.Equal("true", toggle.GetAttribute("aria-expanded"));
        Assert.Equal(cut.Find("nav[aria-label='sidebar navigation']").Id, toggle.GetAttribute("aria-controls"));
    }

    [Fact]
    public void Sidebar_Escape_ClosesTheDrawer_OnANarrowViewport()
    {
        // Arrange
        TestContext.JSInterop.Setup<bool>("twSidebar.isMobileViewport").SetResult(true);
        var isOpen = true;
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSidebarOpen, true)
            .Add(x => x.IsSidebarOpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v)));

        // Act
        cut.Find("nav[aria-label='sidebar navigation']").KeyDown(Key("Escape"));

        // Assert
        Assert.False(isOpen);
        cut.WaitForAssertion(() => Assert.True(WasInvoked("twSidebar.focusById")));
    }

    [Fact]
    public void Sidebar_Escape_LeavesThePersistentSidebarOpen_OnAWideViewport()
    {
        // Arrange
        TestContext.JSInterop.Setup<bool>("twSidebar.isMobileViewport").SetResult(false);
        var isOpen = true;
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSidebarOpen, true)
            .Add(x => x.IsSidebarOpenChanged, EventCallback.Factory.Create<bool>(this, v => isOpen = v)));

        // Act
        cut.Find("nav[aria-label='sidebar navigation']").KeyDown(Key("Escape"));

        // Assert
        Assert.True(isOpen);
    }

    [Fact]
    public void Sidebar_Search_IsLabelled_AndAnnouncesHowManyItemsMatch()
    {
        // Arrange
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSearchable, true)
            .Add(x => x.NavigationItems,
            [
                new NavigationItem { Label = "Button", Href = "/button" },
                new NavigationItem { Label = "Forms", NavigationItems = [new NavigationItem { Label = "Checkbox", Href = "/checkbox" }, new NavigationItem { Label = "Chip", Href = "/chip" }] },
            ]));
        var input = cut.Find("input[type='search']");

        // Assert
        Assert.Equal("Search navigation", input.GetAttribute("aria-label"));

        // Act
        input.Input("ch");

        // Assert
        Assert.Equal("2 results", cut.Find("[role='status']").TextContent);

        // Act
        cut.Find("input[type='search']").Input("zzz");

        // Assert
        Assert.Equal("No results", cut.Find("[role='status']").TextContent);
    }

    #endregion

    #region Composite widgets

    [Fact]
    public void TreeItem_RightExpands_LeftCollapses_WithoutRaisingOnClick()
    {
        // Arrange
        var clicked = false;
        var cut = TestContext.Render<TwTreeList>(p => p
            .Add(x => x.AriaLabel, "Files")
            .AddChildContent<TwTreeListItem>(item => item
                .Add(x => x.Label, "Documents")
                .Add(x => x.OnClick, () => clicked = true)
                .AddChildContent<TwTreeListItem>(child => child.Add(x => x.Label, "resume.pdf"))));
        var documents = cut.Find("li[role='treeitem']");
        Assert.Equal("false", documents.GetAttribute("aria-expanded"));

        // Act
        documents.KeyDown(Key("ArrowRight"));

        // Assert
        Assert.Equal("true", cut.Find("li[role='treeitem']").GetAttribute("aria-expanded"));

        // Act
        cut.Find("li[role='treeitem']").KeyDown(Key("ArrowLeft"));

        // Assert
        Assert.Equal("false", cut.Find("li[role='treeitem']").GetAttribute("aria-expanded"));
        Assert.False(clicked);
        Assert.True(WasInvoked("twRoving.attach"));
    }

    [Fact]
    public void TreeItem_WithCheckboxes_SpaceChecks_AndTheVisualCheckboxIsOutOfTheTabOrder()
    {
        // Arrange
        var isChecked = false;
        var cut = TestContext.Render<TwTreeList>(p => p
            .Add(x => x.ShowCheckboxes, true)
            .Add(x => x.AriaLabel, "Files")
            .AddChildContent<TwTreeListItem>(item => item
                .Add(x => x.Label, "readme.md")
                .Add(x => x.CheckedChanged, EventCallback.Factory.Create<bool>(this, v => isChecked = v))));

        // Assert - the tree item carries aria-checked, so the inner checkbox is for the pointer only
        Assert.Equal("-1", cut.Find("input[type='checkbox']").GetAttribute("tabindex"));
        Assert.Equal("true", cut.Find("input[type='checkbox']").Closest("span[aria-hidden]")!.GetAttribute("aria-hidden"));

        // Act
        cut.Find("li[role='treeitem']").KeyDown(Key(" "));

        // Assert
        Assert.True(isChecked);
        Assert.Equal("true", cut.Find("li[role='treeitem']").GetAttribute("aria-checked"));
    }

    [Fact]
    public void PickList_TransferButtons_StayFocusable_AndTheMoveIsAnnounced()
    {
        // Arrange
        var cut = TestContext.Render<TwPickList<string>>(p => p
            .Add(x => x.SourceItems, ["Apple", "Pear"])
            .Add(x => x.SourceLabel, "Available")
            .Add(x => x.TargetLabel, "Chosen"));
        cut.FindAll("li[role='option']")[0].Click();

        // Act
        cut.Find("button[aria-label='Move selected items to Chosen']").Click();

        // Assert - the button just emptied its own selection; natively disabling it would drop focus
        var button = cut.Find("button[aria-label='Move selected items to Chosen']");
        Assert.False(button.HasAttribute("disabled"));
        Assert.Equal("true", button.GetAttribute("aria-disabled"));
        Assert.Equal("1 item moved to Chosen", cut.Find("[role='status']").TextContent);
        Assert.True(WasInvoked("twRoving.attach"));
    }

    [Fact]
    public void Carousel_ArrowKeysComeThroughScript_AndEdgeButtonsStayFocusable()
    {
        // Arrange
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.Loop, false)
            .AddChildContent<TwCarouselItem>(item => item.AddChildContent("One"))
            .AddChildContent<TwCarouselItem>(item => item.AddChildContent("Two")));

        // Assert - a key handler on the element would also fire while typing in a field inside a slide
        Assert.True(WasInvoked("twCarousel.attach"));
        var previous = cut.Find("button[aria-label='Previous slide']");
        Assert.False(previous.HasAttribute("disabled"));
        Assert.Equal("true", previous.GetAttribute("aria-disabled"));

        // Act
        cut.InvokeAsync(() => cut.Instance.NextSlideFromKeyAsync());

        // Assert
        Assert.Equal(1, cut.Instance.SelectedIndex);
    }

    [Fact]
    public void Carousel_AutoPlay_StartsPaused_ForReducedMotion_AndThePauseButtonComesFirst()
    {
        // Arrange
        TestContext.JSInterop.Setup<bool>("twCarousel.prefersReducedMotion").SetResult(true);

        // Act
        var cut = TestContext.Render<TwCarousel>(p => p
            .Add(x => x.AutoPlay, true)
            .AddChildContent<TwCarouselItem>(item => item.AddChildContent("One"))
            .AddChildContent<TwCarouselItem>(item => item.AddChildContent("Two")));

        // Assert
        cut.WaitForAssertion(() => Assert.True(cut.Instance.IsAutoPlayManuallyPaused));
        Assert.Equal("Play automatic slideshow", cut.FindAll("button")[0].GetAttribute("aria-label"));
        Assert.Equal("polite", cut.Find("[aria-live]").GetAttribute("aria-live"));
    }

    [Fact]
    public void Tabs_TheTabListCarriesTheName_AndThePanelIsATabStop()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(p => p
            .Add(x => x.AriaLabel, "Account settings")
            .AddChildContent<TwTab>(tab => tab.Add(x => x.Label, "Profile").AddChildContent("Profile content")));

        // Assert
        Assert.Equal("Account settings", cut.Find("[role='tablist']").GetAttribute("aria-label"));
        Assert.Null(cut.Find("[role='tablist']").ParentElement!.GetAttribute("aria-label"));
        Assert.Equal("0", cut.Find("[role='tabpanel']").GetAttribute("tabindex"));
    }

    #endregion

    #region Names, roles and states

    [Fact]
    public void Checkbox_IndeterminateState_IsPushedToTheDomProperty()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCheckbox<bool?>>(p => p.Add(x => x.Value, null));

        // Assert - "indeterminate" has no attribute form, so only script can make a screen reader say "mixed"
        var invocation = TestContext.JSInterop.Invocations.Single(i => i.Identifier == "twCheckbox.setIndeterminate");
        Assert.True(Assert.IsType<bool>(invocation.Arguments[1]));

        // Act
        cut.Render(p => p.Add(x => x.Value, true));

        // Assert
        var last = TestContext.JSInterop.Invocations.Last(i => i.Identifier == "twCheckbox.setIndeterminate");
        Assert.False(Assert.IsType<bool>(last.Arguments[1]));
    }

    [Fact]
    public void Checkbox_ThatIsNeverIndeterminate_MakesNoScriptCall()
    {
        // Arrange & Act
        TestContext.Render<TwCheckbox<bool>>(p => p.Add(x => x.Value, true));

        // Assert
        Assert.False(WasInvoked("twCheckbox.setIndeterminate"));
    }

    [Theory]
    [InlineData(Color.Danger, "alert")]
    [InlineData(Color.Warning, "alert")]
    [InlineData(Color.Info, "status")]
    [InlineData(Color.Success, "status")]
    public void Alert_OnlyErrorsAndWarningsInterrupt(Color color, string expectedRole)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwAlert>(p => p.Add(x => x.Color, color).Add(x => x.Text, "Message"));

        // Assert
        Assert.Equal(expectedRole, cut.Find(".tw-alert").GetAttribute("role"));
    }

    [Fact]
    public void Alert_Role_CanBeOverridden_OrRemoved()
    {
        // Arrange & Act
        var explicitRole = TestContext.Render<TwAlert>(p => p.Add(x => x.Role, "alert").Add(x => x.Text, "Message"));
        var noRole = TestContext.Render<TwAlert>(p => p.Add(x => x.Color, Color.Danger).Add(x => x.Role, "none").Add(x => x.Text, "Message"));

        // Assert
        Assert.Equal("alert", explicitRole.Find(".tw-alert").GetAttribute("role"));
        Assert.Null(noRole.Find(".tw-alert").GetAttribute("role"));
    }

    [Fact]
    public void Alert_Dismiss_HandsFocusOn_BeforeTheButtonDisappears()
    {
        // Arrange
        var cut = TestContext.Render<TwAlert>(p => p.Add(x => x.Dismissible, true).Add(x => x.Text, "Message"));

        // Act
        cut.Find("button[aria-label='Close']").Click();

        // Assert
        Assert.True(WasInvoked("twFocus.moveToNeighbour"));
    }

    [Fact]
    public void Progress_AriaLabel_NamesTheProgressElement()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwProgress<int>>(p => p
            .Add(x => x.Value, 40)
            .Add(x => x.Max, 100)
            .Add(x => x.AriaLabel, "Upload progress"));

        // Assert - on the wrapper div it named nothing
        Assert.Equal("Upload progress", cut.Find("progress").GetAttribute("aria-label"));
        Assert.Null(cut.Find("progress").ParentElement!.GetAttribute("aria-label"));
    }

    [Fact]
    public void Switch_IsAnnouncedAsASwitch()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSwitch<bool>>(p => p.Add(x => x.Value, true));

        // Assert
        Assert.Equal("switch", cut.Find("input").GetAttribute("role"));
    }

    [Fact]
    public void Textfield_Required_IsExposed()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(p => p.Add(x => x.Required, true));

        // Assert
        Assert.True(cut.Find("input").HasAttribute("required"));
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-required"));
    }

    [Fact]
    public void Textfield_Disabled_CanBeSwitchedBackOff()
    {
        // Arrange
        var cut = TestContext.Render<TwTextfield<string>>(p => p.Add(x => x.Disabled, true).Add(x => x.ReadOnly, true));
        Assert.True(cut.Find("input").HasAttribute("disabled"));

        // Act
        cut.Render(p => p.Add(x => x.Disabled, false).Add(x => x.ReadOnly, false));

        // Assert
        Assert.False(cut.Find("input").HasAttribute("disabled"));
        Assert.False(cut.Find("input").HasAttribute("readonly"));
    }

    [Fact]
    public void RadioGroup_ExposesRequiredAndItsError()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwRadioGroup<string>>(p => p
            .Add(x => x.Legend, "Plan")
            .Add(x => x.Required, true)
            .Add(x => x.Invalid, true)
            .Add(x => x.ErrorMessage, "Choose a plan")
            .Add(x => x.Items, [new RadioGroupItem<string> { Label = "Free", Value = "free" }]));

        // Assert
        var group = cut.Find("fieldset");
        var error = cut.Find("p[role='alert']");
        Assert.Equal("radiogroup", group.GetAttribute("role"));
        Assert.Equal("true", group.GetAttribute("aria-required"));
        Assert.Equal("true", group.GetAttribute("aria-invalid"));
        Assert.Equal(error.Id, group.GetAttribute("aria-describedby"));
        Assert.Equal("Choose a plan", error.TextContent);
    }

    [Fact]
    public void CheckboxGroup_DescribesItselfWithItsError()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCheckboxGroup<string>>(p => p
            .Add(x => x.Legend, "Toppings")
            .Add(x => x.Invalid, true)
            .Add(x => x.ErrorMessage, "Choose at least one"));

        // Assert
        Assert.Equal(cut.Find("p[role='alert']").Id, cut.Find("fieldset").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void GroupError_IsNotRendered_UntilTheGroupIsInvalid()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCheckboxGroup<string>>(p => p.Add(x => x.ErrorMessage, "Choose at least one"));

        // Assert
        Assert.Empty(cut.FindAll("p[role='alert']"));
        Assert.Null(cut.Find("fieldset").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void MultiSelect_Trigger_CarriesItsValueAndState()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.Multiple, true)
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Label, "Fruit")
            .Add(x => x.Required, true)
            .Add(x => x.Invalid, true)
            .Add(x => x.ErrorMessage, "Choose a fruit")
            .Add(x => x.Values, ["Apple", "Pear", "Plum"])
            .Add(x => x.SelectedValues, ["Apple", "Plum"]));

        // Assert - the chips sit outside the combobox, so without this its value is never announced
        var trigger = cut.Find("button[role='combobox']");
        Assert.Equal("2 selected: Apple, Plum", trigger.TextContent.Trim());
        Assert.Equal("true", trigger.GetAttribute("aria-required"));
        Assert.Equal("true", trigger.GetAttribute("aria-invalid"));
        Assert.Equal(cut.Find("p[role='alert']").Id, trigger.GetAttribute("aria-describedby"));
    }

    [Fact]
    public void Select_FocusBorder_UsesClassesThatAreWrittenOutInTheTheme()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.PreferNativePicker, false)
            .Add(x => x.Values, ["Apple"]));

        // Assert - rewriting "focus:" to "focus-visible:" at runtime produced class names Tailwind never saw,
        // so the select had no focus indicator at all
        var classes = cut.Find("button[role='combobox']").GetAttribute("class")!;
        Assert.Contains("focus-visible:ring-2", classes);
        Assert.Contains("focus-visible:border-purple-600", classes);
    }

    [Fact]
    public void Toast_RoleFollowsSeverity_AndTheContainerStaysReachableBehindADialog()
    {
        // Arrange
        var toastService = TestContext.Services.GetRequiredService<ITwToastService>();
        var cut = TestContext.Render<TwToastProvider>();

        // Act
        cut.InvokeAsync(() =>
        {
            toastService.AddToast(new ToastModel { Title = "Saved", Color = Color.Success });
            toastService.AddToast(new ToastModel { Title = "Failed", Color = Color.Danger });
        });

        // Assert
        var toasts = cut.FindAll("div[aria-atomic='true']");
        Assert.Contains(toasts, t => t.TextContent.Contains("Saved") && t.GetAttribute("role") == "status");
        Assert.Contains(toasts, t => t.TextContent.Contains("Failed") && t.GetAttribute("role") == "alert");

        var container = cut.Find("[data-tw-inert-exempt]");
        Assert.Equal("region", container.GetAttribute("role"));
        Assert.Equal("Notifications", container.GetAttribute("aria-label"));
    }

    [Fact]
    public void Table_ScrollWrapper_IsAFocusableNamedRegion()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTable>(p => p.Add(x => x.AriaLabel, "Orders"));

        // Assert
        var region = cut.Find("[role='region']");
        Assert.Equal("0", region.GetAttribute("tabindex"));
        Assert.Equal("Orders, Scrollable table", region.GetAttribute("aria-label"));
    }

    [Fact]
    public void Pagination_AnnouncesThePage_AndKeepsTheEdgeButtonsFocusable()
    {
        // Arrange
        var cut = TestContext.Render<TwPagination>(p => p.Add(x => x.TotalPages, 3).Add(x => x.ActivePage, 2));

        // Act
        cut.Find("button[aria-label='Next page']").Click();

        // Assert
        Assert.Equal("Page 3 of 3", cut.Find("[role='status']").TextContent);
        var next = cut.Find("button[aria-label='Next page']");
        Assert.False(next.HasAttribute("disabled"));
        Assert.Equal("true", next.GetAttribute("aria-disabled"));
    }

    [Fact]
    public void Collapse_Content_IsAGroupNotALandmark_AndTheTriggerCanSitInAHeading()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCollapse>(p => p
            .Add(x => x.Title, "Shipping")
            .Add(x => x.HeadingLevel, 3)
            .AddChildContent("Ships in two days."));

        // Assert
        Assert.Empty(cut.FindAll("[role='region']"));
        Assert.Equal("group", cut.Find($"#{cut.Find("button").GetAttribute("aria-controls")}").GetAttribute("role"));
        Assert.Equal("H3", cut.Find("button").ParentElement!.TagName);
    }

    [Fact]
    public void Tooltip_AroundAControl_DescribesTheControl_AndAddsNoTabStop()
    {
        // Arrange
        TestContext.JSInterop.Setup<bool>("twTooltip.describeControl", _ => true).SetResult(true);

        // Act
        var cut = TestContext.Render<TwTooltip>(p => p
            .Add(x => x.Text, "Saves the form")
            .AddChildContent("<button>Save</button>"));

        // Assert
        cut.WaitForAssertion(() => Assert.Null(cut.Find("div").GetAttribute("tabindex")));
        Assert.Null(cut.Find("div").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void Tooltip_AroundPlainContent_IsItselfTheTabStop()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTooltip>(p => p.Add(x => x.Text, "More detail").AddChildContent("Plain text"));

        // Assert
        Assert.Equal("0", cut.Find("div").GetAttribute("tabindex"));
        Assert.NotNull(cut.Find("div").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void Slider_ReadOnly_LocksTheKeysThatWouldMoveTheNativeValue()
    {
        // Arrange & Act
        TestContext.Render<TwSlider<int>>(p => p
            .Add(x => x.Value, 5).Add(x => x.Min, 0).Add(x => x.Max, 10).Add(x => x.Step, 1)
            .Add(x => x.ReadOnly, true));

        // Assert
        var invocation = TestContext.JSInterop.Invocations.Single(i => i.Identifier == "twSliderLock.set");
        Assert.True(Assert.IsType<bool>(invocation.Arguments[1]));
    }

    [Fact]
    public void CodeBlock_TheScrollingElement_IsTheFocusableNamedOne()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCodeBlock>(p => p.Add(x => x.Content, "var x = 1;").Add(x => x.Language, "csharp"));

        // Assert
        // The highlighter gives the code element the overflow, so that is the element a keyboard must reach.
        var code = cut.Find("code");
        Assert.Equal("0", code.GetAttribute("tabindex"));
        Assert.Equal("region", code.GetAttribute("role"));
        Assert.Equal("CSHARP code", code.GetAttribute("aria-label"));
        Assert.Null(cut.Find("pre").GetAttribute("tabindex"));
    }

    [Fact]
    public void InputRoot_DoesNotPutAnAccessibleName_OnAWrapperWithNoRole()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTextfield<string>>(p => p.Add(x => x.AriaLabel, "Email"));

        // Assert
        Assert.Equal("Email", cut.Find("input").GetAttribute("aria-label"));
        Assert.Null(cut.Find("input").ParentElement!.GetAttribute("aria-label"));
    }

    #endregion
}
