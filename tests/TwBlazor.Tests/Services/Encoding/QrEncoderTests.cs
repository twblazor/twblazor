using System.Security.Cryptography;
using System.Text;
using TwBlazor.Enums;
using TwBlazor.Services;
using TwBlazor.Services.Encoding;

namespace TwBlazor.Tests.Services.Encoding;

public class QrEncoderTests
{
    private readonly TwQrCodeService service = new();

    // Verified module for module against an independent encoder and decoded with ZXing.
    private static readonly string[] _helloWorldVersion1Quartile =
    [
        "111111101100001111111",
        "100000101001001000001",
        "101110101001101011101",
        "101110101000001011101",
        "101110101010001011101",
        "100000100010001000001",
        "111111101010101111111",
        "000000001000000000000",
        "011010110000101011111",
        "010000001111000010001",
        "001101110110001011000",
        "011011010011010101110",
        "100010101011101110101",
        "000000001101001000101",
        "111111101010000101100",
        "100000100101101101000",
        "101110101010001111111",
        "101110100101010100010",
        "101110101001011101001",
        "100000101011110001011",
        "111111100001011100001",
    ];

    private static string[] ToRows(QrMatrix matrix)
    {
        var rows = new string[matrix.Size];
        for (var y = 0; y < matrix.Size; y++)
        {
            var row = new StringBuilder(matrix.Size);
            for (var x = 0; x < matrix.Size; x++)
            {
                row.Append(matrix[x, y] ? '1' : '0');
            }

            rows[y] = row.ToString();
        }

        return rows;
    }

    private static string Fingerprint(QrMatrix matrix) =>
        Convert.ToHexStringLower(SHA256.HashData(System.Text.Encoding.ASCII.GetBytes(string.Concat(ToRows(matrix)))));

    [Fact]
    public void Encode_HelloWorld_MatchesTheReferenceMatrix()
    {
        var matrix = service.Encode("HELLO WORLD", new QrEncodeOptions { ErrorCorrection = QrErrorCorrection.Quartile, BoostErrorCorrection = false });

        Assert.Equal(1, matrix.Version);
        Assert.Equal(QrErrorCorrection.Quartile, matrix.ErrorCorrection);
        Assert.Equal(0, matrix.Mask);
        Assert.Equal(_helloWorldVersion1Quartile, ToRows(matrix));
    }

    [Theory]
    [InlineData("01234567", QrErrorCorrection.Medium, false, 1, 0, "647568cc3975d44fd3cb46f4447f71b7633ed372fa43385e0b671677e8409aaa")]
    [InlineData("https://twblazor.com", QrErrorCorrection.Medium, true, 2, 7, "e84f8ec24069e2b027e164334a8f7ae5ffd036ff2f9ed4cf0f60b5d390bf106e")]
    [InlineData("héllo wörld \U0001F600 日本", QrErrorCorrection.High, false, 4, 2, "4abd39558df9ef4ddf035aa2392cf34922f1e201d61462c431422ba899c51ff2")]
    public void Encode_MatchesTheReferenceMatrices(string value, QrErrorCorrection level, bool boost, int version, int mask, string fingerprint)
    {
        var matrix = service.Encode(value, new QrEncodeOptions { ErrorCorrection = level, BoostErrorCorrection = boost });

        Assert.Equal(version, matrix.Version);
        Assert.Equal(mask, matrix.Mask);
        Assert.Equal(fingerprint, Fingerprint(matrix));
    }

    [Fact]
    public void Encode_AVersionWithVersionInformation_MatchesTheReferenceMatrix()
    {
        var value = string.Concat(Enumerable.Repeat("The quick brown fox jumps over the lazy dog. ", 7));

        var matrix = service.Encode(value, new QrEncodeOptions { ErrorCorrection = QrErrorCorrection.Low, BoostErrorCorrection = false });

        Assert.Equal(11, matrix.Version);
        Assert.Equal("48c8fcd458979d46ecf51f0d44987deb009e5a141670dae9bedee83d7494fb90", Fingerprint(matrix));
    }

    [Fact]
    public void Encode_TheLargestVersion_MatchesTheReferenceMatrix()
    {
        var matrix = service.Encode(new string('a', 2953), new QrEncodeOptions { ErrorCorrection = QrErrorCorrection.Low });

        Assert.Equal(40, matrix.Version);
        Assert.Equal(177, matrix.Size);
        Assert.Equal("c27a04b5479583f147f7a50ff0707d1cd63ee3f0a5af3365cdd06bb9091adb1e", Fingerprint(matrix));
    }

