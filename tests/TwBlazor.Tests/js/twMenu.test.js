// @vitest-environment jsdom

import { afterEach, describe, expect, test, vi } from 'vitest';

// Load the script once - it assigns window.twMenu on evaluation.
await import('../../../src/TwBlazor/wwwroot/js/twblazor.js');

function buildMenu() {
    const root = document.createElement('div');
    root.innerHTML = `
        <span><button id="trigger" aria-haspopup="menu">Actions</button></span>
        <div role="menu" tabindex="-1" data-tw-popover>
            <button role="menuitem" tabindex="-1">Edit</button>
            <button role="menuitem" tabindex="-1" aria-disabled="true">Delete</button>
            <div role="separator"></div>
            <div role="group">
                <div role="presentation">Show</div>
                <button role="menuitemcheckbox" tabindex="-1" aria-checked="true">Grid</button>
                <button role="menuitemcheckbox" tabindex="-1" aria-checked="false">Guides</button>
            </div>
        </div>`;
    document.body.appendChild(root);
    // jsdom has no layout, so the visibility filter needs every element to report a box.
    root.querySelectorAll('*').forEach(function (el) { el.getClientRects = () => [{}]; });
    return { root, menu: root.querySelector('[role="menu"]'), trigger: root.querySelector('span') };
}

function key(target, keyName, extra = {}) {
    const event = new KeyboardEvent('keydown', { key: keyName, bubbles: true, cancelable: true, ...extra });
    target.dispatchEvent(event);
    return event;
}

function focusedText() {
    return document.activeElement?.textContent.trim();
}

describe('twMenu', () => {
    afterEach(() => {
        vi.restoreAllMocks();
        document.body.innerHTML = '';
    });

    test('open focuses the first item, or the last one', () => {
        const { menu, trigger } = buildMenu();
        vi.spyOn(globalThis.twPicker, 'registerScrollReposition').mockImplementation(() => { });

        globalThis.twMenu.open(menu, trigger, null, 'first');
        expect(focusedText()).toBe('Edit');

        globalThis.twMenu.close(menu);
        globalThis.twMenu.open(menu, trigger, null, 'last');
        expect(focusedText()).toBe('Guides');
    });

    test('open positions the menu against its button and close stops tracking it', () => {
        const { menu, trigger } = buildMenu();
        const dotnetRef = { invokeMethodAsync: vi.fn() };
        const register = vi.spyOn(globalThis.twPicker, 'registerScrollReposition').mockImplementation(() => { });
        const unregister = vi.spyOn(globalThis.twPicker, 'unregisterScrollReposition').mockImplementation(() => { });

        globalThis.twMenu.open(menu, trigger, dotnetRef, 'first');
        globalThis.twMenu.close(menu);

        expect(register).toHaveBeenCalledWith(trigger, menu, false, dotnetRef);
        expect(unregister).toHaveBeenCalledWith(menu);
    });

    test('the arrow keys move between items, skip disabled ones and wrap round', () => {
        const { menu } = buildMenu();
        globalThis.twMenu.open(menu, null, null, 'first');

        expect(key(document.activeElement, 'ArrowDown').defaultPrevented).toBe(true);
        expect(focusedText()).toBe('Grid');
        key(document.activeElement, 'ArrowDown');
        expect(focusedText()).toBe('Guides');
        key(document.activeElement, 'ArrowDown');
        expect(focusedText()).toBe('Edit');
        key(document.activeElement, 'ArrowUp');
        expect(focusedText()).toBe('Guides');
    });

    test('Home and End move to the first and last items', () => {
        const { menu } = buildMenu();
        globalThis.twMenu.open(menu, null, null, 'first');

        key(document.activeElement, 'End');
        expect(focusedText()).toBe('Guides');
        key(document.activeElement, 'Home');
        expect(focusedText()).toBe('Edit');
    });

    test('typing a letter moves to the next item that starts with it', () => {
        const { menu } = buildMenu();
        globalThis.twMenu.open(menu, null, null, 'first');

        key(document.activeElement, 'g');
        expect(focusedText()).toBe('Grid');
        key(document.activeElement, 'G');
        expect(focusedText()).toBe('Guides');
        expect(key(document.activeElement, 'z').defaultPrevented).toBe(false);
        expect(focusedText()).toBe('Guides');
    });

    test('Tab closes the menu without stopping focus moving on', () => {
        const { menu } = buildMenu();
        const dotnetRef = { invokeMethodAsync: vi.fn() };
        globalThis.twMenu.open(menu, null, dotnetRef, 'first');

        const tab = key(document.activeElement, 'Tab');

        expect(tab.defaultPrevented).toBe(false);
        expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('CloseFromTab');
    });

    test('leaves Space, Enter and modified keys to the browser', () => {
        const { menu } = buildMenu();
        globalThis.twMenu.open(menu, null, null, 'first');

        expect(key(document.activeElement, ' ').defaultPrevented).toBe(false);
        expect(key(document.activeElement, 'Enter').defaultPrevented).toBe(false);
        expect(key(document.activeElement, 'g', { ctrlKey: true }).defaultPrevented).toBe(false);
        expect(focusedText()).toBe('Edit');
    });

    test('a menu without items takes focus itself', () => {
        const menu = document.createElement('div');
        menu.tabIndex = -1;
        document.body.appendChild(menu);

        globalThis.twMenu.open(menu, null, null, 'first');

        expect(document.activeElement).toBe(menu);
        expect(key(menu, 'ArrowDown').defaultPrevented).toBe(false);
    });

    test('focus moves to the button inside the trigger', () => {
        const { trigger } = buildMenu();

        globalThis.twMenu.focus(trigger);

        expect(document.activeElement.id).toBe('trigger');
        expect(() => globalThis.twMenu.focus(null)).not.toThrow();
    });

    test('opening twice adds one listener, and closing removes it', () => {
        const { menu } = buildMenu();
        const add = vi.spyOn(menu, 'addEventListener');
        globalThis.twMenu.open(menu, null, null, 'first');
        globalThis.twMenu.open(menu, null, null, 'first');

        expect(add).toHaveBeenCalledTimes(1);

        globalThis.twMenu.close(menu);
        key(menu.querySelector('[role="menuitem"]'), 'ArrowDown');
        expect(focusedText()).toBe('Edit');
        expect(() => globalThis.twMenu.close(null)).not.toThrow();
        expect(() => globalThis.twMenu.open(null)).not.toThrow();
    });
});
