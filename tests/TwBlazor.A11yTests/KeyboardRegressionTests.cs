using Microsoft.Playwright;
using System.Globalization;
using TwBlazor.A11yTests.Infrastructure;
using static Microsoft.Playwright.Assertions;

namespace TwBlazor.A11yTests;

/// <summary>
/// Real-browser checks for failures that passed every markup-level test: focus landing on the wrong
/// element, a page left unusable after a popover is torn down, and focus indicators that exist in the
/// computed style but cannot be seen. Each one asserts what the user ends up with, not which script
/// function was called.
/// </summary>
[Collection(A11yCollection.Name)]
public class KeyboardRegressionTests(A11yFixture fixture)
{
    private const string inertLeftBehind = "document.querySelectorAll('[inert][data-tw-dialog-inert]').length";

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

    private static ILocator Popover(IPage page) => page.Locator("[data-tw-popover]");

    /// <summary>
    /// The date on the focused day button, read from its accessible name ("Selected, March 10, 2026").
    /// </summary>
    private static async Task<DateTime> FocusedDayAsync(IPage page)
    {
        var label = await page.EvaluateAsync<string>(
            "document.activeElement?.closest('[role=\"grid\"]') ? (document.activeElement.getAttribute('aria-label') ?? '') : ''");
        // "Selected, March 10, 2026" and "March 10, 2026" both end with the date.
        var match = System.Text.RegularExpressions.Regex.Match(label, @"[A-Z][a-z]+ \d{1,2}, \d{4}$", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(1));
        Assert.True(match.Success, $"The focused element is not a day button (accessible name: '{label}').");
        return DateTime.ParseExact(match.Value, "MMMM d, yyyy", CultureInfo.InvariantCulture);
    }

