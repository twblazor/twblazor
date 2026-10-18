// @vitest-environment jsdom

import { afterEach, describe, expect, test, vi } from 'vitest';

// Load the script once - it assigns window.twDataTable on evaluation.
await import('../../../src/TwBlazor/wwwroot/js/twblazor.js');

// A data table root with a column group and a header row, the way TwDataTable renders it.
function buildTable(names, widths, direction = 'ltr') {
    const root = document.createElement('div');
    root.innerHTML = `
        <table>
            <colgroup>${names.map((n) => `<col data-column="${n}">`).join('')}</colgroup>
            <thead><tr>${names.map((n) => `<th><span>${n}</span><span data-resize-column="${n}"></span></th>`).join('')}</tr></thead>
        </table>`;
    root.querySelectorAll('th').forEach((th, i) => {
        th.getBoundingClientRect = () => ({ width: widths[i] });
        th.style.direction = direction;
    });
    document.body.appendChild(root);
    return root;
}

function handle(root, name) {
    return root.querySelector(`[data-resize-column="${name}"]`);
}

function pointer(type, target, clientX, extra = {}) {
    const event = new Event(type, { bubbles: true, cancelable: true });
    Object.assign(event, { clientX, button: 0, pointerId: 1 }, extra);
    target.dispatchEvent(event);
    return event;
}

describe('twDataTable', () => {
    afterEach(() => {
        vi.restoreAllMocks();
        document.body.innerHTML = '';
        localStorage.clear();
    });

    describe('state storage', () => {
        test('saves and loads state under a key', () => {
            globalThis.twDataTable.saveState('orders', '{"page":2}');

            expect(globalThis.twDataTable.loadState('orders')).toBe('{"page":2}');
        });

        test('returns null when nothing is saved', () => {
            expect(globalThis.twDataTable.loadState('missing')).toBeNull();
        });

        test('returns null instead of throwing when storage is blocked', () => {
            vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => { throw new Error('SecurityError'); });

            expect(globalThis.twDataTable.loadState('orders')).toBeNull();
        });

        test('does not throw when storage is full', () => {
            vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => { throw new Error('QuotaExceededError'); });

            expect(() => globalThis.twDataTable.saveState('orders', '{}')).not.toThrow();
        });
    });

    describe('focus', () => {
        test('focuses the element with the id', () => {
            const button = document.createElement('button');
            button.id = 'move-later';
            document.body.appendChild(button);

            globalThis.twDataTable.focus('move-later');

            expect(document.activeElement).toBe(button);
        });

        test('ignores an id that is not on the page', () => {
            expect(() => globalThis.twDataTable.focus('missing')).not.toThrow();
        });
    });

    describe('measureColumn', () => {
        test('measures the header cell that lines up with the column', () => {
            const root = buildTable(['name', 'price'], [180.4, 96]);

            expect(globalThis.twDataTable.measureColumn(root, 'name')).toBe(180);
            expect(globalThis.twDataTable.measureColumn(root, 'price')).toBe(96);
        });

        test('returns 0 for a column that is not shown', () => {
            const root = buildTable(['name'], [180]);

            expect(globalThis.twDataTable.measureColumn(root, 'missing')).toBe(0);
        });
    });

    describe('attachResize', () => {
        test('dragging the handle sizes the column and commits the width when released', () => {
            const root = buildTable(['name', 'price'], [200, 100]);
            const dotnetRef = { invokeMethodAsync: vi.fn() };
            globalThis.twDataTable.attachResize(root, dotnetRef, 48, 2000);

            const down = pointer('pointerdown', handle(root, 'name'), 500);
            pointer('pointermove', handle(root, 'name'), 560);

            expect(down.defaultPrevented).toBe(true);
            expect(root.querySelector('col[data-column="name"]').style.width).toBe('260px');

            pointer('pointerup', handle(root, 'name'), 560);

            expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('CommitColumnWidth', 'name', 260);
        });

        test('dragging towards the start widens the column in a right to left layout', () => {
            const root = buildTable(['name'], [200], 'rtl');
            const dotnetRef = { invokeMethodAsync: vi.fn() };
            globalThis.twDataTable.attachResize(root, dotnetRef, 48, 2000);

            pointer('pointerdown', handle(root, 'name'), 500);
            pointer('pointermove', handle(root, 'name'), 450);
            pointer('pointerup', handle(root, 'name'), 450);

            expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('CommitColumnWidth', 'name', 250);
        });

        test('keeps the width within the limits', () => {
            const root = buildTable(['name'], [100]);
            const dotnetRef = { invokeMethodAsync: vi.fn() };
            globalThis.twDataTable.attachResize(root, dotnetRef, 48, 300);

            pointer('pointerdown', handle(root, 'name'), 500);
            pointer('pointermove', handle(root, 'name'), 0);

            expect(root.querySelector('col').style.width).toBe('48px');

            pointer('pointermove', handle(root, 'name'), 2000);

            expect(root.querySelector('col').style.width).toBe('300px');
        });

        test('does not commit a drag that ends where it started', () => {
            const root = buildTable(['name'], [100]);
            const dotnetRef = { invokeMethodAsync: vi.fn() };
            globalThis.twDataTable.attachResize(root, dotnetRef, 48, 2000);

            pointer('pointerdown', handle(root, 'name'), 500);
            pointer('pointercancel', handle(root, 'name'), 500);

            expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();
        });

        test('ignores presses away from a handle and presses with another button', () => {
            const root = buildTable(['name'], [100]);
            const dotnetRef = { invokeMethodAsync: vi.fn() };
            globalThis.twDataTable.attachResize(root, dotnetRef, 48, 2000);

            const onHeader = pointer('pointerdown', root.querySelector('th span'), 500);
            const rightClick = pointer('pointerdown', handle(root, 'name'), 500, { button: 2 });

            expect(onHeader.defaultPrevented).toBe(false);
            expect(rightClick.defaultPrevented).toBe(false);
        });

        test('attaching twice does not double the listeners, and detaching removes them', () => {
            const root = buildTable(['name'], [100]);
            const dotnetRef = { invokeMethodAsync: vi.fn() };
            const addSpy = vi.spyOn(root, 'addEventListener');
            globalThis.twDataTable.attachResize(root, dotnetRef, 48, 2000);
            globalThis.twDataTable.attachResize(root, dotnetRef, 48, 2000);

            expect(addSpy).toHaveBeenCalledTimes(1);

            globalThis.twDataTable.detachResize(root);
            const down = pointer('pointerdown', handle(root, 'name'), 500);

            expect(down.defaultPrevented).toBe(false);
            expect(() => globalThis.twDataTable.detachResize(root)).not.toThrow();
        });
    });
});
