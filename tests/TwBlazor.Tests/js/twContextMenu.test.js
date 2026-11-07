// @vitest-environment jsdom

import { afterEach, beforeEach, describe, expect, test, vi } from 'vitest';

await import('../../../src/TwBlazor/wwwroot/js/twblazor.js');

const viewport = { width: 1000, height: 800 };

function rect(left, top, width, height) {
    return { left, top, width, height, right: left + width, bottom: top + height, x: left, y: top };
}

// Builds an area with a focusable control in it and, beside it, the open menu Blazor renders. jsdom has no layout, so
// the panel reports a fixed size.
function build({ dir, disabled, panelSize = { width: 200, height: 150 }, focusableArea = false } = {}) {
    const root = document.createElement('div');
    if (dir) root.setAttribute('dir', dir);
    root.innerHTML = `
        <button id="before">Before</button>
        <div id="area" data-tw-context-menu="${disabled ? 'disabled' : 'enabled'}"${focusableArea ? ' tabindex="0"' : ''}>
            <button id="row">Row</button>
            <span id="text">Text</span>
        </div>
        <div id="menu" role="menu" tabindex="-1" style="position:fixed;left:0;top:0;visibility:hidden" data-tw-popover>
            <button role="menuitem" tabindex="-1">Copy</button>
            <button role="menuitem" tabindex="-1">Paste</button>
        </div>`;
    document.body.appendChild(root);
    const area = root.querySelector('#area');
    const menu = root.querySelector('#menu');
    menu.getBoundingClientRect = () => rect(0, 0, panelSize.width, panelSize.height);
    const dotnetRef = { invokeMethodAsync: vi.fn(() => Promise.resolve()) };
    return { root, area, menu, dotnetRef, row: root.querySelector('#row'), before: root.querySelector('#before') };
}

function contextMenu(target, x, y) {
    const event = new MouseEvent('contextmenu', { bubbles: true, cancelable: true, clientX: x, clientY: y });
    target.dispatchEvent(event);
    return event;
}

function key(target, keyName, extra = {}) {
    const event = new KeyboardEvent('keydown', { key: keyName, bubbles: true, cancelable: true, ...extra });
    target.dispatchEvent(event);
    return event;
}

function pointerDown(target, button = 0) {
    const event = new MouseEvent('pointerdown', { bubbles: true, cancelable: true, button: button });
    target.dispatchEvent(event);
    return event;
}

function px(value) {
    return Number.parseFloat(value);
}

