// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using System.Reflection;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Calendar;

/// <summary>
/// Covers how <see cref="TwCalendarEventDialog{T}"/> renders configured property fields (including the
/// read-only and date kinds) and the time/all-day setters the date pickers drive.
/// </summary>
public class TwCalendarEventDialogFieldsTests : TwBlazorTestBase
{
    private static Schedule<EventDetailsStub> SampleEvent(EventDetailsStub? value = null) => new()
    {
        Name = "Design review",
        DateTimeStart = new DateTimeOffset(2026, 3, 18, 10, 0, 0, TimeSpan.Zero),
        DateTimeEnd = new DateTimeOffset(2026, 3, 18, 11, 0, 0, TimeSpan.Zero),
        Value = value
    };

    private static IReadOnlyList<TwCalendarDialogField> Fields(params string[] names) =>
        TwCalendarDialogField.Resolve(typeof(EventDetailsStub), names.ToDictionary(n => n, n => n));

    private IRenderedComponent<TwCalendarEventDialog<EventDetailsStub>> Render(Schedule<EventDetailsStub> evt, bool readOnly = false, IReadOnlyList<TwCalendarDialogField>? fields = null, Action<Bunit.ComponentParameterCollectionBuilder<TwCalendarEventDialog<EventDetailsStub>>>? more = null) =>
        TestContext.Render<TwCalendarEventDialog<EventDetailsStub>>(p =>
        {
            p.Add(x => x.WorkingEvent, evt)
             .Add(x => x.IsNew, false)
             .Add(x => x.ReadOnly, readOnly);

            if (fields is not null)
            {
                p.Add(x => x.Fields, fields);
            }

            more?.Invoke(p);
        });

    private static T Invoke<T>(object instance, string method, params object[] args) =>
        (T)instance.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(instance, args)!;

    private static void Invoke(object instance, string method, params object[] args) =>
        instance.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(instance, args);

    [Fact]
    public void ReadOnly_RendersTheContentTemplate()
    {
        var cut = Render(SampleEvent(new EventDetailsStub()), readOnly: true, more: p => p
            .Add(x => x.ContentTemplate, evt => builder => builder.AddContent(0, $"Template for {evt.Name}")));

        Assert.Contains("Template for Design review", cut.Markup);
    }

    [Fact]
    public void ReadOnly_ListsOnlyFieldsThatHaveAValue()
    {
        var value = new EventDetailsStub { Notes = "Bring the mockups" };
        var cut = Render(SampleEvent(value), readOnly: true, fields: Fields("Notes", "Capacity"));

        Assert.Contains("Bring the mockups", cut.Markup);
        Assert.DoesNotContain("Capacity", cut.Markup);
    }

    [Fact]
    public void Editing_WithoutAValue_RendersNoFields()
    {
        var cut = Render(SampleEvent(null), fields: Fields("Notes", "Attendees"));

        Assert.DoesNotContain("Notes", cut.Markup);
        Assert.DoesNotContain("Attendees", cut.Markup);
    }

    [Fact]
    public void Editing_ReadOnlyProperty_IsShownAsPlainText()
    {
        var cut = Render(SampleEvent(new EventDetailsStub()), fields: Fields("ReadOnlyLabel"));

        Assert.Contains("ReadOnlyLabel", cut.Markup);
        Assert.Contains("fixed", cut.Markup);
        Assert.DoesNotContain(cut.FindAll("input"), i => i.GetAttribute("value") == "fixed");
    }

    [Fact]
    public void Editing_DateProperty_RendersADateTimePicker()
    {
        var withoutField = Render(SampleEvent(new EventDetailsStub { Deadline = new DateTime(2026, 4, 1, 9, 30, 0) }));
        var withField = Render(SampleEvent(new EventDetailsStub { Deadline = new DateTime(2026, 4, 1, 9, 30, 0) }), fields: Fields("Deadline"));

        Assert.True(withField.FindComponents<TwDateTimePicker>().Count > withoutField.FindComponents<TwDateTimePicker>().Count);
    }