    [Theory]
    [InlineData(0, "c10578dc3118651ac1d950efaf7563b15d61ec606fa0759958a0c8c60a4f5cff")]
    [InlineData(1, "639fb6640ece5b985a9fc527f1d051b8fe27c1f321365382eedeac4ace18e0e1")]
    [InlineData(2, "6e210e13cac244eff2a65fbf9aa427b2f703a0efaac9cc744b1cbb7ab78db707")]
    [InlineData(3, "783e644ac6f8782fc5e766059e22b964b79f6429b3a2eda9e33cfccf2c0a2a9c")]
    [InlineData(4, "c720c2dadca8dfb3028df37e7cef0d7273fd5a6abffebcf62630cbd978925de5")]
    [InlineData(5, "1260f0456ddecb58c2dd0da374ba0dbb8ce6512a9f46fc4934dab1dde82196dd")]
    [InlineData(6, "5f9de085d819bf4bb68d2af9382f131aec92941dc1ce11912bd11aad0008cfb0")]
    [InlineData(7, "c55b196e646e3d6d4e3129ad4f9b32cd1b6e10af12ee8b759325d3951d149e5b")]
    public void Encode_WithAFixedMask_MatchesTheReferenceMatrix(int mask, string fingerprint)
    {
        var matrix = service.Encode("Mask test 123", new QrEncodeOptions { Mask = mask, BoostErrorCorrection = false });

        Assert.Equal(mask, matrix.Mask);
        Assert.Equal(fingerprint, Fingerprint(matrix));
    }

    [Theory]
    [InlineData(1, QrErrorCorrection.Low, 17)]
    [InlineData(1, QrErrorCorrection.Medium, 14)]
    [InlineData(1, QrErrorCorrection.Quartile, 11)]
    [InlineData(1, QrErrorCorrection.High, 7)]
    [InlineData(10, QrErrorCorrection.Medium, 213)]
    [InlineData(40, QrErrorCorrection.Low, 2953)]
    [InlineData(40, QrErrorCorrection.Medium, 2331)]
    [InlineData(40, QrErrorCorrection.Quartile, 1663)]
    [InlineData(40, QrErrorCorrection.High, 1273)]
    public void Encode_FillsAVersionToItsPublishedByteCapacity_AndNoFurther(int version, QrErrorCorrection level, int capacity)
    {
        var options = new QrEncodeOptions { ErrorCorrection = level, BoostErrorCorrection = false, MinVersion = version, MaxVersion = version };

        Assert.Equal(version, service.Encode(new string('a', capacity), options).Version);
        Assert.Throws<SymbolEncodingException>(() => service.Encode(new string('a', capacity + 1), options));
    }

    [Theory]
    [InlineData('7', 7089)]
    [InlineData('A', 4296)]
    public void Encode_UsesTheDenserModes_ForDigitsAndUpperCaseText(char character, int capacity)
    {
        var options = new QrEncodeOptions { ErrorCorrection = QrErrorCorrection.Low, MinVersion = 40 };

        Assert.Equal(40, service.Encode(new string(character, capacity), options).Version);
        Assert.Throws<SymbolEncodingException>(() => service.Encode(new string(character, capacity + 1), options));
    }

    [Fact]
    public void Encode_PicksTheSmallestVersionThatFits()
    {
        Assert.Equal(1, service.Encode(new string('a', 14)).Version);
        Assert.Equal(2, service.Encode(new string('a', 15)).Version);
    }

    [Fact]
    public void Encode_BoostsErrorCorrection_OnlyWhenTheCodeStaysTheSameSize()
    {
        var boosted = service.Encode("A", new QrEncodeOptions { ErrorCorrection = QrErrorCorrection.Low });
        var notBoosted = service.Encode("A", new QrEncodeOptions { ErrorCorrection = QrErrorCorrection.Low, BoostErrorCorrection = false });

        Assert.Equal(QrErrorCorrection.High, boosted.ErrorCorrection);
        Assert.Equal(QrErrorCorrection.Low, notBoosted.ErrorCorrection);
        Assert.Equal(notBoosted.Version, boosted.Version);
    }

