using TwBlazor.Utilities.ClassMerge;

namespace TwBlazor.Tests.Utilities.ClassMerge;

public class TwClassValueTests
{
    [Theory]
    [InlineData("4", true)]
    [InlineData("0.5", true)]
    [InlineData(".5", true)]
    [InlineData("5.", true)]
    [InlineData("-3", true)]
    [InlineData("+3", true)]
    [InlineData("1e3", true)]
    [InlineData("0x1F", true)]
    [InlineData("Infinity", true)]
    [InlineData("", false)]
    [InlineData("4px", false)]
    [InlineData("1/2", false)]
    [InlineData("NaN", false)]
    [InlineData("1,5", false)]
    [InlineData("one", false)]
    public void Number_AcceptsWhatJavaScriptNumberAccepts(string value, bool expected)
    {
        Assert.Equal(expected, TwClassValue.Get(TwClassValidator.Number)(value));
    }

    [Theory]
    [InlineData("4", true)]
    [InlineData("4.0", true)]
    [InlineData("1e2", true)]
    [InlineData("0x10", true)]
    [InlineData("4.5", false)]
    [InlineData("Infinity", false)]
    [InlineData("", false)]
    [InlineData("four", false)]
    public void Integer_AcceptsWholeNumbers(string value, bool expected)
    {
        Assert.Equal(expected, TwClassValue.Get(TwClassValidator.Integer)(value));
    }

    [Theory]
    [InlineData(nameof(TwClassValidator.Fraction), "1/2", true)]
    [InlineData(nameof(TwClassValidator.Fraction), "1.5/2", true)]
    [InlineData(nameof(TwClassValidator.Fraction), "1/", false)]
    [InlineData(nameof(TwClassValidator.Fraction), "a/b", false)]
    [InlineData(nameof(TwClassValidator.Percent), "50%", true)]
    [InlineData(nameof(TwClassValidator.Percent), "12.5%", true)]
    [InlineData(nameof(TwClassValidator.Percent), "%", false)]
    [InlineData(nameof(TwClassValidator.Percent), "50", false)]
    [InlineData(nameof(TwClassValidator.TshirtSize), "sm", true)]
    [InlineData(nameof(TwClassValidator.TshirtSize), "2xl", true)]
    [InlineData(nameof(TwClassValidator.TshirtSize), "3.5xl", true)]
    [InlineData(nameof(TwClassValidator.TshirtSize), "base", false)]
    [InlineData(nameof(TwClassValidator.TshirtSize), "xxl", false)]
    [InlineData(nameof(TwClassValidator.Any), "anything", true)]
    [InlineData(nameof(TwClassValidator.AnyNonArbitrary), "red-500", true)]
    [InlineData(nameof(TwClassValidator.AnyNonArbitrary), "[#fff]", false)]
    [InlineData(nameof(TwClassValidator.AnyNonArbitrary), "(--brand)", false)]
    [InlineData(nameof(TwClassValidator.NamedContainerQuery), "@container/main", true)]
    [InlineData(nameof(TwClassValidator.NamedContainerQuery), "@container-size/main", true)]
    [InlineData(nameof(TwClassValidator.NamedContainerQuery), "@container-normal/main", true)]
    [InlineData(nameof(TwClassValidator.NamedContainerQuery), "@container", false)]
    [InlineData(nameof(TwClassValidator.NamedContainerQuery), "@container/", false)]
    [InlineData(nameof(TwClassValidator.NamedContainerQuery), "@container-size/", false)]
    [InlineData(nameof(TwClassValidator.NamedContainerQuery), "container/main", false)]
    public void PlainValidators_ClassifyTheValue(string validator, string value, bool expected)
    {
        Assert.Equal(expected, TwClassValue.Get(Enum.Parse<TwClassValidator>(validator))(value));
    }

