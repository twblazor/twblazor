using Bunit;
using System.Globalization;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Attachment;

public class TwAttachmentTests : TwBlazorTestBase
{
    private TwAttachmentTheme theme => Theme.Components.Require<TwAttachmentTheme>();

    private IRenderedComponent<TwAttachment> Render(Action<ComponentParameterCollectionBuilder<TwAttachment>>? configure = null) =>
        TestContext.Render<TwAttachment>(p =>
        {
            p.Add(x => x.Id, "file");
            p.Add(x => x.FileName, "report.pdf");
            configure?.Invoke(p);
        });

    private static string Status(IRenderedComponent<TwAttachment> cut) => cut.Find("[role='status']").TextContent;

    [Fact]
    public void TwAttachment_IsAGroupNamedAfterTheFile()
    {
        var root = Render().Find("#file");

        Assert.Equal("group", root.GetAttribute("role"));
        Assert.Equal("report.pdf", root.GetAttribute("aria-label"));
        Assert.Contains(theme.Base, root.GetAttribute("class"));
    }

    [Fact]
    public void TwAttachment_AriaLabel_ReplacesTheFileName()
    {
        Assert.Equal("Quarterly report", Render(p => p.Add(x => x.AriaLabel, "Quarterly report")).Find("#file").GetAttribute("aria-label"));
    }

    [Fact]
    public void TwAttachment_AriaLabelledBy_ReplacesTheAriaLabel()
    {
        var root = Render(p => p.Add(x => x.AriaLabelledBy, "heading")).Find("#file");

        Assert.Equal("heading", root.GetAttribute("aria-labelledby"));
        Assert.False(root.HasAttribute("aria-label"));
    }

    [Fact]
    public void TwAttachment_ShowsTheFileName_WithItsFullNameAsATitle()
    {
        var name = Render().Find($"span[class='{theme.Name}']");

        Assert.Equal("report.pdf", name.TextContent);
        Assert.Equal("report.pdf", name.GetAttribute("title"));
    }

    [Fact]
    public void TwAttachment_WhenReady_ShowsItsSizeAndType()
    {
        Assert.Equal("1.5 MB · PDF", Render(p => p.Add(x => x.FileSize, 1_572_864)).Find($"span[class='{theme.Meta}']").TextContent);
    }

