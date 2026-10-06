using Bunit;
using TwBlazor.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Card;

public class TwCardTests : TwBlazorTestBase
{
    #region Rendering

    [Fact]
    public void TwCard_Renders_WithDefaultClasses()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>();

        // Assert
        var card = cut.Find("div");
        var classes = card.GetAttribute("class");
        Assert.Contains("rounded", classes);
        Assert.Contains("shadow-sm", classes);
        Assert.Contains("border-[oklch(92%_0_0)]", classes);
    }

    [Fact]
    public void TwCard_GeneratesId_WhenNotProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>();

        // Assert
        var card = cut.Find("div");
        var id = card.GetAttribute("id");
        Assert.NotNull(id);
        Assert.StartsWith("card-", id);
    }

    [Fact]
    public void TwCard_UsesProvidedId()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.Id, "my-card"));

        // Assert
        var card = cut.Find("div");
        Assert.Equal("my-card", card.GetAttribute("id"));
    }

    [Fact]
    public void TwCard_AppliesCustomClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.Class, "custom-card-class"));

        // Assert
        var card = cut.Find("div");
        Assert.Contains("custom-card-class", card.GetAttribute("class"));
    }

    #endregion

    #region ChildContent

    [Fact]
    public void TwCard_Renders_WithChildContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.ChildContent, RenderFragmentBuilder("Card body content")));

        // Assert
        var body = cut.Find("div.px-6.py-5");
        Assert.Contains("Card body content", body.TextContent);
    }

    #endregion

    #region Header

    [Fact]
    public void TwCard_DoesNotRender_Header_WhenNoTitleOrHeaderContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>();

        // Assert
        Assert.Empty(cut.FindAll("h3"));
        Assert.Empty(cut.FindAll("div.px-6.pt-5.pb-0"));
    }

    [Fact]
    public void TwCard_Renders_WithTitle()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.Title, "Card Title"));

        // Assert
        var heading = cut.Find("h3");
        Assert.Contains("Card Title", heading.TextContent);
        Assert.Contains("text-lg", heading.GetAttribute("class"));
        Assert.Contains("font-semibold", heading.GetAttribute("class"));
    }

    [Fact]
    public void TwCard_DoesNotRender_H3_WhenTitleIsEmpty()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.Title, string.Empty)
            .Add(p => p.HeaderContent, RenderFragmentBuilder("Header only")));

        // Assert
        Assert.Empty(cut.FindAll("h3"));
    }

    [Fact]
    public void TwCard_Renders_WithHeaderContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.HeaderContent, RenderFragmentBuilder("Custom header")));

        // Assert
        var header = cut.Find("div");
        Assert.Contains("Custom header", header.TextContent);
    }

    [Fact]
    public void TwCard_Renders_WithTitleAndHeaderContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.Title, "My Title")
            .Add(p => p.HeaderContent, RenderFragmentBuilder("Custom header content")));

        // Assert
        var header = cut.Find("div");
        Assert.Contains("My Title", header.TextContent);
        Assert.Contains("Custom header content", header.TextContent);
    }

    #endregion

    #region Footer

    [Fact]
    public void TwCard_DoesNotRender_Footer_WhenNoFooterContent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>();

        // Assert
        Assert.Empty(cut.FindAll("div.border-t"));
    }

    #endregion

    #region Bordered

    [Fact]
    public void TwCard_Bordered_IsTrue_ByDefault()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>();

        // Assert
        var card = cut.Find("div");
        Assert.Contains("border-[oklch(92%_0_0)]", card.GetAttribute("class"));
    }

    [Fact]
    public void TwCard_Bordered_False_DoesNotApplyBorderClasses()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.Bordered, false));

        // Assert
        var card = cut.Find("div");
        Assert.DoesNotContain("border-gray-200", card.GetAttribute("class"));
        Assert.DoesNotContain("dark:border-gray-700", card.GetAttribute("class"));
    }

    #endregion

    #region Shadow

    [Theory]
    [InlineData(Shadow.None, "shadow-none")]
    [InlineData(Shadow.Sm, "shadow-sm")]
    [InlineData(Shadow.Md, "shadow")]
    [InlineData(Shadow.Lg, "shadow-lg")]
    public void TwCard_AppliesShadow(Shadow shadow, string expectedClass)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.Shadow, shadow));

        // Assert
        var card = cut.Find("div");
        Assert.Contains(expectedClass, card.GetAttribute("class"));
    }

    #endregion

    #region Rounded

    [Theory]
    [InlineData(Rounded.None, "rounded-none")]
    [InlineData(Rounded.Sm, "rounded-sm")]
    [InlineData(Rounded.Md, "rounded")]
    [InlineData(Rounded.Lg, "rounded-lg")]
    [InlineData(Rounded.Full, "rounded-full")]
    public void TwCard_AppliesRounded(Rounded rounded, string expectedClass)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.Rounded, rounded));

        // Assert
        var card = cut.Find("div");
        Assert.Contains(expectedClass, card.GetAttribute("class"));
    }

    #endregion
}
