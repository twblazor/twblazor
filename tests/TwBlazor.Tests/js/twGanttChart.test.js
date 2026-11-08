// @vitest-environment jsdom

import { afterEach, describe, expect, test, vi } from 'vitest';

await import('../../../src/TwBlazor/wwwroot/js/twblazor.js');

const gantt = globalThis.twGanttChart;

// A plot 1000px wide at (0, 0) with one bar outline from 20% to 50% (200px to 500px), 100px to 110px down.
function buildPlot({ flags = 'move resize progress link', progress = 0.5 } = {}) {
    const plot = document.createElement('div');
    plot.innerHTML = `
        <div data-gantt-overlay data-ghost-class="ghost" data-line-class="line">
            <div data-gantt-bar data-key="build" data-flags="${flags}" data-left="20" data-width="30" data-progress="${progress}" style="top:10%;height:2%"></div>
        </div>`;
    document.body.appendChild(plot);
    plot.getBoundingClientRect = () => ({ left: 0, top: 0, right: 1000, bottom: 1000, width: 1000, height: 1000 });
    const bar = plot.querySelector('[data-gantt-bar]');
    bar.getBoundingClientRect = () => ({ left: 200, right: 500, top: 100, bottom: 110, width: 300, height: 10 });
    return { plot, bar };
}

function pointer(type, target, x, y) {
    const event = new MouseEvent(type, { bubbles: true, cancelable: true, clientX: x, clientY: y, button: 0 });
    Object.defineProperty(event, 'pointerId', { value: 1 });
    target.dispatchEvent(event);
}

afterEach(() => {
    document.body.innerHTML = '';
});

describe('hit', () => {
    test.each([
        [300, 105, 'move'],
        [202, 105, 'resize-start'],
        [498, 105, 'resize-end'],
        [350, 108, 'progress'],
        [190, 105, 'link-start'],
        [510, 105, 'link-end']
    ])('at (%i, %i) grabs %s', (x, y, zone) => {
        const { plot } = buildPlot();
        expect(gantt.hit(plot, x, y)?.zone).toBe(zone);
    });

    test('only offers what the bar allows', () => {
        const { plot } = buildPlot({ flags: 'move' });
        expect(gantt.hit(plot, 202, 105).zone).toBe('move');
        expect(gantt.hit(plot, 190, 105)).toBeNull();
    });

    test('misses rows above and below', () => {
        const { plot } = buildPlot();
        expect(gantt.hit(plot, 300, 60)).toBeNull();
    });
});

describe('result', () => {
    const bar = { dataset: { left: '20', width: '30' } };
    const rect = { left: 200, width: 300 };

    test('a move keeps the length', () => {
        expect(gantt.result({ bar, rect, zone: 'move' }, 1000, 300, 400)).toEqual({ start: 30, end: 60 });
    });

    test('resizing the start cannot pass the end', () => {
        expect(gantt.result({ bar, rect, zone: 'resize-start' }, 1000, 200, 900)).toEqual({ start: 50, end: 50 });
    });

    test('resizing the end cannot pass the start', () => {
        expect(gantt.result({ bar, rect, zone: 'resize-end' }, 1000, 500, 0)).toEqual({ start: 20, end: 20 });
    });

    test('progress is the pointer across the bar, kept between 0 and 1', () => {
        expect(gantt.result({ bar, rect, zone: 'progress' }, 1000, 0, 275).progress).toBeCloseTo(0.25);
        expect(gantt.result({ bar, rect, zone: 'progress' }, 1000, 0, 900).progress).toBe(1);
    });
});

describe('dragging', () => {
    test('a drag shows a preview, then reports the new dates', async () => {
        const { plot } = buildPlot();
        const dotnetRef = { invokeMethodAsync: vi.fn().mockResolvedValue(true) };
        gantt.attach(plot, dotnetRef);

        pointer('pointerdown', plot, 300, 105);
        pointer('pointermove', document, 400, 105);
        expect(plot.querySelector('[data-gantt-ghost]').style.left).toBe('30%');

        pointer('pointerup', document, 400, 105);
        await vi.waitFor(() => expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('CommitBarDrag', 'build', 'move', 30, 60));
        await vi.waitFor(() => expect(plot.querySelector('[data-gantt-ghost]')).toBeNull());
        gantt.detach(plot);
    });

    test('a press without movement is left as a click', () => {
        const { plot } = buildPlot();
        const dotnetRef = { invokeMethodAsync: vi.fn() };
        gantt.attach(plot, dotnetRef);

        pointer('pointerdown', plot, 300, 105);
        pointer('pointerup', document, 301, 105);

        expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();
        gantt.detach(plot);
    });

    test('hovering a bar marks it so its handles show', () => {
        const { plot, bar } = buildPlot();
        gantt.attach(plot, { invokeMethodAsync: vi.fn() });

        pointer('pointermove', plot, 300, 105);
        expect(bar.hasAttribute('data-gantt-active')).toBe(true);
        expect(plot.style.cursor).toBe('grab');

        pointer('pointermove', plot, 300, 600);
        expect(bar.hasAttribute('data-gantt-active')).toBe(false);
        gantt.detach(plot);
    });
});

describe('scrollTo', () => {
    test('puts the point a little after the start of the view', () => {
        const scroller = document.createElement('div');
        document.body.appendChild(scroller);
        Object.defineProperty(scroller, 'scrollWidth', { value: 2000 });
        Object.defineProperty(scroller, 'clientWidth', { value: 500 });

        gantt.scrollTo(scroller, 0.5);

        expect(scroller.scrollLeft).toBe(925);
    });
});
