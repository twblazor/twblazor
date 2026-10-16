// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Calendar;

/// <summary>
/// Opens the real dialog through <see cref="TwCalendar{T}"/> and <see cref="TwDialogProvider"/> to check
/// how <see cref="TwCalendar{T}.DialogProperties"/> flows into it.
/// </summary>
public class TwCalendarDialogPropertiesTests : TwBlazorTestBase
{
    private static readonly DateTime day = new(2026, 3, 18);

    private static readonly Dictionary<string, string> properties = new()
    {
        ["Notes"] = "Notes",
        ["Attendees"] = "Guests",
        ["IsOnline"] = "Online meeting",
        ["Priority"] = "Priority",
        ["Missing"] = "Ignored"
    };

    private static Schedule<EventDetailsStub> Event(bool readOnly = false) => new()
    {
        Name = "Design review",
        DateTimeStart = new DateTimeOffset(day.AddHours(10), TimeSpan.Zero),
        DateTimeEnd = new DateTimeOffset(day.AddHours(11), TimeSpan.Zero),
        ReadOnly = readOnly,
        Value = new EventDetailsStub { Notes = "Bring the mockups", Attendees = 4, IsOnline = true, Priority = EventPriority.High }
    };

    private IRenderedComponent<TwDialogProvider> RenderProvider() => TestContext.Render<TwDialogProvider>();

    private IRenderedComponent<TwCalendar<EventDetailsStub>> RenderCalendar(
        List<Schedule<EventDetailsStub>> schedules,
        bool editable = true,
        Dictionary<string, string>? dialogProperties = null,
        Action<List<Schedule<EventDetailsStub>>>? onChanged = null) =>
        TestContext.Render<TwCalendar<EventDetailsStub>>(p =>
        {
            p.Add(x => x.SelectedDate, day)
             .Add(x => x.View, TwCalendarView.Day)
             .Add(x => x.Editable, editable)
             .Add(x => x.DialogProperties, dialogProperties ?? properties)
             .Add(x => x.Schedules, schedules);

            if (onChanged is not null)
            {
                p.Add(x => x.SchedulesChanged, EventCallback.Factory.Create<List<Schedule<EventDetailsStub>>>(this, onChanged));
            }
        });

    private static void OpenEvent(IRenderedComponent<TwCalendar<EventDetailsStub>> calendar) =>
        calendar.Find("button[aria-label^='Design review']").Click();

    private static async Task<AngleSharp.Dom.IElement> DialogButtonAsync(IRenderedComponent<TwDialogProvider> provider, string text) =>
        (await provider.WaitForElementAsync("[role='dialog']")).QuerySelectorAll("button").First(b => b.TextContent.Trim() == text);

    [Fact]
    public async Task ReadOnlyDialog_UsesTheStandardTitle_AndListsConfiguredPropertyValues()
    {
        var provider = RenderProvider();
        var calendar = RenderCalendar([Event(readOnly: true)]);

        OpenEvent(calendar);

        var dialog = await provider.WaitForElementAsync("[role='dialog']");
        Assert.Contains("Design review", dialog.QuerySelector("h2")!.TextContent);
        Assert.Contains("18 Mar 2026 10:00 - 11:00", dialog.TextContent);
        Assert.Contains("Notes", dialog.TextContent);
        Assert.Contains("Bring the mockups", dialog.TextContent);
        Assert.Contains("Guests", dialog.TextContent);
        Assert.Contains("Online meeting", dialog.TextContent);
        Assert.Contains("Yes", dialog.TextContent);
        Assert.Contains("High", dialog.TextContent);
        Assert.DoesNotContain("Ignored", dialog.TextContent);
        Assert.Empty(dialog.QuerySelectorAll("input"));
    }

    [Fact]
    public async Task ReadOnlyDialog_HidesPropertiesWithNoValue()
    {
        var evt = Event(readOnly: true);
        evt.Value!.Notes = null;
        var provider = RenderProvider();
        var calendar = RenderCalendar([evt]);

        OpenEvent(calendar);

        var dialog = await provider.WaitForElementAsync("[role='dialog']");
        Assert.DoesNotContain(">Notes<", dialog.InnerHtml);
        Assert.Contains("Guests", dialog.TextContent);
    }

    [Fact]
    public async Task ReadOnlyDialog_IsUsedForEveryEvent_WhenCalendarIsNotEditable()
    {
        var provider = RenderProvider();
        var calendar = RenderCalendar([Event()], editable: false);

        OpenEvent(calendar);

        var dialog = await provider.WaitForElementAsync("[role='dialog']");
        Assert.Empty(dialog.QuerySelectorAll("input"));
        Assert.DoesNotContain(dialog.QuerySelectorAll("button"), b => b.TextContent.Trim() == "Save");
    }

