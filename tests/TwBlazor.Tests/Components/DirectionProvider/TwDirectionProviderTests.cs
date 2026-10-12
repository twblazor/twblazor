// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using TwBlazor.Components;
using TwBlazor.Configuration;
using TwBlazor.Enums;
using ThemeBase = TwBlazor.Theme.Theme;

namespace TwBlazor.Tests.Components.DirectionProvider;

public class TwDirectionProviderTests : TwBlazorTestBase
{
    private static BunitContext CreateContext(Action<TwLocalizationOptions> configure)
    {
        var context = new BunitContext();
        context.Services.AddTwBlazor(options => configure(options.Localization), ThemeBase.CreateDefaultTheme());
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    private static RenderFragment ThreeTabs() => builder =>
    {
        builder.OpenComponent<TwTabContainer>(0);
        builder.AddAttribute(1, "ChildContent", (RenderFragment)(tabs =>
        {
            for (var i = 1; i <= 3; i++)
            {
                var label = $"Tab {i}";
                tabs.OpenComponent<TwTab>(0);
                tabs.SetKey(label);
                tabs.AddAttribute(1, "Label", label);
                tabs.AddAttribute(2, "ChildContent", (RenderFragment)(content => content.AddContent(0, label)));
                tabs.CloseComponent();
            }
        }));
        builder.CloseComponent();
    };

    private static RenderFragment Provider(TextDirection? direction, RenderFragment content) => builder =>
    {
        builder.OpenComponent<TwDirectionProvider>(0);
        builder.AddAttribute(1, "Direction", direction);
        builder.AddAttribute(2, "ChildContent", content);
        builder.CloseComponent();
    };

    private static string SelectedTab(IRenderedComponent<TwDirectionProvider> cut) =>
        cut.FindAll("button[role='tab']").Single(tab => tab.GetAttribute("aria-selected") == "true").TextContent.Trim();

    [Fact]
    public void Renders_LeftToRight_ForALeftToRightLanguage()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDirectionProvider>(p => p.AddChildContent("Content"));

        // Assert
        var root = cut.Find("div");
        Assert.Equal("ltr", root.GetAttribute("dir"));
        Assert.Equal("Content", root.TextContent.Trim());
    }

    [Theory]
    [InlineData(TextDirection.RightToLeft, "rtl")]
    [InlineData(TextDirection.LeftToRight, "ltr")]
    public void Direction_SetsTheDirAttribute(TextDirection direction, string expected)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDirectionProvider>(p => p.Add(x => x.Direction, direction));

        // Assert
        Assert.Equal(expected, cut.Find("div").GetAttribute("dir"));
    }

    [Theory]
    [InlineData("ar", "rtl")]
    [InlineData("he", "rtl")]
    [InlineData("fa", "rtl")]
    [InlineData("ur", "rtl")]
    [InlineData("en-GB", "ltr")]
    [InlineData("ja", "ltr")]
    public void WithoutADirection_FollowsTheLanguage(string culture, string expected)
    {
        // Arrange
        using var context = CreateContext(localization => localization.Culture = CultureInfo.GetCultureInfo(culture));

        // Act
        var cut = context.Render<TwDirectionProvider>();

        // Assert
        Assert.Equal(expected, cut.Find("div").GetAttribute("dir"));
    }

    [Fact]
    public void WithoutADirection_FollowsTheConfiguredDirection_OverTheLanguage()
    {
        // Arrange
        using var context = CreateContext(localization =>
        {
            localization.Culture = TestCultures.British;
            localization.Direction = TextDirection.RightToLeft;
        });

        // Act
        var cut = context.Render<TwDirectionProvider>();

        // Assert
        Assert.Equal("rtl", cut.Find("div").GetAttribute("dir"));
    }

    [Fact]
    public void Direction_WinsOverTheLanguage()
    {
        // Arrange
        using var context = CreateContext(localization => localization.Culture = CultureInfo.GetCultureInfo("ar"));

        // Act
        var cut = context.Render<TwDirectionProvider>(p => p.Add(x => x.Direction, TextDirection.LeftToRight));

        // Assert
        Assert.Equal("ltr", cut.Find("div").GetAttribute("dir"));
    }

    [Fact]
    public void NestedProvider_WithoutADirection_InheritsTheEnclosingOne()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDirectionProvider>(p => p
            .Add(x => x.Direction, TextDirection.RightToLeft)
            .Add(x => x.ChildContent, Provider(null, content => content.AddContent(0, "Inner"))));

        // Assert
        Assert.Equal(["rtl", "rtl"], cut.FindAll("div[dir]").Select(provider => provider.GetAttribute("dir")));
    }

    [Fact]
    public void NestedProvider_WithItsOwnDirection_OverridesTheEnclosingOne()
    {
        // Arrange
        var cut = TestContext.Render<TwDirectionProvider>(p => p
            .Add(x => x.Direction, TextDirection.RightToLeft)
            .Add(x => x.ChildContent, Provider(TextDirection.LeftToRight, ThreeTabs())));

        // Act
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert - the tabs sit in the left to right island, so Arrow Right still means "next"
        Assert.Equal(["rtl", "ltr"], cut.FindAll("div[dir]").Select(provider => provider.GetAttribute("dir")));
        Assert.Equal("Tab 2", SelectedTab(cut));
    }

    [Fact]
    public void ComponentsInside_SwapTheirArrowKeys_WhenRightToLeft()
    {
        // Arrange
        var cut = TestContext.Render<TwDirectionProvider>(p => p
            .Add(x => x.Direction, TextDirection.RightToLeft)
            .Add(x => x.ChildContent, ThreeTabs()));

        // Act
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        // Assert
        Assert.Equal("Tab 2", SelectedTab(cut));
    }

    [Fact]
    public void ChangingDirection_UpdatesTheComponentsInside()
    {
        // Arrange
        var cut = TestContext.Render<TwDirectionProvider>(p => p
            .Add(x => x.Direction, TextDirection.LeftToRight)
            .Add(x => x.ChildContent, ThreeTabs()));

        // Act
        cut.Render(p => p
            .Add(x => x.Direction, TextDirection.RightToLeft)
            .Add(x => x.ChildContent, ThreeTabs()));
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        // Assert
        Assert.Equal("rtl", cut.Find("div[dir]").GetAttribute("dir"));
        Assert.Equal("Tab 2", SelectedTab(cut));
    }

    [Fact]
    public void Renders_IdClassStyleAndAttributes_OnTheRoot()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwDirectionProvider>(p => p
            .Add(x => x.Id, "app-direction")
            .Add(x => x.Class, "contents")
            .Add(x => x.Style, "color: red;")
            .AddUnmatched("data-testid", "direction"));

        // Assert
        var root = cut.Find("div");
        Assert.Equal("app-direction", root.Id);
        Assert.Equal("contents", root.GetAttribute("class"));
        Assert.Equal("color: red;", root.GetAttribute("style"));
        Assert.Equal("direction", root.GetAttribute("data-testid"));
    }

    [Fact]
    public void DirAttribute_CannotBeOverriddenByAnUnmatchedAttribute()
    {
        // Arrange & Act - the attribute and the direction handed to the components inside have to agree
        var cut = TestContext.Render<TwDirectionProvider>(p => p
            .Add(x => x.Direction, TextDirection.RightToLeft)
            .AddUnmatched("dir", "ltr"));

        // Assert
        Assert.Equal("rtl", cut.Find("div").GetAttribute("dir"));
    }
}
