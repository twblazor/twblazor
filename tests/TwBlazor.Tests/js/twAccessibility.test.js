// @vitest-environment jsdom

import { beforeEach, afterEach, describe, expect, test, vi } from 'vitest';

await import('../../../src/TwBlazor/wwwroot/js/twblazor.js');

// jsdom does no layout, so every element reports no client rects and would count as hidden.
const originalGetClientRects = Element.prototype.getClientRects;

beforeEach(() => {
    Element.prototype.getClientRects = function () {
        return this.hasAttribute('hidden') ? [] : [{}];
    };
});

afterEach(() => {
    Element.prototype.getClientRects = originalGetClientRects;
    document.body.innerHTML = '';
    vi.restoreAllMocks();
});

const key = (target, name, init = {}) => {
    const event = new KeyboardEvent('keydown', { key: name, bubbles: true, cancelable: true, ...init });
    target.dispatchEvent(event);
    return event;
};

describe('twDialog inert ownership', () => {
    beforeEach(() => {
        document.body.innerHTML = `
            <div id="app">
                <div id="page">page</div>
                <div id="toasts" data-tw-inert-exempt>toasts</div>
                <div id="dialogs">
                    <div id="dialog">
                        <div id="picker"><div id="panel"></div></div>
                        <button id="save">Save</button>
                    </div>
                </div>
            </div>`;
    });

    test('a picker closing inside a dialog leaves the page behind the dialog inert', () => {
        const page = document.getElementById('page');
        window.twDialog.setBackgroundInert(document.getElementById('dialogs'), 'dialog');
        window.twDialog.setBackgroundInert(document.getElementById('picker'), 'picker-1');

        expect(page.hasAttribute('inert')).toBe(true);
        expect(document.getElementById('save').hasAttribute('inert')).toBe(true);

        window.twDialog.clearBackgroundInert('picker-1');

        expect(page.hasAttribute('inert')).toBe(true);
        expect(document.getElementById('save').hasAttribute('inert')).toBe(false);

        window.twDialog.clearBackgroundInert('dialog');

        expect(page.hasAttribute('inert')).toBe(false);
        expect(page.hasAttribute('data-tw-dialog-inert-owners')).toBe(false);
    });

    test('the same owner marking twice is cleared in one go', () => {
        const page = document.getElementById('page');
        window.twDialog.setBackgroundInert(document.getElementById('dialogs'), 'dialog');
        window.twDialog.setBackgroundInert(document.getElementById('dialogs'), 'dialog');

        window.twDialog.clearBackgroundInert('dialog');

        expect(page.hasAttribute('inert')).toBe(false);
    });

    test('clearing with no owner lifts everything', () => {
        window.twDialog.setBackgroundInert(document.getElementById('dialogs'), 'dialog');
        window.twDialog.setBackgroundInert(document.getElementById('picker'), 'picker-1');

        window.twDialog.clearBackgroundInert();

        expect(document.querySelectorAll('[inert]').length).toBe(0);
    });

    test('the toast container is never made inert', () => {
        window.twDialog.setBackgroundInert(document.getElementById('dialogs'), 'dialog');

        expect(document.getElementById('toasts').hasAttribute('inert')).toBe(false);
    });
});

