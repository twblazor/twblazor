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
public partial class TwTableColumn : TwBlazorComponentBase, IDisposable
{
    private string? resizedWidth;
    private string? lastWidthParameter;

    [CascadingParameter] private TwTable? table { get; set; }

    /// <summary>
    /// Gets or sets whether people can resize this column when the table's <see cref="TwTable.ResizableColumns"/> is
    /// set. Defaults to <c>true</c>. A column that spans several columns is never resizable.
    /// </summary>
    [Parameter] public bool Resizable { get; set; } = true;

    /// <summary>
    /// Gets or sets the column's name for assistive technology, used to name its resize border ("Resize Price").
    /// Set it to the column's heading.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the narrowest a person can make the column by resizing it, in pixels. Defaults to <c>48</c>.
    /// </summary>
    [Parameter] public int MinWidth { get; set; } = 48;

    /// <summary>
    /// Gets or sets the widest a person can make the column by resizing it, in pixels. Defaults to <c>2000</c>.
    /// </summary>
    [Parameter] public int MaxWidth { get; set; } = 2000;

    /// <summary>
    /// Raised with the column's new width, such as <c>180px</c>, after a person resizes it. Bind it with
    /// <c>@bind-Width</c> to keep the width.
    /// </summary>
    [Parameter] public EventCallback<string> WidthChanged { get; set; }

    private bool isResizable => table is { ResizableColumns: true } && Resizable && Span <= 1;

    private string? effectiveWidth => resizedWidth ?? Width;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // A width the parent passes in replaces one a person dragged to.
        if (Width != lastWidthParameter)
        {
            lastWidthParameter = Width;
            resizedWidth = null;
        }

        table?.RegisterColumn(this);
    }

    /// <summary>
    /// Takes the width a person resized the column to.
    /// </summary>
    internal async Task ApplyResizedWidthAsync(string width)
    {
        if (width == effectiveWidth)
        {
            return;
        }

        resizedWidth = width;
        StateHasChanged();
        await WidthChanged.InvokeAsync(width);
    }

    /// <summary>
    /// Leaves the table, so it stops applying widths to this column.
    /// </summary>
    public void Dispose()
    {
        table?.UnregisterColumn(this);
        GC.SuppressFinalize(this);
    }

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
            var width = string.IsNullOrWhiteSpace(effectiveWidth) ? string.Empty : $"width:{effectiveWidth.Trim()};";
            var style = $"{width}{Style}";
            return string.IsNullOrWhiteSpace(style) ? null : style;
        }
    }
}
