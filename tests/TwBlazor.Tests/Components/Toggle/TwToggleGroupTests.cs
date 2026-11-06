using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Toggle;

public class TwToggleGroupTests : TwBlazorTestBase
{
    private TwToggleTheme theme => Theme.Components.Require<TwToggleTheme>();

    private static string[] ClassesOf(AngleSharp.Dom.IElement element) =>
        (element.GetAttribute("class") ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);

    private static void AssertHasAll(string expected, string[] actual)
    {
        foreach (var token in expected.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            Assert.Contains(token, actual);
        }
    }

    private sealed record Item(string Value, bool Disabled = false, Color? Color = null, Rounded? Rounded = null, string? Class = null);

    private static RenderFragment Items(params Item[] items) => builder =>
    {
        var seq = 0;
        foreach (var item in items)
        {
            builder.OpenComponent<TwToggleGroupItem<string>>(seq++);
            builder.AddAttribute(seq++, "Value", item.Value);
            builder.AddAttribute(seq++, "Label", item.Value.ToUpperInvariant());
            builder.AddAttribute(seq++, "Id", $"item-{item.Value}");
            builder.AddAttribute(seq++, "Disabled", item.Disabled);
            if (item.Color.HasValue)
            {
                builder.AddAttribute(seq++, "Color", item.Color);
            }
            if (item.Rounded.HasValue)
            {
                builder.AddAttribute(seq++, "Rounded", item.Rounded);
            }
            if (item.Class is not null)
            {
                builder.AddAttribute(seq++, "Class", item.Class);
            }
            builder.CloseComponent();
        }
    };

    private static RenderFragment abc => Items(new("a"), new("b"), new("c"));

    private IRenderedComponent<TwToggleGroup<string>> RenderSingle(string? value, RenderFragment? items = null, Action<ComponentParameterCollectionBuilder<TwToggleGroup<string>>>? configure = null) =>
        TestContext.Render<TwToggleGroup<string>>(p =>
        {
            p.Add(x => x.Value, value).Add(x => x.ChildContent, items ?? abc);
            configure?.Invoke(p);
        });

    private IRenderedComponent<TwToggleGroup<string>> RenderMultiple(IEnumerable<string> values, RenderFragment? items = null, Action<ComponentParameterCollectionBuilder<TwToggleGroup<string>>>? configure = null) =>
        TestContext.Render<TwToggleGroup<string>>(p =>
        {
            p.Add(x => x.Multiple, true).Add(x => x.Values, values).Add(x => x.ChildContent, items ?? abc);
            configure?.Invoke(p);
        });

    private static AngleSharp.Dom.IElement Button(IRenderedComponent<TwToggleGroup<string>> cut, string value) => cut.Find($"#item-{value}");

    #region Rendering and ARIA

    [Fact]
    public void Single_RendersARadioGroup_OfRadios()
    {
        // Act
        var cut = RenderSingle("b");

        // Assert
        var group = cut.Find("div");
        Assert.Equal("radiogroup", group.GetAttribute("role"));
        Assert.Equal("horizontal", group.GetAttribute("aria-orientation"));

        var buttons = cut.FindAll("button");
        Assert.Equal(3, buttons.Count);
        Assert.All(buttons, b => Assert.Equal("radio", b.GetAttribute("role")));
        Assert.All(buttons, b => Assert.Null(b.GetAttribute("aria-pressed")));
        Assert.Equal("false", Button(cut, "a").GetAttribute("aria-checked"));
        Assert.Equal("true", Button(cut, "b").GetAttribute("aria-checked"));
    }

