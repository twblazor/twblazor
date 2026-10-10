using Microsoft.Playwright;
using TwBlazor.A11yTests.Infrastructure;
using static Microsoft.Playwright.Assertions;

namespace TwBlazor.A11yTests;

/// <summary>
/// Drives the real docs site with the keyboard, in a real browser. An axe scan can only say whether the
/// markup is valid; these check the things a keyboard or screen reader user actually runs into: where focus
/// goes, which key closes what, and whether anything can be reached that should not be.
/// </summary>
[Collection(A11yCollection.Name)]
public class KeyboardNavigationTests(A11yFixture fixture)
{
    private const string ActiveElementId = "document.activeElement?.id ?? ''";

    private async Task<IPage> OpenAsync(string route, int width = 1280, int height = 900)
    {
        var page = await fixture.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = width, Height = height }
        });
        await page.GotoAsync(new Uri(fixture.BaseAddress, route).ToString());
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return page;
    }

    private static ILocator Dialog(IPage page) => page.Locator("[role='dialog'][aria-modal='true']");

    [Fact]
    public async Task SkipLink_StaysOnThePage_AndMovesFocusToTheMainContent()
    {
        var page = await OpenAsync("button");
        try
        {
            await page.Keyboard.PressAsync("Tab");
            await Expect(page.Locator("a:focus")).ToHaveTextAsync("Skip to main content");

            await page.Keyboard.PressAsync("Enter");

            await page.WaitForFunctionAsync("document.activeElement?.id === 'main-content'");
            Assert.EndsWith("/button", new Uri(page.Url).AbsolutePath);
            Assert.Equal("MAIN", await page.EvaluateAsync<string>("document.activeElement.tagName"));
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task SearchDialog_ClosesOnASingleEscape_AndReturnsFocusToItsButton()
    {
        var page = await OpenAsync("");
        try
        {
            await page.Locator("#search-dialog-toggle").FocusAsync();
            await page.Keyboard.PressAsync("Enter");
            await Expect(Dialog(page)).ToBeVisibleAsync();
            await page.WaitForFunctionAsync("document.activeElement?.tagName === 'INPUT'");

            await page.Keyboard.PressAsync("Escape");

            await Expect(Dialog(page)).ToHaveCountAsync(0);
            await page.WaitForFunctionAsync("document.activeElement?.id === 'search-dialog-toggle'");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task Dialog_ClosesOnASingleEscape_EvenWhenFocusIsNotInsideIt()
    {
        var page = await OpenAsync("dialog");
        try
        {
            await page.GetByRole(AriaRole.Button, new() { Name = "Show Confirm Dialog" }).ClickAsync();
            await Expect(Dialog(page)).ToBeVisibleAsync();

            // The state a screen reader's reading cursor, or a re-render of the focused control, leaves behind.
            await page.EvaluateAsync("document.activeElement.blur()");
            Assert.Equal("BODY", await page.EvaluateAsync<string>("document.activeElement.tagName"));

            await page.Keyboard.PressAsync("Escape");

            await Expect(Dialog(page)).ToHaveCountAsync(0);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task DatePicker_DoesNotOpenOnFocus_OpensWithArrowDown_AndClosesWithEscapeFromTheField()
    {
        var page = await OpenAsync("date-picker");
        try
        {
            var field = page.Locator("#basic-datepicker");
            var panel = page.Locator("#basic-datepicker-panel");

            await field.FocusAsync();
            await Expect(panel).ToHaveCountAsync(0);

            // Arrow Down opens the calendar and puts focus on a day in the grid.
            await page.Keyboard.PressAsync("ArrowDown");
            await Expect(panel).ToBeVisibleAsync();
            await page.WaitForFunctionAsync("document.activeElement?.closest('[role=\"grid\"]') !== null");

            await page.Keyboard.PressAsync("Escape");
            await Expect(panel).ToHaveCountAsync(0);
            await page.WaitForFunctionAsync("document.activeElement?.id === 'basic-datepicker'");

            // Opened with a click, focus stays in the field, and Escape there closes the panel too.
            await field.ClickAsync();
            await Expect(panel).ToBeVisibleAsync();
            Assert.Equal("basic-datepicker", await page.EvaluateAsync<string>(ActiveElementId));

            await page.Keyboard.PressAsync("Escape");
            await Expect(panel).ToHaveCountAsync(0);

            // Nothing is left inert once it has closed.
            Assert.Equal(0, await page.EvaluateAsync<int>("document.querySelectorAll('[inert][data-tw-dialog-inert]').length"));
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task DatePicker_PageDown_TurnsToTheNextMonth_AndKeepsFocusInTheGrid()
    {
        var page = await OpenAsync("date-picker");
        try
        {
            await page.Locator("#basic-datepicker").FocusAsync();
            await page.Keyboard.PressAsync("ArrowDown");
            await page.WaitForFunctionAsync("document.activeElement?.closest('[role=\"grid\"]') !== null");
            var before = await page.EvaluateAsync<string>("document.activeElement.closest('[role=\"grid\"]').getAttribute('aria-label')");

            await page.Keyboard.PressAsync("PageDown");

            await page.WaitForFunctionAsync(
                "label => document.activeElement?.closest('[role=\"grid\"]')?.getAttribute('aria-label') !== undefined"
                + " && document.activeElement.closest('[role=\"grid\"]').getAttribute('aria-label') !== label",
                before);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task ClosedSidebar_CannotBeTabbedInto()
    {
        var page = await OpenAsync("button");
        try
        {
            var navigation = page.Locator("nav[aria-label='sidebar navigation']");
            await page.GetByRole(AriaRole.Button, new() { Name = "Close sidebar" }).ClickAsync();
            await Expect(navigation).ToHaveAttributeAsync("inert", "");

            await page.EvaluateAsync("document.activeElement.blur()");
            for (var i = 0; i < 6; i++)
            {
                await page.Keyboard.PressAsync("Tab");
                Assert.False(await page.EvaluateAsync<bool>(
                    "document.querySelector('nav[aria-label=\"sidebar navigation\"]').contains(document.activeElement)"));
            }
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task SidebarDrawer_OnANarrowScreen_TakesFocus_BlocksThePage_AndClosesWithEscape()
    {
        var page = await OpenAsync("button", width: 600);
        try
        {
            var navigation = page.Locator("nav[aria-label='sidebar navigation']");
            await Expect(navigation).ToHaveAttributeAsync("inert", "");

            var toggle = page.GetByRole(AriaRole.Button, new() { Name = "Open sidebar" });
            await toggle.FocusAsync();
            await page.Keyboard.PressAsync("Enter");

            await page.WaitForFunctionAsync(
                "document.querySelector('nav[aria-label=\"sidebar navigation\"]').contains(document.activeElement)");
            await Expect(page.Locator("#main-content")).ToHaveAttributeAsync("inert", "");

            await page.Keyboard.PressAsync("Escape");

            await Expect(navigation).ToHaveAttributeAsync("inert", "");
            await page.WaitForFunctionAsync("document.activeElement?.getAttribute('aria-label') === 'Open sidebar'");
            Assert.False(await page.EvaluateAsync<bool>("document.getElementById('main-content').hasAttribute('inert')"));
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task Tree_IsOneTabStop_AndTheArrowKeysMoveBetweenItsNodes()
    {
        var page = await OpenAsync("tree-list");
        try
        {
            var tree = page.Locator("[role='tree']").First;
            await Expect(tree.Locator("[role='treeitem'][tabindex='0']")).ToHaveCountAsync(1);

            await tree.Locator("[role='treeitem']").First.FocusAsync();
            Assert.Equal("false", await page.EvaluateAsync<string>("document.activeElement.getAttribute('aria-expanded')"));

            // Right expands the node, Right again moves to its first child, Left moves back to the parent.
            await page.Keyboard.PressAsync("ArrowRight");
            await page.WaitForFunctionAsync("document.activeElement?.getAttribute('aria-expanded') === 'true'");

            await page.Keyboard.PressAsync("ArrowRight");
            await page.WaitForFunctionAsync("document.activeElement?.textContent.includes('resume.pdf') && !document.activeElement.hasAttribute('aria-expanded')");

            await page.Keyboard.PressAsync("ArrowLeft");
            await page.WaitForFunctionAsync("document.activeElement?.getAttribute('aria-expanded') === 'true'");

            await page.Keyboard.PressAsync("End");
            await page.WaitForFunctionAsync("document.activeElement?.textContent.trim() === 'readme.md'");
            await Expect(tree.Locator("[role='treeitem'][tabindex='0']")).ToHaveCountAsync(1);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task FirstLoad_LeavesFocusAtTheTopOfThePage()
    {
        var page = await OpenAsync("button");
        try
        {
            // Focusing the heading on arrival would start the keyboard below the skip link and the navigation.
            Assert.Equal("BODY", await page.EvaluateAsync<string>("document.activeElement.tagName"));
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task Navigating_MovesFocusToTheNewPagesHeading()
    {
        var page = await OpenAsync("button");
        try
        {
            await page.Locator("nav[aria-label='sidebar navigation'] a[href='/get-started']").ClickAsync();

            await page.WaitForURLAsync("**/get-started");
            await page.WaitForFunctionAsync("document.activeElement?.tagName === 'H1'");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task EveryComponentPage_DocumentsItsKeyboardControls()
    {
        var page = await OpenAsync("tabs");
        try
        {
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Keyboard navigation" })).ToBeVisibleAsync();
            await Expect(page.GetByRole(AriaRole.Table, new() { Name = "TwTabs keyboard controls" })).ToBeVisibleAsync();
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task FocusIndicator_SurvivesForcedColours()
    {
        var page = await OpenAsync("button");
        try
        {
            // Windows High Contrast drops box-shadow rings, so each control must still carry an outline.
            await page.EmulateMediaAsync(new PageEmulateMediaOptions { ForcedColors = ForcedColors.Active });
            await page.Locator("#main-content button:not([disabled])").First.FocusAsync();
            await page.Keyboard.PressAsync("Shift+Tab");
            await page.Keyboard.PressAsync("Tab");

            var outlineStyle = await page.EvaluateAsync<string>("getComputedStyle(document.activeElement).outlineStyle");
            var outlineWidth = await page.EvaluateAsync<string>("getComputedStyle(document.activeElement).outlineWidth");

            Assert.NotEqual("none", outlineStyle);
            Assert.NotEqual("0px", outlineWidth);
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
