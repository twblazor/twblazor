// @vitest-environment jsdom

import { afterEach, describe, expect, test, vi } from 'vitest';

await import('../../../src/TwBlazor/wwwroot/js/twblazor.js');

const sheets = globalThis.twSpreadsheet;

// The markup TwSpreadsheet renders: a root holding the formula bar and a grid table with a heading row and two rows of
// two cells.
function build({ readonly = false } = {}) {
    const root = document.createElement('div');
    root.setAttribute('data-tw-spreadsheet', '');
    root.innerHTML = `
        <input id="bar" data-tw-spreadsheet-editor="bar" />
        <table id="sh" tabindex="0" role="grid" ${readonly ? 'data-readonly="true"' : ''}>
            <thead><tr><th id="corner"></th><th id="colA">A</th><th id="colB">B</th></tr></thead>
            <tbody>
                <tr><th id="row1">1</th><td id="sh-A1"></td><td id="sh-B1"></td></tr>
                <tr><th id="row2">2</th><td id="sh-A2"></td><td id="sh-B2"><input id="editor" data-tw-spreadsheet-editor="cell" /></td></tr>
            </tbody>
        </table>`;
    document.body.appendChild(root);
    const grid = root.querySelector('#sh');
    const dotnetRef = { invokeMethodAsync: vi.fn(() => Promise.resolve()) };
    sheets.attach(grid, dotnetRef);
    return { root, grid, dotnetRef };
}

function keydown(target, key, props = {}) {
    const event = new KeyboardEvent('keydown', { key, bubbles: true, cancelable: true, ...props });
    target.dispatchEvent(event);
    return event;
}

function mouse(target, type, props = {}) {
    const event = new MouseEvent(type, { bubbles: true, cancelable: true, button: 0, buttons: 1, ...props });
    target.dispatchEvent(event);
    return event;
}

function calls(dotnetRef, method) {
    return dotnetRef.invokeMethodAsync.mock.calls.filter(c => c[0] === method).map(c => c.slice(1));
}

afterEach(() => {
    document.querySelectorAll('[data-tw-spreadsheet]').forEach(root => {
        sheets.detach(root.querySelector('#sh'));
        root.remove();
    });
    vi.restoreAllMocks();
});

describe('twSpreadsheet keys in the grid', () => {
    test.each(['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Home', 'End', 'PageUp', 'PageDown', 'Enter', 'F2', 'Delete', 'Backspace', 'Escape'])('stops the default of %s', key => {
        const { grid } = build();

        expect(keydown(grid, key).defaultPrevented).toBe(true);
    });

    test('leaves Tab alone, so it leaves the grid', () => {
        const { grid } = build();

        expect(keydown(grid, 'Tab').defaultPrevented).toBe(false);
    });

    test('stops the default of the shortcuts but not of paste', () => {
        const { grid } = build();

        expect(keydown(grid, 'b', { ctrlKey: true }).defaultPrevented).toBe(true);
        expect(keydown(grid, 'A', { metaKey: true }).defaultPrevented).toBe(true);
        expect(keydown(grid, 'v', { ctrlKey: true }).defaultPrevented).toBe(false);
        expect(keydown(grid, ' ', { shiftKey: true }).defaultPrevented).toBe(true);
    });

    test('collects typed characters and asks .NET to start editing once', () => {
        const { grid, dotnetRef } = build();

        expect(keydown(grid, '4').defaultPrevented).toBe(true);
        keydown(grid, '2');

        expect(calls(dotnetRef, 'BeginTyping')).toHaveLength(1);
        expect(grid.__twSpreadsheet.buffer).toBe('42');
    });

    test('does not start editing in a read-only grid, with Alt, or during IME composition', () => {
        const { grid, dotnetRef } = build({ readonly: true });

        keydown(grid, 'x');
        expect(calls(dotnetRef, 'BeginTyping')).toHaveLength(0);

        grid.removeAttribute('data-readonly');
        keydown(grid, 'x', { altKey: true });
        keydown(grid, 'x', { isComposing: true });
        expect(calls(dotnetRef, 'BeginTyping')).toHaveLength(0);
    });
});