    [Fact]
    public void Encode_HigherErrorCorrection_NeedsALargerCodeForTheSameText()
    {
        var value = new string('a', 100);

        var low = service.Encode(value, new QrEncodeOptions { ErrorCorrection = QrErrorCorrection.Low, BoostErrorCorrection = false });
        var high = service.Encode(value, new QrEncodeOptions { ErrorCorrection = QrErrorCorrection.High, BoostErrorCorrection = false });

        Assert.True(high.Version > low.Version);
    }

    [Fact]
    public void Encode_MinVersion_KeepsShortValuesAtAFixedSize()
    {
        var matrix = service.Encode("hi", new QrEncodeOptions { MinVersion = 5 });

        Assert.Equal(5, matrix.Version);
        Assert.Equal(37, matrix.Size);
    }

    [Fact]
    public void Encode_EmptyText_GivesAValidVersion1Code()
    {
        var matrix = service.Encode(string.Empty);

        Assert.Equal(1, matrix.Version);
        Assert.True(matrix[0, 0]);
    }

    [Theory]
    [InlineData(0, 40, null)]
    [InlineData(1, 41, null)]
    [InlineData(10, 5, null)]
    [InlineData(1, 40, -1)]
    [InlineData(1, 40, 8)]
    public void Encode_RejectsOptionsOutsideTheStandard(int minVersion, int maxVersion, int? mask)
    {
        var options = new QrEncodeOptions { MinVersion = minVersion, MaxVersion = maxVersion, Mask = mask };

        Assert.Throws<ArgumentOutOfRangeException>(() => service.Encode("x", options));
    }

