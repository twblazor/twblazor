using TwBlazor.Components;

namespace TwBlazor.Tests.Components.Table;

public class TwTableRowItemComparerTests
{
    private static readonly TwTableRowItemComparer<string> _comparer = TwTableRowItemComparer<string>.Instance;

    [Fact]
    public void Equals_MatchesTheSameItem_AtADifferentIndex()
    {
        var before = new TwTableRow<string>("Apple", 0, "2");
        var after = new TwTableRow<string>("Apple", 3, "5");

        Assert.True(_comparer.Equals(before, after));
        Assert.Equal(_comparer.GetHashCode(before), _comparer.GetHashCode(after));
    }

    [Fact]
    public void Equals_DoesNotMatchDifferentItems_AtTheSameIndex()
    {
        Assert.False(_comparer.Equals(new TwTableRow<string>("Apple", 0, null), new TwTableRow<string>("Pear", 0, null)));
    }

    [Fact]
    public void GetHashCode_IsZero_ForANullItem()
    {
        var comparer = TwTableRowItemComparer<string?>.Instance;

        Assert.Equal(0, comparer.GetHashCode(new TwTableRow<string?>(null, 0, null)));
        Assert.True(comparer.Equals(new TwTableRow<string?>(null, 0, null), new TwTableRow<string?>(null, 1, null)));
    }
}
