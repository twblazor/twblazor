namespace TwBlazor.ArchitectureTests;

/// <summary>
/// Shape rules for the public types in <c>TwBlazor.Enums</c>, checked with plain reflection against
/// <see cref="TwBlazorArchitecture.PublicTypes"/> so no third-party architecture-testing package is needed.
/// </summary>
public class EnumsTests
{
    #region Documented exceptions

    // Rule 1 baseline: the two static classes that hold sentinel constants standing in for enum values in
    // component parameters (GridSpan.Full and ItemOrder.First/Last). They are not enums but deliberately live
    // in TwBlazor.Enums next to the values they represent.
    private static readonly HashSet<string> _nonEnumTypesInEnumsNamespace = new(StringComparer.Ordinal)
    {
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Enums.GridSpan)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Enums.ItemOrder)),
    };

    #endregion

    /// <summary>
    /// Rule 1: every public type in <c>TwBlazor.Enums</c> is an enum, apart from the documented sentinel
    /// constant holders.
    /// </summary>
    [Fact]
    public void EnumsNamespace_ShouldContainOnlyEnums()
    {
        // Arrange
        AssertTypesExist(_nonEnumTypesInEnumsNamespace);

        // Act
        var actual = EnumsNamespaceTypes()
            .Where(t => !t.IsEnum)
            .Select(TwBlazorArchitecture.TypeName);

        // Assert
        Assert.Equal(SortedNames(_nonEnumTypesInEnumsNamespace), SortedNames(actual));
    }

    /// <summary>
    /// Rule 2: every enum uses the default <see cref="int"/> underlying type, so enum values serialize to the
    /// plain integers components and scripts expect.
    /// </summary>
    [Fact]
    public void Enums_ShouldUseTheDefaultIntUnderlyingType()
    {
        // Act
        var offenders = EnumsNamespaceTypes()
            .Where(t => t.IsEnum && Enum.GetUnderlyingType(t) != typeof(int))
            .Select(TwBlazorArchitecture.TypeName)
            .ToList();

        // Assert
        Assert.Contains(EnumsNamespaceTypes(), t => t.IsEnum);
        Assert.Empty(offenders);
    }

    /// <summary>
    /// Gets the public types declared directly in <c>TwBlazor.Enums</c>.
    /// </summary>
    private static IReadOnlyList<Type> EnumsNamespaceTypes() =>
    [
        .. TwBlazorArchitecture.PublicTypes
            .Where(t => TwBlazorArchitecture.IsInNamespace(t, TwBlazorArchitecture.EnumsNamespace)),
    ];

    /// <summary>
    /// Asserts that every documented exception still resolves to a public type, so the list cannot rot silently.
    /// </summary>
    private static void AssertTypesExist(IEnumerable<string> typeNames)
    {
        foreach (var typeName in typeNames)
        {
            Assert.Contains(TwBlazorArchitecture.PublicTypes, t => TwBlazorArchitecture.TypeName(t) == typeName);
        }
    }

    private static List<string> SortedNames(IEnumerable<string> typeNames) =>
    [
        .. typeNames.OrderBy(n => n, StringComparer.Ordinal),
    ];
}