describe('twSpreadsheet keys in an editor', () => {
    test.each(['Enter', 'Escape', 'Tab', 'F2'])('stops the default of %s', key => {
        const { root } = build();

        expect(keydown(root.querySelector('#editor'), key).defaultPrevented).toBe(true);
        expect(keydown(root.querySelector('#bar'), key).defaultPrevented).toBe(true);
    });

    test('stops the arrow keys only in a cell started by typing', () => {
        const { root, grid } = build();
        const editor = root.querySelector('#editor');

        expect(keydown(editor, 'ArrowLeft').defaultPrevented).toBe(false);
        grid.dataset.editing = 'enter';
        expect(keydown(editor, 'ArrowLeft').defaultPrevented).toBe(true);
        expect(keydown(root.querySelector('#bar'), 'ArrowLeft').defaultPrevented).toBe(false);
    });

    test('leaves Tab alone while renaming a sheet', () => {
        const { root } = build();
        const rename = document.createElement('input');
        rename.dataset.twSpreadsheetEditor = 'rename';
        root.appendChild(rename);

        expect(keydown(rename, 'Tab').defaultPrevented).toBe(false);
        expect(keydown(rename, 'Enter').defaultPrevented).toBe(true);
    });
});

describe('twSpreadsheet focusEditor', () => {
    test('puts the typed characters in, reports them, and places the caret at the end', () => {
        const { root, grid } = build();
        const editor = root.querySelector('#editor');
        const input = vi.fn();
        editor.addEventListener('input', input);
        keydown(grid, '1');
        keydown(grid, '2');

        sheets.focusEditor(grid, 'editor', true);

        expect(editor.value).toBe('12');
        expect(input).toHaveBeenCalledOnce();
        expect(document.activeElement).toBe(editor);
        expect(editor.selectionStart).toBe(2);
        expect(grid.__twSpreadsheet.pending).toBe(false);
        expect(grid.__twSpreadsheet.buffer).toBe('');
    });

    test('keeps the contents when the edit was not started by typing', () => {
        const { root, grid } = build();
        const editor = root.querySelector('#editor');
        editor.value = '=SUM(';

        sheets.focusEditor(grid, 'editor', false);

        expect(editor.value).toBe('=SUM(');
        expect(editor.selectionStart).toBe(5);
    });

    test('drops the typed characters when the field is gone', () => {
        const { grid } = build();
        keydown(grid, '1');

        sheets.focusEditor(grid, 'missing', true);

        expect(grid.__twSpreadsheet.buffer).toBe('');
        expect(grid.__twSpreadsheet.pending).toBe(false);
    });
});

describe('twSpreadsheet pointer', () => {
    test('a press on a cell focuses the grid and reports its row and column', () => {
        const { root, grid, dotnetRef } = build();

        const event = mouse(root.querySelector('#sh-B2'), 'mousedown', { shiftKey: true });

        expect(event.defaultPrevented).toBe(true);
        expect(document.activeElement).toBe(grid);
        expect(calls(dotnetRef, 'PointerDown')).toEqual([[1, 1, true]]);
    });

    test('presses on headings report -1 for the heading', () => {
        const { root, dotnetRef } = build();

        mouse(root.querySelector('#colB'), 'mousedown');
        mouse(root.querySelector('#row2'), 'mousedown');
        mouse(root.querySelector('#corner'), 'mousedown');

        expect(calls(dotnetRef, 'PointerDown')).toEqual([[-1, 1, false], [1, -1, false], [-1, -1, false]]);
    });

    test('keeps focus where it is while a formula is pointing at cells', () => {
        const { root, grid } = build();
        const bar = root.querySelector('#bar');
        bar.focus();
        grid.dataset.pointing = 'true';

        mouse(root.querySelector('#sh-A1'), 'mousedown');

        expect(document.activeElement).toBe(bar);
    });

    test('ignores presses in a cell editor and with other buttons', () => {
        const { root, dotnetRef } = build();

        mouse(root.querySelector('#editor'), 'mousedown');
        mouse(root.querySelector('#sh-A1'), 'mousedown', { button: 2 });

        expect(calls(dotnetRef, 'PointerDown')).toHaveLength(0);
    });

    test('dragging reports each new cell once, and stops when the button is up', () => {
        const { root, dotnetRef } = build();
        const b1 = root.querySelector('#sh-B1');
        document.elementFromPoint = vi.fn(() => b1);

        mouse(root.querySelector('#sh-A1'), 'mousedown');
        mouse(document, 'mousemove');
        mouse(document, 'mousemove');
        expect(calls(dotnetRef, 'PointerExtend')).toEqual([[0, 1]]);

        document.elementFromPoint = vi.fn(() => root.querySelector('#sh-A2'));
        mouse(document, 'mousemove', { buttons: 0 });
        mouse(document, 'mousemove');
        expect(calls(dotnetRef, 'PointerExtend')).toHaveLength(1);
    });

    test('mouseup ends a drag', () => {
        const { root, dotnetRef } = build();
        document.elementFromPoint = vi.fn(() => root.querySelector('#sh-B2'));

        mouse(root.querySelector('#sh-A1'), 'mousedown');
        mouse(document, 'mouseup');
        mouse(document, 'mousemove');

        expect(calls(dotnetRef, 'PointerExtend')).toHaveLength(0);
    });

    test('a double-click on a cell edits it, but not on a heading', () => {
        const { root, dotnetRef } = build();

        mouse(root.querySelector('#sh-A2'), 'dblclick');
        mouse(root.querySelector('#colA'), 'dblclick');

        expect(calls(dotnetRef, 'PointerDoubleClick')).toEqual([[1, 0]]);
    });
});

