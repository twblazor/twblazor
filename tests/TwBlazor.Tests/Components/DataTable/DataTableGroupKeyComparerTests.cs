using TwBlazor.Components;

namespace TwBlazor.Tests.Components.DataTable;

public class DataTableGroupKeyComparerTests
{
    private static readonly DataTableGroupKeyComparer _comparer = DataTableGroupKeyComparer.Instance;

    [Fact]
    public void Compare_BothNull_AreEqual()
    {
        Assert.Equal(0, _comparer.Compare(null, null));
    }

    [Fact]
    public void Compare_NullKey_SortsBeforeAValue()
    {
        Assert.True(_comparer.Compare(null, "a") < 0);
        Assert.True(_comparer.Compare("a", null) > 0);
    }

    [Fact]
    public void Compare_ComparableKeys_UseTheirOwnOrder()
    {
        Assert.True(_comparer.Compare(2, 10) < 0);
        Assert.True(_comparer.Compare("b", "a") > 0);
    }

    [Fact]
    public void Compare_KeysThatCannotBeCompared_FallBackToTheirText()
    {
        // An int and a string throw from Comparer<object>.Default, so the text "10" vs "9" decides.
        Assert.True(_comparer.Compare(10, "9") < 0);
        Assert.Equal(0, _comparer.Compare(new object(), new object()));
    }
}
