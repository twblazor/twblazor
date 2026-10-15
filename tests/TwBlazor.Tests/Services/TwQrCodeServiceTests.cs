using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Enums;
using TwBlazor.Services.Encoding;
using TwBlazor.Services;

namespace TwBlazor.Tests.Services;

public class TwQrCodeServiceTests : TwBlazorTestBase
{
    private readonly ITwQrCodeService service;

    public TwQrCodeServiceTests()
    {
        service = TestContext.Services.GetRequiredService<ITwQrCodeService>();
    }

    [Fact]
    public void AddTwBlazor_RegistersTheService_AsASingleton()
    {
        Assert.IsType<TwQrCodeService>(service);
        Assert.Same(service, TestContext.Services.GetRequiredService<ITwQrCodeService>());
    }

    [Fact]
    public void AddTwBlazor_KeepsAServiceRegisteredBeforeIt()
    {
        var services = new ServiceCollection();
        var custom = new TwQrCodeService();
        services.AddSingleton<ITwQrCodeService>(custom);

        services.AddTwBlazor(Theme);

        Assert.Same(custom, services.BuildServiceProvider().GetRequiredService<ITwQrCodeService>());
    }

    [Fact]
    public void Encode_RejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => service.Encode((string)null!));
        Assert.Throws<ArgumentNullException>(() => service.Encode((byte[])null!));
    }

    [Fact]
    public void Encode_Bytes_MatchesTheSameTextAsUtf8()
    {
        const string value = "https://twblazor.com/qr-code?ref=docs";

        var fromText = service.Encode(value);
        var fromBytes = service.Encode(System.Text.Encoding.UTF8.GetBytes(value));

        Assert.Equal(fromText.Version, fromBytes.Version);
        Assert.Equal(fromText.Mask, fromBytes.Mask);
        for (var y = 0; y < fromText.Size; y++)
        {
            for (var x = 0; x < fromText.Size; x++)
            {
                Assert.Equal(fromText[x, y], fromBytes[x, y]);
            }
        }
    }

    [Fact]
    public void Encode_EmptyBytes_GivesAVersion1Code()
    {
        Assert.Equal(1, service.Encode([]).Version);
    }

    [Fact]
    public void TryEncode_ReturnsTheMatrix_WhenTheTextFits()
    {
        Assert.True(service.TryEncode("hello", null, out var matrix));
        Assert.Equal(1, matrix.Version);
    }

    [Fact]
    public void TryEncode_ReturnsFalse_WhenTheTextIsTooLongOrNull()
    {
        Assert.False(service.TryEncode(new string('a', 1274), new QrEncodeOptions { ErrorCorrection = QrErrorCorrection.High }, out var tooLong));
        Assert.Null(tooLong);
        Assert.False(service.TryEncode(null, null, out var missing));
        Assert.Null(missing);
    }

    [Fact]
    public void TryEncode_StillThrows_ForInvalidOptions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => service.TryEncode("x", new QrEncodeOptions { Mask = 9 }, out _));
    }
}
