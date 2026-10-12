// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Models;

/// <summary>
/// A value positioned at two categories, used by <see cref="TwBlazor.Components.TwCategoricalScatterChart"/>.
/// </summary>
/// <param name="X">The horizontal category.</param>
/// <param name="Y">The vertical category.</param>
/// <param name="Value">The value represented by the mark size.</param>
/// <param name="Label">An optional point label.</param>
public sealed record ChartCategoricalPoint(string X, string Y, double Value, string? Label = null);

/// <summary>
/// A node in a hierarchy, used by tree and sunburst charts.
/// </summary>
/// <param name="Label">The node label.</param>
/// <param name="Value">The value of a leaf node. Parent values are computed from their children.</param>
/// <param name="Children">The child nodes.</param>
public sealed record ChartHierarchyNode(string Label, double Value = 0, IReadOnlyList<ChartHierarchyNode>? Children = null);

/// <summary>
/// A node in a graph, flow, or diagram.
/// </summary>
/// <param name="Id">A unique identifier referenced by links.</param>
/// <param name="Label">The visible node label.</param>
/// <param name="Stage">The optional horizontal stage used by a Sankey diagram.</param>
public sealed record ChartGraphNode(string Id, string Label, int Stage = 0);

/// <summary>
/// A directed or undirected link between two graph nodes.
/// </summary>
/// <param name="Source">The source node identifier.</param>
/// <param name="Target">The target node identifier.</param>
/// <param name="Value">The link weight or flow.</param>
public sealed record ChartGraphLink(string Source, string Target, double Value = 1);

/// <summary>
/// A named set and its size, used in set-overlap diagrams.
/// </summary>
/// <param name="Label">The set label.</param>
/// <param name="Value">The number of elements in the set.</param>
public sealed record ChartSet(string Label, double Value);

/// <summary>
/// The size of the overlap between two sets in a Venn or Euler diagram.
/// </summary>
/// <param name="First">The first set label.</param>
/// <param name="Second">The second set label.</param>
/// <param name="Value">The number of shared elements.</param>
public sealed record ChartSetIntersection(string First, string Second, double Value);