    [Fact]
    public void Multiple_RendersAGroup_OfToggleButtons()
    {
        // Act
        var cut = RenderMultiple(["a", "c"]);

        // Assert
        var group = cut.Find("div");
        Assert.Equal("group", group.GetAttribute("role"));
        Assert.Null(group.GetAttribute("aria-orientation"));
        Assert.Equal("true", Button(cut, "a").GetAttribute("aria-pressed"));
        Assert.Equal("false", Button(cut, "b").GetAttribute("aria-pressed"));
        Assert.Equal("true", Button(cut, "c").GetAttribute("aria-pressed"));
        Assert.All(cut.FindAll("button"), b => Assert.Null(b.GetAttribute("role")));
    }

    [Fact]
    public void Vertical_StacksTheItems_AndSetsAriaOrientation()
    {
        // Act
        var cut = RenderSingle("a", configure: p => p.Add(x => x.Vertical, true));

        // Assert
        var group = cut.Find("div");
        Assert.Equal("vertical", group.GetAttribute("aria-orientation"));
        AssertHasAll(theme.GroupVertical, ClassesOf(group));
    }

    [Fact]
    public void Horizontal_IsTheDefaultLayout()
    {
        // Act
        var group = RenderSingle("a").Find("div");

        // Assert
        AssertHasAll(theme.GroupHorizontal, ClassesOf(group));
    }

    [Fact]
    public void AriaLabelAndLabelledBy_AreRenderedOnTheGroup()
    {
        // Act
        var cut = RenderSingle("a", configure: p => p.Add(x => x.AriaLabel, "Alignment").Add(x => x.AriaLabelledBy, "heading"));

        // Assert
        var group = cut.Find("div");
        Assert.Equal("Alignment", group.GetAttribute("aria-label"));
        Assert.Equal("heading", group.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void IdStyleAndAttributes_AreRenderedOnTheGroup()
    {
        // Act
        var cut = RenderSingle("a", configure: p => p.Add(x => x.Id, "grp").Add(x => x.Style, "color: red").AddUnmatched("data-test", "x"));

        // Assert
        var group = cut.Find("div");
        Assert.Equal("grp", group.Id);
        Assert.Equal("color: red", group.GetAttribute("style"));
        Assert.Equal("x", group.GetAttribute("data-test"));
    }

    [Fact]
    public void Items_RenderTwToggle_WithTheirLabels()
    {
        // Act
        var cut = RenderSingle("a");

        // Assert
        Assert.Equal(3, cut.FindComponents<TwToggle>().Count);
        Assert.Contains("A", Button(cut, "a").TextContent);
    }

    [Fact]
    public void Item_OutsideAGroup_Throws()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => TestContext.Render<TwToggleGroupItem<string>>(p => p.Add(x => x.Value, "a")));
    }

    #endregion

    #region Styling

    [Fact]
    public void TextVariant_SetsTheItemsOnAFilledBar_WithRaisedPressedItem()
    {
        // Act
        var cut = RenderSingle("a");

        // Assert
        AssertHasAll(theme.GroupText, ClassesOf(cut.Find("div")));
        AssertHasAll(theme.GroupPressed, ClassesOf(Button(cut, "a")));
    }

    [Fact]
    public void OutlinedVariant_BordersTheGroup_NotEachItem()
    {
        // Act
        var cut = RenderSingle("a", configure: p => p.Add(x => x.Variant, ToggleVariant.Outlined));

        // Assert
        AssertHasAll(theme.GroupOutlined, ClassesOf(cut.Find("div")));
        Assert.DoesNotContain("border", ClassesOf(Button(cut, "b")));
        AssertHasAll(theme.Pressed, ClassesOf(Button(cut, "a")));
    }

    [Fact]
    public void Size_IsAppliedToEveryItem()
    {
        // Act
        var cut = RenderSingle("a", configure: p => p.Add(x => x.Size, Size.Lg));

        // Assert
        Assert.All(cut.FindAll("button"), b => AssertHasAll(theme.Lg, ClassesOf(b)));
    }

    [Fact]
    public void GroupColor_TintsThePressedItem()
    {
        // Act
        var cut = RenderSingle("a", configure: p => p.Add(x => x.Color, Color.Success));

        // Assert
        AssertHasAll(theme.Colors.Success, ClassesOf(Button(cut, "a")));
    }