describe('twContextMenu', () => {
    beforeEach(() => {
        Object.defineProperty(document.documentElement, 'clientWidth', { configurable: true, value: viewport.width });
        Object.defineProperty(document.documentElement, 'clientHeight', { configurable: true, value: viewport.height });
        vi.spyOn(globalThis.twPicker, '_fixedContainingBlock').mockReturnValue({ left: 0, top: 0, right: viewport.width, bottom: viewport.height });
    });

    afterEach(() => {
        document.querySelectorAll('[data-tw-context-menu]').forEach(function (area) { globalThis.twContextMenu.detach(area); });
        globalThis.twDialog._escapeStack.length = 0;
        vi.restoreAllMocks();
        document.body.innerHTML = '';
    });

    test('a right-click in the area opens the menu at the pointer instead of the browser menu', () => {
        const { area, row, dotnetRef } = build();
        globalThis.twContextMenu.attach(area, dotnetRef);

        const event = contextMenu(row, 120, 340);

        expect(event.defaultPrevented).toBe(true);
        expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('OpenAt', 120, 340, false);
    });

    test('a disabled area leaves the browser menu alone', () => {
        const { area, row, dotnetRef } = build({ disabled: true });
        globalThis.twContextMenu.attach(area, dotnetRef);

        expect(contextMenu(row, 10, 10).defaultPrevented).toBe(false);
        expect(key(row, 'F10', { shiftKey: true }).defaultPrevented).toBe(false);
        expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();
    });

    test('a right-click outside the area is left to the browser', () => {
        const { area, before, dotnetRef } = build();
        globalThis.twContextMenu.attach(area, dotnetRef);

        expect(contextMenu(before, 10, 10).defaultPrevented).toBe(false);
        expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();
    });

    test('Shift+F10 and the Menu key open the menu under the focused element', () => {
        const { area, row, dotnetRef } = build();
        row.getBoundingClientRect = () => rect(40, 100, 80, 30);
        globalThis.twContextMenu.attach(area, dotnetRef);
        row.focus();

        expect(key(row, 'F10', { shiftKey: true }).defaultPrevented).toBe(true);
        expect(dotnetRef.invokeMethodAsync).toHaveBeenLastCalledWith('OpenAt', 40, 130, true);

        expect(key(row, 'ContextMenu').defaultPrevented).toBe(true);
        expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledTimes(2);
    });

    test('the contextmenu event that follows the Menu key does not open the menu a second time', () => {
        const { area, row, dotnetRef } = build();
        globalThis.twContextMenu.attach(area, dotnetRef);
        row.focus();

        key(row, 'ContextMenu');
        const event = contextMenu(row, 0, 0);

        expect(event.defaultPrevented).toBe(true);
        expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledTimes(1);
    });

    test('in a right to left layout the keyboard opens the menu from the focused element\'s right edge', () => {
        const { area, row, dotnetRef } = build({ dir: 'rtl' });
        row.getBoundingClientRect = () => rect(40, 100, 80, 30);
        globalThis.twContextMenu.attach(area, dotnetRef);
        row.focus();

        key(row, 'F10', { shiftKey: true });

        expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('OpenAt', 120, 130, true);
    });

    test('the keyboard opens the menu under the area when focus is not inside it', () => {
        const { area, dotnetRef } = build({ focusableArea: true });
        area.getBoundingClientRect = () => rect(10, 20, 300, 200);
        globalThis.twContextMenu.attach(area, dotnetRef);

        key(area, 'F10', { shiftKey: true });

        expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('OpenAt', 10, 220, true);
    });

    test('other keys and modified F10 are left alone', () => {
        const { area, row, dotnetRef } = build();
        globalThis.twContextMenu.attach(area, dotnetRef);

        expect(key(row, 'F10').defaultPrevented).toBe(false);
        expect(key(row, 'F10', { shiftKey: true, ctrlKey: true }).defaultPrevented).toBe(false);
        expect(key(row, 'Enter').defaultPrevented).toBe(false);
        expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();
    });

    test('a nested area opens only its own menu', () => {
        const outer = build();
        const inner = build();
        outer.area.appendChild(inner.area);
        globalThis.twContextMenu.attach(outer.area, outer.dotnetRef);
        globalThis.twContextMenu.attach(inner.area, inner.dotnetRef);

        contextMenu(inner.row, 5, 5);

        expect(inner.dotnetRef.invokeMethodAsync).toHaveBeenCalledTimes(1);
        expect(outer.dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();
    });

    test('attaching twice adds the listeners once', () => {
        const { area, dotnetRef } = build();
        const add = vi.spyOn(area, 'addEventListener');

        globalThis.twContextMenu.attach(area, dotnetRef);
        globalThis.twContextMenu.attach(area, dotnetRef);

        expect(add).toHaveBeenCalledTimes(2);
        expect(() => globalThis.twContextMenu.attach(null, dotnetRef)).not.toThrow();
    });

    test('open places the menu at the point and shows it', () => {
        const { area, menu, dotnetRef } = build();
        globalThis.twContextMenu.attach(area, dotnetRef);

        globalThis.twContextMenu.open(area, menu, dotnetRef, 120, 340, 'menu');

        expect(px(menu.style.left)).toBe(120);
        expect(px(menu.style.top)).toBe(340);
        expect(menu.style.position).toBe('fixed');
        expect(menu.style.visibility).toBe('');
        expect(document.activeElement).toBe(menu);
    });

    test('opened from the keyboard, focus goes to the first item', () => {
        const { area, menu, dotnetRef } = build();
        globalThis.twContextMenu.attach(area, dotnetRef);

        globalThis.twContextMenu.open(area, menu, dotnetRef, 10, 10, 'first');

        expect(document.activeElement.textContent).toBe('Copy');
    });

    test('near the end and bottom of the window the menu flips to the other side of the point', () => {
        const { area, menu, dotnetRef } = build();
        globalThis.twContextMenu.attach(area, dotnetRef);

        globalThis.twContextMenu.open(area, menu, dotnetRef, 950, 760, 'menu');

        expect(px(menu.style.left)).toBe(750);
        expect(px(menu.style.top)).toBe(610);
    });

    test('in a right to left layout the menu opens towards the left of the point', () => {
        const { area, menu, dotnetRef } = build({ dir: 'rtl' });
        globalThis.twContextMenu.attach(area, dotnetRef);

        globalThis.twContextMenu.open(area, menu, dotnetRef, 500, 100, 'menu');
        expect(px(menu.style.left)).toBe(300);

        globalThis.twContextMenu.open(area, menu, dotnetRef, 50, 100, 'menu');
        expect(px(menu.style.left)).toBe(50);
    });

    test('a menu that cannot fit either side is kept inside the window', () => {
        const { area, menu, dotnetRef } = build({ panelSize: { width: 600, height: 150 } });
        globalThis.twContextMenu.attach(area, dotnetRef);

        globalThis.twContextMenu.open(area, menu, dotnetRef, 500, 100, 'menu');

        expect(px(menu.style.left)).toBe(viewport.width - 600 - globalThis.twPicker._panelEdgeGapPx);
    });

    test('a menu taller than the window is capped so it scrolls', () => {
        const { area, menu, dotnetRef } = build({ panelSize: { width: 200, height: 2000 } });
        globalThis.twContextMenu.attach(area, dotnetRef);

        globalThis.twContextMenu.open(area, menu, dotnetRef, 10, 10, 'menu');

        expect(menu.style.maxHeight).toBe((viewport.height - 2 * globalThis.twPicker._panelEdgeGapPx) + 'px');
    });

    test('a press outside the menu dismisses it, and only once', () => {
        const { area, menu, before, dotnetRef } = build();
        globalThis.twContextMenu.attach(area, dotnetRef);
        globalThis.twContextMenu.open(area, menu, dotnetRef, 10, 10, 'menu');

        pointerDown(menu.querySelector('button'));
        expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();

        pointerDown(before);
        pointerDown(before);

        expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledTimes(1);
        expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('Dismiss');
    });

    test('a left click in the area dismisses the menu, a right-click there moves it instead', () => {
        const { area, menu, row, dotnetRef } = build();
        globalThis.twContextMenu.attach(area, dotnetRef);
        globalThis.twContextMenu.open(area, menu, dotnetRef, 10, 10, 'menu');

        pointerDown(row, 2);
        expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();

        pointerDown(row, 0);
        expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('Dismiss');
    });

    test('a page scroll or resize dismisses the menu; a scroll inside it does not', () => {
        const { area, menu, dotnetRef } = build();
        globalThis.twContextMenu.attach(area, dotnetRef);
        globalThis.twContextMenu.open(area, menu, dotnetRef, 10, 10, 'menu');

        menu.dispatchEvent(new Event('scroll'));
        expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();

        globalThis.dispatchEvent(new Event('resize'));
        expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('Dismiss');
    });

    test('Escape closes the menu through the shared Escape stack', () => {
        const { area, menu, dotnetRef } = build();
        globalThis.twContextMenu.attach(area, dotnetRef);
        globalThis.twContextMenu.open(area, menu, dotnetRef, 10, 10, 'first');

        key(document.activeElement, 'Escape');

        expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('Close');
    });

    test('Tab dismisses the menu, and the browser menu never opens over it', () => {
        const { area, menu, dotnetRef } = build();
        globalThis.twContextMenu.attach(area, dotnetRef);
        globalThis.twContextMenu.open(area, menu, dotnetRef, 10, 10, 'first');

        expect(contextMenu(document.activeElement, 0, 0).defaultPrevented).toBe(true);
        key(document.activeElement, 'Tab');

        expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('Dismiss');
    });

    test('opening again while open moves the menu without adding listeners', () => {
        const { area, menu, dotnetRef } = build();
        globalThis.twContextMenu.attach(area, dotnetRef);
        const add = vi.spyOn(document, 'addEventListener');
        globalThis.twContextMenu.open(area, menu, dotnetRef, 10, 10, 'menu');

        globalThis.twContextMenu.open(area, menu, dotnetRef, 300, 200, 'menu');

        expect(add.mock.calls.filter((call) => call[0] === 'pointerdown')).toHaveLength(1);
        expect(px(menu.style.left)).toBe(300);
        expect(px(menu.style.top)).toBe(200);
    });

    test('close returns focus to the element that had it before the menu opened', () => {
        const { area, menu, row, dotnetRef } = build();
        globalThis.twContextMenu.attach(area, dotnetRef);
        row.focus();
        globalThis.twContextMenu.open(area, menu, dotnetRef, 10, 10, 'first');
        expect(document.activeElement.textContent).toBe('Copy');

        globalThis.twContextMenu.close(area, menu, true);

        expect(document.activeElement).toBe(row);
        expect(globalThis.twDialog._escapeStack).toHaveLength(0);
    });

    test('close without returning focus leaves focus alone and removes the listeners', () => {
        const { area, menu, row, before, dotnetRef } = build();
        globalThis.twContextMenu.attach(area, dotnetRef);
        row.focus();
        globalThis.twContextMenu.open(area, menu, dotnetRef, 10, 10, 'first');

        globalThis.twContextMenu.close(area, menu, false);
        pointerDown(before);

        expect(document.activeElement.textContent).toBe('Copy');
        expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();
    });

    test('with nothing focused before, close returns focus to a focusable area', () => {
        const { area, menu, dotnetRef } = build({ focusableArea: true });
        globalThis.twContextMenu.attach(area, dotnetRef);
        globalThis.twContextMenu.open(area, menu, dotnetRef, 10, 10, 'first');

        globalThis.twContextMenu.close(area, menu, true);

        expect(document.activeElement).toBe(area);
    });

    test('close and open do nothing for an area that was never attached', () => {
        const { area, menu, dotnetRef } = build();

        expect(() => globalThis.twContextMenu.open(area, menu, dotnetRef, 0, 0, 'menu')).not.toThrow();
        expect(() => globalThis.twContextMenu.close(area, menu, true)).not.toThrow();
        expect(() => globalThis.twContextMenu.close(null, null, true)).not.toThrow();
        expect(() => globalThis.twContextMenu.detach(null)).not.toThrow();
        expect(() => globalThis.twContextMenu.place(area, null, 0, 0)).not.toThrow();
    });

    test('detach removes the area\'s listeners and those of an open menu', () => {
        const { area, menu, row, before, dotnetRef } = build();
        globalThis.twContextMenu.attach(area, dotnetRef);
        globalThis.twContextMenu.open(area, menu, dotnetRef, 10, 10, 'menu');

        globalThis.twContextMenu.detach(area);
        pointerDown(before);

        expect(contextMenu(row, 1, 1).defaultPrevented).toBe(false);
        expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();
        expect(globalThis.twDialog._escapeStack).toHaveLength(0);
    });
});
