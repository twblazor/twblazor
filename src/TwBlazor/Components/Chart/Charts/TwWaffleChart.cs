// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Charting;
using TwBlazor.Models;

namespace TwBlazor.Components;

/// <summary>
/// A waffle chart: a ten by ten grid of squares, each worth one percent, colored by the share of each value.
/// It shows parts of a whole in countable units, which is easier to read than angles.
/// </summary>
public class TwWaffleChart : TwValueChartBase
{
    private const int side = 10;

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder)
    {
        var data = GetFoldedData();
        var total = data.Sum(item => item.Value);
        if (total <= 0)
        {
            return;
        }

        builder.Shape = ChartShape.Square;
        var cells = Allocate(data, total);
        var cell = 0;

        for (var i = 0; i < data.Count; i++)
        {
            var color = ChartColor.Series(i);
            var share = FormatShare(data[i].Value, total);
            builder.Legend($"{data[i].Label} ({share})", color);

            if (cells[i] > 0)
            {
                builder.Datum((cell % side + 0.5) * side, (cell / side + 0.5) * side, side, side, data[i].Label,
                    [new ChartDatumRow(ValueHeader, FormatValue(data[i].Value), color), new ChartDatumRow("Share", share)], ChartHover.None);
            }

            for (var k = 0; k < cells[i]; k++, cell++)
            {
                builder.Rect(cell % side * side + 0.7, cell / side * side + 0.7, side - 1.4, side - 1.4, color, ChartCorner.All, gap: false, title: $"{data[i].Label}: {share}");
            }
        }
    }

    /// <summary>
    /// Shares one hundred cells between the values using the largest remainder method, so the grid is always full.
    /// </summary>
    private static int[] Allocate(IReadOnlyList<ChartValue> data, double total)
    {
        var exact = data.Select(item => item.Value / total * side * side).ToArray();
        var cells = exact.Select(value => (int)Math.Floor(value)).ToArray();
        var remaining = side * side - cells.Sum();

        foreach (var index in Enumerable.Range(0, data.Count).OrderByDescending(i => exact[i] - cells[i]).Take(remaining))
        {
            cells[index]++;
        }

        return cells;
    }
}