    [Fact]
    public void Editing_DateProperty_WritesThePickedValueBack()
    {
        var value = new EventDetailsStub { Deadline = new DateTime(2026, 4, 1, 9, 30, 0) };
        var cut = Render(SampleEvent(value), fields: Fields("Deadline"));

        var picker = cut.FindComponents<TwDateTimePicker>()[^1];
        cut.InvokeAsync(() => picker.Instance.SelectedDateTimeChanged.InvokeAsync(new DateTime(2026, 5, 2, 8, 0, 0)));

        Assert.Equal(new DateTime(2026, 5, 2, 8, 0, 0), value.Deadline);
    }

    [Fact]
    public void Editing_BooleanProperty_WritesTheSwitchBack()
    {
        var value = new EventDetailsStub { IsOnline = false };
        var cut = Render(SampleEvent(value), fields: Fields("IsOnline"));

        var toggle = cut.FindComponents<TwSwitch<bool>>()[^1];
        cut.InvokeAsync(() => toggle.Instance.ValueChanged.InvokeAsync(true));

        Assert.True(value.IsOnline);
    }

    [Fact]
    public void Mutate_WithoutAValue_DoesNothing()
    {
        var evt = SampleEvent(null);
        var cut = Render(evt);

        var exception = Record.Exception(() => Invoke(cut.Instance, "Mutate", (Action<object>)(_ => throw new InvalidOperationException("should not run"))));

        Assert.Null(exception);
        Assert.Null(evt.Value);
    }

    [Fact]
    public void SetAllDay_WithTheCurrentState_ChangesNothing()
    {
        var evt = SampleEvent(new EventDetailsStub());
        var start = evt.DateTimeStart;
        var end = evt.DateTimeEnd;
        var cut = Render(evt);

        Invoke(cut.Instance, "SetAllDay", false);

        Assert.Equal(start, evt.DateTimeStart);
        Assert.Equal(end, evt.DateTimeEnd);
    }

    [Fact]
    public void SetAllDay_TurnedOn_SnapsToMidnightThroughTheNextMidnight()
    {
        var evt = SampleEvent(new EventDetailsStub());
        var cut = Render(evt);

        Invoke(cut.Instance, "SetAllDay", true);

        Assert.Equal(new DateTime(2026, 3, 18), evt.DateTimeStart.DateTime);
        Assert.Equal(new DateTime(2026, 3, 19), evt.DateTimeEnd.DateTime);
    }

    [Fact]
    public void SetLocalStartAndEnd_KeepTheOffsetOfTheExistingTimes()
    {
        var offset = TimeSpan.FromHours(2);
        var evt = SampleEvent(new EventDetailsStub());
        evt.DateTimeStart = new DateTimeOffset(2026, 3, 18, 10, 0, 0, offset);
        evt.DateTimeEnd = new DateTimeOffset(2026, 3, 18, 11, 0, 0, offset);
        var cut = Render(evt);

        Invoke(cut.Instance, "SetLocalStart", new DateTime(2026, 3, 18, 12, 0, 0));
        Invoke(cut.Instance, "SetLocalEnd", new DateTime(2026, 3, 18, 13, 30, 0));

        Assert.Equal(new DateTimeOffset(2026, 3, 18, 12, 0, 0, offset), evt.DateTimeStart);
        Assert.Equal(new DateTimeOffset(2026, 3, 18, 13, 30, 0, offset), evt.DateTimeEnd);
    }

    [Fact]
    public void ThePickersForStartAndEnd_DriveTheEventTimes()
    {
        var evt = SampleEvent(new EventDetailsStub());
        var cut = Render(evt);

        var pickers = cut.FindComponents<TwDateTimePicker>();
        cut.InvokeAsync(() => pickers[0].Instance.SelectedDateTimeChanged.InvokeAsync(new DateTime(2026, 3, 18, 8, 0, 0)));
        cut.InvokeAsync(() => pickers[1].Instance.SelectedDateTimeChanged.InvokeAsync(new DateTime(2026, 3, 18, 9, 0, 0)));

        Assert.Equal(8, evt.DateTimeStart.Hour);
        Assert.Equal(9, evt.DateTimeEnd.Hour);
    }
}
