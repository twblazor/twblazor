using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Barcoding;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Services;
using ThemeBase = TwBlazor.Theme.Theme;

namespace TwBlazor.Tests.Components.Barcode;

public class TwBarcodeTests : TwBlazorTestBase
{
    private TwBarcodeTheme theme => Theme.Components.Require<TwBarcodeTheme>();

    [Fact]
    public void TwBarcode_GeneratesId_WhenNotProvided_AndUsesProvidedId()
    {
        Assert.StartsWith("barcode-", TestContext.Render<TwBarcode>(p => p.Add(x => x.Value, "a")).Find("div").GetAttribute("id"));
        Assert.Equal("ticket", TestContext.Render<TwBarcode>(p => p.Add(x => x.Value, "a").Add(x => x.Id, "ticket")).Find("div").GetAttribute("id"));
    }

    [Fact]
    public void TwBarcode_DrawsTheBars_AsOneBackgroundAndOnePath_WithTheText()
    {
        var cut = TestContext.Render<TwBarcode>(p => p.Add(x => x.Value, "5012345678900").Add(x => x.Format, BarcodeFormat.Ean13));

        var service = new TwBarcodeService();
        var svg = cut.Find("svg");
        Assert.Equal("0 0 117 50", svg.GetAttribute("viewBox"));
        Assert.Equal("true", svg.GetAttribute("aria-hidden"));
        Assert.Equal("false", svg.GetAttribute("focusable"));
        Assert.Equal(theme.Light, svg.QuerySelector("rect")!.GetAttribute("class"));
        Assert.Equal(theme.Dark, svg.QuerySelector("path")!.GetAttribute("class"));
        Assert.Equal(service.GetSvgPath(service.Encode("5012345678900", BarcodeFormat.Ean13)), svg.QuerySelector("path")!.GetAttribute("d"));
        Assert.Equal("5012345678900", cut.Find("span").TextContent);
        Assert.Equal(theme.Text, cut.Find("span").GetAttribute("class"));
    }

    [Fact]
    public void TwBarcode_ShowsTheCheckDigit_ItAdded()
    {
        var cut = TestContext.Render<TwBarcode>(p => p.Add(x => x.Value, "400638133393").Add(x => x.Format, BarcodeFormat.Ean13));

        Assert.Equal("4006381333931", cut.Find("span").TextContent);
        Assert.Equal("Barcode: 4006381333931", cut.Find("div").GetAttribute("aria-label"));
    }

    [Fact]
    public void TwBarcode_ShowText_CanHideTheText()
    {
        var cut = TestContext.Render<TwBarcode>(p => p.Add(x => x.Value, "a").Add(x => x.ShowText, false));

        Assert.Empty(cut.FindAll("span"));
    }

