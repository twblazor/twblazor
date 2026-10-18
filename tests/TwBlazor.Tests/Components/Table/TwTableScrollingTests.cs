using Bunit;
using TwBlazor.Components;
using TwBlazor.Configuration.Components;

namespace TwBlazor.Tests.Components.Table;

public class TwTableScrollingTests : TwBlazorTestBase
{
    private TwTableTheme tableTheme => Theme.Components.Require<TwTableTheme>();

    [Fact]
    public void MaxHeight_LimitsTheScrollArea()
    {
        // Act
        var cut = TestContext.Render<TwTable>(parameters => parameters.Add(p => p.MaxHeight, " 24rem "));

        // Assert
        Assert.Equal("max-height:24rem", cut.Find("table").ParentElement!.GetAttribute("style"));
    }

    [Fact]
    public void MaxHeight_IsUnset_ByDefault()
    {
        // Act
        var cut = TestContext.Render<TwTable>();

        // Assert
        Assert.Null(cut.Find("table").ParentElement!.GetAttribute("style"));
    }

    [Fact]
    public void StickyHeader_KeepsTheHeaderInView()
    {
        // Act
        var cut = TestContext.Render<TwTable>(parameters => parameters
            .Add(p => p.StickyHeader, true)
            .Add(p => p.TableHeader, RenderFragmentBuilder(string.Empty)));

        // Assert
        Assert.Contains(tableTheme.StickyHeader, cut.Find("thead").GetAttribute("class"));
    }

    [Fact]
    public void StickyHeader_IsOff_ByDefault()
    {
        // Act
        var cut = TestContext.Render<TwTable>(parameters => parameters.Add(p => p.TableHeader, RenderFragmentBuilder(string.Empty)));

        // Assert
        Assert.DoesNotContain(tableTheme.StickyHeader, cut.Find("thead").GetAttribute("class"));
    }
}
