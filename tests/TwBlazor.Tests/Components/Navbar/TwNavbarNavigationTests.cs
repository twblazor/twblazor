// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using TwBlazor.Components;
using TwBlazor.Models;

namespace TwBlazor.Tests.Components.Navbar;

public class TwNavbarNavigationTests : TwBlazorTestBase
{
    private NavigationManager Navigation => TestContext.Services.GetRequiredService<NavigationManager>();

    private static List<NavigationItem> Items() =>
    [
        new() { Id = "home", Label = "Home", Href = "/" },
        new() { Id = "reports", Label = "Reports", Href = "/Reports" },
        new() { Id = "heading", Label = "No link" },
    ];

    [Fact]
    public void Item_WhoseHrefMatchesTheCurrentUrl_IsMarkedAsTheCurrentPage()
    {
        // Arrange
        Navigation.NavigateTo("reports");

        // Act
        var cut = TestContext.Render<TwNavbar>(p => p
            .Add(x => x.DisableResponsiveCollapse, true)
            .Add(x => x.NavigationItems, Items()));

        // Assert - the comparison ignores case and the leading slash
        Assert.Equal("page", cut.Find("#reports").GetAttribute("aria-current"));
        Assert.Null(cut.Find("#home").GetAttribute("aria-current"));
    }

    [Fact]
    public void Item_WithoutAnHref_IsNeverTheCurrentPage()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwNavbar>(p => p
            .Add(x => x.DisableResponsiveCollapse, true)
            .Add(x => x.NavigationItems, Items()));

        // Assert
        Assert.Null(cut.Find("#heading").GetAttribute("aria-current"));
    }

    [Fact]
    public void Navigating_MovesTheCurrentPageMarker_AndClosesTheMenu()
    {
        // Arrange
        var cut = TestContext.Render<TwNavbar>(p => p.Add(x => x.NavigationItems, Items()));
        cut.Find("button").Click();
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));

        // Act
        Navigation.NavigateTo("reports");

        // Assert
        cut.WaitForAssertion(() =>
        {
            Assert.Equal("false", cut.Find("button").GetAttribute("aria-expanded"));
            Assert.Equal("page", cut.Find("#reports").GetAttribute("aria-current"));
        });
    }

    [Fact]
    public void ToggleAriaLabel_ReplacesTheDefaultName()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwNavbar>(p => p.Add(x => x.ToggleAriaLabel, "Open site menu"));

        // Assert
        Assert.Equal("Open site menu", cut.Find("button").GetAttribute("aria-label"));
    }

    [Fact]
    public void Dispose_StopsListeningForNavigation()
    {
        // Arrange
        var cut = TestContext.Render<TwNavbar>(p => p.Add(x => x.NavigationItems, Items()));
        cut.Find("button").Click();

        // Act
        cut.Instance.Dispose();
        Navigation.NavigateTo("reports");

        // Assert - a disposed navbar no longer reacts, so its menu state is left as it was
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-expanded"));
    }
}
