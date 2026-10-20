using Bunit;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;
using TwBlazor.Models;

namespace TwBlazor.Tests.Components.Bubble;

public class TwBubbleTests : TwBlazorTestBase
{
    private TwBubbleTheme theme => Theme.Components.Require<TwBubbleTheme>();

    private static List<BubbleReaction> Reactions() =>
    [
        new() { Emoji = "👍", Count = 3, Reacted = true, Label = "Thumbs up" },
        new() { Emoji = "🎉", Count = 1200 }
    ];

    private IRenderedComponent<TwBubble> Render(Action<ComponentParameterCollectionBuilder<TwBubble>>? configure = null) =>
        TestContext.Render<TwBubble>(p =>
        {
            p.AddChildContent("Hello there");
            configure?.Invoke(p);
        });

    private AngleSharp.Dom.IElement Surface(IRenderedComponent<TwBubble> cut) => cut.Find($"div[class*='{theme.Surface}']");

    [Fact]
    public void TwBubble_RendersItsContent_WithAGeneratedId()
    {
        var cut = Render();

        Assert.StartsWith("bubble-", cut.Find("div").GetAttribute("id"));
        Assert.Equal("Hello there", Surface(cut).TextContent);
    }

    [Fact]
    public void TwBubble_ByDefault_IsSoftNeutral_AtTheStart_WithATail()
    {
        var classes = Surface(Render()).GetAttribute("class")!;

        Assert.Contains(theme.Neutral, classes);
        Assert.Contains(theme.Tip, classes);
        Assert.Contains(theme.StartTail, classes);
        Assert.DoesNotContain(theme.EndTail, classes);
        Assert.Contains(Theme.Flexbox.Align.Start, Render().Find("div").GetAttribute("class"));
    }