describe('twSpreadsheet clipboard and focus', () => {
    test('a paste into the grid hands the text to .NET', () => {
        const { grid, dotnetRef } = build();
        const event = new Event('paste', { bubbles: true, cancelable: true });
        event.clipboardData = { getData: vi.fn(() => 'a\tb') };

        grid.dispatchEvent(event);

        expect(event.defaultPrevented).toBe(true);
        expect(calls(dotnetRef, 'PasteText')).toEqual([['a\tb']]);
    });

    test('copyText writes to the clipboard and ignores a refusal', async () => {
        const writeText = vi.fn(() => Promise.reject(new Error('denied')));
        Object.defineProperty(navigator, 'clipboard', { value: { writeText }, configurable: true });

        await expect(sheets.copyText('x')).resolves.toBeUndefined();
        expect(writeText).toHaveBeenCalledWith('x');
    });

    test('focus and scrollToCell work, and ignore what is missing', () => {
        const { root, grid } = build();
        const cell = root.querySelector('#sh-A2');
        cell.scrollIntoView = vi.fn();

        sheets.focus(grid);
        sheets.scrollToCell('sh-A2');
        sheets.scrollToCell('nope');
        sheets.focus(null);

        expect(document.activeElement).toBe(grid);
        expect(cell.scrollIntoView).toHaveBeenCalledWith({ block: 'nearest', inline: 'nearest' });
    });

    test('attach only once, and detach removes the listeners', () => {
        const { root, grid, dotnetRef } = build();
        sheets.attach(grid, dotnetRef);

        sheets.detach(grid);
        sheets.detach(grid);
        mouse(root.querySelector('#sh-A1'), 'mousedown');

        expect(calls(dotnetRef, 'PointerDown')).toHaveLength(0);
        expect(grid.__twSpreadsheet).toBeUndefined();
    });
});

