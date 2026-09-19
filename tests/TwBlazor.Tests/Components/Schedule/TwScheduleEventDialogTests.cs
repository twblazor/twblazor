// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Models;
using TwBlazor.Services;

namespace TwBlazor.Tests.Components.Schedule;

public class TwScheduleEventDialogTests : TwBlazorTestBase
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
        var cut = TestContext.Render<TwScheduleEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.IsNew, true));

        Assert.DoesNotContain("Delete", cut.Markup);
        Assert.Contains("Save", cut.Markup);
        Assert.Contains("Cancel", cut.Markup);
    }

    [Fact]
    public void EditMode_ShowsDeleteButton()
    {
        var cut = TestContext.Render<TwScheduleEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.IsNew, false));

        Assert.Contains("Delete", cut.Markup);
        Assert.Contains("Save", cut.Markup);
    }

    [Fact]
    public void ReadOnlyMode_HidesSaveAndDelete_ShowsCloseInsteadOfCancel()
    {
        var cut = TestContext.Render<TwScheduleEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.IsNew, false)
            .Add(x => x.ReadOnly, true));

        Assert.DoesNotContain("Delete", cut.Markup);
        Assert.DoesNotContain("Save", cut.Markup);
        Assert.Contains("Close", cut.Markup);
        Assert.DoesNotContain("Cancel", cut.Markup);
    }

    [Fact]
    public void ReadOnlyMode_RendersOwnColorNameHeader_InsteadOfFormFields()
    {
        var evt = SampleEvent();
        evt.Color = "#2563eb";

        var cut = TestContext.Render<TwScheduleEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, evt)
            .Add(x => x.IsNew, false)
            .Add(x => x.ReadOnly, true));

        var header = cut.FindComponent<TwScheduleEventDialogHeader>();
        Assert.Equal("Design review", header.Instance.Name);
        Assert.Equal("#2563eb", header.Instance.Color);

        Assert.Empty(cut.FindComponents<TwTextfield<string>>());
        Assert.Empty(cut.FindComponents<TwDateTimePicker>());
        Assert.Empty(cut.FindComponents<TwColorPicker>());
    }

    [Fact]
    public void ReadOnlyMode_ShowsStartAndEndAsFormattedText()
    {
        var cut = TestContext.Render<TwScheduleEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.IsNew, false)
            .Add(x => x.ReadOnly, true));

        Assert.Contains("18 Mar 2026, 10:00 AM", cut.Markup);
        Assert.Contains("18 Mar 2026, 11:00 AM", cut.Markup);
    }

    [Fact]
    public void ReadOnlyMode_HeaderCloseIcon_CancelsDialog()
    {
        var reference = new FakeDialogReference();
        var dialogInstance = new TwDialogInstance(reference);

        var cut = TestContext.Render<TwScheduleEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.IsNew, false)
            .Add(x => x.ReadOnly, true)
            .AddCascadingValue(dialogInstance));

        cut.Find("button[aria-label='Close']").Click();

        Assert.True(reference.ClosedResult?.Canceled);
    }

    [Fact]
    public void EditMode_DoesNotRenderReadOnlyHeader()
    {
        var cut = TestContext.Render<TwScheduleEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.IsNew, false));

        Assert.Empty(cut.FindComponents<TwScheduleEventDialogHeader>());
    }

    [Fact]
    public void ContentTemplate_Renders_WhenProvided()
    {
        var cut = TestContext.Render<TwScheduleEventDialog<string>>(p => p
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
        var cut = TestContext.Render<TwScheduleEventDialog<string>>(p => p
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

        var cut = TestContext.Render<TwScheduleEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, workingEvent)
            .Add(x => x.IsNew, false)
            .AddCascadingValue(dialogInstance));

        var colorTextInput = cut.FindAll("input").First(i => i.GetAttribute("type") == "text" && i.GetAttribute("placeholder") == "#RRGGBB");
        colorTextInput.Change("#2563EB");

        FindButtonByText(cut, "Save").Click();

        var result = Assert.IsType<TwScheduleEventDialogResult<string>>(reference.ClosedResult?.Data);
        Assert.Equal("#2563EB", result.Event.Color);
    }

    [Fact]
    public void Save_ClosesDialog_WithSaveActionAndWorkingEvent()
    {
        var reference = new FakeDialogReference();
        var dialogInstance = new TwDialogInstance(reference);
        var workingEvent = SampleEvent();

        var cut = TestContext.Render<TwScheduleEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, workingEvent)
            .Add(x => x.IsNew, false)
            .AddCascadingValue(dialogInstance));

        FindButtonByText(cut, "Save").Click();

        var result = Assert.IsType<TwScheduleEventDialogResult<string>>(reference.ClosedResult?.Data);
        Assert.Equal(TwScheduleEventDialogAction.Save, result.Action);
        Assert.Same(workingEvent, result.Event);
    }

    [Fact]
    public void Delete_ClosesDialog_WithDeleteAction()
    {
        var reference = new FakeDialogReference();
        var dialogInstance = new TwDialogInstance(reference);
        var workingEvent = SampleEvent();

        var cut = TestContext.Render<TwScheduleEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, workingEvent)
            .Add(x => x.IsNew, false)
            .AddCascadingValue(dialogInstance));

        FindButtonByText(cut, "Delete").Click();

        var result = Assert.IsType<TwScheduleEventDialogResult<string>>(reference.ClosedResult?.Data);
        Assert.Equal(TwScheduleEventDialogAction.Delete, result.Action);
    }

    [Fact]
    public void Cancel_ClosesDialogAsCanceled()
    {
        var reference = new FakeDialogReference();
        var dialogInstance = new TwDialogInstance(reference);

        var cut = TestContext.Render<TwScheduleEventDialog<string>>(p => p
            .Add(x => x.WorkingEvent, SampleEvent())
            .Add(x => x.IsNew, true)
            .AddCascadingValue(dialogInstance));

        FindButtonByText(cut, "Cancel").Click();

        Assert.True(reference.ClosedResult?.Canceled);
    }

    private static AngleSharp.Dom.IElement FindButtonByText(IRenderedComponent<TwScheduleEventDialog<string>> cut, string text) =>
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