describe('twDialog Escape handling', () => {
    const dialogRef = () => ({ invokeMethodAsync: vi.fn(() => Promise.resolve()) });

    afterEach(() => {
        window.twDialog._escapeStack.slice().forEach(entry => window.twDialog.unregisterEscape(entry.surface));
    });

    test('one Escape closes the dialog even when focus is outside it', () => {
        document.body.innerHTML = '<div id="surface"><input id="field"></div>';
        const ref = dialogRef();
        window.twDialog.registerEscape(document.getElementById('surface'), ref);

        key(document.body, 'Escape');

        expect(ref.invokeMethodAsync).toHaveBeenCalledTimes(1);
        expect(ref.invokeMethodAsync).toHaveBeenCalledWith('CloseFromEscape');
    });

    test('other keys do nothing', () => {
        document.body.innerHTML = '<div id="surface"><input id="field"></div>';
        const ref = dialogRef();
        window.twDialog.registerEscape(document.getElementById('surface'), ref);

        key(document.getElementById('field'), 'Enter');

        expect(ref.invokeMethodAsync).not.toHaveBeenCalled();
    });

    test('only the topmost dialog is closed', () => {
        document.body.innerHTML = '<div id="first"></div><div id="second"></div>';
        const first = dialogRef();
        const second = dialogRef();
        window.twDialog.registerEscape(document.getElementById('first'), first);
        window.twDialog.registerEscape(document.getElementById('second'), second);

        key(document.body, 'Escape');

        expect(second.invokeMethodAsync).toHaveBeenCalledTimes(1);
        expect(first.invokeMethodAsync).not.toHaveBeenCalled();

        window.twDialog.unregisterEscape(document.getElementById('second'));
        key(document.body, 'Escape');

        expect(first.invokeMethodAsync).toHaveBeenCalledTimes(1);
    });

    test('Escape inside an open popover panel belongs to the popover', () => {
        document.body.innerHTML = '<div id="surface"><div data-tw-popover><button id="day">1</button></div></div>';
        const ref = dialogRef();
        window.twDialog.registerEscape(document.getElementById('surface'), ref);

        key(document.getElementById('day'), 'Escape');

        expect(ref.invokeMethodAsync).not.toHaveBeenCalled();
    });

    test('Escape in a trigger whose popover is open belongs to the popover', () => {
        document.body.innerHTML = '<div id="surface"><input id="trigger" role="combobox" aria-expanded="true"></div>';
        const ref = dialogRef();
        window.twDialog.registerEscape(document.getElementById('surface'), ref);

        key(document.getElementById('trigger'), 'Escape');
        expect(ref.invokeMethodAsync).not.toHaveBeenCalled();

        document.getElementById('trigger').setAttribute('aria-expanded', 'false');
        key(document.getElementById('trigger'), 'Escape');
        expect(ref.invokeMethodAsync).toHaveBeenCalledTimes(1);
    });

    test('registering the same surface twice calls it once, and unregistering stops the calls', () => {
        document.body.innerHTML = '<div id="surface"></div>';
        const surface = document.getElementById('surface');
        const ref = dialogRef();
        window.twDialog.registerEscape(surface, ref);
        window.twDialog.registerEscape(surface, ref);

        key(document.body, 'Escape');
        expect(ref.invokeMethodAsync).toHaveBeenCalledTimes(1);

        window.twDialog.unregisterEscape(surface);
        key(document.body, 'Escape');
        expect(ref.invokeMethodAsync).toHaveBeenCalledTimes(1);
    });

    test('a dialog that has left the page is not called', () => {
        document.body.innerHTML = '<div id="surface"></div>';
        const surface = document.getElementById('surface');
        const ref = dialogRef();
        window.twDialog.registerEscape(surface, ref);
        surface.remove();

        key(document.body, 'Escape');

        expect(ref.invokeMethodAsync).not.toHaveBeenCalled();
    });
});

describe('twDialog.focusPanel', () => {
    test('prefers the day that holds the grid tab stop', () => {
        document.body.innerHTML = `
            <div id="panel">
                <button id="previous">Previous month</button>
                <table role="grid"><tr><td><button tabindex="-1">1</button><button id="selected" tabindex="0">2</button></td></tr></table>
            </div>`;

        window.twDialog.focusPanel(document.getElementById('panel'));

        expect(document.activeElement.id).toBe('selected');
    });

    test('falls back to the first focusable control', () => {
        document.body.innerHTML = '<div id="panel"><button id="up">Increase hour</button><input id="hour"></div>';

        window.twDialog.focusPanel(document.getElementById('panel'));

        expect(document.activeElement.id).toBe('up');
    });

    test('does nothing without a panel', () => {
        expect(() => window.twDialog.focusPanel(null)).not.toThrow();
    });
});

