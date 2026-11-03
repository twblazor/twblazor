using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;

namespace TwBlazor.Tests.Components.Menu;

public class TwMenuTests : TwBlazorTestBase
{
    private TwMenuTheme menuTheme => Theme.Components.Require<TwMenuTheme>();

    private IRenderedComponent<TwMenu> Render(RenderFragment? items = null, Action<ComponentParameterCollectionBuilder<TwMenu>>? configure = null, string? label = "Actions") =>
        TestContext.Render<TwMenu>(parameters =>
        {
            parameters
                .Add(p => p.Id, "actions")
                .Add(p => p.Label, label)
                .Add(p => p.ChildContent, items ?? (builder =>
                {
                    builder.OpenComponent<TwMenuItem>(0);
                    builder.AddComponentParameter(1, nameof(TwMenuItem.Label), "Edit");
                    builder.CloseComponent();
                }));
            configure?.Invoke(parameters);
        });

    private static RenderFragment Item(string label, Action? onClick = null, bool disabled = false, bool closeOnClick = true) => builder =>
    {
        builder.OpenComponent<TwMenuItem>(0);
        builder.AddComponentParameter(1, nameof(TwMenuItem.Label), label);
        builder.AddComponentParameter(2, nameof(TwMenuItem.Disabled), disabled);
        builder.AddComponentParameter(3, nameof(TwMenuItem.CloseOnClick), closeOnClick);
        builder.AddComponentParameter(4, nameof(TwMenuItem.StartIcon), TwBlazor.Enums.Icon.Pencil);
        if (onClick is not null)
        {
            builder.AddComponentParameter(5, nameof(TwMenuItem.OnClick), EventCallback.Factory.Create(new object(), onClick));
        }
        builder.CloseComponent();
    };

    [Fact]
    public void Button_IsAMenuButton_ThatStartsClosed()
    {
        // Act
        var cut = Render();

        // Assert
        var trigger = cut.Find("#actions-trigger");
        Assert.Equal("menu", trigger.GetAttribute("aria-haspopup"));
        Assert.Equal("false", trigger.GetAttribute("aria-expanded"));
        Assert.Equal("actions-menu", trigger.GetAttribute("aria-controls"));
        Assert.Contains("Actions", trigger.TextContent);
        Assert.Empty(cut.FindAll("[role='menu']"));
    }

    [Fact]
    public void ClickingTheButton_OpensTheMenu_OnItsFirstItem()
    {
        // Arrange
        bool? opened = null;
        var cut = Render(configure: p => p.Add(x => x.OpenChanged, (bool open) => opened = open));

        // Act
        cut.Find("#actions-trigger").Click();

        // Assert
        Assert.Equal("true", cut.Find("#actions-trigger").GetAttribute("aria-expanded"));
        var menu = cut.Find("#actions-menu");
        Assert.Equal("menu", menu.GetAttribute("role"));
        Assert.Equal("actions-trigger", menu.GetAttribute("aria-labelledby"));
        Assert.Equal("vertical", menu.GetAttribute("aria-orientation"));
        Assert.True(cut.Instance.IsOpen);
        Assert.True(opened);
        var open = Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twMenu.open");
        Assert.Equal("first", open.Arguments[3]);
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.registerOutsideClick");
    }

    [Fact]
    public void ArrowUp_OpensTheMenu_OnItsLastItem()
    {
        // Arrange
        var cut = Render();

        // Act
        cut.Find("#actions-trigger").KeyDown("ArrowUp");

        // Assert
        Assert.Equal("last", TestContext.JSInterop.Invocations.Single(i => i.Identifier == "twMenu.open").Arguments[3]);
    }

    [Fact]
    public void ArrowDown_OpensTheMenu_OnItsFirstItem()
    {
        // Arrange
        var cut = Render();

        // Act
        cut.Find("#actions-trigger").KeyDown("ArrowDown");

        // Assert
        Assert.Equal("first", TestContext.JSInterop.Invocations.Single(i => i.Identifier == "twMenu.open").Arguments[3]);
    }

    [Fact]
    public void ClickingTheButtonAgain_ClosesTheMenu_AndReturnsFocus()
    {
        // Arrange
        bool? opened = null;
        var cut = Render(configure: p => p.Add(x => x.OpenChanged, (bool open) => opened = open));
        cut.Find("#actions-trigger").Click();

        // Act
        cut.Find("#actions-trigger").Click();

        // Assert
        Assert.Empty(cut.FindAll("[role='menu']"));
        Assert.False(opened);
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twMenu.focus");
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.unregisterOutsideClick");
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twMenu.close");
    }

