// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Charting;
using TwBlazor.Models;

namespace TwBlazor.Components;

/// <summary>
/// A treemap: rectangles that tile the plot, each sized by its value, for showing many parts of a whole in a
/// compact space.
/// </summary>
/// <remarks>
/// Tiles are laid out largest first and kept as close to square as possible. Negative and zero values are left out.
/// </remarks>
public class TwTreemapChart : TwValueChartBase
{
    // The plot is wider than it is tall, so tiles are laid out in a 2:1 box and squeezed back to plot percentages.
    private const double aspect = 2;

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder)
    {
        var data = Data.Where(item => item.Value > 0).OrderByDescending(item => item.Value).ToList();
        var total = data.Sum(item => item.Value);
        if (total <= 0)
        {
            return;
        }

        var color = ChartColor.Series(0);
        foreach (var (item, x, y, width, height) in Squarify(data, total))
        {
            builder.Rect(x, y, width, height, color, ChartCorner.All);

            if (width > 14 && height > 14)
            {
                builder.Add(new ChartText(x, y, item.Label, ChartAnchor.Start, ChartBaseline.Top, ChartTextTone.Inverse, 8, 8, Strong: true));
            }

            builder.Datum(x + width / 2, y + height / 2, width, height, item.Label,
                [new ChartDatumRow(ValueHeader, FormatValue(item.Value), color), new ChartDatumRow("Share", FormatShare(item.Value, total))]);
        }
    }

    private static IEnumerable<(ChartValue Item, double X, double Y, double Width, double Height)> Squarify(List<ChartValue> data, double total)
    {
        double x = 0, y = 0, width = 100 * aspect, height = 100;
        var areaPerUnit = width * height / total;
        var index = 0;

        while (index < data.Count)
        {
            var shortSide = Math.Min(width, height);
            List<double> row = [data[index].Value * areaPerUnit];

            while (index + row.Count < data.Count)
            {
                var candidate = data[index + row.Count].Value * areaPerUnit;
                if (WorstRatio([.. row, candidate], shortSide) > WorstRatio(row, shortSide))
                {
                    break;
                }

                row.Add(candidate);
            }

            var depth = row.Sum() / shortSide;
            double offset = 0;

            foreach (var area in row)
            {
                var length = area / depth;
                yield return width >= height
                    ? (data[index], x / aspect, y + offset, depth / aspect, length)
                    : (data[index], (x + offset) / aspect, y, length / aspect, depth);
                offset += length;
                index++;
            }

            if (width >= height)
            {
                x += depth;
                width -= depth;
            }
            else
            {
                y += depth;
                height -= depth;
            }
        }
    }

    private static double WorstRatio(List<double> row, double side)
    {
        var sum = row.Sum();
        return Math.Max(side * side * row.Max() / (sum * sum), sum * sum / (side * side * row.Min()));
    }
}
