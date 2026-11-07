using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Menu;

/// <summary>
/// Tests for the menu items' shortcut hints, destructive and inset looks, radio options and submenus, which
/// <see cref="TwMenu"/> and <see cref="TwContextMenu"/> share.
/// </summary>
public class TwMenuItemFeatureTests : TwBlazorTestBase
{
    private TwMenuTheme menuTheme => Theme.Components.Require<TwMenuTheme>();

    private static readonly string[] _radioOptions = ["Pedro", "Colm", "Ana"];

    private IRenderedComponent<TwMenu> RenderMenu(RenderFragment items) =>
        TestContext.Render<TwMenu>(parameters => parameters
            .Add(p => p.Id, "actions")
            .Add(p => p.Label, "Actions")
            .Add(p => p.ChildContent, items));

    private static RenderFragment RadioGroup(string? value, Action<string?> changed, bool closeOnClick = false, bool disableLast = false) => builder =>
    {
        builder.OpenComponent<TwMenuRadioGroup<string>>(0);
        builder.AddComponentParameter(1, nameof(TwMenuRadioGroup<string>.Id), "people");
        builder.AddComponentParameter(2, nameof(TwMenuRadioGroup<string>.Label), "People");
        builder.AddComponentParameter(3, nameof(TwMenuRadioGroup<string>.Value), value);
        builder.AddComponentParameter(4, nameof(TwMenuRadioGroup<string>.ValueChanged), EventCallback.Factory.Create(new object(), changed));
        builder.AddComponentParameter(5, nameof(TwMenuRadioGroup<string>.ChildContent), (RenderFragment)(items =>
        {
            foreach (var (name, index) in _radioOptions.Select((name, index) => (name, index)))
            {
                items.OpenComponent<TwMenuRadioItem<string>>(10 + index);
                items.AddComponentParameter(20, nameof(TwMenuRadioItem<string>.Value), name);
                items.AddComponentParameter(21, nameof(TwMenuRadioItem<string>.Label), name);
                items.AddComponentParameter(22, nameof(TwMenuRadioItem<string>.CloseOnClick), closeOnClick);
                items.AddComponentParameter(23, nameof(TwMenuRadioItem<string>.Disabled), disableLast && index == 2);
                items.CloseComponent();
            }
        }));
        builder.CloseComponent();
    };

    private static RenderFragment Submenu(Action? onChoose = null, bool disabled = false) => builder =>
    {
        builder.OpenComponent<TwSubmenu>(0);
        builder.AddComponentParameter(1, nameof(TwSubmenu.Id), "more");
        builder.AddComponentParameter(2, nameof(TwSubmenu.Label), "More tools");
        builder.AddComponentParameter(3, nameof(TwSubmenu.Disabled), disabled);
        builder.AddComponentParameter(4, nameof(TwSubmenu.ChildContent), (RenderFragment)(items =>
        {
            items.OpenComponent<TwMenuItem>(0);
            items.AddComponentParameter(1, nameof(TwMenuItem.Label), "Save page");
            if (onChoose is not null)
            {
                items.AddComponentParameter(2, nameof(TwMenuItem.OnClick), EventCallback.Factory.Create(new object(), onChoose));
            }
            items.CloseComponent();
        }));
        builder.CloseComponent();
    };

    [Fact]
    public void Item_ShowsItsShortcutHint_AfterItsText()
    {
        // Act
        var cut = TestContext.Render<TwMenuItem>(p => p
            .Add(x => x.Label, "Copy")
            .Add(x => x.Shortcut, "Ctrl+C"));

        // Assert
        var spans = cut.FindAll("button > span");
        Assert.Equal(2, spans.Count);
        Assert.Equal("Copy", spans[0].TextContent);
        Assert.Equal("Ctrl+C", spans[1].TextContent);
        Assert.Equal(menuTheme.ItemShortcut, spans[1].GetAttribute("class"));
    }

    [Fact]
    public void Item_WithoutAShortcut_HasNoHint()
    {
        // Act
        var cut = TestContext.Render<TwMenuItem>(p => p.Add(x => x.Label, "Copy"));

        // Assert
        Assert.Single(cut.FindAll("button > span"));
    }

    [Fact]
    public void DestructiveItem_GetsTheWarningLook()
    {
        // Act
        var cut = TestContext.Render<TwMenuItem>(p => p
            .Add(x => x.Label, "Delete")
            .Add(x => x.Destructive, true));

        // Assert
        Assert.Contains("text-red-700", cut.Find("button").GetAttribute("class"));
        Assert.DoesNotContain("text-red-700", TestContext.Render<TwMenuItem>(p => p.Add(x => x.Label, "Edit")).Find("button").GetAttribute("class"));
    }