describe('twRoving', () => {
    const selector = '[role="option"]';
    const tabStops = container => Array.from(container.querySelectorAll('[tabindex="0"]')).map(el => el.id);

    describe('list', () => {
        let list;

        beforeEach(() => {
            document.body.innerHTML = `
                <ul id="list" role="listbox">
                    <li id="a" role="option" tabindex="0">A</li>
                    <li id="b" role="option" tabindex="0">B</li>
                    <li id="c" role="option" tabindex="0" aria-disabled="true">C</li>
                    <li id="d" role="option" tabindex="0">D</li>
                </ul>`;
            list = document.getElementById('list');
            window.twRoving.attach(list, selector, 'list');
        });

        test('leaves exactly one option in the Tab order', () => {
            expect(tabStops(list)).toEqual(['a']);
        });

        test('arrow keys move focus and the Tab stop, skipping disabled options', () => {
            document.getElementById('a').focus();

            const down = key(document.getElementById('a'), 'ArrowDown');
            expect(document.activeElement.id).toBe('b');
            expect(down.defaultPrevented).toBe(true);

            key(document.getElementById('b'), 'ArrowDown');
            expect(document.activeElement.id).toBe('d');
            expect(tabStops(list)).toEqual(['d']);

            key(document.getElementById('d'), 'ArrowUp');
            expect(document.activeElement.id).toBe('b');
        });

        test('Home and End jump to the first and last option', () => {
            document.getElementById('b').focus();

            key(document.getElementById('b'), 'End');
            expect(document.activeElement.id).toBe('d');

            key(document.getElementById('d'), 'Home');
            expect(document.activeElement.id).toBe('a');
        });

        test('the arrows stop at the ends', () => {
            document.getElementById('a').focus();

            key(document.getElementById('a'), 'ArrowUp');

            expect(document.activeElement.id).toBe('a');
        });

        test('Space is kept from scrolling the page, and Tab is left alone', () => {
            expect(key(document.getElementById('a'), ' ').defaultPrevented).toBe(true);
            expect(key(document.getElementById('a'), 'Tab').defaultPrevented).toBe(false);
        });

        test('Left and Right are ignored outside a tree', () => {
            expect(key(document.getElementById('a'), 'ArrowRight').defaultPrevented).toBe(false);
        });

        test('after the option holding the Tab stop is removed, sync hands it to another and restores focus', () => {
            key(document.getElementById('a'), 'ArrowDown');
            document.getElementById('b').remove();

            window.twRoving.sync(list, selector, true);

            expect(tabStops(list)).toEqual(['a']);
            expect(document.activeElement.id).toBe('a');
        });

        test('attaching again only re-syncs', () => {
            document.getElementById('a').setAttribute('tabindex', '0');
            document.getElementById('b').setAttribute('tabindex', '0');

            window.twRoving.attach(list, selector, 'list');
            document.getElementById('a').focus();
            key(document.getElementById('a'), 'ArrowDown');

            expect(tabStops(list)).toEqual(['b']);
            expect(document.activeElement.id).toBe('b');
        });

        test('detach removes the key handling', () => {
            window.twRoving.detach(list);
            document.getElementById('a').focus();

            key(document.getElementById('a'), 'ArrowDown');

            expect(document.activeElement.id).toBe('a');
        });
    });

    describe('tree', () => {
        const itemSelector = '[role="treeitem"]';
        let tree;

        beforeEach(() => {
            document.body.innerHTML = `
                <ul id="tree" role="tree">
                    <li id="docs" role="treeitem" tabindex="0" aria-expanded="true">
                        <ul role="group">
                            <li id="resume" role="treeitem" tabindex="0"><input id="check" type="checkbox"></li>
                        </ul>
                    </li>
                    <li id="pictures" role="treeitem" tabindex="0" aria-expanded="false">
                        <ul role="group" hidden>
                            <li id="holiday" role="treeitem" tabindex="0" hidden></li>
                        </ul>
                    </li>
                </ul>`;
            tree = document.getElementById('tree');
            window.twRoving.attach(tree, itemSelector, 'tree');
        });

        test('Down skips the children of a collapsed node', () => {
            document.getElementById('resume').focus();

            key(document.getElementById('resume'), 'ArrowDown');
            expect(document.activeElement.id).toBe('pictures');

            key(document.getElementById('pictures'), 'ArrowDown');
            expect(document.activeElement.id).toBe('pictures');
        });

        test('Right on an expanded node moves to its first child', () => {
            document.getElementById('docs').focus();

            key(document.getElementById('docs'), 'ArrowRight');

            expect(document.activeElement.id).toBe('resume');
        });

        test('Right on a collapsed node leaves focus for the component to expand it', () => {
            document.getElementById('pictures').focus();

            key(document.getElementById('pictures'), 'ArrowRight');

            expect(document.activeElement.id).toBe('pictures');
        });

        test('Left on a child moves to its parent, and on an expanded node leaves focus for the component to collapse it', () => {
            document.getElementById('resume').focus();

            key(document.getElementById('resume'), 'ArrowLeft');
            expect(document.activeElement.id).toBe('docs');

            key(document.getElementById('docs'), 'ArrowLeft');
            expect(document.activeElement.id).toBe('docs');
        });

        test('keys pressed in a control inside a node are not handled', () => {
            const check = document.getElementById('check');
            check.focus();

            const event = key(check, 'ArrowDown');

            expect(event.defaultPrevented).toBe(false);
            expect(document.activeElement.id).toBe('check');
        });
    });
});

