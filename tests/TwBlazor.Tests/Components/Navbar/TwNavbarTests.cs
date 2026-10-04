using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;

namespace TwBlazor.Tests.Components.Navbar;

public class TwNavbarTests : TwBlazorTestBase
{
    private TwSidebarTheme sidebarTheme => Theme.Components.Require<TwSidebarTheme>();

    #region Rendering

    [Fact]
    public void ShouldRender_NavElement_WithDefaultAriaLabel()
    {
        // Act
        var cut = TestContext.Render<TwNavbar>();

        // Assert
        var nav = cut.Find("nav");
        Assert.Equal("Top navigation", nav.GetAttribute("aria-label"));
    }

    [Fact]
    public void ShouldRender_CustomAriaLabel_WhenProvided()
    {
        // Act
        var cut = TestContext.Render<TwNavbar>(p => p
            .Add(x => x.AriaLabel, "my navbar")
        );

        // Assert
        Assert.Equal("my navbar", cut.Find("nav").GetAttribute("aria-label"));
    }

    [Fact]
    public void ShouldRender_ChildContent_InsideNav()
    {
        // Act
        var cut = TestContext.Render<TwNavbar>(p => p
            .Add(x => x.ChildContent, b => b.AddMarkupContent(0, "<span class=\"inner\">Hello</span>"))
        );

        // Assert
        Assert.NotNull(cut.Find("span.inner"));
        Assert.Contains("Hello", cut.Markup);
    }

    [Fact]
    public void ShouldRender_StructuredSlots()
    {
        var cut = TestContext.Render<TwNavbar>(p => p
            .Add(x => x.BrandContent, b => b.AddMarkupContent(0, "<strong>Brand</strong>"))
            .Add(x => x.NavigationContent, b => b.AddMarkupContent(0, "<span class=\"links\">Links</span>"))
            .Add(x => x.ActionsContent, b => b.AddMarkupContent(0, "<button>Action</button>")));

        Assert.NotNull(cut.Find("strong"));
        Assert.NotNull(cut.Find("span.links"));
        Assert.NotNull(cut.Find("button"));
    }

    [Fact]
    public void ShouldRender_NavigationItems_AndMarkActiveItem()
    {
        var cut = TestContext.Render<TwNavbar>(p => p
            .Add(x => x.DisableResponsiveCollapse, true)
            .Add(x => x.NavigationItems,
            [
                new() { Label = "Home", Href = "/" },
                new() { Label = "Current", Href = "/current", IsActive = true },
                new() { Label = "Hidden", Href = "/hidden", Hidden = true }
            ]));

        Assert.Equal("page", cut.Find("a[href='/current']").GetAttribute("aria-current"));
        Assert.DoesNotContain("Hidden", cut.Markup);
    }

    [Fact]
    public void ShouldNotRender_ResponsiveToggle_WhenDisabled()
    {
        var cut = TestContext.Render<TwNavbar>(p => p
            .Add(x => x.DisableResponsiveCollapse, true));

        Assert.Empty(cut.FindAll("button"));
        Assert.Contains("flex", cut.Find("div[id$='-menu']").GetAttribute("class"));
    }

    [Fact]
    public void ShouldRender_WithNoChildContent_WhenNotProvided()
    {
        // Act
        var cut = TestContext.Render<TwNavbar>();

        // Assert
        var nav = cut.Find("nav");
        Assert.NotNull(nav.QuerySelector("div[id$='-menu']"));
    }

    #endregion

    #region Responsive Collapse Behavior

    [Fact]
    public void ShouldApply_DesktopOverrideClasses_WhenResponsiveCollapseEnabled()
    {
        // Act - lg:w-auto/lg:flex/lg:flex-row/lg:flex-1 must always be present so a menu left open
        // on a narrow viewport snaps back to the normal single-row desktop layout at desktop widths.
        var cut = TestContext.Render<TwNavbar>();

        // Assert
        var cls = cut.Find("div[id$='-menu']").GetAttribute("class");
        Assert.Contains("lg:w-auto", cls);
        Assert.Contains("lg:flex-1", cls);
        Assert.Contains("lg:flex", cls);
        Assert.Contains("lg:flex-row", cls);
    }

