using AngleSharp.Dom;
using Bunit;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Separator;

public class TwSeparatorTests : TwBlazorTestBase
{
    private TwSeparatorTheme theme => Theme.Components.Require<TwSeparatorTheme>();

    private IRenderedComponent<TwSeparator> Render(Action<ComponentParameterCollectionBuilder<TwSeparator>>? configure = null) =>
        TestContext.Render<TwSeparator>(p =>
        {
            p.Add(x => x.Id, "separator");
            configure?.Invoke(p);
        });

    private static IElement Root(IRenderedComponent<TwSeparator> cut) => cut.Find("#separator");

    private static string[] Tokens(string classes) => classes.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    private static void AssertHasClasses(IElement element, string classes)
    {
        foreach (var token in Tokens(classes))
        {
            Assert.Contains(token, element.ClassList);
        }
    }

    private static void AssertLacksClasses(IElement element, string classes)
    {
        foreach (var token in Tokens(classes))
        {
            Assert.DoesNotContain(token, element.ClassList);
        }
    }

    private string PaletteValueFor(Color color) => color switch
    {
        Color.Primary => theme.Colors.Primary,
        Color.Accent => theme.Colors.Accent,
        Color.Success => theme.Colors.Success,
        Color.Danger => theme.Colors.Danger,
        Color.Warning => theme.Colors.Warning,
        Color.Info => theme.Colors.Info,
        Color.Light => theme.Colors.Light,
        _ => theme.Colors.Dark,
    };

    private string ThicknessFor(SeparatorThickness thickness) => thickness switch
    {
        SeparatorThickness.Medium => theme.Medium,
        SeparatorThickness.Thick => theme.Thick,
        _ => theme.Thin,
    };

    private string VariantFor(SeparatorVariant variant) => variant switch
    {
        SeparatorVariant.Dashed => theme.Dashed,
        SeparatorVariant.Dotted => theme.Dotted,
        _ => theme.Solid,
    };

    [Fact]
    public void TwSeparator_GeneratesAnId_WhenNotProvided()
    {
        var cut = TestContext.Render<TwSeparator>();

        Assert.StartsWith("separator-", cut.Find("div").GetAttribute("id"));
    }

    [Fact]
    public void TwSeparator_ByDefault_IsASingleHorizontalSemanticLine()
    {
        var cut = Render();

        var root = Root(cut);
        Assert.Equal("DIV", root.TagName);
        Assert.Empty(root.Children);
        Assert.Equal("separator", root.GetAttribute("role"));
        Assert.Equal("horizontal", root.GetAttribute("aria-orientation"));
        Assert.False(root.HasAttribute("aria-hidden"));
        Assert.False(root.HasAttribute("aria-label"));
        Assert.False(root.HasAttribute("aria-labelledby"));
        AssertHasClasses(root, theme.Base);
        AssertHasClasses(root, theme.Horizontal);
        AssertHasClasses(root, theme.HorizontalLine);
        AssertHasClasses(root, theme.Thin);
        AssertHasClasses(root, theme.Solid);
        AssertHasClasses(root, theme.Neutral);
        AssertLacksClasses(root, theme.Vertical);
        AssertLacksClasses(root, theme.VerticalLine);
    }

    [Fact]
    public void TwSeparator_IsNotFocusable()
    {
        var root = Root(Render(p => p.Add(x => x.Label, "or")));

        Assert.False(root.HasAttribute("tabindex"));
    }

    [Fact]
    public void TwSeparator_Vertical_HasVerticalOrientationAndClasses()
    {
        var cut = Render(p => p.Add(x => x.Orientation, SeparatorOrientation.Vertical));

        var root = Root(cut);
        Assert.Equal("vertical", root.GetAttribute("aria-orientation"));
        AssertHasClasses(root, theme.Vertical);
        AssertHasClasses(root, theme.VerticalLine);
        AssertLacksClasses(root, theme.Horizontal);
        AssertLacksClasses(root, theme.HorizontalLine);
    }

    [Fact]
    public void TwSeparator_Decorative_WithoutLabel_IsHiddenFromAssistiveTechnology()
    {
        var cut = Render(p => p
            .Add(x => x.Decorative, true)
            .Add(x => x.Orientation, SeparatorOrientation.Vertical));

        var root = Root(cut);
        Assert.Equal("none", root.GetAttribute("role"));
        Assert.Equal("true", root.GetAttribute("aria-hidden"));
        Assert.False(root.HasAttribute("aria-orientation"));
    }