describe('twCarousel', () => {
    let carousel;
    let ref;

    beforeEach(() => {
        document.body.innerHTML = `
            <div id="carousel">
                <button id="next">Next slide</button>
                <input id="field">
                <div id="slider" role="slider" tabindex="0"></div>
            </div>`;
        carousel = document.getElementById('carousel');
        ref = { invokeMethodAsync: vi.fn(() => Promise.resolve()) };
        window.twCarousel.attach(carousel, ref);
    });

    test('Left and Right change slide', () => {
        const right = key(document.getElementById('next'), 'ArrowRight');
        key(document.getElementById('next'), 'ArrowLeft');

        expect(ref.invokeMethodAsync).toHaveBeenNthCalledWith(1, 'NextSlideFromKey');
        expect(ref.invokeMethodAsync).toHaveBeenNthCalledWith(2, 'PreviousSlideFromKey');
        expect(right.defaultPrevented).toBe(true);
    });

    test('the arrows are left to a text field or a slider inside a slide', () => {
        const inField = key(document.getElementById('field'), 'ArrowRight');
        key(document.getElementById('slider'), 'ArrowLeft');

        expect(ref.invokeMethodAsync).not.toHaveBeenCalled();
        expect(inField.defaultPrevented).toBe(false);
    });

    test('other keys are ignored', () => {
        key(document.getElementById('next'), 'ArrowDown');

        expect(ref.invokeMethodAsync).not.toHaveBeenCalled();
    });

    test('attaching twice registers one handler, and detach removes it', () => {
        window.twCarousel.attach(carousel, ref);
        key(document.getElementById('next'), 'ArrowRight');
        expect(ref.invokeMethodAsync).toHaveBeenCalledTimes(1);

        window.twCarousel.detach(carousel);
        key(document.getElementById('next'), 'ArrowRight');
        expect(ref.invokeMethodAsync).toHaveBeenCalledTimes(1);
    });

    test('prefersReducedMotion reports the media query, and false when it is unavailable', () => {
        window.matchMedia = vi.fn(() => ({ matches: true }));
        expect(window.twCarousel.prefersReducedMotion()).toBe(true);

        window.matchMedia = vi.fn(() => { throw new Error('unsupported'); });
        expect(window.twCarousel.prefersReducedMotion()).toBe(false);
    });
});

describe('twSidebar focus and drawer helpers', () => {
    test('focusMain focuses the main region', () => {
        document.body.innerHTML = '<a id="skip" href="#main-content">Skip</a><main id="main-content" tabindex="0"></main>';
        const main = document.getElementById('main-content');
        main.scrollIntoView = vi.fn();

        window.twSidebar.focusMain(main);

        expect(document.activeElement).toBe(main);
        expect(main.scrollIntoView).toHaveBeenCalled();
        expect(() => window.twSidebar.focusMain(null)).not.toThrow();
    });

    test('focusById focuses the element, and ignores an unknown id', () => {
        document.body.innerHTML = '<button id="toggle">Open sidebar</button>';

        window.twSidebar.focusById('toggle');

        expect(document.activeElement.id).toBe('toggle');
        expect(() => window.twSidebar.focusById('missing')).not.toThrow();
    });

    test('syncDrawer makes the page inert only while the sidebar is an open drawer', () => {
        document.body.innerHTML = '<main id="main"></main>';
        const main = document.getElementById('main');
        let wide = false;
        let onChange;
        window.matchMedia = vi.fn(() => ({
            get matches() { return wide; },
            addEventListener: (_, handler) => { onChange = handler; }
        }));

        expect(window.twSidebar.syncDrawer(main, true)).toBe(true);
        expect(main.hasAttribute('inert')).toBe(true);

        expect(window.twSidebar.syncDrawer(main, false)).toBe(false);
        expect(main.hasAttribute('inert')).toBe(false);

        // Open on a narrow viewport, then widen it: the content must not stay inert beside the sidebar.
        window.twSidebar.syncDrawer(main, true);
        wide = true;
        onChange();
        expect(main.hasAttribute('inert')).toBe(false);

        expect(window.twSidebar.syncDrawer(null, true)).toBe(false);
    });
});