    [Fact]
    public void ShouldToggleMenuOpen_WhenToggleClicked()
    {
        // Act
        var cut = TestContext.Render<TwNavbar>();
        cut.Find("button").Click();

        // Assert - the open menu expands in normal flow (full width, stacked column) rather than
        // overlapping content as an absolutely-positioned overlay.
        var cls = cut.Find("div[id$='-menu']").GetAttribute("class");
        Assert.Contains("w-full", cls);
        Assert.Contains("flex-col", cls);
        Assert.DoesNotContain("absolute", cls);
        Assert.DoesNotContain("hidden", cls);
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void ShouldApply_OpaqueBackground_WhenMenuOpen()
    {
        // Arrange - the open dropdown must carry its own background (matching the navbar's) so
        // NavbarLink's light text stays legible instead of showing white-on-transparent over
        // whatever the page happens to render behind it.
        var cut = TestContext.Render<TwNavbar>();

        // Act
        cut.Find("button").Click();

        // Assert
        var cls = cut.Find("div[id$='-menu']").GetAttribute("class") ?? string.Empty;
        var expectedBackgroundToken = sidebarTheme.NavbarMobileMenu.Split(' ')[0];
        Assert.Contains(expectedBackgroundToken, cls);
    }

    [Fact]
    public void NavbarContentWrapper_ShouldNotBePositioned()
    {
        // Arrange - the wrapper has no absolutely-positioned descendant (the mobile menu expands in
        // normal flow), so it should never need "position: relative" of its own.
        var cut = TestContext.Render<TwNavbar>();

        // Act
        var wrapper = cut.Find("nav > div");

        // Assert
        var classes = (wrapper.GetAttribute("class") ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Assert.DoesNotContain("relative", classes);
    }

    #endregion

    #region Navigation

    [Fact]
    public void ShouldCloseMobileMenu_OnLocationChanged()
    {
        // Arrange - opening the menu, then navigating client-side (e.g. following a nav link),
        // must close it again since the hosting layout isn't re-rendered by the navigation itself.
        var cut = TestContext.Render<TwNavbar>();
        cut.Find("button").Click();
        Assert.Contains("flex-col", cut.Find("div[id$='-menu']").GetAttribute("class"));

        var navigationManager = TestContext.Services.GetRequiredService<NavigationManager>();

        // Act
        navigationManager.NavigateTo("/some-other-page");

        // Assert
        cut.WaitForAssertion(() =>
            Assert.Contains("hidden", cut.Find("div[id$='-menu']").GetAttribute("class")));
    }

    [Fact]
    public void ShouldUpdate_ActiveLink_AfterClientSideNavigation()
    {
        // Arrange
        var cut = TestContext.Render<TwNavbar>(p => p
            .Add(x => x.DisableResponsiveCollapse, true)
            .Add(x => x.NavigationItems,
            [
                new() { Label = "Home", Href = "/" },
                new() { Label = "Products", Href = "/products" }
            ]));

        var navigationManager = TestContext.Services.GetRequiredService<NavigationManager>();

        // Act
        navigationManager.NavigateTo("/products");

        // Assert
        cut.WaitForAssertion(() =>
            Assert.Equal("page", cut.Find("a[href='/products']").GetAttribute("aria-current")));
    }

    #endregion

    #region Base Theme Classes

    [Fact]
    public void ShouldApply_NavbarThemeBaseClasses()
    {
        // Act
        var cut = TestContext.Render<TwNavbar>();

        // Assert
        var cls = cut.Find("nav").GetAttribute("class");
        Assert.Contains(sidebarTheme.Navbar.Split(' ')[0], cls);
    }

    #endregion

    #region Fixed = false (sticky)

    [Fact]
    public void ShouldApply_StickyClasses_WhenFixed_False()
    {
        // Act
        var cut = TestContext.Render<TwNavbar>(p => p
            .Add(x => x.Fixed, false)
        );

        // Assert
        var cls = cut.Find("nav").GetAttribute("class");
        Assert.Contains("sticky", cls);
        Assert.Contains("top-0", cls);
    }

    [Fact]
    public void ShouldNotApply_FixedPositionClasses_WhenFixed_False()
    {
        // Act
        var cut = TestContext.Render<TwNavbar>(p => p
            .Add(x => x.Fixed, false)
        );

        // Assert
        var cls = cut.Find("nav").GetAttribute("class");
        Assert.DoesNotContain("left-0", cls);
        Assert.DoesNotContain("right-0", cls);
    }

    #endregion

    #region Fixed = true

    [Fact]
    public void ShouldApply_FixedClasses_WhenFixed_True()
    {
        // Act
        var cut = TestContext.Render<TwNavbar>(p => p
            .Add(x => x.Fixed, true)
        );

        // Assert
        var cls = cut.Find("nav").GetAttribute("class");
        Assert.Contains("fixed", cls);
        Assert.Contains("top-0", cls);
        Assert.Contains("left-0", cls);
        Assert.Contains("right-0", cls);
    }

    [Fact]
    public void ShouldNotApply_StickyClass_WhenFixed_True()
    {
        // Act
        var cut = TestContext.Render<TwNavbar>(p => p
            .Add(x => x.Fixed, true)
        );

        // Assert
        Assert.DoesNotContain("sticky", cut.Find("nav").GetAttribute("class"));
    }

    #endregion

    #region Class parameter

    [Fact]
    public void ShouldApply_ExtraClass_WhenClassProvided()
    {
        // Act
        var cut = TestContext.Render<TwNavbar>(p => p
            .Add(x => x.Class, "my-custom-class")
        );

        // Assert
        Assert.Contains("my-custom-class", cut.Find("nav").GetAttribute("class"));
    }

    [Fact]
    public void ShouldApply_ExtraClass_AlongsideFixedClasses()
    {
        // Act
        var cut = TestContext.Render<TwNavbar>(p => p
            .Add(x => x.Fixed, true)
            .Add(x => x.Class, "extra-fixed")
        );

        // Assert
        var cls = cut.Find("nav").GetAttribute("class");
        Assert.Contains("fixed", cls);
        Assert.Contains("extra-fixed", cls);
    }

    [Fact]
    public void ShouldApply_ExtraClass_AlongsideStickyClasses()
    {
        // Act
        var cut = TestContext.Render<TwNavbar>(p => p
            .Add(x => x.Fixed, false)
            .Add(x => x.Class, "extra-sticky")
        );

        // Assert
        var cls = cut.Find("nav").GetAttribute("class");
        Assert.Contains("sticky", cls);
        Assert.Contains("extra-sticky", cls);
    }

    #endregion

    #region Default Fixed value

    [Fact]
    public void ShouldDefault_ToSticky_WhenFixed_NotProvided()
    {
        // Act
        var cut = TestContext.Render<TwNavbar>();

        // Assert
        var cls = cut.Find("nav").GetAttribute("class");
        Assert.Contains("sticky", cls);
        Assert.DoesNotContain("left-0", cls);
    }

    #endregion
}
