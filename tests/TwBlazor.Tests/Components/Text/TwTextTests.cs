using Bunit;
using TwBlazor.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Text;

/// <summary>
/// Tests <see cref="TwText"/>: its semantic element, neutral and semantic colours, typography scale and truncation.
/// </summary>
public class TwTextTests : TwBlazorTestBase
{
    private AngleSharp.Dom.IElement Render(Action<ComponentParameterCollectionBuilder<TwText>>? configure = null) =>
        TestContext.Render<TwText>(p => configure?.Invoke(p)).Find("[id]");

    private static void AssertHasAll(AngleSharp.Dom.IElement element, string classes) =>
        Assert.All(classes.Split(' ', StringSplitOptions.RemoveEmptyEntries), value => Assert.Contains(value, element.ClassList));

    private static void AssertHasNone(AngleSharp.Dom.IElement element, string classes) =>
        Assert.All(classes.Split(' ', StringSplitOptions.RemoveEmptyEntries), value => Assert.DoesNotContain(value, element.ClassList));

    [Fact]
    public void ByDefault_RendersASpan_WithTheHeadingTone()
    {
        var element = Render();

        Assert.Equal("SPAN", element.TagName);
        AssertHasAll(element, Theme.Colors.NeutralText.Heading);
    }

    [Theory]
    [InlineData(LayoutElement.P, "P")]
    [InlineData(LayoutElement.H1, "H1")]
    [InlineData(LayoutElement.H2, "H2")]
    [InlineData(LayoutElement.Label, "LABEL")]
    [InlineData(LayoutElement.Strong, "STRONG")]
    [InlineData(LayoutElement.Em, "EM")]
    [InlineData(LayoutElement.Small, "SMALL")]
    public void Element_RendersTheChosenSemanticTag(LayoutElement element, string tagName)
    {
        var cut = Render(p => p.Add(x => x.Element, element));

        Assert.Equal(tagName, cut.TagName);
    }

    [Theory]
    [InlineData(TextTone.Default, "Heading")]
    [InlineData(TextTone.Secondary, "Secondary")]
    [InlineData(TextTone.Muted, "Muted")]
    [InlineData(TextTone.Subtle, "Subtle")]
    public void Tone_ResolvesTheThemeNeutralTextToken(TextTone tone, string token)
    {
        var element = Render(p => p.Add(x => x.Tone, tone));
        var expected = token switch
        {
            "Heading" => Theme.Colors.NeutralText.Heading,
            "Secondary" => Theme.Colors.NeutralText.Secondary,
            "Muted" => Theme.Colors.NeutralText.Muted,
            _ => Theme.Colors.NeutralText.Subtle
        };

        AssertHasAll(element, expected);
    }

    [Fact]
    public void Inherit_AddsNoColourClass()
    {
        var element = Render(p => p.Add(x => x.Tone, TextTone.Inherit));

        Assert.Empty(element.ClassList);
    }

    [Fact]
    public void Color_TakesPrecedenceOverTone()
    {
        var element = Render(p => p
            .Add(x => x.Tone, TextTone.Subtle)
            .Add(x => x.Color, Color.Primary));

        AssertHasAll(element, Theme.Colors.TextColors.Medium.Primary);
        AssertHasAll(element, Theme.Colors.DarkTextColors.Light.Primary);
        AssertHasNone(element, Theme.Colors.NeutralText.Subtle);
    }

    [Fact]
    public void SizeWeightAndTruncate_UseTheThemeScales()
    {
        var element = Render(p => p
            .Add(x => x.Size, TextSize.Xl3)
            .Add(x => x.Weight, TextWeight.Bold)
            .Add(x => x.Truncate, true));

        Assert.Contains(Theme.Typography.Size.Xl3, element.ClassList);
        Assert.Contains(Theme.Typography.Weight.Bold, element.ClassList);
        Assert.Contains(Theme.Typography.Truncate, element.ClassList);
    }

    [Fact]
    public void Class_IsMergedLast()
    {
        var element = Render(p => p.Add(x => x.Class, "mt-4 custom-text"));

        Assert.Contains("mt-4", element.ClassList);
        Assert.Contains("custom-text", element.ClassList);
    }
}