    [Fact]
    public void TwAttachment_WithoutSizeOrExtension_ShowsNoMeta()
    {
        var cut = TestContext.Render<TwAttachment>(p => p.Add(x => x.FileName, "README"));

        Assert.Empty(cut.FindAll($"span[class='{theme.Meta}']"));
    }

    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(512, "512 B")]
    [InlineData(1024, "1 KB")]
    [InlineData(1536, "1.5 KB")]
    [InlineData(1_048_576, "1 MB")]
    [InlineData(5_368_709_120, "5 GB")]
    [InlineData(-10, "0 B")]
    public void FormatFileSize_UsesTheLargestFittingUnit(long bytes, string expected)
    {
        Assert.Equal(expected, TwAttachment.FormatFileSize(bytes, TestCultures.British));
    }

    [Fact]
    public void FormatFileSize_FollowsTheCulturesDecimalSeparator()
    {
        Assert.Equal("1,5 KB", TwAttachment.FormatFileSize(1536, CultureInfo.GetCultureInfo("de-DE")));
    }

    [Theory]
    [InlineData("photo.bin", "image/png", TwBlazor.Enums.Icon.File_Earmark_Image)]
    [InlineData("song.bin", "audio/mpeg", TwBlazor.Enums.Icon.File_Earmark_Music)]
    [InlineData("clip.bin", "video/mp4", TwBlazor.Enums.Icon.File_Earmark_Play)]
    [InlineData("doc.bin", "application/pdf", TwBlazor.Enums.Icon.File_Earmark_Pdf)]
    [InlineData("photo.JPG", null, TwBlazor.Enums.Icon.File_Earmark_Image)]
    [InlineData("notes.docx", null, TwBlazor.Enums.Icon.File_Earmark_Word)]
    [InlineData("data.csv", null, TwBlazor.Enums.Icon.File_Earmark_Spreadsheet)]
    [InlineData("deck.pptx", null, TwBlazor.Enums.Icon.File_Earmark_Slides)]
    [InlineData("bundle.zip", null, TwBlazor.Enums.Icon.File_Earmark_Zip)]
    [InlineData("readme.md", null, TwBlazor.Enums.Icon.File_Earmark_Text)]
    [InlineData("Program.cs", null, TwBlazor.Enums.Icon.File_Earmark_Code)]
    [InlineData("unknown", "application/octet-stream", TwBlazor.Enums.Icon.File_Earmark)]
    [InlineData(null, null, TwBlazor.Enums.Icon.File_Earmark)]
    public void GetFileIcon_PicksTheIconForTheKindOfFile(string? fileName, string? contentType, TwBlazor.Enums.Icon expected)
    {
        Assert.Equal(expected, TwAttachment.GetFileIcon(fileName, contentType));
    }

    [Fact]
    public void TwAttachment_WithoutPreview_ShowsADecorativeTypeIcon()
    {
        var cut = Render();

        var box = cut.Find($"span[class='{theme.IconBox}']");
        Assert.Equal("true", box.GetAttribute("aria-hidden"));
        Assert.Contains("bi-file-earmark-pdf", box.InnerHtml);
    }

    [Fact]
    public void TwAttachment_Icon_OverridesTheTypeIcon()
    {
        Assert.Contains("bi-robot", Render(p => p.Add(x => x.Icon, TwBlazor.Enums.Icon.Robot)).Find($"span[class='{theme.IconBox}']").InnerHtml);
    }

    [Fact]
    public void TwAttachment_PreviewSrc_ShowsADecorativeImage()
    {
        var image = Render(p => p.Add(x => x.PreviewSrc, "/thumb.png")).Find("img");

        Assert.Equal("/thumb.png", image.GetAttribute("src"));
        Assert.Equal(string.Empty, image.GetAttribute("alt"));
        Assert.Equal(theme.Preview, image.GetAttribute("class"));
    }

    [Fact]
    public void TwAttachment_FallsBackToTheIcon_WhenThePreviewFailsToLoad()
    {
        var cut = Render(p => p.Add(x => x.PreviewSrc, "/missing.png"));

        cut.Find("img").TriggerEvent("onerror", EventArgs.Empty);

        Assert.Empty(cut.FindAll("img"));
        Assert.NotNull(cut.Find($"span[class='{theme.IconBox}']"));
    }

    [Fact]
    public void TwAttachment_Href_MakesTheNameALink_WhenReady()
    {
        var link = Render(p => p.Add(x => x.Href, "/files/report.pdf").Add(x => x.Target, "_blank")).Find("a");

        Assert.Equal("/files/report.pdf", link.GetAttribute("href"));
        Assert.Equal("noopener", link.GetAttribute("rel"));
        Assert.Contains(theme.NameLink, link.GetAttribute("class"));
    }

    [Fact]
    public void TwAttachment_Href_IsNotALink_WhileUploading()
    {
        Assert.Empty(Render(p => p.Add(x => x.Href, "/files/report.pdf").Add(x => x.Status, AttachmentStatus.Uploading)).FindAll("a"));
    }

    [Fact]
    public void TwAttachment_Uploading_ShowsANamedProgressBar()
    {
        var cut = Render(p => p.Add(x => x.Status, AttachmentStatus.Uploading).Add(x => x.Progress, 40d));

        var progress = cut.Find("progress");
        Assert.Equal("40", progress.GetAttribute("value"));
        Assert.Equal("100", progress.GetAttribute("max"));
        Assert.Equal("Uploading report.pdf", progress.GetAttribute("aria-label"));
        Assert.Empty(cut.FindAll($"span[class='{theme.Meta}']"));
    }

    [Fact]
    public void TwAttachment_UploadingWithoutProgress_IsIndeterminate()
    {
        var progress = Render(p => p.Add(x => x.Status, AttachmentStatus.Uploading)).Find("progress");

        Assert.False(progress.HasAttribute("value"));
        Assert.Equal("true", progress.GetAttribute("aria-busy"));
    }

    [Fact]
    public void TwAttachment_Failed_ShowsTheDefaultError_AndTheFailedBorder()
    {
        var cut = Render(p => p.Add(x => x.Status, AttachmentStatus.Failed));

        Assert.Equal("report.pdf failed to upload", cut.Find("#file-error").TextContent);
        Assert.Contains(theme.Failed, cut.Find("#file").GetAttribute("class"));
    }

    [Fact]
    public void TwAttachment_ErrorText_ReplacesTheDefaultError()
    {
        Assert.Equal("File too large", Render(p => p.Add(x => x.Status, AttachmentStatus.Failed).Add(x => x.ErrorText, "File too large")).Find("#file-error").TextContent);
    }

    [Fact]
    public void TwAttachment_WithoutCallbacks_HasNoButtons()
    {
        Assert.Empty(Render(p => p.Add(x => x.Status, AttachmentStatus.Failed)).FindAll("button"));
    }

    [Fact]
    public void TwAttachment_RemoveButton_NamesTheFile()
    {
        var button = Render(p => p.Add(x => x.OnRemove, () => { })).Find("button");

        Assert.Equal("Remove report.pdf", button.GetAttribute("aria-label"));
    }

    [Fact]
    public async Task TwAttachment_ClickingRemove_InvokesOnRemove()
    {
        var removed = new TaskCompletionSource();
        var cut = Render(p => p.Add(x => x.OnRemove, () => removed.TrySetResult()));

        cut.Find("button").Click();

        await removed.Task.WaitAsync(TimeSpan.FromSeconds(5), Xunit.TestContext.Current.CancellationToken);
        Assert.True(removed.Task.IsCompletedSuccessfully);
    }

    [Fact]
    public void TwAttachment_RetryButton_OnlyShowsWhenFailed()
    {
        Assert.Empty(Render(p => p.Add(x => x.OnRetry, () => { })).FindAll("button"));
        Assert.Single(Render(p => p.Add(x => x.OnRetry, () => { }).Add(x => x.Status, AttachmentStatus.Failed)).FindAll("button"));
    }

    [Fact]
    public async Task TwAttachment_RetryButton_NamesTheFile_IsDescribedByTheError_AndInvokesOnRetry()
    {
        var retried = new TaskCompletionSource();
        var cut = Render(p => p.Add(x => x.Status, AttachmentStatus.Failed).Add(x => x.OnRetry, () => retried.TrySetResult()));

        var button = cut.Find("button");
        Assert.Equal("Retry uploading report.pdf", button.GetAttribute("aria-label"));
        Assert.Equal("file-error", button.GetAttribute("aria-describedby"));

        button.Click();

        await retried.Task.WaitAsync(TimeSpan.FromSeconds(5), Xunit.TestContext.Current.CancellationToken);
        Assert.True(retried.Task.IsCompletedSuccessfully);
    }

    [Fact]
    public void TwAttachment_Disabled_DisablesItsButtons()
    {
        var cut = Render(p => p
            .Add(x => x.Status, AttachmentStatus.Failed)
            .Add(x => x.OnRetry, () => { })
            .Add(x => x.OnRemove, () => { })
            .Add(x => x.Disabled, true));

        Assert.All(cut.FindAll("button"), button => Assert.True(button.HasAttribute("disabled")));
    }

    [Fact]
    public void TwAttachment_Actions_RenderBeforeTheBuiltInButtons()
    {
        var cut = Render(p => p
            .Add(x => x.Actions, builder => builder.AddMarkupContent(0, "<button type=\"button\" class=\"open\">Open</button>"))
            .Add(x => x.OnRemove, () => { }));

        var buttons = cut.Find($"div[class='{theme.Actions}']").QuerySelectorAll("button");
        Assert.Equal("open", buttons[0].GetAttribute("class"));
        Assert.Equal("Remove report.pdf", buttons[1].GetAttribute("aria-label"));
    }

    [Fact]
    public void TwAttachment_ReadyFromTheStart_AnnouncesNothing()
    {
        Assert.Equal(string.Empty, Status(Render()));
    }

    [Fact]
    public void TwAttachment_AnnouncesEachStepOfTheUpload()
    {
        var cut = Render(p => p.Add(x => x.Status, AttachmentStatus.Uploading));
        Assert.Equal("Uploading report.pdf", Status(cut));

        cut.Render(p => p.Add(x => x.Progress, 50d));
        Assert.Equal("Uploading report.pdf", Status(cut));

        cut.Render(p => p.Add(x => x.Status, AttachmentStatus.Ready));
        Assert.Equal("report.pdf uploaded", Status(cut));

        cut.Render(p => p.Add(x => x.Status, AttachmentStatus.Failed));
        Assert.Equal("report.pdf failed to upload", Status(cut));
    }

    [Fact]
    public void TwAttachment_AppliesClassStyleAndAttributes()
    {
        var root = Render(p => p.Add(x => x.Class, "my-file").Add(x => x.Style, "color: red").AddUnmatched("data-test", "x")).Find("#file");

        Assert.Contains("my-file", root.GetAttribute("class"));
        Assert.Equal("color: red", root.GetAttribute("style"));
        Assert.Equal("x", root.GetAttribute("data-test"));
    }
}
