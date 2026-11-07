using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using IconEnum = TwBlazor.Enums.Icon;

namespace TwBlazor.Tests.Components.InputGroup;

public class TwInputGroupTests : TwBlazorTestBase
{
    private const string controlMarker = "data-tw-input-group-control";

    private static readonly string[] _countries = ["UK", "USA"];

    private static readonly string[] _cities = ["London", "Leeds"];

    private TwInputGroupTheme theme => Theme.Components.Require<TwInputGroupTheme>();

    private TwInputTheme inputTheme => Theme.Components.Require<TwInputTheme>();

    private static string[] ClassesOf(IElement element) =>
        (element.GetAttribute("class") ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);

    private static void AssertHasClasses(IElement element, string expected)
    {
        var actual = ClassesOf(element);
        foreach (var token in expected.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            Assert.Contains(token, actual);
        }
    }

    private static RenderFragment Textfield(Action<RenderTreeBuilderAttributes>? configure = null) => builder =>
    {
        builder.OpenComponent<TwTextfield<string>>(0);
        builder.AddAttribute(1, nameof(TwTextfield<string>.Id), "field");
        configure?.Invoke(new RenderTreeBuilderAttributes(builder));
        builder.CloseComponent();
    };

    private static RenderFragment Addon(InputGroupAlign align, RenderFragment content) => builder =>
    {
        builder.OpenComponent<TwInputGroupAddon>(0);
        builder.AddAttribute(1, nameof(TwInputGroupAddon.Align), align);
        builder.AddAttribute(2, nameof(TwInputGroupAddon.ChildContent), content);
        builder.CloseComponent();
    };

    private static RenderFragment Text(string text, string id, bool describesInput = true) => builder =>
    {
        builder.OpenComponent<TwInputGroupText>(0);
        builder.AddAttribute(1, nameof(TwInputGroupText.Id), id);
        builder.AddAttribute(2, nameof(TwInputGroupText.Text), text);
        builder.AddAttribute(3, nameof(TwInputGroupText.DescribesInput), describesInput);
        builder.CloseComponent();
    };

    private static RenderFragment Combine(params RenderFragment[] fragments) => builder =>
    {
        foreach (var fragment in fragments)
        {
            builder.AddContent(0, fragment);
        }
    };

    /// <summary>
    /// Adds attributes to the component currently open on a <see cref="Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder"/>.
    /// </summary>
    private sealed class RenderTreeBuilderAttributes(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        public RenderTreeBuilderAttributes Add(string name, object? value)
        {
            builder.AddAttribute(10, name, value);
            return this;
        }
    }

    private IRenderedComponent<TwInputGroup> RenderGroup(RenderFragment content, Action<ComponentParameterCollectionBuilder<TwInputGroup>>? configure = null) =>
        TestContext.Render<TwInputGroup>(parameters =>
        {
            parameters.Add(p => p.ChildContent, content);
            configure?.Invoke(parameters);
        });

    private static IElement Box(IRenderedComponent<TwInputGroup> cut) => cut.Find("[data-tw-input-group]");

    #region Group

    [Fact]
    public void TwInputGroup_RendersARoleGroup_WithTheRootAndGroupClasses()
    {
        var cut = RenderGroup(Textfield());

        var root = cut.Find("[role=group]");
        AssertHasClasses(root, theme.Root);
        AssertHasClasses(Box(cut), theme.Group);
        Assert.True(root.Contains(Box(cut)));
    }

    [Fact]
    public void TwInputGroup_RendersIdStyleAndAttributes_OnTheRoot()
    {
        var cut = RenderGroup(Textfield(), p => p
            .Add(x => x.Id, "group")
            .Add(x => x.Style, "color: red;")
            .AddUnmatched("data-testid", "group-test"));

        var root = cut.Find("[role=group]");
        Assert.Equal("group", root.Id);
        Assert.Equal("color: red;", root.GetAttribute("style"));
        Assert.Equal("group-test", root.GetAttribute("data-testid"));
    }

