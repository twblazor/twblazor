using TwBlazor.Configuration;
using TwBlazor.Utilities.ClassMerge;

namespace TwBlazor.Tests.Utilities.ClassMerge;

public class TwClassMergerTests
{
    private static readonly TwClassMerger _merger = new(new TwClassMergeOptions());

    [Theory]
    [InlineData("px-4 px-2", "px-2")]
    [InlineData("p-4 p-2", "p-2")]
    [InlineData("m-1 m-3", "m-3")]
    [InlineData("w-4 w-full", "w-full")]
    [InlineData("w-1/2 w-1/3", "w-1/3")]
    [InlineData("h-8 h-10", "h-10")]
    [InlineData("flex block", "block")]
    [InlineData("relative absolute", "absolute")]
    [InlineData("z-10 z-50", "z-50")]
    [InlineData("opacity-50 opacity-100", "opacity-100")]
    [InlineData("gap-2 gap-4", "gap-4")]
    [InlineData("cursor-pointer cursor-not-allowed", "cursor-not-allowed")]
    [InlineData("duration-200 duration-300", "duration-300")]
    public void Merge_KeepsTheLastClass_WhenTwoUtilitiesSetTheSameProperty(string input, string expected)
    {
        Assert.Equal(expected, _merger.Merge(input));
    }

    [Theory]
    [InlineData("px-4 py-2", "px-4 py-2")]
    [InlineData("pr-4 pl-2", "pr-4 pl-2")]
    [InlineData("text-sm text-red-500", "text-sm text-red-500")]
    [InlineData("bg-red-500 shadow-sm rounded", "bg-red-500 shadow-sm rounded")]
    [InlineData("flex items-center justify-between", "flex items-center justify-between")]
    public void Merge_KeepsEveryClass_WhenTheyDoNotConflict(string input, string expected)
    {
        Assert.Equal(expected, _merger.Merge(input));
    }

    [Theory]
    [InlineData("px-4 pr-2", "px-4 pr-2")]
    [InlineData("pr-2 px-4", "px-4")]
    [InlineData("p-2 px-4", "p-2 px-4")]
    [InlineData("px-4 p-2", "p-2")]
    [InlineData("pt-1 pb-1 py-4", "py-4")]
    [InlineData("mt-1 my-4", "my-4")]
    [InlineData("top-0 inset-0", "inset-0")]
    [InlineData("inset-0 top-2", "inset-0 top-2")]
    [InlineData("overflow-x-auto overflow-hidden", "overflow-hidden")]
    [InlineData("w-4 h-4 size-8", "size-8")]
    [InlineData("rounded-tl rounded-lg", "rounded-lg")]
    [InlineData("rounded-lg rounded-t-none", "rounded-lg rounded-t-none")]
    [InlineData("rounded-tl-lg rounded-t-none", "rounded-t-none")]
    public void Merge_AllowsALaterShorthandToOverrideItsParts_ButNotTheOtherWayRound(string input, string expected)
    {
        Assert.Equal(expected, _merger.Merge(input));
    }

    [Theory]
    [InlineData("text-sm text-lg", "text-lg")]
    [InlineData("text-lg text-sm", "text-sm")]
    [InlineData("text-red-500 text-blue-600", "text-blue-600")]
    [InlineData("text-red-500 text-sm", "text-red-500 text-sm")]
    [InlineData("text-left text-center", "text-center")]
    [InlineData("text-sm text-[oklch(50%_0.1_200)]", "text-sm text-[oklch(50%_0.1_200)]")]
    [InlineData("text-[#fff] text-[oklch(50%_0.1_200)]", "text-[oklch(50%_0.1_200)]")]
    [InlineData("text-[14px] text-sm", "text-sm")]
    [InlineData("text-sm leading-6", "text-sm leading-6")]
    [InlineData("leading-6 text-sm", "text-sm")]
    [InlineData("text-sm/6 leading-4", "text-sm/6 leading-4")]
    [InlineData("leading-4 text-sm/6", "text-sm/6")]
    [InlineData("font-bold font-normal", "font-normal")]
    [InlineData("font-sans font-mono", "font-mono")]
    [InlineData("font-bold font-mono", "font-bold font-mono")]
    [InlineData("text-ellipsis text-clip", "text-clip")]
    public void Merge_TellsTextSizeColorAndAlignmentApart(string input, string expected)
    {
        Assert.Equal(expected, _merger.Merge(input));
    }

