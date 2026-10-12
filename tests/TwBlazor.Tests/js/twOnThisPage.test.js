// @vitest-environment jsdom

import { afterEach, beforeEach, describe, expect, test, vi } from 'vitest';

// Load the script once - it assigns window.twOnThisPage on evaluation.
await import('../../../src/TwBlazor/wwwroot/js/twblazor.js');

// jsdom has no layout engine, so each section's position is stubbed through getBoundingClientRect.
function addSection(id, top) {
    const section = document.createElement('section');
    section.id = id;
    section.getBoundingClientRect = () => ({ top });
    document.body.appendChild(section);
    return section;
}

function addNav(ids) {
    const nav = document.createElement('nav');
    ids.forEach((id) => {
        const link = document.createElement('a');
        link.dataset.twSectionId = id;
        nav.appendChild(link);
    });
    document.body.appendChild(nav);
    return nav;
}

function currentIds(nav) {
    return Array.from(nav.querySelectorAll('[aria-current="location"]')).map((link) => link.dataset.twSectionId);
}

function scroll(target) {
    target.dispatchEvent(new Event('scroll'));
}

describe('twOnThisPage', () => {
    const ids = ['one', 'two', 'three'];
    let nav;

    beforeEach(() => {
        nav = addNav(ids);
    });

    afterEach(() => {
        globalThis.twOnThisPage.dispose('toc');
        globalThis.twOnThisPage.dispose('other');
        document.body.innerHTML = '';
        vi.restoreAllMocks();
    });

    test('marks the first section as current before anything has scrolled past the offset', () => {
        addSection('one', 300);
        addSection('two', 900);
        addSection('three', 1500);

        globalThis.twOnThisPage.observe('toc', nav, ids, 140);

        expect(currentIds(nav)).toEqual(['one']);
    });

    test('marks the last section whose top has passed the offset', () => {
        addSection('one', -600);
        addSection('two', 100);
        addSection('three', 700);

        globalThis.twOnThisPage.observe('toc', nav, ids, 140);

        expect(currentIds(nav)).toEqual(['two']);
    });

    test('uses the given offset', () => {
        addSection('one', -600);
        addSection('two', 200);
        addSection('three', 700);

        globalThis.twOnThisPage.observe('toc', nav, ids, 240);

        expect(currentIds(nav)).toEqual(['two']);
    });

    test('moves the current marker as the page scrolls', () => {
        addSection('one', 0);
        const two = addSection('two', 600);
        addSection('three', 1200);
        globalThis.twOnThisPage.observe('toc', nav, ids, 140);

        two.getBoundingClientRect = () => ({ top: 20 });
        scroll(document);

        expect(currentIds(nav)).toEqual(['two']);
    });

    test('hears scrolling inside a nested scroll container', () => {
        const scroller = document.createElement('div');
        document.body.appendChild(scroller);
        addSection('one', 0);
        const two = addSection('two', 600);
        addSection('three', 1200);
        globalThis.twOnThisPage.observe('toc', nav, ids, 140);

        two.getBoundingClientRect = () => ({ top: 20 });
        scroll(scroller);

        expect(currentIds(nav)).toEqual(['two']);
    });

    test('marks the last section once its scroll container reaches the bottom', () => {
        const scroller = document.createElement('div');
        Object.defineProperties(scroller, {
            scrollTop: { value: 500 },
            clientHeight: { value: 500 },
            scrollHeight: { value: 1000 }
        });
        document.body.appendChild(scroller);
        addSection('one', -400);
        addSection('two', 100);
        addSection('three', 400);
        globalThis.twOnThisPage.observe('toc', nav, ids, 140);

        scroll(scroller);

        expect(currentIds(nav)).toEqual(['three']);
    });

    test('does not treat an unscrolled container as being at the bottom', () => {
        const scroller = document.createElement('div');
        Object.defineProperties(scroller, {
            scrollTop: { value: 0 },
            clientHeight: { value: 500 },
            scrollHeight: { value: 500 }
        });
        document.body.appendChild(scroller);
        addSection('one', 0);
        addSection('two', 600);
        addSection('three', 1200);
        globalThis.twOnThisPage.observe('toc', nav, ids, 140);

        scroll(scroller);

        expect(currentIds(nav)).toEqual(['one']);
    });

    test('skips ids that have no element on the page', () => {
        addSection('one', -600);
        addSection('three', 100);

        globalThis.twOnThisPage.observe('toc', nav, ids, 140);

        expect(currentIds(nav)).toEqual(['three']);
    });

    test('only marks links inside its own nav', () => {
        const other = addNav(ids);
        addSection('one', 0);

        globalThis.twOnThisPage.observe('toc', nav, ids, 140);

        expect(currentIds(nav)).toEqual(['one']);
        expect(currentIds(other)).toEqual([]);
    });

    test('keeps two instances independent', () => {
        const other = addNav(ids);
        addSection('one', 0);
        const two = addSection('two', 600);
        globalThis.twOnThisPage.observe('toc', nav, ids, 140);
        globalThis.twOnThisPage.observe('other', other, ids, 140);

        globalThis.twOnThisPage.dispose('toc');
        two.getBoundingClientRect = () => ({ top: 20 });
        scroll(document);

        expect(currentIds(nav)).toEqual(['one']);
        expect(currentIds(other)).toEqual(['two']);
    });

    test('dispose stops following the scroll position', () => {
        addSection('one', 0);
        const two = addSection('two', 600);
        globalThis.twOnThisPage.observe('toc', nav, ids, 140);

        globalThis.twOnThisPage.dispose('toc');
        two.getBoundingClientRect = () => ({ top: 20 });
        scroll(document);

        expect(currentIds(nav)).toEqual(['one']);
    });

    test('observing again replaces the previous listener instead of stacking another', () => {
        const add = vi.spyOn(document, 'addEventListener');
        const remove = vi.spyOn(document, 'removeEventListener');
        addSection('one', 0);

        globalThis.twOnThisPage.observe('toc', nav, ids, 140);
        globalThis.twOnThisPage.observe('toc', nav, ids, 140);

        expect(add).toHaveBeenCalledTimes(2);
        expect(remove).toHaveBeenCalledTimes(1);
        expect(globalThis.twOnThisPage._stops.size).toBe(1);
    });

    test('does nothing without a nav or without ids', () => {
        const add = vi.spyOn(document, 'addEventListener');

        globalThis.twOnThisPage.observe('toc', null, ids, 140);
        globalThis.twOnThisPage.observe('toc', nav, [], 140);
        globalThis.twOnThisPage.observe('toc', nav, null, 140);

        expect(add).not.toHaveBeenCalled();
        expect(globalThis.twOnThisPage._stops.size).toBe(0);
    });

    test('dispose is safe for a key that was never observed', () => {
        expect(() => globalThis.twOnThisPage.dispose('missing')).not.toThrow();
    });
});
