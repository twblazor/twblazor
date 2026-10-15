// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Services.Encoding;

/// <summary>
/// Builds the row of modules for a barcode from bar and space widths or from bit patterns.
/// </summary>
internal sealed class BarcodeModuleWriter
{
    private readonly List<bool> _modules;

    /// <summary>
    /// Initializes a new instance of the <see cref="BarcodeModuleWriter"/> class.
    /// </summary>
    /// <param name="capacity">The expected number of modules.</param>
    internal BarcodeModuleWriter(int capacity = 128)
    {
        _modules = new List<bool>(capacity);
    }

    /// <summary>
    /// Appends alternating bars and spaces, starting with a bar, from a string of widths such as <c>"212222"</c>.
    /// </summary>
    /// <param name="widths">The width of each element in modules, one digit per element.</param>
    internal void AppendWidths(string widths)
    {
        var dark = true;
        foreach (var width in widths)
        {
            for (var i = 0; i < width - '0'; i++)
            {
                _modules.Add(dark);
            }

            dark = !dark;
        }
    }

    /// <summary>
    /// Appends modules from a bit pattern such as <c>"0001101"</c>, where <c>1</c> is dark.
    /// </summary>
    /// <param name="bits">The pattern.</param>
    internal void AppendBits(string bits)
    {
        foreach (var bit in bits)
        {
            _modules.Add(bit == '1');
        }
    }

    /// <summary>
    /// Copies the modules written so far.
    /// </summary>
    /// <returns>The modules, <see langword="true"/> for dark.</returns>
    internal bool[] ToArray() => [.. _modules];
}
