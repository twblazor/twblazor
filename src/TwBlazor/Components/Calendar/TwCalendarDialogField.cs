// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.Reflection;
using System.Text;

namespace TwBlazor.Components;

/// <summary>
/// The kind of editor or display a <see cref="TwCalendarDialogField"/> gets in the event dialog,
/// worked out from the type of the property it points at.
/// </summary>
public enum TwCalendarDialogFieldKind
{
    /// <summary>A <see cref="string"/> property, edited with a text field.</summary>
    Text,

    /// <summary>A numeric property (<see cref="int"/>, <see cref="decimal"/>, <see cref="double"/> and so on), edited with a number field.</summary>
    Number,

    /// <summary>A <see cref="bool"/> property, edited with a switch.</summary>
    Boolean,

    /// <summary>A <see cref="DateTime"/> or <see cref="DateTimeOffset"/> property, edited with a date and time picker.</summary>
    DateTime,

    /// <summary>An enum property, edited with a select.</summary>
    Enum,

    /// <summary>Any other type. Shown as text, never editable.</summary>
    Unsupported
}

/// <summary>
/// One property of <see cref="Schedule{T}.Value"/> that the event dialog shows (and, when it has a
/// public setter, edits). Built from <see cref="TwCalendar{T}.DialogProperties"/> via reflection.
/// </summary>
public sealed class TwCalendarDialogField
{
    private readonly PropertyInfo _property;
    private readonly Type _underlyingType;

    private TwCalendarDialogField(PropertyInfo property, string label)
    {
        _property = property;
        Label = label;
        _underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        Kind = GetKind(_underlyingType);
    }

    /// <summary>
    /// The name of the property on <see cref="Schedule{T}.Value"/>.
    /// </summary>
    public string Name => _property.Name;

    /// <summary>
    /// The label shown beside the value.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// How the dialog displays and edits this field.
    /// </summary>
    public TwCalendarDialogFieldKind Kind { get; }

    /// <summary>
    /// Whether the dialog can edit this field: the property needs a public setter and a supported type.
    /// </summary>
    public bool CanEdit => Kind != TwCalendarDialogFieldKind.Unsupported && _property.SetMethod is { IsPublic: true };

    /// <summary>
    /// The names of the enum's members when <see cref="Kind"/> is <see cref="TwCalendarDialogFieldKind.Enum"/>.
    /// </summary>
    public IReadOnlyList<string> EnumNames => Kind == TwCalendarDialogFieldKind.Enum ? System.Enum.GetNames(_underlyingType) : [];

    /// <summary>
    /// Builds the fields for the properties named in <paramref name="properties"/> (property name to
    /// label) on <paramref name="type"/>, in the dictionary's order. Names that don't match a public
    /// readable instance property are skipped. An empty label falls back to the property name split
    /// into words, so <c>OrderNumber</c> shows as "Order number".
    /// </summary>
    public static IReadOnlyList<TwCalendarDialogField> Resolve(Type type, IReadOnlyDictionary<string, string>? properties)
    {
        if (properties is null || properties.Count == 0)
        {
            return [];
        }

        var fields = new List<TwCalendarDialogField>();
        foreach (var (name, label) in properties)
        {
            var info = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (info is { CanRead: true } && info.GetIndexParameters().Length == 0)
            {
                fields.Add(new TwCalendarDialogField(info, string.IsNullOrWhiteSpace(label) ? Humanize(info.Name) : label));
            }
        }

        return fields;
    }

    /// <summary>
    /// Gets the raw property value from <paramref name="target"/>.
    /// </summary>
    public object? GetValue(object? target) => target is null ? null : _property.GetValue(target);

    /// <summary>
    /// Gets the value as display text: dates as <c>d MMM yyyy HH:mm</c>, booleans as Yes/No, an empty
    /// string for <see langword="null"/>.
    /// </summary>
    public string GetDisplayText(object? target) => GetValue(target) switch
    {
        null => string.Empty,
        DateTime date => date.ToString("d MMM yyyy HH:mm", CultureInfo.CurrentCulture),
        DateTimeOffset date => date.DateTime.ToString("d MMM yyyy HH:mm", CultureInfo.CurrentCulture),
        bool flag => flag ? "Yes" : "No",
        IFormattable formattable => formattable.ToString(null, CultureInfo.CurrentCulture),
        var other => other.ToString() ?? string.Empty
    };

