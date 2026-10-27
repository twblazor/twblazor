using System.Reflection;
using System.Runtime.CompilerServices;

namespace TwBlazor.ArchitectureTests;

/// <summary>
/// Type catalog for the <c>TwBlazor</c> library assembly, shared by the architecture tests. Everything is plain
/// <see cref="System.Reflection"/> so the rules need no third-party architecture-testing package.
/// </summary>
public static class TwBlazorArchitecture
{
    /// <summary>The namespace components live in (feature subnamespaces live below it).</summary>
    public const string ComponentsNamespace = "TwBlazor.Components";

    /// <summary>The namespace shared enums live in.</summary>
    public const string EnumsNamespace = "TwBlazor.Enums";

    /// <summary>The namespace shared models live in.</summary>
    public const string ModelsNamespace = "TwBlazor.Models";

    /// <summary>The namespace component themes live in.</summary>
    public const string ComponentThemesNamespace = "TwBlazor.Configuration.Components";

    private const BindingFlags publicMembers = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

    private static readonly Lazy<Type[]> _allTypes = new(LoadAllTypes);

    /// <summary>
    /// Gets the <c>TwBlazor</c> library assembly. This is the only assembly the architecture rules cover; the
    /// theme package, the docs and the test assemblies are out of scope.
    /// </summary>
    public static Assembly Library => typeof(TwBlazorComponentBase).Assembly;

    /// <summary>
    /// Gets every public, top-level, non compiler-generated type in the library. Nested public types are
    /// implementation details of their declaring type and are checked by the owning rule instead.
    /// </summary>
    public static IReadOnlyList<Type> PublicTypes =>
    [
        .. _allTypes.Value.Where(t =>
            t.IsPublic
            && !t.IsNested
            && !t.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)),
    ];

    /// <summary>
    /// Determines whether <paramref name="type"/> is declared directly in <paramref name="namespaceName"/>.
    /// </summary>
    public static bool IsInNamespace(Type type, string namespaceName) =>
        string.Equals(type.Namespace, namespaceName, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether <paramref name="type"/> is declared in <paramref name="namespaceName"/> or any of its
    /// subnamespaces.
    /// </summary>
    public static bool IsInNamespaceTree(Type type, string namespaceName) =>
        type.Namespace is { } typeNamespace
        && (string.Equals(typeNamespace, namespaceName, StringComparison.Ordinal)
            || typeNamespace.StartsWith(namespaceName + ".", StringComparison.Ordinal));

    /// <summary>
    /// Gets the types a type exposes through its public surface: its base type, its implemented interfaces, its
    /// public property and field types, its public constructor parameters, and its public method parameters and
    /// return types. Nullable, array, and generic argument types (for example <c>Task&lt;T&gt;</c> and
    /// <c>IEnumerable&lt;T&gt;</c>) are unwrapped so the element type is included too.
    /// </summary>
    public static IReadOnlyCollection<Type> GetReferencedTypes(Type type)
    {
        var exposedTypes = new List<Type>();

        if (type.BaseType is { } baseType)
        {
            exposedTypes.Add(baseType);
        }

        exposedTypes.AddRange(type.GetInterfaces());
        exposedTypes.AddRange(type.GetFields(publicMembers).Select(f => f.FieldType));
        exposedTypes.AddRange(type.GetProperties(publicMembers).Select(p => p.PropertyType));

        foreach (var constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
        {
            exposedTypes.AddRange(constructor.GetParameters().Select(p => p.ParameterType));
        }

        foreach (var method in type.GetMethods(publicMembers).Where(m => !m.IsSpecialName))
        {
            exposedTypes.Add(method.ReturnType);
            exposedTypes.AddRange(method.GetParameters().Select(p => p.ParameterType));
        }

        var referencedTypes = new HashSet<Type>();
        foreach (var exposedType in exposedTypes)
        {
            Unwrap(exposedType, referencedTypes);
        }

        return referencedTypes;
    }

    /// <summary>
    /// Gets the name used in failure messages and in the documented exception lists.
    /// </summary>
    public static string TypeName(Type type) => type.FullName ?? type.Name;

    private static void Unwrap(Type type, HashSet<Type> into)
    {
        if (!into.Add(type))
        {
            return;
        }

        if (Nullable.GetUnderlyingType(type) is { } underlyingType)
        {
            Unwrap(underlyingType, into);
            return;
        }

        if (type.IsArray && type.GetElementType() is { } elementType)
        {
            Unwrap(elementType, into);
            return;
        }

        if (type.IsGenericType)
        {
            foreach (var argument in type.GetGenericArguments())
            {
                Unwrap(argument, into);
            }
        }
    }

    private static Type[] LoadAllTypes()
    {
        try
        {
            return Library.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            var loaderMessages = string.Join(
                Environment.NewLine,
                ex.LoaderExceptions.Select(e => e?.Message ?? "<unknown loader exception>"));

            throw new InvalidOperationException(
                $"Could not load every type in {Library.GetName().Name}.{Environment.NewLine}{loaderMessages}",
                ex);
        }
    }
}