    [Fact]
    public void InsetItem_IsIndented_UnlessItHasAnIcon()
    {
        // Act
        var inset = TestContext.Render<TwMenuItem>(p => p
            .Add(x => x.Label, "Forward")
            .Add(x => x.Inset, true));
        var withIcon = TestContext.Render<TwMenuItem>(p => p
            .Add(x => x.Label, "Back")
            .Add(x => x.Inset, true)
            .Add(x => x.StartIcon, TwBlazor.Enums.Icon.Arrow_Left));

        // Assert
        Assert.Contains(menuTheme.ItemInset, inset.Find("button").GetAttribute("class"));
        Assert.DoesNotContain(menuTheme.ItemInset, withIcon.Find("button").GetAttribute("class"));
    }

    [Fact]
    public void ItemClass_IsAddedLast()
    {
        // Act
        var cut = TestContext.Render<TwMenuItem>(p => p
            .Add(x => x.Label, "Delete")
            .Add(x => x.Destructive, true)
            .Add(x => x.Class, "text-blue-500"));

        // Assert - the consumer's class wins over the destructive colour.
        var classes = cut.Find("button").GetAttribute("class");
        Assert.Contains("text-blue-500", classes);
        Assert.DoesNotContain("text-red-700", classes);
    }

    [Fact]
    public void CheckboxItem_ShowsItsShortcutHint()
    {
        // Act
        var cut = TestContext.Render<TwMenuCheckboxItem>(p => p
            .Add(x => x.Label, "Show bookmarks")
            .Add(x => x.Shortcut, "Ctrl+Shift+B"));

        // Assert
        Assert.Equal("Ctrl+Shift+B", cut.Find($"span.{menuTheme.ItemShortcut.Split(' ')[0]}").TextContent);
    }

    [Fact]
    public void RadioGroup_IsALabelledGroup_OfRadioItems()
    {
        // Act
        var cut = TestContext.Render(RadioGroup("Colm", _ => { }));

        // Assert
        var group = cut.Find("[role='group']");
        Assert.Equal("people-label", group.GetAttribute("aria-labelledby"));
        Assert.Equal("People", cut.Find("#people-label").TextContent);
        var items = cut.FindAll("[role='menuitemradio']");
        Assert.Equal(["false", "true", "false"], items.Select(i => i.GetAttribute("aria-checked")));
        Assert.All(items, item => Assert.Equal("-1", item.GetAttribute("tabindex")));
    }

    [Fact]
    public void RadioItem_ShowsItsDot_OnlyWhenChosen()
    {
        // Act
        var cut = TestContext.Render(RadioGroup("Colm", _ => { }));

        // Assert
        var dots = cut.FindAll("[role='menuitemradio'] i");
        Assert.All(dots, dot => Assert.Contains("bi-dot", dot.GetAttribute("class")));
        Assert.Contains(menuTheme.CheckIconHidden, dots[0].GetAttribute("class"));
        Assert.DoesNotContain(menuTheme.CheckIconHidden, dots[1].GetAttribute("class"));
    }

    [Fact]
    public void ChoosingARadioItem_ChangesTheGroupsValue_AndMovesTheDot()
    {
        // Arrange
        string? chosen = null;
        var cut = TestContext.Render(RadioGroup("Colm", value => chosen = value));

        // Act
        cut.FindAll("[role='menuitemradio']")[0].Click();

        // Assert
        Assert.Equal("Pedro", chosen);
        Assert.Equal(["true", "false", "false"], cut.FindAll("[role='menuitemradio']").Select(i => i.GetAttribute("aria-checked")));
    }

    [Fact]
    public void ChoosingTheChosenRadioItem_DoesNotRaiseAChange()
    {
        // Arrange
        var changes = 0;
        var cut = TestContext.Render(RadioGroup("Colm", _ => changes++));

        // Act
        cut.FindAll("[role='menuitemradio']")[1].Click();

        // Assert
        Assert.Equal(0, changes);
    }

    [Fact]
    public void ADisabledRadioItem_CannotBeChosen()
    {
        // Arrange
        string? chosen = null;
        var cut = TestContext.Render(RadioGroup("Colm", value => chosen = value, disableLast: true));

        // Act
        var ana = cut.FindAll("[role='menuitemradio']")[2];
        ana.Click();

        // Assert
        Assert.Null(chosen);
        Assert.Equal("true", ana.GetAttribute("aria-disabled"));
        Assert.Contains(menuTheme.ItemDisabled, ana.GetAttribute("class"));
    }

    [Fact]
    public void RadioItem_OutsideAGroup_IsNeverChosen()
    {
        // Act
        var cut = TestContext.Render<TwMenuRadioItem<int>>(p => p
            .Add(x => x.Value, 1)
            .Add(x => x.Label, "One")
            .Add(x => x.Shortcut, "1"));
        cut.Find("button").Click();

        // Assert
        Assert.Equal("false", cut.Find("button").GetAttribute("aria-checked"));
        Assert.Contains("1", cut.Find("button").TextContent);
    }