    [Theory]
    [InlineData("bg-red-500 bg-blue-500", "bg-blue-500")]
    [InlineData("bg-red-500 bg-[url('x.png')]", "bg-red-500 bg-[url('x.png')]")]
    [InlineData("bg-red-500 bg-[length:1.5em_1.5em]", "bg-red-500 bg-[length:1.5em_1.5em]")]
    [InlineData("bg-red-500 bg-[right_0.5rem_center]", "bg-red-500 bg-[right_0.5rem_center]")]
    [InlineData("bg-red-500 bg-no-repeat", "bg-red-500 bg-no-repeat")]
    [InlineData("bg-red-500 bg-cover", "bg-red-500 bg-cover")]
    [InlineData("bg-red-500 bg-center", "bg-red-500 bg-center")]
    [InlineData("bg-center bg-top", "bg-top")]
    [InlineData("bg-red-500 bg-[oklch(97%_0_0)]", "bg-[oklch(97%_0_0)]")]
    [InlineData("bg-gradient-to-r bg-none", "bg-none")]
    [InlineData("from-red-500 from-blue-500", "from-blue-500")]
    [InlineData("from-red-500 from-10%", "from-red-500 from-10%")]
    [InlineData("from-red-500 to-blue-500", "from-red-500 to-blue-500")]
    public void Merge_TellsBackgroundColorImageSizePositionAndRepeatApart(string input, string expected)
    {
        Assert.Equal(expected, _merger.Merge(input));
    }

    [Theory]
    [InlineData("border border-2", "border-2")]
    [InlineData("border-2 border-red-500", "border-2 border-red-500")]
    [InlineData("border-red-500 border-blue-500", "border-blue-500")]
    [InlineData("border-t-2 border-b-4", "border-t-2 border-b-4")]
    [InlineData("border-t-2 border-2", "border-2")]
    [InlineData("border-2 border-t-4", "border-2 border-t-4")]
    [InlineData("border-x-2 border-r-4", "border-x-2 border-r-4")]
    [InlineData("border-r-4 border-x-2", "border-x-2")]
    [InlineData("border-x-red-500 border-x-blue-500", "border-x-blue-500")]
    [InlineData("border-x-2 border-x-red-500", "border-x-2 border-x-red-500")]
    [InlineData("border-solid border-dashed", "border-dashed")]
    [InlineData("border-[length:3px] border-[#fff]", "border-[length:3px] border-[#fff]")]
    [InlineData("border-[3px] border-2", "border-2")]
    [InlineData("border-l-4 border-2", "border-2")]
    [InlineData("divide-y divide-gray-200", "divide-y divide-gray-200")]
    [InlineData("outline outline-2 outline-red-500", "outline-2 outline-red-500")]
    [InlineData("ring ring-2 ring-red-500", "ring-2 ring-red-500")]
    [InlineData("ring-offset-2 ring-offset-white", "ring-offset-2 ring-offset-white")]
    public void Merge_TellsBorderWidthColorAndStyleApart_BySide(string input, string expected)
    {
        Assert.Equal(expected, _merger.Merge(input));
    }

    [Theory]
    [InlineData("shadow shadow-md", "shadow-md")]
    [InlineData("shadow-sm shadow-none", "shadow-none")]
    [InlineData("shadow-md shadow-red-500", "shadow-md shadow-red-500")]
    [InlineData("shadow-red-500 shadow-blue-500", "shadow-blue-500")]
    [InlineData("shadow-[0_1px_2px_black] shadow-lg", "shadow-lg")]
    [InlineData("shadow-lg shadow-[#fff]", "shadow-lg shadow-[#fff]")]
    [InlineData("blur-sm blur-lg", "blur-lg")]
    [InlineData("transition transition-colors", "transition-colors")]
    [InlineData("scale-75 scale-x-50", "scale-75 scale-x-50")]
    [InlineData("scale-x-50 scale-75", "scale-75")]
    public void Merge_TellsShadowSizeAndColorApart(string input, string expected)
    {
        Assert.Equal(expected, _merger.Merge(input));
    }

