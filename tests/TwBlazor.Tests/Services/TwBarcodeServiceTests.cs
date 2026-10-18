using Microsoft.Extensions.DependencyInjection;
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
}
