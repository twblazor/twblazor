// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Charting;

/// <summary>
/// A linear scale that maps a data value to a percentage (0 to 100) of an axis, with round tick values.
/// </summary>
public sealed class ChartScale
{
    private ChartScale(double min, double max, IReadOnlyList<double> ticks)
    {
        Min = min;
        Max = max;
        Ticks = ticks;
    }

    /// <summary>Gets the value at the start of the axis.</summary>
    public double Min { get; }

    /// <summary>Gets the value at the end of the axis.</summary>
    public double Max { get; }

    /// <summary>Gets the tick values, evenly spaced from <see cref="Min"/> to <see cref="Max"/>.</summary>
    public IReadOnlyList<double> Ticks { get; }

    /// <summary>Maps a data value to its percentage along the axis.</summary>
    /// <param name="value">The data value.</param>
    public double Map(double value) => Max <= Min ? 0 : (value - Min) / (Max - Min) * 100;

    /// <summary>
    /// Creates a scale that covers a range of values and starts, ends and ticks on round numbers.
    /// </summary>
    /// <param name="min">The smallest data value.</param>
    /// <param name="max">The largest data value.</param>
    /// <param name="includeZero">Whether the scale must include zero. Bars need this so their length stays honest.</param>
    /// <param name="tickTarget">Roughly how many intervals to aim for.</param>
    public static ChartScale Nice(double min, double max, bool includeZero = true, int tickTarget = 5)
    {
        if (double.IsNaN(min) || double.IsNaN(max) || double.IsInfinity(min) || double.IsInfinity(max))
        {
            min = 0;
            max = 1;
        }

        if (includeZero)
        {
            min = Math.Min(min, 0);
            max = Math.Max(max, 0);
        }

        if (max <= min)
        {
            var pad = min == 0 ? 1 : Math.Abs(min) * 0.1;
            min -= includeZero ? 0 : pad;
            max += pad;
        }

        var step = NiceStep((max - min) / Math.Max(1, tickTarget));
        var niceMin = Math.Floor(min / step) * step;
        var niceMax = Math.Ceiling(max / step) * step;

        List<double> ticks = [];
        for (var i = 0; niceMin + i * step <= niceMax + step / 2; i++)
        {
            ticks.Add(Math.Round(niceMin + i * step, 10));
        }

        return new ChartScale(niceMin, niceMax, ticks);
    }

    /// <summary>
    /// Creates a scale that covers a set of values. See <see cref="Nice(double, double, bool, int)"/>.
    /// </summary>
    /// <param name="values">The data values.</param>
    /// <param name="includeZero">Whether the scale must include zero.</param>
    public static ChartScale Nice(IEnumerable<double> values, bool includeZero = true)
    {
        var list = values.Where(double.IsFinite).ToList();
        return list.Count == 0 ? Nice(0, 1, includeZero) : Nice(list.Min(), list.Max(), includeZero);
    }

    /// <summary>
    /// Rounds a raw interval up to the nearest 1, 2 or 5 times a power of ten.
    /// </summary>
    /// <param name="raw">The raw interval.</param>
    public static double NiceStep(double raw)
    {
        if (raw <= 0 || !double.IsFinite(raw))
        {
            return 1;
        }

        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(raw)));
        var fraction = raw / magnitude;
        var nice = fraction <= 1 ? 1 : fraction <= 2 ? 2 : fraction <= 5 ? 5 : 10;
        return nice * magnitude;
    }
}

/// <summary>
/// Divides part of an axis into equal slots, one per category.
/// </summary>
/// <param name="Count">The number of slots.</param>
/// <param name="Start">Where the first slot begins, as a percentage of the axis.</param>
/// <param name="End">Where the last slot ends, as a percentage of the axis.</param>
public readonly record struct ChartBand(int Count, double Start = 0, double End = 100)
{
    /// <summary>Gets the size of one slot.</summary>
    public double Step => Count <= 0 ? 0 : (End - Start) / Count;

    /// <summary>Gets the center of a slot.</summary>
    /// <param name="index">The zero-based slot index.</param>
    public double Center(int index) => Start + Step * (index + 0.5);
}
