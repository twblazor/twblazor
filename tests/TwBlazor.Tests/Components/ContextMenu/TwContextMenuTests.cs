using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;

namespace TwBlazor.Tests.Components.ContextMenu;

public class TwContextMenuTests : TwBlazorTestBase
{
    private TwMenuTheme menuTheme => Theme.Components.Require<TwMenuTheme>();

    private IRenderedComponent<TwContextMenu> Render(RenderFragment? items = null, Action<ComponentParameterCollectionBuilder<TwContextMenu>>? configure = null) =>
        TestContext.Render<TwContextMenu>(parameters =>
        {
            parameters
                .Add(p => p.Id, "files")
                .Add(p => p.ChildContent, RenderFragmentBuilder("Right-click here"))
                .Add(p => p.MenuContent, items ?? Item("Copy"));
            configure?.Invoke(parameters);
        });

    private static RenderFragment Item(string label, Action? onClick = null, bool closeOnClick = true) => builder =>
    {
        builder.OpenComponent<TwMenuItem>(0);
        builder.AddComponentParameter(1, nameof(TwMenuItem.Label), label);
        builder.AddComponentParameter(2, nameof(TwMenuItem.CloseOnClick), closeOnClick);
        if (onClick is not null)
        {
            builder.AddComponentParameter(3, nameof(TwMenuItem.OnClick), EventCallback.Factory.Create(new object(), onClick));
        }
        builder.CloseComponent();
    };

    private BunitJSInterop js => TestContext.JSInterop;

    [Fact]
    public void Area_ShowsItsContent_AndTheMenuStartsClosed()
    {
        // Act
        var cut = Render(configure: p => p.Add(x => x.Class, "h-40"));

        // Assert
        var area = cut.Find("#files");
        Assert.Equal("Right-click here", area.TextContent.Trim());
        Assert.Equal("enabled", area.GetAttribute("data-tw-context-menu"));
        Assert.Contains("h-40", area.GetAttribute("class"));
        Assert.Contains(menuTheme.ContextArea.Split(' ')[0], area.GetAttribute("class"));
        Assert.Null(area.GetAttribute("tabindex"));
        Assert.Null(area.GetAttribute("role"));
        Assert.Empty(cut.FindAll("[role='menu']"));
        Assert.False(cut.Instance.IsOpen);
    }

    [Fact]
    public void Area_IsAttachedToTheScript_OnFirstRender()
    {
        // Act
        var cut = Render();
        cut.Render();

        // Assert
        Assert.Single(js.Invocations, i => i.Identifier == "twContextMenu.attach");
    }

    [Fact]
    public async Task OpeningWithAPointer_ShowsTheMenuAtThePoint_WithFocusOnTheMenu()
    {
        // Arrange
        bool? opened = null;
        var cut = Render(configure: p => p.Add(x => x.OpenChanged, (bool open) => opened = open));

        // Act
        await cut.InvokeAsync(() => cut.Instance.OpenAtAsync(120, 340));

        // Assert
        var menu = cut.Find("#files-menu");
        Assert.Equal("menu", menu.GetAttribute("role"));
        Assert.Equal("vertical", menu.GetAttribute("aria-orientation"));
        Assert.Equal("Context menu", menu.GetAttribute("aria-label"));
        Assert.Equal("-1", menu.GetAttribute("tabindex"));
        Assert.Contains("position:fixed", menu.GetAttribute("style"));
        Assert.Equal(menuTheme.Panel, menu.GetAttribute("class"));
        Assert.True(cut.Instance.IsOpen);
        Assert.True(opened);

        var open = Assert.Single(js.Invocations, i => i.Identifier == "twContextMenu.open");
        Assert.Equal(120d, open.Arguments[3]);
        Assert.Equal(340d, open.Arguments[4]);
        Assert.Equal("menu", open.Arguments[5]);
    }

    [Fact]
    public async Task OpeningFromTheKeyboard_FocusesTheFirstItem()
    {
        // Arrange
        var cut = Render();

        // Act
        await cut.InvokeAsync(() => cut.Instance.OpenAtAsync(10, 20, fromKeyboard: true));

        // Assert
        Assert.Equal("first", js.Invocations.Single(i => i.Identifier == "twContextMenu.open").Arguments[5]);
    }