    [Theory]
    [InlineData(nameof(TwClassValidator.ArbitraryValue), "[10px]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryValue), "[color:red]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryValue), "[]", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryValue), "(--x)", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryLength), "[10px]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryLength), "[2.5rem]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryLength), "[10cqw]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryLength), "[0]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryLength), "[calc(100%-1rem)]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryLength), "[clamp(1rem,2vw,2rem)]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryLength), "[length:var(--x)]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryLength), "[hsl(0_0%_0%)]", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryLength), "[color:10px]", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryLength), "[var(--x)]", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryNumber), "[1.5]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryNumber), "[number:var(--x)]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryNumber), "[1.5px]", false)]
    [InlineData(nameof(TwClassValidator.ArbitrarySize), "[size:200px_100px]", true)]
    [InlineData(nameof(TwClassValidator.ArbitrarySize), "[length:200px_100px]", true)]
    [InlineData(nameof(TwClassValidator.ArbitrarySize), "[bg-size:200px]", true)]
    [InlineData(nameof(TwClassValidator.ArbitrarySize), "[200px_100px]", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryPosition), "[position:200px_100px]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryPosition), "[percentage:30%]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryPosition), "[right_center]", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryPositionKeywords), "[right_0.5rem_center]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryPositionKeywords), "[center]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryPositionKeywords), "[top-left]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryPositionKeywords), "[position:center]", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryPositionKeywords), "[linear-gradient(to_right_bottom,red,blue)]", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryPositionKeywords), "[color-mix(in_oklab,red_center,blue)]", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryPositionKeywords), "[#fff]", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryImage), "[url(./x.png)]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryImage), "[linear-gradient(red,blue)]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryImage), "[repeating-conic-gradient(red,blue)]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryImage), "[image:var(--x)]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryImage), "[url:var(--x)]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryImage), "[#fff]", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryShadow), "[0_1px_2px_black]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryShadow), "[inset_0_1px_0_0_#fff]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryShadow), "[shadow:var(--x)]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryShadow), "[#fff]", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryWeight), "[700]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryWeight), "[weight:var(--x)]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryWeight), "[family-name:var(--x)]", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryFamilyName), "[family-name:var(--x)]", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryFamilyName), "[Inter]", false)]
    public void ArbitraryValueValidators_UseTheLabel_OrInspectTheValue(string validator, string value, bool expected)
    {
        Assert.Equal(expected, TwClassValue.Get(Enum.Parse<TwClassValidator>(validator))(value));
    }

    [Theory]
    [InlineData(nameof(TwClassValidator.ArbitraryVariable), "(--x)", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariable), "(color:--x)", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariable), "()", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariable), "[--x]", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariableLength), "(length:--x)", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariableLength), "(--x)", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariableSize), "(size:--x)", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariableSize), "(bg-size:--x)", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariableSize), "(--x)", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariablePosition), "(position:--x)", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariablePosition), "(--x)", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariableImage), "(image:--x)", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariableImage), "(--x)", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariableFamilyName), "(family-name:--x)", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariableFamilyName), "(--x)", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariableShadow), "(shadow:--x)", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariableShadow), "(--x)", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariableShadow), "(color:--x)", false)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariableWeight), "(weight:--x)", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariableWeight), "(--x)", true)]
    [InlineData(nameof(TwClassValidator.ArbitraryVariableWeight), "(family-name:--x)", false)]
    public void ArbitraryVariableValidators_UseTheLabel(string validator, string value, bool expected)
    {
        Assert.Equal(expected, TwClassValue.Get(Enum.Parse<TwClassValidator>(validator))(value));
    }

    [Fact]
    public void Get_Throws_ForAValidatorThatDoesNotExist()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TwClassValue.Get((TwClassValidator)999));
    }

    [Theory]
    [InlineData("px-4", "px")]
    [InlineData("-mt-2", "mt")]
    [InlineData("inline-block", "display")]
    [InlineData("text-sm", "font-size")]
    [InlineData("text-red-500", "text-color")]
    [InlineData("border-x-2", "border-w-x")]
    [InlineData("border-x-red-500", "border-color-x")]
    [InlineData("w-[calc(100%-1rem)]", "w")]
    [InlineData("[mask-type:alpha]", "arbitrary..mask-type")]
    [InlineData("[--my-var:1px]", "arbitrary..--my-var")]
    [InlineData("@container/main", "container-named")]
    public void Find_ReturnsTheGroup_OfATailwindClass(string className, string expected)
    {
        Assert.Equal(expected, TwClassGroups.Find(className));
    }

    [Theory]
    [InlineData("btn")]
    [InlineData("w-half")]
    [InlineData("blocky")]
    [InlineData("[]")]
    [InlineData("[nocolon]")]
    [InlineData("[:value]")]
    [InlineData("-")]
    [InlineData("")]
    public void Find_ReturnsNull_ForAClassThatIsNotATailwindUtility(string className)
    {
        Assert.Null(TwClassGroups.Find(className));
    }

    [Fact]
    public void Tables_HoldTheConflictsAndModifiers_OfTheDefaultConfig()
    {
        Assert.Contains("pl", TwClassGroups.Conflicts["px"]);
        Assert.Contains("leading", TwClassGroups.ModifierConflicts["font-size"]);
        Assert.Contains("before", TwClassGroups.OrderSensitiveModifiers);
        Assert.Contains("container-type", TwClassGroups.PostfixLookupGroups);
    }
}
