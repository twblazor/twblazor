using Bunit;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Message;

public class TwMessageTests : TwBlazorTestBase
{
    private TwMessageTheme theme => Theme.Components.Require<TwMessageTheme>();

    private static readonly DateTime _sentAt = new(2026, 10, 19, 14, 5, 0, DateTimeKind.Local);

    [Fact]
    public void TwMessage_RendersAnArticle_WithAGeneratedId()
    {
        var cut = TestContext.Render<TwMessage>();

        var article = cut.Find("article");
        Assert.StartsWith("message-", article.GetAttribute("id"));
        Assert.Contains(theme.Base, article.GetAttribute("class"));
    }

    [Fact]
    public void TwMessage_IsNamedByItsAuthorAndTime()
    {
        var cut = TestContext.Render<TwMessage>(p => p
            .Add(x => x.Id, "m1")
            .Add(x => x.Author, "Jane Doe")
            .Add(x => x.Timestamp, _sentAt));

        Assert.Equal("m1-author m1-time", cut.Find("article").GetAttribute("aria-labelledby"));
        Assert.Equal("Jane Doe", cut.Find("#m1-author").TextContent);
        Assert.Equal("14:05", cut.Find("#m1-time").TextContent);
    }

    [Fact]
    public void TwMessage_WithOnlyAnAuthor_IsNamedByTheAuthor()
    {
        var cut = TestContext.Render<TwMessage>(p => p.Add(x => x.Id, "m1").Add(x => x.Author, "Jane Doe"));

        Assert.Equal("m1-author", cut.Find("article").GetAttribute("aria-labelledby"));
        Assert.Empty(cut.FindAll("time"));
    }

    [Fact]
    public void TwMessage_AriaLabel_ReplacesTheGeneratedName()
    {
        var cut = TestContext.Render<TwMessage>(p => p
            .Add(x => x.Author, "Jane Doe")
            .Add(x => x.AriaLabel, "Message from Jane"));

        var article = cut.Find("article");
        Assert.Equal("Message from Jane", article.GetAttribute("aria-label"));
        Assert.False(article.HasAttribute("aria-labelledby"));
    }

