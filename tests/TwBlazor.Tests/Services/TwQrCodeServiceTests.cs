using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Enums;
using TwBlazor.QrCoding;
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
        var fromBytes = service.Encode(Encoding.UTF8.GetBytes(value));

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

    [Fact]
    public void GetSvgPath_DrawsOneRectanglePerRunOfDarkModules_OffsetByTheQuietZone()
    {
        var matrix = service.Encode("HELLO WORLD", new QrEncodeOptions { ErrorCorrection = QrErrorCorrection.Quartile, BoostErrorCorrection = false });

        var path = service.GetSvgPath(matrix);

        // The top row of the reference matrix is 111111101100001111111.
        Assert.StartsWith("M4 4h7v1h-7zM12 4h2v1h-2zM18 4h7v1h-7z", path);
        Assert.Equal(CountRuns(matrix), path.Count(c => c == 'M'));
    }

    [Fact]
    public void GetSvgPath_WithoutAQuietZone_StartsAtTheOrigin()
    {
        var matrix = service.Encode("HELLO");

        Assert.StartsWith("M0 0h7v1h-7z", service.GetSvgPath(matrix, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => service.GetSvgPath(matrix, -1));
    }

    [Fact]
    public void ToSvg_WritesAStandaloneDocument_SizedForTheCodeAndQuietZone()
    {
        var matrix = service.Encode("HELLO");

        var svg = service.ToSvg(matrix);

        Assert.StartsWith("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 29 29\"", svg);
        Assert.Contains("aria-hidden=\"true\"", svg);
        Assert.Contains("fill=\"#ffffff\"", svg);
        Assert.Contains("fill=\"#000000\"", svg);
        Assert.EndsWith("</svg>", svg);
    }

    [Fact]
    public void ToSvg_WithATitle_IsAnImageWithAnEscapedName()
    {
        var matrix = service.Encode("HELLO");

        var svg = service.ToSvg(matrix, new QrSvgOptions { Title = "Menu <Tom & Jerry's>", DarkColor = "\"red", QuietZone = 2 });

        Assert.Contains("viewBox=\"0 0 25 25\"", svg);
        Assert.Contains("role=\"img\"><title>Menu &lt;Tom &amp; Jerry&apos;s&gt;</title>", svg);
        Assert.Contains("fill=\"&quot;red\"", svg);
        Assert.DoesNotContain("aria-hidden", svg);
    }

    [Fact]
    public void ToSvgDataUri_EncodesTheSvgForAnImageSource()
    {
        var matrix = service.Encode("HELLO");

        var uri = service.ToSvgDataUri(matrix);

        Assert.StartsWith("data:image/svg+xml;charset=utf-8,%3Csvg", uri);
        Assert.Equal(service.ToSvg(matrix), Uri.UnescapeDataString(uri["data:image/svg+xml;charset=utf-8,".Length..]));
    }

    private static int CountRuns(QrMatrix matrix)
    {
        var runs = 0;
        for (var y = 0; y < matrix.Size; y++)
        {
            for (var x = 0; x < matrix.Size; x++)
            {
                if (matrix[x, y] && !matrix[x - 1, y])
                {
                    runs++;
                }
            }
        }

        return runs;
    }
}
