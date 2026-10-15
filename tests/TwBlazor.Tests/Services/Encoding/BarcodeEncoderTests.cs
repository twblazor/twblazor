using System.Text;
using TwBlazor.Services.Encoding;
using TwBlazor.Enums;
using TwBlazor.Services;

namespace TwBlazor.Tests.Services.Encoding;

public class BarcodeEncoderTests
{
    private readonly TwBarcodeService service = new();

    private static string ToBits(Barcode barcode)
    {
        var bits = new StringBuilder(barcode.Width);
        for (var x = 0; x < barcode.Width; x++)
        {
            bits.Append(barcode[x] ? '1' : '0');
        }

        return bits.ToString();
    }

    // Each pattern was decoded with ZXing, and the EAN/UPC ones match an independent encoder bit for bit.
    [Theory]
    [InlineData(BarcodeFormat.Code128, "1234567890", "1234567890", "110100111001011001110010001011000111000101101100001010011011110110100111100101100011101011")]
    [InlineData(BarcodeFormat.Code128, "PJJ123C", "PJJ123C", "1101001000011101110110101101110001011011100010011100110110011100101100101110010001000110111010001101100011101011")]
    [InlineData(BarcodeFormat.Code39, "TWBLAZOR 2026", "TWBLAZOR 2026", "10001011101110101010111011100010111000111010101010111010001011101011101010001110111010100010111010001110111010101110101110100010111010101110001010001110101110101011100010101110101000111011101010111000101011101011100011101010100010111011101")]
    [InlineData(BarcodeFormat.Ean13, "400638133393", "4006381333931", "10100011010100111010111101111010001001011001101010100001010000101000010111010010000101100110101")]
    [InlineData(BarcodeFormat.Ean8, "9638507", "96385074", "1010001011010111101111010110111010101001110111001010001001011100101")]
    [InlineData(BarcodeFormat.UpcA, "03600029145", "036000291452", "10100011010111101010111100011010001101000110101010110110011101001100110101110010011101101100101")]
    public void Encode_MatchesTheReferencePatterns(BarcodeFormat format, string value, string text, string bits)
    {
        var barcode = service.Encode(value, format);

        Assert.Equal(format, barcode.Format);
        Assert.Equal(text, barcode.Text);
        Assert.Equal(bits, ToBits(barcode));
    }

    [Theory]
    [InlineData(BarcodeFormat.Code128, 10)]
    [InlineData(BarcodeFormat.Code39, 10)]
    [InlineData(BarcodeFormat.Ean13, 11)]
    [InlineData(BarcodeFormat.Ean8, 7)]
    [InlineData(BarcodeFormat.UpcA, 11)]
    public void Encode_ReportsTheQuietZoneEachFormatNeeds(BarcodeFormat format, int quietZone)
    {
        var value = format switch
        {
            BarcodeFormat.Ean13 => "501234567890",
            BarcodeFormat.Ean8 => "9638507",
            BarcodeFormat.UpcA => "03600029145",
            _ => "ABC"
        };

        Assert.Equal(quietZone, service.Encode(value, format).QuietZone);
    }

    [Theory]
    [InlineData(BarcodeFormat.Ean13, 95)]
    [InlineData(BarcodeFormat.UpcA, 95)]
    [InlineData(BarcodeFormat.Ean8, 67)]
    public void Encode_RetailFormats_HaveTheirFixedWidth(BarcodeFormat format, int width)
    {
        var value = format == BarcodeFormat.Ean8 ? "1234567" : format == BarcodeFormat.UpcA ? "12345678901" : "123456789012";

        Assert.Equal(width, service.Encode(value, format).Width);
    }

    [Fact]
    public void Barcode_TreatsPositionsOutsideTheBarsAsLight()
    {
        var barcode = service.Encode("A");

        Assert.True(barcode[0]);
        Assert.False(barcode[-1]);
        Assert.False(barcode[barcode.Width]);
    }

