// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Enums;

namespace TwBlazor.Charting;

/// <summary>
/// Estimates of the rendered size of a plot and its text, for the layouts that have to keep pixel-sized marks
/// (dots, words, arrowheads) from colliding while positions are percentages.
/// </summary>
/// <remarks>
/// A scene is built without measuring the page, so these mirror the default theme. A layout built on them stays
/// correct at other sizes; it is only less tightly packed.
/// </remarks>
internal static class ChartMetrics
{
    /// <summary>How much wider than tall a <see cref="ChartShape.Wide"/> plot is assumed to be.</summary>
    public const double WideAspect = 2;

    /// <summary>The number of categorical colors, after which charts that color unrelated items start again.</summary>
    public const int SeriesColors = 8;

    private const double characterWidth = 0.6;

    /// <summary>Gets the height in pixels of a <see cref="ChartShape.Wide"/> plot.</summary>
    public static double PlotHeight(ChartHeight height) => height switch
    {
        ChartHeight.Small => 160,
        ChartHeight.Large => 384,
        _ => 256
    };

    /// <summary>Gets the side in pixels of a <see cref="ChartShape.Square"/> plot.</summary>
    public static double SquareSize(ChartHeight height) => height switch
    {
        ChartHeight.Small => 192,
        ChartHeight.Large => 448,
        _ => 288
    };

    /// <summary>Gets the approximate width in pixels of a line of text.</summary>
    public static double TextWidth(string text, double fontSize = 12) => text.Length * fontSize * characterWidth;

    /// <summary>Gets a categorical color for an item, starting again once the colors run out.</summary>
    public static ChartColor Cycle(int index) => ChartColor.Series(index % SeriesColors);
}