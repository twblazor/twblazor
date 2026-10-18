// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.Numerics;

namespace TwBlazor.Utilities;

/// <summary>
/// Parses, formats, steps and clamps the value of a <see cref="Components.TwNumeric{T}"/>, which can be any built-in
/// numeric type or the nullable form of one.
/// </summary>
/// <typeparam name="T">The value type, such as <see cref="int"/>, <see cref="decimal"/> or <c>double?</c>.</typeparam>
internal abstract class NumericValue<T>
{
    /// <summary>
    /// Gets the instance for <typeparamref name="T"/>.
    /// </summary>
    /// <exception cref="NotSupportedException"><typeparamref name="T"/> is not a supported numeric type.</exception>
    public static NumericValue<T> Instance { get; } = Create();

    /// <summary>
    /// Gets whether <typeparamref name="T"/> is nullable, so the field can be left empty.
    /// </summary>
    public abstract bool IsNullable { get; }

    /// <summary>
    /// Gets whether <typeparamref name="T"/> can hold fractions.
    /// </summary>
    public abstract bool IsFractional { get; }

    /// <summary>
    /// Gets whether <typeparamref name="T"/> can hold negative numbers.
    /// </summary>
    public abstract bool IsSigned { get; }

    /// <summary>
    /// Gets the smallest value <typeparamref name="T"/> can hold.
    /// </summary>
    public abstract T MinValue { get; }

    /// <summary>
    /// Gets the largest value <typeparamref name="T"/> can hold.
    /// </summary>
    public abstract T MaxValue { get; }

    /// <summary>
    /// Gets zero.
    /// </summary>
    public abstract T Zero { get; }

    /// <summary>
    /// Gets one, the default step.
    /// </summary>
    public abstract T One { get; }

    /// <summary>
    /// Gets whether <paramref name="value"/> holds a number, rather than being <see langword="null"/>.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true"/> when the value holds a number.</returns>
    public abstract bool HasValue(T value);

    /// <summary>
    /// Gets whether <paramref name="bound"/> limits the value, rather than being <see langword="null"/> or the limit of the type itself.
    /// </summary>
    /// <param name="bound">The minimum or maximum to check.</param>
    /// <returns><see langword="true"/> when the bound is narrower than the type.</returns>
    public abstract bool IsBound(T bound);

    /// <summary>
    /// Compares two values. <see langword="null"/> sorts before every number.
    /// </summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns>Less than zero when <paramref name="left"/> is smaller, zero when equal, otherwise greater than zero.</returns>
    public abstract int Compare(T left, T right);

    /// <summary>
    /// Parses text the user typed.
    /// </summary>
    /// <param name="text">The text, which must not be empty.</param>
    /// <param name="culture">The culture whose separators and signs the text uses.</param>
    /// <param name="value">The parsed number.</param>
    /// <returns><see langword="true"/> when the text is a number <typeparamref name="T"/> can hold.</returns>
    public abstract bool TryParse(string text, CultureInfo culture, out T value);

    /// <summary>
    /// Formats a value for display.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="format">A .NET numeric format string, such as <c>N2</c>, or <see langword="null"/> for the general format.</param>
    /// <param name="culture">The culture to format in.</param>
    /// <returns>The text, or an empty string for <see langword="null"/>.</returns>
    public abstract string Format(T value, string? format, CultureInfo culture);

    /// <summary>
    /// Formats a value in the invariant culture, for attributes such as <c>aria-valuenow</c>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>The text, or <see langword="null"/> for <see langword="null"/>.</returns>
    public abstract string? FormatInvariant(T value);

    /// <summary>
    /// Clamps a value to a range. A <see langword="null"/> value stays <see langword="null"/>.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The lowest allowed value, or <see langword="null"/> for no limit.</param>
    /// <param name="max">The highest allowed value, or <see langword="null"/> for no limit.</param>
    /// <returns>The value, moved into the range when it falls outside it.</returns>
    public abstract T Clamp(T value, T min, T max);

    /// <summary>
    /// Adds a step to a value a number of times, then clamps the result. A <see langword="null"/> value counts as zero,
    /// and a result too large for the type stops at the range's edge instead of overflowing.
    /// </summary>
    /// <param name="value">The value to step from.</param>
    /// <param name="step">The size of one step.</param>
    /// <param name="steps">How many steps to take. Negative steps go down.</param>
    /// <param name="min">The lowest allowed value, or <see langword="null"/> for no limit.</param>
    /// <param name="max">The highest allowed value, or <see langword="null"/> for no limit.</param>
    /// <returns>The stepped and clamped value.</returns>
    public abstract T Add(T value, T step, int steps, T min, T max);

    private static NumericValue<T> Create()
    {
        var underlying = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        var nullable = underlying != typeof(T);

        return underlying switch
        {
            _ when underlying == typeof(int) => new Number<int>(nullable),
            _ when underlying == typeof(long) => new Number<long>(nullable),
            _ when underlying == typeof(short) => new Number<short>(nullable),
            _ when underlying == typeof(sbyte) => new Number<sbyte>(nullable),
            _ when underlying == typeof(uint) => new Number<uint>(nullable),
            _ when underlying == typeof(ulong) => new Number<ulong>(nullable),
            _ when underlying == typeof(ushort) => new Number<ushort>(nullable),
            _ when underlying == typeof(byte) => new Number<byte>(nullable),
            _ when underlying == typeof(decimal) => new Number<decimal>(nullable),
            _ when underlying == typeof(double) => new Number<double>(nullable),
            _ when underlying == typeof(float) => new Number<float>(nullable),
            _ => throw new NotSupportedException($"TwNumeric does not support {typeof(T)}. Use a built-in integer type, decimal, double or float, or the nullable form of one.")
        };
    }

