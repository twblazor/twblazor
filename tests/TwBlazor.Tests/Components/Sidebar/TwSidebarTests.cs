using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Models;

namespace TwBlazor.Tests.Components.Sidebar;

public class TwSidebarTests : TwBlazorTestBase
{
    private TwSidebarTheme sidebarTheme => Theme.Components.Require<TwSidebarTheme>();

    public TwSidebarTests()
    {
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private static List<NavigationItem> BuildSearchNavigationItems() =>
    [
        new() { Label = "Home", Href = "/home" },
        new() { Label = "Get started", Href = "/get-started" },
        new()
        {
            Label = "Forms",
            NavigationItems =
            [
                new() { Label = "Button", Href = "/button" },
                new() { Label = "Checkbox", Href = "/checkbox" },
            ]
        },
        new()
        {
            Label = "Data",
            NavigationItems =
            [
                new() { Label = "Data Table", Href = "/data-table" },
                new() { Label = "Table", Href = "/table" },
            ]
        },
    ];

    [Fact]
    public void ShouldRender_SkipLink_And_MainContent()
    {
        // Arrange
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.ChildContent, b => b.AddMarkupContent(0, "<main>Main area</main>"))
        );

        // Act
        var skipLink = cut.Find("a.sr-only");
        var main = cut.Find("#main-content-root");

