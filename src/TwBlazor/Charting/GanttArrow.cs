// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Charting;

/// <summary>
/// One end of a dependency arrow on a Gantt plot.
/// </summary>
/// <param name="X">Where the end sits across the plot.</param>
/// <param name="Y">Where the end sits down the plot.</param>
/// <param name="Direction">
/// The way the arrow moves at this end: +1 rightwards (leaving a task's end, or arriving at a start), -1 leftwards.
/// </param>
internal readonly record struct GanttArrowEnd(double X, double Y, int Direction);

/// <summary>
/// Routes the dependency arrows of a Gantt chart.
/// </summary>
internal static class GanttArrow
{
    /// <summary>
    /// Routes an arrow with right angles: three segments when the ends face each other, otherwise five, turning
    /// on the line between the two rows so the arrow goes round the task rather than through it.
    /// </summary>
    /// <param name="from">Where the arrow leaves its first task.</param>
    /// <param name="to">Where the arrow arrives at its second task.</param>
    /// <param name="gap">How far the arrow runs straight out of and into a bar before turning.</param>
    /// <param name="rowStep">The height of a row, so a five-segment arrow turns between rows.</param>
    /// <returns>The arrow as SVG path data.</returns>
    public static string Path(GanttArrowEnd from, GanttArrowEnd to, double gap, double rowStep)
    {
        var path = new ChartPathBuilder().MoveTo(from.X, from.Y);
        if (from.Direction != to.Direction)
        {
            var turn = from.Direction > 0 ? Math.Max(from.X, to.X) + gap : Math.Min(from.X, to.X) - gap;
            path.LineTo(turn, from.Y).LineTo(turn, to.Y);
        }
        else if ((to.X - from.X) * from.Direction >= 2 * gap)
        {
            path.LineTo(from.X + from.Direction * gap, from.Y).LineTo(from.X + from.Direction * gap, to.Y);
        }
        else
        {
            var between = from.Y + Math.Sign(to.Y - from.Y) * rowStep / 2;
            path.LineTo(from.X + from.Direction * gap, from.Y)
                .LineTo(from.X + from.Direction * gap, between)
                .LineTo(to.X - to.Direction * gap, between)
                .LineTo(to.X - to.Direction * gap, to.Y);
        }

        return path.LineTo(to.X, to.Y).ToString();
    }
}