describe('small focus and state helpers', () => {
    test('twCheckbox.setIndeterminate sets the DOM property', () => {
        document.body.innerHTML = '<input id="box" type="checkbox">';
        const box = document.getElementById('box');

        window.twCheckbox.setIndeterminate(box, true);
        expect(box.indeterminate).toBe(true);

        window.twCheckbox.setIndeterminate(box, false);
        expect(box.indeterminate).toBe(false);
        expect(() => window.twCheckbox.setIndeterminate(null, true)).not.toThrow();
    });

    test('twFocus.moveToNeighbour focuses the next control, or the previous one at the end of the page', () => {
        document.body.innerHTML = `
            <button id="before">Before</button>
            <div id="alert"><button id="dismiss">Close</button></div>
            <button id="after">After</button>`;

        window.twFocus.moveToNeighbour(document.getElementById('alert'));
        expect(document.activeElement.id).toBe('after');

        document.getElementById('after').remove();
        window.twFocus.moveToNeighbour(document.getElementById('alert'));
        expect(document.activeElement.id).toBe('before');

        expect(() => window.twFocus.moveToNeighbour(null)).not.toThrow();
    });

    test('twFocus.focusBySelector focuses a heading, and reports when nothing matches', () => {
        document.body.innerHTML = '<h1 id="title">Buttons</h1>';

        expect(window.twFocus.focusBySelector('h1')).toBe(true);
        expect(document.activeElement.id).toBe('title');
        expect(document.getElementById('title').getAttribute('tabindex')).toBe('-1');

        expect(window.twFocus.focusBySelector('h2')).toBe(false);
        expect(window.twFocus.focusBySelector('')).toBe(false);
    });

    test('twFocus.focusById focuses the element', () => {
        document.body.innerHTML = '<input id="upload" type="file">';

        window.twFocus.focusById('upload');

        expect(document.activeElement.id).toBe('upload');
    });

    test('twSliderLock blocks the value keys but not Tab, and can be lifted', () => {
        document.body.innerHTML = '<input id="range" type="range">';
        const range = document.getElementById('range');

        window.twSliderLock.set(range, true);
        window.twSliderLock.set(range, true);
        expect(key(range, 'ArrowRight').defaultPrevented).toBe(true);
        expect(key(range, 'PageUp').defaultPrevented).toBe(true);
        expect(key(range, 'Tab').defaultPrevented).toBe(false);

        window.twSliderLock.set(range, false);
        expect(key(range, 'ArrowRight').defaultPrevented).toBe(false);
        expect(() => window.twSliderLock.set(null, true)).not.toThrow();
    });

    test('twTooltip.describeControl describes a focusable child and reports it', () => {
        document.body.innerHTML = `
            <div id="wrapper"><button id="save" aria-describedby="hint">Save</button><span id="tip" role="tooltip">Saves the form</span></div>
            <div id="plain">Plain text<span id="tip2" role="tooltip">More</span></div>`;

        expect(window.twTooltip.describeControl(document.getElementById('wrapper'), 'tip')).toBe(true);
        expect(document.getElementById('save').getAttribute('aria-describedby')).toBe('hint tip');

        // A second call must not add the id twice.
        window.twTooltip.describeControl(document.getElementById('wrapper'), 'tip');
        expect(document.getElementById('save').getAttribute('aria-describedby')).toBe('hint tip');

        expect(window.twTooltip.describeControl(document.getElementById('plain'), 'tip2')).toBe(false);
        expect(window.twTooltip.describeControl(null, 'tip')).toBe(false);
    });

    test('the date grid keydown guard also covers Page Up and Page Down, the tab list guard does not', () => {
        document.body.innerHTML = '<table id="grid"></table><div id="tabs"></div>';
        const grid = document.getElementById('grid');
        const tabs = document.getElementById('tabs');
        window.twTabs.registerKeydownGuard(grid, true);
        window.twTabs.registerKeydownGuard(tabs);

        expect(key(grid, 'PageDown').defaultPrevented).toBe(true);
        expect(key(grid, 'ArrowLeft').defaultPrevented).toBe(true);
        expect(key(tabs, 'PageDown').defaultPrevented).toBe(false);

        window.twTabs.unregisterKeydownGuard(grid);
        expect(key(grid, 'PageDown').defaultPrevented).toBe(false);
    });
});
