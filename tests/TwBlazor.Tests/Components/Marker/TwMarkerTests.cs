using Bunit;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;
using TwBlazor.Enums;

namespace TwBlazor.Tests.Components.Marker;

public class TwMarkerTests : TwBlazorTestBase
{
    private TwMarkerTheme theme => Theme.Components.Require<TwMarkerTheme>();

    private IRenderedComponent<TwMarker> Render(Action<ComponentParameterCollectionBuilder<TwMarker>>? configure = null) =>
        TestContext.Render<TwMarker>(p =>
        {
            p.Add(x => x.Id, "marker");
            configure?.Invoke(p);
        });

    private string? ScreenReaderText(IRenderedComponent<TwMarker> cut) =>
        cut.Find("#marker").Children.SingleOrDefault(child => child.GetAttribute("class") == Theme.Display.ScreenReaderOnly)?.TextContent;

    [Fact]
    public void TwMarker_GeneratesAnId_WhenNotProvided()
    {
        Assert.StartsWith("marker-", TestContext.Render<TwMarker>().Find("div").GetAttribute("id"));
    }

    [Fact]
    public void TwMarker_ByDefault_IsAStatusLine_WithItsLabel_AndNoStatus()
    {
        var cut = Render(p => p.Add(x => x.Label, "Read 3 files"));

        var root = cut.Find("#marker");
        Assert.Contains(theme.Status, root.GetAttribute("class"));
        Assert.False(root.HasAttribute("role"));
        Assert.Equal("Read 3 files", cut.Find($"span[class='{theme.Label}']").TextContent);
        Assert.Null(ScreenReaderText(cut));
        Assert.Empty(cut.FindAll("i"));
    }

    [Theory]
    [InlineData(MarkerStatus.Pending, "Pending", "bi-circle")]
    [InlineData(MarkerStatus.Succeeded, "Done", "bi-check-circle-fill")]
    [InlineData(MarkerStatus.Failed, "Failed", "bi-x-circle-fill")]
    public void TwMarker_Status_ShowsAHiddenIcon_AndWritesTheStatusOut(MarkerStatus status, string text, string iconClass)
    {
        var cut = Render(p => p.Add(x => x.Label, "Searched the web").Add(x => x.Status, status));

        var icon = cut.Find($"span[class*='{theme.Icon}']");
        Assert.Equal("true", icon.GetAttribute("aria-hidden"));
        Assert.Contains(iconClass, icon.InnerHtml);
        Assert.Equal(text, ScreenReaderText(cut));
    }

    [Fact]
    public void TwMarker_StatusText_ComesAfterTheLabel()
    {
        var cut = Render(p => p.Add(x => x.Label, "Searched the web").Add(x => x.Status, MarkerStatus.Succeeded));

        var spans = cut.Find("#marker").Children.Select(child => child.TextContent).ToList();
        Assert.Equal(["Searched the web", "Done"], spans.Skip(1));
    }

    [Theory]
    [InlineData(MarkerStatus.Pending)]
    [InlineData(MarkerStatus.Succeeded)]
    [InlineData(MarkerStatus.Failed)]
    public void TwMarker_Status_ColorsTheIcon(MarkerStatus status)
    {
        var expected = status switch
        {
            MarkerStatus.Pending => theme.Pending,
            MarkerStatus.Succeeded => theme.Succeeded,
            _ => theme.Failed
        };

        Assert.Contains(expected, Render(p => p.Add(x => x.Status, status)).Find($"span[class*='{theme.Icon}']").GetAttribute("class"));
    }

    [Fact]
    public void TwMarker_Running_ShowsAHiddenSpinner_AndWritesTheStatusOut()
    {
        var cut = Render(p => p.Add(x => x.Label, "Searching").Add(x => x.Status, MarkerStatus.Running));

        var spinner = cut.FindComponent<TwSpinner>();
        Assert.Equal(SpinnerSize.Small, spinner.Instance.Size);
        Assert.Equal("true", spinner.Find("output").GetAttribute("aria-hidden"));
        Assert.Equal("In progress", ScreenReaderText(cut));
    }