    [Theory]
    [InlineData("hover:px-2 px-4", "hover:px-2 px-4")]
    [InlineData("px-4 hover:px-2", "px-4 hover:px-2")]
    [InlineData("hover:px-4 hover:px-2", "hover:px-2")]
    [InlineData("md:px-4 md:px-2", "md:px-2")]
    [InlineData("md:hover:px-4 hover:md:px-2", "hover:md:px-2")]
    [InlineData("md:px-4 lg:px-2", "md:px-4 lg:px-2")]
    [InlineData("dark:bg-red-500 dark:bg-blue-500", "dark:bg-blue-500")]
    [InlineData("dark:bg-red-500 bg-blue-500", "dark:bg-red-500 bg-blue-500")]
    [InlineData("px-4 !px-2", "px-4 !px-2")]
    [InlineData("!px-4 !px-2", "!px-2")]
    [InlineData("px-4! px-2!", "px-2!")]
    [InlineData("[&>*]:px-4 [&>*]:px-2", "[&>*]:px-2")]
    [InlineData("[&>*]:px-4 [&>p]:px-2", "[&>*]:px-4 [&>p]:px-2")]
    [InlineData("supports-[appearance:base-select]:flex flex", "supports-[appearance:base-select]:flex flex")]
    public void Merge_OnlyMergesClasses_ThatShareVariantsAndImportance(string input, string expected)
    {
        Assert.Equal(expected, _merger.Merge(input));
    }

    [Theory]
    [InlineData("-mt-2 mt-4", "mt-4")]
    [InlineData("mt-4 -mt-2", "-mt-2")]
    [InlineData("-translate-x-1 translate-x-2", "translate-x-2")]
    [InlineData("w-[10px] w-[20px]", "w-[20px]")]
    [InlineData("[mask-type:alpha] [mask-type:luminance]", "[mask-type:luminance]")]
    [InlineData("[mask-type:alpha] [color:red]", "[mask-type:alpha] [color:red]")]
    [InlineData("hover:[mask-type:alpha] [mask-type:luminance]", "hover:[mask-type:alpha] [mask-type:luminance]")]
    public void Merge_HandlesNegativeAndArbitraryValues(string input, string expected)
    {
        Assert.Equal(expected, _merger.Merge(input));
    }

    [Theory]
    [InlineData("btn btn-primary", "btn btn-primary")]
    [InlineData("tw-icon-pulse tw-icon-pulse", "tw-icon-pulse tw-icon-pulse")]
    [InlineData("my-class px-4 other-class px-2", "my-class other-class px-2")]
    [InlineData("peer group px-4", "peer group px-4")]
    public void Merge_AlwaysKeepsClasses_ThatAreNotTailwindUtilities(string input, string expected)
    {
        Assert.Equal(expected, _merger.Merge(input));
    }

    [Fact]
    public void Merge_KeepsTheOriginalOrder_OfTheClassesItKeeps()
    {
        Assert.Equal("a c e px-2 b", _merger.Merge("a px-4 c e px-3 px-2 b"));
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("  px-4   py-2  ", "px-4 py-2")]
    [InlineData("px-4\tpy-2\npx-1", "py-2 px-1")]
    public void Merge_HandlesEmptyInput_AndNormalizesWhitespace(string? input, string expected)
    {
        Assert.Equal(expected, _merger.Merge(input));
    }

    [Fact]
    public void Merge_ReturnsTheSameResult_WhenCalledAgainWithTheSameInput()
    {
        var first = _merger.Merge("px-4 px-2 flex");
        var second = _merger.Merge("px-4 px-2 flex");

        Assert.Equal("px-2 flex", first);
        Assert.Equal(first, second);
    }

