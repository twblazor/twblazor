namespace TwBlazor.A11yTests;

/// <summary>
/// Guards the split of the axe scans across the shard classes: together they must scan every page once.
/// </summary>
public class AccessibilityRoutesTests
{
    [Fact]
    public void Shards_TogetherCoverEveryScan_ExactlyOnce()
    {
        var all = Rows(AccessibilityRoutes.LightAndDark);
        var sharded = Enumerable.Range(0, AccessibilityRoutes.ShardCount).SelectMany(index => Rows(AccessibilityRoutes.Shard(index))).ToList();

        Assert.Equal(all.Count, sharded.Count);
        Assert.Equal(all.ToHashSet(), sharded.ToHashSet());
    }

    [Fact]
    public void Shards_AreEvenlySized()
    {
        var sizes = Enumerable.Range(0, AccessibilityRoutes.ShardCount).Select(index => Rows(AccessibilityRoutes.Shard(index)).Count).ToList();

        Assert.True(sizes.Max() - sizes.Min() <= 1, string.Join(", ", sizes));
    }

    [Fact]
    public void EveryShardIndex_HasAScanClass()
    {
        var shardClasses = typeof(AccessibilityScanTests).Assembly.GetTypes()
            .Count(type => !type.IsAbstract && type.IsSubclassOf(typeof(AccessibilityScanTests)));

        Assert.Equal(AccessibilityRoutes.ShardCount, shardClasses);
    }

    private static List<string> Rows(TheoryData<string, bool> data) =>
        [.. data.Select(row => $"{row.Data.Item1}|{row.Data.Item2}")];
}