    /// <summary>
    /// Gets the value as text for a text or number input. Uses the invariant culture so a number input
    /// always receives a <c>.</c> decimal separator.
    /// </summary>
    public string GetEditText(object? target) => GetValue(target) switch
    {
        null => string.Empty,
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        var other => other.ToString() ?? string.Empty
    };

    /// <summary>
    /// Gets a <see cref="bool"/> property's value, treating <see langword="null"/> as <see langword="false"/>.
    /// </summary>
    public bool GetBool(object? target) => GetValue(target) is true;

    /// <summary>
    /// Gets a date property's value, or <paramref name="fallback"/> when it is <see langword="null"/>.
    /// </summary>
    public DateTime GetDateTime(object? target, DateTime fallback) => GetValue(target) switch
    {
        DateTime date => date,
        DateTimeOffset date => date.DateTime,
        _ => fallback
    };

    /// <summary>
    /// Sets the property from text typed into a text, number or select input. Text that can't be
    /// parsed is ignored, leaving the property unchanged; empty text clears a nullable property.
    /// </summary>
    /// <returns><see langword="true"/> when the property was changed.</returns>
    public bool SetFromText(object target, string? text)
    {
        if (!CanEdit)
        {
            return false;
        }

        if (Kind == TwCalendarDialogFieldKind.Text)
        {
            _property.SetValue(target, text);
            return true;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            if (Nullable.GetUnderlyingType(_property.PropertyType) is null)
            {
                return false;
            }

            _property.SetValue(target, null);
            return true;
        }

        try
        {
            var parsed = Kind == TwCalendarDialogFieldKind.Enum
                ? System.Enum.Parse(_underlyingType, text)
                : Convert.ChangeType(text, _underlyingType, CultureInfo.InvariantCulture);
            _property.SetValue(target, parsed);
            return true;
        }
        catch (Exception ex) when (ex is FormatException or OverflowException or ArgumentException or InvalidCastException)
        {
            return false;
        }
    }

    /// <summary>
    /// Sets a <see cref="bool"/> property.
    /// </summary>
    public void SetBool(object target, bool value)
    {
        if (Kind == TwCalendarDialogFieldKind.Boolean && CanEdit)
        {
            _property.SetValue(target, value);
        }
    }

    /// <summary>
    /// Sets a <see cref="DateTime"/> or <see cref="DateTimeOffset"/> property.
    /// </summary>
    public void SetDateTime(object target, DateTime value)
    {
        if (Kind != TwCalendarDialogFieldKind.DateTime || !CanEdit)
        {
            return;
        }

        if (_underlyingType == typeof(DateTimeOffset))
        {
            _property.SetValue(target, new DateTimeOffset(value));
        }
        else
        {
            _property.SetValue(target, value);
        }
    }

    private static TwCalendarDialogFieldKind GetKind(Type type)
    {
        if (type == typeof(string))
        {
            return TwCalendarDialogFieldKind.Text;
        }

        if (type == typeof(bool))
        {
            return TwCalendarDialogFieldKind.Boolean;
        }

        if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
        {
            return TwCalendarDialogFieldKind.DateTime;
        }

        if (type.IsEnum)
        {
            return TwCalendarDialogFieldKind.Enum;
        }

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Byte or TypeCode.SByte or TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Int32 or TypeCode.UInt32
                or TypeCode.Int64 or TypeCode.UInt64 or TypeCode.Single or TypeCode.Double or TypeCode.Decimal
                => TwCalendarDialogFieldKind.Number,
            _ => TwCalendarDialogFieldKind.Unsupported
        };
    }

    /// <summary>
    /// Splits a PascalCase name into a sentence-case label ("OrderNumber" becomes "Order number").
    /// </summary>
    internal static string Humanize(string name)
    {
        var builder = new StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (i > 0 && char.IsUpper(c) && (char.IsLower(name[i - 1]) || (i + 1 < name.Length && char.IsLower(name[i + 1]))))
            {
                builder.Append(' ');
                builder.Append(char.ToLowerInvariant(c));
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
