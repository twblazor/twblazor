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

    [Fact]
    public void TwCard_Renders_FooterContent_InADividedFooter()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.ChildContent, RenderFragmentBuilder("Body"))
            .Add(p => p.FooterContent, RenderFragmentBuilder("Footer actions")));

        // Assert
        var footer = cut.Find("div.border-t");
        Assert.Contains("Footer actions", footer.TextContent);
        Assert.DoesNotContain("Body", footer.TextContent);
    }

    [Fact]
    public void TwCard_Footer_IsTheLastSection()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.Title, "Title")
            .Add(p => p.FooterContent, RenderFragmentBuilder("Footer")));

        // Assert
        var card = cut.Find("div");
        Assert.Contains("border-t", card.LastElementChild!.ClassList);
    }

    [Fact]
    public void TwCard_ContentArea_Grows_SoTheFooterSitsAtTheBottom()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.ChildContent, RenderFragmentBuilder("Body"))
            .Add(p => p.FooterContent, RenderFragmentBuilder("Footer")));

        // Assert
        Assert.Contains("flex-col", cut.Find("div").ClassList);
        Assert.Contains("flex-1", cut.Find("div.px-6.py-5").ClassList);
    }

    #endregion

    #region Media

    [Fact]
    public void TwCard_DoesNotRender_Image_WhenNoImageSrc()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.Title, "Title"));

        // Assert
        Assert.Empty(cut.FindAll("img"));
    }

    [Fact]
    public void TwCard_Renders_Image_WithAltText_AndLazyLoading()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.ImageSrc, "/images/bread.webp")
            .Add(p => p.ImageAlt, "A sourdough loaf on a board"));

        // Assert
        var image = cut.Find("img");
        Assert.Equal("/images/bread.webp", image.GetAttribute("src"));
        Assert.Equal("A sourdough loaf on a board", image.GetAttribute("alt"));
        Assert.Equal("lazy", image.GetAttribute("loading"));
        Assert.Contains("object-cover", image.ClassList);
        Assert.Contains("aspect-video", image.ClassList);
    }

    [Fact]
    public void TwCard_Image_WithoutAlt_IsDecorative()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.ImageSrc, "/images/pattern.webp"));

        // Assert - an empty alt tells screen readers to skip the image, rather than read out the file name.
        Assert.Equal(string.Empty, cut.Find("img").GetAttribute("alt"));
    }

    [Fact]
    public void TwCard_ImageClass_IsMergedOverTheTheme()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.ImageSrc, "/images/milk.webp")
            .Add(p => p.ImageClass, "aspect-square"));

        // Assert
        var image = cut.Find("img");
        Assert.Contains("aspect-square", image.ClassList);
        Assert.DoesNotContain("aspect-video", image.ClassList);
    }

    [Fact]
    public void TwCard_Media_IsFirst_AndRoundsOnlyItsTopCorners()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.ImageSrc, "/images/eggs.webp")
            .Add(p => p.Title, "Eggs")
            .Add(p => p.Rounded, Rounded.Lg));

        // Assert
        var media = cut.Find("div").FirstElementChild!;
        Assert.NotNull(media.QuerySelector("img"));
        Assert.Contains("rounded-t-lg", media.ClassList);
        Assert.Contains("overflow-hidden", media.ClassList);
    }

    [Fact]
    public void TwCard_MediaContent_RendersOverTheImage()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.ImageSrc, "/images/strawberries.webp")
            .Add(p => p.MediaContent, RenderFragmentBuilder("Offer")));

        // Assert
        var media = cut.Find("div").FirstElementChild!;
        Assert.Contains("relative", media.ClassList);
        Assert.NotNull(media.QuerySelector("img"));
        Assert.Contains("Offer", media.TextContent);
    }

    [Fact]
    public void TwCard_MediaContent_WithoutImage_IsTheMedia()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.MediaContent, RenderFragmentBuilder("Video player")));

        // Assert
        Assert.Empty(cut.FindAll("img"));
        Assert.Contains("Video player", cut.Find("div").FirstElementChild!.TextContent);
    }

    [Fact]
    public void TwCard_ImageOnly_RendersNoEmptyContentArea()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwCard>(parameters => parameters
            .Add(p => p.ImageSrc, "/images/hero.webp"));

        // Assert
        Assert.Empty(cut.FindAll("div.px-6.py-5"));
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