    [Fact]
    public void Merge_StillWorks_WhenMoreDistinctInputsThanTheCacheHolds()
    {
        for (var i = 0; i < 3000; i++)
            Assert.Equal($"px-{i} flex", _merger.Merge($"px-0 px-{i} flex"));
    }

    [Fact]
    public void Merge_OnlyTrims_WhenDisabled()
    {
        var merger = new TwClassMerger(new TwClassMergeOptions { Enabled = false });

        Assert.Equal("px-4  px-2", merger.Merge("  px-4  px-2  "));
    }

    [Fact]
    public void Merge_UsesCustomGroups_BeforeTheBuiltInOnes()
    {
        var options = new TwClassMergeOptions();
        options.Groups.Add(new TwClassGroup("btn-size", ["btn"]));
        var merger = new TwClassMerger(options);

        Assert.Equal("btn-lg flex", merger.Merge("btn-sm btn-lg flex"));
        Assert.Equal("btn-sm btn-lg", new TwClassMerger(new TwClassMergeOptions()).Merge("btn-sm btn-lg"));
    }

    [Fact]
    public void Merge_MatchesACustomPrefix_OnItsOwn_AndWithVariants()
    {
        var options = new TwClassMergeOptions();
        options.Groups.Add(new TwClassGroup("card", ["card"]));
        var merger = new TwClassMerger(options);

        Assert.Equal("card-flat", merger.Merge("card card-flat"));
        Assert.Equal("card hover:card-flat", merger.Merge("hover:card card hover:card-flat"));
    }

    [Fact]
    public void Merge_AppliesTheOverridesOfACustomGroup()
    {
        var options = new TwClassMergeOptions();
        options.Groups.Add(new TwClassGroup("spacing-all", ["pad"]) { Overrides = ["px", "py"] });
        var merger = new TwClassMerger(options);

        Assert.Equal("pad-4", merger.Merge("px-2 py-2 pad-4"));
        Assert.Equal("pad-4 px-2", merger.Merge("pad-4 px-2"));
    }

    [Fact]
    public void Merge_ExtendsABuiltInGroup_WhenACustomGroupReusesItsName()
    {
        var options = new TwClassMergeOptions();
        options.Groups.Add(new TwClassGroup("p", ["gutter"]));
        var merger = new TwClassMerger(options);

        Assert.Equal("gutter-2", merger.Merge("gutter-1 gutter-2"));
        Assert.Equal("gutter-2", merger.Merge("px-4 pt-2 gutter-2"));
    }

    [Fact]
    public void Merge_MergesOverridesIntoTheBuiltInConflicts_WhenACustomGroupReusesABuiltInName()
    {
        var options = new TwClassMergeOptions();
        options.Groups.Add(new TwClassGroup("px", ["gutter-x"]) { Overrides = ["gap-x"] });
        var merger = new TwClassMerger(options);

        Assert.Equal("gutter-x-2", merger.Merge("pr-4 pl-4 gap-x-2 gutter-x-2"));
    }

    [Fact]
    public void Configure_ReplacesTheMergerTheApplicationUses()
    {
        var original = TwClassMerger.Current;

        try
        {
            var options = new TwClassMergeOptions();
            options.Groups.Add(new TwClassGroup("configure-test", ["configure-test"]));

            TwClassMerger.Configure(options);

            Assert.NotSame(original, TwClassMerger.Current);
            Assert.Equal("configure-test-b", TwClassMerger.Current.Merge("configure-test-a configure-test-b"));
        }
        finally
        {
            TwClassMerger.Configure(new TwClassMergeOptions());
        }
    }

    [Fact]
    public void TwClassMergeOptions_DefaultsToEnabledWithNoCustomGroups()
    {
        var options = new TwClassMergeOptions();

        Assert.True(options.Enabled);
        Assert.Empty(options.Groups);
        Assert.NotNull(new TwBlazorOptions { Theme = null! }.ClassMerge);
    }
}