    [Fact]
    public void ChoosingAnItem_RunsIt_ClosesTheMenu_AndReturnsFocus()
    {
        // Arrange
        var chosen = false;
        var cut = Render(Item("Edit", () => chosen = true));
        cut.Find("#actions-trigger").Click();

        // Act
        cut.Find("[role='menuitem']").Click();

        // Assert
        Assert.True(chosen);
        Assert.False(cut.Instance.IsOpen);
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twMenu.focus");
    }

    [Fact]
    public void AnItemThatDoesNotClose_LeavesTheMenuOpen()
    {
        // Arrange
        var cut = Render(Item("Edit", closeOnClick: false));
        cut.Find("#actions-trigger").Click();

        // Act
        cut.Find("[role='menuitem']").Click();

        // Assert
        Assert.True(cut.Instance.IsOpen);
    }

    [Fact]
    public void ADisabledItem_IsMarked_AndDoesNothing()
    {
        // Arrange
        var chosen = false;
        var cut = Render(Item("Edit", () => chosen = true, disabled: true));
        cut.Find("#actions-trigger").Click();

        // Act
        var item = cut.Find("[role='menuitem']");
        item.Click();

        // Assert
        Assert.Equal("true", item.GetAttribute("aria-disabled"));
        Assert.Contains(menuTheme.ItemDisabled, item.GetAttribute("class"));
        Assert.False(chosen);
        Assert.True(cut.Instance.IsOpen);
    }

    [Fact]
    public void Items_AreOutOfTheTabOrder_AndShowTheirIcon()
    {
        // Arrange
        var cut = Render(Item("Edit"));

        // Act
        cut.Find("#actions-trigger").Click();

        // Assert
        var item = cut.Find("[role='menuitem']");
        Assert.Equal("-1", item.GetAttribute("tabindex"));
        Assert.Equal("button", item.GetAttribute("type"));
        Assert.NotNull(item.QuerySelector("i[aria-hidden='true']"));
        Assert.Contains("Edit", item.TextContent);
    }

    [Fact]
    public async Task Escape_OrAClickOutside_ClosesTheMenu()
    {
        // Arrange
        var cut = Render();
        cut.Find("#actions-trigger").Click();

        // Act - the Escape stack and the outside click handler both call Close.
        await cut.InvokeAsync(() => cut.Instance.CloseAsync());

        // Assert
        Assert.Empty(cut.FindAll("[role='menu']"));
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twMenu.focus");
    }

    [Fact]
    public async Task Tab_ClosesTheMenu_WithoutMovingFocusBack()
    {
        // Arrange
        var cut = Render();
        cut.Find("#actions-trigger").Click();

        // Act
        await cut.InvokeAsync(() => cut.Instance.CloseFromTab());

        // Assert
        Assert.False(cut.Instance.IsOpen);
        Assert.DoesNotContain(TestContext.JSInterop.Invocations, i => i.Identifier == "twMenu.focus");
    }

    [Fact]
    public async Task OpenAsync_OpensTheMenu_AndClosingTwiceIsHarmless()
    {
        // Arrange
        var cut = Render();

        // Act
        await cut.InvokeAsync(() => cut.Instance.OpenAsync());
        await cut.InvokeAsync(() => cut.Instance.OpenAsync());
        await cut.InvokeAsync(() => cut.Instance.CloseAsync());
        await cut.InvokeAsync(() => cut.Instance.CloseAsync());

        // Assert
        Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twMenu.open");
        Assert.Single(TestContext.JSInterop.Invocations, i => i.Identifier == "twMenu.focus");
    }

    [Fact]
    public void ADisabledMenu_DoesNotOpen()
    {
        // Arrange
        var cut = Render(configure: p => p.Add(x => x.Disabled, true));

        // Act
        cut.Find("#actions-trigger").KeyDown("ArrowDown");

        // Assert
        Assert.False(cut.Instance.IsOpen);
        Assert.True(cut.Find("#actions-trigger").HasAttribute("disabled"));
    }

    [Fact]
    public void IconOnly_NamesTheButton_WithItsAriaLabel()
    {
        // Act
        var cut = Render(label: null, configure: p => p
            .Add(x => x.Icon, TwBlazor.Enums.Icon.Three_Dots_Vertical)
            .Add(x => x.IconOnly, true)
            .Add(x => x.AriaLabel, "Row options"));

        // Assert
        var trigger = cut.Find("#actions-trigger");
        Assert.Equal("Row options", trigger.GetAttribute("aria-label"));
        Assert.Equal("menu", trigger.GetAttribute("aria-haspopup"));
    }