    [Theory]
    [InlineData(BubbleVariant.Soft)]
    [InlineData(BubbleVariant.Filled)]
    [InlineData(BubbleVariant.Outlined)]
    public void TwBubble_UsesThePaletteOfItsVariant_ForEachColor(BubbleVariant variant)
    {
        var palette = variant switch
        {
            BubbleVariant.Filled => theme.Filled,
            BubbleVariant.Outlined => theme.Outlined,
            _ => theme.Soft
        };

        foreach (var color in Enum.GetValues<Color>())
        {
            var classes = Surface(Render(p => p.Add(x => x.Variant, variant).Add(x => x.Color, color))).GetAttribute("class")!;

            var expected = TwBlazor.Builders.ColorBuilder.GetPaletteColor(color, palette, string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Assert.Superset(expected.ToHashSet(), classes.Split(' ').ToHashSet());
        }
    }

    [Fact]
    public void TwBubble_FilledWithoutColor_UsesPrimary()
    {
        var classes = Surface(Render(p => p.Add(x => x.Variant, BubbleVariant.Filled))).GetAttribute("class")!;

        Assert.Contains(theme.Filled.Primary, classes);
    }

    [Fact]
    public void TwBubble_OutlinedWithoutColor_IsNeutral_AndHasABorder()
    {
        var classes = Surface(Render(p => p.Add(x => x.Variant, BubbleVariant.Outlined))).GetAttribute("class")!;

        Assert.Contains(theme.NeutralOutlined, classes);
        Assert.Contains(Theme.Border.Width.Thin, classes);
    }

    [Fact]
    public void TwBubble_AlignEnd_PutsTheTailAtTheEnd()
    {
        var cut = Render(p => p.Add(x => x.Align, MessageAlign.End));

        Assert.Contains(theme.EndTail, Surface(cut).GetAttribute("class"));
        Assert.Contains(Theme.Flexbox.Align.End, cut.Find("div").GetAttribute("class"));
    }

    [Fact]
    public void TwBubble_TailFalse_DrawsNoTip()
    {
        var classes = Surface(Render(p => p.Add(x => x.Tail, false))).GetAttribute("class")!;

        Assert.DoesNotContain(theme.Tip, classes);
        Assert.DoesNotContain(theme.StartTail, classes);
        Assert.DoesNotContain(theme.EndTail, classes);
    }

    [Fact]
    public void TwBubble_Align_OverridesTheMessagesAlignment()
    {
        var cut = TestContext.Render<TwMessage>(p => p
            .Add(x => x.Align, MessageAlign.End)
            .AddChildContent<TwBubble>(bubble => bubble.Add(x => x.Align, MessageAlign.Start)));

        Assert.Contains(theme.StartTail, cut.Find($"div[class*='{theme.Surface}']").GetAttribute("class"));
    }

    [Fact]
    public void TwBubble_Rounded_SetsTheCornerRadius()
    {
        var classes = Surface(Render(p => p.Add(x => x.Rounded, Rounded.Full))).GetAttribute("class")!;

        Assert.Contains(RoundedBuilder.GetRounded(Rounded.Full), classes);
    }

    [Fact]
    public void TwBubble_WithoutAName_IsNotAGroup()
    {
        Assert.False(Render().Find("div").HasAttribute("role"));
    }

    [Fact]
    public void TwBubble_WithAriaLabel_IsANamedGroup()
    {
        var root = Render(p => p.Add(x => x.AriaLabel, "Reply from Jane")).Find("div");

        Assert.Equal("group", root.GetAttribute("role"));
        Assert.Equal("Reply from Jane", root.GetAttribute("aria-label"));
    }

    [Fact]
    public void TwBubble_WithoutReactions_RendersNoReactionGroup()
    {
        Assert.Empty(Render(p => p.Add(x => x.Reactions, [])).FindAll("[aria-label='Reactions']"));
    }

    [Fact]
    public void TwBubble_Reactions_AreAGroupOfStaticChips_WhenNotClickable()
    {
        var cut = Render(p => p.Add(x => x.Reactions, Reactions()));

        var group = cut.Find("[aria-label='Reactions']");
        Assert.Equal("group", group.GetAttribute("role"));
        Assert.Empty(group.QuerySelectorAll("button"));
        Assert.Equal(2, cut.FindComponents<TwChip>().Count);
        Assert.All(cut.FindComponents<TwChip>(), chip => Assert.Null(chip.Find("span").GetAttribute("aria-pressed")));
    }

    [Fact]
    public void TwBubble_Reaction_IsAnnouncedByItsLabelAndCount_AndTheEmojiIsHidden()
    {
        var cut = Render(p => p.Add(x => x.Reactions, Reactions()));

        var srOnly = cut.FindAll($"span[class='{Theme.Display.ScreenReaderOnly}']").Select(span => span.TextContent).ToList();
        Assert.Equal(["Thumbs up: 3", "🎉: 1,200"], srOnly);
        Assert.Equal(["👍", "🎉"], cut.FindAll($"span[class='{theme.ReactionEmoji}']").Select(span => span.TextContent));
        Assert.All(cut.FindAll($"span[class='{theme.ReactionEmoji}']"), span => Assert.Equal("true", span.GetAttribute("aria-hidden")));
    }

    [Fact]
    public void TwBubble_ReactionCount_FollowsTheCulture()
    {
        var cut = Render(p => p.Add(x => x.Reactions, Reactions()));

        Assert.Equal("1,200", cut.FindAll($"span[class='{theme.ReactionCount}']")[1].TextContent);
    }

    [Fact]
    public void TwBubble_ReactedReaction_IsFilled_AndTheOthersAreOutlined()
    {
        var chips = Render(p => p.Add(x => x.Reactions, Reactions())).FindComponents<TwChip>();

        Assert.Equal(ButtonVariant.Filled, chips[0].Instance.Variant);
        Assert.Equal(ButtonVariant.Outlined, chips[1].Instance.Variant);
        Assert.All(chips, chip => Assert.Equal(ChipSize.Medium, chip.Instance.Size));
    }

    [Fact]
    public void TwBubble_Reactions_TakeTheBubblesColor()
    {
        var chips = Render(p => p.Add(x => x.Reactions, Reactions()).Add(x => x.Color, Color.Success)).FindComponents<TwChip>();

        Assert.All(chips, chip => Assert.Equal(Color.Success, chip.Instance.Color));
    }

    [Fact]
    public void TwBubble_ClickableReactions_AreToggleButtons_ThatReportWhetherTheyArePressed()
    {
        var cut = Render(p => p.Add(x => x.Reactions, Reactions()).Add(x => x.OnReactionClick, (BubbleReaction _) => { }));

        var buttons = cut.FindAll("button");
        Assert.Equal(2, buttons.Count);
        Assert.Equal("true", buttons[0].GetAttribute("aria-pressed"));
        Assert.Equal("false", buttons[1].GetAttribute("aria-pressed"));
        Assert.Equal("button", buttons[0].GetAttribute("type"));
    }

    [Fact]
    public async Task TwBubble_ClickingAReaction_InvokesOnReactionClick_WithThatReaction()
    {
        var reactions = Reactions();
        var clicked = new TaskCompletionSource<BubbleReaction>();
        var cut = Render(p => p.Add(x => x.Reactions, reactions).Add(x => x.OnReactionClick, (BubbleReaction reaction) => clicked.TrySetResult(reaction)));

        cut.FindAll("button")[1].Click();

        Assert.Same(reactions[1], await clicked.Task.WaitAsync(TimeSpan.FromSeconds(5), Xunit.TestContext.Current.CancellationToken));
    }

    [Fact]
    public void TwBubble_AppliesClassStyleAndAttributes()
    {
        var root = Render(p => p.Add(x => x.Class, "my-bubble").Add(x => x.Style, "color: red").AddUnmatched("data-test", "x")).Find("div");

        Assert.Contains("my-bubble", root.GetAttribute("class"));
        Assert.Equal("color: red", root.GetAttribute("style"));
        Assert.Equal("x", root.GetAttribute("data-test"));
    }
}
