using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace TwBlazor.ArchitectureTests;

/// <summary>
/// Architecture and consistency rules for the <c>TwBlazor</c> library assembly. Every rule is expressed with plain
/// <see cref="System.Reflection"/> against <see cref="TwBlazorArchitecture.PublicTypes"/>, so no third-party
/// architecture-testing package is needed and nothing outside the library assembly is inspected.
/// </summary>
public class ArchitectureTests
{
    #region Documented exceptions

    // Rule 2 baseline: public enums declared next to the feature that owns them instead of in TwBlazor.Enums.
    // These are feature-local enums (charts, data table filtering, calendar/date picker UI state, and the input
    // builders) that predate the "all enums live in TwBlazor.Enums" rule. The list is exact: moving one of these
    // into TwBlazor.Enums makes this test fail so the baseline gets trimmed instead of silently going stale.
    private static readonly HashSet<string> _enumsOutsideEnumsNamespace = new(StringComparer.Ordinal)
    {
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Builders.InputFocusStyle)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Charting.ChartAnchor)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Charting.ChartBaseline)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Charting.ChartColorRole)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Charting.ChartCorner)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Charting.ChartHover)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Charting.ChartShape)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Charting.ChartStroke)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Charting.ChartTextTone)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.DatePicker.DatePickerCalendarView)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwCalendarDialogFieldKind)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwCalendarEventDialogAction)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwCalendarView)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwDataTableFilterLogic)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwDataTableFilterOperator)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwDataTableFilterType)),
    };

    // Rule 3 baseline: the two sentinel-constant holders that live in TwBlazor.Enums because their constants stand
    // in for enum values in component parameters (GridSpan.Full and ItemOrder.First/Last).
    private static readonly HashSet<string> _nonEnumTypesInEnumsNamespace = new(StringComparer.Ordinal)
    {
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Enums.GridSpan)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Enums.ItemOrder)),
    };

    // Rule 4 baseline: the root theme aggregate sits in TwBlazor.Configuration and owns the per-component themes
    // in TwBlazor.Configuration.Components, so it is deliberately not part of that namespace.
    private static readonly HashSet<string> _themeTypesOutsideThemeNamespace = new(StringComparer.Ordinal)
    {
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Configuration.TwBlazorTheme)),
    };

    // Rule 5 baseline: TwChartColor is a shared chart colour value declared beside TwChartTheme so the Tailwind
    // compiler can see the class literals; it is not a theme itself and has no "Theme" suffix.
    private static readonly HashSet<string> _nonThemeTypesInThemeNamespace = new(StringComparer.Ordinal)
    {
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Configuration.Components.TwChartColor)),
    };

    // Rule 5 baseline: abstract theme bases exist only to be inherited, so they cannot expose a public
    // parameterless constructor (their constructor is protected).
    private static readonly HashSet<string> _abstractThemeClasses = new(StringComparer.Ordinal)
    {
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Configuration.Components.TwEncodedSymbolTheme)),
    };

    // Rule 7 baseline: the public non-component types that currently live in a TwBlazor.Components namespace.
    // They are data-table/calendar/schedule models, delegates and enums rather than components. They arguably
    // belong in TwBlazor.Models; until they move, this exact inventory is frozen so a new non-component type in a
    // component namespace is a test failure instead of a silent drift.
    private static readonly HashSet<string> _nonComponentTypesInComponentsNamespace = new(StringComparer.Ordinal)
    {
        // Chart helpers.
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.ChartSeriesSlot)),
        // Calendar models and UI state.
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.Schedule<>)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.DatePicker.DatePickerCalendarView)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwCalendarDialogField)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwCalendarDialogFieldKind)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwCalendarEventDialogAction)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwCalendarEventDialogResult<>)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwCalendarEventLayout)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwCalendarView)),
        // Data table models, delegates and UI state.
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwDataTableCellEditContext<>)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwDataTableColumn<>)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwDataTableColumnFilter)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwDataTableColumnState)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwDataTableExport)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwDataTableFilterCondition)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwDataTableFilterLogic)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwDataTableFilterOperator)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwDataTableFilterType)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwDataTableGroup<>)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwDataTableItemsProvider<>)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwDataTableItemsRequest)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwDataTableItemsResult<>)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwDataTableRowEdit<>)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwDataTableState)),
        // Table row models, delegates and results.
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwTableRow<>)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwTableRowsPage<>)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwTableRowsProvider<>)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwTableRowsRequest)),
        TwBlazorArchitecture.TypeName(typeof(TwBlazor.Components.TwTableRowsResult<>)),
    };

    // Rule 8: the namespaces a model or enum may reference. Abstraction holds the component contracts and
    // Resources holds the generated string resources; both are leaf-facing, so a model may use them without
    // depending on a component, a builder, or the configuration layer.
    private static readonly HashSet<string> _allowedModelNamespaces = new(StringComparer.Ordinal)
    {
        TwBlazorArchitecture.ModelsNamespace,
        TwBlazorArchitecture.EnumsNamespace,
        "TwBlazor.Abstraction",
        "TwBlazor.Resources",
    };

    #endregion

    /// <summary>
    /// Rule 1: every public type is declared in a namespace rooted at <c>TwBlazor</c>. Global-namespace types and
    /// types leaked into a foreign namespace break the package's public surface.
    /// </summary>
    [Fact]
    public void PublicTypes_ShouldBeDeclaredInATwBlazorNamespace()
    {
        // Act
        var offenders = TwBlazorArchitecture.PublicTypes
            .Where(t => t.Namespace is not { } typeNamespace
                || !typeNamespace.StartsWith("TwBlazor", StringComparison.Ordinal))
            .Select(TwBlazorArchitecture.TypeName)
            .ToList();

        // Assert
        Assert.NotEmpty(TwBlazorArchitecture.PublicTypes);
        Assert.Empty(offenders);
    }

    /// <summary>
    /// Rule 2: public enums live in <see cref="TwBlazorArchitecture.EnumsNamespace"/>. The documented baseline
    /// holds the feature-local enums that were declared before this rule; anything new outside the namespace fails.
    /// </summary>
    [Fact]
    public void Enums_ShouldBeDeclaredInTheEnumsNamespace()
    {
        // Act
        var actual = TwBlazorArchitecture.PublicTypes
            .Where(t => t.IsEnum && !TwBlazorArchitecture.IsInNamespace(t, TwBlazorArchitecture.EnumsNamespace))
            .Select(TwBlazorArchitecture.TypeName);

        // Assert
        Assert.Equal(SortedNames(_enumsOutsideEnumsNamespace), SortedNames(actual));
    }

    /// <summary>
    /// Rule 3: <see cref="TwBlazorArchitecture.EnumsNamespace"/> holds enums only, apart from the documented
    /// sentinel-constant holders that stand in for enum values.
    /// </summary>
    [Fact]
    public void EnumsNamespace_ShouldContainOnlyEnums()
    {
        // Act
        var actual = TwBlazorArchitecture.PublicTypes
            .Where(t => TwBlazorArchitecture.IsInNamespace(t, TwBlazorArchitecture.EnumsNamespace) && !t.IsEnum)
            .Select(TwBlazorArchitecture.TypeName);

        // Assert
        Assert.Equal(SortedNames(_nonEnumTypesInEnumsNamespace), SortedNames(actual));
    }

    /// <summary>
    /// Rule 4: every type named <c>Tw*Theme</c> lives in
    /// <see cref="TwBlazorArchitecture.ComponentThemesNamespace"/>, except the documented root theme aggregate.
    /// </summary>
    [Fact]
    public void ThemeTypes_ShouldBeDeclaredInTheComponentThemesNamespace()
    {
        // Act
        var actual = TwBlazorArchitecture.PublicTypes
            .Where(t => t.Name.EndsWith("Theme", StringComparison.Ordinal)
                && !TwBlazorArchitecture.IsInNamespace(t, TwBlazorArchitecture.ComponentThemesNamespace))
            .Select(TwBlazorArchitecture.TypeName);

        // Assert
        Assert.Equal(SortedNames(_themeTypesOutsideThemeNamespace), SortedNames(actual));
    }

    /// <summary>
    /// Rule 5: every theme class in <see cref="TwBlazorArchitecture.ComponentThemesNamespace"/> is named
    /// <c>Tw*Theme</c>, is excluded from code coverage, can be constructed and configured by consumers (public
    /// parameterless constructor), and exposes properties instead of public fields.
    /// </summary>
    [Fact]
    public void ThemeClasses_ShouldFollowTheThemeShape()
    {
        // Arrange
        AssertTypesExist(_nonThemeTypesInThemeNamespace);
        AssertTypesExist(_abstractThemeClasses);

        var themeClasses = TwBlazorArchitecture.PublicTypes
            .Where(t => TwBlazorArchitecture.IsInNamespace(t, TwBlazorArchitecture.ComponentThemesNamespace) && t.IsClass)
            .ToList();

        // Act
        var offenders = new List<string>();
        foreach (var theme in themeClasses)
        {
            var typeName = TwBlazorArchitecture.TypeName(theme);

            if (_nonThemeTypesInThemeNamespace.Contains(typeName))
            {
                continue;
            }

            if (!theme.Name.StartsWith("Tw", StringComparison.Ordinal)
                || !theme.Name.EndsWith("Theme", StringComparison.Ordinal))
            {
                offenders.Add($"{typeName}: theme classes should be named Tw*Theme");
            }

            if (!theme.IsDefined(typeof(ExcludeFromCodeCoverageAttribute), inherit: false))
            {
                offenders.Add($"{typeName}: missing [ExcludeFromCodeCoverage]");
            }

            if (!_abstractThemeClasses.Contains(typeName) && theme.GetConstructor(Type.EmptyTypes) is null)
            {
                offenders.Add($"{typeName}: missing a public parameterless constructor");
            }

            var publicFields = theme.GetFields(BindingFlags.Public | BindingFlags.Instance);
            if (publicFields.Length > 0)
            {
                offenders.Add(
                    $"{typeName}: exposes public field(s) {string.Join(", ", publicFields.Select(f => f.Name))}; use properties");
            }
        }

        // Assert
        Assert.NotEmpty(themeClasses);
        Assert.Empty(offenders);
    }

    /// <summary>
    /// Rule 6: every public component in a <c>TwBlazor.Components</c> namespace is named <c>Tw*</c>, so components
    /// stay distinguishable from the models and helpers they take as parameters.
    /// </summary>
    [Fact]
    public void ComponentTypes_ShouldBeNamedTw()
    {
        // Act
        var componentTypes = GetComponentTypes();
        var offenders = componentTypes
            .Where(t => !t.Name.StartsWith("Tw", StringComparison.Ordinal))
            .Select(TwBlazorArchitecture.TypeName)
            .ToList();

        // Assert
        Assert.NotEmpty(componentTypes);
        Assert.Empty(offenders);
    }

    /// <summary>
    /// Rule 7: every public component in a <c>TwBlazor.Components</c> namespace derives from
    /// <see cref="ComponentBase"/>. The documented baseline freezes the non-component types currently declared in
    /// those namespaces (models, delegates and enums), so only an explicit, reviewed addition can extend it.
    /// </summary>
    [Fact]
    public void ComponentTypes_ShouldDeriveFromComponentBase()
    {
        // Act
        var componentTypes = GetComponentTypes();
        var notDerivedFromComponentBase = componentTypes
            .Where(t => !typeof(ComponentBase).IsAssignableFrom(t))
            .Select(TwBlazorArchitecture.TypeName)
            .ToList();

        var actualNonComponentTypes = TwBlazorArchitecture.PublicTypes
            .Where(t => TwBlazorArchitecture.IsInNamespaceTree(t, TwBlazorArchitecture.ComponentsNamespace)
                && !typeof(ComponentBase).IsAssignableFrom(t))
            .Select(TwBlazorArchitecture.TypeName);

        // Assert
        Assert.NotEmpty(componentTypes);
        Assert.Empty(notDerivedFromComponentBase);
        Assert.Equal(SortedNames(_nonComponentTypesInComponentsNamespace), SortedNames(actualNonComponentTypes));
    }

    /// <summary>
    /// Rule 8: <c>TwBlazor.Models</c> and <c>TwBlazor.Enums</c> are leaf namespaces. Their public surface may use
    /// <c>System</c>, <c>Microsoft.*</c> and the other leaf namespaces, but never a component, theme, builder,
    /// localization, utility, extension or service type.
    /// </summary>
    [Fact]
    public void ModelsAndEnums_ShouldNotDependOnHigherLayers()
    {
        // Arrange
        var leafTypes = TwBlazorArchitecture.PublicTypes
            .Where(t => TwBlazorArchitecture.IsInNamespace(t, TwBlazorArchitecture.ModelsNamespace)
                || TwBlazorArchitecture.IsInNamespace(t, TwBlazorArchitecture.EnumsNamespace))
            .ToList();

        // Act
        var offenders = new List<string>();
        foreach (var leafType in leafTypes)
        {
            var offendingReferences = TwBlazorArchitecture.GetReferencedTypes(leafType)
                .Where(t => t.Namespace is { } referencedNamespace
                    && referencedNamespace.StartsWith("TwBlazor", StringComparison.Ordinal)
                    && !_allowedModelNamespaces.Contains(referencedNamespace))
                .Select(TwBlazorArchitecture.TypeName)
                .Distinct()
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToList();

            if (offendingReferences.Count > 0)
            {
                offenders.Add($"{TwBlazorArchitecture.TypeName(leafType)} -> {string.Join(", ", offendingReferences)}");
            }
        }

        // Assert
        Assert.NotEmpty(leafTypes);
        Assert.Empty(offenders);
    }

    /// <summary>
    /// Gets the public components declared in a <c>TwBlazor.Components</c> namespace.
    /// </summary>
    private static IReadOnlyList<Type> GetComponentTypes() =>
    [
        .. TwBlazorArchitecture.PublicTypes
            .Where(t => TwBlazorArchitecture.IsInNamespaceTree(t, TwBlazorArchitecture.ComponentsNamespace)
                && typeof(IComponent).IsAssignableFrom(t)),
    ];

    /// <summary>
    /// Asserts that every documented exception still resolves to a public type, so the lists cannot rot silently.
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