    [Fact]
    public void TwInputGroup_Class_StylesTheGroup_AndRootClass_StylesTheRoot()
    {
        var cut = RenderGroup(Textfield(), p => p
            .Add(x => x.Class, "custom-group")
            .Add(x => x.RootClass, "custom-root"));

        Assert.Contains("custom-group", ClassesOf(Box(cut)));
        Assert.DoesNotContain("custom-group", ClassesOf(cut.Find("[role=group]")));
        Assert.Contains("custom-root", ClassesOf(cut.Find("[role=group]")));
    }

    [Fact]
    public void TwInputGroup_Class_ReplacesConflictingThemeClasses()
    {
        var cut = RenderGroup(Textfield(), p => p
            .Add(x => x.Variant, InputVariant.Outlined)
            .Add(x => x.Class, "rounded-none"));

        var classes = ClassesOf(Box(cut));
        Assert.Contains("rounded-none", classes);
        Assert.DoesNotContain(RoundedBuilder.GetRounded(Rounded.Lg), classes);
    }

    [Fact]
    public void TwInputGroup_UsesTheThemesDefaultInputVariant_WhenNoVariantIsSet()
    {
        var cut = RenderGroup(Textfield());

        Assert.Equal(InputVariant.Outlined, inputTheme.DefaultInputVariant);
        AssertHasClasses(Box(cut), inputTheme.OutlinedBorder);
    }

    [Fact]
    public void TwInputGroup_Outlined_DrawsTheOutlinedBorder_RoundedOnEveryCorner()
    {
        var cut = RenderGroup(Textfield(), p => p.Add(x => x.Variant, InputVariant.Outlined));

        var box = Box(cut);
        AssertHasClasses(box, inputTheme.OutlinedBorder);
        AssertHasClasses(box, RoundedBuilder.GetRounded(Rounded.Lg));
        AssertHasClasses(box, theme.Padding);
        AssertHasClasses(box, theme.FocusBorder);
        AssertHasClasses(box, theme.InvalidBorder);
        AssertHasClasses(box, theme.Disabled);
    }

    [Fact]
    public void TwInputGroup_Filled_DrawsTheFilledBackground_RoundedOnTheTopCorners()
    {
        var cut = RenderGroup(Textfield(), p => p.Add(x => x.Variant, InputVariant.Filled));

        var box = Box(cut);
        AssertHasClasses(box, inputTheme.FilledBorder);
        AssertHasClasses(box, inputTheme.FilledBackgroundColor);
        AssertHasClasses(box, RoundedBuilder.GetRoundedTop(Rounded.Lg));
        AssertHasClasses(box, theme.Padding);
    }

    [Fact]
    public void TwInputGroup_Default_DrawsOnlyTheBottomBorder_WithoutPadding()
    {
        var cut = RenderGroup(Textfield(), p => p.Add(x => x.Variant, InputVariant.Default));

        var box = Box(cut);
        AssertHasClasses(box, inputTheme.FilledBorder);
        AssertHasClasses(box, theme.DefaultPadding);
        Assert.DoesNotContain(RoundedBuilder.GetRounded(Rounded.Lg), ClassesOf(box));
        Assert.DoesNotContain(RoundedBuilder.GetRoundedTop(Rounded.Lg), ClassesOf(box));
    }

    [Theory]
    [InlineData(Rounded.None)]
    [InlineData(Rounded.Sm)]
    [InlineData(Rounded.Md)]
    [InlineData(Rounded.Full)]
    public void TwInputGroup_Rounded_RoundsTheGroup(Rounded rounded)
    {
        var cut = RenderGroup(Textfield(), p => p
            .Add(x => x.Variant, InputVariant.Outlined)
            .Add(x => x.Rounded, rounded));

        AssertHasClasses(Box(cut), RoundedBuilder.GetRounded(rounded));
    }

