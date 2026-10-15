using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Barcoding;
using TwBlazor.Enums;
using TwBlazor.Services;

namespace TwBlazor.Tests.Services;

public class TwBarcodeServiceTests : TwBlazorTestBase
{
    private readonly ITwBarcodeService service;

    public TwBarcodeServiceTests()
    {
        service = TestContext.Services.GetRequiredService<ITwBarcodeService>();
    }

    [Fact]
    public void AddTwBlazor_RegistersTheService_AsASingleton()
    {
        Assert.IsType<TwBarcodeService>(service);
        Assert.Same(service, TestContext.Services.GetRequiredService<ITwBarcodeService>());
    }

    [Fact]
    public void AddTwBlazor_KeepsAServiceRegisteredBeforeIt()
    {
        var services = new ServiceCollection();
        var custom = new TwBarcodeService();
        services.AddSingleton<ITwBarcodeService>(custom);

        services.AddTwBlazor(Theme);

        Assert.Same(custom, services.BuildServiceProvider().GetRequiredService<ITwBarcodeService>());
    }

    [Fact]
    public void Encode_DefaultsToCode128_AndRejectsNull()
    {
        Assert.Equal(BarcodeFormat.Code128, service.Encode("abc").Format);
        Assert.Throws<ArgumentNullException>(() => service.Encode(null!));
    }

    [Fact]
    public void TryEncode_ReturnsTheBarcode_WhenTheValueSuitsTheFormat()
    {
        Assert.True(service.TryEncode("501234567890", BarcodeFormat.Ean13, out var barcode));
        Assert.Equal("5012345678900", barcode.Text);
    }

    [Fact]
    public void TryEncode_ReturnsFalse_ForAnUnsuitableOrNullValue()
    {
        Assert.False(service.TryEncode("lower case", BarcodeFormat.Code39, out var unsuitable));
        Assert.Null(unsuitable);
        Assert.False(service.TryEncode(null, BarcodeFormat.Code128, out var missing));
        Assert.Null(missing);
    }

    [Fact]
    public void GetSvgPath_DrawsOneRectanglePerBar_OffsetByTheQuietZone()
    {
        var barcode = service.Encode("1234567890");

        var path = service.GetSvgPath(barcode);

        // The pattern starts 11010011100: bars of 2, 1 and 3 modules.
        Assert.StartsWith("M10 0h2v50h-2zM13 0h1v50h-1zM16 0h3v50h-3z", path);
        Assert.StartsWith("M0 0h2v20h-2z", service.GetSvgPath(barcode, 0, 20));
    }

    [Fact]
    public void GetSvgPath_RejectsInvalidSizes()
    {
        var barcode = service.Encode("A");

        Assert.Throws<ArgumentOutOfRangeException>(() => service.GetSvgPath(barcode, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => service.GetSvgPath(barcode, 0, 0));
        Assert.Throws<ArgumentNullException>(() => service.GetSvgPath(null!));
    }

    [Fact]
    public void ToSvg_WritesAStandaloneDocument_SizedForTheBarsAndQuietZone()
    {
        var barcode = service.Encode("5012345678900", BarcodeFormat.Ean13);

        var svg = service.ToSvg(barcode);

        Assert.StartsWith("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 117 50\"", svg);
        Assert.Contains("aria-hidden=\"true\"", svg);
        Assert.DoesNotContain("<text", svg);
        Assert.EndsWith("</svg>", svg);
    }

    [Fact]
    public void ToSvg_WithTextAndTitle_AddsRoomForTheText_AndEscapesIt()
    {
        var barcode = service.Encode("<A&B>");

        var svg = service.ToSvg(barcode, new BarcodeSvgOptions { ShowText = true, Title = "Ticket \"1\"", QuietZone = 0, Height = 30 });

        Assert.Contains($"viewBox=\"0 0 {barcode.Width} 44\"", svg);
        Assert.Contains("role=\"img\"><title>Ticket &quot;1&quot;</title>", svg);
        Assert.Contains(">&lt;A&amp;B&gt;</text>", svg);
    }

    [Fact]
    public void ToSvgDataUri_EncodesTheSvgForAnImageSource()
    {
        var barcode = service.Encode("A");

        var uri = service.ToSvgDataUri(barcode);

        Assert.StartsWith("data:image/svg+xml;charset=utf-8,%3Csvg", uri);
        Assert.Equal(service.ToSvg(barcode), Uri.UnescapeDataString(uri["data:image/svg+xml;charset=utf-8,".Length..]));
    }
}
