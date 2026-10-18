using System.Globalization;
using TwBlazor.Utilities;

namespace TwBlazor.Tests.Utilities;

public class NumericValueTests
{
    private static readonly CultureInfo _british = TestCultures.British;

    [Fact]
    public void DescribesTheType()
    {
        Assert.False(NumericValue<int>.Instance.IsNullable);
        Assert.True(NumericValue<int?>.Instance.IsNullable);
        Assert.False(NumericValue<int>.Instance.IsFractional);
        Assert.True(NumericValue<decimal>.Instance.IsFractional);
        Assert.True(NumericValue<int>.Instance.IsSigned);
        Assert.False(NumericValue<uint>.Instance.IsSigned);
        Assert.Equal(int.MinValue, NumericValue<int>.Instance.MinValue);
        Assert.Equal(byte.MaxValue, NumericValue<byte?>.Instance.MaxValue);
    }

    [Fact]
    public void IsBound_IgnoresNullAndTheTypesOwnLimits()
    {
        var number = NumericValue<int?>.Instance;

        Assert.False(number.IsBound(null));
        Assert.False(number.IsBound(int.MinValue));
        Assert.False(number.IsBound(int.MaxValue));
        Assert.True(number.IsBound(0));
    }

    [Fact]
    public void Compare_SortsNullFirst()
    {
        var number = NumericValue<int?>.Instance;

        Assert.True(number.Compare(null, 1) < 0);
        Assert.True(number.Compare(1, null) > 0);
        Assert.Equal(0, number.Compare(null, null));
        Assert.True(number.Compare(2, 1) > 0);
    }

    [Theory]
    [InlineData("1,234.5", 1234.5)]
    [InlineData(" -3 ", -3)]
    [InlineData("£12.50", 12.5)]
    public void TryParse_AcceptsSeparatorsSignsAndCurrency(string text, double expected)
    {
        Assert.True(NumericValue<decimal>.Instance.TryParse(text, _british, out var value));
        Assert.Equal((decimal)expected, value);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("99999999999")]
    public void TryParse_RejectsWhatTheTypeCannotHold(string text)
    {
        Assert.False(NumericValue<int>.Instance.TryParse(text, _british, out _));
    }

    [Fact]
    public void TryParse_AcceptsExponents_ForBinaryFloatingPoint()
    {
        Assert.True(NumericValue<double>.Instance.TryParse("1e3", _british, out var value));
        Assert.Equal(1000d, value);
    }

    [Fact]
    public void Format_UsesTheFormatAndCulture()
    {
        Assert.Equal("1.234,50", NumericValue<decimal>.Instance.Format(1234.5m, "N2", CultureInfo.GetCultureInfo("de-DE")));
        Assert.Equal(string.Empty, NumericValue<int?>.Instance.Format(null, null, _british));
    }

    [Fact]
    public void FormatInvariant_IgnoresTheCulture()
    {
        Assert.Equal("1234.5", NumericValue<decimal>.Instance.FormatInvariant(1234.5m));
        Assert.Null(NumericValue<int?>.Instance.FormatInvariant(null));
    }

    [Theory]
    [InlineData(-5, 0)]
    [InlineData(5, 5)]
    [InlineData(15, 10)]
    public void Clamp_KeepsTheValueInRange(int value, int expected)
    {
        Assert.Equal(expected, NumericValue<int>.Instance.Clamp(value, 0, 10));
    }

    [Fact]
    public void Clamp_TreatsNullLimitsAsUnbounded_AndKeepsNull()
    {
        var number = NumericValue<int?>.Instance;

        Assert.Equal(-500, number.Clamp(-500, null, null));
        Assert.Null(number.Clamp(null, 0, 10));
    }

    [Fact]
    public void Clamp_WithMaxBelowMin_UsesMin()
    {
        Assert.Equal(5, NumericValue<int>.Instance.Clamp(1, 5, 2));
    }

    [Theory]
    [InlineData(10, 1, 11)]
    [InlineData(10, -1, 9)]
    [InlineData(10, 10, 20)]
    public void Add_TakesSteps(int value, int steps, int expected)
    {
        Assert.Equal(expected, NumericValue<int>.Instance.Add(value, 1, steps, int.MinValue, int.MaxValue));
    }

    [Fact]
    public void Add_TreatsANegativeStepAsPositive()
    {
        Assert.Equal(12, NumericValue<int>.Instance.Add(10, -2, 1, int.MinValue, int.MaxValue));
    }

    [Fact]
    public void Add_StartsFromZero_ForNull()
    {
        Assert.Equal(-1, NumericValue<int?>.Instance.Add(null, 1, -1, null, null));
    }

    [Fact]
    public void Add_StopsAtTheTypesLimits_InsteadOfOverflowing()
    {
        Assert.Equal(int.MaxValue, NumericValue<int>.Instance.Add(int.MaxValue - 1, 5, 1, int.MinValue, int.MaxValue));
        Assert.Equal(byte.MinValue, NumericValue<byte>.Instance.Add((byte)1, (byte)5, -1, byte.MinValue, byte.MaxValue));
        Assert.Equal(double.MaxValue, NumericValue<double>.Instance.Add(double.MaxValue, double.MaxValue, 1, double.MinValue, double.MaxValue));
    }

    [Fact]
    public void Add_StopsAtTheLimits()
    {
        Assert.Equal(100, NumericValue<int>.Instance.Add(95, 1, 10, 0, 100));
        Assert.Equal(0, NumericValue<int>.Instance.Add(5, 1, -10, 0, 100));
    }

    [Fact]
    public void Add_SteppingBinaryFloatingPoint_AvoidsRoundingErrors()
    {
        Assert.Equal(0.3, NumericValue<double>.Instance.Add(0.1, 0.2, 1, double.MinValue, double.MaxValue));
        Assert.Equal(0.3f, NumericValue<float>.Instance.Add(0.1f, 0.1f, 2, float.MinValue, float.MaxValue));
    }

    [Fact]
    public void UnsupportedTypes_Throw()
    {
        var exception = Assert.Throws<TypeInitializationException>(() => NumericValue<string>.Instance);
        Assert.IsType<NotSupportedException>(exception.InnerException);
    }
}
