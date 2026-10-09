using Bunit;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Avatar;

public class TwAvatarTests : TwBlazorTestBase
{
    private TwAvatarTheme theme => Theme.Components.Require<TwAvatarTheme>();

    public TwAvatarTests()
    {
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void TwAvatar_GeneratesId_WhenNotProvided_AndUsesProvidedId()
    {
        Assert.StartsWith("avatar-", TestContext.Render<TwAvatar>().Find("span").GetAttribute("id"));
        Assert.Equal("me", TestContext.Render<TwAvatar>(p => p.Add(x => x.Id, "me")).Find("span").GetAttribute("id"));
    }

    [Theory]
    [InlineData("Jane Doe", "JD")]
    [InlineData("  mary   anne   smith ", "MS")]
    [InlineData("Cher", "C")]
    [InlineData("élodie durand", "ÉD")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData(null, "")]
    public void GetInitials_TakesTheFirstLettersOfTheFirstAndLastWords(string? name, string expected)
    {
        Assert.Equal(expected, TwAvatar.GetInitials(name));
    }

    [Fact]
    public void GetInitials_KeepsACharacterOutsideTheBasicPlaneWhole()
    {
        Assert.Equal("\U0001F600D", TwAvatar.GetInitials("\U0001F600mile Doe"));
    }

    [Fact]
    public void TwAvatar_WithName_ShowsItsInitials_AsTextDirectlyInTheRoot()
    {
        var cut = TestContext.Render<TwAvatar>(p => p.Add(x => x.Name, "Jane Doe"));

        var root = cut.Find("span");
        Assert.Equal("JD", root.TextContent.Trim());
        Assert.Empty(root.Children);
    }

    [Fact]
    public void TwAvatar_Initials_OverrideTheOnesFromTheName()
    {
        var cut = TestContext.Render<TwAvatar>(p => p.Add(x => x.Name, "Design Team").Add(x => x.Initials, " DS "));

        Assert.Equal("DS", cut.Find("span").TextContent.Trim());
    }

    [Fact]
    public void TwAvatar_WithoutPictureOrInitials_ShowsTheIcon()
    {
        var cut = TestContext.Render<TwAvatar>(p => p.Add(x => x.Icon, TwBlazor.Enums.Icon.Robot));

        Assert.Empty(cut.FindAll("img"));
        Assert.Contains("bi-robot", cut.Markup);
        Assert.Contains(theme.Icon, cut.Markup);
    }

    [Fact]
    public void TwAvatar_WithSrc_ShowsThePicture_InsteadOfInitials()
    {
        var cut = TestContext.Render<TwAvatar>(p => p.Add(x => x.Src, "/me.png").Add(x => x.Name, "Jane Doe"));

        var image = cut.Find("img");
        Assert.Equal("/me.png", image.GetAttribute("src"));
        Assert.Equal(string.Empty, image.GetAttribute("alt"));
        Assert.Equal(theme.Image, image.GetAttribute("class"));
        Assert.DoesNotContain("JD", cut.Find("span").TextContent);
    }

    [Fact]
    public void TwAvatar_FallsBackToInitials_WhenThePictureFailsToLoad()
    {
        var cut = TestContext.Render<TwAvatar>(p => p.Add(x => x.Src, "/missing.png").Add(x => x.Name, "Jane Doe"));

        cut.Find("img").TriggerEvent("onerror", EventArgs.Empty);

        Assert.Empty(cut.FindAll("img"));
        Assert.Equal("JD", cut.Find("span").TextContent.Trim());
    }

    [Fact]
    public void TwAvatar_FallsBackToInitials_WhenThePictureHadAlreadyFailedBeforeRendering()
    {
        TestContext.JSInterop.Setup<bool>("twAvatar.isBroken", _ => true).SetResult(true);

        var cut = TestContext.Render<TwAvatar>(p => p.Add(x => x.Src, "/missing.png").Add(x => x.Name, "Jane Doe"));

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll("img")));
        Assert.Equal("JD", cut.Find("span").TextContent.Trim());
    }

    [Fact]
    public void TwAvatar_TriesAgain_WhenGivenANewPictureAfterAFailure()
    {
        var cut = TestContext.Render<TwAvatar>(p => p.Add(x => x.Src, "/missing.png").Add(x => x.Name, "Jane Doe"));
        cut.Find("img").TriggerEvent("onerror", EventArgs.Empty);

        cut.Render(p => p.Add(x => x.Src, "/found.png"));

        Assert.Equal("/found.png", cut.Find("img").GetAttribute("src"));
    }

    [Fact]
    public void TwAvatar_WithName_IsAnImageNamedAfterThePerson()
    {
        var root = TestContext.Render<TwAvatar>(p => p.Add(x => x.Name, "Jane Doe")).Find("span");

        Assert.Equal("img", root.GetAttribute("role"));
        Assert.Equal("Jane Doe", root.GetAttribute("aria-label"));
        Assert.Null(root.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void TwAvatar_AriaLabel_TakesPrecedenceOverTheName()
    {
        var root = TestContext.Render<TwAvatar>(p => p.Add(x => x.Name, "Jane Doe").Add(x => x.AriaLabel, "Your profile")).Find("span");

        Assert.Equal("Your profile", root.GetAttribute("aria-label"));
    }

    [Fact]
    public void TwAvatar_AriaLabelledBy_NamesTheAvatar_WithoutADuplicateLabel()
    {
        var root = TestContext.Render<TwAvatar>(p => p.Add(x => x.Name, "Jane Doe").Add(x => x.AriaLabelledBy, "author")).Find("span");

        Assert.Equal("img", root.GetAttribute("role"));
        Assert.Equal("author", root.GetAttribute("aria-labelledby"));
        Assert.Null(root.GetAttribute("aria-label"));
    }

    [Fact]
    public void TwAvatar_WithoutAName_IsHiddenFromAssistiveTechnology()
    {
        var root = TestContext.Render<TwAvatar>(p => p.Add(x => x.Initials, "JD")).Find("span");

        Assert.Equal("true", root.GetAttribute("aria-hidden"));
        Assert.Null(root.GetAttribute("role"));
        Assert.Null(root.GetAttribute("aria-label"));
    }

    [Theory]
    [InlineData(AvatarSize.Small)]
    [InlineData(AvatarSize.Medium)]
    [InlineData(AvatarSize.Large)]
    [InlineData(AvatarSize.ExtraLarge)]
    [InlineData((AvatarSize)99)]
    public void TwAvatar_AppliesTheSizeClasses(AvatarSize size)
    {
        var expected = size switch
        {
            AvatarSize.Small => theme.Small,
            AvatarSize.Large => theme.Large,
            AvatarSize.ExtraLarge => theme.ExtraLarge,
            _ => theme.Medium
        };

        var cut = TestContext.Render<TwAvatar>(p => p.Add(x => x.Size, size));

        Assert.Contains(expected, cut.Find("span").GetAttribute("class"));
    }

    [Fact]
    public void TwAvatar_DefaultsToMediumNeutralAndRound()
    {
        var classes = TestContext.Render<TwAvatar>().Find("span").GetAttribute("class")!;

        Assert.Contains(theme.Base, classes);
        Assert.Contains(theme.Medium, classes);
        Assert.Contains(theme.Neutral, classes);
        Assert.Contains(Theme.Rounded.Full, classes);
    }

    [Fact]
    public void TwAvatar_AppliesTheColorFromThePalette()
    {
        var classes = TestContext.Render<TwAvatar>(p => p.Add(x => x.Color, Color.Success)).Find("span").GetAttribute("class")!;

        Assert.Contains(theme.Colors.Success, classes);
        Assert.DoesNotContain(theme.Neutral, classes);
    }

    [Fact]
    public void TwAvatar_Rounded_ChangesTheShape()
    {
        var classes = TestContext.Render<TwAvatar>(p => p.Add(x => x.Rounded, Rounded.Lg)).Find("span").ClassList;

        Assert.Contains(Theme.Rounded.Lg, classes);
        Assert.DoesNotContain(Theme.Rounded.Full, classes);
    }

    [Fact]
    public void TwAvatar_AppliesCustomClassStyleAndAttributes()
    {
        var cut = TestContext.Render<TwAvatar>(p => p
            .Add(x => x.Class, "ring-2")
            .Add(x => x.Style, "opacity:0.5")
            .AddUnmatched("data-testid", "avatar"));

        var root = cut.Find("span");
        Assert.Contains("ring-2", root.GetAttribute("class"));
        Assert.Equal("opacity:0.5", root.GetAttribute("style"));
        Assert.Equal("avatar", root.GetAttribute("data-testid"));
    }
}