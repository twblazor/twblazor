// Copyright (c) 2025 Jack Shuter @ TwBlazor - twblazor.com
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TwBlazor.Localization;
using TwBlazor.Utilities;

namespace TwBlazor.Components;

public partial class TwDataTable<TItem>
{
    private bool isEditing;
    private TItem? editingItem;
    private Dictionary<string, object?> editValues = [];
    private Dictionary<string, object?> originalEditValues = [];
    private Dictionary<string, string> editErrors = [];
    private ElementReference editingRowRef;
    private bool escapeRegistered;
    private bool registerEscapePending;

    /// <summary>
    /// Whether rows can be edited in place. Adds an Actions column with an Edit button on each row, which turns the
    /// row's <see cref="TwDataTableColumn{TItem}.Editable"/> cells into editors with Save and Cancel buttons. Only
    /// applies when <see cref="Columns"/> is used. Defaults to <c>false</c>.
    /// </summary>
    [Parameter]
    public bool Editable { get; set; }

    /// <summary>
    /// Gets or sets which rows can be edited, or <c>null</c> for every row. Rows it returns <c>false</c> for get no
    /// Edit button.
    /// </summary>
    [Parameter]
    public Func<TItem, bool>? CanEditRow { get; set; }

    /// <summary>
    /// Raised after a person saves an edited row. Every column with a <see cref="TwDataTableColumn{TItem}.ValueSetter"/>
    /// has already written its value to the row, so this is the place to save the row to a database or API.
    /// </summary>
    [Parameter]
    public EventCallback<TwDataTableRowEdit<TItem>> RowSaved { get; set; }

    /// <summary>
    /// Raised when a person cancels editing a row. The row is left as it was.
    /// </summary>
    [Parameter]
    public EventCallback<TItem> RowEditCancelled { get; set; }

    /// <summary>
    /// Gets or sets how a row is named in the Edit, Save and Cancel buttons' accessible names, such as "Edit Kettle".
    /// Defaults to the text of the first visible column.
    /// </summary>
    [Parameter]
    public Func<TItem, string>? RowLabel { get; set; }

    private bool showActionsColumn => Editable && hasColumns;

    private int renderedColumnCount => visibleColumns.Count + (showActionsColumn ? 1 : 0);

    private string actionButtonsClasses => new ClassBuilder(options.Theme.Display.Flex)
        .AddClass(options.Theme.Flexbox.Align.Center)
        .AddClass(options.Theme.Spacing.Gap.Sm).Build();

    private bool IsEditingRow(TItem row) => isEditing && EqualityComparer<TItem>.Default.Equals(row, editingItem);

    private bool CanEdit(TItem row) => CanEditRow?.Invoke(row) ?? true;

    private bool IsEditableColumn(TwDataTableColumn<TItem> column) => column.Editable && column.PropertySelector is not null;

    private string LabelFor(TItem row)
    {
        if (RowLabel is { } label)
        {
            return label(row);
        }

        var first = visibleColumns.FirstOrDefault();
        return first switch
        {
            null => string.Empty,
            { CellFormatter: { } formatter } => formatter(row),
            { PropertySelector: { } selector } => selector(row)?.ToString() ?? string.Empty,
            _ => string.Empty
        };
    }

    // Rows are found again after a save by their place among the rows the table holds, so the id stays the same
    // while the row is on screen even when the page re-renders.
    private string EditButtonId(TItem row)
    {
        var rows = usesProvider ? loadedItems : viewItems;
        var index = rows.FindIndex(r => EqualityComparer<TItem>.Default.Equals(r, row));
        return $"{Id}-edit-row-{index}";
    }

    private string EditorId(TwDataTableColumn<TItem> column) => $"{Id}-edit-{column.Name}";

    private string? EditError(TwDataTableColumn<TItem> column) => editErrors.GetValueOrDefault(column.Name);

    private object? EditValue(TwDataTableColumn<TItem> column) => editValues.GetValueOrDefault(column.Name);

    private void SetEditValue(TwDataTableColumn<TItem> column, object? value)
    {
        editValues[column.Name] = value;
        editErrors.Remove(column.Name);
    }

    private TwDataTableCellEditContext<TItem> EditContextFor(TwDataTableColumn<TItem> column) =>
        new(editingItem!, column, EditValue(column), EditorId(column), EditError(column), value =>
        {
            SetEditValue(column, value);
            StateHasChanged();
        });

