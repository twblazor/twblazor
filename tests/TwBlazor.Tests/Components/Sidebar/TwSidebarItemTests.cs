using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Models;
using Icons = TwBlazor.Enums.Icon;

namespace TwBlazor.Tests.Components.Sidebar;

public class TwSidebarItemTests : TwBlazorTestBase
{
    public TwSidebarItemTests()
    {
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void ShouldRender_AsParent_WithCorrectRotation_WhenCollapsedStateProvided()
    {
        // Arrange
        var cutCollapsed = TestContext.Render<TwSidebarItem>(p => p
            .Add(x => x.IsParent, true)
            .Add(x => x.IsCollapsed, true)
            .Add(x => x.Label, "Parent Collapsed")
            .Add(x => x.OnClick, EventCallback.Factory.Create(this, () => { }))
        );

        var cutExpanded = TestContext.Render<TwSidebarItem>(p => p
            .Add(x => x.IsParent, true)
            .Add(x => x.IsCollapsed, false)
            .Add(x => x.Label, "Parent Expanded")
            .Add(x => x.OnClick, EventCallback.Factory.Create(this, () => { }))
        );

        // Act
        var svgCollapsedClass = cutCollapsed.Find("svg").GetAttribute("class");
        var svgExpandedClass = cutExpanded.Find("svg").GetAttribute("class");

        // Assert
        Assert.Contains("Parent Collapsed", cutCollapsed.Markup);
        Assert.Contains("Parent Expanded", cutExpanded.Markup);
        Assert.Contains("rotate-0", svgCollapsedClass);   // collapsed -> rotate-0
        Assert.Contains("rotate-180", svgExpandedClass);  // expanded -> rotate-180
    }

    [Fact]
    public void ShouldRender_AsLink_WhenNotParent()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSidebarItem>(p => p
            .Add(x => x.IsParent, false)
            .Add(x => x.Label, "Link Label")
            .Add(x => x.Href, "/home")
        );

        var anchor = cut.Find("a");

        // Assert
        Assert.NotNull(anchor);
        Assert.Equal("/home", anchor.GetAttribute("href"));
        Assert.Contains("Link Label", anchor.TextContent);
    }

    [Fact]
    public void ShouldAppend_CustomClass_ToParentClasses_WhenProvided_And_InvokeOnClick()
    {
        // Arrange
        var clicked = false;
        var cut = TestContext.Render<TwSidebarItem>(p => p
            .Add(x => x.IsParent, true)
            .Add(x => x.IsCollapsed, true)
            .Add(x => x.Label, "Parent With Class")
            .Add(x => x.Class, "custom-parent-class")
            .Add(x => x.OnClick, EventCallback.Factory.Create(this, () => clicked = true))
        );

        var button = cut.Find("button");

        // Act
        var classAttr = button.GetAttribute("class");
        button.Click();

        // Assert
        Assert.Contains("cursor-pointer", classAttr);
        Assert.Contains("custom-parent-class", classAttr); // appended custom class
        Assert.True(clicked); // OnClick invoked
    }

    [Fact]
    public void ShouldAppend_CustomClass_ToLinkClasses_WhenProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSidebarItem>(p => p
            .Add(x => x.IsParent, false)
            .Add(x => x.Label, "Link With Class")
            .Add(x => x.Href, "/test")
            .Add(x => x.Class, "custom-link-class")
        );

        var anchor = cut.Find("a");

        // Act
        var classAttr = anchor.GetAttribute("class");

