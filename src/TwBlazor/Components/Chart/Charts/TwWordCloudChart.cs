// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Charting;
using TwBlazor.Enums;
using TwBlazor.Models;

namespace TwBlazor.Components;

/// <summary>
/// A word cloud: words sized by their values and gathered around the middle of the plot, for a quick impression
/// of which terms dominate a body of text.
/// </summary>
/// <remarks>
/// The area of a word, not its height, follows its value, so long words are not exaggerated. When there are more
/// words than fit, the smallest are left out of the plot; every word stays in the table view. Negative and zero
/// values are left out.
/// </remarks>
public class TwWordCloudChart : TwValueChartBase
{
    // Font sizes in pixels on a medium plot. They scale with the height of the chart.
    private const double minFontSize = 11;

    private const double maxFontSize = 30;

    // No word is set smaller than this, and the largest is not shrunk below the second size, so the cloud stays
    // readable and its largest words still stand out. Past that point words are left out instead.
    private const double smallestFontSize = 9;

    private const double smallestPeakFontSize = 14;

    private const double lineHeight = 1.25;

    // The width and height of the plot the words may fill, and the gap between neighboring words, as plot percentages.
    private const double available = 94;

    private const double wordGap = 3;

    /// <inheritdoc />
    protected override void BuildMarks(ChartSceneBuilder builder)
    {
        var words = Data.Where(item => double.IsFinite(item.Value) && item.Value > 0 && !string.IsNullOrWhiteSpace(item.Label)).OrderByDescending(item => item.Value).ToList();
        if (words.Count == 0)
        {
            return;
        }

        builder.Shape = ChartShape.Square;
        var side = ChartMetrics.SquareSize(effectiveHeight);

        // The first line holds the largest words. It goes in the middle and the rest alternate below and above it.
        List<List<CloudWord>> lines = [];
        foreach (var (line, index) in Arrange(words, side).Select((line, index) => (line, index)))
        {
            lines.Insert(index % 2 == 0 ? lines.Count : 0, line);
        }

        var y = (100 - lines.Sum(line => GetHeight(line, side))) / 2;
        List<(CloudWord Word, double X, double Y, double Height)> placed = [];
        foreach (var line in lines)
        {
            var height = GetHeight(line, side);
            var x = (100 - line.Sum(word => word.Width) - wordGap * (line.Count - 1)) / 2;
            foreach (var word in line)
            {
                placed.Add((word, x + word.Width / 2, y + height / 2, height));
                x += word.Width + wordGap;
            }

            y += height;
        }

        // Largest first, whatever line a word landed on, so the arrow keys step through the words in order of size.
        foreach (var (word, x, centerY, height) in placed.OrderByDescending(item => item.Word.Item.Value))
        {
            builder.Add(new ChartText(x, centerY, word.Item.Label, Tone: ChartTextTone.Primary, Strong: true, FontSize: word.FontSize, MaxWidth: available));
            builder.Datum(x, centerY, word.Width, height, word.Item.Label, [new ChartDatumRow(effectiveValueHeader, FormatValue(word.Item.Value))]);
        }
    }

    /// <summary>
    /// Breaks the words into lines that fit the width of the plot, shrinking them all until the lines also fit
    /// its height. If they still do not fit at the smallest size, only the lines that fit are kept.
    /// </summary>
    /// <param name="words">The words, largest value first. Never empty.</param>
    /// <param name="side">The side of the plot in pixels.</param>
    private static List<List<CloudWord>> Arrange(List<ChartValue> words, double side)
    {
        for (var shrink = side / ChartMetrics.SquareSize(ChartHeight.Medium); ; shrink *= 0.9)
        {
            List<List<CloudWord>> lines = [[]];
            double used = 0;

            foreach (var word in words)
            {
                // A word too long for the plot at its own size is set as large as fits.
                var fontSize = Math.Max(smallestFontSize, (minFontSize + (maxFontSize - minFontSize) * Math.Sqrt(word.Value / words[0].Value)) * shrink);
                fontSize = Math.Min(fontSize, available / (ChartMetrics.TextWidth(word.Label, 1) / side * 100));
                var width = ChartMetrics.TextWidth(word.Label, fontSize) / side * 100;

                if (lines[^1].Count > 0 && used + wordGap + width > available)
                {
                    lines.Add([]);
                    used = 0;
                }

                used += (lines[^1].Count > 0 ? wordGap : 0) + width;
                lines[^1].Add(new CloudWord(word, fontSize, width));
            }

            if (lines.Sum(line => GetHeight(line, side)) <= available)
            {
                return lines;
            }

            if (maxFontSize * shrink <= smallestPeakFontSize)
            {
                double height = 0;
                return [.. lines.TakeWhile((line, index) => (height += GetHeight(line, side)) <= available || index == 0)];
            }
        }
    }

    private static double GetHeight(List<CloudWord> line, double side) => line.Max(word => word.FontSize) * lineHeight / side * 100;

    /// <summary>
    /// A word with the font size in pixels it is drawn at and the width that takes, as a percentage of the plot.
    /// </summary>
    private readonly record struct CloudWord(ChartValue Item, double FontSize, double Width);
}