    [Fact]
    public void ItemColor_WinsOverTheGroupColor()
    {
        // Act
        var cut = RenderMultiple(["a", "b"], Items(new("a"), new("b", Color: Color.Danger)), p => p.Add(x => x.Color, Color.Success));

        // Assert
        AssertHasAll(theme.Colors.Success, ClassesOf(Button(cut, "a")));
        AssertHasAll(theme.Colors.Danger, ClassesOf(Button(cut, "b")));
    }

    [Fact]
    public void ItemClass_IsMergedOntoItsButton()
    {
        // Act
        var cut = RenderSingle("a", Items(new Item("a", Class: "item-custom")));

        // Assert
        Assert.Contains("item-custom", ClassesOf(Button(cut, "a")));
    }

    [Fact]
    public void GroupClass_IsMergedLast()
    {
        // Act
        var group = RenderSingle("a", configure: p => p.Add(x => x.Class, "p-3 group-custom")).Find("div");

        // Assert
        var classes = ClassesOf(group);
        Assert.Contains("group-custom", classes);
        Assert.Contains("p-3", classes);
        Assert.DoesNotContain("p-1", classes);
    }

    [Theory]
    [InlineData(Rounded.Lg, Rounded.Md)]
    [InlineData(Rounded.Md, Rounded.Sm)]
    [InlineData(Rounded.Sm, Rounded.Sm)]
    [InlineData(Rounded.Full, Rounded.Full)]
    [InlineData(Rounded.None, Rounded.None)]
    public void Rounded_TheItemsUseInsetRounding_OfTheGroup(Rounded group, Rounded item)
    {
        // Act
        var cut = RenderSingle("a", configure: p => p.Add(x => x.Rounded, group));

        // Assert
        Assert.Contains(RoundedBuilder.GetRounded(group), ClassesOf(cut.Find("div")));
        Assert.All(cut.FindAll("button"), b => Assert.Contains(RoundedBuilder.GetRounded(item), ClassesOf(b)));
    }

    [Fact]
    public void Rounded_DefaultsToTheThemeDefault_WithInsetItems()
    {
        // Act
        var cut = RenderSingle("a");

        // Assert
        Assert.Contains(RoundedBuilder.GetRounded(Theme.Rounded.DefaultRounded), ClassesOf(cut.Find("div")));
        Assert.Contains(RoundedBuilder.GetRounded(TwBlazor.Builders.RoundedBuilder.Inset(Theme.Rounded.DefaultRounded)), ClassesOf(Button(cut, "a")));
    }

    [Fact]
    public void Rounded_ThemeToggleRounded_FlowsThroughToTheItems()
    {
        // Arrange
        theme.ToggleRounded = Rounded.Md;

        // Act
        var cut = RenderSingle("a");

        // Assert
        Assert.Contains(Theme.Rounded.Md, ClassesOf(cut.Find("div")));
        Assert.Contains(Theme.Rounded.Sm, ClassesOf(Button(cut, "a")));
    }

    [Fact]
    public void Rounded_AnItemCanSetItsOwn()
    {
        // Act
        var cut = RenderSingle("a", Items(new("a", Rounded: Rounded.Full), new("b")));

        // Assert
        Assert.Contains(Theme.Rounded.Full, ClassesOf(Button(cut, "a")));
        Assert.Contains(Theme.Rounded.Md, ClassesOf(Button(cut, "b")));
    }

    [Fact]
    public void Shadow_IsAppliedToTheGroup()
    {
        // Act
        var group = RenderSingle("a", configure: p => p.Add(x => x.Shadow, Shadow.Md)).Find("div");

        // Assert
        Assert.Contains(Theme.Shadows.Md, ClassesOf(group));
    }

    #endregion

    #region Selection and binding