    [Fact]
    public async Task OpeningAgainWhileOpen_MovesTheMenu_WithoutRaisingOpenChangedAgain()
    {
        // Arrange
        var changes = 0;
        var cut = Render(configure: p => p.Add(x => x.OpenChanged, (bool _) => changes++));
        await cut.InvokeAsync(() => cut.Instance.OpenAtAsync(10, 20));

        // Act
        await cut.InvokeAsync(() => cut.Instance.OpenAtAsync(300, 400));

        // Assert
        Assert.Equal(1, changes);
        var opens = js.Invocations.Where(i => i.Identifier == "twContextMenu.open").ToList();
        Assert.Equal(2, opens.Count);
        Assert.Equal(300d, opens[1].Arguments[3]);
        Assert.Single(cut.FindAll("[role='menu']"));
    }

    [Fact]
    public async Task ADisabledArea_NeverOpens_AndLeavesTheBrowserMenu()
    {
        // Arrange
        var cut = Render(configure: p => p.Add(x => x.Disabled, true));

        // Act
        await cut.InvokeAsync(() => cut.Instance.OpenAtAsync(10, 20));

        // Assert
        Assert.False(cut.Instance.IsOpen);
        Assert.Equal("disabled", cut.Find("#files").GetAttribute("data-tw-context-menu"));
        Assert.DoesNotContain(js.Invocations, i => i.Identifier == "twContextMenu.open");
    }

    [Fact]
    public async Task ChoosingAnItem_RunsIt_ClosesTheMenu_AndReturnsFocus()
    {
        // Arrange
        var chosen = false;
        bool? opened = null;
        var cut = Render(Item("Copy", () => chosen = true), p => p.Add(x => x.OpenChanged, (bool open) => opened = open));
        await cut.InvokeAsync(() => cut.Instance.OpenAtAsync(10, 20));

        // Act
        cut.Find("[role='menuitem']").Click();

        // Assert
        Assert.True(chosen);
        Assert.False(cut.Instance.IsOpen);
        Assert.False(opened);
        Assert.Empty(cut.FindAll("[role='menu']"));
        var close = Assert.Single(js.Invocations, i => i.Identifier == "twContextMenu.close");
        Assert.Equal(true, close.Arguments[2]);
    }

    [Fact]
    public async Task AnItemThatDoesNotClose_LeavesTheMenuOpen()
    {
        // Arrange
        var cut = Render(Item("Copy", closeOnClick: false));
        await cut.InvokeAsync(() => cut.Instance.OpenAtAsync(10, 20));

        // Act
        cut.Find("[role='menuitem']").Click();

        // Assert
        Assert.True(cut.Instance.IsOpen);
    }

    [Fact]
    public async Task CheckboxItems_LeaveTheMenuOpen()
    {
        // Arrange
        var isChecked = false;
        RenderFragment items = builder =>
        {
            builder.OpenComponent<TwMenuCheckboxItem>(0);
            builder.AddComponentParameter(1, nameof(TwMenuCheckboxItem.Label), "Show bookmarks");
            builder.AddComponentParameter(2, nameof(TwMenuCheckboxItem.CheckedChanged), EventCallback.Factory.Create<bool>(new object(), value => isChecked = value));
            builder.CloseComponent();
        };
        var cut = Render(items);
        await cut.InvokeAsync(() => cut.Instance.OpenAtAsync(10, 20));

        // Act
        cut.Find("[role='menuitemcheckbox']").Click();

        // Assert
        Assert.True(isChecked);
        Assert.True(cut.Instance.IsOpen);
    }

    [Fact]
    public async Task Close_ReturnsFocus_AndDismiss_DoesNot()
    {
        // Arrange
        var cut = Render();
        await cut.InvokeAsync(() => cut.Instance.OpenAtAsync(10, 20));
        await cut.InvokeAsync(() => cut.Instance.CloseAsync());
        await cut.InvokeAsync(() => cut.Instance.OpenAtAsync(10, 20));

        // Act
        await cut.InvokeAsync(() => cut.Instance.Dismiss());

        // Assert
        var closes = js.Invocations.Where(i => i.Identifier == "twContextMenu.close").ToList();
        Assert.Equal([true, false], closes.Select(c => (bool)c.Arguments[2]!));
        Assert.False(cut.Instance.IsOpen);
    }

