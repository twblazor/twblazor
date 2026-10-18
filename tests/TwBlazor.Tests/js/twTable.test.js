// @vitest-environment jsdom

import { afterEach, describe, expect, test, vi } from 'vitest';

// Load the script once - it assigns window.twTable on evaluation.
await import('../../../src/TwBlazor/wwwroot/js/twblazor.js');

// A table wrapper the way TwTable renders it with ResizableColumns: the table, then the separator layer. Each column
// is { id, width, resizable, label, min, max, span }.
function buildTable(columns, { dir = 'ltr', headerCells = null } = {}) {
    const wrapper = document.createElement('div');
    wrapper.setAttribute('dir', dir);
    const cols = columns.map(function (c) {
        const attributes = [
            `id="${c.id}"`,
            c.span ? `span="${c.span}"` : '',
            c.resizable === false ? '' : 'data-tw-resizable="true"',
            c.label ? `data-tw-label="${c.label}"` : '',
            c.min ? `data-tw-min="${c.min}"` : '',
            c.max ? `data-tw-max="${c.max}"` : ''
        ].join(' ');
        return `<col ${attributes}>`;
    }).join('');
    const cells = (headerCells ?? columns).map(function (c) { return `<th>${c.label ?? c.id}</th>`; }).join('');
    wrapper.innerHTML = `
        <table><colgroup>${cols}</colgroup><thead><tr>${cells}</tr></thead><tbody></tbody></table>
        <div data-tw-resize-layer data-handle-class="handle" data-label-format="Resize {0}" data-value-format="{0} pixels"></div>`;
    document.body.appendChild(wrapper);

    wrapper.getBoundingClientRect = () => ({ left: 100, top: 50, right: 900, bottom: 450, width: 800, height: 400 });
    const widths = (headerCells ?? columns).map(function (c) { return c.width ?? 100; });
    wrapper.querySelectorAll('th').forEach(function (th, i) {
        th.getBoundingClientRect = function () {
            const col = wrapper.querySelectorAll('col')[i];
            const width = col?.style.width ? parseFloat(col.style.width) : widths[i];
            const start = widths.slice(0, i).reduce(function (sum, w) { return sum + w; }, 0);
            const left = dir === 'rtl' ? 900 - start - width : 100 + start;
            return { left: left, right: left + width, top: 50, bottom: 90, width: width, height: 40 };
        };
    });
    return wrapper;
}

function handles(wrapper) {
    return Array.from(wrapper.querySelectorAll('[data-tw-resize-key]'));
}

function pointer(type, target, clientX, extra = {}) {
    const event = new Event(type, { bubbles: true, cancelable: true });
    Object.assign(event, { clientX, button: 0, pointerId: 1 }, extra);
    target.dispatchEvent(event);
    return event;
}

function key(target, keyName, extra = {}) {
    const event = new KeyboardEvent('keydown', { key: keyName, bubbles: true, cancelable: true, ...extra });
    target.dispatchEvent(event);
    return event;
}