describe('twSpreadsheet right-click, resizing and full screen', () => {
    test('a right-click reports the cell so it can be selected before the menu opens', () => {
        const { root, dotnetRef } = build();

        mouse(root.querySelector('#sh-B1'), 'mousedown', { button: 2, buttons: 2 });

        expect(calls(dotnetRef, 'PointerContext')).toEqual([[0, 1]]);
        expect(calls(dotnetRef, 'PointerDown')).toHaveLength(0);
    });

    test('dragging a heading edge resizes the column and reports the width', () => {
        const { root, grid, dotnetRef } = build();
        const colgroup = document.createElement('colgroup');
        colgroup.innerHTML = '<col style="width:48px"><col style="width:100px"><col style="width:100px">';
        grid.prepend(colgroup);
        grid.style.width = '248px';
        grid.dataset.zoom = '2';
        const handle = document.createElement('span');
        handle.dataset.twSpreadsheetResize = '';
        root.querySelector('#colA').appendChild(handle);

        mouse(handle, 'mousedown', { clientX: 100 });
        mouse(document, 'mousemove', { clientX: 140 });

        expect(colgroup.children[1].style.width).toBe('120px');
        expect(grid.style.width).toBe('268px');
        expect(calls(dotnetRef, 'PointerDown')).toHaveLength(0);

        mouse(document, 'mouseup', { clientX: 140 });
        expect(calls(dotnetRef, 'ResizeColumn')).toEqual([[0, 120]]);

        mouse(document, 'mousemove', { clientX: 400 });
        expect(colgroup.children[1].style.width).toBe('120px');
    });

    test('resizing keeps widths within limits and ignores a click without movement', () => {
        const { root, grid, dotnetRef } = build();
        const colgroup = document.createElement('colgroup');
        colgroup.innerHTML = '<col style="width:48px"><col style="width:100px"><col style="width:100px">';
        grid.prepend(colgroup);
        const handle = document.createElement('span');
        handle.dataset.twSpreadsheetResize = '';
        root.querySelector('#colB').appendChild(handle);

        mouse(handle, 'mousedown', { clientX: 100 });
        mouse(document, 'mouseup', { clientX: 100 });
        expect(calls(dotnetRef, 'ResizeColumn')).toHaveLength(0);

        mouse(handle, 'mousedown', { clientX: 100 });
        mouse(document, 'mousemove', { clientX: -500 });
        mouse(document, 'mouseup');
        expect(calls(dotnetRef, 'ResizeColumn')).toEqual([[1, 24]]);
    });

    test('readText returns the clipboard text, or null when refused', async () => {
        Object.defineProperty(navigator, 'clipboard', { value: { readText: vi.fn(() => Promise.resolve('a\tb')) }, configurable: true });
        await expect(sheets.readText()).resolves.toBe('a\tb');

        Object.defineProperty(navigator, 'clipboard', { value: { readText: vi.fn(() => Promise.reject(new Error('denied'))) }, configurable: true });
        await expect(sheets.readText()).resolves.toBeNull();
    });

    test('setFullScreen stops the page scrolling and restores it', () => {
        const { grid } = build();
        document.documentElement.style.overflow = 'scroll';

        sheets.setFullScreen(grid, true);
        expect(document.documentElement.style.overflow).toBe('hidden');

        sheets.setFullScreen(grid, false);
        expect(document.documentElement.style.overflow).toBe('scroll');
        document.documentElement.style.overflow = '';
    });

    test('Escape outside the grid leaves full screen, but not from the grid or an open menu', () => {
        const { root, grid, dotnetRef } = build();
        const button = document.createElement('button');
        const menu = document.createElement('div');
        menu.setAttribute('role', 'menu');
        const item = document.createElement('button');
        menu.appendChild(item);
        root.append(button, menu);

        keydown(button, 'Escape');
        expect(calls(dotnetRef, 'ExitFullScreen')).toHaveLength(0);

        root.dataset.fullscreen = 'true';
        keydown(grid, 'Escape');
        keydown(item, 'Escape');
        expect(calls(dotnetRef, 'ExitFullScreen')).toHaveLength(0);

        keydown(button, 'Escape');
        expect(calls(dotnetRef, 'ExitFullScreen')).toHaveLength(1);
    });
});