    /// <summary>
    /// The implementation for one underlying numeric type.
    /// </summary>
    /// <typeparam name="TNumber">The underlying numeric type of <typeparamref name="T"/>.</typeparam>
    private sealed class Number<TNumber>(bool nullable) : NumericValue<T>
        where TNumber : struct, INumber<TNumber>, IMinMaxValue<TNumber>
    {
        // Binary floating point is stepped in decimal where it fits, so 0.1 + 0.2 lands on 0.3 rather than 0.30000000000000004.
        private static readonly bool _isBinaryFloat = typeof(TNumber) == typeof(double) || typeof(TNumber) == typeof(float);

        private static readonly NumberStyles _styles = _isBinaryFloat
            ? NumberStyles.Number | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowExponent
            : NumberStyles.Number | NumberStyles.AllowCurrencySymbol;

        public override bool IsNullable => nullable;

        public override bool IsFractional { get; } = !TNumber.IsZero(TNumber.CreateTruncating(0.5));

        public override bool IsSigned { get; } = TNumber.IsNegative(TNumber.MinValue);

        public override T MinValue => Wrap(TNumber.MinValue);

        public override T MaxValue => Wrap(TNumber.MaxValue);

        public override T Zero => Wrap(TNumber.Zero);

        public override T One => Wrap(TNumber.One);

        public override bool HasValue(T value) => value is TNumber;

        public override bool IsBound(T bound) => bound is TNumber number && number != TNumber.MinValue && number != TNumber.MaxValue;

        public override int Compare(T left, T right) => (left, right) switch
        {
            (TNumber l, TNumber r) => l.CompareTo(r),
            (TNumber, _) => 1,
            (_, TNumber) => -1,
            _ => 0
        };

        public override bool TryParse(string text, CultureInfo culture, out T value)
        {
            if (TNumber.TryParse(text.Trim(), _styles, culture, out var number) && !TNumber.IsNaN(number) && !TNumber.IsInfinity(number))
            {
                value = Wrap(number);
                return true;
            }

            value = default!;
            return false;
        }

        public override string Format(T value, string? format, CultureInfo culture)
        {
            if (value is not TNumber number)
            {
                return string.Empty;
            }

            return number.ToString(string.IsNullOrEmpty(format) ? null : format, culture);
        }

        public override string? FormatInvariant(T value) =>
            value is TNumber number ? number.ToString(null, CultureInfo.InvariantCulture) : null;

        public override T Clamp(T value, T min, T max) => value is TNumber number ? Wrap(Clamp(number, min, max)) : value;

        public override T Add(T value, T step, int steps, T min, T max)
        {
            var start = value is TNumber number ? number : TNumber.Zero;
            var size = step is TNumber stepNumber ? TNumber.Abs(stepNumber) : TNumber.One;
            var (low, high) = GetRange(min, max);

            TNumber result;
            if (steps >= 0)
            {
                result = TryAdd(start, size, steps, out var sum) ? sum : high;
            }
            else
            {
                result = TrySubtract(start, size, -steps, out var difference) ? difference : low;
            }

            return Wrap(Clamp(result, min, max));
        }

        private static TNumber Clamp(TNumber value, T min, T max)
        {
            var (low, high) = GetRange(min, max);
            if (value < low)
            {
                return low;
            }

            return value > high ? high : value;
        }

        private static (TNumber Low, TNumber High) GetRange(T min, T max)
        {
            var low = min is TNumber minNumber ? minNumber : TNumber.MinValue;
            var high = max is TNumber maxNumber ? maxNumber : TNumber.MaxValue;
            return high < low ? (low, low) : (low, high);
        }

        private static bool TryAdd(TNumber start, TNumber size, int steps, out TNumber result)
        {
            if (_isBinaryFloat && TryDecimal(start, size, steps, out result))
            {
                return true;
            }

            try
            {
                result = checked(start + (size * TNumber.CreateChecked(steps)));
                return TNumber.IsFinite(result);
            }
            catch (OverflowException)
            {
                result = default;
                return false;
            }
        }

        private static bool TrySubtract(TNumber start, TNumber size, int steps, out TNumber result)
        {
            if (_isBinaryFloat && TryDecimal(start, size, -steps, out result))
            {
                return true;
            }

            try
            {
                result = checked(start - (size * TNumber.CreateChecked(steps)));
                return TNumber.IsFinite(result);
            }
            catch (OverflowException)
            {
                result = default;
                return false;
            }
        }

        private static bool TryDecimal(TNumber start, TNumber size, int steps, out TNumber result)
        {
            try
            {
                var sum = decimal.CreateChecked(start) + (decimal.CreateChecked(size) * steps);
                result = TNumber.CreateChecked(sum);
                return true;
            }
            catch (OverflowException)
            {
                result = default;
                return false;
            }
        }

        private static T Wrap(TNumber number) => (T)(object)number;
    }
}