    [Fact]
    public void TwMessage_AriaLabelledBy_ReplacesTheGeneratedName()
    {
        var cut = TestContext.Render<TwMessage>(p => p
            .Add(x => x.Author, "Jane Doe")
            .Add(x => x.AriaLabelledBy, "elsewhere"));

        Assert.Equal("elsewhere", cut.Find("article").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void TwMessage_WithoutAuthorOrTime_HasNoHeaderAndNoName()
    {
        var cut = TestContext.Render<TwMessage>(p => p.AddChildContent("Hello"));

        Assert.False(cut.Find("article").HasAttribute("aria-labelledby"));
        Assert.Empty(cut.FindAll($"div[class='{theme.Header}']"));
    }

    [Fact]
    public void TwMessage_Timestamp_HasAMachineReadableDateTime()
    {
        var cut = TestContext.Render<TwMessage>(p => p.Add(x => x.Timestamp, _sentAt));

        Assert.Equal("2026-10-19T14:05:00", cut.Find("time").GetAttribute("datetime"));
    }

    [Fact]
    public void TwMessage_TimestampFormat_ChangesTheShownTime()
    {
        var cut = TestContext.Render<TwMessage>(p => p.Add(x => x.Timestamp, _sentAt).Add(x => x.TimestampFormat, "d MMM HH:mm"));

        Assert.Equal("19 Oct 14:05", cut.Find("time").TextContent);
    }

    [Fact]
    public void FormatMachineTimestamp_EndsInZ_ForUtc()
    {
        Assert.Equal("2026-10-19T14:05:00Z", TwMessage.FormatMachineTimestamp(new DateTime(2026, 10, 19, 14, 5, 0, DateTimeKind.Utc)));
        Assert.Equal("2026-10-19T14:05:00", TwMessage.FormatMachineTimestamp(new DateTime(2026, 10, 19, 14, 5, 0, DateTimeKind.Unspecified)));
    }

    [Fact]
    public void TwMessage_ShowsADecorativeAvatar_WithTheAuthorsInitials()
    {
        var cut = TestContext.Render<TwMessage>(p => p.Add(x => x.Author, "Jane Doe"));

        var avatar = cut.FindComponent<TwAvatar>();
        Assert.Equal("JD", avatar.Find("span").TextContent.Trim());
        Assert.Equal("true", avatar.Find("span").GetAttribute("aria-hidden"));
        Assert.Equal(AvatarSize.Small, avatar.Instance.Size);
    }

    [Fact]
    public void TwMessage_PassesItsAvatarSettingsToTheAvatar()
    {
        var cut = TestContext.Render<TwMessage>(p => p
            .Add(x => x.Author, "Assistant")
            .Add(x => x.AvatarInitials, "AI")
            .Add(x => x.AvatarSrc, "/bot.png")
            .Add(x => x.AvatarIcon, TwBlazor.Enums.Icon.Robot)
            .Add(x => x.AvatarColor, Color.Info));

        var avatar = cut.FindComponent<TwAvatar>().Instance;
        Assert.Equal("AI", avatar.Initials);
        Assert.Equal("/bot.png", avatar.Src);
        Assert.Equal(TwBlazor.Enums.Icon.Robot, avatar.Icon);
        Assert.Equal(Color.Info, avatar.Color);
    }

    [Fact]
    public void TwMessage_AvatarIcon_ReplacesTheInitialsFromTheAuthor()
    {
        var cut = TestContext.Render<TwMessage>(p => p.Add(x => x.Author, "Assistant").Add(x => x.AvatarIcon, TwBlazor.Enums.Icon.Robot));

        var avatar = cut.FindComponent<TwAvatar>();
        Assert.Null(avatar.Instance.Initials);
        Assert.Contains("bi-robot", avatar.Markup);
        Assert.DoesNotContain(">A<", avatar.Markup);
    }

    [Fact]
    public void TwMessage_WithoutAuthorOrIcon_ShowsThePersonIcon()
    {
        Assert.Contains("bi-person-fill", TestContext.Render<TwMessage>().FindComponent<TwAvatar>().Markup);
    }

    [Fact]
    public void TwMessage_AvatarContent_ReplacesTheBuiltInAvatar()
    {
        var cut = TestContext.Render<TwMessage>(p => p
            .Add(x => x.Author, "Jane Doe")
            .Add(x => x.AvatarContent, builder => builder.AddMarkupContent(0, "<span class=\"custom\">J</span>")));

        Assert.Empty(cut.FindComponents<TwAvatar>());
        Assert.NotNull(cut.Find("span.custom"));
    }

    [Fact]
    public void TwMessage_ShowAvatarFalse_RendersNoAvatarColumn()
    {
        var cut = TestContext.Render<TwMessage>(p => p.Add(x => x.Author, "Jane Doe").Add(x => x.ShowAvatar, false));

        Assert.Empty(cut.FindComponents<TwAvatar>());
        Assert.Empty(cut.FindAll($"span[class='{theme.AvatarSpacer}']"));
    }

    [Fact]
    public void TwMessage_Continued_HidesTheAvatarAndHeaderFromView_ButKeepsTheName()
    {
        var cut = TestContext.Render<TwMessage>(p => p
            .Add(x => x.Id, "m2")
            .Add(x => x.Author, "Jane Doe")
            .Add(x => x.Continued, true));

        Assert.Empty(cut.FindComponents<TwAvatar>());
        Assert.Equal("true", cut.Find($"span[class='{theme.AvatarSpacer}']").GetAttribute("aria-hidden"));
        Assert.Equal(Theme.Display.ScreenReaderOnly, cut.Find("#m2-author").ParentElement!.GetAttribute("class"));
        Assert.Equal("m2-author", cut.Find("article").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void TwMessage_AlignEnd_ReversesTheRowAndAlignsContentToTheEnd()
    {
        var cut = TestContext.Render<TwMessage>(p => p.Add(x => x.Align, MessageAlign.End));

        Assert.Contains(Theme.Flexbox.RowReverse, cut.Find("article").GetAttribute("class"));
        Assert.Contains(Theme.Flexbox.Align.End, cut.Find($"div[class*='{theme.Content}']").GetAttribute("class"));
    }

    [Fact]
    public void TwMessage_AlignStart_DoesNotReverseTheRow()
    {
        var cut = TestContext.Render<TwMessage>();

        Assert.DoesNotContain(Theme.Flexbox.RowReverse, cut.Find("article").GetAttribute("class"));
    }

    [Fact]
    public void TwMessage_RendersItsContentAndFooter()
    {
        var cut = TestContext.Render<TwMessage>(p => p
            .AddChildContent("<p class=\"body\">Hello</p>")
            .Add(x => x.Footer, builder => builder.AddContent(0, "Delivered")));

        Assert.Equal("Hello", cut.Find("p.body").TextContent);
        Assert.Equal("Delivered", cut.Find($"div[class='{theme.Footer}']").TextContent);
    }

    [Fact]
    public void TwMessage_CascadesItsAlignmentToABubble()
    {
        var cut = TestContext.Render<TwMessage>(p => p
            .Add(x => x.Align, MessageAlign.End)
            .AddChildContent<TwBubble>(bubble => bubble.AddChildContent("Hi")));

        var bubbleRoot = cut.FindComponent<TwBubble>().Find("div");
        Assert.Contains(Theme.Flexbox.Align.End, bubbleRoot.GetAttribute("class"));
    }

    [Fact]
    public void TwMessage_AppliesClassStyleAndAttributes()
    {
        var cut = TestContext.Render<TwMessage>(p => p
            .Add(x => x.Class, "my-message")
            .Add(x => x.Style, "color: red")
            .AddUnmatched("data-test", "x"));

        var article = cut.Find("article");
        Assert.Contains("my-message", article.GetAttribute("class"));
        Assert.Equal("color: red", article.GetAttribute("style"));
        Assert.Equal("x", article.GetAttribute("data-test"));
    }
}
