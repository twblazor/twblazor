using Microsoft.Playwright;
using TwBlazor.A11yTests.Infrastructure;

namespace TwBlazor.A11yTests;

/// <summary>
/// Drives the real TwCalendar docs page in a browser. bUnit can't catch these: they depend on real
/// hit-testing against the <c>inert</c> state a picker panel puts the rest of the page in.
/// </summary>
[Collection(A11yCollection.Name)]
public class CalendarDialogInteractionTests(A11yFixture fixture)
{
    [Fact]
    public async Task Save_WorksOnFirstClick_WhileAPickerPanelIsOpen()
    {
        var page = await fixture.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1400, Height = 1100 }
        });

        try
        {
            await page.GotoAsync(new Uri(fixture.BaseAddress, "/calendar").ToString());
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // The "Event Details in the Dialog" example is the fourth calendar on the page and starts
            // on Day view, so the sample "Design review" event is on screen.
            var chip = page.Locator("button[aria-label^='Design review']").Nth(3);
            await chip.ScrollIntoViewIfNeededAsync();
            await chip.ClickAsync();

            var dialog = page.Locator("[role='dialog']");
            await dialog.WaitForAsync();

            await dialog.Locator("input[aria-label='Name']").FillAsync("Renamed event");

            // Focusing the Start picker opens its panel, which makes everything outside it inert.
            await dialog.Locator("input[placeholder='Select a datetime']").First.ClickAsync();
            await page.Locator(".datepicker-header").WaitForAsync();

            // One real mouse click on Save, positioned by hand because Playwright's own click helper
            // refuses to click an element that is covered or inert, which is the situation under test.
            var box = await dialog.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).BoundingBoxAsync();
            Assert.NotNull(box);
            await page.Mouse.ClickAsync(box.X + box.Width / 2, box.Y + box.Height / 2);

            await dialog.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
            Assert.True(await page.GetByText("Renamed event").CountAsync() > 0);
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
