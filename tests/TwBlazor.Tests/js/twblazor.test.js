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

        test('restoreFocus focuses the captured element and cleans up its token attribute', () => {
            const button = document.createElement('button');
            document.body.appendChild(button);
            button.focus();
            const token = window.twDialog.captureFocus();
            const focusSpy = vi.spyOn(button, 'focus');

            window.twDialog.restoreFocus(token);

            expect(focusSpy).toHaveBeenCalled();
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