describe('twSpreadsheet fitting, row resizing, filters and dialogs', () => {
    function withScrollWidth(cell, width) {
        Object.defineProperty(cell, 'scrollWidth', { value: width, configurable: true });
    }

    test('fitColumn measures the widest cell, heading included, within limits', () => {
        const { root, grid } = build();
        withScrollWidth(root.querySelector('#colA'), 30);
        withScrollWidth(root.querySelector('#sh-A1'), 150);
        withScrollWidth(root.querySelector('#sh-A2'), 90);

        expect(sheets.fitColumn(grid, 0)).toBe(154);
        expect(sheets.fitColumn(grid, -1)).toBe(0);
        expect(sheets.fitColumn(null, 0)).toBe(0);

        withScrollWidth(root.querySelector('#sh-A1'), 5000);
        expect(sheets.fitColumn(grid, 0)).toBe(1000);
    });

    test('fitRow measures the row at its natural height, never below the standard height', () => {
        const { grid } = build();
        const row = grid.tBodies[0].rows[0];
        row.style.height = '60px';
        row.getBoundingClientRect = vi.fn(() => ({ height: row.style.height === 'auto' ? 40 : 60 }));

        expect(sheets.fitRow(grid, 0)).toBe(48);
        expect(row.style.height).toBe('60px');

        row.getBoundingClientRect = vi.fn(() => ({ height: 10 }));
        expect(sheets.fitRow(grid, 0)).toBe(32);
        expect(sheets.fitRow(grid, 9)).toBe(0);
    });

    test('a double-click on a column or row edge fits it and reports the size', () => {
        const { root, grid, dotnetRef } = build();
        const columnHandle = document.createElement('span');
        columnHandle.dataset.twSpreadsheetResize = '';
        root.querySelector('#colA').appendChild(columnHandle);
        withScrollWidth(root.querySelector('#sh-A1'), 120);
        const rowHandle = document.createElement('span');
        rowHandle.dataset.twSpreadsheetRowResize = '';
        root.querySelector('#row2').appendChild(rowHandle);
        grid.tBodies[0].rows[1].getBoundingClientRect = vi.fn(() => ({ height: 50 }));

        mouse(columnHandle, 'dblclick');
        mouse(rowHandle, 'dblclick');

        expect(calls(dotnetRef, 'ResizeColumn')).toEqual([[0, 124]]);
        expect(calls(dotnetRef, 'ResizeRow')).toEqual([[1, 58]]);
        expect(calls(dotnetRef, 'PointerDoubleClick')).toHaveLength(0);
    });

    test('dragging a row edge resizes the row and reports the height', () => {
        const { root, grid, dotnetRef } = build();
        const handle = document.createElement('span');
        handle.dataset.twSpreadsheetRowResize = '';
        root.querySelector('#row1').appendChild(handle);
        const row = grid.tBodies[0].rows[0];
        row.style.height = '32px';
        grid.dataset.zoom = '2';

        mouse(handle, 'mousedown', { clientY: 100 });
        mouse(document, 'mousemove', { clientY: 140 });
        expect(row.style.height).toBe('52px');
        mouse(document, 'mouseup');

        expect(calls(dotnetRef, 'ResizeRow')).toEqual([[0, 52]]);
        expect(calls(dotnetRef, 'PointerDown')).toHaveLength(0);
    });

    test('a press on a filter button leaves the selection alone so the button can be clicked', () => {
        const { root, dotnetRef } = build();
        const button = document.createElement('button');
        button.dataset.twSpreadsheetFilter = '';
        root.querySelector('#sh-B1').appendChild(button);

        const event = mouse(button, 'mousedown');

        expect(event.defaultPrevented).toBe(false);
        expect(calls(dotnetRef, 'PointerDown')).toHaveLength(0);
    });

    test('openDialog traps focus, focuses the marked element and closes on Escape', () => {
        const { root, dotnetRef } = build();
        const panel = document.createElement('div');
        panel.innerHTML = '<button id="first">First</button><button id="safe" data-autofocus>Cancel</button>';
        root.appendChild(panel);
        const push = vi.spyOn(globalThis.twDialog, '_pushEscape');
        const trap = vi.spyOn(globalThis.twDialog, 'trapFocus');

        sheets.openDialog(panel, dotnetRef);
        sheets.openDialog(panel, dotnetRef);

        expect(document.activeElement.id).toBe('safe');
        expect(push).toHaveBeenCalledOnce();
        expect(push.mock.calls[0][0]).toMatchObject({ surface: panel, dotnetRef: dotnetRef, method: 'CloseDialog' });
        expect(trap).toHaveBeenCalledWith(panel);

        const release = vi.spyOn(globalThis.twDialog, 'releaseFocusTrap');
        const unregister = vi.spyOn(globalThis.twDialog, 'unregisterEscape');
        sheets.closeDialog(panel);
        sheets.closeDialog(panel);
        expect(release).toHaveBeenCalledOnce();
        expect(unregister).toHaveBeenCalledOnce();
    });

    test('openDialog falls back to the first control, and ignores a missing panel', () => {
        const { root, dotnetRef } = build();
        const panel = document.createElement('div');
        panel.innerHTML = '<button id="only">OK</button>';
        root.appendChild(panel);

        // jsdom has no layout, so the library's focusable check finds nothing; stand in for it.
        vi.spyOn(globalThis.twDialog, 'getFocusableElements').mockReturnValue([panel.querySelector('#only')]);
        sheets.openDialog(null, dotnetRef);
        sheets.closeDialog(null);
        sheets.openDialog(panel, dotnetRef);

        expect(document.activeElement.id).toBe('only');
        sheets.closeDialog(panel);
    });
});

