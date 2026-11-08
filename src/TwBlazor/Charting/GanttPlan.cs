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
    public static GanttPlan Build(IReadOnlyList<ChartTask> tasks, IReadOnlySet<string> collapsed, bool rollUpSummaries) =>
        tasks.Count == 0 ? Empty : new Builder(tasks, collapsed, rollUpSummaries).CreatePlan();

    /// <summary>
    /// Gets the row on screen that stands for a task: its own, or that of the nearest summary above it that is
    /// on screen. Returns -1 for a key no task has.
    /// </summary>
    public int VisibleRowOf(string key) => _byKey.TryGetValue(key, out var index) ? _visibleRow[index] : -1;

    /// <summary>Gets the task with a key, or <see langword="null"/>.</summary>
    public GanttNode? Find(string key) => _byKey.TryGetValue(key, out var index) ? All[index] : null;

    /// <summary>
    /// Holds the working arrays while a plan is built: the tree, then the rolled-up dates, then the rows.
    /// </summary>
    private sealed class Builder
    {
        private readonly IReadOnlyList<ChartTask> _tasks;
        private readonly IReadOnlySet<string> _collapsed;
        private readonly bool _rollUpSummaries;
        private readonly List<int>[] _children;
        private readonly List<int> _roots = [];
        private readonly DateTime[] _starts;
        private readonly DateTime[] _ends;
        private readonly double?[] _progress;
        private readonly List<GanttNode> _all;
        private readonly List<GanttNode> _visible = [];
        private readonly Dictionary<string, int> _byKey = new(StringComparer.Ordinal);
        private readonly List<int> _visibleRow;

        public Builder(IReadOnlyList<ChartTask> tasks, IReadOnlySet<string> collapsed, bool rollUpSummaries)
        {
            _tasks = tasks;
            _collapsed = collapsed;
            _rollUpSummaries = rollUpSummaries;
            _children = [.. tasks.Select(_ => new List<int>())];
            _starts = new DateTime[tasks.Count];
            _ends = new DateTime[tasks.Count];
            _progress = new double?[tasks.Count];
            _all = new List<GanttNode>(tasks.Count);
            _visibleRow = new List<int>(tasks.Count);
        }

        public GanttPlan CreatePlan()
        {
            var parent = FindParents();
            for (var i = 0; i < _tasks.Count; i++)
            {
                (parent[i] < 0 ? _roots : _children[parent[i]]).Add(i);
            }

            foreach (var root in _roots)
            {
                RollUp(root);
            }

            foreach (var root in _roots)
            {
                Walk(root, 0, -1, true, -1);
            }

            return new GanttPlan(_all, _visible, _byKey, [.. _visibleRow]);
        }

        private int[] FindParents()
        {
            var firstByKey = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var i = 0; i < _tasks.Count; i++)
            {
                firstByKey.TryAdd(_tasks[i].Key, i);
            }

            var parent = new int[_tasks.Count];
            for (var i = 0; i < _tasks.Count; i++)
            {
                parent[i] = _tasks[i].ParentId is { } parentId && firstByKey.TryGetValue(parentId, out var p) && p != i ? p : -1;
            }

            BreakLoops(parent);
            return parent;
        }

        // Dates and progress, rolled up from the bottom of the tree.
        private void RollUp(int i)
        {
            var task = _tasks[i];
            var children = _children[i];
            foreach (var child in children)
            {
                RollUp(child);
            }

            var own = task.Progress is { } value ? Math.Clamp(value, 0, 1) : (double?)null;
            if (children.Count == 0 || !_rollUpSummaries)
            {
                _starts[i] = task.Start;
                _ends[i] = task.End < task.Start ? task.Start : task.End;
                _progress[i] = own;
                return;
            }

            _starts[i] = children.Min(child => _starts[child]);
            _ends[i] = children.Max(child => _ends[child]);
            _progress[i] = own ?? WeightedProgress(children, _starts, _ends, _progress);
        }

        // Tree order, with the rows that stay on screen.
        private void Walk(int i, int depth, int parentIndex, bool shown, int nearestVisible)
        {
            var task = _tasks[i];
            var isSummary = _children[i].Count > 0;
            var isExpanded = isSummary && !_collapsed.Contains(task.Key);
            var node = new GanttNode(task, _all.Count, depth, parentIndex, _starts[i], _ends[i], _progress[i], isSummary, !isSummary && _ends[i] <= _starts[i], isExpanded);
            _all.Add(node);
            _byKey.TryAdd(task.Key, node.Index);

            if (shown)
            {
                nearestVisible = _visible.Count;
                _visible.Add(node);
            }

            _visibleRow.Add(nearestVisible);
            foreach (var child in _children[i])
            {
                Walk(child, depth + 1, node.Index, shown && isExpanded, nearestVisible);
            }
        }

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

        /// <summary>
        /// Turns a task whose chain of parents leads back to itself into a top level task, so a loop of
        /// <see cref="ChartTask.ParentId"/>s cannot hang the walk down the tree.
        /// </summary>
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
}
