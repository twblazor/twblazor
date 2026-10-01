// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Models;
using TwBlazor.Services;

namespace TwBlazor.Tests.Components.Calendar;

public class TwCalendarEventDialogTests : TwBlazorTestBase
{
    private static Schedule<string> SampleEvent() => new()
    {
        Name = "Design review",
        DateTimeStart = new DateTimeOffset(2026, 3, 18, 10, 0, 0, TimeSpan.Zero),
        DateTimeEnd = new DateTimeOffset(2026, 3, 18, 11, 0, 0, TimeSpan.Zero)
    };

    [Fact]
    public void CreateMode_HidesDeleteButton()
    {
        var cut = TestContext.Render<TwCalendarEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.IsNew, true));

        Assert.DoesNotContain("Delete", cut.Markup);
        Assert.Contains("Save", cut.Markup);
        Assert.Contains("Cancel", cut.Markup);
    }

    [Fact]
    public void EditMode_ShowsDeleteButton()
    {
        var cut = TestContext.Render<TwCalendarEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.IsNew, false));

        Assert.Contains("Delete", cut.Markup);
        Assert.Contains("Save", cut.Markup);
    }

    [Fact]
    public void ReadOnlyMode_HidesSaveAndDelete_ShowsCloseInsteadOfCancel()
    {
        var cut = TestContext.Render<TwCalendarEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.IsNew, false)
            .Add(x => x.ReadOnly, true));

        Assert.DoesNotContain("Delete", cut.Markup);
        Assert.DoesNotContain("Save", cut.Markup);
        Assert.Contains("Close", cut.Markup);
        Assert.DoesNotContain("Cancel", cut.Markup);
    }

    [Fact]
    public void ReadOnlyMode_RendersSummaryCard_InsteadOfFormFields()
    {
        var evt = SampleEvent();
        evt.Color = "#2563eb";

        var cut = TestContext.Render<TwCalendarEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, evt)
            .Add(x => x.IsNew, false)
            .Add(x => x.ReadOnly, true));

        var summary = cut.Find("div[style*='border-left-color:#2563eb']");
        Assert.Contains("background-color:#2563eb26;", summary.GetAttribute("style"));
        Assert.DoesNotContain("color:#2563eb;", summary.GetAttribute("style")!.Replace("border-left-color:#2563eb;", string.Empty));
        Assert.Contains("18 Mar 2026 10:00 - 11:00", summary.TextContent);

        Assert.Empty(cut.FindComponents<TwTextfield<string>>());
        Assert.Empty(cut.FindComponents<TwDateTimePicker>());
        Assert.Empty(cut.FindComponents<TwColorPicker>());
    }

    [Fact]
    public void ReadOnlyMode_SummaryUsesThemeClasses_WhenEventHasNoColor()
    {
        var cut = TestContext.Render<TwCalendarEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.ReadOnly, true));

        var summary = cut.Find($"div.{Theme.Components.Require<TwBlazor.Configuration.Components.TwCalendarTheme>().EventDialogSummary.Split(' ')[0]}");
        Assert.True(string.IsNullOrEmpty(summary.GetAttribute("style")));
    }

    [Fact]
    public void ReadOnlyMode_DoesNotRepeatStartAndEndAsSeparateFields()
    {
        var cut = TestContext.Render<TwCalendarEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.ReadOnly, true));

        Assert.DoesNotContain(">Start<", cut.Markup);
        Assert.DoesNotContain(">End<", cut.Markup);
    }

    [Theory]
    [InlineData(2026, 3, 18, 10, 0, 2026, 3, 18, 11, 0, "18 Mar 2026 10:00 - 11:00")]
    [InlineData(2026, 10, 1, 9, 5, 2026, 10, 1, 17, 45, "1 Oct 2026 09:05 - 17:45")]
    [InlineData(2026, 3, 18, 22, 0, 2026, 3, 19, 1, 0, "18 Mar 2026 22:00 - 19 Mar 2026 01:00")]
    [InlineData(2026, 12, 31, 23, 0, 2027, 1, 1, 0, 30, "31 Dec 2026 23:00 - 1 Jan 2027 00:30")]
    public void FormatRange_CombinesDate_WhenSameDay_AndShowsBothDatesOtherwise(int sy, int sm, int sd, int sh, int smin, int ey, int em, int ed, int eh, int emin, string expected)
    {
        var start = new DateTimeOffset(sy, sm, sd, sh, smin, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(ey, em, ed, eh, emin, 0, TimeSpan.Zero);

        Assert.Equal(expected, TwCalendarEventDialog<string>.FormatRange(start, end));
    }

    [Fact]
    public void ReadOnlyMode_CloseButton_CancelsDialog()
    {
        var reference = new FakeDialogReference();
        var dialogInstance = new TwDialogInstance(reference);

        var cut = TestContext.Render<TwCalendarEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.IsNew, false)
            .Add(x => x.ReadOnly, true)
            .AddCascadingValue(dialogInstance));

        FindButtonByText(cut, "Close").Click();

        Assert.True(reference.ClosedResult?.Canceled);
    }

    [Fact]
    public void EditMode_RendersStartAndEndPickersInOneResponsiveRow()
    {
        var cut = TestContext.Render<TwCalendarEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.IsNew, false));

        var pickers = cut.FindComponents<TwDateTimePicker>();
        Assert.Equal(2, pickers.Count);

        var row = cut.Find("div.grid");
        Assert.Contains("grid-cols-1", row.GetAttribute("class"));
        Assert.Contains("sm:grid-cols-2", row.GetAttribute("class"));
        Assert.Equal(2, row.QuerySelectorAll("input").Count(i => i.GetAttribute("placeholder") == "Select a datetime" || i.GetAttribute("type") is "text" or "datetime-local"));
    }

    [Fact]
    public void EditMode_AccentBar_FollowsTheEventColor()
    {
        var evt = SampleEvent();
        evt.Color = "#2563eb";

        var cut = TestContext.Render<TwCalendarEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, evt)
            .Add(x => x.IsNew, false));

        Assert.Equal("background-color:#2563eb;", cut.Find("div[aria-hidden='true'].h-1").GetAttribute("style"));
    }

    [Fact]
    public void EditMode_AccentBar_UsesDefaultColor_WhenEventHasNone()
    {
        var cut = TestContext.Render<TwCalendarEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.IsNew, true));

        Assert.Equal("background-color:#9333ea;", cut.Find("div[aria-hidden='true'].h-1").GetAttribute("style"));
    }

    [Fact]
    public void EditMode_AccentBar_UpdatesLive_WhenColorChanges()
    {
        var cut = TestContext.Render<TwCalendarEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.IsNew, false));

        cut.FindAll("input").First(i => i.GetAttribute("placeholder") == "#RRGGBB").Change("#2563EB");

        Assert.Equal("background-color:#2563EB;", cut.Find("div[aria-hidden='true'].h-1").GetAttribute("style"));
    }

    [Fact]
    public void EditMode_DoesNotRenderReadOnlySummary()
    {
        var cut = TestContext.Render<TwCalendarEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.IsNew, false));

        Assert.Empty(cut.FindAll("i.bi-calendar-event"));
    }

    [Fact]
    public void ContentTemplate_Renders_WhenProvided()
    {
        var cut = TestContext.Render<TwCalendarEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.IsNew, false)
            .Add(x => x.ContentTemplate, evt => builder =>
            {
                builder.AddContent(0, $"Custom field for {evt.Name}");
            }));

        Assert.Contains("Custom field for Design review", cut.Markup);
    }

    [Fact]
    public void RendersColorPicker_ForEditingTheEventsColor()
    {
        var cut = TestContext.Render<TwCalendarEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.IsNew, false));

        Assert.Contains(cut.FindAll("button"), b => b.GetAttribute("aria-haspopup") == "dialog");
    }

    [Fact]
    public void Save_PreservesThePickedColor()
    {
        var reference = new FakeDialogReference();
        var dialogInstance = new TwDialogInstance(reference);
        var workingEvent = SampleEvent();

        var cut = TestContext.Render<TwCalendarEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, workingEvent)
            .Add(x => x.IsNew, false)
            .AddCascadingValue(dialogInstance));

        var colorTextInput = cut.FindAll("input").First(i => i.GetAttribute("type") == "text" && i.GetAttribute("placeholder") == "#RRGGBB");
        colorTextInput.Change("#2563EB");

        FindButtonByText(cut, "Save").Click();

        var result = Assert.IsType<TwCalendarEventDialogResult<string>>(reference.ClosedResult?.Data);
        Assert.Equal("#2563EB", result.Event.Color);
    }

    [Fact]
    public void Save_ClosesDialog_WithSaveActionAndWorkingEvent()
    {
        var reference = new FakeDialogReference();
        var dialogInstance = new TwDialogInstance(reference);
        var workingEvent = SampleEvent();

        var cut = TestContext.Render<TwCalendarEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, workingEvent)
            .Add(x => x.IsNew, false)
            .AddCascadingValue(dialogInstance));

        FindButtonByText(cut, "Save").Click();

        var result = Assert.IsType<TwCalendarEventDialogResult<string>>(reference.ClosedResult?.Data);
        Assert.Equal(TwCalendarEventDialogAction.Save, result.Action);
        Assert.Same(workingEvent, result.Event);
    }

    [Fact]
    public void Delete_ClosesDialog_WithDeleteAction()
    {
        var reference = new FakeDialogReference();
        var dialogInstance = new TwDialogInstance(reference);
        var workingEvent = SampleEvent();

        var cut = TestContext.Render<TwCalendarEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, workingEvent)
            .Add(x => x.IsNew, false)
            .AddCascadingValue(dialogInstance));

        FindButtonByText(cut, "Delete").Click();

        var result = Assert.IsType<TwCalendarEventDialogResult<string>>(reference.ClosedResult?.Data);
        Assert.Equal(TwCalendarEventDialogAction.Delete, result.Action);
    }

    [Fact]
    public void Cancel_ClosesDialogAsCanceled()
    {
        var reference = new FakeDialogReference();
        var dialogInstance = new TwDialogInstance(reference);

        var cut = TestContext.Render<TwCalendarEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.IsNew, true)
            .AddCascadingValue(dialogInstance));

        FindButtonByText(cut, "Cancel").Click();

        Assert.True(reference.ClosedResult?.Canceled);
    }

    private static AngleSharp.Dom.IElement FindButtonByText(IRenderedComponent<TwCalendarEventDialog<string>> cut, string text) =>
        cut.FindAll("button").First(b => b.TextContent.Trim() == text);

    private sealed class FakeDialogReference : ITwDialogReference
    {
        public TwDialogResult? ClosedResult { get; private set; }

        public Guid Id { get; } = Guid.NewGuid();
        public string? Title => null;
        public TwDialogOptions? Options => null;
        public RenderFragment? RenderFragment { get; set; }
        public Task<TwDialogResult?> Result => Task.FromResult(ClosedResult);
        public TaskCompletionSource<bool> RenderCompleteTaskCompletionSource { get; } = new();
        public object? Dialog => null;

        public void Close() => ClosedResult = TwDialogResult.Ok();
        public void Close(TwDialogResult? result) => ClosedResult = result;
        public bool Dismiss(TwDialogResult? result) => true;
        public void InjectRenderFragment(RenderFragment renderFragment) { }
        public void InjectDialog(object instance) { }
        public void InjectOptions(TwDialogOptions options) { }
        public void InjectTitle(string? title) { }
        public Task<T?> GetReturnValueAsync<T>() => Task.FromResult(ClosedResult is { Canceled: false, Data: T typed } ? typed : default);
    }
}
