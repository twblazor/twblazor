using TwBlazor.Abstraction;

namespace TwBlazor.ArchitectureTests.Components;

/// <summary>
/// Provides a list of all input components that implement IInputComponent or inherit from TwBlazorInputComponentBase.
/// </summary>
public static class InputComponentsList
{
    /// <summary>
    /// Gets all types that inherit from TwBlazorInputComponentBase.
    /// </summary>
    public static IEnumerable<Type> GetAllInputComponentTypes()
    {
        var assembly = typeof(TwBlazorInputComponentBase).Assembly;

        return assembly.GetTypes()
            .Where(t => !t.IsAbstract
                && !t.IsInterface
                && typeof(TwBlazorInputComponentBase).IsAssignableFrom(t)
                // Exclude the base class itself from the discovered component list
                && t != typeof(TwBlazorInputComponentBase))
            .ToList();
    }

    /// <summary>
    /// Gets all types that implement IInputComponent interface.
    /// </summary>
    public static IEnumerable<Type> GetAllIInputComponentTypes()
    {
        var assembly = typeof(ITwInputComponent).Assembly;

        return assembly.GetTypes()
            .Where(t => !t.IsAbstract
                && !t.IsInterface
                && typeof(ITwInputComponent).IsAssignableFrom(t))
            .ToList();
    }

    /// <summary>
    /// Gets test data for xUnit Theory tests - one row per discovered input component type.
    /// </summary>
    public static TheoryData<Type> GetAllInputComponentTypesAsTestData()
    {
        return [.. GetAllInputComponentTypes()];
    }
}