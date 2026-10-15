using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.QrCoding;
using TwBlazor.Services;
using ThemeBase = TwBlazor.Theme.Theme;

namespace TwBlazor.Tests.Components.QrCode;

public class TwQrCodeTests : TwBlazorTestBase
{
    private TwQrCodeTheme theme => Theme.Components.Require<TwQrCodeTheme>();

    [Fact]
    public void TwQrCode_GeneratesId_WhenNotProvided_AndUsesProvidedId()
    {
        Assert.StartsWith("qrcode-", TestContext.Render<TwQrCode>(p => p.Add(x => x.Value, "a")).Find("svg").GetAttribute("id"));
        Assert.Equal("menu", TestContext.Render<TwQrCode>(p => p.Add(x => x.Value, "a").Add(x => x.Id, "menu")).Find("svg").GetAttribute("id"));
    }

    [Fact]
    public void TwQrCode_DrawsTheCode_AsOneBackgroundAndOnePath()
    {
        var cut = TestContext.Render<TwQrCode>(p => p.Add(x => x.Value, "HELLO WORLD"));

        var svg = cut.Find("svg");
        var expected = new TwQrCodeService().GetSvgPath(new TwQrCodeService().Encode("HELLO WORLD"));
        Assert.Equal("0 0 29 29", svg.GetAttribute("viewBox"));
        Assert.Equal("crispEdges", svg.GetAttribute("shape-rendering"));
        Assert.Single(svg.QuerySelectorAll("rect"));
        Assert.Equal(theme.Light, svg.QuerySelector("rect")!.GetAttribute("class"));
        Assert.Equal(theme.Dark, svg.QuerySelector("path")!.GetAttribute("class"));
        Assert.Equal(expected, svg.QuerySelector("path")!.GetAttribute("d"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void TwQrCode_RendersNothing_WithoutAValue(string? value)
    {
        var cut = TestContext.Render<TwQrCode>(p => p.Add(x => x.Value, value));

        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void TwQrCode_RendersNothing_WhenTheValueIsTooLong()
    {
        var cut = TestContext.Render<TwQrCode>(p => p.Add(x => x.Value, new string('a', 1274)).Add(x => x.ErrorCorrection, QrErrorCorrection.High));

        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void TwQrCode_QuietZone_SetsTheMarginInModules_AndIgnoresNegativeValues()
    {
        var cut = TestContext.Render<TwQrCode>(p => p.Add(x => x.Value, "HELLO").Add(x => x.QuietZone, 1));
        Assert.Equal("0 0 23 23", cut.Find("svg").GetAttribute("viewBox"));
        Assert.StartsWith("M1 1h7", cut.Find("path").GetAttribute("d"));

        cut.Render(p => p.Add(x => x.QuietZone, -3));
        Assert.Equal("0 0 21 21", cut.Find("svg").GetAttribute("viewBox"));
        Assert.StartsWith("M0 0h7", cut.Find("path").GetAttribute("d"));
    }

    [Fact]
    public void TwQrCode_ErrorCorrection_ChangesTheCode()
    {
        var value = new string('a', 40);

        var low = TestContext.Render<TwQrCode>(p => p.Add(x => x.Value, value).Add(x => x.ErrorCorrection, QrErrorCorrection.Low));
        var high = TestContext.Render<TwQrCode>(p => p.Add(x => x.Value, value).Add(x => x.ErrorCorrection, QrErrorCorrection.High));

        Assert.Equal("0 0 37 37", low.Find("svg").GetAttribute("viewBox"));
        Assert.Equal("0 0 45 45", high.Find("svg").GetAttribute("viewBox"));
    }

    [Fact]
    public void TwQrCode_BoostErrorCorrection_CanBeTurnedOff()
    {
        var spy = UseSpyService();

        TestContext.Render<TwQrCode>(p => p.Add(x => x.Value, "A").Add(x => x.ErrorCorrection, QrErrorCorrection.Low).Add(x => x.BoostErrorCorrection, false));

        Assert.False(spy.LastOptions!.BoostErrorCorrection);
        Assert.Equal(QrErrorCorrection.Low, spy.LastOptions.ErrorCorrection);
    }

    [Fact]
    public void TwQrCode_OnlyReEncodes_WhenTheCodeWouldChange()
    {
        var spy = UseSpyService();
        var cut = TestContext.Render<TwQrCode>(p => p.Add(x => x.Value, "one"));

        cut.Render(p => p.Add(x => x.Class, "mx-auto").Add(x => x.Size, QrCodeSize.Large));
        Assert.Equal(1, spy.EncodeCount);

        cut.Render(p => p.Add(x => x.Value, "two"));
        cut.Render(p => p.Add(x => x.ErrorCorrection, QrErrorCorrection.High));
        cut.Render(p => p.Add(x => x.QuietZone, 2));
        Assert.Equal(4, spy.EncodeCount);
    }

    [Fact]
    public void TwQrCode_IsAnImage_NamedAfterItsValue_ByDefault()
    {
        var cut = TestContext.Render<TwQrCode>(p => p.Add(x => x.Value, "https://twblazor.com"));

        var svg = cut.Find("svg");
        Assert.Equal("img", svg.GetAttribute("role"));
        Assert.Equal("QR code: https://twblazor.com", svg.GetAttribute("aria-label"));
        Assert.Equal("false", svg.GetAttribute("focusable"));
        Assert.Null(svg.GetAttribute("tabindex"));
    }

    [Fact]
    public void TwQrCode_AriaLabel_ReplacesTheDefaultName()
    {
        var cut = TestContext.Render<TwQrCode>(p => p.Add(x => x.Value, "https://twblazor.com").Add(x => x.AriaLabel, "Scan to open the docs"));

        Assert.Equal("Scan to open the docs", cut.Find("svg").GetAttribute("aria-label"));
    }

    [Fact]
    public void TwQrCode_AriaLabelledBy_TakesOverTheName()
    {
        var cut = TestContext.Render<TwQrCode>(p => p.Add(x => x.Value, "x").Add(x => x.AriaLabelledBy, "caption"));

        var svg = cut.Find("svg");
        Assert.Equal("caption", svg.GetAttribute("aria-labelledby"));
        Assert.Null(svg.GetAttribute("aria-label"));
    }

    [Fact]
    public void TwQrCode_NameIsTranslated()
    {
        using var context = new BunitContext();
        context.Services.AddTwBlazor(options => options.Localization.Culture = CultureInfo.GetCultureInfo("de"), ThemeBase.CreateDefaultTheme());

        var cut = context.Render<TwQrCode>(p => p.Add(x => x.Value, "abc"));

        Assert.Equal("QR-Code: abc", cut.Find("svg").GetAttribute("aria-label"));
    }

    [Theory]
    [InlineData(QrCodeSize.Small)]
    [InlineData(QrCodeSize.Medium)]
    [InlineData(QrCodeSize.Large)]
    [InlineData(QrCodeSize.ExtraLarge)]
    public void TwQrCode_Size_AppliesTheThemeSize(QrCodeSize size)
    {
        var expected = size switch
        {
            QrCodeSize.Small => theme.Small,
            QrCodeSize.Large => theme.Large,
            QrCodeSize.ExtraLarge => theme.ExtraLarge,
            _ => theme.Medium
        };

        var cut = TestContext.Render<TwQrCode>(p => p.Add(x => x.Value, "a").Add(x => x.Size, size));

        Assert.Contains(expected, cut.Find("svg").GetAttribute("class"));
    }

    [Fact]
    public void TwQrCode_KeepsItsColors_InForcedColorsMode()
    {
        var cut = TestContext.Render<TwQrCode>(p => p.Add(x => x.Value, "a"));

        Assert.Contains("forced-color-adjust-none", cut.Find("svg").GetAttribute("class"));
    }

    [Fact]
    public void TwQrCode_AppliesRoundedShadowClassStyleAndExtraAttributes()
    {
        var cut = TestContext.Render<TwQrCode>(p => p
            .Add(x => x.Value, "a")
            .Add(x => x.Rounded, Rounded.Lg)
            .Add(x => x.Shadow, Shadow.Md)
            .Add(x => x.Class, "my-code")
            .Add(x => x.Style, "margin: 1px")
            .AddUnmatched("data-test", "qr"));

        var svg = cut.Find("svg");
        var classes = svg.GetAttribute("class")!;
        Assert.Contains(RoundedBuilder.GetRounded(Rounded.Lg), classes);
        Assert.Contains(ShadowBuilder.GetShadow(Shadow.Md), classes);
        Assert.Contains("my-code", classes);
        Assert.Equal("margin: 1px", svg.GetAttribute("style"));
        Assert.Equal("qr", svg.GetAttribute("data-test"));
    }

    private SpyQrCodeService UseSpyService()
    {
        var spy = new SpyQrCodeService();
        TestContext.Services.AddSingleton<ITwQrCodeService>(spy);
        return spy;
    }

    private sealed class SpyQrCodeService : ITwQrCodeService
    {
        private readonly TwQrCodeService inner = new();

        public int EncodeCount { get; private set; }

        public QrEncodeOptions? LastOptions { get; private set; }

        public QrMatrix Encode(string value, QrEncodeOptions? options = null) => inner.Encode(value, options);

        public QrMatrix Encode(byte[] data, QrEncodeOptions? options = null) => inner.Encode(data, options);

        public bool TryEncode(string? value, QrEncodeOptions? options, [NotNullWhen(true)] out QrMatrix? matrix)
        {
            EncodeCount++;
            LastOptions = options;
            return inner.TryEncode(value, options, out matrix);
        }

        public string GetSvgPath(QrMatrix matrix, int quietZone = 4) => inner.GetSvgPath(matrix, quietZone);

        public string ToSvg(QrMatrix matrix, QrSvgOptions? options = null) => inner.ToSvg(matrix, options);

        public string ToSvgDataUri(QrMatrix matrix, QrSvgOptions? options = null) => inner.ToSvgDataUri(matrix, options);
    }
}
