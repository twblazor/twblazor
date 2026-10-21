// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components.Forms;

namespace TwBlazor.Abstraction;

/// <summary>
/// A field that a <see cref="Components.TwForm"/> validates when it is submitted.
/// </summary>
internal interface ITwFormField
{
    /// <summary>
    /// Gets the id of the field's focusable control, which the form's error summary links to.
    /// </summary>
    string? Id { get; }

    /// <summary>
    /// Validates the field, running any validator of its own.
    /// </summary>
    /// <returns><see langword="true"/> when the field is valid.</returns>
    Task<bool> ValidateAsync();

    /// <summary>
    /// Gets the model property the field is bound to, or <see langword="null"/> when it is not bound to one.
    /// </summary>
    FieldIdentifier? BoundField { get; }

    /// <summary>
    /// Gets the field's own error message while it is invalid for a reason the form does not know about, such as
    /// a failed validator, or <see langword="null"/>.
    /// </summary>
    string? OwnErrorMessage { get; }
}
