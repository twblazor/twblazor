// @vitest-environment jsdom

import { afterEach, beforeEach, describe, expect, test, vi } from 'vitest';

await import('../../../src/TwBlazor/wwwroot/js/twblazor.js');

const viewport = { width: 1000, height: 800 };

function rect(left, top, width, height) {
    return { left, top, width, height, right: left + width, bottom: top + height, x: left, y: top };
}

// Builds a trigger and a panel with fixed sizes, since jsdom has no layout.
function build(anchorRect, panelSize, dir) {
    const root = document.createElement('div');
    if (dir) root.setAttribute('dir', dir);
    root.innerHTML = '<span><button id="trigger">Open</button></span><div role="dialog" tabindex="-1" data-tw-popover><button id="inside">Inside</button></div>';
    document.body.appendChild(root);
    const anchor = root.querySelector('span');
    const panel = root.querySelector('[role="dialog"]');
    anchor.getBoundingClientRect = () => anchorRect;
    panel.getBoundingClientRect = () => rect(0, 0, panelSize.width, panelSize.height);
    root.querySelectorAll('*').forEach(function (el) { el.getClientRects ??= () => [{}]; });
    panel.querySelectorAll('*').forEach(function (el) { el.getClientRects = () => [{}]; });
    return { root, anchor, panel };
}

function px(value) {
    return Number.parseFloat(value);
}

describe('twPopover.position', () => {
    beforeEach(() => {
        Object.defineProperty(document.documentElement, 'clientWidth', { configurable: true, value: viewport.width });
        Object.defineProperty(document.documentElement, 'clientHeight', { configurable: true, value: viewport.height });
        vi.spyOn(globalThis.twPicker, '_fixedContainingBlock').mockReturnValue({ left: 0, top: 0, right: viewport.width, bottom: viewport.height });
    });

    afterEach(() => {
        vi.restoreAllMocks();
        document.body.innerHTML = '';
    });

    test('opens below the trigger, lined up with its start', () => {
        const { anchor, panel } = build(rect(100, 100, 80, 30), { width: 200, height: 100 });

        expect(globalThis.twPopover.position(anchor, panel, 'bottom', 'start', 8)).toBe('bottom');
        expect(panel.style.position).toBe('fixed');
        expect(px(panel.style.left)).toBe(100);
        expect(px(panel.style.top)).toBe(138);
        expect(panel.dataset.twSide).toBe('bottom');
    });

    test('centres and end-aligns along the trigger', () => {
        const { anchor, panel } = build(rect(300, 100, 100, 30), { width: 200, height: 100 });

        globalThis.twPopover.position(anchor, panel, 'bottom', 'center', 8);
        expect(px(panel.style.left)).toBe(250);

        globalThis.twPopover.position(anchor, panel, 'bottom', 'end', 8);
        expect(px(panel.style.left)).toBe(200);
    });

    test('opens above the trigger', () => {
        const { anchor, panel } = build(rect(100, 400, 80, 30), { width: 200, height: 100 });

        expect(globalThis.twPopover.position(anchor, panel, 'top', 'start', 8)).toBe('top');
        expect(px(panel.style.top)).toBe(292);
    });

    test('flips to the other side when there is no room', () => {
        const { anchor, panel } = build(rect(100, 740, 80, 30), { width: 200, height: 100 });

        expect(globalThis.twPopover.position(anchor, panel, 'bottom', 'start', 8)).toBe('top');
        expect(px(panel.style.top)).toBe(632);
    });

    test('caps the height when neither side has room', () => {
        const { anchor, panel } = build(rect(100, 100, 80, 30), { width: 200, height: 900 });

        globalThis.twPopover.position(anchor, panel, 'bottom', 'start', 8);
        expect(px(panel.style.maxHeight)).toBe(800 - 138 - 8);
    });

    test('keeps the panel inside the viewport across the trigger', () => {
        const { anchor, panel } = build(rect(950, 100, 40, 30), { width: 200, height: 100 });

        globalThis.twPopover.position(anchor, panel, 'bottom', 'start', 8);
        expect(px(panel.style.left)).toBe(1000 - 200 - 8);
    });

    test('start and end follow the layout direction', () => {
        const ltr = build(rect(400, 300, 80, 30), { width: 120, height: 60 });
        expect(globalThis.twPopover.position(ltr.anchor, ltr.panel, 'start', 'start', 8)).toBe('left');
        expect(px(ltr.panel.style.left)).toBe(400 - 8 - 120);
        expect(globalThis.twPopover.position(ltr.anchor, ltr.panel, 'end', 'start', 8)).toBe('right');
        expect(px(ltr.panel.style.left)).toBe(488);

        const rtl = build(rect(400, 300, 80, 30), { width: 120, height: 60 }, 'rtl');
        expect(globalThis.twPopover.position(rtl.anchor, rtl.panel, 'start', 'start', 8)).toBe('right');
        expect(globalThis.twPopover.position(rtl.anchor, rtl.panel, 'end', 'start', 8)).toBe('left');
    });

    test('in a right to left layout, start alignment lines up with the trigger right edge', () => {
        const { anchor, panel } = build(rect(400, 100, 100, 30), { width: 200, height: 100 }, 'rtl');

        globalThis.twPopover.position(anchor, panel, 'bottom', 'start', 8);
        expect(px(panel.style.left)).toBe(300);
    });

    test('ignores missing elements', () => {
        expect(globalThis.twPopover.position(null, null, 'bottom', 'start', 8)).toBeNull();
    });
});

