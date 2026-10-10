using Bunit;
using ClassMergePage = TwBlazor.Docs.Pages.ClassMerge.ClassMerge;

namespace TwBlazor.Docs.Tests.Pages;

public class ClassMergePageTests : DocsTestBase
{
    private IRenderedComponent<ClassMergePage> Render() => TestContext.Render<ClassMergePage>();

    private static IEnumerable<string> CodeBlocks(IRenderedComponent<ClassMergePage> cut) =>
        cut.FindAll("pre code").Select(code => code.TextContent);

    [Theory]
    [InlineData("p-5 p-2 p-4", "p-4")]
    [InlineData("p-3 px-5", "p-3 px-5")]
    [InlineData("inline block", "block")]
    [InlineData("hover:p-2 hover:p-4", "hover:p-4")]
    [InlineData("p-3! p-4! p-5", "p-4! p-5")]
    [InlineData("text-sm leading-6 text-lg/7", "text-lg/7")]
    [InlineData("p-5 p-2 my-non-tailwind-class p-4", "my-non-tailwind-class p-4")]
    [InlineData("w-1/2 w-half", "w-1/2 w-half")]
    [InlineData("tw:px-4 tw:px-2", "tw:px-2")]
    [InlineData("tw:px-4 px-2", "tw:px-4 px-2")]
    [InlineData("bg-red-500 bg-[right_0.5rem_center]", "bg-red-500 bg-[right_0.5rem_center]")]
    [InlineData("!leading-4 !text-sm/6", "!text-sm/6")]
    [InlineData("px-5 p-3", "p-3")]
    [InlineData("rounded-lg rounded-t-none", "rounded-lg rounded-t-none")]
    [InlineData("w-4 h-6 size-8", "size-8")]
    [InlineData("basis-4 grow flex-1", "flex-1")]
    [InlineData("overflow-auto inline line-clamp-1", "line-clamp-1")]
    [InlineData("text-sm text-red-500 text-lg", "text-red-500 text-lg")]
    [InlineData("bg-red-500 bg-cover bg-center bg-blue-500", "bg-cover bg-center bg-blue-500")]
    [InlineData("group-hover:p-1 peer-hover:p-2 group-hover:p-3", "peer-hover:p-2 group-hover:p-3")]
    [InlineData("before:hover:p-1 hover:before:p-2", "before:hover:p-1 hover:before:p-2")]
    [InlineData("text-2xl text-(--my-color)", "text-2xl text-(--my-color)")]
    [InlineData("text-2xl text-(length:--my-size)", "text-(length:--my-size)")]
    [InlineData("hover:[&>*]:p-1 [&>*]:hover:p-2", "hover:[&>*]:p-1 [&>*]:hover:p-2")]
    [InlineData("!p-3 p-4!", "p-4!")]
    [InlineData("text-lg/7 leading-9", "text-lg/7 leading-9")]
    [InlineData("bg-red-500/50 bg-blue-500", "bg-blue-500")]
    [InlineData("btn btn-primary btn", "btn btn-primary btn")]
    [InlineData("ring-brand ring-2 ring-other", "ring-2 ring-other")]
    public void Examples_ShowTheInputAndItsMergedResult_InACodeBlock(string input, string result)
    {
        var cut = Render();

        var code = string.Join('\n', CodeBlocks(cut));

        Assert.Contains($"new ClassBuilder(\"{input}\").Build();", code);
        Assert.Contains($"// → \"{result}\"", code);
    }

    [Fact]
    public void Examples_PutALongResultOnTheLineBelowTheCall()
    {
        var cut = Render();

        var code = string.Join('\n', CodeBlocks(cut));

        Assert.Matches(@"\.Build\(\);\n// → ""\[--scroll-offset:56px\] lg:\[--scroll-offset:44px\]""", code);
    }

    [Theory]
    [InlineData("Last conflicting class wins")]
    [InlineData("Supports arbitrary values")]
    [InlineData("Preserves non-Tailwind classes")]
    [InlineData("Supports a Tailwind prefix")]
    [InlineData("Tells apart classes that share a prefix")]
    [InlineData("What it does not do")]
    [InlineData("Differences from tailwind-merge")]
    [InlineData("Composition")]
    [InlineData("Turn it off")]
    [InlineData("Add your own utilities")]
    [InlineData("Override other groups")]
    public void Page_GivesEachTopicItsOwnSection_SoTheOutlineListsIt(string title)
    {
        var cut = Render();

        var headings = cut.FindAll("h2").Select(h => h.TextContent).ToList();

        Assert.Contains(title, headings);
        Assert.Contains(cut.FindAll("a[data-outline-id]"), link => link.TextContent.Trim() == title);
    }

    [Fact]
    public void Configuration_HasItsOwnId_SoTheOutlineDoesNotLinkToTheSidebarGroup()
    {
        var cut = Render();

        var link = cut.FindAll("a[data-outline-id]").Single(a => a.TextContent.Trim() == "Configuration");
        var id = link.GetAttribute("data-outline-id");

        Assert.NotEqual("configuration", id);
        Assert.Single(cut.FindAll($"[id='{id}']"));
    }

    [Fact]
    public void Page_LinksToItsOwnSections_ThroughItsPath_SoTheBaseHrefDoesNotSendThemToTheHomepage()
    {
        var cut = Render();

        var links = cut.FindAll("a[href*='#class-merge']").Select(a => a.GetAttribute("href")!).ToList();

        Assert.NotEmpty(links);
        Assert.All(links, href =>
        {
            Assert.StartsWith("/class-merge#", href);
            Assert.Single(cut.FindAll($"[id='{href.Split('#')[1]}']"));
        });
    }

    [Fact]
    public void Page_UsesEachSectionIdOnlyOnce()
    {
        var cut = Render();

        var ids = cut.FindAll("[id]").Select(element => element.Id).ToList();

        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void Page_HasNoSubHeadings_InsideItsSections()
    {
        var cut = Render();

        Assert.Empty(cut.FindAll("h3"));
    }

    [Fact]
    public void Page_UsesCodeBlocksInsteadOfTablesAndInputs()
    {
        var cut = Render();

        Assert.Empty(cut.FindAll("table"));
        Assert.Empty(cut.FindAll("input"));
    }

    [Fact]
    public void Page_DocumentsTheOptions()
    {
        var cut = Render();

        var code = string.Join('\n', CodeBlocks(cut));

        Assert.Contains("ClassMerge.Enabled", code);
        Assert.Contains("TwClassGroup", code);
        Assert.Contains("Overrides", code);
        Assert.Contains("ClassMerge.Prefix", code);
        Assert.Contains("ClassMerge.CacheSize", code);
        Assert.Contains("ClassMerge.OrderSensitiveVariants", code);
        Assert.Contains("ModifierOverrides", code);
        Assert.Contains("Classes = ", code);
    }
}
