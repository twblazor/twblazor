// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Enums;

namespace TwBlazor.Services.Encoding;

/// <summary>
/// Settings that control how data is encoded into a <see cref="QrMatrix"/>.
/// </summary>
public sealed record QrEncodeOptions
{
    /// <summary>
    /// Gets the shared default options: <see cref="QrErrorCorrection.Medium"/> with boosting on.
    /// </summary>
    public static QrEncodeOptions Default { get; } = new();

    /// <summary>
    /// Gets the minimum error correction level. Defaults to <see cref="QrErrorCorrection.Medium"/>.
    /// </summary>
    public QrErrorCorrection ErrorCorrection { get; init; } = QrErrorCorrection.Medium;

    /// <summary>
    /// Gets whether to raise the error correction level when that does not make the code any larger.
    /// Defaults to <see langword="true"/>: the extra robustness is free.
    /// </summary>
    public bool BoostErrorCorrection { get; init; } = true;

    /// <summary>
    /// Gets the smallest version to use, from 1 to 40. Defaults to 1. Raise it to keep codes the same
    /// size while their content changes.
    /// </summary>
    public int MinVersion { get; init; } = QrTables.minVersion;

    /// <summary>
    /// Gets the largest version to use, from 1 to 40. Defaults to 40.
    /// </summary>
    public int MaxVersion { get; init; } = QrTables.maxVersion;

    /// <summary>
    /// Gets the mask pattern to apply, from 0 to 7, or <see langword="null"/> (the default) to pick the one
    /// that is easiest to scan.
    /// </summary>
    public int? Mask { get; init; }
}