describe('twSpreadsheet touch', () => {
    // A touch event with the given fingers, each a { clientX, clientY } point.
    function touch(target, type, points = [{ clientX: 0, clientY: 0 }]) {
        const event = new Event(type, { bubbles: true, cancelable: true });
        Object.defineProperty(event, 'touches', { value: type === 'touchend' || type === 'touchcancel' ? [] : points });
        target.dispatchEvent(event);
        return event;
    }

    // Marks A1 as the active cell, the way TwSpreadsheet points the grid at it.
    function withActive(grid, id = 'sh-A1') {
        grid.setAttribute('aria-activedescendant', id);
    }

    test('a finger dragged from the active cell extends the selection instead of scrolling', () => {
        const { root, grid, dotnetRef } = build();
        withActive(grid);
        document.elementFromPoint = vi.fn(() => root.querySelector('#sh-B1'));

        expect(touch(root.querySelector('#sh-A1'), 'touchstart').defaultPrevented).toBe(false);
        expect(touch(root.querySelector('#sh-A1'), 'touchmove').defaultPrevented).toBe(true);
        touch(root.querySelector('#sh-A1'), 'touchmove');
        document.elementFromPoint = vi.fn(() => root.querySelector('#sh-B2'));
        touch(root.querySelector('#sh-A1'), 'touchmove');

        expect(calls(dotnetRef, 'PointerExtend')).toEqual([[0, 1], [1, 1]]);
        expect(calls(dotnetRef, 'PointerDown')).toHaveLength(0);
        expect(touch(root.querySelector('#sh-A1'), 'touchend').defaultPrevented).toBe(true);
    });

    test('a finger dragged from a selected cell extends the selection', () => {
        const { root, grid, dotnetRef } = build();
        withActive(grid);
        root.querySelector('#sh-B1').setAttribute('aria-selected', 'true');
        document.elementFromPoint = vi.fn(() => root.querySelector('#sh-B2'));

        touch(root.querySelector('#sh-B1'), 'touchstart');
        touch(root.querySelector('#sh-B1'), 'touchmove');

        expect(calls(dotnetRef, 'PointerExtend')).toEqual([[1, 1]]);
    });

    test('a finger dragged from a cell outside the selection scrolls as usual', () => {
        const { root, grid, dotnetRef } = build();
        withActive(grid);
        root.querySelector('#sh-B2').setAttribute('aria-selected', 'false');
        document.elementFromPoint = vi.fn(() => root.querySelector('#sh-A1'));

        touch(root.querySelector('#sh-B2'), 'touchstart');
        const move = touch(root.querySelector('#sh-B2'), 'touchmove');

        expect(move.defaultPrevented).toBe(false);
        expect(calls(dotnetRef, 'PointerExtend')).toHaveLength(0);
        expect(touch(root.querySelector('#sh-B2'), 'touchend').defaultPrevented).toBe(false);
    });

    test('a tap on the active cell is left to the browser, so its click still selects the cell', () => {
        const { root, grid } = build();
        withActive(grid);

        touch(root.querySelector('#sh-A1'), 'touchstart');

        expect(touch(root.querySelector('#sh-A1'), 'touchend').defaultPrevented).toBe(false);
    });

    test('a second finger hands the gesture back to the browser, such as to pinch zoom', () => {
        const { root, grid, dotnetRef } = build();
        withActive(grid);
        const cell = root.querySelector('#sh-A1');
        document.elementFromPoint = vi.fn(() => root.querySelector('#sh-B1'));

        touch(cell, 'touchstart', [{ clientX: 0, clientY: 0 }, { clientX: 10, clientY: 10 }]);
        expect(touch(cell, 'touchmove').defaultPrevented).toBe(false);

        touch(cell, 'touchstart');
        expect(touch(cell, 'touchmove', [{ clientX: 0, clientY: 0 }, { clientX: 10, clientY: 10 }]).defaultPrevented).toBe(false);
        expect(touch(cell, 'touchmove').defaultPrevented).toBe(false);
        expect(calls(dotnetRef, 'PointerExtend')).toHaveLength(0);
    });

    test('ignores touches in a cell editor, on a filter button, and while a formula is pointing at cells', () => {
        const { root, grid, dotnetRef } = build();
        withActive(grid, 'sh-B2');
        const button = document.createElement('button');
        button.dataset.twSpreadsheetFilter = '';
        root.querySelector('#sh-B2').appendChild(button);
        document.elementFromPoint = vi.fn(() => root.querySelector('#sh-A1'));

        touch(root.querySelector('#editor'), 'touchstart');
        expect(touch(root.querySelector('#editor'), 'touchmove').defaultPrevented).toBe(false);
        touch(button, 'touchstart');
        expect(touch(button, 'touchmove').defaultPrevented).toBe(false);

        grid.dataset.pointing = 'true';
        touch(root.querySelector('#sh-B2'), 'touchstart');
        expect(touch(root.querySelector('#sh-B2'), 'touchmove').defaultPrevented).toBe(false);
        expect(calls(dotnetRef, 'PointerExtend')).toHaveLength(0);
    });

    // Wraps the grid in a scrolling area 300 by 200 pixels at the top left of the viewport, which scrolls up to max
    // pixels each way, with headings 40 pixels wide and 30 tall. Frames run only when the test runs them.
    function withScroller(root, grid, max = 1000) {
        const scroller = document.createElement('div');
        scroller.style.overflow = 'auto';
        grid.parentElement.insertBefore(scroller, grid);
        scroller.appendChild(grid);
        scroller.getBoundingClientRect = () => ({ left: 0, top: 0, right: 300, bottom: 200, width: 300, height: 200 });
        let left = 0;
        let top = 0;
        Object.defineProperty(scroller, 'scrollLeft', { get: () => left });
        Object.defineProperty(scroller, 'scrollTop', { get: () => top });
        scroller.scrollBy = vi.fn((x, y) => {
            left = Math.min(max, Math.max(0, left + x));
            top = Math.min(max, Math.max(0, top + y));
        });
        root.querySelector('#corner').getBoundingClientRect = () => ({ left: 0, top: 0, right: 40, bottom: 30, width: 40, height: 30 });

        const frames = [];
        globalThis.requestAnimationFrame = vi.fn(fn => frames.push(fn));
        globalThis.cancelAnimationFrame = vi.fn();
        const runFrame = () => frames.shift()?.();
        return { scroller, frames, runFrame };
    }

    afterEach(() => {
        delete globalThis.requestAnimationFrame;
        delete globalThis.cancelAnimationFrame;
    });

    test('a finger near an edge of the cells scrolls the grid, faster the further in it is, and keeps going while it rests there', () => {
        const { root, grid } = build();
        withActive(grid);
        const { scroller, frames, runFrame } = withScroller(root, grid);
        document.elementFromPoint = vi.fn(() => null);
        const cell = root.querySelector('#sh-A1');

        touch(cell, 'touchstart');
        touch(cell, 'touchmove', [{ clientX: 150, clientY: 100 }]);
        expect(scroller.scrollBy).not.toHaveBeenCalled();
        expect(frames).toHaveLength(0);

        // 22 pixels into the right edge's 32 pixel band.
        touch(cell, 'touchmove', [{ clientX: 290, clientY: 100 }]);
        expect(scroller.scrollBy.mock.calls).toEqual([[15, 0]]);
        runFrame();
        expect(scroller.scrollBy.mock.calls).toEqual([[15, 0], [15, 0]]);

        // Moving further out while a frame is waiting only updates where the finger is; past the edge is the fastest.
        touch(cell, 'touchmove', [{ clientX: 400, clientY: 50 }]);
        expect(scroller.scrollBy).toHaveBeenCalledTimes(2);
        runFrame();
        expect(scroller.scrollBy.mock.calls[2]).toEqual([24, -10]);

        touch(cell, 'touchend');
        expect(globalThis.cancelAnimationFrame).toHaveBeenCalled();
        runFrame();
        expect(scroller.scrollBy).toHaveBeenCalledTimes(3);
    });

    test('stops scrolling once the grid can scroll no further', () => {
        const { root, grid } = build();
        withActive(grid);
        const { scroller, frames, runFrame } = withScroller(root, grid, 20);
        document.elementFromPoint = vi.fn(() => null);
        const cell = root.querySelector('#sh-A1');

        touch(cell, 'touchstart');
        touch(cell, 'touchmove', [{ clientX: 400, clientY: 100 }]);
        runFrame();
        expect(frames).toHaveLength(0);
        expect(scroller.scrollBy).toHaveBeenCalledTimes(2);
    });

    test('past the cells, or over a heading, the selection goes to the nearest cell in view', () => {
        const { root, grid, dotnetRef } = build();
        withActive(grid);
        withScroller(root, grid, 0);
        document.elementFromPoint = vi.fn(() => root.querySelector('#sh-B2'));
        const cell = root.querySelector('#sh-A1');

        touch(cell, 'touchstart');
        touch(cell, 'touchmove', [{ clientX: 500, clientY: 10 }]);
        expect(document.elementFromPoint).toHaveBeenLastCalledWith(299, 31);
        touch(cell, 'touchmove', [{ clientX: -20, clientY: 900 }]);
        expect(document.elementFromPoint).toHaveBeenLastCalledWith(41, 199);
        expect(calls(dotnetRef, 'PointerExtend')).toEqual([[1, 1]]);
    });

    test('never extends the selection to a heading', () => {
        const { root, grid, dotnetRef } = build();
        withActive(grid);
        document.elementFromPoint = vi.fn(() => root.querySelector('#colB'));
        const cell = root.querySelector('#sh-A1');

        touch(cell, 'touchstart');
        touch(cell, 'touchmove');

        expect(calls(dotnetRef, 'PointerExtend')).toHaveLength(0);
    });

    test('a finger dragging a heading edge resizes the column and reports the width', () => {
        const { root, grid, dotnetRef } = build();
        const colgroup = document.createElement('colgroup');
        colgroup.innerHTML = '<col style="width:48px"><col style="width:100px"><col style="width:100px">';
        grid.prepend(colgroup);
        grid.style.width = '248px';
        const handle = document.createElement('span');
        handle.dataset.twSpreadsheetResize = '';
        root.querySelector('#colA').appendChild(handle);

        expect(touch(handle, 'touchstart', [{ clientX: 100, clientY: 0 }]).defaultPrevented).toBe(true);
        expect(touch(document, 'touchmove', [{ clientX: 130, clientY: 0 }]).defaultPrevented).toBe(true);
        expect(colgroup.children[1].style.width).toBe('130px');
        expect(grid.style.width).toBe('278px');

        touch(document, 'touchend');
        expect(calls(dotnetRef, 'ResizeColumn')).toEqual([[0, 130]]);

        touch(document, 'touchmove', [{ clientX: 400, clientY: 0 }]);
        expect(colgroup.children[1].style.width).toBe('130px');
    });

    test('a finger dragging a row edge resizes the row, and a cancelled touch still reports it', () => {
        const { root, grid, dotnetRef } = build();
        const handle = document.createElement('span');
        handle.dataset.twSpreadsheetRowResize = '';
        root.querySelector('#row1').appendChild(handle);
        const row = grid.tBodies[0].rows[0];
        row.style.height = '32px';

        touch(handle, 'touchstart', [{ clientX: 0, clientY: 100 }]);
        touch(document, 'touchmove', [{ clientX: 0, clientY: 120 }]);
        touch(document, 'touchcancel');

        expect(row.style.height).toBe('52px');
        expect(calls(dotnetRef, 'ResizeRow')).toEqual([[0, 52]]);
        expect(calls(dotnetRef, 'PointerDown')).toHaveLength(0);
    });

    test('detach removes the touch listeners', () => {
        const { root, grid, dotnetRef } = build();
        withActive(grid);
        document.elementFromPoint = vi.fn(() => root.querySelector('#sh-B1'));
        sheets.detach(grid);

        touch(root.querySelector('#sh-A1'), 'touchstart');

        expect(touch(root.querySelector('#sh-A1'), 'touchmove').defaultPrevented).toBe(false);
        expect(calls(dotnetRef, 'PointerExtend')).toHaveLength(0);
    });
});

