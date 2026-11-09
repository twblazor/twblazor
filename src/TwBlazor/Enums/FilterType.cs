// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// The kind of values a column filters, which decides its conditions and the field a person types the value into.
/// </summary>
public enum FilterType
{
    /// <summary>Worked out from the column's values: text, a number, a date, true or false, or an enum.</summary>
    Auto,

    /// <summary>Text, compared ignoring case.</summary>
    Text,

    /// <summary>Numbers of any built in numeric type.</summary>
    Number,

    /// <summary>Dates, compared by day. <see cref="DateTime"/>, <see cref="DateTimeOffset"/> and <see cref="DateOnly"/>.</summary>
    Date,

    /// <summary>True or false values, offered as Yes and No.</summary>
    Boolean,

    /// <summary>A fixed list of values, such as the values of an enum, offered as a list.</summary>
    Enum,
}