    [Fact]
    public async Task EditDialog_ShowsAnEditorForEachConfiguredProperty()
    {
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(true);
        var provider = RenderProvider();
        var calendar = RenderCalendar([Event()]);

        OpenEvent(calendar);

        var dialog = await provider.WaitForElementAsync("[role='dialog']");
        Assert.Contains("Edit event", dialog.QuerySelector("h2")!.TextContent);
        Assert.Contains(dialog.QuerySelectorAll("label"), l => l.TextContent.Contains("Notes"));
        Assert.Contains(dialog.QuerySelectorAll("label"), l => l.TextContent.Contains("Guests"));
        Assert.Contains(dialog.QuerySelectorAll("label"), l => l.TextContent.Contains("Online meeting"));
        Assert.Contains(dialog.QuerySelectorAll("label"), l => l.TextContent.Contains("Priority"));
        Assert.DoesNotContain("Ignored", dialog.TextContent);

        Assert.Equal("Bring the mockups", dialog.QuerySelectorAll("input").First(i => i.GetAttribute("value") == "Bring the mockups").GetAttribute("value"));
        Assert.Equal("number", dialog.QuerySelectorAll("input").First(i => i.GetAttribute("value") == "4").GetAttribute("type"));
        Assert.NotNull(dialog.QuerySelector("select"));
    }

    [Fact]
    public async Task EditDialog_Save_AppliesPropertyEditsToTheEvent()
    {
        TestContext.JSInterop.Setup<bool>("twDevice.prefersNativePicker").SetResult(true);
        List<Schedule<EventDetailsStub>>? saved = null;
        var provider = RenderProvider();
        var calendar = RenderCalendar([Event()], onChanged: list => saved = list);

        OpenEvent(calendar);

        var dialog = await provider.WaitForElementAsync("[role='dialog']");
        dialog.QuerySelectorAll("input").First(i => i.GetAttribute("value") == "Bring the mockups").Change("Bring the mockups and a laptop");
        dialog.QuerySelectorAll("input").First(i => i.GetAttribute("value") == "4").Change("9");
        dialog.QuerySelector("select")!.Change("1"); // option values are 1-based indexes: Low = 1
        (await DialogButtonAsync(provider, "Save")).Click();

        await provider.WaitForAssertionAsync(() => Assert.NotNull(saved));
        var value = Assert.Single(saved!).Value!;
        Assert.Equal("Bring the mockups and a laptop", value.Notes);
        Assert.Equal(9, value.Attendees);
        Assert.Equal(EventPriority.Low, value.Priority);
    }

    [Fact]
    public async Task EditDialog_Cancel_LeavesTheOriginalValueUntouched()
    {
        var original = Event();
        var provider = RenderProvider();
        var calendar = RenderCalendar([original]);

        OpenEvent(calendar);

        var dialog = await provider.WaitForElementAsync("[role='dialog']");
        dialog.QuerySelectorAll("input").First(i => i.GetAttribute("value") == "Bring the mockups").Change("Changed");
        (await DialogButtonAsync(provider, "Cancel")).Click();

        await provider.WaitForAssertionAsync(() => Assert.Empty(provider.FindAll("[role='dialog']")));
        Assert.Equal("Bring the mockups", original.Value!.Notes);
    }

    [Fact]
    public async Task NewEvent_GetsABlankValue_SoPropertiesAreEditable()
    {
        List<Schedule<EventDetailsStub>>? saved = null;
        var provider = RenderProvider();
        var calendar = RenderCalendar([], onChanged: list => saved = list);

        calendar.Find("button[aria-label='09:00']").Click();

        var dialog = await provider.WaitForElementAsync("[role='dialog']");
        Assert.Contains("New event", dialog.QuerySelector("h2")!.TextContent);
        Assert.Contains(dialog.QuerySelectorAll("label"), l => l.TextContent.Contains("Notes"));

        dialog.QuerySelectorAll("input").First(i => i.GetAttribute("aria-label") == "Name").Change("Planning");
        dialog.QuerySelectorAll("input").First(i => i.GetAttribute("type") == "text" && i.ParentElement!.TextContent.Contains("Notes")).Change("Agenda attached");
        (await DialogButtonAsync(provider, "Save")).Click();

        await provider.WaitForAssertionAsync(() => Assert.NotNull(saved));
        var created = Assert.Single(saved!);
        Assert.Equal("Planning", created.Name);
        Assert.Equal("Agenda attached", created.Value!.Notes);
    }

    [Fact]
    public async Task NoDialogProperties_ShowsOnlyTheBuiltInFields()
    {
        var provider = RenderProvider();
        var calendar = RenderCalendar([Event()], dialogProperties: []);

        OpenEvent(calendar);

        var dialog = await provider.WaitForElementAsync("[role='dialog']");
        Assert.DoesNotContain(dialog.QuerySelectorAll("label"), l => l.TextContent.Contains("Notes"));
        Assert.Contains(dialog.QuerySelectorAll("label"), l => l.TextContent.Contains("Name"));
    }

    [Fact]
    public async Task ShallowCopy_IsMadeForEditing_EvenWithoutDialogProperties()
    {
        var original = Event();
        var provider = RenderProvider();
        var calendar = RenderCalendar([original], dialogProperties: []);

        OpenEvent(calendar);
        (await DialogButtonAsync(provider, "Save")).Click();

        await provider.WaitForAssertionAsync(() => Assert.Empty(provider.FindAll("[role='dialog']")));
        Assert.Equal("Bring the mockups", original.Value!.Notes);
    }
}
