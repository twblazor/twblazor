using TwBlazor.Enums;
using TwBlazor.Services;
using TwBlazor.Services.Encoding;

namespace TwBlazor.Tests.Services.Encoding;

public class SymbolSvgTests
{
    private readonly TwQrCodeService qrCodes = new();
    private readonly TwBarcodeService barcodes = new();

    private static int CountRuns(IEncodedSymbol symbol)
    {
        var runs = 0;
        for (var y = 0; y < symbol.Height; y++)
        {
            for (var x = 0; x < symbol.Width; x++)
            {
                if (symbol[x, y] && !symbol[x - 1, y])
                {
                    runs++;
                }
            }
        }

        return runs;
    }

    [Fact]
    public void GetSvgPath_QrCode_DrawsOneRectanglePerRunOfDarkModules_InsideTheQuietZone()
    {
        var matrix = qrCodes.Encode("HELLO WORLD", new QrEncodeOptions { ErrorCorrection = QrErrorCorrection.Quartile, BoostErrorCorrection = false });

        var path = matrix.GetSvgPath();

        // The top row of the reference matrix is 111111101100001111111.
        Assert.StartsWith("M4 4h7v1h-7zM12 4h2v1h-2zM18 4h7v1h-7z", path);
        Assert.Equal(CountRuns(matrix), path.Count(c => c == 'M'));
        Assert.StartsWith("M0 0h7v1h-7z", matrix.GetSvgPath(0));
    }

    [Fact]
    public void GetSvgPath_Barcode_StretchesTheBars_AndOnlyPadsTheSides()
    {
        var barcode = barcodes.Encode("1234567890");

        // The pattern starts 11010011100: bars of 2, 1 and 3 modules.
        Assert.StartsWith("M10 0h2v50h-2zM13 0h1v50h-1zM16 0h3v50h-3z", barcode.GetSvgPath());
        Assert.StartsWith("M0 0h2v20h-2z", barcode.GetSvgPath(0, 20));
        Assert.Equal(CountRuns(barcode), barcode.GetSvgPath().Count(c => c == 'M'));
    }

    [Fact]
    public void GetSvgViewBox_FitsTheSymbolAndItsQuietZone()
    {
        Assert.Equal("0 0 29 29", qrCodes.Encode("HELLO").GetSvgViewBox());
        Assert.Equal("0 0 23 23", qrCodes.Encode("HELLO").GetSvgViewBox(1));
        Assert.Equal("0 0 117 50", barcodes.Encode("5012345678900", BarcodeFormat.Ean13).GetSvgViewBox());
        Assert.Equal("0 0 95 30", barcodes.Encode("5012345678900", BarcodeFormat.Ean13).GetSvgViewBox(0, 30));
    }

    [Fact]
    public void Svg_RejectsInvalidSizes()
    {
        var matrix = qrCodes.Encode("A");

        Assert.Throws<ArgumentOutOfRangeException>(() => matrix.GetSvgPath(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => barcodes.Encode("A").GetSvgPath(0, 0));
        Assert.Throws<ArgumentNullException>(() => ((IEncodedSymbol)null!).GetSvgPath());
    }

    [Fact]
    public void ToSvg_WritesAStandaloneDocument_BlackOnWhite_ByDefault()
    {
        var svg = qrCodes.Encode("HELLO").ToSvg();

        Assert.StartsWith("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 29 29\"", svg);
        Assert.Contains("aria-hidden=\"true\"", svg);
        Assert.Contains("<rect width=\"100%\" height=\"100%\" fill=\"#ffffff\"/>", svg);
        Assert.Contains("<path fill=\"#000000\"", svg);
        Assert.DoesNotContain("<text", svg);
        Assert.EndsWith("</svg>", svg);
    }

    [Fact]
    public void ToSvg_WithoutALightColor_HasATransparentBackground()
    {
        var svg = qrCodes.Encode("HELLO").ToSvg(new SymbolSvgOptions { LightColor = null, DarkColor = "#fff" });

        Assert.DoesNotContain("<rect", svg);
        Assert.Contains("<path fill=\"#fff\"", svg);
    }

    [Fact]
    public void ToSvg_WithACaptionAndTitle_AddsRoomForTheCaption_AndEscapesBoth()
    {
        var barcode = barcodes.Encode("<A&B>");

        var svg = barcode.ToSvg(new SymbolSvgOptions { Caption = barcode.Text, Title = "Ticket \"1\"", QuietZone = 0, BarHeight = 30, DarkColor = "\"red" });

        Assert.Contains($"viewBox=\"0 0 {barcode.Width} 44\"", svg);
        Assert.Contains("role=\"img\"><title>Ticket &quot;1&quot;</title>", svg);
        Assert.Contains(">&lt;A&amp;B&gt;</text>", svg);
        Assert.Contains("fill=\"&quot;red\"", svg);
        Assert.DoesNotContain("aria-hidden", svg);
    }

    [Fact]
    public void ToSvgDataUri_EncodesTheSvgForAnImageSource()
    {
        var matrix = qrCodes.Encode("HELLO");

        var uri = matrix.ToSvgDataUri();

        Assert.StartsWith("data:image/svg+xml;charset=utf-8,%3Csvg", uri);
        Assert.Equal(matrix.ToSvg(), Uri.UnescapeDataString(uri["data:image/svg+xml;charset=utf-8,".Length..]));
    }

    [Fact]
    public void Symbols_ShareOneGridShape()
    {
        var matrix = qrCodes.Encode("HELLO");
        var barcode = barcodes.Encode("HELLO");

        Assert.Equal((21, 21, 4), (matrix.Width, matrix.Height, matrix.QuietZone));
        Assert.Equal(1, barcode.Height);
        Assert.Equal(10, barcode.QuietZone);
        Assert.True(barcode[0, 0]);
        Assert.False(barcode[0, 1]);
    }
}
