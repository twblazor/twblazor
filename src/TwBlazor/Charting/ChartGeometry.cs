// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using System.Text;
using TwBlazor.Enums;

namespace TwBlazor.Charting;

/// <summary>
/// Builds SVG path data in the plot's percentage coordinate space.
/// </summary>
public sealed class ChartPathBuilder
{
    private readonly StringBuilder _data = new();

    /// <summary>Starts a new subpath at a point.</summary>
    public ChartPathBuilder MoveTo(double x, double y) => Append('M', x, y);

    /// <summary>Draws a straight line to a point.</summary>
    public ChartPathBuilder LineTo(double x, double y) => Append('L', x, y);

    /// <summary>Draws a cubic Bezier curve to a point.</summary>
    public ChartPathBuilder CurveTo(double control1X, double control1Y, double control2X, double control2Y, double x, double y) =>
        Append('C', control1X, control1Y, control2X, control2Y, x, y);

    /// <summary>Draws a circular arc to a point.</summary>
    public ChartPathBuilder ArcTo(double radius, bool largeArc, bool clockwise, double x, double y)
    {
        _data.Append('A').Append(Number(radius)).Append(' ').Append(Number(radius)).Append(" 0 ")
            .Append(largeArc ? '1' : '0').Append(' ').Append(clockwise ? '1' : '0').Append(' ')
            .Append(Number(x)).Append(' ').Append(Number(y));
        return this;
    }

    /// <summary>Closes the current subpath.</summary>
    public ChartPathBuilder Close()
    {
        _data.Append('Z');
        return this;
    }

    /// <summary>
    /// Continues the path from its current point (which must be the first of <paramref name="points"/>) through the
    /// rest of the points, joined according to <paramref name="curve"/>.
    /// </summary>
    public ChartPathBuilder Through(IReadOnlyList<(double X, double Y)> points, ChartCurve curve)
    {
        for (var i = 1; i < points.Count; i++)
        {
            var (x, y) = points[i];
            var previous = points[i - 1];

            if (curve == ChartCurve.Step)
            {
                LineTo(x, previous.Y).LineTo(x, y);
            }
            else if (curve == ChartCurve.Smooth)
            {
                var before = points[Math.Max(i - 2, 0)];
                var after = points[Math.Min(i + 1, points.Count - 1)];
                var low = Math.Min(previous.Y, y);
                var high = Math.Max(previous.Y, y);

                // Clamping the control points to the segment's own range stops the curve overshooting the data.
                CurveTo(
                    previous.X + (x - before.X) / 6, Math.Clamp(previous.Y + (y - before.Y) / 6, low, high),
                    x - (after.X - previous.X) / 6, Math.Clamp(y - (after.Y - previous.Y) / 6, low, high),
                    x, y);
            }
            else
            {
                LineTo(x, y);
            }
        }

        return this;
    }

    /// <inheritdoc />
    public override string ToString() => _data.ToString();

    private ChartPathBuilder Append(char command, params double[] values)
    {
        _data.Append(command);
        for (var i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                _data.Append(' ');
            }

            _data.Append(Number(values[i]));
        }

        return this;
    }

    private static string Number(double value) => Math.Round(value, 2).ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Geometry helpers shared by the charts.
/// </summary>
public static class ChartGeometry
{
    /// <summary>
    /// Gets the point at an angle and distance from a center. Angles are in degrees, clockwise from 12 o'clock.
    /// </summary>
    public static (double X, double Y) Polar(double centerX, double centerY, double radius, double angle)
    {
        var radians = (angle - 90) * Math.PI / 180;
        return (centerX + radius * Math.Cos(radians), centerY + radius * Math.Sin(radians));
    }

    /// <summary>
    /// Builds the path of a ring segment (or a pie wedge when <paramref name="innerRadius"/> is zero) between two
    /// angles. Angles are in degrees, clockwise from 12 o'clock.
    /// </summary>
    public static string Sector(double centerX, double centerY, double innerRadius, double outerRadius, double startAngle, double endAngle)
    {
        // A full circle's start and end points coincide, which SVG draws as nothing, so stop just short.
        endAngle = Math.Min(endAngle, startAngle + 359.99);
        var largeArc = endAngle - startAngle > 180;
        var outerStart = Polar(centerX, centerY, outerRadius, startAngle);
        var outerEnd = Polar(centerX, centerY, outerRadius, endAngle);
        var path = new ChartPathBuilder().MoveTo(outerStart.X, outerStart.Y).ArcTo(outerRadius, largeArc, true, outerEnd.X, outerEnd.Y);

        if (innerRadius <= 0)
        {
            path.LineTo(centerX, centerY);
        }
        else
        {
            var innerEnd = Polar(centerX, centerY, innerRadius, endAngle);
            var innerStart = Polar(centerX, centerY, innerRadius, startAngle);
            path.LineTo(innerEnd.X, innerEnd.Y).ArcTo(innerRadius, largeArc, false, innerStart.X, innerStart.Y);
        }

        return path.Close().ToString();
    }

    /// <summary>
    /// Builds the path of a line through points, joined according to <paramref name="curve"/>.
    /// </summary>
    public static string Line(IReadOnlyList<(double X, double Y)> points, ChartCurve curve = ChartCurve.Linear, bool close = false)
    {
        if (points.Count == 0)
        {
            return string.Empty;
        }

        var path = new ChartPathBuilder().MoveTo(points[0].X, points[0].Y).Through(points, curve);
        return (close ? path.Close() : path).ToString();
    }

    /// <summary>
    /// Gets the value at a percentile of a sorted list, interpolating between neighbors.
    /// </summary>
    /// <param name="sorted">The values in ascending order.</param>
    /// <param name="percentile">The percentile, from 0 to 1.</param>
    public static double Percentile(IReadOnlyList<double> sorted, double percentile)
    {
        if (sorted.Count == 0)
        {
            return 0;
        }

        var position = (sorted.Count - 1) * percentile;
        var lower = (int)Math.Floor(position);
        var upper = Math.Min(lower + 1, sorted.Count - 1);
        return sorted[lower] + (sorted[upper] - sorted[lower]) * (position - lower);
    }
}