describe('twSpreadsheet toolbar overflow', () => {
    // Adds the toolbar TwSpreadsheet renders, with its controls box reporting the given widths.
    function withToolbar(root, scrollWidth, clientWidth) {
        const toolbar = document.createElement('div');
        toolbar.setAttribute('data-tw-spreadsheet-toolbar', '');
        toolbar.innerHTML = '<div data-tw-spreadsheet-toolbar-items></div>';
        root.prepend(toolbar);
        const items = toolbar.firstElementChild;
        Object.defineProperty(items, 'scrollWidth', { configurable: true, get: () => scrollWidth });
        Object.defineProperty(items, 'clientWidth', { configurable: true, get: () => clientWidth });
        return { toolbar, items };
    }

    test('marks the toolbar as overflowing only while its controls do not fit', () => {
        const { root, grid } = build();
        const { toolbar } = withToolbar(root, 900, 400);

        sheets.observeToolbar(grid);

        expect(toolbar.hasAttribute('data-overflowing')).toBe(true);
    });

    test('leaves the toolbar unmarked when its controls fit', () => {
        const { root, grid } = build();
        const { toolbar } = withToolbar(root, 400, 400);

        sheets.observeToolbar(grid);

        expect(toolbar.hasAttribute('data-overflowing')).toBe(false);
    });

    test('keeps the mark while the toolbar is expanded, and re-measures on resize', () => {
        const observers = [];
        vi.stubGlobal('ResizeObserver', class {
            constructor(callback) { this.callback = callback; this.disconnect = vi.fn(); observers.push(this); }
            observe() { }
        });
        const { root, grid } = build();
        const { toolbar, items } = withToolbar(root, 900, 400);
        sheets.observeToolbar(grid);
        sheets.observeToolbar(grid);
        expect(observers).toHaveLength(1);

        toolbar.dataset.expanded = 'true';
        Object.defineProperty(items, 'scrollWidth', { configurable: true, get: () => 400 });
        observers[0].callback();
        expect(toolbar.hasAttribute('data-overflowing')).toBe(true);

        delete toolbar.dataset.expanded;
        observers[0].callback();
        expect(toolbar.hasAttribute('data-overflowing')).toBe(false);

        sheets.detach(grid);
        expect(observers[0].disconnect).toHaveBeenCalledOnce();
        vi.unstubAllGlobals();
    });

    test('does nothing without a toolbar or before the grid is attached', () => {
        const { grid } = build();
        expect(() => sheets.observeToolbar(grid)).not.toThrow();
        expect(() => sheets.observeToolbar(document.createElement('table'))).not.toThrow();
    });
});
