// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Calendar;

public enum EventPriority
{
    Low,
    High
}

public class EventDetailsStub
{
    public string? Notes { get; set; }
    public int Attendees { get; set; }
    public int? Capacity { get; set; }
    public decimal Budget { get; set; }
    public bool IsOnline { get; set; }
    public EventPriority Priority { get; set; }
    public DateTime Deadline { get; set; }
    public DateTimeOffset? Reminder { get; set; }
    public string ReadOnlyLabel { get; } = "fixed";
    public List<string> Tags { get; set; } = [];
    public string OrderNumber { get; set; } = string.Empty;
}

public class TwCalendarDialogFieldTests
{
    private static IReadOnlyList<TwCalendarDialogField> Resolve(params (string Name, string Label)[] entries) =>
        TwCalendarDialogField.Resolve(typeof(EventDetailsStub), entries.ToDictionary(e => e.Name, e => e.Label));

    private static TwCalendarDialogField Single(string name, string label = "x") => Assert.Single(Resolve((name, label)));

    [Fact]
    public void Resolve_ReturnsEmpty_WhenNoPropertiesConfigured()
    {
        Assert.Empty(TwCalendarDialogField.Resolve(typeof(EventDetailsStub), null));
        Assert.Empty(TwCalendarDialogField.Resolve(typeof(EventDetailsStub), new Dictionary<string, string>()));
    }

    [Fact]
    public void Resolve_KeepsDictionaryOrder_AndSkipsUnknownNames()
    {
        var fields = Resolve(("Attendees", "Guests"), ("DoesNotExist", "Nope"), ("Notes", "Notes"));

        Assert.Equal(["Attendees", "Notes"], fields.Select(f => f.Name));
        Assert.Equal(["Guests", "Notes"], fields.Select(f => f.Label));
    }

    [Theory]
    [InlineData("OrderNumber", "", "Order number")]
    [InlineData("Notes", "  ", "Notes")]
    [InlineData("IsOnline", "", "Is online")]
    public void Resolve_HumanizesTheName_WhenLabelIsEmpty(string name, string label, string expected)
    {
        Assert.Equal(expected, Single(name, label).Label);
    }

    [Theory]
    [InlineData("Notes", TwCalendarDialogFieldKind.Text)]
    [InlineData("Attendees", TwCalendarDialogFieldKind.Number)]
    [InlineData("Capacity", TwCalendarDialogFieldKind.Number)]
    [InlineData("Budget", TwCalendarDialogFieldKind.Number)]
    [InlineData("IsOnline", TwCalendarDialogFieldKind.Boolean)]
    [InlineData("Priority", TwCalendarDialogFieldKind.Enum)]
    [InlineData("Deadline", TwCalendarDialogFieldKind.DateTime)]
    [InlineData("Reminder", TwCalendarDialogFieldKind.DateTime)]
    [InlineData("Tags", TwCalendarDialogFieldKind.Unsupported)]
    public void Kind_IsDerivedFromThePropertyType(string name, TwCalendarDialogFieldKind expected)
    {
        Assert.Equal(expected, Single(name).Kind);
    }

    [Fact]
    public void CanEdit_IsFalse_ForGetterOnlyAndUnsupportedProperties()
    {
        Assert.False(Single("ReadOnlyLabel").CanEdit);
        Assert.False(Single("Tags").CanEdit);
        Assert.True(Single("Notes").CanEdit);
    }

    [Fact]
    public void EnumNames_ListsTheEnumMembers()
    {
        Assert.Equal(["Low", "High"], Single("Priority").EnumNames);
        Assert.Empty(Single("Notes").EnumNames);
    }

    [Fact]
    public void GetDisplayText_FormatsEachKind()
    {
        var details = new EventDetailsStub
        {
            Notes = "Bring slides",
            IsOnline = true,
            Priority = EventPriority.High,
            Deadline = new DateTime(2026, 10, 1, 9, 5, 0),
            Capacity = null,
            Reminder = new DateTimeOffset(2026, 10, 2, 8, 0, 0, TimeSpan.Zero)
        };

        Assert.Equal("Bring slides", Single("Notes").GetDisplayText(details));
        Assert.Equal("Yes", Single("IsOnline").GetDisplayText(details));
        Assert.Equal("High", Single("Priority").GetDisplayText(details));
        Assert.Equal("1 Oct 2026 09:05", Single("Deadline").GetDisplayText(details));
        Assert.Equal("2 Oct 2026 08:00", Single("Reminder").GetDisplayText(details));
        Assert.Equal(string.Empty, Single("Capacity").GetDisplayText(details));
        Assert.Equal(string.Empty, Single("Notes").GetDisplayText(null));
    }