    [Fact]
    public void IconOnly_WithoutALabel_IsCalledOptions()
    {
        // Act
        var cut = Render(label: null, configure: p => p
            .Add(x => x.Icon, TwBlazor.Enums.Icon.Three_Dots_Vertical)
            .Add(x => x.IconOnly, true));

        // Assert
        Assert.Equal("Options", cut.Find("#actions-trigger").GetAttribute("aria-label"));
    }

    [Fact]
    public async Task DisposeAsync_RemovesTheListenersOfAnOpenMenu()
    {
        // Arrange
        var cut = Render();
        cut.Find("#actions-trigger").Click();

        // Act
        await cut.Instance.DisposeAsync();

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twPicker.unregisterOutsideClick");
    }

    [Fact]
    public void CheckboxItem_ReportsWhetherItIsTicked_AndFlipsWhenChosen()
    {
        // Arrange
        bool? changed = null;
        var cut = TestContext.Render<TwMenuCheckboxItem>(parameters => parameters
            .Add(p => p.Label, "Show grid")
            .Add(p => p.Checked, false)
            .Add(p => p.CheckedChanged, (bool value) => changed = value));
        var item = cut.Find("[role='menuitemcheckbox']");
        Assert.Equal("false", item.GetAttribute("aria-checked"));
        Assert.Contains(menuTheme.CheckIconHidden, item.QuerySelector("i")!.GetAttribute("class"));

        // Act
        item.Click();

        // Assert
        Assert.True(changed);
        Assert.Equal("true", cut.Find("[role='menuitemcheckbox']").GetAttribute("aria-checked"));
        Assert.DoesNotContain(menuTheme.CheckIconHidden, cut.Find("i").GetAttribute("class"));
    }

    [Fact]
    public void CheckboxItem_LeavesTheMenuOpen_UnlessToldToClose()
    {
        // Arrange
        RenderFragment items = builder =>
        {
            builder.OpenComponent<TwMenuCheckboxItem>(0);
            builder.AddComponentParameter(1, nameof(TwMenuCheckboxItem.Label), "Grid");
            builder.CloseComponent();
            builder.OpenComponent<TwMenuCheckboxItem>(2);
            builder.AddComponentParameter(3, nameof(TwMenuCheckboxItem.Label), "Rulers");
            builder.AddComponentParameter(4, nameof(TwMenuCheckboxItem.CloseOnClick), true);
            builder.CloseComponent();
        };
        var cut = Render(items);
        cut.Find("#actions-trigger").Click();

        // Act & Assert
        cut.FindAll("[role='menuitemcheckbox']")[0].Click();
        Assert.True(cut.Instance.IsOpen);

        cut.FindAll("[role='menuitemcheckbox']")[1].Click();
        Assert.False(cut.Instance.IsOpen);
    }

    [Fact]
    public void ADisabledCheckboxItem_DoesNotFlip()
    {
        // Arrange
        var cut = TestContext.Render<TwMenuCheckboxItem>(parameters => parameters
            .Add(p => p.Label, "Snap")
            .Add(p => p.Disabled, true));

        // Act
        cut.Find("button").Click();

        // Assert
        Assert.Equal("false", cut.Find("button").GetAttribute("aria-checked"));
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-disabled"));
    }

    [Fact]
    public void Group_IsNamedByItsHeading()
    {
        // Act
        var cut = TestContext.Render<TwMenuGroup>(parameters => parameters
            .Add(p => p.Id, "sort")
            .Add(p => p.Label, "Sort")
            .Add(p => p.ChildContent, RenderFragmentBuilder("items")));

        // Assert
        var group = cut.Find("[role='group']");
        Assert.Equal("sort-label", group.GetAttribute("aria-labelledby"));
        Assert.Equal("Sort", cut.Find("#sort-label").TextContent);
        Assert.Equal("presentation", cut.Find("#sort-label").GetAttribute("role"));
    }

    [Fact]
    public void Group_WithoutAHeading_HasNoLabelReference()
    {
        // Act
        var cut = TestContext.Render<TwMenuGroup>();

        // Assert
        Assert.Null(cut.Find("[role='group']").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void Divider_IsASeparator()
    {
        // Act
        var cut = TestContext.Render<TwMenuDivider>();

        // Assert
        var divider = cut.Find("[role='separator']");
        Assert.Contains(menuTheme.Divider, divider.GetAttribute("class"));
    }

    [Fact]
    public void Item_WithChildContent_ShowsItInPlaceOfTheLabel()
    {
        // Act
        var cut = TestContext.Render<TwMenuItem>(parameters => parameters
            .Add(p => p.Label, "Ignored")
            .Add(p => p.ChildContent, RenderFragmentBuilder("Custom")));

        // Assert
        Assert.Contains("Custom", cut.Markup);
        Assert.DoesNotContain("Ignored", cut.Markup);
    }
}
