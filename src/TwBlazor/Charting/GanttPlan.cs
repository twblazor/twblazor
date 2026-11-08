// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using TwBlazor.Models;

namespace TwBlazor.Charting;

/// <summary>
/// One task of a Gantt plan, placed in its tree, with the dates and progress it is drawn with.
/// </summary>
/// <param name="Task">The task as given.</param>
/// <param name="Index">The position of the task in tree order.</param>
/// <param name="Depth">How many summary tasks it sits under.</param>
/// <param name="Parent">The tree-order position of its summary task, or -1 for a top-level task.</param>
/// <param name="Start">When it starts; for a summary, the earliest start under it.</param>
/// <param name="End">When it ends; for a summary, the latest end under it.</param>
/// <param name="Progress">How far along it is, from 0 to 1, or <see langword="null"/> when unknown.</param>
/// <param name="IsSummary">Whether other tasks sit under it.</param>
/// <param name="IsMilestone">Whether it has no length.</param>
/// <param name="IsExpanded">Whether the tasks under it are shown.</param>
internal sealed record GanttNode(ChartTask Task, int Index, int Depth, int Parent, DateTime Start, DateTime End, double? Progress, bool IsSummary, bool IsMilestone, bool IsExpanded)
{
    public string Key => Task.Key;
}

/// <summary>
/// Turns a flat list of tasks into the tree a Gantt chart draws: summaries rolled up from the tasks under them,
/// and the rows left on screen once collapsed summaries hide theirs.
/// </summary>
internal sealed class GanttPlan
{
    private readonly Dictionary<string, int> _byKey;
    private readonly int[] _visibleRow;

    private GanttPlan(IReadOnlyList<GanttNode> all, IReadOnlyList<GanttNode> visible, Dictionary<string, int> byKey, int[] visibleRow)
    {
        All = all;
        Visible = visible;
        _byKey = byKey;
        _visibleRow = visibleRow;
    }

    /// <summary>Gets an empty plan.</summary>
    public static GanttPlan Empty { get; } = new([], [], [], []);

    /// <summary>Gets every task, in tree order: each summary followed by the tasks under it.</summary>
    public IReadOnlyList<GanttNode> All { get; }

    /// <summary>Gets the tasks on screen, in tree order.</summary>
    public IReadOnlyList<GanttNode> Visible { get; }

    /// <summary>Gets whether any task sits under another.</summary>
    public bool HasTree => All.Any(node => node.IsSummary);

    /// <summary>Gets the keys of every summary task.</summary>
    public IEnumerable<string> SummaryKeys => All.Where(node => node.IsSummary).Select(node => node.Key);

    /// <summary>
    /// Builds a plan. A parent that is missing, or that would make a loop, leaves its task at the top level.
    /// When two tasks share a key, the first one is the one others refer to.
    /// </summary>
    public static GanttPlan Build(IReadOnlyList<ChartTask> tasks, IReadOnlySet<string> collapsed, bool rollUpSummaries)
    {
        if (tasks.Count == 0)
        {
            return Empty;
        }

        var firstByKey = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < tasks.Count; i++)
        {
            firstByKey.TryAdd(tasks[i].Key, i);
        }

        var parent = new int[tasks.Count];
        for (var i = 0; i < tasks.Count; i++)
        {
            parent[i] = tasks[i].ParentId is { } parentId && firstByKey.TryGetValue(parentId, out var p) && p != i ? p : -1;
        }

        BreakLoops(parent);

        var children = new List<int>[tasks.Count];
        var roots = new List<int>();
        for (var i = 0; i < tasks.Count; i++)
        {
            children[i] = [];
        }

        for (var i = 0; i < tasks.Count; i++)
        {
            (parent[i] < 0 ? roots : children[parent[i]]).Add(i);
        }

        // Dates and progress, rolled up from the bottom of the tree.
        var starts = new DateTime[tasks.Count];
        var ends = new DateTime[tasks.Count];
        var progress = new double?[tasks.Count];
        foreach (var root in roots)
        {
            RollUp(root);
        }

        void RollUp(int i)
        {
            var task = tasks[i];
            foreach (var child in children[i])
            {
                RollUp(child);
            }

            if (children[i].Count == 0 || !rollUpSummaries)
            {
                starts[i] = task.Start;
                ends[i] = task.End < task.Start ? task.Start : task.End;
                progress[i] = task.Progress is { } own ? Math.Clamp(own, 0, 1) : null;
            }
            else
            {
                starts[i] = children[i].Min(child => starts[child]);
                ends[i] = children[i].Max(child => ends[child]);
                progress[i] = task.Progress is { } own ? Math.Clamp(own, 0, 1) : WeightedProgress(children[i], starts, ends, progress);
            }
        }

        // Tree order, with the rows that stay on screen.
        var all = new List<GanttNode>(tasks.Count);
        var visible = new List<GanttNode>();
        var byKey = new Dictionary<string, int>(StringComparer.Ordinal);
        var visibleRow = new List<int>(tasks.Count);
        foreach (var root in roots)
        {
            Walk(root, 0, -1, true, -1);
        }

        void Walk(int i, int depth, int parentIndex, bool shown, int nearestVisible)
        {
            var task = tasks[i];
            var isSummary = children[i].Count > 0;
            var isExpanded = isSummary && !collapsed.Contains(task.Key);
            var node = new GanttNode(task, all.Count, depth, parentIndex, starts[i], ends[i], progress[i], isSummary, !isSummary && ends[i] <= starts[i], isExpanded);
            all.Add(node);
            byKey.TryAdd(task.Key, node.Index);

            if (shown)
            {
                nearestVisible = visible.Count;
                visible.Add(node);
            }

            visibleRow.Add(nearestVisible);
            foreach (var child in children[i])
            {
                Walk(child, depth + 1, node.Index, shown && isExpanded, nearestVisible);
            }
        }

        return new GanttPlan(all, visible, byKey, [.. visibleRow]);
    }

    /// <summary>
    /// Gets the row on screen that stands for a task: its own, or that of the nearest summary above it that is
    /// on screen. Returns -1 for a key no task has.
    /// </summary>
    public int VisibleRowOf(string key) => _byKey.TryGetValue(key, out var index) ? _visibleRow[index] : -1;

    /// <summary>Gets the task with a key, or <see langword="null"/>.</summary>
    public GanttNode? Find(string key) => _byKey.TryGetValue(key, out var index) ? All[index] : null;

    /// <summary>
    /// Progress weighted by duration, so a long task counts for more than a short one. Tasks without progress
    /// count as not started; if none has any, the summary has none either. Children with no length (all
    /// milestones) count equally.
    /// </summary>
    private static double? WeightedProgress(List<int> children, DateTime[] starts, DateTime[] ends, double?[] progress)
    {
        if (children.All(child => progress[child] is null))
        {
            return null;
        }

        var total = children.Sum(child => (ends[child] - starts[child]).TotalHours);
        if (total <= 0)
        {
            return children.Average(child => progress[child] ?? 0);
        }

        return children.Sum(child => (progress[child] ?? 0) * (ends[child] - starts[child]).TotalHours) / total;
    }

    private static void BreakLoops(int[] parent)
    {
        for (var i = 0; i < parent.Length; i++)
        {
            var steps = 0;
            for (var current = parent[i]; current >= 0; current = parent[current])
            {
                if (current == i || ++steps > parent.Length)
                {
                    parent[i] = -1;
                    break;
                }
            }
        }
    }
}