    [Fact]
    public async Task ClosingAClosedMenu_IsHarmless()
    {
        // Arrange
        var changes = 0;
        var cut = Render(configure: p => p.Add(x => x.OpenChanged, (bool _) => changes++));

        // Act
        await cut.InvokeAsync(() => cut.Instance.CloseAsync());
        await cut.InvokeAsync(() => cut.Instance.Dismiss());

        // Assert
        Assert.Equal(0, changes);
        Assert.DoesNotContain(js.Invocations, i => i.Identifier == "twContextMenu.close");
    }

    [Fact]
    public async Task AriaLabel_NamesTheMenu_AndMenuClass_IsAddedToIt()
    {
        // Arrange
        var cut = Render(configure: p => p
            .Add(x => x.AriaLabel, "File actions")
            .Add(x => x.MenuClass, "min-w-64"));

        // Act
        await cut.InvokeAsync(() => cut.Instance.OpenAtAsync(10, 20));

        // Assert
        var menu = cut.Find("[role='menu']");
        Assert.Equal("File actions", menu.GetAttribute("aria-label"));
        Assert.Contains("min-w-64", menu.GetAttribute("class"));
        Assert.DoesNotContain("min-w-48", menu.GetAttribute("class"));
    }

    [Fact]
    public void AFocusableArea_IsATabStop_ThatSaysHowToOpenTheMenu()
    {
        // Act
        var cut = Render(configure: p => p
            .Add(x => x.Focusable, true)
            .AddUnmatched("aria-label", "Canvas"));

        // Assert
        var area = cut.Find("#files");
        Assert.Equal("0", area.GetAttribute("tabindex"));
        Assert.Equal("group", area.GetAttribute("role"));
        Assert.Equal("Canvas", area.GetAttribute("aria-label"));
        Assert.Equal("files-hint", area.GetAttribute("aria-describedby"));
        var hint = cut.Find("#files-hint");
        Assert.Equal("Press Shift+F10 or the Menu key for more options.", hint.TextContent);
        Assert.Equal("sr-only", hint.GetAttribute("class"));
    }

    [Fact]
    public async Task ItemsInASubmenu_CloseTheWholeContextMenu()
    {
        // Arrange
        RenderFragment items = builder =>
        {
            builder.OpenComponent<TwSubmenu>(0);
            builder.AddComponentParameter(1, nameof(TwSubmenu.Label), "Share");
            builder.AddComponentParameter(2, nameof(TwSubmenu.ChildContent), Item("Email"));
            builder.CloseComponent();
        };
        var cut = Render(items);
        await cut.InvokeAsync(() => cut.Instance.OpenAtAsync(10, 20));

        // Act
        cut.Find("[role='menu'] [role='menu'] [role='menuitem']").Click();

        // Assert
        Assert.False(cut.Instance.IsOpen);
    }

    [Fact]
    public async Task DisposeAsync_DetachesTheArea()
    {
        // Arrange
        var cut = Render();

        // Act
        await cut.Instance.DisposeAsync();
        await cut.Instance.DisposeAsync();

        // Assert
        Assert.Single(js.Invocations, i => i.Identifier == "twContextMenu.detach");
    }

    [Fact]
    public async Task ARightToLeftLayout_KeepsTheSameMarkup()
    {
        // Arrange
        var provider = TestContext.Render<TwDirectionProvider>(p => p
            .Add(x => x.Direction, TwBlazor.Enums.TextDirection.RightToLeft)
            .AddChildContent<TwContextMenu>(menu => menu
                .Add(x => x.Id, "rtl")
                .Add(x => x.MenuContent, Item("Copy"))));
        var cut = provider.FindComponent<TwContextMenu>();

        // Act
        await cut.InvokeAsync(() => cut.Instance.OpenAtAsync(10, 20));

        // Assert - the script reads the direction from the page, so it places the menu on the left of the point.
        Assert.Equal("rtl", provider.Find("[dir]").GetAttribute("dir"));
        Assert.NotNull(cut.Find("#rtl-menu"));
    }
}
