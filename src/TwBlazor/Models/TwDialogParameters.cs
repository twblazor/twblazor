// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

// Design and API shape inspired by MudBlazor's DialogParameters
// (https://github.com/MudBlazor/MudBlazor/tree/dev/src/MudBlazor/Services/Dialog), MIT License.

using System.Collections;

namespace TwBlazor.Models;

/// <summary>
/// A bag of component parameters passed to a dialog shown via <see cref="Services.ITwDialogService"/>.
/// </summary>
/// <remarks>
/// Values are applied to the dialog content component as if they were set with the Blazor <c>[Parameter]</c>
/// attribute, matched by name. Use object initializer syntax, e.g. <c>new TwDialogParameters { ["Message"] = "Hi" }</c>.
/// </remarks>
public class TwDialogParameters : IEnumerable<KeyValuePair<string, object?>>
{
    /// <summary>
    /// An empty, shared set of parameters used when none are supplied to a Show call.
    /// </summary>
    internal static readonly TwDialogParameters _default = [];

    private readonly Dictionary<string, object?> _parameters = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the parameter with the specified name.
    /// </summary>
    /// <param name="parameterName">The name of the component parameter.</param>
    public object? this[string parameterName]
    {
        get => _parameters[parameterName];
        set => Add(parameterName, value);
    }

    /// <summary>
    /// Gets the number of parameters in this collection.
    /// </summary>
    public int Count => _parameters.Count;

    /// <summary>
    /// Adds or replaces a parameter value.
    /// </summary>
    /// <param name="parameterName">The name of the component parameter.</param>
    /// <param name="value">The value to assign.</param>
    public void Add(string parameterName, object? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(parameterName);
        _parameters[parameterName] = value;
    }

    /// <summary>
    /// Gets the value of a parameter, cast to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <param name="parameterName">The name of the component parameter.</param>
    /// <returns>The parameter value, or <c>default</c> if not present or not assignable to <typeparamref name="T"/>.</returns>
    public T? Get<T>(string parameterName) =>
        _parameters.TryGetValue(parameterName, out var value) && value is T typed ? typed : default;

    /// <summary>
    /// Attempts to get the value of a parameter, cast to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <param name="parameterName">The name of the component parameter.</param>
    /// <param name="value">The resulting value, if found and assignable.</param>
    /// <returns><c>true</c> if the parameter was found and assignable to <typeparamref name="T"/>; otherwise <c>false</c>.</returns>
    public bool TryGet<T>(string parameterName, out T? value)
    {
        if (_parameters.TryGetValue(parameterName, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _parameters.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
