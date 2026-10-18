// @vitest-environment jsdom

import { afterEach, describe, expect, test, vi } from 'vitest';

// Load the script once - it assigns window.twDataTable on evaluation.
await import('../../../src/TwBlazor/wwwroot/js/twblazor.js');

// A filter dialog and the button that opens it, the way TwDataTable renders them.
function buildFilter() {
    const button = document.createElement('button');
    button.id = 'filter-button';
    const panel = document.createElement('div');
    panel.innerHTML = '<input id="first" /><button id="apply">Filter</button>';
    const outside = document.createElement('button');
    document.body.append(button, panel, outside);
    return { button, panel, outside };
}

function pointer(type, target) {
    const event = new Event(type, { bubbles: true, cancelable: true });
    Object.assign(event, { clientX: 0, clientY: 0, button: 0, pointerId: 1 });
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

    describe('download', () => {
        test('saves the streamed bytes as a file and frees the object URL afterwards', async () => {
            vi.useFakeTimers();
            URL.createObjectURL = vi.fn(() => 'blob:export');
            URL.revokeObjectURL = vi.fn();
            const clicked = [];
            vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(function () {
                clicked.push({ href: this.href, download: this.download, attached: document.body.contains(this) });
            });
            const streamReference = { arrayBuffer: vi.fn(async () => new Uint8Array([1, 2, 3]).buffer) };

            await globalThis.twDataTable.download('orders.csv', 'text/csv', streamReference);

            expect(clicked).toEqual([{ href: 'blob:export', download: 'orders.csv', attached: true }]);
            expect(document.querySelector('a')).toBeNull();
            const blob = URL.createObjectURL.mock.calls[0][0];
            expect(blob.type).toBe('text/csv');
            expect(blob.size).toBe(3);
            expect(URL.revokeObjectURL).not.toHaveBeenCalled();

            vi.runAllTimers();

            expect(URL.revokeObjectURL).toHaveBeenCalledWith('blob:export');
            vi.useRealTimers();
        });
    });

    describe('filter', () => {
        function stubDialog() {
            return {
                reposition: vi.spyOn(globalThis.twPicker, 'registerScrollReposition').mockImplementation(() => { }),
                unreposition: vi.spyOn(globalThis.twPicker, 'unregisterScrollReposition').mockImplementation(() => { }),
                trap: vi.spyOn(globalThis.twDialog, 'trapFocus').mockImplementation(() => { }),
                release: vi.spyOn(globalThis.twDialog, 'releaseFocusTrap').mockImplementation(() => { }),
                focus: vi.spyOn(globalThis.twDialog, 'focusPanel').mockImplementation(() => { }),
                escape: vi.spyOn(globalThis.twDialog, '_pushEscape').mockImplementation(() => { }),
                unescape: vi.spyOn(globalThis.twDialog, 'unregisterEscape').mockImplementation(() => { })
            };
        }

        test('openFilter positions the dialog under its button, claims Escape, traps Tab and moves focus in', () => {
            const { button, panel } = buildFilter();
            const dialog = stubDialog();
            const dotnetRef = { invokeMethodAsync: vi.fn() };

            globalThis.twDataTable.openFilter(panel, 'filter-button', dotnetRef);

            expect(dialog.reposition).toHaveBeenCalledWith(button, panel, false, dotnetRef);
            expect(dialog.escape).toHaveBeenCalledWith({ surface: panel, dotnetRef: dotnetRef, method: 'Close' });
            expect(dialog.trap).toHaveBeenCalledWith(panel);
            expect(dialog.focus).toHaveBeenCalledWith(panel);
        });

        test('a press outside both the dialog and its button closes it', () => {
            const { panel, outside } = buildFilter();
            stubDialog();
            const dotnetRef = { invokeMethodAsync: vi.fn(() => Promise.resolve()) };
            globalThis.twDataTable.openFilter(panel, 'filter-button', dotnetRef);

            pointer('pointerdown', outside);
            pointer('pointerup', outside);

            expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('Close');
        });

        test('presses inside the dialog or on its button leave it open', () => {
            const { button, panel } = buildFilter();
            stubDialog();
            const dotnetRef = { invokeMethodAsync: vi.fn(() => Promise.resolve()) };
            globalThis.twDataTable.openFilter(panel, 'filter-button', dotnetRef);

            pointer('pointerdown', panel.querySelector('input'));
            pointer('pointerup', panel.querySelector('input'));
            pointer('pointerdown', button);
            pointer('pointerup', button);
            pointer('pointerup', button);

            expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();
        });

        test('closeFilter removes the listeners and releases Escape, the trap and the positioning', () => {
            const { panel, outside } = buildFilter();
            const dialog = stubDialog();
            const dotnetRef = { invokeMethodAsync: vi.fn(() => Promise.resolve()) };
            globalThis.twDataTable.openFilter(panel, 'filter-button', dotnetRef);

            globalThis.twDataTable.closeFilter(panel);
            pointer('pointerdown', outside);
            pointer('pointerup', outside);

            expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();
            expect(dialog.unreposition).toHaveBeenCalledWith(panel);
            expect(dialog.unescape).toHaveBeenCalledWith(panel);
            expect(dialog.release).toHaveBeenCalledWith(panel);
            expect(() => globalThis.twDataTable.closeFilter(panel)).not.toThrow();
        });

        test('opening twice is harmless, and a missing button only skips the positioning', () => {
            const { panel } = buildFilter();
            const dialog = stubDialog();
            const dotnetRef = { invokeMethodAsync: vi.fn() };

            globalThis.twDataTable.openFilter(panel, 'missing', dotnetRef);
            globalThis.twDataTable.openFilter(panel, 'missing', dotnetRef);

            expect(dialog.reposition).not.toHaveBeenCalled();
            expect(dialog.trap).toHaveBeenCalledTimes(1);
            expect(() => globalThis.twDataTable.openFilter(null)).not.toThrow();
        });
    });
});