    /// <summary>
    /// Picks the editor for a column from the type of its values. A row whose value is empty borrows the type from
    /// another row, so a nullable number still gets a number field.
    /// </summary>
    private Type? EditValueType(TwDataTableColumn<TItem> column)
    {
        if (originalEditValues.GetValueOrDefault(column.Name) is { } value)
        {
            return value.GetType();
        }

        var rows = usesProvider ? loadedItems : viewItems;
        return rows.Select(r => column.PropertySelector!(r)).FirstOrDefault(v => v is not null)?.GetType();
    }

    private void StartEdit(TItem row)
    {
        if (!showActionsColumn || isEditing || !CanEdit(row))
        {
            return;
        }

        isEditing = true;
        editingItem = row;
        editErrors = [];
        originalEditValues = visibleColumns
            .Where(IsEditableColumn)
            .ToDictionary(c => c.Name, c => c.PropertySelector!(row));
        editValues = new Dictionary<string, object?>(originalEditValues);

        var firstEditor = visibleColumns.FirstOrDefault(IsEditableColumn);
        pendingFocusId = firstEditor is null ? saveButtonId : EditorId(firstEditor);
        registerEscapePending = true;
        sortStatusMessage = localizer[TwStrings.DataTable_Editing, LabelFor(row)];
    }

    private string saveButtonId => $"{Id}-save-row";

    private string cancelButtonId => $"{Id}-cancel-row";

    private async Task SaveEditAsync()
    {
        if (!isEditing)
        {
            return;
        }

        var row = editingItem!;
        editErrors = [];
        foreach (var column in visibleColumns.Where(c => IsEditableColumn(c) && c.Validator is not null && editValues.ContainsKey(c.Name)))
        {
            if (column.Validator!(row, editValues[column.Name]) is { } message)
            {
                editErrors[column.Name] = message;
            }
        }

        if (editErrors.Count > 0)
        {
            pendingFocusId = EditorId(visibleColumns.First(c => editErrors.ContainsKey(c.Name)));
            sortStatusMessage = localizer.Plural(editErrors.Count, TwStrings.DataTable_EditInvalidOne, TwStrings.DataTable_EditInvalidOther, editErrors.Count);
            return;
        }

        foreach (var column in visibleColumns.Where(c => IsEditableColumn(c) && c.ValueSetter is not null && editValues.ContainsKey(c.Name)))
        {
            column.ValueSetter!(row, editValues[column.Name]);
        }

        var edit = new TwDataTableRowEdit<TItem>(row, originalEditValues, new Dictionary<string, object?>(editValues));
        await EndEditAsync();
        RefreshView();

        pendingFocusId = EditButtonId(row);
        sortStatusMessage = localizer[TwStrings.DataTable_RowSaved, LabelFor(row)];
        await RowSaved.InvokeAsync(edit);
    }

    private async Task CancelEditAsync()
    {
        if (!isEditing)
        {
            return;
        }

        var row = editingItem!;
        await EndEditAsync();

        pendingFocusId = EditButtonId(row);
        sortStatusMessage = localizer[TwStrings.DataTable_EditCancelled, LabelFor(row)];
        await RowEditCancelled.InvokeAsync(row);
    }

    /// <summary>
    /// Cancels the edit when a person presses Escape. The row shares the library's Escape order with dialogs and
    /// pickers, so a date picker or list opened from the row closes first and the next press cancels the edit.
    /// </summary>
    /// <returns>A task that completes once the edit is cancelled.</returns>
    [JSInvokable("CloseFromEscape")]
    public async Task CancelEditFromEscapeAsync()
    {
        await InvokeAsync(async () =>
        {
            await CancelEditAsync();
            StateHasChanged();
        });
    }

    private async Task EndEditAsync()
    {
        isEditing = false;
        editingItem = default;
        editValues = [];
        originalEditValues = [];
        editErrors = [];
        registerEscapePending = false;

        if (escapeRegistered)
        {
            escapeRegistered = false;
            await InvokeScriptAsync("twDialog.unregisterEscape", editingRowRef);
        }
    }

    private async Task RegisterEditEscapeAsync()
    {
        if (!registerEscapePending || !isEditing)
        {
            return;
        }

        registerEscapePending = false;
        escapeRegistered = true;
        selfReference ??= DotNetObjectReference.Create(this);
        await InvokeScriptAsync("twDialog.registerEscape", editingRowRef, selfReference);
    }
}