    [Fact]
    public void TwInputGroup_Label_IsRendered_ForTheInput_AndNamesTheGroup()
    {
        var cut = RenderGroup(Textfield(), p => p
            .Add(x => x.Id, "group")
            .Add(x => x.Label, "Website")
            .Add(x => x.For, "field"));

        var label = cut.Find("label");
        Assert.Equal("Website", label.TextContent.Trim());
        Assert.Equal("field", label.GetAttribute("for"));
        Assert.Equal("group-label", label.Id);
        Assert.Equal("group-label", cut.Find("[role=group]").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void TwInputGroup_LabelClass_IsAppliedToTheLabel()
    {
        var cut = RenderGroup(Textfield(), p => p
            .Add(x => x.Label, "Website")
            .Add(x => x.LabelClass, "custom-label"));

        Assert.Contains("custom-label", ClassesOf(cut.Find("label")));
    }

    [Fact]
    public void TwInputGroup_WithoutLabel_RendersNoLabel_AndNoLabelledBy()
    {
        var cut = RenderGroup(Textfield());

        Assert.Empty(cut.FindAll("label"));
        Assert.False(cut.Find("[role=group]").HasAttribute("aria-labelledby"));
    }

    [Fact]
    public void TwInputGroup_AriaLabel_NamesTheGroup_InsteadOfTheLabel()
    {
        var cut = RenderGroup(Textfield(), p => p
            .Add(x => x.Label, "Website")
            .Add(x => x.AriaLabel, "Company website"));

        var root = cut.Find("[role=group]");
        Assert.Equal("Company website", root.GetAttribute("aria-label"));
        Assert.False(root.HasAttribute("aria-labelledby"));
    }

    [Fact]
    public void TwInputGroup_AriaLabelledBy_WinsOverTheLabel()
    {
        var cut = RenderGroup(Textfield(), p => p
            .Add(x => x.Label, "Website")
            .Add(x => x.AriaLabelledBy, "heading"));

        Assert.Equal("heading", cut.Find("[role=group]").GetAttribute("aria-labelledby"));
    }

    #endregion

    #region Inputs in a group

    [Fact]
    public void TwTextfield_InAGroup_IsMarkedAsTheGroupsControl()
    {
        var cut = RenderGroup(Textfield());

        Assert.True(cut.Find("input").HasAttribute(controlMarker));
    }

    [Fact]
    public void TwTextfield_OutsideAGroup_IsNotMarked_AndKeepsItsOwnBorder()
    {
        var cut = TestContext.Render<TwTextfield<string>>(p => p.Add(x => x.Variant, InputVariant.Outlined));

        var input = cut.Find("input");
        Assert.False(input.HasAttribute(controlMarker));
        AssertHasClasses(input, inputTheme.OutlinedBorder);
    }

    [Fact]
    public void TwTextfield_InAGroup_DropsTheBorderBackgroundRoundingAndPadding_TheGroupDraws()
    {
        var cut = RenderGroup(Textfield(a => a.Add("Variant", InputVariant.Filled)));

        var classes = ClassesOf(cut.Find("input"));
        foreach (var token in theme.Control.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            Assert.Contains(token, classes);
        }

        Assert.DoesNotContain("border-b", classes);
        Assert.DoesNotContain(RoundedBuilder.GetRoundedTop(Rounded.Lg), classes);
        Assert.DoesNotContain(inputTheme.FilledBackgroundColor.Split(' ')[0], classes);
        Assert.DoesNotContain("px-3", classes);
    }

    [Fact]
    public void TwTextfield_InAGroup_KeepsItsOwnClassLast()
    {
        var cut = RenderGroup(Textfield(a => a.Add("Class", "ps-10")));

        Assert.Contains("ps-10", ClassesOf(cut.Find("input")));
    }

    [Fact]
    public void TwTextfield_InAnOutlinedGroup_RootFillsTheRow_AndTakesTheBorderOffItsHeight()
    {
        var cut = RenderGroup(Textfield(a => a.Add("RootId", "field-root")), p => p.Add(x => x.Variant, InputVariant.Outlined));

        var root = cut.Find("#field-root");
        AssertHasClasses(root, theme.ControlRoot);
        AssertHasClasses(root, theme.ControlRootOutlinedOffset);
    }

    [Theory]
    [InlineData(InputVariant.Filled)]
    [InlineData(InputVariant.Default)]
    public void TwTextfield_InAnUnderlinedGroup_TakesTheBottomBorderOffItsHeight(InputVariant variant)
    {
        var cut = RenderGroup(Textfield(a => a.Add("RootId", "field-root")), p => p.Add(x => x.Variant, variant));

        var root = cut.Find("#field-root");
        AssertHasClasses(root, theme.ControlRootUnderlinedOffset);
        Assert.DoesNotContain(theme.ControlRootOutlinedOffset, ClassesOf(root));
    }

    [Fact]
    public void TwTextfield_InAGroup_KeepsItsRootClass()
    {
        var cut = RenderGroup(Textfield(a => a.Add("RootId", "field-root").Add("RootClass", "custom-root")));

        Assert.Contains("custom-root", ClassesOf(cut.Find("#field-root")));
    }

    [Fact]
    public void TwTextfield_InADenseGroup_UsesTheDenseSize()
    {
        var cut = RenderGroup(Textfield(), p => p.Add(x => x.Dense, true));

        AssertHasClasses(cut.Find("input"), inputTheme.DenseSize);
    }

    [Fact]
    public void TwTextfield_InAGroup_IsNotDimmedTwice_WhenDisabled()
    {
        var cut = RenderGroup(Textfield(a => a.Add("Disabled", true)));

        var input = cut.Find("input");
        Assert.True(input.HasAttribute("disabled"));
        Assert.DoesNotContain("opacity-40", ClassesOf(input));
    }

    [Fact]
    public void TwTextfield_InAGroup_ShowsItsErrorBelowTheGroup_LinkedToTheInput()
    {
        var cut = RenderGroup(Textfield(a => a.Add("RootId", "field-root").Add("Invalid", true).Add("ErrorMessage", "Required.")));

        var error = cut.Find("p[role=alert]");
        Assert.Equal("Required.", error.TextContent);
        Assert.Equal("field-error", error.Id);
        Assert.False(Box(cut).Contains(error));
        Assert.Empty(cut.Find("#field-root").QuerySelectorAll("p"));

        var input = cut.Find("input");
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Equal("field-error", input.GetAttribute("aria-describedby"));
    }

    [Fact]
    public void TwTextfield_InAGroup_RemovesItsError_OnceValid()
    {
        var invalid = true;
        RenderFragment content = builder =>
        {
            builder.OpenComponent<TwTextfield<string>>(0);
            builder.AddAttribute(1, "Id", "field");
            builder.AddAttribute(2, "Invalid", invalid);
            builder.AddAttribute(3, "ErrorMessage", "Required.");
            builder.CloseComponent();
        };
        var cut = RenderGroup(content);
        Assert.Single(cut.FindAll("p[role=alert]"));

        invalid = false;
        cut.Render(p => p.Add(x => x.ChildContent, content));

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll("p[role=alert]")));
        Assert.False(cut.Find("input").HasAttribute("aria-describedby"));
    }

