// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

namespace TwBlazor.Components;

/// <summary>
/// A row a person has finished editing in a <see cref="TwDataTable{TItem}"/>, passed to
/// <see cref="TwDataTable{TItem}.RowSaved"/>.
/// </summary>
/// <typeparam name="TItem">The type of items displayed in the table.</typeparam>
public sealed class TwDataTableRowEdit<TItem>
{
    internal TwDataTableRowEdit(TItem item, IReadOnlyDictionary<string, object?> originalValues, IReadOnlyDictionary<string, object?> values)
    {
        Item = item;
        OriginalValues = originalValues;
        Values = values;
    }

    /// <summary>
    /// Gets the edited row. Columns with a <see cref="TwDataTableColumn{TItem}.ValueSetter"/> have already written
    /// their new values to it.
    /// </summary>
    public TItem Item { get; }

    /// <summary>
    /// Gets each editable column's value from before the edit, keyed by <see cref="TwDataTableColumn{TItem}.Name"/>.
    /// Use it to undo the change if saving it elsewhere fails.
    /// </summary>
    public IReadOnlyDictionary<string, object?> OriginalValues { get; }

    /// <summary>
    /// Gets each editable column's new value, keyed by <see cref="TwDataTableColumn{TItem}.Name"/>.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Values { get; }

    /// <summary>
    /// Gets the names of the columns whose value changed.
    /// </summary>
    public IEnumerable<string> ChangedColumns => Values.Keys.Where(name => !Equals(Values[name], OriginalValues.GetValueOrDefault(name)));
}

/// <summary>
/// The cell being edited, the context of <see cref="TwDataTableColumn{TItem}.EditTemplate"/>. Render an editor that
/// shows <see cref="Value"/>, calls <see cref="SetValue"/> when it changes, and uses <see cref="Id"/>,
/// <see cref="Label"/> and <see cref="ErrorMessage"/> so it can be focused, is named and reports problems.
/// </summary>
/// <typeparam name="TItem">The type of items displayed in the table.</typeparam>
public sealed class TwDataTableCellEditContext<TItem>
{
    private readonly Action<object?> _setValue;

    internal TwDataTableCellEditContext(TItem item, TwDataTableColumn<TItem> column, object? value, string id, string? errorMessage, Action<object?> setValue)
    {
        Item = item;
        Column = column;
        Value = value;
        Id = id;
        ErrorMessage = errorMessage;
        _setValue = setValue;
    }

    /// <summary>
    /// Gets the row being edited, unchanged until the edit is saved.
    /// </summary>
    public TItem Item { get; }

    /// <summary>
    /// Gets the column being edited.
    /// </summary>
    public TwDataTableColumn<TItem> Column { get; }

    /// <summary>
    /// Gets the value the editor should show.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Gets the id to give the editor's input, so the table can focus it when editing starts or the value needs
    /// fixing.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the accessible name for the editor: the column's title.
    /// </summary>
    public string Label => Column.Title;

    /// <summary>
    /// Gets the message from the column's <see cref="TwDataTableColumn{TItem}.Validator"/>, or <c>null</c> when the
    /// value is fine.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Gets whether the value needs fixing before the row can be saved.
    /// </summary>
    public bool Invalid => ErrorMessage is not null;

    /// <summary>
    /// Records a new value for the cell. It is written to the row when the edit is saved.
    /// </summary>
    /// <param name="value">The new value.</param>
    public void SetValue(object? value) => _setValue(value);
}
