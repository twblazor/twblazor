// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;

namespace TwBlazor.Spreadsheet;

/// <summary>
/// The kinds of value a cell or a formula can hold.
/// </summary>
internal enum SheetValueKind
{
    /// <summary>An empty cell.</summary>
    Empty,

    /// <summary>A number.</summary>
    Number,

    /// <summary>Text.</summary>
    Text,

    /// <summary>A true or false value.</summary>
    Boolean,

    /// <summary>An error, such as <c>#DIV/0!</c>.</summary>
    Error,
}

/// <summary>
/// A value held by a cell or worked out by a formula.
/// </summary>
internal readonly record struct SheetValue
{
    /// <summary>Division by zero.</summary>
    public const string DivideByZero = "#DIV/0!";

    /// <summary>A value of the wrong kind, such as text where a number is needed.</summary>
    public const string ValueError = "#VALUE!";

    /// <summary>A reference to a cell or sheet that does not exist.</summary>
    public const string ReferenceError = "#REF!";

    /// <summary>A function or name that is not known.</summary>
    public const string NameError = "#NAME?";

    /// <summary>A number that cannot be worked out, such as the square root of a negative number.</summary>
    public const string NumberError = "#NUM!";

    /// <summary>A value that is not available, such as from <c>NA()</c>.</summary>
    public const string NotAvailable = "#N/A";

    /// <summary>A formula that refers back to its own cell, directly or through other cells.</summary>
    public const string CircularError = "#CIRC!";

    /// <summary>A formula that could not be read.</summary>
    public const string SyntaxError = "#ERROR!";

    private SheetValue(SheetValueKind kind, double number, string? text)
    {
        Kind = kind;
        Number = number;
        Text = text;
    }

    /// <summary>Gets the empty value.</summary>
    public static SheetValue Empty { get; } = new(SheetValueKind.Empty, 0, null);

    /// <summary>Gets the kind of value.</summary>
    public SheetValueKind Kind { get; }

    /// <summary>Gets the number, or 1 and 0 for true and false.</summary>
    public double Number { get; }

    /// <summary>Gets the text, or the error code for an error.</summary>
    public string? Text { get; }

    /// <summary>Gets whether the value is an error.</summary>
    public bool IsError => Kind == SheetValueKind.Error;

    /// <summary>Creates a number, turning infinity and not-a-number into <see cref="NumberError"/>.</summary>
    /// <param name="value">The number.</param>
    /// <returns>The value.</returns>
    public static SheetValue FromNumber(double value) => double.IsFinite(value) ? new(SheetValueKind.Number, value, null) : Error(NumberError);

    /// <summary>Creates a text value.</summary>
    /// <param name="value">The text.</param>
    /// <returns>The value.</returns>
    public static SheetValue FromText(string value) => new(SheetValueKind.Text, 0, value);

    /// <summary>Creates a true or false value.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The value.</returns>
    public static SheetValue FromBoolean(bool value) => new(SheetValueKind.Boolean, value ? 1 : 0, null);

    /// <summary>Creates an error.</summary>
    /// <param name="code">The error code, such as <see cref="DivideByZero"/>.</param>
    /// <returns>The value.</returns>
    public static SheetValue Error(string code) => new(SheetValueKind.Error, 0, code);

    /// <summary>
    /// Turns the value into a number for arithmetic: empty is 0, true is 1, and text must read as a number.
    /// </summary>
    /// <param name="number">The number.</param>
    /// <param name="error">The error to return instead, when the value is not a number.</param>
    /// <returns><see langword="true"/> when the value is a number.</returns>
    public bool TryGetNumber(out double number, out SheetValue error)
    {
        error = default;
        switch (Kind)
        {
            case SheetValueKind.Number:
            case SheetValueKind.Boolean:
                number = Number;
                return true;
            case SheetValueKind.Empty:
                number = 0;
                return true;
            case SheetValueKind.Text when double.TryParse(Text, NumberStyles.Float, CultureInfo.InvariantCulture, out number):
                return true;
            case SheetValueKind.Error:
                number = 0;
                error = this;
                return false;
            default:
                number = 0;
                error = Error(ValueError);
                return false;
        }
    }

    /// <summary>
    /// Turns the value into text for joining: numbers in the invariant culture, and true or false as
    /// <c>TRUE</c> or <c>FALSE</c>.
    /// </summary>
    /// <returns>The text.</returns>
    public string AsText() => Kind switch
    {
        SheetValueKind.Number => Number.ToString("G15", CultureInfo.InvariantCulture),
        SheetValueKind.Boolean => Number != 0 ? "TRUE" : "FALSE",
        SheetValueKind.Empty => string.Empty,
        _ => Text ?? string.Empty,
    };

    /// <summary>
    /// Turns the value into true or false for a condition: a number is true unless it is 0, and the text
    /// <c>TRUE</c> or <c>FALSE</c> is read as such.
    /// </summary>
    /// <param name="value">The condition.</param>
    /// <param name="error">The error to return instead, when the value is not a condition.</param>
    /// <returns><see langword="true"/> when the value is a condition.</returns>
    public bool TryGetBoolean(out bool value, out SheetValue error)
    {
        error = default;
        switch (Kind)
        {
            case SheetValueKind.Boolean:
            case SheetValueKind.Number:
                value = Number != 0;
                return true;
            case SheetValueKind.Empty:
                value = false;
                return true;
            case SheetValueKind.Text when bool.TryParse(Text, out value):
                return true;
            case SheetValueKind.Error:
                value = false;
                error = this;
                return false;
            default:
                value = false;
                error = Error(ValueError);
                return false;
        }
    }
}