    private static async Task ExpectFocusedDayAsync(IPage page, DateTime expected)
    {
        var name = expected.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture);
        await page.WaitForFunctionAsync(
            "name => document.activeElement?.closest('[role=\"grid\"]') !== null && (document.activeElement.getAttribute('aria-label') ?? '').endsWith(name)",
            name);
    }

    /// <summary>
    /// Whether focusing <paramref name="target"/> from the keyboard changes any pixel in or around it.
    /// Computed style is not evidence of a focus indicator: an outline can be present and clipped away.
    /// </summary>
    private static async Task<bool> FocusChangesPixelsAsync(IPage page, ILocator target, ILocator? area = null)
    {
        area ??= target;
        await area.ScrollIntoViewIfNeededAsync();
        var box = await area.BoundingBoxAsync() ?? throw new InvalidOperationException("The element has no box.");
        var clip = new Clip
        {
            X = Math.Max(0, box.X - 8),
            Y = Math.Max(0, box.Y - 8),
            Width = box.Width + 16,
            Height = box.Height + 16
        };

        await page.EvaluateAsync("document.activeElement?.blur()");
        await page.Mouse.MoveAsync(0, 0);
        var before = await page.ScreenshotAsync(new PageScreenshotOptions { Clip = clip, Animations = ScreenshotAnimations.Disabled });

        // A key press first, so the browser treats the focus that follows as keyboard focus.
        await page.Keyboard.PressAsync("Shift");
        await target.FocusAsync();
        var after = await page.ScreenshotAsync(new PageScreenshotOptions { Clip = clip, Animations = ScreenshotAnimations.Disabled });

        return !before.AsSpan().SequenceEqual(after);
    }

    [Fact]
    public async Task PickerInADialog_EscapeFromOutsideItsPanel_ClosesOnlyThePicker_AndThePageStaysUsable()
    {
        var page = await OpenAsync("calendar");
        try
        {
            var addEvent = page.GetByRole(AriaRole.Button, new() { Name = "Add event" }).First;
            await addEvent.ClickAsync();
            await Expect(Dialog(page)).ToBeVisibleAsync();

            // Open the Start picker with a click (focus stays in the field), then move to the icon button
            // beside it. Focus is now outside the panel and not on the field.
            await Dialog(page).GetByRole(AriaRole.Combobox, new() { Name = "Start" }).ClickAsync();
            await Expect(Popover(page)).ToBeVisibleAsync();
            await page.Keyboard.PressAsync("Shift+Tab");
            await page.WaitForFunctionAsync("document.activeElement?.getAttribute('aria-label')?.startsWith('Open') === true");

            await page.Keyboard.PressAsync("Escape");

            await Expect(Popover(page)).ToHaveCountAsync(0);
            await Expect(Dialog(page)).ToBeVisibleAsync();

            await page.Keyboard.PressAsync("Escape");

            await Expect(Dialog(page)).ToHaveCountAsync(0);
            await page.WaitForFunctionAsync($"{inertLeftBehind} === 0");

            // The page must still work: the same button opens the dialog again.
            await addEvent.ClickAsync();
            await Expect(Dialog(page)).ToBeVisibleAsync();
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task PickerInADialog_EscapeWithFocusLost_ClosesThePickerFirst()
    {
        var page = await OpenAsync("calendar");
        try
        {
            await page.GetByRole(AriaRole.Button, new() { Name = "Add event" }).First.ClickAsync();
            await Expect(Dialog(page)).ToBeVisibleAsync();
            await Dialog(page).GetByRole(AriaRole.Combobox, new() { Name = "Start" }).ClickAsync();
            await Expect(Popover(page)).ToBeVisibleAsync();

            await page.EvaluateAsync("document.activeElement.blur()");
            await page.Keyboard.PressAsync("Escape");

            await Expect(Popover(page)).ToHaveCountAsync(0);
            await Expect(Dialog(page)).ToBeVisibleAsync();
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task LeavingThePage_WithAPickerOpen_LeavesNothingInert()
    {
        var page = await OpenAsync("button");
        try
        {
            await page.EvaluateAsync("Blazor.navigateTo('/date-picker')");
            await page.WaitForURLAsync("**/date-picker");
            await page.Locator("#basic-datepicker").ClickAsync();
            await Expect(page.Locator("#basic-datepicker-panel")).ToBeVisibleAsync();

            await page.GoBackAsync();
            await page.WaitForURLAsync("**/button");

            await page.WaitForFunctionAsync($"{inertLeftBehind} === 0");
            Assert.False(await page.EvaluateAsync<bool>("document.querySelector('nav[aria-label=\"sidebar navigation\"]').hasAttribute('inert')"));
            Assert.False(await page.EvaluateAsync<bool>("document.querySelector('nav[aria-label=\"Top navigation\"]').closest('[inert]') !== null"));
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task DateGrid_FocusFollowsTheKeyboardOntoTheRightDay_AcrossMonths()
    {
        var page = await OpenAsync("date-picker");
        try
        {
            await page.Locator("#basic-datepicker").FocusAsync();
            await page.Keyboard.PressAsync("ArrowDown");
            await page.WaitForFunctionAsync("document.activeElement?.closest('[role=\"grid\"]') !== null");
            var day = await FocusedDayAsync(page);

            // Page Down: the same day of the next month, not whichever date sits in the same cell.
            day = day.AddMonths(1);
            await page.Keyboard.PressAsync("PageDown");
            await ExpectFocusedDayAsync(page, day);

            // The arrows keep tracking after the page has turned.
            day = day.AddDays(7);
            await page.Keyboard.PressAsync("ArrowDown");
            await ExpectFocusedDayAsync(page, day);

            // Back past the first of the month, one day at a time: lands on the last day of the month before.
            while (day.Day > 1)
            {
                day = day.AddDays(-1);
                await page.Keyboard.PressAsync("ArrowLeft");
            }
            await ExpectFocusedDayAsync(page, day);

            day = day.AddDays(-1);
            await page.Keyboard.PressAsync("ArrowLeft");
            await ExpectFocusedDayAsync(page, day);

            // Up a week across the boundary, then two months back and a year forward.
            day = day.AddDays(-7);
            await page.Keyboard.PressAsync("ArrowUp");
            await ExpectFocusedDayAsync(page, day);

            day = day.AddMonths(-1);
            await page.Keyboard.PressAsync("PageUp");
            await ExpectFocusedDayAsync(page, day);

            day = day.AddYears(1);
            await page.Keyboard.PressAsync("Shift+PageDown");
            await ExpectFocusedDayAsync(page, day);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task DateGrid_TabStaysInsideThePanel()
    {
        var page = await OpenAsync("date-picker");
        try
        {
            await page.Locator("#basic-datepicker").FocusAsync();
            await page.Keyboard.PressAsync("ArrowDown");
            await page.WaitForFunctionAsync("document.activeElement?.closest('[role=\"grid\"]') !== null");

            for (var i = 0; i < 8; i++)
            {
                await page.Keyboard.PressAsync("Tab");
                Assert.True(await page.EvaluateAsync<bool>("document.activeElement?.closest('[data-tw-popover]') !== null"),
                    $"Tab press {i + 1} left the picker panel.");
            }
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task PickerIconButton_OpensWithSpace_WithoutScrollingThePage()
    {
        var page = await OpenAsync("date-picker");
        try
        {
            var icon = page.Locator("button[aria-label='Open date picker']").First;
            await icon.FocusAsync();
            await Expect(icon).ToHaveAttributeAsync("aria-expanded", "false");
            var scrollBefore = await page.EvaluateAsync<double>("document.getElementById('main-content').scrollTop");

            await page.Keyboard.PressAsync("Space");

            await Expect(Popover(page)).ToBeVisibleAsync();
            await Expect(icon).ToHaveAttributeAsync("aria-expanded", "true");
            Assert.Equal(scrollBefore, await page.EvaluateAsync<double>("document.getElementById('main-content').scrollTop"));

            await page.Keyboard.PressAsync("Escape");
            await Expect(Popover(page)).ToHaveCountAsync(0);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task Calendar_OpeningADayFromTheMonthView_KeepsFocusInTheCalendar()
    {
        var page = await OpenAsync("calendar");
        try
        {
            await page.GetByRole(AriaRole.Button, new() { Name = "Month view" }).First.ClickAsync();
            var grid = page.Locator("table[role='grid']").First;
            await Expect(grid).ToBeVisibleAsync();

            await grid.Locator("button[tabindex='0']").First.FocusAsync();
            await page.Keyboard.PressAsync("Enter");

            await page.WaitForFunctionAsync("document.activeElement?.id?.endsWith('-title') === true");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task ReadOnlyCalendar_HasNoTimeSlotButtons()
    {
        var page = await OpenAsync("calendar");
        try
        {
            // The first calendar on the page is not editable.
            var readOnly = page.Locator("#main-content [role='group'][aria-label='Select view']").First.Locator("xpath=ancestor::div[.//button[@aria-label='Previous']][1]");
            await page.GetByRole(AriaRole.Button, new() { Name = "Week view" }).First.ClickAsync();

            Assert.Equal(0, await readOnly.Locator("button[aria-disabled='true'][aria-label$='AM'], button[aria-disabled='true'][aria-label$='PM']").CountAsync());
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task ReadOnlySwitch_CannotBeFlippedFromTheKeyboard()
    {
        var page = await OpenAsync("switch");
        try
        {
            var readOnly = page.Locator("input[aria-label='Readonly switch']");
            await readOnly.FocusAsync();
            var before = await readOnly.IsCheckedAsync();

            await page.Keyboard.PressAsync("Space");
            await page.WaitForTimeoutAsync(250);

            Assert.Equal(before, await readOnly.IsCheckedAsync());
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task MultiSelect_RemovingAChip_ReturnsFocusToTheField()
    {
        var page = await OpenAsync("select");
        try
        {
            var remove = page.Locator("#main-content button[aria-label^='Remove ']").First;
            await remove.FocusAsync();

            await page.Keyboard.PressAsync("Enter");

            await page.WaitForFunctionAsync("document.activeElement?.getAttribute('role') === 'combobox'");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task SidebarDrawer_CoversNothingFocusable_AndHasItsOwnCloseButton()
    {
        var page = await OpenAsync("button", width: 600);
        try
        {
            var toggle = page.GetByRole(AriaRole.Button, new() { Name = "Open sidebar" });
            await toggle.FocusAsync();
            await page.Keyboard.PressAsync("Enter");
            await Expect(page.Locator("#main-content-root")).ToHaveAttributeAsync("inert", "");

            // Tab all the way round: focus never leaves the drawer for the top bar hidden behind it.
            for (var i = 0; i < 60; i++)
            {
                await page.Keyboard.PressAsync("Tab");
                Assert.True(await page.EvaluateAsync<bool>(
                    "document.querySelector('nav[aria-label=\"sidebar navigation\"]').contains(document.activeElement)"),
                    $"Tab press {i + 1} left the drawer.");
            }

            var close = page.Locator("nav[aria-label='sidebar navigation']").GetByRole(AriaRole.Button, new() { Name = "Close sidebar" });
            await Expect(close).ToBeVisibleAsync();
            await close.ClickAsync();

            await Expect(page.Locator("nav[aria-label='sidebar navigation']")).ToHaveAttributeAsync("inert", "");
            await page.WaitForFunctionAsync("!document.getElementById('main-content-root').hasAttribute('inert')");
            await page.WaitForFunctionAsync("document.activeElement?.getAttribute('aria-label') === 'Open sidebar'");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task TableScrollArea_ShowsItsFocus_AndIsOnlyATabStopWhenItScrolls()
    {
        var page = await OpenAsync("table", width: 360);
        try
        {
            var wrapper = page.Locator("#main-content div[tabindex='0']:has(> table)").First;
            await Expect(wrapper).ToBeVisibleAsync();

            Assert.True(await FocusChangesPixelsAsync(page, wrapper), "Focusing the table's scroll area changes nothing on screen.");

            // Every wrapper that is a Tab stop really does overflow.
            Assert.True(await page.EvaluateAsync<bool>(
                "[...document.querySelectorAll('#main-content div[tabindex=\"0\"]:has(> table)')].every(el => el.scrollWidth > el.clientWidth + 1 || el.scrollHeight > el.clientHeight + 1)"));
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory]
    [InlineData("button", "#main-content button:not([disabled])", null)]
    [InlineData("switch", "#main-content input[role='switch']:not([disabled])", "xpath=ancestor::label[1]")]
    [InlineData("slider", "#main-content input[type='range']:not([disabled])", "xpath=..")]
    [InlineData("file-upload", "#main-content input[type='file']", "xpath=ancestor::label[1]")]
    [InlineData("tree-list", "#main-content [role='treeitem']", null)]
    [InlineData("checkbox", "#main-content input[type='checkbox']:not([disabled])", "xpath=ancestor::label[1]")]
    [InlineData("textfield", "#main-content input:not([disabled])", null)]
    public async Task FocusIndicator_IsVisible_InForcedColours(string route, string selector, string? areaSelector)
    {
        var page = await OpenAsync(route);
        try
        {
            // Windows High Contrast drops box shadows, which is what most focus rings are drawn with.
            await page.EmulateMediaAsync(new PageEmulateMediaOptions { ForcedColors = ForcedColors.Active });
            var target = page.Locator(selector).First;
            var area = areaSelector is null ? target : target.Locator(areaSelector);

            Assert.True(await FocusChangesPixelsAsync(page, target, area), $"Nothing changes on screen when '{selector}' takes focus in forced colours.");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory]
    [InlineData("switch", "#main-content input[role='switch']:not([disabled])", "xpath=ancestor::label[1]")]
    [InlineData("progress", "#main-content progress", null)]
    [InlineData("slider", "#main-content input[type='range']:not([disabled])", "xpath=..")]
    public async Task Control_IsStillDrawn_InForcedColours(string route, string selector, string? areaSelector)
    {
        var page = await OpenAsync(route);
        try
        {
            await page.EmulateMediaAsync(new PageEmulateMediaOptions { ForcedColors = ForcedColors.Active });
            var target = page.Locator(selector).First;
            var area = areaSelector is null ? target : target.Locator(areaSelector);
            await area.ScrollIntoViewIfNeededAsync();

            // A control painted as one flat colour compresses to almost nothing. Edges and a fill do not.
            var shot = await area.ScreenshotAsync(new LocatorScreenshotOptions { Animations = ScreenshotAnimations.Disabled });
            var colours = await page.EvaluateAsync<int>(
                @"async bytes => {
                    const blob = new Blob([new Uint8Array(bytes)], { type: 'image/png' });
                    const bitmap = await createImageBitmap(blob);
                    const canvas = new OffscreenCanvas(bitmap.width, bitmap.height);
                    const context = canvas.getContext('2d');
                    context.drawImage(bitmap, 0, 0);
                    const data = context.getImageData(0, 0, bitmap.width, bitmap.height).data;
                    const seen = new Set();
                    for (let i = 0; i < data.length; i += 4) seen.add((data[i] << 16) | (data[i + 1] << 8) | data[i + 2]);
                    return seen.size;
                }",
                shot.Select(b => (int)b).ToArray());

            Assert.True(colours >= 2, $"'{selector}' renders as a single flat colour in forced colours.");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
