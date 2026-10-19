// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

// Design and API shape inspired by MudBlazor's DialogResult
// (https://github.com/MudBlazor/MudBlazor/tree/dev/src/MudBlazor/Services/Dialog), MIT License.

namespace TwBlazor.Models;

/// <summary>
/// The result produced when a dialog shown via <see cref="Services.ITwDialogService"/> closes.
/// </summary>
public class TwDialogResult
{
    /// <summary>
    /// Gets the data returned by the dialog, if any.
    /// </summary>
    public object? Data { get; }

    /// <summary>
    /// Gets the runtime type of <see cref="Data"/>.
    /// </summary>
    public Type? DataType { get; }

    /// <summary>
    /// Gets whether the dialog was dismissed via cancellation (backdrop click, Escape key, close button, or an explicit cancel).
    /// </summary>
    public bool Canceled { get; }

    private TwDialogResult(object? data, Type? dataType, bool canceled)
    {
        Data = data;
        DataType = dataType;
        Canceled = canceled;
    }

    /// <summary>
    /// Creates a successful result carrying no data.
    /// </summary>
    public static TwDialogResult Ok() => Ok<object?>(null);

    /// <summary>
    /// Creates a successful result carrying the specified data.
    /// </summary>
    /// <typeparam name="T">The type of the returned data.</typeparam>
    /// <param name="result">The data to return.</param>
    public static TwDialogResult Ok<T>(T result) => Ok(result, typeof(T));

    /// <summary>
    /// Creates a successful result carrying the specified data and originating dialog type.
    /// </summary>
    /// <typeparam name="T">The type of the returned data.</typeparam>
    /// <param name="result">The data to return.</param>
    /// <param name="dialogType">The type of the dialog component that produced this result.</param>
    public static TwDialogResult Ok<T>(T result, Type? dialogType) => new(result, dialogType, false);

    /// <summary>
    /// Creates a cancelled result.
    /// </summary>
    public static TwDialogResult Cancel() => new(null, null, true);
}
