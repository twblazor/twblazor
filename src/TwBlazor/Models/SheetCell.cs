// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using TwBlazor.Enums;

namespace TwBlazor.Models;

/// <summary>
/// A cell of a <see cref="Sheet"/> in a <see cref="Components.TwSpreadsheet"/> workbook: what was typed into it and how
/// it is formatted.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SheetCell
{
    /// <summary>
    /// Gets or sets what was typed into the cell: a number such as <c>42</c> or <c>12.5%</c>, <c>TRUE</c> or
    /// <c>FALSE</c>, a formula starting with <c>=</c> such as <c>=SUM(A1:A3)</c>, or text. Text that starts with an
    /// apostrophe, such as <c>'0123</c>, is kept as text without the apostrophe.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets whether the cell's text is bold.
    /// </summary>
    public bool Bold { get; set; }

    /// <summary>
    /// Gets or sets whether the cell's text is italic.
    /// </summary>
    public bool Italic { get; set; }

    /// <summary>
    /// Gets or sets whether the cell's text is underlined.
    /// </summary>
    public bool Underline { get; set; }

    /// <summary>
    /// Gets or sets how the content lines up across the cell. Defaults to <see cref="SheetCellAlignment.General"/>.
    /// </summary>
    public SheetCellAlignment Alignment { get; set; }

    /// <summary>
    /// Gets or sets how a number in the cell is shown. Defaults to <see cref="SheetNumberFormat.General"/>.
    /// </summary>
    public SheetNumberFormat Format { get; set; }

    /// <summary>
    /// Gets or sets the cell's fill as a hex colour, such as <c>#FEF3C7</c>, or <see langword="null"/> for none.
    /// </summary>
    /// <remarks>Only hex colours (<c>#RGB</c>, <c>#RRGGBB</c> or <c>#RRGGBBAA</c>) are drawn; anything else is
    /// ignored. Pick a fill that keeps the 4.5:1 contrast ratio with the text.</remarks>
    public string? Background { get; set; }

    /// <summary>
    /// Gets or sets the colour of the cell's text as a hex colour, such as <c>#B91C1C</c>, or <see langword="null"/>
    /// for the theme's text colour.
    /// </summary>
    /// <remarks>Only hex colours are drawn; anything else is ignored.</remarks>
    public string? Color { get; set; }

    /// <summary>
    /// Gets or sets the font family of the cell's text, such as <c>Georgia</c>, or <see langword="null"/> for the
    /// theme's font.
    /// </summary>
    /// <remarks>Letters, digits, spaces, hyphens, commas and quotes are allowed; anything else is ignored.</remarks>
    public string? FontFamily { get; set; }

    /// <summary>
    /// Gets or sets the size of the cell's text in points, such as <c>14</c>, or <see langword="null"/> for the
    /// theme's size. Sizes are kept between 6 and 72.
    /// </summary>
    public double? FontSize { get; set; }
}
