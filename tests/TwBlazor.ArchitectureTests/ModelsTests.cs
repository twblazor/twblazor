using System.Reflection;

namespace TwBlazor.ArchitectureTests;

/// <summary>
/// Shape rules for the public types in <c>TwBlazor.Models</c>, checked with plain reflection against
/// <see cref="TwBlazorArchitecture.PublicTypes"/> so no third-party architecture-testing package is needed.
/// </summary>
public class ModelsTests
{
    #region Documented exceptions

    // Rule 2 baseline: the models that are deliberately constructed through a specific constructor or a
    // factory instead of a public parameterless constructor. Positional records require their parameters,
    // PopoverTriggerContext is produced by TwPopover's trigger, and TwDialogResult is produced by its
    // Ok/Cancel factories. The list is exact: adding a public parameterless constructor to one of these
    // makes this test fail so the baseline gets trimmed instead of silently going stale.
    private static readonly HashSet<string> _modelsWithoutParameterlessConstructor = new(StringComparer.Ordinal)
    {
        // Value objects built with a specific constructor or factory.
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.PopoverTriggerContext)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.TwDialogResult)),
        // Positional records.
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.FooterLink)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.FooterSection)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.OnThisPageItem)),
        // Positional chart records.
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.ChartBullet)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.ChartCategoricalPoint)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.ChartGraphLink)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.ChartGraphNode)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.ChartHierarchyNode)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.ChartOhlc)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.ChartPoint)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.ChartPointSeries)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.ChartRange)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.ChartSample)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.ChartSeries)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.ChartSet)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.ChartSetIntersection)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.ChartTask)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Models.ChartValue)),
    };

    #endregion

    /// <summary>
    /// Rule 1: every public type in <c>TwBlazor.Models</c> is a class or record, never an enum, interface,
    /// struct or delegate. Models are data shapes handed to components, so a value type or delegate would
    /// break the component's expectations.
    /// </summary>
    [Fact]
    public void Models_ShouldBeClassesOrRecords()
    {
        // Act
        var offenders = ModelTypes()
            .Where(t => !IsClassOrRecord(t))
            .Select(TwBlazorArchitecture.TypeName)
            .ToList();

        // Assert
        Assert.NotEmpty(ModelTypes());
        Assert.Empty(offenders);
    }

    /// <summary>
    /// Rule 2: every public model can be constructed by consumers with a public parameterless constructor,
    /// apart from the documented value objects built through a specific constructor or factory.
    /// </summary>
    [Fact]
    public void Models_ShouldHaveAPublicParameterlessConstructor()
    {
        // Arrange
        AssertTypesExist(_modelsWithoutParameterlessConstructor);

        // Act
        var actual = ModelTypes()
            .Where(t => t.GetConstructor(Type.EmptyTypes) is null)
            .Select(TwBlazorArchitecture.TypeName);

        // Assert
        Assert.Equal(SortedNames(_modelsWithoutParameterlessConstructor), SortedNames(actual));
    }

    /// <summary>
    /// Rule 3: every public model exposes properties only, never public instance fields.
    /// </summary>
    [Fact]
    public void Models_ShouldNotExposePublicInstanceFields()
    {
        // Act
        var offenders = new List<string>();
        foreach (var type in ModelTypes())
        {
            var publicFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            if (publicFields.Length > 0)
            {
                offenders.Add(
                    $"{TwBlazorArchitecture.TypeName(type)}: public field(s) {string.Join(", ", publicFields.Select(f => f.Name))}");
            }
        }

        // Assert
        Assert.Empty(offenders);
    }

    /// <summary>
    /// Gets the public types declared directly in <c>TwBlazor.Models</c>.
    /// </summary>
    private static IReadOnlyList<Type> ModelTypes() =>
    [
        .. TwBlazorArchitecture.PublicTypes
            .Where(t => TwBlazorArchitecture.IsInNamespace(t, TwBlazorArchitecture.ModelsNamespace)),
    ];

    private static bool IsClassOrRecord(Type type) =>
        type.IsClass && !typeof(Delegate).IsAssignableFrom(type);

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