    [Fact]
    public void Single_Click_SelectsTheItem_AndRaisesValueChanged()
    {
        // Arrange
        string? raised = null;
        var cut = RenderSingle("a", configure: p => p.Add(x => x.ValueChanged, (string? v) => raised = v));

        // Act
        Button(cut, "c").Click();

        // Assert
        Assert.Equal("c", raised);
        Assert.Equal("true", Button(cut, "c").GetAttribute("aria-checked"));
        Assert.Equal("false", Button(cut, "a").GetAttribute("aria-checked"));
    }

    [Fact]
    public void Single_ClickingThePressedItem_KeepsIt_ByDefault()
    {
        // Arrange
        var raised = false;
        var cut = RenderSingle("a", configure: p => p.Add(x => x.ValueChanged, (string? _) => raised = true));

        // Act
        Button(cut, "a").Click();

        // Assert
        Assert.False(raised);
        Assert.Equal("true", Button(cut, "a").GetAttribute("aria-checked"));
    }

    [Fact]
    public void Single_Deselectable_ClickingThePressedItem_ReleasesIt()
    {
        // Arrange
        var raised = new List<string?>();
        var cut = RenderSingle("a", configure: p => p.Add(x => x.Deselectable, true).Add(x => x.ValueChanged, (string? v) => raised.Add(v)));

        // Act
        Button(cut, "a").Click();

        // Assert
        Assert.Equal([null], raised);
        Assert.All(cut.FindAll("button"), b => Assert.Equal("false", b.GetAttribute("aria-checked")));
    }