    [Fact]
    public void RadioItem_LeavesTheMenuOpen_UnlessToldToClose()
    {
        // Arrange
        var open = RenderMenu(RadioGroup("Colm", _ => { }));
        open.Find("#actions-trigger").Click();
        var closing = RenderMenu(RadioGroup("Colm", _ => { }, closeOnClick: true));
        closing.Find("#actions-trigger").Click();

        // Act
        open.FindAll("[role='menuitemradio']")[0].Click();
        closing.FindAll("[role='menuitemradio']")[0].Click();

        // Assert
        Assert.True(open.Instance.IsOpen);
        Assert.False(closing.Instance.IsOpen);
    }

    [Fact]
    public void RadioGroup_WithoutALabel_HasNoLabelReference()
    {
        // Act
        var cut = TestContext.Render<TwMenuRadioGroup<int>>(p => p.Add(x => x.AriaLabel, "Zoom"));

        // Assert
        var group = cut.Find("[role='group']");
        Assert.Null(group.GetAttribute("aria-labelledby"));
        Assert.Equal("Zoom", group.GetAttribute("aria-label"));
    }

    [Fact]
    public void Submenu_IsAMenuItem_ThatOpensAHiddenMenu()
    {
        // Act
        var cut = TestContext.Render(Submenu());

        // Assert
        var trigger = cut.Find("#more");
        Assert.Equal("menuitem", trigger.GetAttribute("role"));
        Assert.Equal("menu", trigger.GetAttribute("aria-haspopup"));
        Assert.Equal("false", trigger.GetAttribute("aria-expanded"));
        Assert.Equal("more-menu", trigger.GetAttribute("aria-controls"));
        Assert.Equal("-1", trigger.GetAttribute("tabindex"));
        Assert.Contains(menuTheme.SubmenuTrigger.Split(' ')[0], trigger.GetAttribute("class"));

        var panel = cut.Find("#more-menu");
        Assert.Equal("menu", panel.GetAttribute("role"));
        Assert.Equal("more", panel.GetAttribute("aria-labelledby"));
        Assert.True(panel.HasAttribute("hidden"));
        Assert.Equal("Save page", panel.QuerySelector("[role='menuitem']")!.TextContent.Trim());

        var wrapper = cut.Find("[data-tw-submenu]");
        Assert.Equal("none", wrapper.GetAttribute("role"));
    }

    [Fact]
    public void Submenu_PointsItsArrowTowardsTheEnd()
    {
        // Act
        var ltr = TestContext.Render(Submenu());
        var rtl = TestContext.Render<TwDirectionProvider>(p => p
            .Add(x => x.Direction, TextDirection.RightToLeft)
            .Add(x => x.ChildContent, Submenu()));

        // Assert
        Assert.Contains("bi-chevron-right", ltr.Find("#more i").GetAttribute("class"));
        Assert.Contains("bi-chevron-left", rtl.Find("#more i").GetAttribute("class"));
    }

    [Fact]
    public void ADisabledSubmenu_IsMarked()
    {
        // Act
        var cut = TestContext.Render(Submenu(disabled: true));

        // Assert
        var trigger = cut.Find("#more");
        Assert.Equal("true", trigger.GetAttribute("aria-disabled"));
        Assert.Contains(menuTheme.ItemDisabled, trigger.GetAttribute("class"));
    }

    [Fact]
    public void Submenu_ShowsItsIcon_OrIsInset()
    {
        // Act
        var withIcon = TestContext.Render<TwSubmenu>(p => p
            .Add(x => x.Label, "Share")
            .Add(x => x.StartIcon, TwBlazor.Enums.Icon.Share)
            .Add(x => x.Inset, true));
        var inset = TestContext.Render<TwSubmenu>(p => p
            .Add(x => x.LabelContent, RenderFragmentBuilder("Share to"))
            .Add(x => x.Inset, true));

        // Assert
        Assert.Contains("bi-share", withIcon.Find("button i").GetAttribute("class"));
        Assert.DoesNotContain(menuTheme.ItemInset, withIcon.Find("button").GetAttribute("class"));
        Assert.Contains(menuTheme.ItemInset, inset.Find("button").GetAttribute("class"));
        Assert.Contains("Share to", inset.Find("button").TextContent);
    }

    [Fact]
    public void ChoosingAnItemInASubmenu_ClosesTheWholeMenu()
    {
        // Arrange
        var chosen = false;
        var cut = RenderMenu(Submenu(() => chosen = true));
        cut.Find("#actions-trigger").Click();

        // Act
        cut.Find("#more-menu [role='menuitem']").Click();

        // Assert
        Assert.True(chosen);
        Assert.False(cut.Instance.IsOpen);
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twMenu.focus");
    }
}
