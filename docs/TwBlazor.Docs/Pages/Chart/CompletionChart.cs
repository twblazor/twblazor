using TwBlazor.Charting;
using TwBlazor.Components;

namespace TwBlazor.Docs.Pages.Chart;

#region CodeExample ChartCustomCode
/// <summary>
/// A custom chart: each value is drawn as a filled track out of 100.
/// </summary>
public class CompletionChart : TwValueChartBase
{
    protected override void BuildMarks(ChartSceneBuilder builder)
    {
        builder.Horizontal = true;
        var band = new ChartBand(Data.Count);
        var color = ChartColor.Series(0);
        builder.BandAxis([.. Data.Select(item => item.Label)], band);

        for (var i = 0; i < Data.Count; i++)
        {
            var filled = Math.Clamp(Data[i].Value, 0, 100);
            builder.Bar(band.Center(i), 6, 0, 100, ChartColor.Track, ChartCorner.All, gap: false);
            builder.Bar(band.Center(i), 6, 0, filled, color, ChartCorner.All, gap: false);
            builder.BandDatum(band.Center(i), band.Step, Data[i].Label,
                [new ChartDatumRow(ValueHeader, FormatValue(Data[i].Value), color)]);
        }
    }
}
#endregion