    [Fact]
    public void Multiple_Click_AddsAndRemovesValues()
    {
        // Arrange
        var raised = new List<string[]>();
        var cut = RenderMultiple(["a"], configure: p => p.Add(x => x.ValuesChanged, (IEnumerable<string> v) => raised.Add([.. v])));

        // Act
        Button(cut, "c").Click();
        Button(cut, "a").Click();

        // Assert
        Assert.Equal(["a", "c"], raised[0]);
        Assert.Equal(["c"], raised[1]);
        Assert.Equal("false", Button(cut, "a").GetAttribute("aria-pressed"));
        Assert.Equal("true", Button(cut, "c").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void Single_ParameterChange_UpdatesThePressedItem()
    {
        // Arrange
        var cut = RenderSingle("a");

        // Act
        cut.Render(p => p.Add(x => x.Value, "b"));

        // Assert
        Assert.Equal("true", Button(cut, "b").GetAttribute("aria-checked"));
        Assert.Equal("false", Button(cut, "a").GetAttribute("aria-checked"));
    }

    [Fact]
    public void IntValues_AreComparedByValue()
    {
        // Act
        var cut = TestContext.Render<TwToggleGroup<int>>(p => p
            .Add(x => x.Multiple, true)
            .Add(x => x.Values, new[] { 2 })
            .Add(x => x.ChildContent, (RenderFragment)(b =>
            {
                for (var i = 1; i <= 3; i++)
                {
                    b.OpenComponent<TwToggleGroupItem<int>>(i * 10);
                    b.AddAttribute(i * 10 + 1, "Value", i);
                    b.AddAttribute(i * 10 + 2, "Label", i.ToString());
                    b.CloseComponent();
                }
            })));

        // Assert
        var pressed = cut.FindAll("button").Select(b => b.GetAttribute("aria-pressed")).ToArray();
        Assert.Equal(["false", "true", "false"], pressed);
    }

    #endregion

    #region Disabled

    [Fact]
    public void Disabled_DisablesEveryItem()
    {
        // Arrange
        var raised = false;
        var cut = RenderSingle("a", configure: p => p.Add(x => x.Disabled, true).Add(x => x.ValueChanged, (string? _) => raised = true));

        // Assert
        Assert.Equal("true", cut.Find("div").GetAttribute("aria-disabled"));
        Assert.All(cut.FindAll("button"), b => Assert.True(b.HasAttribute("disabled")));
        Assert.All(cut.FindAll("button"), b => AssertHasAll(theme.Disabled, ClassesOf(b)));
        Assert.False(raised);
    }

    [Fact]
    public void DisabledItem_IsNativelyDisabled_AndOthersStayEnabled()
    {
        // Act
        var cut = RenderSingle("a", Items(new("a"), new("b", Disabled: true)));

        // Assert
        Assert.True(Button(cut, "b").HasAttribute("disabled"));
        Assert.False(Button(cut, "a").HasAttribute("disabled"));
    }

    #endregion

    #region Roving tabindex and keyboard

    [Fact]
    public void Single_ThePressedItem_IsTheOnlyTabStop()
    {
        // Act
        var cut = RenderSingle("b");

        // Assert
        Assert.Equal("-1", Button(cut, "a").GetAttribute("tabindex"));
        Assert.Equal("0", Button(cut, "b").GetAttribute("tabindex"));
        Assert.Equal("-1", Button(cut, "c").GetAttribute("tabindex"));
    }

    [Fact]
    public void Single_WithNoSelection_TheFirstEnabledItem_IsTheTabStop()
    {
        // Act
        var cut = RenderSingle(null, Items(new("a", Disabled: true), new("b"), new("c")));

        // Assert
        Assert.Equal("0", Button(cut, "b").GetAttribute("tabindex"));
        Assert.Equal("-1", Button(cut, "c").GetAttribute("tabindex"));
    }

    [Fact]
    public void Multiple_TheFirstEnabledItem_IsTheTabStop()
    {
        // Act
        var cut = RenderMultiple(["c"]);

        // Assert
        Assert.Equal("0", Button(cut, "a").GetAttribute("tabindex"));
        Assert.Equal("-1", Button(cut, "c").GetAttribute("tabindex"));
    }

    [Fact]
    public void Multiple_TheFocusedItem_KeepsTheTabStop()
    {
        // Arrange
        var cut = RenderMultiple([]);

        // Act
        Button(cut, "b").Focus();

        // Assert
        Assert.Equal("0", Button(cut, "b").GetAttribute("tabindex"));
        Assert.Equal("-1", Button(cut, "a").GetAttribute("tabindex"));
    }

    [Theory]
    [InlineData("ArrowRight", "b")]
    [InlineData("ArrowDown", "b")]
    [InlineData("ArrowLeft", "c")]
    [InlineData("ArrowUp", "c")]
    [InlineData("Home", "a")]
    [InlineData("End", "c")]
    public void Single_ArrowKeys_MoveAndSelect(string key, string expected)
    {
        // Arrange
        string? raised = null;
        var cut = RenderSingle("a", configure: p => p.Add(x => x.ValueChanged, (string? v) => raised = v));

        // Act
        cut.Find("div").KeyDown(new KeyboardEventArgs { Key = key });

        // Assert
        if (expected == "a")
        {
            Assert.Null(raised); // already on the first item
        }
        else
        {
            Assert.Equal(expected, raised);
        }

        Assert.Equal("true", Button(cut, expected).GetAttribute("aria-checked"));
        Assert.Equal("0", Button(cut, expected).GetAttribute("tabindex"));
    }

    [Fact]
    public void Single_ArrowRight_WrapsFromTheLastToTheFirst()
    {
        // Arrange
        var cut = RenderSingle("c");

        // Act
        cut.Find("div").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        Assert.Equal("true", Button(cut, "a").GetAttribute("aria-checked"));
    }

    [Fact]
    public void ArrowKeys_SkipDisabledItems()
    {
        // Arrange
        var cut = RenderSingle("a", Items(new("a"), new("b", Disabled: true), new("c")));

        // Act
        cut.Find("div").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        Assert.Equal("true", Button(cut, "c").GetAttribute("aria-checked"));
    }

    [Fact]
    public void Multiple_ArrowKeys_MoveFocusWithoutPressing()
    {
        // Arrange
        var raised = false;
        var cut = RenderMultiple([], configure: p => p.Add(x => x.ValuesChanged, (IEnumerable<string> _) => raised = true));

        // Act
        cut.Find("div").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        Assert.False(raised);
        Assert.Equal("0", Button(cut, "b").GetAttribute("tabindex"));
        Assert.Equal("-1", Button(cut, "a").GetAttribute("tabindex"));
        Assert.All(cut.FindAll("button"), b => Assert.Equal("false", b.GetAttribute("aria-pressed")));
    }

    [Fact]
    public void Multiple_ArrowKeys_ContinueFromTheMovedTabStop()
    {
        // Arrange
        var cut = RenderMultiple([]);

        // Act
        cut.Find("div").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        cut.Find("div").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        Assert.Equal("0", Button(cut, "c").GetAttribute("tabindex"));
    }

    [Fact]
    public void ArrowKeys_MoveFocus_ToTheTargetItem()
    {
        // Arrange
        var cut = RenderSingle("a");

        // Act
        cut.Find("div").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier.Contains("focus", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void UnhandledKey_DoesNothing()
    {
        // Arrange
        var raised = false;
        var cut = RenderSingle("a", configure: p => p.Add(x => x.ValueChanged, (string? _) => raised = true));

        // Act
        cut.Find("div").KeyDown(new KeyboardEventArgs { Key = "a" });

        // Assert
        Assert.False(raised);
        Assert.Equal("true", Button(cut, "a").GetAttribute("aria-checked"));
    }

    [Fact]
    public void Disabled_Group_IgnoresArrowKeys()
    {
        // Arrange
        var raised = false;
        var cut = RenderSingle("a", configure: p => p.Add(x => x.Disabled, true).Add(x => x.ValueChanged, (string? _) => raised = true));

        // Act
        cut.Find("div").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        // Assert
        Assert.False(raised);
    }

    [Theory]
    [InlineData("ArrowLeft", "b")]
    [InlineData("ArrowRight", "c")]
    public void RightToLeft_SwapsArrowLeftAndRight(string key, string expected)
    {
        // Arrange
        var cut = TestContext.Render<TwDirectionProvider>(p => p
            .Add(x => x.Direction, TextDirection.RightToLeft)
            .AddChildContent<TwToggleGroup<string>>(g => g
                .Add(x => x.Value, "a")
                .Add(x => x.ChildContent, abc)));

        // Act
        cut.Find("div[role='radiogroup']").KeyDown(new KeyboardEventArgs { Key = key });

        // Assert
        Assert.Equal("true", cut.Find($"#item-{expected}").GetAttribute("aria-checked"));
    }

    [Fact]
    public void RegistersTheKeydownGuard_OnFirstRender()
    {
        // Act
        RenderSingle("a");

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twTabs.registerKeydownGuard");
    }

    [Fact]
    public async Task DisposeAsync_UnregistersTheKeydownGuard()
    {
        // Arrange
        var cut = RenderSingle("a");

        // Act
        await cut.Instance.DisposeAsync();

        // Assert
        Assert.Contains(TestContext.JSInterop.Invocations, i => i.Identifier == "twTabs.unregisterKeydownGuard");
    }

    [Fact]
    public void RemovingAnItem_DropsItFromTheGroup()
    {
        // Arrange
        var cut = RenderMultiple([]);
        cut.Find("div").KeyDown(new KeyboardEventArgs { Key = "End" }); // c is the tab stop

        // Act
        cut.Render(p => p.Add(x => x.ChildContent, Items(new("a"), new("b"))));

        // Assert
        Assert.Equal(2, cut.FindAll("button").Count);
        Assert.Equal("0", Button(cut, "a").GetAttribute("tabindex"));
    }

    #endregion
}
