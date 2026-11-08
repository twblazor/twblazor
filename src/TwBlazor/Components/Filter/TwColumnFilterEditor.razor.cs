// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using TwBlazor.Enums;
using TwBlazor.Filtering;
using TwBlazor.Localization;
using TwBlazor.Models;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

/// <summary>
/// Edits a <see cref="ColumnFilter"/>: two conditions, each a comparison and the value to compare with, joined by And
/// or Or. <see cref="TwDataTable{TItem}"/> and <see cref="TwSpreadsheet"/> show it in their filters, each with its own
/// buttons to apply or clear the filter.
/// </summary>
/// <remarks>
/// The filter is edited in place, so pass a copy and keep it only when the filter is applied. Use it to build your own
/// filter UI around the same conditions, such as in a dialog or a side panel, and test values against the filter with
/// your own code or an <see cref="TwDataTable{TItem}.ItemsProvider"/>.
/// </remarks>
public partial class TwColumnFilterEditor : TwBlazorComponentBase
{
    /// <summary>
    /// Gets or sets the filter to edit. A missing second condition is added, starting with the first comparison
    /// offered.
    /// </summary>
    [Parameter] public ColumnFilter Filter { get; set; } = new();

    /// <summary>
    /// Gets or sets the kind of values the column holds, which decides the comparisons offered and the field the value
    /// is typed into. <see cref="FilterType.Auto"/> is treated as text.
    /// </summary>
    [Parameter] public FilterType Type { get; set; } = FilterType.Text;

    /// <summary>
    /// Gets or sets the comparisons offered, in order, or <see langword="null"/> for the ones that suit
    /// <see cref="Type"/>.
    /// </summary>
    [Parameter] public IReadOnlyList<FilterOperator>? Operators { get; set; }

    /// <summary>
    /// Gets or sets the values offered for <see cref="FilterType.Boolean"/> and <see cref="FilterType.Enum"/>, each a
    /// value and the text shown for it. <see langword="null"/> offers Yes and No for true or false values.
    /// </summary>
    [Parameter] public IReadOnlyList<KeyValuePair<string, string>>? Choices { get; set; }

    /// <summary>
    /// Gets or sets what the ids of the fields start with, such as <c>{prefix}-first-value</c>. Defaults to
    /// <see cref="TwBlazorComponentBase.Id"/>.
    /// </summary>
    [Parameter] public string? FieldIdPrefix { get; set; }

    private string fieldPrefix => string.IsNullOrEmpty(FieldIdPrefix) ? Id ?? string.Empty : FieldIdPrefix;

    /// <summary>
    /// Gets or sets the callback invoked after any part of the filter changes.
    /// </summary>
    [Parameter] public EventCallback OnChange { get; set; }

    private List<OperatorOption> operatorOptions = [];

    private List<ChoiceOption> choiceOptions = [];

    /// <summary>
    /// A comparison the filter offers, with its localized name.
    /// </summary>
    private sealed record OperatorOption(FilterOperator Operator, string Text);

    /// <summary>
    /// A value a true or false, or list, filter offers.
    /// </summary>
    private sealed record ChoiceOption(string Value, string Text);

    private string stackClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Col)
        .AddClass(options.Theme.Spacing.Gap.Get(2)).Build();

    private string rootClasses => new ClassBuilder(stackClasses).AddClass(Class).Build();

    private string rowClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Spacing.Gap.Get(1)).Build();

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        Filter ??= new ColumnFilter();
        var operators = Operators is { Count: > 0 } chosen ? chosen : ColumnFilterEvaluator.OperatorsFor(Type);
        operatorOptions = [.. operators.Select(op => new OperatorOption(op, localizer[$"DataTable_Operator{op}"]))];
        Filter.Second ??= new FilterCondition { Operator = operators[0] };

        choiceOptions = Choices is not null
            ? [.. Choices.Select(c => new ChoiceOption(c.Key, c.Value))]
            : Type == FilterType.Boolean
                ? [new("true", localizer[TwStrings.DataTable_Yes]), new("false", localizer[TwStrings.DataTable_No])]
                : [];
    }

    private Dictionary<string, object> LogicAttributes(FilterLogic logic) => new()
    {
        ["aria-pressed"] = Filter.Logic == logic ? "true" : "false",
    };

    private Task SetLogicAsync(FilterLogic logic)
    {
        Filter.Logic = logic;
        return OnChange.InvokeAsync();
    }

    private Task SetOperatorAsync(FilterCondition condition, FilterOperator op)
    {
        condition.Operator = op;
        return OnChange.InvokeAsync();
    }

    private Task SetValueAsync(FilterCondition condition, string? value)
    {
        condition.Value = value;
        return OnChange.InvokeAsync();
    }
}
