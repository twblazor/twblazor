// @vitest-environment jsdom

import { beforeEach, afterEach, describe, expect, test, vi } from 'vitest';

// Load the script once — it assigns window.twPicker and window.twCodeBlock on evaluation.
// hljs is accessed at call-time inside twCodeBlock.highlightElement, so it does not need to
// be present when the module loads.
await import('../../../src/TwBlazor/wwwroot/js/twblazor.js');

describe('twPicker', () => {
    afterEach(() => {
        vi.restoreAllMocks();
    });

    describe('registerOutsideClick', () => {
        // Simulates a full pointerdown -> pointerup gesture on the registered root's handlers.
        // dx/dy let a test simulate how far the pointer travelled in between, to distinguish a tap
        // (no/small movement) from a scroll or drag (movement past the threshold).
        function simulateGesture(root, downTarget, { upTarget = downTarget, dx = 0, dy = 0 } = {}) {
            root.__twPickerPointerDown({ target: downTarget, clientX: 0, clientY: 0 });
            root.__twPickerPointerUp({ target: upTarget, clientX: dx, clientY: dy });
        }

        test('registers pointerdown, pointerup, and pointercancel listeners on document', () => {
            const root = document.createElement('div');
            const dotnetRef = { invokeMethodAsync: vi.fn() };

            const addSpy = vi.spyOn(document, 'addEventListener');

            window.twPicker.registerOutsideClick(root, dotnetRef);

            expect(addSpy).toHaveBeenCalledWith('pointerdown', expect.any(Function));
            expect(addSpy).toHaveBeenCalledWith('pointerup', expect.any(Function));
            expect(addSpy).toHaveBeenCalledWith('pointercancel', expect.any(Function));
            expect(root.__twPickerPointerDown).toBeDefined();
            expect(root.__twPickerPointerUp).toBeDefined();
            expect(root.__twPickerPointerCancel).toBeDefined();

            window.twPicker.unregisterOutsideClick(root);
        });

        test('does nothing when root is null', () => {
            const addSpy = vi.spyOn(document, 'addEventListener');

            window.twPicker.registerOutsideClick(null, {});

            expect(addSpy).not.toHaveBeenCalled();
        });

        test('calls Close on dotnetRef when a tap (minimal movement) lands outside root', () => {
            const root = document.createElement('div');
            document.body.appendChild(root);
            const outside = document.createElement('span');
            document.body.appendChild(outside);

            const dotnetRef = { invokeMethodAsync: vi.fn() };

            window.twPicker.registerOutsideClick(root, dotnetRef);
            simulateGesture(root, outside);

            expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('Close');

            window.twPicker.unregisterOutsideClick(root);
            document.body.removeChild(root);
            document.body.removeChild(outside);
        });

        describe('replaying a tap that landed on an inert ancestor', () => {
            const flush = () => new Promise((resolve) => setTimeout(resolve, 0));

            // jsdom has no layout, so elementFromPoint is stubbed to return whatever the test says is
            // "really under the pointer" once the panel has closed and the inert state is lifted.
            function setup(elementUnderPointer) {
                const root = document.createElement('div');
                const container = document.createElement('div'); // what the browser hit-tests while inert
                const saveButton = document.createElement('button');
                container.appendChild(saveButton);
                document.body.append(root, container);

                document.elementFromPoint = vi.fn(() => elementUnderPointer(container, saveButton));
                const clickSpy = vi.spyOn(saveButton, 'click');
                const dotnetRef = { invokeMethodAsync: vi.fn(() => Promise.resolve()) };

                window.twPicker.registerOutsideClick(root, dotnetRef);
                return { root, container, saveButton, clickSpy, dotnetRef };
            }

            function teardown({ root, container }) {
                window.twPicker.unregisterOutsideClick(root);
                root.remove();
                container.remove();
                delete document.elementFromPoint;
            }

            test('clicks the control under the pointer once Close has finished', async () => {
                const ctx = setup((_container, saveButton) => saveButton);

                simulateGesture(ctx.root, ctx.container);
                expect(ctx.clickSpy).not.toHaveBeenCalled(); // not before Close resolves
                await flush();

                expect(ctx.dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('Close');
                expect(ctx.clickSpy).toHaveBeenCalledTimes(1);
                teardown(ctx);
            });

            test('still clicks it when Close rejects, so a failed close never eats the tap', async () => {
                const ctx = setup((_container, saveButton) => saveButton);
                ctx.dotnetRef.invokeMethodAsync = vi.fn(() => Promise.reject(new Error('closed')));
                const unhandled = vi.fn();
                process.on('unhandledRejection', unhandled);

                simulateGesture(ctx.root, ctx.container);
                await flush();

                expect(ctx.clickSpy).toHaveBeenCalledTimes(1);
                process.off('unhandledRejection', unhandled);
                teardown(ctx);
            });

            test('does not click again when the pointer was already over the element that received the tap', async () => {
                const ctx = setup((container) => container);

                simulateGesture(ctx.root, ctx.container);
                await flush();

                expect(ctx.clickSpy).not.toHaveBeenCalled();
                teardown(ctx);
            });

            test('does not click an element outside the one that received the tap', async () => {
                const stranger = document.createElement('button');
                document.body.appendChild(stranger);
                const strangerClick = vi.spyOn(stranger, 'click');
                const ctx = setup(() => stranger);

                simulateGesture(ctx.root, ctx.container);
                await flush();

                expect(strangerClick).not.toHaveBeenCalled();
                stranger.remove();
                teardown(ctx);
            });

            test('does nothing when the environment cannot hit-test (no elementFromPoint)', async () => {
                const ctx = setup(() => null);
                delete document.elementFromPoint;

                expect(() => simulateGesture(ctx.root, ctx.container)).not.toThrow();
                await flush();

                expect(ctx.clickSpy).not.toHaveBeenCalled();
                teardown(ctx);
            });

            test('does not replay when the tap landed inside the panel itself', async () => {
                const ctx = setup((_container, saveButton) => saveButton);
                const inner = document.createElement('span');
                ctx.root.appendChild(inner);

                simulateGesture(ctx.root, inner);
                await flush();

                expect(ctx.dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();
                expect(ctx.clickSpy).not.toHaveBeenCalled();
                teardown(ctx);
            });
        });

        test('does not call Close when the tap is inside root', () => {
            const root = document.createElement('div');
            const inner = document.createElement('span');
            root.appendChild(inner);
            document.body.appendChild(root);

            const dotnetRef = { invokeMethodAsync: vi.fn() };

            window.twPicker.registerOutsideClick(root, dotnetRef);
            simulateGesture(root, inner);

            expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();

            window.twPicker.unregisterOutsideClick(root);
            document.body.removeChild(root);
        });

        test('does not call Close when the pointer travels past the move threshold (a scroll/drag, not a tap)', () => {
            // This is the mobile bug this whole gesture-tracking exists to fix: starting a scroll
            // by touching outside the panel must not be treated as a dismissing tap, or there'd be
            // no way to scroll the page to see a panel that's partially off-screen.
            const root = document.createElement('div');
            document.body.appendChild(root);
            const outside = document.createElement('span');
            document.body.appendChild(outside);

            const dotnetRef = { invokeMethodAsync: vi.fn() };

            window.twPicker.registerOutsideClick(root, dotnetRef);
            simulateGesture(root, outside, { dx: 50, dy: 0 }); // moved 50px horizontally - a drag

            expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();

            window.twPicker.unregisterOutsideClick(root);
            document.body.removeChild(root);
            document.body.removeChild(outside);
        });

        test('does not call Close when only pointerup fires without a preceding pointerdown', () => {
            const root = document.createElement('div');
            document.body.appendChild(root);
            const outside = document.createElement('span');
            document.body.appendChild(outside);

            const dotnetRef = { invokeMethodAsync: vi.fn() };

            window.twPicker.registerOutsideClick(root, dotnetRef);
            root.__twPickerPointerUp({ target: outside, clientX: 0, clientY: 0 });

            expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();

            window.twPicker.unregisterOutsideClick(root);
            document.body.removeChild(root);
            document.body.removeChild(outside);
        });

        test('pointercancel discards the in-progress gesture, so a later unrelated pointerup does not close', () => {
            const root = document.createElement('div');
            document.body.appendChild(root);
            const outside = document.createElement('span');
            document.body.appendChild(outside);

            const dotnetRef = { invokeMethodAsync: vi.fn() };

            window.twPicker.registerOutsideClick(root, dotnetRef);
            root.__twPickerPointerDown({ target: outside, clientX: 0, clientY: 0 });
            root.__twPickerPointerCancel();
            root.__twPickerPointerUp({ target: outside, clientX: 0, clientY: 0 });

            expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();

            window.twPicker.unregisterOutsideClick(root);
            document.body.removeChild(root);
            document.body.removeChild(outside);
        });

        test('swallows errors thrown by invokeMethodAsync', () => {
            const root = document.createElement('div');
            document.body.appendChild(root);
            const outside = document.createElement('span');
            document.body.appendChild(outside);

            const error = new Error('interop error');
            const dotnetRef = {
                invokeMethodAsync: vi.fn().mockImplementation(() => { throw error; }),
            };
            const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

            window.twPicker.registerOutsideClick(root, dotnetRef);

            expect(() => simulateGesture(root, outside)).not.toThrow();
            expect(consoleErrorSpy).toHaveBeenCalledWith('twPicker handler error', error);

            window.twPicker.unregisterOutsideClick(root);
            document.body.removeChild(root);
            document.body.removeChild(outside);
        });

        test('a second outside tap does not call Close again once the first has already started closing', () => {
            // Regression test: invokeMethodAsync('Close') disposes the dotnetRef server-side. A
            // second qualifying tap arriving before that takes effect (e.g. from a rapid
            // double-tap) would otherwise invoke it again on an already-disposed object.
            const root = document.createElement('div');
            document.body.appendChild(root);
            const outside = document.createElement('span');
            document.body.appendChild(outside);
            const dotnetRef = { invokeMethodAsync: vi.fn() };

            window.twPicker.registerOutsideClick(root, dotnetRef);
            simulateGesture(root, outside);
            simulateGesture(root, outside);

            expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledTimes(1);

            window.twPicker.unregisterOutsideClick(root);
            document.body.removeChild(root);
            document.body.removeChild(outside);
        });

        test('registering again (a fresh open) resets the closing guard from a previous open', () => {
            const root = document.createElement('div');
            document.body.appendChild(root);
            const outside = document.createElement('span');
            document.body.appendChild(outside);
            const dotnetRef = { invokeMethodAsync: vi.fn() };

            window.twPicker.registerOutsideClick(root, dotnetRef);
            simulateGesture(root, outside);
            window.twPicker.unregisterOutsideClick(root);

            // A fresh open re-registers on the same root element - the guard must not still think
            // it's mid-close from last time.
            window.twPicker.registerOutsideClick(root, dotnetRef);
            simulateGesture(root, outside);

            expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledTimes(2);

            window.twPicker.unregisterOutsideClick(root);
            document.body.removeChild(root);
            document.body.removeChild(outside);
        });
    });

    describe('positionPanel', () => {
        function mockPanel(rect) {
            const panel = document.createElement('div');
            // Real DOMRect derives width/height from the edges - mirror that here so callers only
            // need to specify left/right/top/bottom, the way the real getBoundingClientRect works.
            panel.getBoundingClientRect = () => ({
                ...rect,
                width: rect.right - rect.left,
                height: rect.bottom - rect.top,
            });
            return panel;
        }

        function setViewport(width, height) {
            Object.defineProperty(document.documentElement, 'clientWidth', { value: width, configurable: true });
            Object.defineProperty(document.documentElement, 'clientHeight', { value: height, configurable: true });
        }

        afterEach(() => {
            // Restore real (jsdom-default) viewport getters so other tests aren't affected.
            delete document.documentElement.clientWidth;
            delete document.documentElement.clientHeight;
        });

        test('does nothing when panel is null', () => {
            expect(() => window.twPicker.positionPanel(null)).not.toThrow();
        });

        test('resets any previously-applied inline positioning styles before recomputing', () => {
            setViewport(1000, 800);
            const panel = mockPanel({ left: 10, right: 200, top: 10, bottom: 100 });
            panel.style.left = 'auto';
            panel.style.right = '0';
            panel.style.top = 'auto';
            panel.style.bottom = '100%';
            panel.style.marginTop = '0';
            panel.style.marginBottom = '0.5rem';
            panel.style.maxHeight = '123px';

            window.twPicker.positionPanel(panel);

            // Panel fits within the viewport, so all overrides should be cleared, not reapplied.
            expect(panel.style.left).toBe('');
            expect(panel.style.right).toBe('');
            expect(panel.style.top).toBe('');
            expect(panel.style.bottom).toBe('');
            expect(panel.style.marginTop).toBe('');
            expect(panel.style.marginBottom).toBe('');
            expect(panel.style.maxHeight).toBe('');
        });

        test('leaves positioning alone when the panel fits entirely within the viewport', () => {
            setViewport(1000, 800);
            const panel = mockPanel({ left: 10, right: 200, top: 10, bottom: 100 });

            window.twPicker.positionPanel(panel);

            expect(panel.style.left).toBe('');
            expect(panel.style.top).toBe('');
            expect(panel.style.maxHeight).toBe('');
        });

        test('flips to the left when the panel overflows the right edge', () => {
            setViewport(1000, 800);
            const panel = mockPanel({ left: 900, right: 1100, top: 10, bottom: 100 });

            window.twPicker.positionPanel(panel);

            expect(panel.style.left).toBe('auto');
            expect(panel.style.right).toBe('0px'); // jsdom normalizes the '0' length to '0px'
        });

        test('flips upward when overflowing the bottom edge and there is more room above', () => {
            setViewport(1000, 800);
            const panel = mockPanel({ left: 10, right: 200, top: 700, bottom: 900 });

            window.twPicker.positionPanel(panel);

            expect(panel.style.top).toBe('auto');
            expect(panel.style.bottom).toBe('100%');
            expect(panel.style.marginTop).toBe('0px'); // jsdom normalizes the '0' length to '0px'
            expect(panel.style.marginBottom).toBe('0.5rem');
            // Plenty of room above (700px) for a 200px-tall panel, so no clamp is needed.
            expect(panel.style.maxHeight).toBe('');
        });

        test('does not flip upward when overflowing the bottom edge but there is not more room above', () => {
            setViewport(1000, 800);
            // A panel taller than the viewport, extending well above the top edge too - flipping up
            // wouldn't help since there's even less room above than below.
            const panel = mockPanel({ left: 10, right: 200, top: -150, bottom: 900 });

            window.twPicker.positionPanel(panel);

            expect(panel.style.top).toBe('');
            expect(panel.style.bottom).toBe('');
        });

        test('does not flip upward when a tall panel overflows the bottom edge but the trigger has little room above it', () => {
            // Regression test: a trigger sitting just below a fixed header (60px of room above) with
            // a tall two-month range-picker panel (rect.bottom far past the viewport purely because
            // of the panel's own height) used to flip upward anyway, because the old check compared
            // rect.top against (viewportHeight - rect.bottom) - and rect.bottom grows with panel
            // height, so that side went deeply negative and made "more room above" look true even
            // though there was actually far more room below (740px) than above (60px).
            setViewport(1000, 800);
            const panel = mockPanel({ left: 10, right: 200, top: 60, bottom: 960 });

            window.twPicker.positionPanel(panel);

            expect(panel.style.top).toBe('');
            expect(panel.style.bottom).toBe('');
        });

        test('clamps panel height to the room available above when flipped upward, so it cannot clip off the top of the viewport', () => {
            // Regression test for the mobile bug: even once the flip direction itself is correct
            // (more room above than below, e.g. an on-screen keyboard has covered the bottom of the
            // screen), a panel taller than that available room would previously still render past
            // the top edge - nothing capped its height to the space that was actually picked.
            setViewport(1000, 800);
            // spaceAbove = 600, spaceBelow = 800 - 600 = 200 - more room above, so it flips up, but
            // the panel itself is 600px tall, taller than the 600px above once the edge gap is spent.
            const panel = mockPanel({ left: 10, right: 200, top: 600, bottom: 1200 });

            window.twPicker.positionPanel(panel);

            expect(panel.style.top).toBe('auto');
            expect(panel.style.bottom).toBe('100%');
            // 600px available above, minus the 8px edge gap.
            expect(panel.style.maxHeight).toBe('592px');
        });

        test('clamps panel height to the room available below when the panel is taller than the space below and there is even less room above', () => {
            setViewport(1000, 800);
            // spaceBelow = 800 - 50 = 750, spaceAbove = 50 - so it stays open downward, but the
            // panel (760px tall) is still taller than the 750px actually available below it.
            const panel = mockPanel({ left: 10, right: 200, top: 50, bottom: 810 });

            window.twPicker.positionPanel(panel);

            expect(panel.style.top).toBe('');
            expect(panel.style.bottom).toBe('');
            // 750px available below, minus the 8px edge gap.
            expect(panel.style.maxHeight).toBe('742px');
        });

        test('uses visualViewport.height instead of clientHeight when available (mobile keyboard open)', () => {
            // clientHeight (the full layout viewport) says there's plenty of room below the trigger,
            // but visualViewport.height (the actually-visible area once an on-screen keyboard has
            // covered the bottom of the screen) says there isn't - the panel must flip upward based
            // on the visible height, not the layout height, or it would clip behind the keyboard.
            setViewport(1000, 800);
            vi.stubGlobal('visualViewport', { height: 400 });
            const panel = mockPanel({ left: 10, right: 200, top: 350, bottom: 450 });

            window.twPicker.positionPanel(panel);

            expect(panel.style.top).toBe('auto');
            expect(panel.style.bottom).toBe('100%');
            expect(panel.style.maxHeight).toBe(''); // fits within the 350px above, no clamp needed

            vi.unstubAllGlobals();
        });

        test('clamps to visualViewport.height (not the larger clientHeight) when a keyboard is open and the panel is too tall to fit', () => {
            // Same on-screen-keyboard scenario as above, but the panel itself (e.g. a two-month
            // range picker) is taller than the space visualViewport says is actually available -
            // clamping must use that shrunk height, not the full layout clientHeight, or the panel
            // would still render past the top of the visible area behind the keyboard.
            setViewport(1000, 800);
            vi.stubGlobal('visualViewport', { height: 400 });
            const panel = mockPanel({ left: 10, right: 200, top: 350, bottom: 700 }); // 350px tall

            window.twPicker.positionPanel(panel);

            expect(panel.style.top).toBe('auto');
            expect(panel.style.bottom).toBe('100%');
            // 350px available above (per visualViewport), minus the 8px edge gap.
            expect(panel.style.maxHeight).toBe('342px');

            vi.unstubAllGlobals();
        });

        test('falls back to clientHeight when visualViewport is unavailable', () => {
            setViewport(1000, 800);
            const panel = mockPanel({ left: 10, right: 200, top: 700, bottom: 900 });

            window.twPicker.positionPanel(panel);

            // Same case as the "flips upward" test above - confirms the fallback path still works.
            expect(panel.style.top).toBe('auto');
            expect(panel.style.bottom).toBe('100%');
        });
    });

    describe('positionPanelFixed', () => {
        // Anchor and panel are both plain elements with a stubbed getBoundingClientRect - the panel's
        // "starts" wherever positionPanelFixed itself just placed it (top/left), unlike positionPanel's
        // mockPanel which is handed a fixed rect up front, since a fixed-position panel has no
        // pre-existing rendered position to read until this function sets one.
        function mockElement(rect) {
            const el = document.createElement('div');
            el.getBoundingClientRect = () => ({
                ...rect,
                width: rect.right - rect.left,
                height: rect.bottom - rect.top,
            });
            return el;
        }

        function setViewport(width, height) {
            Object.defineProperty(document.documentElement, 'clientWidth', { value: width, configurable: true });
            Object.defineProperty(document.documentElement, 'clientHeight', { value: height, configurable: true });
        }

        afterEach(() => {
            delete document.documentElement.clientWidth;
            delete document.documentElement.clientHeight;
        });

        test('does nothing when anchor is null', () => {
            const panel = mockElement({ left: 0, right: 100, top: 0, bottom: 50 });
            expect(() => window.twPicker.positionPanelFixed(null, panel)).not.toThrow();
        });

        test('does nothing when panel is null', () => {
            const anchor = mockElement({ left: 0, right: 100, top: 0, bottom: 50 });
            expect(() => window.twPicker.positionPanelFixed(anchor, null)).not.toThrow();
        });

        test('sets position:fixed and places the panel directly below the anchor when it fits', () => {
            setViewport(1000, 800);
            const anchor = mockElement({ left: 50, right: 250, top: 100, bottom: 130 });
            // The panel's own natural size (200px wide, 150px tall) once placed at that provisional
            // position - well within the viewport either way.
            const panel = mockElement({ left: 50, right: 250, top: 130, bottom: 280 });

            window.twPicker.positionPanelFixed(anchor, panel);

            expect(panel.style.position).toBe('fixed');
            expect(panel.style.top).toBe('130px');
            expect(panel.style.left).toBe('50px');
            expect(panel.style.right).toBe('');
            expect(panel.style.bottom).toBe('');
            expect(panel.style.maxHeight).toBe('');
        });

        test('sets the panel width to match the anchor when matchAnchorWidth is true', () => {
            setViewport(1000, 800);
            const anchor = mockElement({ left: 50, right: 350, top: 100, bottom: 130 }); // 300px wide
            const panel = mockElement({ left: 50, right: 350, top: 130, bottom: 200 });

            window.twPicker.positionPanelFixed(anchor, panel, true);

            expect(panel.style.width).toBe('300px');
        });

        test('leaves the panel width alone when matchAnchorWidth is not set', () => {
            setViewport(1000, 800);
            const anchor = mockElement({ left: 50, right: 350, top: 100, bottom: 130 });
            const panel = mockElement({ left: 50, right: 350, top: 130, bottom: 200 });

            window.twPicker.positionPanelFixed(anchor, panel);

            expect(panel.style.width).toBe('');
        });

        test('flips to the right edge when the panel would overflow it', () => {
            setViewport(1000, 800);
            const anchor = mockElement({ left: 900, right: 950, top: 100, bottom: 130 });
            const panel = mockElement({ left: 900, right: 1100, top: 130, bottom: 230 });

            window.twPicker.positionPanelFixed(anchor, panel);

            expect(panel.style.left).toBe('auto');
            expect(panel.style.right).toBe('50px'); // viewportWidth(1000) - anchorRect.right(950)
        });

        test('flips upward when overflowing the bottom edge and there is more room above', () => {
            setViewport(1000, 800);
            const anchor = mockElement({ left: 10, right: 200, top: 700, bottom: 730 });
            const panel = mockElement({ left: 10, right: 200, top: 730, bottom: 830 });

            window.twPicker.positionPanelFixed(anchor, panel);

            expect(panel.style.top).toBe('auto');
            expect(panel.style.bottom).toBe('100px'); // viewportHeight(800) - anchorRect.top(700)
        });

        test('does not flip upward when there is not more room above than below', () => {
            setViewport(1000, 800);
            const anchor = mockElement({ left: 10, right: 200, top: 50, bottom: 80 });
            // Panel taller than the space below (750px), but there's even less room above (50px).
            const panel = mockElement({ left: 10, right: 200, top: 80, bottom: 840 });

            window.twPicker.positionPanelFixed(anchor, panel);

            expect(panel.style.top).toBe('80px');
            expect(panel.style.bottom).toBe('');
        });

        test('clamps panel height to the room available when it is taller than the space below', () => {
            setViewport(1000, 800);
            const anchor = mockElement({ left: 10, right: 200, top: 50, bottom: 80 });
            const panel = mockElement({ left: 10, right: 200, top: 80, bottom: 840 }); // 760px tall

            window.twPicker.positionPanelFixed(anchor, panel);

            // spaceBelow = 800 - 80 = 720, minus the 8px edge gap.
            expect(panel.style.maxHeight).toBe('712px');
        });

        test('uses visualViewport.height instead of clientHeight when available', () => {
            setViewport(1000, 800);
            vi.stubGlobal('visualViewport', { height: 400 });
            const anchor = mockElement({ left: 10, right: 200, top: 350, bottom: 380 });
            const panel = mockElement({ left: 10, right: 200, top: 380, bottom: 480 });

            window.twPicker.positionPanelFixed(anchor, panel);

            expect(panel.style.top).toBe('auto');
            expect(panel.style.bottom).toBe('50px'); // 400 - anchorRect.top(350)

            vi.unstubAllGlobals();
        });
    });

    describe('registerScrollReposition / unregisterScrollReposition', () => {
        afterEach(() => {
            vi.restoreAllMocks();
        });

        test('does nothing when anchor or panel is null', () => {
            const addSpy = vi.spyOn(document, 'addEventListener');
            window.twPicker.registerScrollReposition(null, document.createElement('div'));
            window.twPicker.registerScrollReposition(document.createElement('div'), null);
            expect(addSpy).not.toHaveBeenCalled();
        });

        test('registers a capture-phase scroll listener and a resize listener', () => {
            const anchor = document.createElement('div');
            const panel = document.createElement('div');
            const docAddSpy = vi.spyOn(document, 'addEventListener');
            const winAddSpy = vi.spyOn(window, 'addEventListener');

            window.twPicker.registerScrollReposition(anchor, panel);

            expect(docAddSpy).toHaveBeenCalledWith('scroll', expect.any(Function), true);
            expect(winAddSpy).toHaveBeenCalledWith('resize', expect.any(Function));
            expect(panel.__twPickerScrollHandler).toBeDefined();

            window.twPicker.unregisterScrollReposition(panel);
        });

        test('is idempotent - registering twice only attaches one set of listeners', () => {
            const anchor = document.createElement('div');
            const panel = document.createElement('div');
            const docAddSpy = vi.spyOn(document, 'addEventListener');

            window.twPicker.registerScrollReposition(anchor, panel);
            window.twPicker.registerScrollReposition(anchor, panel);

            expect(docAddSpy).toHaveBeenCalledTimes(1);

            window.twPicker.unregisterScrollReposition(panel);
        });

        test('a scroll event repositions the panel relative to the anchor', () => {
            Object.defineProperty(document.documentElement, 'clientWidth', { value: 1000, configurable: true });
            Object.defineProperty(document.documentElement, 'clientHeight', { value: 800, configurable: true });
            const anchor = document.createElement('div');
            anchor.getBoundingClientRect = () => ({ left: 20, right: 220, top: 40, bottom: 70, width: 200, height: 30 });
            const panel = document.createElement('div');
            panel.getBoundingClientRect = () => ({ left: 20, right: 220, top: 70, bottom: 170, width: 200, height: 100 });
            document.body.appendChild(panel);

            window.twPicker.registerScrollReposition(anchor, panel);
            document.dispatchEvent(new Event('scroll'));

            expect(panel.style.top).toBe('70px');
            expect(panel.style.left).toBe('20px');

            window.twPicker.unregisterScrollReposition(panel);
            document.body.removeChild(panel);
            delete document.documentElement.clientWidth;
            delete document.documentElement.clientHeight;
        });

        test('unregister removes the listeners and the handler marker', () => {
            const anchor = document.createElement('div');
            const panel = document.createElement('div');
            window.twPicker.registerScrollReposition(anchor, panel);

            const docRemoveSpy = vi.spyOn(document, 'removeEventListener');
            const winRemoveSpy = vi.spyOn(window, 'removeEventListener');

            window.twPicker.unregisterScrollReposition(panel);

            expect(docRemoveSpy).toHaveBeenCalledWith('scroll', expect.any(Function), true);
            expect(winRemoveSpy).toHaveBeenCalledWith('resize', expect.any(Function));
            expect(panel.__twPickerScrollHandler).toBeUndefined();
        });

        test('unregister on a panel that was never registered does nothing', () => {
            const panel = document.createElement('div');
            expect(() => window.twPicker.unregisterScrollReposition(panel)).not.toThrow();
        });

        test('positions the panel immediately upon registration, before any scroll/resize event', () => {
            Object.defineProperty(document.documentElement, 'clientWidth', { value: 1000, configurable: true });
            Object.defineProperty(document.documentElement, 'clientHeight', { value: 800, configurable: true });
            const anchor = document.createElement('div');
            anchor.getBoundingClientRect = () => ({ left: 20, right: 220, top: 40, bottom: 70, width: 200, height: 30 });
            const panel = document.createElement('div');
            panel.getBoundingClientRect = () => ({ left: 20, right: 220, top: 70, bottom: 170, width: 200, height: 100 });
            document.body.appendChild(panel);

            window.twPicker.registerScrollReposition(anchor, panel);

            expect(panel.style.top).toBe('70px');
            expect(panel.style.left).toBe('20px');

            window.twPicker.unregisterScrollReposition(panel);
            document.body.removeChild(panel);
            delete document.documentElement.clientWidth;
            delete document.documentElement.clientHeight;
        });

        test('reapplies positioning if something else clears the panel\'s inline style', async () => {
            Object.defineProperty(document.documentElement, 'clientWidth', { value: 1000, configurable: true });
            Object.defineProperty(document.documentElement, 'clientHeight', { value: 800, configurable: true });
            const anchor = document.createElement('div');
            anchor.getBoundingClientRect = () => ({ left: 20, right: 220, top: 40, bottom: 70, width: 200, height: 30 });
            const panel = document.createElement('div');
            panel.getBoundingClientRect = () => ({ left: 20, right: 220, top: 70, bottom: 170, width: 200, height: 100 });
            document.body.appendChild(panel);

            window.twPicker.registerScrollReposition(anchor, panel);
            expect(panel.style.top).toBe('70px');

            panel.removeAttribute('style');
            expect(panel.style.top).toBe('');

            await new Promise((resolve) => queueMicrotask(resolve));

            expect(panel.style.top).toBe('70px');
            expect(panel.style.left).toBe('20px');

            window.twPicker.unregisterScrollReposition(panel);
            document.body.removeChild(panel);
            delete document.documentElement.clientWidth;
            delete document.documentElement.clientHeight;
        });

        test('unregister disconnects the style observer so a later style clear is left alone', async () => {
            const anchor = document.createElement('div');
            anchor.getBoundingClientRect = () => ({ left: 20, right: 220, top: 40, bottom: 70, width: 200, height: 30 });
            const panel = document.createElement('div');
            panel.getBoundingClientRect = () => ({ left: 20, right: 220, top: 70, bottom: 170, width: 200, height: 100 });
            document.body.appendChild(panel);

            window.twPicker.registerScrollReposition(anchor, panel);
            window.twPicker.unregisterScrollReposition(panel);

            panel.removeAttribute('style');
            await new Promise((resolve) => queueMicrotask(resolve));

            expect(panel.style.top).toBe('');

            document.body.removeChild(panel);
        });

        test('does not reposition the panel when the panel itself scrolls (mobile path)', () => {
            vi.spyOn(window.twDevice, 'prefersNativePicker').mockReturnValue(true);
            const anchor = document.createElement('div');
            anchor.getBoundingClientRect = () => ({ left: 20, right: 220, top: 40, bottom: 70, width: 200, height: 30 });
            const panel = document.createElement('div');
            panel.getBoundingClientRect = () => ({ left: 20, right: 220, top: 70, bottom: 170, width: 200, height: 100 });
            document.body.appendChild(panel);
            const dotnetRef = { invokeMethodAsync: vi.fn() };

            window.twPicker.registerScrollReposition(anchor, panel, false, dotnetRef);
            const spy = vi.spyOn(window.twPicker, 'positionPanelFixed');
            panel.dispatchEvent(new Event('scroll'));

            expect(spy).not.toHaveBeenCalled();

            window.twPicker.unregisterScrollReposition(panel);
            document.body.removeChild(panel);
        });

        describe('close-on-scroll (desktop, dotnetRef supplied)', () => {
            test('still positions the panel once on open, same as the reposition path', () => {
                Object.defineProperty(document.documentElement, 'clientWidth', { value: 1000, configurable: true });
                Object.defineProperty(document.documentElement, 'clientHeight', { value: 800, configurable: true });
                vi.spyOn(window.twDevice, 'prefersNativePicker').mockReturnValue(false);
                const anchor = document.createElement('div');
                anchor.getBoundingClientRect = () => ({ left: 20, right: 220, top: 40, bottom: 70, width: 200, height: 30 });
                const panel = document.createElement('div');
                panel.getBoundingClientRect = () => ({ left: 20, right: 220, top: 70, bottom: 170, width: 200, height: 100 });
                document.body.appendChild(panel);
                const dotnetRef = { invokeMethodAsync: vi.fn() };

                window.twPicker.registerScrollReposition(anchor, panel, false, dotnetRef);

                expect(panel.style.top).toBe('70px');
                expect(panel.style.left).toBe('20px');

                window.twPicker.unregisterScrollReposition(panel);
                document.body.removeChild(panel);
                delete document.documentElement.clientWidth;
                delete document.documentElement.clientHeight;
            });

            test('a scroll event closes the panel instead of repositioning it, on a non-touch platform', () => {
                vi.spyOn(window.twDevice, 'prefersNativePicker').mockReturnValue(false);
                const anchor = document.createElement('div');
                anchor.getBoundingClientRect = () => ({ left: 20, right: 220, top: 40, bottom: 70, width: 200, height: 30 });
                const panel = document.createElement('div');
                panel.getBoundingClientRect = () => ({ left: 20, right: 220, top: 70, bottom: 170, width: 200, height: 100 });
                document.body.appendChild(panel);
                const dotnetRef = { invokeMethodAsync: vi.fn() };

                window.twPicker.registerScrollReposition(anchor, panel, false, dotnetRef);
                const topBeforeScroll = panel.style.top;
                document.dispatchEvent(new Event('scroll'));

                expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('Close');
                // Position is left exactly as it was from the initial open - the panel is about to
                // close, so there's no point recomputing where it should sit.
                expect(panel.style.top).toBe(topBeforeScroll);

                window.twPicker.unregisterScrollReposition(panel);
                document.body.removeChild(panel);
            });

            test('scrolling inside the panel does not close it, and the listener stays armed', () => {
                // Regression test: the scroll listener is capture-phase on the document, so a scroll in the
                // panel's own overflowing list would otherwise count as the page scrolling away.
                vi.spyOn(window.twDevice, 'prefersNativePicker').mockReturnValue(false);
                const anchor = document.createElement('div');
                anchor.getBoundingClientRect = () => ({ left: 20, right: 220, top: 40, bottom: 70, width: 200, height: 30 });
                const panel = document.createElement('div');
                panel.getBoundingClientRect = () => ({ left: 20, right: 220, top: 70, bottom: 170, width: 200, height: 100 });
                const list = document.createElement('div');
                panel.appendChild(list);
                document.body.appendChild(panel);
                const dotnetRef = { invokeMethodAsync: vi.fn() };

                window.twPicker.registerScrollReposition(anchor, panel, false, dotnetRef);
                list.dispatchEvent(new Event('scroll'));
                panel.dispatchEvent(new Event('scroll'));

                expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();
                expect(panel.__twPickerScrollHandler).toBeDefined();

                document.dispatchEvent(new Event('scroll'));

                expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('Close');

                document.body.removeChild(panel);
            });

            test('a scroll elsewhere on the page still closes the panel', () => {
                vi.spyOn(window.twDevice, 'prefersNativePicker').mockReturnValue(false);
                const anchor = document.createElement('div');
                anchor.getBoundingClientRect = () => ({ left: 20, right: 220, top: 40, bottom: 70, width: 200, height: 30 });
                const panel = document.createElement('div');
                panel.getBoundingClientRect = () => ({ left: 20, right: 220, top: 70, bottom: 170, width: 200, height: 100 });
                const sibling = document.createElement('div');
                document.body.append(panel, sibling);
                const dotnetRef = { invokeMethodAsync: vi.fn() };

                window.twPicker.registerScrollReposition(anchor, panel, false, dotnetRef);
                sibling.dispatchEvent(new Event('scroll', { bubbles: false }));
                document.dispatchEvent(new Event('scroll'));

                expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledTimes(1);

                document.body.removeChild(panel);
                document.body.removeChild(sibling);
            });

            test('a burst of scroll events (a real scroll gesture fires many) only invokes Close once', () => {
                // Regression test: invokeMethodAsync('Close') disposes the dotnetRef server-side.
                // Firing it again for every scroll event in the same gesture, before the (also
                // async) unregisterScrollReposition round trip has removed the listener, would
                // invoke it on an already-disposed object and throw "no tracked object" in the
                // browser console for each extra event.
                vi.spyOn(window.twDevice, 'prefersNativePicker').mockReturnValue(false);
                const anchor = document.createElement('div');
                anchor.getBoundingClientRect = () => ({ left: 20, right: 220, top: 40, bottom: 70, width: 200, height: 30 });
                const panel = document.createElement('div');
                panel.getBoundingClientRect = () => ({ left: 20, right: 220, top: 70, bottom: 170, width: 200, height: 100 });
                document.body.appendChild(panel);
                const dotnetRef = { invokeMethodAsync: vi.fn() };

                window.twPicker.registerScrollReposition(anchor, panel, false, dotnetRef);
                document.dispatchEvent(new Event('scroll'));
                document.dispatchEvent(new Event('scroll'));
                document.dispatchEvent(new Event('scroll'));
                window.dispatchEvent(new Event('resize'));

                expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledTimes(1);
                expect(panel.__twPickerScrollHandler).toBeUndefined();

                document.body.removeChild(panel);
            });

            test('does not invoke Close if registerOutsideClick already started closing the same picker', () => {
                // Regression test: a scroll gesture can also end in a pointerup outside the panel
                // (e.g. dragging a scrollbar), independently triggering registerOutsideClick's own
                // close path around the same moment. Both listeners share the same anchor/root
                // element, so whichever fires first must stop the other from also invoking Close()
                // on a dotnetRef the first call has already disposed server-side.
                vi.spyOn(window.twDevice, 'prefersNativePicker').mockReturnValue(false);
                const anchor = document.createElement('div');
                anchor.getBoundingClientRect = () => ({ left: 20, right: 220, top: 40, bottom: 70, width: 200, height: 30 });
                const panel = document.createElement('div');
                panel.getBoundingClientRect = () => ({ left: 20, right: 220, top: 70, bottom: 170, width: 200, height: 100 });
                document.body.appendChild(panel);
                const dotnetRef = { invokeMethodAsync: vi.fn() };

                window.twPicker.registerOutsideClick(anchor, dotnetRef);
                window.twPicker.registerScrollReposition(anchor, panel, false, dotnetRef);

                // registerOutsideClick's own pointerup gesture decides to close first...
                anchor.__twPickerPointerDown({ target: document.body, clientX: 0, clientY: 0 });
                anchor.__twPickerPointerUp({ target: document.body, clientX: 0, clientY: 0 });
                // ...then a scroll event from the same gesture fires the close-on-scroll listener.
                document.dispatchEvent(new Event('scroll'));

                expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledTimes(1);

                window.twPicker.unregisterOutsideClick(anchor);
                document.body.removeChild(panel);
            });

            test('a resize event also closes the panel', () => {
                vi.spyOn(window.twDevice, 'prefersNativePicker').mockReturnValue(false);
                const anchor = document.createElement('div');
                anchor.getBoundingClientRect = () => ({ left: 20, right: 220, top: 40, bottom: 70, width: 200, height: 30 });
                const panel = document.createElement('div');
                panel.getBoundingClientRect = () => ({ left: 20, right: 220, top: 70, bottom: 170, width: 200, height: 100 });
                document.body.appendChild(panel);
                const dotnetRef = { invokeMethodAsync: vi.fn() };

                window.twPicker.registerScrollReposition(anchor, panel, false, dotnetRef);
                window.dispatchEvent(new Event('resize'));

                expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('Close');

                window.twPicker.unregisterScrollReposition(panel);
                document.body.removeChild(panel);
            });

            test('does not attach a style-clearing self-heal observer, since it is not maintaining a position', () => {
                vi.spyOn(window.twDevice, 'prefersNativePicker').mockReturnValue(false);
                const anchor = document.createElement('div');
                anchor.getBoundingClientRect = () => ({ left: 20, right: 220, top: 40, bottom: 70, width: 200, height: 30 });
                const panel = document.createElement('div');
                panel.getBoundingClientRect = () => ({ left: 20, right: 220, top: 70, bottom: 170, width: 200, height: 100 });
                document.body.appendChild(panel);
                const dotnetRef = { invokeMethodAsync: vi.fn() };

                window.twPicker.registerScrollReposition(anchor, panel, false, dotnetRef);

                expect(panel.__twPickerStyleObserver).toBeUndefined();

                window.twPicker.unregisterScrollReposition(panel);
                document.body.removeChild(panel);
            });

            test('on a touch platform, still repositions on scroll instead of closing even when dotnetRef is supplied', () => {
                vi.spyOn(window.twDevice, 'prefersNativePicker').mockReturnValue(true);
                const anchor = document.createElement('div');
                anchor.getBoundingClientRect = () => ({ left: 20, right: 220, top: 40, bottom: 70, width: 200, height: 30 });
                const panel = document.createElement('div');
                panel.getBoundingClientRect = () => ({ left: 20, right: 220, top: 70, bottom: 170, width: 200, height: 100 });
                document.body.appendChild(panel);
                const dotnetRef = { invokeMethodAsync: vi.fn() };

                window.twPicker.registerScrollReposition(anchor, panel, false, dotnetRef);
                document.dispatchEvent(new Event('scroll'));

                expect(dotnetRef.invokeMethodAsync).not.toHaveBeenCalled();
                expect(panel.__twPickerStyleObserver).toBeDefined();

                window.twPicker.unregisterScrollReposition(panel);
                document.body.removeChild(panel);
            });

            test('logs rather than throws if invokeMethodAsync itself throws', () => {
                vi.spyOn(window.twDevice, 'prefersNativePicker').mockReturnValue(false);
                vi.spyOn(console, 'error').mockImplementation(() => {});
                const anchor = document.createElement('div');
                anchor.getBoundingClientRect = () => ({ left: 20, right: 220, top: 40, bottom: 70, width: 200, height: 30 });
                const panel = document.createElement('div');
                panel.getBoundingClientRect = () => ({ left: 20, right: 220, top: 70, bottom: 170, width: 200, height: 100 });
                document.body.appendChild(panel);
                const dotnetRef = { invokeMethodAsync: vi.fn(() => { throw new Error('circuit gone'); }) };

                window.twPicker.registerScrollReposition(anchor, panel, false, dotnetRef);

                expect(() => document.dispatchEvent(new Event('scroll'))).not.toThrow();
                expect(console.error).toHaveBeenCalled();

                window.twPicker.unregisterScrollReposition(panel);
                document.body.removeChild(panel);
            });
        });
    });

    describe('unregisterOutsideClick', () => {
        test('removes the registered pointerdown, pointerup, and pointercancel listeners', () => {
            const root = document.createElement('div');
            const dotnetRef = { invokeMethodAsync: vi.fn() };

            window.twPicker.registerOutsideClick(root, dotnetRef);

            const removeSpy = vi.spyOn(document, 'removeEventListener');
            const onPointerDown = root.__twPickerPointerDown;
            const onPointerUp = root.__twPickerPointerUp;
            const onPointerCancel = root.__twPickerPointerCancel;

            window.twPicker.unregisterOutsideClick(root);

            expect(removeSpy).toHaveBeenCalledWith('pointerdown', onPointerDown);
            expect(removeSpy).toHaveBeenCalledWith('pointerup', onPointerUp);
            expect(removeSpy).toHaveBeenCalledWith('pointercancel', onPointerCancel);
        });

        test('does nothing when root is null', () => {
            const removeSpy = vi.spyOn(document, 'removeEventListener');

            window.twPicker.unregisterOutsideClick(null);

            expect(removeSpy).not.toHaveBeenCalled();
        });

        test('does nothing when root has no registered handlers', () => {
            const root = document.createElement('div');
            const removeSpy = vi.spyOn(document, 'removeEventListener');

            window.twPicker.unregisterOutsideClick(root);

            expect(removeSpy).not.toHaveBeenCalled();
        });

        test('after unregister, handlers are cleared from the element', () => {
            const root = document.createElement('div');
            document.body.appendChild(root);
            const outside = document.createElement('span');
            document.body.appendChild(outside);

            const dotnetRef = { invokeMethodAsync: vi.fn() };

            window.twPicker.registerOutsideClick(root, dotnetRef);
            window.twPicker.unregisterOutsideClick(root);

            expect(root.__twPickerPointerDown).toBeUndefined();
            expect(root.__twPickerPointerUp).toBeUndefined();
            expect(root.__twPickerPointerCancel).toBeUndefined();

            document.body.removeChild(root);
            document.body.removeChild(outside);
        });
    });
});

describe('twDevice', () => {
    const originalUserAgent = navigator.userAgent;
    const originalPlatform = navigator.platform;
    const originalMaxTouchPoints = navigator.maxTouchPoints;

    function setNavigator({ userAgent, platform, maxTouchPoints }) {
        Object.defineProperty(window.navigator, 'userAgent', { value: userAgent, configurable: true });
        Object.defineProperty(window.navigator, 'platform', { value: platform, configurable: true });
        Object.defineProperty(window.navigator, 'maxTouchPoints', { value: maxTouchPoints, configurable: true });
    }

    afterEach(() => {
        setNavigator({ userAgent: originalUserAgent, platform: originalPlatform, maxTouchPoints: originalMaxTouchPoints });
    });

    describe('getPlatform', () => {
        test('returns "ios" for an iPhone user agent', () => {
            setNavigator({
                userAgent: 'Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15',
                platform: 'iPhone',
                maxTouchPoints: 5,
            });

            expect(window.twDevice.getPlatform()).toBe('ios');
        });

        test('returns "ios" for a classic iPad user agent', () => {
            setNavigator({
                userAgent: 'Mozilla/5.0 (iPad; CPU OS 17_0 like Mac OS X) AppleWebKit/605.1.15',
                platform: 'iPad',
                maxTouchPoints: 5,
            });

            expect(window.twDevice.getPlatform()).toBe('ios');
        });

        test('returns "ios" for a modern iPad reporting a MacIntel desktop user agent with touch support', () => {
            setNavigator({
                userAgent: 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15',
                platform: 'MacIntel',
                maxTouchPoints: 5,
            });

            expect(window.twDevice.getPlatform()).toBe('ios');
        });

        test('returns "other" for a real (non-touch) Mac desktop', () => {
            setNavigator({
                userAgent: 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15',
                platform: 'MacIntel',
                maxTouchPoints: 0,
            });

            expect(window.twDevice.getPlatform()).toBe('other');
        });

        test('returns "android" for an Android user agent', () => {
            setNavigator({
                userAgent: 'Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36',
                platform: 'Linux armv8l',
                maxTouchPoints: 5,
            });

            expect(window.twDevice.getPlatform()).toBe('android');
        });

        test('returns "other" for a desktop Windows user agent', () => {
            setNavigator({
                userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36',
                platform: 'Win32',
                maxTouchPoints: 0,
            });

            expect(window.twDevice.getPlatform()).toBe('other');
        });

        test('returns "other" when userAgent and platform are unavailable', () => {
            setNavigator({ userAgent: '', platform: '', maxTouchPoints: 0 });

            expect(window.twDevice.getPlatform()).toBe('other');
        });

        test('returns "other" when navigator itself is unavailable', () => {
            vi.stubGlobal('navigator', undefined);

            expect(window.twDevice.getPlatform()).toBe('other');

            vi.unstubAllGlobals();
        });
    });

    describe('prefersNativePicker', () => {
        test('returns true on iOS', () => {
            setNavigator({
                userAgent: 'Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15',
                platform: 'iPhone',
                maxTouchPoints: 5,
            });

            expect(window.twDevice.prefersNativePicker()).toBe(true);
        });

        test('returns true on Android', () => {
            setNavigator({
                userAgent: 'Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36',
                platform: 'Linux armv8l',
                maxTouchPoints: 5,
            });

            expect(window.twDevice.prefersNativePicker()).toBe(true);
        });

        test('returns false on desktop', () => {
            setNavigator({
                userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36',
                platform: 'Win32',
                maxTouchPoints: 0,
            });

            expect(window.twDevice.prefersNativePicker()).toBe(false);
        });
    });
});

describe('twCodeBlock', () => {
    beforeEach(() => {
        window.hljs = { highlightElement: vi.fn() };
    });

    afterEach(() => {
        vi.restoreAllMocks();
        delete window.hljs;
    });

    test('highlightElement delegates to hljs.highlightElement', () => {
        const el = document.createElement('code');

        window.twCodeBlock.highlightElement(el);

        expect(window.hljs.highlightElement).toHaveBeenCalledWith(el);
    });

    test('highlightElement adds the hljs class even when hljs skips an unknown language', () => {
        const el = document.createElement('code');

        window.twCodeBlock.highlightElement(el);

        expect(el.classList.contains('hljs')).toBe(true);
    });
});

// jsdom doesn't run layout, so a real element's getClientRects() is always empty. twDialog's
// focusable-elements filter treats that the same as "not visible", so tests that need an element
// to be treated as focusable stub getClientRects() to report a non-empty rect list.
function makeFocusable(tag = 'button') {
    const el = document.createElement(tag);
    el.getClientRects = () => [{}];
    return el;
}

describe('twDialog', () => {
    afterEach(() => {
        document.body.innerHTML = '';
        window.twDialog._focusMap.clear();
        vi.restoreAllMocks();
    });

    describe('getFocusableElements', () => {
        test('returns empty array when container is null', () => {
            expect(window.twDialog.getFocusableElements(null)).toEqual([]);
        });

        test('returns focusable elements within the container', () => {
            const container = document.createElement('div');
            const button = makeFocusable('button');
            container.appendChild(button);
            document.body.appendChild(container);

            const result = window.twDialog.getFocusableElements(container);

            expect(result).toEqual([button]);
        });

        test('excludes elements marked inert', () => {
            const container = document.createElement('div');
            const button = makeFocusable('button');
            button.setAttribute('inert', '');
            container.appendChild(button);
            document.body.appendChild(container);

            expect(window.twDialog.getFocusableElements(container)).toEqual([]);
        });

        test('excludes elements with no client rects (not visible)', () => {
            const container = document.createElement('div');
            const button = document.createElement('button'); // no getClientRects stub
            container.appendChild(button);
            document.body.appendChild(container);

            expect(window.twDialog.getFocusableElements(container)).toEqual([]);
        });

        test('excludes disabled inputs and buttons', () => {
            const container = document.createElement('div');
            const button = makeFocusable('button');
            button.disabled = true;
            container.appendChild(button);
            document.body.appendChild(container);

            expect(window.twDialog.getFocusableElements(container)).toEqual([]);
        });

        test('returns empty array and logs error when querySelectorAll throws', () => {
            const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
            const container = {
                querySelectorAll: () => { throw new Error('boom'); },
            };

            const result = window.twDialog.getFocusableElements(container);

            expect(result).toEqual([]);
            expect(consoleErrorSpy).toHaveBeenCalledWith('twDialog.getFocusableElements error', expect.any(Error));
        });
    });

    describe('focusSurface', () => {
        test('does nothing when surface is null', () => {
            expect(() => window.twDialog.focusSurface(null)).not.toThrow();
        });

        test('focuses the first focusable descendant when one exists', () => {
            const surface = document.createElement('div');
            const first = makeFocusable('button');
            const second = makeFocusable('button');
            surface.append(first, second);
            document.body.appendChild(surface);

            const focusSpy = vi.spyOn(first, 'focus');

            window.twDialog.focusSurface(surface);

            expect(focusSpy).toHaveBeenCalled();
        });

        test('falls back to focusing the surface itself when no descendant is focusable', () => {
            const surface = makeFocusable('div');
            document.body.appendChild(surface);
            const focusSpy = vi.spyOn(surface, 'focus');

            window.twDialog.focusSurface(surface);

            expect(focusSpy).toHaveBeenCalled();
        });
    });

    describe('trapFocus / releaseFocusTrap', () => {
        test('does nothing when surface is null', () => {
            expect(() => window.twDialog.trapFocus(null)).not.toThrow();
        });

        test('registers a keydown listener exactly once per surface', () => {
            const surface = document.createElement('div');
            const addSpy = vi.spyOn(surface, 'addEventListener');

            window.twDialog.trapFocus(surface);
            window.twDialog.trapFocus(surface); // second call should be a no-op

            expect(addSpy).toHaveBeenCalledTimes(1);
            expect(surface.__twDialogTrapHandler).toBeDefined();
        });

        test('ignores non-Tab keys', () => {
            const surface = document.createElement('div');
            const button = makeFocusable('button');
            surface.appendChild(button);
            document.body.appendChild(surface);
            window.twDialog.trapFocus(surface);

            const event = new window.KeyboardEvent('keydown', { key: 'Escape', bubbles: true });
            const preventSpy = vi.spyOn(event, 'preventDefault');

            surface.dispatchEvent(event);

            expect(preventSpy).not.toHaveBeenCalled();
        });

        test('Tab with no focusable elements prevents default and refocuses the surface', () => {
            const surface = makeFocusable('div');
            document.body.appendChild(surface);
            window.twDialog.trapFocus(surface);
            const focusSpy = vi.spyOn(surface, 'focus');

            const event = new window.KeyboardEvent('keydown', { key: 'Tab', bubbles: true });
            const preventSpy = vi.spyOn(event, 'preventDefault');
            surface.dispatchEvent(event);

            expect(preventSpy).toHaveBeenCalled();
            expect(focusSpy).toHaveBeenCalled();
        });

        test('Tab on the last focusable element wraps to the first', () => {
            const surface = document.createElement('div');
            const first = makeFocusable('button');
            const last = makeFocusable('button');
            surface.append(first, last);
            document.body.appendChild(surface);
            window.twDialog.trapFocus(surface);
            last.focus();

            const event = new window.KeyboardEvent('keydown', { key: 'Tab', bubbles: true });
            const preventSpy = vi.spyOn(event, 'preventDefault');
            const focusSpy = vi.spyOn(first, 'focus');
            last.dispatchEvent(event);

            expect(preventSpy).toHaveBeenCalled();
            expect(focusSpy).toHaveBeenCalled();
        });

        test('Tab while focus is outside the surface wraps to the first', () => {
            const surface = document.createElement('div');
            const first = makeFocusable('button');
            const last = makeFocusable('button');
            surface.append(first, last);
            document.body.appendChild(surface);
            window.twDialog.trapFocus(surface);
            // Active element defaults to document.body, which is not contained by `surface`.

            const event = new window.KeyboardEvent('keydown', { key: 'Tab', bubbles: true });
            const preventSpy = vi.spyOn(event, 'preventDefault');
            const focusSpy = vi.spyOn(first, 'focus');
            surface.dispatchEvent(event);

            expect(preventSpy).toHaveBeenCalled();
            expect(focusSpy).toHaveBeenCalled();
        });

        test('Shift+Tab on the first focusable element wraps to the last', () => {
            const surface = document.createElement('div');
            const first = makeFocusable('button');
            const last = makeFocusable('button');
            surface.append(first, last);
            document.body.appendChild(surface);
            window.twDialog.trapFocus(surface);
            first.focus();

            const event = new window.KeyboardEvent('keydown', { key: 'Tab', shiftKey: true, bubbles: true });
            const preventSpy = vi.spyOn(event, 'preventDefault');
            const focusSpy = vi.spyOn(last, 'focus');
            first.dispatchEvent(event);

            expect(preventSpy).toHaveBeenCalled();
            expect(focusSpy).toHaveBeenCalled();
        });

        test('Tab on a middle focusable element does not prevent default', () => {
            const surface = document.createElement('div');
            const first = makeFocusable('button');
            const middle = makeFocusable('button');
            const last = makeFocusable('button');
            surface.append(first, middle, last);
            document.body.appendChild(surface);
            window.twDialog.trapFocus(surface);
            middle.focus();

            const event = new window.KeyboardEvent('keydown', { key: 'Tab', bubbles: true });
            const preventSpy = vi.spyOn(event, 'preventDefault');
            middle.dispatchEvent(event);

            expect(preventSpy).not.toHaveBeenCalled();
        });

        test('releaseFocusTrap removes the listener', () => {
            const surface = document.createElement('div');
            window.twDialog.trapFocus(surface);
            const removeSpy = vi.spyOn(surface, 'removeEventListener');
            const handler = surface.__twDialogTrapHandler;

            window.twDialog.releaseFocusTrap(surface);

            expect(removeSpy).toHaveBeenCalledWith('keydown', handler);
            expect(surface.__twDialogTrapHandler).toBeUndefined();
        });

        test('releaseFocusTrap does nothing when surface is null', () => {
            expect(() => window.twDialog.releaseFocusTrap(null)).not.toThrow();
        });

        test('releaseFocusTrap does nothing when no trap was registered', () => {
            const surface = document.createElement('div');
            const removeSpy = vi.spyOn(surface, 'removeEventListener');

            window.twDialog.releaseFocusTrap(surface);

            expect(removeSpy).not.toHaveBeenCalled();
        });
    });

    describe('setBackgroundInert / clearBackgroundInert', () => {
        test('does nothing when exceptEl is null', () => {
            expect(() => window.twDialog.setBackgroundInert(null)).not.toThrow();
        });

        test('marks siblings at every level up to body as inert, excluding the element itself', () => {
            document.body.innerHTML = `
                <div id="app">
                    <div id="dialog-root"><div id="surface"></div></div>
                    <div id="sibling-of-root">background</div>
                </div>
                <div id="sibling-of-app">also background</div>
            `;
            const exceptEl = document.getElementById('dialog-root');

            window.twDialog.setBackgroundInert(exceptEl);

            expect(document.getElementById('sibling-of-root').hasAttribute('inert')).toBe(true);
            expect(document.getElementById('sibling-of-app').hasAttribute('inert')).toBe(true);
            expect(exceptEl.hasAttribute('inert')).toBe(false);
        });

        test('does not re-mark an already-inert sibling', () => {
            document.body.innerHTML = `
                <div id="root">
                    <div id="surface"></div>
                    <div id="already-inert" inert></div>
                </div>
            `;
            const alreadyInert = document.getElementById('already-inert');

            window.twDialog.setBackgroundInert(document.getElementById('surface'));

            expect(alreadyInert.hasAttribute('data-tw-dialog-inert')).toBe(false);
        });

        test('clearBackgroundInert removes inert markers it previously set', () => {
            document.body.innerHTML = `
                <div id="root">
                    <div id="surface"></div>
                    <div id="sibling">background</div>
                </div>
            `;
            window.twDialog.setBackgroundInert(document.getElementById('surface'));
            const sibling = document.getElementById('sibling');
            expect(sibling.hasAttribute('inert')).toBe(true);

            window.twDialog.clearBackgroundInert();

            expect(sibling.hasAttribute('inert')).toBe(false);
            expect(sibling.hasAttribute('data-tw-dialog-inert')).toBe(false);
        });
    });

    describe('captureFocus / restoreFocus', () => {
        test('returns null when nothing is focused (active element is body)', () => {
            expect(window.twDialog.captureFocus()).toBeNull();
        });

        test('captures the active element and returns an opaque token', () => {
            const button = document.createElement('button');
            document.body.appendChild(button);
            button.focus();

            const token = window.twDialog.captureFocus();

            expect(token).toMatch(/^tw-focus-/);
            expect(button.getAttribute('data-tw-focus-token')).toBe(token);
        });

        test('restoreFocus does nothing when token is falsy', () => {
            expect(() => window.twDialog.restoreFocus(null)).not.toThrow();
            expect(() => window.twDialog.restoreFocus(undefined)).not.toThrow();
        });

        test('restoreFocus focuses the captured element (without scrolling it into view) and cleans up its token attribute', () => {
            const button = document.createElement('button');
            document.body.appendChild(button);
            button.focus();
            const token = window.twDialog.captureFocus();
            const focusSpy = vi.spyOn(button, 'focus');

            window.twDialog.restoreFocus(token);

            // preventScroll: true - closing on scroll (see twPicker.registerScrollReposition's
            // desktop close-on-scroll path) must not yank the page back to reveal the trigger,
            // undoing the very scroll that closed the panel.
            expect(focusSpy).toHaveBeenCalledWith({ preventScroll: true });
            expect(button.hasAttribute('data-tw-focus-token')).toBe(false);
        });

        test('restoreFocus falls back to querySelector when the element was removed from the in-memory map', () => {
            const button = document.createElement('button');
            document.body.appendChild(button);
            button.focus();
            const token = window.twDialog.captureFocus();
            window.twDialog._focusMap.delete(token); // simulate the map entry going stale

            const focusSpy = vi.spyOn(button, 'focus');
            window.twDialog.restoreFocus(token);

            expect(focusSpy).toHaveBeenCalled();
        });

        test('restoreFocus does nothing when the element can no longer be found', () => {
            // A token that was never captured (or whose element was already removed).
            expect(() => window.twDialog.restoreFocus('tw-focus-does-not-exist')).not.toThrow();
        });
    });
});

describe('twSlider', () => {
    afterEach(() => {
        document.body.innerHTML = '';
    });

    describe('preventScrollKeys', () => {
        test('does nothing when el is null', () => {
            expect(() => window.twSlider.preventScrollKeys(null)).not.toThrow();
        });

        test('registers a keydown listener exactly once per element', () => {
            const el = document.createElement('div');
            const addSpy = vi.spyOn(el, 'addEventListener');

            window.twSlider.preventScrollKeys(el);
            window.twSlider.preventScrollKeys(el); // second call is a no-op

            expect(addSpy).toHaveBeenCalledTimes(1);
        });

        test.each(['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Home', 'End'])(
            'prevents default for %s',
            (key) => {
                const el = document.createElement('div');
                window.twSlider.preventScrollKeys(el);

                const event = new window.KeyboardEvent('keydown', { key, bubbles: true });
                const preventSpy = vi.spyOn(event, 'preventDefault');
                el.dispatchEvent(event);

                expect(preventSpy).toHaveBeenCalled();
            }
        );

        test('does not prevent default for other keys (e.g. Tab)', () => {
            const el = document.createElement('div');
            window.twSlider.preventScrollKeys(el);

            const event = new window.KeyboardEvent('keydown', { key: 'Tab', bubbles: true });
            const preventSpy = vi.spyOn(event, 'preventDefault');
            el.dispatchEvent(event);

            expect(preventSpy).not.toHaveBeenCalled();
        });
    });
});

describe('twTabs', () => {
    afterEach(() => {
        document.body.innerHTML = '';
    });

    describe('registerKeydownGuard / unregisterKeydownGuard', () => {
        test('does nothing when tablist is null', () => {
            expect(() => window.twTabs.registerKeydownGuard(null)).not.toThrow();
        });

        test('registers a keydown listener exactly once per tablist', () => {
            const tablist = document.createElement('div');
            const addSpy = vi.spyOn(tablist, 'addEventListener');

            window.twTabs.registerKeydownGuard(tablist);
            window.twTabs.registerKeydownGuard(tablist); // second call is a no-op

            expect(addSpy).toHaveBeenCalledTimes(1);
        });

        test.each(['ArrowRight', 'ArrowLeft', 'ArrowUp', 'ArrowDown', 'Home', 'End'])(
            'prevents default for %s',
            (key) => {
                const tablist = document.createElement('div');
                window.twTabs.registerKeydownGuard(tablist);

                const event = new window.KeyboardEvent('keydown', { key, bubbles: true });
                const preventSpy = vi.spyOn(event, 'preventDefault');
                tablist.dispatchEvent(event);

                expect(preventSpy).toHaveBeenCalled();
            }
        );

        test('does not prevent default for Tab, leaving focus free to move out of the tablist', () => {
            const tablist = document.createElement('div');
            window.twTabs.registerKeydownGuard(tablist);

            const event = new window.KeyboardEvent('keydown', { key: 'Tab', bubbles: true });
            const preventSpy = vi.spyOn(event, 'preventDefault');
            tablist.dispatchEvent(event);

            expect(preventSpy).not.toHaveBeenCalled();
        });

        test('unregisterKeydownGuard removes the listener', () => {
            const tablist = document.createElement('div');
            window.twTabs.registerKeydownGuard(tablist);
            const removeSpy = vi.spyOn(tablist, 'removeEventListener');
            const handler = tablist.__twTabsKeydownHandler;

            window.twTabs.unregisterKeydownGuard(tablist);

            expect(removeSpy).toHaveBeenCalledWith('keydown', handler);
            expect(tablist.__twTabsKeydownHandler).toBeUndefined();
        });

        test('unregisterKeydownGuard does nothing when tablist is null', () => {
            expect(() => window.twTabs.unregisterKeydownGuard(null)).not.toThrow();
        });

        test('unregisterKeydownGuard does nothing when no guard was registered', () => {
            const tablist = document.createElement('div');
            const removeSpy = vi.spyOn(tablist, 'removeEventListener');

            window.twTabs.unregisterKeydownGuard(tablist);

            expect(removeSpy).not.toHaveBeenCalled();
        });
    });
});

describe('twSidebar', () => {
    afterEach(() => {
        vi.restoreAllMocks();
        vi.unstubAllGlobals();
    });

    describe('isMobileViewport', () => {
        test('returns false when the viewport matches the "lg" breakpoint (desktop)', () => {
            vi.stubGlobal('matchMedia', vi.fn().mockReturnValue({ matches: true }));

            expect(window.twSidebar.isMobileViewport()).toBe(false);
        });

        test('returns true when the viewport does not match the "lg" breakpoint (mobile)', () => {
            vi.stubGlobal('matchMedia', vi.fn().mockReturnValue({ matches: false }));

            expect(window.twSidebar.isMobileViewport()).toBe(true);
        });

        test('returns true and logs an error when matchMedia throws', () => {
            const error = new Error('matchMedia unavailable');
            vi.stubGlobal('matchMedia', vi.fn().mockImplementation(() => { throw error; }));
            const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

            expect(window.twSidebar.isMobileViewport()).toBe(true);
            expect(consoleErrorSpy).toHaveBeenCalledWith('twSidebar.isMobileViewport error', error);
        });
    });

    describe('scrollToTop', () => {
        test('resets the element scroll position to the top', () => {
            const el = document.createElement('div');
            el.scrollTop = 480;

            window.twSidebar.scrollToTop(el);

            expect(el.scrollTop).toBe(0);
        });

        test('does nothing and does not throw when the element is null', () => {
            expect(() => window.twSidebar.scrollToTop(null)).not.toThrow();
        });
    });
});

describe('twColorPicker', () => {
    function mockElement(rect) {
        const el = document.createElement('div');
        el.getBoundingClientRect = () => rect;
        return el;
    }

    describe('relativePosition', () => {
        test('returns [0, 0] when el is null', () => {
            expect(window.twColorPicker.relativePosition(null, 50, 60)).toEqual([0, 0]);
        });

        test('returns the client point translated into element-relative coordinates', () => {
            const el = mockElement({ left: 20, top: 10 });

            expect(window.twColorPicker.relativePosition(el, 50, 60)).toEqual([30, 50]);
        });

        test('clamps to negative offsets when the point is above/left of the element origin', () => {
            const el = mockElement({ left: 100, top: 100 });

            expect(window.twColorPicker.relativePosition(el, 10, 20)).toEqual([-90, -80]);
        });
    });

    describe('getSize', () => {
        test('returns [0, 0] when el is null', () => {
            expect(window.twColorPicker.getSize(null)).toEqual([0, 0]);
        });

        test('returns the element\'s rendered width and height', () => {
            const el = mockElement({ width: 240, height: 32 });

            expect(window.twColorPicker.getSize(el)).toEqual([240, 32]);
        });
    });

    describe('supportsEyeDropper', () => {
        afterEach(() => {
            delete globalThis.EyeDropper;
        });

        test('returns false when the EyeDropper API is unavailable', () => {
            expect(window.twColorPicker.supportsEyeDropper()).toBe(false);
        });

        test('returns true when the EyeDropper API is present', () => {
            globalThis.EyeDropper = function () {};

            expect(window.twColorPicker.supportsEyeDropper()).toBe(true);
        });
    });

    describe('openEyeDropper', () => {
        afterEach(() => {
            delete globalThis.EyeDropper;
            vi.restoreAllMocks();
        });

        test('returns null when the EyeDropper API is unavailable', async () => {
            await expect(window.twColorPicker.openEyeDropper()).resolves.toBeNull();
        });

        test('returns the picked color as a hex string', async () => {
            globalThis.EyeDropper = function () {
                this.open = () => Promise.resolve({ sRGBHex: '#7f56d9' });
            };

            await expect(window.twColorPicker.openEyeDropper()).resolves.toBe('#7f56d9');
        });

        test('returns null and does not log when the user cancels the pick (AbortError)', async () => {
            globalThis.EyeDropper = function () {
                this.open = () => Promise.reject(new DOMException('cancelled', 'AbortError'));
            };
            const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

            await expect(window.twColorPicker.openEyeDropper()).resolves.toBeNull();
            expect(consoleErrorSpy).not.toHaveBeenCalled();
        });

        test('returns null and logs unexpected errors', async () => {
            const error = new Error('eyedropper unavailable');
            globalThis.EyeDropper = function () {
                this.open = () => Promise.reject(error);
            };
            const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

            await expect(window.twColorPicker.openEyeDropper()).resolves.toBeNull();
            expect(consoleErrorSpy).toHaveBeenCalledWith('twColorPicker.openEyeDropper error', error);
        });
    });
});

describe('twSelect', () => {
    let listbox;
    let dotnetRef;

    function build(optionTexts, selectedIndex) {
        listbox = document.createElement('div');
        listbox.setAttribute('role', 'listbox');
        listbox.tabIndex = 0;
        optionTexts.forEach((text, i) => {
            const option = document.createElement('div');
            option.id = `opt-${i}`;
            option.setAttribute('role', 'option');
            option.setAttribute('data-value', String(i + 1));
            option.textContent = text;
            option.scrollIntoView = vi.fn();
            listbox.appendChild(option);
        });
        document.body.appendChild(listbox);
        dotnetRef = { invokeMethodAsync: vi.fn() };
        window.twSelect.attachListbox(listbox, dotnetRef, selectedIndex === undefined ? null : `opt-${selectedIndex}`);
    }

    function press(key) {
        const e = new KeyboardEvent('keydown', { key, bubbles: true, cancelable: true });
        listbox.dispatchEvent(e);
        return e;
    }

    const activeId = () => listbox.getAttribute('aria-activedescendant');

    afterEach(() => {
        listbox?.remove();
        vi.useRealTimers();
    });

    test('highlights the selected option and focuses the listbox on attach', () => {
        build(['USA', 'UK', 'Canada'], 1);

        expect(activeId()).toBe('opt-1');
        expect(document.getElementById('opt-1').getAttribute('data-active')).toBe('true');
        expect(document.activeElement).toBe(listbox);
    });

    test('highlights the first option when nothing is selected', () => {
        build(['USA', 'UK'], undefined);

        expect(activeId()).toBe('opt-0');
    });

    test('ArrowDown and ArrowUp move the highlight and stop at the ends', () => {
        build(['USA', 'UK', 'Canada'], 0);

        expect(press('ArrowDown').defaultPrevented).toBe(true);
        expect(activeId()).toBe('opt-1');
        press('ArrowDown');
        press('ArrowDown');
        expect(activeId()).toBe('opt-2');
        press('ArrowUp');
        expect(activeId()).toBe('opt-1');
        press('ArrowUp');
        press('ArrowUp');
        expect(activeId()).toBe('opt-0');
    });

    test('Home and End jump to the first and last option', () => {
        build(['USA', 'UK', 'Canada'], 1);

        press('End');
        expect(activeId()).toBe('opt-2');
        press('Home');
        expect(activeId()).toBe('opt-0');
    });

    test('only one option is highlighted at a time', () => {
        build(['USA', 'UK', 'Canada'], 0);

        press('ArrowDown');

        expect(listbox.querySelectorAll('[data-active="true"]').length).toBe(1);
    });

    test('Enter and Space commit the highlighted option', () => {
        build(['USA', 'UK', 'Canada'], 1);

        press('Enter');
        press(' ');

        expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledTimes(2);
        expect(dotnetRef.invokeMethodAsync).toHaveBeenCalledWith('SelectOption', 2);
    });

    test('commits through the supplied method, so a multi-select can toggle instead of select', () => {
        build(['USA', 'UK', 'Canada'], 0);
        listbox.remove();
        listbox = undefined;
        const multi = document.createElement('div');
        multi.tabIndex = 0;
        const option = document.createElement('div');
        option.id = 'm-0';
        option.setAttribute('role', 'option');
        option.setAttribute('data-value', '3');
        option.scrollIntoView = vi.fn();
        multi.appendChild(option);
        document.body.appendChild(multi);
        const ref = { invokeMethodAsync: vi.fn() };
        window.twSelect.attachListbox(multi, ref, 'm-0', 'ToggleOption');

        multi.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', bubbles: true, cancelable: true }));
        multi.dispatchEvent(new KeyboardEvent('keydown', { key: ' ', bubbles: true, cancelable: true }));

        expect(ref.invokeMethodAsync).toHaveBeenCalledTimes(2);
        expect(ref.invokeMethodAsync).toHaveBeenCalledWith('ToggleOption', 3);
        expect(multi.getAttribute('aria-activedescendant')).toBe('m-0');
        multi.remove();
    });

    test('typing a letter highlights the next option starting with it, cycling on repeats', () => {
        build(['Apple', 'Banana', 'Blueberry', 'Cherry'], 0);

        press('b');
        expect(activeId()).toBe('opt-1');
        vi.useFakeTimers();
        press('x');
        vi.advanceTimersByTime(window.twSelect._typeaheadResetMs + 1);
        press('b');
        expect(activeId()).toBe('opt-2');
    });

    test('keys it does not handle are left alone', () => {
        build(['USA', 'UK'], 0);

        expect(press('Tab').defaultPrevented).toBe(false);
        expect(press('Escape').defaultPrevented).toBe(false);
    });

    test('pointer movement highlights the hovered option', () => {
        build(['USA', 'UK', 'Canada'], 0);

        document.getElementById('opt-2').dispatchEvent(new Event('pointermove', { bubbles: true }));

        expect(activeId()).toBe('opt-2');
    });

    test('attaching twice does not register duplicate handlers', () => {
        build(['USA', 'UK', 'Canada'], 0);
        window.twSelect.attachListbox(listbox, dotnetRef, 'opt-0');

        press('ArrowDown');

        expect(activeId()).toBe('opt-1');
    });

    test('does nothing for a missing listbox', () => {
        expect(() => window.twSelect.attachListbox(null, {}, null)).not.toThrow();
    });
});