    [Fact]
    public void TwSeparator_Decorative_WithLabel_ReadsTheLabelAsPlainText()
    {
        var cut = Render(p => p
            .Add(x => x.Decorative, true)
            .Add(x => x.Label, "or")
            .Add(x => x.AriaLabel, "ignored")
            .Add(x => x.AriaLabelledBy, "heading"));

        var root = Root(cut);
        Assert.Equal("none", root.GetAttribute("role"));
        Assert.False(root.HasAttribute("aria-hidden"));
        Assert.False(root.HasAttribute("aria-orientation"));
        Assert.False(root.HasAttribute("aria-label"));
        Assert.False(root.HasAttribute("aria-labelledby"));
        Assert.Equal("or", root.TextContent.Trim());
    }

    [Fact]
    public void TwSeparator_WithLabel_RendersALineTheLabelAndALine()
    {
        var cut = Render(p => p.Add(x => x.Label, "or continue with"));

        var root = Root(cut);
        Assert.Equal(3, root.Children.Length);
        AssertHasClasses(root, theme.LabeledHorizontal);
        AssertHasClasses(root, theme.Thin);
        AssertHasClasses(root, theme.Neutral);
        AssertLacksClasses(root, theme.HorizontalLine);

        var label = root.Children[1];
        Assert.Equal(theme.Label, label.GetAttribute("class"));
        Assert.Equal("or continue with", label.TextContent.Trim());

        foreach (var line in new[] { root.Children[0], root.Children[2] })
        {
            Assert.Empty(line.Children);
            Assert.Equal(string.Empty, line.TextContent);
            AssertHasClasses(line, theme.LabelLine);
            AssertHasClasses(line, theme.HorizontalLine);
            AssertHasClasses(line, theme.Solid);
            AssertHasClasses(line, theme.LineStretch);
        }
    }

    [Fact]
    public void TwSeparator_Label_NamesTheSeparator()
    {
        var root = Root(Render(p => p.Add(x => x.Label, "Today")));

        Assert.Equal("separator", root.GetAttribute("role"));
        Assert.Equal("horizontal", root.GetAttribute("aria-orientation"));
        Assert.Equal("Today", root.GetAttribute("aria-label"));
    }

    [Fact]
    public void TwSeparator_AriaLabel_OverridesTheLabelAsTheName()
    {
        var root = Root(Render(p => p
            .Add(x => x.Label, "Today")
            .Add(x => x.AriaLabel, "Messages from today")));

        Assert.Equal("Messages from today", root.GetAttribute("aria-label"));
    }

    [Fact]
    public void TwSeparator_AriaLabel_NamesASeparatorWithoutALabel()
    {
        var root = Root(Render(p => p.Add(x => x.AriaLabel, "End of section")));

        Assert.Equal("End of section", root.GetAttribute("aria-label"));
        Assert.Empty(root.Children);
    }

