// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Enums;

/// <summary>
/// Defines the state a status marker reports, such as the progress of a tool call.
/// </summary>
public enum MarkerStatus
{
    /// <summary>
    /// No state. The marker shows its icon, if it has one, and its label.
    /// </summary>
    None,

    /// <summary>
    /// The step is waiting to start.
    /// </summary>
    Pending,

    /// <summary>
    /// The step is in progress. A spinner is shown.
    /// </summary>
    Running,

    /// <summary>
    /// The step finished successfully.
    /// </summary>
    Succeeded,

    /// <summary>
    /// The step failed.
    /// </summary>
    Failed
}