    [Fact]
    public void Encode_RejectsAnUndefinedErrorCorrectionLevel()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Encode("x", new QrEncodeOptions { ErrorCorrection = (QrErrorCorrection)9 }));
    }

    [Fact]
    public void Matrix_DrawsTheThreeFinderPatterns_AndReportsTheirArea()
    {
        var matrix = service.Encode("HELLO");
        var last = matrix.Size - 1;

        Assert.True(matrix[0, 0] && matrix[last, 0] && matrix[0, last]);
        Assert.False(matrix.IsFinderPattern(last, last));
        Assert.True(matrix.IsFinderPattern(7, 7));
        Assert.True(matrix.IsFinderPattern(matrix.Size - 8, 0));
        Assert.True(matrix.IsFinderPattern(0, matrix.Size - 8));
        Assert.False(matrix.IsFinderPattern(8, 8));
        Assert.False(matrix.IsFinderPattern(-1, 0));
    }

    [Fact]
    public void Matrix_TreatsPositionsOutsideTheGridAsLight()
    {
        var matrix = service.Encode("HELLO");

        Assert.False(matrix[-1, 0]);
        Assert.False(matrix[0, -1]);
        Assert.False(matrix[matrix.Size, 0]);
        Assert.False(matrix[0, matrix.Size]);
    }

    [Theory]
    [InlineData(1, new int[0])]
    [InlineData(2, new[] { 6, 18 })]
    [InlineData(7, new[] { 6, 22, 38 })]
    [InlineData(32, new[] { 6, 34, 60, 86, 112, 138 })]
    [InlineData(40, new[] { 6, 30, 58, 86, 114, 142, 170 })]
    public void AlignmentPositions_MatchTheStandard(int version, int[] expected)
    {
        Assert.Equal(expected, QrTables.GetAlignmentPositions(version));
    }

    [Theory]
    [InlineData(1, 208)]
    [InlineData(7, 1568)]
    [InlineData(40, 29648)]
    public void RawDataModules_MatchTheStandard(int version, int expected)
    {
        Assert.Equal(expected, QrTables.GetRawDataModules(version));
    }

    [Theory]
    [InlineData("0123456789", "Numeric")]
    [InlineData("HTTPS://TWBLAZOR.COM/$%*+-", "Alphanumeric")]
    [InlineData("https://twblazor.com", "Byte")]
    [InlineData("日本", "Byte")]
    public void Segments_UseTheDensestModeThatHoldsTheWholeText(string value, string expected)
    {
        Assert.Equal(Enum.Parse<QrSegmentMode>(expected), Assert.Single(QrSegment.FromText(value)).mode);
    }

    [Fact]
    public void Segments_CountUtf8Bytes_NotCharacters()
    {
        Assert.Equal(6, Assert.Single(QrSegment.FromText("日本")).characterCount);
    }

    [Theory]
    [InlineData("1", 4)]
    [InlineData("12", 7)]
    [InlineData("123", 10)]
    [InlineData("1234", 14)]
    public void NumericSegment_PacksThreeDigitsIntoTenBits(string digits, int bits)
    {
        Assert.Equal(bits, QrSegment.MakeNumeric(digits).data.length);
    }

    [Fact]
    public void Segments_RejectCharactersOutsideTheirMode()
    {
        Assert.Throws<ArgumentException>(() => QrSegment.MakeNumeric("12a"));
        Assert.Throws<ArgumentException>(() => QrSegment.MakeAlphanumeric("abc"));
    }

    [Fact]
    public void ReedSolomon_ProducesTheKnownErrorCorrectionForHelloWorld()
    {
        // The well known worked example: "HELLO WORLD" at version 1-M.
        byte[] data = [32, 91, 11, 120, 209, 114, 220, 77, 67, 64, 236, 17, 236, 17, 236, 17];
        var remainder = new byte[10];

        ReedSolomon.ComputeRemainder(data, ReedSolomon.GetDivisor(10), remainder);

        Assert.Equal([196, 35, 39, 119, 235, 215, 231, 226, 93, 23], remainder);
    }

    [Fact]
    public void BuildCodewords_WritesHeaderDataTerminatorAndAlternatingPadding()
    {
        var codewords = QrEncoder.BuildCodewords(QrSegment.FromText("HELLO WORLD"), 1, QrErrorCorrection.Medium);

        Assert.Equal([32, 91, 11, 120, 209, 114, 220, 77, 67, 64, 236, 17, 236, 17, 236, 17], codewords);
    }

    [Fact]
    public void AddErrorCorrectionAndInterleave_MatchesTheStandardsNumericExample()
    {
        // ISO/IEC 18004 Annex I: "01234567" at version 1-M.
        var data = QrEncoder.BuildCodewords(QrSegment.FromText("01234567"), 1, QrErrorCorrection.Medium);

        var result = QrEncoder.AddErrorCorrectionAndInterleave(data, 1, QrErrorCorrection.Medium);

        Assert.Equal([16, 32, 12, 86, 97, 128, 236, 17, 236, 17, 236, 17, 236, 17, 236, 17], data);
        Assert.Equal([165, 36, 212, 193, 237, 54, 199, 135, 44, 85], result[16..]);
    }

    [Fact]
    public void AddErrorCorrectionAndInterleave_TakesOneCodewordFromEachBlockInTurn()
    {
        // Version 5-Q has two short blocks of 15 data codewords and two long blocks of 16.
        var data = Enumerable.Range(0, QrTables.GetDataCodewords(5, QrErrorCorrection.Quartile)).Select(i => (byte)i).ToArray();

        var result = QrEncoder.AddErrorCorrectionAndInterleave(data, 5, QrErrorCorrection.Quartile);

        Assert.Equal(QrTables.GetRawDataModules(5) / 8, result.Length);
        Assert.Equal([0, 15, 30, 46, 1, 16, 31, 47], result[..8]);
        Assert.Equal([14, 29, 44, 60, 45, 61], result[56..62]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(31)]
    public void ReedSolomon_RejectsUnsupportedDegrees(int degree)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ReedSolomon.GetDivisor(degree));
    }

    [Fact]
    public void ReedSolomon_CachesEachDivisor()
    {
        Assert.Same(ReedSolomon.GetDivisor(7), ReedSolomon.GetDivisor(7));
    }

    [Fact]
    public void BitBuffer_RejectsValuesWiderThanTheirBitCount()
    {
        var buffer = new QrBitBuffer();

        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Append(4, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Append(0, 32));
    }

    [Fact]
    public void BitBuffer_OnlyConvertsWholeBytes()
    {
        var buffer = new QrBitBuffer(1);
        buffer.Append(0b101, 3);

        Assert.True(buffer[0]);
        Assert.False(buffer[1]);
        Assert.Throws<InvalidOperationException>(() => buffer.ToBytes());
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer[3]);

        buffer.Append(0b10101, 5);
        Assert.Equal([0b10110101], buffer.ToBytes());
    }
}