    [Fact]
    public void TwSeparator_AriaLabelledBy_ReplacesTheName()
    {
        var root = Root(Render(p => p
            .Add(x => x.Label, "Today")
            .Add(x => x.AriaLabel, "ignored")
            .Add(x => x.AriaLabelledBy, "heading")));

        Assert.Equal("heading", root.GetAttribute("aria-labelledby"));
        Assert.False(root.HasAttribute("aria-label"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void TwSeparator_BlankLabel_IsTreatedAsNoLabel(string label)
    {
        var root = Root(Render(p => p.Add(x => x.Label, label)));

        Assert.Empty(root.Children);
        Assert.False(root.HasAttribute("aria-label"));
        AssertHasClasses(root, theme.HorizontalLine);
    }

    [Fact]
    public void TwSeparator_ChildContent_IsShownInTheLine_AndNeedsAnAriaLabelForAName()
    {
        var cut = Render(p => p.Add(x => x.ChildContent, RenderFragmentBuilder("Custom")));

        var root = Root(cut);
        Assert.Equal(3, root.Children.Length);
        Assert.Equal("Custom", root.Children[1].TextContent.Trim());
        Assert.False(root.HasAttribute("aria-label"));
    }

    [Fact]
    public void TwSeparator_Decorative_WithChildContent_ShowsTheContentAsPlainText()
    {
        var cut = Render(p => p
            .Add(x => x.Decorative, true)
            .Add(x => x.ChildContent, RenderFragmentBuilder("Custom")));

        var root = Root(cut);
        Assert.Equal("none", root.GetAttribute("role"));
        Assert.False(root.HasAttribute("aria-hidden"));
        Assert.Equal("Custom", root.Children[1].TextContent.Trim());
    }

    [Fact]
    public void TwSeparator_Decorative_WithoutLabel_IgnoresAriaNaming()
    {
        var cut = Render(p => p
            .Add(x => x.Decorative, true)
            .Add(x => x.AriaLabel, "ignored")
            .Add(x => x.AriaLabelledBy, "heading"));

        var root = Root(cut);
        Assert.Equal("true", root.GetAttribute("aria-hidden"));
        Assert.False(root.HasAttribute("aria-label"));
        Assert.False(root.HasAttribute("aria-labelledby"));
    }

    [Fact]
    public void TwSeparator_ChildContent_TakesPrecedenceOverLabelText_WhileLabelStillNamesIt()
    {
        var cut = Render(p => p
            .Add(x => x.Label, "Today")
            .Add(x => x.ChildContent, RenderFragmentBuilder("Custom")));

        var root = Root(cut);
        Assert.Equal("Custom", root.Children[1].TextContent.Trim());
        Assert.DoesNotContain("Today", root.TextContent);
        Assert.Equal("Today", root.GetAttribute("aria-label"));
    }

    [Theory]
    [InlineData(SeparatorLabelAlign.Start, true, false)]
    [InlineData(SeparatorLabelAlign.Center, false, false)]
    [InlineData(SeparatorLabelAlign.End, false, true)]
    public void TwSeparator_LabelAlign_InsetsTheLineBeforeOrAfterTheLabel(SeparatorLabelAlign align, bool startInset, bool endInset)
    {
        var root = Root(Render(p => p
            .Add(x => x.Label, "Label")
            .Add(x => x.LabelAlign, align)));

        var startLine = root.Children[0];
        var endLine = root.Children[2];
        AssertHasClasses(startLine, startInset ? theme.LineInset : theme.LineStretch);
        AssertLacksClasses(startLine, startInset ? theme.LineStretch : theme.LineInset);
        AssertHasClasses(endLine, endInset ? theme.LineInset : theme.LineStretch);
        AssertLacksClasses(endLine, endInset ? theme.LineStretch : theme.LineInset);
    }

    [Fact]
    public void TwSeparator_DefaultLabelAlign_IsCenter()
    {
        Assert.Equal(SeparatorLabelAlign.Center, new TwSeparator().LabelAlign);
    }

    [Fact]
    public void TwSeparator_VerticalWithLabel_StacksTheLinesAboveAndBelowTheLabel()
    {
        var root = Root(Render(p => p
            .Add(x => x.Orientation, SeparatorOrientation.Vertical)
            .Add(x => x.Label, "or")));

        Assert.Equal("vertical", root.GetAttribute("aria-orientation"));
        AssertHasClasses(root, theme.LabeledVertical);
        AssertLacksClasses(root, string.Join(' ', Tokens(theme.LabeledHorizontal).Except(Tokens(theme.LabeledVertical))));
        AssertHasClasses(root.Children[0], theme.VerticalLine);
        AssertHasClasses(root.Children[2], theme.VerticalLine);
        AssertLacksClasses(root.Children[0], theme.HorizontalLine);
    }

    [Theory]
    [InlineData(SeparatorVariant.Solid)]
    [InlineData(SeparatorVariant.Dashed)]
    [InlineData(SeparatorVariant.Dotted)]
    public void TwSeparator_Variant_StylesTheLine(SeparatorVariant variant)
    {
        var plain = Root(Render(p => p.Add(x => x.Variant, variant)));
        AssertHasClasses(plain, VariantFor(variant));

        var labeled = Root(Render(p => p.Add(x => x.Variant, variant).Add(x => x.Label, "or")));
        AssertLacksClasses(labeled, VariantFor(variant));
        AssertHasClasses(labeled.Children[0], VariantFor(variant));
        AssertHasClasses(labeled.Children[2], VariantFor(variant));
    }

    [Theory]
    [InlineData(SeparatorThickness.Thin)]
    [InlineData(SeparatorThickness.Medium)]
    [InlineData(SeparatorThickness.Thick)]
    public void TwSeparator_Thickness_SetsTheLineWidth(SeparatorThickness thickness)
    {
        var plain = Root(Render(p => p.Add(x => x.Thickness, thickness)));
        AssertHasClasses(plain, ThicknessFor(thickness));

        var labeled = Root(Render(p => p.Add(x => x.Thickness, thickness).Add(x => x.Label, "or")));
        AssertHasClasses(labeled, ThicknessFor(thickness));

        foreach (var other in Enum.GetValues<SeparatorThickness>().Where(t => t != thickness))
        {
            AssertLacksClasses(plain, ThicknessFor(other));
        }
    }

    [Theory]
    [InlineData(Color.Primary)]
    [InlineData(Color.Accent)]
    [InlineData(Color.Success)]
    [InlineData(Color.Danger)]
    [InlineData(Color.Warning)]
    [InlineData(Color.Info)]
    [InlineData(Color.Light)]
    [InlineData(Color.Dark)]
    public void TwSeparator_Color_ColorsTheLineAndLabel(Color color)
    {
        var plain = Root(Render(p => p.Add(x => x.Color, color)));
        AssertHasClasses(plain, PaletteValueFor(color));
        AssertLacksClasses(plain, theme.Neutral);

        var labeled = Root(Render(p => p.Add(x => x.Color, color).Add(x => x.Label, "or")));
        AssertHasClasses(labeled, PaletteValueFor(color));
        AssertHasClasses(labeled.Children[0], theme.LabelLine);
    }

    [Fact]
    public void TwSeparator_Class_IsAppended_AndWinsOverThemeClasses()
    {
        var root = Root(Render(p => p.Add(x => x.Class, "my-4 w-1/2")));

        Assert.Contains("my-4", root.ClassList);
        Assert.Contains("w-1/2", root.ClassList);
        Assert.DoesNotContain("w-full", root.ClassList);
    }

    [Fact]
    public void TwSeparator_Class_IsAppliedToTheLabeledRoot()
    {
        var root = Root(Render(p => p.Add(x => x.Label, "or").Add(x => x.Class, "my-6")));

        Assert.Contains("my-6", root.ClassList);
        Assert.DoesNotContain("my-6", root.Children[0].ClassList);
    }

    [Fact]
    public void TwSeparator_PassesStyleAndUnmatchedAttributes()
    {
        var root = Root(Render(p => p
            .Add(x => x.Style, "opacity: 0.5")
            .AddUnmatched("data-test", "sep")));

        Assert.Equal("opacity: 0.5", root.GetAttribute("style"));
        Assert.Equal("sep", root.GetAttribute("data-test"));
    }

    [Fact]
    public void TwSeparator_Defaults()
    {
        var separator = new TwSeparator();

        Assert.Equal(SeparatorOrientation.Horizontal, separator.Orientation);
        Assert.False(separator.Decorative);
        Assert.Equal(SeparatorVariant.Solid, separator.Variant);
        Assert.Equal(SeparatorThickness.Thin, separator.Thickness);
        Assert.Null(separator.Color);
        Assert.Null(separator.Label);
        Assert.Null(separator.ChildContent);
    }

    [Fact]
    public void TwSeparatorTheme_UsesLogicalBordersAndBorderLines_ForRightToLeftAndForcedColors()
    {
        // A vertical line drawn on the inline-start edge mirrors in a right to left layout, and a line drawn as a
        // border (rather than a background) stays visible in forced colors mode.
        Assert.Contains("border-s", theme.VerticalLine);
        Assert.DoesNotContain("border-l", theme.VerticalLine);
        Assert.Contains("border-t", theme.HorizontalLine);
        Assert.DoesNotContain("bg-", theme.HorizontalLine + theme.VerticalLine + theme.Base);
    }

    [Fact]
    public void TwSeparatorTheme_LineWidth_IsDrivenByTheSeparatorWidthVariable()
    {
        // The border width of both lines reads the --separator-width variable, which Thin/Medium/Thick each set to
        // a distinct value on the root element.
        Assert.Contains("length:--separator-width", theme.HorizontalLine);
        Assert.Contains("length:--separator-width", theme.VerticalLine);

        Assert.Contains("--separator-width", theme.Thin);
        Assert.Contains("--separator-width", theme.Medium);
        Assert.Contains("--separator-width", theme.Thick);

        Assert.Equal(3, new[] { theme.Thin, theme.Medium, theme.Thick }.Distinct().Count());
    }
}
