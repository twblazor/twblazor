// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TwBlazor.Charting;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// The base class for every chart. A chart type derives from it and overrides <see cref="BuildScene"/> to describe
/// what to draw; this class renders that description and supplies the legend, tooltips, keyboard navigation,
/// empty state and table view.
/// </summary>
/// <remarks>
/// <para>
/// To add a new chart type, derive from this class (or from <see cref="TwCategoryChartBase"/> or
/// <see cref="TwValueChartBase"/> when the data fits), declare its parameters and build its marks with the
/// <see cref="ChartSceneBuilder"/>. No markup, styling or accessibility code is needed in the chart itself.
/// </para>
/// <para>
/// Accessibility: the plot is a <c>group</c> named by the surrounding <see cref="TwChart"/> title. Each data point
/// is an <c>img</c> whose label states its values. The plot is a single tab stop; the arrow, Home and End keys move
/// between data points, and each shows the same tooltip on focus as on hover. The table view presents the same
/// data without the plot.
/// </para>
/// </remarks>
public abstract partial class TwChartBase : TwBlazorComponentBase, IAsyncDisposable
{
    private readonly HashSet<int> _hiddenSeries = [];

    private ChartScene scene = ChartScene.Empty;
    private ElementReference plotRef;
    private ElementReference[] hitRefs = [];
    private int activeIndex;
    private bool showTable;
    private bool keydownGuardRegistered;

    [Inject] private IJSRuntime jsRuntime { get; set; } = null!;

    /// <summary>
    /// Gets or sets the <see cref="TwChart"/> container the chart sits in, if any.
    /// </summary>
    [CascadingParameter] public TwChart? Chart { get; set; }

    /// <summary>
    /// Gets or sets the height of the plot area.
    /// </summary>
    /// <remarks>
    /// If not set, the surrounding <see cref="TwChart"/> decides, falling back to <see cref="ChartHeight.Medium"/>.
    /// </remarks>
    [Parameter] public ChartHeight? Height { get; set; }

    /// <summary>
    /// Gets or sets whether the legend is shown when the chart has one.
    /// </summary>
    /// <remarks>
    /// Default is <see langword="true"/>. A chart with a single series has no legend: its title names it.
    /// </remarks>
    [Parameter] public bool ShowLegend { get; set; } = true;

