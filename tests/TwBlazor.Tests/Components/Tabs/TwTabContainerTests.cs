using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using TwBlazor.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Tabs;

public class TwTabContainerTests : TwBlazorTestBase
{
    [Fact]
    public void TwTabContainer_Renders_WithDefaultValues()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();
            }));

        // Assert
        var container = cut.Find("div");
        Assert.NotNull(container);

        var tablist = cut.Find("div[role='tablist']");
        Assert.NotNull(tablist);
        Assert.Contains("flex flex-wrap", tablist.GetAttribute("class"));
        Assert.Contains("border-b", tablist.GetAttribute("class"));

        var tabpanel = cut.Find("div[role='tabpanel']");
        Assert.NotNull(tabpanel);
    }

    [Fact]
    public void TwTabContainer_Renders_MultipleTabs()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();

                builder.OpenComponent<TwTab>(4);
                builder.AddAttribute(5, "Label", "Tab 2");
                builder.AddAttribute(6, "ChildContent", (RenderFragment)(b => b.AddContent(7, "Content 2")));
                builder.CloseComponent();

                builder.OpenComponent<TwTab>(8);
                builder.AddAttribute(9, "Label", "Tab 3");
                builder.AddAttribute(10, "ChildContent", (RenderFragment)(b => b.AddContent(11, "Content 3")));
                builder.CloseComponent();
            }));

        // Assert
        var buttons = cut.FindAll("button[role='tab']");
        Assert.Equal(3, buttons.Count);
        Assert.Contains("Tab 1", buttons[0].TextContent);
        Assert.Contains("Tab 2", buttons[1].TextContent);
        Assert.Contains("Tab 3", buttons[2].TextContent);
    }

    [Fact]
    public void TwTabContainer_FirstTab_IsActiveByDefault()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();

                builder.OpenComponent<TwTab>(4);
                builder.AddAttribute(5, "Label", "Tab 2");
                builder.AddAttribute(6, "ChildContent", (RenderFragment)(b => b.AddContent(7, "Content 2")));
                builder.CloseComponent();
            }));

        // Assert
        var buttons = cut.FindAll("button[role='tab']");
        Assert.Equal("true", buttons[0].GetAttribute("aria-selected"));
        Assert.Equal("false", buttons[1].GetAttribute("aria-selected"));

        var tabpanel = cut.Find("div[role='tabpanel']");
        Assert.Contains("Content 1", tabpanel.TextContent);
        Assert.DoesNotContain("Content 2", tabpanel.TextContent);
    }

    [Fact]
    public void TwTabContainer_ViewTab_ChangesActiveTab()
    {
        // Arrange
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();

                builder.OpenComponent<TwTab>(4);
                builder.AddAttribute(5, "Label", "Tab 2");
                builder.AddAttribute(6, "ChildContent", (RenderFragment)(b => b.AddContent(7, "Content 2")));
                builder.CloseComponent();
            }));

        var buttons = cut.FindAll("button[role='tab']");

        // Act
        buttons[1].Click();

        // Assert
        var updatedButtons = cut.FindAll("button[role='tab']");
        Assert.Equal("false", updatedButtons[0].GetAttribute("aria-selected"));
        Assert.Equal("true", updatedButtons[1].GetAttribute("aria-selected"));

        var tabpanel = cut.Find("div[role='tabpanel']");
        Assert.DoesNotContain("Content 1", tabpanel.TextContent);
        Assert.Contains("Content 2", tabpanel.TextContent);
    }

    [Fact]
    public void TwTabContainer_ViewTab_DoesNotChangeActiveTab_WhenTabIsDisabled()
    {
        // Arrange
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();

                builder.OpenComponent<TwTab>(4);
                builder.AddAttribute(5, "Label", "Tab 2");
                builder.AddAttribute(6, "ChildContent", (RenderFragment)(b => b.AddContent(7, "Content 2")));
                builder.AddAttribute(8, "Disabled", true);
                builder.CloseComponent();
            }));

        var buttons = cut.FindAll("button[role='tab']");

        // Act
        buttons[1].Click();

        // Assert
        var updatedButtons = cut.FindAll("button[role='tab']");
        Assert.Equal("true", updatedButtons[0].GetAttribute("aria-selected"));
        Assert.Equal("false", updatedButtons[1].GetAttribute("aria-selected"));

        var tabpanel = cut.Find("div[role='tabpanel']");
        Assert.Contains("Content 1", tabpanel.TextContent);
        Assert.DoesNotContain("Content 2", tabpanel.TextContent);
    }

    [Fact]
    public void TwTabContainer_Renders_WithTabColor()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.TabColor, Color.Primary)
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();
            }));

        // Assert
        var button = cut.Find("button[role='tab']");
        Assert.NotNull(button);
        Assert.Contains("text-purple", button.GetAttribute("class"));
    }

    [Fact]
    public void TwTabContainer_Renders_WithDenseMode()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.Dense, true)
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();
            }));

        // Assert
        var button = cut.Find("button[role='tab']");
        Assert.Contains("px-4", button.GetAttribute("class"));
    }

    [Fact]
    public void TwTabContainer_Renders_WithNonDenseMode()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.Dense, false)
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();
            }));

        // Assert
        var button = cut.Find("button[role='tab']");
        Assert.Contains("py-5 px-6", button.GetAttribute("class"));
    }

    [Fact]
    public void TwTabContainer_Renders_WithCustomTabContainerClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.TabContainerClass, "custom-tab-class")
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();
            }));

        // Assert
        var tablist = cut.Find("div[role='tablist']");
        Assert.Contains("custom-tab-class", tablist.GetAttribute("class"));
    }

    [Fact]
    public void TwTabContainer_Renders_WithCustomContainerClass()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ContainerClass, "custom-content-class")
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();
            }));

        // Assert
        var tabpanel = cut.Find("div[role='tabpanel']");
        Assert.Contains("custom-content-class", tabpanel.GetAttribute("class"));
    }

    [Fact]
    public void TwTabContainer_AppliesRoundedTop_ToTabContainer()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();
            }));

        // Assert
        var tablist = cut.Find("div[role='tablist']");
        var classAttribute = tablist.GetAttribute("class");
        Assert.Contains("rounded-t", classAttribute);
    }

    [Fact]
    public void TwTabContainer_AppliesRoundedBottom_ToContentContainer()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();
            }));

        // Assert
        var tabpanel = cut.Find("div[role='tabpanel']");
        var classAttribute = tabpanel.GetAttribute("class");
        Assert.Contains("rounded-b", classAttribute);
    }

    [Fact]
    public void ViewTab_DoesNotActivateTab_WhenCalledDirectly_AndTabIsDisabled()
    {
        // Arrange - via the UI, TwButton's own Disabled guard already prevents a click from
        // ever reaching ViewTab, so the TwTabContainer_ViewTab_DoesNotChangeActiveTab_WhenTabIsDisabled
        // test above never actually exercises ViewTab's own `if (tab.Disabled) return;` guard.
        // ViewTab is public API though, and can be invoked directly (e.g. via a component
        // reference), so that guard needs its own direct coverage.
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();

                builder.OpenComponent<TwTab>(4);
                builder.AddAttribute(5, "Label", "Tab 2");
                builder.AddAttribute(6, "ChildContent", (RenderFragment)(b => b.AddContent(7, "Content 2")));
                builder.AddAttribute(8, "Disabled", true);
                builder.CloseComponent();
            }));

        var disabledTab = cut.FindComponents<TwTab>()[1].Instance;

        // Act
        cut.Instance.ViewTab(disabledTab);

        // Assert
        Assert.NotEqual(disabledTab, cut.Instance.ActiveTab);
    }

    [Fact]
    public void TwTabContainer_DisabledFirstTab_SkipsToFirstEnabledTab_AsInitialActive()
    {
        // Arrange & Act - a disabled first tab must NOT become the initially active tab: the active tab
        // gets tabindex="0" (roving tabindex) while a disabled tab also gets the native `disabled`
        // attribute, and native `disabled` always wins over tabindex - so if it were left active, no tab
        // in the whole list would be reachable via the Tab key.
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.AddAttribute(4, "Disabled", true);
                builder.CloseComponent();

                builder.OpenComponent<TwTab>(5);
                builder.AddAttribute(6, "Label", "Tab 2");
                builder.AddAttribute(7, "ChildContent", (RenderFragment)(b => b.AddContent(8, "Content 2")));
                builder.CloseComponent();
            }));

        // Assert - Tab 2 (the first enabled tab) is active and is the only one with tabindex="0".
        var buttons = cut.FindAll("button[role='tab']");
        Assert.Equal("false", buttons[0].GetAttribute("aria-selected"));
        Assert.Equal("-1", buttons[0].GetAttribute("tabindex"));
        Assert.Equal("true", buttons[1].GetAttribute("aria-selected"));
        Assert.Equal("0", buttons[1].GetAttribute("tabindex"));

        var tabpanel = cut.Find("div[role='tabpanel']");
        Assert.Contains("Content 2", tabpanel.TextContent);
    }

    [Fact]
    public void TwTabContainer_AllTabsDisabled_FallsBackToFirstTabActive_WithoutCrashing()
    {
        // Arrange & Act - edge case: every tab is disabled, so there's no enabled tab to promote. Rather
        // than leaving ActiveTab null (which would render no tabindex="0"/aria-selected="true" anywhere
        // at all), the first registered tab is left active as a fallback.
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.AddAttribute(4, "Disabled", true);
                builder.CloseComponent();

                builder.OpenComponent<TwTab>(5);
                builder.AddAttribute(6, "Label", "Tab 2");
                builder.AddAttribute(7, "ChildContent", (RenderFragment)(b => b.AddContent(8, "Content 2")));
                builder.AddAttribute(9, "Disabled", true);
                builder.CloseComponent();
            }));

        // Assert
        Assert.NotNull(cut.Instance.ActiveTab);
        var buttons = cut.FindAll("button[role='tab']");
        Assert.Equal("true", buttons[0].GetAttribute("aria-selected"));
        Assert.Equal("false", buttons[1].GetAttribute("aria-selected"));
    }

    [Fact]
    public void TwTabContainer_PanelId_IsStable_AndAriaControlsMatchesOnEveryTab_RegardlessOfActiveTab()
    {
        // Arrange
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();

                builder.OpenComponent<TwTab>(4);
                builder.AddAttribute(5, "Label", "Tab 2");
                builder.AddAttribute(6, "ChildContent", (RenderFragment)(b => b.AddContent(7, "Content 2")));
                builder.CloseComponent();
            }));

        var tabpanelBefore = cut.Find("div[role='tabpanel']");
        var panelIdBefore = tabpanelBefore.GetAttribute("id");

        var buttonsBefore = cut.FindAll("button[role='tab']");
        // Assert - every tab's aria-controls references the one real panel, before switching tabs.
        Assert.All(buttonsBefore, b => Assert.Equal(panelIdBefore, b.GetAttribute("aria-controls")));

        // Act - switch the active tab.
        buttonsBefore[1].Click();

        // Assert - the panel's own id (and every tab's aria-controls) is unchanged after switching, so
        // inactive tabs never end up pointing at a different tab's panel id.
        var tabpanelAfter = cut.Find("div[role='tabpanel']");
        Assert.Equal(panelIdBefore, tabpanelAfter.GetAttribute("id"));

        var buttonsAfter = cut.FindAll("button[role='tab']");
        Assert.All(buttonsAfter, b => Assert.Equal(panelIdBefore, b.GetAttribute("aria-controls")));
    }

    [Fact]
    public void TwTabContainer_RendersDarkModeClasses()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();
            }));

        // Assert
        var tablist = cut.Find("div[role='tablist']");
        Assert.Contains("dark:border-[oklch(21.15%_0.012_254.09)]", tablist.GetAttribute("class"));
        Assert.Contains("dark:bg-[oklch(25.33%_0.016_252.42)]", tablist.GetAttribute("class"));

        var tabpanel = cut.Find("div[role='tabpanel']");
        Assert.Contains("dark:bg-[oklch(25.33%_0.016_252.42)]", tabpanel.GetAttribute("class"));
    }

    [Fact]
    public void TwTabContainer_Default_HasFullBorderOutline()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();
            }));

        // Assert - the tab list and panel together form a complete box outline,
        // so the outline is visible even when the container background matches the page background.
        var tablist = cut.Find("div[role='tablist']");
        var tablistClass = tablist.GetAttribute("class") ?? string.Empty;
        Assert.Contains("border-t", tablistClass);
        Assert.Contains("border-l", tablistClass);
        Assert.Contains("border-r", tablistClass);
        Assert.Contains("border-b", tablistClass);

        var tabpanel = cut.Find("div[role='tabpanel']");
        var tabpanelClass = tabpanel.GetAttribute("class") ?? string.Empty;
        Assert.Contains("border-l", tabpanelClass);
        Assert.Contains("border-r", tabpanelClass);
        Assert.Contains("border-b", tabpanelClass);
    }

    [Fact]
    public void TwTabContainer_TransparentContainer_DefaultsToFalse_AndAppliesBackground()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();
            }));

        // Assert
        var tablist = cut.Find("div[role='tablist']");
        Assert.Contains("bg-[oklch(100%_0_0)]", tablist.GetAttribute("class"));

        var tabpanel = cut.Find("div[role='tabpanel']");
        Assert.Contains("bg-[oklch(100%_0_0)]", tabpanel.GetAttribute("class"));
    }

    [Fact]
    public void TwTabContainer_TransparentContainer_RemovesBackgroundClass_ButKeepsBorder()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.TransparentContainer, true)
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();
            }));

        // Assert
        var tablist = cut.Find("div[role='tablist']");
        var tablistClass = tablist.GetAttribute("class") ?? string.Empty;
        Assert.DoesNotContain("bg-white", tablistClass);
        Assert.DoesNotContain("dark:bg-gray-800", tablistClass);
        Assert.Contains("border-[oklch(95%_0_0)]", tablistClass);

        var tabpanel = cut.Find("div[role='tabpanel']");
        var tabpanelClass = tabpanel.GetAttribute("class") ?? string.Empty;
        Assert.DoesNotContain("bg-white", tabpanelClass);
        Assert.DoesNotContain("dark:bg-gray-800", tabpanelClass);
        Assert.Contains("border-[oklch(95%_0_0)]", tabpanelClass);
    }

    private static RenderFragment ThreeTabs(bool disableSecond = false) => builder =>
    {
        builder.OpenComponent<TwTab>(0);
        builder.AddAttribute(1, "Label", "Tab 1");
        builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
        builder.CloseComponent();

        builder.OpenComponent<TwTab>(4);
        builder.AddAttribute(5, "Label", "Tab 2");
        builder.AddAttribute(6, "ChildContent", (RenderFragment)(b => b.AddContent(7, "Content 2")));
        if (disableSecond)
        {
            builder.AddAttribute(8, "Disabled", true);
        }
        builder.CloseComponent();

        builder.OpenComponent<TwTab>(9);
        builder.AddAttribute(10, "Label", "Tab 3");
        builder.AddAttribute(11, "ChildContent", (RenderFragment)(b => b.AddContent(12, "Content 3")));
        builder.CloseComponent();
    };

    [Fact]
    public void HandleTabKeyDown_ArrowRight_DoesNothing_WhenNoTabsRegistered()
    {
        // Arrange - GetAdjacentEnabledTab's very first guard bails out when there's no ActiveTab
        // (which can only ever be true when no TwTab has registered), before it ever indexes into
        // _tabs. The tablist itself still renders even with zero tabs.
        var cut = TestContext.Render<TwTabContainer>();

        // Act & Assert - should not throw despite there being nothing to navigate to.
        var exception = Record.Exception(() =>
            cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" }));
        Assert.Null(exception);
        Assert.Null(cut.Instance.ActiveTab);
    }

    [Fact]
    public void HandleTabKeyDown_ArrowRight_DoesNothing_WhenEveryTabIsDisabled()
    {
        // Arrange - GetAdjacentEnabledTab wraps all the way around looking for an enabled tab; when
        // every tab is disabled it exhausts the loop and returns null, so the keypress must be a no-op
        // rather than throwing or changing ActiveTab away from the fallback-active first tab.
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.AddAttribute(4, "Disabled", true);
                builder.CloseComponent();

                builder.OpenComponent<TwTab>(5);
                builder.AddAttribute(6, "Label", "Tab 2");
                builder.AddAttribute(7, "ChildContent", (RenderFragment)(b => b.AddContent(8, "Content 2")));
                builder.AddAttribute(9, "Disabled", true);
                builder.CloseComponent();
            }));

        // Act
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert - still the first (fallback-active) tab; nothing crashed or changed.
        var buttons = cut.FindAll("button[role='tab']");
        Assert.Equal("true", buttons[0].GetAttribute("aria-selected"));
        Assert.Equal("false", buttons[1].GetAttribute("aria-selected"));
    }

    [Fact]
    public void HandleTabKeyDown_ArrowRight_MovesToNextTab()
    {
        // Arrange
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, ThreeTabs()));

        // Act
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        var buttons = cut.FindAll("button[role='tab']");
        Assert.Equal("true", buttons[1].GetAttribute("aria-selected"));
    }

    [Fact]
    public void HandleTabKeyDown_ArrowDown_MovesToNextTab()
    {
        // Arrange
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, ThreeTabs()));

        // Act
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });

        // Assert
        var buttons = cut.FindAll("button[role='tab']");
        Assert.Equal("true", buttons[1].GetAttribute("aria-selected"));
    }

    [Fact]
    public void HandleTabKeyDown_ArrowRight_WrapsFromLastToFirst()
    {
        // Arrange
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, ThreeTabs()));
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "End" }); // -> Tab 3

        // Act
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert - wraps back around to Tab 1
        var buttons = cut.FindAll("button[role='tab']");
        Assert.Equal("true", buttons[0].GetAttribute("aria-selected"));
    }

    [Fact]
    public void HandleTabKeyDown_ArrowLeft_MovesToPreviousTab()
    {
        // Arrange
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, ThreeTabs()));
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "End" }); // -> Tab 3

        // Act
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        // Assert
        var buttons = cut.FindAll("button[role='tab']");
        Assert.Equal("true", buttons[1].GetAttribute("aria-selected"));
    }

    [Fact]
    public void HandleTabKeyDown_ArrowUp_MovesToPreviousTab()
    {
        // Arrange
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, ThreeTabs()));
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "End" }); // -> Tab 3

        // Act
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });

        // Assert
        var buttons = cut.FindAll("button[role='tab']");
        Assert.Equal("true", buttons[1].GetAttribute("aria-selected"));
    }

    [Fact]
    public void HandleTabKeyDown_ArrowLeft_WrapsFromFirstToLast()
    {
        // Arrange
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, ThreeTabs()));

        // Act - already on Tab 1 (first)
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        // Assert - wraps to Tab 3 (last)
        var buttons = cut.FindAll("button[role='tab']");
        Assert.Equal("true", buttons[2].GetAttribute("aria-selected"));
    }

    [Fact]
    public void HandleTabKeyDown_Home_MovesToFirstEnabledTab()
    {
        // Arrange
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, ThreeTabs()));
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "End" }); // -> Tab 3

        // Act
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "Home" });

        // Assert
        var buttons = cut.FindAll("button[role='tab']");
        Assert.Equal("true", buttons[0].GetAttribute("aria-selected"));
    }

    [Fact]
    public void HandleTabKeyDown_End_MovesToLastEnabledTab()
    {
        // Arrange
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, ThreeTabs()));

        // Act
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "End" });

        // Assert
        var buttons = cut.FindAll("button[role='tab']");
        Assert.Equal("true", buttons[2].GetAttribute("aria-selected"));
    }

    [Fact]
    public void HandleTabKeyDown_SkipsDisabledTab_WhenMovingNext()
    {
        // Arrange - Tab 2 is disabled; ArrowRight from Tab 1 should skip straight to Tab 3.
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, ThreeTabs(disableSecond: true)));

        // Act
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        var buttons = cut.FindAll("button[role='tab']");
        Assert.Equal("false", buttons[0].GetAttribute("aria-selected"));
        Assert.Equal("false", buttons[1].GetAttribute("aria-selected"));
        Assert.Equal("true", buttons[2].GetAttribute("aria-selected"));
    }

    [Fact]
    public void HandleTabKeyDown_UnhandledKey_DoesNotChangeActiveTab()
    {
        // Arrange
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, ThreeTabs()));

        // Act
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "A" });

        // Assert
        var buttons = cut.FindAll("button[role='tab']");
        Assert.Equal("true", buttons[0].GetAttribute("aria-selected"));
    }

    [Fact]
    public void HandleTabKeyDown_DoesNothing_WhenOnlyOneEnabledTab_AndTargetIsCurrentTab()
    {
        // Arrange - only Tab 1 is enabled, so GetAdjacentEnabledTab wraps back to the current tab and
        // the `target == ActiveTab` guard should return early without error.
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, builder =>
            {
                builder.OpenComponent<TwTab>(0);
                builder.AddAttribute(1, "Label", "Tab 1");
                builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(3, "Content 1")));
                builder.CloseComponent();

                builder.OpenComponent<TwTab>(4);
                builder.AddAttribute(5, "Label", "Tab 2");
                builder.AddAttribute(6, "ChildContent", (RenderFragment)(b => b.AddContent(7, "Content 2")));
                builder.AddAttribute(8, "Disabled", true);
                builder.CloseComponent();
            }));

        // Act & Assert - should not throw
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        var buttons = cut.FindAll("button[role='tab']");
        Assert.Equal("true", buttons[0].GetAttribute("aria-selected"));
    }

    [Fact]
    public void OnAfterRender_RegistersKeydownGuard_OnlyOnFirstRender()
    {
        // Arrange & Act
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, ThreeTabs()));

        // Act - trigger a subsequent render
        cut.Find("div[role='tablist']").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert - only registered once, on the first render
        var invocations = TestContext.JSInterop.Invocations
            .Where(i => i.Identifier == "twTabs.registerKeydownGuard")
            .ToList();
        Assert.Single(invocations);
    }

    [Fact]
    public void OnAfterRender_SwallowsJSDisconnectedException_WhenRegisteringGuard()
    {
        // Arrange
        TestContext.JSInterop.SetupVoid("twTabs.registerKeydownGuard", _ => true)
            .SetException(new JSDisconnectedException("Circuit disconnected"));

        // Act & Assert - should not throw/propagate during rendering
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, ThreeTabs()));

        Assert.NotNull(cut.Find("div[role='tablist']"));
    }

    [Fact]
    public async Task DisposeAsync_UnregistersKeydownGuard_WhenRegistered()
    {
        // Arrange
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, ThreeTabs()));

        // Act
        await cut.Instance.DisposeAsync();

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twTabs.unregisterKeydownGuard");
    }

    [Fact]
    public async Task DisposeAsync_DoesNothing_WhenGuardWasNeverRegistered()
    {
        // Arrange - a bare instance constructed outside DI/rendering never runs OnAfterRenderAsync,
        // so keydownGuardRegistered stays false. This intentionally sets the ChildContent parameter
        // outside a component to test the disposal behavior in this edge case scenario.
#pragma warning disable BL0005 // Component parameter should not be set outside of its component
        var container = new TwTabContainer { ChildContent = _ => { } };
#pragma warning restore BL0005

        // Act & Assert - should not throw despite JSRuntime never being injected
        await container.DisposeAsync();

        // Verify disposal completed without exception
        Assert.NotNull(container);
    }

    [Fact]
    public async Task DisposeAsync_SwallowsJSDisconnectedException()
    {
        // Arrange
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, ThreeTabs()));
        TestContext.JSInterop.SetupVoid("twTabs.unregisterKeydownGuard", _ => true)
            .SetException(new JSDisconnectedException("Circuit disconnected"));

        // Act & Assert - should not throw
        await cut.Instance.DisposeAsync();

        // Verify disposal completed despite JSDisconnectedException
        Assert.NotNull(cut.Instance);
    }

    [Fact]
    public async Task DisposeAsync_SwallowsInvalidOperationException()
    {
        // Arrange
        var cut = TestContext.Render<TwTabContainer>(parameters => parameters
            .Add(p => p.ChildContent, ThreeTabs()));
        TestContext.JSInterop.SetupVoid("twTabs.unregisterKeydownGuard", _ => true)
            .SetException(new InvalidOperationException("JS interop unavailable"));

        // Act & Assert - should not throw
        await cut.Instance.DisposeAsync();

        // Verify disposal completed despite InvalidOperationException
        Assert.NotNull(cut.Instance);
    }
}
