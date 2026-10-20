using TwBlazor.A11yTests.Infrastructure;

namespace TwBlazor.A11yTests;

// Each shard is its own class so xUnit runs them in parallel. Keep one class per index below
// AccessibilityRoutes.ShardCount, or some routes are never scanned (see AccessibilityRoutesTests).

/// <summary>
/// The first quarter of the axe scans.
/// </summary>
public class AccessibilityScanShard1(A11yFixture fixture) : AccessibilityScanTests(fixture)
{
    [Theory]
    [MemberData(nameof(AccessibilityRoutes.Shard), 0, MemberType = typeof(AccessibilityRoutes))]
    public Task Page_HasNoAxeViolations(string route, bool dark) => ScanAsync(route, dark);
}

/// <summary>
/// The second quarter of the axe scans.
/// </summary>
public class AccessibilityScanShard2(A11yFixture fixture) : AccessibilityScanTests(fixture)
{
    [Theory]
    [MemberData(nameof(AccessibilityRoutes.Shard), 1, MemberType = typeof(AccessibilityRoutes))]
    public Task Page_HasNoAxeViolations(string route, bool dark) => ScanAsync(route, dark);
}

/// <summary>
/// The third quarter of the axe scans.
/// </summary>
public class AccessibilityScanShard3(A11yFixture fixture) : AccessibilityScanTests(fixture)
{
    [Theory]
    [MemberData(nameof(AccessibilityRoutes.Shard), 2, MemberType = typeof(AccessibilityRoutes))]
    public Task Page_HasNoAxeViolations(string route, bool dark) => ScanAsync(route, dark);
}

/// <summary>
/// The last quarter of the axe scans.
/// </summary>
public class AccessibilityScanShard4(A11yFixture fixture) : AccessibilityScanTests(fixture)
{
    [Theory]
    [MemberData(nameof(AccessibilityRoutes.Shard), 3, MemberType = typeof(AccessibilityRoutes))]
    public Task Page_HasNoAxeViolations(string route, bool dark) => ScanAsync(route, dark);
}
