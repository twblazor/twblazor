// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines how a number in a <see cref="TwBlazor.Components.TwSpreadsheet"/> cell is shown. Only the display changes: the
/// cell keeps the full number for formulas.
/// </summary>
/// <remarks>Numbers are shown in the culture the app is localized to, so the decimal separator and the currency symbol
/// follow it.</remarks>
public enum SheetNumberFormat
{
    /// <summary>
    /// Shows the number as it is, with up to ten significant digits.
    /// </summary>
    General,

    /// <summary>
    /// Shows the number with two decimal places and thousands separators, such as <c>1,234.50</c>.
    /// </summary>
    Number,

    /// <summary>
    /// Shows the number as an amount of money with the culture's currency symbol, such as <c>£1,234.50</c>.
    /// </summary>
    Currency,

    /// <summary>
    /// Shows the number multiplied by 100 with a percent sign, such as <c>12.50%</c> for 0.125.
    /// </summary>
    Percent,
}
