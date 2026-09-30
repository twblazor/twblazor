using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Pagination;

public class TwPaginationTests : TwBlazorTestBase
{
    // Page number buttons contain a "page " screen-reader-only prefix (<span class="sr-only">page </span>)
    // ahead of the visible digits, so strip it to compare against the visible label alone.
    // Pagination controls render as <button> elements (not <a>) since they perform an in-page
    // action rather than navigating - "Link"/"a" naming here refers to the pagination items, not the HTML tag.
    private static string LinkText(IElement a) => a.TextContent.Replace("page ", string.Empty).Trim();

    private static List<string> PageLinkTexts(IRenderedComponent<TwPagination> cut) =>
        cut.FindAll("nav ul li button")
            .Select(LinkText)
            .ToList();

    #region Structure / attributes

    [Fact]
    public void Renders_Nav_WithDefaultAriaLabel()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5));

        var nav = cut.Find("nav");
        Assert.Equal("pagination", nav.GetAttribute("aria-label"));
    }

    [Fact]
    public void Renders_Nav_WithCustomAriaLabel()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5)
            .Add(p => p.AriaLabel, "Search results pagination"));

        var nav = cut.Find("nav");
        Assert.Equal("Search results pagination", nav.GetAttribute("aria-label"));
    }

    [Fact]
    public void Renders_Nav_WithAriaLabelledBy()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5)
            .Add(p => p.AriaLabelledBy, "results-heading"));

        var nav = cut.Find("nav");
        Assert.Equal("results-heading", nav.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void Renders_PreviousAndNextButtons_AsChevronIcons()
    {
        // Previous/Next are icon-only buttons (Chevron_Left/Chevron_Right), not text labels.
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5));

        var buttons = cut.FindAll("nav ul li button");
        Assert.Contains("bi-chevron-left", buttons[0].QuerySelector("i")!.GetAttribute("class"));
        Assert.Contains("bi-chevron-right", buttons[^1].QuerySelector("i")!.GetAttribute("class"));
    }

    [Fact]
    public void GeneratesId_WhenNotProvided()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5));

        var nav = cut.Find("nav");
        var id = nav.GetAttribute("id");
        Assert.NotNull(id);
        Assert.StartsWith("pagination-", id);
    }

    [Fact]
    public void AppliesId_ToNavElement_WhenProvided()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.Id, "search-pagination")
            .Add(p => p.TotalPages, 5));

        var nav = cut.Find("nav");
        Assert.Equal("search-pagination", nav.GetAttribute("id"));
    }

    [Fact]
    public void AppliesStyle_ToNavElement()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5)
            .Add(p => p.Style, "margin-top: 1rem;"));

        var nav = cut.Find("nav");
        Assert.Equal("margin-top: 1rem;", nav.GetAttribute("style"));
    }

    [Fact]
    public void AppliesAdditionalAttributes_ToNavElement()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5)
            .Add(p => p.Attributes, new Dictionary<string, object> { ["data-testid"] = "my-pagination" }));

        var nav = cut.Find("nav");
        Assert.Equal("my-pagination", nav.GetAttribute("data-testid"));
    }

    [Fact]
    public void AppliesClass_ToRootDiv()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5)
            .Add(p => p.Class, "my-custom-class"));

        var root = cut.Find("nav").ParentElement;
        Assert.NotNull(root);
        Assert.Contains("my-custom-class", root!.GetAttribute("class"));
    }

    [Fact]
    public void DoesNotRenderPageSizeSelect_ByDefault()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5));

        Assert.Empty(cut.FindAll("select"));
    }

    #endregion

    #region VisiblePages windowing

    [Fact]
    public void ShowsFirstThreePages_WhenActivePageIsOne()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 1)
            .Add(p => p.TotalPages, 10));

        var pageTexts = PageLinkTexts(cut).Skip(1).SkipLast(1);
        Assert.Equal(["1", "2", "3"], pageTexts);
    }

    [Fact]
    public void ShowsSlidingWindow_WhenActivePageIsMiddle()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 5)
            .Add(p => p.TotalPages, 10));

        var pageTexts = PageLinkTexts(cut).Skip(1).SkipLast(1);
        Assert.Equal(["4", "5", "6"], pageTexts);
    }

    [Fact]
    public void ShowsLastThreePages_WhenActivePageIsLast()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 10)
            .Add(p => p.TotalPages, 10));

        var pageTexts = PageLinkTexts(cut).Skip(1).SkipLast(1);
        Assert.Equal(["8", "9", "10"], pageTexts);
    }

    [Fact]
    public void ShowsAllPages_WhenTotalPagesLessThanThree()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 2)
            .Add(p => p.TotalPages, 2));

        var pageTexts = PageLinkTexts(cut).Skip(1).SkipLast(1);
        Assert.Equal(["1", "2"], pageTexts);
    }

    [Fact]
    public void ShowsSinglePage_WhenTotalPagesIsOne()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 1)
            .Add(p => p.TotalPages, 1));

        var pageTexts = PageLinkTexts(cut).Skip(1).SkipLast(1);
        Assert.Equal(["1"], pageTexts);
    }

    [Fact]
    public void ShowsNoPageNumbers_WhenTotalPagesIsZero()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 1)
            .Add(p => p.TotalPages, 0));

        var items = cut.FindAll("nav ul li");
        Assert.Equal(2, items.Count); // Previous + Next only
    }

    #endregion

    #region Active page styling

    [Fact]
    public void AppliesActiveClasses_ToCurrentPage()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 5)
            .Add(p => p.TotalPages, 10));

        var links = cut.FindAll("nav ul li button");
        var activeLink = links.Single(a => LinkText(a) == "5");

        Assert.Contains("text-purple-600", activeLink.GetAttribute("class"));
        Assert.Contains("bg-purple-200", activeLink.GetAttribute("class"));
    }

    [Fact]
    public void AppliesInactiveClasses_ToOtherPages()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 5)
            .Add(p => p.TotalPages, 10));

        var links = cut.FindAll("nav ul li button");
        var inactiveLink = links.Single(a => LinkText(a) == "4");

        Assert.Contains("text-[oklch(50%_0.006_285.885)]", inactiveLink.GetAttribute("class"));
        Assert.DoesNotContain("text-purple-600", inactiveLink.GetAttribute("class"));
    }

    [Fact]
    public void SetsAriaCurrent_OnActivePageOnly()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 5)
            .Add(p => p.TotalPages, 10));

        var links = cut.FindAll("nav ul li button");
        var activeLink = links.Single(a => LinkText(a) == "5");
        var inactiveLink = links.Single(a => LinkText(a) == "4");

        Assert.Equal("page", activeLink.GetAttribute("aria-current"));
        Assert.Null(inactiveLink.GetAttribute("aria-current"));
    }

    #endregion

    #region Dense

    [Fact]
    public void UsesDefaultSize_WhenNotDense()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5));

        foreach (var button in cut.FindAll("nav ul li button"))
        {
            Assert.Contains("h-10", button.GetAttribute("class"));
            Assert.DoesNotContain("h-8", button.GetAttribute("class"));
        }
    }

    [Fact]
    public void UsesSmallerSize_ForEveryButton_WhenDense()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5)
            .Add(p => p.Dense, true));

        var buttons = cut.FindAll("nav ul li button");
        Assert.NotEmpty(buttons);

        foreach (var button in buttons)
        {
            Assert.Contains("h-8", button.GetAttribute("class"));
            Assert.DoesNotContain("h-10", button.GetAttribute("class"));
        }
    }

    [Fact]
    public void PageSizeSelect_IsDense_WhenDense()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5)
            .Add(p => p.ShowPageSize, true)
            .Add(p => p.Dense, true));

        Assert.Contains("h-8", cut.Find("select").GetAttribute("class"));
    }

    [Fact]
    public void PageSizeSelect_IsNotDense_ByDefault()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5)
            .Add(p => p.ShowPageSize, true));

        Assert.Contains("h-10", cut.Find("select").GetAttribute("class"));
    }

    #endregion

    #region Rounded corners

    [Fact]
    public void AppliesRounded_ToTheButtonGroup_NotEachButton()
    {
        // The list clips every button to one rounded, bordered container (like the table), so the
        // rounded builder's class sits on the <ul> and the buttons stay square.
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 2)
            .Add(p => p.TotalPages, 5));

        Assert.Contains(RoundedBuilder.GetRounded(), cut.Find("nav ul").GetAttribute("class"));
        Assert.Contains("overflow-hidden", cut.Find("nav ul").GetAttribute("class"));

        foreach (var button in cut.FindAll("nav ul li button"))
        {
            Assert.DoesNotContain(RoundedBuilder.GetRounded(), button.GetAttribute("class"));
        }
    }

    [Fact]
    public void ButtonGroup_UsesTheTablesBorderAndDividerColors()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5));

        var list = cut.Find("nav ul").GetAttribute("class");
        Assert.Contains("border-[oklch(95%_0_0)]", list);
        Assert.Contains("divide-x", list);
    }

    #endregion

    #region Disabled boundary state

    [Fact]
    public void PreviousButton_IsDisabled_OnFirstPage()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 1)
            .Add(p => p.TotalPages, 5));

        var previous = cut.FindAll("nav ul li button")[0];

        // Native disabled removes the button from the tab order and blocks activation outright -
        // stronger than the tabindex="-1" hack the old <a>-based markup needed, since <a> has no
        // disabled attribute of its own.
        Assert.Equal("true", previous.GetAttribute("aria-disabled"));
        Assert.True(previous.HasAttribute("disabled"));
        Assert.Contains("cursor-not-allowed", previous.GetAttribute("class"));
    }

    [Fact]
    public void PreviousButton_IsEnabled_WhenNotOnFirstPage()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 2)
            .Add(p => p.TotalPages, 5));

        var previous = cut.FindAll("nav ul li button")[0];

        Assert.Null(previous.GetAttribute("aria-disabled"));
        Assert.Null(previous.GetAttribute("tabindex"));
        Assert.DoesNotContain("pointer-events-none", previous.GetAttribute("class"));
        Assert.Contains("hover:bg-purple-50", previous.GetAttribute("class"));
    }

    [Fact]
    public void NextButton_IsDisabled_OnLastPage()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 5)
            .Add(p => p.TotalPages, 5));

        var next = cut.FindAll("nav ul li button")[^1];

        // Native disabled removes the button from the tab order and blocks activation outright -
        // stronger than the tabindex="-1" hack the old <a>-based markup needed, since <a> has no
        // disabled attribute of its own.
        Assert.Equal("true", next.GetAttribute("aria-disabled"));
        Assert.True(next.HasAttribute("disabled"));
        Assert.Contains("cursor-not-allowed", next.GetAttribute("class"));
    }

    [Fact]
    public void NextButton_IsEnabled_WhenNotOnLastPage()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 4)
            .Add(p => p.TotalPages, 5));

        var next = cut.FindAll("nav ul li button")[^1];

        Assert.Null(next.GetAttribute("aria-disabled"));
        Assert.Null(next.GetAttribute("tabindex"));
        Assert.DoesNotContain("pointer-events-none", next.GetAttribute("class"));
        Assert.Contains("hover:bg-purple-50", next.GetAttribute("class"));
    }

    [Fact]
    public void BothButtons_AreDisabled_WhenOnlyOnePageExists()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 1)
            .Add(p => p.TotalPages, 1));

        var links = cut.FindAll("nav ul li button");

        Assert.Equal("true", links[0].GetAttribute("aria-disabled"));
        Assert.Equal("true", links[^1].GetAttribute("aria-disabled"));
    }

    #endregion

    #region OnPageClick behavior

    [Fact]
    public void ClickingPageNumber_UpdatesActivePage_AndInvokesCallback()
    {
        int? callbackValue = null;
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 1)
            .Add(p => p.TotalPages, 10)
            .Add(p => p.ActivePageChanged, EventCallback.Factory.Create<int>(this, v => callbackValue = v)));

        cut.FindAll("nav ul li button").Single(a => LinkText(a) == "3").Click();

        Assert.Equal(3, callbackValue);
        var activeLink = cut.FindAll("nav ul li button").Single(a => a.GetAttribute("aria-current") == "page");
        Assert.Equal("3", LinkText(activeLink));
    }

    [Fact]
    public void ClickingActivePage_DoesNothing_NoCallbackInvoked()
    {
        var callbackInvoked = false;
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 5)
            .Add(p => p.TotalPages, 10)
            .Add(p => p.ActivePageChanged, EventCallback.Factory.Create<int>(this, _ => callbackInvoked = true)));

        cut.FindAll("nav ul li button").Single(a => LinkText(a) == "5").Click();

        Assert.False(callbackInvoked);
    }

    [Fact]
    public void ClickingNext_AdvancesPage()
    {
        int? callbackValue = null;
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 5)
            .Add(p => p.TotalPages, 10)
            .Add(p => p.ActivePageChanged, EventCallback.Factory.Create<int>(this, v => callbackValue = v)));

        cut.FindAll("nav ul li button")[^1].Click(); // Next

        Assert.Equal(6, callbackValue);
    }

    [Fact]
    public void ClickingPrevious_GoesBack()
    {
        int? callbackValue = null;
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 5)
            .Add(p => p.TotalPages, 10)
            .Add(p => p.ActivePageChanged, EventCallback.Factory.Create<int>(this, v => callbackValue = v)));

        cut.FindAll("nav ul li button")[0].Click(); // Previous

        Assert.Equal(4, callbackValue);
    }

    [Fact]
    public void ClickingPrevious_AtFirstPage_DoesNothing()
    {
        var callbackInvoked = false;
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 1)
            .Add(p => p.TotalPages, 10)
            .Add(p => p.ActivePageChanged, EventCallback.Factory.Create<int>(this, _ => callbackInvoked = true)));

        cut.FindAll("nav ul li button")[0].Click(); // Previous, clamped to 1

        Assert.False(callbackInvoked);
    }

    [Fact]
    public void ClickingNext_AtLastPage_DoesNothing()
    {
        var callbackInvoked = false;
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 10)
            .Add(p => p.TotalPages, 10)
            .Add(p => p.ActivePageChanged, EventCallback.Factory.Create<int>(this, _ => callbackInvoked = true)));

        cut.FindAll("nav ul li button")[^1].Click(); // Next, clamped to 10

        Assert.False(callbackInvoked);
    }

    [Fact]
    public void ClickingPage_WithoutCallback_DoesNotThrow_AndStillUpdatesUi()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.ActivePage, 1)
            .Add(p => p.TotalPages, 10));

        cut.FindAll("nav ul li button").Single(a => LinkText(a) == "2").Click();

        var activeLink = cut.FindAll("nav ul li button").Single(a => a.GetAttribute("aria-current") == "page");
        Assert.Equal("2", LinkText(activeLink));
    }

    #endregion

    #region Page size selector

    [Fact]
    public void ShowsPageSizeSelect_WhenEnabled()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5)
            .Add(p => p.ShowPageSize, true));

        Assert.Single(cut.FindAll("select"));
    }

    [Fact]
    public void PageSizeSelect_ShowsOptions()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5)
            .Add(p => p.ShowPageSize, true));

        var optionTexts = cut.FindAll("select option").Select(o => o.TextContent.Trim()).ToList();
        Assert.Equal(["5", "10", "25", "50", "100"], optionTexts);
    }

    [Fact]
    public void PageSizeSelect_MarksCurrentPageSizeSelected()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5)
            .Add(p => p.ShowPageSize, true)
            .Add(p => p.PageSize, 25));

        var selected = cut.Find("select option[selected]");
        Assert.Equal("25", selected.TextContent.Trim());
    }

    [Fact]
    public void PageSizeSelect_UsesCustomOptions()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5)
            .Add(p => p.ShowPageSize, true)
            .Add(p => p.PageSize, 20)
            .Add(p => p.PageSizeOptions, [10, 20, 30]));

        var optionTexts = cut.FindAll("select option").Select(o => o.TextContent.Trim()).ToList();
        Assert.Equal(["10", "20", "30"], optionTexts);
    }

    [Fact]
    public void SelectingNewPageSize_InvokesCallback_AndUpdatesSelection()
    {
        int? callbackValue = null;
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5)
            .Add(p => p.ShowPageSize, true)
            .Add(p => p.PageSize, 10)
            .Add(p => p.PageSizeChanged, EventCallback.Factory.Create<int>(this, v => callbackValue = v)));

        // Options [5, 10, 25, 50, 100] map to select option ids 1..5; id 3 is 25.
        cut.Find("select").Change("3");

        Assert.Equal(25, callbackValue);
        var selected = cut.Find("select option[selected]");
        Assert.Equal("25", selected.TextContent.Trim());
    }

    [Fact]
    public void SelectingSamePageSize_DoesNotInvokeCallback()
    {
        var callbackInvoked = false;
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.TotalPages, 5)
            .Add(p => p.ShowPageSize, true)
            .Add(p => p.PageSize, 10)
            .Add(p => p.PageSizeChanged, EventCallback.Factory.Create<int>(this, _ => callbackInvoked = true)));

        // Option id 2 corresponds to 10, already the current PageSize.
        cut.Find("select").Change("2");

        Assert.False(callbackInvoked);
    }

    [Fact]
    public void PageSizeSelect_IdIsDerivedFromComponentId()
    {
        var cut = TestContext.Render<TwPagination>(parameters => parameters
            .Add(p => p.Id, "results-pagination")
            .Add(p => p.TotalPages, 5)
            .Add(p => p.ShowPageSize, true));

        var select = cut.Find("select");
        Assert.Equal("results-pagination-page-size", select.GetAttribute("id"));
    }

    #endregion
}
