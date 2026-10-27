using TwBlazor.Abstraction;

namespace TwBlazor.ArchitectureTests.Components;

/// <summary>
/// Provides a list of all components that inherit from TwBlazorComponentBase or implement IComponent.
/// </summary>
public static class ComponentsList
{
    /// <summary>
    /// Gets all types that inherit from TwBlazorComponentBase.
    /// </summary>
    public static IEnumerable<Type> GetAllComponentTypes()
    {
        var assembly = typeof(TwBlazorComponentBase).Assembly;

        return assembly.GetTypes()
            .Where(t => !t.IsAbstract
                && !t.IsInterface
                && typeof(TwBlazorComponentBase).IsAssignableFrom(t))
            .ToList();
    }

    /// <summary>
    /// Gets all types that implement ITwBlazorComponent interface.
    /// </summary>
    public static IEnumerable<Type> GetAllITwBlazorComponentTypes()
    {
        var assembly = typeof(ITwComponent).Assembly;

        return assembly.GetTypes()
            .Where(t => !t.IsAbstract
                && !t.IsInterface
                && typeof(ITwComponent).IsAssignableFrom(t))
            .ToList();
    }

    /// <summary>
    /// Gets test data for xUnit Theory tests - one row per discovered component type.
    /// </summary>
    public static TheoryData<Type> GetAllComponentTypesAsTestData()
    {
        return [.. GetAllComponentTypes()];
    }
}