    [Fact]
    public void GetEditText_UsesInvariantCulture_ForNumbers()
    {
        var details = new EventDetailsStub { Budget = 12.5m };
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            Assert.Equal("12.5", Single("Budget").GetEditText(details));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void SetFromText_SetsText_Numbers_AndEnums()
    {
        var details = new EventDetailsStub();

        Assert.True(Single("Notes").SetFromText(details, "Hello"));
        Assert.True(Single("Attendees").SetFromText(details, "12"));
        Assert.True(Single("Budget").SetFromText(details, "99.95"));
        Assert.True(Single("Priority").SetFromText(details, "High"));

        Assert.Equal("Hello", details.Notes);
        Assert.Equal(12, details.Attendees);
        Assert.Equal(99.95m, details.Budget);
        Assert.Equal(EventPriority.High, details.Priority);
    }

    [Theory]
    [InlineData("Attendees", "abc")]
    [InlineData("Attendees", "99999999999")]
    [InlineData("Priority", "Nope")]
    public void SetFromText_IgnoresUnparsableText(string name, string text)
    {
        var details = new EventDetailsStub { Attendees = 3, Priority = EventPriority.Low };

        Assert.False(Single(name).SetFromText(details, text));
        Assert.Equal(3, details.Attendees);
        Assert.Equal(EventPriority.Low, details.Priority);
    }

    [Fact]
    public void SetFromText_EmptyText_ClearsNullableButLeavesNonNullable()
    {
        var details = new EventDetailsStub { Capacity = 10, Attendees = 4 };

        Assert.True(Single("Capacity").SetFromText(details, ""));
        Assert.False(Single("Attendees").SetFromText(details, ""));

        Assert.Null(details.Capacity);
        Assert.Equal(4, details.Attendees);
    }

    [Fact]
    public void SetFromText_DoesNothing_ForReadOnlyProperty()
    {
        Assert.False(Single("ReadOnlyLabel").SetFromText(new EventDetailsStub(), "changed"));
    }

    [Fact]
    public void SetBool_AndSetDateTime_UpdateTheProperty()
    {
        var details = new EventDetailsStub();

        Single("IsOnline").SetBool(details, true);
        Single("Deadline").SetDateTime(details, new DateTime(2026, 1, 2, 3, 4, 0));
        Single("Reminder").SetDateTime(details, new DateTime(2026, 5, 6, 7, 8, 0));

        Assert.True(details.IsOnline);
        Assert.Equal(new DateTime(2026, 1, 2, 3, 4, 0), details.Deadline);
        Assert.Equal(new DateTime(2026, 5, 6, 7, 8, 0), details.Reminder!.Value.DateTime);
    }

    [Fact]
    public void SetBool_AndSetDateTime_IgnoreFieldsOfAnotherKind()
    {
        var details = new EventDetailsStub { Notes = "keep" };

        Single("Notes").SetBool(details, true);
        Single("Notes").SetDateTime(details, DateTime.Now);

        Assert.Equal("keep", details.Notes);
    }

    [Fact]
    public void GetBool_AndGetDateTime_ReadValues_WithFallbackForNull()
    {
        var details = new EventDetailsStub { IsOnline = true, Deadline = new DateTime(2026, 1, 1) };
        var fallback = new DateTime(2000, 1, 1);

        Assert.True(Single("IsOnline").GetBool(details));
        Assert.False(Single("IsOnline").GetBool(null));
        Assert.Equal(new DateTime(2026, 1, 1), Single("Deadline").GetDateTime(details, fallback));
        Assert.Equal(fallback, Single("Reminder").GetDateTime(details, fallback));
    }

    [Theory]
    [InlineData("Notes", "Notes")]
    [InlineData("OrderNumber", "Order number")]
    [InlineData("URLPath", "URL path")]
    [InlineData("IsOnline", "Is online")]
    public void Humanize_SplitsPascalCaseIntoSentenceCase(string name, string expected)
    {
        Assert.Equal(expected, TwCalendarDialogField.Humanize(name));
    }
}
