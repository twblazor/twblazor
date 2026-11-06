using TwBlazor.Builders;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Utilities;

public class RoundedBuilderTests : TwBlazorTestBase
{
    [Theory]
    [InlineData(Rounded.None, "rounded-none")]
    [InlineData(Rounded.Sm, "rounded-sm")]
    [InlineData(Rounded.Md, "rounded")]
    [InlineData(Rounded.Lg, "rounded-lg")]
    [InlineData(Rounded.Full, "rounded-full")]
    public void GetRounded_ReturnsCorrectClass_ForEachRoundedValue(Rounded rounded, string expectedClass)
    {
        // Act
        var result = RoundedBuilder.GetRounded(rounded);

        // Assert
        Assert.Equal(expectedClass, result);
    }

    [Fact]
    public void GetRounded_ReturnsEmptyString_WhenRoundedIsNull()
    {
        // Act
        var result = RoundedBuilder.GetRounded(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetRounded_ReturnsThemeDefaultRounded_ForUnknownValue()
    {
        // Arrange - Cast an invalid enum value
        var unknownRounded = (Rounded)999;

        // Act
        var result = RoundedBuilder.GetRounded(unknownRounded);

        // Assert - unknown values fall back to whatever Theme.Rounded.DefaultRounded resolves to,
        // not a hardcoded level, so this stays correct if the theme's default ever changes.
        Assert.Equal(RoundedBuilder.GetRounded(Theme.Rounded.DefaultRounded), result);
    }

    [Fact]
    public void GetRounded_None_ProducesNoRoundedCorners()
    {
        // Act
        var result = RoundedBuilder.GetRounded(Rounded.None);

        // Assert
        Assert.Equal("rounded-none", result);
    }

    [Fact]
    public void GetRounded_Full_ProducesFullyRoundedCorners()
    {
        // Act
        var result = RoundedBuilder.GetRounded(Rounded.Full);

        // Assert
        Assert.Equal("rounded-full", result);
    }

    [Fact]
    public void GetRounded_Md_ReturnsSimpleRounded()
    {
        // Arrange - Md should return base "rounded" class

        // Act
        var result = RoundedBuilder.GetRounded(Rounded.Md);

        // Assert
        Assert.Equal("rounded", result);
    }

    [Fact]
    public void GetRounded_Parameterless_ReturnsClass_ForDefaultRoundedLevel()
    {
        // Act
        var result = RoundedBuilder.GetRounded();

        // Assert
        Assert.Equal(RoundedBuilder.GetRounded(Theme.Rounded.DefaultRounded), result);
    }

    [Theory]
    [InlineData(Rounded.None)]
    [InlineData(Rounded.Sm)]
    [InlineData(Rounded.Md)]
    [InlineData(Rounded.Lg)]
    [InlineData(Rounded.Full)]
    public void GetRounded_ReturnsNonEmptyString_ForAllValidRoundedValues(Rounded rounded)
    {
        // Act
        var result = RoundedBuilder.GetRounded(rounded);

        // Assert
        Assert.NotEmpty(result);
        Assert.StartsWith("rounded", result);
    }

    [Theory]
    [InlineData(Rounded.None, "rounded-t-none")]
    [InlineData(Rounded.Sm, "rounded-t-sm")]
    [InlineData(Rounded.Md, "rounded-t")]
    [InlineData(Rounded.Lg, "rounded-t-lg")]
    [InlineData(Rounded.Full, "rounded-t-full")]
    public void GetRoundedTop_ReturnsCorrectClass_ForEachRoundedValue(Rounded rounded, string expectedClass)
    {
        // Act
        var result = RoundedBuilder.GetRoundedTop(rounded);

        // Assert
        Assert.Equal(expectedClass, result);
    }

    [Fact]
    public void GetRoundedTop_ReturnsEmptyString_WhenRoundedIsNull()
    {
        // Act
        var result = RoundedBuilder.GetRoundedTop(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetRoundedTop_ReturnsDefaultRounded_ForUnknownValue()
    {
        // Arrange - Cast an invalid enum value
        var unknownRounded = (Rounded)999;

        // Act
        var result = RoundedBuilder.GetRoundedTop(unknownRounded);

        // Assert
        Assert.Equal("rounded-t-lg", result); //default to lg for unknown values
    }

    [Fact]
    public void GetRoundedTop_Md_ReturnsSimpleRoundedTop()
    {
        // Arrange - Md should return base "rounded-t" class

        // Act
        var result = RoundedBuilder.GetRoundedTop(Rounded.Md);

        // Assert
        Assert.Equal("rounded-t", result);
    }

    [Fact]
    public void GetRoundedTop_Parameterless_ReturnsTopClass_ForDefaultRoundedLevel()
    {
        // Act
        var result = RoundedBuilder.GetRoundedTop();

        // Assert
        Assert.Equal(RoundedBuilder.GetRoundedTop(Theme.Rounded.DefaultRounded), result);
    }

    [Theory]
    [InlineData(Rounded.None, "rounded-b-none")]
    [InlineData(Rounded.Sm, "rounded-b-sm")]
    [InlineData(Rounded.Md, "rounded-b")]
    [InlineData(Rounded.Lg, "rounded-b-lg")]
    [InlineData(Rounded.Full, "rounded-b-full")]
    public void GetRoundedBottom_ReturnsCorrectClass_ForEachRoundedValue(Rounded rounded, string expectedClass)
    {
        // Act
        var result = RoundedBuilder.GetRoundedBottom(rounded);

        // Assert
        Assert.Equal(expectedClass, result);
    }

    [Fact]
    public void GetRoundedBottom_ReturnsEmptyString_WhenRoundedIsNull()
    {
        // Act
        var result = RoundedBuilder.GetRoundedBottom(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetRoundedBottom_ReturnsDefaultRounded_ForUnknownValue()
    {
        // Arrange - Cast an invalid enum value
        var unknownRounded = (Rounded)999;

        // Act
        var result = RoundedBuilder.GetRoundedBottom(unknownRounded);

        // Assert
        Assert.Equal("rounded-b-lg", result); // default to lg for unknown values
    }

    [Fact]
    public void GetRoundedBottom_Md_ReturnsSimpleRoundedBottom()
    {
        // Arrange - Md should return base "rounded-b" class

        // Act
        var result = RoundedBuilder.GetRoundedBottom(Rounded.Md);

        // Assert
        Assert.Equal("rounded-b", result);
    }

    [Theory]
    [InlineData(Rounded.None, "rounded-s-none")]
    [InlineData(Rounded.Sm, "rounded-s-sm")]
    [InlineData(Rounded.Md, "rounded-s")]
    [InlineData(Rounded.Lg, "rounded-s-lg")]
    [InlineData(Rounded.Full, "rounded-s-full")]
    public void GetRoundedStart_ReturnsCorrectClass_ForEachRoundedValue(Rounded rounded, string expectedClass)
    {
        // Act
        var result = RoundedBuilder.GetRoundedStart(rounded);

        // Assert
        Assert.Equal(expectedClass, result);
    }

    [Fact]
    public void GetRoundedStart_ReturnsEmptyString_WhenRoundedIsNull()
    {
        // Act
        var result = RoundedBuilder.GetRoundedStart(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetRoundedStart_ReturnsDefaultRounded_ForUnknownValue()
    {
        // Arrange - Cast an invalid enum value
        var unknownRounded = (Rounded)999;

        // Act
        var result = RoundedBuilder.GetRoundedStart(unknownRounded);

        // Assert
        Assert.Equal("rounded-s-lg", result); // default to lg for unknown values
    }

    [Fact]
    public void GetRoundedStart_Parameterless_ReturnsStartClass_ForDefaultRoundedLevel()
    {
        // Act
        var result = RoundedBuilder.GetRoundedStart();

        // Assert
        Assert.Equal(RoundedBuilder.GetRoundedStart(Theme.Rounded.DefaultRounded), result);
    }

    [Theory]
    [InlineData(Rounded.None, "rounded-e-none")]
    [InlineData(Rounded.Sm, "rounded-e-sm")]
    [InlineData(Rounded.Md, "rounded-e")]
    [InlineData(Rounded.Lg, "rounded-e-lg")]
    [InlineData(Rounded.Full, "rounded-e-full")]
    public void GetRoundedEnd_ReturnsCorrectClass_ForEachRoundedValue(Rounded rounded, string expectedClass)
    {
        // Act
        var result = RoundedBuilder.GetRoundedEnd(rounded);

        // Assert
        Assert.Equal(expectedClass, result);
    }

    [Fact]
    public void GetRoundedEnd_ReturnsEmptyString_WhenRoundedIsNull()
    {
        // Act
        var result = RoundedBuilder.GetRoundedEnd(null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetRoundedEnd_ReturnsDefaultRounded_ForUnknownValue()
    {
        // Arrange - Cast an invalid enum value
        var unknownRounded = (Rounded)999;

        // Act
        var result = RoundedBuilder.GetRoundedEnd(unknownRounded);

        // Assert
        Assert.Equal("rounded-e-lg", result); // default to lg for unknown values
    }

    [Fact]
    public void GetRoundedEnd_Parameterless_ReturnsEndClass_ForDefaultRoundedLevel()
    {
        // Act
        var result = RoundedBuilder.GetRoundedEnd();

        // Assert
        Assert.Equal(RoundedBuilder.GetRoundedEnd(Theme.Rounded.DefaultRounded), result);
    }

    [Theory]
    [InlineData(Rounded.None, Rounded.None)]
    [InlineData(Rounded.Sm, Rounded.Sm)]
    [InlineData(Rounded.Md, Rounded.Md)]
    [InlineData(Rounded.Lg, Rounded.Lg)]
    [InlineData(Rounded.Full, Rounded.Lg)]
    public void ForSurface_CapsFullAtLarge(Rounded rounded, Rounded expected)
    {
        Assert.Equal(expected, TwBlazor.Builders.RoundedBuilder.ForSurface(rounded));
    }

    [Theory]
    [InlineData(Rounded.None, Rounded.None)]
    [InlineData(Rounded.Sm, Rounded.Sm)]
    [InlineData(Rounded.Md, Rounded.Sm)]
    [InlineData(Rounded.Lg, Rounded.Md)]
    [InlineData(Rounded.Full, Rounded.Full)]
    public void Inset_IsOneStepTighter_AndKeepsFull(Rounded rounded, Rounded expected)
    {
        Assert.Equal(expected, TwBlazor.Builders.RoundedBuilder.Inset(rounded));
    }
}
