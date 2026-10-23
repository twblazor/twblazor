// @vitest-environment jsdom

import { afterEach, describe, expect, test } from 'vitest';

await import('../../../src/TwBlazor/wwwroot/js/twblazor.js');

// A page with a button that opens the drawer, the drawer itself, and an empty dialog host. jsdom has no layout,
// so every element reports a box and counts as visible to the focus helpers.
function build(panelHtml) {
    document.body.innerHTML =
        '<main><button id="opener">Open</button><a href="#x" id="page-link">Link</a></main>' +
        '<div data-tw-dialog-host></div>' +
        '<div id="root"><div id="backdrop"></div><div id="panel" role="dialog" tabindex="-1">' + panelHtml + '</div></div>';
    document.querySelectorAll('*').forEach(function (el) { el.getClientRects = () => [{}]; });
    document.getElementById('opener').focus();
    return {
        root: document.getElementById('root'),
        panel: document.getElementById('panel'),
        main: document.querySelector('main'),
        host: document.querySelector('[data-tw-dialog-host]')
    };
}

function press(target, key, shiftKey) {
    const event = new KeyboardEvent('keydown', { key: key, shiftKey: !!shiftKey, bubbles: true, cancelable: true });
    target.dispatchEvent(event);
    return event;
}

describe('twDrawer', () => {
    afterEach(() => {
        document.body.innerHTML = '';
        document.documentElement.style.overflow = '';
        document.documentElement.style.scrollbarGutter = '';
        globalThis.twDrawer._scrollLocks = 0;
        globalThis.twDialog._escapeStack.length = 0;
        globalThis.twDialog._trapStack.length = 0;
    });

    test('a modal drawer makes the page inert but leaves the dialog host usable', () => {
        const { root, panel, main, host } = build('<input id="first">');

        globalThis.twDrawer.open(root, panel, null, { modal: true, closeOnEscape: false });

        expect(main.hasAttribute('inert')).toBe(true);
        expect(host.hasAttribute('inert')).toBe(false);
        expect(root.hasAttribute('inert')).toBe(false);
        expect(document.documentElement.style.overflow).toBe('hidden');
    });

    test('focus moves to the first control, skipping the close button', () => {
        const { root, panel } = build('<button data-tw-drawer-close>Close</button><input id="first">');

        globalThis.twDrawer.open(root, panel, null, { modal: true });

        expect(document.activeElement.id).toBe('first');
    });

    test('focus moves to the element marked data-tw-autofocus, or the control inside it', () => {
        const { root, panel } = build('<input id="first"><div data-tw-autofocus><input id="marked"></div>');

        globalThis.twDrawer.open(root, panel, null, { modal: true });

        expect(document.activeElement.id).toBe('marked');
    });

    test('with no controls, focus moves to the panel itself', () => {
        const { root, panel } = build('<p>Just text</p>');

        globalThis.twDrawer.open(root, panel, null, { modal: false });

        expect(document.activeElement).toBe(panel);
    });

    test('Tab wraps inside a modal drawer', () => {
        const { root, panel } = build('<input id="first"><input id="last">');
        globalThis.twDrawer.open(root, panel, null, { modal: true });
        document.getElementById('last').focus();

        const event = press(document.getElementById('last'), 'Tab');

        expect(event.defaultPrevented).toBe(true);
        expect(document.activeElement.id).toBe('first');
    });

    test('a non-modal drawer leaves the page usable and does not lock scrolling', () => {
        const { root, panel, main } = build('<input id="first">');

        globalThis.twDrawer.open(root, panel, null, { modal: false });

        expect(main.hasAttribute('inert')).toBe(false);
        expect(document.documentElement.style.overflow).toBe('');
        expect(press(document.getElementById('first'), 'Tab').defaultPrevented).toBe(false);
    });

    test('Escape asks .NET to close the drawer', () => {
        const { root, panel } = build('<input id="first">');
        const calls = [];
        const dotnetRef = { invokeMethodAsync: (method) => { calls.push(method); return Promise.resolve(); } };

        globalThis.twDrawer.open(root, panel, dotnetRef, { modal: true, closeOnEscape: true });
        press(document.getElementById('first'), 'Escape');

        expect(calls).toEqual(['CloseFromEscape']);
    });

    test('closing undoes everything and returns focus to the opener', () => {
        const { root, panel, main } = build('<input id="first">');
        globalThis.twDrawer.open(root, panel, null, { modal: true, closeOnEscape: true });

        globalThis.twDrawer.close(panel, true);

        expect(main.hasAttribute('inert')).toBe(false);
        expect(document.documentElement.style.overflow).toBe('');
        expect(globalThis.twDialog._escapeStack).toHaveLength(0);
        expect(globalThis.twDialog._trapStack).toHaveLength(0);
        expect(document.activeElement.id).toBe('opener');
    });

    test('closing leaves focus alone when it has already moved out of a non-modal drawer', () => {
        const { root, panel } = build('<input id="first">');
        globalThis.twDrawer.open(root, panel, null, { modal: false });
        document.getElementById('page-link').focus();

        globalThis.twDrawer.close(panel, true);

        expect(document.activeElement.id).toBe('page-link');
        expect(document.querySelector('[data-tw-focus-token]')).toBeNull();
    });

    test('two modal drawers share the scroll lock', () => {
        const first = build('<input>');
        globalThis.twDrawer.open(first.root, first.panel, null, { modal: true });
        const second = document.createElement('div');
        second.innerHTML = '<input>';
        second.querySelectorAll('*').forEach(function (el) { el.getClientRects = () => [{}]; });
        document.body.appendChild(second);
        globalThis.twDrawer.open(second, second, null, { modal: true });

        globalThis.twDrawer.close(first.panel, false);
        expect(document.documentElement.style.overflow).toBe('hidden');

        globalThis.twDrawer.close(second, false);
        expect(document.documentElement.style.overflow).toBe('');
    });

    test('open and close ignore a missing panel and a second open', () => {
        const { root, panel } = build('<input>');

        globalThis.twDrawer.open(null, panel, null, { modal: true });
        globalThis.twDrawer.close(null, true);
        globalThis.twDrawer.open(root, panel, null, { modal: true });
        globalThis.twDrawer.open(root, panel, null, { modal: true });

        expect(globalThis.twDrawer._scrollLocks).toBe(1);
    });
});