    /// <summary>
    /// Gets or sets whether clicking a legend entry hides or shows its series.
    /// </summary>
    /// <remarks>
    /// Default is <see langword="true"/>. A series keeps its color while others are hidden.
    /// </remarks>
    [Parameter] public bool InteractiveLegend { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the button that switches between the plot and a table of the same data is shown.
    /// </summary>
    /// <remarks>
    /// Default is <see langword="true"/>. The table is the accessible alternative to the plot, so only turn this
    /// off when the same data is available elsewhere on the page.
    /// </remarks>
    [Parameter] public bool ShowTableToggle { get; set; } = true;

    /// <summary>
    /// Gets or sets the label of the button that switches to the table view.
    /// </summary>
    [Parameter] public string TableViewLabel { get; set; } = "View as table";

    /// <summary>
    /// Gets or sets the label of the button that switches back to the plot.
    /// </summary>
    [Parameter] public string ChartViewLabel { get; set; } = "View as chart";

    /// <summary>
    /// Gets or sets the accessible name of the table view.
    /// </summary>
    [Parameter] public string TableLabel { get; set; } = "Chart data";

    /// <summary>
    /// Gets or sets the accessible name of the legend.
    /// </summary>
    [Parameter] public string LegendLabel { get; set; } = "Legend";

    /// <summary>
    /// Gets or sets the message shown when there is no data to plot.
    /// </summary>
    [Parameter] public string EmptyText { get; set; } = "No data to display";

    /// <summary>
    /// Gets or sets the .NET numeric format string used for values on axes, tooltips and the table view,
    /// for example <c>"C0"</c> or <c>"0.0%"</c>.
    /// </summary>
    /// <remarks>
    /// If not set, values use thousands separators and up to two decimal places. Ignored when
    /// <see cref="ValueFormatter"/> is set.
    /// </remarks>
    [Parameter] public string? ValueFormat { get; set; }

    /// <summary>
    /// Gets or sets a function that formats values, for full control over units and abbreviations.
    /// </summary>
    [Parameter] public Func<double, string>? ValueFormatter { get; set; }

    private TwChartTheme theme => options.Theme.Components.Require<TwChartTheme>();

    private TwTableTheme tableTheme => options.Theme.Components.Require<TwTableTheme>();

    private ChartHeight effectiveHeight => Height ?? Chart?.Height ?? ChartHeight.Medium;

    private string? effectiveAriaLabelledBy => AriaLabelledBy ?? (AriaLabel == null ? Chart?.titleId : null);

    private string? effectiveAriaLabel => AriaLabel ?? (effectiveAriaLabelledBy == null ? "Chart" : null);

    private bool hasLegend => ShowLegend && scene.Legend.Count > 0;

    private bool hasTableToggle => ShowTableToggle && scene.Table != null;

    private bool hasToolbar => hasLegend || hasTableToggle;

    private bool isTableVisible => showTable && hasTableToggle;

    private bool isPlotRendered => !scene.IsEmpty && !isTableVisible;

    private string rootClasses => new ClassBuilder(theme.Chart).AddClass(Class).Build();

    private string plotClasses => new ClassBuilder(theme.Plot)
        .AddClass(effectiveHeight switch
        {
            ChartHeight.Small => theme.Small,
            ChartHeight.Large => theme.Large,
            _ => theme.Medium
        }, scene.Shape == ChartShape.Wide)
        .AddClass(theme.Square, scene.Shape == ChartShape.Square)
        .AddClass(effectiveHeight switch
        {
            ChartHeight.Small => theme.SquareSmall,
            ChartHeight.Large => theme.SquareLarge,
            _ => theme.SquareMedium
        }, scene.Shape == ChartShape.Square)
        .Build();

    private string yTickClasses => $"{theme.Tick} {theme.YTick}";

    private string xTickClasses => $"{theme.Tick} {theme.XTick}";

    private string xAxisTitleClasses => $"{theme.AxisTitle} {theme.XAxisTitle}";

    private string rowHeaderClasses => $"{tableTheme.BodyCellPadding} {theme.TableRowHeader}";

    /// <summary>
    /// Describes what the chart draws by adding marks, data points, axes, legend entries and a table to
    /// <paramref name="builder"/>. Add nothing to show the empty state.
    /// </summary>
    /// <param name="builder">The builder that collects the scene.</param>
    protected internal abstract void BuildScene(ChartSceneBuilder builder);

    /// <summary>
    /// Gets whether the reader has hidden a series by clicking its legend entry.
    /// </summary>
    /// <param name="seriesIndex">The zero-based index of the series.</param>
    protected bool IsSeriesHidden(int seriesIndex) => _hiddenSeries.Contains(seriesIndex);

    /// <summary>
    /// Formats a value for display using <see cref="ValueFormatter"/> or <see cref="ValueFormat"/>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    protected internal string FormatValue(double value) =>
        ValueFormatter?.Invoke(value) ?? value.ToString(ValueFormat ?? "#,0.##", CultureInfo.CurrentCulture);

    /// <summary>
    /// Builds the chart's scene from its current parameters.
    /// </summary>
    internal ChartScene CreateScene()
    {
        var builder = new ChartSceneBuilder();
        BuildScene(builder);
        return builder.Build();
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        RebuildScene();
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!isPlotRendered || scene.Datums.Count == 0)
        {
            keydownGuardRegistered = false;
            return;
        }

        if (keydownGuardRegistered)
        {
            return;
        }

        try
        {
            // Stops the arrow, Home and End keys scrolling the page while they move between data points.
            await jsRuntime.InvokeVoidAsync("twTabs.registerKeydownGuard", plotRef);
            keydownGuardRegistered = true;
        }
        catch (JSDisconnectedException)
        {
            // The circuit disconnected before the script could run; nothing to register.
        }
    }

    /// <summary>
    /// Releases the keydown guard registered for the plot.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (keydownGuardRegistered)
        {
            try
            {
                await jsRuntime.InvokeVoidAsync("twTabs.unregisterKeydownGuard", plotRef);
            }
            catch (JSDisconnectedException)
            {
                // The circuit is already gone; nothing left to clean up.
            }
            catch (InvalidOperationException)
            {
                // JS interop unavailable during teardown (e.g. prerendering); safe to ignore.
            }
        }