    [Theory]
    [InlineData(null, BarcodeFormat.Code128)]
    [InlineData("", BarcodeFormat.Code128)]
    [InlineData("lower", BarcodeFormat.Code39)]
    [InlineData("123", BarcodeFormat.Ean13)]
    public void TwBarcode_RendersNothing_WhenTheValueIsMissingOrUnsuitable(string? value, BarcodeFormat format)
    {
        var cut = TestContext.Render<TwBarcode>(p => p.Add(x => x.Value, value).Add(x => x.Format, format));

        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void TwBarcode_QuietZone_OverridesTheFormatsMargin_AndIgnoresNegativeValues()
    {
        var cut = TestContext.Render<TwBarcode>(p => p.Add(x => x.Value, "1234567890").Add(x => x.QuietZone, 2));
        Assert.Equal("0 0 94 50", cut.Find("svg").GetAttribute("viewBox"));
        Assert.StartsWith("M2 0h2", cut.Find("path").GetAttribute("d"));

        cut.Render(p => p.Add(x => x.QuietZone, -5));
        Assert.Equal("0 0 90 50", cut.Find("svg").GetAttribute("viewBox"));
    }

    [Fact]
    public void TwBarcode_OnlyReEncodes_WhenTheBarcodeWouldChange()
    {
        var spy = new SpyBarcodeService();
        TestContext.Services.AddSingleton<ITwBarcodeService>(spy);
        var cut = TestContext.Render<TwBarcode>(p => p.Add(x => x.Value, "ONE"));

        cut.Render(p => p.Add(x => x.Size, BarcodeSize.Large).Add(x => x.ShowText, false).Add(x => x.Class, "mx-auto"));
        Assert.Equal(1, spy.EncodeCount);

        cut.Render(p => p.Add(x => x.Value, "TWO"));
        cut.Render(p => p.Add(x => x.Format, BarcodeFormat.Code39));
        cut.Render(p => p.Add(x => x.QuietZone, 4));
        Assert.Equal(4, spy.EncodeCount);
    }

    [Fact]
    public void TwBarcode_IsAnImage_NamedAfterItsText_ByDefault()
    {
        var cut = TestContext.Render<TwBarcode>(p => p.Add(x => x.Value, "ORDER-42"));

        var root = cut.Find("div");
        Assert.Equal("img", root.GetAttribute("role"));
        Assert.Equal("Barcode: ORDER-42", root.GetAttribute("aria-label"));
        Assert.Null(root.GetAttribute("tabindex"));
        Assert.Equal("true", cut.Find("span").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void TwBarcode_AriaLabel_And_AriaLabelledBy_ReplaceTheDefaultName()
    {
        var labelled = TestContext.Render<TwBarcode>(p => p.Add(x => x.Value, "A").Add(x => x.AriaLabel, "Order number"));
        var labelledBy = TestContext.Render<TwBarcode>(p => p.Add(x => x.Value, "A").Add(x => x.AriaLabelledBy, "caption"));

        Assert.Equal("Order number", labelled.Find("div").GetAttribute("aria-label"));
        Assert.Equal("caption", labelledBy.Find("div").GetAttribute("aria-labelledby"));
        Assert.Null(labelledBy.Find("div").GetAttribute("aria-label"));
    }

    [Fact]
    public void TwBarcode_NameIsTranslated()
    {
        using var context = new BunitContext();
        context.Services.AddTwBlazor(options => options.Localization.Culture = CultureInfo.GetCultureInfo("fr"), ThemeBase.CreateDefaultTheme());

        var cut = context.Render<TwBarcode>(p => p.Add(x => x.Value, "ABC"));

        Assert.Equal("Code-barres : ABC", cut.Find("div").GetAttribute("aria-label"));
    }

    [Theory]
    [InlineData(BarcodeSize.Small)]
    [InlineData(BarcodeSize.Medium)]
    [InlineData(BarcodeSize.Large)]
    public void TwBarcode_Size_AppliesTheThemeHeight(BarcodeSize size)
    {
        var expected = size switch
        {
            BarcodeSize.Small => theme.Small,
            BarcodeSize.Large => theme.Large,
            _ => theme.Medium
        };

        var cut = TestContext.Render<TwBarcode>(p => p.Add(x => x.Value, "a").Add(x => x.Size, size));

        Assert.Equal(expected, cut.Find("svg").GetAttribute("class"));
    }

    [Fact]
    public void TwBarcode_KeepsItsColors_InForcedColorsMode()
    {
        var cut = TestContext.Render<TwBarcode>(p => p.Add(x => x.Value, "a"));

        Assert.Contains("forced-color-adjust-none", cut.Find("div").GetAttribute("class"));
    }

    [Fact]
    public void TwBarcode_AppliesRoundedShadowClassStyleAndExtraAttributes()
    {
        var cut = TestContext.Render<TwBarcode>(p => p
            .Add(x => x.Value, "a")
            .Add(x => x.Rounded, Rounded.Md)
            .Add(x => x.Shadow, Shadow.Sm)
            .Add(x => x.Class, "my-barcode")
            .Add(x => x.Style, "margin: 1px")
            .AddUnmatched("data-test", "bc"));

        var root = cut.Find("div");
        var classes = root.GetAttribute("class")!;
        Assert.Contains(RoundedBuilder.GetRounded(Rounded.Md), classes);
        Assert.Contains(ShadowBuilder.GetShadow(Shadow.Sm), classes);
        Assert.Contains("my-barcode", classes);
        Assert.Equal("margin: 1px", root.GetAttribute("style"));
        Assert.Equal("bc", root.GetAttribute("data-test"));
    }

    private sealed class SpyBarcodeService : ITwBarcodeService
    {
        private readonly TwBarcodeService inner = new();

        public int EncodeCount { get; private set; }

        public TwBlazor.Barcoding.Barcode Encode(string value, BarcodeFormat format = BarcodeFormat.Code128) => inner.Encode(value, format);

        public bool TryEncode(string? value, BarcodeFormat format, [NotNullWhen(true)] out TwBlazor.Barcoding.Barcode? barcode)
        {
            EncodeCount++;
            return inner.TryEncode(value, format, out barcode);
        }

        public string GetSvgPath(TwBlazor.Barcoding.Barcode barcode, int? quietZone = null, int height = 50) => inner.GetSvgPath(barcode, quietZone, height);

        public string ToSvg(TwBlazor.Barcoding.Barcode barcode, BarcodeSvgOptions? options = null) => inner.ToSvg(barcode, options);

        public string ToSvgDataUri(TwBlazor.Barcoding.Barcode barcode, BarcodeSvgOptions? options = null) => inner.ToSvgDataUri(barcode, options);
    }
}
