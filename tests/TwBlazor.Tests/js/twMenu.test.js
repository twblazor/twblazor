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

    test('opening twice adds the listeners once, and closing removes them', () => {
        const { menu } = buildMenu();
        const add = vi.spyOn(menu, 'addEventListener');
        globalThis.twMenu.open(menu, null, null, 'first');
        globalThis.twMenu.open(menu, null, null, 'first');

        expect(add.mock.calls.map((call) => call[0])).toEqual(['keydown', 'click', 'pointerover']);

        globalThis.twMenu.close(menu);
        key(menu.querySelector('[role="menuitem"]'), 'ArrowDown');
        expect(focusedText()).toBe('Edit');
        expect(() => globalThis.twMenu.close(null)).not.toThrow();
        expect(() => globalThis.twMenu.open(null)).not.toThrow();
    });
});

function buildSubmenuMenu(dir) {
    const root = document.createElement('div');
    if (dir) root.setAttribute('dir', dir);
    root.innerHTML = `
        <div role="menu" tabindex="-1" data-tw-popover>
            <button role="menuitem" tabindex="-1">Back</button>
            <div role="none" data-tw-submenu>
                <button id="more" role="menuitem" tabindex="-1" aria-haspopup="menu" aria-expanded="false">More tools</button>
                <div role="menu" tabindex="-1" hidden>
                    <button role="menuitem" tabindex="-1">Save page</button>
                    <div role="none" data-tw-submenu>
                        <button id="dev" role="menuitem" tabindex="-1" aria-haspopup="menu" aria-expanded="false">Developer</button>
                        <div role="menu" tabindex="-1" hidden>
                            <button role="menuitem" tabindex="-1">Console</button>
                        </div>
                    </div>
                    <button role="menuitem" tabindex="-1">Name window</button>
                </div>
            </div>
            <div role="none" data-tw-submenu>
                <button id="off" role="menuitem" tabindex="-1" aria-haspopup="menu" aria-expanded="false" aria-disabled="true">Share</button>
                <div role="menu" tabindex="-1" hidden>
                    <button role="menuitem" tabindex="-1">Email</button>
                </div>
            </div>
            <button role="menuitem" tabindex="-1">Reload</button>
        </div>`;
    document.body.appendChild(root);
    const menu = root.querySelector('[role="menu"]');
    const more = root.querySelector('#more');
    const dev = root.querySelector('#dev');
    return { root, menu, more, dev, moreMenu: more.nextElementSibling, devMenu: dev.nextElementSibling, off: root.querySelector('#off') };
}

function pointerOver(target, pointerType = 'mouse') {
    const event = new Event('pointerover', { bubbles: true });
    event.pointerType = pointerType;
    target.dispatchEvent(event);
}