    [Theory]
    [InlineData("PJJ123C", new[] { 104, 48, 42, 42, 17, 18, 19, 35, 55, 106 })]
    [InlineData("1234567890", new[] { 105, 12, 34, 56, 78, 90, 85, 106 })]
    [InlineData("12", new[] { 105, 12, 14, 106 })]
    [InlineData("a12345", new[] { 104, 65, 17, 99, 23, 45, 96, 106 })]
    [InlineData("a\tb", new[] { 104, 65, 101, 73, 100, 66, 84, 106 })]
    [InlineData("\tA", new[] { 103, 73, 33, 36, 106 })]
    public void Code128_PicksCodeSetsToKeepTheBarcodeShort(string value, int[] symbols)
    {
        Assert.Equal(symbols, Code128Symbology.GetSymbols(value));
    }

    [Fact]
    public void Code128_SwitchesBackFromSetC_WhenTheDigitsRunOut()
    {
        var symbols = Code128Symbology.GetSymbols("X123456Y");

        Assert.Equal([104, 56, 99, 12, 34, 56, 100, 57], symbols[..^2]);
    }

    [Fact]
    public void Code128_EveryPatternIsElevenModules_AndStopIsThirteen()
    {
        for (var value = 0; value < Code128Symbology.stop; value++)
        {
            Assert.Equal(11, Code128Symbology.GetPattern(value).Sum(c => c - '0'));
        }

        Assert.Equal(13, Code128Symbology.GetPattern(Code128Symbology.stop).Sum(c => c - '0'));
    }

    [Fact]
    public void Code128_LeavesControlCharactersOutOfTheText()
    {
        Assert.Equal("ab", service.Encode("a\tb").Text);
    }

    [Theory]
    [InlineData("")]
    [InlineData("café")]
    public void Code128_RejectsEmptyOrNonAsciiValues(string value)
    {
        Assert.Throws<SymbolEncodingException>(() => service.Encode(value));
    }

    [Fact]
    public void Code39_EveryPatternHasThreeWideElements()
    {
        foreach (var c in "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-. $/+%*")
        {
            var pattern = Code39Symbology.GetPattern(c);
            Assert.NotNull(pattern);
            Assert.Equal(9, pattern.Length);
            Assert.Equal(3, pattern.Count(element => element == '1'));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("lower")]
    [InlineData("A*B")]
    public void Code39_RejectsCharactersItCannotHold(string value)
    {
        Assert.Throws<SymbolEncodingException>(() => service.Encode(value, BarcodeFormat.Code39));
    }

    [Theory]
    [InlineData("400638133393", 1)]
    [InlineData("501234567890", 0)]
    [InlineData("9638507", 4)]
    [InlineData("03600029145", 2)]
    public void CheckDigit_UsesAlternatingWeightsOfThreeAndOne(string digits, int check)
    {
        Assert.Equal(check, EanUpcSymbology.GetCheckDigit(digits));
    }

    [Theory]
    [InlineData(BarcodeFormat.Ean13, "4006381333932")]
    [InlineData(BarcodeFormat.Ean8, "96385075")]
    [InlineData(BarcodeFormat.UpcA, "036000291453")]
    public void RetailFormats_RejectAWrongCheckDigit(BarcodeFormat format, string value)
    {
        var exception = Assert.Throws<SymbolEncodingException>(() => service.Encode(value, format));

        Assert.Contains("check digit", exception.Message);
    }

    [Theory]
    [InlineData(BarcodeFormat.Ean13, "12345")]
    [InlineData(BarcodeFormat.Ean13, "12345678901234")]
    [InlineData(BarcodeFormat.Ean13, "50123456789A")]
    [InlineData(BarcodeFormat.Ean8, "123456789")]
    [InlineData(BarcodeFormat.UpcA, "1234567890")]
    public void RetailFormats_RejectTheWrongNumberOfDigits(BarcodeFormat format, string value)
    {
        Assert.Throws<SymbolEncodingException>(() => service.Encode(value, format));
    }

    [Fact]
    public void Encode_RejectsAnUnknownFormat()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Encode("1", (BarcodeFormat)99));
    }
}