    [Fact]
    public void TwTextfield_InAGroup_RemovesItsError_WhenItIsRemoved()
    {
        var show = true;
        RenderFragment content = builder =>
        {
            if (show)
            {
                builder.OpenComponent<TwTextfield<string>>(0);
                builder.AddAttribute(1, "Id", "field");
                builder.AddAttribute(2, "Invalid", true);
                builder.AddAttribute(3, "ErrorMessage", "Required.");
                builder.CloseComponent();
            }
        };
        var cut = RenderGroup(content);
        Assert.Single(cut.FindAll("p[role=alert]"));

        show = false;
        cut.Render(p => p.Add(x => x.ChildContent, content));

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll("p[role=alert]")));
    }

    [Fact]
    public void TwTextfield_Multiline_InAGroup_IsMarkedAsTheGroupsControl()
    {
        var cut = RenderGroup(Combine(
            Textfield(a => a.Add("Multiline", true)),
            Addon(InputGroupAlign.BlockEnd, Text("0/280", "counter", describesInput: false))));

        var textarea = cut.Find("textarea");
        Assert.True(textarea.HasAttribute(controlMarker));
        AssertHasClasses(textarea, theme.Control);
    }

    [Fact]
    public void TwNumeric_InAGroup_LaysOutItsOwnRoot_AndMarksItsInput()
    {
        RenderFragment content = builder =>
        {
            builder.OpenComponent<TwNumeric<int>>(0);
            builder.AddAttribute(1, "Id", "number");
            builder.AddAttribute(2, "RootId", "number-root");
            builder.CloseComponent();
        };
        var cut = RenderGroup(content);

        AssertHasClasses(cut.Find("#number-root"), theme.ControlRoot);
        Assert.True(cut.Find("input").HasAttribute(controlMarker));
    }

    [Fact]
    public void TwDatePicker_InAGroup_OnlyItsOuterRootIsLaidOutByTheGroup()
    {
        RenderFragment content = builder =>
        {
            builder.OpenComponent<TwDatePicker>(0);
            builder.AddAttribute(1, "Id", "date");
            builder.AddAttribute(2, "RootId", "date-root");
            builder.CloseComponent();
        };
        var cut = RenderGroup(content);

        var outerRoot = cut.Find("#date-root");
        AssertHasClasses(outerRoot, theme.ControlRootOutlinedOffset);

        // The text field inside the picker has a root of its own, which must not take the border off twice.
        var nestedRoots = outerRoot.QuerySelectorAll("div[id]").Where(d => d.QuerySelector("input") is not null).ToList();
        Assert.NotEmpty(nestedRoots);
        Assert.All(nestedRoots, r => Assert.DoesNotContain(theme.ControlRootOutlinedOffset, ClassesOf(r)));
        Assert.True(cut.Find("input").HasAttribute(controlMarker));
    }

    [Fact]
    public void TwSelect_InAGroup_IsMarked_DropsItsBorder_AndKeepsRoomForTheChevron()
    {
        RenderFragment content = builder =>
        {
            builder.OpenComponent<TwSelect<string>>(0);
            builder.AddAttribute(1, "Id", "country");
            builder.AddAttribute(2, "Values", _countries);
            builder.AddAttribute(3, "PreferNativePicker", true);
            builder.CloseComponent();
        };
        var cut = RenderGroup(content);

        var select = cut.Find("select");
        Assert.True(select.HasAttribute(controlMarker));
        var classes = ClassesOf(select);
        Assert.Contains("border-0", classes);
        Assert.Contains("pe-10", classes);
        // The group's Control classes strip the select's own background, so a native select keeps its
        // popup-safe background even inside a group, rather than falling back to the OS popup surface.
        AssertHasClasses(select, inputTheme.SelectNativeBackground);
    }

    [Fact]
    public void TwSelect_OutsideAGroup_IsNotMarked()
    {
        var cut = TestContext.Render<TwSelect<string>>(p => p
            .Add(x => x.Values, _countries)
            .Add(x => x.PreferNativePicker, true));

        Assert.False(cut.Find("select").HasAttribute(controlMarker));
    }

    [Fact]
    public void TwSelect_InAGroup_CustomTrigger_IsMarked_AndDropsItsBorder()
    {
        RenderFragment content = builder =>
        {
            builder.OpenComponent<TwSelect<string>>(0);
            builder.AddAttribute(1, "Id", "country");
            builder.AddAttribute(2, "Values", _countries);
            builder.AddAttribute(3, "PreferNativePicker", false);
            builder.CloseComponent();
        };
        var cut = RenderGroup(content);

        var trigger = cut.Find("button[role=combobox]");
        Assert.True(trigger.HasAttribute(controlMarker));
        Assert.Contains("border-0", ClassesOf(trigger));
    }

    [Fact]
    public void TwAutocomplete_InAGroup_IsMarked_AndDropsItsBorder()
    {
        RenderFragment content = builder =>
        {
            builder.OpenComponent<TwAutocomplete<string>>(0);
            builder.AddAttribute(1, "Id", "city");
            builder.AddAttribute(2, "Values", _cities);
            builder.CloseComponent();
        };
        var cut = RenderGroup(content);

        var input = cut.Find("input[role=combobox]");
        Assert.True(input.HasAttribute(controlMarker));
        Assert.Contains("border-0", ClassesOf(input));
    }

    [Fact]
    public void TwColorPicker_InAGroup_ItsPanelFields_AreNotJoinedToTheGroup()
    {
        RenderFragment content = builder =>
        {
            builder.OpenComponent<TwColorPicker>(0);
            builder.AddAttribute(1, "Id", "colour");
            builder.CloseComponent();
        };
        var cut = RenderGroup(content);
        Assert.Single(cut.FindAll($"[{controlMarker}]"));

        cut.Find("button[aria-haspopup='dialog']").Click();

        var panel = cut.Find("[role=dialog]");
        var panelInputs = panel.QuerySelectorAll("input");
        Assert.NotEmpty(panelInputs);
        Assert.All(panelInputs, i => Assert.False(i.HasAttribute(controlMarker)));
        Assert.Single(cut.FindAll($"[{controlMarker}]"));
    }

    #endregion

    #region Addon

    [Theory]
    [InlineData(InputGroupAlign.InlineStart, "inline-start")]
    [InlineData(InputGroupAlign.InlineEnd, "inline-end")]
    [InlineData(InputGroupAlign.BlockStart, "block-start")]
    [InlineData(InputGroupAlign.BlockEnd, "block-end")]
    public void TwInputGroupAddon_Align_SetsDataAlign_AndItsClasses(InputGroupAlign align, string expected)
    {
        var cut = TestContext.Render<TwInputGroupAddon>(p => p.Add(x => x.Align, align));

        var addon = cut.Find("div");
        Assert.Equal(expected, addon.GetAttribute("data-align"));
        AssertHasClasses(addon, theme.Addon);
        var alignClasses = align switch
        {
            InputGroupAlign.InlineEnd => theme.AddonInlineEnd,
            InputGroupAlign.BlockStart => theme.AddonBlockStart,
            InputGroupAlign.BlockEnd => theme.AddonBlockEnd,
            _ => theme.AddonInlineStart
        };
        AssertHasClasses(addon, alignClasses);
    }

    [Fact]
    public void TwInputGroupAddon_DefaultsToInlineStart()
    {
        var cut = TestContext.Render<TwInputGroupAddon>();

        Assert.Equal("inline-start", cut.Find("div").GetAttribute("data-align"));
    }

    [Fact]
    public void TwInputGroupAddon_RendersItsContent_IdClassStyleAndAttributes()
    {
        var cut = TestContext.Render<TwInputGroupAddon>(p => p
            .Add(x => x.Id, "addon")
            .Add(x => x.Class, "custom-addon")
            .Add(x => x.Style, "color: red;")
            .AddUnmatched("data-testid", "addon-test")
            .Add(x => x.ChildContent, RenderFragmentBuilder("kg")));

        var addon = cut.Find("div");
        Assert.Equal("addon", addon.Id);
        Assert.Contains("custom-addon", ClassesOf(addon));
        Assert.Equal("color: red;", addon.GetAttribute("style"));
        Assert.Equal("addon-test", addon.GetAttribute("data-testid"));
        Assert.Equal("kg", addon.TextContent.Trim());
    }

    #endregion

    #region Text

    [Fact]
    public void TwInputGroupText_RendersItsText_WithTheThemeClasses()
    {
        var cut = TestContext.Render<TwInputGroupText>(p => p
            .Add(x => x.Text, "https://")
            .Add(x => x.Class, "custom-text"));

        var span = cut.Find("span");
        Assert.Equal("https://", span.TextContent);
        AssertHasClasses(span, theme.Text);
        Assert.Contains("custom-text", ClassesOf(span));
    }

    [Fact]
    public void TwInputGroupText_ChildContent_IsShownInsteadOfText()
    {
        var cut = TestContext.Render<TwInputGroupText>(p => p
            .Add(x => x.Text, "ignored")
            .Add(x => x.ChildContent, RenderFragmentBuilder("shown")));

        Assert.Equal("shown", cut.Find("span").TextContent);
    }

    [Fact]
    public void TwInputGroupText_DescribesTheInput()
    {
        var cut = RenderGroup(Combine(
            Addon(InputGroupAlign.InlineStart, Text("https://", "prefix")),
            Textfield(),
            Addon(InputGroupAlign.InlineEnd, Text(".com", "suffix"))));

        cut.WaitForAssertion(() => Assert.Equal("prefix suffix", cut.Find("input").GetAttribute("aria-describedby")));
    }

    [Fact]
    public void TwInputGroupText_DescribesInputFalse_IsNotAddedToTheInput()
    {
        var cut = RenderGroup(Combine(
            Textfield(),
            Addon(InputGroupAlign.InlineEnd, Text("0/10", "counter", describesInput: false))));

        Assert.False(cut.Find("input").HasAttribute("aria-describedby"));
    }

    [Fact]
    public void TwInputGroupText_IsAddedAfterTheInputsErrorMessage()
    {
        var cut = RenderGroup(Combine(
            Addon(InputGroupAlign.InlineStart, Text("@", "prefix")),
            Textfield(a => a.Add("Invalid", true).Add("ErrorMessage", "Required."))));

        cut.WaitForAssertion(() => Assert.Equal("field-error prefix", cut.Find("input").GetAttribute("aria-describedby")));
    }

    [Fact]
    public void TwInputGroupText_IsRemovedFromTheInput_WhenItIsRemoved()
    {
        var show = true;
        RenderFragment content = builder =>
        {
            if (show)
            {
                builder.AddContent(0, Addon(InputGroupAlign.InlineStart, Text("https://", "prefix")));
            }

            builder.AddContent(1, Textfield());
        };
        var cut = RenderGroup(content);
        cut.WaitForAssertion(() => Assert.Equal("prefix", cut.Find("input").GetAttribute("aria-describedby")));

        show = false;
        cut.Render(p => p.Add(x => x.ChildContent, content));

        cut.WaitForAssertion(() => Assert.False(cut.Find("input").HasAttribute("aria-describedby")));
    }

    [Fact]
    public void TwInputGroupText_OutsideAGroup_DescribesNothing()
    {
        var cut = TestContext.Render<TwInputGroupText>(p => p.Add(x => x.Text, "kg"));

        Assert.Equal("kg", cut.Find("span").TextContent);
        cut.Instance.Dispose();
    }

    [Fact]
    public void JoinDescribedBy_JoinsTheInputsIdsWithTheGroups()
    {
        Assert.Null(TwInputGroupContext.JoinDescribedBy(null, null));
        Assert.Null(TwInputGroupContext.JoinDescribedBy(null, string.Empty));
        Assert.Equal("error", TwInputGroupContext.JoinDescribedBy(null, "error"));

        var group = new TwInputGroup();
        var context = new TwInputGroupContext(group, InputVariant.Outlined, false, Rounded.Lg, string.Empty, string.Empty, "prefix");
        Assert.Equal("prefix", TwInputGroupContext.JoinDescribedBy(context, null));
        Assert.Equal("error prefix", TwInputGroupContext.JoinDescribedBy(context, "error"));
    }

    #endregion

    #region Button

    private static RenderFragment Button(Action<RenderTreeBuilderAttributes>? configure = null) => builder =>
    {
        builder.OpenComponent<TwInputGroupButton>(0);
        configure?.Invoke(new RenderTreeBuilderAttributes(builder));
        builder.CloseComponent();
    };

    [Fact]
    public void TwInputGroupButton_IsACompactTextButton_ThatDoesNotSubmit()
    {
        var cut = TestContext.Render<TwInputGroupButton>(p => p.Add(x => x.Label, "Copy"));

        var button = cut.Find("button");
        Assert.Equal("button", button.GetAttribute("type"));
        Assert.Equal("Copy", button.TextContent.Trim());

        var inner = cut.FindComponent<TwButton>().Instance;
        Assert.Equal(ButtonVariant.Text, inner.Variant);
        Assert.True(inner.Dense);
        AssertHasClasses(button, theme.Button);
    }

    [Fact]
    public void TwInputGroupButton_ForwardsItsParameters_ToTheButton()
    {
        var clicked = false;
        var cut = TestContext.Render<TwInputGroupButton>(p => p
            .Add(x => x.Id, "copy")
            .Add(x => x.Label, "Copy")
            .Add(x => x.Variant, ButtonVariant.Filled)
            .Add(x => x.Color, Color.Primary)
            .Add(x => x.StartIcon, IconEnum.Copy)
            .Add(x => x.EndIcon, IconEnum.Check)
            .Add(x => x.Type, "submit")
            .Add(x => x.Tooltip, "Copy the link")
            .Add(x => x.AriaLabel, "Copy link")
            .Add(x => x.Class, "custom-button")
            .Add(x => x.Style, "color: red;")
            .AddUnmatched("data-testid", "button-test")
            .Add(x => x.OnClick, () => clicked = true));

        var inner = cut.FindComponent<TwButton>().Instance;
        Assert.Equal(ButtonVariant.Filled, inner.Variant);
        Assert.Equal(Color.Primary, inner.Color);
        Assert.Equal(IconEnum.Copy, inner.StartIcon);
        Assert.Equal(IconEnum.Check, inner.EndIcon);
        Assert.Equal("Copy the link", inner.Tooltip);

        var button = cut.Find("button");
        Assert.Equal("copy", button.Id);
        Assert.Equal("submit", button.GetAttribute("type"));
        Assert.Equal("Copy link", button.GetAttribute("aria-label"));
        Assert.Contains("custom-button", ClassesOf(button));
        Assert.Equal("button-test", button.GetAttribute("data-testid"));

        button.Click();
        Assert.True(clicked);
    }

    [Fact]
    public void TwInputGroupButton_Disabled_DisablesTheButton()
    {
        var cut = TestContext.Render<TwInputGroupButton>(p => p
            .Add(x => x.Label, "Send")
            .Add(x => x.Disabled, true));

        Assert.True(cut.Find("button").HasAttribute("disabled"));
    }

    [Fact]
    public void TwInputGroupButton_InAGroup_IsRoundedOneStepInsideTheGroup()
    {
        var cut = RenderGroup(Combine(Textfield(), Addon(InputGroupAlign.InlineEnd, Button(a => a.Add("Label", "Go")))), p => p
            .Add(x => x.Variant, InputVariant.Outlined)
            .Add(x => x.Rounded, Rounded.Lg));

        Assert.Equal(Rounded.Md, cut.FindComponent<TwButton>().Instance.Rounded);
    }

    [Fact]
    public void TwInputGroupButton_Rounded_WinsOverTheGroups()
    {
        var cut = RenderGroup(Combine(Textfield(), Addon(InputGroupAlign.InlineEnd, Button(a => a.Add("Label", "Go").Add("Rounded", Rounded.Full)))));

        Assert.Equal(Rounded.Full, cut.FindComponent<TwButton>().Instance.Rounded);
    }

    [Fact]
    public void TwInputGroupButton_IconButton_StaysRound_InAGroup()
    {
        var cut = RenderGroup(Combine(Textfield(), Addon(InputGroupAlign.InlineEnd, Button(a => a.Add("IconButton", true).Add("StartIcon", IconEnum.Copy).Add("AriaLabel", "Copy")))));

        var inner = cut.FindComponent<TwButton>().Instance;
        Assert.True(inner.IconButton);
        Assert.Null(inner.Rounded);
    }

    [Fact]
    public void TwInputGroupButton_OutsideAGroup_UsesTheButtonsOwnRounding()
    {
        var cut = TestContext.Render<TwInputGroupButton>(p => p.Add(x => x.Label, "Go"));

        Assert.Null(cut.FindComponent<TwButton>().Instance.Rounded);
    }

    [Fact]
    public void TwInputGroupButton_InADenseGroup_ShrinksToFit()
    {
        var cut = RenderGroup(Combine(
            Textfield(),
            Addon(InputGroupAlign.InlineEnd, Button(a => a.Add("Label", "Go"))),
            Addon(InputGroupAlign.InlineEnd, Button(a => a.Add("IconButton", true).Add("StartIcon", IconEnum.Copy).Add("AriaLabel", "Copy")))),
            p => p.Add(x => x.Dense, true));

        var buttons = cut.FindAll("button");
        AssertHasClasses(buttons[0], theme.ButtonDense);
        AssertHasClasses(buttons[1], theme.IconButtonDense);
    }

    [Fact]
    public void TwInputGroupButton_InARegularGroup_IsNotShrunk()
    {
        var cut = RenderGroup(Combine(Textfield(), Addon(InputGroupAlign.InlineEnd, Button(a => a.Add("Label", "Go")))));

        Assert.DoesNotContain("h-6", ClassesOf(cut.Find("button")));
    }

    #endregion
}