describe('twMenu submenus', () => {
    afterEach(() => {
        vi.useRealTimers();
        vi.restoreAllMocks();
        document.body.innerHTML = '';
    });

    test('the arrow keys move between the items of the focused menu only', () => {
        const { menu } = buildSubmenuMenu();
        globalThis.twMenu.open(menu, null, null, 'first');

        key(document.activeElement, 'ArrowDown');
        expect(focusedText()).toBe('More tools');
        key(document.activeElement, 'ArrowDown');
        expect(focusedText()).toBe('Reload');
        key(document.activeElement, 'End');
        expect(focusedText()).toBe('Reload');
    });

    test('Arrow Right opens a submenu on its first item and Arrow Left closes it again', () => {
        const { menu, more, moreMenu } = buildSubmenuMenu();
        const position = vi.spyOn(globalThis.twPopover, 'position');
        globalThis.twMenu.open(menu, null, null, 'first');
        more.focus();

        expect(key(more, 'ArrowRight').defaultPrevented).toBe(true);
        expect(more.getAttribute('aria-expanded')).toBe('true');
        expect(moreMenu.hidden).toBe(false);
        expect(position).toHaveBeenCalledWith(more, moreMenu, 'end', 'start', 0);
        expect(focusedText()).toBe('Save page');

        key(document.activeElement, 'ArrowDown');
        expect(focusedText()).toBe('Developer');

        expect(key(document.activeElement, 'ArrowLeft').defaultPrevented).toBe(true);
        expect(moreMenu.hidden).toBe(true);
        expect(more.getAttribute('aria-expanded')).toBe('false');
        expect(document.activeElement).toBe(more);
    });

    test('in a right to left layout the two arrows swap', () => {
        const { menu, more, moreMenu } = buildSubmenuMenu('rtl');
        globalThis.twMenu.open(menu, null, null, 'first');
        more.focus();

        expect(key(more, 'ArrowRight').defaultPrevented).toBe(false);
        expect(moreMenu.hidden).toBe(true);

        key(more, 'ArrowLeft');
        expect(focusedText()).toBe('Save page');

        key(document.activeElement, 'ArrowRight');
        expect(moreMenu.hidden).toBe(true);
        expect(document.activeElement).toBe(more);
    });

    test('Escape closes only the submenu and is kept from the menu around it', () => {
        const { menu, more, moreMenu, dev, devMenu } = buildSubmenuMenu();
        globalThis.twMenu.open(menu, null, null, 'first');
        globalThis.twMenu.openSubmenu(more, true);
        dev.focus();
        key(dev, 'ArrowRight');
        expect(focusedText()).toBe('Console');

        const escape = key(document.activeElement, 'Escape');

        expect(escape.defaultPrevented).toBe(true);
        expect(devMenu.hidden).toBe(true);
        expect(moreMenu.hidden).toBe(false);
        expect(document.activeElement).toBe(dev);
    });

    test('Escape and the outward arrow in the top menu are left alone', () => {
        const { menu } = buildSubmenuMenu();
        globalThis.twMenu.open(menu, null, null, 'first');

        expect(key(document.activeElement, 'Escape').defaultPrevented).toBe(false);
        expect(key(document.activeElement, 'ArrowLeft').defaultPrevented).toBe(false);
        expect(key(document.activeElement, 'ArrowRight').defaultPrevented).toBe(false);
        expect(focusedText()).toBe('Back');
    });

    test('closing a submenu closes the submenus inside it', () => {
        const { more, moreMenu, dev, devMenu } = buildSubmenuMenu();
        globalThis.twMenu.openSubmenu(more, false);
        globalThis.twMenu.openSubmenu(dev, true);

        globalThis.twMenu.closeSubmenu(more, false);

        expect(devMenu.hidden).toBe(true);
        expect(dev.getAttribute('aria-expanded')).toBe('false');
        expect(moreMenu.hidden).toBe(true);
        // Focus was inside the closed submenu, so it goes back to its item instead of being lost.
        expect(document.activeElement).toBe(more);
        expect(() => globalThis.twMenu.closeSubmenu(null, true)).not.toThrow();
    });

    test('a disabled submenu item does not open', () => {
        const { menu, off } = buildSubmenuMenu();
        globalThis.twMenu.open(menu, null, null, 'first');

        off.click();
        globalThis.twMenu.openSubmenu(off, true);

        expect(off.getAttribute('aria-expanded')).toBe('false');
        expect(off.nextElementSibling.hidden).toBe(true);
    });

    test('a click opens the submenu: from the keyboard into it, from a pointer leaving focus on the item', () => {
        const { menu, more } = buildSubmenuMenu();
        globalThis.twMenu.open(menu, null, null, 'first');
        more.focus();

        more.dispatchEvent(new MouseEvent('click', { bubbles: true, detail: 1 }));
        expect(more.getAttribute('aria-expanded')).toBe('true');
        expect(document.activeElement).toBe(more);

        more.dispatchEvent(new MouseEvent('click', { bubbles: true, detail: 0 }));
        expect(focusedText()).toBe('Save page');
    });

    test('moving to another item closes a submenu opened by hovering', () => {
        const { menu, more, moreMenu } = buildSubmenuMenu();
        globalThis.twMenu.open(menu, null, null, 'first');
        globalThis.twMenu.openSubmenu(more, false);
        more.focus();

        key(more, 'ArrowDown');

        expect(focusedText()).toBe('Reload');
        expect(moreMenu.hidden).toBe(true);
    });

    test('resting the pointer on an item opens its submenu and on another item closes it', () => {
        vi.useFakeTimers();
        const { menu, more, moreMenu } = buildSubmenuMenu();
        globalThis.twMenu.open(menu, null, null, 'first');

        pointerOver(more);
        expect(moreMenu.hidden).toBe(true);
        vi.advanceTimersByTime(globalThis.twMenu._hoverDelay);
        expect(moreMenu.hidden).toBe(false);
        expect(focusedText()).toBe('Back');

        pointerOver(menu.querySelector('button'));
        vi.advanceTimersByTime(globalThis.twMenu._hoverDelay);
        expect(moreMenu.hidden).toBe(true);
    });

    test('crossing another item on the way into an open submenu keeps it open', () => {
        vi.useFakeTimers();
        const { menu, more, moreMenu } = buildSubmenuMenu();
        globalThis.twMenu.open(menu, null, null, 'first');
        globalThis.twMenu.openSubmenu(more, false);

        pointerOver(menu.querySelector('button'));
        pointerOver(moreMenu.querySelector('button'));
        vi.advanceTimersByTime(globalThis.twMenu._hoverDelay);

        expect(moreMenu.hidden).toBe(false);
    });

    test('touch does not open submenus by hovering, and closing the menu stops a pending hover', () => {
        vi.useFakeTimers();
        const { menu, more, moreMenu } = buildSubmenuMenu();
        globalThis.twMenu.open(menu, null, null, 'first');

        pointerOver(more, 'touch');
        vi.advanceTimersByTime(globalThis.twMenu._hoverDelay);
        expect(moreMenu.hidden).toBe(true);

        pointerOver(more);
        globalThis.twMenu.close(menu);
        vi.advanceTimersByTime(globalThis.twMenu._hoverDelay);
        expect(moreMenu.hidden).toBe(true);
    });

    test('focusInitial can focus the menu itself, from where Arrow Down reaches the first item', () => {
        const { menu } = buildSubmenuMenu();
        globalThis.twMenu.attach(menu, null, 'Dismiss');

        globalThis.twMenu.focusInitial(menu, 'menu');
        expect(document.activeElement).toBe(menu);

        key(menu, 'ArrowDown');
        expect(focusedText()).toBe('Back');
        expect(() => globalThis.twMenu.focusInitial(null, 'first')).not.toThrow();
        expect(() => globalThis.twMenu.detach(null)).not.toThrow();
    });

    test('Tab calls the close method the menu was attached with', () => {
        const { menu } = buildSubmenuMenu();
        const dotnetRef = { invokeMethodAsync: vi.fn() };
        globalThis.twMenu.attach(menu, dotnetRef, 'Dismiss');
        globalThis.twMenu.focusInitial(menu, 'first');

        key(document.activeElement, 'Tab');

        expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('Dismiss');
    });
});