        // Assert - the skip link's href must point at an id that actually exists in the DOM
        // (#main-content-root, the wrapper around the navbar + page content), not a dangling
        // #main-content id that nothing carries.
        Assert.NotNull(skipLink);
        Assert.Equal("#main-content-root", skipLink.GetAttribute("href"));
        Assert.NotNull(main);
        Assert.Contains("Main area", cut.Markup);
    }

    [Fact]
    public void SkipLink_IsVisuallyHidden_ButRevealed_OnFocus()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSidebar>();

        // Assert - sr-only alone would leave the link permanently invisible even when it receives
        // keyboard focus; a focus:not-sr-only (or equivalent) override is required so sighted
        // keyboard users can actually see the link once they Tab to it.
        var skipLink = cut.Find("a.sr-only");
        var classAttribute = skipLink.GetAttribute("class") ?? string.Empty;
        Assert.Contains("focus:not-sr-only", classAttribute);
    }

    [Fact]
    public void ShouldToggleSidebar_WhenToggleButtonClicked()
    {
        // Arrange
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSidebarOpen, false)
            .Add(x => x.ChildContent, b => b.AddMarkupContent(0, "<div>content</div>"))
        );

        // Act & Assert - Initial state: closed
        var toggleButton = cut.Find("button");
        Assert.Equal("Open sidebar", toggleButton.GetAttribute("aria-label"));

        // Click to open
        toggleButton.Click();
        cut.Render();

        // Assert - After click: open
        toggleButton = cut.Find("button");
        Assert.Equal("Close sidebar", toggleButton.GetAttribute("aria-label"));

        // Click to close
        toggleButton.Click();
        cut.Render();

        // Assert - After second click: closed again
        toggleButton = cut.Find("button");
        Assert.Equal("Open sidebar", toggleButton.GetAttribute("aria-label"));
    }

    [Fact]
    public void ShouldRender_SearchInput_WhenIsSearchable()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSearchable, true)
        );

        var input = cut.Find("input[type='text']");

        // Assert
        Assert.NotNull(input);
        Assert.Equal("Search...", input.GetAttribute("placeholder"));
    }

    [Fact]
    public void ShouldNotRender_SearchInput_WhenIsSearchableFalse()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSearchable, false)
        );

        // Assert
        Assert.Empty(cut.FindAll("input[type='search']"));
    }

    [Fact]
    public void ShouldRender_Header_And_Navbar_Content()
    {
        // Arrange
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.HeaderContent, (b => b.AddMarkupContent(0, "<div class=\"hdr\">header</div>")))
            .Add(x => x.NavbarContent, (b => b.AddMarkupContent(0, "<div class=\"navcontent\">nav</div>")))
            .Add(x => x.ChildContent, (b => b.AddMarkupContent(0, "<div class=\"body\">body</div>")))
        );

        // Act & Assert
        Assert.Contains("hdr", cut.Markup);
        Assert.Contains("navcontent", cut.Markup);
        Assert.Contains("body", cut.Markup);
    }

    [Fact]
    public void ShouldPassThrough_NavbarBrandNavigationAndActionsContent_ToEmbeddedNavbar()
    {
        // Arrange & Act - these forward straight to the embedded TwNavbar's own slots, distinct from
        // NavbarContent (which lands in TwNavbar's plain ChildContent alongside the drawer toggle).
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.NavbarBrandContent, b => b.AddMarkupContent(0, "<strong class=\"brand\">Acme</strong>"))
            .Add(x => x.NavbarNavigationContent, b => b.AddMarkupContent(0, "<a class=\"navlink\" href=\"/\">Home</a>"))
            .Add(x => x.NavbarActionsContent, b => b.AddMarkupContent(0, "<button class=\"navaction\">Sign in</button>"))
        );

        // Assert
        var navbar = cut.Find("nav[aria-label='Top navigation']");
        Assert.NotNull(navbar.QuerySelector("strong.brand"));
        Assert.NotNull(navbar.QuerySelector("a.navlink"));
        Assert.NotNull(navbar.QuerySelector("button.navaction"));
    }

    [Fact]
    public void ShouldPassThrough_NavbarNavigationItems_ToEmbeddedNavbar()
    {
        // Arrange & Act - independent of NavigationItems, which populates the sidebar itself.
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.NavbarNavigationItems,
            [
                new() { Label = "Home", Href = "/" },
                new() { Label = "Products", Href = "/products" }
            ])
        );

        // Assert
        var navbar = cut.Find("nav[aria-label='Top navigation']");
        Assert.NotNull(navbar.QuerySelector("a[href='/']"));
        Assert.NotNull(navbar.QuerySelector("a[href='/products']"));
    }

    [Fact]
    public void ShouldApply_CorrectSidebar_And_Toggle_And_MainClasses_WhenIsSidebarOpen_True()
    {
        // Arrange
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSidebarOpen, true)
            .Add(x => x.SidebarClass, "sidebar-extra")
            .Add(x => x.NavbarClass, "navbar-extra")
            .Add(x => x.ToggleButtonClass, "toggle-extra")
            .Add(x => x.MainContentClass, "main-extra")
            .Add(x => x.MainContentRootClass, "root-extra")
        );

        // Act
        var sidebarNav = cut.Find("nav[aria-label='sidebar navigation']");
        var navbar = cut.Find("nav[aria-label='Top navigation']");
        var mainContentRoot = cut.Find("#main-content-root");
        var mainContent = mainContentRoot.QuerySelector(":scope > nav + div");

        // Assert
        Assert.Contains("translate-x-0", sidebarNav.GetAttribute("class"));
        Assert.Contains("sidebar-extra", sidebarNav.GetAttribute("class"));
        Assert.Contains("navbar-extra", navbar.GetAttribute("class"));
        Assert.Contains("root-extra", mainContentRoot.GetAttribute("class"));
        Assert.Contains("main-extra", mainContent!.GetAttribute("class"));
    }

    [Fact]
    public void ShouldApply_CorrectSidebar_And_Toggle_And_MainClasses_WhenIsSidebarOpen_False()
    {
        // Arrange
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSidebarOpen, false)
            .Add(x => x.SidebarClass, "sidebar-extra")
            .Add(x => x.NavbarClass, "navbar-extra")
            .Add(x => x.ToggleButtonClass, "toggle-extra")
            .Add(x => x.MainContentClass, "main-extra")
            .Add(x => x.MainContentRootClass, "root-extra")
        );

        // Act
        var sidebarNavClosed = cut.Find("nav[aria-label='sidebar navigation']");
        var navbarClosed = cut.Find("nav[aria-label='Top navigation']");
        var mainContentRootClosed = cut.Find("#main-content-root");
        var mainContentClosed = mainContentRootClosed.QuerySelector(":scope > nav + div");

        // Assert
        Assert.Contains("-translate-x-full", sidebarNavClosed.GetAttribute("class"));
        Assert.Contains("sidebar-extra", sidebarNavClosed.GetAttribute("class"));
        Assert.Contains("navbar-extra", navbarClosed.GetAttribute("class"));
        Assert.Contains("root-extra", mainContentRootClosed.GetAttribute("class"));
        Assert.Contains("main-extra", mainContentClosed!.GetAttribute("class"));
    }

    [Fact]
    public void ShouldInvoke_IsSidebarOpenChanged_WhenToggled_FromFalseToTrue()
    {
        // Arrange
        var invoked = false;
        bool? received = null;

        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSidebarOpen, false)
            .Add(x => x.IsSidebarOpenChanged, EventCallback.Factory.Create(this, (bool v) =>
            {
                invoked = true;
                received = v;
            }))
        );

        // Act
        var toggle = cut.Find("button");
        toggle.Click();

        // Assert
        Assert.True(invoked);
        Assert.NotNull(received);
        Assert.True(received.Value);
    }

    [Fact]
    public void ShouldInvoke_IsSidebarOpenChanged_WhenToggled_FromTrueToFalse()
    {
        // Arrange
        var invoked = false;
        bool? received = null;

        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSidebarOpen, true)
            .Add(x => x.IsSidebarOpenChanged, EventCallback.Factory.Create(this, (bool v) =>
            {
                invoked = true;
                received = v;
            }))
        );

        // Act
        var toggle = cut.Find("button");
        toggle.Click();

        // Assert
        Assert.True(invoked);
        Assert.NotNull(received);
        Assert.False(received.Value);
    }

    [Fact]
    public void ShouldRender_ParentWithChildren_WhenNavigationItemIsParentAndCollapsedTrue()
    {
        // Arrange
        var parent = new NavigationItem
        {
            Label = "Parent",
            Collapsed = true,
            NavigationItems =
            [
                new() { Label = "Child1", Href = "/c1" },
                new() { Label = "Child2", Href = "/c2" }
            ]
        };

        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.NavigationItems, [parent])
        );

        // Act
        var button = cut.Find("button");
        var ul = cut.Find("ul");
        var childAnchors = cut.FindAll("a[href]").Where(c => !c.ClassName!.Contains("sr-only")).ToList();

        // Assert
        Assert.Contains("Parent", button.TextContent);
        Assert.Equal(2, childAnchors.Count);
        Assert.Equal("/c1", childAnchors[0].GetAttribute("href"));
        Assert.Equal("/c2", childAnchors[1].GetAttribute("href"));
    }

    [Fact]
    public void ShouldRender_ThreeLevelsDeep_WhenNavigationItemsAreNestedGroups()
    {
        // Arrange - TwSidebarNavigationList must recurse to arbitrary depth, e.g. a "Dates & Time"
        // group nested inside "Forms", itself nested at the sidebar's top level.
        var grandparent = new NavigationItem
        {
            Label = "Forms",
            Collapsed = false,
            NavigationItems =
            [
                new()
                {
                    Label = "Dates & Time",
                    Collapsed = false,
                    NavigationItems =
                    [
                        new() { Label = "Date Picker", Href = "/date-picker" },
                        new() { Label = "Time Picker", Href = "/time-picker" }
                    ]
                }
            ]
        };

        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.NavigationItems, [grandparent])
        );

        // Act
        var buttons = cut.FindAll("button").Where(b => b.TextContent.Contains("Forms") || b.TextContent.Contains("Dates & Time")).ToList();
        var leafAnchors = cut.FindAll("a[href]").Where(a => !a.ClassName!.Contains("sr-only")).ToList();

        // Assert - both group levels render as their own toggle buttons, and both leaves at the
        // third level render as links.
        Assert.Equal(2, buttons.Count);
        Assert.Contains(buttons, b => b.TextContent.Contains("Forms"));
        Assert.Contains(buttons, b => b.TextContent.Contains("Dates & Time"));
        Assert.Equal(2, leafAnchors.Count);
        Assert.Contains(leafAnchors, a => a.GetAttribute("href") == "/date-picker");
        Assert.Contains(leafAnchors, a => a.GetAttribute("href") == "/time-picker");
    }

    [Fact]
    public void ShouldRender_FourLevelsDeep_WhenNavigationItemsAreNestedGroups()
    {
        // Arrange - one level deeper still, confirming TwSidebarNavigationList's recursion has no
        // hardcoded depth limit: Root -> Branch -> Twig -> leaves.
        var root = new NavigationItem
        {
            Label = "Root",
            Collapsed = false,
            NavigationItems =
            [
                new()
                {
                    Label = "Branch",
                    Collapsed = false,
                    NavigationItems =
                    [
                        new()
                        {
                            Label = "Twig",
                            Collapsed = false,
                            NavigationItems =
                            [
                                new() { Label = "Leaf1", Href = "/leaf1" },
                                new() { Label = "Leaf2", Href = "/leaf2" }
                            ]
                        }
                    ]
                }
            ]
        };

        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.NavigationItems, [root])
        );

        // Act
        var buttons = cut.FindAll("button")
            .Where(b => b.TextContent.Contains("Root") || b.TextContent.Contains("Branch") || b.TextContent.Contains("Twig"))
            .ToList();
        var leafAnchors = cut.FindAll("a[href]").Where(a => !a.ClassName!.Contains("sr-only")).ToList();

        // Assert - all three group levels render as their own toggle buttons, and both leaves at
        // the fourth level render as links.
        Assert.Equal(3, buttons.Count);
        Assert.Contains(buttons, b => b.TextContent.Contains("Root"));
        Assert.Contains(buttons, b => b.TextContent.Contains("Branch"));
        Assert.Contains(buttons, b => b.TextContent.Contains("Twig"));
        Assert.Equal(2, leafAnchors.Count);
        Assert.Contains(leafAnchors, a => a.GetAttribute("href") == "/leaf1");
        Assert.Contains(leafAnchors, a => a.GetAttribute("href") == "/leaf2");
    }

    private static NavigationItem BuildFourLevelTree() => new()
    {
        Label = "Root",
        Collapsed = false,
        NavigationItems =
        [
            new()
            {
                Label = "Branch",
                Collapsed = false,
                NavigationItems =
                [
                    new()
                    {
                        Label = "Twig",
                        Collapsed = false,
                        NavigationItems =
                        [
                            new() { Label = "Leaf1", Href = "/leaf1" },
                            new() { Label = "Leaf2", Href = "/leaf2" }
                        ]
                    }
                ]
            }
        ]
    };

    [Fact]
    public void ShouldApply_LevelDeepAccent_WhenThirdLevelGroupIsOpen()
    {
        // Arrange & Act - Twig sits at depth 2 (the third level), so its own open toggle gets the
        // deep-level accent.
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.NavigationItems, [BuildFourLevelTree()])
        );

        var twigButton = cut.FindAll("button").Single(b => b.TextContent.Contains("Twig"));

        // Assert - "oklch" is the distinctive token identifying the deep-level accent color, since
        // "border-l-2" alone is too generic to distinguish it from other border utilities.
        Assert.Contains(sidebarTheme.NavigationItemActiveLevelDeep.Split(' ').First(t => t.Contains("oklch")), twigButton.GetAttribute("class"));
    }

    [Fact]
    public void ShouldApply_GroupRail_ToWrapperSpanningParentRowAndDeepChildContainer()
    {
        // Arrange & Act - Branch's children (Twig) sit three levels deep, so the wrapper spanning
        // Branch's own toggle row and its child container should carry the guide rail, connecting the
        // two visually instead of the rail only starting where the children begin. Root's own children
        // (Branch) are just one level deep, so Root's wrapper should not.
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.NavigationItems, [BuildFourLevelTree()])
        );

        var rootButton = cut.FindAll("button").Single(b => b.TextContent.Contains("Root"));
        var branchButton = cut.FindAll("button").Single(b => b.TextContent.Contains("Branch"));
        var railToken = sidebarTheme.NavigationGroupRailDeep.Split(' ').First(t => t.Contains("oklch"));

        // Assert
        Assert.Contains(railToken, branchButton.ParentElement!.GetAttribute("class"));
        Assert.DoesNotContain(railToken, rootButton.ParentElement!.GetAttribute("class") ?? string.Empty);
    }

    [Fact]
    public void ShouldNotApply_GroupRail_WhenDeepGroupIsCollapsed()
    {
        // Arrange - Branch's children (Twig) still sit three levels deep, but Branch itself is
        // collapsed, so its child container is hidden and the connecting rail should not show either.
        var tree = BuildFourLevelTree();
        tree.NavigationItems[0].Collapsed = true; // Branch

        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.NavigationItems, [tree])
        );

        // Act
        var branchButton = cut.FindAll("button").Single(b => b.TextContent.Contains("Branch"));
        var railToken = sidebarTheme.NavigationGroupRailDeep.Split(' ').First(t => t.Contains("oklch"));

        // Assert
        Assert.DoesNotContain(railToken, branchButton.ParentElement!.GetAttribute("class") ?? string.Empty);
    }

    [Fact]
    public void ShouldHide_ChildList_WhenNavigationItemIsParentAndCollapsedFalse()
    {
        // Arrange
        var parent = new NavigationItem
        {
            Label = "Parent",
            Collapsed = false, // Not collapsed means children should be visible
            NavigationItems =
            [
                new() { Label = "Child1", Href = "/c1" }
            ]
        };

        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.NavigationItems, [parent])
        );

        // Act
        var divElements = cut.FindAll("div").Where(d => d.ClassList.Contains("hidden")).ToList();
        var childAnchors = cut.FindAll("a[href]").Where(c => !c.ClassName!.Contains("sr-only")).ToList();

        // Assert
        Assert.Empty(divElements); // When not collapsed, there should be NO divs with hidden class
        var childAnchor = Assert.Single(childAnchors); // Child link should be visible
        Assert.Equal("/c1", childAnchor.GetAttribute("href"));
    }

    [Fact]
    public void ShouldRender_SingleNavigationItem_WhenNoChildren()
    {
        // Arrange
        var singleItem = new NavigationItem
        {
            Label = "Single Item",
            Href = "/single"
        };

        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.NavigationItems, [singleItem])
        );

        // Act
        var anchors = cut.FindAll("a[href]").Where(c => !c.ClassName!.Contains("sr-only")).ToList();

        // Assert
        var anchor = Assert.Single(anchors);
        Assert.Equal("/single", anchor.GetAttribute("href"));
    }

    [Fact]
    public void ShouldRender_SidebarContent_WhenProvided()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.SidebarContent, (b => b.AddMarkupContent(0, "<div class=\"custom-sidebar\">Custom Content</div>")))
        );

        // Assert
        Assert.Contains("custom-sidebar", cut.Markup);
        Assert.Contains("Custom Content", cut.Markup);
    }

    [Fact]
    public void ShouldNotInvoke_IsSidebarOpenChanged_WhenNoDelegate()
    {
        // Arrange
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSidebarOpen, false)
        );

        // Act & Assert - Should not throw
        // The toggle button is now an icon, so just verify it exists and can be clicked
        var toggleButton = cut.Find("button");
        toggleButton.Click();

        // Verify component still renders after click
        Assert.NotNull(cut.Find("#main-content-root"));
    }

    [Fact]
    public void Search_ShouldFilter_TopLevelItems_ByLabel()
    {
        // Arrange
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSearchable, true)
            .Add(x => x.NavigationItems, BuildSearchNavigationItems())
        );

        // Act
        cut.Find("input").Input("Home");

        // Assert
        var anchors = cut.FindAll("a[href]").Where(a => !a.ClassName!.Contains("sr-only")).ToList();
        var anchor = Assert.Single(anchors);
        Assert.Equal("/home", anchor.GetAttribute("href"));
    }

    [Fact]
    public void Search_ShouldFilter_CaseInsensitively()
    {
        // Arrange
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSearchable, true)
            .Add(x => x.NavigationItems, BuildSearchNavigationItems())
        );

        // Act
        cut.Find("input").Input("GET STARTED");

        // Assert
        var anchors = cut.FindAll("a[href]").Where(a => !a.ClassName!.Contains("sr-only")).ToList();
        var anchor = Assert.Single(anchors);
        Assert.Equal("/get-started", anchor.GetAttribute("href"));
    }

    [Fact]
    public void Search_ShouldShowParent_WithMatchingChildren_WhenParentDoesNotMatch()
    {
        // Arrange
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSearchable, true)
            .Add(x => x.NavigationItems, BuildSearchNavigationItems())
        );

        // Act
        cut.Find("input").Input("Button");

        // Assert - "Forms" parent appears (with Button child), "Data" does not appear
        var buttons = cut.FindAll("button").Where(b => b.TextContent.Contains("Forms")).ToList();
        Assert.Single(buttons);

        var anchors = cut.FindAll("a[href]").Where(a => !a.ClassName!.Contains("sr-only")).ToList();
        var anchor = Assert.Single(anchors);
        Assert.Equal("/button", anchor.GetAttribute("href"));
    }

    [Fact]
    public void Search_ShouldExpandParent_WhenChildrenMatch()
    {
        // Arrange
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSearchable, true)
            .Add(x => x.NavigationItems, BuildSearchNavigationItems())
        );

        // Act
        cut.Find("input").Input("Button");

        // Assert - the child list should be visible (no hidden class on its container)
        var hiddenDivs = cut.FindAll("div").Where(d => d.ClassList.Contains("hidden")).ToList();
        Assert.Empty(hiddenDivs);
    }

    [Fact]
    public void Search_ShouldShowNoItems_WhenNoMatch()
    {
        // Arrange
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSearchable, true)
            .Add(x => x.NavigationItems, BuildSearchNavigationItems())
        );

        // Act
        cut.Find("input").Input("zzznomatch");

        // Assert
        var anchors = cut.FindAll("a[href]").Where(a => !a.ClassName!.Contains("sr-only")).ToList();
        var parentButtons = cut.FindAll("button").Where(b => b.GetAttribute("aria-label") == null).ToList();
        Assert.Empty(anchors);
        Assert.Empty(parentButtons);
    }

    [Fact]
    public void Search_ShouldRestoreAllItems_WhenSearchCleared()
    {
        // Arrange
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSearchable, true)
            .Add(x => x.NavigationItems, BuildSearchNavigationItems())
        );
        var input = cut.Find("input");
        input.Input("Button");

        // Act - clear search
        input.Input(string.Empty);

        // Assert - all top-level items are back
        var anchors = cut.FindAll("a[href]").Where(a => !a.ClassName!.Contains("sr-only")).ToList();
        var parentButtons = cut.FindAll("button").Where(b => b.GetAttribute("aria-label") == null).ToList();
        Assert.Equal(6, anchors.Count);   // Home, Get started + 2 under Forms + 2 under Data
        Assert.Equal(2, parentButtons.Count); // Forms, Data
    }

    [Fact]
    public void Search_ShouldMatchMultipleChildren_AcrossDifferentParents()
    {
        // Arrange
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSearchable, true)
            .Add(x => x.NavigationItems, BuildSearchNavigationItems())
        );

        // Act - "table" matches "Data Table" and "Table" under Data
        cut.Find("input").Input("table");

        // Assert - only "Data" parent shown with both matching children
        var parentButtons = cut.FindAll("button").Where(b => b.GetAttribute("aria-label") == null).ToList();
        var parentButton = Assert.Single(parentButtons);
        Assert.Contains("Data", parentButton.TextContent);

        var anchors = cut.FindAll("a[href]").Where(a => !a.ClassName!.Contains("sr-only")).ToList();
        Assert.Equal(2, anchors.Count);
        Assert.Contains(anchors, a => a.GetAttribute("href") == "/data-table");
        Assert.Contains(anchors, a => a.GetAttribute("href") == "/table");
    }

    [Fact]
    public void ParentItem_UsesExplicitId_WhenSupplied()
    {
        // Arrange - GetParentItemId falls back to a hashcode-derived id only when the consumer
        // doesn't supply one; when NavigationItem.Id is set, it must be used verbatim.
        var parent = new NavigationItem
        {
            Id = "custom-parent-id",
            Label = "Parent",
            NavigationItems = [new() { Label = "Child1", Href = "/c1" }]
        };

        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.NavigationItems, [parent])
        );

        // Assert
        var button = cut.Find("button");
        Assert.Equal("custom-parent-id", button.GetAttribute("id"));
        Assert.Equal("custom-parent-id-children", button.GetAttribute("aria-controls"));
    }

    [Fact]
    public void ParentItem_FallsBackToHashcodeId_WhenIdNotSupplied()
    {
        // Arrange
        var parent = new NavigationItem
        {
            Label = "Parent",
            NavigationItems = [new() { Label = "Child1", Href = "/c1" }]
        };

        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.NavigationItems, [parent])
        );

        // Assert
        var button = cut.Find("button");
        Assert.StartsWith("sidebar-item-", button.GetAttribute("id"));
    }

    [Fact]
    public void ShouldNotThrow_WhenNavigationItemsParameterIsNull()
    {
        // Arrange & Act - NavigationItems defaults to [] but a consumer could still pass null
        // explicitly; OnParametersSet must fall back to an empty list rather than throwing.
        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.NavigationItems, null!)
        );

        var items = cut.FindAll("a[href]").Where(a => !a.ClassName!.Contains("sr-only"));

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    public void ShouldRender_OnlySidebarDrawerToggle_NotNavbarOwnToggle()
    {
        // Arrange & Act - the embedded TwNavbar always has its own responsive toggle disabled, so
        // the sidebar's own drawer toggle is the only button inside it.
        var cut = TestContext.Render<TwSidebar>();

        // Assert
        var navbar = cut.Find("nav[aria-label='Top navigation']");
        Assert.Single(navbar.QuerySelectorAll("button"));
    }

    #region Navigation

    [Fact]
    public void ShouldUpdate_ActiveLink_AfterClientSideNavigation()
    {
        // Arrange - a layout hosting TwSidebar isn't re-rendered by client-side navigation, so
        // without a LocationChanged subscription the active-link highlight would never refresh.
        var cut = TestContext.Render<TwSidebar>(p => p
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

    [Fact]
    public void ShouldCloseSidebar_OnLocationChanged_WhenMobileViewport()
    {
        // Arrange - on mobile, IsSidebarOpen represents a transient overlay that should close once
        // a link inside it is followed, matching TwNavbar's own close-on-navigate behavior.
        TestContext.JSInterop.Setup<bool>("twSidebar.isMobileViewport").SetResult(true);

        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSidebarOpen, true)
        );

        var navigationManager = TestContext.Services.GetRequiredService<NavigationManager>();

        // Act
        navigationManager.NavigateTo("/some-other-page");

        // Assert
        cut.WaitForAssertion(() =>
            Assert.Contains("-translate-x-full", cut.Find("nav[aria-label='sidebar navigation']").GetAttribute("class")));
    }

    [Fact]
    public void ShouldNotCloseSidebar_OnLocationChanged_WhenDesktopViewport()
    {
        // Arrange - on desktop, IsSidebarOpen represents a persistent panel (sidebarClasses has no
        // lg: reset), so navigating must never auto-close it there.
        TestContext.JSInterop.Setup<bool>("twSidebar.isMobileViewport").SetResult(false);

        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSidebarOpen, true)
        );

        var navigationManager = TestContext.Services.GetRequiredService<NavigationManager>();

        // Act
        navigationManager.NavigateTo("/some-other-page");

        // Assert
        cut.WaitForAssertion(() =>
            Assert.Contains("translate-x-0", cut.Find("nav[aria-label='sidebar navigation']").GetAttribute("class")));
    }

    [Fact]
    public void ShouldInvoke_IsSidebarOpenChanged_WhenClosedByLocationChanged()
    {
        // Arrange
        TestContext.JSInterop.Setup<bool>("twSidebar.isMobileViewport").SetResult(true);

        var invoked = false;
        bool? received = null;

        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSidebarOpen, true)
            .Add(x => x.IsSidebarOpenChanged, EventCallback.Factory.Create(this, (bool v) =>
            {
                invoked = true;
                received = v;
            }))
        );

        var navigationManager = TestContext.Services.GetRequiredService<NavigationManager>();

        // Act
        navigationManager.NavigateTo("/some-other-page");

        // Assert
        cut.WaitForAssertion(() =>
        {
            Assert.True(invoked);
            Assert.False(received);
        });
    }

    [Fact]
    public void ShouldScrollMainContentToTop_OnLocationChanged()
    {
        // Arrange
        var cut = TestContext.Render<TwSidebar>();
        var navigationManager = TestContext.Services.GetRequiredService<NavigationManager>();

        // Act
        navigationManager.NavigateTo("/some-other-page");

        // Assert
        cut.WaitForAssertion(() =>
            Assert.Single(TestContext.JSInterop.Invocations["twSidebar.scrollToTop"]));
    }

    [Fact]
    public void ShouldNotScrollMainContentToTop_OnLocationChanged_WhenNavigatingToFragment()
    {
        // Arrange
        TestContext.JSInterop.Setup<bool>("twSidebar.isMobileViewport").SetResult(false);

        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSidebarOpen, true)
        );
        var navigationManager = TestContext.Services.GetRequiredService<NavigationManager>();

        // Act
        navigationManager.NavigateTo("/some-other-page#section");

        // Assert - the location handler has run once its viewport check was invoked
        cut.WaitForAssertion(() =>
            Assert.Single(TestContext.JSInterop.Invocations["twSidebar.isMobileViewport"]));
        Assert.Empty(TestContext.JSInterop.Invocations["twSidebar.scrollToTop"]);
    }

    #endregion

    [Fact]
    public void Search_ShouldExcludeItem_WithNullLabel()
    {
        // Arrange - FilterNavigationItem's label match uses item.Label?.Contains(...) ?? false,
        // so an item with no label at all must never match a search term.
        var items = new List<NavigationItem>
        {
            new() { Label = null, Href = "/unlabeled" },
            new() { Label = "Home", Href = "/home" }
        };

        var cut = TestContext.Render<TwSidebar>(p => p
            .Add(x => x.IsSearchable, true)
            .Add(x => x.NavigationItems, items)
        );

        // Act
        cut.Find("input").Input("home");

        // Assert
        var anchors = cut.FindAll("a[href]").Where(a => !a.ClassName!.Contains("sr-only")).ToList();
        var anchor = Assert.Single(anchors);
        Assert.Equal("/home", anchor.GetAttribute("href"));
    }
}
