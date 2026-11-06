using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Localization;

namespace TwBlazor.Tests.Components.Empty;

public class TwEmptyTests : TwBlazorTestBase
{
    private TwEmptyTheme theme => Theme.Components.Require<TwEmptyTheme>();

    private static string ClassesOf(AngleSharp.Dom.IElement element) => element.GetAttribute("class") ?? string.Empty;

    private static void AssertHasClasses(string actual, string expected)
    {
        var classes = actual.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.Ordinal);
        foreach (var expectedClass in expected.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            Assert.Contains(expectedClass, classes);
        }
    }

    private static void AssertLacksClasses(string actual, string unexpected)
    {
        var classes = actual.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.Ordinal);
        foreach (var unexpectedClass in unexpected.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            Assert.DoesNotContain(unexpectedClass, classes);
        }
    }

    private static string PaletteValueFor(TwEmptyTheme theme, Color color) => color switch
    {
        Color.Primary => theme.TileColors.Primary,
        Color.Accent => theme.TileColors.Accent,
        Color.Success => theme.TileColors.Success,
        Color.Danger => theme.TileColors.Danger,
        Color.Warning => theme.TileColors.Warning,
        Color.Info => theme.TileColors.Info,
        Color.Light => theme.TileColors.Light,
        Color.Dark => theme.TileColors.Dark,
        _ => throw new ArgumentOutOfRangeException(nameof(color))
    };

    // ---- Defaults ----

    [Fact]
    public void TwEmpty_Renders_WithDefaults()
    {
        var cut = TestContext.Render<TwEmpty>();

        var root = cut.Find("div");
        AssertHasClasses(ClassesOf(root), theme.Base);
        AssertHasClasses(ClassesOf(root), theme.Medium);
        Assert.Null(root.GetAttribute("role"));
    }

    [Fact]
    public void TwEmpty_DefaultTitle_IsLocalized()
    {
        var cut = TestContext.Render<TwEmpty>();

        var heading = cut.Find("h3");
        Assert.Equal(Localizer[TwStrings.Empty_Title], heading.TextContent);
        Assert.Equal("No data", heading.TextContent);
    }

    [Fact]
    public void TwEmpty_DefaultIcon_IsInboxOnATile()
    {
        var cut = TestContext.Render<TwEmpty>();

        var tile = cut.Find("span[aria-hidden='true']");
        AssertHasClasses(ClassesOf(tile), theme.Tile);
        AssertHasClasses(ClassesOf(tile), theme.TileMedium);
        AssertHasClasses(ClassesOf(tile), theme.TileNeutral);
        Assert.Equal("true", tile.GetAttribute("aria-hidden"));
        Assert.Contains("bi-inbox", ClassesOf(tile.QuerySelector("i")!));
    }

    [Fact]
    public void TwEmpty_Icon_IsHiddenFromAssistiveTechnology()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.MediaVariant, EmptyMediaVariant.Plain));

        Assert.Equal("true", cut.Find("i").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void TwEmpty_NoDescription_RendersNoParagraph()
    {
        var cut = TestContext.Render<TwEmpty>();

        Assert.Empty(cut.FindAll("p"));
    }

    [Fact]
    public void TwEmpty_NoActions_RendersNoActionRow()
    {
        var cut = TestContext.Render<TwEmpty>();

        Assert.Empty(cut.FindAll("button"));
        Assert.Empty(cut.FindAll("a"));
        Assert.DoesNotContain(cut.FindAll("div"), d => ClassesOf(d) == theme.Actions);
    }

    // ---- Title and description ----

    [Fact]
    public void TwEmpty_Title_RendersInHeading()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Id, "orders")
            .Add(x => x.Title, "No orders yet"));

        var heading = cut.Find("h3");
        Assert.Equal("No orders yet", heading.TextContent);
        Assert.Equal("orders-title", heading.GetAttribute("id"));
        AssertHasClasses(ClassesOf(heading), theme.Title);
        AssertHasClasses(ClassesOf(heading), theme.TitleMedium);
    }

    [Fact]
    public void TwEmpty_EmptyTitle_HidesTheTitle()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Title, string.Empty)
            .Add(x => x.Description, "Only a description"));

        Assert.Empty(cut.FindAll("h1, h2, h3, h4, h5, h6"));
        Assert.Equal("Only a description", cut.Find("p").TextContent);
    }

    [Fact]
    public void TwEmpty_NoTitleOrDescription_RendersNoHeader()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Title, string.Empty));

        Assert.Empty(cut.FindAll("h1, h2, h3, h4, h5, h6"));
        Assert.DoesNotContain(cut.FindAll("div"), d => ClassesOf(d) == theme.Header);
    }

    [Theory]
    [InlineData(1, "h1")]
    [InlineData(2, "h2")]
    [InlineData(4, "h4")]
    [InlineData(6, "h6")]
    [InlineData(0, "h1")]
    [InlineData(9, "h6")]
    public void TwEmpty_TitleLevel_SetsAndClampsTheHeading(int level, string expectedTag)
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Title, "Nothing")
            .Add(x => x.TitleLevel, level));

        Assert.Equal("Nothing", cut.Find(expectedTag).TextContent);
    }

    [Fact]
    public void TwEmpty_Description_RendersParagraph()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Id, "orders")
            .Add(x => x.Description, "Orders you place will appear here."));

        var description = cut.Find("p");
        Assert.Equal("Orders you place will appear here.", description.TextContent);
        Assert.Equal("orders-description", description.GetAttribute("id"));
        AssertHasClasses(ClassesOf(description), theme.Description);
        AssertHasClasses(ClassesOf(description), theme.DescriptionMedium);
    }

    [Fact]
    public void TwEmpty_ChildContent_RendersBelowTheDescription()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Description, "Description")
            .Add(x => x.ChildContent, RenderFragmentBuilder("Extra hint")));

        var content = cut.FindAll("div").Single(d => ClassesOf(d) == theme.Content);
        Assert.Equal("Extra hint", content.TextContent);
        var markup = cut.Markup;
        Assert.True(markup.IndexOf("Description", StringComparison.Ordinal) < markup.IndexOf("Extra hint", StringComparison.Ordinal));
    }

    // ---- Media ----

    [Fact]
    public void TwEmpty_Icon_CanBeChanged()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Icon, TwBlazor.Enums.Icon.Search));

        Assert.Contains("bi-search", ClassesOf(cut.Find("i")));
    }

    [Fact]
    public void TwEmpty_NullIcon_RendersNoMedia()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Icon, (TwBlazor.Enums.Icon?)null));

        Assert.Empty(cut.FindAll("i"));
        Assert.DoesNotContain(cut.FindAll("div"), d => ClassesOf(d) == theme.Media);
    }

    [Fact]
    public void TwEmpty_PlainMedia_RendersIconWithoutTile()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.MediaVariant, EmptyMediaVariant.Plain));

        var icon = cut.Find("i");
        AssertHasClasses(ClassesOf(icon), theme.PlainIconMedium);
        AssertHasClasses(ClassesOf(icon), theme.PlainIcon);
        Assert.DoesNotContain(cut.FindAll("span"), s => ClassesOf(s).Contains(theme.Tile, StringComparison.Ordinal));
    }

    [Fact]
    public void TwEmpty_PlainMedia_WithColor_UsesTheIconColor()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.MediaVariant, EmptyMediaVariant.Plain)
            .Add(x => x.Color, Color.Danger));

        var classes = ClassesOf(cut.Find("i"));
        AssertHasClasses(classes, IconColorBuilder.GetIconColor(Color.Danger));
        AssertLacksClasses(classes, theme.PlainIcon);
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
    public void TwEmpty_Color_TintsTheTile(Color color)
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Color, color));

        var tile = cut.Find("span[aria-hidden='true']");
        AssertHasClasses(ClassesOf(tile), PaletteValueFor(theme, color));
        AssertLacksClasses(ClassesOf(tile), theme.TileNeutral);
    }

    [Theory]
    [InlineData(Rounded.None, "rounded-none")]
    [InlineData(Rounded.Md, "rounded")]
    [InlineData(Rounded.Full, "rounded-lg")]
    public void TwEmpty_Tile_FollowsTheSurfaceRounding(Rounded rounded, string expected)
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Rounded, rounded));

        var tile = cut.Find("span[aria-hidden='true']");
        Assert.Contains(expected, tile.ClassList);
    }

    [Fact]
    public void TwEmpty_AvatarName_ReplacesTheIcon()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.AvatarName, "Jane Doe"));

        var avatar = cut.FindComponent<TwAvatar>();
        Assert.Equal("Jane Doe", avatar.Instance.Name);
        Assert.Equal(AvatarSize.Large, avatar.Instance.Size);
        Assert.Contains("JD", avatar.Markup);
        Assert.DoesNotContain("bi-inbox", cut.Markup);
    }

    [Fact]
    public void TwEmpty_Avatar_IsHiddenFromAssistiveTechnology()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.AvatarName, "Jane Doe"));

        var avatar = cut.Find("[role=img]");
        Assert.Equal("true", avatar.ParentElement!.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void TwEmpty_AvatarSrc_RendersPicture()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.AvatarSrc, "/jane.png"));

        Assert.Equal("/jane.png", cut.Find("img").GetAttribute("src"));
    }

    [Theory]
    [InlineData(Size.Sm, AvatarSize.Medium)]
    [InlineData(Size.Md, AvatarSize.Large)]
    [InlineData(Size.Lg, AvatarSize.ExtraLarge)]
    public void TwEmpty_AvatarSize_FollowsSize(Size size, AvatarSize expected)
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Size, size)
            .Add(x => x.AvatarName, "Jane Doe"));

        Assert.Equal(expected, cut.FindComponent<TwAvatar>().Instance.Size);
    }

    [Fact]
    public void TwEmpty_MediaContent_ReplacesIconAndAvatar()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.AvatarName, "Jane Doe")
            .Add(x => x.MediaContent, (RenderFragment)(b => b.AddMarkupContent(0, "<svg class=\"custom-media\"></svg>"))));

        Assert.NotNull(cut.Find("svg.custom-media"));
        Assert.Empty(cut.FindComponents<TwAvatar>());
        Assert.Empty(cut.FindAll("i"));
    }

    // ---- Actions ----

    [Fact]
    public void TwEmpty_ActionText_RendersPrimaryButton_AndInvokesOnAction()
    {
        var clicks = 0;
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Id, "projects")
            .Add(x => x.ActionText, "Create project")
            .Add(x => x.ActionIcon, TwBlazor.Enums.Icon.Plus_Lg)
            .Add(x => x.OnAction, () => clicks++));

        var button = cut.Find("#projects-action");
        Assert.Contains("Create project", button.TextContent);
        Assert.Contains("bi-plus-lg", button.InnerHtml);
        var actionButton = cut.FindComponent<TwButton>();
        Assert.Equal(Color.Primary, actionButton.Instance.Color);

        button.Click();
        Assert.Equal(1, clicks);
    }

    [Fact]
    public void TwEmpty_ActionHref_RendersLink()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.ActionText, "Shop")
            .Add(x => x.ActionHref, "/shop"));

        var link = cut.Find("a");
        Assert.Equal("/shop", link.GetAttribute("href"));
        Assert.Contains("Shop", link.TextContent);
        Assert.Empty(cut.FindAll("button"));
    }

    [Fact]
    public void TwEmpty_SecondaryAction_RendersOutlinedButton_AfterThePrimary()
    {
        var secondaryClicks = 0;
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Id, "projects")
            .Add(x => x.ActionText, "Create")
            .Add(x => x.SecondaryActionText, "Import")
            .Add(x => x.OnSecondaryAction, () => secondaryClicks++));

        var buttons = cut.FindComponents<TwButton>();
        Assert.Equal(2, buttons.Count);
        Assert.Equal("Create", buttons[0].Instance.Label);
        Assert.Equal("Import", buttons[1].Instance.Label);
        Assert.Equal(ButtonVariant.Outlined, buttons[1].Instance.Variant);

        cut.Find("#projects-secondary-action").Click();
        Assert.Equal(1, secondaryClicks);
    }

    [Fact]
    public void TwEmpty_SecondaryActionHref_RendersLink()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.SecondaryActionText, "Docs")
            .Add(x => x.SecondaryActionHref, "/docs"));

        Assert.Equal("/docs", cut.Find("a").GetAttribute("href"));
    }

    [Fact]
    public void TwEmpty_ActionContent_RendersAfterBuiltInActions()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.ActionText, "Create")
            .Add(x => x.ActionContent, (RenderFragment)(b => b.AddMarkupContent(0, "<a class=\"custom-action\" href=\"/help\">Help</a>"))));

        var actions = cut.FindAll("div").Single(d => ClassesOf(d) == theme.Actions);
        var children = actions.Children;
        Assert.Equal("button", children[0].TagName, ignoreCase: true);
        Assert.Contains("custom-action", children[^1].ClassList);
    }

    [Fact]
    public void TwEmpty_ActionContent_AloneRendersTheActionRow()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.ActionContent, (RenderFragment)(b => b.AddMarkupContent(0, "<span class=\"only\">x</span>"))));

        var actions = cut.FindAll("div").Single(d => ClassesOf(d) == theme.Actions);
        Assert.NotNull(actions.QuerySelector(".only"));
        Assert.Empty(cut.FindComponents<TwButton>());
    }

    [Fact]
    public void TwEmpty_Actions_FollowTheRounding()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Rounded, Rounded.None)
            .Add(x => x.ActionText, "Create")
            .Add(x => x.SecondaryActionText, "Import"));

        Assert.All(cut.FindComponents<TwButton>(), b => Assert.Equal(Rounded.None, b.Instance.Rounded));
    }

    // ---- Sizes ----

    [Theory]
    [InlineData(Size.Sm)]
    [InlineData(Size.Md)]
    [InlineData(Size.Lg)]
    public void TwEmpty_Size_AppliesSizeClassesToEveryPart(Size size)
    {
        var (container, tile, title, description) = size switch
        {
            Size.Sm => (theme.Small, theme.TileSmall, theme.TitleSmall, theme.DescriptionSmall),
            Size.Lg => (theme.Large, theme.TileLarge, theme.TitleLarge, theme.DescriptionLarge),
            _ => (theme.Medium, theme.TileMedium, theme.TitleMedium, theme.DescriptionMedium)
        };

        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Size, size)
            .Add(x => x.Title, "Title")
            .Add(x => x.Description, "Description"));

        AssertHasClasses(ClassesOf(cut.Find("div")), container);
        AssertHasClasses(ClassesOf(cut.Find("span[aria-hidden='true']")), tile);
        AssertHasClasses(ClassesOf(cut.Find("h3")), title);
        AssertHasClasses(ClassesOf(cut.Find("p")), description);
    }

    [Theory]
    [InlineData(Size.Sm)]
    [InlineData(Size.Md)]
    [InlineData(Size.Lg)]
    public void TwEmpty_Size_ScalesThePlainIcon(Size size)
    {
        var expected = size switch
        {
            Size.Sm => theme.PlainIconSmall,
            Size.Lg => theme.PlainIconLarge,
            _ => theme.PlainIconMedium
        };

        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Size, size)
            .Add(x => x.MediaVariant, EmptyMediaVariant.Plain));

        AssertHasClasses(ClassesOf(cut.Find("i")), expected);
    }

    // ---- Surface ----

    [Fact]
    public void TwEmpty_Default_HasNoBorderBackgroundOrRounding()
    {
        var cut = TestContext.Render<TwEmpty>();

        var classes = ClassesOf(cut.Find("div"));
        AssertLacksClasses(classes, theme.Outlined);
        AssertLacksClasses(classes, theme.Filled);
        Assert.DoesNotContain(cut.Find("div").ClassList, c => c == "rounded" || c.StartsWith("rounded-", StringComparison.Ordinal));
    }

    [Fact]
    public void TwEmpty_Outlined_AddsDashedBorderAndSurfaceRounding()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Outlined, true));

        var root = cut.Find("div");
        AssertHasClasses(ClassesOf(root), theme.Outlined);
        Assert.Contains("border-dashed", root.ClassList);
        Assert.Contains(RoundedBuilder.GetRounded(Theme.Rounded.DefaultRounded), root.ClassList);
    }

    [Fact]
    public void TwEmpty_Filled_AddsBackground()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Filled, true));

        var root = cut.Find("div");
        AssertHasClasses(ClassesOf(root), theme.Filled);
        AssertLacksClasses(ClassesOf(root), "border-dashed");
    }

    [Fact]
    public void TwEmpty_OutlinedAndFilled_Combine()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Outlined, true)
            .Add(x => x.Filled, true));

        var classes = ClassesOf(cut.Find("div"));
        AssertHasClasses(classes, theme.Outlined);
        AssertHasClasses(classes, theme.Filled);
    }

    [Theory]
    [InlineData(Rounded.None, "rounded-none")]
    [InlineData(Rounded.Sm, "rounded-sm")]
    [InlineData(Rounded.Full, "rounded-lg")]
    public void TwEmpty_Outlined_RoundingIsCappedForASurface(Rounded rounded, string expected)
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Outlined, true)
            .Add(x => x.Rounded, rounded));

        var radius = cut.Find("div").ClassList.Where(c => c == "rounded" || c.StartsWith("rounded-", StringComparison.Ordinal)).ToList();
        Assert.Equal([expected], radius);
    }

    [Fact]
    public void TwEmpty_Shadow_IsOnlyAppliedWhenSet()
    {
        var withoutShadow = TestContext.Render<TwEmpty>();
        Assert.DoesNotContain(withoutShadow.Find("div").ClassList, c => c.StartsWith("shadow", StringComparison.Ordinal));

        var withShadow = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Shadow, Shadow.Lg));
        Assert.Contains(ShadowBuilder.GetShadow(Shadow.Lg), withShadow.Find("div").ClassList);
    }

    // ---- Accessibility ----

    [Fact]
    public void TwEmpty_Live_IsAStatus()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Live, true));

        Assert.Equal("status", cut.Find("div").GetAttribute("role"));
    }

    [Fact]
    public void TwEmpty_AriaLabel_MakesItANamedRegion()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.AriaLabel, "Orders"));

        var root = cut.Find("div");
        Assert.Equal("region", root.GetAttribute("role"));
        Assert.Equal("Orders", root.GetAttribute("aria-label"));
    }

    [Fact]
    public void TwEmpty_AriaLabelledBy_MakesItANamedRegion()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.AriaLabelledBy, "heading"));

        var root = cut.Find("div");
        Assert.Equal("region", root.GetAttribute("role"));
        Assert.Equal("heading", root.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void TwEmpty_LiveWithAriaLabel_StaysAStatus()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Live, true)
            .Add(x => x.AriaLabel, "Search results"));

        Assert.Equal("status", cut.Find("div").GetAttribute("role"));
    }

    // ---- Base parameters ----

    [Fact]
    public void TwEmpty_Class_IsAppliedLast()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Class, "my-empty"));

        var classes = ClassesOf(cut.Find("div")).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal("my-empty", classes[^1]);
    }

    [Fact]
    public void TwEmpty_IdStyleAndAttributes_AreRendered()
    {
        var cut = TestContext.Render<TwEmpty>(p => p
            .Add(x => x.Id, "empty-1")
            .Add(x => x.Style, "min-height: 10rem")
            .AddUnmatched("data-test", "value"));

        var root = cut.Find("div");
        Assert.Equal("empty-1", root.GetAttribute("id"));
        Assert.Equal("min-height: 10rem", root.GetAttribute("style"));
        Assert.Equal("value", root.GetAttribute("data-test"));
    }

    [Fact]
    public void TwEmpty_GeneratesAnId_WhenNotSet()
    {
        var cut = TestContext.Render<TwEmpty>();

        var id = cut.Find("div").GetAttribute("id");
        Assert.StartsWith("empty-", id);
        Assert.Equal($"{id}-title", cut.Find("h3").GetAttribute("id"));
    }
}