describe('twTable', () => {
    afterEach(() => {
        vi.restoreAllMocks();
        document.body.innerHTML = '';
    });

    describe('layoutResize', () => {
        test('adds a named, focusable separator for each resizable column, in column order', () => {
            const wrapper = buildTable([
                { id: 'name', label: 'Name', width: 200, min: 80 },
                { id: 'notes', label: 'Notes', resizable: false },
                { id: 'price', label: 'Price', width: 120 }
            ]);

            globalThis.twTable.layoutResize(wrapper);

            const [name, price] = handles(wrapper);
            expect(handles(wrapper)).toHaveLength(2);
            expect(name.getAttribute('role')).toBe('separator');
            expect(name.getAttribute('aria-orientation')).toBe('vertical');
            expect(name.getAttribute('tabindex')).toBe('0');
            expect(name.getAttribute('aria-label')).toBe('Resize Name');
            expect(name.getAttribute('aria-valuenow')).toBe('200');
            expect(name.getAttribute('aria-valuetext')).toBe('200 pixels');
            expect(name.getAttribute('aria-valuemin')).toBe('80');
            expect(name.getAttribute('aria-valuemax')).toBe('2000');
            expect(name.className).toBe('handle');
            expect(price.getAttribute('aria-label')).toBe('Resize Price');
        });

        test('places each separator on the end border of its header cell', () => {
            const wrapper = buildTable([{ id: 'name', width: 200 }, { id: 'price', width: 120 }]);
            wrapper.scrollLeft = 30;

            globalThis.twTable.layoutResize(wrapper);

            const [name, price] = handles(wrapper);
            expect(name.style.left).toBe('230px');
            expect(price.style.left).toBe('350px');
            expect(name.style.top).toBe('0px');
            expect(name.style.height).toBe('40px');
        });

        test('uses the start border in a right to left layout', () => {
            const wrapper = buildTable([{ id: 'name', width: 200 }], { dir: 'rtl' });

            globalThis.twTable.layoutResize(wrapper);

            // The cell spans 700 to 900 in the viewport, so its end (left) border is 600 into the wrapper.
            expect(handles(wrapper)[0].style.left).toBe('600px');
        });

        test('follows the columns when they are reordered, and removes separators for columns that are gone', () => {
            const wrapper = buildTable([{ id: 'a', label: 'A' }, { id: 'b', label: 'B' }, { id: 'c', label: 'C' }]);
            globalThis.twTable.layoutResize(wrapper);
            const first = handles(wrapper)[0];

            const colgroup = wrapper.querySelector('colgroup');
            colgroup.appendChild(wrapper.querySelector('#a'));
            wrapper.querySelector('#b').remove();
            globalThis.twTable.layoutResize(wrapper);

            expect(handles(wrapper).map(function (h) { return h.dataset.twResizeKey; })).toEqual(['c', 'a']);
            expect(handles(wrapper)[1]).toBe(first);
        });

        test('hides a separator whose column has no header cell', () => {
            const wrapper = buildTable([{ id: 'a' }, { id: 'b' }], { headerCells: [{ id: 'a' }] });

            globalThis.twTable.layoutResize(wrapper);

            expect(handles(wrapper)[1].hidden).toBe(true);
        });

        test('lines columns up with header cells across spans', () => {
            const wrapper = buildTable(
                [{ id: 'a', span: 2, resizable: false }, { id: 'b', label: 'B', width: 50 }],
                { headerCells: [{ id: 'x', width: 100 }, { id: 'y', width: 100 }, { id: 'b', label: 'B', width: 50 }] });

            globalThis.twTable.layoutResize(wrapper);

            expect(handles(wrapper)[0].getAttribute('aria-valuenow')).toBe('50');
        });

        test('does nothing without a layer', () => {
            const wrapper = document.createElement('div');

            expect(() => globalThis.twTable.layoutResize(wrapper)).not.toThrow();
            expect(() => globalThis.twTable.layoutResize(null)).not.toThrow();
        });
    });

    describe('attachResize', () => {
        test('dragging a separator resizes its column and commits every column width', () => {
            const wrapper = buildTable([{ id: 'name', width: 200 }, { id: 'price', width: 120 }]);
            const dotnetRef = { invokeMethodAsync: vi.fn() };
            globalThis.twTable.attachResize(wrapper, dotnetRef);
            const handle = handles(wrapper)[0];

            const down = pointer('pointerdown', handle, 300);
            pointer('pointermove', handle, 350);

            expect(down.defaultPrevented).toBe(true);
            expect(wrapper.querySelector('#name').style.width).toBe('250px');
            expect(wrapper.querySelector('#price').style.width).toBe('120px');

            pointer('pointerup', handle, 350);

            expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('CommitColumnWidths', { name: 250, price: 120 });
        });

        test('dragging towards the start widens the column in a right to left layout', () => {
            const wrapper = buildTable([{ id: 'name', width: 200 }], { dir: 'rtl' });
            const dotnetRef = { invokeMethodAsync: vi.fn() };
            globalThis.twTable.attachResize(wrapper, dotnetRef);
            const handle = handles(wrapper)[0];

            pointer('pointerdown', handle, 500);
            pointer('pointermove', handle, 460);
            pointer('pointercancel', handle, 460);

            expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('CommitColumnWidths', { name: 240 });
        });

        test('keeps the width within the column limits', () => {
            const wrapper = buildTable([{ id: 'name', width: 200, min: 150, max: 260 }]);
            globalThis.twTable.attachResize(wrapper, { invokeMethodAsync: vi.fn() });
            const handle = handles(wrapper)[0];

            pointer('pointerdown', handle, 300);
            pointer('pointermove', handle, 0);
            expect(wrapper.querySelector('#name').style.width).toBe('150px');

            pointer('pointermove', handle, 900);
            expect(wrapper.querySelector('#name').style.width).toBe('260px');
        });

        test('the arrow keys resize the focused separator, with Shift for bigger steps', () => {
            const wrapper = buildTable([{ id: 'name', width: 200 }]);
            const dotnetRef = { invokeMethodAsync: vi.fn() };
            globalThis.twTable.attachResize(wrapper, dotnetRef);
            const handle = handles(wrapper)[0];

            const right = key(handle, 'ArrowRight');
            expect(right.defaultPrevented).toBe(true);
            expect(dotnetRef.invokeMethodAsync).toHaveBeenLastCalledWith('CommitColumnWidths', { name: 216 });

            key(handle, 'ArrowLeft', { shiftKey: true });
            expect(dotnetRef.invokeMethodAsync).toHaveBeenLastCalledWith('CommitColumnWidths', { name: 152 });
            expect(handle.getAttribute('aria-valuenow')).toBe('152');
        });

        test('the arrow keys follow the border in a right to left layout', () => {
            const wrapper = buildTable([{ id: 'name', width: 200 }], { dir: 'rtl' });
            const dotnetRef = { invokeMethodAsync: vi.fn() };
            globalThis.twTable.attachResize(wrapper, dotnetRef);

            key(handles(wrapper)[0], 'ArrowLeft');

            expect(dotnetRef.invokeMethodAsync).toHaveBeenLastCalledWith('CommitColumnWidths', { name: 216 });
        });

        test('ignores other keys, other buttons and presses away from a separator', () => {
            const wrapper = buildTable([{ id: 'name', width: 200 }]);
            const dotnetRef = { invokeMethodAsync: vi.fn() };
            globalThis.twTable.attachResize(wrapper, dotnetRef);
            const handle = handles(wrapper)[0];

            expect(key(handle, 'Enter').defaultPrevented).toBe(false);
            expect(pointer('pointerdown', handle, 0, { button: 2 }).defaultPrevented).toBe(false);
            expect(pointer('pointerdown', wrapper.querySelector('th'), 0).defaultPrevented).toBe(false);
            expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();
        });

        test('attaching twice is harmless, and detaching removes the separators and listeners', () => {
            const wrapper = buildTable([{ id: 'name', width: 200 }]);
            const dotnetRef = { invokeMethodAsync: vi.fn() };
            const addSpy = vi.spyOn(wrapper, 'addEventListener');
            globalThis.twTable.attachResize(wrapper, dotnetRef);
            globalThis.twTable.attachResize(wrapper, dotnetRef);
            expect(addSpy).toHaveBeenCalledTimes(2);

            const handle = handles(wrapper)[0];
            globalThis.twTable.detachResize(wrapper);

            expect(handles(wrapper)).toHaveLength(0);
            key(handle, 'ArrowRight');
            expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();
            expect(() => globalThis.twTable.detachResize(wrapper)).not.toThrow();
            expect(() => globalThis.twTable.attachResize(null, dotnetRef)).not.toThrow();
        });

        test('relays the separators out when the table resizes', () => {
            const observed = [];
            let callback = null;
            globalThis.ResizeObserver = class {
                constructor(cb) { callback = cb; }
                observe(el) { observed.push(el); }
                disconnect() { }
            };
            globalThis.requestAnimationFrame = function (fn) { fn(); return 1; };
            const wrapper = buildTable([{ id: 'name', width: 200 }]);
            globalThis.twTable.attachResize(wrapper, { invokeMethodAsync: vi.fn() });
            const layout = vi.spyOn(globalThis.twTable, 'layoutResize');

            callback();

            expect(observed).toContain(wrapper.querySelector('table'));
            expect(layout).toHaveBeenCalledWith(wrapper);
            delete globalThis.ResizeObserver;
            delete globalThis.requestAnimationFrame;
        });
    });
});