        // Assert
        Assert.Contains("hover:bg-[oklch(98%_0_0)]", classAttr);
        Assert.Contains("custom-link-class", classAttr); // appended custom class
    }

    [Theory]
    [InlineData(true, "button")]
    [InlineData(false, "a")]
    public void ShouldRender_DecorativeIcon_BeforeLabel_WhenIconProvided(bool isParent, string selector)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSidebarItem>(p => p
            .Add(x => x.IsParent, isParent)
            .Add(x => x.Label, "Home")
            .Add(x => x.Href, "/home")
            .Add(x => x.Icon, Icons.House)
        );

        var item = cut.Find(selector);
        var icon = item.FirstElementChild!;

        // Assert
        Assert.Equal("I", icon.TagName);
        Assert.Contains("bi-house", icon.GetAttribute("class"));
        Assert.Contains("shrink-0", icon.GetAttribute("class"));
        Assert.Equal("true", icon.GetAttribute("aria-hidden"));
        Assert.Equal("Home", item.TextContent.Trim());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ShouldNotRender_Icon_WhenIconNotProvided(bool isParent)
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSidebarItem>(p => p
            .Add(x => x.IsParent, isParent)
            .Add(x => x.Label, "Home")
            .Add(x => x.Href, "/home")
        );

        // Assert
        Assert.Empty(cut.FindAll("i"));
    }

    [Fact]
    public void ShouldInitialize_Icon_FromNavigationItem_WhenProvided()
    {
        // Arrange
        var navItem = new NavigationItem { Label = "Theme", Href = "/theme", Icon = Icons.Brush };

        // Act
        var cut = TestContext.Render<TwSidebarItem>(p => p.Add(x => x.NavigationItem, navItem));

        // Assert
        Assert.Contains("bi-brush", cut.Find("a > i").GetAttribute("class"));
    }

    [Fact]
    public void ShouldInitialize_Href_And_Label_FromNavigationItem_WhenProvided()
    {
        // Arrange
        var navItem = new NavigationItem
        {
            Label = "Nav Item Label",
            Href = "/nav-item"
        };

        // Act
        var cut = TestContext.Render<TwSidebarItem>(p => p
            .Add(x => x.NavigationItem, navItem)
            .Add(x => x.IsParent, false)
        );

        var anchor = cut.Find("a");

        // Assert
        Assert.Equal("/nav-item", anchor.GetAttribute("href"));
        Assert.Contains("Nav Item Label", anchor.TextContent);
    }

    [Fact]
    public void ShouldRender_EndContent_InsideTheLink_AfterTheLabel()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSidebarItem>(p => p
            .Add(x => x.Label, "Inbox")
            .Add(x => x.Href, "/inbox")
            .Add(x => x.EndContent, "<em class=\"badge\">12</em>")
        );

        // Assert
        var anchor = cut.Find("a");
        Assert.Equal("12", anchor.QuerySelector("em.badge")!.TextContent);
        Assert.True(anchor.InnerHtml.IndexOf("Inbox", StringComparison.Ordinal) < anchor.InnerHtml.IndexOf("badge", StringComparison.Ordinal));
    }

    [Fact]
    public void ShouldRender_EndContent_InsideTheParentButton_BetweenLabelAndChevron()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSidebarItem>(p => p
            .Add(x => x.IsParent, true)
            .Add(x => x.Label, "Charts")
            .Add(x => x.EndContent, "<em class=\"badge\">New</em>")
        );

        // Assert
        var html = cut.Find("button").InnerHtml;
        var badge = html.IndexOf("badge", StringComparison.Ordinal);
        Assert.True(html.IndexOf("Charts", StringComparison.Ordinal) < badge);
        Assert.True(badge < html.IndexOf("<svg", StringComparison.Ordinal));
    }

    [Fact]
    public void ShouldNotRender_EndContentWrapper_WhenNoEndContent()
    {
        // Arrange & Act
        var link = TestContext.Render<TwSidebarItem>(p => p.Add(x => x.Label, "Inbox").Add(x => x.Href, "/inbox"));
        var parent = TestContext.Render<TwSidebarItem>(p => p.Add(x => x.IsParent, true).Add(x => x.Label, "Charts"));

        // Assert
        Assert.Single(link.FindAll("a > span"));
        Assert.Single(parent.FindAll("button > span"));
    }
}