        GC.SuppressFinalize(this);
    }

    private void RebuildScene()
    {
        scene = CreateScene();

        if (hitRefs.Length != scene.Datums.Count)
        {
            hitRefs = new ElementReference[scene.Datums.Count];
        }

        activeIndex = Math.Clamp(activeIndex, 0, Math.Max(0, scene.Datums.Count - 1));
    }

    private void ToggleSeries(int seriesIndex)
    {
        if (!_hiddenSeries.Remove(seriesIndex))
        {
            _hiddenSeries.Add(seriesIndex);
        }

        RebuildScene();
    }

    private void ToggleTable() => showTable = !showTable;

    private async Task OnPlotKeyDown(KeyboardEventArgs e)
    {
        var last = scene.Datums.Count - 1;
        if (last < 0)
        {
            return;
        }

        var next = e.Key switch
        {
            "ArrowRight" or "ArrowDown" => Math.Min(activeIndex + 1, last),
            "ArrowLeft" or "ArrowUp" => Math.Max(activeIndex - 1, 0),
            "Home" => 0,
            "End" => last,
            _ => activeIndex
        };

        if (next != activeIndex)
        {
            activeIndex = next;
            await hitRefs[next].FocusAsync();
        }
    }

    private TwChartColor GetColor(ChartColor color) => color.Role switch
    {
        ChartColorRole.Positive => theme.Positive,
        ChartColorRole.Negative => theme.Negative,
        ChartColorRole.Total => theme.Total,
        ChartColorRole.Sequential => theme.Sequential,
        ChartColorRole.Track => theme.Track,
        ChartColorRole.Grid => theme.Grid,
        ChartColorRole.Axis => theme.Axis,
        ChartColorRole.Ink => theme.Ink,
        _ => theme.Series[Math.Min(color.Slot, theme.Series.Count - 1)]
    };

    private string GetLegendItemClasses(ChartLegendItem item, bool interactive) => new ClassBuilder(theme.LegendItem)
        .AddClass(theme.LegendButton, interactive)
        .AddClass(theme.LegendItemHidden, item.Hidden)
        .Build();

    private string GetSwatchClasses(ChartColor color) => $"{theme.LegendSwatch} {GetColor(color).Background}";

    private string GetTooltipKeyClasses(ChartColor color) => $"{theme.TooltipKey} {GetColor(color).Background}";

    private string GetStrokeClasses(ChartStroke stroke) => stroke switch
    {
        ChartStroke.Hairline => theme.StrokeHairline,
        ChartStroke.Thick => theme.StrokeThick,
        _ => theme.StrokeNormal
    };

    private string GetLineClasses(ChartLine line) => $"{GetColor(line.Color).Stroke} {GetStrokeClasses(line.Stroke)}";

    private string GetPathClasses(ChartPath path) => path.Filled
        ? new ClassBuilder(GetColor(path.Color).Fill).AddClass(theme.SurfaceStroke, path.Outline).Build()
        : $"{theme.FillNone} {GetColor(path.Color).Stroke} {GetStrokeClasses(path.Stroke)}";

    private string GetRectClasses(ChartRect rect) => new ClassBuilder(theme.Rect)
        .AddClass(GetColor(rect.Color).Background)
        .AddClass(rect.Corner switch
        {
            ChartCorner.Top => theme.RoundedTop,
            ChartCorner.Bottom => theme.RoundedBottom,
            ChartCorner.Left => theme.RoundedLeft,
            ChartCorner.Right => theme.RoundedRight,
            ChartCorner.All => theme.RoundedAll,
            _ => string.Empty
        })
        .AddClass(theme.SurfaceRing, rect.Gap)
        .Build();

    private string GetDotClasses(ChartDot dot) => $"{theme.Dot} {GetColor(dot.Color).Background}";

    private string GetTextClasses(ChartText text) => new ClassBuilder(theme.Text)
        .AddClass(text.Tone switch
        {
            ChartTextTone.Primary => theme.TextPrimary,
            ChartTextTone.Inverse => theme.TextInverse,
            _ => theme.TextMuted
        })
        .AddClass(theme.TextStrong, text.Strong)
        .AddClass(theme.TextLarge, text.Large)
        .Build();

    private static string Number(double value) => Math.Round(value, 3).ToString(CultureInfo.InvariantCulture);

    private static string? GetOpacityStyle(double opacity) => opacity >= 1 ? null : $"opacity:{Number(opacity)}";

    private static string GetRectStyle(ChartRect rect) =>
        $"left:{Number(rect.X)}%;top:{Number(rect.Y)}%;width:{Number(rect.Width)}%;height:{Number(rect.Height)}%;{GetOpacityStyle(rect.Opacity)}";

    private static string GetDotStyle(ChartDot dot) =>
        $"left:{Number(dot.X)}%;top:{Number(dot.Y)}%;width:{Number(dot.Diameter)}px;height:{Number(dot.Diameter)}px;transform:translate(-50%,-50%);{GetOpacityStyle(dot.Opacity)}";

    private static string GetGlyphStyle(ChartGlyph glyph) =>
        $"left:{Number(glyph.X)}%;top:{Number(glyph.Y)}%;font-size:{Number(glyph.Size)}px;transform:translate(-50%,-50%)";

    private static string GetTextStyle(ChartText text)
    {
        var shiftX = text.Anchor switch { ChartAnchor.Start => 0, ChartAnchor.End => -100, _ => -50 };
        var shiftY = text.Baseline switch { ChartBaseline.Top => 0, ChartBaseline.Bottom => -100, _ => -50 };
        return $"left:{Number(text.X)}%;top:{Number(text.Y)}%;transform:translate(calc({shiftX}% + {Number(text.OffsetX)}px),calc({shiftY}% + {Number(text.OffsetY)}px))";
    }

    // Inline rather than themed: TwTooltip's wrapper is "relative", and an "absolute" class beside it would leave
    // the winner to stylesheet order.
    private static string GetDatumStyle(ChartDatum datum) =>
        $"position:absolute;left:{Number(datum.X)}%;top:{Number(datum.Y)}%;width:{Number(datum.Width)}%;height:{Number(datum.Height)}%;transform:translate(-50%,-50%)";

    private static string GetXTickStyle(ChartAxis axis, ChartTick tick)
    {
        var slot = axis.Ticks.Count > 1 ? Math.Abs(axis.Ticks[1].Position - axis.Ticks[0].Position) : 100;
        return $"left:{Number(tick.Position)}%;max-width:{Number(slot)}%;transform:translateX(-50%)";
    }
}