describe('twPopover.open and close', () => {
    beforeEach(() => {
        Object.defineProperty(document.documentElement, 'clientWidth', { configurable: true, value: viewport.width });
        Object.defineProperty(document.documentElement, 'clientHeight', { configurable: true, value: viewport.height });
        vi.spyOn(globalThis.twPicker, '_fixedContainingBlock').mockReturnValue({ left: 0, top: 0, right: viewport.width, bottom: viewport.height });
    });

    afterEach(() => {
        vi.restoreAllMocks();
        document.body.innerHTML = '';
        globalThis.twDialog._escapeStack.length = 0;
    });

    test('focuses the panel content and claims Escape', async () => {
        const { root, anchor, panel } = build(rect(100, 100, 80, 30), { width: 200, height: 100 });
        const dotnetRef = { invokeMethodAsync: vi.fn().mockResolvedValue(undefined) };

        globalThis.twPopover.open(root, anchor, panel, dotnetRef, { placement: 'bottom', align: 'start', offset: 8 });

        expect(document.activeElement.id).toBe('inside');
        document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
        await Promise.resolve();
        expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('CloseFromEscape');

        globalThis.twPopover.close(root, panel);
    });

    test('leaves focus alone without autoFocus', () => {
        const { root, anchor, panel } = build(rect(100, 100, 80, 30), { width: 200, height: 100 });
        document.getElementById('trigger').focus();

        globalThis.twPopover.open(root, anchor, panel, { invokeMethodAsync: vi.fn() }, { autoFocus: false });

        expect(document.activeElement.id).toBe('trigger');
        globalThis.twPopover.close(root, panel);
    });

    test('closes when focus moves outside it', async () => {
        const { root, anchor, panel } = build(rect(100, 100, 80, 30), { width: 200, height: 100 });
        const outside = document.createElement('button');
        document.body.appendChild(outside);
        const dotnetRef = { invokeMethodAsync: vi.fn().mockResolvedValue(undefined) };
        globalThis.twPopover.open(root, anchor, panel, dotnetRef, {});

        panel.querySelector('#inside').dispatchEvent(new FocusEvent('focusout', { bubbles: true, relatedTarget: document.getElementById('trigger') }));
        expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();

        panel.querySelector('#inside').dispatchEvent(new FocusEvent('focusout', { bubbles: true, relatedTarget: outside }));
        await Promise.resolve();
        expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('CloseFromFocusOut');

        globalThis.twPopover.close(root, panel);
    });

    test('registers an outside click unless told not to', () => {
        const register = vi.spyOn(globalThis.twPicker, 'registerOutsideClick');
        const first = build(rect(100, 100, 80, 30), { width: 200, height: 100 });
        globalThis.twPopover.open(first.root, first.anchor, first.panel, { invokeMethodAsync: vi.fn() }, {});
        expect(register).toHaveBeenCalledTimes(1);
        globalThis.twPopover.close(first.root, first.panel);

        const second = build(rect(100, 100, 80, 30), { width: 200, height: 100 });
        globalThis.twPopover.open(second.root, second.anchor, second.panel, { invokeMethodAsync: vi.fn() }, { closeOnOutsideClick: false });
        expect(register).toHaveBeenCalledTimes(1);
        globalThis.twPopover.close(second.root, second.panel);
    });

    test('follows the trigger on scroll until closed', () => {
        const { root, anchor, panel } = build(rect(100, 100, 80, 30), { width: 200, height: 100 });
        globalThis.twPopover.open(root, anchor, panel, { invokeMethodAsync: vi.fn() }, { autoFocus: false });
        const position = vi.spyOn(globalThis.twPopover, 'position');

        document.dispatchEvent(new Event('scroll'));
        expect(position).toHaveBeenCalledTimes(1);

        panel.dispatchEvent(new Event('scroll'));
        expect(position).toHaveBeenCalledTimes(1);

        globalThis.twPopover.close(root, panel);
        document.dispatchEvent(new Event('scroll'));
        expect(position).toHaveBeenCalledTimes(1);
        expect(panel.__twPopoverReposition).toBeUndefined();
        expect(root.__twPopoverFocusOut).toBeUndefined();
    });

    test('open and close ignore missing elements', () => {
        expect(() => globalThis.twPopover.open(null, null, null, null, {})).not.toThrow();
        expect(() => globalThis.twPopover.close(null, null)).not.toThrow();
    });
});
