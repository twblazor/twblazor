using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Enums;
using TwBlazor.Services;

namespace TwBlazor.Tests.Services;

public class TwBarcodeServiceTests : TwBlazorTestBase
{
    private readonly ITwBarcodeService _service;

    public TwBarcodeServiceTests()
    {
        _service = TestContext.Services.GetRequiredService<ITwBarcodeService>();
    }

    [Fact]
    public void AddTwBlazor_RegistersTheService_AsASingleton()
    {
        Assert.IsType<TwBarcodeService>(_service);
        Assert.Same(_service, TestContext.Services.GetRequiredService<ITwBarcodeService>());
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
        Assert.Equal(BarcodeFormat.Code128, _service.Encode("abc").Format);
        Assert.Throws<ArgumentNullException>(() => _service.Encode(null!));
    }

    [Fact]
    public void TryEncode_ReturnsTheBarcode_WhenTheValueSuitsTheFormat()
    {
        Assert.True(_service.TryEncode("501234567890", BarcodeFormat.Ean13, out var barcode));
        Assert.Equal("5012345678900", barcode.Text);
    }

    [Fact]
    public void TryEncode_ReturnsFalse_ForAnUnsuitableOrNullValue()
    {
        Assert.False(_service.TryEncode("lower case", BarcodeFormat.Code39, out var unsuitable));
        Assert.Null(unsuitable);
        Assert.False(_service.TryEncode(null, BarcodeFormat.Code128, out var missing));
        Assert.Null(missing);
    }
}