    [Fact]
    public void TwMarker_Icon_OverridesTheStatusIcon()
    {
        var cut = Render(p => p.Add(x => x.Icon, TwBlazor.Enums.Icon.Tools).Add(x => x.Status, MarkerStatus.Succeeded));

        Assert.Contains("bi-tools", cut.Find($"span[class*='{theme.Icon}']").InnerHtml);
        Assert.Equal("Done", ScreenReaderText(cut));
    }

    [Fact]
    public void TwMarker_Color_ReplacesTheStatusColor()
    {
        var classes = Render(p => p.Add(x => x.Icon, TwBlazor.Enums.Icon.Tools).Add(x => x.Color, Color.Info).Add(x => x.Status, MarkerStatus.Failed))
            .Find($"span[class*='{theme.Icon}']").GetAttribute("class")!;

        Assert.Contains(IconColorBuilder.GetIconColor(Color.Info), classes);
        Assert.DoesNotContain(theme.Failed, classes);
    }

    [Fact]
    public void TwMarker_Detail_ShowsAfterTheLabel()
    {
        Assert.Equal("2.1s", Render(p => p.Add(x => x.Label, "Ran tests").Add(x => x.Detail, "2.1s")).Find($"span[class='{theme.Detail}']").TextContent);
    }

    [Fact]
    public void TwMarker_Live_MakesTheStatusLineAStatusRegion()
    {
        Assert.Equal("status", Render(p => p.Add(x => x.Live, true)).Find("#marker").GetAttribute("role"));
    }

    [Fact]
    public void TwMarker_ChildContent_ReplacesTheLabel()
    {
        var cut = Render(p => p.Add(x => x.Label, "ignored").AddChildContent("<strong>Custom</strong>"));

        Assert.Equal("Custom", cut.Find("strong").TextContent);
        Assert.DoesNotContain("ignored", cut.Markup);
    }

    [Fact]
    public void TwMarker_Separator_WithoutLabel_IsAnUnnamedSeparatorWithOneLine()
    {
        var cut = Render(p => p.Add(x => x.Variant, MarkerVariant.Separator));

        var root = cut.Find("#marker");
        Assert.Equal("separator", root.GetAttribute("role"));
        Assert.False(root.HasAttribute("aria-label"));
        Assert.Contains(theme.Separator, root.GetAttribute("class"));
        Assert.Single(cut.FindAll($"span[class='{theme.Line}']"));
    }

    [Fact]
    public void TwMarker_Separator_WithLabel_IsNamedByIt_AndShowsItBetweenTwoLines()
    {
        var cut = Render(p => p.Add(x => x.Variant, MarkerVariant.Separator).Add(x => x.Label, "Today"));

        Assert.Equal("Today", cut.Find("#marker").GetAttribute("aria-label"));
        Assert.Equal("Today", cut.Find($"span[class='{theme.SeparatorLabel}']").TextContent);
        Assert.Equal(2, cut.FindAll($"span[class='{theme.Line}']").Count);
    }

    [Fact]
    public void TwMarker_Separator_AriaLabel_NamesCustomContent()
    {
        var cut = Render(p => p.Add(x => x.Variant, MarkerVariant.Separator).Add(x => x.AriaLabel, "New messages").AddChildContent("<b>New</b>"));

        Assert.Equal("New messages", cut.Find("#marker").GetAttribute("aria-label"));
        Assert.Equal("New", cut.Find("b").TextContent);
    }

    [Fact]
    public void TwMarker_Separator_AriaLabelledBy_ReplacesTheLabel()
    {
        var root = Render(p => p.Add(x => x.Variant, MarkerVariant.Separator).Add(x => x.Label, "Today").Add(x => x.AriaLabelledBy, "day")).Find("#marker");

        Assert.Equal("day", root.GetAttribute("aria-labelledby"));
        Assert.False(root.HasAttribute("aria-label"));
    }

    [Theory]
    [InlineData(MarkerVariant.Status)]
    [InlineData(MarkerVariant.Separator)]
    public void TwMarker_AppliesClassStyleAndAttributes(MarkerVariant variant)
    {
        var root = Render(p => p.Add(x => x.Variant, variant).Add(x => x.Class, "my-marker").Add(x => x.Style, "color: red").AddUnmatched("data-test", "x")).Find("#marker");

        Assert.Contains("my-marker", root.GetAttribute("class"));
        Assert.Equal("color: red", root.GetAttribute("style"));
        Assert.Equal("x", root.GetAttribute("data-test"));
    }
}
