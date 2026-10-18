// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;

namespace TwBlazor.Components;

/// <summary>
/// Defines one or more columns of a <see cref="TwTable"/>, rendered as a <c>&lt;col&gt;</c> element inside the
/// table's <see cref="TwTable.ColumnGroup"/>.
/// </summary>
/// <remarks>A column takes a width without any Tailwind class, so it works from any project. With the default
/// automatic layout the width is the narrowest the column will go, so long or breakable content in other columns
/// cannot squeeze it. With <see cref="TwTable.FixedLayout"/> the width is exact. Columns without a width share
/// whatever space is left.</remarks>
public partial class TwTableColumn : TwBlazorComponentBase
{
    /// <summary>
    /// Gets or sets the width of the column as any CSS length, such as <c>12rem</c>, <c>160px</c> or <c>25%</c>.
    /// Leave it unset to size the column from the remaining space.
    /// </summary>
    [Parameter] public string? Width { get; set; }

    /// <summary>
    /// Gets or sets how many consecutive columns this definition covers. Defaults to <c>1</c>.
    /// </summary>
    [Parameter] public int Span { get; set; } = 1;

    private int? columnSpan => Span > 1 ? Span : null;

    private string? columnClasses => string.IsNullOrWhiteSpace(Class) ? null : Class;

    private string? columnStyle
    {
        get
        {
            var width = string.IsNullOrWhiteSpace(Width) ? string.Empty : $"width:{Width.Trim()};";
            var style = $"{width}{Style}";
            return string.IsNullOrWhiteSpace(style) ? null : style;
        }
    }
}
